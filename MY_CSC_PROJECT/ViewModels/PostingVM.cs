using MY_CSC_PROJECT.Models;

namespace MY_CSC_PROJECT.ViewModels
{
    public class PostingVM
    {
        public List<PostingStage> Postings { get; set; }

        public PostingStage Posting { get; set; } = new PostingStage();

        public Document Document { get; set; } = new Document();
    }
}
