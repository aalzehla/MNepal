using System;
using System.Data;
using MNepalProject.Models;
using MNepalWCF.UserModels;
using MNepalWCF.Models;

namespace MNepalWCF.Utilities
{
    public class CustCheckUtils
    {
        #region Customer Checker

        public static DataTable GetCustUserInfo(string cmobile)
        {
            var objModel = new CustCheckerUserModel();
            var objCustUserInfo = new MNClientExt
            {
                UserName = cmobile
            };
            return objModel.GetCustUserCheckInfo(objCustUserInfo);
        }

        #endregion

        public static void LogSMS(SMSLog Log)
        {
            CustCheckerUserModel model = new CustCheckerUserModel();
            Log.SentOn = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt ");
            model.InsertSMSLog(Log);
        }

        #region Customer Checker

        public static DataTable GetCustStatusInfo(string cmobile)
        {
            var objModel = new CustCheckerUserModel();
            var objCustUserInfo = new MNClientExt
            {
                UserName = cmobile
            };
            return objModel.GetCustUserStatus(objCustUserInfo);
        }

        #endregion
    }
}