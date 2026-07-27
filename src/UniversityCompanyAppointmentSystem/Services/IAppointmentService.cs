using UniversityCompanyAppointmentSystem.Models;
using UniversityCompanyAppointmentSystem.ViewModels.AppointmentSlots;
using UniversityCompanyAppointmentSystem.ViewModels.Appointments;

namespace UniversityCompanyAppointmentSystem.Services
{
    // Simple result wrapper so services can return "did it work + why not" without throwing exceptions
    // for expected situations like "this slot is full".
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int Id { get; set; }   // id of the created/affected record, when relevant

        public static ServiceResult Ok(string message, int id = 0) => new() { Success = true, Message = message, Id = id };
        public static ServiceResult Fail(string message) => new() { Success = false, Message = message };
    }

    public interface IAppointmentService
    {
        // Returns the slots for a university, annotated with remaining capacity, for the booking page.
        Task<List<AvailableSlotViewModel>> GetAvailableSlotsAsync(int universityId);

        Task<ServiceResult> BookAppointmentAsync(BookAppointmentViewModel model, int companyId);

        Task<List<Appointment>> GetForCompanyAsync(int companyId);
        Task<List<Appointment>> GetForUniversityAsync(int universityId);

        Task<Appointment?> GetDetailsAsync(int appointmentId);

        Task<ServiceResult> ConfirmAsync(int appointmentId, int universityId);
        Task<ServiceResult> RejectAsync(int appointmentId, int universityId);
        Task<ServiceResult> CompleteAsync(int appointmentId, int universityId);
        Task<ServiceResult> CancelAsync(int appointmentId, int universityId);
    }
}
