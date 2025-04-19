using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MY_CSC_PROJECT.Models
{
    public class RolePermission
    {
        [Key]
        public int PermissionID { get; set; }

        public int RoleID { get; set; }

        [ForeignKey("RoleID")]
        public Role? Role { get; set; }

        public string StageName { get; set; }

        public bool CanAccess { get; set; }
        public bool CanForward { get; set; }
        public bool CanAdd { get; set; } 
        public bool CanEdit { get; set; } 
        public bool CanExport { get; set; } 
        public bool CanRemove { get; set; } 
    }
}
