using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WCF.MNepal.Models;
using WCF.MNepal.UserModels;

namespace WCF.MNepal.Utilities
{
    public class ResetPinUtils
    {
        public static DataTable GetExistUserName(string clientCode, string mobile)
        {
            var objUserModel = new ResetPin();
            var objUserInfo = new PinChange
            {
                ClientCode = clientCode,
                mobile = mobile,
                Mode = "CUFRP"
            };
            return objUserModel.GetUserName(objUserInfo);
        }
    }
}