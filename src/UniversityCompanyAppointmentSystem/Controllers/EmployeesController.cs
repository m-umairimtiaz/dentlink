using Microsoft.AspNetCore.Mvc;
using UniversityCompanyAppointmentSystem.Models;
using UniversityCompanyAppointmentSystem.Services;
using UniversityCompanyAppointmentSystem.ViewModels.Employees;

namespace UniversityCompanyAppointmentSystem.Controllers
{
    // Lets a logged-in Company manage its own employee list: search, add, edit, delete,
    // and select multiple employees before booking a group appointment.
    public class EmployeesController : CompanyBaseController
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService, INotificationService notificationService)
            : base(notificationService)
        {
            _employeeService = employeeService;
        }

        // GET: /Employees?searchTerm=...
        [HttpGet]
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var employees = await _employeeService.GetForCompanyAsync(CompanyId, searchTerm); // CompanyId comes from the base controller

            var model = new EmployeeListViewModel
            {
                SearchTerm = searchTerm,
                Employees = employees
            };

            return View(model);
        }

        // GET: /Employees/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new EmployeeFormViewModel());
        }

        // POST: /Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Uniqueness rules: employee number and civil ID must not be reused within the SAME company.
            if (await _employeeService.IsEmployeeNumberDuplicateAsync(CompanyId, model.EmployeeNumber, null))
            {
                ModelState.AddModelError(nameof(model.EmployeeNumber), "This employee number is already used by another employee.");
            }
            if (await _employeeService.IsCivilIdDuplicateAsync(CompanyId, model.CivilId, null))
            {
                ModelState.AddModelError(nameof(model.CivilId), "This civil ID is already used by another employee.");
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var employee = new Employee
            {
                CompanyId = CompanyId,                 // always attach to the current logged-in company
                EmployeeNumber = model.EmployeeNumber,
                FullName = model.FullName,
                CivilId = model.CivilId,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                Department = model.Department,
                JobTitle = model.JobTitle
            };

            await _employeeService.CreateAsync(employee);

            TempData["SuccessMessage"] = $"Employee \"{employee.FullName}\" was added successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Employees/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _employeeService.GetByIdAsync(id, CompanyId); // ownership check inside the service
            if (employee == null)
            {
                TempData["ErrorMessage"] = "Employee not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new EmployeeFormViewModel
            {
                EmployeeId = employee.EmployeeId,
                EmployeeNumber = employee.EmployeeNumber,
                FullName = employee.FullName,
                CivilId = employee.CivilId,
                PhoneNumber = employee.PhoneNumber,
                Email = employee.Email,
                Department = employee.Department,
                JobTitle = employee.JobTitle
            };

            return View(model);
        }

        // POST: /Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeFormViewModel model)
        {
            if (id != model.EmployeeId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var employee = await _employeeService.GetByIdAsync(id, CompanyId); // re-check ownership before saving
            if (employee == null)
            {
                TempData["ErrorMessage"] = "Employee not found.";
                return RedirectToAction(nameof(Index));
            }

            if (await _employeeService.IsEmployeeNumberDuplicateAsync(CompanyId, model.EmployeeNumber, id))
            {
                ModelState.AddModelError(nameof(model.EmployeeNumber), "This employee number is already used by another employee.");
            }
            if (await _employeeService.IsCivilIdDuplicateAsync(CompanyId, model.CivilId, id))
            {
                ModelState.AddModelError(nameof(model.CivilId), "This civil ID is already used by another employee.");
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            employee.EmployeeNumber = model.EmployeeNumber;
            employee.FullName = model.FullName;
            employee.CivilId = model.CivilId;
            employee.PhoneNumber = model.PhoneNumber;
            employee.Email = model.Email;
            employee.Department = model.Department;
            employee.JobTitle = model.JobTitle;

            await _employeeService.UpdateAsync(employee);

            TempData["SuccessMessage"] = $"Employee \"{employee.FullName}\" was updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Employees/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            bool deleted = await _employeeService.DeleteAsync(id, CompanyId); // ownership check inside the service

            TempData[deleted ? "SuccessMessage" : "ErrorMessage"] =
                deleted ? "Employee deleted successfully." : "Employee not found.";

            return RedirectToAction(nameof(Index));
        }
    }
}
