using System.ComponentModel.DataAnnotations;

namespace PostgresTest.ViewModels
{
    public class SigninViewModel
    {
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Paswords are not the same")]
        [DataType(DataType.Password)]
        [Display(Name = " Confirm Password")]
        public string PasswordConfirm { get; set; }
    }
}