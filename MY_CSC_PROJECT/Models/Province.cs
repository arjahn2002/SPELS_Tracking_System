using System.ComponentModel.DataAnnotations;

namespace MY_CSC_PROJECT.Models
{
    public class Province
    {
        [Key]
        public int ProvinceID { get; set; }

        public string ProvinceName { get; set; }
    }
}
