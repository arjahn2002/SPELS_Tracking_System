using MY_CSC_PROJECT.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MY_CSC_PROJECT.ViewModels
{
    public class UserVM
    {
        [Required(ErrorMessage = "Please enter a full name.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Please enter a username.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Please enter a password.")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "The {0} must be between {2} and {1} characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Password does not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Please select a role.")]
        public int RoleID { get; set; }
        [ForeignKey("RoleID")]
        public Role? Role { get; set; }

        public List<User> Users { get; set; }

        public User User { get; set; } = new User();

    }
}
