using SPELS_TRACKING_SYSTEM.Models;

namespace SPELS_TRACKING_SYSTEM.ViewModels
{
    public class ODVM
    {
        public List<ODStage> ODs { get; set; }

        public ODStage OD { get; set; } = new ODStage();

        public Document Document { get; set; } = new Document();
    }
}
