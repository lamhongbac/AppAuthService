using AuthService.Shared.RequestResponse;
using MSA.Shared;
using System.Threading.Tasks;

namespace AppAuth.BLL.Interfaces
{
    public interface IUserService
    {
        Task<BOProcessResult> CreateUserAsync(string apiKey, CreateAppUserRequest request);
        Task<BOProcessResult> AssignRolesAsync(string apiKey, AssignUserRolesRequest request);
    }
}
