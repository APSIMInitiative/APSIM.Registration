using System;
using System.Linq;
using System.Threading.Tasks;
using APSIM.Registration.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using APSIM.Registration.Models;
using APSIM.Registration.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using APSIM.Registration.Utilities;
using ISO3166;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Reflection;
using System.IO;
using System.Net.Mime;

namespace APSIM.Registration.Pages
{
    public class IndexModel : PageModel
    {
        private const string apsimName = "APSIM Next Generation";
        private const string oldApsimName = "APSIM Classic";
        private const string apsoilName = "Apsoil";
        private const string apsoilDownloadUrl = "https://apsimdev.apsim.info/ApsoilWeb/ApsoilSetup.exe";
        private const string smtpHostVariableName = "SMTP_HOST";
        private const string smtpPortVariableName = "SMTP_PORT";
        private const string smtpUsernameVariableName = "SMTP_USER";
        private const string smtpTokenVariableName = "SMTP_TOKEN";
        private const string pdfMimeType = "application/pdf";
        private const string versionNameLatest = "Latest";
        private const string emailFromAddress = "no-reply@www.apsim.info";
        private const string commercialEmailBody = "EmailBodyCommercial.html";
        private const string nonCommercialEmailBody = "EmailBody.html";
        private const string referencingGuideFileName = "referencing-guide.pdf";
        private const string referencingGuideAttachmentDisplayName = "Guide to Referencing APSIM in Publications.pdf";
        private const string commercialLicencePdf = "APSIM_Commercial_Licence.pdf";
        private const string nonCommercialLicencePdf ="APSIM_NonCommercial_RD_licence.pdf";
        private const string licenseAttachmentDisplayName = "APSIM License.pdf";

        public static IReadOnlyList<Product> Products => new List<Product>()
        {
            new Product(apsimName, GetApsimXUpgrades),
            new Product(oldApsimName, GetApsimClassicUpgrades),
            new Product(apsoilName, GetApsoilVersions),
        };

        private readonly IDbContextGenerator<RegistrationsDbContext> generator;
        private readonly RegistrationController controller;
        private readonly ILogger<IndexModel> logger;

        /// <summary>
        /// Max number of rows of product versions to show on the downloads page.
        /// </summary>
        /// <value></value>
        public int NumDownloads { get; set; } = 10;

        /// <summary>
        /// State variable - show the index page?
        /// </summary>
        public bool ShowIndex { get; private set; } = true;

        /// <summary>
        /// State variable - show the registration form?
        /// </summary>
        public bool ShowRegistrationForm { get; private set; }

        /// <summary>
        /// State variable - show the downloads area?
        /// </summary>
        public bool ShowDownloads { get; private set; }

        /// <summary>
        /// This is bound to a checkbox displayed to the user. If true (checked),
        /// the user wants to subscribe to the mailing list.
        /// </summary>
        public bool Subscribe { get; private set; }

        /// <summary>
        /// This is bound to the email input textbox on the index page.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string Email { get; set; }

        /// <summary>
        /// This is bound to the inputs on the registration page.
        /// </summary>
        [BindProperty]
        public Models.Registration RegistrationDetails { get; set; }

        /// <summary>
        /// Create a new <see cref="RegisterModel"/> instance.
        /// </summary>
        /// <param name="context">The registrations database context.</param>
        public IndexModel(IDbContextGenerator<RegistrationsDbContext> generator, RegistrationController controller, ILogger<IndexModel> logger)
        {
            this.generator = generator;
            this.controller = controller;
            this.logger = logger;
        }

        public async Task OnGetAsync()
        {
            await Submit();
        }

        public async Task OnPostAsync()
        {
            await Submit();
        }

        private async Task Submit()
        {
            if (ShowRegistrationForm)
                await OnPostSubmitRegistrationAsync();
            else if (ShowDownloads)
            {
                // Do nothing
            }
            else
                CheckIsRegistered(Email);
        }

