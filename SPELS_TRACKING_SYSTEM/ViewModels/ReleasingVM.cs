using SPELS_TRACKING_SYSTEM.Models;

namespace SPELS_TRACKING_SYSTEM.ViewModels
{
    public class ReleasingVM
    {
        public List<ReleasingStage> Releasings { get; set; }

        public ReleasingStage Releasing { get; set; } = new ReleasingStage();

        public Document Document { get; set; } = new Document();
    }
}
