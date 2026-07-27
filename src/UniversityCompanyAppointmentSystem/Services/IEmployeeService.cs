using UniversityCompanyAppointmentSystem.Models;

namespace UniversityCompanyAppointmentSystem.Services
{
    // Handles everything to do with a Company's Employees list, including the
    // "must be unique per company" rules for EmployeeNumber and CivilId.
    public interface IEmployeeService
    {
        Task<List<Employee>> GetForCompanyAsync(int companyId, string? searchTerm);
        Task<Employee?> GetByIdAsync(int employeeId, int companyId);   // returns null if not found OR not owned by this company

        Task<bool> IsEmployeeNumberDuplicateAsync(int companyId, string employeeNumber, int? excludeEmployeeId);
        Task<bool> IsCivilIdDuplicateAsync(int companyId, string civilId, int? excludeEmployeeId);

        Task CreateAsync(Employee employee);
        Task UpdateAsync(Employee employee);
        Task<bool> DeleteAsync(int employeeId, int companyId);          // returns false if the employee wasn't found/owned

        Task<List<Employee>> GetByIdsForCompanyAsync(List<int> employeeIds, int companyId);
    }
}
