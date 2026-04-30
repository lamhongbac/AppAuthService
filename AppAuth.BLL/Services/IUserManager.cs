using AuthService.Shared.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppAuth.BLL.Services
{
    public interface IUserManager
    {
        bool CreateUser(string username, string password);
        bool ValidateUser(string username, string password);
        User GetUser(string username);
        User GetUser(int userId);

    }
}
