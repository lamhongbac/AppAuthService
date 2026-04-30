using AppAuth.BLL.Interfaces;
using MSA.Shared;
using AuthService.Shared;
using AuthService.Shared.RequestResponse;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AppAuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<BOProcessResult>> Register(CompanyRegistrationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(BOProcessResult.Failure("Invalid request data", (int)AuthServiceErrorCode.ValidationError));
            }

            var result = await _companyService.RegisterCompanyAsync(request);
            
            if (result.OK)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}
