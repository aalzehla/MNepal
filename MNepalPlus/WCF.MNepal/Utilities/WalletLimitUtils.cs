using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using MNepalProject.Models;
using WCF.MNepal.UserModels;

namespace WCF.MNepal.Utilities
{
    public class WalletLimitUtils
    {
        #region Wallet Limit Utils

        public static DataTable GetWalletUserLimitInfo(string userName)
        {
            var objModel = new TransactionLimitUserModel();
            var objUserLimitInfo = new MNClientExt
            {
                UserName = userName
            };
            return objModel.GetTranLimitInfo(objUserLimitInfo);
        }

        #endregion
        
        #region Bank Limit Utils

        public static DataTable GetBankUserLimitInfo(string userName)
        {
            var objModel = new TransactionLimitUserModel();
            var objUserLimitInfo = new MNClientExt
            {
                UserName = userName
            };
            return objModel.GetBankTranLimitInfo(objUserLimitInfo);
        }



        public static DataTable GetLimit(string userName)
        {
            var objModel = new TransactionLimitUserModel();
            var objUserLimitInfo = new MNClientExt
            {
                UserName = userName
            };
            return objModel.GetTransactionLimit(objUserLimitInfo);
        }

        #endregion

        #region Wallet Limit Utils

        public static DataTable GetWalletLimit(string userName, string sc)
        {
            var objModel = new TransactionLimitUserModel();
            var objUserLimitInfo = new MNClientExt
            {
                UserName = userName,
                WalletProfileCode = sc

            };
            return objModel.GetWalletTransactionLimit(objUserLimitInfo);
        }

        #endregion

        #region Check KYC

        public static DataTable CheckCustKYC(string userName)
        {
            var objModel = new TransactionLimitUserModel();
            var objUserLimitInfo = new MNClientExt
            {
                UserName = userName

            };
            return objModel.CheckKYC(objUserLimitInfo);
        }

        #endregion

        #region Check Bank KYC

        public static DataTable CheckCustBankKYC(string userName)
        {
            var objModel = new TransactionLimitUserModel();
            var objUserLimitInfo = new MNClientExt
            {
                UserName = userName

            };
            return objModel.CheckBankKYC(objUserLimitInfo);
        }

        #endregion

        #region Individual Transaction Limit

        public static DataTable GetIndividualLimit(string userName)
        {
            var objModel = new TransactionLimitUserModel();
            var objUserLimitInfo = new MNClientExt
            {
                UserName = userName
            };
            return objModel.GetIndvTransactionLimit(objUserLimitInfo);
        }

        public static DataTable GetBankUserIndvTotalLimitInfo(string userName)
        {
            var objModel = new TransactionLimitUserModel();
            var objUserLimitInfo = new MNClientExt
            {
                UserName = userName
            };
            return objModel.GetBankTranIndvTotLimitInfo(objUserLimitInfo);
        }

        #endregion

        #region Fund Transfer Wallet Limit Utils

        public static DataTable GetSuperWalletTxnLimit(string userName, string walletProfileCode)
        {
            var objModel = new TransactionLimitUserModel();
            var objUserLimitInfo = new MNClientExt
            {
                UserName = userName,
                WalletProfileCode = walletProfileCode

            };
            return objModel.GetSuperWalletTxnLimit(objUserLimitInfo);
        }

        #endregion

        #region Wallet Limit NON KYC
        public static DataTable GetWalletLimitNonKYC(string userName, string sc)
        {
            var objModel = new TransactionLimitUserModel();
            var objUserLimitInfo = new MNClientExt
            {
                UserName = userName,
                WalletProfileCode = sc

            };
            return objModel.GetWalletTransactionLimitNonKYC(objUserLimitInfo);
        }
        #endregion

        #region Wallet Limit With KYC WithOut Bank Limit
        public static DataTable GetWalletLimitWithKYCOutBank(string userName, string sc)
        {
            var objModel = new TransactionLimitUserModel();
            var objUserLimitInfo = new MNClientExt
            {
                UserName = userName,
                WalletProfileCode = sc

            };
            return objModel.GetWalletTranLimitWithKYCOutBank(objUserLimitInfo);
        }
        #endregion


    }
}