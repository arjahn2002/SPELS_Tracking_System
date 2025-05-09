using SPELS_TRACKING_SYSTEM.Models;

namespace SPELS_TRACKING_SYSTEM.ViewModels
{
    public class PostingVM
    {
        public List<PostingStage> Postings { get; set; }

        public PostingStage Posting { get; set; } = new PostingStage();

        public Document Document { get; set; } = new Document();
    }
}
