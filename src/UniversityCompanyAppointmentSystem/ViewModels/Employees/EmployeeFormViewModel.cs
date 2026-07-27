using System.ComponentModel.DataAnnotations;

namespace UniversityCompanyAppointmentSystem.ViewModels.Employees
{
    // Used for both the "Add Employee" and "Edit Employee" forms.
    // EmployeeId is 0 when creating a new employee, and the real ID when editing.
    public class EmployeeFormViewModel
    {
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Employee number is required.")]
        [MaxLength(50)]
        [Display(Name = "Employee Number")]
        public string EmployeeNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(150)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Civil ID is required.")]
        [MaxLength(50)]
        [Display(Name = "Civil ID")]
        public string CivilId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [MaxLength(20)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required.")]
        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "Job title is required.")]
        [MaxLength(100)]
        [Display(Name = "Job Title")]
        public string JobTitle { get; set; } = string.Empty;
    }
}
