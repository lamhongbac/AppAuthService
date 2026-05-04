using AuthService.Shared.RequestResponse;
using MSA.Shared.DataTypes;
using System.Threading.Tasks;

namespace AppAuth.BLL.Interfaces
{
    public interface IAuthService
    {
        Task<BOProcessResult> AuthenticateAsync(string apiKey, LoginRequest request);
        Task<BOProcessResult> RefreshTokenAsync(string apiKey, RefreshTokenRequest request);
        Task<BOProcessResult> SwitchAppAsync(string apiKey, string currentToken, SwitchAppRequest request);
        Task<bool> AuthorizeAsync(int userId, string appNumber, string objectNumber, string rightType);
    }
}
