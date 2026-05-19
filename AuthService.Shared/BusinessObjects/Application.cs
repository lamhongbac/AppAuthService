using MSA.Shared.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Shared.BusinessObjects
{
    /// <summary>
    /// connect DAL thuc hien cac chuc nang 
    /// CRUD (create, read, update, delete) + Get tren database,
    /// cung cap cac phuong thuc de tu controller goi den DAL
    /// </summary>
    public class Application:BaseObject
    {
        public string ApiKey { get; set; } = string.Empty; // for 

        public string AppKey { get; set; } = string.Empty;

        public string Issuer { get; set; } = string.Empty;

        public string Audience { get; set; } = string.Empty;

        public int? Expire { get; set; }

        public int RefExpire { get; set; }

    }
}
