using System;
using System.Security.Cryptography;

namespace DroneRL_backend.Models
{
    public static class Roles
    {
        public const string Admin = "admin";
        public const string User = "user";
    }

    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string PasswordHash { get; set; }   // "соль:хеш" в Base64, сам пароль не хранится
        public string Role { get; set; }

        public bool IsAdmin => Role == Roles.Admin;

        public void SetPassword(string password)
        {
            PasswordHash = HashPassword(password);
        }

        public bool CheckPassword(string password)
        {
            var parts = (PasswordHash ?? "").Split(':');
            if (parts.Length != 2)
                return false;
            var salt = Convert.FromBase64String(parts[0]);
            var expected = Convert.FromBase64String(parts[1]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(password ?? "", salt, 100_000, HashAlgorithmName.SHA256, 32);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }

        public static string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(16);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
            return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        }
    }
}
