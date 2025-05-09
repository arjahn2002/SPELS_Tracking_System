using System.ComponentModel.DataAnnotations;

namespace SPELS_TRACKING_SYSTEM.Models
{
    public class OtherFOs
    {
        [Key]
        public int OtherFOsID { get; set; }

        public string OtherFOsName { get; set; }
    }
}
