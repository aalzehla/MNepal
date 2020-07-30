using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MNepalWeb.Models
{
    public class UserProfilesInfo
    {


        public string CanARegistrationRejected
        {
            get;
            set;
        }

        public string CanAModificationRejected
        {
            get;
            set;
        }


        public string UPID
        {
            get;
            set;
        }

        public string MenuAssignId
        {
            get;
            set;
        }

        public string HierarchyList
        {
            get;
            set;
        }

        public string ClientCode
        {
            get;
            set;
        }

        public string UserProfileStatus
        {
            get;
            set;
        }
        
        [Display(Name = "Role")]
        public string ProfileName
        {
            get;
            set;
        }

        [Display(Name = "RoleDesc")]
        public string ProfileDesc
        {
            get;
            set;
        }

        public string ProfileGroup
        {
            get;
            set;
        }

        public string Mode
        {
            get;
            set;
        }

        public int RegIdOut
        {
            get;
            set;
        }

        /****************************************************/

        public string CanAdmin
        {
            get;
            set;
        }

        public string CanAdminReg
        {
            get;
            set;
        }

        public string CanAdminModify
        {
            get;
            set;
        }

        public string CanAPwdReset
        {
            get;
            set;
        }
        
        
        //EDTI start
        public string CanAARegistration
        {
            get;
            set;
        }

        public string CanAAModified
        {
            get;
            set;
        }

        public string CanAAPwdReset
        {
            get;
            set;
        }

        //edit end
        /****************************************************/

        public string CanCust
        {
            get;
            set;
        }

        public string CanCReg { get; set; }

        public string CanCApprove { get; set; }

        public string @CanCModify { get; set; }

        public string CApproveModify { get; set; }

        public string CStatus { get; set; }

        public string CRejectedList { get; set; }

        public string CPinReset { get; set; }

        public string CApprovePinReset { get; set; }

        public string CanCDelete { get; set; }

        public string CApproveDelete { get; set; }

        public string CUnBlockService { get; set; }

        public string CApproveUnblock { get; set; }

        public string CCharge { get; set; }

        public string CApproveRenewCharge { get; set; }


        /****************************************************/

        public string BranchSetup { get; set; }

        public string BranchRegistration { get; set; }
        public string BranchModification { get; set; }
        public string BranchStatus { get; set; }

        /****************************************************/

        public string AcTypeSetup { get; set; }
        public string AcTypeSetupm { get; set; }

        //****************************************************

        public string MNepalPay { get; set; }

        public string ManageService { get; set; }
        public string CheckStatus { get; set; }
        public string Cancellation { get; set; }

        /****************************************************/

        public string UserSetup { get; set; }

        public string UCreateAdminProfile { get; set; }

        public string UModifyAdminProfile { get; set; }

        public string UCreateCustProfile { get; set; }

        public string UModifyCustProfile { get; set; }

        /****************************************************/

        public string Request { get; set; }

        public string RChequeBookList { get; set; }

        public string RStmtList { get; set; }

        public string RComplainList { get; set; }

        public string RRecommandationList { get; set; }


        /****************************************************/

        public string Messaging { get; set; }

        public string MUncastMsg { get; set; }

        public string MBulkMsg { get; set; }

        public string MResendMsg { get; set; }

        /****************************************************/

        public string Report { get; set; }

        public string ReSmsInOut { get; set; }

        public string ReRegisteredCustomer { get; set; }

        public string ReMerchantPayment { get; set; }

        public string ReCardRequest { get; set; }

        public string ReAdminDetail { get; set; }

        public string ReChequeBookRequest { get; set; }

        public string ReAdminActivities { get; set; }

        public string ReMNepalPayTran { get; set; }

        public string ReServicePayments { get; set; }

        public string ReCharge { get; set; }

        public string ReStmtRequest { get; set; }

        public string ReCustActivities { get; set; }
        /*ReMNepalPayCancellation*/
        public string ReMNepalPayCancel { get; set; }

        public string ReTransactionDetail { get; set; }

        public string ReMNepalPayReport { get; set; }

        /*New Added Reports*/
        public string ReCusActivityReport { get; set; }

        public string ReCusLogReport { get; set; }

        public string ReMerchantsReport { get; set; }

        public string ReTranSummaryReport { get; set; }

        public string ReAdminDetailsReport { get; set; }

        /****************************************************/

        public string Payee { get; set; }

        public string PMerchantRegister { get; set; }
        /*PCommissionPartnerModify*/
        public string PComPartnerModify { get; set; }

        /*PModifyCommissionSlab*/
        public string PModifyComSlab { get; set; }

        public string PMerchantModification { get; set; }

        public string PMerchantDetails { get; set; }

        public string PCommissionSlabDetail { get; set; }
        /*PCommisionPartnerRegistration*/
        public string PComPartnerRegister { get; set; }
        /*PAddCommissionSlab*/
        public string PAddComSlab { get; set; }

        /****************************************************/

        public string Setting { get; set; }

        public string SKeywordSetting { get; set; }

        public string SModifyMsg { get; set; }

        public string SCreateStaticKeyword { get; set; }

        public string SChangePwd { get; set; }

        public string SModifyStaticKeyword { get; set; }


    }
}