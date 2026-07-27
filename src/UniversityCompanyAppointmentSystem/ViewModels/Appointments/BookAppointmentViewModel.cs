using System.ComponentModel.DataAnnotations;
using UniversityCompanyAppointmentSystem.Models;

namespace UniversityCompanyAppointmentSystem.ViewModels.Appointments
{
    // Powers the "Book Appointment for Selected Employees" page.
    // One BookAppointmentViewModel = one group appointment for many employees.
    public class BookAppointmentViewModel
    {
        [Required(ErrorMessage = "Please select a university.")]
        [Display(Name = "University")]
        public int UniversityId { get; set; }

        // Filled by the page so the "Select University" dropdown can be redrawn on validation errors.
        public List<University> Universities { get; set; } = new();

        [Required(ErrorMessage = "Please select an available appointment slot.")]
        [Display(Name = "Appointment Slot")]
        public int AppointmentSlotId { get; set; }

        // Available slots for the currently chosen university (loaded via AJAX, but also
        // repopulated here on the server in case the page needs to redisplay with errors).
        public List<AppointmentSlot> AvailableSlots { get; set; } = new();

        [Required(ErrorMessage = "Please enter the purpose of the appointment.")]
        [MaxLength(500)]
        public string Purpose { get; set; } = string.Empty;

        // IDs of the employees the company selected on the Employees page.
        [Required(ErrorMessage = "At least one employee must be selected.")]
        [MinLength(1, ErrorMessage = "At least one employee must be selected.")]
        public List<int> EmployeeIds { get; set; } = new();

        // The actual employee rows to display in the "selected employees" table.
        public List<Employee> SelectedEmployees { get; set; } = new();
    }
}
