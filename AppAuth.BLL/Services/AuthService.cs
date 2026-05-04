using AppAuth.BLL.Interfaces;
using AppAuth.Utils;
using AuthService.DAL.Repos;
using AuthService.Shared;
using AuthService.Shared.BusinessObjects;
using AuthService.Shared.RequestResponse;
using MSA.Shared.DataTypes;
using Microsoft.Extensions.Configuration;
using MSAUtility;

namespace AppAuth.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAppUserRepository _userRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IAppRoleRepository _roleRepository;
        private readonly IRoleRightRepository _roleRightRepository;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IAppObjectRepository _appObjectRepository;
        private readonly IApiKeyService _apiKeyService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IConfiguration _configuration;
        private readonly ISecurityAuditService _securityAuditService;
        private readonly IBlacklistService _blacklistService;
        private readonly IAuditLogService _auditLogService;

        public AuthService(
            IAppUserRepository userRepository,
            IUserRoleRepository userRoleRepository,
            IAppRoleRepository roleRepository,
            IRoleRightRepository roleRightRepository,
            IApplicationRepository applicationRepository,
            IAppObjectRepository appObjectRepository,
            IApiKeyService apiKeyService,
            IRefreshTokenRepository refreshTokenRepository,
            IConfiguration configuration,
            ISecurityAuditService securityAuditService,
            IBlacklistService blacklistService,
            IAuditLogService auditLogService)
        {
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
            _roleRightRepository = roleRightRepository;
            _applicationRepository = applicationRepository;
            _appObjectRepository = appObjectRepository;
            _apiKeyService = apiKeyService;
            _refreshTokenRepository = refreshTokenRepository;
            _configuration = configuration;
            _securityAuditService = securityAuditService;
            _blacklistService = blacklistService;
            _auditLogService = auditLogService;
        }

        public async Task<BOProcessResult> AuthenticateAsync(string apiKey, LoginRequest request)
        {
            try
            {
                // 1. Validate ApiKey
                var company = await _apiKeyService.ValidateApiKeyAsync(apiKey);
                if (company == null)
                {
                    return BOProcessResult.Failure("Invalid Api-Key", (int)AuthServiceErrorCode.InvalidToken);
                }

                // 2. Kiểm tra User Blacklist
                if (await _blacklistService.IsUserBlacklistedAsync(request.UserName))
                {
                    return BOProcessResult.Failure("Your account has been blacklisted due to security reasons.", (int)AuthServiceErrorCode.UserLocked);
                }

                // 3. Tìm User
                var users = await _userRepository.FindAsync(u => u.UserName == request.UserName);
                var user = users?.FirstOrDefault();
                if (user == null)
                {
                    // Track failed attempt by username
                    await _securityAuditService.TrackFailedAttemptAsync(request.UserName, "user");
                    
                    await _auditLogService.LogAsync(new AuditLogEntry {
                        UserName = request.UserName,
                        Action = "Login",
                        Details = "User not found",
                        IsSuccess = false
                    });

                    return BOProcessResult.Failure("Invalid username or password", (int)AuthServiceErrorCode.InvalidCredentials);
                }

                // 4. Kiểm tra Password
                string hashedPassword = SecurityUtility.HashToken(request.Password, user.PwdKey);
                if (hashedPassword != user.Pwd)
                {
                    // Track failed attempt
                    await _securityAuditService.TrackFailedAttemptAsync(request.UserName, "user");

                    await _auditLogService.LogAsync(new AuditLogEntry {
                        UserName = request.UserName,
                        Action = "Login",
                        Details = "Invalid password",
                        IsSuccess = false
                    });

                    return BOProcessResult.Failure("Invalid username or password", (int)AuthServiceErrorCode.InvalidCredentials);
                }

                // Login thành công -> Reset bộ đếm
                MSAUtility.SecurityUtility.ResetFailedAttempts(request.UserName);

                if (!user.IsActive)
                {
                    return BOProcessResult.Failure("User is locked", (int)AuthServiceErrorCode.UserLocked);
                }

                var apps = await _applicationRepository.FindAsync(a => a.CompanyId == company.Id && a.Number == request.AppNumber);
                var app = apps?.FirstOrDefault();
                if (app == null)
                {
                    return BOProcessResult.Failure("Application not found", (int)AuthServiceErrorCode.AppNotFound);
                }

                // 5. Lấy danh sách Roles của User trong App này
                var userRoles = await _userRoleRepository.FindAsync(ur => ur.UserId == user.Id);
                var userRoleIds = userRoles.Select(ur => ur.AppRoleId).ToList();
                
                var rolesInApp = await _roleRepository.FindAsync(r => r.AppId == app.Id && userRoleIds.Contains(r.Id));
                var roleIds = rolesInApp.Select(r => r.Id).ToList();

                // 6. Lấy và Gộp quyền (Merge Rights)
                var mergedRights = new Dictionary<int, ObjectRight>();
                
                foreach (var roleId in roleIds)
                {
                    var roleRights = await _roleRightRepository.FindAsync(rr => rr.RoleId == roleId);
                    foreach (var rr in roleRights)
                    {
                        if (!mergedRights.ContainsKey(rr.AppObjectId))
                        {
                            // Lấy thông tin AppObject để điền vào DTO
                            var appObjs = await _appObjectRepository.FindAsync(o => o.Id == rr.AppObjectId);
                            var obj = appObjs.FirstOrDefault();
                            
                            mergedRights[rr.AppObjectId] = new ObjectRight
                            {
                                ObjectId = rr.AppObjectId,
                                ObjectNumber = obj?.Number ?? "",
                                ObjectName = obj?.Name ?? "",
                                CanList = rr.CanList,
                                CanRead = rr.CanRead,
                                CanCreate = rr.CanCreate,
                                CanUpdate = rr.CanUpdate,
                                CanDelete = rr.CanDelete
                            };
                        }
                        else
                        {
                            var existing = mergedRights[rr.AppObjectId];
                            existing.CanList |= rr.CanList;
                            existing.CanRead |= rr.CanRead;
                            existing.CanCreate |= rr.CanCreate;
                            existing.CanUpdate |= rr.CanUpdate;
                            existing.CanDelete |= rr.CanDelete;
                        }
                    }
                }

                // 7. Tạo UserData
                var userData = new UserData
                {
                    Id = user.Id,
                    Username = user.UserName,
                    FullName = user.FullName,
                    Email = user.Email,
                    Mobile = user.Mobile,
                    CurrentAppNumber = app.Number,
                    MergedRights = mergedRights.Values.ToList()
                };

                // 8. Quyết định JwtConfig (Sử dụng App nếu valid, ngược lại dùng Company)
                var appConfig = new AppAuth.Utils.JwtConfig
                {
                    Key = app.AppKey ?? "",
                    Issuer = app.Issuer ?? "",
                    Audience = app.Audience ?? "",
                    Expire = app.Expire ?? 0,
                    RefExpire = app.RefExpire
                };

                var companyConfig = new AppAuth.Utils.JwtConfig
                {
                    Key = company.AppKey ?? "",
                    Issuer = company.Issuer ?? "",
                    Audience = company.Audience ?? "",
                    Expire = company.Expire ?? 0,
                    RefExpire = company.RefExpire ?? 0
                };

                // Binary Choice: Nếu App config hợp lệ thì dùng App, ngược lại dùng Company
                var finalConfig = appConfig.IsValid() ? appConfig : companyConfig;
                
                // Nếu cả hai đều không hợp lệ, fallback về appsettings để đảm bảo hệ thống không chết
                if (!finalConfig.IsValid())
                {
                    finalConfig.Issuer = string.IsNullOrEmpty(finalConfig.Issuer) ? (_configuration["JwtDefaults:Issuer"] ?? "AuthService") : finalConfig.Issuer;
                    finalConfig.Audience = string.IsNullOrEmpty(finalConfig.Audience) ? (_configuration["JwtDefaults:Audience"] ?? "MSA_Apps") : finalConfig.Audience;
                    finalConfig.Expire = finalConfig.Expire == 0 ? int.Parse(_configuration["JwtDefaults:Expire"] ?? "60") : finalConfig.Expire;
                    finalConfig.RefExpire = finalConfig.RefExpire == 0 ? int.Parse(_configuration["JwtDefaults:RefExpire"] ?? "43200") : finalConfig.RefExpire;
                    finalConfig.Key = string.IsNullOrEmpty(finalConfig.Key) ? Guid.NewGuid().ToString("N") : finalConfig.Key;
                }

                var refTokenData = new RefTokenData(_refreshTokenRepository);
                var jwtUtil = new JwtUtil(finalConfig, refTokenData);
                
                var jwtResult = await jwtUtil.GenerateJwt(userData);

                // 9. Trả về LoginResponse
                var response = new LoginResponse
                {
                    UserInfo = userData,
                    jwtInfo = jwtResult
                };

                // Audit Log: Success
                await _auditLogService.LogAsync(new AuditLogEntry
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    AppNumber = app.Number,
                    Action = "Login",
                    Details = "Login successful",
                    IsSuccess = true
                });

                return BOProcessResult.Success(response, "Login successful");
            }
            catch (Exception ex)
            {
                // Audit Log: Error
                await _auditLogService.LogAsync(new AuditLogEntry
                {
                    UserName = request.UserName,
                    AppNumber = request.AppNumber,
                    Action = "Login",
                    Details = $"Error: {ex.Message}",
                    IsSuccess = false
                });
                return BOProcessResult.Failure(ex.Message, (int)AuthServiceErrorCode.SystemError);
            }
        }

        public async Task<BOProcessResult> RefreshTokenAsync(string apiKey, RefreshTokenRequest request)
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

                // 3. Lấy JwtConfig và JwtUtil
                var jwtConfig = await GetJwtConfig(app, company);
                var refTokenData = new RefTokenData(_refreshTokenRepository);
                var jwtUtil = new JwtUtil(jwtConfig, refTokenData);

                // 4. Giải mã AccessToken (chấp nhận hết hạn)
                var principal = jwtUtil.GetPrincipalFromExpiredToken(request.AccessToken);
                if (principal == null)
                {
                    return BOProcessResult.Failure("Invalid Access Token", (int)AuthServiceErrorCode.InvalidToken);
                }

                var userName = principal.Claims.FirstOrDefault(c => c.Type == "UserName")?.Value;
                if (string.IsNullOrEmpty(userName))
                {
                    return BOProcessResult.Failure("Invalid token claims", (int)AuthServiceErrorCode.InvalidToken);
                }

                // 5. Kiểm tra Refresh Token trong DB
                string tokenHash = SecurityUtility.HashToken(request.RefreshToken, jwtConfig.Key);
                var tokens = await _refreshTokenRepository.FindAsync(t => t.TokenHash == tokenHash);
                var oldToken = tokens?.FirstOrDefault();

                if (oldToken == null)
                {
                    return BOProcessResult.Failure("Invalid refresh token", (int)AuthServiceErrorCode.InvalidToken);
                }

                // Dấu hiệu HACK: Sử dụng lại một Refresh Token đã từng được dùng (Token Reuse)
                // Đây là cơ chế Refresh Token Rotation. Nếu bị reuse, tức là token đã bị lộ.
                if (oldToken.IsUsed)
                {
                    await _securityAuditService.FlagCompromisedSessionAsync(oldToken.UserId, "Token Reuse Detected", "Client attempted to use a previously used refresh token.");
                    return BOProcessResult.Failure("Security violation: Token reuse detected. Your account has been forced to logout.", (int)AuthServiceErrorCode.InvalidToken);
                }

                if (oldToken.IsRevoked || oldToken.ExpiredAt < DateTime.UtcNow)
                {
                    return BOProcessResult.Failure("Expired or revoked refresh token", (int)AuthServiceErrorCode.InvalidToken);
                }

                // 6. Tìm User để lấy thông tin gộp quyền (merged rights)
                var users = await _userRepository.FindAsync(u => u.Id == oldToken.UserId);
                var user = users?.FirstOrDefault();
                if (user == null || !user.IsActive)
                {
                    return BOProcessResult.Failure("User not found or locked", (int)AuthServiceErrorCode.UserLocked);
                }

                // 7. Lấy Merged Rights (tương tự login)
                var mergedRights = await GetMergedRights(user.Id, app.Id);

                var userData = new UserData
                {
                    Id = user.Id,
                    Username = user.UserName,
                    FullName = user.FullName,
                    Email = user.Email,
                    Mobile = user.Mobile,
                    CurrentAppNumber = app.Number,
                    MergedRights = mergedRights
                };

                // 8. Sinh cặp token mới
                var jwtResult = await jwtUtil.GenerateJwt(userData);
                if (jwtResult == null)
                {
                    return BOProcessResult.Failure("Failed to generate token", (int)AuthServiceErrorCode.SystemError);
                }

                // 9. Cập nhật trạng thái token cũ
                oldToken.IsUsed = true;
                oldToken.ModifiedOn = DateTime.UtcNow;
                oldToken.ModifiedBy = "System";
                
                // Tìm ID của token mới vừa lưu (Dựa trên JwtId hoặc tìm token mới nhất của user)
                // Tuy nhiên, JwtUtil đã lưu vào DB rồi. 
                // Ta có thể cải tiến JwtUtil để trả về ID của record vừa tạo, hoặc tạm thời bỏ qua ReplacedByTokenId 
                // hoặc tìm record mới nhất.
                
                await _refreshTokenRepository.UpdateAsync(oldToken);

                var response = new LoginResponse
                {
                    UserInfo = userData,
                    jwtInfo = jwtResult
                };

                await _auditLogService.LogAsync(new AuditLogEntry {
                    UserId = user.Id,
                    UserName = user.UserName,
                    AppNumber = app.Number,
                    Action = "RefreshToken",
                    Details = "Token rotated successfully",
                    IsSuccess = true
                });

                return BOProcessResult.Success(response, "Token refreshed successfully");
            }
            catch (Exception ex)
            {
                await _auditLogService.LogAsync(new AuditLogEntry {
                    Action = "RefreshToken",
                    Details = $"Error: {ex.Message}",
                    IsSuccess = false
                });
                return BOProcessResult.Failure(ex.Message, (int)AuthServiceErrorCode.SystemError);
            }
        }

        public async Task<BOProcessResult> SwitchAppAsync(string apiKey, string currentToken, SwitchAppRequest request)
        {
            try
            {
                // 1. Validate ApiKey
                var company = await _apiKeyService.ValidateApiKeyAsync(apiKey);
                if (company == null)
                {
                    return BOProcessResult.Failure("Invalid Api-Key", (int)AuthServiceErrorCode.InvalidToken);
                }

                // 2. Parse token (unvalidated) để lấy App cũ
                var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(currentToken);
                var oldAppNumber = jwtToken.Claims.FirstOrDefault(c => c.Type == "AppNumber")?.Value;
                
                if (string.IsNullOrEmpty(oldAppNumber))
                {
                    return BOProcessResult.Failure("Invalid token: Missing AppNumber claim", (int)AuthServiceErrorCode.InvalidToken);
                }

                // 3. Lấy JwtConfig của App cũ để validate token
                var oldApps = await _applicationRepository.FindAsync(a => a.CompanyId == company.Id && a.Number == oldAppNumber);
                var oldApp = oldApps?.FirstOrDefault();
                if (oldApp == null)
                {
                    return BOProcessResult.Failure("Original application not found", (int)AuthServiceErrorCode.AppNotFound);
                }

                var oldJwtConfig = await GetJwtConfig(oldApp, company);
                var refTokenData = new RefTokenData(_refreshTokenRepository);
                var oldJwtUtil = new JwtUtil(oldJwtConfig, refTokenData);

                // 4. Validate token hiện tại (có thể chưa hết hạn)
                var principal = oldJwtUtil.GetPrincipalFromExpiredToken(currentToken); // Hàm này chấp nhận token còn hạn hoặc đã hết hạn
                if (principal == null)
                {
                    return BOProcessResult.Failure("Invalid Access Token", (int)AuthServiceErrorCode.InvalidToken);
                }

                var userName = principal.Claims.FirstOrDefault(c => c.Type == "UserName")?.Value;
                if (string.IsNullOrEmpty(userName))
                {
                    return BOProcessResult.Failure("Invalid token claims", (int)AuthServiceErrorCode.InvalidToken);
                }

                // 5. Tìm User
                var users = await _userRepository.FindAsync(u => u.UserName == userName);
                var user = users?.FirstOrDefault();
                if (user == null || !user.IsActive)
                {
                    return BOProcessResult.Failure("User not found or locked", (int)AuthServiceErrorCode.UserLocked);
                }

                // 6. Tìm Application đích
                var targetApps = await _applicationRepository.FindAsync(a => a.CompanyId == company.Id && a.Number == request.AppNumber);
                var targetApp = targetApps?.FirstOrDefault();
                if (targetApp == null)
                {
                    return BOProcessResult.Failure("Target application not found", (int)AuthServiceErrorCode.AppNotFound);
                }

                // 7. Lấy Merged Rights cho App đích
                var mergedRights = await GetMergedRights(user.Id, targetApp.Id);

                var userData = new UserData
                {
                    Id = user.Id,
                    Username = user.UserName,
                    FullName = user.FullName,
                    Email = user.Email,
                    Mobile = user.Mobile,
                    CurrentAppNumber = targetApp.Number,
                    MergedRights = mergedRights
                };

                // 8. Sinh JWT cho App đích
                var targetJwtConfig = await GetJwtConfig(targetApp, company);
                var targetJwtUtil = new JwtUtil(targetJwtConfig, refTokenData);

                var jwtResult = await targetJwtUtil.GenerateJwt(userData);
                if (jwtResult == null)
                {
                    return BOProcessResult.Failure("Failed to generate token", (int)AuthServiceErrorCode.SystemError);
                }

                var response = new LoginResponse
                {
                    UserInfo = userData,
                    jwtInfo = jwtResult
                };

                return BOProcessResult.Success(response, "App switched successfully");
            }
            catch (Exception ex)
            {
                return BOProcessResult.Failure(ex.Message, (int)AuthServiceErrorCode.SystemError);
            }
        }

        public async Task<bool> AuthorizeAsync(int userId, string appNumber, string objectNumber, string rightType)
        {
            try
            {
                // 1. Tìm Application
                var apps = await _applicationRepository.FindAsync(a => a.Number == appNumber);
                var app = apps?.FirstOrDefault();
                if (app == null) return false;

                // 2. Tìm AppObject trong Application này
                var objs = await _appObjectRepository.FindAsync(o => o.AppId == app.Id && o.Number == objectNumber);
                var obj = objs?.FirstOrDefault();
                if (obj == null) return false;

                // 3. Lấy tất cả RoleId mà User đang có
                var userRoles = await _userRoleRepository.FindAsync(ur => ur.UserId == userId);
                var roleIds = userRoles.Select(ur => ur.AppRoleId).ToList();

                // 4. Lọc các Role thuộc về Application này
                var rolesInApp = await _roleRepository.FindAsync(r => r.AppId == app.Id && roleIds.Contains(r.Id));
                var activeRoleIds = rolesInApp.Select(r => r.Id).ToList();

                if (!activeRoleIds.Any()) return false;

                // 5. Kiểm tra quyền trong bảng RoleRight cho tập hợp RoleIds này
                var rights = await _roleRightRepository.FindAsync(rr => 
                    activeRoleIds.Contains(rr.RoleId) && 
                    rr.AppObjectId == obj.Id);

                if (rights == null || !rights.Any()) return false;

                // 6. Kiểm tra loại quyền tương ứng (rightType: List, Read, Create, Update, Delete)
                return rightType.ToLower() switch
                {
                    "list" => rights.Any(r => r.CanList),
                    "read" => rights.Any(r => r.CanRead),
                    "create" => rights.Any(r => r.CanCreate),
                    "update" => rights.Any(r => r.CanUpdate),
                    "delete" => rights.Any(r => r.CanDelete),
                    _ => false
                };
            }
            catch
            {
                return false;
            }
        }

        private async Task<AppAuth.Utils.JwtConfig> GetJwtConfig(global::AuthService.DAL.Models.Application app, global::AuthService.DAL.Models.Company company)
        {
            var appConfig = new AppAuth.Utils.JwtConfig
            {
                Key = app.AppKey ?? "",
                Issuer = app.Issuer ?? "",
                Audience = app.Audience ?? "",
                Expire = app.Expire ?? 0,
                RefExpire = app.RefExpire
            };

            var companyConfig = new AppAuth.Utils.JwtConfig
            {
                Key = company.AppKey ?? "",
                Issuer = company.Issuer ?? "",
                Audience = company.Audience ?? "",
                Expire = company.Expire ?? 0,
                RefExpire = company.RefExpire ?? 0
            };

            var finalConfig = appConfig.IsValid() ? appConfig : companyConfig;
            if (!finalConfig.IsValid())
            {
                finalConfig.Issuer = string.IsNullOrEmpty(finalConfig.Issuer) ? (_configuration["JwtDefaults:Issuer"] ?? "AuthService") : finalConfig.Issuer;
                finalConfig.Audience = string.IsNullOrEmpty(finalConfig.Audience) ? (_configuration["JwtDefaults:Audience"] ?? "MSA_Apps") : finalConfig.Audience;
                finalConfig.Expire = finalConfig.Expire == 0 ? int.Parse(_configuration["JwtDefaults:Expire"] ?? "60") : finalConfig.Expire;
                finalConfig.RefExpire = finalConfig.RefExpire == 0 ? int.Parse(_configuration["JwtDefaults:RefExpire"] ?? "43200") : finalConfig.RefExpire;
                finalConfig.Key = string.IsNullOrEmpty(finalConfig.Key) ? Guid.NewGuid().ToString("N") : finalConfig.Key;
            }
            return finalConfig;
        }

        private async Task<List<ObjectRight>> GetMergedRights(int userId, int appId)
        {
            var userRoles = await _userRoleRepository.FindAsync(ur => ur.UserId == userId);
            var userRoleIds = userRoles.Select(ur => ur.AppRoleId).ToList();
            
            var rolesInApp = await _roleRepository.FindAsync(r => r.AppId == appId && userRoleIds.Contains(r.Id));
            var roleIds = rolesInApp.Select(r => r.Id).ToList();

            var mergedRights = new Dictionary<int, ObjectRight>();
            
            foreach (var roleId in roleIds)
            {
                var roleRights = await _roleRightRepository.FindAsync(rr => rr.RoleId == roleId);
                foreach (var rr in roleRights)
                {
                    if (!mergedRights.ContainsKey(rr.AppObjectId))
                    {
                        var appObjs = await _appObjectRepository.FindAsync(o => o.Id == rr.AppObjectId);
                        var obj = appObjs.FirstOrDefault();
                        
                        mergedRights[rr.AppObjectId] = new ObjectRight
                        {
                            ObjectId = rr.AppObjectId,
                            ObjectNumber = obj?.Number ?? "",
                            ObjectName = obj?.Name ?? "",
                            CanList = rr.CanList,
                            CanRead = rr.CanRead,
                            CanCreate = rr.CanCreate,
                            CanUpdate = rr.CanUpdate,
                            CanDelete = rr.CanDelete
                        };
                    }
                    else
                    {
                        var existing = mergedRights[rr.AppObjectId];
                        existing.CanList |= rr.CanList;
                        existing.CanRead |= rr.CanRead;
                        existing.CanCreate |= rr.CanCreate;
                        existing.CanUpdate |= rr.CanUpdate;
                        existing.CanDelete |= rr.CanDelete;
                    }
                }
            }
            return mergedRights.Values.ToList();
        }
    }
}
