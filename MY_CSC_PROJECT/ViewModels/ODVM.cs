using MY_CSC_PROJECT.Models;

namespace MY_CSC_PROJECT.ViewModels
{
    public class ODVM
    {
        public List<ODStage> ODs { get; set; }

        public ODStage OD { get; set; } = new ODStage();

        public Document Document { get; set; } = new Document();
    }
}
