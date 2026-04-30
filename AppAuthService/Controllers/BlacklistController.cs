using AppAuth.BLL.Interfaces;
using AuthService.Shared.RequestResponse;
using MSA.Shared;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AppAuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlacklistController : BaseController
    {
        private readonly IBlacklistService _blacklistService;

        public BlacklistController(IBlacklistService blacklistService, IApiKeyService apiKeyService)
            : base(apiKeyService)
        {
            _blacklistService = blacklistService;
        }

        [HttpPost("add")]
        public async Task<ActionResult<BOProcessResult>> Add(BlacklistRequest request)
        {
            var (company, error) = await ValidateApiKey();
            if (error != null) return Unauthorized(error);

            if (string.IsNullOrEmpty(request.Type) || string.IsNullOrEmpty(request.Value))
            {
                return BadRequest(BOProcessResult.Failure("Type and Value are required"));
            }

            await _blacklistService.AddToBlacklistAsync(request.Type, request.Value, request.Reason);
            return Ok(BOProcessResult.Success(null, $"Successfully added {request.Value} to {request.Type} blacklist."));
        }

        [HttpDelete("remove")]
        public async Task<ActionResult<BOProcessResult>> Remove(string type, string value)
        {
            var (company, error) = await ValidateApiKey();
            if (error != null) return Unauthorized(error);

            await _blacklistService.RemoveFromBlacklistAsync(type, value);
            return Ok(BOProcessResult.Success(null, $"Successfully removed {value} from {type} blacklist."));
        }
    }
}
