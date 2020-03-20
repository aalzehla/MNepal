using MNepalWeb.Helper;
using MNepalWeb.Models;
using MNepalWeb.Settings;
using MNepalWeb.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Web.SessionState;

namespace MNepalWeb.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class AgentController : Controller
    {
        #region "Agent CreateRegistration"

        DAL objdal = new DAL();
        // GET: Agent/CreateRegistration
        public ActionResult CreateRegistration()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            //start milayako
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
            //end milayako
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

                ViewBag.User = "agent";

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

                ViewBag.Bank = item.Where(x => x.Value != "0000").ToList();
                string bankCode = item[0].Value.ToString();

                UserInfo userobj = new UserInfo();
                return View(userobj);

                
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        //start milayako
        public JsonResult getdistrict(int id)
        {
            string provincestring = "select * from MNDistrict where ProvinceID='" + id + "'";


            DataTable dt = new DataTable();
            dt = objdal.MyMethod(provincestring);
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem { Text = "--Select District--", Value = "0" });
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
        //end milayako
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


        // POST: Agent/CreateRegistration
        [HttpPost]
        public ActionResult CreateRegistration(FormCollection collection, UserInfo model)
        {

            UserInfo userInfo = new UserInfo();
            try
            {
                string userName = (string)Session["LOGGED_USERNAME"];

                string clientCode = (string)Session["LOGGEDUSER_ID"];
                string name = (string)Session["LOGGEDUSER_NAME"];
                string userType = (string)Session["LOGGED_USERTYPE"];

                //start milayako3
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
                //end milayako3

                TempData["userType"] = userType;

                if (TempData["userType"] != null)
                {
                    this.ViewData["userType"] = this.TempData["userType"];
                    ViewBag.UserType = this.TempData["userType"];
                    ViewBag.Name = name;

           
                    userInfo.UserName = collection["txtUserName"].ToString();
                    userInfo.ContactNumber1 = collection["txtContactNumber1"].ToString();
                    userInfo.FName = collection["txtFirstName"].ToString();
                    userInfo.MName = collection["txtMiddleName"].ToString();
                    userInfo.LName = collection["txtLastName"].ToString();
                    userInfo.FName = collection["txtFirstName"].ToString();
                   
                    userInfo.Name = userInfo.FName + " " + userInfo.MName + " " + userInfo.LName;
                    userInfo.Gender = collection["txtGender"].ToString();
                    userInfo.DOB = collection["DOB"].ToString();

                    userInfo.Nationality = collection["txtNationality"].ToString();
                    userInfo.FatherName = collection["txtFatherName"].ToString();
                    userInfo.SpouseName = collection["SpouseName"].ToString();
                    userInfo.MaritalStatus = collection["MaritalStatus"].ToString();


                    userInfo.GrandFatherName = collection["txtGrandFatherName"].ToString();
                    userInfo.FatherInlawName = collection["txtFatherInlawName"].ToString();
                    userInfo.Occupation = collection["txtOccupation"].ToString();
                  
                  
                    userInfo.EmailAddress = collection["txtEmail"].ToString();
                    userInfo.PanNo = collection["txtPanNo"].ToString();

                    //userInfo.Photo1 = collection["txtPhoto1"].ToString();
                    userInfo.Country = collection["txtCountry"].ToString();
                    userInfo.ContactNumber2 = collection["txtContactNumber2"].ToString();

                    userInfo.PProvince = collection["PProvince"].ToString();
                    userInfo.PDistrict = collection["PDistrict"].ToString();

                    //userInfo.PZone = collection["txtPZone"].ToString();
                    //userInfo.PDistrict = collection["txtPDistrict"].ToString();


                    userInfo.PVDC = collection["txtPVDC"].ToString();
                    userInfo.PHouseNo = collection["txtPHouseNo"].ToString();
                    userInfo.PWardNo = collection["txtPWardNo"].ToString();
                    userInfo.PAddress = userInfo.PHouseNo + " " + userInfo.PWardNo + " " + userInfo.PDistrict + " " + userInfo.PVDC +" " +userInfo.PZone ;


                    userInfo.CProvince = collection["CProvince"].ToString();
                    userInfo.CDistrict = collection["CDistrict"].ToString();

                    //userInfo.CZone = collection["txtCZone"].ToString();
                    //userInfo.CDistrict = collection["txtCDistrict"].ToString();
                    userInfo.CVDC = collection["txtCVDC"].ToString();
                    userInfo.CHouseNo = collection["txtCHouseNo"].ToString();
                    userInfo.CWardNo = collection["txtCWardNo"].ToString();
                    userInfo.CAddress = userInfo.CHouseNo + " " + userInfo.CWardNo + " " + userInfo.CDistrict + " " + userInfo.CVDC + " " + userInfo.CZone;
                    userInfo.Address = userInfo.CAddress;

                    userInfo.Citizenship = collection["txtCitizenship"].ToString();
                    userInfo.CitizenshipIssueDate = collection["CitizenshipIssueDate"].ToString();
                    userInfo.CitizenshipPlaceOfIssue = collection["txtCitizenshipPlaceOfIssue"].ToString();

                    userInfo.License = collection["txtLicense"].ToString();
                    //test//
                    if (!string.IsNullOrEmpty(userInfo.LicenseIssueDate))
                    {
                        DateTime.ParseExact(userInfo.LicenseIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        userInfo.LicenseIssueDate = "";
                    }
                    userInfo.LicensePlaceOfIssue = collection["txtLicensePlaceOfIssue"].ToString();

                    if (!string.IsNullOrEmpty(userInfo.LicenseExpireDate))
                    {
                        DateTime.ParseExact(userInfo.LicenseExpireDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        userInfo.LicenseExpireDate = "";
                    }
                    

                    userInfo.Passport = collection["txtPassport"].ToString();

                    if (!string.IsNullOrEmpty(userInfo.PassportIssueDate))
                    {
                        DateTime.ParseExact(userInfo.PassportIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        userInfo.PassportIssueDate = "";
                    }
                   
                    userInfo.PassportPlaceOfIssue = collection["txtPassportPlaceOfIssue"].ToString();

                    if (!string.IsNullOrEmpty(userInfo.PassportExpireDate))
                    {
                        DateTime.ParseExact(userInfo.PassportExpireDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        userInfo.PassportExpireDate = "";
                    }
                    

                    userInfo.Document = collection["txtDocument"].ToString();
                    //userInfo.Front = collection["txtFront"].ToString();
                    //userInfo.Back = collection["txtBack"].ToString();

                    userInfo.UserType = "agent";
                    userInfo.Status = "Active";
                    userInfo.IsApproved = "UnApprove";
                    userInfo.IsRejected = "F";

                    userInfo.BankNo = collection["BankNoBin"];
                    userInfo.BankAccountNumber = collection["txtBankAccountNumber"].ToString();
                    userInfo.BranchCode = string.Empty;

                    if (userInfo.BankAccountNumber != "")
                        userInfo.BranchCode = userInfo.BankAccountNumber.Substring(0, 3);

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
                    ViewBag.Bank = item.Where(x => x.Value != "0000").ToList();

                    string bankCode = item[0].Value.ToString();

                    //userInfo.AgentId = collection["txtAgentId"].ToString();

                    //Password and Pin auto generated//

                    //string txtWardNo = collection["txtWardNo"].ToString();
                    //string txtDistrict = collection["txtDistrict"].ToString();
                    //string txtZone = collection["txtZone"].ToString();
                    //userInfo.Address = txtAddress + "," + txtWardNo + "," + txtDistrict + "," + txtZone;

                    //userInfo.PIN = collection["txtPin"].ToString();
                    //userInfo.Status = collection["txtStatus"].ToString();

                    //
                    //

                    //userInfo.ClientCode = collection["txtCClientCode"].ToString();


                    //userInfo.WalletNumber = collection["txtWalletNumber"].ToString();
                    //userInfo.WBankCode = collection["WalletCode"];
                    //userInfo.WBranchCode = collection["WBranchCode"].ToString();
                    //userInfo.WIsDefault = collection["txtWIsDefault"].ToString();
                    //userInfo.AgentId = collection["txtAgentId"].ToString();




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
                                    int results = RegisterUtils.AgentRegisterCustomerInfo(userInfo);
                                    if (results > 0)
                                    {
                                        result = true;
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
                            result = false;
                            errorMessage = "Already Register username";
                        }
                    }
                    else
                    {
                        result = false;
                    }

                    this.ViewData["registration_message"] = result
                                                  ? "Registration information is successfully added."
                                                  : "Error while inserting the information. ERROR :: "
                                                    + errorMessage;
                    this.ViewData["message_class"] = result ? "success_info" : "failed_info";

                    //For Bank
                    if (result)
                    {
                        TempData["registration_message"] =ViewData["registration_message"];

                        TempData["message_class"] =ViewData["message_class"];
                        return this.RedirectToAction("CreateRegistration");
                    }


                    return View(userInfo);
                }
                else
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            catch(Exception ex)
            {
                return View(userInfo);
            }
        }

        #endregion


        #region "Agent List Index "


        // GET: Agent
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

        // GET: Agent/AgentDetails/5
        public ActionResult AgentDetails(string SearchCol, string txtMobileNo, string txtName, string txtAccountNo)
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

                UserInfo userInfo = new UserInfo
                {
                    ContactNumber1 = txtMobileNo,
                    Name = txtName,
                    WalletNumber = txtAccountNo
                };

                List<UserInfo> customerStatus = new List<UserInfo>();

                if (SearchCol == "All") 
                {
                    DataTable dtableCustomerStatusByAc = AgentUtils.GetAgentProfileByALL(userInfo.Name, userInfo.WalletNumber, userInfo.ContactNumber1);
                    if (dtableCustomerStatusByAc != null && dtableCustomerStatusByAc.Rows.Count > 0)
                    {
                        UserInfo regobj = new UserInfo
                        {
                            ClientCode = dtableCustomerStatusByAc.Rows[0]["ClientCode"].ToString(),
                            Name = dtableCustomerStatusByAc.Rows[0]["Name"].ToString(),
                            Address = dtableCustomerStatusByAc.Rows[0]["Address"].ToString(),
                            PIN = dtableCustomerStatusByAc.Rows[0]["PIN"].ToString(),
                            Status = dtableCustomerStatusByAc.Rows[0]["Status"].ToString(),
                            ContactNumber1 = dtableCustomerStatusByAc.Rows[0]["ContactNumber1"].ToString(),
                            ContactNumber2 = dtableCustomerStatusByAc.Rows[0]["ContactNumber2"].ToString(),
                            UserName = dtableCustomerStatusByAc.Rows[0]["UserName"].ToString(),
                            UserType = dtableCustomerStatusByAc.Rows[0]["userType"].ToString()
                        };

                        customerStatus.Add(regobj);
                        ViewData["dtableAgentStatus"] = dtableCustomerStatusByAc;
                    }
                }
                if ((userInfo.Name != "") && (userInfo.WalletNumber != ""))
                {
                    DataTable dtableCustomerStatusByAc = AgentUtils.GetAgentProfileByNameAC(userInfo.Name, userInfo.WalletNumber);
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

                        customerStatus.Add(regobj);
                        ViewData["dtableAgentStatus"] = dtableCustomerStatusByAc;
                    }
                }
                if ((userInfo.ContactNumber1 != "") && (SearchCol == "Mobile Number"))
                {
                    DataTable dtableCustomerStatusByMobileNo = AgentUtils.GetAgentProfileByMobileNo(userInfo.ContactNumber1);
                    if (dtableCustomerStatusByMobileNo != null && dtableCustomerStatusByMobileNo.Rows.Count > 0)
                    {
                        UserInfo regobj = new UserInfo
                        {
                            ClientCode = dtableCustomerStatusByMobileNo.Rows[0]["ClientCode"].ToString(),
                            Name = dtableCustomerStatusByMobileNo.Rows[0]["Name"].ToString(),
                            Address = dtableCustomerStatusByMobileNo.Rows[0]["Address"].ToString(),
                            PIN = dtableCustomerStatusByMobileNo.Rows[0]["PIN"].ToString(),
                            Status = dtableCustomerStatusByMobileNo.Rows[0]["Status"].ToString(),
                            ContactNumber1 = dtableCustomerStatusByMobileNo.Rows[0]["ContactNumber1"].ToString(),
                            ContactNumber2 = dtableCustomerStatusByMobileNo.Rows[0]["ContactNumber2"].ToString(),
                            UserName = dtableCustomerStatusByMobileNo.Rows[0]["UserName"].ToString(),
                            UserType = dtableCustomerStatusByMobileNo.Rows[0]["userType"].ToString()
                        };

                        customerStatus.Add(regobj);
                        ViewData["dtableAgentStatus"] = dtableCustomerStatusByMobileNo;
                    }
                }
                if ((userInfo.Name != "") && (SearchCol == "Name"))
                {
                    DataTable dtableCustomerStatusByName = AgentUtils.GetAgentProfileByName(userInfo.Name);
                    if (dtableCustomerStatusByName != null && dtableCustomerStatusByName.Rows.Count > 0)
                    {
                        UserInfo regobj = new UserInfo
                        {
                            ClientCode = dtableCustomerStatusByName.Rows[0]["ClientCode"].ToString(),
                            Name = dtableCustomerStatusByName.Rows[0]["Name"].ToString(),
                            Address = dtableCustomerStatusByName.Rows[0]["Address"].ToString(),
                            PIN = dtableCustomerStatusByName.Rows[0]["PIN"].ToString(),
                            Status = dtableCustomerStatusByName.Rows[0]["Status"].ToString(),
                            ContactNumber1 = dtableCustomerStatusByName.Rows[0]["ContactNumber1"].ToString(),
                            ContactNumber2 = dtableCustomerStatusByName.Rows[0]["ContactNumber2"].ToString(),
                            UserName = dtableCustomerStatusByName.Rows[0]["UserName"].ToString(),
                            UserType = dtableCustomerStatusByName.Rows[0]["userType"].ToString()
                        };

                        customerStatus.Add(regobj);
                        ViewData["dtableAgentStatus"] = dtableCustomerStatusByName;
                    }
                }
                if (userInfo.WalletNumber != "")
                {
                    DataTable dtableCustomerStatusByAc = AgentUtils.GetAgentProfileByAC(userInfo.WalletNumber);
                    if (dtableCustomerStatusByAc != null && dtableCustomerStatusByAc.Rows.Count > 0)
                    {
                        UserInfo regobj = new UserInfo
                        {
                            ClientCode = dtableCustomerStatusByAc.Rows[0]["ClientCode"].ToString(),
                            Name = dtableCustomerStatusByAc.Rows[0]["Name"].ToString(),
                            Address = dtableCustomerStatusByAc.Rows[0]["Address"].ToString(),
                            PIN = dtableCustomerStatusByAc.Rows[0]["PIN"].ToString(),
                            Status = dtableCustomerStatusByAc.Rows[0]["Status"].ToString(),
                            ContactNumber1 = dtableCustomerStatusByAc.Rows[0]["ContactNumber1"].ToString(),
                            ContactNumber2 = dtableCustomerStatusByAc.Rows[0]["ContactNumber2"].ToString(),
                            UserName = dtableCustomerStatusByAc.Rows[0]["UserName"].ToString(),
                            UserType = dtableCustomerStatusByAc.Rows[0]["userType"].ToString()
                        };

                        customerStatus.Add(regobj);
                        ViewData["dtableAgentStatus"] = dtableCustomerStatusByAc;
                    }
                }

                return View(customerStatus);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        #endregion


        #region "Agent Modify"


        // GET: Agent/AgentModify/5
        public ActionResult AgentModify(string id)
        {

            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            if (this.TempData["agenttmodify_messsage"] != null)
            {
                this.ViewData["agenttmodify_messsage"] = this.TempData["agenttmodify_messsage"];
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
               // ViewBag.Bank = item;


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
                    regobj.EmailAddress= dtableUserInfo.Rows[0]["EmailAddress"].ToString();
                    regobj.IsApproved = dtableUserInfo.Rows[0]["IsApproved"].ToString();
                    regobj.IsRejected = dtableUserInfo.Rows[0]["IsRejected"].ToString();
                    regobj.WalletNumber = dtableUserInfo.Rows[0]["WalletNumber"].ToString();
                    regobj.BankNo = dtableUserInfo.Rows[0]["BankNo"].ToString();
                    regobj.BranchCode = dtableUserInfo.Rows[0]["BranchCode"].ToString();
                    regobj.BankAccountNumber = dtableUserInfo.Rows[0]["BankAccountNumber"].ToString();

                }
                ViewBag.Bank = new SelectList(item,"Value","Text" ,regobj.BankNo);
                return View(regobj);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        // POST: Agent/AgentModify/5
        [HttpPost]
        public ActionResult AgentModify(string id, string btnCommand, FormCollection collection)
        {
            string Id="";
            string displayMessage = "";
            string messageClass = "";
            try
            {
                string userName = (string)Session["LOGGED_USERNAME"];
                string clientCode = (string)Session["LOGGEDUSER_ID"];
                string name = (string)Session["LOGGEDUSER_NAME"];
                string userType = (string)Session["LOGGED_USERTYPE"];

                TempData["userType"] = userType;

                if (TempData["userType"] != null)
                {
                    UserInfo userInfoModify = new UserInfo();
                    this.ViewData["userType"] = this.TempData["userType"];
                    ViewBag.UserType = this.TempData["userType"];
                    ViewBag.Name = name;
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
                  
                    if (btnCommand == "Submit")
                    {
                        string cClientCode = collection["ClientCode"].ToString();
                        Id = cClientCode;
                        string cAddress = collection["Address"].ToString();
                        string cName = collection["Name"].ToString();
                        //string cStatus = collection["Status"].ToString();
                        string cContactNumber1 = collection["ContactNumber1"].ToString();
                        string cContactNumber2 = collection["ContactNumber2"].ToString();
                        string cWalletNumber = collection["WalletNumber"].ToString();
                        string cBankNo = collection["BankNoBin"].ToString();
                        string cBranchCode = collection["BranchCode"].ToString();
                        string cBankAccountNumber = collection["BankAccountNumber"].ToString();
                        string cIsApproved = collection["IsApproved"].ToString();
                        string Emaill = collection["EmailAddress"].ToString();
                        //string cIsRejected = collection["IsRejected"].ToString();
                        //string cPIN = collection["PIN"].ToString();

                        userInfoModify.ClientCode = cClientCode;
                        userInfoModify.Address = cAddress;
                        userInfoModify.Name = cName;
                       //userInfoModify.Status = cStatus;
                        userInfoModify.ContactNumber1 = cContactNumber1;
                        userInfoModify.ContactNumber2 = cContactNumber2;
                        userInfoModify.WalletNumber = cWalletNumber;
                        userInfoModify.EmailAddress = Emaill;
                        userInfoModify.BankNo = cBankNo;
                        userInfoModify.BranchCode = cBranchCode;
                        userInfoModify.BankAccountNumber = cBankAccountNumber;
                        userInfoModify.IsApproved = cIsApproved;
                     //    userInfoModify.IsRejected = cIsRejected;
                     //   userInfoModify.PIN = cPIN;

                        if ((cWalletNumber != "") && (cContactNumber1 != "") && (cName != "") && (cAddress != ""))
                        {
                            bool isUpdated = CustomerUtils.UpdateAgentInfo(userInfoModify);
                            displayMessage = isUpdated
                                                     ? "Agent Information has successfully been updated."
                                                     : "Error while updating Agent Information";
                            messageClass = isUpdated ? CssSetting.SuccessMessageClass : CssSetting.FailedMessageClass;
                        }
                        else
                        {
                            displayMessage = "Required Field is Empty";
                            messageClass = CssSetting.FailedMessageClass;
                        }

                        this.TempData["agenttmodify_messsage"] = displayMessage;
                        this.TempData["message_class"] = messageClass;
                        return RedirectToAction("AgentModify");
                    }
                    ViewBag.Bank = new SelectList(item, "Value", "Text", userInfoModify.BankNo);
                    return View();

                }
                else
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            catch(Exception ex)
            {
                displayMessage = "Error while updating Agent Information. Error cause::" + ex.Message;
                messageClass =  CssSetting.FailedMessageClass;
                return RedirectToAction("AgentModify", new { id = Id });
           
            }
            finally
            {
                this.TempData["agenttmodify_messsage"] = displayMessage;
                this.TempData["message_class"] = messageClass;
            }
        }


        #endregion


        #region "Agent Status"


        // GET: Agent/AgentStatus
        public ActionResult AgentStatus()
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


        // GET: Agent/AgentDetailView
        [HttpGet]
        public ActionResult AgentDetailView(string txtMobileNo, string txtName, string txtAccountNo)
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

                UserInfo userInfo = new UserInfo
                {
                    ContactNumber1 = txtMobileNo,
                    Name = txtName,
                    WalletNumber = txtAccountNo
                };

                List<UserInfo> customerStatus = new List<UserInfo>();

                if ((userInfo.Name != "") && (userInfo.WalletNumber != "") && (userInfo.ContactNumber1 != ""))
                {
                    DataTable dtableCustomerStatusByAll = AgentUtils.GetAgentProfileByALL(userInfo.Name, userInfo.WalletNumber, userInfo.ContactNumber1);
                    if (dtableCustomerStatusByAll != null && dtableCustomerStatusByAll.Rows.Count > 0)
                    {
                        UserInfo regobj = new UserInfo();
                        regobj.ClientCode = dtableCustomerStatusByAll.Rows[0]["ClientCode"].ToString();
                        regobj.Name = dtableCustomerStatusByAll.Rows[0]["Name"].ToString();
                        regobj.Address = dtableCustomerStatusByAll.Rows[0]["Address"].ToString();
                        regobj.PIN = dtableCustomerStatusByAll.Rows[0]["PIN"].ToString();
                        regobj.Status = dtableCustomerStatusByAll.Rows[0]["Status"].ToString();
                        regobj.ContactNumber1 = dtableCustomerStatusByAll.Rows[0]["ContactNumber1"].ToString();
                        regobj.ContactNumber2 = dtableCustomerStatusByAll.Rows[0]["ContactNumber2"].ToString();
                        regobj.UserName = dtableCustomerStatusByAll.Rows[0]["UserName"].ToString();
                        regobj.UserType = dtableCustomerStatusByAll.Rows[0]["userType"].ToString();

                        customerStatus.Add(regobj);
                        ViewData["dtableAgentStatus"] = dtableCustomerStatusByAll;
                    }
                }
                if ((userInfo.Name != "") && (userInfo.WalletNumber != ""))
                {
                    DataTable dtableCustomerStatusByAc = AgentUtils.GetAgentProfileByNameAC(userInfo.Name, userInfo.WalletNumber);
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

                        customerStatus.Add(regobj);
                        ViewData["dtableAgentStatus"] = dtableCustomerStatusByAc;
                    }
                }
                if (userInfo.ContactNumber1 != "")
                {
                    DataTable dtableCustomerStatusByMobileNo = AgentUtils.GetAgentProfileByMobileNo(userInfo.ContactNumber1);
                    if (dtableCustomerStatusByMobileNo != null && dtableCustomerStatusByMobileNo.Rows.Count > 0)
                    {
                        UserInfo regobj = new UserInfo
                        {
                            ClientCode = dtableCustomerStatusByMobileNo.Rows[0]["ClientCode"].ToString(),
                            Name = dtableCustomerStatusByMobileNo.Rows[0]["Name"].ToString(),
                            Address = dtableCustomerStatusByMobileNo.Rows[0]["Address"].ToString(),
                            PIN = dtableCustomerStatusByMobileNo.Rows[0]["PIN"].ToString(),
                            Status = dtableCustomerStatusByMobileNo.Rows[0]["Status"].ToString(),
                            ContactNumber1 = dtableCustomerStatusByMobileNo.Rows[0]["ContactNumber1"].ToString(),
                            ContactNumber2 = dtableCustomerStatusByMobileNo.Rows[0]["ContactNumber2"].ToString(),
                            UserName = dtableCustomerStatusByMobileNo.Rows[0]["UserName"].ToString(),
                            UserType = dtableCustomerStatusByMobileNo.Rows[0]["userType"].ToString()
                        };

                        customerStatus.Add(regobj);
                        ViewData["dtableAgentStatus"] = dtableCustomerStatusByMobileNo;
                    }
                }
                if (userInfo.Name != "")
                {
                    DataTable dtableCustomerStatusByName = AgentUtils.GetAgentProfileByName(userInfo.Name);
                    if (dtableCustomerStatusByName != null && dtableCustomerStatusByName.Rows.Count > 0)
                    {
                        UserInfo regobj = new UserInfo
                        {
                            ClientCode = dtableCustomerStatusByName.Rows[0]["ClientCode"].ToString(),
                            Name = dtableCustomerStatusByName.Rows[0]["Name"].ToString(),
                            Address = dtableCustomerStatusByName.Rows[0]["Address"].ToString(),
                            PIN = dtableCustomerStatusByName.Rows[0]["PIN"].ToString(),
                            Status = dtableCustomerStatusByName.Rows[0]["Status"].ToString(),
                            ContactNumber1 = dtableCustomerStatusByName.Rows[0]["ContactNumber1"].ToString(),
                            ContactNumber2 = dtableCustomerStatusByName.Rows[0]["ContactNumber2"].ToString(),
                            UserName = dtableCustomerStatusByName.Rows[0]["UserName"].ToString(),
                            UserType = dtableCustomerStatusByName.Rows[0]["userType"].ToString()
                        };

                        customerStatus.Add(regobj);
                        ViewData["dtableAgentStatus"] = dtableCustomerStatusByName;
                    }
                }
                if (userInfo.WalletNumber != "")
                {
                    DataTable dtableCustomerStatusByAc = AgentUtils.GetAgentProfileByAC(userInfo.WalletNumber);
                    if (dtableCustomerStatusByAc != null && dtableCustomerStatusByAc.Rows.Count > 0)
                    {
                        UserInfo regobj = new UserInfo
                        {
                            ClientCode = dtableCustomerStatusByAc.Rows[0]["ClientCode"].ToString(),
                            Name = dtableCustomerStatusByAc.Rows[0]["Name"].ToString(),
                            Address = dtableCustomerStatusByAc.Rows[0]["Address"].ToString(),
                            PIN = dtableCustomerStatusByAc.Rows[0]["PIN"].ToString(),
                            Status = dtableCustomerStatusByAc.Rows[0]["Status"].ToString(),
                            ContactNumber1 = dtableCustomerStatusByAc.Rows[0]["ContactNumber1"].ToString(),
                            ContactNumber2 = dtableCustomerStatusByAc.Rows[0]["ContactNumber2"].ToString(),
                            UserName = dtableCustomerStatusByAc.Rows[0]["UserName"].ToString(),
                            UserType = dtableCustomerStatusByAc.Rows[0]["userType"].ToString()
                        };

                        customerStatus.Add(regobj);
                        ViewData["dtableAgentStatus"] = dtableCustomerStatusByAc;
                    }
                }

                return View(customerStatus);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        #endregion

    }
}
