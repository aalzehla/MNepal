using MNepalWeb.Models;
using MNepalWeb.UserModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace MNepalWeb.Utilities
{
    public class AgentUtils
    {
        #region "GET Agent Profile Information By Searching Mobile Number"

        /// <summary>
        /// Get Customer Profile By Searching Mobile No
        /// </summary>
        /// <param name="mobileNo"></param>
        /// <returns></returns>
        public static DataTable GetAgentProfileByMobileNo(string mobileNo)
        {
            var objUserModel = new CustomerUserModel();
            var objUserInfo = new UserInfo
            {
                ContactNumber1 = mobileNo,
                Mode = "SABMN" // SEARCH AGENT BY MobileNo
            };
            return objUserModel.GetCustomerDetailInformation(objUserInfo);
        }

        #endregion

        #region "GET Agent Profile Information By Searching Name"

        /// <summary>
        /// Get Customer Profile By Searching Name
        /// </summary>
        /// <param name="mobileNo"></param>
        /// <returns></returns>
        public static DataTable GetAgentProfileByName(string name)
        {
            var objUserModel = new CustomerUserModel();
            var objUserInfo = new UserInfo
            {
                Name = name,
                Mode = "SABN" // SEARCH AGENT BY Name
            };
            return objUserModel.GetCustomerDetailInformation(objUserInfo);
        }

        #endregion

        #region "GET Agent Profile Information By Searching Account Number"

        /// <summary>
        /// Get Customer Profile By Searching Account Number
        /// </summary>
        /// <param name="mobileNo"></param>
        /// <returns></returns>
        public static DataTable GetAgentProfileByAC(string accountNo)
        {
            var objUserModel = new CustomerUserModel();
            var objUserInfo = new UserInfo
            {
                WalletNumber = accountNo,
                Mode = "SABAC" // SEARCH AGENT BY Account Number
            };
            return objUserModel.GetCustomerDetailInformation(objUserInfo);
        }

        #endregion

        #region "GET Agent Profile Information By Searching Name & Account Number"

        /// <summary>
        /// Get Customer Profile By Searching Name N Account Number
        /// </summary>
        /// <param name="mobileNo"></param>
        /// <param name="accountNo"></param>
        /// <returns></returns>
        public static DataTable GetAgentProfileByNameAC(string name, string accountNo)
        {
            var objUserModel = new CustomerUserModel();
            var objUserInfo = new UserInfo
            {
                Name = name,
                WalletNumber = accountNo,
                Mode = "SABNAC" // SEARCH AGENT BY Name/AccountNumber
            };
            return objUserModel.GetCustomerDetailInformation(objUserInfo);
        }

        #endregion

        #region "GET Agent Profile Information By Searching Name & Account Number"

        /// <summary>
        /// Get Customer Profile By Searching Name N Account Number
        /// </summary>
        /// <param name="name"></param>
        /// <param name="accountNo"></param>
        /// <param name="mobileNo"></param>
        /// <returns></returns>
        public static DataTable GetAgentProfileByALL(string name, string accountNo, string mobileNo)
        {
            var objUserModel = new CustomerUserModel();
            var objUserInfo = new UserInfo
            {
                Name = name,
                WalletNumber = accountNo,
                ContactNumber1 = mobileNo,
                Mode = "SABALL" // SEARCH AGENT BY ALL
            };
            return objUserModel.GetCustomerDetailInformation(objUserInfo);
        }

        #endregion


        #region "GET Agent ID"

        /// <summary>
        /// Get Agent ID By ClientCode
        /// </summary>
        /// <param name="clientCode"></param>
        /// <returns></returns>
        public static DataTable GetAgentId(string clientCode)
        {
            var objUserModel = new CustomerUserModel();
            var objUserInfo = new UserInfo
            {
                ClientCode = clientCode,
                Mode = "GAID" // Get Agent ID By ClientCode
            };
            return objUserModel.GetAgentIdModel(objUserInfo);
        }

        #endregion


    }
}