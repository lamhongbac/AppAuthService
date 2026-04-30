using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Shared.DataType
{
    /// <summary>
    /// cac constant string to identify the data types of tokens, such as jwt and rft
    /// </summary>
    [Serializable]
    public class DataTypes
    {
        public const string Jwt = "jwt";
        public const string Rft = "rft";
        
    }
}
