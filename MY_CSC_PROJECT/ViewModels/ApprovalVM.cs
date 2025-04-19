using MY_CSC_PROJECT.Models;

namespace MY_CSC_PROJECT.ViewModels
{
    public class ApprovalVM
    {
        public List<ApprovalStage> Approvals { get; set; }

        public ApprovalStage Approval { get; set; } = new ApprovalStage();

        public Document Document { get; set; } = new Document();
    }
}
