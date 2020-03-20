using MNepalWeb.Models;
using MNepalWeb.UserModels;
using System;
using System.Data;
using System.Globalization;

namespace MNepalWeb.Utilities
{
    public static class RegisterUtils
    {
        #region "DATATABLE Checking UserName Utilities"

        /// <summary>
        /// Get the UserName Details Information
        /// </summary>
        /// <returns>Returns the datatable of UserName Information</returns>
        public static DataTable GetCheckUserName(string userName)
        {
            var objModel = new LoginUserModels();
            var objUserInfo = new UserInfo
            {
                UserName = userName,
               Mode = "GCUN" //Get Check User Name
                 
            };
            return objModel.GetUserInformation(objUserInfo);
        }

        #endregion


        #region "DATATABLE Checking Admin UserName Utilities"
         
        public static DataTable GetCheckAdminUserName(string userName)
        {
            var objModel = new LoginUserModels();
            var objUserInfo = new UserInfo
            {
                UserName = userName,
                Mode = "GCAUN" //Get Check Admin User Name
            };
            return objModel.GetUserInformation(objUserInfo);
        }

        #endregion
        

         

        #region "DATATABLE Checking Mobile Number Utilities"

        /// <summary>
        /// Get the Mobile Number Details Information
        /// </summary>
        /// <returns>Returns the datatable of Mobile Number Information</returns>
        public static DataTable GetCheckMobileNo(string mobileNo)
        {
            var objMobileModel = new RegisterUserModel();
            var objMobileInfo = new UserInfo
            {
                ContactNumber1 = mobileNo,
                Mode = "GCMN" //Get Check Mobile No
            };
            return objMobileModel.GetMobileInfo(objMobileInfo);
        }

  
        #endregion


        #region "DATATABLE Checking Client Code Utilities"

        /// <summary>
        /// Get the Client Code Details Information
        /// </summary>
        /// <returns>Returns the datatable of Client Code Information</returns>
        public static DataTable GetClientCode()
        {
            var objMobileModel = new RegisterUserModel();
            var objMobileInfo = new UserInfo
            {
                Mode = "GCCODE" //Get Client Code
            };
            return objMobileModel.GetClientCodeInfo(objMobileInfo);
        }

        #endregion


        #region "REGISTRATION ADMIN CUSTOMER INFORMATION Utilities"

        /// <summary>
        /// Get the Mobile Number Details Information
        /// </summary>
        /// <returns>Returns the datatable of Mobile Number Information</returns>
        public static int RegisterAdminInfo(UserInfo userInfo, string bankNo)
        {
            var objMobileModel = new RegisterUserModel();
            var objMobileInfo = new UserInfo
            {
                ClientCode = userInfo.ClientCode,
                UserName = userInfo.UserName,
                Password = userInfo.Password,
                UserType = userInfo.UserType,
                UserGroup = userInfo.UserGroup,
                BranchCode = userInfo.BranchCode,
                PushSMS = userInfo.PushSMS,
                Name = userInfo.Name,
                Address = userInfo.Address,
                PIN = userInfo.PIN,
                Status = userInfo.Status,
                IsApproved = userInfo.IsApproved,
                IsRejected = userInfo.IsRejected,
                ContactNumber1 = userInfo.ContactNumber1,
                ContactNumber2 = userInfo.ContactNumber2,
                EmailAddress = userInfo.EmailAddress,
                COC = userInfo.COC,
                CreatedBy = userInfo.CreatedBy,
                BankNo = bankNo,
                Mode = "IAP" //INSERT ADMIN/USER PROFILE
            };
            return objMobileModel.AdminRegisterUserInfo(objMobileInfo);
        }

        public static int RegisterSAdminInfo(UserInfo userInfo)
        {
            var objMobileModel = new SAdminUserModel();
            var objMobileInfo = new UserInfo
            {
                ClientCode = userInfo.ClientCode,
                UserName = userInfo.UserName,
                Password = userInfo.Password,
                UserType = userInfo.UserType,
                UserGroup = userInfo.UserGroup,
                BranchCode = userInfo.BranchCode,
                PushSMS = userInfo.PushSMS,
                Name = userInfo.Name,
                Address = userInfo.Address,
                PIN = userInfo.PIN,
                Status = userInfo.Status,
                IsApproved = userInfo.IsApproved,
                IsRejected = userInfo.IsRejected,
                ContactNumber1 = userInfo.ContactNumber1,
                ContactNumber2 = userInfo.ContactNumber2,
                EmailAddress = userInfo.EmailAddress,
                COC = userInfo.COC,
                Mode = "IAP" //INSERT ADMIN/USER PROFILE
            };
            return objMobileModel.SAdminRegisterUserInfo(objMobileInfo);
        }

