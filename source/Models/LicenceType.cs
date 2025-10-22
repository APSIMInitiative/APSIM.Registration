using System.ComponentModel.DataAnnotations;

namespace APSIM.Registration.Models
{
    public enum LicenceType
    {

        [Display(Name = "General Use")]
        GeneralUse,

        [Display(Name = "Special Use")]
        SpecialUse,

        [Display(Name = "Non-Commercial")]
        NonCommercial,
    }
}