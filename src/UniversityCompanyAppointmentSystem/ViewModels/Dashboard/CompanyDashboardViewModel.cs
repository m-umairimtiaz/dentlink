using UniversityCompanyAppointmentSystem.Models;

namespace UniversityCompanyAppointmentSystem.ViewModels.Dashboard
{
    // Statistics and recent activity shown on the Company Dashboard home page.
    public class CompanyDashboardViewModel
    {
        public int TotalEmployees { get; set; }
        public int TotalAppointments { get; set; }
        public List<Appointment> UpcomingAppointments { get; set; } = new();
        public List<Notification> RecentNotifications { get; set; } = new();
    }
}
