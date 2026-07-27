using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversityCompanyAppointmentSystem.Models
{
    // One group appointment booked by a Company with a University for a set of
    // selected Employees. The employees linked to this appointment live in the
    // AppointmentEmployee join table (many-to-many relationship).
    public class Appointment
    {
        [Key]                                     // primary key column: AppointmentId
        public int AppointmentId { get; set; }

        [Required, MaxLength(30)]                 // human readable reference shown to users, e.g. APT-20260815-0001
        public string AppointmentReference { get; set; } = string.Empty;

        [Required]                                // which university this appointment is with
        public int UniversityId { get; set; }

        [ForeignKey(nameof(UniversityId))]
        public University? University { get; set; }

        [Required]                                // which company made the booking
        public int CompanyId { get; set; }

        [ForeignKey(nameof(CompanyId))]
        public Company? Company { get; set; }

        [Required]                                // which published slot this appointment was booked into
        public int AppointmentSlotId { get; set; }

        [ForeignKey(nameof(AppointmentSlotId))]
        public AppointmentSlot? AppointmentSlot { get; set; }

        [Required]                                // copied from the slot at booking time, so history stays correct
        public DateOnly AppointmentDate { get; set; }

        [Required]
        public TimeOnly StartTime { get; set; }

        [Required]
        public TimeOnly EndTime { get; set; }

        [Required, MaxLength(500)]                // reason/purpose of the visit, entered by the company
        public string Purpose { get; set; } = string.Empty;

        [Required]                                // current status, see AppointmentStatus enum
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.Now;   // when the booking was submitted
        public DateTime UpdatedAt { get; set; } = DateTime.Now;   // last time the status/record changed

        // The employees selected by the company for this appointment (many-to-many via join table)
        public ICollection<AppointmentEmployee> AppointmentEmployees { get; set; } = new List<AppointmentEmployee>();

        // Notifications generated because of this appointment (submitted/confirmed/rejected/etc.)
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
