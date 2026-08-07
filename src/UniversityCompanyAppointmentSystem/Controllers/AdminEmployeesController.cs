using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityCompanyAppointmentSystem.Data;
using UniversityCompanyAppointmentSystem.ViewModels.Admin;

namespace UniversityCompanyAppointmentSystem.Controllers
{
    public class AdminEmployeesController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;

        public AdminEmployeesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? searchTerm, int? companyId)
        {
            var query = _context.Employees.Include(e => e.Company).AsQueryable();

            if (companyId.HasValue)
                query = query.Where(e => e.CompanyId == companyId.Value);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(e =>
                    e.FullName.Contains(term) ||
                    e.EmployeeNumber.Contains(term) ||
                    e.CivilId.Contains(term) ||
                    e.Email.Contains(term) ||
                    (e.Company != null && e.Company.CompanyName.Contains(term)));
            }

            var model = new AdminEmployeeListViewModel
            {
                SearchTerm = searchTerm,
                CompanyId = companyId,
                Companies = await _context.Companies.OrderBy(c => c.CompanyName).ToListAsync(),
                Employees = await query.OrderBy(e => e.Company!.CompanyName).ThenBy(e => e.FullName).ToListAsync()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Company)
                .Include(e => e.AppointmentEmployees)
                    .ThenInclude(ae => ae.Appointment!)
                        .ThenInclude(a => a.University)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null) return NotFound();
            return View(employee);
        }
    }
}
