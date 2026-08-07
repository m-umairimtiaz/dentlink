using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityCompanyAppointmentSystem.Data;
using UniversityCompanyAppointmentSystem.Models;
using UniversityCompanyAppointmentSystem.ViewModels.Admin;

namespace UniversityCompanyAppointmentSystem.Controllers
{
    public class AdminReportsController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;

        public AdminReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateOnly? fromDate, DateOnly? toDate, int? universityId, int? companyId)
        {
            var query = _context.Appointments
                .Include(a => a.University)
                .Include(a => a.Company)
                .Include(a => a.AppointmentEmployees)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(a => a.AppointmentDate >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(a => a.AppointmentDate <= toDate.Value);
            if (universityId.HasValue)
                query = query.Where(a => a.UniversityId == universityId.Value);
            if (companyId.HasValue)
                query = query.Where(a => a.CompanyId == companyId.Value);

            var appointments = await query.ToListAsync();

            var model = new AdminReportsViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                UniversityId = universityId,
                CompanyId = companyId,
                Universities = await _context.Universities.OrderBy(u => u.UniversityName).ToListAsync(),
                Companies = await _context.Companies.OrderBy(c => c.CompanyName).ToListAsync(),
                TotalAppointments = appointments.Count,
                PendingCount = appointments.Count(a => a.Status == AppointmentStatus.Pending),
                ConfirmedCount = appointments.Count(a => a.Status == AppointmentStatus.Confirmed),
                RejectedCount = appointments.Count(a => a.Status == AppointmentStatus.Rejected),
                CancelledCount = appointments.Count(a => a.Status == AppointmentStatus.Cancelled),
                CompletedCount = appointments.Count(a => a.Status == AppointmentStatus.Completed),
                TotalEmployeesBooked = appointments.Sum(a => a.AppointmentEmployees.Count),
                ByUniversity = appointments
                    .GroupBy(a => a.University?.UniversityName ?? "Unknown")
                    .Select(g => new AdminReportRow
                    {
                        Name = g.Key,
                        AppointmentCount = g.Count(),
                        EmployeeCount = g.Sum(a => a.AppointmentEmployees.Count),
                        CompletedCount = g.Count(a => a.Status == AppointmentStatus.Completed)
                    })
                    .OrderByDescending(r => r.AppointmentCount)
                    .ToList(),
                ByCompany = appointments
                    .GroupBy(a => a.Company?.CompanyName ?? "Unknown")
                    .Select(g => new AdminReportRow
                    {
                        Name = g.Key,
                        AppointmentCount = g.Count(),
                        EmployeeCount = g.Sum(a => a.AppointmentEmployees.Count),
                        CompletedCount = g.Count(a => a.Status == AppointmentStatus.Completed)
                    })
                    .OrderByDescending(r => r.AppointmentCount)
                    .ToList()
            };

            return View(model);
        }
    }
}
