using MNepalWeb.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MNepalWeb.InterfaceServices
{
    public interface IUserDbService
    {
        DataTable GetUserInformation(UserInfo objUserInfo);
    }
}
