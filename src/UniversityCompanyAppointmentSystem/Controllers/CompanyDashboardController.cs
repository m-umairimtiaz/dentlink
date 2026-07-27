using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using UniversityCompanyAppointmentSystem.Data;
using UniversityCompanyAppointmentSystem.Models;
using UniversityCompanyAppointmentSystem.Services;
using UniversityCompanyAppointmentSystem.ViewModels.Dashboard;

namespace UniversityCompanyAppointmentSystem.Controllers
{
    // The landing page a Company sees right after logging in: quick statistics
    // plus upcoming appointments and recent notifications.
    public class CompanyDashboardController : CompanyBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public CompanyDashboardController(ApplicationDbContext context, INotificationService notificationService)
            : base(notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            int totalEmployees = await _context.Employees.CountAsync(e => e.CompanyId == CompanyId);
            int totalAppointments = await _context.Appointments.CountAsync(a => a.CompanyId == CompanyId);

            var upcoming = await _context.Appointments
                .Include(a => a.University)
                .Include(a => a.AppointmentEmployees)
                .Where(a => a.CompanyId == CompanyId
                            && a.AppointmentDate >= today
                            && (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed))
                .OrderBy(a => a.AppointmentDate).ThenBy(a => a.StartTime)
                .Take(5)
                .ToListAsync();

            var recentNotifications = await _notificationService.GetForCompanyAsync(CompanyId);

            var model = new CompanyDashboardViewModel
            {
                TotalEmployees = totalEmployees,
                TotalAppointments = totalAppointments,
                UpcomingAppointments = upcoming,
                RecentNotifications = recentNotifications.Take(5).ToList()
            };

            return View(model);
        }
    }
}
