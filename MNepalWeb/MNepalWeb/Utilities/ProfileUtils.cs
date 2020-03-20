using MNepalWeb.Models;
using MNepalWeb.UserModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace MNepalWeb.Utilities
{
    public class ProfileUtils
    {
        #region "GET User Profile Information"

        public static DataTable GetUserProfileInfo(string clientCode)
        {
            var objUserModel = new LoginUserModels();
            var objUserInfo = new UserInfo
            {
                ClientCode = clientCode,
                Mode = "GCUPI" // GET USER PROFILE INFORMATION
            };
            return objUserModel.GetUserInformation(objUserInfo);
        }

        public static DataSet GetUserProfileInfoDS(string clientCode)
        {
            var objUserModel = new LoginUserModels();
            var objUserInfo = new UserInfo
            {
                ClientCode = clientCode,
                Mode = "GCUDPI" // GET USER PROFILE INFORMATION
            };
            return objUserModel.GetUserInformationDSet(objUserInfo);
        }
        


        public static DataSet GetCustModifiedValue(string clientCode)
        {
            var objUserModel = new LoginUserModels();
            var objUserInfo = new UserInfo
            {
                ClientCode = clientCode
               
            };
            return objUserModel.GetCustModifiedValue(objUserInfo);
        }
        public static DataSet GetAdminModifiedValue(string clientCode, string bankNo)
        {
            var objUserModel = new LoginUserModels();
            var objUserInfo = new UserInfo
            {
                ClientCode = clientCode,
                BankNo = bankNo

            };
            return objUserModel.GetAdminModifiedValue(objUserInfo);
        }
        #endregion


        #region "GET SuperAdmin Profile Information"

        public static DataTable GetSuperAdminProfile()
        {
            var objUserModel = new LoginUserModels();
            var objUserInfo = new UserInfo
            {
                Mode = "GSAP" // GET SUPER ADMIN PROFILE
            };
            return objUserModel.GetAllUserInformation(objUserInfo);
        }

        #endregion


        #region "GET Admin Profile Information"

        public static DataTable GetAdminProfile()
        {
            var objUserModel = new LoginUserModels();
            var objUserInfo = new UserInfo
            {
                Mode = "GAP" // GET ADMIN PROFILE
            };
            return objUserModel.GetAllUserInformation(objUserInfo);
        }

        #endregion


        #region "GET UnApprove Admin Profile Information"

        public static DataTable GetUnApproveAdminProfile()
        {
            var objUserModel = new LoginUserModels();
            var objUserInfo = new UserInfo
            {
                Mode = "GUAP" // GET UNAPPROVE ADMIN PROFILE
            };
            return objUserModel.GetAllUserInformation(objUserInfo);
        }

        #endregion


        #region "GET All Agent Profile Information"

        public static DataTable GetAllAgentProfile()
        {
            var objUserModel = new LoginUserModels();
            var objUserInfo = new UserInfo
            {
                Mode = "GAAP" // GET ALL AGENT PROFILE
            };
            return objUserModel.GetAllUserInformation(objUserInfo);
        }

        #endregion


        #region "GET All User Profile Information"

        public static DataTable GetAllUserProfile()
        {
            var objUserModel = new LoginUserModels();
            var objUserInfo = new UserInfo
            {
                Mode = "GAUP" // GET ALL USER PROFILE
            };
            return objUserModel.GetAllUserInformation(objUserInfo);
        }

        #endregion


        #region "REJECTED LIST FOR APPROVE CUSTOMER Information"

        public static DataTable GetApproveRJCustomerProfile(string userType)
        {
            var objUserModel = new LoginUserModels();
            var objUserInfo = new UserInfo
            {
                UserType = userType,
                Mode = "RLAC" // --REJECTED LIST FOR APPROVE CUSTOMER
            };
            return objUserModel.GetAllUserInformation(objUserInfo);
        }

        #endregion


        #region "REJECTED LIST FOR UNAPPROVE CUSTOMER Information"

        public static DataTable GetUnApproveRJCustomerProfile(string userType)
        {
            var objUserModel = new LoginUserModels();
            var objUserInfo = new UserInfo
            {
                UserType = userType,
                Mode = "RLUC" // --REJECTED LIST FOR UNAPPROVE CUSTOMER
            };
            return objUserModel.GetAllUserInformation(objUserInfo);
        }

        #endregion

        #region "GET Admin Modified List"

        public static DataTable GetAdminModify()
        {
            var objUserModel = new LoginUserModels();
            var objUserInfo = new UserInfo
            {

                Mode = "AMA" // ADMIN MODIFICATION APPROVE
            };
            return objUserModel.GetAllUserInformation(objUserInfo);
        }

        #endregion

        #region "REJECTED LIST FOR APPROVE ADMIN Information"

        public static List<UserInfo> GetApproveRJAdminProfile(string IsModified)
        {
            var objUserModel = new LoginUserModels();
            var objUserInfo = new UserInfo();
           
            return objUserModel.GetAllAdminInformation(objUserInfo, IsModified);
        }

        #endregion


        //#region "REJECTED LIST FOR UNAPPROVE Admin Information"

        //public static DataTable GetUnApproveRJAdminProfile()
        //{
        //    var objUserModel = new LoginUserModels();
        //    var objUserInfo = new UserInfo();

        //    return objUserModel.GetAllAdminInformation(objUserInfo);
        //}

        //#endregion

        #region SELF Registration Approval
        public static DataSet GetSelfRegDetailDS(string clientCode)
        {
            var objUserModel = new LoginUserModels();
            var objUserInfo = new CustomerSRInfo
            {
                ClientCode = clientCode,
                
            };
            return objUserModel.GetSelfRegInfoDSet(objUserInfo);
        }

        #endregion

        #region Get Customer Name
        public static DataTable GetCustomerName(string UserName)
        {
            var objUserModel = new LoginUserModels();
            var objUserInfo = new UserInfo
            {
                UserName = UserName
            };
            return objUserModel.GetCustomerName(objUserInfo);
        }
        #endregion
    }
}