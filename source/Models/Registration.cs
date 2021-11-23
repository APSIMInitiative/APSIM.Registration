using System;
using System.ComponentModel.DataAnnotations;
using ISO3166;

namespace APSIM.Registration.Models
{
    public class Registration
    {
        [Key]
        public int ID { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Organisation { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Product { get; set; }

        [Required]
        public string Version { get; set; }

        [Required]
        public string Platform { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public LicenceType LicenceType { get; set; }

        public string LicensorName { get; set; }
        public string LicensorEmail { get; set; }
        public string CompanyTurnover { get; set; }
        public string CompanyRego { get; set; }
        public string CompanyAddress { get; set; }

        /// <summary>
        /// Create a new <see cref="Registration"/> instance.
        /// </summary>
        public Registration()
        {
        }

        /// <summary>
        /// Create a copy of another registration.
        /// </summary>
        /// <param name="old">The registration to be copied.</param>
        public Registration(Registration old)
        {
            FirstName = old.FirstName;
            LastName = old.LastName;
            Organisation = old.Organisation;
            Country = old.Country;
            Email = old.Email;
            Product = old.Product;
            Version = old.Version;
            Platform = old.Platform;
            Type = old.Type;
            LicenceType = old.LicenceType;
            LicensorName = old.LicensorName;
            LicensorEmail = old.LicensorEmail;
            CompanyTurnover = old.CompanyTurnover;
            CompanyRego = old.CompanyRego;
            CompanyAddress = old.CompanyAddress;
        }
    }
}
