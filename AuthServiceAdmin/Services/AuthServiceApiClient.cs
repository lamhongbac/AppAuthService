using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AuthService.Shared.RequestResponse;
using MSA.Shared;

namespace AuthServiceAdmin.Services
{
    public class AuthServiceApiClient
    {
        private readonly HttpClient _http;

        public AuthServiceApiClient(HttpClient http, IConfiguration config)
        {
            _http = http;
            var baseUrl = config["AuthServiceApi"] ?? "http://localhost:5000/";
            if (!baseUrl.EndsWith("/")) baseUrl += "/";
            _http.BaseAddress = new Uri(baseUrl);
        }

        public async Task<BOProcessResult> GetCompaniesAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<BOProcessResult>("api/company");
                return response ?? BOProcessResult.Failure("Empty response");
            }
            catch (Exception ex)
            {
                return BOProcessResult.Failure(ex.Message);
            }
        }

        public async Task<BOProcessResult> RegisterCompanyAsync(CompanyRegistrationRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/company/register", request);
                return await response.Content.ReadFromJsonAsync<BOProcessResult>() ?? BOProcessResult.Failure("Empty response");
            }
            catch (Exception ex)
            {
                return BOProcessResult.Failure(ex.Message);
            }
        }

        public async Task<BOProcessResult> GetApplicationsAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<BOProcessResult>("api/application");
                return response ?? BOProcessResult.Failure("Empty response");
            }
            catch (Exception ex)
            {
                return BOProcessResult.Failure(ex.Message);
            }
        }

        public async Task<BOProcessResult> RegisterApplicationAsync(string apiKey, ApplicationRegistrationRequest request)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, "api/application/register");
                req.Headers.Add("x-api-key", apiKey);
                req.Content = JsonContent.Create(request);

                var response = await _http.SendAsync(req);
                return await response.Content.ReadFromJsonAsync<BOProcessResult>() ?? BOProcessResult.Failure("Empty response");
            }
            catch (Exception ex)
            {
                return BOProcessResult.Failure(ex.Message);
            }
        }

        public async Task<BOProcessResult> GetObjectsAsync(int appId)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<BOProcessResult>($"api/application/{appId}/objects");
                return response ?? BOProcessResult.Failure("Empty response");
            }
            catch (Exception ex)
            {
                return BOProcessResult.Failure(ex.Message);
            }
        }

        public async Task<BOProcessResult> AddObjectsAsync(string apiKey, AddAppObjectsRequest request)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, "api/application/objects");
                req.Headers.Add("x-api-key", apiKey);
                req.Content = JsonContent.Create(request);

                var response = await _http.SendAsync(req);
                return await response.Content.ReadFromJsonAsync<BOProcessResult>() ?? BOProcessResult.Failure("Empty response");
            }
            catch (Exception ex)
            {
                return BOProcessResult.Failure(ex.Message);
            }
        }

        public async Task<BOProcessResult> CreateRoleAsync(string apiKey, CreateAppRoleRequest request)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, "api/role/create");
                req.Headers.Add("x-api-key", apiKey);
                req.Content = JsonContent.Create(request);

                var response = await _http.SendAsync(req);
                return await response.Content.ReadFromJsonAsync<BOProcessResult>() ?? BOProcessResult.Failure("Empty response");
            }
            catch (Exception ex)
            {
                return BOProcessResult.Failure(ex.Message);
            }
        }

        public async Task<BOProcessResult> GetRolesByAppAsync(int appId)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<BOProcessResult>($"api/role/app/{appId}");
                return response ?? BOProcessResult.Failure("Empty response");
            }
            catch (Exception ex)
            {
                return BOProcessResult.Failure(ex.Message);
            }
        }

        public async Task<BOProcessResult> GetUsersAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<BOProcessResult>("api/user");
                return response ?? BOProcessResult.Failure("Empty response");
            }
            catch (Exception ex)
            {
                return BOProcessResult.Failure(ex.Message);
            }
        }

        public async Task<BOProcessResult> CreateUserAsync(string apiKey, CreateAppUserRequest request)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, "api/user/create");
                req.Headers.Add("x-api-key", apiKey);
                req.Content = JsonContent.Create(request);

                var response = await _http.SendAsync(req);
                return await response.Content.ReadFromJsonAsync<BOProcessResult>() ?? BOProcessResult.Failure("Empty response");
            }
            catch (Exception ex)
            {
                return BOProcessResult.Failure(ex.Message);
            }
        }

        public async Task<BOProcessResult> AssignRolesAsync(string apiKey, AssignUserRolesRequest request)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, "api/user/assign-roles");
                req.Headers.Add("x-api-key", apiKey);
                req.Content = JsonContent.Create(request);

                var response = await _http.SendAsync(req);
                return await response.Content.ReadFromJsonAsync<BOProcessResult>() ?? BOProcessResult.Failure("Empty response");
            }
            catch (Exception ex)
            {
                return BOProcessResult.Failure(ex.Message);
            }
        }
    }
}
