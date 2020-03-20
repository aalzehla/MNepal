using System;
using System.Data;
using MNepalWeb.Models;
using MNepalWeb.UserModels;
using System.Linq;
using System.Collections.Generic;
using MNepalWeb.ViewModel;

namespace MNepalWeb.Utilities
{
    public class UserProfileUtils
    {
        #region "GET Manage Admin Profile Information"

        public static DataTable GetAdminProfile()
        {
            var objUserProfileModel = new UserProfileUserModel();
            var objUserProfileInfo = new UserProfilesInfo
            {
                Mode = "GMAP" // GET MANAGE ADMIN PROFILE
            };
            return objUserProfileModel.GetAllUserProfileInfo(objUserProfileInfo);
        }

        #endregion


        #region "GET Admin Profile Name Information"

        public static DataSet GetDataSetPopulateProfileName()
        {
            var objUserProfileModel = new UserProfileUserModel();
            var objUserProfileInfo = new UserProfilesInfo()
            {
                Mode = "GMAP" // GET MANAGE ADMIN PROFILE
            };
            return objUserProfileModel.GetDsProfileNameInfo(objUserProfileInfo);
        }

        #endregion


        #region "GET User Profile Information"

        public static DataTable GetAdminUserProfileInfo(string profileCode)
        {
            var objUserModel = new UserProfileUserModel();
            var objUserInfo = new UserProfilesInfo()
            {
                UPID = profileCode,
                Mode = "VAP" // GET ADMIN USER PROFILE TABLE INFORMATION
            };
            return objUserModel.GetAdminProfileUserInformation(objUserInfo);
        }

        public static DataTable GetAdminUserProfileMenuInfo(string clientCode)
        {
            var objUserModel = new UserProfileUserModel();
            var objUserInfo = new UserProfilesInfo()
            {
                UPID = clientCode,
                Mode = "VAPM" // GET ADMIN USER PROFILE MENU TABLE INFORMATION
            };
            return objUserModel.GetAdminProfileMenuUserInformation(objUserInfo);
        }

        public static DataSet GetAdminUserProfileInfo1(string clientCode)
        {
            var objUserModel = new UserProfileUserModel();
            var objUserInfo = new UserProfilesInfo()
            {
                ClientCode = clientCode,
                Mode = "GAUPI" // GET ADMIN USER PROFILE INFORMATION
            };
            return objUserModel.GetAdminUserMenuListInformation(objUserInfo);
        }

        #endregion
        public static DataTable GetAdminUserProfile(string ProfileCode)
        {
            var objUserModel = new UserProfileUserModel();
          
            return objUserModel.GetAdminProfileNameInformation(ProfileCode);
        }

        #region "CREATE ADMIN PROFILE INFORMATION Utilities"

        /// <summary>
        /// Create Admin Profile
        /// </summary>
        /// <returns>Returns the datatable of Create Admin Profile </returns>
        public static int CreateAdminProfileInfo(UserProfilesInfo userProfileInfo)
        {
            string menuAllowed = String.Empty;
            //if (userProfileInfo.CanAdmin != null)
            //{
            //    menuAllowed = "'" + userProfileInfo.CanAdmin + "',";
            //}

            var objUserProfileInfo = new UserProfilesInfo
            {
                ProfileName = userProfileInfo.ProfileName,
                ProfileDesc = userProfileInfo.ProfileDesc,
                Mode = "IAUP" //INSERT ADMIN USER PROFILE
            };

            string[] chosenUser = userProfileInfo.GetType()
                .GetProperties()
                .Select(p =>
                {
                    object value = p.GetValue(userProfileInfo, null);
                    return value == null ? null : value.ToString();
                }).Where(s => !string.IsNullOrEmpty(s)).ToArray();

            string salesManCode = string.Join(",", chosenUser
                .Select(x => string.Format("'{0}'", x)));

            string objMenuAllow = salesManCode;
            var objUserProfileUserModel = new UserProfileUserModel();
            
            return objUserProfileUserModel.CreateAdminSetupInfo(objMenuAllow, objUserProfileInfo);
        }

        #endregion

        
        #region "UPDATE ADMIN PROFILE INFORMATION Utilities"

        /// <summary>
        /// Update Admin Profile
        /// </summary>
        /// <returns>Returns the datatable of Create Admin Profile </returns>
        public static int UpdateAdminProfileInfo(UserProfilesInfo userProfileInfo)
        {
            var objUserProfileUserModel = new UserProfileUserModel();

            var objUserProfileInfo = new UserProfilesInfo
            {
                UPID = userProfileInfo.UPID,
                ProfileName = userProfileInfo.ProfileName,
                ProfileDesc = userProfileInfo.ProfileDesc,
                UserProfileStatus=userProfileInfo.UserProfileStatus,
                Mode = "UAUP" //UPDATE ADMIN USER PROFILE
            };

            string[] chosenUser = userProfileInfo.GetType()
                .GetProperties()
                .Select(p =>
                {
                    object value = p.GetValue(userProfileInfo, null);
                    return value == null ? null : value.ToString();
                }).Where(s => !string.IsNullOrEmpty(s)).ToArray();

            string salesManCode = string.Join(",", chosenUser
                .Select(x => string.Format("'{0}'", x)));

            string objMenuAllow = salesManCode;
            return objUserProfileUserModel.UpdateAdminProfile(objMenuAllow, objUserProfileInfo);
        }

        #endregion

        public static int CreateAdminProfile(AdminProfileVM menumodel)
        {
            string menuAllowed = String.Empty;
            //if (userProfileInfo.CanAdmin != null)
            //{
            //    menuAllowed = "'" + userProfileInfo.CanAdmin + "',";
            //}


            var selected = menumodel.MNMenus.Where(x => x.IsSelected).Select(x => x.Hierarchy).ToArray();
            menuAllowed = string.Join(",", selected.Select(x => string.Format("'{0}'", x)));

            
            var objUserProfileUserModel = new UserProfileUserModel();

            return objUserProfileUserModel.CreateAdminProfile(menumodel.ProfileName,menumodel.ProfileDesc,menuAllowed);
        }

        public static int UpdateAdminProfile(AdminProfileVM menumodel)
        {
            var objUserProfileUserModel = new UserProfileUserModel();

            string menuAllowed = String.Empty;
            //if (userProfileInfo.CanAdmin != null)
            //{
            //    menuAllowed = "'" + userProfileInfo.CanAdmin + "',";
            //}


            var selected = menumodel.MNMenus.Where(x => x.IsSelected).Select(x => x.Hierarchy).ToArray();
            menuAllowed = string.Join(",", selected.Select(x => string.Format("'{0}'", x)));
            return objUserProfileUserModel.UpdateAdminProfile(menumodel.ProfileCode,menumodel.ProfileName,menumodel.ProfileDesc,menumodel.ProfileStatus, menuAllowed);
        }

        public static List<MNMenuTable> GetAdminMenu()
        {
            var objUserProfileUserModel = new UserProfileUserModel();

            return objUserProfileUserModel.GetMenu();
        }

    }
}