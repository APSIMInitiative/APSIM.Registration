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

namespace APSIM.Registration.Service.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IDbContextGenerator<RegistrationsDbContext> generator;
        private readonly RegistrationController controller;
        private readonly ILogger<IndexModel> logger;

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
                    RegistrationDetails.Product = "APSIM";
                    RegistrationDetails.Version = "7.10";
                    RegistrationDetails.Platform = "Windows";
                    RegistrationDetails.LicenceType = LicenceType.NonCommercial;
                    RegistrationDetails.Email = email;
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
    }
}
