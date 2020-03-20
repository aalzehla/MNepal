using MNSuperadmin.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MNSuperadmin.InterfaceServices
{
    public interface IUserDbService
    {
        DataTable GetUserInformation(UserInfo objUserInfo);
    }
}