        public static int CreateWalletAcInfo(UserInfo userInfo)
        {
            var objMobileModel = new RegisterUserModel();
            var objMobileInfo = new UserInfo
            {
                ClientCode = userInfo.ClientCode,
                Name = userInfo.Name,
                Address = userInfo.Address,
                WalletNumber = userInfo.WalletNumber
            };
            return objMobileModel.CreateWalletAcInfo(objMobileInfo);
        }


        /// <summary>
        /// Get the Mobile Number Details Information
        /// </summary>
        /// <returns>Returns the datatable of Mobile Number Information</returns>
        public static int AgentRegisterCustomerInfo(UserInfo userInfo)
        {
            var objMobileModel = new RegisterUserModel();
            var objMobileInfo = new UserInfo
            {
                Name = userInfo.Name,
                FName = userInfo.FName,
                MName = userInfo.MName,
                LName = userInfo.LName,
                Address = userInfo.Address,
                Gender = userInfo.Gender,
                DOB = DateTime.ParseExact(userInfo.DOB, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),//userInfo.DOB,
                Country = userInfo.Country,
                Nationality = userInfo.Nationality,
                MaritalStatus = userInfo.MaritalStatus,
                FatherName = userInfo.FatherName,
                SpouseName = userInfo.SpouseName,
                GrandFatherName = userInfo.GrandFatherName,
                FatherInlawName = userInfo.FatherInlawName,
                Occupation = userInfo.Occupation,
                PProvince = userInfo.PProvince,
                PDistrict = userInfo.PDistrict,
                //PZone = userInfo.PZone,
                //PDistrict = userInfo.PDistrict,
                PWardNo = userInfo.PWardNo,
                PVDC = userInfo.PVDC,
                PHouseNo = userInfo.PHouseNo,
                PAddress = userInfo.PAddress,
                CProvince = userInfo.CProvince,
                CDistrict = userInfo.CDistrict,
                //CZone = userInfo.CZone,
                //CDistrict = userInfo.CDistrict,
                CVDC = userInfo.CVDC,
                
                CWardNo = userInfo.CWardNo,
                CHouseNo = userInfo.CHouseNo,
                CAddress = userInfo.CAddress,
                Citizenship = userInfo.Citizenship,
                CitizenshipIssueDate = DateTime.ParseExact(userInfo.CitizenshipIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
                
                CitizenshipPlaceOfIssue = userInfo.CitizenshipPlaceOfIssue,
                License = userInfo.License,
                Document = userInfo.Document,
                LicenseIssueDate = userInfo.LicenseIssueDate,
                LicenseExpireDate = userInfo.LicenseExpireDate,
                LicensePlaceOfIssue = userInfo.LicensePlaceOfIssue,
                Passport = userInfo.Passport,
                PassportIssueDate = userInfo.PassportIssueDate,
                PassportExpireDate = userInfo.PassportExpireDate,
                PassportPlaceOfIssue = userInfo.PassportPlaceOfIssue,
                PanNo = userInfo.PanNo,
                
                //Photo1 = userInfo.Photo1,
                //Front = userInfo.Front,
                //Back = userInfo.Back,
                PIN = CustomerUtils.GeneratePin(),
                Password = CustomerUtils.GeneratePassword(),
                Status = userInfo.Status,
                ContactNumber1 = userInfo.ContactNumber1,
                ContactNumber2 = userInfo.ContactNumber2,
                EmailAddress = userInfo.EmailAddress,
                UserName = userInfo.UserName,
                IsApproved = userInfo.IsApproved,
                IsRejected = userInfo.IsRejected,
                UserType = userInfo.UserType,
                BankAccountNumber = userInfo.BankAccountNumber,
                BankNo = userInfo.BankNo,
                BranchCode = userInfo.BranchCode

                //WalletNumber = userInfo.WalletNumber,
                //WBankCode = userInfo.WBankCode,
                //WBranchCode = userInfo.WBranchCode,
                //WIsDefault = userInfo.WIsDefault,
                //AgentId = userInfo.AgentId,

                //IsDefault = userInfo.IsDefault,
                //Mode = "IAGP" //INSERT AGENT PROFILE
            };
            return objMobileModel.AgentRegisterUserInfo(objMobileInfo);
        }





        public static int RegisterUsersInfo(UserInfo userInfo)
        {
            var objMobileModel = new RegisterUserModel();
            var objMobileInfo = new UserInfo
            {
                ClientCode = userInfo.ClientCode,
                Name = userInfo.Name,
                Gender = userInfo.Gender,
                Nationality = userInfo.Nationality,
                Country = userInfo.Country,
                DOB = userInfo.DOB,


                BSDateOfBirth = userInfo.BSDateOfBirth,
                FatherName = userInfo.FatherName,
                MotherName = userInfo.MotherName,
                GrandFatherName = userInfo.GrandFatherName,
                Occupation = userInfo.Occupation,
                MaritalStatus = userInfo.MaritalStatus,
                SpouseName = userInfo.SpouseName,
                FatherInlawName = userInfo.FatherInlawName,
                //start milayako 02

                PStreet = userInfo.PStreet,
                PProvince = userInfo.PProvince,
                PDistrictID = userInfo.PDistrictID,
                PWardNo = userInfo.PWardNo,
                PMunicipalityVDC = userInfo.PMunicipalityVDC,
                PHouseNo = userInfo.PHouseNo,

                CStreet = userInfo.CStreet,
                CProvince = userInfo.CProvince,
                CDistrictID = userInfo.CDistrictID,
                CMunicipalityVDC = userInfo.CMunicipalityVDC,
                CWardNo = userInfo.CWardNo,
                CHouseNo = userInfo.CHouseNo,
                //end milayako 02

                Address = userInfo.Address, //current address
                PIN = userInfo.PIN,
                Status = userInfo.Status,
                ContactNumber1 = userInfo.ContactNumber1,
                ContactNumber2 = userInfo.ContactNumber2,
                EmailAddress = userInfo.EmailAddress,

                Document = userInfo.Document,
                Citizenship = userInfo.Citizenship,
                CitizenshipIssueDate = userInfo.CitizenshipIssueDate,
                BSCitizenshipIssueDate = userInfo.BSCitizenshipIssueDate,
                CitizenshipPlaceOfIssue = userInfo.CitizenshipPlaceOfIssue,


                License = userInfo.License,
                LicenseIssueDate = userInfo.LicenseIssueDate,
                BSLicenseIssueDate = userInfo.BSLicenseIssueDate,
                LicenseExpireDate = userInfo.LicenseExpireDate,
                BSLicenseExpireDate = userInfo.BSLicenseExpireDate,
                LicensePlaceOfIssue = userInfo.LicensePlaceOfIssue,




                Passport = userInfo.Passport,
                PassportIssueDate = userInfo.PassportIssueDate,
                PassportExpireDate = userInfo.PassportExpireDate,
                PassportPlaceOfIssue = userInfo.PassportPlaceOfIssue,
                //CitizenshipIssueDate = DateTime.ParseExact(userInfo.CitizenshipIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                //                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
                //LicenseIssueDate = DateTime.ParseExact(userInfo.LicenseIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                //                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
                //LicenseExpireDate = DateTime.ParseExact(userInfo.LicenseExpireDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                //                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
                //PassportIssueDate = DateTime.ParseExact(userInfo.PassportIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                //                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
                //PassportExpireDate = DateTime.ParseExact(userInfo.PassportExpireDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                //                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),

                PassportImage= userInfo.PassportImageName,
                FrontImage= userInfo.FrontImageName,
                BackImage= userInfo.BackImageName,


                UserName = userInfo.UserName,
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
                ProfileCode=userInfo.ProfileCode,
                TxnAccounts=userInfo.TxnAccounts,
                AdminUserName=userInfo.AdminUserName,
                AdminBranch=userInfo.AdminBranch,
                Transaction=userInfo.Transaction,
                DateRange=userInfo.DateRange,
                StartDate=userInfo.StartDate,
                EndDate=userInfo.EndDate,
                LimitType=userInfo.LimitType,
                TransactionLimit = userInfo.TransactionLimit,
                TransactionCount=userInfo.TransactionCount,
                TransactionLimitMonthly=userInfo.TransactionLimitMonthly,
                TransactionLimitDaily=userInfo.TransactionLimitDaily,
                retrievalReference = userInfo.retrievalReference,
                Mode = "IBUP" //INSERT USERS PROFILE
            };
            return objMobileModel.RegisterUsersInfo(objMobileInfo);
        }



        public static int RegisterMerchantInfo(UserInfo userInfo,string MerchantCategory)
        {
            var objMobileModel = new RegisterUserModel();
            var objMobileInfo = new UserInfo
            {
                ClientCode = userInfo.ClientCode,
                Name = userInfo.Name,
                Address = userInfo.Address,
                Status = userInfo.Status,
                ContactNumber1 = userInfo.ContactNumber1,
                ContactNumber2 = userInfo.ContactNumber2,
                EmailAddress = userInfo.EmailAddress,
                UserName = userInfo.UserName,
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
        
               
            };
            return objMobileModel.RegisterMerchantInfo(objMobileInfo, MerchantCategory);
        }
        #endregion


        #region Customer Self Registration

        public static int CustomerSelfReg(CustomerSRInfo srInfo)
        {
            var objMobileModel = new RegisterUserModel();
            var objMobileInfo = new CustomerSRInfo
            {
                //Name = srInfo.Name,
                FName = srInfo.FName,
                MName = srInfo.MName,
                LName = srInfo.LName,
                //Address = srInfo.Address,
                Gender = srInfo.Gender,
                DOB = DateTime.ParseExact(srInfo.DOB, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),//userInfo.DOB,
                Country = srInfo.Country,
                Nationality = srInfo.Nationality,
                MaritalStatus = srInfo.MaritalStatus,
                FatherName = srInfo.FatherName,
                MotherName = srInfo.MotherName,
                SpouseName = srInfo.SpouseName,
                GrandFatherName = srInfo.GrandFatherName,
                FatherInlawName = srInfo.FatherInlawName,
                Occupation = srInfo.Occupation,
                PProvince = srInfo.PProvince,
                PDistrict = srInfo.PDistrict,
                //PZone = srInfo.PZone,
                //PDistrict = srInfo.PDistrict,
                PWardNo = srInfo.PWardNo,
                PVDC = srInfo.PVDC,
                PHouseNo = srInfo.PHouseNo,
                //PAddress = srInfo.PAddress,
                CProvince = srInfo.CProvince,

                CDistrict = srInfo.CDistrict,
                //CZone = srInfo.CZone,
                //CDistrict = srInfo.CDistrict,
                CVDC = srInfo.CVDC,
                
                CWardNo = srInfo.CWardNo,
                CHouseNo = srInfo.CHouseNo,
                //CAddress = srInfo.CAddress,
                Citizenship = srInfo.Citizenship,
                CitizenshipIssueDate = DateTime.ParseExact(srInfo.CitizenshipIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),

                CitizenshipPlaceOfIssue = srInfo.CitizenshipPlaceOfIssue,
                License = srInfo.License,
                //LicenseIssueDate = DateTime.ParseExact(userInfo.LicenseIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                //                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
                LicenseIssueDate = srInfo.LicenseIssueDate,
                LicenseExpireDate = srInfo.LicenseExpireDate,
                LicensePlaceOfIssue = srInfo.LicensePlaceOfIssue,
                Passport = srInfo.Passport,
                PassportIssueDate = srInfo.PassportIssueDate,
                PassportExpireDate = srInfo.PassportExpireDate,
                PassportPlaceOfIssue = srInfo.PassportPlaceOfIssue,
                PanNo = srInfo.PanNo,
                Document = srInfo.Document,              
                PIN = CustomerUtils.GeneratePin(),
                Password = CustomerUtils.GeneratePassword(),
                Status = srInfo.Status,
                //ContactNumber1 = srInfo.ContactNumber1,
                //ContactNumber2 = srInfo.ContactNumber2,
                EmailAddress = srInfo.EmailAddress,
                UserName = srInfo.UserName,
                //IsApproved = srInfo.IsApproved,
                IsRejected = srInfo.IsRejected,
                UserType = srInfo.UserType,
                OTPCode = srInfo.OTPCode,
                Source = srInfo.Source,
                //BankAccountNumber = srInfo.BankAccountNumber,
                //BankNo = srInfo.BankNo,
                BranchCode = srInfo.BranchCode,  
                FrontImage = srInfo.FrontImage,
                BackImage = srInfo.BackImage,
                PassportImage = srInfo.PassportImage,
                Mode = "ISR" //INSERT SELF REGISTRATION PROFILE
            };
            return objMobileModel.CustomerSelfRegInfo(objMobileInfo);
        }
        #endregion

    }
}