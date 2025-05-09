using System.ComponentModel.DataAnnotations;

namespace SPELS_TRACKING_SYSTEM.Models
{
    public class Position
    {
        [Key]
        public int PositionID { get; set; }

        public string PositionName { get; set; }
    }
}
