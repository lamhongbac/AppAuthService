using AppAuth.BLL.Interfaces;
using AuthService.DAL.Models;
using AuthService.DAL.Repos;
using AuthService.Shared;
using AuthService.Shared.RequestResponse;
using MSA.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppAuth.BLL.Services
{
    public class RoleService : IRoleService
    {
        private readonly IAppRoleRepository _roleRepository;
        private readonly IRoleRightRepository _roleRightRepository;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IAppObjectRepository _appObjectRepository;
        private readonly IApiKeyService _apiKeyService;

        public RoleService(
            IAppRoleRepository roleRepository,
            IRoleRightRepository roleRightRepository,
            IApplicationRepository applicationRepository,
            IAppObjectRepository appObjectRepository,
            IApiKeyService apiKeyService)
        {
            _roleRepository = roleRepository;
            _roleRightRepository = roleRightRepository;
            _applicationRepository = applicationRepository;
            _appObjectRepository = appObjectRepository;
            _apiKeyService = apiKeyService;
        }

        public async Task<BOProcessResult> CreateRoleAsync(string apiKey, CreateAppRoleRequest request)
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

                // 3. Kiểm tra trùng Role Number trong App
                var existingRoles = await _roleRepository.FindAsync(r => r.AppId == app.Id && r.Number == request.Number);
                if (existingRoles != null && existingRoles.Any())
                {
                    return BOProcessResult.Failure("Role code already exists in this application", (int)AuthServiceErrorCode.DuplicateAppCode); // Reuse code or add new
                }

                // 4. Tạo AppRole
                var role = new AppRole
                {
                    AppId = app.Id,
                    Name = request.Name,
                    Number = request.Number,
                    Description = request.Description,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBy = "System",
                    CreatedOn = DateTime.UtcNow,
                    ModifiedBy = "System",
                    ModifiedOn = DateTime.UtcNow
                };

                bool roleSuccess = await _roleRepository.AddAsync(role);
                if (!roleSuccess)
                {
                    return BOProcessResult.Failure("Failed to create role", (int)AuthServiceErrorCode.SystemError);
                }

                // 5. Thêm các quyền (RoleRights)
                int rightsCount = 0;
                foreach (var rightDto in request.Rights)
                {
                    // Tìm AppObject: Bắt buộc phải thuộc đúng Application của Role
                    var objects = await _appObjectRepository.FindAsync(o => o.AppId == app.Id && o.Number == rightDto.AppObjectNumber);
                    var appObj = objects?.FirstOrDefault();
                    
                    if (appObj == null)
                    {
                        // Nếu object không thuộc app này, ta có thể trả về lỗi hoặc bỏ qua. 
                        // Ở đây tôi chọn trả về lỗi để đảm bảo tính chính xác của dữ liệu gửi lên.
                        return BOProcessResult.Failure($"Object '{rightDto.AppObjectNumber}' does not belong to Application '{app.Number}'", (int)AuthServiceErrorCode.ValidationError);
                    }

                    var roleRight = new RoleRight
                    {
                        RoleId = role.Id,
                        AppObjectId = appObj.Id,
                        CanList = rightDto.CanList,
                        CanCreate = rightDto.CanCreate,
                        CanRead = rightDto.CanRead,
                        CanUpdate = rightDto.CanUpdate,
                        CanDelete = rightDto.CanDelete,
                        CreatedBy = "System",
                        CreatedOn = DateTime.UtcNow,
                        ModifiedBy = "System",
                        ModifiedOn = DateTime.UtcNow
                    };

                    await _roleRightRepository.AddAsync(roleRight);
                    rightsCount++;
                }

                return BOProcessResult.Success(new { RoleId = role.Id, RightsCount = rightsCount }, "Role created successfully with rights");
            }
            catch (Exception ex)
            {
                return BOProcessResult.Failure(ex.Message, (int)AuthServiceErrorCode.SystemError);
            }
        }
    }
}
