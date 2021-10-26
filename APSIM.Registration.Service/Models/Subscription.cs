using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APSIM.Registration.Service.Models
{
    /// <summary>
    /// Represents a subscription to the apsim mailing list.
    /// </summary>
    [Table("Subscribers")]
    public class Subscription
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string Email { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        /// <summary>
        /// Create a new <see cref="Subscription"/> instance.
        /// </summary>
        public Subscription()
        {
        }

        /// <summary>
        /// Create a new <see cref="Subscription"/> instance for a given email address.
        /// </summary>
        /// <param name="email">Email address of the subscription.</param>
        public Subscription(string email)
        {
            Email = email;
        }
    }
}
