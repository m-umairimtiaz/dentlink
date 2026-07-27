namespace UniversityCompanyAppointmentSystem.Services
{
    // Small abstraction so controllers never touch raw cryptography code directly.
    public interface IPasswordHasher
    {
        string Hash(string plainTextPassword);                        // turns a plain password into a stored hash
        bool Verify(string plainTextPassword, string storedHash);      // checks a login attempt against the stored hash
    }
}
