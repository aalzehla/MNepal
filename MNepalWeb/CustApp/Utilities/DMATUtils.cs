using CustApp.Models;
using CustApp.UserModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace CustApp.Utilities
{
    public class DMATUtils
    {
        public static Dictionary<string, string> GetDematName()
        {
            var objDMATModel = new DMATUserModel();

            return objDMATModel.GetDMATName();
        }

        #region Get Demat Details
        public static DataSet GetDematDetails(DMAT DMObj)
        {
            var objUserModel = new DMATUserModel();
            var objUserInfo = new DMAT
            {
                ClientCode = DMObj.ClientCode,
                UserName = DMObj.UserName,
                NWCounter = DMObj.NWCounter,
                CustomerID = DMObj.CustomerID,
                refStan = DMObj.refStan,
                Mode = "DMD" // GET DMAT Details
            };
            return objUserModel.GetDematPaymentDetails(objUserInfo);
        }
        #endregion
    }
}