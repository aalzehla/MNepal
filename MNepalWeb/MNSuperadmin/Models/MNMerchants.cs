using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace MNSuperadmin.Models
{
    public class MNMerchants
    {
        public string MID { get; set; }

        public string ParentID { get; set; }

        public string catid { get; set; }
        public string Name { get; set; }
        public string mname { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Mode { get; set; }


        [Display(Name = "Business Name")]
        public string BusinessName { get; set; }
        [Display(Name = "Registration Number")]
        public string RegistrationNumber { get; set; }
        [Display(Name = "VAT Number")]
        public string VATNumber { get; set; }
        [Display(Name = "Landline Number")]
        public string LandlineNumber { get; set; }
        [Display(Name = "Merchant Type")]
        public string MerchantType { get; set; }
        [Display(Name = "Website Name")]
        public string WebsiteName { get; set; }

        [Display(Name = "CitizenshipIssueDate")]
        public string CitizenshipIssueDate { get; set; }

        [Display(Name = "BSCitizenshipIssueDate")]
        public string BSCitizenshipIssueDate { get; set; }

        [Display(Name = "CitizenshipIssueDate1")]
        public string CitizenshipIssueDate1 { get; set; }

        [Display(Name = "DateFormat")]
        public string DateFormat { get; set; }

        [Display(Name = "IDIssueDate")]
        public string IDIssueDate { get; set; }

        public string PanNo { get; set; }

        public string Photo1 { get; set; }
       
        public string ContactNumber2
        {
            get;
            set;
        }

        public string PAddress { get; set; }
        
        public string PProvince { get; set; }
        
        public string PDistrictID { get; set; }
       
        public string PDistrict { get; set; }

        public string PMunicipalityVDC { get; set; }

        public string PVDC { get; set; }
        public string PHouseNo { get; set; }
        public string PWardNo { get; set; }
        public string PStreet { get; set; }

        public string Citizenship { get; set; }

        public string CitizenshipPlaceOfIssue { get; set; }

        public string Document { get; set; }
        public HttpPostedFileBase Front { get; set; }
        public HttpPostedFileBase Back { get; set; }

        public HttpPostedFileBase PassportPhoto { get; set; }

        public string FrontImage { get; set; }

        public string BackImage { get; set; }

        public string PassportImage { get; set; }

        public string FrontImageName { get; set; }

        public string BackImageName { get; set; }

        public string PassportImageName { get; set; }

        public string ClientCode
        {
            get;
            set;
        }
        [Display(Name = "First Name")]
        public string FName { get; set; }

        public string ApprovedBy { get; set; }

        public string ApprovedDate { get; set; } //Agent Modification Approve//
        //edit start
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        //edit end 
        [Display(Name = "Middle Name")]
        public string MName { get; set; }
        [Display(Name = "Last Name")]
        public string LName { get; set; }

        public string Gender { get; set; }

        public List<SelectListItem> Profiles { get; set; }

        public List<SelectListItem> BranchCodes { get; set; }

        [Display(Name = "Branch Name")]
        public string BranchName
        {
            get;
            set;
        }


        public string ProfileName
        {
            get;
            set;
        }

        public string AProfileName
        {
            get;
            set;
        }


        public string ProfileCode
        {
            get;
            set;
        }


        public string ProfileDesc
        {
            get;
            set;
        }

        
        public string Address
        {
            get;
            set;
        }
        public string HasKYC
        {
            get;
            set;
        }


        public string PIN
        {
            get;
            set;
        }
        public string Status
        {
            get;
            set;
        }
        public string BlockStatus
        {
            get;
            set;
        }

        public string BlockRemarks
        {
            get;
            set;
        }
        public string ID
        {
            get;
            set;
        }

        [Display(Name = "Mobile Number")]
        public string ContactNumber1
        {
            get;
            set;
        }

        [Display(Name = "Email Address")]
        public string EmailAddress
        {
            get;
            set;
        }

        [Display(Name = "User Name")]
        public string UserName
        {
            get;
            set;
        }

        public string Password
        {
            get;
            set;
        }

        [Display(Name = "User Type")]
        public string UserType
        {
            get;
            set;
        }

        [Display(Name = "User Group")]
        public string UserGroup
        {
            get;
            set;
        }

        public string IsApproved
        {
            get;
            set;
        }

        public string IsRejected
        {
            get;
            set;
        }

        public string IsModified
        {
            get;
            set;
        }

        [Display(Name = "Wallet Number")]
        public string WalletNumber
        {
            get;
            set;
        }
        public string Amount
        {
            get;
            set;
        }

        [Display(Name = "Bank Account Number")]
        public string BankAccountNumber
        {
            get;
            set;
        }

        [Display(Name = "Bank No")]
        public string BankNo
        {
            get;
            set;
        }
        [Display(Name = "Branch Code")]
        public string BranchCode
        {
            get;
            set;
        }

        public string PushSMS
        {
            get;
            set;
        }

        public string COC
        {
            get;
            set;
        }

        public int ReturnValue
        {
            get;
            set;
        }

        public int RegIDOut
        {
            get;
            set;
        }


        public string WBankCode
        {
            get;
            set;
        }

        public string WBranchCode
        {
            get;
            set;
        }
        public string WIsDefault
        {
            get;
            set;
        }

        public string IsDefault
        {
            get;
            set;
        }
        public string AgentId
        {
            get;
            set;
        }

        public string OPassword
        {
            get;
            set;
        }

        public string OPIN
        {
            get;
            set;
        }

        [Display(Name = "New MobileNo")]
        public string NewMobileNo
        {
            get;
            set;
        }



        public string ServiceProvider
        {
            get;
            set;
        }


        public string BlockCustomer
        {
            get;
            set;
        }

        public string ClientStatus
        {
            get;
            set;
        }

        public string AcNumber
        {
            get;
            set;
        }

        public string Alias
        {
            get;
            set;
        }

        public string AcOwner
        {
            get;
            set;
        }

        public string IsPrimary
        {
            get;
            set;
        }

        public string AcType
        {
            get;
            set;
        }

        public string TxnEnabled
        {
            get;
            set;
        }

        public string UserBranchCode
        {
            get;
            set;
        }

        public string UserBranchName
        {
            get;
            set;
        }

        public string TBranchCode
        {
            get;
            set;
        }

        public IEnumerable<TransactionInfo> Trans { get; set; }


        public string WalletName { get; set; }
        public string WalletCode { get; set; }
        public string WardNumber { get; set; }
        public string District { get; set; }

        public string Province { get; set; }
        public string Address1 { get; set; }
        public string TxnAccounts { get; set; }
        public string Remarks { get; set; }
        public string AdminUserName { get; set; }
        public string AdminBranch { get; set; }
        public string ModifyingBranch { get; set; }
        public string ModifyingAdmin { get; set; }

        /*for SuperAdmin */
        public string SuperAdminUserName { get; set; }

        public string RejectedBy { get; set; }

        /*For Maker checker modification user*/
        public List<MNMakerChecker> MakerChecker { get; set; }
        public List<InMemMNTransactionAccount> MakerCheckerTransAccounts { get; set; }

        /*For Individual Transaction Limit*/
        public string Transaction { get; set; }

        [Display(Name = "Date Range")]
        public string DateRange { get; set; }

        public string LimitType { get; set; }
        public string TransactionLimit { get; set; }
        public string TransactionCount { get; set; }
        public string TransactionLimitMonthly { get; set; }
        public string TransactionLimitDaily { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string SelfRegistered { get; set; }

        public string GoodBaln { get; set; }

        public string CommissionId { get; set; }
        public string FeeID { get; set; }
        public string TieredStart { get; set; }
        public string TieredEnd { get; set; }
        public string MinAmt { get; set; }
        public string MaxAmt { get; set; }
        public string Percentage { get; set; }
        public string FlatFee { get; set; }
        public string FeeType { get; set; }

        public string MerchantCategory { get; set; }

        public HttpPostedFileBase RegCertificatePhoto { get; set; }
        public HttpPostedFileBase TaxClearFront { get; set; }
        public HttpPostedFileBase TaxClearBack { get; set; }

        public string RegCertificateImage { get; set; }

        public string TaxClearFrontImage { get; set; }

        public string TaxClearBackImage { get; set; }

        public string RegCertificatePhotoName { get; set; }

        public string TaxClearFrontName { get; set; }

        public string TaxClearBackName { get; set; }
    }
    
    public class MNClientKYC
    {

        public string Gender { get; set; }
        public string DateOfBirth { get; set; }
        public string BSDateOfBirth { get; set; }
        public string Nationality { get; set; }
        public string Country { get; set; }
        public string Occupation { get; set; }
        public string MaritalStatus { get; set; }
        public string SpouseName { get; set; }
        public string FatherInLaw { get; set; }
        public string FathersName { get; set; }
        public string MothersName { get; set; }
        public string GFathersName { get; set; }
        public string PProvince { get; set; }
        public string PDistrict { get; set; }
        public string PMunicipalityVDC { get; set; }
        public string PHouseNo { get; set; }
        public string PWardNo { get; set; }
        public string PStreet { get; set; }
        public string CProvince { get; set; }
        public string CDistrict { get; set; }
        public string CMunicipalityVDC { get; set; }
        public string CHouseNo { get; set; }
        public string CWardNo { get; set; }
        public string CStreet { get; set; }
        public string LicenseExpiryDate { get; set; }
        public string BSLicenseExpiryDate { get; set; }
        public string PassportExpiryDate { get; set; }





        [Display(Name = "CitizenshipIssueDate")]
        public string CitizenIssueDate { get; set; }
        public string CitizenshipIssueDate { get; set; }
        [Display(Name = "BSCitizenshipIssueDate")]
        public string BSCitizenIssueDate { get; set; }
        public string BSCitizenshipIssueDate { get; set; }
        public string CitizenshipNo { get; set; }
        public string Citizenship { get; set; }
        public string CitizenPlaceOfIssue { get; set; }


        public string LicenseNo { get; set; }
        public string LicensePlaceOfIssue { get; set; }
        public string LicenseIssueDate { get; set; }
        public string LicenseExpireDate { get; set; }
        public string BSLicenseIssueDate { get; set; }
        public string BSLicenseExpireDate { get; set; }

        public string PassportNo { get; set; }
        public string PassportPlaceOfIssue { get; set; }
        public string PassportIssueDate { get; set; }
        public string PassportExpireDate { get; set; }



        public string CitizenshipPlaceOfIssue { get; set; }
        public string PANNumber { get; set; }
        public string PanNo { get; set; }
        public string FName { get; set; }
        
        public string MName { get; set; }

        public string LName { get; set; }

        public string ClientCode
        {
            get;
            set;
        }

    }

    public class MNMerchantKYCDoc
    {
        public string ClientCode { get; set; }
        
        public string Document { get; set; }

        public string BackImage { get; set; }

        public string PassportImage { get; set; }

        public string FrontImage { get; set; }

        public string RegCertImage { get; set; }

        public string PassportImageName { get; set; }
      
        public string MerchantCategory { get; set; }

        public HttpPostedFileBase RegCertificatePhoto { get; set; }
        public HttpPostedFileBase TaxClearFront { get; set; }
        public HttpPostedFileBase TaxClearBack { get; set; }

        public HttpPostedFileBase Front { get; set; }
        public HttpPostedFileBase Back { get; set; }

        public HttpPostedFileBase PassportPhoto { get; set; }

        public string RegCertificateImage { get; set; }

        public string TaxClearFrontImage { get; set; }

        public string TaxClearBackImage { get; set; }

        public string RegCertificatePhotoName { get; set; }

        public string TaxClearFrontName { get; set; }

        public string TaxClearBackName { get; set; }
    }

}