        public async Task<ActionResult> OnPostSubmitRegistrationAsync()
        {
            Request.QueryString = QueryString.Empty;
            ShowIndex = false;
            if (!ModelState.IsValid)
            {
                ShowRegistrationForm = true;
                ShowDownloads = false;
                PageResult page = Page();
                return page;
            }

            try
            {
                logger.LogDebug($"Adding new registration to registrations DB...");
                controller.Register(RegistrationDetails);

                if (Subscribe)
                {
                    logger.LogDebug("Subscribing to mailing list...");
                    controller.Subscribe(RegistrationDetails.Email);
                }

                logger.LogDebug("Sending registration email...");
                await SendRegistrationEmail();

                if (RegistrationDetails.LicenceType == LicenceType.Commercial)
                {
                    logger.LogDebug($"Sending invoice email...");
                    await SendInvoiceEmail();
                }
            }
            catch (Exception err)
            {
                logger.LogError(err, $"An error occurred while performing registration");
            }

            ShowRegistrationForm = false;
            ShowDownloads = true;
            PageResult result = Page();
            return result;
        }

        /// <summary>
        /// Check if the given email address has an active registration
        /// associated with it. If so, display the downloads page. If not,
        /// display the registration page.
        /// </summary>
        /// <param name="email">The email address to be checked.</param>
        private void CheckIsRegistered(string email)
        {
            if (string.IsNullOrEmpty(email))
                ShowIndex = true;
            else
            {
                ShowIndex = false;
                ShowRegistrationForm = true;
                try
                {
                    ShowRegistrationForm = !IsRegistered(email);
                }
                catch (Exception error)
                {
                    logger.LogError(error, "Encountered an error while checking if user is registered");
                }

                if (ShowRegistrationForm)
                {
                    RegistrationDetails.Email = email;
                    OnGetRegister();
                }
                else
                    ShowDownloads = true;
            }
        }

        public void OnGetRegister()
        {
            ShowIndex = false;
            ShowDownloads = false;
            ShowRegistrationForm = true;
            RegistrationDetails = new Models.Registration();
            RegistrationDetails.Product = apsimName;
            RegistrationDetails.Version = versionNameLatest;
            RegistrationDetails.Platform = "Windows";
            RegistrationDetails.LicenceType = LicenceType.NonCommercial;
            RegistrationDetails.Country = "Australia";
        }

        /// <summary>
        /// Check if an email address has an active registration
        /// associated with it.
        /// </summary>
        /// <param name="email">The email address to be checked.</param>
        private bool IsRegistered(string email)
        {
            using (RegistrationsDbContext context = generator.Generate())
                return context.Registrations.Any(r => r.Email == email);
        }

        /// <summary>
        /// Get all apsoil versions available for download.
        /// </summary>
        /// <param name="n">Max number of versions.</param>
        private static List<ProductVersion> GetApsoilVersions(int n)
        {
            return new List<ProductVersion>() { new ProductVersion(apsoilName, "7.1", "", new DateTime(2013, 5, 31), apsoilDownloadUrl, null, null) };
        }

        /// <summary>
        /// Get all available versions of old apsim.
        /// </summary>
        /// <param name="n">Maximum number of versions to return.</param>
        private static List<ProductVersion> GetApsimClassicUpgrades(int n)
        {
            if (n <= 0)
                n = 1000; // fixme

            List<BuildJob> upgrades = WebUtilities.CallRESTService<List<BuildJob>>($"http://apsimdev.apsim.info/APSIM.Builds.Service/BuildsClassic.svc/GetReleases?numRows={n}");

            // fixme - we need to upload all of the legacy installers to apsim.info but need
            // more disk space to do so. In the meantime I've uploaded the versions where
            // we incremented the version number.
            //int[] versionsToKeep = new[] { 402, 700, 1017, 1388, 2287, 3009, 3377, 3616, 3868, 4047, 4159 };
            upgrades = upgrades.Where(u => u.BuiltOnJenkins).ToList();

            // fixme - start time is not the same as merge time!
            List<ProductVersion> result = upgrades.Select(u => new ProductVersion(u.Description, u.VersionString, u.IssueURL, u.StartTime, u.WindowsInstallerURL, u.LinuxBinariesURL, null)).ToList();
            result.AddRange(GetStaticApsimClassicVersions());
            if (result.Count > n)
                result = result.Take(n).ToList();

            return result;
        }

