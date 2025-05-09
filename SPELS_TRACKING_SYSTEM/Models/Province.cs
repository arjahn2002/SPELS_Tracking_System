using System.ComponentModel.DataAnnotations;

namespace SPELS_TRACKING_SYSTEM.Models
{
    public class Province
    {
        [Key]
        public int ProvinceID { get; set; }

        public string ProvinceName { get; set; }
    }
}
