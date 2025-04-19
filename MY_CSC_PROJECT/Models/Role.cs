using System.ComponentModel.DataAnnotations;

namespace MY_CSC_PROJECT.Models
{
    public class Role
    {
        [Key]
        public int RoleID { get; set; }

        public string RoleName { get; set; }

        public virtual ICollection<RolePermission> RolePermissions { get; set; }
    }
}
