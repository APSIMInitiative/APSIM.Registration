using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace APSIM.Registration.Models
{
    /// <summary>
    /// Encapsulates a software package maintained by the APSIM Initiative.
    /// </summary>
    public class Product
    {
        /// <summary>
        /// All available versions of this product.
        /// </summary>
        public IReadOnlyList<ProductVersion> Versions { get; private init; }

        /// <summary>
        /// Name of the product.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of the product.</param>
        /// <param name="versions">All available versions of this product.</param>
        public Product(string name, IReadOnlyList<ProductVersion> versions)
        {
            Name = name;
            Versions = versions;
        }
    }
}
