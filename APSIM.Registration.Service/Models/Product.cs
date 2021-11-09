using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace APSIM.Registration.Service.Models
{
    /// <summary>
    /// Encapsulates a software package maintained by the APSIM Initiative.
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Function which will fetch available versions of this product.
        /// </summary>
        private Func<int, List<ProductVersion>> GetAvailableVersions;

        /// <summary>
        /// Name of the product.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Function which will fetch available versions of this product.
        /// </summary>
        public List<ProductVersion> GetVersions(int n)
        {
            return GetAvailableVersions(n);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of the product.</param>
        /// <param name="getVersions">Function which will fetch available versions of this product.</param>
        public Product(string name, Func<int, List<ProductVersion>> getVersions)
        {
            Name = name;
            GetAvailableVersions = getVersions;
        }
    }
}
