using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace APSIM.Registration.Service.Models
{
    public enum LicenceType
    {
        [Display(Name = "Non-Commercial")]
        NonCommercial,

        [Display(Name = "Commercial")]
        Commercial
    }
}