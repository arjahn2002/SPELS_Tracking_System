using MY_CSC_PROJECT.Models;

namespace MY_CSC_PROJECT.ViewModels
{
    public class SpecialEligibilityVM
    {
        public List<SpecialEligibility> SpecialEligibilities { get; set; }

        public SpecialEligibility SpecialEligibility { get; set; } = new SpecialEligibility();
    }
}
