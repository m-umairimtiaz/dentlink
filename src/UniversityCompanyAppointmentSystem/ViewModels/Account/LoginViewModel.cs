using System.ComponentModel.DataAnnotations;

namespace UniversityCompanyAppointmentSystem.ViewModels.Account
{
    // Data collected on the shared login form (used by both Universities and Companies).
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
