using System.ComponentModel.DataAnnotations;

namespace SPELS_TRACKING_SYSTEM.Models
{
    public enum StatusType
    {
        [Display(Name = "Evaluation Stage")]
        EvaluationStage = 1,

        [Display(Name = "Proofing Stage")]
        ProofingStage = 2,

        [Display(Name = "Posting Stage")]
        PostingStage = 3,

        [Display(Name = "Approval Stage")]
        ApprovalStage = 4,

        [Display(Name = "OD Stage")]
        ODStage = 5,

        [Display(Name = "Approved")]
        Approved = 6,
    }
}
