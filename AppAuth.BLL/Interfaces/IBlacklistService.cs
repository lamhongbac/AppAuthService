using System.Threading.Tasks;

namespace AppAuth.BLL.Interfaces
{
    public interface IBlacklistService
    {
        Task<bool> IsIpBlacklistedAsync(string ip);
        Task<bool> IsUserBlacklistedAsync(string username);
        Task AddToBlacklistAsync(string type, string value, string reason);
        Task RemoveFromBlacklistAsync(string type, string value);
    }
}
