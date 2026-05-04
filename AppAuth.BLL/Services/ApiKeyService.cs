using AppAuth.BLL.Interfaces;
using AuthService.DAL.Models;
using AuthService.DAL.Repos;
using MSA.Shared.DataTypes;
using System.Linq;
using System.Threading.Tasks;

namespace AppAuth.BLL.Interfaces
{
    public interface IApiKeyService
    {
        Task<Company?> ValidateApiKeyAsync(string apiKey);
    }
}

namespace AppAuth.BLL.Services
{
    public class ApiKeyService : IApiKeyService
    {
        private readonly ICompanyRepository _companyRepository;

        public ApiKeyService(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public async Task<Company?> ValidateApiKeyAsync(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey)) return null;
            
            var companies = await _companyRepository.FindAsync(c => c.RegKey == apiKey);
            return companies?.FirstOrDefault();
        }
    }
}
