using AuthService.Shared;
using AuthService.Shared.BusinessObjects;
using AuthService.DAL.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MSAUtility;

namespace AppAuth.Utils
{
    /// <summary>
    /// JwtUtil su dung BLL để xử lý logic, đến phien BLL sẽ gọi DAL để xử lý dữ liệu
    /// </summary>
    public class JwtUtil
    {
        private readonly RefTokenData _tokenDatas;
        private readonly JwtConfig _config;
        
        public JwtUtil(JwtConfig config, RefTokenData tokenDatas)
        {
            _config = config;
            _tokenDatas = tokenDatas;
        }

        public async Task<JwtData> GenerateJSONWebToken(UserData userInfo)
        {
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            List<Claim> claims = new List<Claim>();

            if (!string.IsNullOrEmpty(userInfo.Email))
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Email, userInfo.Email));
                claims.Add(new Claim(JwtRegisteredClaimNames.Sub, userInfo.Email));
            }
            
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userInfo.Id.ToString()));
            claims.Add(new Claim("UserName", userInfo.Username));
            claims.Add(new Claim("AppNumber", userInfo.CurrentAppNumber));

            SecurityTokenDescriptor securityTokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_config.Expire),
                SigningCredentials = credentials,
                Issuer = _config.Issuer,
                Audience = _config.Audience,
            };

            var jwtoken = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);
            string jwt = jwtSecurityTokenHandler.WriteToken(jwtoken);
            string refreshToken = GenerateRefreshToken();
            
            // Hashing refresh token using shared SecurityUtility
            string tokenHash = SecurityUtility.HashToken(refreshToken, _config.Key);

            RefTokenTracking refTokenTracking = new RefTokenTracking()
            {
                Id = Guid.NewGuid(),
                TokenHash = tokenHash,
                IssuedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddMinutes(_config.RefExpire),
                IsRevoked = false,
                IsUsed = false,
                JwtId = jwtoken.Id,
                UserId = userInfo.Id,
                CreatedBy = "System",
                CreatedOn = DateTime.UtcNow,
                ModifiedBy = "System",
                ModifiedOn = DateTime.UtcNow
            };

            await _tokenDatas.AddToken(refTokenTracking);

            return new JwtData
            {
                Jwt = jwt,
                Rft = refreshToken
            };
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.Key)),
                ValidateLifetime = false, // Chấp nhận token đã hết hạn
                ValidIssuer = _config.Issuer,
                ValidAudience = _config.Audience
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            
            if (!(securityToken is JwtSecurityToken jwtSecurityToken) || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }

        public async Task<JwtData?> GenerateJwt(UserData userInfo)
        {
            if (userInfo == null)
                return null;
            return await GenerateJSONWebToken(userInfo);
        }
    }
}
