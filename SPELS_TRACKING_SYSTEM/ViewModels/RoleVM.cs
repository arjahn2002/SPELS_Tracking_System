using SPELS_TRACKING_SYSTEM.Models;

namespace SPELS_TRACKING_SYSTEM.ViewModels
{
    public class RoleVM
    {
        public List<Role> Roles { get; set; }

        public Role Role { get; set; } = new Role();

        public List<RolePermissionVM> Stages { get; set; } = new List<RolePermissionVM>();
    }
}
