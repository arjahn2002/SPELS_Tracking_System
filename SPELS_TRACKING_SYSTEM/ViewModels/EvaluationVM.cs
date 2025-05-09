using SPELS_TRACKING_SYSTEM.Models;

namespace SPELS_TRACKING_SYSTEM.ViewModels
{
    public class EvaluationVM
    {
        public List<EvaluationStage> Evaluations { get; set; }

        public EvaluationStage Evaluation { get; set; } = new EvaluationStage();

        public Document? Document { get; set; } = new Document();
    }
}
