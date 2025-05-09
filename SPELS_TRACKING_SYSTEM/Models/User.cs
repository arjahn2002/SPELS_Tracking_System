using System.ComponentModel.DataAnnotations.Schema;

namespace SPELS_TRACKING_SYSTEM.Models
{
    public class User
    {
        public int UserID { get; set; }

        public string Fullname { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public int RoleID { get; set; }

        [ForeignKey("RoleID")]
        public Role? Role { get; set; }
    }
}
