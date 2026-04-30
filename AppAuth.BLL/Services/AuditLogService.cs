using AppAuth.BLL.Interfaces;
using AuthService.Shared;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace AppAuth.BLL.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly string _logFilePath;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly object _lock = new object();

        public AuditLogService(IHttpContextAccessor httpContextAccessor)
        {
            _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "audit_logs.jsonl");
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(AuditLogEntry entry)
        {
            try
            {
                entry.Timestamp = DateTime.UtcNow;
                
                // Tự động lấy IP nếu chưa có
                if (string.IsNullOrEmpty(entry.IpAddress))
                {
                    entry.IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                }

                string logLine = JsonSerializer.Serialize(entry);

                // Ghi ra Console
                string status = entry.IsSuccess ? "SUCCESS" : "FAILED";
                Console.WriteLine($"[AUDIT] {entry.Timestamp:yyyy-MM-dd HH:mm:ss} | {status} | User: {entry.UserName} | Action: {entry.Action} | IP: {entry.IpAddress}");

                // Ghi ra file
                lock (_lock)
                {
                    File.AppendAllLines(_logFilePath, new[] { logLine });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to write audit log: {ex.Message}");
            }
            await Task.CompletedTask;
        }
    }
}
