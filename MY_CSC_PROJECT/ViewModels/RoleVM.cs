using MY_CSC_PROJECT.Models;

namespace MY_CSC_PROJECT.ViewModels
{
    public class RoleVM
    {
        public List<Role> Roles { get; set; }

        public Role Role { get; set; } = new Role();

        public List<RolePermissionVM> Stages { get; set; } = new List<RolePermissionVM>();
    }
}
