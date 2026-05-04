using AppAuth.BLL.Interfaces;
using AuthService.DAL.Models;
using AuthService.Shared;
using MSA.Shared.DataTypes;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AppAuthService.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        protected readonly IApiKeyService ApiKeyService;

        protected BaseController(IApiKeyService apiKeyService)
        {
            ApiKeyService = apiKeyService;
        }

        protected string? GetApiKeyHeader()
        {
            if (Request.Headers.TryGetValue("x-api-key", out var apiKey))
            {
                return apiKey;
            }
            return null;
        }

        protected async Task<(Company? company, BOProcessResult? error)> ValidateApiKey()
        {
            string? apiKey = GetApiKeyHeader();
            if (string.IsNullOrEmpty(apiKey))
            {
                return (null, BOProcessResult.Failure("Missing x-api-key in header", (int)AuthServiceErrorCode.InvalidToken));
            }

            var company = await ApiKeyService.ValidateApiKeyAsync(apiKey);
            if (company == null)
            {
                return (null, BOProcessResult.Failure("Invalid x-api-key", (int)AuthServiceErrorCode.InvalidToken));
            }

            return (company, null);
        }
    }
}
