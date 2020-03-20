using System.Data;
using MNepalProject.Models;
using MNepalWCF.UserModels;

namespace MNepalWCF.Utilities
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

        public static DataTable GetWalletLimit(string userName)
        {
            var objModel = new TransactionLimitUserModel();
            var objUserLimitInfo = new MNClientExt
            {
                UserName = userName
            };
            return objModel.GetWalletTransactionLimit(objUserLimitInfo);
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

    }
}