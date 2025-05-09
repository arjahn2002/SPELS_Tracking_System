using System.ComponentModel.DataAnnotations;

namespace SPELS_TRACKING_SYSTEM.Models
{
    public class Role
    {
        [Key]
        public int RoleID { get; set; }

        public string RoleName { get; set; }

        public virtual ICollection<RolePermission> RolePermissions { get; set; }
    }
}
