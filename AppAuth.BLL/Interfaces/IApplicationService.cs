using AuthService.Shared.RequestResponse;
using MSA.Shared.DataTypes;
using System.Threading.Tasks;

namespace AppAuth.BLL.Interfaces
{
    public interface IApplicationService
    {
        Task<BOProcessResult> RegisterApplicationAsync(string apiKey, ApplicationRegistrationRequest request);
        Task<BOProcessResult> AddAppObjectsAsync(string apiKey, AddAppObjectsRequest request);
    }
}
