using System.ComponentModel.DataAnnotations;

namespace SPELS_TRACKING_SYSTEM.Models
{
    public enum SubmissionType
    {
        [Display(Name = "Walk-in")]
        WalkIn = 1,

        [Display(Name = "From Other FO's")]
        FromOtherFOs = 2,

        [Display(Name = "From Other Region")]
        FromOtherRegion = 3
    }
}
