using System.ComponentModel.DataAnnotations;

namespace AuthService.Shared.RequestResponse
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Application Number is required")]
        public string AppNumber { get; set; } = string.Empty;
    }

    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "AccessToken is required")]
        public string AccessToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "RefreshToken is required")]
        public string RefreshToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "Application Number is required")]
        public string AppNumber { get; set; } = string.Empty;
    }

    public class SwitchAppRequest
    {
        [Required(ErrorMessage = "Target Application Number is required")]
        public string AppNumber { get; set; } = string.Empty;
    }

    public class BlacklistRequest
    {
        [Required(ErrorMessage = "Type is required (ip or user)")]
        public string Type { get; set; } = string.Empty;

        [Required(ErrorMessage = "Value is required")]
        public string Value { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;
    }
}
