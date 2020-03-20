using MNSuperadmin.Helper;
using MNSuperadmin.Models;
using MNSuperadmin.Settings;
using MNSuperadmin.Utilities;
using System;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.SessionState;

namespace MNSuperadmin.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class ChangePasswordController : Controller
    {
        // GET: ChangePassword
        [HttpGet]
        public ActionResult Index()
        {
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            if (this.TempData["password_messsage"] != null)
            {
                this.ViewData["password_messsage"] = this.TempData["password_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            TempData["userType"] = userType;
            //Check Role link start
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkIndexRole(System.Reflection.MethodBase.GetCurrentMethod().Name, clientCode, this.ControllerContext.RouteData.Values["controller"].ToString());
            //Check Role link end
            if (TempData["userType"] != null&& checkRole)
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

        // POST: ChangePassword/Index/1
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
                string oldPwd = collection["txtOldPwd"].ToString();
                string newPwd = collection["txtNewPwd"].ToString();
                string retypePwd = collection["retypePassword"].ToString();
				
				userInfo.OPassword = oldPwd;
                userInfo.Password = newPwd;
                userInfo.ClientCode = clientCode;

                string displayMessage = null;
                string messageClass = null;

                try {
                  
                   
                    if ((newPwd != "") && (retypePwd != "") && (oldPwd != ""))
                    {
                        PasswordValidator validate = new PasswordValidator();
                        if (newPwd != retypePwd)
                        {
                            displayMessage = "New password and re-typed password do not match";
                            messageClass = CssSetting.FailedMessageClass;
                        }
                       else 
                        if (!validate.IsValid(newPwd))
                        {
                            displayMessage = "Password should be of 8-12 characters long which should include 1 uppercase & 1 number";
                            messageClass = CssSetting.FailedMessageClass;
                        }
                        else
                        if (newPwd == retypePwd)
                        {
                            bool isUpdated = PasswordUtils.UpdateUserPasswordInfo(userInfo);
                            displayMessage = isUpdated
                                                     ? "Password Information has successfully been updated."
                                                     : "Error :: Please Enter Correct old Password.";
                            messageClass = isUpdated ? CssSetting.SuccessMessageClass : CssSetting.FailedMessageClass;
                            if(isUpdated)
                            {
                                FormsAuthentication.SignOut();
                                Session.Abandon();//Clear the session
                                return RedirectToAction("Index", "Login");
                            }


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
                    displayMessage = ex.Message;
                    messageClass = CssSetting.FailedMessageClass;
                }

                this.TempData["password_messsage"] = displayMessage;
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