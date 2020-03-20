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
    public class PayeeController : Controller
    {
        #region "Merchant Registration"

        //GET: Payee/MerchantRegistration
        [HttpGet]
        public ActionResult MerchantRegistration()
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

                ViewBag.User = "merchant";

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
                ViewBag.Bank = item.Where(x => x.Value != "0000").ToList();
                //ViewBag.Bank = item;
                string bankCode = item[0].Value.ToString();
                string unique = TraceIdGenerator.GetUniqueWalletNo();
                ViewBag.WalletNumber = bankCode + t + unique;

                ViewBag.MerchantCategory = MerchantUtils.GetMerchantsType();


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


        //POST: Payee/MerchantRegistration/1
        [HttpPost]
        public ActionResult MerchantRegistration(FormCollection collection)
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

                    UserInfo userInfo = new UserInfo();
                    userInfo.UserType = collection["txtUserType"].ToString();

                    userInfo.UserName = collection["txtUserName"].ToString();
                    userInfo.Password = CustomerUtils.GeneratePassword();

                    string FName = collection["txtFullName"].ToString();
                    //string MName = collection["txtMName"].ToString();
                    //string LName = collection["txtLastName"].ToString();
                    userInfo.Name = FName; //+ " " + MName + " " + LName;

                    string txtAddress = collection["txtAddress"].ToString();
                    string txtWardNo = collection["txtWardNo"].ToString();
                    string txtDistrict = collection["txtDistrict"].ToString();
                    string txtZone = collection["txtZone"].ToString();
                    userInfo.Address = txtAddress + "," + txtWardNo + "," + txtDistrict + "," + txtZone;
                    string MerchantCategory = collection["MerchantCategory"].ToString(); 
                    userInfo.PIN = string.Empty;//collection["txtPin"].ToString();
                    userInfo.Status = collection["txtStatus"].ToString();
                    userInfo.ContactNumber1 = collection["txtContactNumber1"].ToString();
                    userInfo.ContactNumber2 = collection["txtContactNumber2"].ToString();

                    userInfo.EmailAddress = string.Empty;

                    userInfo.ClientCode = collection["txtCClientCode"].ToString();

                    userInfo.IsApproved = collection["txtIsApproved"].ToString();
                    userInfo.IsRejected = collection["txtIsRejected"].ToString();

                    userInfo.WalletNumber = collection["txtWalletNumber"].ToString();
                    userInfo.WBankCode = collection["WalletCode"];
                    userInfo.WBranchCode = collection["WBranchCode"].ToString();
                    userInfo.WIsDefault = collection["txtWIsDefault"].ToString();
                    userInfo.AgentId = collection["txtAgentId"].ToString();

                    userInfo.BankNo = collection["BankNoBin"];
                    userInfo.BankAccountNumber = collection["txtBankAccountNumber"].ToString();

                    userInfo.BranchCode = string.Empty;
                    
                    if (userInfo.BankAccountNumber != "")
                        userInfo.BranchCode = userInfo.BankAccountNumber.Substring(0, 3);

                    userInfo.IsDefault = collection["txtIsDefault"].ToString();

                    if (string.IsNullOrEmpty(collection["txtUserType"]))
                    {
                        ModelState.AddModelError("UserType", "*Please enter UserType");
                    }
                    if (string.IsNullOrEmpty(collection["txtUserName"]))
                    {
                        ModelState.AddModelError("UserName", "*Please enter UserName");
                    }
                    if (string.IsNullOrEmpty(collection["txtFullName"]))
                    {
                        ModelState.AddModelError("FirstName", "*Please enter Full Name");
                    }
                    if (string.IsNullOrEmpty(collection["txtAddress"]))
                    {
                        ModelState.AddModelError("Address", "*Please enter Address");
                    }
                    if (string.IsNullOrEmpty(collection["txtWardNo"]))
                    {
                        ModelState.AddModelError("WardNo", "*Please enter WardNo");
                    }
                    if (string.IsNullOrEmpty(collection["txtDistrict"]))
                    {
                        ModelState.AddModelError("District", "*Please enter District");
                    }
                    if (string.IsNullOrEmpty(collection["txtZone"]))
                    {
                        ModelState.AddModelError("Zone", "*Please enter Zone");
                    }
                    if (string.IsNullOrEmpty(collection["txtStatus"]))
                    {
                        ModelState.AddModelError("Status", "*Please enter Status");
                    }
                    if (string.IsNullOrEmpty(collection["txtContactNumber1"]))
                    {
                        ModelState.AddModelError("ContactNumber", "*Please enter Contact Number");
                    }
                    if (string.IsNullOrEmpty(collection["txtIsApproved"]))
                    {
                        ModelState.AddModelError("IsApproved", "*Please enter IsApproved");
                    }
                    if (string.IsNullOrEmpty(collection["txtIsRejected"]))
                    {
                        ModelState.AddModelError("IsRejected", "*Please enter IsRejected");
                    }

                    if (string.IsNullOrEmpty(collection["txtWalletNumber"]))
                    {
                        ModelState.AddModelError("WalletNumber", "*Please enter WalletNumber");
                    }
                    if (string.IsNullOrEmpty(collection["WalletCode"]))
                    {
                        ModelState.AddModelError("WalletCode", "*Please enter txtWBankCode");
                    }
                    if (string.IsNullOrEmpty(collection["WBranchCode"]))
                    {
                        ModelState.AddModelError("WBranchCode", "*Please enter WBranchCode");
                    }
                    if (string.IsNullOrEmpty(collection["txtWIsDefault"]))
                    {
                        ModelState.AddModelError("txtWIsDefault", "*Please enter WIsDefault");
                    }
                    if (string.IsNullOrEmpty(collection["txtAgentId"]))
                    {
                        ModelState.AddModelError("AgentId", "*Please enter AgentId");
                    }
                    if (string.IsNullOrEmpty(collection["txtIsDefault"]))
                    {
                        ModelState.AddModelError("IsDefault", "*Please enter IsDefault");
                    }

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
                    ViewBag.Bank = item.Where(x => x.Value != "0000").ToList();
                    //ViewBag.Bank = item;
                    ViewBag.User = "merchant";
                    ViewBag.MerchantCategory = MerchantUtils.GetMerchantsType();
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
                        DataTable dtableMobileNo = RegisterUtils.GetCheckMobileNo(userInfo.ContactNumber1);
                        if (dtableMobileNo.Rows.Count == 0)
                        {
                            if (collection.AllKeys.Any())
                            {
                                try
                                {
                                    int results = RegisterUtils.RegisterMerchantInfo(userInfo,MerchantCategory);
                                    if (results > 0)
                                    {
                                        result = true;
                                        //SMSUtils util = new SMSUtils();
                                        //string Message = "Dear Customer," + "\n";
                                        //Message += " Your Password is " + userInfo.Password
                                        //    + "." + "\n" + "Thank You";
                                        //Message += "-MNepal";
                                        //util.SendSMS(Message,userInfo.ContactNumber1);


                                        //int resultmsg = RegisterUtils.CreateWalletAcInfo(userInfo);
                                        //if (resultmsg == 100)
                                        //{
                                        //    result = true;
                                        //}
                                        //else
                                        //{
                                        //    result = false;
                                        //}
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
                            if (checkMobileNo == userInfo.ContactNumber1)
                            {
                                result = false;
                                errorMessage = " Primary Contact Number is already in the member list";
                            }
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
                    //List<SelectListItem> item = new List<SelectListItem>();
                    //DataSet dsBank = BankUtils.GetDataSetPopulateBankCode();
                    //ViewBag.bank = dsBank.Tables[0];
                    //foreach (DataRow dr in ViewBag.bank.Rows)
                    //{
                    //    item.Add(new SelectListItem
                    //    {
                    //        Text = @dr["BankName"].ToString(),
                    //        Value = @dr["BankCode"].ToString()
                    //    });
                    //}
                    ////ViewBag.Wallet = item;
                    //ViewBag.Bank = item;

                    return this.RedirectToAction("MerchantRegistration");
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
        public ActionResult GetCheckingUserName(string Username)
        {
            if ((Username != "") || (Username != null))
            {
                string result = string.Empty;
                DataTable dtableMobileNo = RegisterUtils.GetCheckUserName(Username);
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
            else
                return Json("Failed to Retrieve Data", JsonRequestBehavior.AllowGet);
        }

        #region "Merchant Index/List "

        // GET: Payee/Index
        [HttpGet]
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

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        #endregion


        #region "Merchant Modification"

        // GET: Payee/MerchantDetail
        public ActionResult MerchantDetail(string SearchCol, string txtName)
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

                UserInfo userInfo = new UserInfo();
                userInfo.Name = txtName;// = collection["txtName"].ToString();

                List<UserInfo> MerchantStatus = new List<UserInfo>();




                if (SearchCol == "All")
                {
                    DataTable dtableStatusByAll = CustomerUtils.GetMerchantUserProfileByALL(userInfo.Name, userInfo.WalletNumber, userInfo.ContactNumber1);
                    if (dtableStatusByAll != null && dtableStatusByAll.Rows.Count > 0)
                    {
                        UserInfo regobj = new UserInfo();
                        regobj.ClientCode = dtableStatusByAll.Rows[0]["ClientCode"].ToString();
                        regobj.Name = dtableStatusByAll.Rows[0]["Name"].ToString();
                        regobj.Address = dtableStatusByAll.Rows[0]["Address"].ToString();
                        regobj.PIN = dtableStatusByAll.Rows[0]["PIN"].ToString();
                        regobj.Status = dtableStatusByAll.Rows[0]["Status"].ToString();
                        regobj.ContactNumber1 = dtableStatusByAll.Rows[0]["ContactNumber1"].ToString();
                        regobj.ContactNumber2 = dtableStatusByAll.Rows[0]["ContactNumber2"].ToString();
                        regobj.UserName = dtableStatusByAll.Rows[0]["UserName"].ToString();
                        regobj.UserType = dtableStatusByAll.Rows[0]["userType"].ToString();

                        MerchantStatus.Add(regobj);
                        ViewData["dtableMerchantStatus"] = dtableStatusByAll;
                    }
                }

                if ((userInfo.Name != "") && (SearchCol == "Name"))
                {
                    DataTable dtableStatusByName = CustomerUtils.GetMerchantUserProfileByName(userInfo.Name);
                    if (dtableStatusByName != null && dtableStatusByName.Rows.Count > 0)
                    {
                        UserInfo regobj = new UserInfo();
                        regobj.ClientCode = dtableStatusByName.Rows[0]["ClientCode"].ToString();
                        regobj.Name = dtableStatusByName.Rows[0]["Name"].ToString();
                        regobj.Address = dtableStatusByName.Rows[0]["Address"].ToString();
                        regobj.PIN = dtableStatusByName.Rows[0]["PIN"].ToString();
                        regobj.Status = dtableStatusByName.Rows[0]["Status"].ToString();
                        regobj.ContactNumber1 = dtableStatusByName.Rows[0]["ContactNumber1"].ToString();
                        regobj.ContactNumber2 = dtableStatusByName.Rows[0]["ContactNumber2"].ToString();
                        regobj.UserName = dtableStatusByName.Rows[0]["UserName"].ToString();
                        regobj.UserType = dtableStatusByName.Rows[0]["userType"].ToString();

                        MerchantStatus.Add(regobj);
                        ViewData["dtableMerchantStatus"] = dtableStatusByName;
                    }
                }
                return View(MerchantStatus);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        // GET: Payee/Modification
        [HttpGet]
        public ActionResult Modification(string id)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            if (this.TempData["merchant_messsage"] != null)
            {
                this.ViewData["merchant_messsage"] = this.TempData["merchant_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                UserInfo regobj = new UserInfo();
                DataTable dtableUserInfo = ProfileUtils.GetUserProfileInfo(id.ToString());

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
                ViewBag.Bank = item.Where(x => x.Value != "0000").ToList();
                //ViewBag.Bank = item;



                if (dtableUserInfo != null && dtableUserInfo.Rows.Count > 0)
                {
                    regobj.ClientCode = dtableUserInfo.Rows[0]["ClientCode"].ToString();
                    regobj.Name = dtableUserInfo.Rows[0]["Name"].ToString();
                    regobj.Address = dtableUserInfo.Rows[0]["Address"].ToString();
                    regobj.PIN = dtableUserInfo.Rows[0]["PIN"].ToString();
                    regobj.Status = dtableUserInfo.Rows[0]["Status"].ToString();
                    regobj.ContactNumber1 = dtableUserInfo.Rows[0]["ContactNumber1"].ToString();
                    regobj.ContactNumber2 = dtableUserInfo.Rows[0]["ContactNumber2"].ToString();
                    regobj.UserName = dtableUserInfo.Rows[0]["UserName"].ToString();
                    regobj.UserType = dtableUserInfo.Rows[0]["userType"].ToString();
                    regobj.IsApproved = dtableUserInfo.Rows[0]["IsApproved"].ToString();
                    regobj.IsRejected = dtableUserInfo.Rows[0]["IsRejected"].ToString();
                    regobj.WalletNumber = dtableUserInfo.Rows[0]["WalletNumber"].ToString();

                    regobj.BankNo = dtableUserInfo.Rows[0]["BankNo"].ToString();
                    regobj.BranchCode = dtableUserInfo.Rows[0]["BranchCode"].ToString();
                    regobj.BankAccountNumber = dtableUserInfo.Rows[0]["BankAccountNumber"].ToString();

                }
                return View(regobj);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        //POST: Payee/Modification/1
        [HttpPost]
        public ActionResult Modification(string btnCommand, FormCollection collection,UserInfo model)
        {
            UserInfo userInfoModify = new UserInfo();
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

                    string displayMessage = null;
                    string messageClass = null;

                    if (btnCommand == "Submit")
                    {

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
                      

                        string cClientCode = collection["ClientCode"].ToString();
                        string cName = collection["Name"].ToString();
                        string cAddress = collection["Address"].ToString();
                        string cStatus = collection["txtStatus"].ToString();
                        string cContactNumber1 = collection["ContactNumber1"].ToString();
                        string cContactNumber2 = collection["ContactNumber2"].ToString();
                        //string cWalletNumber = collection["WalletNumber"].ToString();
                        string cBranchCode = collection["BranchCode"].ToString();
                        string cBankNo = collection["BankNo"].ToString();
                        string cBankAccountNumber = collection["BankAccountNumber"].ToString();
                        string cIsApproved = collection["txtIsApproved"].ToString();
                        string cIsRejected = collection["txtIsRejected"].ToString();
                        //string cPIN = collection["PIN"].ToString();

                     
                        userInfoModify.ClientCode = cClientCode;
                        userInfoModify.Name = cName;
                        userInfoModify.EmailAddress = "";
                        userInfoModify.Address = cAddress;
                        userInfoModify.Status = cStatus;
                        userInfoModify.ContactNumber1 = cContactNumber1;
                        userInfoModify.ContactNumber2 = cContactNumber2;
                        //userInfoModify.WalletNumber = cWalletNumber;
                        userInfoModify.BankNo = cBankNo;
                        userInfoModify.BranchCode = cBankAccountNumber.Substring(0, 3);
                        userInfoModify.BankAccountNumber = cBankAccountNumber;
                        userInfoModify.IsApproved = cIsApproved;
                       // userInfoModify.IsRejected = cIsRejected;
                        //userInfoModify.PIN = cPIN; (cPIN != "") && && (cAddress != "")

                        if ((cIsRejected != "") && (cIsApproved != "")
                            && (cContactNumber1 != "") && (cStatus != ""))
                        {
                            bool isUpdated = CustomerUtils.UpdateMerchantInfo(userInfoModify);
                            displayMessage = isUpdated
                                                     ? "Merchant Information has successfully been updated."
                                                     : "Error while updating Merchant Information";
                            messageClass = isUpdated ? CssSetting.SuccessMessageClass : CssSetting.FailedMessageClass;
                        }
                        else
                        {
                            displayMessage = "Required Field is Empty";
                            messageClass = CssSetting.FailedMessageClass;
                        }

                        this.TempData["merchant_messsage"] = displayMessage;
                        this.TempData["message_class"] = messageClass;

                        return RedirectToAction("Modification");
                    }
                    return View(model);

                }
                else
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            catch
            {
                return View(model);
            }
        }


        #endregion


        #region "Merchant Details"

        // GET: Payee/MerchantDetails
        public ActionResult MerchantDetails()
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


                DataTable dtableMerchantDetail = MerchantUtils.GetMerchantDetailInfo();
                if (dtableMerchantDetail != null && dtableMerchantDetail.Rows.Count > 0)
                {
                    MNMerchants merchantDetail = new MNMerchants();
                    merchantDetail.Name = dtableMerchantDetail.Rows[0]["Name"].ToString();
                    ViewData["MerchantDetail"] = dtableMerchantDetail;
                }
                else
                {
                    dtableMerchantDetail = null;
                    ViewData["MerchantDetail"] = dtableMerchantDetail;
                }
                return View(dtableMerchantDetail);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        #endregion

    }
}