using SPELS_TRACKING_SYSTEM.Models;

namespace SPELS_TRACKING_SYSTEM.ViewModels
{
    public class SpecialEligibilityVM
    {
        public List<SpecialEligibility> SpecialEligibilities { get; set; }

        public SpecialEligibility SpecialEligibility { get; set; } = new SpecialEligibility();
    }
}
