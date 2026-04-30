using AppAuth.BLL.Interfaces;
using AppAuth.Utils;
using AuthService.DAL.Models;
using AuthService.DAL.Repos;
using AuthService.Shared;
using AuthService.Shared.RequestResponse;
using MSA.Shared;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppAuth.BLL.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IAppObjectRepository _appObjectRepository;
        private readonly IApiKeyService _apiKeyService;
        private readonly IConfiguration _configuration;

        public ApplicationService(
            IApplicationRepository applicationRepository,
            IAppObjectRepository appObjectRepository,
            IApiKeyService apiKeyService,
            IConfiguration configuration)
        {
            _applicationRepository = applicationRepository;
            _appObjectRepository = appObjectRepository;
            _apiKeyService = apiKeyService;
            _configuration = configuration;
        }

        public async Task<BOProcessResult> RegisterApplicationAsync(string apiKey, ApplicationRegistrationRequest request)
        {
            try
            {
                // 1. Validate ApiKey
                var company = await _apiKeyService.ValidateApiKeyAsync(apiKey);
                if (company == null)
                {
                    return BOProcessResult.Failure("Invalid Api-Key", (int)AuthServiceErrorCode.InvalidToken);
                }

                // 2. Kiểm tra trùng AppCode trong cùng công ty
                var existing = await _applicationRepository.FindAsync(a => a.CompanyId == company.Id && a.Number == request.Number);
                if (existing != null && existing.Any())
                {
                    return BOProcessResult.Failure("Application code already exists in this company", (int)AuthServiceErrorCode.DuplicateAppCode);
                }

                // 3. Quyết định JwtConfig
                var appConfig = new AppAuth.Utils.JwtConfig
                {
                    Key = Guid.NewGuid().ToString("N"),
                    Issuer = request.JwtConfig?.Issuer ?? "",
                    Audience = request.JwtConfig?.Audience ?? "",
                    Expire = request.JwtConfig?.Expire ?? 0,
                    RefExpire = request.JwtConfig?.RefExpire ?? 0
                };

                // 4. Tạo Application
                var application = new Application
                {
                    Name = request.Name,
                    Number = request.Number,
                    Description = request.Description,
                    CompanyId = company.Id,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBy = "System",
                    CreatedOn = DateTime.UtcNow,
                    ModifiedBy = "System",
                    ModifiedOn = DateTime.UtcNow
                };

                // Nếu JwtConfig gửi lên hợp lệ, ta mới lưu vào Application
                // Nếu không, các trường JWT trong Application sẽ để mặc định (null/0) để hệ thống tự kế thừa từ Company khi Login
                if (appConfig.IsValid())
                {
                    application.AppKey = appConfig.Key;
                    application.Issuer = appConfig.Issuer;
                    application.Audience = appConfig.Audience;
                    application.Expire = appConfig.Expire;
                    application.RefExpire = appConfig.RefExpire;
                }
                else
                {
                    // Đảm bảo các trường bắt buộc vẫn có giá trị nếu cần, 
                    // nhưng ở đây ta muốn null để đánh dấu là dùng của Company
                    application.AppKey = ""; 
                    application.Issuer = null;
                    application.Audience = null;
                    application.Expire = null;
                    // RefExpire trong DB là NOT NULL nên ta để 0
                    application.RefExpire = 0; 
                }

                bool success = await _applicationRepository.AddAsync(application);
                if (success)
                {
                    return BOProcessResult.Success(new { AppKey = application.AppKey, AppId = application.Id }, "Application registered successfully");
                }

                return BOProcessResult.Failure("Failed to register application", (int)AuthServiceErrorCode.RegistrationFailed);
            }
            catch (Exception ex)
            {
                return BOProcessResult.Failure(ex.Message, (int)AuthServiceErrorCode.SystemError);
            }
        }

        public async Task<BOProcessResult> AddAppObjectsAsync(string apiKey, AddAppObjectsRequest request)
        {
            try
            {
                // 1. Validate ApiKey
                var company = await _apiKeyService.ValidateApiKeyAsync(apiKey);
                if (company == null)
                {
                    return BOProcessResult.Failure("Invalid Api-Key", (int)AuthServiceErrorCode.InvalidToken);
                }

                // 2. Tìm Application
                var apps = await _applicationRepository.FindAsync(a => a.CompanyId == company.Id && a.Number == request.AppNumber);
                var app = apps?.FirstOrDefault();
                if (app == null)
                {
                    return BOProcessResult.Failure("Application not found", (int)AuthServiceErrorCode.AppNotFound);
                }

                // 3. Thêm danh sách AppObjects
                int count = 0;
                foreach (var objDto in request.Objects)
                {
                    // Kiểm tra xem object đã tồn tại chưa (trong cùng app)
                    var existingObjs = await _appObjectRepository.FindAsync(o => o.AppId == app.Id && o.Number == objDto.Number);
                    if (existingObjs != null && existingObjs.Any()) continue;

                    var appObj = new AppObject
                    {
                        AppId = app.Id,
                        Name = objDto.Name,
                        Number = objDto.Number,
                        Description = objDto.Description,
                        ObjectType = objDto.ObjectType,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = "System",
                        CreatedOn = DateTime.UtcNow,
                        ModifiedBy = "System",
                        ModifiedOn = DateTime.UtcNow
                    };
                    
                    await _appObjectRepository.AddAsync(appObj);
                    count++;
                }

                return BOProcessResult.Success(new { AddedCount = count }, $"Successfully added {count} application objects");
            }
            catch (Exception ex)
            {
                return BOProcessResult.Failure(ex.Message, (int)AuthServiceErrorCode.SystemError);
            }
        }
    }
}
