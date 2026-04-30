using AppAuth.BLL.Interfaces;
using AppAuth.Utils;
using AuthService.DAL.Models;
using AuthService.DAL.Repos;
using MSA.Shared;
using AuthService.Shared;
using AuthService.Shared.RequestResponse;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace AppAuth.BLL.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IConfiguration _configuration;

        public CompanyService(ICompanyRepository companyRepository, IConfiguration configuration)
        {
            _companyRepository = companyRepository;
            _configuration = configuration;
        }

        public async Task<BOProcessResult> RegisterCompanyAsync(CompanyRegistrationRequest request)
        {
            try
            {
                // 1. Kiểm tra xem mã công ty đã tồn tại chưa
                var existing = await _companyRepository.FindAsync(c => c.Number == request.Number);
                if (existing != null && System.Linq.Enumerable.Any(existing))
                {
                    return BOProcessResult.Failure("Company with this code already exists", (int)AuthServiceErrorCode.DuplicateCompanyCode);
                }

                // 2. Tạo ApiKey (RegKey) dạng Guid-N
                string regKey = Guid.NewGuid().ToString("N");

                // 3. Chuẩn bị JwtConfig mặc định từ appsettings
                var config = new AppAuth.Utils.JwtConfig
                {
                    Key = Guid.NewGuid().ToString("N"),
                    Issuer = _configuration["JwtDefaults:Issuer"] ?? "AuthService",
                    Audience = _configuration["JwtDefaults:Audience"] ?? "MSA_Apps",
                    Expire = int.Parse(_configuration["JwtDefaults:Expire"] ?? "60"),
                    RefExpire = int.Parse(_configuration["JwtDefaults:RefExpire"] ?? "43200")
                };

                // Kiểm tra tính hợp lệ trước khi lưu
                if (!config.IsValid())
                {
                    return BOProcessResult.Failure("Invalid default JWT configuration in appsettings", (int)AuthServiceErrorCode.ValidationError);
                }

                // 4. Mapping sang Entity
                var company = new Company
                {
                    Name = request.Name,
                    Number = request.Number,
                    Description = request.Description,
                    RegKey = regKey,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBy = "System",
                    CreatedOn = DateTime.UtcNow,
                    ModifiedBy = "System",
                    ModifiedOn = DateTime.UtcNow,
                    
                    AppKey = config.Key, 
                    Issuer = config.Issuer,
                    Audience = config.Audience,
                    Expire = config.Expire,
                    RefExpire = config.RefExpire
                };

                // 5. Lưu vào DB
                bool success = await _companyRepository.AddAsync(company);
                if (success)
                {
                    return BOProcessResult.Success(new { RegKey = regKey, CompanyId = company.Id }, "Company registered successfully", (int)AuthServiceErrorCode.Success);
                }

                return BOProcessResult.Failure("Failed to register company", (int)AuthServiceErrorCode.RegistrationFailed);
            }
            catch (Exception ex)
            {
                return BOProcessResult.Failure(ex.Message, (int)AuthServiceErrorCode.SystemError);
            }
        }
    }
}
