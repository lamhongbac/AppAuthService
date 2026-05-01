using AppAuth.BLL.Interfaces;
using AuthService.Shared;
using AuthService.Shared.RequestResponse;
using AuthService.DAL.Repos;
using MSA.Shared;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;

namespace AppAuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : BaseController
    {
        private readonly IRoleService _roleService;
        private readonly IAppRoleRepository _roleRepository;

        public RoleController(IRoleService roleService, IApiKeyService apiKeyService, IAppRoleRepository roleRepository)
            : base(apiKeyService)
        {
            _roleService = roleService;
            _roleRepository = roleRepository;
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

        [HttpGet("app/{appId}")]
        public async Task<ActionResult<BOProcessResult>> GetByApp(int appId)
        {
            var roles = await _roleRepository.FindAsync(r => r.AppId == appId);
            return Ok(BOProcessResult.Success(roles.ToList()));
        }
    }
}
