using CustApp.Models;
using CustApp.UserModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace CustApp.Utilities
{
    public class ReqTokenUtils
    {
        #region "ReqToken Utilities"

        /// <summary>
        /// Get the ReqToken information of given clientCode
        /// </summary>
        /// <param name="clientCode">Pass clientCode as string</param>
        /// <returns>Returns the datatable of ReqToken information</returns>
        public static DataTable GetReqToken(string reqToken)
        {
            var objUserModel = new ReqTokenUserModels();
            var objUserInfo = new TopUpPay
            {
                TokenUnique = reqToken
            };
            return objUserModel.GetRegToken(objUserInfo);
        }

        #endregion


        #region "ReqToken Utilities Insert"

        public static int InsertReqToken(string reqToken)
        {
            var objMobileModel = new ReqTokenUserModels();

            return objMobileModel.InsertReqToken(reqToken);
        }

        #endregion

    }
}