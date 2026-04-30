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
    public class RoleController : BaseController
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService, IApiKeyService apiKeyService)
            : base(apiKeyService)
        {
            _roleService = roleService;
        }

        [HttpPost("create")]
        public async Task<ActionResult<BOProcessResult>> CreateRole(CreateAppRoleRequest request)
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

            var result = await _roleService.CreateRoleAsync(apiKey, request);
            
            if (result.OK)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}
