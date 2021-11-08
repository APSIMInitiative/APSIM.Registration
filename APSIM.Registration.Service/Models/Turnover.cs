using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace APSIM.Registration.Service.Models
{
    /// <summary>
    /// Company sales turnover categories.
    /// </summary>
    public enum Turnover
    {
        [Display(Name = "<$2 million")]
        LessThanTwo,

        [Display(Name = "$2-40 million")]
        TwoToForty,

        [Display(Name = ">$40 million")]
        GreaterThanFortyMil
    }
}
