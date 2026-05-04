using MSA.Shared.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Shared
{
    /// <summary>
    /// 
    /// </summary>
    public class LoginResponse
    {
        public LoginResponse()
        {
            
        }
        public UserData UserInfo { get; set; }
        public JwtData jwtInfo { get; set; }
    }
}
