using System;
using System.Linq;
using APSIM.Registration.Service.Data;
using Microsoft.EntityFrameworkCore;

namespace APSIM.Registration.Migration
{
    public class Migrator
    {
        private readonly string oldConnectionString;
        private readonly string newConnectionString;

        public Migrator(string oldConnectionString, string newConnectionString)
        {
            this.oldConnectionString = oldConnectionString;
            this.newConnectionString = newConnectionString;
            
        }

        /// <summary>
        /// Migrate data from old DB to new one.
        /// </summary>
        /// <param name="maxRecords">Max number of records to migrate. 0 for unlimited.</param>
        public void Migrate(ushort maxRecords = 0)
        {
            DbContextOptionsBuilder<RegistrationsDbContext> oldBuilder = new DbContextOptionsBuilder<RegistrationsDbContext>();
            oldBuilder = oldBuilder.UseLazyLoadingProxies().UseSqlServer(oldConnectionString);

            DbContextOptionsBuilder<RegistrationsDbContext> newBuilder = new DbContextOptionsBuilder<RegistrationsDbContext>();
            newBuilder = newBuilder.UseLazyLoadingProxies().UseMySQL(newConnectionString);

            using (RegistrationsDbContext oldContext = new RegistrationsDbContext(oldBuilder.Options))
            using (RegistrationsDbContext newContext = new RegistrationsDbContext(newBuilder.Options))
            {
                newContext.Database.EnsureCreated();

                try
                {
                    int numRegistrations = maxRecords == 0 ? oldContext.Registrations.Count() : Math.Min(maxRecords, oldContext.Registrations.Count());
                    int i = 1;
                    foreach (var registration in oldContext.Registrations.Take(numRegistrations))
                    {
                        double progress = 100.0 * i / numRegistrations;
                        Console.Write($"Copying registrations: {progress:F2} (ID={registration.ID}, {i}/{numRegistrations})...\r");
                        newContext.Registrations.Add(registration);
                        i++;
                    }
                    Console.WriteLine();

                    int numSubscriptions = maxRecords == 0 ? oldContext.Subscriptions.Count() : Math.Min(maxRecords, oldContext.Subscriptions.Count());
                    i = 1;
                    foreach (var subscription in oldContext.Subscriptions.Take(numSubscriptions))
                    {
                        double progress = 100.0 * i / numSubscriptions;
                        Console.Write($"Copying subscribers: {progress:F2} ({i}/{numSubscriptions})...\r");
                        newContext.Subscriptions.Add(subscription);
                        i++;
                    }

                    Console.WriteLine();
                    Console.WriteLine("Saving changes to database...");
                    newContext.SaveChanges();
                }
                catch
                {
                    Console.WriteLine();
                    throw;
                }
            }
        }
    }
}