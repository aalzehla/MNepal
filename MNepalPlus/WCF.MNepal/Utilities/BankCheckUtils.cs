using MNepalProject.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WCF.MNepal.UserModels;

namespace WCF.MNepal.Utilities
{
    public class BankCheckUtils
    {
        #region Bank Checker

        public static DataTable GetBankInfo()
        {
            var objModel = new BankCheckerUserModel();
            var objBankInfo = new MNBankTable
            { };
            return objModel.GetBankInfo(objBankInfo);
        }

        #endregion
    }
}