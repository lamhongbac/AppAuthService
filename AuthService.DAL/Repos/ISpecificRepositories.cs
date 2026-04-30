using AuthService.DAL.Models;

namespace AuthService.DAL.Repos
{
    public interface IApplicationRepository : IBaseRepository<Application> { }
    public interface IAppObjectRepository : IBaseRepository<AppObject> { }
    public interface IAppRoleRepository : IBaseRepository<AppRole> { }
    public interface IAppUserRepository : IBaseRepository<AppUser> { }
    public interface ICompanyRepository : IBaseRepository<Company> { }
    public interface IRefreshTokenRepository : IBaseRepository<RefTokenTracking> { }
    public interface IRoleRightRepository : IBaseRepository<RoleRight> { }
    public interface IUserRoleRepository : IBaseRepository<UserRole> { }
}
