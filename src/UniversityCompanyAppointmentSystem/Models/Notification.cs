using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversityCompanyAppointmentSystem.Models
{
    // A simple in-website notification. Exactly one of UniversityId/CompanyId is
    // filled in depending on who the notification is for; AppointmentId links back
    // to the appointment that triggered the notification (when applicable).
    public class Notification
    {
        [Key]                                       // primary key column: NotificationId
        public int NotificationId { get; set; }

        public int? UniversityId { get; set; }      // set when this notification is for a university, otherwise null

        [ForeignKey(nameof(UniversityId))]
        public University? University { get; set; }

        public int? CompanyId { get; set; }         // set when this notification is for a company, otherwise null

        [ForeignKey(nameof(CompanyId))]
        public Company? Company { get; set; }

        public int? AppointmentId { get; set; }     // the related appointment, if any

        [ForeignKey(nameof(AppointmentId))]
        public Appointment? Appointment { get; set; }

        [Required, MaxLength(500)]                  // the message text shown to the user
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;   // false = unread (shown in the unread badge count)

        public DateTime CreatedAt { get; set; } = DateTime.Now; // when the notification was generated
    }
}
