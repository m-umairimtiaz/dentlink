using Microsoft.EntityFrameworkCore;
using UniversityCompanyAppointmentSystem.Data;
using UniversityCompanyAppointmentSystem.Models;
using UniversityCompanyAppointmentSystem.ViewModels.AppointmentSlots;
using UniversityCompanyAppointmentSystem.ViewModels.Appointments;

namespace UniversityCompanyAppointmentSystem.Services
{
    // Contains all the business rules for booking and managing group appointments.
    // Keeping this logic here (instead of inside the controller) keeps controllers thin
    // and makes the rules easy to find in one place.
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public AppointmentService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<List<AvailableSlotViewModel>> GetAvailableSlotsAsync(int universityId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today); // "today" with no time part, to compare against AppointmentDate

            // Load every future slot for this university, together with the appointments already
            // booked into each slot (so we can work out how many seats are left).
            var slots = await _context.AppointmentSlots
                .Where(s => s.UniversityId == universityId && s.AppointmentDate >= today)
                .Include(s => s.Appointments)
                    .ThenInclude(a => a.AppointmentEmployees)
                .OrderBy(s => s.AppointmentDate)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            var result = new List<AvailableSlotViewModel>();

            foreach (var slot in slots)
            {
                // Only Pending and Confirmed appointments occupy seats; Rejected/Cancelled ones free them up again.
                int alreadyBooked = slot.Appointments
                    .Where(a => a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed)
                    .Sum(a => a.AppointmentEmployees.Count);

                int remaining = slot.MaximumEmployees - alreadyBooked;

                bool isBookable = slot.Status == SlotStatus.Available && remaining > 0;

                result.Add(new AvailableSlotViewModel
                {
                    AppointmentSlotId = slot.AppointmentSlotId,
                    DisplayText = $"{slot.AppointmentDate:dd MMM yyyy}, {slot.StartTime:hh\\:mm tt} - {slot.EndTime:hh\\:mm tt} " +
                                  (isBookable ? $"({remaining} seats left)" : "(unavailable)"),
                    RemainingCapacity = Math.Max(remaining, 0),
                    IsBookable = isBookable
                });
            }

