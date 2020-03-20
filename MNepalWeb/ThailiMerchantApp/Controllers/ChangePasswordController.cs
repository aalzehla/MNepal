using ThailiMerchantApp.Models;
using ThailiMerchantApp.Settings;
using ThailiMerchantApp.Utilities;
using System;
using System.Data;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.SessionState;
using System.Threading.Tasks;
using System.Net.Http;
using ThailiMerchantApp.Helper;
using System.IO;
using ThailiMerchantApp.App_Start;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;


namespace ThailiMerchantApp.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class ChangePasswordController : Controller
    {
        // GET: ChangePassword
        [HttpGet]
        public ActionResult Index()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            if (this.TempData["password_messsage"] != null)
            {
                this.ViewData["password_messsage"] = this.TempData["password_messsage"];
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
                            displayMessage = "Password should be of 5-12 characters long which should include 1 uppercase & 1 number";
                            messageClass = CssSetting.FailedMessageClass;
                        }
                        else
                        if (newPwd == retypePwd)
                        {
                            bool isUpdated = PasswordUtils.UpdateUserPasswordInfo(userInfo);
                            displayMessage = isUpdated
                                                     ? "Your Password has successfully been changed. " /*Password Information has successfully been updated.*/
                                                     : "Your Password doesn't match the old password. "; /*Error::Please Enter Correct old Password.*/
                           messageClass = isUpdated ? CssSetting.SuccessMessageClass : CssSetting.FailedMessageClass;
                            if(isUpdated)
                            {
                                FormsAuthentication.SignOut();
                                Session.RemoveAll();
                                //Session.Abandon();//Clear the session
                                //return RedirectToAction("Index", "Login");
                            }
                            else
                            {
                                this.TempData["password_messsage"] = displayMessage;
                                this.TempData["message_class"] = messageClass;
                                Session.Timeout = 1;
                                return RedirectToAction("Index", "ChangePassword");

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