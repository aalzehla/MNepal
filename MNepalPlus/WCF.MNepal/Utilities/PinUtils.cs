using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WCF.MNepal.Models;
using WCF.MNepal.UserModels;

namespace WCF.MNepal.Utilities
{
    public class PinUtils
    {
        public static DataTable GetPinInfo(string mobileNo)
        {
            var objUserModel = new PinUserModel();
            var objUserInfo = new PinChange()
            {
                mobile = mobileNo
            };
            return objUserModel.GetPinInformation(objUserInfo);
        }

        public static int ResetPinInfo(string mobileNo, string clientCode)
        {
            var objUserModel = new PinUserModel();
            var objUserInfo = new PinChange()
            {
                mobile = mobileNo,
                ClientCode = clientCode
            };
            return objUserModel.ResetPinInformation(objUserInfo);
        }

        public static string GeneratePin()
        {
            int _min = 1000;
            int _max = 9999;
            Random _rdm = new Random();
            return _rdm.Next(_min, _max).ToString();
        }
    }
}