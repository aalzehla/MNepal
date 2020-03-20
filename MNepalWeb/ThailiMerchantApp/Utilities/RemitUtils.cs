using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using ThailiMerchantApp.Models;
using ThailiMerchantApp.UserModels;

namespace ThailiMNepalAgent.Utilities
{
    public class RemitUtils
    {
        #region "REGISTRATION REMIT INFORMATION Utilities"

        /// <summary>
        /// Get the MNRemit Information
        /// </summary>
        /// <returns>Returns the datatable of MNRemit Information</returns>
        public static int InsertRemitInfo(MNRemit remitInfo)
        {
            DateTime today = DateTime.Now;
            DateTime expiryDate = today.AddDays(15);
            var objRemitModel = new RemitUserModel();
            var objRemitInfo = new MNRemit
            {
                ClientCode = remitInfo.ClientCode,
                TraceID = remitInfo.TraceID,
                SenderMobileNo = remitInfo.SenderMobileNo,
                RecipientMobileNo = remitInfo.RecipientMobileNo,
                BeneficialName = remitInfo.BeneficialName,
                RequestTokenCode = remitInfo.RequestTokenCode,
                Amount = remitInfo.Amount,
                TokenID = remitInfo.TokenID,
                Purpose = remitInfo.Purpose,
                TokenCreatedDate = DateTime.Now.ToString("dd/MM/yyyy"),
                TokenExpiryDate = expiryDate.ToString("dd/MM/yyyy"),
                Mode = "IR" //INSERT REMIT
            };
            return objRemitModel.InsertRemitInfo(objRemitInfo);
        }

        #endregion

        #region "GET Remit Information By Mobile Number"

        /// <summary>
        /// Get Remit Information By Mobile Number
        /// </summary>
        /// <param name="mobileNo"></param>
        /// <returns></returns>
        public static DataTable GetRemitInformation(string mobileNo)
        {
            var objRemitModel = new RemitUserModel();
            var objRemitInfo = new MNRemit
            {
                RecipientMobileNo = mobileNo,
                Mode = "SRBM" // SEARCH REMIT BY MobileNo
            };
            return objRemitModel.GetRemitInfo(objRemitInfo);
        }

        #endregion
    }
}