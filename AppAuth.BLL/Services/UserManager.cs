using AuthService.Shared.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppAuth.BLL.Services
{
    public class UserManager : IUserManager
    {
        public bool CreateUser(string username, string password)
        {
            throw new NotImplementedException();
        }

        public User GetUser(string username)
        {
            throw new NotImplementedException();
        }

        public User GetUser(int userId)
        {
            throw new NotImplementedException();
        }

        public bool ValidateUser(string username, string password)
        {
            throw new NotImplementedException();
        }
    }
}
