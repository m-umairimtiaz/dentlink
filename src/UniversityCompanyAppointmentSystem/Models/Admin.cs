using System.ComponentModel.DataAnnotations;

namespace UniversityCompanyAppointmentSystem.Models
{
    // Platform administrator: can oversee all universities, companies, employees, and reporting.
    public class Admin
    {
        [Key]
        public int AdminId { get; set; }

        [Required, MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
