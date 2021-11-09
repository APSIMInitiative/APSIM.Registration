using APSIM.Registration.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace APSIM.Registration.Data
{
    /// <summary>
    /// This class wraps the registrations table.
    /// </summary>
    public class RegistrationsDbContext : DbContext, IRegistrationsDbContext, ISubscriptionsDbContext
    {
        /// <summary>
        /// Name of the registrations table in the DB.
        /// </summary>
        private const string registrationsTableName = "Registrations";

        /// <summary>
        /// Registrations in the DB.
        /// </summary>
        public DbSet<Models.Registration> Registrations { get; set; }

        /// <summary>
        /// Subscribers table in the DB.
        /// </summary>
        public DbSet<Subscription> Subscriptions { get; set; }

        /// <summary>
        /// Create a new <see cref="RegistrationsDbContext"/> instance.
        /// </summary>
        /// <param name="options">DB context creation options.</param>
        public RegistrationsDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}
