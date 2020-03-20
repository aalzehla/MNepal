using ThailiMerchantApp.Models;
using ThailiMerchantApp.UserModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ThailiMerchantApp.Utilities
{
    public static class PasswordUtils
    {
        #region "Member Change Password Utilities"

        public static bool UpdateUserPasswordInfo(this UserInfo member)
        {
            var objMemberInfo = new UserInfo()
            {
				OPassword = member.OPassword,
                Password = member.Password,
                ClientCode = member.ClientCode,
                Mode = "CPWD" // Change Password
            };
            return new PasswordUserModel().UpdateUserPasswordInfo(objMemberInfo) > 0;
        }

        #endregion

        #region "Member Forget Password Utilities"

        public static bool UpdateForgetPasswordInfo(this UserInfo member)
        {
            var objMemberInfo = new UserInfo()
            {
                UserName = member.UserName,
                Password = member.Password,
                Mode = "FPWD" // Forget Password
            };
            return new PasswordUserModel().ForgetUserPasswordInfo(objMemberInfo) > 0;
        }

        #endregion

        #region "Admin Member Change Password Utilities"

        public static bool UpdateAdminPasswordInfo(this UserInfo member)
        {
            var objMemberInfo = new UserInfo()
            {
                Password = member.Password,
                ClientCode = member.ClientCode,
                Mode = "APWD" // ADMIN PWD RESET
            };
            return new PasswordUserModel().UpdateAdminPasswordInfo(objMemberInfo) > 0;
        }


        public static bool UpdateAdminPasswordReset(this UserInfo member)
        {
            var objMemberInfo = new UserInfo()
            {
                Password = member.Password,
                ClientCode = member.ClientCode,
                Mode = "APR" // ADMIN PWD RESET
            };
            return new PasswordUserModel().UpdateAdminPasswordInfo(objMemberInfo) > 0;
        }
        #endregion

    }
}