using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using UniversityCompanyAppointmentSystem.Data;
using UniversityCompanyAppointmentSystem.Models;
using UniversityCompanyAppointmentSystem.Services;
using UniversityCompanyAppointmentSystem.ViewModels.Dashboard;

namespace UniversityCompanyAppointmentSystem.Controllers
{
    // The landing page a University sees right after logging in, plus a simple
    // "Companies" page listing every company that has booked with this university.
    public class UniversityDashboardController : UniversityBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public UniversityDashboardController(ApplicationDbContext context, INotificationService notificationService)
            : base(notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            int totalCompanies = await _context.Appointments
                .Where(a => a.UniversityId == UniversityId)
                .Select(a => a.CompanyId)
                .Distinct()
                .CountAsync();

            var upcomingAppointments = await _context.Appointments
                .Include(a => a.AppointmentEmployees)
                .Where(a => a.UniversityId == UniversityId
                            && a.AppointmentDate >= today
                            && (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed))
                .ToListAsync();

            var todaysAppointments = await _context.Appointments
                .Include(a => a.Company)
                .Include(a => a.AppointmentEmployees)
                .Where(a => a.UniversityId == UniversityId && a.AppointmentDate == today)
                .OrderBy(a => a.StartTime)
                .ToListAsync();

            var recentNotifications = await _notificationService.GetForUniversityAsync(UniversityId);

            var model = new UniversityDashboardViewModel
            {
                TotalCompanies = totalCompanies,
                TotalUpcomingAppointments = upcomingAppointments.Count,
                TotalEmployeesExpected = upcomingAppointments.Sum(a => a.AppointmentEmployees.Count),
                TodaysAppointments = todaysAppointments,
                RecentNotifications = recentNotifications.Take(5).ToList()
            };

            return View(model);
        }

        // GET: /UniversityDashboard/Companies
        [HttpGet]
        public async Task<IActionResult> Companies()
        {
            // Group this university's appointments by company to build one summary row per company.
            var summaries = await _context.Appointments
                .Where(a => a.UniversityId == UniversityId)
                .Include(a => a.Company)
                .GroupBy(a => a.Company!)
                .Select(g => new CompanySummaryViewModel
                {
                    CompanyId = g.Key.CompanyId,
                    CompanyName = g.Key.CompanyName,
                    ContactPersonName = g.Key.ContactPersonName,
                    Email = g.Key.Email,
                    PhoneNumber = g.Key.PhoneNumber,
                    TotalAppointments = g.Count()
                })
                .OrderBy(c => c.CompanyName)
                .ToListAsync();

            return View(summaries);
        }
    }
}
