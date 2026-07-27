using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using UniversityCompanyAppointmentSystem.Common;
using UniversityCompanyAppointmentSystem.Services;

namespace UniversityCompanyAppointmentSystem.Controllers
{
    // Any controller that only Universities are allowed to use should inherit from this class.
    // Mirrors CompanyBaseController but checks for a University session instead.
    public abstract class UniversityBaseController : Controller
    {
        private readonly INotificationService _notificationService;

        protected int UniversityId { get; private set; }
        protected string UniversityName { get; private set; } = string.Empty;

        protected UniversityBaseController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            int? universityId = context.HttpContext.Session.GetInt32(SessionKeys.UniversityId);

            if (universityId == null)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            UniversityId = universityId.Value;
            UniversityName = context.HttpContext.Session.GetString(SessionKeys.DisplayName) ?? string.Empty;

            ViewBag.UnreadNotificationCount = await _notificationService.GetUnreadCountForUniversityAsync(UniversityId);
            ViewBag.CurrentAccountName = UniversityName;
            ViewBag.CurrentAccountType = "University";

            await next();
        }
    }
}
