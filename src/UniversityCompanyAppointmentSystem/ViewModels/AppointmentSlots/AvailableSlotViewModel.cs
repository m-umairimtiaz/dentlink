namespace UniversityCompanyAppointmentSystem.ViewModels.AppointmentSlots
{
    // Lightweight shape returned as JSON to the booking page when a company
    // picks a university, so JavaScript can fill in the "Select Slot" dropdown.
    public class AvailableSlotViewModel
    {
        public int AppointmentSlotId { get; set; }
        public string DisplayText { get; set; } = string.Empty; // e.g. "15 Aug 2026, 10:00 - 11:00 (12 seats left)"
        public int RemainingCapacity { get; set; }               // how many more employees this slot can still take
        public bool IsBookable { get; set; }                     // false if unavailable, full, or in the past (shown greyed out)
    }
}
