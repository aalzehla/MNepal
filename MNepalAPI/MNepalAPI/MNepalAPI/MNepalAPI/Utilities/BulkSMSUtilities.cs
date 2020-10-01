using MNepalAPI.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static MNepalAPI.Models.Notifications;

namespace MNepalAPI.Utilities
{
    public class BulkSMSUtilities
    {
        public static int BulkSMS(string mobileNumber, string message, DateTime dateTime)
        {
            var objresPaypointBulkSMSModel = new BulkSMSUserModel();
            var objresPaypointBulkSMSInfo = new BulkSMSModel
            {

                customerNumber = mobileNumber,
                message = message,
                smsDateTime =dateTime           
            };
            return objresPaypointBulkSMSModel.ResponseBulkSMSInfo(objresPaypointBulkSMSInfo);
        }
    }
}