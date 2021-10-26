using System;
using Microsoft.EntityFrameworkCore;

namespace APSIM.Registration.Service.Data
{
    /// <summary>
    /// An interface for a registrations DB context.
    /// </summary>
    public interface IRegistrationsDbContext : IDisposable
    {
        int SaveChanges();
        DbSet<Models.Registration> Registrations { get; set; }
    }
}
