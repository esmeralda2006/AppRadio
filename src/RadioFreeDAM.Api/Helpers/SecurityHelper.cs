using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace RadioFreeDAM.Api.Helpers;

public static class SecurityHelper
{
    private const int Iterations = 600000;
    private const int SaltSize = 128 / 8; // 16 bytes

    public static string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password)) 
            return string.Empty;

        // Generate a 128-bit salt using a sequence of cryptographically strong random bytes.
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        // derive a 256-bit subkey (use HMACSHA256 with 10,000 iterations)
        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: Iterations,
            numBytesRequested: 256 / 8));

        return $"{Convert.ToBase64String(salt)}.{hashed}";
    }

    public static bool VerifyPassword(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(storedHash)) return false;

        try
        {
            // New PBKDF2 format: "salt.hash"
            if (storedHash.Contains("."))
            {
                var parts = storedHash.Split('.');
                if (parts.Length != 2) return false;

                byte[] salt = Convert.FromBase64String(parts[0]);
                string hashed = parts[1];

                string hashedInput = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: Iterations,
                    numBytesRequested: 256 / 8));

                return hashed == hashedInput;
            }

            // Legacy fallback (assume SHA256)
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            var hash = BitConverter.ToString(bytes).Replace("-", "").ToLower();
            return hash == storedHash.ToLower();
        }
        catch
        {
            return false;
        }
    }
}
