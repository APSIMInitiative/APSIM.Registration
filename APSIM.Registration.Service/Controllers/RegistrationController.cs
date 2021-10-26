using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using APSIM.Registration.Service.Models;
using APSIM.Registration.Service.Data;

namespace APSIM.Registration.Service.Controllers
{
    [ApiController]
    [Route("api/registration")]
    public class RegistrationController : ControllerBase
    {
        private const string registrationType = "Registration";

        private const string commercialLicenceName = "Commercial";
        private const string nonCommercialLicenceName = "Non-Commercial";
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
        [HttpPost("add")]
        public ActionResult Add(Models.Registration registration)
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
        /// Add a registration into the database.
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="organisation"></param>
        /// <param name="address1"></param>
        /// <param name="address2"></param>
        /// <param name="city"></param>
        /// <param name="state"></param>
        /// <param name="postcode"></param>
        /// <param name="country"></param>
        /// <param name="email"></param>
        /// <param name="product"></param>
        [HttpGet("add")]
        public ActionResult Add(string firstName, string lastName, string organisation, string address1, string address2,
                    string city, string state, string postcode, string country, string email, string product)
        {
            try
            {
                var registration = new Models.Registration();
                registration.FirstName = firstName;
                registration.LastName = lastName;
                registration.Organisation = organisation;
                registration.Country = country;
                registration.Email = email;
                registration.Product = product;
                registration.Version = "1";
                registration.Platform = "Windows";
                registration.Type = registrationType;
                registration.LicenceType = nonCommercialLicenceName;

                return Add(registration);
            }
            catch (Exception error)
            {
                return HandleError(error);
            }
        }

        /// <summary>
        /// Add a upgrade registration into the database.
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="organisation"></param>
        /// <param name="country"></param>
        /// <param name="email"></param>
        /// <param name="product"></param>
        [HttpGet("addnew")]
        public ActionResult AddNew(string firstName, string lastName, string organisation, string country, string email, string product)
        {
            try
            {
                var registration = new Models.Registration();
                registration.FirstName = firstName;
                registration.LastName = lastName;
                registration.Organisation = organisation;
                registration.Country = country;
                registration.Email = email;
                registration.Product = product;
                registration.Version = "1"; // To mimic the old behaviour
                registration.Platform = "Windows";
                registration.Type = registrationType;
                registration.LicenceType = nonCommercialLicenceName;

                return Add(registration);
            }
            catch (Exception error)
            {
                return HandleError(error);
            }
        }

        /// <summary>
        /// Add a upgrade or registration into the database.
        /// </summary>
        /// <remarks>
        /// Called by the APSIM Upgrade GUI.
        /// </remarks>
        /// <param name="firstName">First name of the registrant.</param>
        /// <param name="lastName">Last name of the registrant.</param>
        /// <param name="organisation">Organisation of the registrant.</param>
        /// <param name="country"></param>
        /// <param name="email"></param>
        /// <param name="product"></param>
        /// <param name="version"></param>
        /// <param name="platform"></param>
        /// <param name="type"></param>
        [HttpGet("addregistration")]
        public ActionResult AddRegistration(string firstName, string lastName, string organisation, string country, string email, string product, string version, string platform, string type)
        {
            try
            {
                var registration = new Models.Registration();
                registration.FirstName = firstName;
                registration.LastName = lastName;
                registration.Organisation = organisation;
                registration.Country = country;
                registration.Email = email;
                registration.Product = product;
                registration.Version = version;
                registration.Platform = platform;
                registration.Type = type;
                registration.LicenceType = nonCommercialLicenceName;
                try
                {
                    registration.LicenceType = IsCommercialUser(email) ? commercialLicenceName : nonCommercialLicenceName;
                }
                catch (Exception err)
                {
                    logger.LogError(err, $"Failed to check if {email} belongs to a commercial user");
                }

                return Add(registration);
            }
            catch (Exception error)
            {
                return HandleError(error);
            }
        }

        /// <summary>
        /// Adds a registration for a commercial user.
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="organisation"></param>
        /// <param name="country"></param>
        /// <param name="email"></param>
        /// <param name="product"></param>
        /// <param name="version"></param>
        /// <param name="platform"></param>
        /// <param name="type"></param>
        /// <param name="licensorName"></param>
        /// <param name="licensorEmail"></param>
        /// <param name="companyTurnover"></param>
        /// <param name="companyRego"></param>
        /// <param name="companyAddress"></param>
        [HttpGet("addcommercialregistration")]
        public ActionResult AddCommercialRegistration(string firstName, string lastName, string organisation, string country, string email, string product, string version, string platform, string type, string licensorName, string licensorEmail, string companyTurnover, string companyRego, string companyAddress)
        {
            try
            {
                var registration = new Models.Registration();
                registration.FirstName = firstName;
                registration.LastName = lastName;
                registration.Organisation = organisation;
                registration.Country = country;
                registration.Email = email;
                registration.Product = product;
                registration.Version = version;
                registration.Platform = platform;
                registration.Type = type;
                registration.LicencorName = licensorName;
                registration.LicencorEmail = licensorEmail;
                registration.CompanyTurnover = companyTurnover;
                registration.CompanyRego = companyRego;
                registration.CompanyAddress = companyAddress;

                return Add(registration);
            }
            catch (Exception err)
            {
                return HandleError(err);
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
                throw new NotImplementedException();
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

        /// <summary>
        /// Checks if an email is registered to a commercial licence.
        /// </summary>
        /// <param name="email">Email address.</param>
        private bool IsCommercialUser(string email)
        {
            Func<string, string, bool> areEqual = (s1, s2) => s1.Equals(s2, StringComparison.InvariantCultureIgnoreCase);
            using (IRegistrationsDbContext context = dbContextGenerator.Generate())
                return context.Registrations.Where(r => areEqual(r.Email, email)).Any(r => areEqual(r.LicenceType, commercialLicenceName));
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
