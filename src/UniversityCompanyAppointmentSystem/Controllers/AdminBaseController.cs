using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using UniversityCompanyAppointmentSystem.Common;

namespace UniversityCompanyAppointmentSystem.Controllers
{
    // Controllers that only platform Admins may use inherit from this class.
    public abstract class AdminBaseController : Controller
    {
        protected int AdminId { get; private set; }
        protected string AdminName { get; private set; } = string.Empty;

        public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            int? adminId = context.HttpContext.Session.GetInt32(SessionKeys.AdminId);

            if (adminId == null)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return Task.CompletedTask;
            }

            AdminId = adminId.Value;
            AdminName = context.HttpContext.Session.GetString(SessionKeys.DisplayName) ?? string.Empty;

            ViewBag.UnreadNotificationCount = 0;
            ViewBag.CurrentAccountName = AdminName;
            ViewBag.CurrentAccountType = "Admin";

            return next();
        }
    }
}
