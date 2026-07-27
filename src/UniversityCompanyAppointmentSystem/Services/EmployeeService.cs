using Microsoft.EntityFrameworkCore;
using UniversityCompanyAppointmentSystem.Data;
using UniversityCompanyAppointmentSystem.Models;

namespace UniversityCompanyAppointmentSystem.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;

        public EmployeeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Employee>> GetForCompanyAsync(int companyId, string? searchTerm)
        {
            // Start with only this company's employees - a company must never see another company's staff.
            var query = _context.Employees.Where(e => e.CompanyId == companyId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                // Search by full name OR employee number, case-insensitive "contains" match.
                query = query.Where(e =>
                    e.FullName.Contains(searchTerm) ||
                    e.EmployeeNumber.Contains(searchTerm));
            }

            return await query.OrderBy(e => e.FullName).ToListAsync();
        }

        public async Task<Employee?> GetByIdAsync(int employeeId, int companyId)
        {
            // The CompanyId check here is what stops a company editing/deleting another company's employee.
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.CompanyId == companyId);
        }

        public async Task<bool> IsEmployeeNumberDuplicateAsync(int companyId, string employeeNumber, int? excludeEmployeeId)
        {
            return await _context.Employees.AnyAsync(e =>
                e.CompanyId == companyId &&
                e.EmployeeNumber == employeeNumber &&
                e.EmployeeId != (excludeEmployeeId ?? 0));   // when editing, ignore the employee's own row
        }

        public async Task<bool> IsCivilIdDuplicateAsync(int companyId, string civilId, int? excludeEmployeeId)
        {
            return await _context.Employees.AnyAsync(e =>
                e.CompanyId == companyId &&
                e.CivilId == civilId &&
                e.EmployeeId != (excludeEmployeeId ?? 0));
        }

        public async Task CreateAsync(Employee employee)
        {
            employee.CreatedAt = DateTime.Now;
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Employee employee)
        {
            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int employeeId, int companyId)
        {
            var employee = await GetByIdAsync(employeeId, companyId);  // ownership check happens inside GetByIdAsync
            if (employee == null)
            {
                return false;
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Employee>> GetByIdsForCompanyAsync(List<int> employeeIds, int companyId)
        {
            // Only returns employees that both match the requested IDs AND belong to this company.
            return await _context.Employees
                .Where(e => employeeIds.Contains(e.EmployeeId) && e.CompanyId == companyId)
                .ToListAsync();
        }
    }
}
