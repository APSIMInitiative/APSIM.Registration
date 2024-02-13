using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using APSIM.Registration.Controllers;
using APSIM.Registration.Data;
using APSIM.Registration.Models;
using APSIM.Registration.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace APSIM.Registration.Pages
{
    public class IndexModel : PageModel
    {
        private const string buildsApi = "https://builds.apsim.info";
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
        private const string specialUseEmailBody = "EmailBodyCommercial.html";
        private const string generalUseEmailBody = "EmailBody.html";
        private const string referencingGuideFileName = "referencing-guide.pdf";
        private const string referencingGuideAttachmentDisplayName = "Guide to Referencing APSIM in Publications.pdf";
        private const string specialUseLicencePdf = "APSIM_Special_Use_Licence.pdf";
        private const string generalUseLicencePdf = "APSIM_General_Use_licence.pdf";
        private const string licenseAttachmentDisplayName = "APSIM License.pdf";
        private const string emailCookieName = "Email";
        public IReadOnlyList<Organisation> AiMembers { get; private init; }

        static IndexModel()
        {
            UpdateProducts();
        }

        internal static void UpdateProducts()
        {
            Products = new List<Product>()
            {
                new Product(apsimName, GetAllApsimXUpgrades()),
                new Product(oldApsimName, GetApsimClassicUpgrades()),
                new Product(apsoilName, GetApsoilVersions()),
            };
        }

        public static IReadOnlyList<Product> Products { get; private set; }

        public enum View
        {
            LandingPage,
            RegistrationForm,
            Downloads
        }
        private static readonly CookieOptions cookieOptions = new CookieOptions()
        {
            Secure = true,
            HttpOnly = true,
            SameSite = SameSiteMode.Strict
        };

        [BindProperty]
        public View Sender { get; set; }

        private readonly IDbContextGenerator<RegistrationsDbContext> generator;
        private readonly RegistrationController controller;
        private readonly ILogger<IndexModel> logger;

        /// <summary>
        /// Max number of rows of product versions to show on the downloads page.
        /// </summary>
        [BindProperty]
        public int NumDownloads { get; set; } = 10;

        /// <summary>
        /// This is bound to a checkbox displayed to the user. If true (checked),
        /// the user wants to subscribe to the mailing list.
        /// </summary>
        [BindProperty]
        public bool Subscribe { get; private set; }

        /// <summary>
        /// This is bound to the inputs on the registration page.
        /// </summary>
        [BindProperty]
        public Models.Registration RegistrationDetails { get; set; }

        /// <summary>
        /// Filter for versions to be displayed to the user.
        /// </summary>
        public string VersionFilter { get; private set; }

        /// <summary>
        /// Product filter for versions to be displayed to the user.
        /// </summary>
        public string ProductFilter { get; private set; }

        /// <summary>
        /// Create a new <see cref="RegisterModel"/> instance.
        /// </summary>
        /// <param name="context">The registrations database context.</param>
        public IndexModel(IDbContextGenerator<RegistrationsDbContext> generator, RegistrationController controller, ILogger<IndexModel> logger)
        {
            this.generator = generator;
            this.controller = controller;
            this.logger = logger;

            AiMembers = new List<Organisation>()
            {
                new Organisation("CSIRO", "csiro.au"),
                new Organisation("UQ", "uq.edu.au"),
                new Organisation("USQ", "usq.edu.au"),
                new Organisation("AgResearch", "agresearch.co.nz"),
                new Organisation("Plant and Food Research", "plantandfood.co.nz"),
                new Organisation("Iowa State University", "iastate.edu"),
                new Organisation("DAF", "daf.qld.gov.au")
            };
        }

        /// <summary>
        /// Create the landing page view.
        /// </summary>
        private ActionResult LandingPage()
        {
            return Page();
        }

        /// <summary>
        /// Create the registration form view.
        /// </summary>
        private ActionResult RegistrationForm(string email)
        {
            RegistrationDetails = new Models.Registration();
            RegistrationDetails.Product = apsimName;
            RegistrationDetails.Version = versionNameLatest;
            RegistrationDetails.Platform = "Windows";
            RegistrationDetails.LicenceType = LicenceType.GeneralUse;
            RegistrationDetails.Country = "Australia";
            RegistrationDetails.Email = email;
            Organisation org = FindAIMember(RegistrationDetails.Email);
            if (org != null)
                RegistrationDetails.Organisation = org.Name;
            if (!string.IsNullOrEmpty(VersionFilter))
            {
                (Product product, ProductVersion version) = GetProductVersion(VersionFilter);
                if (product != null && version != null)
                {
                    RegistrationDetails.Product = product.Name;
                    RegistrationDetails.Version = version.Number;
                }
            }

            return Partial("RegistrationForm", this);
        }

        /// <summary>
        /// Create the downloads page view.
        /// </summary>
        private ActionResult Downloads()
        {
            return Partial("Downloads", this);
        }

        /// <summary>
        /// Handler for GET requests.
        /// </summary>
        /// <remarks>
        /// Should only be called when the user navigates to the registration website.
        /// </remarks>
        public async Task<ActionResult> OnGetAsync(string email, string product, string version, string platform)
        {
            return await HandleRequest(email, product, version, platform);
        }

        /// <summary>
        /// Handler for POST requests.
        /// </summary>
        public async Task<ActionResult> OnPostAsync(string email, string product, string version, string platform)
        {
            return await HandleRequest(email, product, version, platform);
        }

        public async Task<ActionResult> HandleRequest(string email, string product, string version, string platform)
        {
            email = SaveToSession(email, emailCookieName);
            if (!string.IsNullOrEmpty(version))
                VersionFilter = version;
            if (!string.IsNullOrEmpty(product))
                ProductFilter = product;
            else
                ProductFilter = apsimName;

            // If we're here from the registration form, perform the registration.
            if (Sender == View.RegistrationForm)
                return await OnPostSubmitAsync();

            // If no email has been provided, go to landing page.
            if (string.IsNullOrEmpty(email))
                return LandingPage();

            // If controller is registered, show the downloads page.
            if (controller.IsRegistered(email).Value)
            {
                // If product, version, or platform have not been provided,
                // just show the downloads page.
                if (string.IsNullOrEmpty(version))
                    return Downloads();

                if (string.IsNullOrEmpty(platform))
                {
                    // User requested a specific product and version, but not
                    // platform. We show the downloads table, filtered to show
                    // only rows matching the given version filter.

                    return Downloads();
                }

                // Attempt to find a matching product version. If nothing is found,
                // serve the downloads page and log a warning message (server-side).
                (Product actualProduct, ProductVersion productVersion) = GetProductVersion(version);
                if (productVersion == null)
                {
                    logger.LogWarning($"Unable to serve {product} {version} - unknown product or version.");
                    return Downloads();
                }

                // If a product, version, and platform have all been provided,
                // we can serve that file. First though, we attempt to register
                // an upgrade to the DB under the user's email address.
                try
                {
                    await controller.UpgradeAsync(email, productVersion.Number, platform);
                }
                catch (Exception err)
                {
                    logger.LogError(err, $"Failed to log an upgrade");
                }

                // Finally, serve the file.
                return Redirect(GetDownloadURL(productVersion, platform));
            }

            // If we get to this point, an email address has been provided,
            // but there is no registration associated with the email in the DB.
            // Therefore we need to show the registration form.
            return RegistrationForm(email);
        }

        /// <summary>
        /// Handler for /register endpoint. This will delete the email cookie
        /// and return a redirect to the current page, allowing a user to re-
        /// register, or register with another email address.
        /// </summary>
#pragma warning disable 1998
        public async Task<ActionResult> OnGetRegisterAsync()
#pragma warning restore 1998
        {
            HttpContext.Request.Cookies.TryGetValue(emailCookieName, out string email);
            return RegistrationForm(email);
        }

        /// <summary>
        /// Handler for POST requests to the Submit endpoint. This is called
        /// when the user clicks 'submit' on the registration form.
        /// </summary>
        public async Task<ActionResult> OnPostSubmitAsync()
        {
            // Ensure registration details are valid.
            if (!ModelState.IsValid)
                return RegistrationForm(RegistrationDetails.Email);

            // Registration details are valid. Store the user's email address in
            // the session data for later retrieval.
            HttpContext.Response.Cookies.Append(emailCookieName, RegistrationDetails.Email, cookieOptions);

            // Perform some administrative tasks. Any errors will be logged but
            // not displayed to the user, who just wants to see the downloads page.
            try
            {
                // Write registration to DB.
                logger.LogDebug($"Adding new registration to registrations DB...");
                controller.Register(RegistrationDetails);

                // Optionally subscribe to mailing list.
                if (Subscribe)
                {
                    logger.LogDebug("Subscribing to the mailing list...");
                    controller.Subscribe(RegistrationDetails.Email);
                }

                // Send registration email.
                logger.LogDebug("Sending registration email...");
                await SendRegistrationEmail();

                // Send invoice email.
                if (RegistrationDetails.LicenceType == LicenceType.SpecialUse)
                {
                    logger.LogDebug($"Sending invoice email...");
                    await SendInvoiceEmail();
                }
            }
            catch (Exception err)
            {
                logger.LogError(err, $"An error occurred while performing registration");
            }

            return Downloads();
        }

        private string SaveToSession(string value, string cookieName)
        {
            // If no value provided - use provided cookie value (if available).
            if (string.IsNullOrEmpty(value))
            {
                if (HttpContext.Request.Cookies.TryGetValue(cookieName, out string cookieValue))
                    return cookieValue;
                return null;
            }

            // Otherwise, save this value as a cookie.
            HttpContext.Response.Cookies.Append(cookieName, value, cookieOptions);
            return value;
        }

        /// <summary>
        /// Find a product version with a matching version name.
        /// </summary>
        /// <param name="versionName">Version name.</param>
        private (Product, ProductVersion) GetProductVersion(string versionName)
        {
            // fixme - slow
            foreach (Product product in Products)
            {
                ProductVersion version = GetProductVersion(product, versionName);
                if (version != null)
                    return (product, version);
            }
            return (null, null);
        }

        /// <summary>
        /// Get all apsoil versions available for download.
        /// </summary>
        private static IReadOnlyList<ProductVersion> GetApsoilVersions()
        {
            return new List<ProductVersion>() { new ProductVersion(apsoilName, "7.1", "", new DateTime(2013, 5, 31), apsoilDownloadUrl, null, null) };
        }

        /// <summary>
        /// Get all available versions of old apsim.
        /// </summary>
        private static IReadOnlyList<ProductVersion> GetApsimClassicUpgrades()
        {
            // If version filter is provided, fetch all available versions from
            // builds API.
            // if (n <= 0 || !string.IsNullOrEmpty(versionFilter))
            //     n = 1000; // fixme

            List<BuildJob> upgrades = WebUtilities.CallRESTService<List<BuildJob>>($"{buildsApi}/api/oldapsim/list");

            // fixme - we need to upload all of the legacy installers to apsim.info but need
            // more disk space to do so. In the meantime I've uploaded the versions where
            // we incremented the version number.
            //int[] versionsToKeep = new[] { 402, 700, 1017, 1388, 2287, 3009, 3377, 3616, 3868, 4047, 4159 };
            upgrades = upgrades.Where(u => u.BuiltOnJenkins).ToList();

            // fixme - start time is not the same as merge time!
            List<ProductVersion> result = upgrades.Select(u => new ProductVersion(u.Description, u.VersionString, u.IssueURL, u.StartTime, u.WindowsInstallerURL, u.LinuxBinariesURL, null)).ToList();
            result.AddRange(GetStaticApsimClassicVersions());
            // if (result.Count > n)
            //     result = result.Take(n).ToList();

            // if (!string.IsNullOrEmpty(versionFilter))
            //     result = result.Where(v => v.Number.Contains(versionFilter)).ToList();

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
        private static IReadOnlyList<ProductVersion> GetAllApsimXUpgrades()
        {
            // todo: proper async all the way up
            Task<List<Release>> releases = WebUtilities.PostAsync<List<Release>>($"{buildsApi}/api/nextgen/list");
            releases.Wait();
            return releases.Result.Select(u => new ProductVersion(u)).ToList();
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
            email.Subject = "APSIM Special Use Registration Notification";
            email.IsBodyHtml = true;

            StringBuilder body = new StringBuilder();
            body.AppendLine("<p>This is an automated notification of an APSIM Special Use license agreement.<p>");
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
        private async Task<string> GetEmailBody(LicenceType license, string productName, string versionName, string platform)
        {
            bool commercial = license == LicenceType.SpecialUse;
            string mailBodyFile = commercial ? specialUseEmailBody : generalUseEmailBody;
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
            bool commercial = licenceType == LicenceType.SpecialUse;
            string licenceFileName = commercial ? specialUseLicencePdf : generalUseLicencePdf;
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
        /// <remarks>
        /// fixme: this is slow
        /// </remarks>
        private static ProductVersion GetProductVersion(Product product, string versionName)
        {
            if (versionName == versionNameLatest)
            {
                ProductVersion version = product.Versions.FirstOrDefault();
                if (version == null)
                    throw new Exception($"Prodcut {product.Name} has no versions available for download");
                return version;
            }
            IEnumerable<ProductVersion> versions = product.Versions;
            ProductVersion match = versions.FirstOrDefault(v => v.Number == versionName);
            if (match == null)
                match = versions.FirstOrDefault(v => v.Number.Contains(versionName));
            return match;
        }

        /// <summary>
        /// Find a product with the given name. Throw if not found.
        /// </summary>
        /// <param name="product">Name of the product.</param>
        private Product GetProduct(string productName)
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

        public Organisation FindAIMember(string email)
        {
            if (string.IsNullOrEmpty(email))
                return null;
            email = email.Trim();
            return AiMembers.FirstOrDefault(m => email.EndsWith(m.DomainName, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// Check if an email address belongs to a member of the APSIM Initiative.
        /// </summary>
        /// <param name="email">Email address to be checked.</param>
        public bool IsAIMember(string email)
        {
            return FindAIMember(email) != null;
        }

        public string SubmitButtonText()
        {
            return IsAIMember(RegistrationDetails.Email) ? "Submit" : "I agree to the T&Cs below. Proceed to Downloads";
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
            if (licenseType == LicenceType.SpecialUse)
                return "APSIM Software Special Use Licence";

            return "APSIM Software General Use Licence";
        }
    }
}
