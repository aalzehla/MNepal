using MNSuperadmin.Models;
using MNSuperadmin.Settings;
using MNSuperadmin.UserModels;
using MNSuperadmin.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;

namespace MNSuperadmin.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class LimitSetupController : Controller
    {
        #region View WalletLimit
        // GET: WalletLimitSetup

        public ActionResult Index()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                List<UserProfilesInfo> userProfilesList = new List<UserProfilesInfo>();
                DataTable dtWalletLimit = WalletLimitUserModel.GetAllWalletInfo();

                foreach (DataRow row in dtWalletLimit.Rows)
                {
                    UserProfilesInfo userobj = new UserProfilesInfo
                    {
                        WalletProfileCode = row["WalletProfileCode"].ToString(),
                        //WFeatureCode = row["WFeatureCode"].ToString(),
                        WTxnCount = row["WTxnCount"].ToString(),
                        WPerTxnAmt = row["WPerTxnAmt"].ToString(),
                        WPerDayAmt = row["WPerDayAmt"].ToString(),
                        WTxnAmtM = row["WTxnAmtM"].ToString(),
                        //WTxnCode = row["WTxnCode"].ToString()
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
        #endregion


        #region Edit WalletLimit
        //GET: WalletLimitSetup/Edit
        [HttpGet]
        public ActionResult EditLimitSetup(string walletProfileCode)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            if (this.TempData["editWalletmodify_messsage"] != null)
            {
                this.ViewData["editWalletmodify_messsage"] = this.TempData["editWalletmodify_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                UserProfilesInfo userobj = new UserProfilesInfo();
                DataSet DSet = WalletLimitUtils.GetWalletInfoDS(walletProfileCode);

                DataTable dtWalletLimitInfo = DSet.Tables["dtWalletInfo"];

                if (dtWalletLimitInfo.Rows.Count == 1)
                {
                    userobj.WalletProfileCode = dtWalletLimitInfo.Rows[0]["WalletProfileCode"].ToString();
                    userobj.WTxnCount = dtWalletLimitInfo.Rows[0]["WTxnCount"].ToString();
                    userobj.WPerTxnAmt = dtWalletLimitInfo.Rows[0]["WPerTxnAmt"].ToString();
                    userobj.WPerDayAmt = dtWalletLimitInfo.Rows[0]["WPerDayAmt"].ToString();
                    userobj.WTxnAmtM = dtWalletLimitInfo.Rows[0]["WTxnAmtM"].ToString();

                    if (userType != "superadmin")
                    {
                        this.TempData["editregister_messsage"] = "No such SuperAdmin exists";
                        this.TempData["message_class"] = CssSetting.FailedMessageClass;
                        return RedirectToAction("Index", "SuperAdmin");
                    }
                }
                else
                {
                    this.TempData["editLimit_messsage"] = "No such SuperAdmin exists";
                    this.TempData["message_class"] = CssSetting.FailedMessageClass;
                }
                
                return View(userobj);

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        //POST: WalletLimitSetup/Edit
        [HttpPost]
        public ActionResult EditLimitSetup(string btnCommand, FormCollection collection)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            string displayMessage = null;
            string messageClass = null;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;
                bool isUpdated = false;
                UserProfilesInfo userProfilesInfo = new UserProfilesInfo();
                try
                {
                    if (btnCommand == "Submit")
                    {
                        string walletProfileCode = collection["WalletProfileCode"].ToString();
                        string wTxnCount = collection["WTxnCount"].ToString();
                        string wPerTxnAmt = collection["WPerTxnAmt"].ToString();
                        string wPerDayAmt = collection["WPerDayAmt"].ToString();
                        string wTxnAmtM = collection["WTxnAmtM"].ToString();


                        /*New Data*/
                        userProfilesInfo.WalletProfileCode = walletProfileCode;
                        userProfilesInfo.WTxnCount = wTxnCount;
                        userProfilesInfo.WPerTxnAmt = wPerTxnAmt;
                        userProfilesInfo.WPerDayAmt = wPerDayAmt;
                        userProfilesInfo.WTxnAmtM = wTxnAmtM;

                        if ((wTxnCount != "") && (walletProfileCode != "") && (wPerTxnAmt != "") && (wPerDayAmt != "") && (wTxnAmtM != ""))
                        {

                            isUpdated = WalletLimitUtils.UpdateWalletInfo(userProfilesInfo);
                            displayMessage = isUpdated
                                                     ? "Wallet Limit successfully updated."
                                                     : "Error while updating Agent Information";
                            messageClass = isUpdated ? CssSetting.SuccessMessageClass : CssSetting.FailedMessageClass;
                        }
                        else
                        {
                            displayMessage = "Required Field is Empty";
                            messageClass = CssSetting.FailedMessageClass;
                        }

                        this.TempData["editWalletmodify_messsage"] = displayMessage;
                        this.TempData["message_class"] = messageClass;
                        return RedirectToAction("EditLimitSetup");

                    }
                }
                
                catch(Exception ex)
                {
                   
                }
                finally
                {

                }
            }

            return View();
        }

        #endregion
    }
}