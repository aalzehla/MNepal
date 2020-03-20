using System.Data;
using MNepalWCF.Models;
using MNepalWCF.UserModels;

namespace MNepalWCF.Utilities
{
    public class CheckerUtils
    {
        public static DataTable GetCheckUserNameInfo(string mobileNo)
        {
            var objUserModel = new CheckerUserModels();
            var objUserInfo = new UserInfo
            {
                UserName = mobileNo,
                Password = "",
                ClientCode = "",
                Mode = "GCUN"
            };
            return objUserModel.GetRegisterInformation(objUserInfo);
        }

        public static int RegisterUsersInfo(string mobileNo, string userType, string otpCode, string source)
        {
            var objMobileModel = new CheckerUserModels();
            var objMobileInfo = new UserInfo
            {
                UserName = mobileNo,
                UserType = userType,
                OTPCode = otpCode,
                Source = source,
                Mode = "ISR"
            };
            return objMobileModel.RegisterUsersInfo(objMobileInfo);
        }

        public static DataTable GetRegisterOTPInfo(string mobileNo, string otpCode)
        {
            var objUserModel = new CheckerUserModels();
            var objUserInfo = new UserInfo
            {
                UserName = mobileNo,
                OTPCode = otpCode,
                Mode = "SSR" //SELECT SELF REGISTRATION
            };
            return objUserModel.GetRegisterOTPInformation(objUserInfo);
        }
    }
}