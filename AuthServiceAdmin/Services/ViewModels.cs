using System;

namespace AuthServiceAdmin.Services
{
    public class CompanyVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string RegKey { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
    }

    public class ApplicationVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public int? Expire { get; set; }
        public int? RefExpire { get; set; }
    }

    public class AppObjectVM
    {
        public int Id { get; set; }
        public int AppId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ObjectType { get; set; } = "View";
    }

    public class AppRoleVM
    {
        public int Id { get; set; }
        public int AppId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
    }

    public class AppUserVM
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Mobile { get; set; }
    }
}
