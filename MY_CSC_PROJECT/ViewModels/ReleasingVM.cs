using MY_CSC_PROJECT.Models;

namespace MY_CSC_PROJECT.ViewModels
{
    public class ReleasingVM
    {
        public List<ReleasingStage> Releasings { get; set; }

        public ReleasingStage Releasing { get; set; } = new ReleasingStage();

        public Document Document { get; set; } = new Document();
    }
}
