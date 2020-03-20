using ThailiMNepalAgent.Models;
using ThailiMNepalAgent.UserModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace ThailiMNepalAgent.Utilities
{
    public class StmtUtils
    {
        #region "GET User Wallet and Bank AC Information By ClientCode"

        /// <summary>
        /// Get User Wallet and Bank AC Information By ClientCode
        /// </summary>
        /// <param name="clientCode"></param>
        /// <returns></returns>
        public static DataTable GetWalletORBankACNoByClientCode(string clientCode)
        {
            var objUserModel = new StmtUserModel();
            var objUserInfo = new UserInfo
            {
                ClientCode = clientCode,
                Mode = "SBCC" // SEARCH BY ClientCode
            };
            return objUserModel.GetWalletORBankACNoInformation(objUserInfo);
        }

        #endregion

        #region "GET User Statement Information"

        /// <summary>
        /// Get User Statement Information
        /// </summary>
        /// <param name="MainCodeDB"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="branchCode"></param>
        /// <returns></returns>
        public static DataTable GetStatememt(string MainCodeDB, string startDate, string endDate, string branchCode)
        {
            var objUserModel = new StmtUserModel();
            var objStmtInfo = new StatementInfo
            {
                MainCode = MainCodeDB,
                StartDate = startDate,
                EndDate = endDate,
                BranchCode = branchCode
            };
            return objUserModel.GetUserStatement(objStmtInfo);
        }

        #endregion

    }
}