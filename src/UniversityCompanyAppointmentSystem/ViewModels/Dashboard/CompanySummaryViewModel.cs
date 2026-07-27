namespace UniversityCompanyAppointmentSystem.ViewModels.Dashboard
{
    // One row on the University's "Companies" page: a company plus a quick count
    // of how many appointments it has booked with this university.
    public class CompanySummaryViewModel
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactPersonName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int TotalAppointments { get; set; }
    }
}
