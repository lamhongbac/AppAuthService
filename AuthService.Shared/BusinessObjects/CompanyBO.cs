using System;

namespace AuthService.Shared.BusinessObjects
{
    public class CompanyBO
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public string RegKey { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        // JWT Config defaults for Company level if needed
        public string AppKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int Expire { get; set; } = 60; // Minutes
        public int RefExpire { get; set; } = 43200; // 30 Days in minutes
    }
}
