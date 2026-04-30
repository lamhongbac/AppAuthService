using System.ComponentModel.DataAnnotations;

namespace AuthService.Shared.RequestResponse
{
    public class CompanyRegistrationRequest
    {
        [Required(ErrorMessage = "Company Name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Company Code (Number) is required")]
        public string Number { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
