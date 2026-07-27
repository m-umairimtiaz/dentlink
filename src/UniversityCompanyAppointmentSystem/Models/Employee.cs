using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversityCompanyAppointmentSystem.Models
{
    // An Employee belongs to exactly one Company. A company can select many
    // employees at once and book one group appointment for all of them.
    public class Employee
    {
        [Key]                                    // primary key column: EmployeeId
        public int EmployeeId { get; set; }

        [Required]                               // foreign key pointing back to the owning Company
        public int CompanyId { get; set; }

        [ForeignKey(nameof(CompanyId))]          // tells EF Core which property is the FK for this navigation
        public Company? Company { get; set; }

        [Required, MaxLength(50)]                // company-defined employee number, unique per company
        public string EmployeeNumber { get; set; } = string.Empty;

        [Required, MaxLength(150)]               // employee full name
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(50)]                // civil ID / national ID, unique per company
        public string CivilId { get; set; } = string.Empty;

        [Required, MaxLength(20)]                // employee phone number
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, MaxLength(150)]               // employee email address
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(100)]               // department the employee works in
        public string Department { get; set; } = string.Empty;

        [Required, MaxLength(100)]               // employee's job title
        public string JobTitle { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now; // when the employee record was added

        // An employee can appear in many appointments (join table)
        public ICollection<AppointmentEmployee> AppointmentEmployees { get; set; } = new List<AppointmentEmployee>();
    }
}
