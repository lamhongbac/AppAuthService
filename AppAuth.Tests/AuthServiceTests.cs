using AppAuth.BLL.Interfaces;
using AppAuth.BLL.Services;
using AuthService.DAL.Models;
using AuthService.DAL.Repos;
using AuthService.Shared.RequestResponse;
using Microsoft.Extensions.Configuration;
using Moq;

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace AppAuth.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IAppUserRepository> _userRepo = new();
        private readonly Mock<IApplicationRepository> _appRepo = new();
        private readonly Mock<IApiKeyService> _apiKeyService = new();
        private readonly Mock<IRefreshTokenRepository> _refTokenRepo = new();
        private readonly Mock<IConfiguration> _config = new();
        private readonly Mock<ISecurityAuditService> _securityAudit = new();
        private readonly Mock<IBlacklistService> _blacklist = new();
        private readonly Mock<IAuditLogService> _auditLog = new();
        private readonly Mock<IUserRoleRepository> _userRoleRepo = new();
        private readonly Mock<IAppRoleRepository> _roleRepo = new();
        private readonly Mock<IRoleRightRepository> _roleRightRepo = new();
        private readonly Mock<IAppObjectRepository> _appObjRepo = new();

        private readonly AppAuth.BLL.Services.AuthService _authService;

        public AuthServiceTests()
        {
            _authService = new AppAuth.BLL.Services.AuthService(
                _userRepo.Object,
                _userRoleRepo.Object,
                _roleRepo.Object,
                _roleRightRepo.Object,
                _appRepo.Object,
                _appObjRepo.Object,
                _apiKeyService.Object,
                _refTokenRepo.Object,
                _config.Object,
                _securityAudit.Object,
                _blacklist.Object,
                _auditLog.Object
            );
        }

        [Fact]
        public async Task AuthenticateAsync_InvalidApiKey_ReturnsFailure()
        {
            // Arrange
            _apiKeyService.Setup(s => s.ValidateApiKeyAsync(It.IsAny<string>()))
                .ReturnsAsync((Company)null);

            // Act
            var result = await _authService.AuthenticateAsync("invalid_key", new LoginRequest());

            // Assert
            Assert.False(result.OK);
            Assert.Equal("Invalid Api-Key", result.Message);
        }

        [Fact]
        public async Task AuthenticateAsync_BlacklistedUser_ReturnsFailure()
        {
            // Arrange
            _apiKeyService.Setup(s => s.ValidateApiKeyAsync(It.IsAny<string>()))
                .ReturnsAsync(new Company { Id = 1 });
            _blacklist.Setup(s => s.IsUserBlacklistedAsync("bad_user"))
                .ReturnsAsync(true);

            // Act
            var result = await _authService.AuthenticateAsync("key", new LoginRequest { UserName = "bad_user" });

            // Assert
            Assert.False(result.OK);
            Assert.Contains("blacklisted", result.Message);
        }

        [Fact]
        public async Task AuthenticateAsync_UserNotFound_ReturnsFailureAndTracksAttempt()
        {
            // Arrange
            _apiKeyService.Setup(s => s.ValidateApiKeyAsync(It.IsAny<string>()))
                .ReturnsAsync(new Company { Id = 1 });
            _blacklist.Setup(s => s.IsUserBlacklistedAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _userRepo.Setup(s => s.FindAsync(It.IsAny<Expression<System.Func<AppUser, bool>>>()))
                .ReturnsAsync(new List<AppUser>()); // User not found

            // Act
            var result = await _authService.AuthenticateAsync("key", new LoginRequest { UserName = "ghost" });

            // Assert
            Assert.False(result.OK);
            _securityAudit.Verify(s => s.TrackFailedAttemptAsync("ghost", "user"), Times.Once);
        }
    }
}
