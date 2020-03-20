using System;
using System.Data;
using MNepalWCF.Models;
using MNepalWCF.UserModels;

namespace MNepalWCF.Utilities
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


        #region REGISTER TOKEN INFO Utilities

        /// <summary>
        /// Insert the RemitTokenInfo
        /// </summary>
        /// <param name="remitTokenInfo"></param>
        /// <returns></returns>
        public static int InsertRemitTokenInfo(MNRemit remitTokenInfo)
        {
            DateTime today = DateTime.Now;
            DateTime expiryDate = today.AddDays(15);
            var objRemitModel = new RemitUserModel();
            var objRemitInfo = new MNRemit
            {
                ClientCode = remitTokenInfo.ClientCode,
                TraceID = remitTokenInfo.TraceID,
                RequestTokenCode = remitTokenInfo.RequestTokenCode,
                TokenID = remitTokenInfo.TokenID,
                Mode = "UTIR" //UPDATE TOKEN ID REMIT
            };
            return objRemitModel.InsertRemitTokenInfo(objRemitInfo);
        }
        
        #endregion

    }
}