            return result;
        }

        public async Task<ServiceResult> BookAppointmentAsync(BookAppointmentViewModel model, int companyId)
        {
            // 1) The university must exist.
            var university = await _context.Universities.FindAsync(model.UniversityId);
            if (university == null)
            {
                return ServiceResult.Fail("Selected university was not found.");
            }

            // 2) The slot must exist and belong to the chosen university.
            var slot = await _context.AppointmentSlots
                .Include(s => s.Appointments)
                    .ThenInclude(a => a.AppointmentEmployees)
                .FirstOrDefaultAsync(s => s.AppointmentSlotId == model.AppointmentSlotId && s.UniversityId == model.UniversityId);
            if (slot == null)
            {
                return ServiceResult.Fail("Selected appointment slot was not found.");
            }

            // 3) The slot must be marked Available and not be in the past.
            if (slot.Status != SlotStatus.Available)
            {
                return ServiceResult.Fail("This appointment slot is unavailable and cannot be booked.");
            }
            if (slot.AppointmentDate < DateOnly.FromDateTime(DateTime.Today))
            {
                return ServiceResult.Fail("This appointment slot is in the past and cannot be booked.");
            }

            // 4) The employees must actually belong to this company (prevents booking another company's staff).
            var employees = await _context.Employees
                .Where(e => model.EmployeeIds.Contains(e.EmployeeId) && e.CompanyId == companyId)
                .ToListAsync();
            if (employees.Count == 0)
            {
                return ServiceResult.Fail("At least one employee must be selected.");
            }

            // 5) Capacity check: seats already used by Pending/Confirmed appointments + this new request must fit.
            int alreadyBooked = slot.Appointments
                .Where(a => a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed)
                .Sum(a => a.AppointmentEmployees.Count);
            int remaining = slot.MaximumEmployees - alreadyBooked;
            if (employees.Count > remaining)
            {
                return ServiceResult.Fail($"Only {remaining} seat(s) remain in this slot, but {employees.Count} employees were selected.");
            }

            // Everything checked out - create the appointment.
            var appointment = new Appointment
            {
                UniversityId = university.UniversityId,
                CompanyId = companyId,
                AppointmentSlotId = slot.AppointmentSlotId,
                AppointmentDate = slot.AppointmentDate,   // copied from the slot so the record is self-contained
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                Purpose = model.Purpose,
                Status = AppointmentStatus.Pending,       // every new appointment starts as Pending
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                AppointmentReference = "TEMP"              // placeholder, replaced below once we have the real ID
            };

            // Link every selected employee to this one appointment (the "group appointment" part).
            foreach (var employee in employees)
            {
                appointment.AppointmentEmployees.Add(new AppointmentEmployee { EmployeeId = employee.EmployeeId });
            }

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync(); // after this call, appointment.AppointmentId is populated by the database

            // Build a human-friendly reference number now that we know the generated ID, e.g. APT-20260815-0007
            appointment.AppointmentReference = $"APT-{appointment.AppointmentDate:yyyyMMdd}-{appointment.AppointmentId:D4}";
            await _context.SaveChangesAsync();

            // Notify both sides that a new appointment request exists.
            var company = await _context.Companies.FindAsync(companyId);
            string companyName = company?.CompanyName ?? "A company";
            string dateText = appointment.AppointmentDate.ToString("dd MMMM yyyy");
            string timeText = appointment.StartTime.ToString("hh:mm tt");

            await _notificationService.CreateAsync(
                universityId: university.UniversityId,
                companyId: null,
                appointmentId: appointment.AppointmentId,
                message: $"{companyName} has submitted an appointment request for {employees.Count} employees on {dateText} at {timeText}.");

            await _notificationService.CreateAsync(
                universityId: null,
                companyId: companyId,
                appointmentId: appointment.AppointmentId,
                message: $"Your appointment request with {university.UniversityName} for {employees.Count} employees on {dateText} at {timeText} has been submitted.");

            return ServiceResult.Ok("Appointment booked successfully and is pending confirmation.", appointment.AppointmentId);
        }

        public async Task<List<Appointment>> GetForCompanyAsync(int companyId)
        {
            return await _context.Appointments
                .Include(a => a.University)
                .Include(a => a.AppointmentEmployees)
                .Where(a => a.CompanyId == companyId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetForUniversityAsync(int universityId)
        {
            return await _context.Appointments
                .Include(a => a.Company)
                .Include(a => a.AppointmentEmployees)
                .Where(a => a.UniversityId == universityId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Appointment?> GetDetailsAsync(int appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.University)
                .Include(a => a.Company)
                .Include(a => a.AppointmentEmployees)
                    .ThenInclude(ae => ae.Employee)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
        }

        public async Task<ServiceResult> ConfirmAsync(int appointmentId, int universityId)
        {
            var appointment = await FindOwnedByUniversityAsync(appointmentId, universityId);
            if (appointment == null) return ServiceResult.Fail("Appointment not found.");
            if (appointment.Status != AppointmentStatus.Pending)
                return ServiceResult.Fail("Only pending appointments can be confirmed.");

            appointment.Status = AppointmentStatus.Confirmed;
            appointment.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            await NotifyCompanyOfStatusChangeAsync(appointment, "confirmed");
            return ServiceResult.Ok("Appointment confirmed.");
        }

        public async Task<ServiceResult> RejectAsync(int appointmentId, int universityId)
        {
            var appointment = await FindOwnedByUniversityAsync(appointmentId, universityId);
            if (appointment == null) return ServiceResult.Fail("Appointment not found.");
            if (appointment.Status != AppointmentStatus.Pending)
                return ServiceResult.Fail("Only pending appointments can be rejected.");

            appointment.Status = AppointmentStatus.Rejected;
            appointment.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            await NotifyCompanyOfStatusChangeAsync(appointment, "rejected");
            return ServiceResult.Ok("Appointment rejected.");
        }

        public async Task<ServiceResult> CompleteAsync(int appointmentId, int universityId)
        {
            var appointment = await FindOwnedByUniversityAsync(appointmentId, universityId);
            if (appointment == null) return ServiceResult.Fail("Appointment not found.");
            if (appointment.Status != AppointmentStatus.Confirmed)
                return ServiceResult.Fail("Only confirmed appointments can be marked as completed.");

            appointment.Status = AppointmentStatus.Completed;
            appointment.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            await NotifyCompanyOfStatusChangeAsync(appointment, "completed");
            return ServiceResult.Ok("Appointment marked as completed.");
        }

        public async Task<ServiceResult> CancelAsync(int appointmentId, int universityId)
        {
            var appointment = await FindOwnedByUniversityAsync(appointmentId, universityId);
            if (appointment == null) return ServiceResult.Fail("Appointment not found.");
            if (appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.Completed)
                return ServiceResult.Fail("This appointment can no longer be cancelled.");

            appointment.Status = AppointmentStatus.Cancelled;
            appointment.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            await NotifyCompanyOfStatusChangeAsync(appointment, "cancelled");
            return ServiceResult.Ok("Appointment cancelled.");
        }

        // Helper: loads an appointment only if it belongs to the given university (ownership check).
        private async Task<Appointment?> FindOwnedByUniversityAsync(int appointmentId, int universityId)
        {
            return await _context.Appointments
                .Include(a => a.AppointmentEmployees)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && a.UniversityId == universityId);
        }

        // Helper: builds and stores the "your appointment has been ___" notification for the company.
        private async Task NotifyCompanyOfStatusChangeAsync(Appointment appointment, string statusWord)
        {
            var university = await _context.Universities.FindAsync(appointment.UniversityId);
            string universityName = university?.UniversityName ?? "the university";
            string dateText = appointment.AppointmentDate.ToString("dd MMMM yyyy");
            string timeText = appointment.StartTime.ToString("hh:mm tt");
            int employeeCount = appointment.AppointmentEmployees.Count;

            await _notificationService.CreateAsync(
                universityId: null,
                companyId: appointment.CompanyId,
                appointmentId: appointment.AppointmentId,
                message: $"Your appointment with {universityName} for {employeeCount} employees on {dateText} at {timeText} has been {statusWord}.");
        }
    }
}
