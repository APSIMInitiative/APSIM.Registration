using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APSIM.Registration.Data;
using APSIM.Registration.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace APSIM.Registration.Controllers
{
    [ApiController]
    [Route("api")]
    public class RegistrationController : ControllerBase
    {
        private readonly ILogger<RegistrationController> logger;

        private readonly RegistrationsDbContextGenerator dbContextGenerator;

        public RegistrationController(ILogger<RegistrationController> logger, RegistrationsDbContextGenerator generator)
        {
            this.logger = logger;
            this.dbContextGenerator = generator;
        }

        /// <summary>
        /// Add a new registration to the registrations DB.
        /// </summary>
        /// <param name="registration">Registration details.</param>
        [HttpPost("register")]
        public ActionResult Register(Models.Registration registration)
        {
            try
            {
                using (IRegistrationsDbContext context = dbContextGenerator.Generate())
                {
                    context.Registrations.Add(registration);
                    context.SaveChanges();
                }
                return Ok(registration);
            }
            catch (Exception error)
            {
                return HandleError(error);
            }
        }

        /// <summary>
        /// Register an upgrade for the given email address.
        /// </summary>
        /// <param name="email">The email address.</param>
        [HttpPost("upgrade")]
        public async Task<ActionResult> UpgradeAsync(string email, string version, string platform)
        {
            try
            {
                using (IRegistrationsDbContext context = dbContextGenerator.Generate())
                {
                    Models.Registration first = context.Registrations.FirstOrDefault(r => r.Email == email);
                    Models.Registration newRegistration = new Models.Registration(first);
                    newRegistration.Type = "Upgrade";
                    newRegistration.Version = version;
                    newRegistration.Platform = platform;
                    await context.Registrations.AddAsync(newRegistration);
                    context.SaveChanges();
                }
                return Ok();
            }
            catch (Exception error)
            {
                return HandleError(error);
            }
        }

        /// <summary>
        /// Return a list of registrations as a CSV string.
        /// </summary>
        /// <param name="key">The token.</param>
        [HttpGet("getallregistrations")]
        public ActionResult<string> GetAllRegistrations(string token)
        {
            try
            {
                string validToken = Environment.GetEnvironmentVariable("REGISTRATION_TOKEN");
                if (validToken == null)
                    throw new Exception("Cannot find environment variable REGISTRATION_TOKEN");
                if (validToken == token)
                {
                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine("Date,FirstName,LastName,Organisation,Country,Email,Product,Version,Platform,Type,LicenceType,LicensorName,LicensorEmail,CompanyTurnover,CompanyRego,CompanyAddress");

                    using IRegistrationsDbContext context = dbContextGenerator.Generate();
                    foreach (var registration in context.Registrations)
                    {
                        stringBuilder.Append(SanitiseValue(registration.Date.ToString("yyyy-MM-dd")));
                        stringBuilder.Append(",");
                        stringBuilder.Append(SanitiseValue(registration.FirstName));
                        stringBuilder.Append(",");
                        stringBuilder.Append(SanitiseValue(registration.LastName));
                        stringBuilder.Append(",");
                        stringBuilder.Append(SanitiseValue(registration.Organisation));
                        stringBuilder.Append(",");
                        stringBuilder.Append(SanitiseCountry(registration.Country));
                        stringBuilder.Append(",");
                        stringBuilder.Append(SanitiseValue(registration.Email));
                        stringBuilder.Append(",");
                        stringBuilder.Append(SanitiseValue(registration.Product));
                        stringBuilder.Append(",");
                        stringBuilder.Append(SanitiseValue(registration.Version));
                        stringBuilder.Append(",");
                        stringBuilder.Append(SanitiseValue(registration.Platform));
                        stringBuilder.Append(",");
                        stringBuilder.Append(SanitiseValue(registration.Type));
                        stringBuilder.Append(",");
                        stringBuilder.Append(SanitiseValue(registration.LicenceType.ToString()));
                        stringBuilder.Append(",");
                        stringBuilder.Append(SanitiseValue(registration.LicensorName));
                        stringBuilder.Append(",");
                        stringBuilder.Append(SanitiseValue(registration.LicensorEmail));
                        stringBuilder.Append(",");
                        stringBuilder.Append(SanitiseValue(registration.CompanyTurnover));
                        stringBuilder.Append(",");
                        stringBuilder.Append(SanitiseValue(registration.CompanyRego));
                        stringBuilder.Append(",");
                        stringBuilder.AppendLine(SanitiseValue(registration.CompanyAddress));
                    }
                    return stringBuilder.ToString();
                }
                else
                    throw new Exception("Missing or invalid token");
            }
            catch (Exception error)
            {
                return HandleError(error);
            }
        }

        /// <summary>
        /// Remove double quotes from value and wrap the value in double quotes.
        /// </summary>
        /// <param name="value">The string value.</param>
        private string SanitiseValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                value = string.Empty;
            else
                value = value.Replace("\"", "");
            return $"\"{value}\"";
        }

        /// <summary>
        /// Remove double quotes from value and wrap the value in double quotes.
        /// </summary>
        /// <param name="country">The string value.</param>
        private string SanitiseCountry(string country)
        {
            if (string.IsNullOrEmpty(country))
                country = string.Empty;
            else if (country == "Congo, Democratic Republic of the")
                country = "Congo";
            else if (country == "Kosovo")
                country = "Serbia";

            return SanitiseValue(country);
        }

        /// <summary>
        /// Update cached product versions used by the downloads
        /// webpage. This is called by the APSIM.Builds API,
        /// but should ultimately disappear as a public endpoint
        /// when APSIM.Builds is refactored into this project.
        /// </summary>
        [HttpGet("updateproducts")]
        public ActionResult Update()
        {
            try
            {
                APSIM.Registration.Pages.IndexModel.UpdateProducts();
                return Ok();
            }
            catch (Exception error)
            {
                return HandleError(error);
            }
        }

        /// <summary>
        /// Check if a user with a given email address has previously
        /// accepted the licence terms and conditions.
        /// </summary>
        /// <param name="email">Email address.</param>
        [HttpGet("isregistered")]
        public ActionResult<bool> IsRegistered(string email)
        {
            try
            {
                IEnumerable<Models.Registration> registrations = GetRegistrations(email);

                // Registrations expire every 3 years.
                int days = 365 * 3;
                return registrations.Reverse().Any(r => (DateTime.Now - r.Date).TotalDays < days);
            }
            catch (Exception error)
            {
                return HandleError(error);
            }
        }

        /// <summary>
        /// Subscribe to the mailing list.
        /// </summary>
        /// <param name="email">Email address.</param>
        [HttpGet("subscribe")]
        public ActionResult Subscribe(string email)
        {
            try
            {
                using (ISubscriptionsDbContext context = dbContextGenerator.Generate())
                {
                    context.Subscriptions.Add(new Subscription(email));
                    context.SaveChanges();
                }
                return Ok();
            }
            catch (Exception err)
            {
                return HandleError(err);
            }
        }

        /// <summary>
        /// Unsubscribe from the mailing list.
        /// </summary>
        /// <param name="email">Email address.</param>
        [HttpGet("unsubscribe")]
        public ActionResult Unsubscribe(string email)
        {
            try
            {
                using (ISubscriptionsDbContext context = dbContextGenerator.Generate())
                {
                    Func<Subscription, bool> isMatch = s => s.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase);
                    context.Subscriptions.RemoveRange(context.Subscriptions.Where(isMatch));
                    context.SaveChanges();
                }
                return Ok();
            }
            catch (Exception error)
            {
                return HandleError(error);
            }
        }

        private IEnumerable<Models.Registration> GetRegistrations(string email)
        {
            using (IRegistrationsDbContext context = dbContextGenerator.Generate())
                return context.Registrations.Where(r => r.Email == email).ToList();
        }

        /// <summary>
        /// Checks if an email is registered to a commercial licence.
        /// </summary>
        /// <param name="email">Email address.</param>
        private bool IsCommercialUser(string email)
        {
            Func<string, string, bool> areEqual = (s1, s2) => s1.Equals(s2, StringComparison.InvariantCultureIgnoreCase);
            using (IRegistrationsDbContext context = dbContextGenerator.Generate())
                return context.Registrations.Any(r => areEqual(r.Email, email) && r.LicenceType == LicenceType.SpecialUse);
        }

        /// <summary>
        /// Handle an error (by logging it) and return an appropriate HTTP response.
        /// </summary>
        /// <param name="err">The error.</param>
        protected ActionResult HandleError(Exception err)
        {
            logger.LogError(err, "");
            return StatusCode(StatusCodes.Status500InternalServerError, err.Message);
        }
    }
}
