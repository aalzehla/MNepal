using KellermanSoftware.CompareNetObjects;
using MNepalWeb.Helper;
using MNepalWeb.Models;
using MNepalWeb.Settings;
using MNepalWeb.UserModels;
using MNepalWeb.Utilities;
using MNepalWeb.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.SessionState;
using System.Web.UI.WebControls;

namespace MNepalWeb.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class AdminController : Controller
    {
        #region "Admin Registration"

        // GET: Admin/Registration
        public ActionResult Registration()
        {
           
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            string bankCode = (string)Session["BankCode"];
            //Check Role link start
            bool checkRole = false;
            string methodlink = System.Reflection.MethodBase.GetCurrentMethod().Name;
            RoleChecker roleChecker = new RoleChecker();
            checkRole = roleChecker.checkRole(methodlink, clientCode);
            //Check Role link end
            TempData["userType"] = userType;

            if (TempData["userType"] != null&& checkRole==true)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                if (this.TempData["registration_message"] != null)
                {
                    this.ViewData["registration_message"] = this.TempData["registration_message"];
                    this.ViewData["message_class"] = this.TempData["message_class"];
                }

                ViewBag.User = "admin";

                //For Branch
                List<SelectListItem> item = new List<SelectListItem>();
                DataSet dsBank = BranchUtils.GetDataSetPopulateBranchName(bankCode);
                DataTable bank = dsBank.Tables[0];
                foreach (DataRow dr in bank.Rows)
                {
                    if (dr["IsBlocked"].ToString() == "F" && dr["BankCode"].ToString() == "0004")
                    {
                        item.Add(new SelectListItem
                        {
                            Text = dr["BranchName"].ToString(),
                            Value = dr["BranchCode"].ToString()
                        });
                    }
                }
                ViewBag.BranchName = item.OrderBy(x => x.Text);
                //For ProfileName
                List<SelectListItem> itemProfile = new List<SelectListItem>();
                DataSet dsProfile = UserProfileUtils.GetDataSetPopulateProfileName();

                DataRow[] rows = dsProfile.Tables[0].Select(string.Format("ProfileStatus='Active'"));
                ViewBag.profile = rows;//dsProfile.Tables[0];

                foreach (DataRow dr in rows)
                {
                    itemProfile.Add(new SelectListItem
                    {
                        Text = dr["ProfileName"].ToString(),
                        Value = dr["ProfileCode"].ToString()
                    });
                }
                ViewBag.ProfileName = itemProfile.OrderBy(x => x.Text);

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

                /*
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
                ViewBag.WalletNumber = bankCode + t + unique;*/

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        public ActionResult GetPopulateBankCode(string bankCode)
        {
            if ((bankCode != "") || (bankCode != null))
            {
                DataSet dsBank = BankUtils.GetDataSetPopulateBranchCode(bankCode);
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
            {
                return this.View();
            }

        }

        public string RandomString(int noOfCharacter)
        {
            string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random r = new Random();
            string randonpassword = new string(
                Enumerable.Repeat(characters, noOfCharacter)
                    .Select(s => s[r.Next(s.Length)])
                    .ToArray());
            return randonpassword;
        }


        // POST: Admin/Registration
        [HttpPost]
        public ActionResult Registration(FormCollection collection)
        {
            try
            {
                string userName = (string)Session["LOGGED_USERNAME"];
                string clientCode = (string)Session["LOGGEDUSER_ID"];
                string name = (string)Session["LOGGEDUSER_NAME"];
                string userType = (string)Session["LOGGED_USERTYPE"];
                string bankCode = (string)Session["BankCode"];

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

                    userInfo.UserGroup = collection["ProfileName"].ToString();

                    userInfo.PushSMS = collection["txtPushSMS"].ToString();
                    userInfo.BranchCode = collection["BranchName"].ToString();

                    string FName = collection["txtFirstName"].ToString().Capitalize();

                    userInfo.Name = FName;

                    userInfo.Address = string.Empty;

                    userInfo.PIN = string.Empty;
                    userInfo.Status = "Active";
                    userInfo.IsApproved = "Approve";
                    userInfo.IsRejected = "F";

                    userInfo.ContactNumber1 = string.Empty;
                    userInfo.ContactNumber2 = string.Empty;
                    userInfo.EmailAddress = collection["txtEmail"] ?? string.Empty;
                    userInfo.COC = collection["txtCOC"] ?? "F";
                    //userInfo.COC = "F";
                    userInfo.ClientCode = collection["txtCClientCode"];
                    userInfo.CreatedBy = userName;

                    //if (string.IsNullOrEmpty(collection["txtUserType"]))
                    //{
                    //    ModelState.AddModelError("UserType", "*Please enter UserType");
                    //}
                    if (string.IsNullOrEmpty(collection["txtUserName"]))
                    {
                        ModelState.AddModelError("UserName", "*Please enter UserName");
                    }
                    if (string.IsNullOrEmpty(collection["ProfileName"]))
                    {
                        ModelState.AddModelError("ProfileName", "*Please enter ProfileName");
                    }
                    if (string.IsNullOrEmpty(collection["txtFirstName"]))
                    {
                        ModelState.AddModelError("FirstName", "*Please enter First Name");
                    }
                    if (string.IsNullOrEmpty(collection["txtPushSMS"]))
                    {
                        ModelState.AddModelError("PushSMS", "*Please enter PushSMS");
                    }
                    if (string.IsNullOrEmpty(collection["BranchName"]))
                    {
                        ModelState.AddModelError("BranchName", "*Please enter BranchName");
                    }
                    List<SelectListItem> item = new List<SelectListItem>();
                    DataSet dsBank = BranchUtils.GetDataSetPopulateBranchName(bankCode);
                    ViewBag.bank = dsBank.Tables[0];
                    foreach (DataRow dr in ViewBag.bank.Rows)
                    {
                        if (dr["IsBlocked"].ToString() == "F")
                        {
                            item.Add(new SelectListItem
                            {
                                Text = dr["BranchName"].ToString(),
                                Value = dr["BranchCode"].ToString()
                            });
                        }
                    }
                    ViewBag.BranchName = item.OrderBy(x => x.Text); ;



                    //For ProfileName
                    List<SelectListItem> itemProfile = new List<SelectListItem>();
                                      
                    DataSet dsProfile = UserProfileUtils.GetDataSetPopulateProfileName();
                    DataRow[] rows = dsProfile.Tables[0].Select(string.Format("ProfileStatus='Active'"));
                    
                    foreach (DataRow dr in rows)
                    {
                        itemProfile.Add(new SelectListItem
                        {
                            Text = @dr["ProfileName"].ToString(),
                            Value = @dr["ProfileName"].ToString()
                        });
                    }
                    ViewBag.ProfileName = itemProfile.OrderBy(x => x.Text);

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
                        //DataTable dtableMobileNo = RegisterUtils.GetCheckMobileNo(userInfo.ContactNumber1);
                        //if (dtableMobileNo.Rows.Count == 0)
                        //{
                        // DataTable dtCheckUserName = RegisterUtils.GetCheckUserName(userInfo.UserName);
                      DataTable dtCheckUserName = RegisterUtils.GetCheckAdminUserName(userInfo.UserName);
                        if (dtCheckUserName.Rows.Count == 0)
                        {
                            if (collection.AllKeys.Any())
                            {
                                try
                                {
                                    int results = RegisterUtils.RegisterAdminInfo(userInfo, bankCode);
                                    if (results > 0)
                                    {
                                        result = true;

                                       
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
                        else if (dtCheckUserName.Rows.Count > 0)
                        {
                            result = false;
                            errorMessage = "Already registerd username";
                        }

                        //}
                        //else if (dtableMobileNo != null && dtableMobileNo.Rows.Count > 0)
                        //{
                        //    string checkMobileNo = dtableMobileNo.Rows[0]["ContactNumber1"].ToString();
                        //    result = false;
                        //    errorMessage = "Already Register username";
                        //}
                    }
                    else
                    {
                        result = false;
                    }

                    this.TempData["registration_message"] = result
                                                  ? "Registration information is successfully added. Please Check Approve Registration Admin and perform accordingly"
                                                  : "Error while inserting the information. ERROR :: "
                                                    + errorMessage;
                    this.TempData["message_class"] = result ? "success_info" : "failed_info";

                    ////For Bank
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
                return RedirectToAction("Registration");
            }

            ////For Branch
            //List<SelectListItem> item = new List<SelectListItem>();
            //DataSet dsBank = BranchUtils.GetDataSetPopulateBranchName();
            //ViewBag.bank = dsBank.Tables[0];
            //foreach (DataRow dr in ViewBag.bank.Rows)
            //{
            //    item.Add(new SelectListItem
            //    {
            //        Text = @dr["BranchName"].ToString(),
            //        Value = @dr["BranchName"].ToString()
            //    });
            //}
            //ViewBag.BranchName = item;

            ////For ProfileName
            //List<SelectListItem> itemProfile = new List<SelectListItem>();
            //DataSet dsProfile = UserProfileUtils.GetDataSetPopulateProfileName();
            //ViewBag.profile = dsProfile.Tables[0];
            //foreach (DataRow dr in ViewBag.profile.Rows)
            //{
            //    itemProfile.Add(new SelectListItem
            //    {
            //        Text = @dr["UserProfileName"].ToString(),
            //        Value = @dr["UPID"].ToString()
            //    });
            //}
            //ViewBag.ProfileName = itemProfile;
        }


        #endregion


        #region "Admin View Details"

        // GET: Admin
        public ActionResult Index()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            //Check Role link start
            bool checkRole = false;
            string methodlink = System.Reflection.MethodBase.GetCurrentMethod().Name;
            RoleChecker roleChecker = new RoleChecker();
            checkRole = roleChecker.checkIndexRole(methodlink, clientCode, this.ControllerContext.RouteData.Values["controller"].ToString());
            //Check Role link end
            TempData["userType"] = userType;

            if (TempData["userType"] != null&& checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;



                //DataSet dsBank = BranchUtils.GetDataSetPopulateBranchName();
                //DataTable bank = dsBank.Tables[0];
                //DataSet dsProfile = UserProfileUtils.GetDataSetPopulateProfileName();
                //DataTable profile = dsProfile.Tables[0];

                List<UserInfo> registrationtList = new List<UserInfo>();
                DataTable dtblRegistration = ProfileUtils.GetAdminProfile();

                foreach (DataRow row in dtblRegistration.Rows)
                {
                    UserInfo regobj = new UserInfo();
                    regobj.ClientCode = row["ClientCode"].ToString();
                    regobj.Name = row["Name"].ToString();
                    regobj.Address = row["Address"].ToString();
                    regobj.PIN = row["PIN"].ToString();
                    regobj.Status = row["Status"].ToString();
                    regobj.ContactNumber1 = row["ContactNumber1"].ToString();
                    regobj.ContactNumber2 = row["ContactNumber2"].ToString();
                    regobj.EmailAddress = row["EmailAddress"].ToString();
                    regobj.UserName = row["UserName"].ToString();
                    regobj.UserType = row["userType"].ToString();
                    regobj.UserBranchCode = row["UserBranchCode"].ToString();
                    regobj.UserBranchName = row["UserBranchName"].ToString();
                    regobj.CreatedBy = row["CreatedBy"].ToString();
                    regobj.EmailAddress = row["EmailAddress"].ToString();
                    regobj.ProfileName = row["ProfileName"].ToString();
                    regobj.AProfileName = row["AProfileName"].ToString();
                    registrationtList.Add(regobj);
                }
                return View(registrationtList);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }


        // GET: Admin/EditRegistration
        [HttpGet]
        public ActionResult EditRegistration(string clientCodeId)
        {
          
            if (string.IsNullOrEmpty(clientCodeId))
            {
                return RedirectToAction("Index", "Admin");
            }
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            string bankCode = (string)Session["BankCode"];

            TempData["userType"] = userType;

            if (this.TempData["register_messsage"] != null)
            {
                this.ViewData["editregister_messsage"] = this.TempData["editregister_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                //For Branch
                List<SelectListItem> item = new List<SelectListItem>();
                DataSet dsBank = BranchUtils.GetDataSetPopulateBranchName(bankCode);
                ViewBag.bank = dsBank.Tables[0];
                foreach (DataRow dr in ViewBag.bank.Rows)
                {
                    if (dr["IsBlocked"].ToString() == "F" && dr["BankCode"].ToString() == "0004")
                    {
                        item.Add(new SelectListItem
                        {
                            Text = dr["BranchName"].ToString(),
                            Value = dr["BranchCode"].ToString()
                        });
                    }
                }
                //ViewBag.BranchName = item;

                //For ProfileName
                List<SelectListItem> itemProfile = new List<SelectListItem>();
                DataSet dsProfile = UserProfileUtils.GetDataSetPopulateProfileName();
                DataRow[] rows = dsProfile.Tables[0].Select(string.Format("ProfileStatus='Active'"));
                ViewBag.profile = rows;//dsProfile.Tables[0];
                foreach (DataRow dr in rows)
                {
                    itemProfile.Add(new SelectListItem
                    {
                        Text = @dr["ProfileName"].ToString(),
                        Value = @dr["ProfileCode"].ToString()
                    });
                }

              
                UserInfo userInfo = new UserInfo();
                DataTable dtblRegistration = ProfileUtils.GetUserProfileInfo(clientCodeId);
                if (dtblRegistration.Rows.Count == 1)
                {
                    userInfo.ClientCode = dtblRegistration.Rows[0]["ClientCode"].ToString();
                    userInfo.Name = dtblRegistration.Rows[0]["Name"].ToString().Capitalize();
                    //userInfo.Address = dtblRegistration.Rows[0]["Address"].ToString();
                    userInfo.Status = dtblRegistration.Rows[0]["Status"].ToString();
                    //userInfo.ContactNumber1 = dtblRegistration.Rows[0]["ContactNumber1"].ToString();
                    //userInfo.ContactNumber2 = dtblRegistration.Rows[0]["ContactNumber2"].ToString();
                    userInfo.EmailAddress = dtblRegistration.Rows[0]["EmailAddress"].ToString();
                    userInfo.ProfileName = dtblRegistration.Rows[0]["ProfileName"].ToString();
                    userInfo.UserName = dtblRegistration.Rows[0]["UserName"].ToString();
                    userInfo.UserType = dtblRegistration.Rows[0]["UserType"].ToString();
                    //userInfo.IsApproved = dtblRegistration.Rows[0]["IsApproved"].ToString();
                    //userInfo.IsRejected = dtblRegistration.Rows[0]["IsRejected"].ToString();
                    //userInfo.WalletNumber = dtblRegistration.Rows[0]["WalletNumber"].ToString();
                    //userInfo.BankNo = dtblRegistration.Rows[0]["BankNo"].ToString();
                    userInfo.BranchCode = dtblRegistration.Rows[0]["UserBranchCode"].ToString();
                    //userInfo.BankAccountNumber = dtblRegistration.Rows[0]["BankAccountNumber"].ToString();
                    userInfo.COC = dtblRegistration.Rows[0]["COC"].ToString();
                    userInfo.IsRejected = dtblRegistration.Rows[0]["IsRejected"].ToString();
                    userInfo.Remarks = dtblRegistration.Rows[0]["Remarks"].ToString();
                    //userInfo.AProfileName = dtblRegistration.Rows[0]["AProfileName"].ToString();

                    if (userInfo.UserType!="admin")
                    {
                        this.TempData["editregister_messsage"] = "No such Admin exists";
                        this.TempData["message_class"] = CssSetting.FailedMessageClass;
                        return RedirectToAction("Index", "Admin");
                    }
                }
                else
                {
                    this.TempData["editregister_messsage"] = "No such Admin exists";
                    this.TempData["message_class"] = CssSetting.FailedMessageClass;
                }
                ViewBag.AdminProfile = new SelectList(itemProfile, "Value", "Text", userInfo.ProfileName).OrderBy(x => x.Text);
                ViewBag.BranchName = new SelectList(item, "Value", "Text", userInfo.BranchCode).OrderBy(x => x.Text);//,selectedValue:userInfo.BranchCode

                return View(userInfo);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        // POST: Admin/EditRegistration/1
        [HttpPost]
        public ActionResult EditRegistration(string btnCommand, FormCollection collection)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            string bankCode = (string)Session["BankCode"];

            TempData["userType"] = userType;

            string displayMessage = null;
            string messageClass = null;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;
                bool isUpdated = false;
                UserInfo userInfoModify = new UserInfo();
                try
                {
                    if (btnCommand == "Submit")
                    {
                        string cClientCode = collection["ClientCode"].ToString();
                        string cUserName = collection["Username"].ToString();
                        string cName = collection["Name"].ToString();
                        string cProfileName = collection["ProfileName"].ToString();
                        string cEmailAddress = collection["EmailAddress"].ToString();
                        string cBranchCode = collection["BranchCode"].ToString();
                        string cCOC = "F";

                        string cStatus = collection["Status"].ToString();
                        if (collection.AllKeys.Contains("COC"))
                        {
                            cCOC = collection["COC"].ToString();
                        }
                        /*New Data*/
                        userInfoModify.ClientCode = cClientCode;//
                        userInfoModify.UserName = cUserName;//
                        userInfoModify.Name = cName.Capitalize();//
                        userInfoModify.ProfileName = cProfileName;//
                        userInfoModify.EmailAddress = cEmailAddress;//
                        userInfoModify.BranchCode = cBranchCode;
                        userInfoModify.COC = cCOC; // 
                        userInfoModify.Status = cStatus;//
                        //userInfoModify.AdminBranch = (string)Session["UserBranch"]; //modifying admin branch
                        //userInfoModify.AdminUserName = (string)Session["UserName"]; //modifying admin username

                        /*map new data to MNUser*/
                        MNUser userdataNew = new MNUser();
                        MNUser cust = new MNUser();
                        MNClient mnclientnew = new MNClient();
                        mnclientnew.ClientCode = userInfoModify.ClientCode;
                        mnclientnew.Name = userInfoModify.Name;
                        mnclientnew.Status = userInfoModify.Status;
                        mnclientnew.BranchCode = userInfoModify.BranchCode;

                        MNClientContact mnclientcontactnew = new MNClientContact();
                        mnclientcontactnew.EmailAddress = userInfoModify.EmailAddress;

                        MNClientExt mnclientextnew = new MNClientExt();
                        mnclientextnew.ProfileName = userInfoModify.ProfileName;
                        mnclientextnew.COC = userInfoModify.COC;
                        mnclientextnew.UserName = userInfoModify.UserName;

                        userdataNew.MNClient = mnclientnew;
                        userdataNew.MNClientContact = mnclientcontactnew;
                        userdataNew.MNClientExt = mnclientextnew;
                        
                        /*old data*/
                        UserInfo userInfo = new UserInfo();
                        MNUser userdataOld = new MNUser();
                        DataTable dtblRegistration = ProfileUtils.GetUserProfileInfo(cClientCode);
                        if (dtblRegistration.Rows.Count == 1)
                        {
                            userInfo.ClientCode = dtblRegistration.Rows[0]["ClientCode"].ToString();
                            userInfo.Name = dtblRegistration.Rows[0]["Name"].ToString().Capitalize();
                            userInfo.Status = dtblRegistration.Rows[0]["Status"].ToString();
                            userInfo.EmailAddress = dtblRegistration.Rows[0]["EmailAddress"].ToString();
                            userInfo.ProfileName = dtblRegistration.Rows[0]["ProfileName"].ToString();
                            userInfo.UserName = dtblRegistration.Rows[0]["UserName"].ToString();
                            userInfo.BranchCode = dtblRegistration.Rows[0]["UserBranchCode"].ToString();
                            userInfo.COC = dtblRegistration.Rows[0]["COC"].ToString();

                            MNClient mnclientold = new MNClient();
                            mnclientold.ClientCode = userInfo.ClientCode;
                            mnclientold.Name = userInfo.Name;
                            mnclientold.Status = userInfo.Status;
                            mnclientold.BranchCode = userInfo.BranchCode;

                            MNClientContact mnclientcontactold = new MNClientContact();
                            mnclientcontactold.EmailAddress = userInfo.EmailAddress;

                            MNClientExt mnclientextold = new MNClientExt();
                            mnclientextold.ProfileName = userInfo.ProfileName;
                            mnclientextold.COC = userInfo.COC;
                            mnclientextold.UserName = userInfo.UserName;

                            userdataOld.MNClient = mnclientold;
                            userdataOld.MNClientContact = mnclientcontactold;
                            userdataOld.MNClientExt = mnclientextold;

                            /*map old data to MNUser*/
                        }
                                               

                        List<SelectListItem> item = new List<SelectListItem>();
                        DataSet dsBank = BranchUtils.GetDataSetPopulateBranchName(bankCode);
                        ViewBag.bank = dsBank.Tables[0];
                        foreach (DataRow dr in ViewBag.bank.Rows)
                        {
                            item.Add(new SelectListItem
                            {
                                Text = @dr["BranchName"].ToString(),
                                Value = @dr["BranchCode"].ToString()
                            });
                        }

                        List<SelectListItem> itemProfile = new List<SelectListItem>();
                        DataSet dsProfile = UserProfileUtils.GetDataSetPopulateProfileName();

                        DataRow[] rows = dsProfile.Tables[0].Select(string.Format("ProfileStatus='Active'"));

                        foreach (DataRow dr in rows)
                        {
                            itemProfile.Add(new SelectListItem
                            {
                                Text = @dr["ProfileName"].ToString(),
                                Value = @dr["ProfileCode"].ToString()
                            });
                        }

                        ViewBag.AdminProfile = new SelectList(itemProfile, "Value", "Text", userInfoModify.ProfileName).OrderBy(x => x.Text);
                        ViewBag.BranchName = new SelectList(item, "Value", "Text", userInfoModify.BranchCode).OrderBy(x => x.Text);

                        /*Difference compare*/
                        CompareLogic compareLogic = new CompareLogic();
                        ComparisonConfig config = new ComparisonConfig();
                        config.MaxDifferences = int.MaxValue;
                        config.IgnoreCollectionOrder = false;
                        compareLogic.Config = config;
                        ComparisonResult result = compareLogic.Compare(userdataOld, userdataNew); //  firstparameter orginal,second parameter modified
                        List<MNMakerChecker> makerCheckers = new List<MNMakerChecker>();
                        if (!result.AreEqual)
                        {
                            isUpdated = true;
                            foreach (Difference diff in result.Differences)
                            {
                                MNMakerChecker makerchecker = new MNMakerChecker();
                                int index = diff.PropertyName.IndexOf('.');
                                makerchecker.ColumnName = diff.PropertyName.Substring(index + 1);
                                //makerchecker.Code = oldCust.MNClient.ClientCode;
                                makerchecker.TableName = diff.ParentPropertyName;
                                makerchecker.OldValue = diff.Object1Value;
                                makerchecker.NewValue = diff.Object2Value;
                                makerchecker.Module = "ADMIN";
                                makerCheckers.Add(makerchecker);
                            }
                        }

                        if (isUpdated)
                        {
                            //call Procedure here

                            // parameters 
                            /*
                            ClientCode customerClientCode varchar(8)
                            @ModifyingUser Modifying Admin varchar(50)
                            @ModifyingBranch Modifying AdminBranchCode varchar(3)
                            @ModifiedFields XML, XML string of changed fields 
                            @TxnAccountsToDelete XML  XML string of TXNAcc to delete 
                            @TxnAccountsToAdd XML XML string of TXNAcc to Add,
                            */
                            //makerchecker.BranchCode = (string)Session["UserBranch"];
                            //makerchecker.EditedBy= (string)Session["UserName"];
                            //makerchecker.EditedOn = DateTime.Now;

                            string modifyingAdmin = (string)Session["UserName"];
                            string modifyingBranch = (string)Session["UserBranch"];
                            CusProfileUtils cUtil = new CusProfileUtils();
                            string ModifiedFieldXML = cUtil.GetMakerCheckerXMLStr(makerCheckers);
                            bool inserted = CustomerUtils.InsertMakerChecker(cClientCode, modifyingAdmin, modifyingBranch, ModifiedFieldXML);

                            displayMessage = inserted
                                                     ? "Changes will take effect after approval"
                                                     : "Error updating customer, Please try again";
                            messageClass = inserted ? CssSetting.SuccessMessageClass : CssSetting.FailedMessageClass;
                        }
                        else
                        {
                            displayMessage = "Nothing Changed";
                            messageClass = CssSetting.SuccessMessageClass;
                        }


                        //if ((cProfileName != "") && (cEmailAddress != ""))
                        //{

                        //    isUpdated = CustomerUtils.UpdateAdminInfo(userInfoModify);
                        //    displayMessage = isUpdated
                        //                        ? "The Admin User Information has successfully been updated.Please Check Approve Modified Admin and perform accordingly"
                        //                        : "Error while updating Member information";
                        //    messageClass = isUpdated ? CssSetting.SuccessMessageClass : CssSetting.FailedMessageClass;
                        //}
                        //else
                        //{
                        //    displayMessage = "Required Field is Empty";
                        //    messageClass = CssSetting.FailedMessageClass;
                        //}
                    }
                }
                catch (Exception ex)
                {
                    displayMessage = ex.Message;
                    messageClass = CssSetting.FailedMessageClass;
                }

                this.TempData["editregister_messsage"] = displayMessage;
                this.TempData["message_class"] = messageClass;
                //if (isUpdated)
                    return RedirectToAction("Index", "Admin");
                //else
                //    return View(userInfoModify);
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
            string bankCode = (string)Session["BankCode"];

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
                DataSet dsBank = BranchUtils.GetDataSetPopulateBranchName(bankCode);
                DataTable bank = dsBank.Tables[0];
                DataSet dsProfile = UserProfileUtils.GetDataSetPopulateProfileName();
                DataTable profile = dsProfile.Tables[0];
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
                    userInfo.ProfileName = dtblRegistration.Rows[0]["ProfileName"].ToString();
                    userInfo.AProfileName = dtblRegistration.Rows[0]["AProfileName"].ToString();
                    DataRow[] rows = bank.Select(string.Format("BranchCode='{0}'", dtblRegistration.Rows[0]["UserBranchCode"]));

                  

                    if (rows.Count() > 0)
                    {
                        userInfo.BranchName = rows[0]["BranchName"].ToString();
                    }
                    else
                    {
                        userInfo.BranchName = "";
                    }
                   



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


        #region "Admin Password Reset"

        // GET: Admin/PasswordReset
        [HttpGet]
        public ActionResult PasswordResetList()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            //Check Role link start
            bool checkRole = false;
            string methodlink = System.Reflection.MethodBase.GetCurrentMethod().Name;
            RoleChecker roleChecker = new RoleChecker();
            checkRole = roleChecker.checkRole(methodlink, clientCode);
            //Check Role link end
            TempData["userType"] = userType;

            if (this.TempData["password_messsage"] != null)
            {
                this.ViewData["password_messsage"] = this.TempData["password_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            if (TempData["userType"] != null && checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                List<UserInfo> RegistrationtList = new List<UserInfo>();
                DataTable dtblRegistration = ProfileUtils.GetAdminProfile();
                
                
                foreach (DataRow row in dtblRegistration.Rows)
                {
                    UserInfo regobj = new UserInfo();
                    regobj.ClientCode = row["ClientCode"].ToString();
                    regobj.Name = row["Name"].ToString();
                    regobj.Address = row["Address"].ToString();
                    regobj.PIN = row["PIN"].ToString();
                    regobj.Status = row["Status"].ToString();
                    regobj.ContactNumber1 = row["ContactNumber1"].ToString();
                    regobj.ContactNumber2 = row["ContactNumber2"].ToString();
                    regobj.UserName = row["UserName"].ToString();
                    regobj.UserType = row["userType"].ToString();
                    regobj.UserBranchCode = row["UserBranchCode"].ToString();
                    regobj.UserBranchName = row["UserBranchName"].ToString();
                    regobj.ProfileName = row["ProfileName"].ToString();
                    regobj.AProfileName = row["AProfileName"].ToString();

                    RegistrationtList.Add(regobj);
                }
               
                return View(RegistrationtList.OrderBy(x => x.Name));
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        // GET: Admin/PasswordReset
        [HttpGet]
        public ActionResult PasswordReset(string clientCodeID)
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

                ViewBag.ClientCode = clientCodeID;
                UserInfo info = new UserInfo();
                string Name = string.Empty;
                string ContactNumber1 = string.Empty;
                string UserName = string.Empty;
                string email = string.Empty;
                DataTable dtblRegistration = ProfileUtils.GetUserProfileInfo(clientCodeID);
                if (dtblRegistration.Rows.Count == 1)
                {
                    Name = dtblRegistration.Rows[0]["Name"].ToString();
                    ContactNumber1 = dtblRegistration.Rows[0]["ContactNumber1"].ToString();
                    UserName = dtblRegistration.Rows[0]["UserName"].ToString();
                    email = dtblRegistration.Rows[0]["EmailAddress"].ToString();
                    info.Name = Name;
                    info.ContactNumber1 = ContactNumber1;
                    info.UserName = UserName;
                    info.EmailAddress = email;
                    info.ClientCode = clientCodeID;

                }
                //ViewBag.Name = Name;
                //ViewBag.ContactNumber1 = ContactNumber1;
                //ViewBag.UserName = UserName;
                //ViewBag.Email = email;

                //List<UserInfo> RegistrationtList = new List<UserInfo>();
                //DataTable dtblRegistration = ProfileUtils.GetAdminProfile();

                //foreach (DataRow row in dtblRegistration.Rows)
                //{
                //    UserInfo regobj = new UserInfo();
                //    regobj.ClientCode = row["ClientCode"].ToString();
                //    regobj.Name = row["Name"].ToString();
                //    regobj.Address = row["Address"].ToString();
                //    regobj.PIN = row["PIN"].ToString();
                //    regobj.Status = row["Status"].ToString();
                //    regobj.ContactNumber1 = row["ContactNumber1"].ToString();
                //    regobj.ContactNumber2 = row["ContactNumber2"].ToString();
                //    regobj.UserName = row["UserName"].ToString();
                //    regobj.UserType = row["userType"].ToString();

                //    RegistrationtList.Add(regobj);
                //}
                //return View(RegistrationtList);
                return View(info);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        //TEST

        [HttpPost]
        public ActionResult PassReset(string btnCommand, FormCollection collection)
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
                    string cClientCode = collection["txtClientCode"].ToString();
                    //string oldPassword = collection["Password"].ToString();
                    //string newPin = collection["txtNewPin"].ToString();
                    string Email = collection["txtEmail"].ToString();
                    string AdminName = collection["txtName"].ToString();
                    string UserName = collection["txtUserName"].ToString();


                    UserInfo userInfo = new UserInfo();
                    userInfo.ClientCode = cClientCode;
                    userInfo.EmailAddress = Email;
                    userInfo.Name = AdminName;
                    userInfo.UserName = UserName;
                    userInfo.AdminBranch = (string)Session["UserBranch"];
                    userInfo.AdminUserName = (string)Session["UserName"];


                    string Status = "APR";

                    bool isUpdated = PinUtils.UpdateUserPassword(userInfo, Status);



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
                                             ? "Password reset request successful for " + userInfo.UserName + ". Changes will take effect after approval."
                                             : "Error while updating";
                    messageClass = isUpdated ? CssSetting.SuccessMessageClass : CssSetting.FailedMessageClass;
                    if (isUpdated)
                    {
                        this.TempData["password_messsage"] = displayMessage;
                        this.TempData["message_class"] = messageClass;
                        return RedirectToAction("PasswordResetList", "Admin");
                    }
                    else
                    {
                        displayMessage = "Unable to reset the password. Please retry.";
                        messageClass = CssSetting.FailedMessageClass;
                    }

                    this.TempData["password_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;
                    return RedirectToAction("PasswordReset");
                }
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        // TEST
        // POST: Admin/PasswordReset/1
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
                string password = CustomerUtils.GeneratePassword();//= collection["txtEmail"].ToString();
                string Email = collection["txtEmail"].ToString();
                string AdminName = collection["txtName"].ToString();
                string UserName = collection["txtUserName"].ToString();
                UserInfo userInfo = new UserInfo();
                userInfo.Password = password;
                userInfo.ClientCode = txtClientCode;
                userInfo.Password = password;
                userInfo.EmailAddress = Email;
                userInfo.Name = AdminName;
                userInfo.UserName = UserName;

                string displayMessage = null;
                string messageClass = null;

                try
                {
                    if ((password != "") && (txtClientCode != ""))
                    {
                        bool isUpdated = PasswordUtils.UpdateAdminPasswordInfo(userInfo);
                        if (isUpdated)
                        {

                            string Subject = "Password Reset";
                            string MailSubject = "<span style='font-size:15px;'><h4>Dear " + userInfo.Name + ",</h4>";
                            MailSubject += "Your password reset request for user <b>" + userInfo.UserName + "</b> has been acknowledged";
                            MailSubject += "and you have been issued with a new temporary password.<br/>";
                            MailSubject += "<br/><b>Password: " + userInfo.Password + "</b> <br/>";
                            MailSubject += "                    (Please change your password after login.)<br/>";
                            MailSubject += "<br/>Thank You <br/>";
                            MailSubject += "<br/>Nepal Investment Bank Ltd. </span><br/>";
                            MailSubject += @"<br/>Note: This is confidential mail, Please do not share with other. Your credential purpose is for NIBL Administration Console only.<br/>";
                            MailSubject += "<hr/>";
                            try
                                {
                            EMailUtil SendMail = new EMailUtil();
                            SendMail.SendMail(userInfo.EmailAddress, Subject, MailSubject);
                                }
                            catch (Exception ex)
                                {
                                    throw new Exception("Admin account has been approved but system is unable to send email. Please Contact Administrator");
                                }

                        }
                        displayMessage = isUpdated
                                                 ? "Password Information has successfully been updated."
                                                 : "Error while updating Password information";
                        messageClass = isUpdated ? CssSetting.SuccessMessageClass : CssSetting.FailedMessageClass;
                    }
                    else
                    {
                        displayMessage = "Unable to perform the requested action.Please refresh the page and try again.";
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


        #region "Admin Approve"

        // GET: Admin/ApproveList
        public ActionResult ApproveList()
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

                List<UserInfo> RegistrationtList = new List<UserInfo>();
                DataTable dtblRegistration = ProfileUtils.GetUnApproveAdminProfile();

                foreach (DataRow row in dtblRegistration.Rows)
                {
                    UserInfo regobj = new UserInfo();
                    regobj.ClientCode = row["ClientCode"].ToString();
                    regobj.Name = row["Name"].ToString();
                    regobj.Address = row["Address"].ToString();
                    regobj.PIN = row["PIN"].ToString();
                    regobj.Status = row["Status"].ToString();
                    regobj.ContactNumber1 = row["ContactNumber1"].ToString();
                    regobj.ContactNumber2 = row["ContactNumber2"].ToString();
                    regobj.EmailAddress = row["EmailAddress"].ToString();
                    regobj.UserName = row["UserName"].ToString();
                    regobj.UserType = row["userType"].ToString();

                    RegistrationtList.Add(regobj);
                    ViewData["dtableApproveList"] = row;
                }
                return View(RegistrationtList);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }

        #endregion


        #region "Admin Approve Modify"

        // GET: Admin/ApproveModifyList
        public ActionResult ApproveModifyList()
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
                DataTable dtblRegistration = ProfileUtils.GetAdminProfile();

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
                    ViewData["dtableApproveList"] = row;
                }
                return View(registrationtList);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }

        // GET: Admin/ApproveModify
        [HttpGet]
        public ActionResult ApproveModify(string clientCodeId)
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
                DataTable dtblRegistration = ProfileUtils.GetUserProfileInfo(clientCodeId);
                if (dtblRegistration.Rows.Count == 1)
                {
                    userInfo.ClientCode = dtblRegistration.Rows[0]["ClientCode"].ToString();
                    userInfo.Name = dtblRegistration.Rows[0]["Name"].ToString();
                    userInfo.Address = dtblRegistration.Rows[0]["Address"].ToString();
                  //  userInfo.PIN = dtblRegistration.Rows[0]["PIN"].ToString();
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
                else
                {
                    return RedirectToAction("ApproveModifyList");
                }
                return View(userInfo);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        // POST: Admin/ApproveModify/1
        [HttpPost]
        public ActionResult ApproveModify(string btnCommand, FormCollection collection)
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
                        string cClientCode = collection["ClientCode"];
                        string cAddress = collection["Address"];
                        string cStatus = collection["Status"];
                        string cContactNumber1 = collection["ContactNumber1"].ToString();
                        string cContactNumber2 = collection["ContactNumber2"].ToString();
                        string cWalletNumber = collection["WalletNumber"].ToString();
                        string cBranchCode = collection["BranchCode"].ToString();
                        string cBankNo = collection["BankNo"].ToString();
                        string cBankAccountNumber = collection["BankAccountNumber"].ToString();
                        string cIsApproved = collection["IsApproved"].ToString();
                        string cIsRejected = collection["IsRejected"].ToString();
                        string cPin = collection["PIN"].ToString();

                        UserInfo userInfoModify = new UserInfo
                        {
                            ClientCode = cClientCode,
                            Address = cAddress,
                            Status = cStatus,
                            ContactNumber1 = cContactNumber1,
                            ContactNumber2 = cContactNumber2,
                            WalletNumber = cWalletNumber,
                            BankNo = cBankNo,
                            BranchCode = cBankAccountNumber.Substring(0, 3),
                            BankAccountNumber = cBankAccountNumber,
                            IsApproved = cIsApproved,
                            IsRejected = cIsRejected,
                            PIN = cPin
                        };

                        if ((cIsRejected != "") && (cIsApproved != "")
                        && (cWalletNumber != "") && (cContactNumber1 != "") && (cStatus != ""))
                        {
                            bool isUpdated = false;
                            isUpdated = CustomerUtils.UpdateCustomerUserInfo(userInfoModify);
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

                return RedirectToAction("ApproveModify");
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        #endregion


        #region "Email Send"

        //[HttpPost]
        //public ActionResult SendEmail(EmailSend obj)
        //{
        //    try
        //    {
        //        //Configuring webMail class to send emails  
        //        //gmail smtp server  
        //        WebMail.SmtpServer = "smtp.gmail.com";
        //        //gmail port to send emails  
        //        WebMail.SmtpPort = 587;
        //        WebMail.SmtpUseDefaultCredentials = true;
        //        //sending emails with secure protocol  
        //        WebMail.EnableSsl = true;
        //        //EmailId used to send emails from application  
        //        WebMail.UserName = "YourGamilId@gmail.com";
        //        WebMail.Password = "YourGmailPassword";

        //        //Sender email address.  
        //        WebMail.From = "SenderGamilId@gmail.com";

        //        //Send email  
        //        WebMail.Send(to: obj.ToEmail, subject: obj.EmailSubject, body: obj.EMailBody, cc: obj.EmailCc, bcc: obj.EmailBcc, isBodyHtml: true);
        //        ViewBag.Status = "Email Sent Successfully.";
        //    }
        //    catch (Exception)
        //    {
        //        ViewBag.Status = "Problem while sending email, Please check details.";

        //    }
        //    return View();
        //}

        #endregion

        #region Admin Modified Approve
        // GET: Admin
        public ActionResult ModifiedApprove()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            string bankCode = (string)Session["BankCode"];

            TempData["userType"] = userType;
            //Check Role link start
            bool checkRole = false;
            string methodlink = System.Reflection.MethodBase.GetCurrentMethod().Name;
            RoleChecker roleChecker = new RoleChecker();
            checkRole = roleChecker.checkRole(methodlink, clientCode);
            //Check Role link end


            if (this.TempData["custapprove_messsage"] != null)
            {
                this.ViewData["custapprove_messsage"] = this.TempData["custapprove_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }


            if (TempData["userType"] != null&& checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;



                DataSet dsBank = BranchUtils.GetDataSetPopulateBranchName(bankCode);
                DataTable bank = dsBank.Tables[0];
                DataSet dsProfile = UserProfileUtils.GetDataSetPopulateProfileName();
                DataTable profile = dsProfile.Tables[0];

                List<UserInfo> registrationtList = new List<UserInfo>();
                DataTable dtblRegistration = ProfileUtils.GetAdminModify();

                foreach (DataRow row in dtblRegistration.Rows)
                {
                    UserInfo regobj = new UserInfo();
                    regobj.ClientCode = row["ClientCode"].ToString();
                    regobj.Name = row["Name"].ToString();
                    regobj.Address = row["Address"].ToString();
                    regobj.PIN = row["PIN"].ToString();
                    regobj.Status = row["Status"].ToString();
                    regobj.ContactNumber1 = row["ContactNumber1"].ToString();
                    regobj.ContactNumber2 = row["ContactNumber2"].ToString();
                    regobj.EmailAddress = row["EmailAddress"].ToString();
                    regobj.UserName = row["UserName"].ToString();
                    regobj.UserType = row["userType"].ToString();
                    regobj.UserBranchCode = row["UserBranchCode"].ToString();
                    regobj.UserBranchName = row["UserBranchName"].ToString();
                    regobj.ModifyingAdmin = row["ModifiedBy"].ToString();
                    regobj.ProfileName = row["ProfileName"].ToString();
                    regobj.AProfileName = row["AProfileName"].ToString();
                    registrationtList.Add(regobj);
                }
                return View(registrationtList);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }

        //Edit Data//
        // GET: Admin/EditModified
        [HttpGet]
        public ActionResult EditModified(string clientCodeId)
        {
            if (string.IsNullOrEmpty(clientCodeId))
            {
                return RedirectToAction("Index", "Admin");
            }
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            string bankCode = (string)Session["BankCode"];

            TempData["userType"] = userType;


            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                //For Branch
                List<SelectListItem> item = new List<SelectListItem>();
                DataSet dsBank = BranchUtils.GetDataSetPopulateBranchName(bankCode);
                ViewBag.bank = dsBank.Tables[0];
                foreach (DataRow dr in ViewBag.bank.Rows)
                {
                    if (dr["IsBlocked"].ToString() == "F")
                    {
                        item.Add(new SelectListItem
                        {
                            Text = dr["BranchName"].ToString(),
                            Value = dr["BranchCode"].ToString()
                        });
                    }
                }
                //ViewBag.BranchName = item;

                //For ProfileName
                List<SelectListItem> itemProfile = new List<SelectListItem>();
                DataSet dsProfile = UserProfileUtils.GetDataSetPopulateProfileName();
                ViewBag.profile = dsProfile.Tables[0];
                foreach (DataRow dr in ViewBag.profile.Rows)
                {
                    itemProfile.Add(new SelectListItem
                    {
                        Text = @dr["ProfileName"].ToString(),
                        Value = @dr["ProfileCode"].ToString()
                    });
                }


                UserInfo userInfo = new UserInfo();
                DataTable dtblRegistration = ProfileUtils.GetUserProfileInfo(clientCodeId);
                if (dtblRegistration.Rows.Count == 1)
                {
                    userInfo.ClientCode = dtblRegistration.Rows[0]["ClientCode"].ToString();
                    userInfo.Name = dtblRegistration.Rows[0]["Name"].ToString().Capitalize();
                    userInfo.Address = dtblRegistration.Rows[0]["Address"].ToString();
                //    userInfo.PIN = dtblRegistration.Rows[0]["PIN"].ToString();
                    userInfo.Status = dtblRegistration.Rows[0]["Status"].ToString();
                    userInfo.ContactNumber1 = dtblRegistration.Rows[0]["ContactNumber1"].ToString();
                    userInfo.ContactNumber2 = dtblRegistration.Rows[0]["ContactNumber2"].ToString();
                    userInfo.EmailAddress = dtblRegistration.Rows[0]["EmailAddress"].ToString();
                    userInfo.ProfileName = dtblRegistration.Rows[0]["ProfileName"].ToString();
                    userInfo.UserName = dtblRegistration.Rows[0]["UserName"].ToString();
                    userInfo.UserType = dtblRegistration.Rows[0]["UserType"].ToString();
                    userInfo.IsApproved = dtblRegistration.Rows[0]["IsApproved"].ToString();
                    userInfo.IsRejected = dtblRegistration.Rows[0]["IsRejected"].ToString();
                    userInfo.WalletNumber = dtblRegistration.Rows[0]["WalletNumber"].ToString();
                    userInfo.BankNo = dtblRegistration.Rows[0]["BankNo"].ToString();
                    userInfo.BranchCode = dtblRegistration.Rows[0]["UserBranchCode"].ToString();
                    userInfo.BankAccountNumber = dtblRegistration.Rows[0]["BankAccountNumber"].ToString();
                    userInfo.COC = dtblRegistration.Rows[0]["COC"].ToString();
                    if(userInfo.UserType!="admin")
                    {
                        this.TempData["custapprove_messsage"] = "Admin User not found";
                        this.TempData["message_class"] = CssSetting.FailedMessageClass;
                        return RedirectToAction("ModifiedApprove");
                    }
                }
                else
                {
                    this.TempData["custapprove_messsage"] = "Admin User not found";
                    this.TempData["message_class"] = CssSetting.FailedMessageClass;
                    return RedirectToAction("ModifiedApprove");
                }
                DataSet DSetMakerchecker = ProfileUtils.GetAdminModifiedValue(clientCodeId, bankCode);
                List<MNMakerChecker> ModifiedValues = ExtraUtility.DatatableToListClass<MNMakerChecker>(DSetMakerchecker.Tables["MNMakerChecker"]);
                userInfo.MakerChecker = ModifiedValues;
                ViewBag.AdminProfile = new SelectList(itemProfile, "Value", "Text", userInfo.ProfileName).OrderBy(x => x.Text);
                ViewBag.BranchName = new SelectList(item, "Value", "Text", userInfo.BranchCode).OrderBy(x => x.Text);//,selectedValue:userInfo.BranchCode

                return View(userInfo);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        [HttpPost]
        public ActionResult ApproveModifyAdmin(UserInfo model, string btnApprove)
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
                    userInfoApproval.AdminBranch = Session["UserBranch"].ToString(); ;
                    userInfoApproval.Remarks = model.Remarks;


                    string Rejected = "T";
                    string Approve = "UnApprove";

                    userInfoApproval.Remarks = model.Remarks;

                    int ret = CustomerUtils.RejectModifiedAdmin(userInfoApproval);

                    if (ret == 100)
                    {
                        displayMessage = "Admin" + model.Name + " has been Rejected. Please Check Modification Rejected and perform accordingly";
                        messageClass = CssSetting.SuccessMessageClass;
                    }
                    else if (ret == -1 || ret != 100)
                    {
                        displayMessage = "Error while rejecting Admin " + model.Name;
                        messageClass = CssSetting.FailedMessageClass;
                    }

                    this.TempData["custapprove_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;
                    return RedirectToAction("ModifiedApprove");

                }
                else if (btnApprove.ToUpper() == "APPROVE")
                {
                    userInfoApproval.AdminUserName = Session["UserName"].ToString();
                    userInfoApproval.AdminBranch = Session["UserBranch"].ToString();

                    string Rejected = "F";
                    string Approve = "Approve";

                    userInfoApproval.ApprovedBy = userName;
                    int ret = CustomerUtils.ApproveModifiedAdmin(userInfoApproval);
                    if (ret == 100)
                    {
                        displayMessage = "modification Information for  Admin " + model.Name + " has successfully been approved.";
                        messageClass = CssSetting.SuccessMessageClass;
                    }
                    else if (ret == -1 || ret != 100)
                    {
                        displayMessage = "Error while approving Admin Information";
                        messageClass = CssSetting.FailedMessageClass;
                    }

                    this.TempData["custapprove_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;

                    return RedirectToAction("ModifiedApprove");
                }
                else
                {

                    this.TempData["custapprove_messsage"] = "Unauthorized execution";
                    this.TempData["message_class"] = CssSetting.FailedMessageClass;

                    return RedirectToAction("ViewModified");
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

        #region Admin Rejected List
        //Admin Modification Rejected
        // GET: Admin/RejectedList
        public ActionResult RejectedList()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            string bankCode = (string)Session["BankCode"];

            //Check Role link start
            bool checkRole = false;
            string methodlink = System.Reflection.MethodBase.GetCurrentMethod().Name;
            RoleChecker roleChecker = new RoleChecker();
            checkRole = roleChecker.checkRole(methodlink, clientCode);
            //Check Role link end
            TempData["userType"] = userType;

            if (TempData["userType"] != null && checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                string IsModified = "T";
                List<UserInfo> userobj = new List<UserInfo>();

                /// <summary>
                /// UNAPPROVE REJECTED LIST OF CUSTOMER
                /// </summary>
                /// 
                userobj = ProfileUtils.GetApproveRJAdminProfile(IsModified);
                //DataTable dtableUNApproveReject = ProfileUtils.GetUnApproveRJCustomerProfile();



                return View(userobj);

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        //Admin Registration Rejected List
        // GET: Admin/RejectedList
        public ActionResult RegRejectedList()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            string bankCode = (string)Session["BankCode"];
            //Check Role link start
            bool checkRole = false;
            string methodlink = System.Reflection.MethodBase.GetCurrentMethod().Name;
            RoleChecker roleChecker = new RoleChecker();
            checkRole = roleChecker.checkRole(methodlink, clientCode);
            //Check Role link end
            TempData["userType"] = userType;

            if (TempData["userType"] != null && checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                string IsModified = "F";
                List<UserInfo> userobj = new List<UserInfo>();

                /// <summary>
                /// UNAPPROVE REJECTED LIST OF ADMIN
                /// 
                userobj = ProfileUtils.GetApproveRJAdminProfile(IsModified);
                //DataTable dtableUNApproveReject = ProfileUtils.GetUnApproveRJCustomerProfile();



                return View(userobj);

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        //View Admin Modification Rejected
        [HttpGet]
        public ActionResult ViewAdminRejected(string clientCodeId)
        {
            if (string.IsNullOrEmpty(clientCodeId))
            {
                return RedirectToAction("Index", "Admin");
            }
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            string bankCode = (string)Session["BankCode"];

            TempData["userType"] = userType;

            if (this.TempData["register_messsage"] != null)
            {
                this.ViewData["editregister_messsage"] = this.TempData["editregister_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                //For Branch
                List<SelectListItem> item = new List<SelectListItem>();
                DataSet dsBank = BranchUtils.GetDataSetPopulateBranchName(bankCode);
                ViewBag.bank = dsBank.Tables[0];
                foreach (DataRow dr in ViewBag.bank.Rows)
                {
                    if (dr["IsBlocked"].ToString() == "F")
                    {
                        item.Add(new SelectListItem
                        {
                            Text = dr["BranchName"].ToString(),
                            Value = dr["BranchCode"].ToString()
                        });
                    }
                }
                //ViewBag.BranchName = item;

                //For ProfileName
                List<SelectListItem> itemProfile = new List<SelectListItem>();
                DataSet dsProfile = UserProfileUtils.GetDataSetPopulateProfileName();
                ViewBag.profile = dsProfile.Tables[0];
                foreach (DataRow dr in ViewBag.profile.Rows)
                {
                    itemProfile.Add(new SelectListItem
                    {
                        Text = @dr["ProfileName"].ToString(),
                        Value = @dr["ProfileCode"].ToString()
                    });
                }


                UserInfo userInfo = new UserInfo();
                DataTable dtblRegistration = ProfileUtils.GetUserProfileInfo(clientCodeId);
                if (dtblRegistration.Rows.Count == 1)
                {
                    userInfo.ClientCode = dtblRegistration.Rows[0]["ClientCode"].ToString();
                    userInfo.Name = dtblRegistration.Rows[0]["Name"].ToString().Capitalize();
                    userInfo.Status = dtblRegistration.Rows[0]["Status"].ToString();
                    userInfo.EmailAddress = dtblRegistration.Rows[0]["EmailAddress"].ToString();
                    userInfo.ProfileName = dtblRegistration.Rows[0]["ProfileName"].ToString();
                    userInfo.UserName = dtblRegistration.Rows[0]["UserName"].ToString();
                    userInfo.UserType = dtblRegistration.Rows[0]["UserType"].ToString();
                    userInfo.IsApproved = dtblRegistration.Rows[0]["IsApproved"].ToString();
                    userInfo.IsRejected = dtblRegistration.Rows[0]["IsRejected"].ToString();
                    userInfo.BranchCode = dtblRegistration.Rows[0]["UserBranchCode"].ToString();
                    userInfo.BankAccountNumber = dtblRegistration.Rows[0]["BankAccountNumber"].ToString();
                    userInfo.COC = dtblRegistration.Rows[0]["COC"].ToString();
                    userInfo.Remarks = dtblRegistration.Rows[0]["Remarks"].ToString();
                }
                ViewBag.AdminProfile = new SelectList(itemProfile, "Value", "Text", userInfo.ProfileName).OrderBy(x => x.Text);
                ViewBag.BranchName = new SelectList(item, "Value", "Text", userInfo.BranchCode).OrderBy(x => x.Text);//,selectedValue:userInfo.BranchCode

                return View(userInfo);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        //View Admin Registration Rejected
        [HttpGet]
        public ActionResult ViewAdminRegRej(string clientCodeId)
        {
            if (string.IsNullOrEmpty(clientCodeId))
            {
                return RedirectToAction("Index", "Admin");
            }
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            string bankCode = (string)Session["BankCode"];

            TempData["userType"] = userType;

            if (this.TempData["register_messsage"] != null)
            {
                this.ViewData["editregister_messsage"] = this.TempData["editregister_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                //For Branch
                List<SelectListItem> item = new List<SelectListItem>();
                DataSet dsBank = BranchUtils.GetDataSetPopulateBranchName(bankCode);
                ViewBag.bank = dsBank.Tables[0];
                foreach (DataRow dr in ViewBag.bank.Rows)
                {
                    if (dr["IsBlocked"].ToString() == "F" && dr["BankCode"].ToString() == "0004")
                    {
                        item.Add(new SelectListItem
                        {
                            Text = dr["BranchName"].ToString(),
                            Value = dr["BranchCode"].ToString()
                        });
                    }
                }
                //ViewBag.BranchName = item;

                //For ProfileName
                List<SelectListItem> itemProfile = new List<SelectListItem>();
                DataSet dsProfile = UserProfileUtils.GetDataSetPopulateProfileName();
                ViewBag.profile = dsProfile.Tables[0];
                foreach (DataRow dr in ViewBag.profile.Rows)
                {
                    itemProfile.Add(new SelectListItem
                    {
                        Text = @dr["ProfileName"].ToString(),
                        Value = @dr["ProfileCode"].ToString()
                    });
                }


                UserInfo userInfo = new UserInfo();
                DataTable dtblRegistration = ProfileUtils.GetUserProfileInfo(clientCodeId);
                if (dtblRegistration.Rows.Count == 1)
                {
                    userInfo.ClientCode = dtblRegistration.Rows[0]["ClientCode"].ToString();
                    userInfo.Name = dtblRegistration.Rows[0]["Name"].ToString().Capitalize();
                    userInfo.Status = dtblRegistration.Rows[0]["Status"].ToString();
                    userInfo.EmailAddress = dtblRegistration.Rows[0]["EmailAddress"].ToString();
                    userInfo.ProfileName = dtblRegistration.Rows[0]["ProfileName"].ToString();
                    userInfo.UserName = dtblRegistration.Rows[0]["UserName"].ToString();
                    userInfo.UserType = dtblRegistration.Rows[0]["UserType"].ToString();
                    userInfo.IsApproved = dtblRegistration.Rows[0]["IsApproved"].ToString();
                    userInfo.IsRejected = dtblRegistration.Rows[0]["IsRejected"].ToString();
                    userInfo.BranchCode = dtblRegistration.Rows[0]["UserBranchCode"].ToString();
                    userInfo.BankAccountNumber = dtblRegistration.Rows[0]["BankAccountNumber"].ToString();
                    userInfo.COC = dtblRegistration.Rows[0]["COC"].ToString();
                    userInfo.Remarks = dtblRegistration.Rows[0]["Remarks"].ToString();
                }
                ViewBag.AdminProfile = new SelectList(itemProfile, "Value", "Text", userInfo.ProfileName).OrderBy(x => x.Text);
                ViewBag.BranchName = new SelectList(item, "Value", "Text", userInfo.BranchCode).OrderBy(x => x.Text);//,selectedValue:userInfo.BranchCode

                return View(userInfo);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }




        [HttpPost]
        public ActionResult ApproveAdminRejected(UserInfo model, string btnCommand)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            string bankCode = (string)Session["BankCode"];

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


                if (btnCommand.ToUpper() == "SUBMIT")
                {
                    userInfoApproval.ClientCode = model.ClientCode;

                    userInfoApproval.UserName = model.UserName;
                    userInfoApproval.Name = model.Name.Capitalize();
                    userInfoApproval.ProfileName = model.ProfileName;

                    userInfoApproval.EmailAddress = model.EmailAddress;
                    userInfoApproval.BranchCode = model.BranchCode;
                    if (model.COC != null)
                        userInfoApproval.COC = model.COC;
                    else
                        userInfoApproval.COC = "F";
                    userInfoApproval.Status = model.Status;
                    userInfoApproval.AdminBranch = (string)Session["UserBranch"];
                    userInfoApproval.AdminUserName = (string)Session["UserName"];

                    List<SelectListItem> item = new List<SelectListItem>();
                    DataSet dsBank = BranchUtils.GetDataSetPopulateBranchName(bankCode);
                    ViewBag.bank = dsBank.Tables[0];
                    foreach (DataRow dr in ViewBag.bank.Rows)
                    {
                        item.Add(new SelectListItem
                        {
                            Text = @dr["BranchName"].ToString(),
                            Value = @dr["BranchCode"].ToString()
                        });
                    }

                    List<SelectListItem> itemProfile = new List<SelectListItem>();
                    DataSet dsProfile = UserProfileUtils.GetDataSetPopulateProfileName();
                    ViewBag.profile = dsProfile.Tables[0];
                    foreach (DataRow dr in ViewBag.profile.Rows)
                    {
                        itemProfile.Add(new SelectListItem
                        {
                            Text = @dr["ProfileName"].ToString(),
                            Value = @dr["ProfileCode"].ToString()
                        });
                    }

                    ViewBag.AdminProfile = new SelectList(itemProfile, "Value", "Text", userInfoApproval.ProfileName).OrderBy(x => x.Text);
                    ViewBag.BranchName = new SelectList(item, "Value", "Text", userInfoApproval.BranchCode).OrderBy(x => x.Text);
                    //userInfoApproval.AdminUserName = "";
                    //userInfoApproval.AdminBranch = "";

                    //string Rejected = "F";
                    //string Approve = "Approve";

                    userInfoApproval.ApprovedBy = userName;
                    bool ret = CustomerUtils.AprvRjAdmin(userInfoApproval);
                    if (ret)
                    {
                        displayMessage = "Registration Information for  Admin " + model.Name + " has successfully been Re-Modified.Please check Approve Modified Admin and perform accordingly. ";
                        messageClass = CssSetting.SuccessMessageClass;
                    }
                    else
                    {
                        displayMessage = "Error while approving Admin Information";
                        messageClass = CssSetting.FailedMessageClass;
                    }

                    this.TempData["custmodify_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;

                    return RedirectToAction("RejectedList");
                }
                else
                {

                    this.TempData["custmodify_messsage"] = "Unauthorized execution";
                    this.TempData["message_class"] = CssSetting.FailedMessageClass;

                    return RedirectToAction("ViewApproveAdmin");
                }
            }

            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpPost]
        public ActionResult ApproveAdminRegRejected(UserInfo model, string btnCommand)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            string bankCode = (string)Session["BankCode"];

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


                if (btnCommand.ToUpper() == "SUBMIT")
                {
                    userInfoApproval.ClientCode = model.ClientCode;

                    userInfoApproval.UserName = model.UserName;
                    userInfoApproval.Name = model.Name.Capitalize();
                    userInfoApproval.ProfileName = model.ProfileName;

                    userInfoApproval.EmailAddress = model.EmailAddress;
                    userInfoApproval.BranchCode = model.BranchCode;
                    if (model.COC != null)
                        userInfoApproval.COC = model.COC;
                    else
                        userInfoApproval.COC = "F";
                    userInfoApproval.Status = model.Status;
                    userInfoApproval.AdminBranch = (string)Session["UserBranch"];
                    userInfoApproval.AdminUserName = (string)Session["UserName"];

                    List<SelectListItem> item = new List<SelectListItem>();
                    DataSet dsBank = BranchUtils.GetDataSetPopulateBranchName(bankCode);
                    ViewBag.bank = dsBank.Tables[0];
                    foreach (DataRow dr in ViewBag.bank.Rows)
                    {
                        item.Add(new SelectListItem
                        {
                            Text = @dr["BranchName"].ToString(),
                            Value = @dr["BranchCode"].ToString()
                        });
                    }

                    List<SelectListItem> itemProfile = new List<SelectListItem>();
                    DataSet dsProfile = UserProfileUtils.GetDataSetPopulateProfileName();
                    ViewBag.profile = dsProfile.Tables[0];
                    foreach (DataRow dr in ViewBag.profile.Rows)
                    {
                        itemProfile.Add(new SelectListItem
                        {
                            Text = @dr["ProfileName"].ToString(),
                            Value = @dr["ProfileCode"].ToString()
                        });
                    }

                    ViewBag.AdminProfile = new SelectList(itemProfile, "Value", "Text", userInfoApproval.ProfileName).OrderBy(x => x.Text);
                    ViewBag.BranchName = new SelectList(item, "Value", "Text", userInfoApproval.BranchCode).OrderBy(x => x.Text);
                    //userInfoApproval.AdminUserName = "";
                    //userInfoApproval.AdminBranch = "";

                    //string Rejected = "F";
                    //string Approve = "Approve";

                    userInfoApproval.ApprovedBy = userName;
                    bool ret = CustomerUtils.AprvRjAdmin(userInfoApproval);
                    if (ret)
                    {
                        displayMessage = "Registration Information for  Admin " + model.Name + " has been modified. Please Check Approve Registration Admin and perform accordingly.";
                        messageClass = CssSetting.SuccessMessageClass;
                    }
                    else
                    {
                        displayMessage = "Error while approving Admin Information";
                        messageClass = CssSetting.FailedMessageClass;
                    }

                    this.TempData["custmodify_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;

                    return RedirectToAction("RegRejectedList");
                }
                else
                {

                    this.TempData["custmodify_messsage"] = "Unauthorized execution";
                    this.TempData["message_class"] = CssSetting.FailedMessageClass;

                    return RedirectToAction("ViewApproveAdmin");
                }
            }

            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        #endregion

        #region APPROVE ADMIN REGISTRATION LIST
        [HttpGet]
        public ActionResult ApproveAdminList(string Key, string Value)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            //Check Role link start
            bool checkRole = false;
            string methodlink = System.Reflection.MethodBase.GetCurrentMethod().Name;
            RoleChecker roleChecker = new RoleChecker();
            checkRole = roleChecker.checkRole(methodlink, clientCode);
            //Check Role link end
            string UserName = Value;

           


            if (this.TempData["custapprove_messsage"] != null)
            {
                this.ViewData["custapprove_messsage"] = this.TempData["custapprove_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }


            TempData["userType"] = userType;
            if (TempData["userType"] != null&&checkRole==true)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                //For ProfileName
                //List<SelectListItem> itemProfile = new List<SelectListItem>();
                //DataSet dsProfile = UserProfileUtils.GetDataSetPopulateProfileName();
                //ViewBag.profile = dsProfile.Tables[0];
                //foreach (DataRow dr in ViewBag.profile.Rows)
                //{
                //    itemProfile.Add(new SelectListItem
                //    {
                //        Text = @dr["ProfileName"].ToString(),
                //        Value = @dr["ProfileName"].ToString()
                //    });
                //}

                //ViewBag.ProfileName = itemProfile.OrderBy(x => x.Text);

                //For ProfileName
                List<SelectListItem> itemProfile = new List<SelectListItem>();
                DataSet dsProfile = UserProfileUtils.GetDataSetPopulateProfileName();
                ViewBag.profile = dsProfile.Tables[0];
                foreach (DataRow dr in ViewBag.profile.Rows)
                {
                    itemProfile.Add(new SelectListItem
                    {
                        Text = @dr["ProfileName"].ToString(),
                        Value = @dr["ProfileCode"].ToString()
                    });
                }

                


                List<UserInfo> adminUnApproveObj = new List<UserInfo>();
                var adminInfo = CustomerUtils.GetUnApproveAdminProfile("admin", UserName);
                ViewBag.ProfileName = new SelectList(itemProfile, "Value", "Text", itemProfile).OrderBy(x => x.Text);


                ViewBag.Value = Value;
                return View(adminInfo);

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }



        //View Approve Admin Registration Details
        //ViewApproveAdmin
        public ActionResult ViewApproveAdmin(string clientCodeId)
        {
            if (string.IsNullOrEmpty(clientCodeId))
            {
                return RedirectToAction("Index", "Admin");
            }
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            string bankCode = (string)Session["BankCode"];

            TempData["userType"] = userType;

            if (this.TempData["register_messsage"] != null)
            {
                this.ViewData["editregister_messsage"] = this.TempData["editregister_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                //For Branch
                List<SelectListItem> item = new List<SelectListItem>();
                DataSet dsBank = BranchUtils.GetDataSetPopulateBranchName(bankCode);
                ViewBag.bank = dsBank.Tables[0];
                foreach (DataRow dr in ViewBag.bank.Rows)
                {
                    if (dr["IsBlocked"].ToString() == "F")
                    {
                        item.Add(new SelectListItem
                        {
                            Text = dr["BranchName"].ToString(),
                            Value = dr["BranchCode"].ToString()
                        });
                    }
                }
                //ViewBag.BranchName = item;

                //For ProfileName
                List<SelectListItem> itemProfile = new List<SelectListItem>();
                DataSet dsProfile = UserProfileUtils.GetDataSetPopulateProfileName();
                ViewBag.profile = dsProfile.Tables[0];
                foreach (DataRow dr in ViewBag.profile.Rows)
                {
                    itemProfile.Add(new SelectListItem
                    {
                        Text = @dr["ProfileName"].ToString(),
                        Value = @dr["ProfileCode"].ToString()
                    });
                }


                UserInfo userInfo = new UserInfo();
                DataTable dtblRegistration = ProfileUtils.GetUserProfileInfo(clientCodeId);
                if (dtblRegistration.Rows.Count == 1)
                {
                    userInfo.ClientCode = dtblRegistration.Rows[0]["ClientCode"].ToString();
                    userInfo.Name = dtblRegistration.Rows[0]["Name"].ToString().Capitalize();
                    userInfo.Address = dtblRegistration.Rows[0]["Address"].ToString();
                    userInfo.PIN = dtblRegistration.Rows[0]["PIN"].ToString();
                    userInfo.Status = dtblRegistration.Rows[0]["Status"].ToString();
                    userInfo.ContactNumber1 = dtblRegistration.Rows[0]["ContactNumber1"].ToString();
                    userInfo.ContactNumber2 = dtblRegistration.Rows[0]["ContactNumber2"].ToString();
                    userInfo.EmailAddress = dtblRegistration.Rows[0]["EmailAddress"].ToString();
                    userInfo.ProfileName = dtblRegistration.Rows[0]["ProfileName"].ToString();
                    userInfo.UserName = dtblRegistration.Rows[0]["UserName"].ToString();
                    userInfo.UserType = dtblRegistration.Rows[0]["UserType"].ToString();
                    userInfo.IsApproved = dtblRegistration.Rows[0]["IsApproved"].ToString();
                    userInfo.IsRejected = dtblRegistration.Rows[0]["IsRejected"].ToString();
                    userInfo.WalletNumber = dtblRegistration.Rows[0]["WalletNumber"].ToString();
                    userInfo.BankNo = dtblRegistration.Rows[0]["BankNo"].ToString();
                    userInfo.BranchCode = dtblRegistration.Rows[0]["UserBranchCode"].ToString();
                    userInfo.BankAccountNumber = dtblRegistration.Rows[0]["BankAccountNumber"].ToString();
                    userInfo.COC = dtblRegistration.Rows[0]["COC"].ToString();
                }
                ViewBag.AdminProfile = new SelectList(itemProfile, "Value", "Text", userInfo.ProfileName).OrderBy(x => x.Text);
                ViewBag.BranchName = new SelectList(item, "Value", "Text", userInfo.BranchCode).OrderBy(x => x.Text);//,selectedValue:userInfo.BranchCode

                return View(userInfo);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpPost]
        public ActionResult ApproveRegisterAdmin(UserInfo model, string btnApprove)
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

                    int ret = CustomerUtils.AdminRegisterReject(userInfoApproval, Rejected);

                    if (ret == 1)
                    {
                        displayMessage = "Admin" + model.Name + " has been Rejected. Please Check Rejectlist and perform accordingly";
                        messageClass = CssSetting.SuccessMessageClass;
                    }
                    else if (ret == -1 || ret != 100)
                    {
                        displayMessage = "Error while rejecting Admin " + model.Name;
                        messageClass = CssSetting.FailedMessageClass;
                    }

                    this.TempData["custapprove_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;
                    return RedirectToAction("ApproveAdminList");

                }
                else if (btnApprove.ToUpper() == "APPROVE")
                {
                    userInfoApproval.AdminUserName = "";
                    userInfoApproval.AdminBranch = "";

                    //string Rejected = "F";
                    string Approve = "Approve";
                    userInfoApproval.ApprovedBy = userName;
                    int ret = CustomerUtils.AdminRegisterApprove(userInfoApproval, Approve);
                    bool passChange = false;
                    if (ret == 1)
                    {
                        UserInfo userInfo = new UserInfo();
                        DataTable dtblRegistration = ProfileUtils.GetUserProfileInfo(model.ClientCode);
                        if (dtblRegistration.Rows.Count == 1)
                        {
                            userInfo.Name = dtblRegistration.Rows[0]["Name"].ToString().Capitalize();
                            userInfo.EmailAddress = dtblRegistration.Rows[0]["EmailAddress"].ToString();
                            userInfo.UserName = dtblRegistration.Rows[0]["UserName"].ToString();
                            userInfo.Password = dtblRegistration.Rows[0]["Password"].ToString();
                            userInfo.ClientCode = dtblRegistration.Rows[0]["ClientCode"].ToString();
                            userInfo.Password = CustomerUtils.GeneratePassword();
                             passChange = PasswordUtils.ResetPassword(userInfo);
                        }
                        
                        if (passChange) { 
                        if (userInfo.EmailAddress != "" && userInfo.EmailAddress != string.Empty)
                        {
                            string Subject = "New Admin Registration";
                            string MailSubject = "<span style='font-size:15px;'><h4>Dear " + userInfo.Name + ",</h4>";
                            MailSubject += "A new Account has been created  for you at Nepal Investment Bank Ltd. Mobile Banking,";
                            MailSubject += "and you have been issued with a new temporary password.<br/>";
                            MailSubject += "<br/><b>Username: " + userInfo.UserName + "</b> <br/>";
                            MailSubject += "<br/><b>Password: " + userInfo.Password + "</b> <br/>";
                            MailSubject += "                    (Please change your password after login.)<br/>";
                            MailSubject += "<br/>Thank You <br/>";
                            MailSubject += "<br/>Nepal Investment Bank Ltd. </span><br/>";
                            MailSubject += @"<br/>Note: This is confidential mail, Please do not share with other. Your credential purpose is for NIBL Administration Console only.<br/>";
                            MailSubject += "<hr/>";
                            EMailUtil SendMail = new EMailUtil();
                            try
                            {
                                SendMail.SendMail(userInfo.EmailAddress, Subject, MailSubject);
                            }
                            catch
                            {

                            }
                        }
                            displayMessage = "Registration Information for  Admin " + model.Name + " has successfully been approved.";
                            messageClass = CssSetting.SuccessMessageClass;
                        }
                        else
                        {
                            displayMessage = "Error while approving Admin Information";
                            messageClass = CssSetting.FailedMessageClass;
                        }
                       
                    }
                    else if (ret == -1 || ret != 100)
                    {
                        displayMessage = "Error while approving Admin Information";
                        messageClass = CssSetting.FailedMessageClass;
                    }

                    this.TempData["custapprove_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;
                    return RedirectToAction("ApproveAdminList");
                }
                else
                {

                    this.TempData["custapprove_messsage"] = "Unauthorized execution";
                    this.TempData["message_class"] = CssSetting.FailedMessageClass;

                    return RedirectToAction("ViewApproveAdmin");
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

        #region Approve Password Reset

        [HttpGet]
        public ActionResult ApprovePasswordReset(string UserName)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            //Check Role link start
            bool checkRole = false;
            string methodlink = System.Reflection.MethodBase.GetCurrentMethod().Name;
            RoleChecker roleChecker = new RoleChecker();
            checkRole = roleChecker.checkRole(methodlink, clientCode);
            //Check Role link end
            TempData["userType"] = userType;

            if (TempData["userType"] != null&& checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                var model = PinUtils.GetPasswordResetList(UserName);
                return View(model);
            }
            else { return RedirectToAction("Index", "Login"); }
        }


        [HttpGet]
        public ContentResult PasswordResetApprove(string ClientCode, string Name, string ActionClick)
        {

            string convert = string.Empty; ;
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

                bool isUpdated = false;

                UserInfo info = new UserInfo();
                DataTable dtableUserInfo = ProfileUtils.GetUserProfileInfo(ClientCode);
                if (dtableUserInfo != null && dtableUserInfo.Rows.Count > 0)
                {
                    info.ClientCode = dtableUserInfo.Rows[0]["ClientCode"].ToString();
                    info.Name = dtableUserInfo.Rows[0]["Name"].ToString();
                    info.EmailAddress = dtableUserInfo.Rows[0]["EmailAddress"].ToString();
                    info.Status = dtableUserInfo.Rows[0]["Status"].ToString();
                    info.Password = CustomerUtils.GeneratePassword();
                    info.UserName = dtableUserInfo.Rows[0]["UserName"].ToString();
                    info.UserType = dtableUserInfo.Rows[0]["userType"].ToString();
                    info.IsApproved = dtableUserInfo.Rows[0]["IsApproved"].ToString();
                    info.IsRejected = dtableUserInfo.Rows[0]["IsRejected"].ToString();


                }

                if (ActionClick == "Approve")
                {

                    if ((info.Password != "") && (info.ClientCode != "") && (info.Status == "APR"))
                    {
                        isUpdated = PasswordUtils.UpdateAdminPasswordReset(info);
                        if (isUpdated)
                        {

                            string Subject = "Password Reset";
                            string MailSubject = "<span style='font-size:15px;'><h4>Dear " + info.Name + ",</h4>";
                            MailSubject += "Your password reset request for user <b>" + info.UserName + "</b> has been acknowledged";
                            MailSubject += "and you have been issued with a new temporary password.<br/>";
                            MailSubject += "<br/><b>Password: " + info.Password + "</b> <br/>";
                            MailSubject += "                    (Please change your password after login.)<br/>";
                            MailSubject += "<br/>Thank You <br/>";
                            MailSubject += "<br/>Nepal Investment Bank Ltd. </span><br/>";
                            MailSubject += @"<br/>Note: This is confidential mail, Please do not share with other. Your credential purpose is for NIBL Administration Console only.<br/>";
                            MailSubject += "<hr/>";
                            EMailUtil SendMail = new EMailUtil();
                            SendMail.SendMail(info.EmailAddress, Subject, MailSubject);

                            convert = JsonConvert.SerializeObject(
                            new
                            {
                                StatusCode = "200",
                                StatusMessage = "Password reset successful and Email has been sent to " + info.UserName
                            }

                        );
                        }
                        else
                        {
                            convert = JsonConvert.SerializeObject(
                            new
                            {
                                StatusCode = "210",
                                StatusMessage = "Error Approving customer pin"
                            }
                        );


                        }
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
                             StatusMessage = "Reverted password request for " + info.Name
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
                convert = JsonConvert.SerializeObject(
                       new
                       {
                           StatusCode = "210",
                           StatusMessage = ex.Message
                       }
                   );
                return Content(convert, "application/json");
            }
        }

        #endregion

      
    }
}