using System.ComponentModel.DataAnnotations;

namespace MY_CSC_PROJECT.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Please enter your username.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Please enter your password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
