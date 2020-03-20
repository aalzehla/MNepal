using MNSuperadmin.Utilities;
using MNSuperadmin.Models;
using MNSuperadmin.Settings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Reflection;
using System.Dynamic;
using MNSuperadmin.ViewModel;
using MNSuperadmin.UserModels;
using MNSuperadmin.Helper;

namespace MNSuperadmin.Controllers
{
    public class CustomerController : Controller
    {
        public ActionResult WalletKYCVerification(string Key, string Value)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            //Check Role link start    
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkRole(System.Reflection.MethodBase.GetCurrentMethod().Name, clientCode);
            //Check Role link end


            if (this.TempData["custapprove_messsage"] != null)
            {
                this.ViewData["custapprove_messsage"] = this.TempData["custapprove_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            TempData["userType"] = userType;

            if (TempData["userType"] != null&&checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                List<UserInfo> custUnApproveObj = new List<UserInfo>();
                UserInfo userInfo = new UserInfo();
                DataTable dtblUnapproveCus = new DataTable();
                DataTable dtblCus = CustomerUtils.GetUnApproveCustKYCProfile("user");
                string COCs = (string)Session["COC"].ToString();  
                bool COC;
                if (Boolean.TryParse(COCs, out COC))
                    COC = true;
                else
                    COC = false;
                 
                string UserBranchCode = "999";
                ViewBag.Value = Value;
                if (Key == "Mobile")
                {
                    if (!string.IsNullOrEmpty(Value))
                    {
                        EnumerableRowCollection<DataRow> row;
                        if (COC)
                        {
                            row = dtblCus.AsEnumerable().Where(r => r.Field<string>("MobileNumber") == Value);
                        }
                        else
                        {
                            row = dtblCus.AsEnumerable().Where(r => r.Field<string>("MobileNumber") == Value).Where(r => r.Field<string>("UserBranchCode") == UserBranchCode);
                        }

                        if (row.Any())
                            dtblUnapproveCus = row.CopyToDataTable();
                    }

                }
                else
                {
                    EnumerableRowCollection<DataRow> row;
                    if (!COC)
                    {
                        row = dtblCus.AsEnumerable().Where(r => r.Field<string>("UserBranchCode") == UserBranchCode);
                        if (row.Any())
                            dtblUnapproveCus = row.CopyToDataTable();
                    }
                    else
                    {
                        dtblUnapproveCus = dtblCus;
                    }
                }

                if (dtblUnapproveCus.Rows.Count > 0)
                {
                    userInfo.ClientCode = dtblUnapproveCus.Rows[0]["ClientCode"].ToString();
                    userInfo.ContactNumber1 = dtblUnapproveCus.Rows[0]["MobileNumber"].ToString();
                    userInfo.Name = dtblUnapproveCus.Rows[0]["Name"].ToString();
                    userInfo.Address = dtblUnapproveCus.Rows[0]["Address"].ToString();
                     ViewBag.CustClientCode = userInfo.ClientCode;
                    ViewBag.ContactNumber1 = userInfo.ContactNumber1;
                    ViewBag.CName = userInfo.Name;
                    ViewBag.Address = userInfo.Address;
                    ViewBag.SearchValue = Value;
                    custUnApproveObj.Add(userInfo);


                }


                //var selfregistred = dtblUnapproveCus.AsEnumerable().Where(r => r.Field<string>("SelfRegistered") == "T");
                //ViewData["dtableCustForApprovalSelf"] = new DataTable();

                //if (selfregistred.Any())
                //    ViewData["dtableCustForApprovalSelf"] = selfregistred.CopyToDataTable();


                //start milayako 01
                var selfregistred = dtblUnapproveCus.AsEnumerable().Where(r => r.Field<string>("SelfRegistered") == "T");
                var agentregistred = dtblUnapproveCus.AsEnumerable().Where(r => r.Field<string>("AgentRegistered") == "T");

                ViewData["dtableCustForApprovalSelf"] = new DataTable();
                ViewData["dtableCustForApprovalAgent"] = new DataTable();

                if (selfregistred.Any())
                    ViewData["dtableCustForApprovalSelf"] = selfregistred.CopyToDataTable();
                if (agentregistred.Any())
                    ViewData["dtableCustForApprovalAgent"] = agentregistred.CopyToDataTable();
                //end milayako 01

                return View(userInfo);

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
         
        public ActionResult AgentWalletKYC(string Key, string Value)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            if (this.TempData["custapprove_messsage"] != null)
            {
                this.ViewData["custapprove_messsage"] = this.TempData["custapprove_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            TempData["userType"] = userType;
            //Check Role link start    
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkRole(System.Reflection.MethodBase.GetCurrentMethod().Name, clientCode);
            //Check Role link end


            if (TempData["userType"] != null&& checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                List<UserInfo> custUnApproveObj = new List<UserInfo>();
                UserInfo userInfo = new UserInfo();
                DataTable dtblUnapproveCus = new DataTable();
                DataTable dtblCus = CustomerUtils.GetUnApproveCustKYCProfile("user");
                string COCs = (string)Session["COC"].ToString();
                bool COC;
                if (Boolean.TryParse(COCs, out COC))
                    COC = true;
                else
                    COC = false;

                string UserBranchCode = "999";
                ViewBag.Value = Value;
                if (Key == "Mobile")
                {
                    if (!string.IsNullOrEmpty(Value))
                    {
                        EnumerableRowCollection<DataRow> row;
                        if (COC)
                        {
                            row = dtblCus.AsEnumerable().Where(r => r.Field<string>("MobileNumber") == Value);
                        }
                        else
                        {
                            row = dtblCus.AsEnumerable().Where(r => r.Field<string>("MobileNumber") == Value).Where(r => r.Field<string>("UserBranchCode") == UserBranchCode);
                        }

                        if (row.Any())
                            dtblUnapproveCus = row.CopyToDataTable();
                    }

                }
                else
                {
                    EnumerableRowCollection<DataRow> row;
                    if (!COC)
                    {
                        row = dtblCus.AsEnumerable().Where(r => r.Field<string>("UserBranchCode") == UserBranchCode);
                        if (row.Any())
                            dtblUnapproveCus = row.CopyToDataTable();
                    }
                    else
                    {
                        dtblUnapproveCus = dtblCus;
                    }
                }

                if (dtblUnapproveCus.Rows.Count > 0)
                {
                    userInfo.ClientCode = dtblUnapproveCus.Rows[0]["ClientCode"].ToString();
                    userInfo.ContactNumber1 = dtblUnapproveCus.Rows[0]["MobileNumber"].ToString();
                    userInfo.Name = dtblUnapproveCus.Rows[0]["Name"].ToString();
                    userInfo.Address = dtblUnapproveCus.Rows[0]["Address"].ToString();


                    ViewBag.CustClientCode = userInfo.ClientCode;
                    ViewBag.ContactNumber1 = userInfo.ContactNumber1;
                    ViewBag.CName = userInfo.Name;
                    ViewBag.Address = userInfo.Address;
                    ViewBag.SearchValue = Value;
                    custUnApproveObj.Add(userInfo);


                }
                  
                var selfregistred = dtblUnapproveCus.AsEnumerable().Where(r => r.Field<string>("SelfRegistered") == "T");
                var agentregistred = dtblUnapproveCus.AsEnumerable().Where(r => r.Field<string>("AgentRegistered") == "T");

                ViewData["dtableCustForApprovalSelf"] = new DataTable();
                ViewData["dtableCustForApprovalAgent"] = new DataTable();

                if (selfregistred.Any())
                    ViewData["dtableCustForApprovalSelf"] = selfregistred.CopyToDataTable();
                if (agentregistred.Any())
                    ViewData["dtableCustForApprovalAgent"] = agentregistred.CopyToDataTable();
                 
                return View(userInfo);

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
         
        public ActionResult Approve(UserInfo model, string btnApprove, FormCollection collection, string Remarks)
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

                //For Remarks Other
                //string Remarks = Request.Form["Remarks"].ToString();
                //if (Remarks.Equals("Others"))
                //{
                //    Remarks = Request.Form["OthersRemarks"].ToString();
                //}

                string displayMessage = null;
                string messageClass = null;

                UserInfo userInfoApproval = new UserInfo();
                userInfoApproval.ClientCode = model.ClientCode;
                userInfoApproval.UserName = userName;
                if (btnApprove.ToUpper() == "REJECT")
                {
                    if (Remarks.Equals("Others"))
                    {
                        userInfoApproval.Remarks = collection["OthersRemarks"].ToString();
                    }
                    else
                    {
                        userInfoApproval.Remarks = model.Remarks;
                    }
                    //userInfoApproval.Remarks = model.Remarks;
                    int ret = CustomerUtils.CustInfoReject(userInfoApproval);
                    if (ret == 100)
                    {
                        displayMessage = "Customer " + model.FName + " has been Rejected. Please Check Wallet Customer Status and perform accordingly";
                        //SMS
                        SMSUtils SMS = new SMSUtils();

                        string Message = string.Format("Dear {0},\n Your KYC has been rejected" + ".\n Thank You -MNepal", model.FName);
                        //string Message = "Dear Customer," + "\n";
                        //Message += " Your T-Pin is " + Pin + " and Password is " + Password
                        //    + "." + "\n" + "Thank You";
                        //Message += "-MNepal";

                        SMSLog Log = new SMSLog();

                        Log.UserName = model.UserName;
                        Log.Purpose = "NR"; //New Registration
                        Log.SentBy = name;
                        Log.Message = Message;
                        //Log SMS
                        CustomerUtils.LogSMS(Log);
                        SMS.SendSMS(Message, model.UserName);



                        
                        messageClass = CssSetting.SuccessMessageClass;
                    }
                    else if (ret == -1 || ret != 100)
                    {
                        displayMessage = "Error while rejecting Customer " + model.Name;
                        messageClass = CssSetting.FailedMessageClass;
                    }

                    this.TempData["custapprove_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;

                    return RedirectToAction("AgentWalletKYC");

                }
                else if (btnApprove.ToUpper() == "APPROVE")
                {
                    userInfoApproval.ApprovedBy = userName;
                    int ret = CustomerUtils.CustInfoApproval(userInfoApproval);
                    if (ret == 100)
                    {
                        //DataSet DSet = ProfileUtils.GetUserProfileInfoDS(model.ClientCode);
                        //DataTable dtableUserInfo = DSet.Tables["dtUserInfo"];
                        //string Pin = dtableUserInfo.Rows[0]["PIN"].ToString();
                        //string Password = dtableUserInfo.Rows[0]["Password"].ToString();

                        displayMessage = "Customer Information for  user " + model.FName + " has successfully been approved.";

                        //model.Password = CustomerUtils.GeneratePassword();
                        bool passChange = true;//PasswordUtils.ResetPassword(model);
                        if (passChange) { 
                        //SMS
                        SMSUtils SMS = new SMSUtils();

                            //string Message = string.Format("Dear {0},\n Your new T-Pin is {1} and Password is {2}.\n Thank You -MNepal", model.FName, Pin, model.Password);
                            //string Message = string.Format("Dear {0},\n Your KYC at NIBL has been verified. \n Thank You -MNepal", model.FName);
                            string Message = string.Format("Dear {0},\n Your KYC has been verified. Please logout and re-login your thaili wallet. \n Thank You -MNepal", model.Name);

                            //string Message = "Dear Customer," + "\n";
                            //Message += " Your T-Pin is " + Pin + " and Password is " + Password
                            //    + "." + "\n" + "Thank You";
                            //Message += "-MNepal";

                            SMSLog Log = new SMSLog();

                        Log.UserName = model.UserName;
                        Log.Purpose = "NR"; //New Registration
                        Log.SentBy = userInfoApproval.ApprovedBy;
                        Log.Message = Message; //encrypted when logging
                        //Log SMS
                        CustomerUtils.LogSMS(Log);
                        SMS.SendSMS(Message, model.UserName);


                        messageClass = CssSetting.SuccessMessageClass;
                        }
                        else {
                            displayMessage = "Error while approving Customer Information";
                            messageClass = CssSetting.FailedMessageClass;
                        }
                    }
                    else if (ret == -1 || ret != 100)
                    {
                        displayMessage = "Error while approving Customer Information";
                        messageClass = CssSetting.FailedMessageClass;
                    }

                    this.TempData["custapprove_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;

                    return RedirectToAction("AgentWalletKYC");
                }
                else
                { 
                    this.TempData["custapprove_messsage"] = "Unauthorized execution";
                    this.TempData["message_class"] = CssSetting.FailedMessageClass;

                    return RedirectToAction("AgentWalletKYC");
                }
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }
        public ActionResult KYCApprove(UserInfo model, string btnApprove, FormCollection collection, string Remarks)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            model.Name = model.FName;
            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                //For Remarks Other
                //if (model.Remarks.Equals("Others"))
                //{
                //    model.Remarks = "Other: "+Request.Form["OthersRemarks"].ToString();
                //}

                string displayMessage = null;
                string messageClass = null;

                UserInfo userInfoApproval = new UserInfo();
                userInfoApproval.ClientCode = model.ClientCode;
                userInfoApproval.UserName = userName;
                if (btnApprove.ToUpper() == "REJECT")
                {
                    if (Remarks.Equals("Others"))
                    {
                        userInfoApproval.Remarks = collection["OthersRemarks"].ToString();
                    }
                    else
                    {
                        userInfoApproval.Remarks = model.Remarks;
                    }
                    //userInfoApproval.Remarks = model.Remarks;
                    int ret = CustomerUtils.CustInfoReject(userInfoApproval);
                    if (ret == 100)
                    {
                        
                        displayMessage = "Customer " + model.Name + " has been Rejected. Please Check Wallet Customer Status and perform accordingly";
                        messageClass = CssSetting.SuccessMessageClass;
                        //SMS
                        SMSUtils SMS = new SMSUtils();

                        string Message = string.Format("Dear {0},\n Your KYC at NIBL has been rejected. Please, submit once again referring to the remarks. \n Thank You -MNepal", model.Name);
                        //string Message = "Dear Customer," + "\n";
                        //Message += " Your T-Pin is " + Pin + " and Password is " + Password
                        //    + "." + "\n" + "Thank You";
                        //Message += "-MNepal";

                        SMSLog Log = new SMSLog();

                        Log.UserName = model.UserName;
                        Log.Purpose = "NR"; //New Registration
                        Log.SentBy = model.UserName;
                        Log.Message = Message;
                        //Log SMS
                        CustomerUtils.LogSMS(Log);
                        SMS.SendSMS(Message, model.UserName);
                    }
                    else if (ret == -1 || ret != 100)
                    {
                        displayMessage = "Error while rejecting Customer " + model.Name;
                        messageClass = CssSetting.FailedMessageClass;
                    }

                    this.TempData["custapprove_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;

                    return RedirectToAction("WalletKYCVerification");

                }
                else
                if (btnApprove.ToUpper() == "APPROVE")
                {
                    userInfoApproval.ApprovedBy = userName;
                    int ret = CustomerUtils.CustKYCInfoApproval(userInfoApproval);
                    if (ret == 100)
                    {
                       

                        displayMessage = "Customer Information for  user " + model.Name + " has successfully been approved.";


                        //SMS
                        SMSUtils SMS = new SMSUtils();

                        string Message = string.Format("Dear {0},\n Your KYC has been verified. Please logout and re-login your thaili wallet. \n Thank You -MNepal", model.Name);
                        //string Message = "Dear Customer," + "\n";
                        //Message += " Your T-Pin is " + Pin + " and Password is " + Password
                        //    + "." + "\n" + "Thank You";
                        //Message += "-MNepal";

                        SMSLog Log = new SMSLog();

                        Log.UserName = model.UserName;
                        Log.Purpose = "NR"; //New Registration
                        Log.SentBy = userInfoApproval.ApprovedBy;
                        Log.Message = Message;
                        //Log SMS
                        CustomerUtils.LogSMS(Log);
                        SMS.SendSMS(Message, model.UserName);


                        messageClass = CssSetting.SuccessMessageClass;
                    }
                    else if (ret == -1 || ret != 100)
                    {
                        displayMessage = "Error while approving Customer Information";
                        messageClass = CssSetting.FailedMessageClass;
                    }

                    this.TempData["custapprove_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;

                    return RedirectToAction("WalletKYCVerification");
                }
                else
                {

                    this.TempData["custapprove_messsage"] = "Unauthorized execution";
                    this.TempData["message_class"] = CssSetting.FailedMessageClass;

                    return RedirectToAction("WalletKYCVerification");
                }
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }

        public ActionResult ViewSelfRegDetail(string id)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            string mobile = (string)Session["Mobile"];

            if (this.TempData["custmodify_messsage"] != null)
            {
                this.ViewData["custmodify_messsage"] = this.TempData["custmodify_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;



                CustomerSRInfo srInfo = new CustomerSRInfo();
                DataSet DSet = ProfileUtils.GetSelfRegDetailDS(id);
                DataTable dtableSrInfo = DSet.Tables["dtSrInfo"];


                if (dtableSrInfo != null && dtableSrInfo.Rows.Count > 0)
                {
                    srInfo.ClientCode = dtableSrInfo.Rows[0]["ClientCode"].ToString();

                    srInfo.UserName = dtableSrInfo.Rows[0]["UserName"].ToString();

                    srInfo.Name = srInfo.FName + " " + srInfo.MName + " " + srInfo.LName;
                    srInfo.FName = dtableSrInfo.Rows[0]["FName"].ToString();
                    srInfo.MName = dtableSrInfo.Rows[0]["MName"].ToString();
                    srInfo.LName = dtableSrInfo.Rows[0]["LName"].ToString();
                     
                    srInfo.Gender = dtableSrInfo.Rows[0]["Gender"].ToString();
                    srInfo.DOB = dtableSrInfo.Rows[0]["DateOfBirth"].ToString().Split()[0];
                    srInfo.BSDateOfBirth = dtableSrInfo.Rows[0]["BSDateOfBirth"].ToString();

                    srInfo.Nationality = dtableSrInfo.Rows[0]["Nationality"].ToString();
                    srInfo.FatherName = dtableSrInfo.Rows[0]["FathersName"].ToString();
                    srInfo.MotherName = dtableSrInfo.Rows[0]["MothersName"].ToString();
                    srInfo.SpouseName = dtableSrInfo.Rows[0]["SpouseName"].ToString();
                    srInfo.MaritalStatus = dtableSrInfo.Rows[0]["MaritalStatus"].ToString(); 

                    srInfo.GrandFatherName = dtableSrInfo.Rows[0]["GFathersName"].ToString();
                    srInfo.FatherInlawName = dtableSrInfo.Rows[0]["FatherInlaw"].ToString();
                    srInfo.Occupation = dtableSrInfo.Rows[0]["Occupation"].ToString();


                    srInfo.EmailAddress = dtableSrInfo.Rows[0]["EmailAddress"].ToString();
                    srInfo.PanNo = dtableSrInfo.Rows[0]["PanNumber"].ToString();


                    srInfo.Country = dtableSrInfo.Rows[0]["CountryCode"].ToString();


                    srInfo.PProvince = dtableSrInfo.Rows[0]["PProvince"].ToString();
                    srInfo.PDistrict = dtableSrInfo.Rows[0]["PDistrict"].ToString();

                    srInfo.PVDC = dtableSrInfo.Rows[0]["PMunicipalityVDC"].ToString();
                    srInfo.PHouseNo = dtableSrInfo.Rows[0]["PHouseNo"].ToString();
                    srInfo.PWardNo = dtableSrInfo.Rows[0]["PWardNo"].ToString();
                    srInfo.PStreet = dtableSrInfo.Rows[0]["PStreet"].ToString();

                    srInfo.CProvince = dtableSrInfo.Rows[0]["CProvince"].ToString();
                    srInfo.CDistrict = dtableSrInfo.Rows[0]["CDistrict"].ToString();


                    srInfo.CVDC = dtableSrInfo.Rows[0]["CMunicipalityVDC"].ToString();
                    srInfo.CHouseNo = dtableSrInfo.Rows[0]["CHouseNo"].ToString();
                    srInfo.CWardNo = dtableSrInfo.Rows[0]["CWardNo"].ToString();
                    srInfo.CStreet = dtableSrInfo.Rows[0]["CStreet"].ToString();

                    srInfo.Citizenship = dtableSrInfo.Rows[0]["CitizenshipNo"].ToString();
                    srInfo.CitizenshipIssueDate = dtableSrInfo.Rows[0]["CitizenIssueDate"].ToString().Split()[0];
                    srInfo.BSCitizenshipIssueDate = dtableSrInfo.Rows[0]["BSCitizenIssueDate"].ToString();
                    srInfo.CitizenshipPlaceOfIssue = dtableSrInfo.Rows[0]["CitizenPlaceOfIssue"].ToString();

                    srInfo.License = dtableSrInfo.Rows[0]["LicenseNo"].ToString();

                    srInfo.LicensePlaceOfIssue = dtableSrInfo.Rows[0]["LicensePlaceOfIssue"].ToString();
                    srInfo.LicenseIssueDate = dtableSrInfo.Rows[0]["LicenseIssueDate"].ToString();
                    srInfo.BSLicenseIssueDate = dtableSrInfo.Rows[0]["BSLicenseIssueDate"].ToString();
                    srInfo.LicenseExpireDate = dtableSrInfo.Rows[0]["LicenseExpiryDate"].ToString();
                    srInfo.BSLicenseExpireDate = dtableSrInfo.Rows[0]["BSLicenseExpiryDate"].ToString();


                    srInfo.Passport = dtableSrInfo.Rows[0]["PassportNo"].ToString();

                    srInfo.PassportPlaceOfIssue = dtableSrInfo.Rows[0]["PassportPlaceOfIssue"].ToString();
                    srInfo.PassportIssueDate = dtableSrInfo.Rows[0]["PassportIssueDate"].ToString();
                    srInfo.PassportExpireDate = dtableSrInfo.Rows[0]["PassportExpiryDate"].ToString();


                    srInfo.Document = dtableSrInfo.Rows[0]["DocType"].ToString();
                     
                    srInfo.BranchCode = "004";

                    srInfo.FrontImage = dtableSrInfo.Rows[0]["FrontImage"].ToString();
                    srInfo.BackImage = dtableSrInfo.Rows[0]["BackImage"].ToString();
                    srInfo.PassportImage = dtableSrInfo.Rows[0]["PassportImage"].ToString();
                    srInfo.CustStatus = dtableSrInfo.Rows[0]["CustStatus"].ToString();
                    ViewBag.CustStatus = srInfo.CustStatus;
                    ViewBag.DocType = srInfo.Document;
                    ViewBag.FrontImage = srInfo.FrontImage;
                    ViewBag.BackImage = srInfo.BackImage;
                    ViewBag.PassportImage = srInfo.PassportImage;

                    srInfo.DOB = DateConvert(srInfo.DOB);

                    srInfo.CitizenshipIssueDate = DateConvert(srInfo.CitizenshipIssueDate);
                    srInfo.LicenseIssueDate = DateConvert(srInfo.LicenseIssueDate);
                    srInfo.LicenseExpireDate = DateConvert(srInfo.LicenseExpireDate);

                    srInfo.PassportIssueDate = DateConvert(srInfo.PassportIssueDate);
                    srInfo.PassportExpireDate = DateConvert(srInfo.PassportExpireDate);
                } 
                return View(srInfo);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        
        #region Agent Registration Customer Approval
        public ActionResult ViewAgentRegDetail(string id)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            string mobile = (string)Session["Mobile"];

            if (this.TempData["custmodify_messsage"] != null)
            {
                this.ViewData["custmodify_messsage"] = this.TempData["custmodify_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;



                CustomerSRInfo srInfo = new CustomerSRInfo();
                DataSet DSet = ProfileUtils.GetAgentRegDetailDS(id);
                DataTable dtableSrInfo = DSet.Tables["dtSrInfo"];


                if (dtableSrInfo != null && dtableSrInfo.Rows.Count > 0)
                {
                    srInfo.ClientCode = dtableSrInfo.Rows[0]["ClientCode"].ToString();

                    srInfo.UserName = dtableSrInfo.Rows[0]["UserName"].ToString();

                    srInfo.Name = srInfo.FName + " " + srInfo.MName + " " + srInfo.LName;
                    srInfo.FName = dtableSrInfo.Rows[0]["FName"].ToString();
                    srInfo.MName = dtableSrInfo.Rows[0]["MName"].ToString();
                    srInfo.LName = dtableSrInfo.Rows[0]["LName"].ToString();



                    srInfo.Gender = dtableSrInfo.Rows[0]["Gender"].ToString();
                    srInfo.DOB = dtableSrInfo.Rows[0]["DateOfBirth"].ToString().Split()[0];
                    srInfo.BSDateOfBirth = dtableSrInfo.Rows[0]["BSDateOfBirth"].ToString().Split()[0];

                    srInfo.Nationality = dtableSrInfo.Rows[0]["Nationality"].ToString();
                    srInfo.FatherName = dtableSrInfo.Rows[0]["FathersName"].ToString();
                    srInfo.MotherName = dtableSrInfo.Rows[0]["MothersName"].ToString();
                    srInfo.SpouseName = dtableSrInfo.Rows[0]["SpouseName"].ToString();
                    srInfo.MaritalStatus = dtableSrInfo.Rows[0]["MaritalStatus"].ToString();
                    srInfo.MaritalStatus = srInfo.MaritalStatus[0].ToString();

                    srInfo.GrandFatherName = dtableSrInfo.Rows[0]["GFathersName"].ToString();
                    srInfo.FatherInlawName = dtableSrInfo.Rows[0]["FatherInlaw"].ToString();
                    srInfo.Occupation = dtableSrInfo.Rows[0]["Occupation"].ToString();


                    srInfo.EmailAddress = dtableSrInfo.Rows[0]["EmailAddress"].ToString();
                    srInfo.PanNo = dtableSrInfo.Rows[0]["PanNumber"].ToString();


                    srInfo.Country = dtableSrInfo.Rows[0]["CountryCode"].ToString();

                    srInfo.PStreet = dtableSrInfo.Rows[0]["PStreet"].ToString();
                    srInfo.PProvince = dtableSrInfo.Rows[0]["PProvince"].ToString();
                    srInfo.PDistrict = dtableSrInfo.Rows[0]["PDistrict"].ToString();

                    srInfo.PVDC = dtableSrInfo.Rows[0]["PMunicipalityVDC"].ToString();
                    srInfo.PHouseNo = dtableSrInfo.Rows[0]["PHouseNo"].ToString();
                    srInfo.PWardNo = dtableSrInfo.Rows[0]["PWardNo"].ToString();

                    srInfo.CStreet = dtableSrInfo.Rows[0]["CStreet"].ToString();
                    srInfo.CProvince = dtableSrInfo.Rows[0]["CProvince"].ToString();
                    srInfo.CDistrict = dtableSrInfo.Rows[0]["CDistrict"].ToString();


                    srInfo.CVDC = dtableSrInfo.Rows[0]["CMunicipalityVDC"].ToString();
                    srInfo.CHouseNo = dtableSrInfo.Rows[0]["CHouseNo"].ToString();
                    srInfo.CWardNo = dtableSrInfo.Rows[0]["CWardNo"].ToString();




                    srInfo.Citizenship = dtableSrInfo.Rows[0]["CitizenshipNo"].ToString();
                    srInfo.CitizenshipIssueDate = dtableSrInfo.Rows[0]["CitizenIssueDate"].ToString().Split()[0];
                    srInfo.BSCitizenshipIssueDate = dtableSrInfo.Rows[0]["BSCitizenIssueDate"].ToString().Split()[0];
                    srInfo.CitizenshipPlaceOfIssue = dtableSrInfo.Rows[0]["CitizenPlaceOfIssue"].ToString();

                    srInfo.License = dtableSrInfo.Rows[0]["LicenseNo"].ToString();

                    srInfo.LicensePlaceOfIssue = dtableSrInfo.Rows[0]["LicensePlaceOfIssue"].ToString();
                    srInfo.LicenseIssueDate = dtableSrInfo.Rows[0]["LicenseIssueDate"].ToString();
                    srInfo.BSLicenseIssueDate = dtableSrInfo.Rows[0]["BSLicenseIssueDate"].ToString().Split()[0];
                    srInfo.LicenseExpireDate = dtableSrInfo.Rows[0]["LicenseExpiryDate"].ToString();
                    srInfo.BSLicenseExpireDate = dtableSrInfo.Rows[0]["BSLicenseExpiryDate"].ToString().Split()[0];


                    srInfo.Passport = dtableSrInfo.Rows[0]["PassportNo"].ToString();

                    srInfo.PassportPlaceOfIssue = dtableSrInfo.Rows[0]["PassportPlaceOfIssue"].ToString();
                    srInfo.PassportIssueDate = dtableSrInfo.Rows[0]["PassportIssueDate"].ToString();
                    srInfo.PassportExpireDate = dtableSrInfo.Rows[0]["PassportExpiryDate"].ToString();


                    srInfo.Document = dtableSrInfo.Rows[0]["DocType"].ToString();
                     
                    srInfo.BranchCode = "004";


                    srInfo.FrontImage = dtableSrInfo.Rows[0]["FrontImage"].ToString();
                    srInfo.BackImage = dtableSrInfo.Rows[0]["BackImage"].ToString();
                    srInfo.PassportImage = dtableSrInfo.Rows[0]["PassportImage"].ToString();
                    srInfo.CustStatus = dtableSrInfo.Rows[0]["CustStatus"].ToString();

                    srInfo.DOB = DateConvert(srInfo.DOB);

                    srInfo.CitizenshipIssueDate = DateConvert(srInfo.CitizenshipIssueDate);
                    srInfo.LicenseIssueDate = DateConvert(srInfo.LicenseIssueDate);
                    srInfo.LicenseExpireDate = DateConvert(srInfo.LicenseExpireDate);

                    srInfo.PassportIssueDate = DateConvert(srInfo.PassportIssueDate);
                    srInfo.PassportExpireDate = DateConvert(srInfo.PassportExpireDate);
                }


                return View(srInfo);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        #endregion

        #region "Customer Pin/Password reset by superadmin"

        // GET: Customer/PinResetList
        [HttpGet]
        public ActionResult PinResetList()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;
            //Check Role link start    
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkRole(System.Reflection.MethodBase.GetCurrentMethod().Name, clientCode);
            //Check Role link end


            if (TempData["userType"] != null && checkRole)
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

        [HttpGet]
        public ActionResult PinResetDetail(string txtMobileNo)
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
                if ((txtMobileNo == "") || (txtMobileNo == null))
                {
                    return View();
                }
                else
                {
                    UserInfo userInfo = new UserInfo();
                    userInfo.ContactNumber1 = txtMobileNo;

                    List<UserInfo> CustomerList = new List<UserInfo>();


                    if (userInfo.ContactNumber1 != null)
                    {
                        DataTable dtableCustomerStatusByAc = CustomerUtils.GetCustomerProfileByMobileNo(userInfo.ContactNumber1); 
                        if (dtableCustomerStatusByAc != null && dtableCustomerStatusByAc.Rows.Count > 0)
                        {
                            UserInfo regobj = new UserInfo();
                            regobj.ClientCode = dtableCustomerStatusByAc.Rows[0]["ClientCode"].ToString();
                            regobj.Name = dtableCustomerStatusByAc.Rows[0]["Name"].ToString();
                            regobj.Address = dtableCustomerStatusByAc.Rows[0]["Address"].ToString();
                            regobj.PIN = dtableCustomerStatusByAc.Rows[0]["PIN"].ToString();
                            regobj.Status = dtableCustomerStatusByAc.Rows[0]["Status"].ToString();
                            regobj.ContactNumber1 = dtableCustomerStatusByAc.Rows[0]["ContactNumber1"].ToString();
                            regobj.ContactNumber2 = dtableCustomerStatusByAc.Rows[0]["ContactNumber2"].ToString();
                            regobj.UserName = dtableCustomerStatusByAc.Rows[0]["UserName"].ToString();
                            regobj.UserType = dtableCustomerStatusByAc.Rows[0]["userType"].ToString();

                            CustomerList.Add(regobj);
                            ViewData["dtableCustomerList"] = dtableCustomerStatusByAc;
                        }
                    }
                    return View(CustomerList);
                }
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpGet]
        public ActionResult PinReset(string id)
        {

            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("PinResetList", "Customer");
            }
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (this.TempData["pinreset_messsage"] != null)
            {
                this.ViewData["pinreset_messsage"] = this.TempData["pinreset_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                if ((id != null) || (id != ""))
                {
                    UserInfo regobj = new UserInfo();
                    DataTable dtableUserInfo = ProfileUtils.GetUserProfileInfo(id.ToString());
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
                        regobj.BankAccountNumber = dtableUserInfo.Rows[0]["BankAccountNumber"].ToString();

                    }
                    return View(regobj);
                }
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpPost]
        public ActionResult PinReset(string btnCommand, FormCollection collection)
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

                if (btnCommand == "Reset")
                {
                    string cClientCode = collection["ClientCode"].ToString();
                    string oldPin = collection["PIN"].ToString();
                    //string newPin = collection["txtNewPin"].ToString();
                    string MobileNumber = collection["UserName"].ToString();
                    string CName = collection["Name"].ToString();
                    UserInfo userInfo = new UserInfo();
                    userInfo.OPIN = oldPin;
                    userInfo.ClientCode = cClientCode;
                    userInfo.UserName = MobileNumber;
                    userInfo.AdminBranch = (string)Session["UserBranch"];
                    userInfo.AdminUserName = (string)Session["UserName"];
                    userInfo.Name = CName;
                    string Status = "PINR";
                    string Message = "PIN";
                    var type = collection["type"].ToString();
                    if (type == "Pin")
                    {
                        Status = "PINR";
                        Message = "PIN";
                    }
                    else if (type == "Pass")
                    {
                        Status = "PASR";
                        Message = "Password";
                    }
                    else if (type == "Both")
                    {
                        Status = "PPR";
                        Message = "PIN/Password";
                    }
                    bool isUpdated = PinUtils.UpdateUserPin(userInfo, Status);

                    //bool isInserted
                    //if(isUpdated)
                    //{
                    //    SMSUtils SMS = new SMSUtils();
                    //    string Message = string.Format("Dear {0},\n Your new T-Pin is {1}.\n Thank You -MNepal",userInfo.Name,userInfo.PIN);
                    //    //string Message = "Dear Customer," + "\n";
                    //    //Message += "Your new T-Pin is " + userInfo.PIN 
                    //    //    + "." + "\n" + "Thank You";
                    //    //Message += "-MNepal";
                    //    SMS.SendSMS(Message, userInfo.UserName);

                    //}
                    displayMessage = isUpdated
                                             ? Message + " reset request successful for " + userInfo.UserName + ". Changes will take effect after approval."
                                             : "Error while updating PIN Number";
                    messageClass = isUpdated ? CssSetting.SuccessMessageClass : CssSetting.FailedMessageClass;
                    if (isUpdated)
                    {
                        this.TempData["pinreset_messsage"] = displayMessage;
                        this.TempData["message_class"] = messageClass;
                        return RedirectToAction("PinResetList", "Customer");
                    }

                    else
                    {
                        displayMessage = "Pin is Empty";
                        messageClass = CssSetting.FailedMessageClass;
                    }

                    this.TempData["pinreset_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;
                    return RedirectToAction("PinReset");
                }
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpGet]
        public ActionResult ApprovePin(string MobileNumber)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;
            //Check Role link start    
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkRole(System.Reflection.MethodBase.GetCurrentMethod().Name, clientCode);
            //Check Role link end


            if (TempData["userType"] != null && checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;
                string AdminUserName = (string)Session["UserName"];
                string AdminBranch = (string)Session["UserBranch"];


                string COC = (string)Session["COC"];
                bool flag;
                bool boolCOC = Boolean.TryParse(COC, out flag);
                var model = PinUtils.GetUserPinResetList(AdminBranch, boolCOC, MobileNumber);
                return View(model);
            }
            else { return RedirectToAction("Index", "Login"); }
        }

        [HttpGet]
        public ContentResult PinResetApprove(string ClientCode, string Name, string PhoneNumber, string ActionClick)
        {

            string convert = string.Empty;
            if (string.IsNullOrEmpty(ClientCode))
            {
                convert = JsonConvert.SerializeObject(
                    new
                    {
                        StatusCode = "400",
                        StatusMessage = "Server Error while fetching the data"
                    }
                 );
                return Content(convert, "application/json");
            }
            try
            {
                string AdminUserName = (string)Session["UserName"];
                string AdminBranch = (string)Session["UserBranch"];

                //for converting string to bool in COC

                string COC = (string)Session["COC"];
                bool flag;
                bool boolCOC = Boolean.TryParse(COC, out flag);


                //bool COC = (bool)Session["COC"];
                bool isUpdated = false;

                UserInfo info = new UserInfo();
                DataTable dtableUserInfo = ProfileUtils.GetUserProfileInfo(ClientCode);

                if (dtableUserInfo != null && dtableUserInfo.Rows.Count > 0)
                {
                    info.ClientCode = dtableUserInfo.Rows[0]["ClientCode"].ToString();
                    info.Name = dtableUserInfo.Rows[0]["Name"].ToString();
                    info.Address = dtableUserInfo.Rows[0]["Address"].ToString();
                    info.Status = dtableUserInfo.Rows[0]["Status"].ToString();
                    info.ContactNumber1 = dtableUserInfo.Rows[0]["ContactNumber1"].ToString();
                    info.ContactNumber2 = dtableUserInfo.Rows[0]["ContactNumber2"].ToString();
                    info.UserName = dtableUserInfo.Rows[0]["UserName"].ToString();
                    info.UserType = dtableUserInfo.Rows[0]["userType"].ToString();
                    info.IsApproved = dtableUserInfo.Rows[0]["IsApproved"].ToString();
                    info.IsRejected = dtableUserInfo.Rows[0]["IsRejected"].ToString();
                    info.WalletNumber = dtableUserInfo.Rows[0]["WalletNumber"].ToString();
                    info.BankAccountNumber = dtableUserInfo.Rows[0]["BankAccountNumber"].ToString();

                    //for email
                    info.EmailAddress = dtableUserInfo.Rows[0]["EmailAddress"].ToString();


                }

                if (ActionClick == "Approve")
                {

                    string Mode = "";
                    string Message = string.Empty;
                    string resp = string.Empty;
                    if (info.Status == "PINR")
                    {
                        info.PIN = CustomerUtils.GeneratePin();

                        Mode = "PIN";
                        Message = string.Format("Dear {0},\n Your new T-Pin is {1}.\n Thank You -MNepal", info.Name.Split()[0], info.PIN);
                        resp = "T-Pin reset Successful for " + info.UserName;

                        //for email pin only

                        string Subject = "Customer T-Pin Reset";

                        string MailSubject = "<span style='font-size:15px;'><h4>Dear " + info.Name + ",</h4>";
                        MailSubject += "Your T-Pin reset request for Customer " + info.UserName + " has been acknowledged and you have been issued with a new temporary T-Pin.<br/>";

                        MailSubject += "<br/><b>Username: " + info.UserName + "</b> <br/>";
                        MailSubject += "<br/><b>PIN: " + info.PIN + "</b> <br/>";
                        MailSubject += "                    (Please change your T-Pin after login.)<br/>";

                        MailSubject += "<br/>Thank You <br/>";
                        MailSubject += "<br/>Nepal Investment Bank Ltd. </span><br/>";
                        MailSubject += @"<br/>Note: This is confidential mail, Please do not share with other. Your credential purpose is for NIBL Administration Console only.<br/>";
                        MailSubject += "<hr/>";

                        EMailUtil SendMail = new EMailUtil();
                        try
                        {
                            SendMail.SendMail(info.EmailAddress, Subject, MailSubject);
                        }
                        catch
                        {

                        }



                    }
                    else if (info.Status == "PASR")
                    {
                        info.Password = CustomerUtils.GeneratePassword();
                        Mode = "PASS";
                        Message = string.Format("Dear {0},\n Your new Password is {1}.\n Thank You -MNepal", info.Name.Split()[0], info.Password);
                        resp = "Password reset Successful for " + info.UserName;

                        //for email password only

                        string Subject = "Customer Password Reset";

                        string MailSubject = "<span style='font-size:15px;'><h4>Dear " + info.Name + ",</h4>";
                        MailSubject += "Your password reset request for Customer " + info.UserName + " has been acknowledged and you have been issued with a new temporary password.<br/>";

                        MailSubject += "<br/><b>Username: " + info.UserName + "</b> <br/>";
                        MailSubject += "<br/><b>Password: " + info.Password + "</b> <br/>";
                        MailSubject += "                    (Please change your password after login.)<br/>";

                        MailSubject += "<br/>Thank You <br/>";
                        MailSubject += "<br/>Nepal Investment Bank Ltd. </span><br/>";
                        MailSubject += @"<br/>Note: This is confidential mail, Please do not share with other. Your credential purpose is for NIBL Administration Console only.<br/>";
                        MailSubject += "<hr/>";

                        EMailUtil SendMail = new EMailUtil();
                        try
                        {
                            SendMail.SendMail(info.EmailAddress, Subject, MailSubject);
                        }
                        catch
                        {

                        }



                    }
                    else if (info.Status == "PPR")
                    {
                        info.PIN = CustomerUtils.GeneratePin();
                        info.Password = CustomerUtils.GeneratePassword();
                        Mode = "BOTH";
                        Message = string.Format("Dear {0},\n Your new T-Pin is {1} and Password is {2}.\n Thank You -MNepal", info.Name.Split()[0], info.PIN, info.Password);
                        resp = "T-Pin and Password reset Successful for " + info.UserName.Split()[0];

                        //for email both T-Pin and password

                        string Subject = "Customer Password/T-Pin Reset";

                        string MailSubject = "<span style='font-size:15px;'><h4>Dear " + info.Name + ",</h4>";
                        MailSubject += "Your password and T-Pin reset request for Customer " + info.UserName + " has been acknowledged and you have been issued with a new temporary password and T-Pin.<br/>";

                        MailSubject += "<br/><b>Username: " + info.UserName + "</b> <br/>";
                        MailSubject += "<br/><b>Password: " + info.Password + "</b> <br/>";
                        MailSubject += "<br/><b>PIN: " + info.PIN + "</b> <br/>";
                        MailSubject += "                    (Please change your password and T-Pin after login.)<br/>";

                        MailSubject += "<br/>Thank You <br/>";
                        MailSubject += "<br/>Nepal Investment Bank Ltd. </span><br/>";
                        MailSubject += @"<br/>Note: This is confidential mail, Please do not share with other. Your credential purpose is for NIBL Administration Console only.<br/>";
                        MailSubject += "<hr/>";

                        EMailUtil SendMail = new EMailUtil();
                        try
                        {
                            SendMail.SendMail(info.EmailAddress, Subject, MailSubject);
                        }
                        catch
                        {

                        }

                    }
                    isUpdated = PinUtils.ApproveUserPinInfo(info, Mode);
                    if (isUpdated)
                    {
                        SMSUtils SMS = new SMSUtils();
                        SMS.SendSMS(Message, info.UserName);
                        //string Message = "Dear Customer," + "\n";
                        //Message += "Your new T-Pin is " + userInfo.PIN 
                        //    + "." + "\n" + "Thank You";
                        //Message += "-MNepal";

                        //SMSLog log = new SMSLog();
                        //log.Message = Message;
                        //log.Purpose = "PR";
                        //log.SentBy = Session["UserName"].ToString();
                        //log.UserName = info.UserName;
                        //CustomerUtils.LogSMS(log);

                        //string resp = "T-Pin reset Successful for " + info.UserName;
                        convert = JsonConvert.SerializeObject(
                         new
                         {
                             StatusCode = "200",
                             StatusMessage = resp
                         }
                       );


                    }
                    else
                    {
                        convert = JsonConvert.SerializeObject(
                        new
                        {
                            StatusCode = "210",
                            StatusMessage = "Error Approving Customer pin"
                        }
                    );
                    }
                }
                else if (ActionClick == "Revert")
                {
                    isUpdated = PinUtils.RevertPinInfo(info.ClientCode);
                    if (isUpdated)
                    {

                        convert = JsonConvert.SerializeObject(
                         new
                         {
                             StatusCode = "200",
                             StatusMessage = "Reverted Pin/password request for Customer " + info.Name
                         }
                       );


                    }
                    else
                    {
                        convert = JsonConvert.SerializeObject(
                        new
                        {
                            StatusCode = "210",
                            StatusMessage = "Error Reverting"
                        }
                    );
                    }
                }
                return Content(convert, "application/json");
            }
            catch (Exception ex)
            {
                return Content(ex.Message, "application/json");
            }
        }

        public ContentResult ApprovePinDatatable(DataTableAjaxPostModel model, string MobileNumber, string change)
        {
            int filteredResultsCount;
            int totalResultsCount;
            string ParaChanged = "T";
            string convert;
            string AdminUserName = (string)Session["UserName"];
            string AdminBranch = (string)Session["UserBranch"];
            bool COC = (bool)Session["COC"];
            var result = new List<UserInfo>();
            ParaChanged = change;
            if (Session["PinApprove"] != null && ParaChanged == "F")
            {
                result = Session["PinApprove"] as List<UserInfo>;
            }
            else
            {

                result = PinUtils.GetPinResetList(AdminBranch, COC, MobileNumber);
                Session["PinApprove"] = result;
            }
            var res = ExtraUtility.FilterAndSort<UserInfo>(model, result, out totalResultsCount, out filteredResultsCount);
            convert = JsonConvert.SerializeObject(new
            {
                draw = model.draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = res
            });
            return Content(convert, "application/json");
        }
        #endregion







        public ActionResult WalletCustStatus(string Key, string Value)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            if (this.TempData["custapprove_messsage"] != null)
            {
                this.ViewData["custapprove_messsage"] = this.TempData["custapprove_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                List<UserInfo> custUnApproveObj = new List<UserInfo>();
                UserInfo userInfo = new UserInfo();
                DataTable dtblUnapproveCus = new DataTable();
                //DataTable dtblCus = CustomerUtils.GetUnApproveCustProfile("user");
                DataTable dtblCus = CustomerUtils.GetUnApproveCustProfileWalletCustStatus("user");
                bool COC = Session["COC"] == null ? false : (bool)Session["COC"];
                string UserBranchCode = "999";
                ViewBag.Value = Value;
                if (Key == "Mobile")
                {
                    if (!string.IsNullOrEmpty(Value))
                    {
                        EnumerableRowCollection<DataRow> row;
                        if (COC)
                        {
                            row = dtblCus.AsEnumerable().Where(r => r.Field<string>("MobileNumber") == Value);
                        }
                        else
                        {
                            row = dtblCus.AsEnumerable().Where(r => r.Field<string>("MobileNumber") == Value).Where(r => r.Field<string>("UserBranchCode") == UserBranchCode);
                        }

                        if (row.Any())
                            dtblUnapproveCus = row.CopyToDataTable();
                    }

                }
                else
                {
                    EnumerableRowCollection<DataRow> row;
                    if (!COC)
                    {
                        row = dtblCus.AsEnumerable().Where(r => r.Field<string>("UserBranchCode") == UserBranchCode);
                        if (row.Any())
                            dtblUnapproveCus = row.CopyToDataTable();
                    }
                    else
                    {
                        dtblUnapproveCus = dtblCus;
                    }
                }

                if (dtblUnapproveCus.Rows.Count > 0)
                {
                    userInfo.ClientCode = dtblUnapproveCus.Rows[0]["ClientCode"].ToString();
                    userInfo.ContactNumber1 = dtblUnapproveCus.Rows[0]["MobileNumber"].ToString();
                    userInfo.Name = dtblUnapproveCus.Rows[0]["Name"].ToString();
                    userInfo.Address = dtblUnapproveCus.Rows[0]["Address"].ToString();
                    userInfo.HasKYC = dtblUnapproveCus.Rows[0]["HasKYC"].ToString();

                    ViewBag.CustClientCode = userInfo.ClientCode;
                    ViewBag.ContactNumber1 = userInfo.ContactNumber1;
                    ViewBag.CName = userInfo.Name;
                    ViewBag.Address = userInfo.Address;
                    ViewBag.HasKYC = userInfo.HasKYC;
                    ViewBag.SearchValue = Value;
                    custUnApproveObj.Add(userInfo);
                }
                 
                var selfregistred = dtblUnapproveCus.AsEnumerable().Where(r => r.Field<string>("SelfRegistered") == "T");
                //var agentregistred = dtblUnapproveCus.AsEnumerable().Where(r => r.Field<string>("AgentRegistered") == "T");

                ViewData["dtableCustForApprovalSelf"] = new DataTable();
                //ViewData["dtableCustForApprovalAgent"] = new DataTable();

                if (selfregistred.Any())
                    ViewData["dtableCustForApprovalSelf"] = selfregistred.CopyToDataTable();
                //if (agentregistred.Any())
                //    ViewData["dtableCustForApprovalAgent"] = agentregistred.CopyToDataTable();
                
                return View(userInfo);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        public ActionResult ViewSelfRegDetailWalletCustStatus(string id)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            string mobile = (string)Session["Mobile"];

            if (this.TempData["custmodify_messsage"] != null)
            {
                this.ViewData["custmodify_messsage"] = this.TempData["custmodify_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                CustomerSRInfo srInfo = new CustomerSRInfo();
                //DataSet DSet = ProfileUtils.GetSelfRegDetailDS(id);
                DataSet DSet = ProfileUtils.GetSelfRegDetailDSWalletCustStatus(id);
                
                DataTable dtableSrInfo = DSet.Tables["dtSrInfo"];

                if (dtableSrInfo != null && dtableSrInfo.Rows.Count > 0)
                {
                    srInfo.ClientCode = dtableSrInfo.Rows[0]["ClientCode"].ToString();

                    srInfo.UserName = dtableSrInfo.Rows[0]["UserName"].ToString();

                    srInfo.Name = srInfo.FName + " " + srInfo.MName + " " + srInfo.LName;
                    srInfo.FName = dtableSrInfo.Rows[0]["FName"].ToString();
                    srInfo.MName = dtableSrInfo.Rows[0]["MName"].ToString();
                    srInfo.LName = dtableSrInfo.Rows[0]["LName"].ToString();

                    srInfo.Gender = dtableSrInfo.Rows[0]["Gender"].ToString();
                    srInfo.DOB = dtableSrInfo.Rows[0]["DateOfBirth"].ToString().Split()[0];
                    srInfo.BSDateOfBirth = dtableSrInfo.Rows[0]["BSDateOfBirth"].ToString();

                    srInfo.Nationality = dtableSrInfo.Rows[0]["Nationality"].ToString();
                    srInfo.FatherName = dtableSrInfo.Rows[0]["FathersName"].ToString();
                    srInfo.MotherName = dtableSrInfo.Rows[0]["MothersName"].ToString();
                    srInfo.SpouseName = dtableSrInfo.Rows[0]["SpouseName"].ToString();
                    srInfo.MaritalStatus = dtableSrInfo.Rows[0]["MaritalStatus"].ToString();

                    srInfo.GrandFatherName = dtableSrInfo.Rows[0]["GFathersName"].ToString();
                    srInfo.FatherInlawName = dtableSrInfo.Rows[0]["FatherInlaw"].ToString();
                    srInfo.Occupation = dtableSrInfo.Rows[0]["Occupation"].ToString();

                    srInfo.EmailAddress = dtableSrInfo.Rows[0]["EmailAddress"].ToString();
                    srInfo.PanNo = dtableSrInfo.Rows[0]["PanNumber"].ToString();

                    srInfo.Country = dtableSrInfo.Rows[0]["CountryCode"].ToString();

                    srInfo.PProvince = dtableSrInfo.Rows[0]["PProvince"].ToString();
                    srInfo.PDistrict = dtableSrInfo.Rows[0]["PDistrict"].ToString();

                    srInfo.PVDC = dtableSrInfo.Rows[0]["PMunicipalityVDC"].ToString();
                    srInfo.PHouseNo = dtableSrInfo.Rows[0]["PHouseNo"].ToString();
                    srInfo.PWardNo = dtableSrInfo.Rows[0]["PWardNo"].ToString();
                    srInfo.PStreet = dtableSrInfo.Rows[0]["PStreet"].ToString();


                    srInfo.CProvince = dtableSrInfo.Rows[0]["CProvince"].ToString();
                    srInfo.CDistrict = dtableSrInfo.Rows[0]["CDistrict"].ToString();

                    srInfo.CVDC = dtableSrInfo.Rows[0]["CMunicipalityVDC"].ToString();
                    srInfo.CHouseNo = dtableSrInfo.Rows[0]["CHouseNo"].ToString();
                    srInfo.CWardNo = dtableSrInfo.Rows[0]["CWardNo"].ToString();
                    srInfo.CStreet = dtableSrInfo.Rows[0]["CStreet"].ToString();

                    srInfo.Citizenship = dtableSrInfo.Rows[0]["CitizenshipNo"].ToString();
                    srInfo.CitizenshipIssueDate = dtableSrInfo.Rows[0]["CitizenIssueDate"].ToString().Split()[0];
                    srInfo.BSCitizenshipIssueDate = dtableSrInfo.Rows[0]["BSCitizenIssueDate"].ToString();
                    srInfo.CitizenshipPlaceOfIssue = dtableSrInfo.Rows[0]["CitizenPlaceOfIssue"].ToString();

                    srInfo.License = dtableSrInfo.Rows[0]["LicenseNo"].ToString();

                    srInfo.LicensePlaceOfIssue = dtableSrInfo.Rows[0]["LicensePlaceOfIssue"].ToString();
                    srInfo.LicenseIssueDate = dtableSrInfo.Rows[0]["LicenseIssueDate"].ToString();
                    srInfo.BSLicenseIssueDate = dtableSrInfo.Rows[0]["BSLicenseIssueDate"].ToString();
                    srInfo.LicenseExpireDate = dtableSrInfo.Rows[0]["LicenseExpiryDate"].ToString();
                    srInfo.BSLicenseExpireDate = dtableSrInfo.Rows[0]["BSLicenseExpiryDate"].ToString();


                    srInfo.Passport = dtableSrInfo.Rows[0]["PassportNo"].ToString();

                    srInfo.PassportPlaceOfIssue = dtableSrInfo.Rows[0]["PassportPlaceOfIssue"].ToString();
                    srInfo.PassportIssueDate = dtableSrInfo.Rows[0]["PassportIssueDate"].ToString();
                    srInfo.PassportExpireDate = dtableSrInfo.Rows[0]["PassportExpiryDate"].ToString();


                    srInfo.Document = dtableSrInfo.Rows[0]["DocType"].ToString(); 

                    srInfo.BranchCode = "004";

                    srInfo.FrontImage = dtableSrInfo.Rows[0]["FrontImage"].ToString();
                    srInfo.BackImage = dtableSrInfo.Rows[0]["BackImage"].ToString();
                    srInfo.PassportImage = dtableSrInfo.Rows[0]["PassportImage"].ToString();

                    srInfo.CustStatus = dtableSrInfo.Rows[0]["CustStatus"].ToString();
                    ViewBag.CustStatus = srInfo.CustStatus;
                    ViewBag.DocType = srInfo.Document;
                    ViewBag.FrontImage = srInfo.FrontImage;
                    ViewBag.BackImage = srInfo.BackImage;
                    ViewBag.PassportImage = srInfo.PassportImage;

                    srInfo.DOB = DateConvert(srInfo.DOB);

                    srInfo.CitizenshipIssueDate = DateConvert(srInfo.CitizenshipIssueDate);
                    srInfo.LicenseIssueDate = DateConvert(srInfo.LicenseIssueDate);
                    srInfo.LicenseExpireDate = DateConvert(srInfo.LicenseExpireDate);

                    srInfo.PassportIssueDate = DateConvert(srInfo.PassportIssueDate);
                    srInfo.PassportExpireDate = DateConvert(srInfo.PassportExpireDate);
                }
                return View(srInfo);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
         
       
        public string DateConvert(string DOB)
        {
            if (DOB != "")
            {
                DateTime dt = Convert.ToDateTime(DOB);
        string x = dt.ToString("dd/MM/yyyy");
        DOB = x;
            }

            return (DOB);
         }

       
        #region Wallet Customer Status New
        [HttpGet]
        public ActionResult WalletCustStatusNew()
        {

            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;
            //Check Role link start    
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkRole(System.Reflection.MethodBase.GetCurrentMethod().Name, clientCode);
            //Check Role link end


            if (TempData["userType"] != null&& checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;
               CustReport para = new CustReport(); 
                return View(new RegCusDetailVM
                {
                    Parameter = para,
                    CustomerData = new List<CustomerData>()

                }); 
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
             public ContentResult WalletCustStatusNewTable(DataTableAjaxPostModel model, string MobileNumber,string CustomerName, string HasKYC,  string change, string ToExcel)
        {
            if (!Request.IsAjaxRequest())
            {
                Response.Write("Invalid Execution");
                return Content("");
            }

            int filteredResultsCount;
            int totalResultsCount;
            string ParaChanged = "T";
            CustReport ac = new CustReport();
            string convert; 
            if (Session["UserName"] == null)
            {

                convert = JsonConvert.SerializeObject(new
                {
                    draw = model.draw,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    error = "Session Expired, Please re-login."
                });

                return Content(convert, "application/json");

            }
       
            ac.MobileNo = MobileNumber;
            ac.CustomerName = CustomerName; 
            ac.HasKYC = HasKYC; 
            ac.UserName = (string)Session["UserName"];
            ParaChanged = change;
            var result = new List<CustomerData>();
            if (Session["CustomerData"] != null && ParaChanged == "F")
            {
                result = Session["CustomerData"] as List<CustomerData>;
            }
            else
            {
                ReportUserModel rep = new ReportUserModel();
                result = rep.CustomerDetails(ac);
                Session["CustomerData"] = result;
            }

            if (ToExcel == "T")
            {
                DataTable excel = ToDataTable<CustomerData>(result);
                //ExtraUtility.DataTableToInMemExcel(excel, "TEST.xls");
                string Handler = Guid.NewGuid().ToString();
                TempData[Handler] = excel;
                string FileName = DateTime.Now + "_CustomerData.xls";
                convert = JsonConvert.SerializeObject(new
                {
                    FileGuid = Handler,
                    FileName = FileName
                });
                return Content(convert, "application/json");
            }
            var res = FilterAndSort<CustomerData>(model, result, out totalResultsCount, out filteredResultsCount);
            var resultList = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();
                resultset.MobileNo = item.MobileNo;
                resultset.CustomerName = item.CustomerName;
                resultset.CreatedBy = item.CreatedBy;
                resultset.HasKYC = item.HasKYC;
                resultset.ClientCode = item.ClientCode; 
                resultList.Add(resultset);
            }
            convert = JsonConvert.SerializeObject(new
            {
                draw = model.draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = resultList
            });
            return Content(convert, "application/json");

        }
        #endregion
        public List<T> FilterAndSort<T>(DataTableAjaxPostModel model, List<T> source, out int TotalCount, out int Filtered)
        {

            int skip = model.start;
            int take = model.length;

            string sortBy = "";
            bool sortDir = true;
            var filter = source.AsQueryable();
            Func<T, Object> orderByFunc = null;
            if (model.order != null)
            {
                sortBy = model.columns[model.order[0].column].data;
                sortDir = model.order[0].dir.ToLower() == "asc";
                orderByFunc = p => p.GetType().GetProperty(sortBy).GetValue(p, null);
            }
            if (orderByFunc != null)
            {
                if (sortDir)
                    filter = filter.OrderBy(orderByFunc).AsQueryable();
                else
                    filter = filter.OrderByDescending(orderByFunc).AsQueryable();
            }
            TotalCount = source.Count;
            Filtered = filter.Count();
            var res = filter.Skip(skip).Take(take).ToList();

            return res;
        }

        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable();

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            foreach (var column in dataTable.Columns.Cast<DataColumn>().ToArray())
            {
                if (dataTable.AsEnumerable().All(dr => dr.IsNull(column)))
                    dataTable.Columns.Remove(column);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }
         
    }
}