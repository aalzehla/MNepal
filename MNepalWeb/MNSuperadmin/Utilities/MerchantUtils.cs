using MNSuperadmin.Models;
using MNSuperadmin.UserModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;

namespace MNSuperadmin.Utilities
{
    public static class MerchantUtils
    {
        #region "GET Merchant Detail Information"

        public static DataTable GetMerchantDetailInfo()
        {
            var objMerchantModel = new MerchantUserModels();
            var objMerchantInfo = new MNMerchants
            {
                Mode = "GMDI" // GET MERCHANT DETAIL INFORMATION
            };
            return objMerchantModel.GetMerchantDetailInformation(objMerchantInfo);
        }
        #endregion
        
        #region
        public static bool InsertMerchantMakerChecker(string ClientCode, string ModifyingAdmin, string ModifyingBranch, string ModifiedField)
        {
            return new MerchantUserModels().InsertIntoMakerCheckerMerchant(ClientCode, ModifyingAdmin, ModifyingBranch, ModifiedField) == 100;
        }
        #endregion

        #region
        public static DataTable GetMerchantModifyInfo(string clientCode)
        {
            var objUserModel = new LoginUserModels();
            var objUserInfo = new UserInfo
            {
                ClientCode = clientCode,
                Mode = "GMMPI" // GET MERCHANT MODIFY INFORMATION
            };
            return objUserModel.GetUserInformation(objUserInfo);
        }
        #endregion

        #region
        internal static bool RejectMerchantModified(this UserInfo member)
        {
            var objUserModel = new MerchantUserModels();
            var objUserInfo = new UserInfo
            {
                ClientCode = member.ClientCode,
                AdminUserName = member.AdminUserName,
                AdminBranch = member.AdminBranch,
                Remarks = member.Remarks

            };
            return objUserModel.RejectMerchantModified(objUserInfo) == 100;
        }
        #endregion

        #region Approving Merchant Registration
        internal static int MerchantRegisterApprove(UserInfo member, string Approve)
        {
            var objUserModel = new MerchantUserModels();
            var objUserInfo = new UserInfo
            {
                ClientCode = member.ClientCode,

            };
            return objUserModel.MerchantRegApprove(objUserInfo, Approve);
        }

        #endregion

        #region Approving Merchant Modification
        internal static bool MerchantModifyApprove(this UserInfo member)
        {
            var objUserModel = new MerchantUserModels();
            var objUserInfo = new UserInfo
            {
                ClientCode = member.ClientCode,
                AdminUserName = member.UserName,
                
            };
            return objUserModel.MerchantModApprove(objUserInfo) == 100;
        }

        #endregion

        #region Merchant Reg Reject/Approve

        internal static int MerchantRegReject(UserInfo member, string Rejected)
        {
            var objUserModel = new MerchantUserModels();
            var objUserInfo = new UserInfo
            {
                ClientCode = member.ClientCode,
                UserName = member.UserName,
                Remarks = member.Remarks,
                AdminUserName = member.AdminUserName,
                AdminBranch = member.AdminBranch


            };
            return objUserModel.MerchantRegReject(objUserInfo, Rejected);
        }
        #endregion

        #region Merchant Modify Reject/Approve

        internal static int MerchantModReject(UserInfo member, string Rejected)
        {
            var objUserModel = new MerchantUserModels();
            var objUserInfo = new UserInfo
            {
                ClientCode = member.ClientCode,
                UserName = member.UserName,
                Remarks = member.Remarks,
                AdminUserName = member.AdminUserName,
                AdminBranch = member.AdminBranch


            };
            return objUserModel.MerchantModReject(objUserInfo, Rejected);
        }
        #endregion

