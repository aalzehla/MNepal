using MNepalWeb.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using MNepalWeb.UserModels;
using System.Data.SqlClient;

namespace MNepalWeb.Utilities
{
    public class LoginUtils
    {

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
                Mode = "CAL" // Check Admin Login
            };
            return objUserModel.GetUserInformation(objUserInfo);
        }


        public static void LogOutUser(string userName)
        {
            var objUserModel = new LoginUserModels();
            var objUserInfo = new UserInfo
            {
                UserName = userName,
                Password = " ",
                Mode = "LOGOUT" // Check User Login
            };
           objUserModel.GetUserInformation(objUserInfo);
        }

        public static bool ResetFLogin(ViewModel.ResetVM model)
        {
            var objUserModel = new LoginUserModels();
            
            int i= objUserModel.ResetFirstPassword(model);
            if (i == 100)
                return true;
            else
                return false;
        }



        public static Dictionary<string, string> CheckCred(ViewModel.ResetVM model)
        {
          var objUserModel = new LoginUserModels();
          return objUserModel.Checkcredential(model.ClientCode);
        }


        public static void LogAction (MNAdminLog log)
        {
            LoginUserModels logModel = new LoginUserModels();
            logModel.LogAction(log);
        }
        #endregion

        /// <summary>
        /// Check if the logged in user is allowed to browse the page or not
        /// </summary>
        /// <param name="Controller">Browsed Controller</param>
        /// <param name="Action">Browsed Action</param>
        public void CheckUser(string Controller,string Action)
        {

        }


    }
}