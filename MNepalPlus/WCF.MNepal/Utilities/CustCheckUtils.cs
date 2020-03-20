using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using MNepalProject.Models;
using WCF.MNepal.UserModels;
using WCF.MNepal.Models;

namespace WCF.MNepal.Utilities
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
       

        public static DataTable GetCustBlockedUserInfo(string cmobile)
        {
            var objModel = new CustCheckerUserModel();
            var objCustUserInfo = new MNClientExt
            {
                UserName = cmobile
            };
            return objModel.GetCustBlockedUserInfo(objCustUserInfo);
        }

        #endregion
        #region Merchant Chercker
        public static bool GetMerchantUserCheckInfo(string cmobile)
        {
            var objModel = new CustCheckerUserModel();
            var objCustUserInfo = new MNClientExt
            {
                UserName = cmobile
            };
            return objModel.GetMerchantUserCheckInfo(objCustUserInfo).Rows.Count > 0;
        }
        #endregion

        #region Quick Register KYC Details

        public static DataTable GetQRCustKYCInfo(string cmobile)
        {
            var objModel = new CustCheckerUserModel();
            var objCustUserInfo = new MNClientExt
            {
                UserName = cmobile
            };
            return objModel.GetQRCustKYCInfoUserModel(objCustUserInfo);
        }

        #endregion

        #region Customer Details

        public static DataTable GetCustInfo(string cmobile)
        {
            var objModel = new CustCheckerUserModel();           
            return objModel.GetCustUserInfo(cmobile);
        }

        #endregion
        #region Customer Details

        public static string CheckAlreadyRequestedAccount(string cmobile)
        {
            var objModel = new CustCheckerUserModel();
            return objModel.CheckAlreadyRequestedAccount(cmobile);
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
        
        #region GetClientCodeFromPRN
        public static string GetMobileNumberFromPRN(string PRN)
        {
            var objModel = new CustCheckerUserModel();
            string mode = "GMNP";
            
            return objModel.GetMobileNumberFromPRN(mode, PRN);
        }

        #endregion
        #region GetEBankingRequestInfoFromPRN
        public static DataTable GetEBankingRequest(string PRN)
        {
            var objModel = new CustCheckerUserModel();
            string mode = "GERP";

            return objModel.GetEBankingRequest(mode, PRN);
        }

        #endregion

        #region InsertEBankingResponse
        public static int InsertEBankingResponse(SoapTransaction soapTransaction, string statusCode)
        {
            var objModel = new CustCheckerUserModel();


            return objModel.InsertEBankingResponse(soapTransaction, statusCode);
        }

        #endregion

        #region Agent Checker
        public static DataTable GetAgentInfo(string cmobile)
        {
            var objModel = new CustCheckerUserModel();
            var objCustUserInfo = new MNClientExt
            {
                UserName = cmobile
            };
            return objModel.GetAgentCheckInfo(objCustUserInfo);
        }
        #endregion

        #region Merchant Checker
        public static DataTable GetMerchantInfo(string cmobile)
        {
            var objModel = new CustCheckerUserModel();
            var objCustUserInfo = new MNClientExt
            {
                UserName = cmobile
            };
            return objModel.GetMerchantCheckInfo(objCustUserInfo);
        }
        #endregion 

        #region Merchant Checker
        public static DataTable GetUserInfo(string cmobile)
        {
            var objModel = new CustCheckerUserModel();
            var objCustUserInfo = new MNClientExt
            {
                UserName = cmobile
            };
            return objModel.GetUserCheckInfo(objCustUserInfo);
        }

        #endregion
        #region User Checker
        public static DataTable GetUserCheckInfo(string cmobile)
        {
            var objModel = new CustCheckerUserModel();
            var objCustUserInfo = new MNClientExt
            {
                UserName = cmobile
            };
            return objModel.GetUserCheckInfo(objCustUserInfo);
        }
        #endregion

        public static DataTable SessionChecker(string cmobile,string sessionID, string mode)
        {
            var objModel = new CustCheckerUserModel();
            return objModel.SessionChecker(cmobile,sessionID, mode);
        }

        public static string GetName(string MobileNumber)
        {
            string name = "Customer";
            try
            {
                DataTable dataTable = GetCustInfo(MobileNumber);
                name = dataTable.Rows[0]["Name"].ToString();
                if (name.Contains(" "))
                {
                    name = name.Substring(0, name.IndexOf(" "));
                }
                if (name == "" || name == null)
                {
                    name = "Customer";
                }
            }
            catch (Exception e)
            {
                return name;
            }

            return name;
        }
    }
}