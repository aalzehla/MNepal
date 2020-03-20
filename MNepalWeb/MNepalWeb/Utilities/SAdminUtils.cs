using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalWeb.Models;
using MNepalWeb.UserModels;

namespace MNepalWeb.Utilities
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