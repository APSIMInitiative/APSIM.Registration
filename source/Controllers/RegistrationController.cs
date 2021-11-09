using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using APSIM.Registration.Models;
using APSIM.Registration.Data;

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
        /// Check if a user with a given email address has previously
        /// accepted the licence terms and conditions.
        /// </summary>
        /// <param name="email">Email address.</param>
        [HttpGet("isregistered")]
        public ActionResult<bool> IsRegistered(string email)
        {
            try
            {
                using (IRegistrationsDbContext context = dbContextGenerator.Generate())
                {
                    Func<Models.Registration, bool> isMatch = r => r.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase);
                    return context.Registrations.Any(isMatch);
                }
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
                return context.Registrations.Any(r => areEqual(r.Email, email) && r.LicenceType == LicenceType.Commercial);
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
