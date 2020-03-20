using ThailiMNepalAgent.Models;
using ThailiMNepalAgent.Settings;
using ThailiMNepalAgent.Utilities;
using System;
using System.Data;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.SessionState;

namespace ThailiMNepalAgent.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class PinResetController : Controller
    {
        // GET: PinReset
        public ActionResult Index()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            if (this.TempData["pin_messsage"] != null)
            {
                this.ViewData["pin_messsage"] = this.TempData["pin_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                ///start milayako
                MNBalance availBaln = new MNBalance();
                DataTable dtableUser1 = AvailBalnUtils.GetAvailBaln(clientCode);
                if (dtableUser1 != null && dtableUser1.Rows.Count > 0)
                {
                    availBaln.amount = dtableUser1.Rows[0]["AvailBaln"].ToString();

                    ViewBag.AvailBalnAmount = availBaln.amount;
                }

                //end milayako
                UserInfo userInfo = new UserInfo();
                DataSet DSet = ProfileUtils.GetCusDetailProfileInfoDS(clientCode);

                DataTable dtableUser = DSet.Tables["dtUserInfo"];
                DataTable dKYC = DSet.Tables["dtKycDetail"];
                DataTable dDoc = DSet.Tables["dtKycDoc"];
                if (dtableUser != null && dtableUser.Rows.Count > 0)
                {
                    userInfo.ContactNumber1 = dtableUser.Rows[0]["ContactNumber1"].ToString();
                    userInfo.ContactNumber2 = dtableUser.Rows[0]["ContactNumber2"].ToString();
                   
                    ViewBag.ContactNumber1 = userInfo.ContactNumber1;
                    ViewBag.ContactNumber2 = userInfo.ContactNumber2;                    
                    ViewBag.SelfReg = userInfo.SelfRegistered;
                    //added//
                }

                if (dKYC != null && dKYC.Rows.Count > 0)
                {
                    userInfo.CustStatus = dKYC.Rows[0]["CustStatus"].ToString();

                    ViewBag.CustStatus = userInfo.CustStatus;
                }

                if (dDoc != null && dDoc.Rows.Count > 0)
                {
                    userInfo.Document = dDoc.Rows[0]["DocType"].ToString();
                    userInfo.FrontImage = dDoc.Rows[0]["FrontImage"].ToString();
                    userInfo.BackImage = dDoc.Rows[0]["BackImage"].ToString();
                    userInfo.PassportImage = dDoc.Rows[0]["PassportImage"].ToString();

                    ViewBag.DocType = userInfo.Document;
                    ViewBag.FrontImage = userInfo.FrontImage;
                    ViewBag.BackImage = userInfo.BackImage;
                    ViewBag.PassportImage = userInfo.PassportImage;

                }

                //Check Link Bank Account
                string HasBankKYC = string.Empty;
                DataTable dtableUserCheckLinkBankAcc = ProfileUtils.CheckLinkBankAcc(userName);
                if (dtableUserCheckLinkBankAcc != null && dtableUserCheckLinkBankAcc.Rows.Count > 0)
                {
                    userInfo.BankAccountNumber = dtableUserCheckLinkBankAcc.Rows[0]["HasBankKYC"].ToString();

                    ViewBag.HasBankKYC = userInfo.BankAccountNumber;
                    HasBankKYC = ViewBag.HasBankKYC;

                }
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        // POST: PinReset
        [HttpPost]
        public ActionResult Index(FormCollection collection)
        {
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                UserInfo userInfo = new UserInfo();
                string oldPin = collection["txtOldPin"].ToString();
                string newPin = collection["txtNewPin"].ToString();
                string retypePin = collection["txtRetypePin"].ToString();
				
				userInfo.OPIN = oldPin;
                userInfo.PIN = newPin;
                userInfo.ClientCode = clientCode;

                string displayMessage = null;
                string messageClass = null;

                try
                {
                    if ((newPin != "") && (retypePin != "") && (oldPin != ""))
                    {
                        if (newPin == retypePin)
                        {
                            bool isUpdated = PinUtils.UpdateUserPinInfo(userInfo);
                            
                            if (isUpdated)
                            {
                                displayMessage = isUpdated
                                                     ? "Your T-PIN has been successfully changed."/*PIN has successfully been updated.*/
                                                     : "Your T-PIN doesn't match to the New T-PIN."; /*ERROR::Please Enter Correct old pin number!*/
                                messageClass = isUpdated ? CssSetting.SuccessMessageClass : CssSetting.FailedMessageClass;
                                FormsAuthentication.SignOut();
                                Session.RemoveAll();
                                //Session.Abandon();//Clear the session
                                //return RedirectToAction("Index", "Login");
                            }
                            else
                            {
                                displayMessage = "Your T-PIN doesn't match to the Old T-PIN.";
                                messageClass = CssSetting.FailedMessageClass;
                                this.TempData["pin_messsage"] = displayMessage;
                                this.TempData["message_class"] = messageClass;
                                Session.Timeout = 1;
                                return RedirectToAction("Index", "PinReset");
                            }
                        }
                    }
                    else
                    {
                        displayMessage = "Pin is Empty";
                        messageClass = CssSetting.FailedMessageClass;
                        this.TempData["pin_messsage"] = displayMessage;
                        this.TempData["message_class"] = messageClass;
                        Session.Timeout = 1;
                        return RedirectToAction("Index", "PinReset");

                    }

                }
                catch (Exception ex)
                {
                    displayMessage = ex.Message;
                    messageClass = CssSetting.FailedMessageClass;
                }

                this.TempData["login_message"] = displayMessage;
                this.TempData["message_class"] = messageClass;
                Session.Timeout = 1;
                return RedirectToAction("Index", "Login");

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

    }
}