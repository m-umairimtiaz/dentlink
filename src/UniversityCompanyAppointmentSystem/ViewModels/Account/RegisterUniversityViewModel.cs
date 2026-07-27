using System.ComponentModel.DataAnnotations;

namespace UniversityCompanyAppointmentSystem.ViewModels.Account
{
    // Data collected on the "Register as University" form.
    // We use a ViewModel (instead of the University entity) so the form only
    // exposes the fields we actually want the user to fill in, with their own
    // validation messages tailored to the UI.
    public class RegisterUniversityViewModel
    {
        [Required(ErrorMessage = "University name is required.")]
        [MaxLength(200)]
        [Display(Name = "University Name")]
        public string UniversityName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact person name is required.")]
        [MaxLength(150)]
        [Display(Name = "Contact Person Name")]
        public string ContactPersonName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [MaxLength(20)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
