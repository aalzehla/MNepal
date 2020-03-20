using System.Data;
using MNepalWCF.Models;
using MNepalWCF.UserModels;

namespace MNepalWCF.Utilities
{
    public class LoginUtils
    {
        #region "Insert Sync Detail"

        ////INSERT WALLET/BANK BALANCE SYNC PROFILE
        public static int CreateMobileUsersInfo(SyncDetail userInfo)
        {
            var objLoginMobileModel = new MobileBalnSync();
            var objLoginMobileInfo = new SyncDetail
            {
                Mobile = userInfo.Mobile,
                Tid = userInfo.Tid,
                Mode = "ISBD" //Create Sync Balance Details
            };
            return objLoginMobileModel.CreateMobileSyncInfo(objLoginMobileInfo);
        }

        #endregion

        #region "Last Sync Info"

        ////GET LAST BALANCE SYNC PROFILE
        public static DataTable GetSyncInfo(string mobile,string tid)
        {
            var objLoginMobileModel = new MobileBalnSync();
            var objSyncInfo = new SyncDetail
            {
                Mobile = mobile,
                Tid = tid,
                Mode = "SSBD" //GET Sync Balance Details
            };
            return objLoginMobileModel.GetLastSyncInfo(objSyncInfo);
        }

        #endregion

        #region "Login Related Utilities"

        /// <summary>
        /// Get the login information of given username and password
        /// </summary>
        /// <param name="userName">Pass userName as string</param>
        /// <param name="passWord">Pass passWord as string</param>
        /// <returns>Returns the datatable of login information</returns>
        public static DataTable GetLoginInfo(string userName, string passWord)
        {
            var objUserModel = new LoginUserModels();
            var objUserInfo = new UserInfo
            {
                UserName = userName,
                Password = passWord,
                ClientCode = "",
                Mode = "CUL" // Check User Login
            };
            return objUserModel.GetUserInformation(objUserInfo);
        }

        #endregion

        #region "Insert Login Mobile Detail"

        ////INSERT MOBILE USERS PROFILE
        public static int CreateMobileUserInfo(Login userInfo)
        {
            var objLoginMobileModel = new CreateMobileDetailUserModels();
            var objLoginMobileInfo = new Login
            {
                Mobile = userInfo.Mobile,
                DeviceID = userInfo.DeviceID,
                GeneratedPass = userInfo.GeneratedPass,
                Mode = "IMD" //Create Mobile Details
            };
            return objLoginMobileModel.CreateMobileUserInfo(objLoginMobileInfo);
        }

        #endregion
    }
}