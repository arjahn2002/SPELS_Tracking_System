using System.ComponentModel.DataAnnotations;

namespace MY_CSC_PROJECT.Models
{
    public class OtherFOs
    {
        [Key]
        public int OtherFOsID { get; set; }

        public string OtherFOsName { get; set; }
    }
}
