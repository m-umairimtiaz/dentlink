using System.ComponentModel.DataAnnotations;

namespace UniversityCompanyAppointmentSystem.Models
{
    // A Company account. Companies manage their own Employees and
    // book group Appointments with a University for selected employees.
    public class Company
    {
        [Key]                                   // primary key column: CompanyId
        public int CompanyId { get; set; }

        [Required, MaxLength(200)]              // company name is required
        public string CompanyName { get; set; } = string.Empty;

        [Required, MaxLength(150)]              // name of the person who manages the account
        public string ContactPersonName { get; set; } = string.Empty;

        [Required, MaxLength(150)]              // login email, must also be unique (enforced by a unique index)
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(20)]               // contact phone number
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]                              // stores a salted hash, never the raw password
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now; // when the account was registered

        // Navigation properties
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
