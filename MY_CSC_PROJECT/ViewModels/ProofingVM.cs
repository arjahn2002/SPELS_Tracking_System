using MY_CSC_PROJECT.Models;

namespace MY_CSC_PROJECT.ViewModels
{
    public class ProofingVM
    {
        public List<ProofingStage> Proofings { get; set; }

        public ProofingStage Proofing  { get; set; } = new ProofingStage();

        public Document Document { get; set; } = new Document();
    }
}
