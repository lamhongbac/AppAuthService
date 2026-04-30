using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppAuth.Utils
{
    public class JwtConfig
    {
        public string Key { get; set; } = string.Empty; //jwtKey
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int Expire { get; set; }
        public int RefExpire { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Key) && 
                   !string.IsNullOrEmpty(Issuer) && 
                   !string.IsNullOrEmpty(Audience) && 
                   Expire > 0 && 
                   RefExpire > 0;
        }
    }
}
