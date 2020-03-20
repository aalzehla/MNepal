using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNSuperadmin.Models;
using MNSuperadmin.UserModels;

namespace MNSuperadmin.Utilities
{
    public static class SAdminUtils
    {

        public static bool UpdateSuperAdminInfo(this UserInfo member)
        {
            var objMemberInfo = new UserInfo()
            {
             ClientCode = member.ClientCode,
             Name = member.Name,
             EmailAddress=member.EmailAddress

            };
            return new SAdminUserModel().SAdminUpdateUserInfo(objMemberInfo)> 0;
        }
    }
}