        /// <summary>
        /// Get the historical apsim classic major release versions.
        /// </summary>
        private static IEnumerable<ProductVersion> GetStaticApsimClassicVersions()
        {
            string filesUrl = "https://apsimdev.apsim.info/APSIMClassicFiles/";
            yield return new ProductVersion("APSIM 7.10", "Apsim7.10-r4159", "", new DateTime(2018, 4, 1), filesUrl + "Apsim7.10-r4159.ApsimSetup.exe", null, null);
            yield return new ProductVersion("APSIM 7.9", "Apsim7.9-r4047", "", new DateTime(2017, 6, 14), filesUrl + "Apsim7.9-r4047.ApsimSetup.exe", null, null);
            yield return new ProductVersion("APSIM 7.8", "Apsim7.8-r3868", "", new DateTime(2016, 3, 29), filesUrl + "Apsim7.8-r3868.ApsimSetup.exe", null, null);
            yield return new ProductVersion("APSIM 7.7", "Apsim7.7-r3616", "", new DateTime(2014, 12, 16), filesUrl + "Apsim7.7-r3616.ApsimSetup.exe", null, null);
            yield return new ProductVersion("APSIM 7.6", "Apsim7.6-r3377", "", new DateTime(2014, 3, 24), filesUrl + "Apsim7.6-r3377.ApsimSetup.exe", null, null);
            yield return new ProductVersion("APSIM 7.5", "Apsim7.5-r3009", "", new DateTime(2013, 4, 15), filesUrl + "Apsim7.5-r3009.ApsimSetup.exe", null, null);
            yield return new ProductVersion("APSIM 7.4", "Apsim7.4-r2287", "", new DateTime(2012, 2, 13), filesUrl + "Apsim7.4-r2287.ApsimSetup.exe", null, null);
            yield return new ProductVersion("APSIM 7.3", "Apsim7.3-r1388", "", new DateTime(2011, 2, 27), filesUrl + "Apsim7.3-r1388.ApsimSetup.exe", null, null);
            yield return new ProductVersion("APSIM 7.2", "Apsim7.2-r1017", "", new DateTime(2010, 8, 23), filesUrl + "Apsim7.2-r1017.ApsimSetup.exe", null, null);
            yield return new ProductVersion("APSIM 7.1", "Apsim7.1-r700", "", new DateTime(2009, 11, 14), filesUrl + "Apsim7.1-r700.ApsimSetup.exe", null, null);
            yield return new ProductVersion("APSIM 7.0", "Apsim7.0-r402", "", new DateTime(2009, 4, 26), filesUrl + "Apsim7.0-r402.ApsimSetup.exe", null, null);
            yield return new ProductVersion("APSIM 6.1", "Apsim6.1", "", new DateTime(2008, 5, 15), filesUrl + "Apsim6.1Setup.exe", null, null);
            yield return new ProductVersion("APSIM 6.0", "Apsim6.0", "", new DateTime(2008, 3, 27), filesUrl + "Apsim6.0Setup.exe", null, null);
        }

        /// <summary>
        /// Get all available apsim versions.
        /// </summary>
        /// <param name="n">Maximum number of versions to return.</param>
        private static List<ProductVersion> GetApsimXUpgrades(int n)
        {
            List<Upgrade> upgrades = WebUtilities.CallRESTService<List<Upgrade>>($"https://apsimdev.apsim.info/APSIM.Builds.Service/Builds.svc/GetLastNUpgrades?n={n}");
            return upgrades.Select(u => new ProductVersion(u)).ToList();
        }

