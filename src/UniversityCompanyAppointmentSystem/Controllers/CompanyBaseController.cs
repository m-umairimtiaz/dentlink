using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using UniversityCompanyAppointmentSystem.Common;
using UniversityCompanyAppointmentSystem.Services;

namespace UniversityCompanyAppointmentSystem.Controllers
{
    // Any controller that only Companies are allowed to use should inherit from this class.
    // It centralises the "is a company actually logged in?" check so we don't repeat it
    // in every single action method.
    public abstract class CompanyBaseController : Controller
    {
        private readonly INotificationService _notificationService;

        protected int CompanyId { get; private set; }        // the logged-in company's ID, filled in below
        protected string CompanyName { get; private set; } = string.Empty;

        protected CompanyBaseController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // This runs automatically before every action method in any controller derived from this class.
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            int? companyId = context.HttpContext.Session.GetInt32(SessionKeys.CompanyId); // read from session cookie

            if (companyId == null)
            {
                // Not logged in as a company -> bounce back to the login page instead of running the action.
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            CompanyId = companyId.Value;
            CompanyName = context.HttpContext.Session.GetString(SessionKeys.DisplayName) ?? string.Empty;

            // Make the unread notification count available to every view (used by the nav bar badge).
            ViewBag.UnreadNotificationCount = await _notificationService.GetUnreadCountForCompanyAsync(CompanyId);
            ViewBag.CurrentAccountName = CompanyName;
            ViewBag.CurrentAccountType = "Company";

            await next(); // continue on to the actual action method
        }
    }
}
