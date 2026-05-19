using AppAuth.BLL.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AppAuth.BLL.Services
{
    public class BlacklistService : IBlacklistService
    {
        private readonly string _filePath;
        private static readonly object _lock = new object();

        public BlacklistService()
        {
            // Lưu file blacklist.json tại thư mục gốc của ứng dụng
            _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "blacklist.json");
            InitializeFile();
        }

        private void InitializeFile()
        {
            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, JsonSerializer.Serialize(new BlacklistData()));
            }
        }

#pragma warning disable CS1998
        public async Task<bool> IsIpBlacklistedAsync(string ip)
#pragma warning restore CS1998
        {
            var data = ReadDataAsync();
            return data.Ips.Any(x => x.Value == ip);
        }

#pragma warning disable CS1998
        public async Task<bool> IsUserBlacklistedAsync(string username)
#pragma warning restore CS1998
        {
            var data = ReadDataAsync();
            return data.Usernames.Any(x => x.Value == username);
        }

#pragma warning disable CS1998
        public async Task AddToBlacklistAsync(string type, string value, string reason)
#pragma warning restore CS1998
        {
            var data = ReadDataAsync();
            var entry = new BlacklistEntry { Value = value, Reason = reason, CreatedAt = DateTime.UtcNow };

            if (type.ToLower() == "ip")
            {
                if (!data.Ips.Any(x => x.Value == value)) data.Ips.Add(entry);
            }
            else
            {
                if (!data.Usernames.Any(x => x.Value == value)) data.Usernames.Add(entry);
            }

            SaveDataAsync(data);
        }

#pragma warning disable CS1998
        public async Task RemoveFromBlacklistAsync(string type, string value)
#pragma warning restore CS1998
        {
            var data = ReadDataAsync();
            if (type.ToLower() == "ip")
                data.Ips.RemoveAll(x => x.Value == value);
            else
                data.Usernames.RemoveAll(x => x.Value == value);

            SaveDataAsync(data);
        }

        private BlacklistData ReadDataAsync()
        {
            lock (_lock)
            {
                string json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<BlacklistData>(json) ?? new BlacklistData();
            }
        }

        private void SaveDataAsync(BlacklistData data)
        {
            lock (_lock)
            {
                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
        }
    }

    public class BlacklistData
    {
        public List<BlacklistEntry> Ips { get; set; } = new List<BlacklistEntry>();
        public List<BlacklistEntry> Usernames { get; set; } = new List<BlacklistEntry>();
    }

    public class BlacklistEntry
    {
        public string Value { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
