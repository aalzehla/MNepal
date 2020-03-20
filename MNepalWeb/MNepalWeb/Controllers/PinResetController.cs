using MNepalWeb.Models;
using MNepalWeb.Settings;
using MNepalWeb.Utilities;
using System;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.SessionState;

namespace MNepalWeb.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class PinResetController : Controller
    {
        // GET: PinReset
        public ActionResult Index()
        {
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

                            displayMessage = isUpdated
                                                     ? "PIN has successfully been updated."
                                                     : "ERROR:: Please Enter Correct old pin number!";
                            messageClass = isUpdated ? CssSetting.SuccessMessageClass : CssSetting.FailedMessageClass;
                            if (isUpdated)
                            {
                                FormsAuthentication.SignOut();
                                Session.Abandon();//Clear the session
                                return RedirectToAction("Index", "Login");
                            }
                        }
                    }
                    else
                    {
                        displayMessage = "Pin is Empty";
                        messageClass = CssSetting.FailedMessageClass;

                    }

                }
                catch (Exception ex)
                {
                    displayMessage = ex.Message;
                    messageClass = CssSetting.FailedMessageClass;
                }

                this.TempData["pin_messsage"] = displayMessage;
                this.TempData["message_class"] = messageClass;
                return RedirectToAction("Index");

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

    }
}