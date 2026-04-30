using System.Threading.Tasks;
using AuthService.Shared;

namespace AppAuth.BLL.Interfaces
{
    public interface IAuditLogService
    {
        Task LogAsync(AuditLogEntry entry);
    }
}
