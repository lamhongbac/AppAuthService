using MSA.Shared;
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
        public string ApiKey { get; set; } // for 

        public string AppKey { get; set; }

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public int? Expire { get; set; }

        public int RefExpire { get; set; }

    }
}
