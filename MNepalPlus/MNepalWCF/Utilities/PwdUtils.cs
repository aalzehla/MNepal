using System.Data;
using MNepalWCF.Models;
using MNepalWCF.UserModels;

namespace MNepalWCF.Utilities
{
    public class PwdUtils
    {
        public static DataTable GetPwdInfo(string mobileNo)
        {
            var objUserModel = new PwdUserModel();
            var objUserInfo = new PwdChange()
            {
                mobile = mobileNo
            };
            return objUserModel.GetPwdInformation(objUserInfo);
        }
    }
}