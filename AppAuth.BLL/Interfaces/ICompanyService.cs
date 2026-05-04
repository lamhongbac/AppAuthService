using MSA.Shared.DataTypes;
using AuthService.Shared.RequestResponse;
using System.Threading.Tasks;

namespace AppAuth.BLL.Interfaces
{
    public interface ICompanyService
    {
        Task<BOProcessResult> RegisterCompanyAsync(CompanyRegistrationRequest request);
    }
}
