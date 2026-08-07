using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityCompanyAppointmentSystem.Data;
using UniversityCompanyAppointmentSystem.Models;
using UniversityCompanyAppointmentSystem.Services;
using UniversityCompanyAppointmentSystem.ViewModels.Admin;

namespace UniversityCompanyAppointmentSystem.Controllers
{
    public class AdminCompaniesController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public AdminCompaniesController(ApplicationDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var query = _context.Companies.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(c =>
                    c.CompanyName.Contains(term) ||
                    c.Email.Contains(term) ||
                    c.ContactPersonName.Contains(term));
            }

            ViewBag.SearchTerm = searchTerm;
            var list = await query.OrderBy(c => c.CompanyName).ToListAsync();
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.CompanyId == id);
            if (company == null) return NotFound();

            var model = new AdminCompanyDetailsViewModel
            {
                Company = company,
                Employees = await _context.Employees
                    .Where(e => e.CompanyId == id)
                    .OrderBy(e => e.FullName)
                    .ToListAsync(),
                RecentAppointments = await _context.Appointments
                    .Include(a => a.University)
                    .Include(a => a.AppointmentEmployees)
                    .Where(a => a.CompanyId == id)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(20)
                    .ToListAsync()
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("Form", new AdminAccountFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminAccountFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Password))
                ModelState.AddModelError(nameof(model.Password), "Password is required.");

            if (!ModelState.IsValid)
                return View("Form", model);

            if (await EmailTakenAsync(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "This email is already registered.");
                return View("Form", model);
            }

            _context.Companies.Add(new Company
            {
                CompanyName = model.Name,
                ContactPersonName = model.ContactPersonName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                PasswordHash = _passwordHasher.Hash(model.Password!),
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Company created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null) return NotFound();

            return View("Form", new AdminAccountFormViewModel
            {
                Id = company.CompanyId,
                Name = company.CompanyName,
                ContactPersonName = company.ContactPersonName,
                Email = company.Email,
                PhoneNumber = company.PhoneNumber
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminAccountFormViewModel model)
        {
            if (id != model.Id) return BadRequest();

            if (!string.IsNullOrWhiteSpace(model.Password) || !string.IsNullOrWhiteSpace(model.ConfirmPassword))
            {
                if (string.IsNullOrWhiteSpace(model.Password) || model.Password.Length < 6)
                    ModelState.AddModelError(nameof(model.Password), "Password must be at least 6 characters.");
                if (model.Password != model.ConfirmPassword)
                    ModelState.AddModelError(nameof(model.ConfirmPassword), "Passwords do not match.");
            }
            else
            {
                ModelState.Remove(nameof(model.Password));
                ModelState.Remove(nameof(model.ConfirmPassword));
            }

            if (!ModelState.IsValid)
                return View("Form", model);

            var company = await _context.Companies.FindAsync(id);
            if (company == null) return NotFound();

            if (await EmailTakenAsync(model.Email, excludeCompanyId: id))
            {
                ModelState.AddModelError(nameof(model.Email), "This email is already registered.");
                return View("Form", model);
            }

            company.CompanyName = model.Name;
            company.ContactPersonName = model.ContactPersonName;
            company.Email = model.Email;
            company.PhoneNumber = model.PhoneNumber;
            if (!string.IsNullOrWhiteSpace(model.Password))
                company.PasswordHash = _passwordHasher.Hash(model.Password);

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Company updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task<bool> EmailTakenAsync(string email, int? excludeCompanyId = null)
        {
            return await _context.Companies.AnyAsync(c => c.Email == email && c.CompanyId != (excludeCompanyId ?? 0))
                || await _context.Universities.AnyAsync(u => u.Email == email)
                || await _context.Admins.AnyAsync(a => a.Email == email);
        }
    }
}
