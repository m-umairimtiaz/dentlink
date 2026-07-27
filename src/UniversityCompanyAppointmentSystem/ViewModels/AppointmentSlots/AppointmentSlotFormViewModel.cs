using System.ComponentModel.DataAnnotations;
using UniversityCompanyAppointmentSystem.Models;

namespace UniversityCompanyAppointmentSystem.ViewModels.AppointmentSlots
{
    // Used for both the "Add Slot" and "Edit Slot" forms on the University side.
    public class AppointmentSlotFormViewModel
    {
        public int AppointmentSlotId { get; set; }   // 0 when creating a new slot

        [Required(ErrorMessage = "Appointment date is required.")]
        [Display(Name = "Appointment Date")]
        [DataType(DataType.Date)]
        public DateOnly AppointmentDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required(ErrorMessage = "Start time is required.")]
        [Display(Name = "Start Time")]
        [DataType(DataType.Time)]
        public TimeOnly StartTime { get; set; }

        [Required(ErrorMessage = "End time is required.")]
        [Display(Name = "End Time")]
        [DataType(DataType.Time)]
        public TimeOnly EndTime { get; set; }

        [Required(ErrorMessage = "Maximum number of employees is required.")]
        [Range(1, 1000, ErrorMessage = "Maximum employees must be between 1 and 1000.")]
        [Display(Name = "Maximum Employees")]
        public int MaximumEmployees { get; set; } = 10;

        [Required]
        public SlotStatus Status { get; set; } = SlotStatus.Available;
    }
}
