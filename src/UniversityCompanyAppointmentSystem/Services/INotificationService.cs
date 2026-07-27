using UniversityCompanyAppointmentSystem.Models;

namespace UniversityCompanyAppointmentSystem.Services
{
    // Handles creating and reading in-website notifications for both account types.
    public interface INotificationService
    {
        Task CreateAsync(int? universityId, int? companyId, int? appointmentId, string message);

        Task<List<Notification>> GetForCompanyAsync(int companyId);
        Task<List<Notification>> GetForUniversityAsync(int universityId);

        Task<int> GetUnreadCountForCompanyAsync(int companyId);
        Task<int> GetUnreadCountForUniversityAsync(int universityId);

        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadForCompanyAsync(int companyId);
        Task MarkAllAsReadForUniversityAsync(int universityId);
    }
}
