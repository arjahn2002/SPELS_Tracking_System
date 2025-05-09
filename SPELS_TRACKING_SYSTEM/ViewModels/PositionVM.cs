using SPELS_TRACKING_SYSTEM.Models;

namespace SPELS_TRACKING_SYSTEM.ViewModels
{
    public class PositionVM
    {
        public List<Position> Positions { get; set; }

        public Position Position { get; set; } = new Position();
    }
}
