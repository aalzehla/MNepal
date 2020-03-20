using MNSuperadmin.Models;
using MNSuperadmin.UserModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace MNSuperadmin.Utilities
{
    public class CustomerSupportUtils
    {
        #region customer support details
        public static DataSet CustomerSupportDetails(string supportId)
        {
             
            var objUserModel = new CustomerSupportUserModel();
            var objUserInfo = new CustomerSupport
            {
                //ClientCode = clientCode,
                //UserName = userName,
                SupportId = supportId,

                Mode = "CSD" // details for customer support 
            };
            return objUserModel.CustomerSupportDetails(objUserInfo);
        }

        #endregion
    }
}