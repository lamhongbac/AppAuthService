using AppAuth.BLL.Interfaces;
using MSA.Shared;
using AuthService.Shared;
using AuthService.Shared.RequestResponse;
using AuthService.DAL.Repos;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;

namespace AppAuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;
        private readonly ICompanyRepository _companyRepository;

        public CompanyController(ICompanyService companyService, ICompanyRepository companyRepository)
        {
            _companyService = companyService;
            _companyRepository = companyRepository;
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

        [HttpGet]
        public async Task<ActionResult<BOProcessResult>> Get()
        {
            var companies = await _companyRepository.GetAllAsync();
            return Ok(BOProcessResult.Success(companies.ToList()));
        }
    }
}
