using System.ComponentModel.DataAnnotations;

namespace APSIM.Registration.Models
{
    public enum LicenceType
    {

        [Display(Name = "General Use")]
        GeneralUse,

        [Display(Name = "Special Use")]
        SpecialUse,

        [Display(Name = "Non-Commercial (Do not use)")]
        NonCommercial,

        [Display(Name = "Commercial (Do not use)")]
        Commercial
    }
}