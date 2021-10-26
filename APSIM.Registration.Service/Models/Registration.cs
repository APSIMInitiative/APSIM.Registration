using System;
using System.ComponentModel.DataAnnotations;

namespace APSIM.Registration.Service.Models
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
        public string LicenceType { get; set; }

        public string LicencorName { get; set; }
        public string LicencorEmail { get; set; }
        public string CompanyTurnover { get; set; }
        public string CompanyRego { get; set; }
        public string CompanyAddress { get; set; }
    }
}
