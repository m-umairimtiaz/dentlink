using UniversityCompanyAppointmentSystem.Models;

namespace UniversityCompanyAppointmentSystem.ViewModels.Notifications
{
    // Powers the Notifications/Index page: the list itself plus the unread count.
    public class NotificationListViewModel
    {
        public List<Notification> Notifications { get; set; } = new();
        public int UnreadCount { get; set; }
    }
}
