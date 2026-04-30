using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AuthService.Shared.RequestResponse
{
    public class RoleRightDTO
    {
        [Required]
        public string AppObjectNumber { get; set; } = string.Empty;
        public bool CanList { get; set; }
        public bool CanCreate { get; set; }
        public bool CanRead { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }
    }

    public class CreateAppRoleRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Number { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public string AppNumber { get; set; } = string.Empty;

        public List<RoleRightDTO> Rights { get; set; } = new List<RoleRightDTO>();
    }
}
