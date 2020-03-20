using MNepalWeb.Helper;
using MNepalWeb.Models;
using MNepalWeb.Settings;
using MNepalWeb.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Web.SessionState;

namespace MNepalWeb.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class SuperAdminController : Controller
    {
        public ActionResult GetCheckingUserName(string username)
        {
            if ((username != "") || (username != null))
            {
                string result = string.Empty;
                DataTable dtableMobileNo = RegisterUtils.GetCheckUserName(username);
                if (dtableMobileNo.Rows.Count == 0)
                {
                    result = "Success";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    result = "Already Register username";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            return RedirectToAction("Registration");
        }


        #region "SuperAdmin Registration"

        // GET: SuperAdmin/Registration
        public ActionResult Registration()
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

                if (this.TempData["registration_message"] != null)
                {
                    this.ViewData["registration_message"] = this.TempData["registration_message"];
                    this.ViewData["message_class"] = this.TempData["message_class"];
                }

                ViewBag.User = "superadmin";

                string t = string.Empty;
                DataTable dtClientCode = RegisterUtils.GetClientCode();
                if (dtClientCode.Rows.Count == 1)
                {
                    string stringclientCode = dtClientCode.Rows[0]["ClientCode"].ToString();
                    int fixedlength = 8;

                    int maxclientCode = Convert.ToInt32(stringclientCode) + 1;
                    t = maxclientCode.ToString("D" + fixedlength); // magic

                    ViewBag.CClientCode = t;
                }

                //For Bank
                List<SelectListItem> item = new List<SelectListItem>();
                DataSet dsBank = BankUtils.GetDataSetPopulateBankCode();
                ViewBag.bank = dsBank.Tables[0];
                foreach (DataRow dr in ViewBag.bank.Rows)
                {
                    item.Add(new SelectListItem
                    {
                        Text = @dr["BankName"].ToString(),
                        Value = @dr["BankCode"].ToString()
                    });
                }
                //ViewBag.Wallet = item;
                ViewBag.Wallet = item[0].Text.ToString();
                ViewBag.WalletCode = item[0].Value.ToString();
                ViewBag.Bank = item;

                string bankCode = item[0].Value.ToString();
                string unique = TraceIdGenerator.GetUniqueWalletNo();
                ViewBag.WalletNumber = bankCode + t + unique;

                return this.View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        public ActionResult GetPopulateBankCode(string BankCode)
        {
            if ((BankCode != "") || (BankCode != null))
            {
                DataSet dsBank = BankUtils.GetDataSetPopulateBranchCode(BankCode);
                ViewBag.bank = dsBank.Tables[0];
                List<SelectListItem> item = new List<SelectListItem>();

                foreach (DataRow dr in ViewBag.bank.Rows)
                {
                    item.Add(new SelectListItem
                    {
                        Value = @dr["BranchCode"].ToString(),
                        Text = @dr["BranchCode"].ToString()
                    });
                }
                ViewBag.BranchCode = item;

                return Json(item, JsonRequestBehavior.AllowGet);
            }
            else
                return View();

        }


        // POST: SuperAdmin/Registration
        [HttpPost]
        public ActionResult Registration(FormCollection collection)
        {
            try
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

                    string fName = collection["txtFirstName"];

                    UserInfo userInfo = new UserInfo
                    {
                        UserType = collection["txtUserType"],
                        UserName = collection["txtUserName"],
                        Password = collection["txtPassword"],
                        Name = fName,
                        Address = string.Empty,
                        PIN = string.Empty,
                        Status = collection["txtStatus"],
                        ContactNumber1 = collection["txtContactNumber1"],
                        ContactNumber2 = string.Empty,
                        EmailAddress = collection["txtUserName"],
                        ClientCode = collection["txtCClientCode"],
                        IsApproved = collection["txtIsApproved"],
                        IsRejected = collection["txtIsRejected"],
                        WalletNumber = collection["txtWalletNumber"],
                        WBankCode = collection["WalletCode"],
                        WBranchCode = collection["WBranchCode"],
                        WIsDefault = collection["txtWIsDefault"],
                        AgentId = collection["txtAgentId"],
                        BankNo = collection["BankNoBin"],
                        BankAccountNumber = collection["txtBankAccountNumber"],
                        BranchCode = string.Empty,
                        IsDefault = collection["txtIsDefault"]
                    };
                    
                   

                    //if (string.IsNullOrEmpty(collection["txtUserType"]))
                    //{
                    //    ModelState.AddModelError("UserType", "*Please enter UserType");
                    //}
                    if (string.IsNullOrEmpty(collection["txtUserName"]))
                    {
                        ModelState.AddModelError("UserName", "*Please enter UserName");
                    }
                    if (string.IsNullOrEmpty(collection["txtPassword"]))
                    {
                        ModelState.AddModelError("Password", "*Please enter Password");
                    }
                    if (string.IsNullOrEmpty(collection["txtFirstName"]))
                    {
                        ModelState.AddModelError("FirstName", "*Please enter First Name");
                    }
                    //if (string.IsNullOrEmpty(collection["txtLastName"]))
                    //{
                    //    ModelState.AddModelError("LastName", "*Please enter Last Name");
                    //}
                    //if (string.IsNullOrEmpty(collection["txtAddress"]))
                    //{
                    //    ModelState.AddModelError("Address", "*Please enter Address");
                    //}
                    //if (string.IsNullOrEmpty(collection["txtWardNo"]))
                    //{
                    //    ModelState.AddModelError("WardNo", "*Please enter WardNo");
                    //}
                    //if (string.IsNullOrEmpty(collection["txtDistrict"]))
                    //{
                    //    ModelState.AddModelError("District", "*Please enter District");
                    //}
                    //if (string.IsNullOrEmpty(collection["txtZone"]))
                    //{
                    //    ModelState.AddModelError("Zone", "*Please enter Zone");
                    //}
                    //if (string.IsNullOrEmpty(collection["txtPin"]))
                    //{
                    //    ModelState.AddModelError("Pin", "*Please enter Pin");
                    //}
                    //if (string.IsNullOrEmpty(collection["txtStatus"]))
                    //{
                    //    ModelState.AddModelError("Status", "*Please enter Status");
                    //}
                    //if (string.IsNullOrEmpty(collection["txtContactNumber1"]))
                    //{
                    //    ModelState.AddModelError("ContactNumber", "*Please enter Contact Number");
                    //}
                    //if (string.IsNullOrEmpty(collection["txtIsApproved"]))
                    //{
                    //    ModelState.AddModelError("IsApproved", "*Please enter IsApproved");
                    //}
                    //if (string.IsNullOrEmpty(collection["txtIsRejected"]))
                    //{
                    //    ModelState.AddModelError("IsRejected", "*Please enter IsRejected");
                    //}
                    //if (string.IsNullOrEmpty(collection["txtWalletNumber"]))
                    //{
                    //    ModelState.AddModelError("WalletNumber", "*Please enter WalletNumber");
                    //}
                    //if (string.IsNullOrEmpty(collection["WalletCode"]))
                    //{
                    //    ModelState.AddModelError("WalletCode", "*Please enter txtWBankCode");
                    //}
                    //if (string.IsNullOrEmpty(collection["WBranchCode"]))
                    //{
                    //    ModelState.AddModelError("WBranchCode", "*Please enter WBranchCode");
                    //}
                    //if (string.IsNullOrEmpty(collection["txtWIsDefault"]))
                    //{
                    //    ModelState.AddModelError("txtWIsDefault", "*Please enter WIsDefault");
                    //}
                    //if (string.IsNullOrEmpty(collection["txtAgentId"]))
                    //{
                    //    ModelState.AddModelError("AgentId", "*Please enter AgentId");
                    //}
                    //if (string.IsNullOrEmpty(collection["BankNoBin"]))
                    //{
                    //    ModelState.AddModelError("BankNoBin", "*Please enter BankCode");
                    //}
                    //if (string.IsNullOrEmpty(userInfo.BranchCode))//collection["txtBranchCode"]))
                    //{
                    //    ModelState.AddModelError("BranchCode", "*Please enter BranchCode");
                    //}
                    //if (string.IsNullOrEmpty(collection["txtIsDefault"]))
                    //{
                    //    ModelState.AddModelError("IsDefault", "*Please enter IsDefault");
                    //}

                    if (!ViewData.ModelState.IsValid)
                    {
                        this.ViewData["registration_message"] = " *Validation Error.";
                        this.ViewData["message_class"] = "failed_info";
                        return View();
                    }

                    bool result = false;
                    string errorMessage = string.Empty;
                    if ((userInfo.UserName != "") && (userInfo.Password != ""))
                    {
                        DataTable dtableMobileNo = RegisterUtils.GetCheckUserName(userInfo.UserName);
                        if (dtableMobileNo.Rows.Count == 0)
                        {
                            if (collection.AllKeys.Any())
                            {
                                try
                                {
                                    int results = RegisterUtils.RegisterSAdminInfo(userInfo);
                                    if (results > 0)
                                    {
                                        result = true;
                                        //int resultmsg = RegisterUtils.CreateWalletAcInfo(userInfo);
                                       // if (resultmsg == 100)
                                      //  {
                                       // }
                                       // else
                                      //  {
                                       //     result = false;
                                       // }
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
                            }
                        }
                        else if (dtableMobileNo != null && dtableMobileNo.Rows.Count > 0)
                        {
                            string checkMobileNo = dtableMobileNo.Rows[0]["ContactNumber1"].ToString();
                            result = false;
                            errorMessage = "Already Register username";
                        }
                    }
                    else
                    {
                        result = false;
                    }

                    this.TempData["registration_message"] = result
                                                  ? "Registration information is successfully added."
                                                  : "Error while inserting the information. ERROR :: "
                                                    + errorMessage;
                    this.TempData["message_class"] = result ? "success_info" : "failed_info";

                    //For Bank
                    List<SelectListItem> item = new List<SelectListItem>();
                    DataSet dsBank = BankUtils.GetDataSetPopulateBankCode();
                    ViewBag.bank = dsBank.Tables[0];
                    foreach (DataRow dr in ViewBag.bank.Rows)
                    {
                        item.Add(new SelectListItem
                        {
                            Text = @dr["BankName"].ToString(),
                            Value = @dr["BankCode"].ToString()
                        });
                    }
                    //ViewBag.Wallet = item;
                    ViewBag.Bank = item;

                    return this.RedirectToAction("Registration");
                }
                else
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            catch
            {
                return View();
            }
        }


        #endregion


        #region "SuperAdmin View Details"

        // GET: SuperAdmin
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

                List<UserInfo> registrationtList = new List<UserInfo>();
                DataTable dtblRegistration = ProfileUtils.GetSuperAdminProfile();

                foreach (DataRow row in dtblRegistration.Rows)
                {
                    UserInfo regobj = new UserInfo
                    {
                        ClientCode = row["ClientCode"].ToString(),
                        Name = row["Name"].ToString(),
                        Address = row["Address"].ToString(),
                        PIN = row["PIN"].ToString(),
                        Status = row["Status"].ToString(),
                        ContactNumber1 = row["ContactNumber1"].ToString(),
                        ContactNumber2 = row["ContactNumber2"].ToString(),
                        EmailAddress = row["EmailAddress"].ToString(),
                        UserName = row["UserName"].ToString(),
                        UserType = row["userType"].ToString()
                    };

                    registrationtList.Add(regobj);
                }
                return View(registrationtList);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }


        // GET: SuperAdmin/EditRegistration
        [HttpGet]
        public ActionResult EditRegistration(string clientCodeID)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (this.TempData["register_messsage"] != null)
            {
                this.ViewData["register_messsage"] = this.TempData["register_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                UserInfo userInfo = new UserInfo();
                DataTable dtblRegistration = ProfileUtils.GetUserProfileInfo(clientCodeID);
                if (dtblRegistration.Rows.Count == 1)
                {
                    userInfo.ClientCode = dtblRegistration.Rows[0]["ClientCode"].ToString();
                    userInfo.Name = dtblRegistration.Rows[0]["Name"].ToString();
                    userInfo.Address = dtblRegistration.Rows[0]["Address"].ToString();
                    userInfo.PIN = dtblRegistration.Rows[0]["PIN"].ToString();
                    userInfo.Status = dtblRegistration.Rows[0]["Status"].ToString();
                    userInfo.ContactNumber1 = dtblRegistration.Rows[0]["ContactNumber1"].ToString();
                    userInfo.ContactNumber2 = dtblRegistration.Rows[0]["ContactNumber2"].ToString();
                    userInfo.EmailAddress = dtblRegistration.Rows[0]["EmailAddress"].ToString();
                    userInfo.UserName = dtblRegistration.Rows[0]["UserName"].ToString();
                    userInfo.UserType = dtblRegistration.Rows[0]["UserType"].ToString();
                    userInfo.IsApproved = dtblRegistration.Rows[0]["IsApproved"].ToString();
                    userInfo.IsRejected = dtblRegistration.Rows[0]["IsRejected"].ToString();
                    userInfo.WalletNumber = dtblRegistration.Rows[0]["WalletNumber"].ToString();

                    userInfo.BankNo = dtblRegistration.Rows[0]["BankNo"].ToString();
                    userInfo.BranchCode = dtblRegistration.Rows[0]["BranchCode"].ToString();
                    userInfo.BankAccountNumber = dtblRegistration.Rows[0]["BankAccountNumber"].ToString();
                }
                return View(userInfo);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        // POST: SuperAdmin/EditRegistration/1
        [HttpPost]
        public ActionResult EditRegistration(string btnCommand, FormCollection collection)
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

                try
                {
                    if (btnCommand == "Submit")
                    {
                        string cClientCode = collection["ClientCode"].ToString();
                        string cAddress = collection["Address"].ToString();
                        string cName = collection["Name"].ToString();
                        string cEmail = collection["EmailAddress"].ToString();

                        //string cStatus = collection["Status"].ToString();
                        //string cContactNumber1 = collection["ContactNumber1"].ToString();
                        //string cContactNumber2 = collection["ContactNumber2"].ToString();
                        //string cWalletNumber = collection["WalletNumber"].ToString();

                        //string cBankNo = collection["BankNo"].ToString();
                        // string cBranchCode = collection["BranchCode"].ToString();
                        //string cBankAccountNumber = collection["BankAccountNumber"].ToString();
                        //string cIsApproved = collection["IsApproved"].ToString();
                        //string cIsRejected = collection["IsRejected"].ToString();
                        //string cPIN = collection["PIN"].ToString();

                        UserInfo userInfoModify = new UserInfo
                        {
                            ClientCode = cClientCode,
                            Address = cAddress,
                            Name=cName,
                            EmailAddress=cEmail
                            
                        };

                        if (userInfoModify.Name!="")
                        {
                            bool isUpdated = false;
                            isUpdated = SAdminUtils.UpdateSuperAdminInfo(userInfoModify);
                            displayMessage = isUpdated
                                                ? "The Admin User Information has successfully been updated."
                                                : "Error while updating Member information";
                            messageClass = isUpdated ? CssSetting.SuccessMessageClass : CssSetting.FailedMessageClass;
                        }
                        else
                        {
                            displayMessage = "Required Field is Empty";
                            messageClass = CssSetting.FailedMessageClass;
                        }
                    }
                }
                catch (Exception ex)
                {
                    displayMessage = ex.Message;
                    messageClass = CssSetting.FailedMessageClass;
                }

                this.TempData["register_messsage"] = displayMessage;
                this.TempData["message_class"] = messageClass;

                return RedirectToAction("EditRegistration",new {clientCodeId= collection["ClientCode"].ToString() });
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        public ActionResult ViewRegistration(string clientCodeID)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (this.TempData["register_messsage"] != null)
            {
                this.ViewData["register_messsage"] = this.TempData["register_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                UserInfo userInfo = new UserInfo();
                DataTable dtblRegistration = ProfileUtils.GetUserProfileInfo(clientCodeID);
                if (dtblRegistration.Rows.Count == 1)
                {
                    userInfo.ClientCode = dtblRegistration.Rows[0]["ClientCode"].ToString();
                    userInfo.Name = dtblRegistration.Rows[0]["Name"].ToString();
                    userInfo.Address = dtblRegistration.Rows[0]["Address"].ToString();
                    userInfo.PIN = dtblRegistration.Rows[0]["PIN"].ToString();
                    userInfo.Status = dtblRegistration.Rows[0]["Status"].ToString();
                    userInfo.ContactNumber1 = dtblRegistration.Rows[0]["ContactNumber1"].ToString();
                    userInfo.ContactNumber2 = dtblRegistration.Rows[0]["ContactNumber2"].ToString();
                    userInfo.UserName = dtblRegistration.Rows[0]["UserName"].ToString();
                    userInfo.UserType = dtblRegistration.Rows[0]["UserType"].ToString();
                    userInfo.IsApproved = dtblRegistration.Rows[0]["IsApproved"].ToString();
                    userInfo.IsRejected = dtblRegistration.Rows[0]["IsRejected"].ToString();
                    userInfo.WalletNumber = dtblRegistration.Rows[0]["WalletNumber"].ToString();
                    userInfo.BankAccountNumber = dtblRegistration.Rows[0]["BankAccountNumber"].ToString();

                    ViewBag.ADClientCode = userInfo.ClientCode;
                    ViewBag.CName = userInfo.Name;
                    ViewBag.Address = userInfo.Address;
                    ViewBag.PIN = userInfo.PIN;
                    ViewBag.UserName = userInfo.UserName;
                    ViewBag.CUserType = userInfo.UserType;
                    ViewBag.Status = userInfo.Status;
                    ViewBag.ContactNumber1 = userInfo.ContactNumber1;
                    ViewBag.ContactNumber2 = userInfo.ContactNumber2;
                    ViewBag.IsApproved = userInfo.IsApproved;
                    ViewBag.IsRejected = userInfo.IsRejected;
                    ViewBag.WalletNumber = userInfo.WalletNumber;
                    ViewBag.BankAccountNumber = userInfo.BankAccountNumber;
                    return View(userInfo);
                }
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }


        #endregion


        #region "SuperAdmin Password Reset"

        // GET: SuperAdmin/PasswordReset
        [HttpGet]
        public ActionResult PasswordResetList()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (this.TempData["password_messsage"] != null)
            {
                this.ViewData["password_messsage"] = this.TempData["password_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                List<UserInfo> registrationtList = new List<UserInfo>();
                DataTable dtblRegistration = ProfileUtils.GetSuperAdminProfile();

                foreach (DataRow row in dtblRegistration.Rows)
                {
                    UserInfo regobj = new UserInfo
                    {
                        ClientCode = row["ClientCode"].ToString(),
                        Name = row["Name"].ToString(),
                        Address = row["Address"].ToString(),
                        PIN = row["PIN"].ToString(),
                        Status = row["Status"].ToString(),
                        ContactNumber1 = row["ContactNumber1"].ToString(),
                        ContactNumber2 = row["ContactNumber2"].ToString(),
                        UserName = row["UserName"].ToString(),
                        UserType = row["userType"].ToString()
                    };

                    registrationtList.Add(regobj);
                }
                return View(registrationtList);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        // GET: SuperAdmin/PasswordReset
        [HttpGet]
        public ActionResult PasswordReset(string clientCodeId)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (this.TempData["password_messsage"] != null)
            {
                this.ViewData["password_messsage"] = this.TempData["password_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                @ViewBag.ClientCode = clientCodeId;

                string Name = string.Empty;
                string ContactNumber1 = string.Empty;
                string UserName = string.Empty;
                DataTable dtblRegistration = ProfileUtils.GetUserProfileInfo(clientCodeId);
                if (dtblRegistration.Rows.Count == 1)
                {
                    Name = dtblRegistration.Rows[0]["Name"].ToString();
                    ContactNumber1 = dtblRegistration.Rows[0]["ContactNumber1"].ToString();
                    UserName = dtblRegistration.Rows[0]["UserName"].ToString();
                }
                ViewBag.Name = Name;
                ViewBag.ContactNumber1 = ContactNumber1;
                ViewBag.UserName = UserName;

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        // POST: SuperAdmin/PasswordReset/1
        [HttpPost]
        public ActionResult PasswordReset(FormCollection collection)
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

                string txtClientCode = collection["txtClientCode"].ToString();
                string password = collection["txtPassword"].ToString();

                UserInfo userInfo = new UserInfo
                {
                    Password = password,
                    ClientCode = txtClientCode
                };

                string displayMessage = string.Empty;
                string messageClass = string.Empty;

                try
                {
                    if ((password != "") && (txtClientCode != ""))
                    {
                        bool isUpdated = PasswordUtils.UpdateAdminPasswordInfo(userInfo);
                        displayMessage = isUpdated
                                                 ? "Password Information has successfully been updated."
                                                 : "Error while updating Password information";
                        messageClass = isUpdated ? CssSetting.SuccessMessageClass : CssSetting.FailedMessageClass;
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
                //return RedirectToAction("PasswordReset");
                return RedirectToAction("PasswordResetList");
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        #endregion
    }
}