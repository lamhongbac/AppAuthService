using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MSAUtility
{
    public static class SecurityUtility
    {
        private static readonly ConcurrentDictionary<string, (int count, DateTime lastAttempt)> _failedAttempts = new();
        private const int MaxFailedAttempts = 5;
        private const int LockoutMinutes = 15;

        // Sử dụng SHA512 cho hashing
        public static string HashToken(string plainText, string salt)
        {
            using (var sha512 = SHA512.Create())
            {
                var combinedBytes = Encoding.UTF8.GetBytes(plainText + salt);
                var hashBytes = sha512.ComputeHash(combinedBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        // Kiểm tra độ mạnh của mật khẩu
        public static (bool isValid, string errorMessage) IsPasswordStrong(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return (false, "Password must be at least 8 characters long.");

            if (!password.Any(char.IsUpper))
                return (false, "Password must contain at least one uppercase letter.");

            if (!password.Any(char.IsLower))
                return (false, "Password must contain at least one lowercase letter.");

            if (!password.Any(char.IsDigit))
                return (false, "Password must contain at least one digit.");

            if (!password.Any(c => !char.IsLetterOrDigit(c)))
                return (false, "Password must contain at least one special character.");

            return (true, string.Empty);
        }

        // Theo dõi số lần thử sai
        public static (bool shouldBlock, int currentCount) TrackFailedAttempt(string key)
        {
            var now = DateTime.UtcNow;
            var entry = _failedAttempts.AddOrUpdate(key, 
                (1, now), 
                (k, old) => {
                    // Nếu lần thử cuối cách đây quá LockoutMinutes, reset lại count
                    if ((now - old.lastAttempt).TotalMinutes > LockoutMinutes)
                        return (1, now);
                    return (old.count + 1, now);
                });

            return (entry.count >= MaxFailedAttempts, entry.count);
        }

        // Reset số lần thử sai (khi login thành công)
        public static void ResetFailedAttempts(string key)
        {
            _failedAttempts.TryRemove(key, out _);
        }

        // Tạo Salt ngẫu nhiên
        public static string GenerateSalt(int size = 32)
        {
            var buffer = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(buffer);
            }
            return Convert.ToBase64String(buffer);
        }
    }
}
