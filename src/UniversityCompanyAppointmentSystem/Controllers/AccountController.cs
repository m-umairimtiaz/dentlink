using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityCompanyAppointmentSystem.Common;
using UniversityCompanyAppointmentSystem.Data;
using UniversityCompanyAppointmentSystem.Models;
using UniversityCompanyAppointmentSystem.Services;
using UniversityCompanyAppointmentSystem.ViewModels.Account;

namespace UniversityCompanyAppointmentSystem.Controllers
{
    // Handles registration, login and logout for both Universities and Companies.
    // This is intentionally a very simple, session-based account system (no ASP.NET Identity,
    // no email verification, no roles/claims) as requested.
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public AccountController(ApplicationDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // GET: /Account/Register  -> simple page asking "Are you a University or a Company?"
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // GET: /Account/RegisterUniversity
        [HttpGet]
        public IActionResult RegisterUniversity()
        {
            return View(new RegisterUniversityViewModel());
        }

        // POST: /Account/RegisterUniversity
        [HttpPost]
        [ValidateAntiForgeryToken]                      // protects the form from cross-site request forgery
        public async Task<IActionResult> RegisterUniversity(RegisterUniversityViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);                     // redisplay the form with validation messages
            }

            // Email must be unique across Universities, Companies, and Admins.
            bool emailTaken = await _context.Universities.AnyAsync(u => u.Email == model.Email)
                             || await _context.Companies.AnyAsync(c => c.Email == model.Email)
                             || await _context.Admins.AnyAsync(a => a.Email == model.Email);
            if (emailTaken)
            {
                ModelState.AddModelError(nameof(model.Email), "This email is already registered.");
                return View(model);
            }

            var university = new University
            {
                UniversityName = model.UniversityName,
                ContactPersonName = model.ContactPersonName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                PasswordHash = _passwordHasher.Hash(model.Password), // never store the plain password
                CreatedAt = DateTime.Now
            };

            _context.Universities.Add(university);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "University account created successfully. Please log in.";
            return RedirectToAction(nameof(Login));
        }

        // GET: /Account/RegisterCompany
        [HttpGet]
        public IActionResult RegisterCompany()
        {
            return View(new RegisterCompanyViewModel());
        }

        // POST: /Account/RegisterCompany
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterCompany(RegisterCompanyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool emailTaken = await _context.Universities.AnyAsync(u => u.Email == model.Email)
                             || await _context.Companies.AnyAsync(c => c.Email == model.Email)
                             || await _context.Admins.AnyAsync(a => a.Email == model.Email);
            if (emailTaken)
            {
                ModelState.AddModelError(nameof(model.Email), "This email is already registered.");
                return View(model);
            }

            var company = new Company
            {
                CompanyName = model.CompanyName,
                ContactPersonName = model.ContactPersonName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                PasswordHash = _passwordHasher.Hash(model.Password),
                CreatedAt = DateTime.Now
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Company account created successfully. Please log in.";
            return RedirectToAction(nameof(Login));
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Platform admin first, then company, then university.
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == model.Email);
            if (admin != null)
            {
                if (!_passwordHasher.Verify(model.Password, admin.PasswordHash))
                {
                    ModelState.AddModelError(string.Empty, "Incorrect email or password.");
                    return View(model);
                }

                HttpContext.Session.SetInt32(SessionKeys.AdminId, admin.AdminId);
                HttpContext.Session.SetString(SessionKeys.AccountType, "Admin");
                HttpContext.Session.SetString(SessionKeys.DisplayName, admin.FullName);

                return RedirectToAction("Index", "AdminDashboard");
            }

            // Try to find the email among Companies first, then Universities.
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Email == model.Email);
            if (company != null)
            {
                if (!_passwordHasher.Verify(model.Password, company.PasswordHash))
                {
                    ModelState.AddModelError(string.Empty, "Incorrect email or password.");
                    return View(model);
                }

                // Save the logged-in company's identity in the session cookie.
                HttpContext.Session.SetInt32(SessionKeys.CompanyId, company.CompanyId);
                HttpContext.Session.SetString(SessionKeys.AccountType, "Company");
                HttpContext.Session.SetString(SessionKeys.DisplayName, company.CompanyName);

                return RedirectToAction("Index", "CompanyDashboard");
            }

            var university = await _context.Universities.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (university != null)
            {
                if (!_passwordHasher.Verify(model.Password, university.PasswordHash))
                {
                    ModelState.AddModelError(string.Empty, "Incorrect email or password.");
                    return View(model);
                }

                HttpContext.Session.SetInt32(SessionKeys.UniversityId, university.UniversityId);
                HttpContext.Session.SetString(SessionKeys.AccountType, "University");
                HttpContext.Session.SetString(SessionKeys.DisplayName, university.UniversityName);

                return RedirectToAction("Index", "UniversityDashboard");
            }

            // No account matched this email.
            ModelState.AddModelError(string.Empty, "Incorrect email or password.");
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();                 // wipe everything stored in the session
            return RedirectToAction(nameof(Login));
        }
    }
}