        #region
        public static bool UpdateRejectedMerchantInfo(this UserInfo member, String MerchantCategory)
        {
            var objMemberInfo = new UserInfo()
            {
                ClientCode = member.ClientCode,
                Address = member.Address,
                Status = member.Status,
                Name = member.Name,
                ClientStatus = member.ClientStatus,
                FName = member.FName,
                MName = member.MName,
                LName = member.LName,
                PStreet = member.PStreet,
                PVDC = member.PVDC,
                PHouseNo = member.PHouseNo,
                PWardNo = member.PWardNo,
                BusinessName = member.BusinessName,
                RegistrationNumber = member.RegistrationNumber,
                VATNumber = member.VATNumber,
                LandlineNumber = member.LandlineNumber,
                PanNo = member.PanNo,
                WebsiteName = member.WebsiteName,
                EmailAddress = member.EmailAddress,
                PProvince = member.PProvince,
                PDistrictID = member.PDistrictID,
                ContactNumber1 = member.ContactNumber1,
                ContactNumber2 = member.ContactNumber2,
                BranchCode = member.BranchCode,
                BankAccountNumber = member.BankAccountNumber,
                BankNo = member.BankNo,
                Citizenship = member.Citizenship,
                CitizenshipIssueDate = DateTime.ParseExact(member.CitizenshipIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                               .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
                BSCitizenshipIssueDate = member.BSCitizenshipIssueDate,
                CitizenshipPlaceOfIssue = member.CitizenshipPlaceOfIssue,
                IsApproved = member.IsApproved,
                IsRejected = member.IsRejected,
                PassportImageName = member.PassportImageName,
                FrontImageName = member.FrontImageName,
                BackImageName = member.BackImageName,
                RegCertificatePhotoName = member.RegCertificatePhotoName,
                TaxClearFrontName = member.TaxClearFrontName,
                TaxClearBackName = member.TaxClearBackName

            };
            return new MerchantUserModels().UpdateRejectedMerchantInfo(objMemberInfo, MerchantCategory) > 0;
        }
        #endregion

        #region "GET Merchant User Profile Information By Searching Name"

        public static DataTable GetMerchantUserProfileByALL(string name, string accountNo, string mobileNo)
        {
            var objUserModel = new MerchantUserModels();
            var objUserInfo = new UserInfo
            {
                Name = name,
                WalletNumber = accountNo,
                ContactNumber1 = mobileNo,
                Mode = "SMBALL" // SEARCH MERCHANT BY ALL
            };
            return objUserModel.GetCustomerDetailInformation(objUserInfo);
        }

        #endregion

        #region "GET Merchant User Profile Information By Searching Mobile No"

        public static DataTable GetMerchantUserProfileMobileOrName(string name, string accountNo, string mobileNo)
        {
            var objUserModel = new MerchantUserModels();
            var objUserInfo = new UserInfo
            {
                Name = name,
                WalletNumber = accountNo,
                ContactNumber1 = mobileNo,
                Mode = "SMBNM" // SEARCH MERCHANT BY Mobile No
            };
            return objUserModel.GetCustomerDetailInformation(objUserInfo);
        }

        #endregion

       

        #region "GET Merchant User Profile Information By Searching Name"

        public static DataTable GetMerchantUserProfileByName(string name)
        {
            var objUserModel = new CustomerUserModel();
            var objUserInfo = new UserInfo
            {
                Name = name,
                Mode = "SMBN" // SEARCH MERCHANT BY Name
            };
            return objUserModel.GetCustomerDetailInformation(objUserInfo);
        }

        #endregion

        #region "APPROVED LIST FOR APPROVE Merchant Information"

        public static List<UserInfo> GetMerchantApprovedList(string IsModified, string UserName)
        {
            var objUserModel = new MerchantUserModels();
            var objUserInfo = new UserInfo();

            return objUserModel.GetAllMerchantApprovedInformation(objUserInfo, IsModified, UserName);
        }

        #endregion

       
        #region "REJECTED LIST FOR Merchant Modification"

        public static List<UserInfo> GetMerchantModificationRejectList(string IsModified,string UserName)
        {
            var objUserModel = new MerchantUserModels();
            var objUserInfo = new UserInfo();

            return objUserModel.GetAllMerchantModifyRejectedInformation(objUserInfo, IsModified,UserName);
        }

        #endregion

        #region

        public static int RegisterMerchantInfo(UserInfo userInfo, string MerchantCategory)
        {
            var objMobileModel = new MerchantUserModels();
            var objMobileInfo = new UserInfo
            {
                ClientCode = userInfo.ClientCode,
                FName = userInfo.FName,
                MName = userInfo.MName,
                LName = userInfo.LName,
                PStreet = userInfo.PStreet,
                PVDC = userInfo.PVDC,
                PHouseNo = userInfo.PHouseNo,
                PWardNo = userInfo.PWardNo,
                Address = userInfo.Address,
                Status = userInfo.Status,
                BusinessName = userInfo.BusinessName,
                RegistrationNumber = userInfo.RegistrationNumber,
                VATNumber = userInfo.VATNumber,
                PanNo = userInfo.PanNo,
                WebsiteName = userInfo.WebsiteName,
                Citizenship = userInfo.Citizenship,
                CitizenshipIssueDate = DateTime.ParseExact(userInfo.CitizenshipIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),//userInfo.DOB,
                BSCitizenshipIssueDate = userInfo.BSCitizenshipIssueDate,
                CitizenshipPlaceOfIssue = userInfo.CitizenshipPlaceOfIssue,
                LandlineNumber = userInfo.LandlineNumber,
                ContactNumber1 = userInfo.ContactNumber1,
                ContactNumber2 = userInfo.ContactNumber2,
                EmailAddress = userInfo.EmailAddress,
                PDistrictID = userInfo.PDistrictID,
                PProvince = userInfo.PProvince,
                UserName = userInfo.UserName,
                PIN = userInfo.PIN,
                Password = userInfo.Password,
                IsApproved = userInfo.IsApproved,
                IsRejected = userInfo.IsRejected,
                UserType = userInfo.UserType,
                WalletNumber = userInfo.WalletNumber,
                WBankCode = userInfo.WBankCode,
                WBranchCode = userInfo.WBranchCode,
                WIsDefault = userInfo.WIsDefault,
                AgentId = userInfo.AgentId,
                BankAccountNumber = userInfo.BankAccountNumber,
                BankNo = userInfo.BankNo,
                BranchCode = userInfo.BranchCode,
                IsDefault = userInfo.IsDefault,
                FrontImage = userInfo.FrontImageName,
                BackImage = userInfo.BackImageName,
                PassportImage = userInfo.PassportImageName,
                RegCertificateImage = userInfo.RegCertificatePhotoName,
                TaxClearFrontImage = userInfo.TaxClearFrontName,
                TaxClearBackImage = userInfo.TaxClearBackName,
                retrievalReference = userInfo.retrievalReference

            };
            return objMobileModel.RegisterMerchantInfo(objMobileInfo, MerchantCategory);
        }


        public static DataTable GetMerchantProfileInfo(string clientCode)
        {
            var objUserModel = new LoginUserModels();
            var objUserInfo = new UserInfo
            {
                ClientCode = clientCode,
                Mode = "GMUPI" // GET Merchant PROFILE INFORMATION
            };
            return objUserModel.GetUserInformation(objUserInfo);
        }


        #region "REJECTED LIST FOR APPROVE ADMIN Information"

        public static List<UserInfo> GetMerchantRejectedList(string IsModified, string UserName)
        {
            var objUserModel = new MerchantUserModels();
            var objUserInfo = new UserInfo();

            return objUserModel.GetAllMerchantRejectedInformation(objUserInfo, IsModified, UserName);
        }

        #endregion

       


        #region "APPROVED LIST FOR APPROVE ADMIN Information"

        public static List<UserInfo> GetRegisteredMerchantList(string IsModified, string UserName)
        {
            var objUserModel = new MerchantUserModels();
            var objUserInfo = new UserInfo();

            return objUserModel.GetAllRegisteredMerchantInformation(objUserInfo, IsModified, UserName);
        }

        #endregion





       

        #endregion

        #region "APPROVED LIST FOR Merchant Modification"

        public static List<UserInfo> GetModifiedMerchantList(string IsModified,String UserName)
        {
            var objUserModel = new MerchantUserModels();
            var objUserInfo = new UserInfo();

            return objUserModel.GetAllMerchantModifiedInformation(objUserInfo, IsModified,UserName);
        }

        #endregion

        #region Adding Values in MakerChecker for old and new in Province,District and MerchantCategory 
        public static DataSet GetSuperAdminMerchantModifiedValue(string clientCode)
        {
            var objUserModel = new MerchantUserModels();
            var objUserInfo = new UserInfo
            {
                ClientCode = clientCode

            };
            return objUserModel.GetMerchantModifiedValue(objUserInfo);
        }
        #endregion

        public static bool UpdateMerchantStatus(string ClientCode, string Status, string SAdminBranchCode, string SAdminUserName, string BlockRemarks)
        {

            return new MerchantUserModels().UpdateMerchantStatus(ClientCode, Status, SAdminBranchCode, SAdminUserName, BlockRemarks) == 1;

        }

        #region GET Merchant Status Changed List

        internal static List<UserInfo> GetMerchantStatus(string MobileNo)
        {
            var objUserModel = new MerchantUserModels();
            return objUserModel.GetMerchantStatusList(MobileNo);

        }

        #endregion

        internal static bool MerchantStatusReject(string ClientCode, string Status)
        {
            var objUserModel = new MerchantUserModels();

            return objUserModel.StatusReject(ClientCode, Status) == 1;
        }


        public static List<UserInfo> GetPinResetList(string BranchCode, bool COC, string MobileNo)
        {
            return new MerchantUserModels().GetPinApproveList(BranchCode, COC, MobileNo);
        }
        

        public static int InsertResponseQuickSelfReg(string userName, string retRef, string statusCode, string statusMsg)
        {
            var objMobileModel = new MerchantUserModels();

            return objMobileModel.InsertResponseQuickSelfReg(userName, retRef, statusCode, statusMsg);
        }



        public static Dictionary<string, string> GetServiceType()
        {
            var objMerchantModel = new MerchantUserModels();

            return objMerchantModel.GetServiceType();
        }

        public static Dictionary<string, string> GetMerchantsType()
        {
            var objMerchantModel = new MerchantUserModels();

            return objMerchantModel.GetMerchantsType();
        }
    }
}