using System.ComponentModel.DataAnnotations;

namespace UniversityCompanyAppointmentSystem.Models
{
    // A University account. Universities publish AppointmentSlots and
    // confirm/reject the Appointments that Companies book into those slots.
    public class University
    {
        [Key]                                   // primary key column: UniversityId
        public int UniversityId { get; set; }

        [Required, MaxLength(200)]              // university name is required, max 200 characters
        public string UniversityName { get; set; } = string.Empty;

        [Required, MaxLength(150)]              // name of the person who manages the account
        public string ContactPersonName { get; set; } = string.Empty;

        [Required, MaxLength(150)]              // login email, must also be unique (enforced by a unique index)
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(20)]               // contact phone number
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]                              // stores a salted hash, never the raw password
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now; // when the account was registered

        // Navigation properties (collections of related rows in other tables)
        public ICollection<AppointmentSlot> AppointmentSlots { get; set; } = new List<AppointmentSlot>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
