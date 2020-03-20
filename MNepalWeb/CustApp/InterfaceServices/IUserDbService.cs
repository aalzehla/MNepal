using CustApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustApp.InterfaceServices
{
    public interface IUserDbService
    {
        DataTable GetUserInformation(UserInfo objUserInfo);
    }
}
