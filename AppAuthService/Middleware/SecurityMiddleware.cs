using AppAuth.BLL.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace AppAuthService.Middleware
{
    public class SecurityMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IBlacklistService blacklistService)
        {
            // 1. Kiểm tra IP Blacklist
            var remoteIp = context.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrEmpty(remoteIp))
            {
                if (await blacklistService.IsIpBlacklistedAsync(remoteIp))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Access denied: Your IP is blacklisted.");
                    return;
                }
            }

            // 2. Chuyển sang middleware tiếp theo
            await _next(context);
            
            // Lưu ý: Việc kiểm tra User Blacklist sẽ thực hiện sau khi Authentication Middleware chạy 
            // hoặc kiểm tra trực tiếp trong các logic nghiệp vụ (AuthService).
        }
    }
}