        /// <summary>
        /// Send an invoice email. Only applicable to subscribers under a commercial license.
        /// </summary>
        private async Task SendInvoiceEmail()
        {
            MailMessage email = new MailMessage();
            email.From = new MailAddress("no-reply@www.apsim.info");
#if DEBUG
            email.To.Add("drew.holzworth@csiro.au"); // debug
#else
            email.To.Add("apsim@csiro.au"); // prod
#endif
            email.Subject = "APSIM Commercial Registration Notification";
            email.IsBodyHtml = true;

            StringBuilder body = new StringBuilder();
            body.AppendLine("<p>This is an automated notification of an APSIM commercial license agreement.<p>");
            body.AppendLine("<table>");
            body.AppendLine($"<tr><td>Product</td><td>{RegistrationDetails.Product}</td>");
            body.AppendLine($"<tr><td>Version</td><td>{RegistrationDetails.Version}</td>");
            body.AppendLine($"<tr><td>First name</td><td>{RegistrationDetails.FirstName}</td>");
            body.AppendLine($"<tr><td>Last name</td><td>{RegistrationDetails.LastName}</td>");
            body.AppendLine($"<tr><td>Organisation</td><td>{RegistrationDetails.Organisation}</td>");
            body.AppendLine($"<tr><td>Country</td><td>{RegistrationDetails.Country}</td>");
            body.AppendLine($"<tr><td>Email</td><td>{RegistrationDetails.Email}</td>");
            body.AppendLine($"<tr><td>Licence type</td><td>{RegistrationDetails.LicenceType}</td>");
            body.AppendLine($"<tr><td>Licensor name</td><td>{RegistrationDetails.LicensorName}</td>");
            body.AppendLine($"<tr><td>Licensor email</td><td>{RegistrationDetails.LicensorEmail}</td>");
            body.AppendLine($"<tr><td>Contractor turnover</td><td>{RegistrationDetails.CompanyTurnover}</td>");
            body.AppendLine($"<tr><td>Company registration number</td><td>{RegistrationDetails.CompanyRego}</td>");
            body.AppendLine($"<tr><td>Company Address</td><td>{RegistrationDetails.CompanyAddress}</td>");
            body.AppendLine("</table>");

            email.Body = body.ToString();

            SmtpClient smtp = CreateMailClient();
            await smtp.SendMailAsync(email);
        }

        /// <summary>
        /// Send an email to the user.
        /// </summary>
        private async Task SendRegistrationEmail()
        {
            try
            {
                MailMessage email = new MailMessage();
                email.IsBodyHtml = true;
                email.From = new MailAddress(emailFromAddress);
                email.To.Add(RegistrationDetails.Email);
                email.Subject = GetRegistrationEmailSubject(RegistrationDetails.LicenceType);
                email.Body = await GetEmailBody(RegistrationDetails.LicenceType, RegistrationDetails.Product, RegistrationDetails.Version, RegistrationDetails.Platform);

                email.Attachments.Add(CreateLicenseFileAttachment(RegistrationDetails.LicenceType));
                email.Attachments.Add(CreateReferencingGuideAttachment());

                SmtpClient smtp = CreateMailClient();
                await smtp.SendMailAsync(email);
            }
            catch (Exception error)
            {
                throw new Exception("Encounterd an error while attempting to generate email.", error);
            }
        }

