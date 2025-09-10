using System.ComponentModel.DataAnnotations;

namespace APSIM.Registration.Models
{
    public enum LicenceType
    {
        [Display(Name = "General Use")]
        GeneralUse = 0,

        [Display(Name = "Special Use")]
        SpecialUse = 1,
        NonCommercial = 1


    }
}