using ThailiMerchantApp.Models;
using ThailiMerchantApp.UserModels;
using System;
using System.Data;
using System.Globalization;

namespace ThailiMerchantApp.Utilities
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
                 Mode = "GCMMN" //Get Check Mobile No
               // Mode = "GCAMN" //Get Check Mobile No of agent
                //Mode="GCUMN" //get check cust mobile no 

            };
            return objMobileModel.GetMobileInfo(objMobileInfo);
        }
        public static DataTable GetCheckAgentNo(string mobileNo)
        {
            var objMobileModel = new RegisterUserModel();
            var objMobileInfo = new UserInfo
            {
                ContactNumber1 = mobileNo,
                Mode = "GCAMN" //Get Check Mobile No of agent

            };
            return objMobileModel.GetMobileInfo(objMobileInfo);
        }
        public static DataTable GetCheckMerchantNo(string mobileNo)
        {
            var objMobileModel = new RegisterUserModel();
            var objMobileInfo = new UserInfo
            {
                ContactNumber1 = mobileNo, 
                Mode = "GCMMN" //Get Check Mobile No of agent
                  
            };
            return objMobileModel.GetMobileInfo(objMobileInfo);
        }


        #endregion
        //start milayako

        #region "DATATABLE Checking Agent Mobile Number Utilities"

        /// <summary>
        /// Get the Agent Mobile Number Details Information
        /// </summary>
        /// <returns>Returns the datatable of Agent Mobile Number Information</returns>
        public static DataTable GetCheckAgentMobileNo(string mobileNo)
        {
            var objMobileModel = new RegisterUserModel();
            var objMobileInfo = new UserInfo
            {
                ContactNumber1 = mobileNo,
                Mode = "GCAMN" //Get Check Agent Mobile No
            };
            return objMobileModel.GetMobileInfo(objMobileInfo);
        }


        #endregion
        //end milayako

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
        public static int RegisterAdminInfo(UserInfo userInfo)
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
                Mode = "IAP" //INSERT ADMIN/USER PROFILE
            };
            return objMobileModel.AdminRegisterUserInfo(objMobileInfo);
        }

        //public static int RegisterSAdminInfo(UserInfo userInfo)
        //{
        //    var objMobileModel = new SAdminUserModel();
        //    var objMobileInfo = new UserInfo
        //    {
        //        ClientCode = userInfo.ClientCode,
        //        UserName = userInfo.UserName,
        //        Password = userInfo.Password,
        //        UserType = userInfo.UserType,
        //        UserGroup = userInfo.UserGroup,
        //        BranchCode = userInfo.BranchCode,
        //        PushSMS = userInfo.PushSMS,
        //        Name = userInfo.Name,
        //        Address = userInfo.Address,
        //        PIN = userInfo.PIN,
        //        Status = userInfo.Status,
        //        IsApproved = userInfo.IsApproved,
        //        IsRejected = userInfo.IsRejected,
        //        ContactNumber1 = userInfo.ContactNumber1,
        //        ContactNumber2 = userInfo.ContactNumber2,
        //        EmailAddress = userInfo.EmailAddress,
        //        COC = userInfo.COC,
        //        Mode = "IAP" //INSERT ADMIN/USER PROFILE
        //    };
        //    return objMobileModel.SAdminRegisterUserInfo(objMobileInfo);
        //}

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
                //LicenseIssueDate = DateTime.ParseExact(userInfo.LicenseIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                //                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
                LicenseIssueDate = userInfo.LicenseIssueDate,
                LicenseExpireDate = userInfo.LicenseExpireDate,
                LicensePlaceOfIssue = userInfo.LicensePlaceOfIssue,
                Passport = userInfo.Passport,
                PassportIssueDate = userInfo.PassportIssueDate,
                PassportExpireDate = userInfo.PassportExpireDate,
                PassportPlaceOfIssue = userInfo.PassportPlaceOfIssue,
                PanNo = userInfo.PanNo,
                Document = userInfo.Document,
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

        
        public static int CustomerRegAppRegisterCustomerInfo(UserInfo userInfo)
        {
            var objMobileModel = new RegisterUserModel();
            var objMobileInfo = new UserInfo
            {
                Name = userInfo.Name,
                Address = userInfo.Address,
                FName = userInfo.FName,
                MName = userInfo.MName,
                LName = userInfo.LName,
                Gender = userInfo.Gender,
                DOB = DateTime.ParseExact(userInfo.DOB, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),//userInfo.DOB,

                Nationality = userInfo.Nationality,
                MaritalStatus = userInfo.MaritalStatus,
                SpouseName = userInfo.SpouseName,
                FatherName = userInfo.FatherName,
                MotherName = userInfo.MotherName,
                GrandFatherName = userInfo.GrandFatherName,
                Occupation = userInfo.Occupation,
                PProvince = userInfo.PProvince,
                PDistrict = userInfo.PDistrict,
                PWardNo = userInfo.PWardNo,
                PVDC = userInfo.PVDC,
                PHouseNo = userInfo.PHouseNo,
                PStreet = userInfo.PStreet,
                CProvince = userInfo.CProvince,
                CDistrict = userInfo.CDistrict,
                CVDC = userInfo.CVDC,
                CWardNo = userInfo.CWardNo,
                CHouseNo = userInfo.CHouseNo,
                CStreet = userInfo.CStreet,
                Citizenship = userInfo.Citizenship,
                CitizenshipIssueDate = DateTime.ParseExact(userInfo.CitizenshipIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),

                CitizenshipPlaceOfIssue = userInfo.CitizenshipPlaceOfIssue,
                License = userInfo.License,
                //LicenseIssueDate = DateTime.ParseExact(userInfo.LicenseIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                //                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
                LicenseIssueDate = userInfo.LicenseIssueDate,
                LicenseExpireDate = userInfo.LicenseExpireDate,
                LicensePlaceOfIssue = userInfo.LicensePlaceOfIssue,
                Passport = userInfo.Passport,
                PassportIssueDate = userInfo.PassportIssueDate,
                PassportExpireDate = userInfo.PassportExpireDate,
                PassportPlaceOfIssue = userInfo.PassportPlaceOfIssue,
                PanNo = userInfo.PanNo,
                Document = userInfo.Document,
                PIN = CustomerUtils.GeneratePin(),
                Password = CustomerUtils.GeneratePassword(),
                Status = userInfo.Status,
                UserName = userInfo.UserName,
                IsApproved = userInfo.IsApproved,
                IsRejected = userInfo.IsRejected,
                UserType = userInfo.UserType,
                FrontImage = userInfo.FrontImage,
                BackImage = userInfo.BackImage,
                PassportImage = userInfo.PassportImage


                //BankAccountNumber = userInfo.BankAccountNumber,
                //BankNo = userInfo.BankNo,
                //BranchCode = userInfo.BranchCode

                //WalletNumber = userInfo.WalletNumber,
                //WBankCode = userInfo.WBankCode,
                //WBranchCode = userInfo.WBranchCode,
                //WIsDefault = userInfo.WIsDefault,
                //AgentId = userInfo.AgentId,

                //IsDefault = userInfo.IsDefault,
                //Mode = "IAGP" //INSERT AGENT PROFILE
            };
            return objMobileModel.CustomerRegAppRegisterUserInfo(objMobileInfo);
        }

        



        public static int RegisterUsersInfo(UserInfo userInfo)
        {
            var objMobileModel = new RegisterUserModel();
            var objMobileInfo = new UserInfo
            {
                ClientCode = userInfo.ClientCode,
                Name = userInfo.Name,
                Address = userInfo.Address,

                //start milayako3
                WardNumber = userInfo.WardNumber,
                Province = userInfo.Province,
                District = userInfo.District,
                //end milayako3

                PIN = userInfo.PIN,
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
                Mode = "IUP" //INSERT USERS PROFILE
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
                DOB = DateTime.ParseExact(srInfo.DOB, "yyyy-MM-dd", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),//userInfo.DOB DateOfBirth in A.D.,
                //BSDateOfBirth = DateTime.ParseExact(srInfo.BSDateOfBirth, "yyyy-MM-dd", CultureInfo.InvariantCulture)
                //                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture), //DateOfBirth in B.S.
                BSDateOfBirth = srInfo.BSDateOfBirth,


                //DateADBS = srInfo.DateADBS,
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
                PStreet = srInfo.PStreet,
                //PAddress = srInfo.PAddress,
                CProvince = srInfo.CProvince,

                CDistrict = srInfo.CDistrict,
                //CZone = srInfo.CZone,
                //CDistrict = srInfo.CDistrict,
                CVDC = srInfo.CVDC,
                
                CWardNo = srInfo.CWardNo,
                CHouseNo = srInfo.CHouseNo,
                CStreet = srInfo.CStreet,
                //CAddress = srInfo.CAddress,
                Citizenship = srInfo.Citizenship,
                CitizenshipIssueDate = srInfo.CitizenshipIssueDate,
                BSCitizenshipIssueDate = srInfo.BSCitizenshipIssueDate,

                CitizenshipPlaceOfIssue = srInfo.CitizenshipPlaceOfIssue,
                License = srInfo.License,
                LicenseIssueDate = srInfo.LicenseIssueDate,
                BSLicenseIssueDate = srInfo.BSLicenseIssueDate,
                LicenseExpireDate = srInfo.LicenseExpireDate,
                BSLicenseExpireDate = srInfo.BSLicenseExpireDate,
                LicensePlaceOfIssue = srInfo.LicensePlaceOfIssue,
                Passport = srInfo.Passport,
                PassportIssueDate = srInfo.PassportIssueDate,
                PassportExpireDate = srInfo.PassportExpireDate,
                PassportPlaceOfIssue = srInfo.PassportPlaceOfIssue,
                PanNo = srInfo.PanNo,
                Document = srInfo.Document,              
                PIN = srInfo.PIN,
                Password = srInfo.Password,
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
                FrontImage = srInfo.FrontImageName,
                BackImage = srInfo.BackImageName,
                PassportImage = srInfo.PassportImageName,
                // Mode = "ISR" //INSERT SELF REGISTRATION PROFILE

                //start 
                Mode = "AISR"


                //end
            };
            return objMobileModel.CustomerSelfRegInfo(objMobileInfo);
        }
        #endregion


        //FOR EBanking
        #region EBanking
        public static int EBanking(CustomerSRInfo srInfo)
        {
            var objMobileModel = new RegisterUserModel();
            var objMobileInfo = new CustomerSRInfo
            {
                //UserName = srInfo.UserName,
                //Password = srInfo.Password,
                //PIN = srInfo.PIN,
                //UserType = srInfo.UserType,
                //OTPCode = srInfo.OTPCode,
                //Source = srInfo.Source,
                ClientCode = srInfo.ClientCode,
                PaymentReferenceNumber = srInfo.PaymentReferenceNumber,
                ItemCode = srInfo.ItemCode,
                Amount = srInfo.Amount,
                EBDate = srInfo.EBDate,

                Mode = "QISREB" //INSERT SELF REGISTRATION PROFILE
            };
            return objMobileModel.EBanking(objMobileInfo);
        }
        #endregion

        #region "DATATABLE Checking User Mobile Number Utilities"

        /// <summary>
        /// Get the Mobile Number Details Information
        /// </summary>
        /// <returns>Returns the datatable of Mobile Number Information</returns>
        public static DataTable GetCheckUserMobileNo(string mobileNo)
        {
            var objMobileModel = new RegisterUserModel();
            var objMobileInfo = new UserInfo
            {
                ContactNumber1 = mobileNo,
                Mode = "GCUMN" //Get Check Mobile No
            };
            return objMobileModel.GetMobileInfo(objMobileInfo);
        }


        #endregion
    }
}