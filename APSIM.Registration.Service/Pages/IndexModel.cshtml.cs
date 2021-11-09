using System;
using System.Linq;
using System.Threading.Tasks;
using APSIM.Registration.Service.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using APSIM.Registration.Service.Models;
using APSIM.Registration.Service.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using APSIM.Registration.Service.Utilities;
using ISO3166;

namespace APSIM.Registration.Service.Pages
{
    public class IndexModel : PageModel
    {
        private const string apsimName = "APSIM Next Generation";
        private const string oldApsimName = "APSIM Classic";
        private const string apsoilName = "Apsoil";
        private const string apsoilDownloadUrl = "https://apsimdev.apsim.info/ApsoilWeb/ApsoilSetup.exe";

        public IReadOnlyList<Product> Products => new List<Product>()
        {
            new Product(apsimName, GetApsimXUpgrades),
            new Product(oldApsimName, GetApsimClassicUpgrades),
            new Product(apsoilName, GetApsoilVersions),
        };

        private readonly IDbContextGenerator<RegistrationsDbContext> generator;
        private readonly RegistrationController controller;
        private readonly ILogger<IndexModel> logger;

        public int NumDownloads { get; set; } = 10;

        public bool ShowIndex { get; private set; } = true;

        public bool ShowRegistrationForm { get; private set; }

        public bool ShowDownloads { get; private set; }

        [BindProperty(SupportsGet = true)]
        public string Email { get; set; }

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
                await OnPostRegisterAsync();
            else if (ShowDownloads)
            {
                // Do nothing
            }
            else
                CheckIsRegistered(Email);
        }

        public async Task<ActionResult> OnPostRegisterAsync()
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
                controller.Register(RegistrationDetails);
            }
            catch (Exception err)
            {
                logger.LogError(err, "Failed to save registration to database");
            }

            ShowRegistrationForm = false;
            ShowDownloads = true;
            PageResult result = Page();
            return result;
        }

        private void CheckIsRegistered(string email)
        {
            if (string.IsNullOrEmpty(email))
                ShowIndex = true;
            else
            {
                ShowIndex = false;
                ShowRegistrationForm = !IsRegistered(email);
                if (ShowRegistrationForm)
                {
                    RegistrationDetails = new Models.Registration();
                    RegistrationDetails.Product = apsimName;
                    RegistrationDetails.Version = "Latest";
                    RegistrationDetails.Platform = "Windows";
                    RegistrationDetails.LicenceType = LicenceType.NonCommercial;
                    RegistrationDetails.Email = email;
                    RegistrationDetails.Country = "Australia";
                }
                ShowDownloads = !ShowRegistrationForm;
            }
        }

        private bool IsRegistered(string email)
        {
            using (RegistrationsDbContext context = generator.Generate())
            {
                Func<Models.Registration, bool> isRegistered = r => r.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase);
                return context.Registrations.Any(isRegistered);
            }
        }

        private static List<ProductVersion> GetApsoilVersions(int n)
        {
            return new List<ProductVersion>() { new ProductVersion(apsoilName, "7.1", "", new DateTime(2013, 5, 31), apsoilDownloadUrl, null, null) };
        }

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

        private static List<ProductVersion> GetApsimXUpgrades(int n)
        {
            List<Upgrade> upgrades = WebUtilities.CallRESTService<List<Upgrade>>($"https://apsimdev.apsim.info/APSIM.Builds.Service/Builds.svc/GetLastNUpgrades?n={n}");
            return upgrades.Select(u => new ProductVersion(u)).ToList();
        }
    }
}
