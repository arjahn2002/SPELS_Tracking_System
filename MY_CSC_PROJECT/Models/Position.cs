using System.ComponentModel.DataAnnotations;

namespace MY_CSC_PROJECT.Models
{
    public class Position
    {
        [Key]
        public int PositionID { get; set; }

        public string PositionName { get; set; }
    }
}
