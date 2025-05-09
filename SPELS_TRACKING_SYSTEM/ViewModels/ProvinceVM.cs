using SPELS_TRACKING_SYSTEM.Models;

namespace SPELS_TRACKING_SYSTEM.ViewModels
{
    public class ProvinceVM
    {
        public List<Province> Provinces { get; set; }

        public Province Province { get; set; } = new Province();
    }
}
