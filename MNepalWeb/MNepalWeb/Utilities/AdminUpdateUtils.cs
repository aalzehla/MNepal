using MNepalWeb.Models;
using MNepalWeb.UserModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalWeb.Utilities
{
    public class AdminUpdateUtils
    {
        #region "Update Admin Password"

        /// <summary>
        /// Update Admin Password
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public static bool UpdateAdminPwd(UserInfo member)
        {
            var objMemberInfo = new UserInfo()
            {
                Name = member.Name,
                Address = member.Address,
                PIN = member.PIN,
                Password = member.Password,
                ContactNumber1 = member.ContactNumber1,
                ContactNumber2 = member.ContactNumber2,
                ClientCode = member.ClientCode,
                Mode = "UPWD" // Edit/Update Password
            };
            return new RegisterUserModel().UpdateMemberInfo(objMemberInfo) > 0;
        }

        #endregion
    }
}