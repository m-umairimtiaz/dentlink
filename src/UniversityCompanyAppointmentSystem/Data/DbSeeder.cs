using UniversityCompanyAppointmentSystem.Models;
using UniversityCompanyAppointmentSystem.Services;

namespace UniversityCompanyAppointmentSystem.Data
{
    // Fills the database with a small amount of sample data the first time the app runs,
    // so there is something to look at immediately without registering new accounts.
    // Every step checks "does this already exist?" first, so it is safe to run on every startup.
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, IPasswordHasher passwordHasher)
        {
            // ---------- One sample University ----------
            var university = context.Universities.FirstOrDefault(u => u.Email == "admin@ku.edu.kw");
            if (university == null)
            {
                university = new University
                {
                    UniversityName = "Kuwait University",
                    ContactPersonName = "Dr. Ahmad Al-Sabah",
                    Email = "admin@ku.edu.kw",
                    PhoneNumber = "+965 2225 1234",
                    PasswordHash = passwordHasher.Hash("University@123"), // sample login password, see README
                    CreatedAt = DateTime.Now
                };
                context.Universities.Add(university);
                await context.SaveChangesAsync();
            }

            // ---------- One sample Company ----------
            var company = context.Companies.FirstOrDefault(c => c.Email == "hr@gulftech.com");
            if (company == null)
            {
                company = new Company
                {
                    CompanyName = "Gulf Tech Company",
                    ContactPersonName = "Sara Al-Fahad",
                    Email = "hr@gulftech.com",
                    PhoneNumber = "+965 9001 2345",
                    PasswordHash = passwordHasher.Hash("Company@123"), // sample login password, see README
                    CreatedAt = DateTime.Now
                };
                context.Companies.Add(company);
                await context.SaveChangesAsync();
            }

            // ---------- Several sample Employees for that Company ----------
            if (!context.Employees.Any(e => e.CompanyId == company.CompanyId))
            {
                var employees = new List<Employee>
                {
                    new() { CompanyId = company.CompanyId, EmployeeNumber = "EMP-001", FullName = "John Smith",     CivilId = "290010112233", PhoneNumber = "+965 5001 0001", Email = "john.smith@gulftech.com",     Department = "IT",          JobTitle = "Software Engineer" },
                    new() { CompanyId = company.CompanyId, EmployeeNumber = "EMP-002", FullName = "Fatima Noor",    CivilId = "290020223344", PhoneNumber = "+965 5001 0002", Email = "fatima.noor@gulftech.com",    Department = "Human Resources", JobTitle = "HR Specialist" },
                    new() { CompanyId = company.CompanyId, EmployeeNumber = "EMP-003", FullName = "Omar Khalid",    CivilId = "290030334455", PhoneNumber = "+965 5001 0003", Email = "omar.khalid@gulftech.com",    Department = "Finance",      JobTitle = "Accountant" },
                    new() { CompanyId = company.CompanyId, EmployeeNumber = "EMP-004", FullName = "Layla Hassan",   CivilId = "290040445566", PhoneNumber = "+965 5001 0004", Email = "layla.hassan@gulftech.com",   Department = "Marketing",   JobTitle = "Marketing Coordinator" },
                    new() { CompanyId = company.CompanyId, EmployeeNumber = "EMP-005", FullName = "Yousef Ibrahim", CivilId = "290050556677", PhoneNumber = "+965 5001 0005", Email = "yousef.ibrahim@gulftech.com", Department = "IT",          JobTitle = "Network Administrator" }
                };
                context.Employees.AddRange(employees);
                await context.SaveChangesAsync();
            }

            // ---------- A few sample Appointment Slots for the University ----------
            if (!context.AppointmentSlots.Any(s => s.UniversityId == university.UniversityId))
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var slots = new List<AppointmentSlot>
                {
                    new() { UniversityId = university.UniversityId, AppointmentDate = today.AddDays(15), StartTime = new TimeOnly(9, 0),  EndTime = new TimeOnly(10, 0), MaximumEmployees = 15, Status = SlotStatus.Available },
                    new() { UniversityId = university.UniversityId, AppointmentDate = today.AddDays(17), StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 0), MaximumEmployees = 20, Status = SlotStatus.Available },
                    new() { UniversityId = university.UniversityId, AppointmentDate = today.AddDays(20), StartTime = new TimeOnly(13, 0), EndTime = new TimeOnly(14, 0), MaximumEmployees = 10, Status = SlotStatus.Available },
                    new() { UniversityId = university.UniversityId, AppointmentDate = today.AddDays(23), StartTime = new TimeOnly(9, 0),  EndTime = new TimeOnly(10, 0), MaximumEmployees = 25, Status = SlotStatus.Unavailable }
                };
                context.AppointmentSlots.AddRange(slots);
                await context.SaveChangesAsync();
            }
        }
    }
}
