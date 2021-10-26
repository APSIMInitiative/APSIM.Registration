using Microsoft.EntityFrameworkCore;

namespace APSIM.Registration.Service.Data
{
    /// <summary>
    /// An interface for a class which can create a DB context.
    /// </summary>
    /// <typeparam name="TContext">Type of DB context which this instance can generate.</typeparam>
    public interface IDbContextGenerator<TContext>
    {
        /// <summary>
        /// Generate the DB context.
        /// </summary>
        TContext Generate();
    }
}