        /// <summary>
        /// Get an email body for the given registration details.
        /// </summary>
        private static async Task<string> GetEmailBody(LicenceType license, string productName, string versionName, string platform)
        {
            bool commercial = license == LicenceType.Commercial;
            string mailBodyFile = commercial ? commercialEmailBody : nonCommercialEmailBody;
            string body = await ReadResourceFile(mailBodyFile);

            Product product = GetProduct(productName);
            ProductVersion version = GetProductVersion(product, versionName);

            string downloadUrl = GetDownloadURL(version, platform);
            body = body.Replace("$DownloadURL$", downloadUrl);
            body = body.Replace("$PASSWORD$", GetProductPassword(productName, versionName));
            body = body.Replace("$MSI$", GetMsiLink(product, version));
            return body;
        }

        /// <summary>
        /// Create an email attachment containing the guide to referencing apsim in publications.
        /// </summary>
        private Attachment CreateReferencingGuideAttachment()
        {
            Stream referencingStream = GetResourceStream(referencingGuideFileName);
            Attachment attachment = new Attachment(referencingStream, new ContentType(pdfMimeType));
            attachment.ContentDisposition.FileName = referencingGuideAttachmentDisplayName;
            return attachment;
        }

        /// <summary>
        /// Create an email attachment containing the license file.
        /// </summary>
        /// <param name="licenceType">License type.</param>
        private Attachment CreateLicenseFileAttachment(LicenceType licenceType)
        {
            bool commercial = licenceType == LicenceType.Commercial;
            string licenceFileName = commercial ? commercialLicencePdf : nonCommercialLicencePdf;
            Stream licenseStream = GetResourceStream(licenceFileName);
            Attachment attachment = new Attachment(licenseStream, new ContentType(pdfMimeType));
            attachment.ContentDisposition.FileName = licenseAttachmentDisplayName;
            return attachment;
        }

        /// <summary>
        /// Find a product with the given version name. Throw if not found.
        /// </summary>
        /// <param name="product">The product.</param>
        /// <param name="versionName">Version name.</param>
        private static ProductVersion GetProductVersion(Product product, string versionName)
        {
            if (versionName == versionNameLatest)
            {
                ProductVersion version = product.GetVersions(1).FirstOrDefault();
                if (version == null)
                    throw new Exception($"Prodcut {product.Name} has no versions available for download");
                return version;
            }
            List<ProductVersion> versions = product.GetVersions(0);
            ProductVersion match = versions.FirstOrDefault(v => v.Number == versionName);
            if (match == null)
                match = versions.FirstOrDefault(v => v.Number.Contains(versionName));
            if (match == null)
                throw new ArgumentException($"Unable to find {product.Name} version {versionName}");
            return match;
        }

        /// <summary>
        /// Find a product with the given name. Throw if not found.
        /// </summary>
        /// <param name="product">Name of the product.</param>
        private static Product GetProduct(string productName)
        {
            Product product = Products.FirstOrDefault(p => p.Name == productName);
            if (product == null)
                throw new ArgumentException($"Unknown product {productName}");
            return product;
        }

        /// <summary>
        /// Get an MSI link for the given product.
        /// </summary>
        /// <param name="product">Name of the product.</param>
        /// <param name="url"></param>
        private static string GetMsiLink(Product product, ProductVersion version)
        {
            // The .msi files seem to have gone missing. I'm just going to
            // disable this option for now.
            return string.Empty;

            /*
            int versionNumber = GetClassicVersionNumber(version);

            // if (product.Name == apsimName || versionNumber <= 73)
                // Old APSIM 7.3 or earlier (or nextgen).
                return string.Empty;

            string msi = Path.ChangeExtension(version.DownloadLinkWindows, ".msi");
            StringBuilder msg = new StringBuilder();
            msg.Append("<p>");
            msg.Append("To download a version of APSIM that doesn't check for the required Microsoft ");
            msg.Append($"runtime libraries <a href={msi}>click here</a>. This can be useful ");
            msg.Append("when APSIM won't install because it thinks the required runtimes aren't present.");
            msg.Append("</p>");
            return msg.ToString();
            */
        }

        private static int GetClassicVersionNumber(ProductVersion version)
        {
            if (double.TryParse(version.Number, out double d))
                return Convert.ToInt32(d * 10);
            return int.MaxValue;
        }

