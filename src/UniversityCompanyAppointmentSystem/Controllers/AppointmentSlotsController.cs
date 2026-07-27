using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityCompanyAppointmentSystem.Common;
using UniversityCompanyAppointmentSystem.Data;
using UniversityCompanyAppointmentSystem.Models;
using UniversityCompanyAppointmentSystem.Services;
using UniversityCompanyAppointmentSystem.ViewModels.AppointmentSlots;

namespace UniversityCompanyAppointmentSystem.Controllers
{
    // Lets a University publish/manage the time slots that companies can book into.
    // One action (GetAvailableSlots) is also called by Companies via AJAX from the booking
    // page, so this controller checks session manually instead of using a role-restricted base class.
    public class AppointmentSlotsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAppointmentService _appointmentService;
        private readonly INotificationService _notificationService;

        public AppointmentSlotsController(ApplicationDbContext context, IAppointmentService appointmentService, INotificationService notificationService)
        {
            _context = context;
            _appointmentService = appointmentService;
            _notificationService = notificationService;
        }

        // Fills in the same ViewBag values CompanyBaseController/UniversityBaseController set
        // automatically, so _Layout.cshtml renders the university sidebar and notification badge
        // correctly on every page here too (this controller is a plain Controller, not a base class).
        private async Task SetNavigationViewBagAsync(int universityId)
        {
            ViewBag.CurrentAccountType = "University";
            ViewBag.CurrentAccountName = HttpContext.Session.GetString(SessionKeys.DisplayName);
            ViewBag.UnreadNotificationCount = await _notificationService.GetUnreadCountForUniversityAsync(universityId);
        }

        // GET: /AppointmentSlots  -> list of every slot this university has published.
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int? universityId = HttpContext.Session.GetInt32(SessionKeys.UniversityId);
            if (universityId == null) return RedirectToAction("Login", "Account");
            await SetNavigationViewBagAsync(universityId.Value);

            var slots = await _context.AppointmentSlots
                .Where(s => s.UniversityId == universityId)
                .OrderBy(s => s.AppointmentDate)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            return View(slots);
        }

        // GET: /AppointmentSlots/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            int? universityId = HttpContext.Session.GetInt32(SessionKeys.UniversityId);
            if (universityId == null) return RedirectToAction("Login", "Account");
            await SetNavigationViewBagAsync(universityId.Value);

            return View(new AppointmentSlotFormViewModel());
        }

        // POST: /AppointmentSlots/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentSlotFormViewModel model)
        {
            int? universityId = HttpContext.Session.GetInt32(SessionKeys.UniversityId);
            if (universityId == null) return RedirectToAction("Login", "Account");
            await SetNavigationViewBagAsync(universityId.Value);

            if (model.AppointmentDate < DateOnly.FromDateTime(DateTime.Today))
            {
                ModelState.AddModelError(nameof(model.AppointmentDate), "Appointment date cannot be in the past.");
            }
            if (model.EndTime <= model.StartTime)
            {
                ModelState.AddModelError(nameof(model.EndTime), "End time must be after start time.");
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var slot = new AppointmentSlot
            {
                UniversityId = universityId.Value,
                AppointmentDate = model.AppointmentDate,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                MaximumEmployees = model.MaximumEmployees,
                Status = model.Status,
                CreatedAt = DateTime.Now
            };

            _context.AppointmentSlots.Add(slot);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Appointment slot created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /AppointmentSlots/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            int? universityId = HttpContext.Session.GetInt32(SessionKeys.UniversityId);
            if (universityId == null) return RedirectToAction("Login", "Account");
            await SetNavigationViewBagAsync(universityId.Value);

            var slot = await _context.AppointmentSlots
                .FirstOrDefaultAsync(s => s.AppointmentSlotId == id && s.UniversityId == universityId); // ownership check
            if (slot == null)
            {
                TempData["ErrorMessage"] = "Appointment slot not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new AppointmentSlotFormViewModel
            {
                AppointmentSlotId = slot.AppointmentSlotId,
                AppointmentDate = slot.AppointmentDate,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                MaximumEmployees = slot.MaximumEmployees,
                Status = slot.Status
            };

            return View(model);
        }

        // POST: /AppointmentSlots/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppointmentSlotFormViewModel model)
        {
            int? universityId = HttpContext.Session.GetInt32(SessionKeys.UniversityId);
            if (universityId == null) return RedirectToAction("Login", "Account");
            if (id != model.AppointmentSlotId) return BadRequest();
            await SetNavigationViewBagAsync(universityId.Value);

            if (model.EndTime <= model.StartTime)
            {
                ModelState.AddModelError(nameof(model.EndTime), "End time must be after start time.");
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var slot = await _context.AppointmentSlots
                .FirstOrDefaultAsync(s => s.AppointmentSlotId == id && s.UniversityId == universityId);
            if (slot == null)
            {
                TempData["ErrorMessage"] = "Appointment slot not found.";
                return RedirectToAction(nameof(Index));
            }

            slot.AppointmentDate = model.AppointmentDate;
            slot.StartTime = model.StartTime;
            slot.EndTime = model.EndTime;
            slot.MaximumEmployees = model.MaximumEmployees;
            slot.Status = model.Status;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Appointment slot updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /AppointmentSlots/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            int? universityId = HttpContext.Session.GetInt32(SessionKeys.UniversityId);
            if (universityId == null) return RedirectToAction("Login", "Account");

            var slot = await _context.AppointmentSlots
                .FirstOrDefaultAsync(s => s.AppointmentSlotId == id && s.UniversityId == universityId);
            if (slot == null)
            {
                TempData["ErrorMessage"] = "Appointment slot not found.";
                return RedirectToAction(nameof(Index));
            }

            // A slot that already has appointments booked into it should not be deleted outright,
            // otherwise those appointments would point to a slot that no longer exists.
            bool hasAppointments = await _context.Appointments.AnyAsync(a => a.AppointmentSlotId == id);
            if (hasAppointments)
            {
                TempData["ErrorMessage"] = "This slot already has appointments booked and cannot be deleted. Mark it Unavailable instead.";
                return RedirectToAction(nameof(Index));
            }

            _context.AppointmentSlots.Remove(slot);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Appointment slot deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /AppointmentSlots/GetAvailableSlots?universityId=5
        // Called by JavaScript (fetch/AJAX) from the company's booking page whenever the
        // "Select University" dropdown changes, so it can fill in the slot dropdown.
        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int universityId)
        {
            int? companyId = HttpContext.Session.GetInt32(SessionKeys.CompanyId);
            if (companyId == null)
            {
                return Unauthorized();
            }

            List<AvailableSlotViewModel> slots = await _appointmentService.GetAvailableSlotsAsync(universityId);
            return Json(slots); // ASP.NET Core MVC serializes the list to JSON automatically
        }
    }
}
