using System.ComponentModel.DataAnnotations;

namespace MY_CSC_PROJECT.Models
{
    public class SpecialEligibility
    {
        [Key]
        public int SpecialEligibilityID { get; set; }

        public string SpecialEligibilityName { get; set; }
    }
}
