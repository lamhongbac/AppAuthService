using AppAuth.BLL.Interfaces;
using AuthService.DAL.Models;
using AuthService.DAL.Repos;
using AuthService.Shared;
using AuthService.Shared.RequestResponse;
using MSA.Shared.DataTypes;
using MSAUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppAuth.BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IAppUserRepository _userRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IApiKeyService _apiKeyService;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IAppRoleRepository _roleRepository;

        public UserService(
            IAppUserRepository userRepository,
            IUserRoleRepository userRoleRepository,
            IApiKeyService apiKeyService,
            IApplicationRepository applicationRepository,
            IAppRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _apiKeyService = apiKeyService;
            _applicationRepository = applicationRepository;
            _roleRepository = roleRepository;
        }

        public async Task<BOProcessResult> CreateUserAsync(string apiKey, CreateAppUserRequest request)
        {
            try
            {
                // 1. Validate ApiKey
                var company = await _apiKeyService.ValidateApiKeyAsync(apiKey);
                if (company == null)
                {
                    return BOProcessResult.Failure("Invalid Api-Key", (int)AuthServiceErrorCode.InvalidToken);
                }

                // 2. Kiểm tra trùng Username
                var existingUsers = await _userRepository.FindAsync(u => u.UserName == request.UserName);
                if (existingUsers != null && existingUsers.Any())
                {
                    return BOProcessResult.Failure("Username already exists", (int)AuthServiceErrorCode.DuplicateUsername);
                }

                // 3. Băm mật khẩu
                string salt = SecurityUtility.GenerateSalt();
                string hashedPassword = SecurityUtility.HashToken(request.Password, salt);

                // 4. Tạo AppUser
                var user = new AppUser
                {
                    UserName = request.UserName,
                    FullName = request.FullName,
                    Pwd = hashedPassword,
                    PwdKey = salt,
                    Email = request.Email,
                    Mobile = request.Mobile,
                    CardNum = request.CardNum,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBy = "System",
                    CreatedOn = DateTime.UtcNow,
                    ModifiedBy = "System",
                    ModifiedOn = DateTime.UtcNow
                };

                bool success = await _userRepository.AddAsync(user);
                if (success)
                {
                    return BOProcessResult.Success(new { UserId = user.Id, UserName = user.UserName }, "User created successfully");
                }

                return BOProcessResult.Failure("Failed to create user", (int)AuthServiceErrorCode.SystemError);
            }
            catch (Exception ex)
            {
                return BOProcessResult.Failure(ex.Message, (int)AuthServiceErrorCode.SystemError);
            }
        }

        public async Task<BOProcessResult> AssignRolesAsync(string apiKey, AssignUserRolesRequest request)
        {
            try
            {
                // 1. Validate ApiKey
                var company = await _apiKeyService.ValidateApiKeyAsync(apiKey);
                if (company == null)
                {
                    return BOProcessResult.Failure("Invalid Api-Key", (int)AuthServiceErrorCode.InvalidToken);
                }

                // 2. Tìm User
                var users = await _userRepository.FindAsync(u => u.UserName == request.UserName);
                var user = users?.FirstOrDefault();
                if (user == null)
                {
                    return BOProcessResult.Failure("User not found", (int)AuthServiceErrorCode.UserLocked); // Reuse or add UserNotFound
                }

                // 3. Tìm Application
                var apps = await _applicationRepository.FindAsync(a => a.CompanyId == company.Id && a.Number == request.AppNumber);
                var app = apps?.FirstOrDefault();
                if (app == null)
                {
                    return BOProcessResult.Failure("Application not found", (int)AuthServiceErrorCode.AppNotFound);
                }

                // 4. Xóa các quyền cũ của user trong App này (Sync roles)
                var rolesInApp = await _roleRepository.FindAsync(r => r.AppId == app.Id);
                var roleIdsInApp = rolesInApp.Select(r => r.Id).ToList();
                
                var existingUserRoles = await _userRoleRepository.FindAsync(ur => ur.UserId == user.Id && roleIdsInApp.Contains(ur.AppRoleId));
                foreach (var ur in existingUserRoles)
                {
                    await _userRoleRepository.DeleteAsync(ur);
                }

                // 5. Thêm các quyền mới
                int assignedCount = 0;
                foreach (var roleNumber in request.RoleNumbers)
                {
                    var roles = await _roleRepository.FindAsync(r => r.AppId == app.Id && r.Number == roleNumber);
                    var role = roles?.FirstOrDefault();
                    if (role == null) continue;

                    var userRole = new UserRole
                    {
                        UserId = user.Id,
                        AppRoleId = role.Id,
                        CreatedBy = "System",
                        CreatedOn = DateTime.UtcNow,
                        ModifiedBy = "System",
                        ModifiedOn = DateTime.UtcNow
                    };

                    await _userRoleRepository.AddAsync(userRole);
                    assignedCount++;
                }

                return BOProcessResult.Success(new { AssignedCount = assignedCount }, $"Successfully assigned {assignedCount} roles to user in application '{app.Number}'");
            }
            catch (Exception ex)
            {
                return BOProcessResult.Failure(ex.Message, (int)AuthServiceErrorCode.SystemError);
            }
        }
    }
}
