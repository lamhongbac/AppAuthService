using System.Threading.Tasks;
using MSA.Shared.DataTypes;

namespace AppAuth.BLL.Interfaces
{
    public interface ISecurityAuditService
    {
        /// <summary>
        /// Ghi nhận dấu hiệu bất thường và quyết định có cần thu hồi token hay không
        /// </summary>
        Task FlagCompromisedSessionAsync(int userId, string reason, string details);

        /// <summary>
        /// Thu hồi tất cả Refresh Token của một user (Force Logout)
        /// </summary>
        Task RevokeAllUserTokensAsync(int userId);

        /// <summary>
        /// Kiểm tra xem một yêu cầu có dấu hiệu bất thường (IP lạ, UserAgent thay đổi đột ngột...)
        /// </summary>
        Task<bool> DetectAnomaliesAsync(int userId, string currentIp, string userAgent);
        /// <summary>
        /// Theo dõi số lần thử sai để kích hoạt Rate Limiting
        /// </summary>
        Task TrackFailedAttemptAsync(string key, string type);
    }
}
