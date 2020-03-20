using MNepalProject.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WCF.MNepal.Models;
using WCF.MNepal.UserModels;

namespace WCF.MNepal.Utilities
{
    public class CashOutUtils
    {
        #region "REGISTRATION CASHOUT INFORMATION Utilities"

        /// <summary>
        /// Get the MNRemit Information
        /// </summary>
        /// <returns>Returns the datatable of MNRemit Information</returns>
        public static int InsertCashOutInfo(CashOut cashInfo)
        {
            DateTime today = DateTime.Now;
            DateTime expiryDate = today.AddHours(1);
            var objCashModel = new CashOutUserModel();
            var objCashInfo = new CashOut
            {
                ClientCode = cashInfo.ClientCode,
                TraceID = cashInfo.TraceID,
                //AgentMobileNo = cashInfo.AgentMobileNo,
                CustomerMobileNo = cashInfo.CustomerMobileNo,
                BeneficialName = cashInfo.BeneficialName,
                RequestTokenCode = cashInfo.RequestTokenCode,
                Amount = cashInfo.Amount,
                TokenID = cashInfo.TokenID,
                Purpose = cashInfo.Purpose,
                TokenCreatedDate = DateTime.Now.ToString("dd/MM/yyyy"),
                TokenExpiryDate = expiryDate.ToString("dd/MM/yyyy"),
                Mode = "CO" //INSERT REMIT
            };
            return objCashModel.InsertCashOutInfo(objCashInfo);
        }

        public static int UpdateCashOut(CashOut cashInfo)
        {
            
            var objCashModel = new CashOutUserModel();
            var objCashInfo = new CashOut
            {
                AgentMobileNo = cashInfo.AgentMobileNo,
                CustomerMobileNo = cashInfo.CustomerMobileNo,
                Status = cashInfo.Status,
                Remarks = cashInfo.Remarks,  
                RequestTokenCode = cashInfo.RequestTokenCode,
                Mode = "UC" // update cash out transaction by agent
            };
            return objCashModel.UpdateCashOut(objCashInfo);
        }

        #endregion

        #region "GET CashOut Information By AGENT Mobile Number"

        /// <summary>
        /// Get Remit Information By Mobile Number
        /// </summary>
        /// <param name="mobileNo"></param>
        /// <returns></returns>
        public static DataTable GetCashOutInfo(string mobileNo,string umobile,string transactioncode, string amount)
        {
            var objCashModel = new CashOutUserModel();
            var objCashInfo = new CashOut
            {
                CustomerMobileNo = umobile,
                RequestTokenCode = transactioncode,
                AgentMobileNo = mobileNo,
                Amount = amount,
                Mode = "SCOA" // SEARCH CASHOUT BY Agent MobileNo
            };
            return objCashModel.GetCashOutInfo(objCashInfo);
        }


        #endregion


        #region CashOutList
        //public static DataTable GetCashOutList(string senderMobileNo, string traceID, string recipientMobileNo, string tokenCreatedDate, string tokenExpiryDate, string amount, string status)


        public static DataTable GetCashOutList(string SenderMobileNo, string TokenID, string RecipientMobileNo, string Amount, string Status)
            

        {
            var objModel = new CashOutUserModel();
            var objCustUserInfo = new CashOutList
            {
                 
                SenderMobileNo = SenderMobileNo,

                TokenID = TokenID,
                RecipientMobileNo = RecipientMobileNo,
                 
                Amount = Amount,
                Status = Status

            };
            return objModel.GetCashOutList(objCustUserInfo);
        }

        #endregion

        #region"Check RequestTokenCode
        public static DataTable CheckRequestToken(string requestTokenCode,string userName)
        {
            var objModel = new CashOutUserModel();
            var objCustUserInfo = new CashOut
            {
                RequestTokenCode = requestTokenCode,
                SenderMobileNo= userName
            };
            return objModel.CheckRequestToken(objCustUserInfo);
        }
        #endregion
    }
}