using MNSuperadmin.Models;
using MNSuperadmin.UserModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace MNSuperadmin.Utilities
{
    public static class WalletLimitUtils
    {
        public static DataSet GetWalletInfoDS(string walletProfileCode)
        {
            var objUserModel = new WalletLimitUserModel();
            var objUserInfo = new LimitWallet
            {
                WalletProfileCode = walletProfileCode,
                Mode = "GSWI" // GET SUPER ADMIN WALLET INFORMATION
            };
            return objUserModel.GetWalletInfoDSet(objUserInfo);
        }

        public static bool UpdateWalletInfo(this UserProfilesInfo wallet)
        {
            var objWalletInfo = new UserProfilesInfo()
            {
                WalletProfileCode = wallet.WalletProfileCode,
                WTxnCount = wallet.WTxnCount,
                WPerTxnAmt = wallet.WPerTxnAmt,
                WPerDayAmt = wallet.WPerDayAmt,
                WTxnAmtM = wallet.WTxnAmtM,
               
            };
            return new WalletLimitUserModel().UpdateWalletLimit(objWalletInfo) > 0;
        }

        
    }
}