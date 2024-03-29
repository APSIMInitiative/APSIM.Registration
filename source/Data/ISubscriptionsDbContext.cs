using System;
using APSIM.Registration.Models;
using Microsoft.EntityFrameworkCore;

namespace APSIM.Registration.Data
{
    /// <summary>
    /// An interface for a subscriptions DB context.
    /// </summary>
    public interface ISubscriptionsDbContext : IDisposable
    {
        int SaveChanges();
        DbSet<Subscription> Subscriptions { get; set; }
    }
}
