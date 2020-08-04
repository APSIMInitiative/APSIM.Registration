using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using APSIM.Shared.Utilities;
using System.Reflection;
using System.Net.Mail;
using System.Text;
using System.Data;
using System.Net;

namespace ProductRegistration
{

    public partial class Registration : System.Web.UI.Page
    {
        /// <summary>
        /// Page has loaded - setup all controls.
        /// </summary>
        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (Product.Items.Count == 0)
            {
                Product.Items.Add("APSIM");
                Product.Items.Add("Apsoil");
                Product.Items.Add("HowWet?");
                Product.Items.Add("HowOften?");
                Product.Items.Add("HowMuch?");
                Product.Items.Add("HowLeaky?");
                Product.Items.Add("Perfect");
                Country.Items.AddRange(Constants.Countries.Select(c => new ListItem(c)).ToArray());
                if (Request.QueryString["Product"] != null)
                {
                    Product.Text = Product.Text = Request.QueryString["Product"];
                }

                List<string> Versions = new List<string>();
                Version.Items.Clear();
                string DownloadDirectory = Path.Combine(Request.PhysicalApplicationPath, "Downloads");
                if (Directory.Exists(DownloadDirectory))
                {
                    foreach (string SubDirectory in Directory.GetDirectories(DownloadDirectory))
                    {
                        foreach (string FileName in Directory.GetFiles(SubDirectory, "Apsim*.exe"))
                        {
                            string VersionNumber = Path.GetFileNameWithoutExtension(FileName).Replace("Apsim", "");
                            VersionNumber = VersionNumber.Replace("Setup", "");
                            Versions.Add(VersionNumber);
                        }
                    }
                }
                Versions.Sort(new VersionComparer());
                Versions.Reverse();
                Versions.Add("Next Generation (Windows)");
                Versions.Add("Next Generation (Debian)");
                Versions.Add("Next Generation (Mac)");
                foreach (string VersionString in Versions)
                    Version.Items.Add(VersionString);
            }
        }

