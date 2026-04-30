using AppAuth.BLL.Interfaces;
using AuthService.DAL.Repos;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AppAuth.BLL.Services
{
    public class SecurityAuditService : ISecurityAuditService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IBlacklistService _blacklistService;

        public SecurityAuditService(IRefreshTokenRepository refreshTokenRepository, IBlacklistService blacklistService)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _blacklistService = blacklistService;
        }

        public async Task FlagCompromisedSessionAsync(int userId, string reason, string details)
        {
            // 1. Log vào hệ thống cảnh báo (có thể dùng DB hoặc Serilog)
            Console.WriteLine($"[SECURITY ALERT] User {userId} compromised! Reason: {reason}. Details: {details}");

            // 2. Tự động thu hồi toàn bộ token của user này để bảo vệ tài khoản
            await RevokeAllUserTokensAsync(userId);

            // 3. (Tùy chọn) Tự động đưa user vào blacklist nếu mức độ nghiêm trọng cao
            // await _blacklistService.AddToBlacklistAsync("user", userId.ToString(), reason);
        }

        public async Task RevokeAllUserTokensAsync(int userId)
        {
            var activeTokens = await _refreshTokenRepository.FindAsync(t => t.UserId == userId && !t.IsRevoked && !t.IsUsed);
            
            foreach (var token in activeTokens)
            {
                token.IsRevoked = true;
                token.ModifiedOn = DateTime.UtcNow;
                token.ModifiedBy = "SecurityAuditSystem";
                await _refreshTokenRepository.UpdateAsync(token);
            }
        }

        public async Task<bool> DetectAnomaliesAsync(int userId, string currentIp, string userAgent)
        {
            // Placeholder: Kiểm tra IP blacklist hoặc thay đổi môi trường
            return await _blacklistService.IsIpBlacklistedAsync(currentIp);
        }

        public async Task TrackFailedAttemptAsync(string key, string type)
        {
            var (shouldBlock, count) = MSAUtility.SecurityUtility.TrackFailedAttempt(key);
            
            if (shouldBlock)
            {
                await _blacklistService.AddToBlacklistAsync(type, key, $"Automatic lockout: {count} failed attempts.");
                Console.WriteLine($"[SECURITY ALERT] {type} {key} has been blacklisted due to {count} failed attempts.");
            }
        }
    }
}
