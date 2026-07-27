using UniversityCompanyAppointmentSystem.Models;

namespace UniversityCompanyAppointmentSystem.ViewModels.Dashboard
{
    // Statistics and recent activity shown on the University Dashboard home page.
    public class UniversityDashboardViewModel
    {
        public int TotalCompanies { get; set; }             // distinct companies that have booked at least one appointment
        public int TotalUpcomingAppointments { get; set; }
        public int TotalEmployeesExpected { get; set; }     // sum of employees across upcoming confirmed/pending appointments
        public List<Appointment> TodaysAppointments { get; set; } = new();
        public List<Notification> RecentNotifications { get; set; } = new();
    }
}
