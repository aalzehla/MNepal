using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WCF.MNepal.Models;
using WCF.MNepal.UserModels;

namespace WCF.MNepal.Utilities
{
    public class CustomerUtils
    {
        public static void LogSMS(SMSLog Log)
        {
            CustActivityUserModel model = new CustActivityUserModel();
            Log.SentOn = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt ");
            model.InsertSMSLog(Log);
        }
    }
}