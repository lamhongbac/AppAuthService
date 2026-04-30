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
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService, IApiKeyService apiKeyService)
            : base(apiKeyService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<BOProcessResult>> Login(LoginRequest request)
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

            var result = await _authService.AuthenticateAsync(apiKey, request);
            
            if (result.OK)
            {
                return Ok(result);
            }

            return Unauthorized(result);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<BOProcessResult>> RefreshToken(RefreshTokenRequest request)
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

            var result = await _authService.RefreshTokenAsync(apiKey, request);
            
            if (result.OK)
            {
                return Ok(result);
            }

            return Unauthorized(result);
        }
        [HttpPost("switch-app")]
        public async Task<ActionResult<BOProcessResult>> SwitchApp(SwitchAppRequest request)
        {
            string? apiKey = GetApiKeyHeader();
            if (string.IsNullOrEmpty(apiKey))
            {
                return Unauthorized(BOProcessResult.Failure("Missing x-api-key in header", (int)AuthServiceErrorCode.InvalidToken));
            }

            // Lấy current token từ Authorization header
            string? authHeader = Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(BOProcessResult.Failure("Missing or invalid Authorization header", (int)AuthServiceErrorCode.InvalidToken));
            }
            string currentToken = authHeader.Substring(7);

            if (!ModelState.IsValid)
            {
                return BadRequest(BOProcessResult.Failure("Invalid request data", (int)AuthServiceErrorCode.ValidationError));
            }

            var result = await _authService.SwitchAppAsync(apiKey, currentToken, request);
            
            if (result.OK)
            {
                return Ok(result);
            }

            return Unauthorized(result);
        }
    }
}
