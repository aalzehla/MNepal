using MNepalWeb.Models;
using MNepalWeb.UserModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace MNepalWeb.Utilities
{
    public class BankUtils
    {
        #region "DataSet Bank Code Utilities"

        /// <summary>
        /// Get the Bank Code Details Information
        /// </summary>
        /// <returns>Returns the dataset of Bank Code Information</returns>
        public static DataSet GetDataSetPopulateBankCode()
        {
            var objCountryModel = new BankUserModel();
            var objCountryInfo = new UserInfo()
            {
                Mode = "GBC" //Get Bank Code
            };
            return objCountryModel.GetDsBankCodeInfo(objCountryInfo);
        }

        public static DataSet GetDataSetPopulateBranchCode(string BankCode)
        {
            var objCountryModel = new BankUserModel();
            var objCountryInfo = new UserInfo()
            {
                WBankCode = BankCode,
                Mode = "GBRC" //Get Bank Code
            };
            return objCountryModel.GetDsBranchCodeInfo(objCountryInfo);
        }

        #endregion
    }
}