using MNSuperadmin.Helper;
using MNSuperadmin.Models;
using MNSuperadmin.Settings;
using MNSuperadmin.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Web.SessionState;

namespace MNSuperadmin.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class PayeeController : Controller
    {
        
        DAL objdal = new DAL();
        #region "GET: Merchant Registration"
        //GET: Payee/MerchantRegistration
        [HttpGet]
        public ActionResult Registration()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;


            //start
            string provincestring = "select * from MNProvince";
            DataTable dt = new DataTable();
            dt = objdal.MyMethod(provincestring);
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SelectListItem
                {
                    Text = Convert.ToString(row.ItemArray[1]),
                    Value = Convert.ToString(row.ItemArray[0])
                });

            }
            ViewBag.PProvince = list;
            ViewBag.CProvince = list;
            //end

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
        #endregion



        #region "POST: Merchant Registration"
        //POST: Payee/MerchantRegistration
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

                    UserInfo userInfo = new UserInfo();
                    userInfo.UserType = collection["txtUserType"].ToString();

                    userInfo.UserName = collection["txtUserName"].ToString();
                    userInfo.Password = CustomerUtils.GeneratePassword();

                    //string FName = collection["txtFullName"].ToString();
                    userInfo.Name = userInfo.FName; /*+ " " + MName + " " + LName;*/
                    userInfo.FName = collection["txtFirstName"].ToString();
                    userInfo.MName = collection["txtMiddleName"].ToString();
                    userInfo.LName = collection["txtLastName"].ToString();
                    userInfo.BusinessName = collection["txtBusinessName"].ToString();
                    userInfo.RegistrationNumber = collection["txtRegistrationNumber"].ToString();
                    userInfo.VATNumber = collection["txtVATNumber"].ToString();
                    userInfo.PStreet = collection["PStreet"].ToString();
                    userInfo.PVDC = collection["txtPVDC"].ToString();
                    userInfo.PHouseNo = collection["txtPHouseNo"].ToString();
                    userInfo.PWardNo = collection["txtPWardNo"].ToString();
                    userInfo.PProvince = collection["PProvince"].ToString();
                    userInfo.PDistrictID = collection["PDistrictID"].ToString();
                    userInfo.LandlineNumber = collection["txtLandlineNumber"].ToString();
                    userInfo.EmailAddress = collection["Email"].ToString();


                    userInfo.Address = userInfo.PStreet + "," + userInfo.PWardNo + "," + userInfo.PDistrictID + "," + userInfo.PProvince;
                    string MerchantCategory = collection["MerchantCategory"].ToString();
                    userInfo.PIN = string.Empty;//collection["txtPin"].ToString();
                    userInfo.Status = collection["txtStatus"].ToString();
                    userInfo.ContactNumber1 = collection["txtContactNumber1"].ToString();
                    userInfo.ContactNumber2 = collection["txtContactNumber2"].ToString();
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
                    //if (string.IsNullOrEmpty(collection["txtFullName"]))
                    //{
                    //    ModelState.AddModelError("FirstName", "*Please enter First Name");
                    //}
                    //if (string.IsNullOrEmpty(collection["txtAddress"]))
                    //{
                    //    ModelState.AddModelError("Address", "*Please enter Address");
                    //}
                    //if (string.IsNullOrEmpty(collection["txtPWardNo"]))
                    //{
                    //    ModelState.AddModelError("WardNo", "*Please enter WardNo");
                    //}
                    //if (string.IsNullOrEmpty(collection["PDistrictID"]))
                    //{
                    //    ModelState.AddModelError("District", "*Please Select District");
                    //}
                    //if (string.IsNullOrEmpty(collection["txtZone"]))
                    //{
                    //    ModelState.AddModelError("Zone", "*Please enter Zone");
                    //}
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
                                    int results = RegisterUtils.RegisterMerchantInfo(userInfo, MerchantCategory);
                                    if (results > 0)
                                    {
                                        result = true;
                                        SMSUtils util = new SMSUtils();
                                        string Message = "Dear Merchant " + userInfo.FName + "," + "\n";
                                        Message += " Your Merchant Registration request has been queued for the approval. You'll be notified shortly."
                                            + "." + "\n" + "Thank You";
                                        Message += "-MNepal";
                                        util.SendSMS(Message, userInfo.ContactNumber1);


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
        


        #region "View all Registered Merchant Lists"
        // GET: Payee/Index
        [HttpGet]
        public ActionResult ViewMerchantRegistration()
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
                if (this.TempData["merchant_messsage"] != null)
                {
                    this.ViewData["merchant_messsage"] = this.TempData["merchant_messsage"];
                    this.ViewData["message_class"] = this.TempData["message_class"];
                }
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        #endregion



        #region "Approve/Reject Merchant"
        // GET: Payee/Modification
        [HttpGet]
        public ActionResult ApproveReject(string id)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];



            //if (this.TempData["merchant_messsage"] != null)
            //{
            //    this.ViewData["merchant_messsage"] = this.TempData["merchant_messsage"];
            //    this.ViewData["message_class"] = this.TempData["message_class"];
            //}

            TempData["userType"] = userType;

            if (TempData["userType"] != null && id != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                UserInfo regobj = new UserInfo();

                DataTable dtableUserInfo = ProfileUtils.GetMerchantProfileInfo(id.ToString());

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
                    regobj.PDistrictID = getDistrictName(dtableUserInfo.Rows[0]["PDistrictID"].ToString());

                    regobj.PProvince = getProvince(dtableUserInfo.Rows[0]["PProvince"].ToString());

                    regobj.FName = dtableUserInfo.Rows[0]["FirstName"].ToString();
                    regobj.MName = dtableUserInfo.Rows[0]["MiddleName"].ToString();
                    regobj.LName = dtableUserInfo.Rows[0]["LastName"].ToString();
                    regobj.BusinessName = dtableUserInfo.Rows[0]["BusinessName"].ToString();
                    regobj.RegistrationNumber = dtableUserInfo.Rows[0]["RegistrationNumber"].ToString();
                    regobj.VATNumber = dtableUserInfo.Rows[0]["VATNumber"].ToString();
                    regobj.PStreet = dtableUserInfo.Rows[0]["PStreet"].ToString();
                    regobj.PVDC = dtableUserInfo.Rows[0]["PMunicipalityVDC"].ToString();
                    regobj.PHouseNo = dtableUserInfo.Rows[0]["PHouseNo"].ToString();
                    regobj.PWardNo = dtableUserInfo.Rows[0]["PWardNo"].ToString();
                    regobj.LandlineNumber = dtableUserInfo.Rows[0]["LandlineNumber"].ToString();
                    regobj.EmailAddress = dtableUserInfo.Rows[0]["EmailAddress"].ToString();

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
        #endregion



        #region "Approve/Reject value is passed here when the approve/reject button is triggered"
        [HttpPost]
        public ActionResult ApproveMerchant(UserInfo model, string btnApprove)
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




                UserInfo userInfoApproval = new UserInfo();
                userInfoApproval.ClientCode = model.ClientCode;
                userInfoApproval.UserName = userName;

                if (btnApprove.ToUpper() == "REJECT")
                {
                    userInfoApproval.AdminUserName = Session["UserName"].ToString();
                    userInfoApproval.AdminBranch = "";
                    userInfoApproval.Remarks = model.Remarks;


                    string Rejected = "T";
                    //string Approve = "UnApprove";

                    userInfoApproval.Remarks = model.Remarks;

                    int ret = MerchantUtils.MerchantRegReject(userInfoApproval, Rejected);

                    if (ret == 1)
                    {


                        //SMSUtils util = new SMSUtils();
                        //string Message = "Dear Merchant, " + userInfoApproval.FName + "\n";
                        //Message += " Your Merchant Registration request has been rejected."
                        //    + "." + "\n" + "Thank You";
                        //Message += "-MNepal";
                        //util.SendSMS(Message, userInfoApproval.ContactNumber1);

                        displayMessage = "Merchant" + model.Name + " has been Rejected. Please Check Rejectlist and perform accordingly";
                        messageClass = CssSetting.SuccessMessageClass;




                    }
                    else if (ret == -1 || ret != 100)
                    {
                        displayMessage = "Error while rejecting Merchant " + model.Name;
                        messageClass = CssSetting.FailedMessageClass;
                    }

                    this.TempData["merchant_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;
                    return RedirectToAction("RegisteredMerchants");

                }
                if (btnApprove.ToUpper() == "APPROVE")
                {
                    userInfoApproval.SuperAdminUserName = "";
                    //userInfoApproval.AdminBranch = "";

                    //string Rejected = "F";
                    string Approve = "Approve";
                    userInfoApproval.ApprovedBy = userName;
                    int ret = MerchantUtils.MerchantRegisterApprove(userInfoApproval, Approve);
                    if (ret == 1)
                    {
                        UserInfo userInfo = new UserInfo();
                        DataTable dtblRegistration = ProfileUtils.GetMerchantProfileInfo(model.ClientCode);
                        if (dtblRegistration.Rows.Count == 1)
                        {
                            userInfo.Name = dtblRegistration.Rows[0]["Name"].ToString().Capitalize();
                            userInfo.EmailAddress = dtblRegistration.Rows[0]["EmailAddress"].ToString();
                            userInfo.UserName = dtblRegistration.Rows[0]["UserName"].ToString();
                            userInfo.Password = dtblRegistration.Rows[0]["Password"].ToString();
                            userInfo.PIN = dtblRegistration.Rows[0]["PIN"].ToString();
                            userInfo.ClientCode = dtblRegistration.Rows[0]["ClientCode"].ToString();
                        }
                        userInfo.Password = CustomerUtils.GeneratePassword();
                        bool passChange = PasswordUtils.ResetPassword(userInfo);
                        userInfo.PIN = CustomerUtils.GeneratePin();
                        if (userInfo.EmailAddress != "" && userInfo.EmailAddress != string.Empty)
                        {
                            //    string Subject = "New Agent Registration";
                            //    string MailSubject = "<span style='font-size:15px;'><h4>Dear " + userInfo.Name + ",</h4>";
                            //    MailSubject += "A new Account has been created  for you at Nepal Investment Bank Ltd. Mobile Banking,";
                            //    MailSubject += "and you have been issued with a new temporary password.<br/>";
                            //    MailSubject += "<br/><b>Username: " + userInfo.UserName + "</b> <br/>";
                            //    MailSubject += "<br/><b>Password: " + userInfo.Password + "</b> <br/>";
                            //    MailSubject += "<br/><b>PIN: " + userInfo.PIN + "</b> <br/>";
                            //    MailSubject += "                    (Please change your password after login.)<br/>";
                            //    MailSubject += "<br/>Thank You <br/>";
                            //    MailSubject += "<br/>Nepal Investment Bank Ltd. </span><br/>";
                            //    MailSubject += @"<br/>Note: This is confidential mail, Please do not share with other. Your credential purpose is for NIBL Administration Console only.<br/>";
                            //    MailSubject += "<hr/>";
                            //    EMailUtil SendMail = new EMailUtil();
                            //    try
                            //    {
                            //        SendMail.SendMail(userInfo.EmailAddress, Subject, MailSubject);
                            //    }
                            //    catch
                            //    {

                            //    }


                            //for sms 
                            SMSUtils SMS = new SMSUtils();
                            string Message = string.Format("Dear Merchant " + userInfo.Name.Split()[0] + ", Your Merchant Registration has been approved." + "\n Your new T-Pin is " + userInfo.PIN + " and Password is " + userInfo.Password + ".\n Thank You -MNepal");

                            SMSLog Log = new SMSLog();

                            Log.UserName = userInfo.UserName;
                            Log.Purpose = "NR"; //New Registration
                            Log.SentBy = userInfoApproval.ApprovedBy;
                            Log.Message = "Your T-Pin is " + ExtraUtility.Encrypt(userInfo.PIN) + "" + " and Password is " + ExtraUtility.Encrypt(userInfo.Password); //encrypted when logging
                                                                                                                                                                      //Log SMS
                            CustomerUtils.LogSMS(Log);

                            SMS.SendSMS(Message, userInfo.UserName);

                        }
                        displayMessage = "Registration Information for Merchant " + userInfo.Name + " has successfully been approved. Please check the Detail List Below.";
                        messageClass = CssSetting.SuccessMessageClass;
                    }
                    else if (ret == -1 || ret != 100)
                    {
                        displayMessage = "Error while approving Agent Information";
                        messageClass = CssSetting.FailedMessageClass;
                    }

                    this.TempData["merchant_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;
                    return RedirectToAction("ApprovedMerchantList");
                }
                else
                {

                    this.TempData["agentapprove_messsage"] = "Unauthorized execution";
                    this.TempData["message_class"] = CssSetting.FailedMessageClass;

                    return RedirectToAction("RejectedMerchantList", "Payee");
                }
            }
            //else if (btnCommand.Substring(0, 13) == "btnSelApprove")
            //{
            //    RedirectToAction("ApproveList");
            //}
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        #endregion



        #region "GET: All Merchant Details"
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
        #endregion



        #region "GET: Merchant Modification"
        // GET: Payee/Modification
        [HttpGet]
        public ActionResult Modification(string id)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            //start milayako 02

            string provincestring = "select * from MNProvince";
            DataTable dt = new DataTable();
            dt = objdal.MyMethod(provincestring);
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SelectListItem
                {
                    Text = Convert.ToString(row.ItemArray[1]),
                    Value = Convert.ToString(row.ItemArray[0])
                });
            }
            ViewBag.PProvince = list;
            ViewBag.CProvince = list;
            //end milayako 02 


            if (this.TempData["merchant_messsage"] != null)
            {
                this.ViewData["merchant_messsage"] = this.TempData["merchant_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            TempData["userType"] = userType;

            if (TempData["userType"] != null && id !=null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                UserInfo regobj = new UserInfo();
                 
                DataTable dtableUserInfo = ProfileUtils.GetMerchantProfileInfo(id.ToString());
                
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
                    regobj.PDistrictID = dtableUserInfo.Rows[0]["PDistrictID"].ToString();
                    regobj.PProvince = dtableUserInfo.Rows[0]["PProvince"].ToString();

                    regobj.FName = dtableUserInfo.Rows[0]["FirstName"].ToString();
                    regobj.MName = dtableUserInfo.Rows[0]["MiddleName"].ToString();
                    regobj.LName = dtableUserInfo.Rows[0]["LastName"].ToString();
                    regobj.BusinessName = dtableUserInfo.Rows[0]["BusinessName"].ToString();
                    regobj.RegistrationNumber = dtableUserInfo.Rows[0]["RegistrationNumber"].ToString();
                    regobj.VATNumber = dtableUserInfo.Rows[0]["VATNumber"].ToString();
                    regobj.PStreet = dtableUserInfo.Rows[0]["PStreet"].ToString();
                    regobj.PVDC = dtableUserInfo.Rows[0]["PMunicipalityVDC"].ToString();
                    regobj.PHouseNo = dtableUserInfo.Rows[0]["PHouseNo"].ToString();
                    regobj.PWardNo = dtableUserInfo.Rows[0]["PWardNo"].ToString();
                    //regobj.PProvince = dtableUserInfo.Rows[0]["PProvince"].ToString();
                    //regobj.PDistrictID = dtableUserInfo.Rows[0]["PDistrictID"].ToString();
                    regobj.LandlineNumber = dtableUserInfo.Rows[0]["LandlineNumber"].ToString();
                    regobj.EmailAddress = dtableUserInfo.Rows[0]["EmailAddress"].ToString();

                    regobj.BankNo = dtableUserInfo.Rows[0]["BankNo"].ToString();
                    regobj.BranchCode = dtableUserInfo.Rows[0]["BranchCode"].ToString();
                    regobj.BankAccountNumber = dtableUserInfo.Rows[0]["BankAccountNumber"].ToString();

                    ViewBag.PProvince = new SelectList(list, "Value", "Text", regobj.PProvince);
                    ViewBag.PDistrictID = new SelectList(ProvinceToDistrict(regobj.PProvince), "Value", "Text", regobj.PDistrict);

                }
                return View(regobj);
            }
            
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        #endregion


        


        #region "POST: Merchant Modification"
        //POST: Payee/Modification/1
        [HttpPost]
        public ActionResult Modification(string btnCommand, FormCollection collection, UserInfo model)
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
                        //string cName = collection["FirstName"].ToString();
                        //string cAddress = collection["Address"].ToString();
                        string cStatus = collection["txtStatus"].ToString();
                        string cContactNumber1 = collection["ContactNumber1"].ToString();
                        string cContactNumber2 = collection["ContactNumber2"].ToString();
                        //string cWalletNumber = collection["WalletNumber"].ToString();
                        string cBranchCode = collection["BranchCode"].ToString();
                        string cBankNo = collection["BankNo"].ToString();
                        string cBankAccountNumber = collection["BankAccountNumber"].ToString();
                        string cIsApproved = collection["txtIsApproved"].ToString();
                        string cIsRejected = collection["txtIsRejected"].ToString();
                        userInfoModify.PProvince = collection["PProvince"].ToString();
                        userInfoModify.PDistrictID = collection["PDistrictID"].ToString();
                        //string cPIN = collection["PIN"].ToString();


                        userInfoModify.ClientCode = cClientCode;
                        //userInfoModify.Name = cName;
                        userInfoModify.EmailAddress = "";
                        //userInfoModify.Address = cAddress;
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

                        userInfoModify.FName = collection["txtFirstName"].ToString();
                        userInfoModify.MName = collection["txtMiddleName"].ToString();
                        userInfoModify.LName = collection["txtLastName"].ToString();
                        userInfoModify.BusinessName = collection["txtBusinessName"].ToString();
                        userInfoModify.RegistrationNumber = collection["txtRegistrationNumber"].ToString();
                        userInfoModify.VATNumber = collection["txtVATNumber"].ToString();
                        userInfoModify.PStreet = collection["PStreet"].ToString();
                        userInfoModify.PVDC = collection["txtPVDC"].ToString();
                        userInfoModify.PHouseNo = collection["txtPHouseNo"].ToString();
                        userInfoModify.PWardNo = collection["txtPWardNo"].ToString();
                        userInfoModify.PProvince = collection["PProvince"].ToString();
                        userInfoModify.PDistrictID = collection["PDistrictID"].ToString();
                        userInfoModify.LandlineNumber = collection["txtLandlineNumber"].ToString();
                        userInfoModify.EmailAddress = collection["Email"].ToString();
                        userInfoModify.Name = userInfoModify.FName + " " + userInfoModify.MName + " " + userInfoModify.LName;
                        userInfoModify.Address = userInfoModify.PVDC + " " + userInfoModify.PDistrictID + " " + userInfoModify.PProvince;


                        if ((cIsRejected != "") && (cIsApproved != "")
                            && (cContactNumber1 != "") && (cStatus != ""))
                        {
                            bool isUpdated = CustomerUtils.UpdateMerchantInfo(userInfoModify);
                            displayMessage = isUpdated
                                                     ? "Merchant Information for " + userInfoModify.Name +" has successfully been updated."
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

                        return RedirectToAction("ApprovedMerchantList");
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


        

        


        

        #region "GET: Rejected Lists of Merchant "
        // GET: Payee/Index
        [HttpGet]
        public ActionResult RejectedMerchantList()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (this.TempData["merchant_messsage"] != null)
            {
                this.ViewData["merchant_messsage"] = this.TempData["merchant_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                string IsModified = "F";
                List<UserInfo> userobj = new List<UserInfo>();

                /// <summary>
                /// UNAPPROVE REJECTED LIST OF Merchant
                /// 
                //userobj = ProfileUtils.GetMerchantRejectedList(IsModified);
                //DataTable dtableUNApproveReject = ProfileUtils.GetUnApproveRJCustomerProfile();



                return View(userobj);

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        #endregion


        #region "GET: Modify Rejected Merchants "
        [HttpGet]
        public ActionResult RejectedModification(string id)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            //start milayako 02

            string provincestring = "select * from MNProvince";
            DataTable dt = new DataTable();
            dt = objdal.MyMethod(provincestring);
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SelectListItem
                {
                    Text = Convert.ToString(row.ItemArray[1]),
                    Value = Convert.ToString(row.ItemArray[0])
                });
            }
            ViewBag.PProvince = list;
            ViewBag.CProvince = list;
            //end milayako 02 


            if (this.TempData["merchant_messsage"] != null)
            {
                this.ViewData["merchant_messsage"] = this.TempData["merchant_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            TempData["userType"] = userType;

            if (TempData["userType"] != null && id != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                UserInfo regobj = new UserInfo();

                DataTable dtableUserInfo = ProfileUtils.GetMerchantProfileInfo(id.ToString());

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
                    regobj.PDistrictID = dtableUserInfo.Rows[0]["PDistrictID"].ToString();
                    regobj.PProvince = dtableUserInfo.Rows[0]["PProvince"].ToString();

                    regobj.FName = dtableUserInfo.Rows[0]["FirstName"].ToString();
                    regobj.MName = dtableUserInfo.Rows[0]["MiddleName"].ToString();
                    regobj.LName = dtableUserInfo.Rows[0]["LastName"].ToString();
                    regobj.BusinessName = dtableUserInfo.Rows[0]["BusinessName"].ToString();
                    regobj.RegistrationNumber = dtableUserInfo.Rows[0]["RegistrationNumber"].ToString();
                    regobj.VATNumber = dtableUserInfo.Rows[0]["VATNumber"].ToString();
                    regobj.PStreet = dtableUserInfo.Rows[0]["PStreet"].ToString();
                    regobj.PVDC = dtableUserInfo.Rows[0]["PMunicipalityVDC"].ToString();
                    regobj.PHouseNo = dtableUserInfo.Rows[0]["PHouseNo"].ToString();
                    regobj.PWardNo = dtableUserInfo.Rows[0]["PWardNo"].ToString();
                    //regobj.PProvince = dtableUserInfo.Rows[0]["PProvince"].ToString();
                    //regobj.PDistrictID = dtableUserInfo.Rows[0]["PDistrictID"].ToString();
                    regobj.LandlineNumber = dtableUserInfo.Rows[0]["LandlineNumber"].ToString();
                    regobj.EmailAddress = dtableUserInfo.Rows[0]["EmailAddress"].ToString();

                    regobj.BankNo = dtableUserInfo.Rows[0]["BankNo"].ToString();
                    regobj.BranchCode = dtableUserInfo.Rows[0]["BranchCode"].ToString();
                    regobj.BankAccountNumber = dtableUserInfo.Rows[0]["BankAccountNumber"].ToString();

                    ViewBag.PProvince = new SelectList(list, "Value", "Text", regobj.PProvince);
                    ViewBag.PDistrictID = new SelectList(ProvinceToDistrict(regobj.PProvince), "Value", "Text", regobj.PDistrict);

                }
                return View(regobj);
            }

            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        #endregion


        #region "POST: Modify Rejected Merchants "
        //POST: Payee/Modification/1
        [HttpPost]
        public ActionResult RejectedModification(string btnCommand, FormCollection collection, UserInfo model)
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
                        //string cName = collection["FirstName"].ToString();
                        //string cAddress = collection["Address"].ToString();
                        string cStatus = collection["txtStatus"].ToString();
                        string cContactNumber1 = collection["ContactNumber1"].ToString();
                        string cContactNumber2 = collection["ContactNumber2"].ToString();
                        //string cWalletNumber = collection["WalletNumber"].ToString();
                        string cBranchCode = collection["BranchCode"].ToString();
                        string cBankNo = collection["BankNo"].ToString();
                        string cBankAccountNumber = collection["BankAccountNumber"].ToString();
                        string cIsApproved = collection["txtIsApproved"].ToString();
                        string cIsRejected = collection["txtIsRejected"].ToString();
                        userInfoModify.PProvince = collection["PProvince"].ToString();
                        userInfoModify.PDistrictID = collection["PDistrictID"].ToString();
                        //string cPIN = collection["PIN"].ToString();


                        userInfoModify.ClientCode = cClientCode;
                        //userInfoModify.Name = cName;
                        userInfoModify.EmailAddress = "";
                        //userInfoModify.Address = cAddress;
                        userInfoModify.Status = "InActive";
                        userInfoModify.ContactNumber1 = cContactNumber1;
                        userInfoModify.ContactNumber2 = cContactNumber2;
                        //userInfoModify.WalletNumber = cWalletNumber;
                        userInfoModify.BankNo = cBankNo;
                        userInfoModify.BranchCode = cBankAccountNumber.Substring(0, 3);
                        userInfoModify.BankAccountNumber = cBankAccountNumber;
                        userInfoModify.IsApproved = "UnApprove";
                        // userInfoModify.IsRejected = cIsRejected;
                        //userInfoModify.PIN = cPIN; (cPIN != "") && && (cAddress != "")

                        userInfoModify.FName = collection["txtFirstName"].ToString();
                        userInfoModify.MName = collection["txtMiddleName"].ToString();
                        userInfoModify.LName = collection["txtLastName"].ToString();
                        userInfoModify.BusinessName = collection["txtBusinessName"].ToString();
                        userInfoModify.RegistrationNumber = collection["txtRegistrationNumber"].ToString();
                        userInfoModify.VATNumber = collection["txtVATNumber"].ToString();
                        userInfoModify.PStreet = collection["PStreet"].ToString();
                        userInfoModify.PVDC = collection["txtPVDC"].ToString();
                        userInfoModify.PHouseNo = collection["txtPHouseNo"].ToString();
                        userInfoModify.PWardNo = collection["txtPWardNo"].ToString();
                        userInfoModify.PProvince = collection["PProvince"].ToString();
                        userInfoModify.PDistrictID = collection["PDistrictID"].ToString();
                        userInfoModify.LandlineNumber = collection["txtLandlineNumber"].ToString();
                        userInfoModify.EmailAddress = collection["Email"].ToString();
                        userInfoModify.Name = userInfoModify.FName + " " + userInfoModify.MName + " " + userInfoModify.LName;
                        userInfoModify.Address = userInfoModify.PVDC + " " + userInfoModify.PDistrictID + " " + userInfoModify.PProvince;


                        if ((cIsRejected != "") && (cIsApproved != "")
                            && (cContactNumber1 != "") && (cStatus != ""))
                        {
                            bool isUpdated = CustomerUtils.UpdateRejectedMerchantInfo(userInfoModify);
                            displayMessage = isUpdated
                                                     ? "Merchant Information has successfully been updated. Please go to Approve Merchant Details"
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

                        return RedirectToAction("RejectedMerchantList");
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

        #region "GET: List for Approved Merchants "
        [HttpGet]
        public ActionResult ApprovedMerchantList()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (this.TempData["merchant_messsage"] != null)
            {
                this.ViewData["merchant_messsage"] = this.TempData["merchant_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                string IsModified = "F";
                List<UserInfo> userobj = new List<UserInfo>();

                /// <summary>
                /// UNAPPROVE REJECTED LIST OF Merchant
                /// 
                //userobj = ProfileUtils.GetMerchantApprovedList(IsModified);
                //DataTable dtableUNApproveReject = ProfileUtils.GetUnApproveRJCustomerProfile();



                return View(userobj);

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        #endregion

        #region "GET: List for Newly Registered Merchants "
        [HttpGet]
        public ActionResult RegisteredMerchants()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (this.TempData["merchant_messsage"] != null)
            {
                this.ViewData["merchant_messsage"] = this.TempData["merchant_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                string IsModified = "F";
                List<UserInfo> userobj = new List<UserInfo>();

                /// <summary>
                /// UNAPPROVE REJECTED LIST OF Merchant
                /// 
                //userobj = ProfileUtils.GetRegisteredMerchantList(IsModified);
                //DataTable dtableUNApproveReject = ProfileUtils.GetUnApproveRJCustomerProfile();



                return View(userobj);

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        #endregion


        #region "GET: View Merchant Details/ this is triggered when the search option is clicked"
        // GET: Payee/Modification
        [HttpGet]
        public ActionResult ViewMerchantDetails(string id)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];



            //if (this.TempData["merchant_messsage"] != null)
            //{
            //    this.ViewData["merchant_messsage"] = this.TempData["merchant_messsage"];
            //    this.ViewData["message_class"] = this.TempData["message_class"];
            //}

            TempData["userType"] = userType;

            if (TempData["userType"] != null && id != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                UserInfo regobj = new UserInfo();

                DataTable dtableUserInfo = ProfileUtils.GetMerchantProfileInfo(id.ToString());

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
                    regobj.PDistrictID = getDistrictName(dtableUserInfo.Rows[0]["PDistrictID"].ToString());

                    regobj.PProvince = getProvince(dtableUserInfo.Rows[0]["PProvince"].ToString());

                    regobj.FName = dtableUserInfo.Rows[0]["FirstName"].ToString();
                    regobj.MName = dtableUserInfo.Rows[0]["MiddleName"].ToString();
                    regobj.LName = dtableUserInfo.Rows[0]["LastName"].ToString();
                    regobj.BusinessName = dtableUserInfo.Rows[0]["BusinessName"].ToString();
                    regobj.RegistrationNumber = dtableUserInfo.Rows[0]["RegistrationNumber"].ToString();
                    regobj.VATNumber = dtableUserInfo.Rows[0]["VATNumber"].ToString();
                    regobj.PStreet = dtableUserInfo.Rows[0]["PStreet"].ToString();
                    regobj.PVDC = dtableUserInfo.Rows[0]["PMunicipalityVDC"].ToString();
                    regobj.PHouseNo = dtableUserInfo.Rows[0]["PHouseNo"].ToString();
                    regobj.PWardNo = dtableUserInfo.Rows[0]["PWardNo"].ToString();
                    regobj.LandlineNumber = dtableUserInfo.Rows[0]["LandlineNumber"].ToString();
                    regobj.EmailAddress = dtableUserInfo.Rows[0]["EmailAddress"].ToString();

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
        #endregion


        public string getProvince(string id)
        {
            string provincestring = "select * from MNProvince where ProvinceID='" + id + "'";


            DataTable dt = new DataTable();
            dt = objdal.MyMethod(provincestring);
            string name = string.Empty;
            foreach (DataRow row in dt.Rows)
            {
                name = row["Name"].ToString();


            }
            return name;


        }

        public List<SelectListItem> ProvinceToDistrict(string id)
        {
            string districtstring = "select * from MNDistrict WHERE ProvinceID = '" + id + "'";
            DataTable dt1 = new DataTable();
            dt1 = objdal.MyMethod(districtstring);
            List<SelectListItem> list1 = new List<SelectListItem>();
            foreach (DataRow row in dt1.Rows)
            {
                list1.Add(new SelectListItem
                {
                    Text = Convert.ToString(row.ItemArray[1]),
                    Value = Convert.ToString(row.ItemArray[0])
                });

            }
            return list1;
        }

        public string getDistrictName(string id)
        {
            string districtstring = "select Name from MNDistrict where DistrictID='" + id + "'";
            DataTable dt = new DataTable();
            dt = objdal.MyMethod(districtstring);
            string name = string.Empty;
            foreach (DataRow row in dt.Rows)
            {
                name = row["Name"].ToString();


            }
            return name;
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

        public JsonResult getdistrict(int id)
        {
            string provincestring = "select * from MNDistrict where ProvinceID='" + id + "'";


            DataTable dt = new DataTable();
            dt = objdal.MyMethod(provincestring);
            List<SelectListItem> list = new List<SelectListItem>();
            //list.Add(new SelectListItem { Text = "--Select District--", Value = "0" });
            foreach (DataRow row in dt.Rows)
            {

                list.Add(new SelectListItem { Text = Convert.ToString(row.ItemArray[1]), Value = Convert.ToString(row.ItemArray[0]) });

            }
            return Json(new SelectList(list, "Value", "Text", JsonRequestBehavior.AllowGet));


        }

        public string getProvice(int id)
        {
            string provincestring = "select * from MNProvince where ProvinceID='" + id + "'";


            DataTable dt = new DataTable();
            dt = objdal.MyMethod(provincestring);
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem { Text = "--Select Province--", Value = "0" });
            foreach (DataRow row in dt.Rows)
            {

                list.Add(new SelectListItem { Text = Convert.ToString(row.ItemArray[1]), Value = Convert.ToString(row.ItemArray[0]) });

            }
            return Json(new SelectList(list, "Text", "Text", JsonRequestBehavior.AllowGet)).ToString();


        }

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


    }
}