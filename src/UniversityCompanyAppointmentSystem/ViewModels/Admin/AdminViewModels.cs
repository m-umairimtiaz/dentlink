using System.ComponentModel.DataAnnotations;
using UniversityCompanyAppointmentSystem.Models;

namespace UniversityCompanyAppointmentSystem.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public int TotalUniversities { get; set; }
        public int TotalCompanies { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int ConfirmedAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public List<Appointment> RecentAppointments { get; set; } = new();
    }

    public class AdminAccountFormViewModel
    {
        public int? Id { get; set; }

        [Required, MaxLength(200)]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        [Display(Name = "Contact Person")]
        public string ContactPersonName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required, Phone, MaxLength(20)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string? ConfirmPassword { get; set; }

        public bool IsEdit => Id.HasValue;
    }

    public class AdminUniversityDetailsViewModel
    {
        public University University { get; set; } = null!;
        public int SlotCount { get; set; }
        public int AppointmentCount { get; set; }
        public List<Appointment> RecentAppointments { get; set; } = new();
        public List<AppointmentSlot> Slots { get; set; } = new();
    }

    public class AdminCompanyDetailsViewModel
    {
        public Company Company { get; set; } = null!;
        public List<Employee> Employees { get; set; } = new();
        public List<Appointment> RecentAppointments { get; set; } = new();
    }

    public class AdminEmployeeListViewModel
    {
        public string? SearchTerm { get; set; }
        public int? CompanyId { get; set; }
        public List<Company> Companies { get; set; } = new();
        public List<Employee> Employees { get; set; } = new();
    }

    public class AdminAppointmentListViewModel
    {
        public string? SearchTerm { get; set; }
        public AppointmentStatus? Status { get; set; }
        public int? UniversityId { get; set; }
        public int? CompanyId { get; set; }
        public List<University> Universities { get; set; } = new();
        public List<Company> Companies { get; set; } = new();
        public List<Appointment> Appointments { get; set; } = new();
    }

    public class AdminReportsViewModel
    {
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public int? UniversityId { get; set; }
        public int? CompanyId { get; set; }

        public List<University> Universities { get; set; } = new();
        public List<Company> Companies { get; set; } = new();

        public int TotalAppointments { get; set; }
        public int PendingCount { get; set; }
        public int ConfirmedCount { get; set; }
        public int RejectedCount { get; set; }
        public int CancelledCount { get; set; }
        public int CompletedCount { get; set; }
        public int TotalEmployeesBooked { get; set; }

        public List<AdminReportRow> ByUniversity { get; set; } = new();
        public List<AdminReportRow> ByCompany { get; set; } = new();
    }

    public class AdminReportRow
    {
        public string Name { get; set; } = string.Empty;
        public int AppointmentCount { get; set; }
        public int EmployeeCount { get; set; }
        public int CompletedCount { get; set; }
    }
}
