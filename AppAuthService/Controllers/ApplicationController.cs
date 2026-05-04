using AppAuth.BLL.Interfaces;
using AuthService.Shared;
using AuthService.Shared.RequestResponse;
using AuthService.DAL.Repos;
using MSA.Shared.DataTypes;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;

namespace AppAuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationController : BaseController
    {
        private readonly IApplicationService _applicationService;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IAppObjectRepository _appObjectRepository;

        public ApplicationController(
            IApplicationService applicationService, 
            IApiKeyService apiKeyService,
            IApplicationRepository applicationRepository,
            IAppObjectRepository appObjectRepository) 
            : base(apiKeyService)
        {
            _applicationService = applicationService;
            _applicationRepository = applicationRepository;
            _appObjectRepository = appObjectRepository;
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

        [HttpGet]
        public async Task<ActionResult<BOProcessResult>> Get()
        {
            var apps = await _applicationRepository.GetAllAsync();
            return Ok(BOProcessResult.Success(apps.ToList()));
        }

        [HttpGet("{appId}/objects")]
        public async Task<ActionResult<BOProcessResult>> GetObjects(int appId)
        {
            var objs = await _appObjectRepository.FindAsync(o => o.AppId == appId);
            return Ok(BOProcessResult.Success(objs.ToList()));
        }
    }
}
