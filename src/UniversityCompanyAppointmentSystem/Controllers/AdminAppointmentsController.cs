using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityCompanyAppointmentSystem.Data;
using UniversityCompanyAppointmentSystem.Models;
using UniversityCompanyAppointmentSystem.ViewModels.Admin;

namespace UniversityCompanyAppointmentSystem.Controllers
{
    public class AdminAppointmentsController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;

        public AdminAppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? searchTerm, AppointmentStatus? status, int? universityId, int? companyId)
        {
            var query = _context.Appointments
                .Include(a => a.University)
                .Include(a => a.Company)
                .Include(a => a.AppointmentEmployees)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(a => a.Status == status.Value);
            if (universityId.HasValue)
                query = query.Where(a => a.UniversityId == universityId.Value);
            if (companyId.HasValue)
                query = query.Where(a => a.CompanyId == companyId.Value);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(a =>
                    a.AppointmentReference.Contains(term) ||
                    a.Purpose.Contains(term) ||
                    (a.University != null && a.University.UniversityName.Contains(term)) ||
                    (a.Company != null && a.Company.CompanyName.Contains(term)));
            }

            var model = new AdminAppointmentListViewModel
            {
                SearchTerm = searchTerm,
                Status = status,
                UniversityId = universityId,
                CompanyId = companyId,
                Universities = await _context.Universities.OrderBy(u => u.UniversityName).ToListAsync(),
                Companies = await _context.Companies.OrderBy(c => c.CompanyName).ToListAsync(),
                Appointments = await query
                    .OrderByDescending(a => a.AppointmentDate)
                    .ThenByDescending(a => a.StartTime)
                    .Take(200)
                    .ToListAsync()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.University)
                .Include(a => a.Company)
                .Include(a => a.AppointmentSlot)
                .Include(a => a.AppointmentEmployees)
                    .ThenInclude(ae => ae.Employee)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null) return NotFound();
            return View(appointment);
        }
    }
}
