using UniversityCompanyAppointmentSystem.Models;

namespace UniversityCompanyAppointmentSystem.ViewModels.Employees
{
    // Powers the main Employees/Index page: the search box, the table of
    // employees (with checkboxes for multi-select), and the "book appointment" button.
    public class EmployeeListViewModel
    {
        public string? SearchTerm { get; set; }              // current search text, kept so the box stays filled in after searching
        public List<Employee> Employees { get; set; } = new(); // employees to display (already filtered/searched)
    }
}
