using Microsoft.AspNetCore.Mvc;
using UniversityCompanyAppointmentSystem.Common;
using UniversityCompanyAppointmentSystem.Services;
using UniversityCompanyAppointmentSystem.ViewModels.Notifications;

namespace UniversityCompanyAppointmentSystem.Controllers
{
    // Shows the notification list and lets the current user (company or university) mark
    // notifications as read. Works for both roles, so it checks the session directly.
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // GET: /Notifications
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int? companyId = HttpContext.Session.GetInt32(SessionKeys.CompanyId);
            int? universityId = HttpContext.Session.GetInt32(SessionKeys.UniversityId);

            NotificationListViewModel model;

            if (companyId != null)
            {
                ViewBag.CurrentAccountType = "Company";
                model = new NotificationListViewModel
                {
                    Notifications = await _notificationService.GetForCompanyAsync(companyId.Value),
                    UnreadCount = await _notificationService.GetUnreadCountForCompanyAsync(companyId.Value)
                };
            }
            else if (universityId != null)
            {
                ViewBag.CurrentAccountType = "University";
                model = new NotificationListViewModel
                {
                    Notifications = await _notificationService.GetForUniversityAsync(universityId.Value),
                    UnreadCount = await _notificationService.GetUnreadCountForUniversityAsync(universityId.Value)
                };
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UnreadNotificationCount = model.UnreadCount; // keeps the shared nav bar badge in sync
            ViewBag.CurrentAccountName = HttpContext.Session.GetString(SessionKeys.DisplayName);
            return View(model);
        }

        // POST: /Notifications/MarkAsRead/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            if (HttpContext.Session.GetInt32(SessionKeys.CompanyId) == null &&
                HttpContext.Session.GetInt32(SessionKeys.UniversityId) == null)
            {
                return RedirectToAction("Login", "Account");
            }

            await _notificationService.MarkAsReadAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // POST: /Notifications/MarkAllAsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            int? companyId = HttpContext.Session.GetInt32(SessionKeys.CompanyId);
            int? universityId = HttpContext.Session.GetInt32(SessionKeys.UniversityId);

            if (companyId != null)
            {
                await _notificationService.MarkAllAsReadForCompanyAsync(companyId.Value);
            }
            else if (universityId != null)
            {
                await _notificationService.MarkAllAsReadForUniversityAsync(universityId.Value);
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
