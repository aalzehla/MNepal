using System;
using MNepalWCF.Models;
using MNepalWCF.UserModels;

namespace MNepalWCF.Utilities
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