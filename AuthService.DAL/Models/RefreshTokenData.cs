using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.DAL.Models
{
    public class RefreshTokenData
    {
        public Guid Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public string JwtId { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiredAt { get; set; }
        public bool IsRevoked { get; set; }
        public bool IsUsed { get; set; } = false;
        public int UserId { get; set; }
    }
}
