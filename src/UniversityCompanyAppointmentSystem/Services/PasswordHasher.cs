using System.Security.Cryptography;

namespace UniversityCompanyAppointmentSystem.Services
{
    // Simple but safe password hashing using PBKDF2 (built into .NET, no extra packages needed).
    // We never store the plain text password - only "salt:hash" is saved in the database.
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16;         // 16 bytes = 128-bit salt
        private const int KeySize = 32;          // 32 bytes = 256-bit derived key
        private const int Iterations = 100_000;  // number of PBKDF2 rounds, slows down brute force attempts

        public string Hash(string plainTextPassword)
        {
            // Generate a random salt so two identical passwords never produce the same hash.
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            // Derive a key (the actual hash) from the password + salt.
            byte[] key = Rfc2898DeriveBytes.Pbkdf2(plainTextPassword, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

            // Store salt and key together, separated by ':', so Verify() can rebuild the same hash later.
            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(key)}";
        }

        public bool Verify(string plainTextPassword, string storedHash)
        {
            string[] parts = storedHash.Split(':');   // split back into salt and key
            if (parts.Length != 2)
            {
                return false;                          // malformed hash, cannot verify
            }

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] expectedKey = Convert.FromBase64String(parts[1]);

            // Re-derive the key using the same salt and compare it to the stored key.
            byte[] actualKey = Rfc2898DeriveBytes.Pbkdf2(plainTextPassword, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

            // Fixed-time comparison avoids leaking timing information about the password.
            return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
        }
    }
}
