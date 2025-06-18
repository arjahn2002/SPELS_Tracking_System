using SPELS_TRACKING_SYSTEM.Models;

namespace SPELS_TRACKING_SYSTEM.ViewModels
{
    public class HomeVM
    {
        public List<DocumentHistory> DocumentHistories { get; set; }
        public bool IsAdmin { get; set; }
    }
}
