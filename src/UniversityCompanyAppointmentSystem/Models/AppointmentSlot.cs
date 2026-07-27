using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversityCompanyAppointmentSystem.Models
{
    // A time slot that a University publishes and makes available for companies to book.
    // One slot can be shared by several appointments as long as the total number of
    // employees booked into it does not exceed MaximumEmployees.
    public class AppointmentSlot
    {
        [Key]                                    // primary key column: AppointmentSlotId
        public int AppointmentSlotId { get; set; }

        [Required]                               // foreign key pointing back to the owning University
        public int UniversityId { get; set; }

        [ForeignKey(nameof(UniversityId))]       // navigation back to the University row
        public University? University { get; set; }

        [Required]                               // the calendar date of the slot (no time component)
        public DateOnly AppointmentDate { get; set; }

        [Required]                               // time the slot starts
        public TimeOnly StartTime { get; set; }

        [Required]                               // time the slot ends
        public TimeOnly EndTime { get; set; }

        [Required, Range(1, 1000)]               // maximum total employees that can be booked into this slot
        public int MaximumEmployees { get; set; }

        [Required]                               // Available or Unavailable (see SlotStatus enum)
        public SlotStatus Status { get; set; } = SlotStatus.Available;

        public DateTime CreatedAt { get; set; } = DateTime.Now; // when the slot was created

        // All appointments (from possibly different companies) booked into this slot
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
