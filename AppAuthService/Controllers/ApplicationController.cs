using AppAuth.BLL.Interfaces;
using AuthService.Shared;
using AuthService.Shared.RequestResponse;
using MSA.Shared;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AppAuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationController : BaseController
    {
        private readonly IApplicationService _applicationService;

        public ApplicationController(IApplicationService applicationService, IApiKeyService apiKeyService) 
            : base(apiKeyService)
        {
            _applicationService = applicationService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<BOProcessResult>> Register(ApplicationRegistrationRequest request)
        {
            string? apiKey = GetApiKeyHeader();
            if (string.IsNullOrEmpty(apiKey))
            {
                return Unauthorized(BOProcessResult.Failure("Missing x-api-key in header", (int)AuthServiceErrorCode.InvalidToken));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(BOProcessResult.Failure("Invalid request data", (int)AuthServiceErrorCode.ValidationError));
            }

            var result = await _applicationService.RegisterApplicationAsync(apiKey, request);
            
            if (result.OK)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost("objects")]
        public async Task<ActionResult<BOProcessResult>> AddObjects(AddAppObjectsRequest request)
        {
            string? apiKey = GetApiKeyHeader();
            if (string.IsNullOrEmpty(apiKey))
            {
                return Unauthorized(BOProcessResult.Failure("Missing x-api-key in header", (int)AuthServiceErrorCode.InvalidToken));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(BOProcessResult.Failure("Invalid request data", (int)AuthServiceErrorCode.ValidationError));
            }

            var result = await _applicationService.AddAppObjectsAsync(apiKey, request);
            
            if (result.OK)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}
