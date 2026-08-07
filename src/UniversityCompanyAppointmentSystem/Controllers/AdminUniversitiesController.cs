using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityCompanyAppointmentSystem.Data;
using UniversityCompanyAppointmentSystem.Models;
using UniversityCompanyAppointmentSystem.Services;
using UniversityCompanyAppointmentSystem.ViewModels.Admin;

namespace UniversityCompanyAppointmentSystem.Controllers
{
    public class AdminUniversitiesController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public AdminUniversitiesController(ApplicationDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var query = _context.Universities.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(u =>
                    u.UniversityName.Contains(term) ||
                    u.Email.Contains(term) ||
                    u.ContactPersonName.Contains(term));
            }

            ViewBag.SearchTerm = searchTerm;
            var list = await query.OrderBy(u => u.UniversityName).ToListAsync();
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var university = await _context.Universities.FirstOrDefaultAsync(u => u.UniversityId == id);
            if (university == null) return NotFound();

            var model = new AdminUniversityDetailsViewModel
            {
                University = university,
                SlotCount = await _context.AppointmentSlots.CountAsync(s => s.UniversityId == id),
                AppointmentCount = await _context.Appointments.CountAsync(a => a.UniversityId == id),
                Slots = await _context.AppointmentSlots
                    .Where(s => s.UniversityId == id)
                    .OrderByDescending(s => s.AppointmentDate)
                    .Take(20)
                    .ToListAsync(),
                RecentAppointments = await _context.Appointments
                    .Include(a => a.Company)
                    .Include(a => a.AppointmentEmployees)
                    .Where(a => a.UniversityId == id)
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

            _context.Universities.Add(new University
            {
                UniversityName = model.Name,
                ContactPersonName = model.ContactPersonName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                PasswordHash = _passwordHasher.Hash(model.Password!),
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "University created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var university = await _context.Universities.FindAsync(id);
            if (university == null) return NotFound();

            return View("Form", new AdminAccountFormViewModel
            {
                Id = university.UniversityId,
                Name = university.UniversityName,
                ContactPersonName = university.ContactPersonName,
                Email = university.Email,
                PhoneNumber = university.PhoneNumber
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

            var university = await _context.Universities.FindAsync(id);
            if (university == null) return NotFound();

            if (await EmailTakenAsync(model.Email, excludeUniversityId: id))
            {
                ModelState.AddModelError(nameof(model.Email), "This email is already registered.");
                return View("Form", model);
            }

            university.UniversityName = model.Name;
            university.ContactPersonName = model.ContactPersonName;
            university.Email = model.Email;
            university.PhoneNumber = model.PhoneNumber;
            if (!string.IsNullOrWhiteSpace(model.Password))
                university.PasswordHash = _passwordHasher.Hash(model.Password);

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "University updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task<bool> EmailTakenAsync(string email, int? excludeUniversityId = null)
        {
            return await _context.Universities.AnyAsync(u => u.Email == email && u.UniversityId != (excludeUniversityId ?? 0))
                || await _context.Companies.AnyAsync(c => c.Email == email)
                || await _context.Admins.AnyAsync(a => a.Email == email);
        }
    }
}