        /// <summary>
        /// User has clicked yes. 
        /// </summary>
        protected void YesButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (ControlsAreValid())
                {
                    WriteToLogFile("User has submitted form with valid information.", MessageType.Info);
                    UpdateDB();
                    if (radioCom.Checked)
                    {
                        WriteToLogFile("User is registering under a commercial license. Sending invoice email to Sarah...", MessageType.Info);
                        SendInvoiceEmail();
                    }
                    string RedirectURL;
                    if (GetProductName().ToLower().Contains("apsim"))
                    {
                        WriteToLogFile("User wants to download APSIM.", MessageType.Info);
                        SendEmail();
                        RedirectURL = "Downloads.aspx";
                    }
                    else
                    {
                        WriteToLogFile("User has registered for a product that is not APSIM.", MessageType.Info);
                        RedirectURL = "Thankyou.aspx?URL=" + GetDownloadURL();
                    }
                    Response.Redirect(RedirectURL, false);
                    Context.ApplicationInstance.CompleteRequest();
                }
            }
            catch (Exception error)
            {
                WriteToLogFile(error.ToString(), MessageType.Error);
                throw error;
            }
        }
        protected void NoButton_Click(object sender, EventArgs e)
        {
            Response.Redirect(Request.UrlReferrer.AbsoluteUri, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// Update the registrations database.
        /// </summary>
        private void UpdateDB()
        {
            try
            {
                string url = "https://apsimdev.apsim.info/APSIM.Registration.Service/Registration.svc/AddRegistration" +
                                "?firstName=" + FirstName.Text +
                                "&lastName=" + LastName.Text +
                                "&organisation=" + Organisation.Text +
                                "&country=" + Country.Text +
                                "&email=" + Email.Text +
                                "&product=" + GetRealProductName() +
                                "&version=" + GetVersion() +
                                "&platform=" + GetPlatform() +
                                "&type=Registration";

                WriteToLogFile("Updating DB. Request: " + url, MessageType.Info);
                try
                {
                    WebUtilities.CallRESTService<object>(url);
                    WriteToLogFile("DB updated successfully.", MessageType.Info);
                }
                catch (Exception error)
                {
                    throw new Exception("Encountered an error while writing to DB.", error);
                }

                // Subscribe if subscribe checkbox is checked.
                if (ChkSubscribe.Checked)
                {
                    url = $"https://apsimdev.apsim.info/APSIM.Registration.Service/Registration.svc/Subscribe?email={Email.Text}";
                    WriteToLogFile("Subscribing to mailing list. Request: " + url, MessageType.Info);
                    try
                    {
                        WebUtilities.CallRESTService<object>(url);
                        WriteToLogFile("Subscribed to mailing list", MessageType.Info);
                    }
                    catch (Exception err)
                    {
                        // For now, don't let a failed subscription cause the entire process to fail.
                        WriteToLogFile(err.ToString(), MessageType.Error);
                        //throw new Exception("Encountered an error while subscribing to mailing list.", err);
                    }
                }
            }
            catch (Exception err)
            {
                // If something goes wrong then that's unfortunate but it shouldn't impact the user.
                WriteToLogFile(err.ToString(), MessageType.Error);
            }
        }

        private void SendInvoiceEmail()
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
            body.AppendLine($"<tr><td>Product</td><td>{GetRealProductName()}</td>");
            body.AppendLine($"<tr><td>Version</td><td>{GetVersion()}</td>");
            body.AppendLine($"<tr><td>First name</td><td>{FirstName.Text}</td>");
            body.AppendLine($"<tr><td>Last name</td><td>{LastName.Text}</td>");
            body.AppendLine($"<tr><td>Organisation</td><td>{Organisation.Text}</td>");
            body.AppendLine($"<tr><td>Country</td><td>{Country.Text}</td>");
            body.AppendLine($"<tr><td>Email</td><td>{Email.Text}</td>");
            body.AppendLine($"<tr><td>Licence type</td><td>{GetLicenseType()}</td>");
            body.AppendLine($"<tr><td>Licensor name</td><td>{LicensorName.Text}</td>");
            body.AppendLine($"<tr><td>Licensor email</td><td>{LicensorEmail.Text}</td>");
            body.AppendLine($"<tr><td>Contractor turnover</td><td>{GetContractorTurnover()}</td>");
            body.AppendLine($"<tr><td>Company registration number</td><td>{companyID.Text}</td>");
            body.AppendLine($"<tr><td>Company Address</td><td>{companyAddress.Text}</td>");
            body.AppendLine("</table>");

            email.Body = body.ToString();
            string[] creds = File.ReadAllLines(@"D:\Websites\email.txt");
            SmtpClient smtp = new SmtpClient(creds[0]);
            smtp.Port = Convert.ToInt32(creds[1]);
            smtp.Credentials = new NetworkCredential(creds[2], creds[3]);
            smtp.Send(email);
        }

        /// <summary>
        /// Send an email to the user.
        /// </summary>
        private void SendEmail()
        {
            try
            {
                System.Net.Mail.MailMessage Mail = new System.Net.Mail.MailMessage();
                Mail.From = new System.Net.Mail.MailAddress("no-reply@www.apsim.info");
                Mail.To.Add(Email.Text);
                if (radioCom.Checked)
                    Mail.Subject = "APSIM Software Commercial Licence";
                else
                    Mail.Subject = "APSIM Software Non-Commercial Licence";
                Mail.IsBodyHtml = true;

                string mailBodyFile = "EmailBody.html";
                if (radioCom.Checked)
                    mailBodyFile = "EmailBodyCommercial.html";
                string body = File.ReadAllText(Path.Combine(Request.PhysicalApplicationPath, mailBodyFile));

                string DownloadURL = GetDownloadURL();
                WriteToLogFile($"Download URL='{DownloadURL}'", MessageType.Info);
                body = body.Replace("$DownloadURL$", DownloadURL);
                body = body.Replace("$PASSWORD$", GetProductPassword());

                if (GetProductVersion() <= 73 || Version.Text.Contains("Next Generation"))
                {
                    // APSIM 7.3 or earlier.
                    body = body.Replace("$MSI$", "");
                }
                else
                {
                    string DownloadMSI = Path.ChangeExtension(DownloadURL, ".msi");

                    body = body.Replace("$MSI$", "<p>To download a version of APSIM that doesn't check for the required Microsoft " +
                                                 "runtime libraries <a href=" + DownloadMSI + ">click here</a>. This can be useful " +
                                                 "when APSIM won't install because it thinks the required runtimes aren't present.</p>");
                }


                Mail.Body = body;


                string licenceFileName = radioCom.Checked ? "APSIM_Commercial_Licence.pdf" : "APSIM_NonCommercial_RD_licence.pdf";
                string AttachmentFileName = Path.Combine(Request.PhysicalApplicationPath, licenceFileName);
                Mail.Attachments.Add(new Attachment(AttachmentFileName));
                
                AttachmentFileName = Path.Combine(Request.PhysicalApplicationPath, "Guide to Referencing APSIM in Publications.pdf");
                Mail.Attachments.Add(new Attachment(AttachmentFileName));

                string[] creds = File.ReadAllLines(@"D:\Websites\email.txt");

                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient(creds[0]);
                smtp.Port = Convert.ToInt32(creds[1]);
                smtp.Credentials = new System.Net.NetworkCredential(creds[2], creds[3]);
                smtp.Send(Mail);
                WriteToLogFile($"Successfully sent email to '{Email.Text}'.", MessageType.Info);
            }
            catch (Exception error)
            {
                throw new Exception("Encounterd an error while attempting to generate email.", error);
            }
        }

        /// <summary>
        /// Return the user selected product name - e.g.
        /// APSIMNext Generation (Mac)
        /// APSIM7.9
        /// Apsoil
        /// </summary>
        private string GetProductName()
        {
            string ProductName = Product.Text;
            if (ProductName.Contains("APSIM"))
                ProductName += Version.Text;
            return ProductName;
        }

        /// <summary>
        /// Gets the name of the product without version - e.g.
        /// APSIM Next Generation
        /// APSIM
        /// Apsoil
        /// </summary>
        private string GetRealProductName()
        {
            if (Version.Visible && Version.Text != null && Version.Text.Contains("Next Gen"))
                return "APSIM Next Generation";
            return Product.Text;
        }

        /// <summary>
        /// Gets the version of the latest released ApsimX build/upgrade.
        /// </summary>
        private string GetLatestApsimXVersion()
        {
            return WebUtilities.CallRESTService<string>("https://apsimdev.apsim.info/APSIM.Builds.Service/Builds.svc/GetLatestVersion");
        }

        private object GetContractorTurnover()
        {
            if (radioLessThan2Mil.Checked)
                return radioLessThan2Mil.Text;
            if (radio2ToFortyMil.Checked)
                return radio2ToFortyMil.Text;
            if (radioBigBucks.Checked)
                return radioBigBucks.Text;

            return "Unspecified";
        }

        private object GetLicenseType()
        {
            return radioNonCom.Checked ? "Non-Commercial" : "Commercial";
        }

        /// <summary>
        /// Gets the version of the selected product.
        /// </summary>
        private string GetVersion()
        {
            if (Version.Text != null && Version.Visible)
            {
                if (Version.Text.Contains("Next Gen"))
                    return GetLatestApsimXVersion();

                // apsim classic
                return Version.Text;
            }
            return "1";
        }

        private string GetPlatform()
        {
            if (Version.Visible && Version.Text != null && Version.Text.Contains("Next Gen"))
            {
                if (Version.Text.Contains("Mac"))
                    return "Mac";
                if (Version.Text.Contains("Debian") || Product.Text.Contains("Linux"))
                    return "Linux";
            }
            return "Windows";
        }

        /// <summary>
        /// Return the password for the specified product.
        /// </summary>
        private string GetProductPassword()
        {
            string Password = "No password is required for this release of APSIM.";
            if (Version.Visible)
            {
                string VersionNumberString = Version.Text;
                if (!VersionNumberString.Contains("Next Generation"))
                {
                    int VersionNumber = Convert.ToInt32(Convert.ToDouble(VersionNumberString) * 10);
                    if (VersionNumberString != "7.10")
                    {
                        if (VersionNumber == 73)
                            Password = "The password for this release of APSIM is: <b>hety-9895-yrw-6040</b>";
                        else if (VersionNumber == 72)
                            Password = "The password for this release of APSIM is: <b>fedt-1141-hsd-2051</b>";
                        else if (VersionNumber < 72)
                            Password = "The Password for this release of APSIM is <b>" + VersionNumberString.Replace(".", "") + "0004860</b>";
                    }
                }
            }
            return Password;
        }

        /// <summary>
        /// Return the version
        /// </summary>
        private int GetProductVersion()
        {
            string VersionNumberString = Version.Text;
            if (VersionNumberString.Contains("Next Generation"))
                return Int32.MaxValue;
            else
                return Convert.ToInt32(Convert.ToDouble(VersionNumberString) * 10);
        }

        /// <summary>
        /// Return the download URL for the specified product.
        /// </summary>
        private string GetDownloadURL()
        {
            string DownloadDirectory = Path.Combine(Request.PhysicalApplicationPath, "Downloads");
            string OurURL = Request.Url.AbsoluteUri.Replace("Register.aspx", "");
            if (OurURL.Contains('?'))
                OurURL = OurURL.Remove(OurURL.IndexOf('?'));

            string ProductName = GetProductName();
            if (ProductName.Contains("APSIMNext Generation"))
            {
                string st = ProductName;
                string operatingSystem = StringUtilities.SplitOffBracketedValue(ref st, '(', ')');
                string url = "http://apsimdev.apsim.info/APSIM.Builds.Service/Builds.svc/GetURLOfLatestVersion?operatingSystem=" + operatingSystem;
                return WebUtilities.CallRESTService<string>(url);
            }
            ProductName = ProductName.Replace("?", "");

            if (Directory.Exists(DownloadDirectory))
                foreach (string SubDirectory in Directory.GetDirectories(DownloadDirectory))
                {
                    foreach (string FileName in Directory.GetFiles(SubDirectory, "*.exe"))
                    {
                        if (FileName.ToLower().Contains(ProductName.ToLower()))
                        {
                            string URL = FileName.Replace(DownloadDirectory, OurURL + "Downloads");
                            return URL.Replace("\\", "/");
                        }
                    }
                }
            throw new Exception("Cannot find product name : " + ProductName);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var address = new System.Net.Mail.MailAddress(email);
                return address.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Return true if controls are valid.
        /// </summary>
        private bool ControlsAreValid()
        {
            List<string> MissingFields = new List<string>();
            if (string.IsNullOrWhiteSpace(FirstName.Text))
                MissingFields.Add("First name");
            if (string.IsNullOrWhiteSpace(LastName.Text))
                MissingFields.Add("Last name");
            if (string.IsNullOrWhiteSpace(Organisation.Text))
                MissingFields.Add("Organisation");
            if (string.IsNullOrWhiteSpace(Email.Text))
                MissingFields.Add("Email");
            if (! (radioNonCom.Checked || radioCom.Checked) )
                MissingFields.Add("License Type");
            if (radioCom.Checked)
            {
                if (string.IsNullOrWhiteSpace(LicensorName.Text))
                    MissingFields.Add("Licensor Name");
                if (string.IsNullOrWhiteSpace(LicensorEmail.Text))
                    MissingFields.Add("Licensor Email");
                if (!(radioLessThan2Mil.Checked || radio2ToFortyMil.Checked || radioBigBucks.Checked))
                    MissingFields.Add("Contractor Turnover");
                if (string.IsNullOrWhiteSpace(companyID.Text))
                    MissingFields.Add("Company Registration Number");
                if (string.IsNullOrEmpty(companyAddress.Text))
                    MissingFields.Add("Company Address");
            }

            if (MissingFields.Count > 0)
            {
                ErrorLabel.Text = "Error. You haven't entered anything for these fields: " + MissingFields.Aggregate((x, y) => $"{x}, {y}");
                ErrorLabel.Visible = true;
                return false;
            }
            if (!IsValidEmail(Email.Text))
            {
                ErrorLabel.Text = "\nEmail address is invalid.";
                ErrorLabel.Visible = true;
                return false;
            }
            ErrorLabel.Visible = false;
            return true;
        }

        /// <summary>Return the valid password for this web service.</summary>
        private static string GetValidPassword()
        {
            string connectionString = File.ReadAllText(@"D:\Websites\ChangeDBPassword.txt");
            int posPassword = connectionString.IndexOf("Password=");
            return connectionString.Substring(posPassword + "Password=".Length);
        }

        private enum MessageType
        {
            Info,
            Warning,
            Error
        }

        private void WriteToLogFile(string message, MessageType type)
        {
            DateTime now = DateTime.Now;
            string logFile = Path.Combine(Request.PhysicalApplicationPath, "logs", now.ToString("yyyy-MM-dd") + ".log");
            string logFileDir = Path.GetDirectoryName(logFile);
            if (!Directory.Exists(logFileDir))
                Directory.CreateDirectory(logFileDir);

            string output = string.Format("{0} {1}: {2}\n", now.ToString("yyyy-mm-dd HH:mm:ss"), type.ToString(), message);
            File.AppendAllText(logFile, output);
        }

        private class VersionComparer : IComparer<string>
        {
            /// <summary>
            /// Compares two version strings and returns a value indicating whether one is less than, equal to, or greater than the other.
            /// </summary>
            /// <param name="x">The first object to compare.</param>
            /// <param name="y">The second object to compare.</param>
            /// <returns>
            /// A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero <paramref name="x" /> is less than <paramref name="y" />. Zero <paramref name="x" /> equals <paramref name="y" />. Greater than zero <paramref name="x" /> is greater than <paramref name="y" />.
            /// </returns>
            public int Compare(string x, string y)
            {
                int indexPeriod = x.IndexOf('.');
                int xVersion = Convert.ToInt32(x.Substring(indexPeriod+1));
                int yVersion = Convert.ToInt32(y.Substring(indexPeriod+1));
                if (xVersion < yVersion)
                    return -1;
                else if (xVersion > yVersion)
                    return 1;
                else
                    return 0;
            }
        }

    }
}