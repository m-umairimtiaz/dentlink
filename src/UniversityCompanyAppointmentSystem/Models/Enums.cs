namespace UniversityCompanyAppointmentSystem.Models
{
    // Status of a single appointment slot that a university publishes.
    // Available  = companies are allowed to book this slot.
    // Unavailable = university has closed/blocked this slot, companies cannot book it.
    public enum SlotStatus
    {
        Available = 0,   // default value, slot can be booked
        Unavailable = 1  // slot is closed for booking
    }

    // Life-cycle status of a group appointment booked by a company.
    // The order below also matches the normal flow: Pending -> Confirmed -> Completed.
    public enum AppointmentStatus
    {
        Pending = 0,    // just submitted by the company, waiting for the university to respond
        Confirmed = 1,  // university accepted the appointment
        Rejected = 2,   // university declined the appointment
        Cancelled = 3,  // appointment was cancelled after being confirmed (or by the company)
        Completed = 4   // the appointment date has passed and the visit happened
    }
}
