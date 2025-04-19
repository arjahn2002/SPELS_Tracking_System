using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MY_CSC_PROJECT.Models
{
    public class ApprovalStage
    {
        [Key]
        public int ApprovalID { get; set; }

        public int DocumentID { get; set; }

        [ForeignKey("DocumentID")]
        public Document? Document { get; set; }

        [Display(Name = "Date Acted")]
        public DateTime DateActed { get; set; }
    }
}
