using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WCF.MNepal.Models;
using WCF.MNepal.UserModels;

namespace WCF.MNepal.Utilities
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