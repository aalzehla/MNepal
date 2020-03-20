using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using MNepalWeb.Models;
using MNepalWeb.Utilities;
using System.Web.SessionState;
using MNepalWeb.ViewModel;
using MNepalWeb.Helper;

namespace MNepalWeb.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class UserRoleSetupController : Controller {
        ///<summary>
        /// Manage Admin Profiles
        ///</summary>
        ///
        // GET: UserSetup
        public ActionResult Index()
        {
            //string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            //Check Role link start          
            string methodlink = System.Reflection.MethodBase.GetCurrentMethod().Name;
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkIndexRole(methodlink, clientCode, this.ControllerContext.RouteData.Values["controller"].ToString());
            //Check Role link end
            TempData["userType"] = userType;

            if (TempData["userType"] != null && checkRole)
            {
                ViewData["userType"] = TempData["userType"];
                ViewBag.UserType = TempData["userType"];
                ViewBag.Name = name;

                if (TempData["admin_messsage"] != null)
                {
                    ViewData["admin_messsage"] = TempData["admin_messsage"];
                    ViewData["message_class"] = TempData["message_class"];
                }


                List<UserProfilesInfo> userProfilesList = new List<UserProfilesInfo>();
                DataTable dtUserProfile = UserProfileUtils.GetAdminProfile();

                foreach (DataRow row in dtUserProfile.Rows)
                {
                    UserProfilesInfo userobj = new UserProfilesInfo
                    {
                        UPID = row["ProfileCode"].ToString(),
                        ProfileName = row["ProfileName"].ToString(),
                        ProfileDesc = row["ProfileDesc"].ToString(),
                        UserProfileStatus = row["ProfileStatus"].ToString(),
                        MenuAssignId = row["ProfileGroup"].ToString()
                    };

                    userProfilesList.Add(userobj);
                }
                return View(userProfilesList);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        /// <summary>
        /// Create Admin Profile
        /// </summary>
        /// <returns></returns>
        /// 
        // GET: UserSetup/CreateAdminProfile
        [HttpGet]
        public ActionResult CreateAdminProfileOLD()
        {
            //string userName = (string)Session["LOGGED_USERNAME"];
            //string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                ViewData["userType"] = TempData["userType"];
                ViewBag.UserType = TempData["userType"];
                ViewBag.Name = name;

                if (TempData["adminprofile_messsage"] != null)
                {
                    ViewData["adminprofile_messsage"] = TempData["adminprofile_messsage"];
                    ViewData["message_class"] = TempData["message_class"];
                }

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        /// <summary>
        /// Create Admin Profile Inserted
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        /// 
        // POST: UserSetup/CreateAdminProfile
        [HttpPost]
        public ActionResult CreateAdminProfileOLD(FormCollection collection)
        {
            //string userName = (string)Session["LOGGED_USERNAME"];
            //string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            bool result = false;

            TempData["userType"] = userType;
            string errorMessage = string.Empty;

            if (TempData["userType"] != null)
            {
                ViewData["userType"] = TempData["userType"];
                ViewBag.UserType = TempData["userType"];
                ViewBag.Name = name;

                UserProfilesInfo userInfo = new UserProfilesInfo
                {
                    ProfileName = collection["ProfileName"] ?? string.Empty,
                    ProfileDesc = collection["ProfileDesc"] ?? string.Empty,
                    //Admin Menu
                    CanAdmin = collection["checkallAdmin"] ?? string.Empty,
                    CanAdminReg = collection["chkARegistration"] ?? string.Empty,
                    CanAARegistration = collection["chkAARegistration"] ?? string.Empty,
                    CanAdminModify = collection["chkAModification"] ?? string.Empty,
                    CanAAModified = collection["chkAAModified"] ?? string.Empty,
                    CanAPwdReset = collection["chkAPwdReset"] ?? string.Empty,
                    CanAAPwdReset = collection["chkAAPwdReset"] ?? string.Empty,
                    CanARegistrationRejected = collection["chkARegistrationRejected"] ?? string.Empty,
                    CanAModificationRejected = collection["chkAModificationRejected"] ?? string.Empty,

                    //Customer Menu
                    CanCust = collection["checkallCustomer"] ?? string.Empty,
                    CanCReg = collection["chkCRegister"] ?? string.Empty,
                    CanCApprove = collection["chkCApprove"] ?? string.Empty,
                    CanCModify = collection["chkCModify"] ?? string.Empty,
                    CApproveModify = collection["chkApproveModify"] ?? string.Empty,
                    CStatus = collection["chkCustomerStatus"] ?? string.Empty,
                    CRejectedList = collection["chkCRejectedList"] ?? string.Empty,
                    CPinReset = collection["chkPinReset"] ?? string.Empty,
                    CApprovePinReset = collection["chkAPinReset"] ?? string.Empty,
                    CanCDelete = collection["chkCDelete"] ?? string.Empty,
                    CApproveDelete = collection["chkApproveDelete"] ?? string.Empty,
                    CUnBlockService = collection["chkCUnBlockService"] ?? string.Empty,
                    CApproveUnblock = collection["chkCApproveUnBlock"] ?? string.Empty,
                    CCharge = collection["chkCustCharge"] ?? string.Empty,
                    CApproveRenewCharge = collection["chkCApproveRC"] ?? string.Empty,

                    BranchSetup = collection["checkallBranchSetup"] ?? string.Empty,
                    BranchRegistration = collection["chkBranchRegister"] ?? string.Empty,
                    BranchModification = collection["chkBranchModify"] ?? string.Empty,
                    BranchStatus = collection["chkBranchStatus"] ?? string.Empty,

                    AcTypeSetup = collection["chkAcType"] ?? string.Empty,
                    AcTypeSetupm = collection["chkAcTypem"] ?? string.Empty,

                    MNepalPay = collection["checkallMNepalPay"] ?? string.Empty,
                    ManageService = collection["chkManageServices"] ?? string.Empty,
                    CheckStatus = collection["chkCheckStatus"] ?? string.Empty,
                    Cancellation = collection["chkCancellation"] ?? string.Empty,
                    UserSetup = collection["checkallUserSetup"] ?? string.Empty,
                    UCreateAdminProfile = collection["chkCAdminProfile"] ?? string.Empty,
                    UModifyAdminProfile = collection["chkMAdminProfile"] ?? string.Empty,
                    UCreateCustProfile = collection["chkCCustomerProfile"] ?? string.Empty,
                    UModifyCustProfile = collection["chkMCustProfile"] ?? string.Empty,
                    Request = collection["checkallRequest"] ?? string.Empty,
                    RChequeBookList = collection["chkChkList"] ?? string.Empty,
                    RStmtList = collection["chkStmtList"] ?? string.Empty,
                    RComplainList = collection["chkComplainList"] ?? string.Empty,
                    RRecommandationList = collection["chkRecomList"] ?? string.Empty,
                    Messaging = collection["checkallMsg"] ?? string.Empty,
                    MUncastMsg = collection["chkUnicastMsg"] ?? string.Empty,
                    MBulkMsg = collection["chkBulkMsg"] ?? string.Empty,
                    MResendMsg = collection["chkResendMsg"] ?? string.Empty,
                    Report = collection["checkallReport"] ?? string.Empty,
                    ReSmsInOut = collection["chkRSMS"] ?? string.Empty,
                    ReChequeBookRequest = collection["chkRCheckBookReq"] ?? string.Empty,
                    ReStmtRequest = collection["chkRStmtReq"] ?? string.Empty,
                    ReRegisteredCustomer = collection["chkRRegisterCust"] ?? string.Empty,
                    ReAdminActivities = collection["chkRAdminActivities"] ?? string.Empty,
                    ReCustActivities = collection["chkRCustAct"] ?? string.Empty,
                    ReMerchantPayment = collection["chkRMerchantPay"] ?? string.Empty,
                    ReMNepalPayTran = collection["chkRMNepalPayTran"] ?? string.Empty,
                    ReMNepalPayCancel = collection["chkRPayCancel"] ?? string.Empty,
                    ReCardRequest = collection["chkRCardRequest"] ?? string.Empty,
                    ReServicePayments = collection["chkServicePayments"] ?? string.Empty,
                    ReTransactionDetail = collection["chkRTranDetail"] ?? string.Empty,
                    ReAdminDetail = collection["chkRAdminDetail"] ?? string.Empty,
                    ReCharge = collection["chkCharge"] ?? string.Empty,
                    ReMNepalPayReport = collection["chkRTopupReport"] ?? string.Empty,

                    /*New Added Reports*/
                    ReCusActivityReport = collection["chkRCusActivity"] ?? string.Empty,
                    ReCusLogReport = collection["chkRCusLog"] ?? string.Empty,
                    ReMerchantsReport = collection["chkMerchants"] ?? string.Empty,
                    ReTranSummaryReport = collection["chkTranSummary"] ?? string.Empty,
                    ReAdminDetailsReport = collection["chkRAdDetails"] ?? string.Empty,
                    /*******************/

                    Payee = collection["checkallPayee"] ?? string.Empty,
                    PMerchantRegister = collection["chkMRegister"] ?? string.Empty,
                    PMerchantModification = collection["chkMModify"] ?? string.Empty,
                    PComPartnerRegister = collection["chkComPartRegister"] ?? string.Empty,
                    PComPartnerModify = collection["chkComPartnerModify"] ?? string.Empty,
                    PMerchantDetails = collection["chkMDetail"] ?? string.Empty,
                    PAddComSlab = collection["chkAComSlab"] ?? string.Empty,
                    PModifyComSlab = collection["chkMComSlab"] ?? string.Empty,
                    PCommissionSlabDetail = collection["chkComSlabDetail"] ?? string.Empty,
                    Setting = collection["checkallSetting"] ?? string.Empty,
                    SKeywordSetting = collection["chkCKeywordSettings"] ?? string.Empty,
                    SCreateStaticKeyword = collection["chkCStaticKeyword"] ?? string.Empty,
                    SModifyStaticKeyword = collection["chkMStaticKeyword"] ?? string.Empty,
                    SModifyMsg = collection["chkMMessages"] ?? string.Empty,
                    SChangePwd = collection["chkChangePwd"] ?? string.Empty
                };

                try
                {
                    if (collection.AllKeys.Any())
                    {
                        int resultmsg = UserProfileUtils.CreateAdminProfileInfo(userInfo);
                        if (resultmsg == 100)
                        {
                            result = true;
                        }
                        else
                        {
                            result = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    result = false;
                    errorMessage = ex.Message;
                }

                TempData["adminprofile_messsage"] = result
                    ? "Admin Profile information is successfully added."
                    : "Error while inserting the information. ERROR :: "
                      + errorMessage;
                TempData["message_class"] = result ? "success_info" : "failed_info";

                return RedirectToAction("CreateAdminProfile", "UserRoleSetup");
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        // GET: UserSetup/ViewAdminProfileSetup
        [HttpGet]
        public ActionResult ViewAdminProfileSetupOLD(string profileCode)
        {
            //string userName = (string)Session["LOGGED_USERNAME"];
            //string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                ViewData["userType"] = TempData["userType"];
                ViewBag.UserType = TempData["userType"];
                ViewBag.Name = name;

                UserProfilesInfo userInfo = new UserProfilesInfo();
                DataTable dtblUserInfo = UserProfileUtils.GetAdminUserProfileInfo(profileCode);
                if (dtblUserInfo.Rows.Count == 1)
                {
                    userInfo.UPID = dtblUserInfo.Rows[0]["ProfileCode"].ToString();
                    userInfo.ProfileName = dtblUserInfo.Rows[0]["ProfileName"].ToString();
                    userInfo.ProfileDesc = dtblUserInfo.Rows[0]["ProfileDesc"].ToString();
                    userInfo.UserProfileStatus = dtblUserInfo.Rows[0]["ProfileStatus"].ToString();
                    userInfo.ProfileGroup = dtblUserInfo.Rows[0]["ProfileGroup"].ToString();
                }
                ViewBag.UPID = userInfo.UPID;
                ViewBag.ProfileName = userInfo.ProfileName;
                ViewBag.ProfileDesc = userInfo.ProfileDesc;
                ViewBag.UserProfileStatus = userInfo.UserProfileStatus;
                ViewBag.ProfileGroup = userInfo.ProfileGroup;


                DataTable dtblUserMenuInfo = UserProfileUtils.GetAdminUserProfileMenuInfo(profileCode);

                foreach (DataRow dr in dtblUserMenuInfo.Rows)
                {
                    string hierarchy = dr["Hierarchy"].ToString();
                    switch (hierarchy)
                    {
                        case "A":
                            userInfo.CanAdmin = dr["Hierarchy"].ToString();
                            break;
                        case "AA":
                            userInfo.CanAdminReg = dr["Hierarchy"].ToString();
                            break;
                        case "AB":
                            userInfo.CanAARegistration = dr["Hierarchy"].ToString();
                            break;
                        case "AC":
                            userInfo.CanARegistrationRejected = dr["Hierarchy"].ToString();
                            break;
                        case "AD":
                            userInfo.CanAdminModify = dr["Hierarchy"].ToString();
                            break;
                        case "AE":
                            userInfo.CanAAModified = dr["Hierarchy"].ToString();
                            break;
                        case "AF":
                            userInfo.CanAModificationRejected = dr["Hierarchy"].ToString();
                            break;
                        case "AG":
                            userInfo.CanAPwdReset = dr["Hierarchy"].ToString();
                            break;
                        case "AH":
                            userInfo.CanAAPwdReset = dr["Hierarchy"].ToString();
                            break;


                        case "B":
                            userInfo.CanCust = hierarchy;
                            break;
                        case "BA":
                            userInfo.CanCReg = dr["Hierarchy"].ToString();
                            break;
                        case "BB":
                            userInfo.CanCApprove = dr["Hierarchy"].ToString();
                            break;
                        case "BC":
                            userInfo.CanCModify = dr["Hierarchy"].ToString();
                            break;
                        case "BD":
                            userInfo.CApproveModify = dr["Hierarchy"].ToString();
                            break;
                        case "BE":
                            userInfo.CStatus = dr["Hierarchy"].ToString();
                            break;
                        case "BF":
                            userInfo.CRejectedList = dr["Hierarchy"].ToString();
                            break;
                        case "BG":
                            userInfo.CPinReset = dr["Hierarchy"].ToString();
                            break;
                        case "BH":
                            userInfo.CApprovePinReset = dr["Hierarchy"].ToString();
                            break;
                        case "BI":
                            userInfo.CanCDelete = dr["Hierarchy"].ToString();
                            break;
                        case "BJ":
                            userInfo.CApproveDelete = dr["Hierarchy"].ToString();
                            break;
                        case "BK":
                            userInfo.CUnBlockService = dr["Hierarchy"].ToString();
                            break;
                        case "BL":
                            userInfo.CApproveUnblock = dr["Hierarchy"].ToString();
                            break;
                        case "BM":
                            userInfo.CCharge = dr["Hierarchy"].ToString();
                            break;
                        case "BN":
                            userInfo.CApproveRenewCharge = dr["Hierarchy"].ToString();
                            break;

                        case "C":
                            userInfo.BranchSetup = dr["Hierarchy"].ToString();
                            break;
                        case "CA":
                            userInfo.BranchRegistration = dr["Hierarchy"].ToString();
                            break;
                        case "CB":
                            userInfo.BranchModification = dr["Hierarchy"].ToString();
                            break;
                        case "CC":
                            userInfo.BranchStatus = dr["Hierarchy"].ToString();
                            break;

                        case "D":
                            userInfo.AcTypeSetup = dr["Hierarchy"].ToString();
                            break;
                        case "DA":
                            userInfo.AcTypeSetupm = dr["Hierarchy"].ToString();
                            break;

                        case "E":
                            userInfo.MNepalPay = dr["Hierarchy"].ToString();
                            break;
                        case "EA":
                            userInfo.ManageService = dr["Hierarchy"].ToString();
                            break;
                        case "EB":
                            userInfo.CheckStatus = dr["Hierarchy"].ToString();
                            break;
                        case "EC":
                            userInfo.Cancellation = dr["Hierarchy"].ToString();
                            break;

                        case "F":
                            userInfo.UserSetup = dr["Hierarchy"].ToString();
                            break;
                        case "FA":
                            userInfo.UCreateAdminProfile = dr["Hierarchy"].ToString();
                            break;
                        case "FB":
                            userInfo.UModifyAdminProfile = dr["Hierarchy"].ToString();
                            break;
                        case "FC":
                            userInfo.UCreateCustProfile = dr["Hierarchy"].ToString();
                            break;
                        case "FD":
                            userInfo.UModifyCustProfile = dr["Hierarchy"].ToString();
                            break;

                        case "G":
                            userInfo.Request = dr["Hierarchy"].ToString();
                            break;
                        case "GA":
                            userInfo.RChequeBookList = dr["Hierarchy"].ToString();
                            break;
                        case "GB":
                            userInfo.RStmtList = dr["Hierarchy"].ToString();
                            break;
                        case "GC":
                            userInfo.RComplainList = dr["Hierarchy"].ToString();
                            break;
                        case "GD":
                            userInfo.RRecommandationList = dr["Hierarchy"].ToString();
                            break;

                        case "H":
                            userInfo.Messaging = dr["Hierarchy"].ToString();
                            break;
                        case "HA":
                            userInfo.MUncastMsg = dr["Hierarchy"].ToString();
                            break;
                        case "HB":
                            userInfo.MBulkMsg = dr["Hierarchy"].ToString();
                            break;
                        case "HC":
                            userInfo.MResendMsg = dr["Hierarchy"].ToString();
                            break;

                        case "I":
                            userInfo.Report = dr["Hierarchy"].ToString();
                            break;
                        case "IA":
                            userInfo.ReSmsInOut = dr["Hierarchy"].ToString();
                            break;
                        case "IB":
                            userInfo.ReChequeBookRequest = dr["Hierarchy"].ToString();
                            break;
                        case "IC":
                            userInfo.ReStmtRequest = dr["Hierarchy"].ToString();
                            break;
                        case "ID":
                            userInfo.ReRegisteredCustomer = dr["Hierarchy"].ToString();
                            break;
                        case "IE":
                            userInfo.ReAdminActivities = dr["Hierarchy"].ToString();
                            break;
                        case "IF":
                            userInfo.ReCustActivities = dr["Hierarchy"].ToString();
                            break;
                        case "IG":
                            userInfo.ReMerchantPayment = dr["Hierarchy"].ToString();
                            break;
                        case "IH":
                            userInfo.ReMNepalPayTran = dr["Hierarchy"].ToString();
                            break;
                        case "II":
                            userInfo.ReMNepalPayCancel = dr["Hierarchy"].ToString();
                            break;
                        case "IJ":
                            userInfo.ReCardRequest = dr["Hierarchy"].ToString();
                            break;
                        case "IK":
                            userInfo.ReServicePayments = dr["Hierarchy"].ToString();
                            break;
                        case "IL":
                            userInfo.ReTransactionDetail = dr["Hierarchy"].ToString();
                            break;
                        case "IM":
                            userInfo.ReAdminDetail = dr["Hierarchy"].ToString();
                            break;
                        case "IN":
                            userInfo.ReCharge = dr["Hierarchy"].ToString();
                            break;
                        case "IO":
                            userInfo.ReMNepalPayReport = dr["Hierarchy"].ToString();
                            break;
                        case "IP":
                            userInfo.ReCusActivityReport = dr["Hierarchy"].ToString();
                            break;
                        case "IQ":
                            userInfo.ReCusLogReport = dr["Hierarchy"].ToString();
                            break;
                        case "IR":
                            userInfo.ReMerchantsReport = dr["Hierarchy"].ToString();
                            break;
                        case "IS":
                            userInfo.ReTranSummaryReport = dr["Hierarchy"].ToString();
                            break;
                        case "IT":
                            userInfo.ReAdminDetailsReport = dr["Hierarchy"].ToString();
                            break;

                        case "J":
                            userInfo.Payee = dr["Hierarchy"].ToString();
                            break;
                        case "JA":
                            userInfo.PMerchantRegister = dr["Hierarchy"].ToString();
                            break;
                        case "JB":
                            userInfo.PMerchantModification = dr["Hierarchy"].ToString();
                            break;
                        case "JC":
                            userInfo.PComPartnerRegister = dr["Hierarchy"].ToString();
                            break;
                        case "JD":
                            userInfo.PComPartnerModify = dr["Hierarchy"].ToString();
                            break;
                        case "JE":
                            userInfo.PMerchantDetails = dr["Hierarchy"].ToString();
                            break;
                        case "JF":
                            userInfo.PAddComSlab = dr["Hierarchy"].ToString();
                            break;
                        case "JG":
                            userInfo.PModifyComSlab = dr["Hierarchy"].ToString();
                            break;
                        case "JH":
                            userInfo.PCommissionSlabDetail = dr["Hierarchy"].ToString();
                            break;

                        case "K":
                            userInfo.Setting = dr["Hierarchy"].ToString();
                            break;
                        case "KA":
                            userInfo.SKeywordSetting = dr["Hierarchy"].ToString();
                            break;
                        case "KB":
                            userInfo.SCreateStaticKeyword = dr["Hierarchy"].ToString();
                            break;
                        case "KC":
                            userInfo.SModifyStaticKeyword = dr["Hierarchy"].ToString();
                            break;
                        case "KD":
                            userInfo.SModifyMsg = dr["Hierarchy"].ToString();
                            break;
                        case "KE":
                            userInfo.SChangePwd = dr["Hierarchy"].ToString();
                            break;
                    }

                }

                ViewBag.ClientCode = userInfo.ClientCode;
                ViewBag.CanAdmin = userInfo.CanAdmin;
                ViewBag.CanAdminReg = userInfo.CanAdminReg;
                ViewBag.CanAdminModify = userInfo.CanAdminModify;
                ViewBag.CanAPwdReset = userInfo.CanAPwdReset;
                ViewBag.CanAARegistration = userInfo.CanAARegistration;
                ViewBag.CanAAModified = userInfo.CanAAModified;
                ViewBag.CanAAPwdReset = userInfo.CanAAPwdReset;
                ViewBag.CanARegistrationRejected = userInfo.CanARegistrationRejected;
                ViewBag.CanAModificationRejected = userInfo.CanAModificationRejected;

                ViewBag.CanCApprove = userInfo.CanCApprove;
                ViewBag.CanCust = userInfo.CanCust;
                ViewBag.CanCReg = userInfo.CanCReg;
                ViewBag.CanCModify = userInfo.CanCModify;
                ViewBag.CApproveModify = userInfo.CApproveModify;
                ViewBag.CStatus = userInfo.CStatus;
                ViewBag.CRejectedList = userInfo.CRejectedList;
                ViewBag.CPinReset = userInfo.CPinReset;
                ViewBag.CApprovePinReset = userInfo.CApprovePinReset;
                ViewBag.CanCDelete = userInfo.CanCDelete;
                ViewBag.CApproveDelete = userInfo.CApproveDelete;
                ViewBag.CUnBlockService = userInfo.CUnBlockService;
                ViewBag.CApproveUnblock = userInfo.CApproveUnblock;
                ViewBag.CCharge = userInfo.CCharge;
                ViewBag.CApproveRenewCharge = userInfo.CApproveRenewCharge;

                ViewBag.BranchSetup = userInfo.BranchSetup;
                ViewBag.BranchRegistration = userInfo.BranchRegistration;
                ViewBag.BranchModification = userInfo.BranchModification;
                ViewBag.BranchStatus = userInfo.BranchStatus;

                ViewBag.AcTypeSetup = userInfo.AcTypeSetup;
                ViewBag.AcTypeSetupm = userInfo.AcTypeSetupm;

                ViewBag.MNepalPay = userInfo.MNepalPay;
                ViewBag.ManageService = userInfo.ManageService;
                ViewBag.CheckStatus = userInfo.CheckStatus;
                ViewBag.Cancellation = userInfo.Cancellation;

                ViewBag.UserSetup = userInfo.UserSetup;
                ViewBag.UCreateAdminProfile = userInfo.UCreateAdminProfile;
                ViewBag.UModifyAdminProfile = userInfo.UModifyAdminProfile;
                ViewBag.UCreateCustProfile = userInfo.UCreateCustProfile;
                ViewBag.UModifyCustProfile = userInfo.UModifyCustProfile;

                ViewBag.Request = userInfo.Request;
                ViewBag.RChequeBookList = userInfo.RChequeBookList;
                ViewBag.RStmtList = userInfo.RStmtList;
                ViewBag.RComplainList = userInfo.RComplainList;
                ViewBag.RRecommandationList = userInfo.RRecommandationList;

                ViewBag.Messaging = userInfo.Messaging;
                ViewBag.MUncastMsg = userInfo.MUncastMsg;
                ViewBag.MBulkMsg = userInfo.MBulkMsg;
                ViewBag.MResendMsg = userInfo.MResendMsg;

                ViewBag.Report = userInfo.Report;
                ViewBag.ReSmsInOut = userInfo.ReSmsInOut;
                ViewBag.ReRegisteredCustomer = userInfo.ReRegisteredCustomer;
                ViewBag.ReMerchantPayment = userInfo.ReMerchantPayment;
                ViewBag.ReCardRequest = userInfo.ReCardRequest;
                ViewBag.ReAdminDetail = userInfo.ReAdminDetail;
                ViewBag.ReChequeBookRequest = userInfo.ReChequeBookRequest;
                ViewBag.ReAdminActivities = userInfo.ReAdminActivities;
                ViewBag.ReMNepalPayTran = userInfo.ReMNepalPayTran;
                ViewBag.ReServicePayments = userInfo.ReServicePayments;
                ViewBag.ReCharge = userInfo.ReCharge;
                ViewBag.ReStmtRequest = userInfo.ReStmtRequest;
                ViewBag.ReCustActivities = userInfo.ReCustActivities;
                ViewBag.ReMNepalPayCancel = userInfo.ReMNepalPayCancel;
                ViewBag.ReTransactionDetail = userInfo.ReTransactionDetail;
                ViewBag.ReMNepalPayReport = userInfo.ReMNepalPayReport;
                ViewBag.ReCusActivityReport = userInfo.ReCusActivityReport;
                ViewBag.ReCusLogReport = userInfo.ReCusLogReport;
                ViewBag.ReMerchantsReport = userInfo.ReMerchantsReport;
                ViewBag.ReTranSummaryReport = userInfo.ReTranSummaryReport;
                ViewBag.ReAdminDetailsReport = userInfo.ReAdminDetailsReport;

                ViewBag.Payee = userInfo.Payee;
                ViewBag.PMerchantRegister = userInfo.PMerchantRegister;
                ViewBag.PComPartnerModify = userInfo.PComPartnerModify;
                ViewBag.PModifyComSlab = userInfo.PModifyComSlab;
                ViewBag.PMerchantModification = userInfo.PMerchantModification;
                ViewBag.PMerchantDetails = userInfo.PMerchantDetails;
                ViewBag.PCommissionSlabDetail = userInfo.PCommissionSlabDetail;
                ViewBag.PCommisionPartnerRegistration = userInfo.PComPartnerRegister;
                ViewBag.PAddComSlab = userInfo.PAddComSlab;

                ViewBag.Setting = userInfo.Setting;
                ViewBag.SKeywordSetting = userInfo.SKeywordSetting;
                ViewBag.SModifyMsg = userInfo.SModifyMsg;
                ViewBag.SCreateStaticKeyword = userInfo.SCreateStaticKeyword;
                ViewBag.SChangePwd = userInfo.SChangePwd;
                ViewBag.SModifyStaticKeyword = userInfo.SModifyStaticKeyword;

                return View(userInfo);

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }
        // GET: UserSetup/EditAdminProfileSetup
        [HttpGet]
        public ActionResult EditAdminProfileSetupOLD(string profileCode)
        {
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                ViewData["userType"] = TempData["userType"];
                ViewBag.UserType = TempData["userType"];
                ViewBag.Name = name;



                UserProfilesInfo userInfo = new UserProfilesInfo();
                DataTable dtblUserInfo = UserProfileUtils.GetAdminUserProfileInfo(profileCode);
                if (dtblUserInfo.Rows.Count == 1)
                {
                    userInfo.UPID = dtblUserInfo.Rows[0]["ProfileCode"].ToString();
                    userInfo.ProfileName = dtblUserInfo.Rows[0]["ProfileName"].ToString();
                    userInfo.ProfileDesc = dtblUserInfo.Rows[0]["ProfileDesc"].ToString();
                    userInfo.UserProfileStatus = dtblUserInfo.Rows[0]["ProfileStatus"].ToString();
                    userInfo.ProfileGroup = dtblUserInfo.Rows[0]["ProfileGroup"].ToString();
                }
                ViewBag.UPID = userInfo.UPID;
                ViewBag.ProfileName = userInfo.ProfileName;
                ViewBag.ProfileDesc = userInfo.ProfileDesc;
                ViewBag.UserProfileStatus = userInfo.UserProfileStatus;
                ViewBag.ProfileGroup = userInfo.ProfileGroup;

                DataTable dtblUserMenuInfo = UserProfileUtils.GetAdminUserProfileMenuInfo(profileCode);

                foreach (DataRow dr in dtblUserMenuInfo.Rows)
                {
                    string hierarchy = dr["Hierarchy"].ToString();
                    switch (hierarchy)
                    {
                        case "A":
                            userInfo.CanAdmin = dr["Hierarchy"].ToString();
                            break;
                        case "AA":
                            userInfo.CanAdminReg = dr["Hierarchy"].ToString();
                            break;
                        case "AB":
                            userInfo.CanAARegistration = dr["Hierarchy"].ToString();
                            break;
                        case "AC":
                            userInfo.CanARegistrationRejected = dr["Hierarchy"].ToString();
                            break;
                        case "AD":
                            userInfo.CanAdminModify = dr["Hierarchy"].ToString();
                            break;
                        case "AE":
                            userInfo.CanAAModified = dr["Hierarchy"].ToString();
                            break;
                        case "AF":
                            userInfo.CanAModificationRejected = dr["Hierarchy"].ToString();
                            break;
                        case "AG":
                            userInfo.CanAPwdReset = dr["Hierarchy"].ToString();
                            break;
                        case "AH":
                            userInfo.CanAAPwdReset = dr["Hierarchy"].ToString();
                            break;

                        case "B":
                            userInfo.CanCust = hierarchy;
                            break;
                        case "BA":
                            userInfo.CanCReg = dr["Hierarchy"].ToString();
                            break;
                        case "BB":
                            userInfo.CanCApprove = dr["Hierarchy"].ToString();
                            break;
                        case "BC":
                            userInfo.CanCModify = dr["Hierarchy"].ToString();
                            break;
                        case "BD":
                            userInfo.CApproveModify = dr["Hierarchy"].ToString();
                            break;
                        case "BE":
                            userInfo.CStatus = dr["Hierarchy"].ToString();
                            break;
                        case "BF":
                            userInfo.CRejectedList = dr["Hierarchy"].ToString();
                            break;
                        case "BG":
                            userInfo.CPinReset = dr["Hierarchy"].ToString();
                            break;
                        case "BH":
                            userInfo.CApprovePinReset = dr["Hierarchy"].ToString();
                            break;
                        case "BI":
                            userInfo.CanCDelete = dr["Hierarchy"].ToString();
                            break;
                        case "BJ":
                            userInfo.CApproveDelete = dr["Hierarchy"].ToString();
                            break;
                        case "BK":
                            userInfo.CUnBlockService = dr["Hierarchy"].ToString();
                            break;
                        case "BL":
                            userInfo.CApproveUnblock = dr["Hierarchy"].ToString();
                            break;
                        case "BM":
                            userInfo.CCharge = dr["Hierarchy"].ToString();
                            break;
                        case "BN":
                            userInfo.CApproveRenewCharge = dr["Hierarchy"].ToString();
                            break;

                        case "C":
                            userInfo.BranchSetup = dr["Hierarchy"].ToString();
                            break;
                        case "CA":
                            userInfo.BranchRegistration = dr["Hierarchy"].ToString();
                            break;
                        case "CB":
                            userInfo.BranchModification = dr["Hierarchy"].ToString();
                            break;
                        case "CC":
                            userInfo.BranchStatus = dr["Hierarchy"].ToString();
                            break;

                        case "D":
                            userInfo.AcTypeSetup = dr["Hierarchy"].ToString();
                            break;
                        case "DA":
                            userInfo.AcTypeSetupm = dr["Hierarchy"].ToString();
                            break;

                        case "E":
                            userInfo.MNepalPay = dr["Hierarchy"].ToString();
                            break;
                        case "EA":
                            userInfo.ManageService = dr["Hierarchy"].ToString();
                            break;
                        case "EB":
                            userInfo.CheckStatus = dr["Hierarchy"].ToString();
                            break;
                        case "EC":
                            userInfo.Cancellation = dr["Hierarchy"].ToString();
                            break;

                        case "F":
                            userInfo.UserSetup = dr["Hierarchy"].ToString();
                            break;
                        case "FA":
                            userInfo.UCreateAdminProfile = dr["Hierarchy"].ToString();
                            break;
                        case "FB":
                            userInfo.UModifyAdminProfile = dr["Hierarchy"].ToString();
                            break;
                        case "FC":
                            userInfo.UCreateCustProfile = dr["Hierarchy"].ToString();
                            break;
                        case "FD":
                            userInfo.UModifyCustProfile = dr["Hierarchy"].ToString();
                            break;

                        case "G":
                            userInfo.Request = dr["Hierarchy"].ToString();
                            break;
                        case "GA":
                            userInfo.RChequeBookList = dr["Hierarchy"].ToString();
                            break;
                        case "GB":
                            userInfo.RStmtList = dr["Hierarchy"].ToString();
                            break;
                        case "GC":
                            userInfo.RComplainList = dr["Hierarchy"].ToString();
                            break;
                        case "GD":
                            userInfo.RRecommandationList = dr["Hierarchy"].ToString();
                            break;

                        case "H":
                            userInfo.Messaging = dr["Hierarchy"].ToString();
                            break;
                        case "HA":
                            userInfo.MUncastMsg = dr["Hierarchy"].ToString();
                            break;
                        case "HB":
                            userInfo.MBulkMsg = dr["Hierarchy"].ToString();
                            break;
                        case "HC":
                            userInfo.MResendMsg = dr["Hierarchy"].ToString();
                            break;

                        case "I":
                            userInfo.Report = dr["Hierarchy"].ToString();
                            break;
                        case "IA":
                            userInfo.ReSmsInOut = dr["Hierarchy"].ToString();
                            break;
                        case "IB":
                            userInfo.ReChequeBookRequest = dr["Hierarchy"].ToString();
                            break;
                        case "IC":
                            userInfo.ReStmtRequest = dr["Hierarchy"].ToString();
                            break;
                        case "ID":
                            userInfo.ReRegisteredCustomer = dr["Hierarchy"].ToString();
                            break;
                        case "IE":
                            userInfo.ReAdminActivities = dr["Hierarchy"].ToString();
                            break;
                        case "IF":
                            userInfo.ReCustActivities = dr["Hierarchy"].ToString();
                            break;
                        case "IG":
                            userInfo.ReMerchantPayment = dr["Hierarchy"].ToString();
                            break;
                        case "IH":
                            userInfo.ReMNepalPayTran = dr["Hierarchy"].ToString();
                            break;
                        case "II":
                            userInfo.ReMNepalPayCancel = dr["Hierarchy"].ToString();
                            break;
                        case "IJ":
                            userInfo.ReCardRequest = dr["Hierarchy"].ToString();
                            break;
                        case "IK":
                            userInfo.ReServicePayments = dr["Hierarchy"].ToString();
                            break;
                        case "IL":
                            userInfo.ReTransactionDetail = dr["Hierarchy"].ToString();
                            break;
                        case "IM":
                            userInfo.ReAdminDetail = dr["Hierarchy"].ToString();
                            break;
                        case "IN":
                            userInfo.ReCharge = dr["Hierarchy"].ToString();
                            break;
                        case "IO":
                            userInfo.ReMNepalPayReport = dr["Hierarchy"].ToString();
                            break;
                        case "IP":
                            userInfo.ReCusActivityReport = dr["Hierarchy"].ToString();
                            break;
                        case "IQ":
                            userInfo.ReCusLogReport = dr["Hierarchy"].ToString();
                            break;
                        case "IR":
                            userInfo.ReMerchantsReport = dr["Hierarchy"].ToString();
                            break;
                        case "IS":
                            userInfo.ReTranSummaryReport = dr["Hierarchy"].ToString();
                            break;
                        case "IT":
                            userInfo.ReAdminDetailsReport = dr["Hierarchy"].ToString();
                            break;

                        case "J":
                            userInfo.Payee = dr["Hierarchy"].ToString();
                            break;
                        case "JA":
                            userInfo.PMerchantRegister = dr["Hierarchy"].ToString();
                            break;
                        case "JB":
                            userInfo.PMerchantModification = dr["Hierarchy"].ToString();
                            break;
                        case "JC":
                            userInfo.PComPartnerRegister = dr["Hierarchy"].ToString();
                            break;
                        case "JD":
                            userInfo.PComPartnerModify = dr["Hierarchy"].ToString();
                            break;
                        case "JE":
                            userInfo.PMerchantDetails = dr["Hierarchy"].ToString();
                            break;
                        case "JF":
                            userInfo.PAddComSlab = dr["Hierarchy"].ToString();
                            break;
                        case "JG":
                            userInfo.PModifyComSlab = dr["Hierarchy"].ToString();
                            break;
                        case "JH":
                            userInfo.PCommissionSlabDetail = dr["Hierarchy"].ToString();
                            break;

                        case "K":
                            userInfo.Setting = dr["Hierarchy"].ToString();
                            break;
                        case "KA":
                            userInfo.SKeywordSetting = dr["Hierarchy"].ToString();
                            break;
                        case "KB":
                            userInfo.SCreateStaticKeyword = dr["Hierarchy"].ToString();
                            break;
                        case "KC":
                            userInfo.SModifyStaticKeyword = dr["Hierarchy"].ToString();
                            break;
                        case "KD":
                            userInfo.SModifyMsg = dr["Hierarchy"].ToString();
                            break;
                        case "KE":
                            userInfo.SChangePwd = dr["Hierarchy"].ToString();
                            break;
                    }

                }
                ViewBag.ClientCode = userInfo.ClientCode;
                ViewBag.MenuAssignID = userInfo.MenuAssignId;
                ViewBag.UPID = userInfo.UPID;
                ViewBag.ProfileName = userInfo.ProfileName;
                ViewBag.ProfileDesc = userInfo.ProfileDesc;
                ViewBag.UserProfileStatus = userInfo.UserProfileStatus;
                ViewBag.CanAdmin = userInfo.CanAdmin;
                ViewBag.CanAdminReg = userInfo.CanAdminReg;
                ViewBag.CanAdminModify = userInfo.CanAdminModify;
                ViewBag.CanAPwdReset = userInfo.CanAPwdReset;
                ViewBag.CanAARegistration = userInfo.CanAARegistration;
                ViewBag.CanAAModified = userInfo.CanAAModified;
                ViewBag.CanAAPwdReset = userInfo.CanAAPwdReset;
                ViewBag.CanARegistrationRejected = userInfo.CanARegistrationRejected;
                ViewBag.CanAModificationRejected = userInfo.CanAModificationRejected;


                ViewBag.CanCust = userInfo.CanCust;
                ViewBag.CanCReg = userInfo.CanCReg;
                ViewBag.CanCApprove = userInfo.CanCApprove;
                ViewBag.CanCModify = userInfo.CanCModify;
                ViewBag.CApproveModify = userInfo.CApproveModify;
                ViewBag.CStatus = userInfo.CStatus;
                ViewBag.CRejectedList = userInfo.CRejectedList;
                ViewBag.CPinReset = userInfo.CPinReset;
                ViewBag.CApprovePinReset = userInfo.CApprovePinReset;
                ViewBag.CanCDelete = userInfo.CanCDelete;
                ViewBag.CApproveDelete = userInfo.CApproveDelete;
                ViewBag.CUnBlockService = userInfo.CUnBlockService;
                ViewBag.CApproveUnblock = userInfo.CApproveUnblock;
                ViewBag.CCharge = userInfo.CCharge;
                ViewBag.CApproveRenewCharge = userInfo.CApproveRenewCharge;

                ViewBag.BranchSetup = userInfo.BranchSetup;
                ViewBag.BranchRegistration = userInfo.BranchRegistration;
                ViewBag.BranchModification = userInfo.BranchModification;
                ViewBag.BranchStatus = userInfo.BranchStatus;

                ViewBag.AcTypeSetup = userInfo.AcTypeSetup;
                ViewBag.AcTypeSetupm = userInfo.AcTypeSetupm;

                ViewBag.MNepalPay = userInfo.MNepalPay;
                ViewBag.ManageService = userInfo.ManageService;
                ViewBag.CheckStatus = userInfo.CheckStatus;
                ViewBag.Cancellation = userInfo.Cancellation;

                ViewBag.UserSetup = userInfo.UserSetup;
                ViewBag.UCreateAdminProfile = userInfo.UCreateAdminProfile;
                ViewBag.UModifyAdminProfile = userInfo.UModifyAdminProfile;
                ViewBag.UCreateCustProfile = userInfo.UCreateCustProfile;
                ViewBag.UModifyCustProfile = userInfo.UModifyCustProfile;

                ViewBag.Request = userInfo.Request;
                ViewBag.RChequeBookList = userInfo.RChequeBookList;
                ViewBag.RStmtList = userInfo.RStmtList;
                ViewBag.RComplainList = userInfo.RComplainList;
                ViewBag.RRecommandationList = userInfo.RRecommandationList;

                ViewBag.Messaging = userInfo.Messaging;
                ViewBag.MUncastMsg = userInfo.MUncastMsg;
                ViewBag.MBulkMsg = userInfo.MBulkMsg;
                ViewBag.MResendMsg = userInfo.MResendMsg;

                ViewBag.Report = userInfo.Report;
                ViewBag.ReSmsInOut = userInfo.ReSmsInOut;
                ViewBag.ReRegisteredCustomer = userInfo.ReRegisteredCustomer;
                ViewBag.ReMerchantPayment = userInfo.ReMerchantPayment;
                ViewBag.ReCardRequest = userInfo.ReCardRequest;
                ViewBag.ReAdminDetail = userInfo.ReAdminDetail;
                ViewBag.ReChequeBookRequest = userInfo.ReChequeBookRequest;
                ViewBag.ReAdminActivities = userInfo.ReAdminActivities;
                ViewBag.ReMNepalPayTran = userInfo.ReMNepalPayTran;
                ViewBag.ReServicePayments = userInfo.ReServicePayments;
                ViewBag.ReCharge = userInfo.ReCharge;
                ViewBag.ReStmtRequest = userInfo.ReStmtRequest;
                ViewBag.ReCustActivities = userInfo.ReCustActivities;
                ViewBag.ReMNepalPayCancel = userInfo.ReMNepalPayCancel;
                ViewBag.ReTransactionDetail = userInfo.ReTransactionDetail;
                ViewBag.ReMNepalPayReport = userInfo.ReMNepalPayReport;
                ViewBag.ReCusActivityReport = userInfo.ReCusActivityReport;
                ViewBag.ReCusLogReport = userInfo.ReCusLogReport;
                ViewBag.ReMerchantsReport = userInfo.ReMerchantsReport;
                ViewBag.ReTranSummaryReport = userInfo.ReTranSummaryReport;
                ViewBag.ReAdminDetailsReport = userInfo.ReAdminDetailsReport;

                ViewBag.Payee = userInfo.Payee;
                ViewBag.PMerchantRegister = userInfo.PMerchantRegister;
                ViewBag.PComPartnerModify = userInfo.PComPartnerModify;
                ViewBag.PModifyComSlab = userInfo.PModifyComSlab;
                ViewBag.PMerchantModification = userInfo.PMerchantModification;
                ViewBag.PMerchantDetails = userInfo.PMerchantDetails;
                ViewBag.PCommissionSlabDetail = userInfo.PCommissionSlabDetail;
                ViewBag.PCommisionPartnerRegistration = userInfo.PComPartnerRegister;
                ViewBag.PAddComSlab = userInfo.PAddComSlab;

                ViewBag.Setting = userInfo.Setting;
                ViewBag.SKeywordSetting = userInfo.SKeywordSetting;
                ViewBag.SModifyMsg = userInfo.SModifyMsg;
                ViewBag.SCreateStaticKeyword = userInfo.SCreateStaticKeyword;
                ViewBag.SChangePwd = userInfo.SChangePwd;
                ViewBag.SModifyStaticKeyword = userInfo.SModifyStaticKeyword;

                return View(userInfo);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }
        // POST: UserSetup/EditAdminProfileSetup
        [HttpPost]
        public ActionResult EditAdminProfileSetupOLD(FormCollection collection)
        {
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                bool result = false;
                string errorMessage = string.Empty;
                ViewData["userType"] = TempData["userType"];
                ViewBag.UserType = TempData["userType"];
                ViewBag.Name = name;

                string Status = collection["UserProfileStatus"].ToString();
                UserProfilesInfo userInfo = new UserProfilesInfo
                {
                    UPID = collection["UPID"] ?? string.Empty,
                    ProfileName = collection["ProfileName"] ?? string.Empty,
                    ProfileDesc = collection["ProfileDesc"] ?? string.Empty,
                    CanAdmin = collection["checkallAdmin"] ?? string.Empty,
                    CanAdminReg = collection["chkARegistration"] ?? string.Empty,
                    CanAARegistration = collection["chkAARegistration"] ?? string.Empty,
                    CanAdminModify = collection["chkAModification"] ?? string.Empty,
                    CanAAModified = collection["chkAAModified"] ?? string.Empty,
                    CanAPwdReset = collection["chkAPwdReset"] ?? string.Empty,
                    CanAAPwdReset = collection["chkAAPwdReset"] ?? string.Empty,
                    CanARegistrationRejected = collection["chkARegistrationRejected"] ?? string.Empty,
                    CanAModificationRejected = collection["chkAModificationRejected"] ?? string.Empty,

                    CanCust = collection["checkallCustomer"] ?? string.Empty,
                    CanCReg = collection["chkCRegister"] ?? string.Empty,
                    CanCApprove = collection["chkCApprove"] ?? string.Empty,
                    CanCModify = collection["chkCModify"] ?? string.Empty,
                    CApproveModify = collection["chkApproveModify"] ?? string.Empty,
                    CStatus = collection["chkCustomerStatus"] ?? string.Empty,
                    CRejectedList = collection["chkCRejectedList"] ?? string.Empty,
                    CPinReset = collection["chkPinReset"] ?? string.Empty,
                    CApprovePinReset = collection["chkAPinReset"] ?? string.Empty,
                    CanCDelete = collection["chkCDelete"] ?? string.Empty,
                    CApproveDelete = collection["chkApproveDelete"] ?? string.Empty,
                    CUnBlockService = collection["chkCUnBlockService"] ?? string.Empty,
                    CApproveUnblock = collection["chkCApproveUnBlock"] ?? string.Empty,
                    CCharge = collection["chkCustCharge"] ?? string.Empty,
                    CApproveRenewCharge = collection["chkCApproveRC"] ?? string.Empty,
                    BranchSetup = collection["checkallBranchSetup"] ?? string.Empty,
                    BranchRegistration = collection["chkBranchRegister"] ?? string.Empty,
                    BranchModification = collection["chkBranchModify"] ?? string.Empty,
                    BranchStatus = collection["chkBranchStatus"] ?? string.Empty,
                    AcTypeSetup = collection["chkAcType"] ?? string.Empty,
                    AcTypeSetupm = collection["chkAcTypem"] ?? string.Empty,
                    MNepalPay = collection["checkallMNepalPay"] ?? string.Empty,
                    ManageService = collection["chkManageServices"] ?? string.Empty,
                    CheckStatus = collection["chkCheckStatus"] ?? string.Empty,
                    Cancellation = collection["chkCancellation"] ?? string.Empty,
                    UserSetup = collection["checkallUserSetup"] ?? string.Empty,
                    UCreateAdminProfile = collection["chkCAdminProfile"] ?? string.Empty,
                    UModifyAdminProfile = collection["chkMAdminProfile"] ?? string.Empty,
                    UCreateCustProfile = collection["chkCCustomerProfile"] ?? string.Empty,
                    UModifyCustProfile = collection["chkMCustProfile"] ?? string.Empty,
                    Request = collection["checkallRequest"] ?? string.Empty,
                    RChequeBookList = collection["chkChkList"] ?? string.Empty,
                    RStmtList = collection["chkStmtList"] ?? string.Empty,
                    RComplainList = collection["chkComplainList"] ?? string.Empty,
                    RRecommandationList = collection["chkRecomList"] ?? string.Empty,
                    Messaging = collection["checkallMsg"] ?? string.Empty,
                    MUncastMsg = collection["chkUnicastMsg"] ?? string.Empty,
                    MBulkMsg = collection["chkBulkMsg"] ?? string.Empty,
                    MResendMsg = collection["chkResendMsg"] ?? string.Empty,
                    Report = collection["checkallReport"] ?? string.Empty,
                    ReSmsInOut = collection["chkRSMS"] ?? string.Empty,
                    ReChequeBookRequest = collection["chkRCheckBookReq"] ?? string.Empty,
                    ReStmtRequest = collection["chkRStmtReq"] ?? string.Empty,
                    ReRegisteredCustomer = collection["chkRRegisterCust"] ?? string.Empty,
                    ReAdminActivities = collection["chkRAdminActivities"] ?? string.Empty,
                    ReCustActivities = collection["chkRCustAct"] ?? string.Empty,
                    ReMerchantPayment = collection["chkRMerchantPay"] ?? string.Empty,
                    ReMNepalPayTran = collection["chkRMNepalPayTran"] ?? string.Empty,
                    ReMNepalPayCancel = collection["chkRPayCancel"] ?? string.Empty,
                    ReCardRequest = collection["chkRCardRequest"] ?? string.Empty,
                    ReServicePayments = collection["chkServicePayments"] ?? string.Empty,
                    ReTransactionDetail = collection["chkRTranDetail"] ?? string.Empty,
                    ReAdminDetail = collection["chkRAdminDetail"] ?? string.Empty,
                    ReCharge = collection["chkCharge"] ?? string.Empty,
                    ReMNepalPayReport = collection["chkRTopupReport"] ?? string.Empty,

                    /*New Added Reports*/
                    ReCusActivityReport = collection["chkRCusActivity"] ?? string.Empty,
                    ReCusLogReport = collection["chkRCusLog"] ?? string.Empty,
                    ReMerchantsReport = collection["chkMerchants"] ?? string.Empty,
                    ReTranSummaryReport = collection["chkTranSummary"] ?? string.Empty,
                    ReAdminDetailsReport = collection["chkRAdDetails"] ?? string.Empty,
                    /*******************/

                    Payee = collection["checkallPayee"] ?? string.Empty,
                    PMerchantRegister = collection["chkMRegister"] ?? string.Empty,
                    PMerchantModification = collection["chkMModify"] ?? string.Empty,
                    PComPartnerRegister = collection["chkComPartRegister"] ?? string.Empty,
                    PComPartnerModify = collection["chkComPartnerModify"] ?? string.Empty,
                    PMerchantDetails = collection["chkMDetail"] ?? string.Empty,
                    PAddComSlab = collection["chkAComSlab"] ?? string.Empty,
                    PModifyComSlab = collection["chkMComSlab"] ?? string.Empty,
                    PCommissionSlabDetail = collection["chkComSlabDetail"] ?? string.Empty,
                    Setting = collection["checkallSetting"] ?? string.Empty,
                    SKeywordSetting = collection["chkCKeywordSettings"] ?? string.Empty,
                    SCreateStaticKeyword = collection["chkCStaticKeyword"] ?? string.Empty,
                    SModifyStaticKeyword = collection["chkMStaticKeyword"] ?? string.Empty,
                    SModifyMsg = collection["chkMMessages"] ?? string.Empty,
                    SChangePwd = collection["chkChangePwd"] ?? string.Empty,
                    UserProfileStatus = Status




                };

                try
                {
                    if (collection.AllKeys.Any())
                    {
                        int resultmsg = UserProfileUtils.UpdateAdminProfileInfo(userInfo);
                        if (resultmsg == 100)
                        {
                            result = true;
                        }
                        else
                        {
                            result = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    result = false;
                    errorMessage = ex.Message;
                }
                TempData["admin_messsage"] = result
                    ? "Admin Profile information is successfully updated."
                    : "Error while inserting the information. ERROR :: "
                      + errorMessage;
                TempData["message_class"] = result ? "success_info" : "failed_info";
                return RedirectToAction("Index", "UserRoleSetup");
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        #region CreateAdminProfileNew
        [HttpGet]
        public ActionResult CreateAdminProfile()
        {
            //string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            //Check Role link start          
            string methodlink = System.Reflection.MethodBase.GetCurrentMethod().Name;
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkRole(methodlink, clientCode);
            //Check Role link end


            TempData["userType"] = userType;

            if (TempData["userType"] != null && checkRole)
            {
                ViewData["userType"] = TempData["userType"];
                ViewBag.UserType = TempData["userType"];
                ViewBag.Name = name;

                if (TempData["adminprofile_messsage"] != null)
                {
                    ViewData["adminprofile_messsage"] = TempData["adminprofile_messsage"];
                    ViewData["message_class"] = TempData["message_class"];
                }
                AdminProfileVM data = new AdminProfileVM();
                List<MNMenuTable> menu = UserProfileUtils.GetAdminMenu();
                data.MNMenus = menu;
                return View(data);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpPost]
        public ActionResult CreateAdminProfile(AdminProfileVM model)
        {
            //string userName = (string)Session["LOGGED_USERNAME"];
            //string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                ViewData["userType"] = TempData["userType"];
                ViewBag.UserType = TempData["userType"];
                ViewBag.Name = name;

                if (TempData["adminprofile_messsage"] != null)
                {
                    ViewData["adminprofile_messsage"] = TempData["adminprofile_messsage"];
                    ViewData["message_class"] = TempData["message_class"];
                }

                bool result = false;
                string errorMessage = string.Empty;
                try
                {

                    int resultmsg = UserProfileUtils.CreateAdminProfile(model);
                    if (resultmsg == 100)
                        {
                            result = true;
                        }
                        else
                        {
                            result = false;
                        }
                }
                catch (Exception ex)
                {
                    result = false;
                    errorMessage = ex.Message;
                }
                TempData["adminprofile_messsage"] = result
                    ? "Admin Profile information is successfully added."
                    : "Error while inserting the information. ERROR :: "
                      + errorMessage;
                TempData["message_class"] = result ? "success_info" : "failed_info";

                if(result)
                    return RedirectToAction("CreateAdminProfile", "UserRoleSetup");
                else
                    return View(model);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        #endregion

        #region EditAdminProfileNew
        [HttpGet]
        public ActionResult EditAdminProfileSetup(string profileCode)
        {
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                ViewData["userType"] = TempData["userType"];
                ViewBag.UserType = TempData["userType"];
                ViewBag.Name = name;

                AdminProfileVM adminProfile = new AdminProfileVM();
                DataTable dtblUserInfo = UserProfileUtils.GetAdminUserProfileInfo(profileCode);
                if (dtblUserInfo.Rows.Count == 1)
                {
                    adminProfile.ProfileCode = dtblUserInfo.Rows[0]["ProfileCode"].ToString();
                    adminProfile.ProfileName = dtblUserInfo.Rows[0]["ProfileName"].ToString();
                    adminProfile.ProfileDesc = dtblUserInfo.Rows[0]["ProfileDesc"].ToString();
                    adminProfile.ProfileStatus = dtblUserInfo.Rows[0]["ProfileStatus"].ToString();
                }
                else
                {
                    TempData["admin_messsage"] ="Profile Not Found";
                    TempData["message_class"] ="failed_info";
                    return RedirectToAction("Index", "UserRoleSetup");
                }
                List<MNMenuTable> menu = UserProfileUtils.GetAdminMenu();
                DataTable dtblUserMenuInfo = UserProfileUtils.GetAdminUserProfileMenuInfo(profileCode);
                List<MNMenuTable> SelectedMenu = ExtraUtility.DatatableToListClass<MNMenuTable>(dtblUserMenuInfo);
                foreach (var x in SelectedMenu)
                {
                    var itemToChange = menu.First(d => d.Hierarchy == x.Hierarchy).IsSelected = true;
                }
                adminProfile.MNMenus = menu;
                return View(adminProfile);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
          
        }

        [HttpPost]
        public ActionResult EditAdminProfileSetup(AdminProfileVM model)
        {
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                ViewData["userType"] = TempData["userType"];
                ViewBag.UserType = TempData["userType"];
                ViewBag.Name = name;

                bool result = false;
                string errorMessage = string.Empty;
                try
                {

                    int resultmsg = UserProfileUtils.UpdateAdminProfile(model);
                    if (resultmsg == 100)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                catch (Exception ex)
                {
                    result = false;
                    errorMessage = ex.Message;
                }
                TempData["admin_messsage"] = result
                   ? "Admin Profile information is successfully updated."
                   : "Error while inserting the information. ERROR :: "
                     + errorMessage;
                TempData["message_class"] = result ? "success_info" : "failed_info";
                if (result)
                    return RedirectToAction("Index", "UserRoleSetup");
                else
                    return View(model);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }
        #endregion

        #region ViewAdminRoleNew
        [HttpGet]
        public ActionResult ViewAdminProfileSetup(string profileCode)
        {
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                ViewData["userType"] = TempData["userType"];
                ViewBag.UserType = TempData["userType"];
                ViewBag.Name = name;

                AdminProfileVM adminProfile = new AdminProfileVM();
                DataTable dtblUserInfo = UserProfileUtils.GetAdminUserProfileInfo(profileCode);
                if (dtblUserInfo.Rows.Count == 1)
                {
                    adminProfile.ProfileCode = dtblUserInfo.Rows[0]["ProfileCode"].ToString();
                    adminProfile.ProfileName = dtblUserInfo.Rows[0]["ProfileName"].ToString();
                    adminProfile.ProfileDesc = dtblUserInfo.Rows[0]["ProfileDesc"].ToString();
                    adminProfile.ProfileStatus = dtblUserInfo.Rows[0]["ProfileStatus"].ToString();
                }
                else
                {
                    TempData["admin_messsage"] = "Profile Not Found";
                    TempData["message_class"] = "failed_info";
                    return RedirectToAction("Index", "UserRoleSetup");
                }
                List<MNMenuTable> menu = UserProfileUtils.GetAdminMenu();
                DataTable dtblUserMenuInfo = UserProfileUtils.GetAdminUserProfileMenuInfo(profileCode);
                List<MNMenuTable> SelectedMenu = ExtraUtility.DatatableToListClass<MNMenuTable>(dtblUserMenuInfo);
                foreach (var x in SelectedMenu)
                {
                    var itemToChange = menu.First(d => d.Hierarchy == x.Hierarchy).IsSelected = true;
                }
                adminProfile.MNMenus = menu;
                return View(adminProfile);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }
        #endregion


        public bool CheckAdminProfile(string ProfileName)
        {
            if (!String.IsNullOrEmpty(ProfileName))
            {

                DataTable ProfileInfo = UserProfileUtils.GetAdminUserProfile(ProfileName);
                if (ProfileInfo.Rows.Count == 0)
                {
                    return true;
                }
                else
                {

                    return false;
                }
            }
            return false;
        }

    }
}