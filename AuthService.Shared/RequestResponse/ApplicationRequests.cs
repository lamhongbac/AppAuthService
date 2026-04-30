using System.ComponentModel.DataAnnotations;

namespace AuthService.Shared.RequestResponse
{
    public class JwtConfigDTO
    {
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public int? Expire { get; set; }
        public int? RefExpire { get; set; }
    }

    public class ApplicationRegistrationRequest
    {
        [Required(ErrorMessage = "Application Name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Application Code (Number) is required")]
        public string Number { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public JwtConfigDTO? JwtConfig { get; set; }
    }

    public class AppObjectDTO
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Number { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public string ObjectType { get; set; } = "View"; // View, Process, Data
    }

    public class AddAppObjectsRequest
    {
        [Required]
        public string AppNumber { get; set; } = string.Empty;
        
        public List<AppObjectDTO> Objects { get; set; } = new List<AppObjectDTO>();
    }
}