        /// <summary>
        /// Get the password required for the given product.
        /// </summary>
        /// <returns></returns>
        private static string GetProductPassword(string productName, string version)
        {
            string password = "No password is required for this release of APSIM.";
            if (productName == oldApsimName)
            {
                if (double.TryParse(version, out double d))
                {
                    int versionNo = Convert.ToInt32(d * 10);
                    if (version != "7.10")
                    {
                        if (versionNo == 73)
                            password = "The password for this release of APSIM is: <b>hety-9895-yrw-6040</b>";
                        else if (versionNo == 72)
                            password = "The password for this release of APSIM is: <b>fedt-1141-hsd-2051</b>";
                        else if (versionNo < 72)
                            password = "The Password for this release of APSIM is <b>" + version.Replace(".", "") + "0004860</b>";
                    }
                }
            }
            return password;
        }

        /// <summary>
        /// Get a download URL for the given product name and platform.
        /// </summary>
        /// <param name="productName">Product name.</param>
        /// <param name="platform">Product platform (e.g. Linux).</param>
        private static string GetDownloadURL(ProductVersion version, string platform)
        {
            if (platform == "Linux")
                return version.DownloadLinkLinux;
            if (platform == "MacOS")
                return version.DownloadLinkMac;
            return version.DownloadLinkWindows;
        }

        /// <summary>
        /// Read a resource file.
        /// </summary>
        /// <param name="file">Name of the resource file.</param>
        private static async Task<string> ReadResourceFile(string file)
        {
            using (StreamReader reader = new StreamReader(GetResourceStream(file)))
                return await reader.ReadToEndAsync();
        }

        /// <summary>
        /// Get a stream for a given embedded resource file. If no embedded resource
        /// matches the given file name exactly, return a stream for the first
        /// resource whose file name contains the given file name. If no match is
        /// found, an exception will be thrown.
        /// </summary>
        /// <param name="name">Name of the embedded resource.</param>
        private static Stream GetResourceStream(string name)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(name);
            if (stream != null)
                return stream;

            string resourceName = assembly.GetManifestResourceNames().FirstOrDefault(n => n.Contains(name));
            if (string.IsNullOrEmpty(resourceName))
                throw new ArgumentException($"Unknown resource file {name}");
            return assembly.GetManifestResourceStream(resourceName);
        }

        /// <summary>
        /// Create a mail client for our sendgrid account. Reads configuration
        /// settings from environment variables.
        /// </summary>
        private SmtpClient CreateMailClient()
        {
            string host = ReadEnvironmentVariable(smtpHostVariableName);
            string user = ReadEnvironmentVariable(smtpUsernameVariableName);
            string token = ReadEnvironmentVariable(smtpTokenVariableName);
            string portNo = ReadEnvironmentVariable(smtpPortVariableName);
            if (!int.TryParse(portNo, out int port))
                throw new ArgumentException($"Unable to parse port number '{portNo}' as integer");

            SmtpClient smtp = new SmtpClient(host, port);
            smtp.Credentials = new NetworkCredential(user, token);
            return smtp;
        }

        /// <summary>
        /// Read an environment variable. Throw if not set.
        /// </summary>
        /// <param name="variableName">Name of the variable to be read.</param>
        private static string ReadEnvironmentVariable(string variableName)
        {
            string value = Environment.GetEnvironmentVariable(variableName);
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(variableName, $"Environment variable not set: {variableName}");
            return value;
        }

        /// <summary>
        /// Retrieve a subject line for a registration email for the given license type.
        /// </summary>
        /// <param name="licenseType">Type of license.</param>
        private string GetRegistrationEmailSubject(LicenceType licenseType)
        {
            if (licenseType == LicenceType.Commercial)
                return "APSIM Software Commercial Licence";

            return "APSIM Software Non-Commercial Licence";
        }
    }
}
