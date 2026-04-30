using AuthService.DAL.Models;

namespace AuthService.DAL.Repos
{
    public class ApplicationRepository : BaseRepository<Application>, IApplicationRepository
    {
        public ApplicationRepository(AppAuthServiceContext context) : base(context) { }
    }

    public class AppObjectRepository : BaseRepository<AppObject>, IAppObjectRepository
    {
        public AppObjectRepository(AppAuthServiceContext context) : base(context) { }
    }

    public class AppRoleRepository : BaseRepository<AppRole>, IAppRoleRepository
    {
        public AppRoleRepository(AppAuthServiceContext context) : base(context) { }
    }

    public class AppUserRepository : BaseRepository<AppUser>, IAppUserRepository
    {
        public AppUserRepository(AppAuthServiceContext context) : base(context) { }
    }

    public class CompanyRepository : BaseRepository<Company>, ICompanyRepository
    {
        public CompanyRepository(AppAuthServiceContext context) : base(context) { }
    }

    public class RefreshTokenRepository : BaseRepository<RefTokenTracking>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(AppAuthServiceContext context) : base(context) { }
    }

    public class RoleRightRepository : BaseRepository<RoleRight>, IRoleRightRepository
    {
        public RoleRightRepository(AppAuthServiceContext context) : base(context) { }
    }

    public class UserRoleRepository : BaseRepository<UserRole>, IUserRoleRepository
    {
        public UserRoleRepository(AppAuthServiceContext context) : base(context) { }
    }
}
