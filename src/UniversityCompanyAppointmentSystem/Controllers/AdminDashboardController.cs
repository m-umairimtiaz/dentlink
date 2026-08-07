using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityCompanyAppointmentSystem.Data;
using UniversityCompanyAppointmentSystem.Models;
using UniversityCompanyAppointmentSystem.ViewModels.Admin;

namespace UniversityCompanyAppointmentSystem.Controllers
{
    public class AdminDashboardController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;

        public AdminDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var recent = await _context.Appointments
                .Include(a => a.University)
                .Include(a => a.Company)
                .Include(a => a.AppointmentEmployees)
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .ToListAsync();

            var model = new AdminDashboardViewModel
            {
                TotalUniversities = await _context.Universities.CountAsync(),
                TotalCompanies = await _context.Companies.CountAsync(),
                TotalEmployees = await _context.Employees.CountAsync(),
                TotalAppointments = await _context.Appointments.CountAsync(),
                PendingAppointments = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Pending),
                ConfirmedAppointments = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Confirmed),
                CompletedAppointments = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Completed),
                RecentAppointments = recent
            };

            return View(model);
        }
    }
}
