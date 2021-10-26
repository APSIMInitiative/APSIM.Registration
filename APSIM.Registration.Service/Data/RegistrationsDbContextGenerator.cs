using System;
using Microsoft.EntityFrameworkCore;

namespace APSIM.Registration.Service.Data
{
    /// <summary>
    /// An interface for a class which can create a DB context.
    /// </summary>
    /// <typeparam name="TContext">Type of DB context which this instance can generate.</typeparam>
    public class RegistrationsDbContextGenerator : IDbContextGenerator<RegistrationsDbContext>
    {
        private const string connectStringEnvVar = "REGO_DB_CONNECT_STRING";

        /// <inheritdoc />
        public RegistrationsDbContext Generate()
        {
            string connectionString = GetConnectionString();
            var builder = new DbContextOptionsBuilder().UseLazyLoadingProxies().UseMySQL(connectionString);

            RegistrationsDbContext context = new RegistrationsDbContext(builder.Options);
            context.Database.EnsureCreated();
            return context;
        }

        /// <summary>
        /// Get a connection string to connect to the DB.
        /// </summary>
        private string GetConnectionString()
        {
            string connectionString = Environment.GetEnvironmentVariable(connectStringEnvVar);
            if (string.IsNullOrEmpty(connectionString))
                throw new Exception($"Unable to read registrations DB connection string from environment (variable {connectStringEnvVar} not set)");
            return connectionString;
        }
    }
}
