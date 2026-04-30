using AuthService.DAL.Models;
using AuthService.DAL.Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppAuth.Utils
{
    public class RefTokenData
    {
        private static List<RefTokenTracking> _refTokens = new List<RefTokenTracking>();
        IRefreshTokenRepository _refreshTokenRepository;
        public RefTokenData(IRefreshTokenRepository refreshTokenRepository) {
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task AddToken(RefTokenTracking token)
        {
            _refTokens.Add(token);
            await _refreshTokenRepository.AddAsync(token);
        }

        
    }
}
