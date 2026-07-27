using Microsoft.EntityFrameworkCore;
using UniversityCompanyAppointmentSystem.Data;
using UniversityCompanyAppointmentSystem.Models;

namespace UniversityCompanyAppointmentSystem.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;   // EF Core database context, injected by DI

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Creates one notification row. Pass universityId OR companyId (not both) depending on who it's for.
        public async Task CreateAsync(int? universityId, int? companyId, int? appointmentId, string message)
        {
            var notification = new Notification
            {
                UniversityId = universityId,       // null when the notification is for a company
                CompanyId = companyId,             // null when the notification is for a university
                AppointmentId = appointmentId,     // links back to the appointment that triggered this
                Message = message,
                IsRead = false,                    // new notifications always start unread
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);  // stage the insert
            await _context.SaveChangesAsync();          // write it to the database
        }

        public async Task<List<Notification>> GetForCompanyAsync(int companyId)
        {
            return await _context.Notifications
                .Where(n => n.CompanyId == companyId)      // only this company's notifications
                .OrderByDescending(n => n.CreatedAt)       // newest first
                .ToListAsync();
        }

        public async Task<List<Notification>> GetForUniversityAsync(int universityId)
        {
            return await _context.Notifications
                .Where(n => n.UniversityId == universityId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountForCompanyAsync(int companyId)
        {
            return await _context.Notifications
                .CountAsync(n => n.CompanyId == companyId && !n.IsRead); // count rows where IsRead is false
        }

        public async Task<int> GetUnreadCountForUniversityAsync(int universityId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UniversityId == universityId && !n.IsRead);
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId); // look up by primary key
            if (notification != null)
            {
                notification.IsRead = true;             // flip the flag
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadForCompanyAsync(int companyId)
        {
            // ExecuteUpdateAsync runs a single UPDATE statement instead of loading every row into memory.
            await _context.Notifications
                .Where(n => n.CompanyId == companyId && !n.IsRead)
                .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true));
        }

        public async Task MarkAllAsReadForUniversityAsync(int universityId)
        {
            await _context.Notifications
                .Where(n => n.UniversityId == universityId && !n.IsRead)
                .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true));
        }
    }
}
