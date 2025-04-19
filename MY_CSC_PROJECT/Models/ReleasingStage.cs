using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MY_CSC_PROJECT.Models
{
    public class ReleasingStage
    {
        [Key]
        public int ReleasingID { get; set; }

        public int DocumentID { get; set; }

        [ForeignKey("DocumentID")]
        public Document? Document { get; set; }

        [Display(Name = "Date Approved")]
        public DateTime DateApproved { get; set; }
    }
}
