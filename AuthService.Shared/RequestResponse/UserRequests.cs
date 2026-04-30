using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AuthService.Shared.RequestResponse
{
    public class CreateAppUserRequest
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        public string? Email { get; set; }
        public string? Mobile { get; set; }
        public string? CardNum { get; set; }
    }

    public class AssignUserRolesRequest
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string AppNumber { get; set; } = string.Empty;

        [Required]
        public List<string> RoleNumbers { get; set; } = new List<string>();
    }
}
