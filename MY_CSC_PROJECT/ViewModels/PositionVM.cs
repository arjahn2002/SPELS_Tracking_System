using MY_CSC_PROJECT.Models;

namespace MY_CSC_PROJECT.ViewModels
{
    public class PositionVM
    {
        public List<Position> Positions { get; set; }

        public Position Position { get; set; } = new Position();
    }
}
