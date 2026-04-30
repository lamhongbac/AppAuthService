using AuthService.Shared.BusinessObjects;
using System;
using System.Collections.Generic;

namespace AuthService.Shared
{
    /// <summary>
    /// tra ve cho client cac thong tin can thiet
    /// </summary>
    public class UserData
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Mobile { get; set; }

        // tại 1 thời điểm login vào 1 app
        public string CurrentAppNumber { get; set; } = string.Empty;

        // Danh sách quyền đã được gộp (Merged) từ tất cả các role của user trong app này
        public List<ObjectRight> MergedRights { get; set; } = new List<ObjectRight>();
    }
}
