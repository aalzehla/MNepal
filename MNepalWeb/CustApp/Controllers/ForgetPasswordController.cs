using CustApp.Helper;
using CustApp.Models;
using CustApp.Settings;
using CustApp.Utilities;
using System;
using System.Data;
using System.Net;
using System.Web.Mvc;
using System.Web.SessionState;

namespace CustApp.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class ForgetPasswordController : Controller
    {
        // GET: ForgetPassword
        [HttpGet]
        public ActionResult Index()
        {
            if (this.TempData["forget_message"] != null)
            {
                this.ViewData["forget_message"] = this.TempData["forget_message"];
                this.ViewData["message_class"] = this.TempData["message_class"];
                this.ViewData["message_topic"] = this.TempData["message_topic"];
            }
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Index(FormCollection collection)
        {
            string userName = collection["txtUserName"] ?? string.Empty;
            string verify = collection["txtVerify"] ?? string.Empty;
            string displayMessage = null;
            string messageClass = string.Empty;
            string messageTopic = string.Empty;
            string code = string.Empty;
            string mobile = string.Empty;
            bool result = false;


            UserInfo userInfo = new UserInfo();
            DataTable dtableGetUserInfo = ProfileUtils.GetCustomerName(userName);
            if (dtableGetUserInfo != null && dtableGetUserInfo.Rows.Count > 0)
            {
                userInfo.FName = dtableGetUserInfo.Rows[0]["FName"].ToString();

                ViewBag.FName = userInfo.FName;
            }

            try
            {
                if (IsValidUserName(userName))
                { 
                    code = TraceIdGenerator.GetUniqueKey();
                    string messagereply = "Dear " + ViewBag.FName + "," + "\n";
                    messagereply += " Your Verification Code is " + code
                        + "." + "\n" + "Close this message and enter code to recover account.";
                    messagereply += "-MNepal";

                    var client = new WebClient();

                    mobile = userName;
                    //var content = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                    //    + "977" + mobile + "&Text=" + messagereply + "");

                    if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                    {
                        //FOR NCELL
                        var contents = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                        + "977" + mobile + "&Text=" + messagereply + "");
                    }
                    else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                        || (mobile.Substring(0, 3) == "986"))
                    {
                        //FOR NTC
                        var contents = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                            + "977" + mobile + "&Text=" + messagereply + "");
                    }

                    SMSLog log = new SMSLog();
                    log.SentBy = mobile;
                    log.Purpose = "Reset";
                    log.UserName = userName;
                    log.Message = messagereply;
                    CustomerUtils.LogSMS(log);

                    Session["Mobile"] = mobile;
                    Session["Code"] = code;
                    return RedirectToAction("Recover"); 
                }
                else
                {
                    displayMessage = "\n Please enter a valid mobile number.";
                    messageClass = CssSetting.FailedMessageClass;
                    messageTopic = "Unregistered number";
                }

            }
            catch (Exception ex)
            {
                displayMessage = ex.Message + "\n Please Contact to the administrator!";
                messageClass = CssSetting.FailedMessageClass;
                messageTopic = "Error";
            }
            this.TempData["forget_message"] = result
                                                  ? "" + displayMessage
                                                  :displayMessage;
            this.TempData["message_class"] = result ? "success_info" : "failed_info";
            this.TempData["message_topic"] = messageTopic;
            return View("Index");
        }

        [HttpGet]
        public bool IsValidUserName(string username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                DataTable dtCheckUserName = RegisterUtils.GetCheckUserName(username);
                if (dtCheckUserName.Rows.Count == 1)
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

        [HttpGet]
        public ActionResult GetCheckingUserName(string username)
        {
            //if ((username != "") || (username != null))
            //{

            //    string result = string.Empty;


            //        DataTable dtCheckUserName = RegisterUtils.GetCheckUserName(username);
            //        if (dtCheckUserName.Rows.Count == 0)
            //        {
            //            result = "Not Registered";
            //            return Json(result, JsonRequestBehavior.AllowGet);
            //        }
            //        else
            //        {
            //            result = "Success";
            //            return Json(result, JsonRequestBehavior.AllowGet);
            //        }




            //}
            //     return RedirectToAction("ChangePassword");
            string result = string.Empty;
            if ((username != "") || (username != null))
            {
                DataTable dtableBlockUserWrongPwd = LoginUtils.GetBlockTime(username);


                DataTable dtableMobileNo = RegisterUtils.GetCheckMobileNo(username);
                if (dtableMobileNo.Rows.Count == 1)
                {
                    if (dtableBlockUserWrongPwd != null && dtableBlockUserWrongPwd.Rows.Count > 0)
                    {
                        result = "Success";
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        result = "Blocked";
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    result = "Fail";
                    string messageTopic = "Error!";
                    ViewData["message_topic"] = messageTopic;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

            }
            else
                return RedirectToAction("ChangePassword");
        }


        // GET: ForgetPassword/Recover
        [HttpGet]
        public ActionResult Recover()
        {
            if (this.TempData["recover_message"] != null)
            {
                this.TempData["recover_message"] = this.TempData["recover_message"];
                this.TempData["message_class"] = this.TempData["message_class"];
            }
            string userName = (string)Session["Mobile"];
            string code = (string)Session["Code"];
            return View("Recover");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Recover(FormCollection collection)
        {
            string verificationCode = collection["txtVerificationCode"] ?? string.Empty;
            string mobile = (string)Session["Mobile"];
            string code = (string)Session["Code"];

            UserInfo userInfo = new UserInfo();
            userInfo.UserName = mobile;

            bool result = false;
            string displayMessage = null;
            string messageClass = null;

            try
            {
                if (verificationCode == code)
                {
                    return RedirectToAction("ChangePassword");
                }
                else
                {
                    displayMessage = "Enter the code received via SMS";
                    messageClass = CssSetting.FailedMessageClass;
                }
            }
            catch (Exception ex)
            {
                displayMessage = ex.Message + "\n Please Contact to the administrator!";
                messageClass = CssSetting.FailedMessageClass;
            }
            this.TempData["recover_message"] = result
                                                  ? "" + displayMessage
                                                  : displayMessage;
            this.TempData["message_class"] = result ? "success_info" : "failed_info";
            return View("Recover");
        }

        // GET: ForgetPassword/ChangePassword
        [HttpGet]
        public ActionResult ChangePassword()
        {
            if (this.TempData["changepwd_message"] != null)
            {
                this.TempData["changepwd_message"] = this.TempData["changepwd_message"];
                this.TempData["message_class"] = this.TempData["message_class"];
            }
            return View("ChangePassword");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(FormCollection collection)
        {
            string password = collection["txtNewPassword"] ?? string.Empty;
            string mobile = (string)Session["Mobile"];
            string RePassword = collection["txtCNewPassword"] ?? string.Empty;
            UserInfo userInfo = new UserInfo();
            userInfo.UserName = mobile;
            userInfo.Password = password;

            bool result = false;
            string displayMessage = null;
            string messageClass = null;

            try
            {

                if ((mobile != "null") || (mobile != "") && (password != ""))
                {
                    if (RePassword == password)
                    {
                        PasswordValidator validate = new PasswordValidator();
                        if (!validate.IsValid(password))
                        {
                            displayMessage = "New Password must be of  8-12 characters long which should include 1 uppercase & 1 number";
                            messageClass = CssSetting.FailedMessageClass;
                        }
                        else
                        {
                            bool isUpdated = PasswordUtils.UpdateForgetPasswordInfo(userInfo);
                            result = isUpdated;
                            displayMessage = isUpdated
                                                     ? "Your password has been successfully changed "
                                                     : "Error while updating Password information";
                            messageClass = isUpdated ? CssSetting.SuccessMessageClass : CssSetting.FailedMessageClass;
                            if (isUpdated)
                            {
                                this.TempData["login_message"] = result
                                                 ? "" + displayMessage
                                                 : "Error:: " + displayMessage;
                                this.TempData["message_class"] = result ? "success_info" : "failed_info";
                                return RedirectToAction("Index", "Login");
                            }
                        }
                    }
                    else
                    {
                        displayMessage = "Password and re typed password do not match";
                        messageClass = CssSetting.FailedMessageClass;
                    }
                }
                else
                {
                    displayMessage = "Password is Empty";
                    messageClass = CssSetting.FailedMessageClass;
                }
            }
            catch (Exception ex)
            {
                displayMessage = ex.Message + "\n Please Contact to the administrator !!";
                messageClass = CssSetting.FailedMessageClass;
            }

            this.TempData["changepwd_message"] = result
                                                  ? "" + displayMessage
                                                  : "Error:: " + displayMessage;
            this.TempData["message_class"] = result ? "success_info" : "failed_info";
            return RedirectToAction("ChangePassword");
        }

        
        public ActionResult ResendCode(FormCollection collection){
            if (this.TempData["forget_message"] != null)
            {
                this.ViewData["forget_message"] = this.TempData["forget_message"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }


            string userName = (string)Session["Mobile"];
            string verify = collection["txtVerify"] ?? string.Empty;
            string displayMessage = null;
            string messageClass = null;
            string code = string.Empty;
            string mobile = string.Empty;
            bool result = false;

            UserInfo userInfo = new UserInfo();
            DataTable dtableGetUserInfo = ProfileUtils.GetCustomerName(userName);
            if (dtableGetUserInfo != null && dtableGetUserInfo.Rows.Count > 0)
            {
                userInfo.FName = dtableGetUserInfo.Rows[0]["FName"].ToString();

                ViewBag.FName = userInfo.FName;
            }

            try
            {
                if (IsValidUserName(userName))
                {
                    code = TraceIdGenerator.GetUniqueKey();
                    string messagereply = "Dear " + ViewBag.FName +"," + "\n";
                    messagereply += " Your Verification Code is " + code
                        + "." + "\n" + "Close this message and enter code to recover account.";
                    messagereply += "-MNepal";

                    var client = new WebClient();

                    mobile = userName;
                    //var content = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                    //    + "977" + mobile + "&Text=" + messagereply + "");

                    if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                    {
                        //FOR NCELL
                        var contents = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                        + "977" + mobile + "&Text=" + messagereply + "");
                    }
                    else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                        || (mobile.Substring(0, 3) == "986"))
                    {
                        //FOR NTC
                        var contents = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                            + "977" + mobile + "&Text=" + messagereply + "");
                    }

                    SMSLog log = new SMSLog();
                    log.SentBy = mobile;
                    log.Purpose = "Reset";
                    log.UserName = userName;
                    log.Message = messagereply;
                    CustomerUtils.LogSMS(log);

                    Session["Mobile"] = mobile;
                    Session["Code"] = code;
                    return RedirectToAction("Recover");
                }
                else
                {
                    displayMessage = "\n The number is not registered !!";
                    messageClass = CssSetting.FailedMessageClass;
                }

            }
            catch (Exception ex)
            {
                displayMessage = ex.Message + "\n Please Contact to the administrator !!";
                messageClass = CssSetting.FailedMessageClass;
            }
            this.ViewData["forget_message"] = result
                                                  ? "" + displayMessage
                                                  : "Error:: " + displayMessage;
            this.ViewData["message_class"] = result ? "success_info" : "failed_info";
            return View("Index");
        }
    }
}