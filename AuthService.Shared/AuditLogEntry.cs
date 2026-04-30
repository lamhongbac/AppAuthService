using System;

namespace AuthService.Shared
{
    public class AuditLogEntry
    {
        public int? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string AppNumber { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // Login, RefreshToken, SwitchApp, BlacklistAdd, etc.
        public string Details { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
