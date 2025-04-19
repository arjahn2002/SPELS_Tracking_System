using MY_CSC_PROJECT.Models;

namespace MY_CSC_PROJECT.ViewModels
{
    public class ProvinceVM
    {
        public List<Province> Provinces { get; set; }

        public Province Province { get; set; } = new Province();
    }
}
