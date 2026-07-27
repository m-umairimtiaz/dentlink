using UniversityCompanyAppointmentSystem.Models;

namespace UniversityCompanyAppointmentSystem.ViewModels.Appointments
{
    // Everything the Appointment Details page needs to display in one place.
    public class AppointmentDetailsViewModel
    {
        public Appointment Appointment { get; set; } = null!;   // the appointment itself (with University/Company loaded)
        public List<Employee> Employees { get; set; } = new();  // the employees linked to this appointment
    }
}
