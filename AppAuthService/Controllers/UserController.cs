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
    public class UserController : BaseController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService, IApiKeyService apiKeyService)
            : base(apiKeyService)
        {
            _userService = userService;
        }

        [HttpPost("create")]
        public async Task<ActionResult<BOProcessResult>> CreateUser(CreateAppUserRequest request)
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

            var result = await _userService.CreateUserAsync(apiKey, request);
            
            if (result.OK)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost("assign-roles")]
        public async Task<ActionResult<BOProcessResult>> AssignRoles(AssignUserRolesRequest request)
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

            var result = await _userService.AssignRolesAsync(apiKey, request);
            
            if (result.OK)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}
