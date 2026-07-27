using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityCompanyAppointmentSystem.Common;
using UniversityCompanyAppointmentSystem.Data;
using UniversityCompanyAppointmentSystem.Services;
using UniversityCompanyAppointmentSystem.ViewModels.Appointments;

namespace UniversityCompanyAppointmentSystem.Controllers
{
    // This controller is shared by BOTH Companies (booking, viewing their own appointments)
    // and Universities (viewing/managing appointment requests). Since the allowed actions are
    // different for each role, we check the session directly in each action instead of using
    // one of the role-restricted base controllers.
    public class AppointmentsController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IEmployeeService _employeeService;
        private readonly INotificationService _notificationService;
        private readonly ApplicationDbContext _context;

        public AppointmentsController(
            IAppointmentService appointmentService,
            IEmployeeService employeeService,
            INotificationService notificationService,
            ApplicationDbContext context)
        {
            _appointmentService = appointmentService;
            _employeeService = employeeService;
            _notificationService = notificationService;
            _context = context;
        }

        // Fills in the same ViewBag values that CompanyBaseController/UniversityBaseController set
        // automatically, so the shared _Layout.cshtml shows the correct sidebar, account name and
        // notification badge on every page rendered by this controller (both roles use it).
        private async Task SetNavigationViewBagAsync(int? companyId, int? universityId)
        {
            if (companyId != null)
            {
                ViewBag.CurrentAccountType = "Company";
                ViewBag.CurrentAccountName = HttpContext.Session.GetString(SessionKeys.DisplayName);
                ViewBag.UnreadNotificationCount = await _notificationService.GetUnreadCountForCompanyAsync(companyId.Value);
            }
            else if (universityId != null)
            {
                ViewBag.CurrentAccountType = "University";
                ViewBag.CurrentAccountName = HttpContext.Session.GetString(SessionKeys.DisplayName);
                ViewBag.UnreadNotificationCount = await _notificationService.GetUnreadCountForUniversityAsync(universityId.Value);
            }
        }

        // GET: /Appointments/Book?employeeIds=1&employeeIds=2...
        // Shown after a company ticks checkboxes on the Employees page and clicks
        // "Book Appointment for Selected Employees".
        [HttpGet]
        public async Task<IActionResult> Book(List<int> employeeIds)
        {
            int? companyId = HttpContext.Session.GetInt32(SessionKeys.CompanyId);
            if (companyId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            await SetNavigationViewBagAsync(companyId, null);

            if (employeeIds == null || employeeIds.Count == 0)
            {
                TempData["ErrorMessage"] = "Please select at least one employee before booking an appointment.";
                return RedirectToAction("Index", "Employees");
            }

            // Only employees that really belong to this company are allowed through.
            var employees = await _employeeService.GetByIdsForCompanyAsync(employeeIds, companyId.Value);
            if (employees.Count == 0)
            {
                TempData["ErrorMessage"] = "Selected employees could not be found.";
                return RedirectToAction("Index", "Employees");
            }

            var model = new BookAppointmentViewModel
            {
                EmployeeIds = employees.Select(e => e.EmployeeId).ToList(),
                SelectedEmployees = employees,
                Universities = await _context.Universities.OrderBy(u => u.UniversityName).ToListAsync()
            };

            return View(model);
        }

        // POST: /Appointments/Book
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(BookAppointmentViewModel model)
        {
            int? companyId = HttpContext.Session.GetInt32(SessionKeys.CompanyId);
            if (companyId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            await SetNavigationViewBagAsync(companyId, null);

            // Re-load the employees so we can redisplay the form if something is invalid.
            var employees = await _employeeService.GetByIdsForCompanyAsync(model.EmployeeIds, companyId.Value);
            model.SelectedEmployees = employees;
            model.Universities = await _context.Universities.OrderBy(u => u.UniversityName).ToListAsync();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _appointmentService.BookAppointmentAsync(model, companyId.Value);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message); // e.g. "slot is full", "slot unavailable"
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Details), new { id = result.Id });
        }

        // GET: /Appointments  -> list appointments for whichever role is currently logged in.
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int? companyId = HttpContext.Session.GetInt32(SessionKeys.CompanyId);
            int? universityId = HttpContext.Session.GetInt32(SessionKeys.UniversityId);

            if (companyId == null && universityId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            await SetNavigationViewBagAsync(companyId, universityId);

            if (companyId != null)
            {
                return View(await _appointmentService.GetForCompanyAsync(companyId.Value));
            }

            return View(await _appointmentService.GetForUniversityAsync(universityId!.Value));
        }

        // GET: /Appointments/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            int? companyId = HttpContext.Session.GetInt32(SessionKeys.CompanyId);
            int? universityId = HttpContext.Session.GetInt32(SessionKeys.UniversityId);
            if (companyId == null && universityId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var appointment = await _appointmentService.GetDetailsAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            // Ownership check: a company may only see its own appointments, a university only its own.
            bool ownedByCompany = companyId != null && appointment.CompanyId == companyId;
            bool ownedByUniversity = universityId != null && appointment.UniversityId == universityId;
            if (!ownedByCompany && !ownedByUniversity)
            {
                return Forbid();
            }

            // Only set the ViewBag for the role that actually owns this appointment.
            await SetNavigationViewBagAsync(ownedByCompany ? companyId : null, ownedByUniversity ? universityId : null);

            var model = new AppointmentDetailsViewModel
            {
                Appointment = appointment,
                Employees = appointment.AppointmentEmployees.Select(ae => ae.Employee!).ToList()
            };

            return View(model);
        }

        // The four university-only status-change actions below all follow the same shape:
        // check the session, call the service, show a message, and go back to Details.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            int? universityId = HttpContext.Session.GetInt32(SessionKeys.UniversityId);
            if (universityId == null) return RedirectToAction("Login", "Account");

            var result = await _appointmentService.ConfirmAsync(id, universityId.Value);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            int? universityId = HttpContext.Session.GetInt32(SessionKeys.UniversityId);
            if (universityId == null) return RedirectToAction("Login", "Account");

            var result = await _appointmentService.RejectAsync(id, universityId.Value);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            int? universityId = HttpContext.Session.GetInt32(SessionKeys.UniversityId);
            if (universityId == null) return RedirectToAction("Login", "Account");

            var result = await _appointmentService.CompleteAsync(id, universityId.Value);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            int? universityId = HttpContext.Session.GetInt32(SessionKeys.UniversityId);
            if (universityId == null) return RedirectToAction("Login", "Account");

            var result = await _appointmentService.CancelAsync(id, universityId.Value);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
