using SPELS_TRACKING_SYSTEM.Models;

namespace SPELS_TRACKING_SYSTEM.ViewModels
{
    public class ApprovalVM
    {
        public List<ApprovalStage> Approvals { get; set; }

        public ApprovalStage Approval { get; set; } = new ApprovalStage();

        public Document Document { get; set; } = new Document();
    }
}
