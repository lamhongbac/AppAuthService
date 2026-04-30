using AuthService.Shared.RequestResponse;
using MSA.Shared;
using System.Threading.Tasks;

namespace AppAuth.BLL.Interfaces
{
    public interface IRoleService
    {
        Task<BOProcessResult> CreateRoleAsync(string apiKey, CreateAppRoleRequest request);
    }
}
