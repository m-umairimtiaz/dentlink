using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversityCompanyAppointmentSystem.Models
{
    // Join table implementing the many-to-many relationship between
    // Appointments and Employees: one appointment can have many employees,
    // and (in theory, over time) one employee can appear in many appointments.
    public class AppointmentEmployee
    {
        [Key]                                      // primary key column: AppointmentEmployeeId
        public int AppointmentEmployeeId { get; set; }

        [Required]
        public int AppointmentId { get; set; }

        [ForeignKey(nameof(AppointmentId))]
        public Appointment? Appointment { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public Employee? Employee { get; set; }
    }
}
