using SPELS_TRACKING_SYSTEM.Models;

namespace SPELS_TRACKING_SYSTEM.ViewModels
{
    public class ProofingVM
    {
        public List<ProofingStage> Proofings { get; set; }

        public ProofingStage Proofing  { get; set; } = new ProofingStage();

        public Document Document { get; set; } = new Document();
    }
}
