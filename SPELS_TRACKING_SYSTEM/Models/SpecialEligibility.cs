using System.ComponentModel.DataAnnotations;

namespace SPELS_TRACKING_SYSTEM.Models
{
    public class SpecialEligibility
    {
        [Key]
        public int SpecialEligibilityID { get; set; }

        public string SpecialEligibilityName { get; set; }
    }
}
