using MNSuperadmin.Helper;
using MNSuperadmin.App_Start;
using MNSuperadmin.Models;
using MNSuperadmin.Settings;
using MNSuperadmin.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.SessionState;
using KellermanSoftware.CompareNetObjects;

namespace MNSuperadmin.Controllers
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
            //Check Role link start    
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkRole(System.Reflection.MethodBase.GetCurrentMethod().Name, clientCode);
            //Check Role link end


            if (TempData["userType"] != null&& checkRole)
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
        public async Task<ActionResult> CreateRegistration(FormCollection collection, UserInfo model)
        {

            UserInfo userInfo = new UserInfo();
            try
            {
                string userName = (string)Session["LOGGED_USERNAME"];
                string clientCode = (string)Session["LOGGEDUSER_ID"];
                string name = (string)Session["LOGGEDUSER_NAME"];
                string userType = (string)Session["LOGGED_USERTYPE"];

                string tid;
                TraceIdGenerator traceid = new TraceIdGenerator();
                tid = traceid.GenerateUniqueTraceID();
                ViewBag.retrievalReference = tid;

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


                    //userInfo.UserName = collection["txtUserName"].ToString();
                    //userInfo.ContactNumber1 = collection["txtContactNumber1"].ToString();
                    userInfo.FName = collection["txtFirstName"].ToString();
                    userInfo.MName = collection["txtMiddleName"].ToString();
                    userInfo.LName = collection["txtLastName"].ToString();
                    userInfo.Name = userInfo.FName + " " + userInfo.MName + " " + userInfo.LName;
                    userInfo.Gender = collection["txtGender"].ToString();
                    userInfo.DOB = collection["DOB"].ToString();
                    userInfo.BSDateOfBirth = collection["BSDateOfBirth"].ToString();
                    userInfo.EmailAddress = collection["Email"].ToString();

                    userInfo.Nationality = collection["txtNationality"].ToString();
                    userInfo.Country = collection["txtCountry"].ToString();
                    userInfo.FatherName = collection["txtFatherName"].ToString();
                    userInfo.MotherName = collection["txtMotherName"].ToString();
                    userInfo.MaritalStatus = collection["MaritalStatus"].ToString();
                    if (collection.AllKeys.Contains("SpouseName"))
                    {
                        userInfo.SpouseName = collection["SpouseName"].ToString();
                    }
                    else
                    {
                        userInfo.SpouseName = "";
                    }

                    if (collection.AllKeys.Contains("FatherInLaw"))
                    {
                        userInfo.FatherInLaw = collection["FatherInLaw"].ToString();
                    }
                    else
                    {
                        userInfo.FatherInLaw = "";
                    }


                    userInfo.GrandFatherName = collection["txtGrandFatherName"].ToString();
                    //userInfo.FatherInlawName = collection["txtFatherInlawName"].ToString();
                    userInfo.Occupation = collection["txtOccupation"].ToString();

                    //userInfo.EmailAddress = collection["txtEmail"].ToString();
                    userInfo.PanNo = collection["txtPanNo"].ToString();

                    //userInfo.Photo1 = collection["txtPhoto1"].ToString();
                    //userInfo.Country = collection["txtCountry"].ToString();
                    //userInfo.ContactNumber2 = collection["txtContactNumber2"].ToString();

                    userInfo.PProvince = collection["PProvinceText"].ToString();
                    userInfo.PDistrict = collection["PDistrictText"].ToString();
                    userInfo.PVDC = collection["txtPVDC"].ToString();
                    userInfo.PHouseNo = collection["txtPHouseNo"].ToString();
                    userInfo.PWardNo = collection["txtPWardNo"].ToString();
                    userInfo.PStreet = collection["PStreet"].ToString();
                    userInfo.Address = userInfo.PProvince + "," + userInfo.PDistrict + "," + userInfo.PVDC + "," + userInfo.PWardNo + "," + userInfo.PHouseNo + "," + userInfo.PStreet;

                    userInfo.CProvince = collection["CProvinceText"].ToString();
                    userInfo.CDistrict = collection["CDistrictText"].ToString();
                    userInfo.CVDC = collection["txtCVDC"].ToString();
                    userInfo.CHouseNo = collection["txtCHouseNo"].ToString();
                    userInfo.CWardNo = collection["txtCWardNo"].ToString();
                    userInfo.CStreet = collection["CStreet"].ToString();
                    userInfo.retrievalReference = ViewBag.retrievalReference;

                    //for citizenship 
                    if (collection.AllKeys.Contains("txtCitizenship"))
                    {
                        userInfo.Citizenship = collection["txtCitizenship"].ToString();
                    }
                    else
                    {
                        userInfo.Citizenship = "";
                    }

                    if (!string.IsNullOrEmpty(collection["CitizenshipIssueDate"].ToString()))
                    {
                        userInfo.CitizenshipIssueDate = collection["CitizenshipIssueDate"].ToString();
                    }
                    else
                    {
                        userInfo.CitizenshipIssueDate = "01/01/2006";
                    }


                    if (!string.IsNullOrEmpty(collection["BSCitizenshipIssueDate"].ToString()))
                    {
                        userInfo.BSCitizenshipIssueDate = collection["BSCitizenshipIssueDate"].ToString();
                    }
                    else
                    {
                        userInfo.BSCitizenshipIssueDate = "2006-01-01";
                    }


                    if (collection.AllKeys.Contains("txtCitizenshipPlaceOfIssue"))
                    {
                        userInfo.CitizenshipPlaceOfIssue = collection["txtCitizenshipPlaceOfIssue"].ToString();
                    }
                    else
                    {
                        userInfo.CitizenshipPlaceOfIssue = "";
                    }

                    //for license
                    if (collection.AllKeys.Contains("txtLicense"))
                    {
                        userInfo.License = collection["txtLicense"].ToString();
                    }
                    else
                    {
                        userInfo.License = "";
                    }
                    if (!string.IsNullOrEmpty(collection["LicenseIssueDate"].ToString()))
                    {
                        userInfo.LicenseIssueDate = collection["LicenseIssueDate"].ToString();
                    }
                    else
                    {
                        userInfo.LicenseIssueDate = "01/01/2006";
                    }


                    if (!string.IsNullOrEmpty(collection["BSLicenseIssueDate"].ToString()))
                    {
                        userInfo.BSLicenseIssueDate = collection["BSLicenseIssueDate"].ToString();
                    }
                    else
                    {
                        userInfo.BSLicenseIssueDate = "2006-01-01";
                    }

                    if (!string.IsNullOrEmpty(collection["LicenseExpireDate"].ToString()))
                    {
                        userInfo.LicenseExpireDate = collection["LicenseExpireDate"].ToString();
                    }
                    else
                    {
                        userInfo.LicenseExpireDate = "01/01/2006";
                    }

                    if (!string.IsNullOrEmpty(collection["BSLicenseExpireDate"].ToString()))
                    {
                        userInfo.BSLicenseExpireDate = collection["BSLicenseExpireDate"].ToString();
                    }
                    else
                    {
                        userInfo.BSLicenseExpireDate = "2006-01-01";
                    }

                    if (collection.AllKeys.Contains("txtLicensePlaceOfIssue"))
                    {
                        userInfo.LicensePlaceOfIssue = collection["txtLicensePlaceOfIssue"].ToString();
                    }
                    else
                    {
                        userInfo.LicensePlaceOfIssue = "";
                    }

                    //if (collection.AllKeys.Contains("LicenseIssueDate"))
                    //{

                    //    userInfo.License = collection["txtLicense"].ToString();
                    //}
                    //else
                    //{
                    //    userInfo.License = "";
                    //}

                    //if (!string.IsNullOrEmpty(userInfo.LicenseIssueDate))
                    //{
                    //    userInfo.LicenseIssueDate =DateTime.ParseExact(userInfo.LicenseIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                    //                           .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    //}
                    //else
                    //{
                    //    userInfo.LicenseIssueDate = "01/01/1900";
                    //}

                    //if (!string.IsNullOrEmpty(userInfo.LicenseExpireDate))
                    //{
                    //    userInfo.LicenseExpireDate=DateTime.ParseExact(userInfo.LicenseExpireDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                    //                           .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    //}
                    //else
                    //{
                    //    userInfo.LicenseExpireDate = "01/01/1900";
                    //}

                    //if (collection.AllKeys.Contains("LicenseIssueDate"))
                    //{
                    //    userInfo.LicensePlaceOfIssue = collection["txtLicensePlaceOfIssue"].ToString();
                    //}
                    //else
                    //{
                    //    userInfo.LicensePlaceOfIssue = "";
                    //}



                    //for license
                    if (collection.AllKeys.Contains("txtPassport"))
                    {
                        userInfo.Passport = collection["txtPassport"].ToString();
                    }
                    else
                    {
                        userInfo.Passport = "";
                    }

                    if (!string.IsNullOrEmpty(collection["PassportIssueDate"].ToString()))
                    {
                        userInfo.PassportIssueDate = collection["PassportIssueDate"].ToString();
                    }
                    else
                    {
                        userInfo.PassportIssueDate = "01/01/2006";
                    }

                    if (!string.IsNullOrEmpty(collection["PassportExpireDate"].ToString()))
                    {
                        userInfo.PassportExpireDate = collection["PassportExpireDate"].ToString();
                    }
                    else
                    {
                        userInfo.PassportExpireDate = "01/01/2006";
                    }


                    if (collection.AllKeys.Contains("txtPassportPlaceOfIssue"))
                    {
                        userInfo.PassportPlaceOfIssue = collection["txtPassportPlaceOfIssue"].ToString();
                    }
                    else
                    {
                        userInfo.PassportPlaceOfIssue = "";
                    }








                    //if (collection.AllKeys.Contains("PassportIssueDate"))
                    //{

                    //    userInfo.Passport = collection["txtPassport"].ToString();
                    //}
                    //else
                    //{
                    //    userInfo.Passport = "";
                    //}

                    //if (!string.IsNullOrEmpty(userInfo.PassportIssueDate))
                    //{
                    //    userInfo.PassportIssueDate = DateTime.ParseExact(userInfo.PassportIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                    //                           .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    //}
                    //else
                    //{
                    //    userInfo.PassportIssueDate = "01/01/1900";
                    //}

                    //if (!string.IsNullOrEmpty(userInfo.PassportExpireDate))
                    //{
                    //    userInfo.PassportExpireDate=DateTime.ParseExact(userInfo.PassportExpireDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                    //                           .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    //}
                    //else
                    //{
                    //    userInfo.PassportExpireDate = "01/01/1900";
                    //}

                    //if (collection.AllKeys.Contains("PassportIssueDate"))
                    //{
                    //    userInfo.PassportPlaceOfIssue = collection["txtPassportPlaceOfIssue"].ToString();
                    //}
                    //else
                    //{
                    //    userInfo.PassportPlaceOfIssue = "";
                    //}

                    userInfo.PassportImage = ParseCv(model.PassportPhoto);
                    userInfo.FrontImage = ParseCv(model.Front);
                    userInfo.BackImage = ParseCv(model.Back);
                    userInfo.Document = collection["txtDocument"].ToString();
                    userInfo.UserName = collection["txtUserName"].ToString();
                    userInfo.UserType = "agent";
                    userInfo.Status = "InActive";
                    userInfo.IsApproved = "UnApprove";
                    userInfo.IsRejected = "F";
                    string mobile = userInfo.UserName;
                    userInfo.CreatedBy = userName;



                    //userInfo.BankNo = collection["BankNoBin"];
                    //userInfo.BankAccountNumber = collection["txtBankAccountNumber"].ToString();
                    //userInfo.BranchCode = string.Empty;

                    //if (userInfo.BankAccountNumber != "")
                    //    userInfo.BranchCode = userInfo.BankAccountNumber.Substring(0, 3);

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
                    //ViewBag.Bank = item.Where(x => x.Value != "0000").ToList();

                    //string bankCode = item[0].Value.ToString();

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


                    if ((userInfo.UserName != ""))
                    {
                        DataTable dtableMobileNo = RegisterUtils.GetCheckMobileNo(userInfo.UserName);
                        if (dtableMobileNo.Rows.Count == 0)
                        {
                            if (collection.AllKeys.Any())
                            {
                                try
                                {
                                    var PP = ReturnFileName(model.PassportPhoto, mobile);
                                    var front = ReturnFileName(model.Front, mobile);
                                    var back = ReturnFileName(model.Back, mobile);

                                    userInfo.PassportImageName = string.Format(PP);
                                    userInfo.FrontImageName = string.Format(front);
                                    userInfo.BackImageName = string.Format(back);
                                    await SavePhoto(userInfo);

                                    userInfo.PassportImageName = Session["PP"].ToString();
                                    userInfo.FrontImageName = Session["Front"].ToString();
                                    userInfo.BackImageName = Session["Back"].ToString();

                                    //var PP = SaveAndReturnFileName(model.PassportPhoto, mobile);
                                    //var front = SaveAndReturnFileName(model.Front, mobile);
                                    //var back = SaveAndReturnFileName(model.Back, mobile);

                                    //userInfo.PassportImage = string.Format(PP);
                                    //userInfo.FrontImage = string.Format(front);
                                    //userInfo.BackImage = string.Format(back);

                                    //userInfo.PassportImage = string.Format("~/Content/Upload/{0}", PP);
                                    //userInfo.FrontImage = string.Format("~/Content/Upload/{0}", front);
                                    //userInfo.BackImage = string.Format("~/Content/Upload/{0}", back);

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


                                        //for sms


                                        //TempData["login_message"] = "Registration information is successfully added.Please wait for approval";
                                        //TempData["message_class"] = CssSetting.SuccessMessageClass;
                                        //SMS
                                        SMSUtils SMS = new SMSUtils();

                                        string Message = string.Format("Dear " + userInfo.FName + ",\n Your KYC request has been queued for the approval. You'll be notified shortly" + " .\n Thank You -MNepal");

                                        SMSLog Log = new SMSLog();

                                        Log.UserName = userInfo.UserName;
                                        Log.Purpose = "KYC"; //New Registration
                                        Log.SentBy = "Superadmin";
                                        Log.Message = "Your KYC request has been queued for the approval. You'll be notified shortly.";
                                        //Log SMS
                                        CustomerUtils.LogSMS(Log);
                                        SMS.SendSMS(Message, userInfo.UserName);
                                        int ret = MerchantUtils.InsertResponseQuickSelfReg(userInfo.UserName, tid, "200", "New Agent Registration");
                                        //return RedirectToAction("CustomerKYCFinish");


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
                            string checkMobileNo = dtableMobileNo.Rows[0]["UserName"].ToString();
                            result = false;
                            errorMessage = "Already Register username";
                        }
                    }
                    else
                    {
                        result = false;
                    }

                    this.ViewData["registration_message"] = result
                                                  ? "Registration information is successfully added. Please Check Approve Agent Registration  and perform accordingly."
                                                  : "Error while inserting the information. ERROR :: "
                                                    + errorMessage;
                    this.ViewData["message_class"] = result ? "success_info" : "failed_info";

                    //For Bank
                    if (result)
                    {
                        TempData["registration_message"] = ViewData["registration_message"];

                        TempData["message_class"] = ViewData["message_class"];
                        return this.RedirectToAction("CreateRegistration");
                    }


                    return View(userInfo);
                }
                else
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            catch (Exception ex)
            {
                return View(userInfo);
            }
        }

        public string SaveAndReturnFileName(HttpPostedFileBase file, string mobile)
        {

            if (file == null)
                return null;

            if (IsImage(file) == false)
                return null;

            int iFileSize = file.ContentLength;
            if (iFileSize > 1048576)
                return null;

            WebImage img = new WebImage(file.InputStream);
            img.Resize(440, 300, true, true);
            var fileName = System.IO.Path.GetFileName(file.FileName);
            string randomFileName = mobile + "_" + Path.GetFileNameWithoutExtension(fileName) +
                                "_" + Guid.NewGuid().ToString().Substring(0, 4) + Path.GetExtension(fileName);
            var path = Path.Combine(Server.MapPath("~/Content/Upload/"), randomFileName);
            img.Save(path);
            return randomFileName;
        }

        private bool IsImage(HttpPostedFileBase file)
        {
            //Checks for image type... you could also do filename extension checks and other things
            return ((file != null) && System.Text.RegularExpressions.Regex.IsMatch(file.ContentType, "image/\\S+") && (file.ContentLength > 0));
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
            //Check Role link start
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkIndexRole(System.Reflection.MethodBase.GetCurrentMethod().Name, clientCode, this.ControllerContext.RouteData.Values["controller"].ToString());
            //Check Role link end

            if (this.TempData["agentmodify_messsage"] != null)
            {
                this.ViewData["agentmodify_messsage"] = this.TempData["agentmodify_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            TempData["userType"] = userType;

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
                if ((userInfo.ContactNumber1 != "") && (SearchCol == "Mobile Number")) //UserName = ContactNumber1
                {
                    DataTable dtableCustomerStatusByMobileNo = AgentUtils.GetAgentProfileByMobileNo(userInfo.ContactNumber1);
                    if (dtableCustomerStatusByMobileNo != null && dtableCustomerStatusByMobileNo.Rows.Count > 0)
                    {
                        UserInfo regobj = new UserInfo
                        {
                            ClientCode = dtableCustomerStatusByMobileNo.Rows[0]["ClientCode"].ToString(),
                            Name = dtableCustomerStatusByMobileNo.Rows[0]["Name"].ToString(),
                            Address = dtableCustomerStatusByMobileNo.Rows[0]["Address"].ToString(),
                            //PIN = dtableCustomerStatusByMobileNo.Rows[0]["PIN"].ToString(),
                            Status = dtableCustomerStatusByMobileNo.Rows[0]["Status"].ToString(),
                            //ContactNumber1 = dtableCustomerStatusByMobileNo.Rows[0]["ContactNumber1"].ToString(),
                            //ContactNumber2 = dtableCustomerStatusByMobileNo.Rows[0]["ContactNumber2"].ToString(),
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
                DataSet DSet = ProfileUtils.GetAgentProfileInfoDS(id);

                DataTable dtableUserInfo = DSet.Tables["dtUserInfo"];
                DataTable dKycInfo = DSet.Tables["dtKycInfo"];
                DataTable dKycDoc = DSet.Tables["dtKycDoc"];

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
                //ViewBag.Wallet = item;
                // ViewBag.Bank = item;


                if (dtableUserInfo != null && dtableUserInfo.Rows.Count > 0)
                {
                    regobj.ClientCode = dtableUserInfo.Rows[0]["ClientCode"].ToString();
                    regobj.Name = dtableUserInfo.Rows[0]["Name"].ToString();
                    regobj.Address = dtableUserInfo.Rows[0]["Address"].ToString();

                    regobj.PProvince = dtableUserInfo.Rows[0]["PProvince"].ToString();
                    regobj.PDistrict = dtableUserInfo.Rows[0]["PDistrict"].ToString();
                    regobj.PMunicipalityVDC = dtableUserInfo.Rows[0]["PMunicipalityVDC"].ToString();
                    regobj.PWardNo = dtableUserInfo.Rows[0]["PWardNo"].ToString();
                    regobj.PHouseNo = dtableUserInfo.Rows[0]["PHouseNo"].ToString();
                    regobj.PStreet = dtableUserInfo.Rows[0]["PStreet"].ToString();

                    regobj.CProvince = dtableUserInfo.Rows[0]["CProvince"].ToString();
                    regobj.CDistrict = dtableUserInfo.Rows[0]["CDistrict"].ToString();
                    regobj.CMunicipalityVDC = dtableUserInfo.Rows[0]["CMunicipalityVDC"].ToString();
                    regobj.CWardNo = dtableUserInfo.Rows[0]["CWardNo"].ToString();
                    regobj.CHouseNo = dtableUserInfo.Rows[0]["CHouseNo"].ToString();
                    regobj.CStreet = dtableUserInfo.Rows[0]["CStreet"].ToString();



                    regobj.IsApproved = dtableUserInfo.Rows[0]["IsApproved"].ToString();
                    regobj.IsRejected = dtableUserInfo.Rows[0]["IsRejected"].ToString();
                    regobj.IsModified = dtableUserInfo.Rows[0]["IsModified"].ToString();

                    regobj.Status = dtableUserInfo.Rows[0]["Status"].ToString();
                    regobj.UserName = dtableUserInfo.Rows[0]["UserName"].ToString();
                    regobj.UserType = dtableUserInfo.Rows[0]["UserType"].ToString();
                    regobj.PIN = dtableUserInfo.Rows[0]["PIN"].ToString();
                    regobj.WalletNumber = dtableUserInfo.Rows[0]["WalletNumber"].ToString();

                    regobj.FName = dtableUserInfo.Rows[0]["FName"].ToString();
                    regobj.MName = dtableUserInfo.Rows[0]["MName"].ToString();
                    regobj.LName = dtableUserInfo.Rows[0]["LName"].ToString();
                    regobj.Gender = dtableUserInfo.Rows[0]["Gender"].ToString();
                    regobj.DOB = dtableUserInfo.Rows[0]["DateOfBirth"].ToString();
                    //regobj.DOB = DateTime.Parse(regobj.DOB.Trim()).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    regobj.DOB = DateTime.Parse(regobj.DOB.Trim()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                    regobj.BSDateOfBirth = dtableUserInfo.Rows[0]["BSDateOfBirth"].ToString();
                    
                    regobj.EmailAddress = dtableUserInfo.Rows[0]["EmailAddress"].ToString();
                    regobj.Nationality = dtableUserInfo.Rows[0]["Nationality"].ToString();
                    regobj.Country = dtableUserInfo.Rows[0]["Country"].ToString();
                    regobj.Occupation = dtableUserInfo.Rows[0]["Occupation"].ToString();
                    regobj.MaritalStatus = dtableUserInfo.Rows[0]["MaritalStatus"].ToString();
                    regobj.SpouseName = dtableUserInfo.Rows[0]["SpouseName"].ToString();

                    regobj.FatherInLaw = dtableUserInfo.Rows[0]["FatherInLaw"].ToString();

                    regobj.FatherName = dtableUserInfo.Rows[0]["FathersName"].ToString();
                    regobj.MotherName = dtableUserInfo.Rows[0]["MothersName"].ToString();
                    regobj.GrandFatherName = dtableUserInfo.Rows[0]["GFathersName"].ToString();
                    regobj.PanNo = dtableUserInfo.Rows[0]["PANNumber"].ToString();


                    regobj.Document = dtableUserInfo.Rows[0]["DocType"].ToString();
                    regobj.Citizenship = dtableUserInfo.Rows[0]["CitizenshipNo"].ToString();
                    regobj.CitizenshipIssueDate = dtableUserInfo.Rows[0]["CitizenIssueDate"].ToString().Split()[0];
                    //regobj.CitizenshipIssueDate = DateTime.Parse(regobj.CitizenshipIssueDate.Trim()).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    regobj.CitizenshipIssueDate = DateTime.Parse(regobj.CitizenshipIssueDate.Trim()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

                    regobj.BSCitizenshipIssueDate = dtableUserInfo.Rows[0]["BSCitizenIssueDate"].ToString().Split()[0];
                    regobj.CitizenshipPlaceOfIssue = dtableUserInfo.Rows[0]["CitizenPlaceOfIssue"].ToString();

                    regobj.License = dtableUserInfo.Rows[0]["LicenseNo"].ToString();
                    regobj.LicenseIssueDate = dtableUserInfo.Rows[0]["LicenseIssueDate"].ToString().Split()[0];
                    //regobj.LicenseIssueDate = DateTime.Parse(regobj.LicenseIssueDate.Trim()).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    regobj.LicenseIssueDate = DateTime.Parse(regobj.LicenseIssueDate.Trim()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

                    regobj.BSLicenseIssueDate = dtableUserInfo.Rows[0]["BSLicenseIssueDate"].ToString().Split()[0];
                    regobj.LicenseExpireDate = dtableUserInfo.Rows[0]["LicenseExpiryDate"].ToString().Split()[0];
                    //regobj.LicenseExpireDate = DateTime.Parse(regobj.LicenseExpireDate.Trim()).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    regobj.LicenseExpireDate = DateTime.Parse(regobj.LicenseExpireDate.Trim()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

                    regobj.BSLicenseExpireDate = dtableUserInfo.Rows[0]["BSLicenseExpiryDate"].ToString().Split()[0];

                    regobj.LicensePlaceOfIssue = dtableUserInfo.Rows[0]["LicensePlaceOfIssue"].ToString();

                    regobj.Passport = dtableUserInfo.Rows[0]["PassportNo"].ToString();
                    regobj.PassportPlaceOfIssue = dtableUserInfo.Rows[0]["PassportPlaceOfIssue"].ToString();
                    regobj.PassportIssueDate = dtableUserInfo.Rows[0]["PassportIssueDate"].ToString().Split()[0];
                 //  regobj.PassportIssueDate = DateTime.Parse(regobj.PassportIssueDate.Trim()).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    regobj.PassportIssueDate = DateTime.Parse(regobj.PassportIssueDate.Trim()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

                    regobj.PassportExpireDate = dtableUserInfo.Rows[0]["PassportExpiryDate"].ToString().Split()[0];
                    //regobj.PassportExpireDate = DateTime.Parse(regobj.PassportExpireDate.Trim()).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    regobj.PassportExpireDate = DateTime.Parse(regobj.PassportExpireDate.Trim()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

                    regobj.PassportImage = dtableUserInfo.Rows[0]["PassportImage"].ToString();
                    regobj.FrontImage = dtableUserInfo.Rows[0]["FrontImage"].ToString();
                    regobj.BackImage = dtableUserInfo.Rows[0]["BackImage"].ToString();




                    //regobj.ContactNumber1 = dtableUserInfo.Rows[0]["ContactNumber1"].ToString();
                    //regobj.ContactNumber2 = dtableUserInfo.Rows[0]["ContactNumber2"].ToString();                  
                    //regobj.EmailAddress= dtableUserInfo.Rows[0]["EmailAddress"].ToString();                                        
                    //regobj.BankNo = dtableUserInfo.Rows[0]["BankNo"].ToString();
                    //regobj.BranchCode = dtableUserInfo.Rows[0]["BranchCode"].ToString();
                    //regobj.BankAccountNumber = dtableUserInfo.Rows[0]["BankAccountNumber"].ToString();

                }
                else
                {
                    this.TempData["custmodify_messsage"] = "User not found";
                    this.TempData["message_class"] = CssSetting.FailedMessageClass;
                    return RedirectToAction("Index");

                }

                //ViewBag.Bank = new SelectList(item,"Value","Text" ,regobj.BankNo);

                // start milayako 02
                ViewBag.PProvince = new SelectList(list, "Value", "Text", regobj.PProvince);
                ViewBag.CProvince = new SelectList(list, "Value", "Text", regobj.CProvince);


                //end milayako 02

                // start milayako 02
                ViewBag.PDistrictID = new SelectList(ProvinceToDistrict(regobj.PProvince), "Value", "Text", regobj.PDistrict);
                ViewBag.CDistrictID = new SelectList(ProvinceToDistrict(regobj.CProvince), "Value", "Text", regobj.CDistrict);

              //  ViewBag.PDistrictID = new SelectList(ProvinceToDistrict(regobj.PProvince), "Value", "Text", regobj.PDistrictID);
                //ViewBag.CDistrictID = new SelectList(ProvinceToDistrict(regobj.CProvince), "Value", "Text", regobj.CDistrictID);
                // END milayako 02
                return View(regobj);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        //Gets the Province ID and returns corresponding District.
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
        // POST: Agent/AgentModify/5
        [HttpPost]
        public async Task<ActionResult> AgentModify(string id, string btnCommand, FormCollection collection, UserInfo model)
        {
            string Id = "";
            string displayMessage = "";
            string messageClass = "";
            UserInfo userInfo = new UserInfo();
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
                        //string cAddress = collection["Address"].ToString();
                        //string cName = collection["Name"].ToString();
                        string cWalletNumber = collection["WalletNumber"].ToString();
                        //string cIsApproved = collection["IsApproved"].ToString();
                        string cUserName = collection["txtUserName"].ToString();
                        string cFName = collection["txtFirstName"].ToString();
                        string cMName = collection["txtMiddleName"].ToString();
                        string cLName = collection["txtLastName"].ToString();
                        string cGender = collection["Gender"].ToString();
                        string cDOB = collection["DOB"].ToString();
                        string cBSDateOfBirth = collection["BSDateOfBirth"].ToString();
                        string cEmailAddress = collection["Email"].ToString();
                        string cNationality = collection["Nationality"].ToString();
                        string cCountry = collection["Country"].ToString();
                        string cFatherName = collection["txtFatherName"].ToString();
                        string cMotherName = collection["txtMotherName"].ToString();
                        string cMaritalStatus = collection["MaritalStatus"].ToString();
                        string cSpouseName = collection["SpouseName"].ToString();
                        string cFatherInLaw = collection["FatherInLaw"].ToString();

                        string cGrandFatherName = collection["txtGrandFatherName"].ToString();
                        string cOccupation = collection["Occupation"].ToString();
                        string cPanNo = collection["txtPanNo"].ToString();

                        string cPProvince = collection["PProvinceText"].ToString();
                        string cPDistrict = collection["PDistrictText"].ToString();
                        string cPVDC = collection["txtPVDC"].ToString();
                        string cPHouseNo = collection["txtPHouseNo"].ToString();
                        string cPWardNo = collection["txtPWardNo"].ToString();
                        string cPStreet = collection["PStreet"].ToString();

                        string cCProvince = collection["CProvinceText"].ToString();
                        string cCDistrict = collection["CDistrictText"].ToString();
                        string cCVDC = collection["txtCVDC"].ToString();
                        string cCHouseNo = collection["txtCHouseNo"].ToString();
                        string cCWardNo = collection["txtCWardNo"].ToString();
                        string cCStreet = collection["CStreet"].ToString();

                        string cDocument = collection["Document"].ToString();
                        string cCitizenship = collection["txtCitizenship"].ToString();
                        string cCitizenshipIssueDate = collection["CitizenshipIssueDate"].ToString();
                        string cBSCitizenshipIssueDate = collection["BSCitizenshipIssueDate"].ToString();
                        string cCitizenshipPlaceOfIssue = collection["txtCitizenshipPlaceOfIssue"].ToString();


                        string cLicense = collection["txtLicense"].ToString();
                        string cLicenseIssueDate = collection["LicenseIssueDate"].ToString();
                        string cBSLicenseIssueDate = collection["BSLicenseIssueDate"].ToString();
                        string cLicenseExpireDate = collection["LicenseExpireDate"].ToString();
                        string cBSLicenseExpireDate = collection["BSLicenseExpireDate"].ToString();
                        string cLicensePlaceOfIssue = collection["txtLicensePlaceOfIssue"].ToString();

                        string cPassport = collection["txtPassport"].ToString();
                        string cPassportIssueDate = collection["txtPassportIssueDate"].ToString();
                        string cPassportExpireDate = collection["txtPassportExpireDate"].ToString();
                        string cPassportPlaceOfIssue = collection["txtPassportPlaceOfIssue"].ToString();

                        //Images OLD Value//
                        string cPassportPhoto = collection["txtOldPP"].ToString();
                        string cFrontPhoto = collection["txtOldfront"].ToString();
                        string cBackPhoto = collection["txtOldback"].ToString();
                        userInfoModify.ClientCode = cClientCode;
  
                        userInfoModify.Name = cFName + " " + cMName + " " + cLName;
                        userInfoModify.WalletNumber = cWalletNumber;
                        userInfoModify.UserType = "agent";
                        userInfoModify.Status = "InActive";
                        userInfoModify.IsApproved = "UnApprove";
                        userInfoModify.IsRejected = "F";
                        userInfoModify.IsModified = "T";
                        userInfoModify.UserName = cUserName;
                        userInfoModify.Address = cPVDC +  "," + getDistrictName(cPDistrict) + ", Province " + cPProvince;   /*+ cPWardNo + "," + cPHouseNo + "," + cPStreet*/
                        userInfoModify.FName = cFName;
                        userInfoModify.MName = cMName;
                        userInfoModify.LName = cLName;
                        userInfoModify.Gender = cGender;
                        userInfoModify.DOB = cDOB;
                        userInfoModify.BSDateOfBirth = cBSDateOfBirth;
                        userInfoModify.EmailAddress = cEmailAddress;
                        userInfoModify.Nationality = cNationality;
                        userInfoModify.Country = cCountry;
                        userInfoModify.Occupation = cOccupation;
                        userInfoModify.MaritalStatus = cMaritalStatus;
                        userInfoModify.SpouseName = cSpouseName;
                        userInfoModify.FatherInLaw = cFatherInLaw;
                        userInfoModify.PanNo = cPanNo;
                        userInfoModify.FatherName = cFatherName;
                        userInfoModify.MotherName = cMotherName;
                        userInfoModify.GrandFatherName = cGrandFatherName;



                        userInfoModify.PProvince = cPProvince;
                        userInfoModify.PDistrict = cPDistrict;
                        userInfoModify.PVDC = cPVDC;
                        userInfoModify.PWardNo = cPWardNo;
                        userInfoModify.PHouseNo = cPHouseNo;
                        userInfoModify.PStreet = cPStreet;

                        userInfoModify.CProvince = cCProvince;
                        userInfoModify.CDistrict = cCDistrict;
                        userInfoModify.CVDC = cCVDC;
                        userInfoModify.CWardNo = cCWardNo;
                        userInfoModify.CHouseNo = cCHouseNo;
                        userInfoModify.CStreet = cCStreet;

                        userInfoModify.Document = cDocument;
                        userInfoModify.Citizenship = cCitizenship;
                        userInfoModify.CitizenshipIssueDate = cCitizenshipIssueDate;
                        userInfoModify.BSCitizenshipIssueDate = cBSCitizenshipIssueDate;
                        userInfoModify.CitizenshipPlaceOfIssue = cCitizenshipPlaceOfIssue;

                        userInfoModify.License = cLicense;
                        userInfoModify.LicenseIssueDate = cLicenseIssueDate;
                        userInfoModify.BSLicenseIssueDate = cBSLicenseIssueDate;
                        userInfoModify.LicenseExpireDate = cLicenseExpireDate;
                        userInfoModify.BSLicenseExpireDate = cBSLicenseExpireDate;
                        userInfoModify.LicensePlaceOfIssue = cLicensePlaceOfIssue;

                        userInfoModify.Passport = cPassport;
                        userInfoModify.PassportIssueDate = cPassportIssueDate;
                        userInfoModify.PassportExpireDate = cPassportExpireDate;
                        userInfoModify.PassportPlaceOfIssue = cPassportPlaceOfIssue;

                        if (model.PassportPhoto != null)
                        {
                            if (!string.IsNullOrEmpty(cPassportPhoto))
                            {
                                //var existingFile = Path.Combine(cPassportPhoto);
                                var existingFile = Request.MapPath(cPassportPhoto);
                                if (System.IO.File.Exists(existingFile))
                                {
                                    System.IO.File.Delete(existingFile);
                                }
                            }
                            var PP = ReturnFileName(model.PassportPhoto, userInfoModify.UserName);
                            userInfoModify.PassportImage = ParseCv(model.PassportPhoto);
                            userInfoModify.PassportImageName = string.Format(PP);
                        }
                        else
                        { 
                            userInfoModify.PassportImageName = "";
                        }


                        if (model.Front != null)
                        {
                            if (!string.IsNullOrEmpty(cFrontPhoto))
                            {

                                var existingFrontFile = Request.MapPath(cFrontPhoto);
                                if (System.IO.File.Exists(existingFrontFile))
                                {
                                    System.IO.File.Delete(existingFrontFile);
                                }
                            }
                            var front = ReturnFileName(model.Front, userInfoModify.UserName);
                            userInfoModify.FrontImage = ParseCv(model.Front);
                            userInfoModify.FrontImageName = string.Format(front);
                        }
                        else
                        {
                            userInfoModify.FrontImageName = "";
                        }


                        if (model.Back != null)
                        {
                            if (!string.IsNullOrEmpty(cBackPhoto))
                            {

                                var existingBackFile = Request.MapPath(cBackPhoto);
                                if (System.IO.File.Exists(existingBackFile))
                                {
                                    System.IO.File.Delete(existingBackFile);
                                }
                            }
                            var back = ReturnFileName(model.Back, userInfoModify.UserName);
                            userInfoModify.BackImage = ParseCv(model.Back);
                            userInfoModify.BackImageName = string.Format(back);
                            
                        }
                        else { 
                            userInfoModify.BackImageName = "";
                        }

                        await SavePhoto(userInfoModify);
                        string sPP = Session["PP"].ToString();
                        string sFront = Session["Front"].ToString();
                        string sBack = Session["Back"].ToString();

                        if (sPP != null && sPP != "")
                        {
                            userInfoModify.PassportImageName = Session["PP"].ToString();
                        }
                        else
                        {
                            userInfoModify.PassportImageName = cPassportPhoto;
                        }
                        if (sFront != null && sFront != "")
                        {
                            userInfoModify.FrontImageName = Session["Front"].ToString();
                        }
                        else
                        {
                            userInfoModify.FrontImageName = cFrontPhoto;
                        }
                        if (sBack != null && sBack != "")
                        {
                            userInfoModify.BackImageName = Session["Back"].ToString();
                        }
                        else
                        {
                            userInfoModify.BackImageName = cBackPhoto;
                        }

                        /*map new data to Agent*/
                        AgentTable NewAgent = new AgentTable();
                        MNClient NewMNClient = new MNClient();
                        MNClientContact NewMNClientContact = new MNClientContact();
                        MNClientKYC NewMNClientKYC = new MNClientKYC();
                        MNClientKYCDoc NewMNClientKYCDoc = new MNClientKYCDoc();
                        
                        NewMNClient.Address = userInfoModify.Address;
                        NewMNClient.Name = userInfoModify.Name;

                        NewMNClientContact.EmailAddress = userInfoModify.EmailAddress;
                        NewMNClientContact.ContactNumber1 = userInfoModify.ContactNumber1;
                        
                        NewMNClientKYC.FName = userInfoModify.FName;
                        NewMNClientKYC.MName = userInfoModify.MName;
                        NewMNClientKYC.LName = userInfoModify.LName;
                        NewMNClientKYC.Gender = userInfoModify.Gender;
                        NewMNClientKYC.BSDateOfBirth = userInfoModify.BSDateOfBirth;
                        NewMNClientKYC.Nationality = userInfoModify.Nationality;
                        NewMNClientKYC.Country = userInfoModify.Country;
                        NewMNClientKYC.Occupation = userInfoModify.Occupation;
                        NewMNClientKYC.MaritalStatus = userInfoModify.MaritalStatus;
                        NewMNClientKYC.SpouseName = userInfoModify.SpouseName;
                        NewMNClientKYC.FatherInLaw = userInfoModify.FatherInLaw;
                        NewMNClientKYC.FathersName = userInfoModify.FatherName;
                        NewMNClientKYC.MothersName = userInfoModify.MotherName;
                        NewMNClientKYC.GFathersName = userInfoModify.GrandFatherName;
                        NewMNClientKYC.PANNumber = userInfoModify.PanNo;

                        NewMNClientKYC.PProvince = userInfoModify.PProvince;
                        NewMNClientKYC.PDistrict = userInfoModify.PDistrict;
                        NewMNClientKYC.PMunicipalityVDC = userInfoModify.PVDC;
                        NewMNClientKYC.PHouseNo = userInfoModify.PHouseNo;
                        NewMNClientKYC.PWardNo = userInfoModify.PWardNo;
                        NewMNClientKYC.PStreet = userInfoModify.PStreet;

                        NewMNClientKYC.CProvince = userInfoModify.CProvince;
                        NewMNClientKYC.CDistrict = userInfoModify.CDistrict;
                        NewMNClientKYC.CMunicipalityVDC = userInfoModify.CVDC;
                        NewMNClientKYC.CHouseNo = userInfoModify.CHouseNo;
                        NewMNClientKYC.CWardNo = userInfoModify.CWardNo;
                        NewMNClientKYC.CStreet = userInfoModify.CStreet;

                        NewMNClientKYC.DateOfBirth = DateTime.ParseExact(userInfoModify.DOB, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        NewMNClientKYC.CitizenIssueDate = DateTime.ParseExact(userInfoModify.CitizenshipIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        NewMNClientKYC.LicenseIssueDate = DateTime.ParseExact(userInfoModify.LicenseIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        NewMNClientKYC.LicenseExpiryDate = DateTime.ParseExact(userInfoModify.LicenseExpireDate, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        NewMNClientKYC.PassportIssueDate = DateTime.ParseExact(userInfoModify.PassportIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        NewMNClientKYC.PassportExpiryDate = DateTime.ParseExact(userInfoModify.PassportExpireDate, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                        NewMNClientKYC.CitizenshipNo = userInfoModify.Citizenship;
                        NewMNClientKYC.CitizenPlaceOfIssue = userInfoModify.CitizenshipPlaceOfIssue;
                        NewMNClientKYC.BSCitizenIssueDate = userInfoModify.BSCitizenshipIssueDate;

                        NewMNClientKYC.LicenseNo = userInfoModify.License;
                        NewMNClientKYC.LicensePlaceOfIssue = userInfoModify.LicensePlaceOfIssue;
                        NewMNClientKYC.BSLicenseIssueDate = userInfoModify.BSLicenseIssueDate;
                        NewMNClientKYC.BSLicenseExpiryDate = userInfoModify.BSLicenseExpireDate;

                        NewMNClientKYC.PassportNo = userInfoModify.Passport;
                        NewMNClientKYC.PassportPlaceOfIssue = userInfoModify.PassportPlaceOfIssue;
                        
                        NewMNClientKYCDoc.DocType = userInfoModify.Document;
                        NewMNClientKYCDoc.PassportImage = userInfoModify.PassportImageName;
                        NewMNClientKYCDoc.FrontImage = userInfoModify.FrontImageName;
                        NewMNClientKYCDoc.BackImage = userInfoModify.BackImageName;
                        
                        /*map NEW data to Agent--END*/

                        NewAgent.MNClient = NewMNClient;
                        NewAgent.MNClientContact = NewMNClientContact;
                        NewAgent.MNClientKYC = NewMNClientKYC;
                        NewAgent.MNClientKYCDoc = NewMNClientKYCDoc;
                        
                        //For Province
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
                        //end Province

                        UserInfo regobj = new UserInfo();
                       
                        DataSet DSet = ProfileUtils.GetAgentProfileInfoDS(id);
                        DataTable dtableUserInfo = DSet.Tables["dtUserInfo"];
                        DataTable dKycInfo = DSet.Tables["dtKycInfo"];
                        DataTable dKycDoc = DSet.Tables["dtKycDoc"];


                        if (dtableUserInfo != null && dtableUserInfo.Rows.Count > 0)
                        {
                            regobj.ClientCode = dtableUserInfo.Rows[0]["ClientCode"].ToString();
                            regobj.Name = dtableUserInfo.Rows[0]["Name"].ToString();
                            regobj.Address = dtableUserInfo.Rows[0]["Address"].ToString();

                            regobj.PProvince = dtableUserInfo.Rows[0]["PProvince"].ToString();
                            regobj.PDistrict = dtableUserInfo.Rows[0]["PDistrict"].ToString();
                            regobj.PMunicipalityVDC = dtableUserInfo.Rows[0]["PMunicipalityVDC"].ToString();
                            regobj.PWardNo = dtableUserInfo.Rows[0]["PWardNo"].ToString();
                            regobj.PHouseNo = dtableUserInfo.Rows[0]["PHouseNo"].ToString();
                            regobj.PStreet = dtableUserInfo.Rows[0]["PStreet"].ToString();

                            regobj.CProvince = dtableUserInfo.Rows[0]["CProvince"].ToString();
                            regobj.CDistrict = dtableUserInfo.Rows[0]["CDistrict"].ToString();
                            regobj.CMunicipalityVDC = dtableUserInfo.Rows[0]["CMunicipalityVDC"].ToString();
                            regobj.CWardNo = dtableUserInfo.Rows[0]["CWardNo"].ToString();
                            regobj.CHouseNo = dtableUserInfo.Rows[0]["CHouseNo"].ToString();
                            regobj.CStreet = dtableUserInfo.Rows[0]["CStreet"].ToString();



                            regobj.IsApproved = dtableUserInfo.Rows[0]["IsApproved"].ToString();
                            regobj.IsRejected = dtableUserInfo.Rows[0]["IsRejected"].ToString();
                            regobj.IsModified = dtableUserInfo.Rows[0]["IsModified"].ToString();

                            regobj.Status = dtableUserInfo.Rows[0]["Status"].ToString();
                            regobj.UserName = dtableUserInfo.Rows[0]["UserName"].ToString();
                            regobj.UserType = dtableUserInfo.Rows[0]["UserType"].ToString();
                            regobj.PIN = dtableUserInfo.Rows[0]["PIN"].ToString();
                            regobj.WalletNumber = dtableUserInfo.Rows[0]["WalletNumber"].ToString();

                            regobj.FName = dtableUserInfo.Rows[0]["FName"].ToString();
                            regobj.MName = dtableUserInfo.Rows[0]["MName"].ToString();
                            regobj.LName = dtableUserInfo.Rows[0]["LName"].ToString();
                            regobj.Gender = dtableUserInfo.Rows[0]["Gender"].ToString();
                            regobj.DOB = dtableUserInfo.Rows[0]["DateOfBirth"].ToString().Split()[0];;
                            regobj.BSDateOfBirth = dtableUserInfo.Rows[0]["BSDateOfBirth"].ToString();

                            regobj.EmailAddress = dtableUserInfo.Rows[0]["EmailAddress"].ToString();
                            regobj.Nationality = dtableUserInfo.Rows[0]["Nationality"].ToString();
                            regobj.Country = dtableUserInfo.Rows[0]["Country"].ToString();
                            regobj.Occupation = dtableUserInfo.Rows[0]["Occupation"].ToString();
                            regobj.MaritalStatus = dtableUserInfo.Rows[0]["MaritalStatus"].ToString();
                            regobj.SpouseName = dtableUserInfo.Rows[0]["SpouseName"].ToString();

                            regobj.FatherInLaw = dtableUserInfo.Rows[0]["FatherInLaw"].ToString();

                            regobj.FatherName = dtableUserInfo.Rows[0]["FathersName"].ToString();
                            regobj.MotherName = dtableUserInfo.Rows[0]["MothersName"].ToString();
                            regobj.GrandFatherName = dtableUserInfo.Rows[0]["GFathersName"].ToString();
                            regobj.PanNo = dtableUserInfo.Rows[0]["PANNumber"].ToString();


                            regobj.Document = dtableUserInfo.Rows[0]["DocType"].ToString();
                            regobj.Citizenship = dtableUserInfo.Rows[0]["CitizenshipNo"].ToString();
                            regobj.CitizenshipIssueDate = dtableUserInfo.Rows[0]["CitizenIssueDate"].ToString().Split()[0];
                            regobj.BSCitizenshipIssueDate = dtableUserInfo.Rows[0]["BSCitizenIssueDate"].ToString().Split()[0];
                            regobj.CitizenshipPlaceOfIssue = dtableUserInfo.Rows[0]["CitizenPlaceOfIssue"].ToString();

                            regobj.License = dtableUserInfo.Rows[0]["LicenseNo"].ToString();
                            regobj.LicenseIssueDate = dtableUserInfo.Rows[0]["LicenseIssueDate"].ToString().Split()[0];
                            regobj.BSLicenseIssueDate = dtableUserInfo.Rows[0]["BSLicenseIssueDate"].ToString().Split()[0];
                            regobj.LicenseExpireDate = dtableUserInfo.Rows[0]["LicenseExpiryDate"].ToString().Split()[0];
                            regobj.BSLicenseExpireDate = dtableUserInfo.Rows[0]["BSLicenseExpiryDate"].ToString().Split()[0];
                            regobj.LicensePlaceOfIssue = dtableUserInfo.Rows[0]["LicensePlaceOfIssue"].ToString();

                            regobj.Passport = dtableUserInfo.Rows[0]["PassportNo"].ToString();
                            regobj.PassportPlaceOfIssue = dtableUserInfo.Rows[0]["PassportPlaceOfIssue"].ToString();
                            regobj.PassportIssueDate = dtableUserInfo.Rows[0]["PassportIssueDate"].ToString().Split()[0];
                            regobj.PassportExpireDate = dtableUserInfo.Rows[0]["PassportExpiryDate"].ToString().Split()[0];
                            
                            regobj.PassportImage = dtableUserInfo.Rows[0]["PassportImage"].ToString();
                            regobj.FrontImage = dtableUserInfo.Rows[0]["FrontImage"].ToString();
                            regobj.BackImage = dtableUserInfo.Rows[0]["BackImage"].ToString();

                        }


                        /*map old data to Agent*/
                        AgentTable OldAgent = new AgentTable();
                        MNClient OldMNClient = new MNClient();
                        MNClientContact OldMNClientContact = new MNClientContact();
                        MNClientKYC OldMNClientKYC = new MNClientKYC();
                        MNClientKYCDoc OldMNClientKYCDoc = new MNClientKYCDoc();
                        
                        OldMNClient.Address = regobj.Address;
                        OldMNClient.Name = regobj.Name;

                        OldMNClientContact.EmailAddress = regobj.EmailAddress;
                        OldMNClientContact.ContactNumber1 = regobj.ContactNumber1;

                        OldMNClientKYC.FName = regobj.FName;
                        OldMNClientKYC.MName = regobj.MName;
                        OldMNClientKYC.LName = regobj.LName;
                        OldMNClientKYC.Gender = regobj.Gender;
                        OldMNClientKYC.BSDateOfBirth = regobj.BSDateOfBirth;
                        OldMNClientKYC.Nationality = regobj.Nationality;
                        OldMNClientKYC.Country = regobj.Country;
                        OldMNClientKYC.Occupation = regobj.Occupation;
                        OldMNClientKYC.MaritalStatus = regobj.MaritalStatus;
                        OldMNClientKYC.SpouseName = regobj.SpouseName;
                        OldMNClientKYC.FatherInLaw = regobj.FatherInLaw;
                        OldMNClientKYC.FathersName = regobj.FatherName;
                        OldMNClientKYC.MothersName = regobj.MotherName;
                        OldMNClientKYC.GFathersName = regobj.GrandFatherName;
                        OldMNClientKYC.PANNumber = regobj.PanNo;

                        OldMNClientKYC.PProvince = regobj.PProvince;
                        OldMNClientKYC.PDistrict = regobj.PDistrict;
                        OldMNClientKYC.PMunicipalityVDC = regobj.PMunicipalityVDC;
                        OldMNClientKYC.PHouseNo = regobj.PHouseNo;
                        OldMNClientKYC.PWardNo = regobj.PWardNo;
                        OldMNClientKYC.PStreet = regobj.PStreet;

                        OldMNClientKYC.CProvince = regobj.CProvince;
                        OldMNClientKYC.CDistrict = regobj.CDistrict;
                        OldMNClientKYC.CMunicipalityVDC = regobj.CMunicipalityVDC;
                        OldMNClientKYC.CHouseNo = regobj.CHouseNo;
                        OldMNClientKYC.CWardNo = regobj.CWardNo;
                        OldMNClientKYC.CStreet = regobj.CStreet;

                        OldMNClientKYC.DateOfBirth = DateTime.Parse(regobj.DOB.Trim()).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        OldMNClientKYC.CitizenIssueDate = DateTime.Parse(regobj.CitizenshipIssueDate.Trim()).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        OldMNClientKYC.LicenseIssueDate = DateTime.Parse(regobj.LicenseIssueDate.Trim()).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        OldMNClientKYC.LicenseExpiryDate = DateTime.Parse(regobj.LicenseExpireDate.Trim()).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        OldMNClientKYC.PassportIssueDate = DateTime.Parse(regobj.PassportIssueDate.Trim()).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        OldMNClientKYC.PassportExpiryDate = DateTime.Parse(regobj.PassportExpireDate.Trim()).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        
                        OldMNClientKYC.CitizenshipNo = regobj.Citizenship;
                        OldMNClientKYC.CitizenPlaceOfIssue = regobj.CitizenshipPlaceOfIssue;
                        OldMNClientKYC.BSCitizenIssueDate = regobj.BSCitizenshipIssueDate;

                        OldMNClientKYC.LicenseNo = regobj.License;
                        OldMNClientKYC.LicensePlaceOfIssue = regobj.LicensePlaceOfIssue;
                        OldMNClientKYC.BSLicenseIssueDate = regobj.BSLicenseIssueDate;
                        OldMNClientKYC.BSLicenseExpiryDate = regobj.BSLicenseExpireDate;

                        OldMNClientKYC.PassportNo = regobj.Passport;
                        OldMNClientKYC.PassportPlaceOfIssue = regobj.PassportPlaceOfIssue;
                        
                        OldMNClientKYCDoc.DocType = regobj.Document;
                        OldMNClientKYCDoc.PassportImage = regobj.PassportImage;
                        OldMNClientKYCDoc.FrontImage = regobj.FrontImage;
                        OldMNClientKYCDoc.BackImage = regobj.BackImage;
                        
                        /*map old data to Agent--END*/

                        
                        OldAgent.MNClient = OldMNClient;
                        OldAgent.MNClientContact = OldMNClientContact;
                        OldAgent.MNClientKYC = OldMNClientKYC;
                        OldAgent.MNClientKYCDoc = OldMNClientKYCDoc;
                       
                        /*Difference compare*/
                        bool isUpdated = false;
                        CompareLogic compareLogic = new CompareLogic();
                        ComparisonConfig config = new ComparisonConfig();
                        config.MaxDifferences = int.MaxValue;
                        config.IgnoreCollectionOrder = false;
                        compareLogic.Config = config;
                        ComparisonResult result = compareLogic.Compare(OldAgent, NewAgent); //  firstparameter orginal,second parameter modified
                        List<MNMakerChecker> makerCheckers = new List<MNMakerChecker>();
                        if (!result.AreEqual)
                        {
                            isUpdated = true;
                            foreach (Difference diff in result.Differences)
                            {
                                MNMakerChecker makerchecker = new MNMakerChecker();
                                int index = diff.PropertyName.IndexOf('.');
                                makerchecker.ColumnName = diff.PropertyName.Substring(index + 1);
                                makerchecker.TableName = diff.ParentPropertyName;
                                makerchecker.OldValue = diff.Object1Value;
                                makerchecker.NewValue = diff.Object2Value;
                                makerchecker.Module = "AGENT";
                                makerCheckers.Add(makerchecker);
                            }
                        }


                        if (isUpdated)
                        {

                            string modifyingAdmin = (string)Session["UserName"];
                            string modifyingBranch = (string)Session["UserBranch"];
                            CusProfileUtils cUtil = new CusProfileUtils();
                            string ModifiedFieldXML = cUtil.GetMakerCheckerXMLStr(makerCheckers);
                            bool inserted = AgentUtils.InsertAgentMakerChecker(cClientCode, modifyingAdmin, modifyingBranch, ModifiedFieldXML);

                            displayMessage = inserted
                                                    ? "Agent Information for " + userInfoModify.FName + " has successfully been updated. Please go to Approve Modification and perform accordingly."
                                                    : "Error while updating Agent Information";
                            messageClass = inserted ? CssSetting.SuccessMessageClass : CssSetting.FailedMessageClass;

                        }
                        else
                        {
                            displayMessage = "Nothing Changed";
                            messageClass = CssSetting.SuccessMessageClass;
                        }




                        this.TempData["agenttmodify_messsage"] = displayMessage;
                        this.TempData["message_class"] = messageClass;
                        return RedirectToAction("Index");
                    }
                    ViewBag.Bank = new SelectList(item, "Value", "Text", userInfoModify.BankNo);
                    return View();

                }
                else
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            catch (Exception ex)
            {
                displayMessage = "Error while updating Agent Information. Error cause::" + ex.Message;
                messageClass = CssSetting.FailedMessageClass;
                return RedirectToAction("AgentModify", new { id = Id });

            }
            finally
            {
                this.TempData["agentmodify_messsage"] = displayMessage;
                this.TempData["message_class"] = messageClass;
            }
        }

        public ActionResult GetCheckingUserName(string Username)
        {
            if ((Username != "") || (Username != null))
            {
                string result = string.Empty;
                DataTable dtableMobileNo = RegisterUtils.GetCheckMobileNo(Username);
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
                return View();
        }
        #endregion
        

        #region Agent Registration Approval list
        [HttpGet]
        public ActionResult ApproveAgentList(string Key, string Value)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            string UserName = Value;

            if (this.TempData["agentapprove_messsage"] != null)
            {
                this.ViewData["agentapprove_messsage"] = this.TempData["agentapprove_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            //Check Role link start    
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkRole(System.Reflection.MethodBase.GetCurrentMethod().Name, clientCode);
            //Check Role link end


            TempData["userType"] = userType;
            if (TempData["userType"] != null&& checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;


                List<UserInfo> agentRegApprove = new List<UserInfo>();
                var agentInfo = AgentUtils.GetAgentRegApprove("agent", UserName);



                ViewBag.Value = Value;
                return View(agentInfo);

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        //View Agent Registration Detail
        [HttpGet]
        public ActionResult ViewAgentRegDetail(string clientCodeId)
        {
            if (string.IsNullOrEmpty(clientCodeId))
            {
                return RedirectToAction("Index", "Agent");
            }
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

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
                //List<SelectListItem> item = new List<SelectListItem>();
                //DataSet dsBank = BranchUtils.GetDataSetPopulateBranchName();
                //ViewBag.bank = dsBank.Tables[0];
                //foreach (DataRow dr in ViewBag.bank.Rows)
                //{
                //    if (dr["IsBlocked"].ToString() == "F")
                //    {
                //        item.Add(new SelectListItem
                //        {
                //            Text = dr["BranchName"].ToString(),
                //            Value = dr["BranchCode"].ToString()
                //        });
                //    }
                //}
                //ViewBag.BranchName = item;

                //For ProfileName
                //List<SelectListItem> itemProfile = new List<SelectListItem>();
                //DataSet dsProfile = UserProfileUtils.GetDataSetPopulateProfileName();
                //ViewBag.profile = dsProfile.Tables[0];
                //foreach (DataRow dr in ViewBag.profile.Rows)
                //{
                //    itemProfile.Add(new SelectListItem
                //    {
                //        Text = @dr["ProfileName"].ToString(),
                //        Value = @dr["ProfileCode"].ToString()
                //    });
                //}


                UserInfo userInfo = new UserInfo();
                DataTable dtblRegistration = ProfileUtils.GetAgentProfileInfo(clientCodeId);
                if (dtblRegistration.Rows.Count == 1)
                {
                    userInfo.ClientCode = dtblRegistration.Rows[0]["ClientCode"].ToString();
                    userInfo.Name = dtblRegistration.Rows[0]["Name"].ToString().Capitalize();
                    userInfo.Address = dtblRegistration.Rows[0]["Address"].ToString();
                    userInfo.UserName = dtblRegistration.Rows[0]["UserName"].ToString();
                    userInfo.WalletNumber = dtblRegistration.Rows[0]["WalletNumber"].ToString();

                    userInfo.FName = dtblRegistration.Rows[0]["FName"].ToString();
                    userInfo.MName = dtblRegistration.Rows[0]["MName"].ToString();
                    userInfo.LName = dtblRegistration.Rows[0]["LName"].ToString();
                    userInfo.DOB = dtblRegistration.Rows[0]["DateOfBirth"].ToString().Split()[0];
                    userInfo.BSDateOfBirth = dtblRegistration.Rows[0]["BSDateOfBirth"].ToString().Split()[0];
                    userInfo.EmailAddress = dtblRegistration.Rows[0]["EmailAddress"].ToString();
                    userInfo.Gender = dtblRegistration.Rows[0]["Gender"].ToString();
                    userInfo.Nationality = dtblRegistration.Rows[0]["Nationality"].ToString();
                    userInfo.Country = dtblRegistration.Rows[0]["Country"].ToString();
                    userInfo.FatherName = dtblRegistration.Rows[0]["FathersName"].ToString();
                    userInfo.MotherName = dtblRegistration.Rows[0]["MothersName"].ToString();
                    userInfo.MaritalStatus = dtblRegistration.Rows[0]["MaritalStatus"].ToString();
                    userInfo.SpouseName = dtblRegistration.Rows[0]["SpouseName"].ToString();
                    ///start father
                    userInfo.FatherInLaw = dtblRegistration.Rows[0]["FatherInLaw"].ToString();
                    //end father
                    userInfo.GrandFatherName = dtblRegistration.Rows[0]["GFathersName"].ToString();
                    userInfo.Occupation = dtblRegistration.Rows[0]["Occupation"].ToString();
                    userInfo.PanNo = dtblRegistration.Rows[0]["PanNumber"].ToString();

                    userInfo.PProvince = dtblRegistration.Rows[0]["PProvince"].ToString();
                    userInfo.PDistrict = getDistrictName(dtblRegistration.Rows[0]["PDistrict"].ToString());
                    userInfo.PVDC = dtblRegistration.Rows[0]["PMunicipalityVDC"].ToString();
                    userInfo.PWardNo = dtblRegistration.Rows[0]["PWardNo"].ToString();
                    userInfo.PHouseNo = dtblRegistration.Rows[0]["PHouseNo"].ToString();
                    userInfo.PStreet = dtblRegistration.Rows[0]["PStreet"].ToString();

                    userInfo.CProvince = dtblRegistration.Rows[0]["CProvince"].ToString();
                    userInfo.CDistrict = getDistrictName(dtblRegistration.Rows[0]["CDistrict"].ToString());
                    userInfo.CVDC = dtblRegistration.Rows[0]["CMunicipalityVDC"].ToString();
                    userInfo.CWardNo = dtblRegistration.Rows[0]["CWardNo"].ToString();
                    userInfo.CHouseNo = dtblRegistration.Rows[0]["CHouseNo"].ToString();
                    userInfo.CStreet = dtblRegistration.Rows[0]["CStreet"].ToString();

                    userInfo.Document = dtblRegistration.Rows[0]["DocType"].ToString();
                    userInfo.Citizenship = dtblRegistration.Rows[0]["CitizenshipNo"].ToString();
                    userInfo.CitizenshipIssueDate = dtblRegistration.Rows[0]["CitizenIssueDate"].ToString().Split()[0];
                    userInfo.BSCitizenshipIssueDate = dtblRegistration.Rows[0]["BSCitizenIssueDate"].ToString().Split()[0];
                    userInfo.CitizenshipPlaceOfIssue = dtblRegistration.Rows[0]["CitizenPlaceOfIssue"].ToString();


                    userInfo.License = dtblRegistration.Rows[0]["LicenseNo"].ToString();
                    userInfo.LicenseIssueDate = dtblRegistration.Rows[0]["LicenseIssueDate"].ToString().Split()[0];
                    userInfo.BSLicenseIssueDate = dtblRegistration.Rows[0]["BSLicenseIssueDate"].ToString().Split()[0];
                    userInfo.LicenseExpireDate = dtblRegistration.Rows[0]["LicenseExpiryDate"].ToString().Split()[0];
                    userInfo.BSLicenseExpireDate = dtblRegistration.Rows[0]["BSLicenseExpiryDate"].ToString().Split()[0];
                    userInfo.LicensePlaceOfIssue = dtblRegistration.Rows[0]["LicensePlaceOfIssue"].ToString();

                    userInfo.Passport = dtblRegistration.Rows[0]["PassportNo"].ToString();
                    userInfo.PassportIssueDate = dtblRegistration.Rows[0]["PassportIssueDate"].ToString().Split()[0];
                    userInfo.PassportExpireDate = dtblRegistration.Rows[0]["PassportExpiryDate"].ToString().Split()[0];
                    userInfo.PassportPlaceOfIssue = dtblRegistration.Rows[0]["PassportPlaceOfIssue"].ToString();

                    userInfo.PassportImage = dtblRegistration.Rows[0]["PassportImage"].ToString();
                    userInfo.FrontImage = dtblRegistration.Rows[0]["FrontImage"].ToString();
                    userInfo.BackImage = dtblRegistration.Rows[0]["BackImage"].ToString();

                    userInfo.DOB = DateConvert(userInfo.DOB);

                    userInfo.CitizenshipIssueDate = DateConvert(userInfo.CitizenshipIssueDate);
                    userInfo.LicenseIssueDate = DateConvert(userInfo.LicenseIssueDate);
                    userInfo.LicenseExpireDate = DateConvert(userInfo.LicenseExpireDate);

                    userInfo.PassportIssueDate = DateConvert(userInfo.PassportIssueDate);
                    userInfo.PassportExpireDate = DateConvert(userInfo.PassportExpireDate);

                    //userInfo.UserType = dtblRegistration.Rows[0]["UserType"].ToString();
                    //userInfo.IsApproved = dtblRegistration.Rows[0]["IsApproved"].ToString();
                    //userInfo.IsRejected = dtblRegistration.Rows[0]["IsRejected"].ToString();
                    //userInfo.PIN = dtblRegistration.Rows[0]["PIN"].ToString();
                    //userInfo.Status = dtblRegistration.Rows[0]["Status"].ToString();
                    //userInfo.ContactNumber1 = dtblRegistration.Rows[0]["ContactNumber1"].ToString();
                    //userInfo.ContactNumber2 = dtblRegistration.Rows[0]["ContactNumber2"].ToString();
                    //userInfo.ProfileName = dtblRegistration.Rows[0]["ProfileName"].ToString();
                    //userInfo.BankNo = dtblRegistration.Rows[0]["BankNo"].ToString();
                    //userInfo.BranchCode = dtblRegistration.Rows[0]["UserBranchCode"].ToString();
                    //userInfo.BankAccountNumber = dtblRegistration.Rows[0]["BankAccountNumber"].ToString();
                    //userInfo.COC = dtblRegistration.Rows[0]["COC"].ToString();
                }
                //ViewBag.AdminProfile = new SelectList(itemProfile, "Value", "Text", userInfo.ProfileName).OrderBy(x => x.Text);
                //ViewBag.BranchName = new SelectList(item, "Value", "Text", userInfo.BranchCode).OrderBy(x => x.Text);//,selectedValue:userInfo.BranchCode

                return View(userInfo);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        //To get the district name in text format
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

        public string getProvinceName(string id)
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

        [HttpPost]
        public ActionResult ApproveRegisterAgent(UserInfo model, string btnApprove)
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
                    userInfoApproval.RejectedBy = userName;


                    string Rejected = "T";
                    //string Approve = "UnApprove";

                    userInfoApproval.Remarks = model.Remarks;

                    int ret = AgentUtils.AgentRegReject(userInfoApproval, Rejected);
                    
                    if (ret == 1)
                    {
                        displayMessage = "Agent" + model.Name + " has been Rejected. Please Check Rejectlist and perform accordingly";
                        messageClass = CssSetting.SuccessMessageClass;
                    }
                    else if (ret == -1 || ret != 100)
                    {
                        displayMessage = "Error while rejecting Admin " + model.Name;
                        messageClass = CssSetting.FailedMessageClass;
                    }
                   
                    this.TempData["agentapprove_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;
                    return RedirectToAction("ApproveAgentList");

                }







                if (btnApprove.ToUpper() == "APPROVE")
                {
                    userInfoApproval.SuperAdminUserName = "";
                    //userInfoApproval.AdminBranch = "";

                    //string Rejected = "F";
                    string Approve = "Approve";
                    userInfoApproval.ApprovedBy = userName;
                    int ret = AgentUtils.AgentRegisterApprove(userInfoApproval, Approve);
                    if (ret == 1)
                    {
                        UserInfo userInfo = new UserInfo();
                        DataTable dtblRegistration = ProfileUtils.GetAgentProfileInfo(model.ClientCode);
                          if (dtblRegistration.Rows.Count == 1)
                        {
                            userInfo.Name = dtblRegistration.Rows[0]["Name"].ToString().Capitalize();
                            userInfo.EmailAddress = dtblRegistration.Rows[0]["EmailAddress"].ToString();
                            userInfo.UserName = dtblRegistration.Rows[0]["UserName"].ToString();
                            userInfo.Password = dtblRegistration.Rows[0]["Password"].ToString();
                            userInfo.PIN = dtblRegistration.Rows[0]["PIN"].ToString();
                            userInfo.ClientCode = dtblRegistration.Rows[0]["ClientCode"].ToString();
                            userInfo.FName = dtblRegistration.Rows[0]["FName"].ToString();
                        }
                        userInfo.Password = CustomerUtils.GeneratePassword();
                        bool passChange = PasswordUtils.ResetPassword(userInfo);
                        if (userInfo.EmailAddress != "" && userInfo.EmailAddress != string.Empty)
                        {
                            string Subject = "New Agent Registration";
                            string MailSubject = "<span style='font-size:15px;'><h4>Dear " + userInfo.FName + ",</h4>";
                            MailSubject += "A new Account has been created  for you at Nepal Investment Bank Ltd. Mobile Banking,";
                            MailSubject += "and you have been issued with a new temporary password.<br/>";
                            MailSubject += "<br/><b>Username: " + userInfo.UserName + "</b> <br/>";
                            MailSubject += "<br/><b>Password: " + userInfo.Password + "</b> <br/>";
                            MailSubject += "<br/><b>PIN: " + userInfo.PIN + "</b> <br/>";
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


                            //for sms 
                            SMSUtils SMS = new SMSUtils();
                            string Message = string.Format("Dear " + userInfo.FName + ",\n Your new T-Pin is " + userInfo.PIN + "and Password is " + userInfo.Password + ".\n Thank You -MNepal");

                            SMSLog Log = new SMSLog();

                            Log.UserName = userInfo.UserName;
                            Log.Purpose = "NR"; //New Registration
                            Log.SentBy = userInfoApproval.ApprovedBy;
                            Log.Message = "Your T-Pin is " + ExtraUtility.Encrypt(userInfo.PIN) + " and Password is " + ExtraUtility.Encrypt(userInfo.Password); //encrypted when logging
                                                                                                                                                                 //Log SMS
                            CustomerUtils.LogSMS(Log);

                            SMS.SendSMS(Message, userInfo.UserName);

                        }
                        displayMessage = "Registration Information for  Agent " + userInfo.Name + " has successfully been approved.";
                        messageClass = CssSetting.SuccessMessageClass;
                    }
                    else if (ret == -1 || ret != 100)
                    {
                        displayMessage = "Error while approving Agent Information";
                        messageClass = CssSetting.FailedMessageClass;
                    }

                    this.TempData["agentapprove_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;
                    return RedirectToAction("ApproveAgentList");
                }
                else
                {

                    this.TempData["agentapprove_messsage"] = "Unauthorized execution";
                    this.TempData["message_class"] = CssSetting.FailedMessageClass;

                    return RedirectToAction("ApproveAgentList", "Agent");
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


        #region Agent Modification Approval list
        [HttpGet]
        public ActionResult ApproveAgentModList(string Key, string Value)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            string UserName = Value;

            if (this.TempData["agentapprove_messsage"] != null)
            {
                this.ViewData["agentapprove_messsage"] = this.TempData["agentapprove_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            //Check Role link start    
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkRole(System.Reflection.MethodBase.GetCurrentMethod().Name, clientCode);
            //Check Role link end


            TempData["userType"] = userType;
            if (TempData["userType"] != null&& checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;


                List<UserInfo> agentRegApprove = new List<UserInfo>();
                var agentInfo = AgentUtils.GetAgentModApprove("agent", UserName);



                ViewBag.Value = Value;
                return View(agentInfo);

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        //View Agent Modification Detail
        [HttpGet]
        public ActionResult ViewAgentModDetail(string clientCodeId)
        {
            if (string.IsNullOrEmpty(clientCodeId))
            {
                return RedirectToAction("Index", "Agent");
            }
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

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

                UserInfo userInfo = new UserInfo();
                DataTable dtblRegistration = ProfileUtils.GetAgentProfileInfo(clientCodeId);
                if (dtblRegistration.Rows.Count == 1)
                {
                    userInfo.ClientCode = dtblRegistration.Rows[0]["ClientCode"].ToString();
                    userInfo.Name = dtblRegistration.Rows[0]["Name"].ToString().Capitalize();
                    userInfo.Address = dtblRegistration.Rows[0]["Address"].ToString();
                    userInfo.UserName = dtblRegistration.Rows[0]["UserName"].ToString();
                    userInfo.WalletNumber = dtblRegistration.Rows[0]["WalletNumber"].ToString();

                    userInfo.FName = dtblRegistration.Rows[0]["FName"].ToString();
                    userInfo.MName = dtblRegistration.Rows[0]["MName"].ToString();
                    userInfo.LName = dtblRegistration.Rows[0]["LName"].ToString();

                    userInfo.DOB = DateTime.Parse(dtblRegistration.Rows[0]["DateOfBirth"].ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                    userInfo.BSDateOfBirth = dtblRegistration.Rows[0]["BSDateOfBirth"].ToString().Split()[0];
                    userInfo.EmailAddress = dtblRegistration.Rows[0]["EmailAddress"].ToString();
                    userInfo.Gender = dtblRegistration.Rows[0]["Gender"].ToString();
                    userInfo.Nationality = dtblRegistration.Rows[0]["Nationality"].ToString();
                    userInfo.Country = dtblRegistration.Rows[0]["Country"].ToString();
                    userInfo.FatherName = dtblRegistration.Rows[0]["FathersName"].ToString();
                    userInfo.MotherName = dtblRegistration.Rows[0]["MothersName"].ToString();
                    userInfo.MaritalStatus = dtblRegistration.Rows[0]["MaritalStatus"].ToString();
                    userInfo.SpouseName = dtblRegistration.Rows[0]["SpouseName"].ToString();

                    userInfo.FatherInLaw = dtblRegistration.Rows[0]["FatherInLaw"].ToString();

                    userInfo.GrandFatherName = dtblRegistration.Rows[0]["GFathersName"].ToString();
                    userInfo.Occupation = dtblRegistration.Rows[0]["Occupation"].ToString();
                    userInfo.PanNo = dtblRegistration.Rows[0]["PanNumber"].ToString();

                    userInfo.PProvince = getProvinceName(dtblRegistration.Rows[0]["PProvince"].ToString());
                    userInfo.PDistrict = getDistrictName(dtblRegistration.Rows[0]["PDistrict"].ToString());
                    userInfo.PVDC = dtblRegistration.Rows[0]["PMunicipalityVDC"].ToString();
                    userInfo.PWardNo = dtblRegistration.Rows[0]["PWardNo"].ToString();
                    userInfo.PHouseNo = dtblRegistration.Rows[0]["PHouseNo"].ToString();
                    userInfo.PStreet = dtblRegistration.Rows[0]["PStreet"].ToString();

                    userInfo.CProvince = getProvinceName(dtblRegistration.Rows[0]["CProvince"].ToString());
                    userInfo.CDistrict = getDistrictName(dtblRegistration.Rows[0]["CDistrict"].ToString());
                    userInfo.CVDC = dtblRegistration.Rows[0]["CMunicipalityVDC"].ToString();
                    userInfo.CWardNo = dtblRegistration.Rows[0]["CWardNo"].ToString();
                    userInfo.CHouseNo = dtblRegistration.Rows[0]["CHouseNo"].ToString();
                    userInfo.CStreet = dtblRegistration.Rows[0]["CStreet"].ToString();

                    userInfo.Document = dtblRegistration.Rows[0]["DocType"].ToString();

                    userInfo.Citizenship = dtblRegistration.Rows[0]["CitizenshipNo"].ToString();
                    userInfo.CitizenshipIssueDate = DateTime.Parse(dtblRegistration.Rows[0]["CitizenIssueDate"].ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                    userInfo.BSCitizenshipIssueDate = dtblRegistration.Rows[0]["BSCitizenIssueDate"].ToString().Split()[0];
                    userInfo.CitizenshipPlaceOfIssue = dtblRegistration.Rows[0]["CitizenPlaceOfIssue"].ToString();

                    userInfo.License = dtblRegistration.Rows[0]["LicenseNo"].ToString();
                    userInfo.LicenseIssueDate = DateTime.Parse(dtblRegistration.Rows[0]["LicenseIssueDate"].ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                    userInfo.BSLicenseIssueDate = dtblRegistration.Rows[0]["BSLicenseIssueDate"].ToString().Split()[0];
                    userInfo.LicenseExpireDate = DateTime.Parse(dtblRegistration.Rows[0]["LicenseExpiryDate"].ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                    userInfo.BSLicenseExpireDate = dtblRegistration.Rows[0]["BSLicenseExpiryDate"].ToString().Split()[0];
                    userInfo.LicensePlaceOfIssue = dtblRegistration.Rows[0]["LicensePlaceOfIssue"].ToString();

                    userInfo.Passport = dtblRegistration.Rows[0]["PassportNo"].ToString();
                    userInfo.PassportExpireDate = DateTime.Parse(dtblRegistration.Rows[0]["PassportExpiryDate"].ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                    userInfo.PassportIssueDate = DateTime.Parse(dtblRegistration.Rows[0]["PassportIssueDate"].ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                    userInfo.PassportPlaceOfIssue = dtblRegistration.Rows[0]["PassportPlaceOfIssue"].ToString();
                   
                    userInfo.PassportImage = dtblRegistration.Rows[0]["PassportImage"].ToString();
                    userInfo.FrontImage = dtblRegistration.Rows[0]["FrontImage"].ToString();
                    userInfo.BackImage = dtblRegistration.Rows[0]["BackImage"].ToString();

                    
                }
                //Get chanegd Values
                DataSet DSetMakerchecker = AgentUtils.GetAgentModifiedValue(clientCodeId);
                List<MNMakerChecker> ModifiedValues = ExtraUtility.DatatableToListClass<MNMakerChecker>(DSetMakerchecker.Tables["MNMakerChecker"]);
                userInfo.MakerChecker = ModifiedValues;


                return View(userInfo);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        [HttpPost]
        public ActionResult ApproveModifiedAgent(UserInfo model, string btnApprove)
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
                    userInfoApproval.RejectedBy = userName;
                    userInfoApproval.Remarks = model.Remarks;

                    bool ret = AgentUtils.AgentModReject(userInfoApproval);

                    if (ret)
                    {
                        displayMessage = "Agent" + model.Name + " has been Rejected. Please Check Rejectlist and perform accordingly";
                        messageClass = CssSetting.SuccessMessageClass;
                    }
                    else
                    {
                        displayMessage = "Error while rejecting Agent " + model.Name;
                        messageClass = CssSetting.FailedMessageClass;
                    }

                    this.TempData["agentapprove_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;
                    return RedirectToAction("ApproveAgentModList");
                }
                if (btnApprove.ToUpper() == "APPROVE")
                {
                    userInfoApproval.ApprovedBy = userName;
                    bool ret = AgentUtils.AgentModApprove(userInfoApproval);
                    if (ret)
                    {

                        displayMessage = "Modification Information for  Agent " + model.Name + " has successfully been approved.";
                        messageClass = CssSetting.SuccessMessageClass;
                    }
                    else
                    {
                        displayMessage = "Error while approving Agent Information";
                        messageClass = CssSetting.FailedMessageClass;
                    }

                    this.TempData["agentapprove_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;
                    return RedirectToAction("ApproveAgentModList");
                }
                else
                {

                    this.TempData["agentapprove_messsage"] = "Unauthorized execution";
                    this.TempData["message_class"] = CssSetting.FailedMessageClass;

                    return RedirectToAction("ApproveAgentModList", "Agent");
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


        #region Agent Deactivation/Reactivation Search
        [HttpGet]
        public ActionResult SearchIndex()
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

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }



        [HttpGet]
        public ActionResult AgentSearchInfo(string SearchCol, string txtMobileNo, string txtName, string txtAccountNo)
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
                if ((userInfo.ContactNumber1 != "") && (SearchCol == "Mobile Number")) //UserName = ContactNumber1
                {
                    DataTable dtableCustomerStatusByMobileNo = AgentUtils.GetAllAgentPBMobileNo(userInfo.ContactNumber1);
                    if (dtableCustomerStatusByMobileNo != null && dtableCustomerStatusByMobileNo.Rows.Count > 0)
                    {
                        UserInfo regobj = new UserInfo
                        {
                            ClientCode = dtableCustomerStatusByMobileNo.Rows[0]["ClientCode"].ToString(),
                            Name = dtableCustomerStatusByMobileNo.Rows[0]["Name"].ToString(),
                            Address = dtableCustomerStatusByMobileNo.Rows[0]["Address"].ToString(),
                            //PIN = dtableCustomerStatusByMobileNo.Rows[0]["PIN"].ToString(),
                            Status = dtableCustomerStatusByMobileNo.Rows[0]["Status"].ToString(),
                            //ContactNumber1 = dtableCustomerStatusByMobileNo.Rows[0]["ContactNumber1"].ToString(),
                            //ContactNumber2 = dtableCustomerStatusByMobileNo.Rows[0]["ContactNumber2"].ToString(),
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


        #region Agent Deactivation/Reactivation

        [HttpGet]
        public ActionResult AgentStatusChange(string id)
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
                DataSet DSet = ProfileUtils.GetAgentProfileInfoDS(id);

                DataTable dtableUserInfo = DSet.Tables["dtUserInfo"];


                if (dtableUserInfo != null && dtableUserInfo.Rows.Count > 0)
                {
                    regobj.ClientCode = dtableUserInfo.Rows[0]["ClientCode"].ToString();
                    regobj.Name = dtableUserInfo.Rows[0]["Name"].ToString();
                    regobj.Address = dtableUserInfo.Rows[0]["Address"].ToString();
                    regobj.EmailAddress = dtableUserInfo.Rows[0]["EmailAddress"].ToString();
                    regobj.Status = dtableUserInfo.Rows[0]["Status"].ToString();
                    regobj.UserName = dtableUserInfo.Rows[0]["UserName"].ToString();
                    regobj.UserType = dtableUserInfo.Rows[0]["UserType"].ToString();


                }
                else
                {
                    this.TempData["custmodify_messsage"] = "User not found";
                    this.TempData["message_class"] = CssSetting.FailedMessageClass;
                    return RedirectToAction("Index");

                }


                return View(regobj);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpPost]

        public ActionResult AgentStatusChange(string btnCommand, FormCollection collection)
        {
            //string Id = "";
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


                    if (btnCommand == "Submit")
                    {

                        string cClientCode = collection["ClientCode"].ToString();
                        string cStatus = collection["Status"].ToString();
                        string cBlockRemarks = collection["BlockRemarks"].ToString();


                        userInfoModify.ClientCode = cClientCode;
                        userInfoModify.Status = cStatus;
                        userInfoModify.BlockRemarks = cBlockRemarks;
                        string SAdminUserName = Session["UserName"].ToString();
                        string SAdminBranchCode = Session["UserBranch"].ToString();

                        if (cStatus != "")
                        {

                            bool isUpdated = CustomerUtils.UpdateAgentStatus(cClientCode, cStatus, SAdminBranchCode, SAdminUserName, cBlockRemarks);
                            displayMessage = isUpdated
                                                     ? "Status has successfully been updated."
                                                     : "Error while updating Status";
                            messageClass = isUpdated ? CssSetting.SuccessMessageClass : CssSetting.FailedMessageClass;
                        }
                        else
                        {
                            displayMessage = "Status is Empty";
                            messageClass = CssSetting.FailedMessageClass;
                        }

                        this.TempData["agenttmodify_messsage"] = displayMessage;
                        this.TempData["message_class"] = messageClass;
                        return RedirectToAction("AgentStatusChange");
                    }

                    return View();

                }
                else
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            catch (Exception ex)
            {
                displayMessage = "Error while updating Agent Information. Error cause::" + ex.Message;
                messageClass = CssSetting.FailedMessageClass;
                return RedirectToAction("AgentStatusChange");

            }
            finally
            {
                this.TempData["agenttmodify_messsage"] = displayMessage;
                this.TempData["message_class"] = messageClass;
            }
        }
        #endregion

        #region Approve Deactivation/Reactivation of Agent
        [HttpGet]
        public ActionResult AgentStatusChangedList(string MobileNumber)
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
                //string BranchCode = Session["UserBranch"].ToString();

                var model = AgentUtils.GetAgentStatus(MobileNumber);
                return View(model);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        [HttpGet]
        public ContentResult StatusApparove(string ClientCode, string Name)
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
                bool isUpdated = false;

                UserInfo info = new UserInfo();
                DataTable dtableUserInfo = ProfileUtils.GetAgentProfileInfo(ClientCode);
                if (dtableUserInfo != null && dtableUserInfo.Rows.Count > 0)
                {
                    info.ClientCode = dtableUserInfo.Rows[0]["ClientCode"].ToString();
                    info.Name = dtableUserInfo.Rows[0]["Name"].ToString();
                    info.Address = dtableUserInfo.Rows[0]["Address"].ToString();
                    info.Status = dtableUserInfo.Rows[0]["Status"].ToString();
                    info.UserName = dtableUserInfo.Rows[0]["UserName"].ToString();
                    info.FName = dtableUserInfo.Rows[0]["FName"].ToString();
                    //for name only
                    string[] tokens = info.Name.Split(' ');
                    string FirstName = tokens[0];

                    //added charmin
                    string Message = string.Empty;
                    if (info.Status == "Blocked")
                    {
                        info.Status = "Active";

                        /*Sms Message for block msg */

                        Message = "Dear" + " " + info.FName + ",\n";
                        Message += "Your account 98******** for agent has been unblocked, for detail please contact administrator"
                            + "." + "\n" + "Thank You";
                        Message += "-MNepal";


                        SMSUtils SMS = new SMSUtils();
                        SMS.SendSMS(Message, info.UserName);
                    }
                    else
                    {
                        info.Status = "Blocked";
                         

                        /*Sms Message for block msg */

                        Message = "Dear" + " " + FirstName + ",\n";
                        Message += "Your account 98******** for agent has been blocked, for detail please contact administrator"
                            + "." + "\n" + "Thank You";
                        Message += "-MNepal";

 
                        SMSUtils SMS = new SMSUtils();
                        SMS.SendSMS(Message, info.UserName);
                    }
                    //added charmin
                    info.EmailAddress = dtableUserInfo.Rows[0]["EmailAddress"].ToString();
                    info.UserName = dtableUserInfo.Rows[0]["UserName"].ToString();
                    info.UserType = dtableUserInfo.Rows[0]["UserType"].ToString();
                    info.IsApproved = dtableUserInfo.Rows[0]["IsApproved"].ToString();
                    info.IsRejected = dtableUserInfo.Rows[0]["IsRejected"].ToString();
                    info.WalletNumber = dtableUserInfo.Rows[0]["WalletNumber"].ToString();





                    string Mode = "";
                    //string Message = string.Empty;
                     Message = string.Empty;
                    string resp = string.Empty;

                   // isUpdated = AgentUtils.AgentStatusApprove(info.ClientCode);
                    isUpdated = AgentUtils.AgentStatusReject(info.ClientCode, info.Status);

                    if (isUpdated)
                    {
                        resp = "Agent Status approved successfully.";
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
                            StatusMessage = "Error Approving Agent status"
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


        //for reject 
        [HttpGet]
        public ContentResult StatusReject(string ClientCode, string Name)
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
                    //if (info.Status == "Blocked")
                    //{
                    //    info.Status = "Active";
                    //}
                    //else
                    //{
                    //    info.Status = "Blocked";
                    //}
                    info.ContactNumber1 = dtableUserInfo.Rows[0]["ContactNumber1"].ToString();
                    info.ContactNumber2 = dtableUserInfo.Rows[0]["ContactNumber2"].ToString();
                    info.UserName = dtableUserInfo.Rows[0]["UserName"].ToString();
                    info.UserType = dtableUserInfo.Rows[0]["userType"].ToString();
                    info.IsApproved = dtableUserInfo.Rows[0]["IsApproved"].ToString();
                    info.IsRejected = dtableUserInfo.Rows[0]["IsRejected"].ToString();
                    info.WalletNumber = dtableUserInfo.Rows[0]["WalletNumber"].ToString();
                    info.BankAccountNumber = dtableUserInfo.Rows[0]["BankAccountNumber"].ToString();


                    string Message = string.Empty;
                    string resp = string.Empty;

                    isUpdated = AgentUtils.AgentStatusReject(info.ClientCode, info.Status);
                    if (isUpdated)
                    {
                        resp = "Client Status rejected successfully.";
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
                            StatusMessage = "Error rejecting customer status"
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



        #endregion


        #region "Agent pin/passord reset by superadmin"

        // GET: Agent/PinResetList
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
                        DataTable dtableCustomerStatusByMobileNo = CustomerUtils.GetUserProfileByMobileNo(userInfo.ContactNumber1);
                        if (dtableCustomerStatusByMobileNo != null && dtableCustomerStatusByMobileNo.Rows.Count > 0)
                        {
                            UserInfo regobj = new UserInfo();
                            regobj.ClientCode = dtableCustomerStatusByMobileNo.Rows[0]["ClientCode"].ToString();
                            regobj.Name = dtableCustomerStatusByMobileNo.Rows[0]["Name"].ToString();
                            regobj.Address = dtableCustomerStatusByMobileNo.Rows[0]["Address"].ToString();
                            regobj.PIN = dtableCustomerStatusByMobileNo.Rows[0]["PIN"].ToString();
                            regobj.Status = dtableCustomerStatusByMobileNo.Rows[0]["Status"].ToString();
                            regobj.ContactNumber1 = dtableCustomerStatusByMobileNo.Rows[0]["ContactNumber1"].ToString();
                            regobj.ContactNumber2 = dtableCustomerStatusByMobileNo.Rows[0]["ContactNumber2"].ToString();
                            regobj.UserName = dtableCustomerStatusByMobileNo.Rows[0]["UserName"].ToString();
                            regobj.UserType = dtableCustomerStatusByMobileNo.Rows[0]["userType"].ToString();

                            CustomerList.Add(regobj);
                            ViewData["dtableCustomerList"] = dtableCustomerStatusByMobileNo;
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
                return RedirectToAction("PinResetList", "Agent");
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
                        return RedirectToAction("PinResetList", "Agent");
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


            if (TempData["userType"] != null&& checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;
                string AdminUserName = (string)Session["UserName"];
                string AdminBranch = (string)Session["UserBranch"];


                string COC = (string)Session["COC"];
                bool flag;
                bool boolCOC = Boolean.TryParse(COC, out flag);
                var model = PinUtils.GetPinResetList(AdminBranch, boolCOC, MobileNumber);
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
                DataSet DSet = ProfileUtils.GetAgentProfileInfoDS(ClientCode);
                DataTable dtableInfo = DSet.Tables["dtUserInfo"];
                
                if (dtableUserInfo != null && dtableUserInfo.Rows.Count > 0)
                {
                    info.ClientCode = dtableUserInfo.Rows[0]["ClientCode"].ToString();
                    info.FName = dtableInfo.Rows[0]["FName"].ToString();
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
                        Message = string.Format("Dear {0},\n Your new T-Pin is {1}.\n Thank You -MNepal", info.FName, info.PIN);
                        resp = "T-Pin reset Successful for " + info.UserName;

                        //for email pin only

                        string Subject = "Agent T-Pin Reset";

                        string MailSubject = "<span style='font-size:15px;'><h4>Dear " + info.FName + ",</h4>";
                        MailSubject += "Your T-Pin reset request for agent " + info.UserName + " has been acknowledged and you have been issued with a new temporary T-Pin.<br/>";

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
                        Message = string.Format("Dear {0},\n Your new Password is {1}.\n Thank You -MNepal", info.FName, info.Password);
                        resp = "Password reset Successful for " + info.UserName;

                        //for email password only

                        string Subject = "Agent Password Reset";

                        string MailSubject = "<span style='font-size:15px;'><h4>Dear " + info.FName + ",</h4>";
                        MailSubject += "Your password reset request for agent " + info.UserName + " has been acknowledged and you have been issued with a new temporary password.<br/>";

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
                        Message = string.Format("Dear {0},\n Your new T-Pin is {1} and Password is {2}.\n Thank You -MNepal", info.FName, info.PIN, info.Password);
                        resp = "T-Pin and Password reset Successful for " + info.UserName.Split()[0];

                        //for email both T-Pin and password

                        string Subject = "Agent Password/T-Pin Reset";

                        string MailSubject = "<span style='font-size:15px;'><h4>Dear " + info.FName + ",</h4>";
                        MailSubject += "Your password and T-Pin reset request for agent " + info.UserName + " has been acknowledged and you have been issued with a new temporary password and T-Pin.<br/>";

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
                            StatusMessage = "Error Approving Agent pin"
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
                             StatusMessage = "Reverted Pin/password request for " + info.Name
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

        #region For image encode and resize
        public static Image resizeImage(Image imgToResize, Size size)
        {
            return (Image)(new Bitmap(imgToResize, size));
        }

        public string ParseCv(HttpPostedFileBase cvFile)
        {
            //WebImage img = new WebImage(cvFile.InputStream);
            //img.Resize(440, 300, true, true);
            var fileName = System.IO.Path.GetFileName(cvFile.FileName);
            string randomFileName = Path.GetExtension(fileName);

            Image image = Image.FromStream(cvFile.InputStream, true, true);
            Image img = resizeImage(image, new Size(400, 300));
            using (MemoryStream m = new MemoryStream())
            {
                img.Save(m, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] imageBytes = m.ToArray();

                // Convert byte[] to Base64 String
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }

            ///original
            //byte[] fileInBytes = new byte[cvFile.ContentLength];
            //using (BinaryReader theReader = new BinaryReader(cvFile.InputStream))
            //{
            //    fileInBytes = theReader.ReadBytes(cvFile.ContentLength);
            //}
            //string fileAsString = Convert.ToBase64String(Compress(fileInBytes));
            //return fileAsString;
        }

        public string ReturnFileName(HttpPostedFileBase file, string mobile)
        {

            if (file == null)
                return null;

            if (IsImage(file) == false)
                return null;

            int iFileSize = file.ContentLength;
            if (iFileSize > 1048576)
                return null;

            var fileName = System.IO.Path.GetFileName(file.FileName);
            string randomFileName = mobile + "_" + Path.GetFileNameWithoutExtension(fileName) +
                                "_" + Guid.NewGuid().ToString().Substring(0, 4) + ".jpeg";
            return randomFileName;
        }
        #endregion

        #region Calling API
        //Sending image to API
        public async Task<ActionResult> SavePhoto(UserInfo srInfo)
        {

            string userName = (string)Session["LOGGED_USERNAME"];
            string mobile = (string)Session["Mobile"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;
            if (TempData["userType"] != null)
            {
                HttpResponseMessage _res = new HttpResponseMessage();

                TraceIdGenerator _tig = new TraceIdGenerator();
                var tid = _tig.GenerateTraceID();

                using (HttpClient client = new HttpClient())
                {
                    var action = "selfReg.svc/SaveImageAPI";
                    var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                    var content = new FormUrlEncodedContent(new[]{
                        new KeyValuePair<string, string>("mobile", srInfo.UserName),
                        new KeyValuePair<string,string>("frontImage", srInfo.FrontImage),
                        new KeyValuePair<string,string>("backImage",srInfo.BackImage),
                        new KeyValuePair<string,string>("ppImage", srInfo.PassportImage),
                        new KeyValuePair<string,string>("frontImageName", srInfo.FrontImageName),
                        new KeyValuePair<string,string>("backImageName", srInfo.BackImageName),
                        new KeyValuePair<string,string>("ppImageName", srInfo.PassportImageName),
                        new KeyValuePair<string,string>("DocumentType", srInfo.Document),
                    });

                    _res = await client.PostAsync(new Uri(uri), content);
                    string responseBody = _res.StatusCode.ToString() + " ," + await _res.Content.ReadAsStringAsync();
                    _res.ReasonPhrase = responseBody;
                    string errorMessage = string.Empty;
                    int responseCode = 0;
                    string message = string.Empty;
                    string responsetext = string.Empty;
                    bool result = false;
                    string ava = string.Empty;
                    string avatra = string.Empty;
                    string avamsg = string.Empty;
                    string PP = string.Empty;
                    try
                    {
                        if (_res.IsSuccessStatusCode)
                        {
                            result = true;
                            responseCode = (int)_res.StatusCode;
                            responsetext = await _res.Content.ReadAsStringAsync();
                            message = _res.Content.ReadAsStringAsync().Result;
                            string respmsg = "";
                            if (!string.IsNullOrEmpty(message))
                            {
                                JavaScriptSerializer ser = new JavaScriptSerializer();
                                var json = ser.Deserialize<JsonParse>(responsetext);
                                message = json.d;
                                JsonParse myNames = ser.Deserialize<JsonParse>(json.d);
                                int code = Convert.ToInt32(myNames.StatusCode);
                                respmsg = myNames.StatusMessage;

                                UserInfo userInfo = new UserInfo();
                                PP = myNames.PP;
                                Session["PP"] = myNames.PP;
                                Session["Front"] = myNames.front;
                                Session["Back"] = myNames.back;

                                if (code != responseCode)
                                {
                                    responseCode = code;
                                }
                            }
                            return Json(new { responseCode = responseCode, responseText = respmsg, PP = PP },
                            JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            result = false;
                            responseCode = (int)_res.StatusCode;
                            responsetext = await _res.Content.ReadAsStringAsync();
                            dynamic json = JValue.Parse(responsetext);
                            message = json.d;
                            if (message == null)
                            {
                                return Json(new { responseCode = responseCode, responseText = responsetext },
                            JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                dynamic item = JValue.Parse(message);

                                return Json(new { responseCode = responseCode, responseText = (string)item["StatusMessage"] },
                                JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return Json(new { responseCode = "400", responseText = ex.Message },
                            JsonRequestBehavior.AllowGet);
                    }
                    this.TempData["fundTransfer_messsage"] = result
                                                    ? "Customer created successfully" + message
                                                    : "ERROR :: " + message;
                    this.TempData["message_class"] = result ? "success_info" : "failed_info";

                    return RedirectToAction("Index", "AgentDashboard");
                }
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }


        }
        #endregion

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





        #region Modification Rejected--- 

        [HttpGet]
        public ActionResult ModificationRejectedList(string Key, string Value)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            string UserName = Value;

            if (this.TempData["agentmodify_messsage"] != null)
            {
                this.ViewData["agentmodify_messsage"] = this.TempData["agentmodify_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            //Check Role link start    
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkRole(System.Reflection.MethodBase.GetCurrentMethod().Name, clientCode);
            //Check Role link end


            TempData["userType"] = userType;
            if (TempData["userType"] != null && checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;


                List<UserInfo> agentRegApprove = new List<UserInfo>();
                var agentInfo = AgentUtils.GetAgentModReject("agent", UserName);



                ViewBag.Value = Value;
                return View(agentInfo);

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }





        // GET: Agent/AgentModify/5
        public ActionResult AgentModifyRejected(string id)
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
            
            if (this.TempData["agentmodify_messsage"] != null)
            {
                this.ViewData["agentmodify_messsage"] = this.TempData["agentmodify_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                UserInfo regobj = new UserInfo();
                DataSet DSet = ProfileUtils.GetAgentProfileInfoDS(id);

                DataTable dtableUserInfo = DSet.Tables["dtUserInfo"];
                DataTable dKycInfo = DSet.Tables["dtKycInfo"];
                DataTable dKycDoc = DSet.Tables["dtKycDoc"];

               
                if (dtableUserInfo != null && dtableUserInfo.Rows.Count > 0)
                {
                    regobj.ClientCode = dtableUserInfo.Rows[0]["ClientCode"].ToString();
                    regobj.Name = dtableUserInfo.Rows[0]["Name"].ToString();
                    regobj.Address = dtableUserInfo.Rows[0]["Address"].ToString();

                    regobj.PProvince = dtableUserInfo.Rows[0]["PProvince"].ToString();
                    regobj.PDistrict = dtableUserInfo.Rows[0]["PDistrict"].ToString();
                    regobj.PMunicipalityVDC = dtableUserInfo.Rows[0]["PMunicipalityVDC"].ToString();
                    regobj.PWardNo = dtableUserInfo.Rows[0]["PWardNo"].ToString();
                    regobj.PHouseNo = dtableUserInfo.Rows[0]["PHouseNo"].ToString();
                    regobj.PStreet = dtableUserInfo.Rows[0]["PStreet"].ToString();

                    regobj.CProvince = dtableUserInfo.Rows[0]["CProvince"].ToString();
                    regobj.CDistrict = dtableUserInfo.Rows[0]["CDistrict"].ToString();
                    regobj.CMunicipalityVDC = dtableUserInfo.Rows[0]["CMunicipalityVDC"].ToString();
                    regobj.CWardNo = dtableUserInfo.Rows[0]["CWardNo"].ToString();
                    regobj.CHouseNo = dtableUserInfo.Rows[0]["CHouseNo"].ToString();
                    regobj.CStreet = dtableUserInfo.Rows[0]["CStreet"].ToString();



                    regobj.IsApproved = dtableUserInfo.Rows[0]["IsApproved"].ToString();
                    regobj.IsRejected = dtableUserInfo.Rows[0]["IsRejected"].ToString();
                    regobj.IsModified = dtableUserInfo.Rows[0]["IsModified"].ToString();

                    regobj.Status = dtableUserInfo.Rows[0]["Status"].ToString();
                    regobj.UserName = dtableUserInfo.Rows[0]["UserName"].ToString();
                    regobj.UserType = dtableUserInfo.Rows[0]["UserType"].ToString();
                    regobj.PIN = dtableUserInfo.Rows[0]["PIN"].ToString();
                    regobj.WalletNumber = dtableUserInfo.Rows[0]["WalletNumber"].ToString();

                    regobj.FName = dtableUserInfo.Rows[0]["FName"].ToString();
                    regobj.MName = dtableUserInfo.Rows[0]["MName"].ToString();
                    regobj.LName = dtableUserInfo.Rows[0]["LName"].ToString();
                    regobj.Gender = dtableUserInfo.Rows[0]["Gender"].ToString();
                    regobj.DOB = dtableUserInfo.Rows[0]["DateOfBirth"].ToString();
                    //regobj.DOB = DateTime.Parse(regobj.DOB.Trim()).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    regobj.DOB = DateTime.Parse(regobj.DOB.Trim()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                    regobj.BSDateOfBirth = dtableUserInfo.Rows[0]["BSDateOfBirth"].ToString();

                    regobj.EmailAddress = dtableUserInfo.Rows[0]["EmailAddress"].ToString();
                    regobj.Nationality = dtableUserInfo.Rows[0]["Nationality"].ToString();
                    regobj.Country = dtableUserInfo.Rows[0]["Country"].ToString();
                    regobj.Occupation = dtableUserInfo.Rows[0]["Occupation"].ToString();
                    regobj.MaritalStatus = dtableUserInfo.Rows[0]["MaritalStatus"].ToString();
                    regobj.SpouseName = dtableUserInfo.Rows[0]["SpouseName"].ToString();

                    regobj.FatherInLaw = dtableUserInfo.Rows[0]["FatherInLaw"].ToString();

                    regobj.FatherName = dtableUserInfo.Rows[0]["FathersName"].ToString();
                    regobj.MotherName = dtableUserInfo.Rows[0]["MothersName"].ToString();
                    regobj.GrandFatherName = dtableUserInfo.Rows[0]["GFathersName"].ToString();
                    regobj.PanNo = dtableUserInfo.Rows[0]["PANNumber"].ToString();


                    regobj.Document = dtableUserInfo.Rows[0]["DocType"].ToString();
                    regobj.Citizenship = dtableUserInfo.Rows[0]["CitizenshipNo"].ToString();
                    regobj.CitizenshipIssueDate = dtableUserInfo.Rows[0]["CitizenIssueDate"].ToString().Split()[0];
                    //regobj.CitizenshipIssueDate = DateTime.Parse(regobj.CitizenshipIssueDate.Trim()).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    regobj.CitizenshipIssueDate = DateTime.Parse(regobj.CitizenshipIssueDate.Trim()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

                    regobj.BSCitizenshipIssueDate = dtableUserInfo.Rows[0]["BSCitizenIssueDate"].ToString().Split()[0];
                    regobj.CitizenshipPlaceOfIssue = dtableUserInfo.Rows[0]["CitizenPlaceOfIssue"].ToString();

                    regobj.License = dtableUserInfo.Rows[0]["LicenseNo"].ToString();
                    regobj.LicenseIssueDate = dtableUserInfo.Rows[0]["LicenseIssueDate"].ToString().Split()[0];
                    //regobj.LicenseIssueDate = DateTime.Parse(regobj.LicenseIssueDate.Trim()).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    regobj.LicenseIssueDate = DateTime.Parse(regobj.LicenseIssueDate.Trim()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

                    regobj.BSLicenseIssueDate = dtableUserInfo.Rows[0]["BSLicenseIssueDate"].ToString().Split()[0];
                    regobj.LicenseExpireDate = dtableUserInfo.Rows[0]["LicenseExpiryDate"].ToString().Split()[0];
                    //regobj.LicenseExpireDate = DateTime.Parse(regobj.LicenseExpireDate.Trim()).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    regobj.LicenseExpireDate = DateTime.Parse(regobj.LicenseExpireDate.Trim()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

                    regobj.BSLicenseExpireDate = dtableUserInfo.Rows[0]["BSLicenseExpiryDate"].ToString().Split()[0];

                    regobj.LicensePlaceOfIssue = dtableUserInfo.Rows[0]["LicensePlaceOfIssue"].ToString();

                    regobj.Passport = dtableUserInfo.Rows[0]["PassportNo"].ToString();
                    regobj.PassportPlaceOfIssue = dtableUserInfo.Rows[0]["PassportPlaceOfIssue"].ToString();
                    regobj.PassportIssueDate = dtableUserInfo.Rows[0]["PassportIssueDate"].ToString().Split()[0];
                    //  regobj.PassportIssueDate = DateTime.Parse(regobj.PassportIssueDate.Trim()).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    regobj.PassportIssueDate = DateTime.Parse(regobj.PassportIssueDate.Trim()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

                    regobj.PassportExpireDate = dtableUserInfo.Rows[0]["PassportExpiryDate"].ToString().Split()[0];
                    //regobj.PassportExpireDate = DateTime.Parse(regobj.PassportExpireDate.Trim()).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    regobj.PassportExpireDate = DateTime.Parse(regobj.PassportExpireDate.Trim()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

                    regobj.PassportImage = dtableUserInfo.Rows[0]["PassportImage"].ToString();
                    regobj.FrontImage = dtableUserInfo.Rows[0]["FrontImage"].ToString();
                    regobj.BackImage = dtableUserInfo.Rows[0]["BackImage"].ToString();

                }
                else
                {
                    this.TempData["custmodify_messsage"] = "User not found";
                    this.TempData["message_class"] = CssSetting.FailedMessageClass;
                    return RedirectToAction("Index");

                }

                ViewBag.PProvince = new SelectList(list, "Value", "Text", regobj.PProvince);
                ViewBag.CProvince = new SelectList(list, "Value", "Text", regobj.CProvince);

                ViewBag.PDistrictID = new SelectList(ProvinceToDistrict(regobj.PProvince), "Value", "Text", regobj.PDistrict);
                ViewBag.CDistrictID = new SelectList(ProvinceToDistrict(regobj.CProvince), "Value", "Text", regobj.CDistrict);

                return View(regobj);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        [HttpPost]
        public async Task<ActionResult> AgentModifyRejected(string id, string btnCommand, FormCollection collection, UserInfo model)
        {
            string Id = "";
            string displayMessage = "";
            string messageClass = "";
            UserInfo userInfo = new UserInfo();
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

                    if (btnCommand == "Submit")
                    {
                        string cClientCode = collection["ClientCode"].ToString();
                        Id = cClientCode;
                        //string cAddress = collection["Address"].ToString();
                        //string cName = collection["Name"].ToString();
                        string cWalletNumber = collection["WalletNumber"].ToString();
                        //string cIsApproved = collection["IsApproved"].ToString();
                        string cUserName = collection["txtUserName"].ToString();
                        string cFName = collection["txtFirstName"].ToString();
                        string cMName = collection["txtMiddleName"].ToString();
                        string cLName = collection["txtLastName"].ToString();
                        string cGender = collection["Gender"].ToString();
                        string cDOB = collection["DOB"].ToString();
                        string cBSDateOfBirth = collection["BSDateOfBirth"].ToString();
                        string cEmailAddress = collection["Email"].ToString();
                        string cNationality = collection["Nationality"].ToString();
                        string cCountry = collection["Country"].ToString();
                        string cFatherName = collection["txtFatherName"].ToString();
                        string cMotherName = collection["txtMotherName"].ToString();
                        string cMaritalStatus = collection["MaritalStatus"].ToString();
                        string cSpouseName = collection["SpouseName"].ToString();
                        string cFatherInLaw = collection["FatherInLaw"].ToString();
                        string cGrandFatherName = collection["txtGrandFatherName"].ToString();
                        string cOccupation = collection["Occupation"].ToString();
                        string cPanNo = collection["txtPanNo"].ToString();

                        string cPProvince = collection["PProvinceText"].ToString();
                        string cPDistrict = collection["PDistrictText"].ToString();
                        string cPVDC = collection["txtPVDC"].ToString();
                        string cPHouseNo = collection["txtPHouseNo"].ToString();
                        string cPWardNo = collection["txtPWardNo"].ToString();
                        string cPStreet = collection["PStreet"].ToString();

                        string cCProvince = collection["CProvinceText"].ToString();
                        string cCDistrict = collection["CDistrictText"].ToString();
                        string cCVDC = collection["txtCVDC"].ToString();
                        string cCHouseNo = collection["txtCHouseNo"].ToString();
                        string cCWardNo = collection["txtCWardNo"].ToString();
                        string cCStreet = collection["CStreet"].ToString();

                        string cDocument = collection["Document"].ToString();
                        string cCitizenship = collection["txtCitizenship"].ToString();
                        string cCitizenshipIssueDate = collection["CitizenshipIssueDate"].ToString();
                        string cBSCitizenshipIssueDate = collection["BSCitizenshipIssueDate"].ToString();
                        string cCitizenshipPlaceOfIssue = collection["txtCitizenshipPlaceOfIssue"].ToString();


                        string cLicense = collection["txtLicense"].ToString();
                        string cLicenseIssueDate = collection["LicenseIssueDate"].ToString();
                        string cBSLicenseIssueDate = collection["BSLicenseIssueDate"].ToString();
                        string cLicenseExpireDate = collection["LicenseExpireDate"].ToString();
                        string cBSLicenseExpireDate = collection["BSLicenseExpireDate"].ToString();
                        string cLicensePlaceOfIssue = collection["txtLicensePlaceOfIssue"].ToString();

                        string cPassport = collection["txtPassport"].ToString();
                        string cPassportIssueDate = collection["txtPassportIssueDate"].ToString();
                        string cPassportExpireDate = collection["txtPassportExpireDate"].ToString();
                        string cPassportPlaceOfIssue = collection["txtPassportPlaceOfIssue"].ToString();

                        //Images OLD Value//
                        string cPassportPhoto = collection["txtOldPP"].ToString();
                        string cFrontPhoto = collection["txtOldfront"].ToString();
                        string cBackPhoto = collection["txtOldback"].ToString();
                        userInfoModify.ClientCode = cClientCode;

                        userInfoModify.Name = cFName + " " + cMName + " " + cLName;
                        userInfoModify.WalletNumber = cWalletNumber;
                        userInfoModify.UserType = "agent";
                        userInfoModify.Status = "InActive";
                        userInfoModify.IsApproved = "UnApprove";
                        userInfoModify.IsRejected = "F";
                        userInfoModify.IsModified = "T";
                        userInfoModify.UserName = cUserName;
                        userInfoModify.Address = cPVDC + "," + getDistrictName(cPDistrict) + ", Province " + cPProvince;   /*+ cPWardNo + "," + cPHouseNo + "," + cPStreet*/
                        userInfoModify.FName = cFName;
                        userInfoModify.MName = cMName;
                        userInfoModify.LName = cLName;
                        userInfoModify.Gender = cGender;
                        userInfoModify.DOB = cDOB;
                        userInfoModify.BSDateOfBirth = cBSDateOfBirth;
                        userInfoModify.EmailAddress = cEmailAddress;
                        userInfoModify.Nationality = cNationality;
                        userInfoModify.Country = cCountry;
                        userInfoModify.Occupation = cOccupation;
                        userInfoModify.MaritalStatus = cMaritalStatus;
                        userInfoModify.SpouseName = cSpouseName;
                        userInfoModify.FatherInLaw = cFatherInLaw;
                        userInfoModify.PanNo = cPanNo;
                        userInfoModify.FatherName = cFatherName;
                        userInfoModify.MotherName = cMotherName;
                        userInfoModify.GrandFatherName = cGrandFatherName;

                        userInfoModify.PProvince = cPProvince;
                        userInfoModify.PDistrict = cPDistrict;
                        userInfoModify.PVDC = cPVDC;
                        userInfoModify.PWardNo = cPWardNo;
                        userInfoModify.PHouseNo = cPHouseNo;
                        userInfoModify.PStreet = cPStreet;

                        userInfoModify.CProvince = cCProvince;
                        userInfoModify.CDistrict = cCDistrict;
                        userInfoModify.CVDC = cCVDC;
                        userInfoModify.CWardNo = cCWardNo;
                        userInfoModify.CHouseNo = cCHouseNo;
                        userInfoModify.CStreet = cCStreet;

                        userInfoModify.Document = cDocument;
                        userInfoModify.Citizenship = cCitizenship;
                        userInfoModify.CitizenshipIssueDate = cCitizenshipIssueDate;
                        userInfoModify.BSCitizenshipIssueDate = cBSCitizenshipIssueDate;
                        userInfoModify.CitizenshipPlaceOfIssue = cCitizenshipPlaceOfIssue;

                        userInfoModify.License = cLicense;
                        userInfoModify.LicenseIssueDate = cLicenseIssueDate;
                        userInfoModify.LicenseExpireDate = cLicenseExpireDate;
                        userInfoModify.BSLicenseIssueDate = cBSLicenseIssueDate;
                        userInfoModify.BSLicenseExpireDate = cBSLicenseExpireDate;
                        userInfoModify.LicensePlaceOfIssue = cLicensePlaceOfIssue;

                        userInfoModify.Passport = cPassport;
                        userInfoModify.PassportIssueDate = cPassportIssueDate;
                        userInfoModify.PassportExpireDate = cPassportExpireDate;
                        userInfoModify.PassportPlaceOfIssue = cPassportPlaceOfIssue;

                        if (model.PassportPhoto != null)
                        {
                            if (!string.IsNullOrEmpty(cPassportPhoto))
                            {
                                var existingFile = Request.MapPath(cPassportPhoto);
                                if (System.IO.File.Exists(existingFile))
                                {
                                    System.IO.File.Delete(existingFile);
                                }
                            }
                            var PP = ReturnFileName(model.PassportPhoto, userInfoModify.UserName);
                            userInfoModify.PassportImage = ParseCv(model.PassportPhoto);
                            userInfoModify.PassportImageName = string.Format(PP);
                        }
                        else
                        {
                            userInfoModify.PassportImageName = "";
                        }


                        if (model.Front != null)
                        {
                            if (!string.IsNullOrEmpty(cFrontPhoto))
                            {

                                var existingFrontFile = Request.MapPath(cFrontPhoto);
                                if (System.IO.File.Exists(existingFrontFile))
                                {
                                    System.IO.File.Delete(existingFrontFile);
                                }
                            }
                            var front = ReturnFileName(model.Front, userInfoModify.UserName);
                            userInfoModify.FrontImage = ParseCv(model.Front);
                            userInfoModify.FrontImageName = string.Format(front);
                        }
                        else
                        {
                            userInfoModify.FrontImageName = "";
                        }


                        if (model.Back != null)
                        {
                            if (!string.IsNullOrEmpty(cBackPhoto))
                            {

                                var existingBackFile = Request.MapPath(cBackPhoto);
                                if (System.IO.File.Exists(existingBackFile))
                                {
                                    System.IO.File.Delete(existingBackFile);
                                }
                            }
                            var back = ReturnFileName(model.Back, userInfoModify.UserName);
                            userInfoModify.BackImage = ParseCv(model.Back);
                            userInfoModify.BackImageName = string.Format(back);

                        }
                        else
                        {
                            userInfoModify.BackImageName = "";
                        }

                        await SavePhoto(userInfoModify);
                        string sPP = Session["PP"].ToString();
                        string sFront = Session["Front"].ToString();
                        string sBack = Session["Back"].ToString();

                        if (sPP != null && sPP != "")
                        {
                            userInfoModify.PassportImageName = Session["PP"].ToString();
                        }
                        else
                        {
                            userInfoModify.PassportImageName = cPassportPhoto;
                        }
                        if (sFront != null && sFront != "")
                        {
                            userInfoModify.FrontImageName = Session["Front"].ToString();
                        }
                        else
                        {
                            userInfoModify.FrontImageName = cFrontPhoto;
                        }
                        if (sBack != null && sBack != "")
                        {
                            userInfoModify.BackImageName = Session["Back"].ToString();
                        }
                        else
                        {
                            userInfoModify.BackImageName = cBackPhoto;
                        }

                      
                        /*map new data to Agent*/
                        AgentTable NewAgent = new AgentTable();
                        MNClient NewMNClient = new MNClient();
                        MNClientContact NewMNClientContact = new MNClientContact();
                        MNClientKYC NewMNClientKYC = new MNClientKYC();
                        MNClientKYCDoc NewMNClientKYCDoc = new MNClientKYCDoc();

                        NewMNClient.Address = userInfoModify.Address;
                        NewMNClient.Name = userInfoModify.Name;

                        NewMNClientContact.EmailAddress = userInfoModify.EmailAddress;
                        NewMNClientContact.ContactNumber1 = userInfoModify.ContactNumber1;

                        NewMNClientKYC.FName = userInfoModify.FName;
                        NewMNClientKYC.MName = userInfoModify.MName;
                        NewMNClientKYC.LName = userInfoModify.LName;
                        NewMNClientKYC.Gender = userInfoModify.Gender;
                        NewMNClientKYC.DateOfBirth = DateTime.ParseExact(userInfoModify.DOB, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        NewMNClientKYC.BSDateOfBirth = userInfoModify.BSDateOfBirth;
                        NewMNClientKYC.Nationality = userInfoModify.Nationality;
                        NewMNClientKYC.Country = userInfoModify.Country;
                        NewMNClientKYC.Occupation = userInfoModify.Occupation;
                        NewMNClientKYC.MaritalStatus = userInfoModify.MaritalStatus;
                        NewMNClientKYC.SpouseName = userInfoModify.SpouseName;
                        NewMNClientKYC.FatherInLaw = userInfoModify.FatherInLaw;
                        NewMNClientKYC.FathersName = userInfoModify.FatherName;
                        NewMNClientKYC.MothersName = userInfoModify.MotherName;
                        NewMNClientKYC.GFathersName = userInfoModify.GrandFatherName;
                        NewMNClientKYC.PANNumber = userInfoModify.PanNo;

                        NewMNClientKYC.PProvince = userInfoModify.PProvince;
                        NewMNClientKYC.PDistrict = userInfoModify.PDistrict;
                        NewMNClientKYC.PMunicipalityVDC = userInfoModify.PVDC;
                        NewMNClientKYC.PHouseNo = userInfoModify.PHouseNo;
                        NewMNClientKYC.PWardNo = userInfoModify.PWardNo;
                        NewMNClientKYC.PStreet = userInfoModify.PStreet;

                        NewMNClientKYC.CProvince = userInfoModify.CProvince;
                        NewMNClientKYC.CDistrict = userInfoModify.CDistrict;
                        NewMNClientKYC.CMunicipalityVDC = userInfoModify.CVDC;
                        NewMNClientKYC.CHouseNo = userInfoModify.CHouseNo;
                        NewMNClientKYC.CWardNo = userInfoModify.CWardNo;
                        NewMNClientKYC.CStreet = userInfoModify.CStreet;

                        NewMNClientKYC.CitizenshipNo = userInfoModify.Citizenship;
                        NewMNClientKYC.CitizenPlaceOfIssue = userInfoModify.CitizenshipPlaceOfIssue;
                        NewMNClientKYC.CitizenIssueDate = DateTime.ParseExact(userInfoModify.CitizenshipIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        NewMNClientKYC.BSCitizenIssueDate = userInfoModify.BSCitizenshipIssueDate;

                        NewMNClientKYC.LicenseNo = userInfoModify.License;
                        NewMNClientKYC.LicensePlaceOfIssue = userInfoModify.LicensePlaceOfIssue;
                        NewMNClientKYC.LicenseIssueDate = DateTime.ParseExact(userInfoModify.LicenseIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        NewMNClientKYC.BSLicenseIssueDate = userInfoModify.BSLicenseIssueDate;
                        NewMNClientKYC.LicenseExpiryDate = DateTime.ParseExact(userInfoModify.LicenseExpireDate, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        NewMNClientKYC.BSLicenseExpiryDate = userInfoModify.BSLicenseExpireDate;

                        NewMNClientKYC.PassportNo = userInfoModify.Passport;
                        NewMNClientKYC.PassportPlaceOfIssue = userInfoModify.PassportPlaceOfIssue;
                        NewMNClientKYC.PassportIssueDate = DateTime.ParseExact(userInfoModify.PassportIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        NewMNClientKYC.PassportExpiryDate = DateTime.ParseExact(userInfoModify.PassportExpireDate, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                        NewMNClientKYCDoc.DocType = userInfoModify.Document;
                        NewMNClientKYCDoc.PassportImage = userInfoModify.PassportImageName;
                        NewMNClientKYCDoc.FrontImage = userInfoModify.FrontImageName;
                        NewMNClientKYCDoc.BackImage = userInfoModify.BackImageName;

                        /*map NEW data to Agent--END*/

                        NewAgent.MNClient = NewMNClient;
                        NewAgent.MNClientContact = NewMNClientContact;
                        NewAgent.MNClientKYC = NewMNClientKYC;
                        NewAgent.MNClientKYCDoc = NewMNClientKYCDoc;
                        
                        //For Province
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
                        //end Province

                        UserInfo regobj = new UserInfo();
                       
                        DataSet DSet = ProfileUtils.GetAgentProfileInfoDS(id);

                        DataTable dtableUserInfo = DSet.Tables["dtUserInfo"];
                        DataTable dKycInfo = DSet.Tables["dtKycInfo"];
                        DataTable dKycDoc = DSet.Tables["dtKycDoc"];


                        if (dtableUserInfo != null && dtableUserInfo.Rows.Count > 0)
                        {
                            regobj.ClientCode = dtableUserInfo.Rows[0]["ClientCode"].ToString();
                            regobj.Name = dtableUserInfo.Rows[0]["Name"].ToString();
                            regobj.Address = dtableUserInfo.Rows[0]["Address"].ToString();

                            regobj.PProvince = dtableUserInfo.Rows[0]["PProvince"].ToString();
                            regobj.PDistrict = dtableUserInfo.Rows[0]["PDistrict"].ToString();
                            regobj.PMunicipalityVDC = dtableUserInfo.Rows[0]["PMunicipalityVDC"].ToString();
                            regobj.PWardNo = dtableUserInfo.Rows[0]["PWardNo"].ToString();
                            regobj.PHouseNo = dtableUserInfo.Rows[0]["PHouseNo"].ToString();
                            regobj.PStreet = dtableUserInfo.Rows[0]["PStreet"].ToString();

                            regobj.CProvince = dtableUserInfo.Rows[0]["CProvince"].ToString();
                            regobj.CDistrict = dtableUserInfo.Rows[0]["CDistrict"].ToString();
                            regobj.CMunicipalityVDC = dtableUserInfo.Rows[0]["CMunicipalityVDC"].ToString();
                            regobj.CWardNo = dtableUserInfo.Rows[0]["CWardNo"].ToString();
                            regobj.CHouseNo = dtableUserInfo.Rows[0]["CHouseNo"].ToString();
                            regobj.CStreet = dtableUserInfo.Rows[0]["CStreet"].ToString();



                            regobj.IsApproved = dtableUserInfo.Rows[0]["IsApproved"].ToString();
                            regobj.IsRejected = dtableUserInfo.Rows[0]["IsRejected"].ToString();
                            regobj.IsModified = dtableUserInfo.Rows[0]["IsModified"].ToString();

                            regobj.Status = dtableUserInfo.Rows[0]["Status"].ToString();
                            regobj.UserName = dtableUserInfo.Rows[0]["UserName"].ToString();
                            regobj.UserType = dtableUserInfo.Rows[0]["UserType"].ToString();
                            regobj.PIN = dtableUserInfo.Rows[0]["PIN"].ToString();
                            regobj.WalletNumber = dtableUserInfo.Rows[0]["WalletNumber"].ToString();

                            regobj.FName = dtableUserInfo.Rows[0]["FName"].ToString();
                            regobj.MName = dtableUserInfo.Rows[0]["MName"].ToString();
                            regobj.LName = dtableUserInfo.Rows[0]["LName"].ToString();
                            regobj.Gender = dtableUserInfo.Rows[0]["Gender"].ToString();
                            regobj.DOB = dtableUserInfo.Rows[0]["DateOfBirth"].ToString().Split()[0];
                            regobj.BSDateOfBirth = dtableUserInfo.Rows[0]["BSDateOfBirth"].ToString();

                            regobj.EmailAddress = dtableUserInfo.Rows[0]["EmailAddress"].ToString();
                            regobj.Nationality = dtableUserInfo.Rows[0]["Nationality"].ToString();
                            regobj.Country = dtableUserInfo.Rows[0]["Country"].ToString();
                            regobj.Occupation = dtableUserInfo.Rows[0]["Occupation"].ToString();
                            regobj.MaritalStatus = dtableUserInfo.Rows[0]["MaritalStatus"].ToString();
                            regobj.SpouseName = dtableUserInfo.Rows[0]["SpouseName"].ToString();
                            regobj.FatherInLaw = dtableUserInfo.Rows[0]["FatherInLaw"].ToString();
                            regobj.FatherName = dtableUserInfo.Rows[0]["FathersName"].ToString();
                            regobj.MotherName = dtableUserInfo.Rows[0]["MothersName"].ToString();
                            regobj.GrandFatherName = dtableUserInfo.Rows[0]["GFathersName"].ToString();
                            regobj.PanNo = dtableUserInfo.Rows[0]["PANNumber"].ToString();

                            regobj.Document = dtableUserInfo.Rows[0]["DocType"].ToString();
                            regobj.Citizenship = dtableUserInfo.Rows[0]["CitizenshipNo"].ToString();
                            regobj.CitizenshipIssueDate = dtableUserInfo.Rows[0]["CitizenIssueDate"].ToString().Split()[0];
                            regobj.BSCitizenshipIssueDate = dtableUserInfo.Rows[0]["BSCitizenIssueDate"].ToString().Split()[0];
                            regobj.CitizenshipPlaceOfIssue = dtableUserInfo.Rows[0]["CitizenPlaceOfIssue"].ToString();

                            regobj.License = dtableUserInfo.Rows[0]["LicenseNo"].ToString();
                            regobj.LicenseIssueDate = dtableUserInfo.Rows[0]["LicenseIssueDate"].ToString().Split()[0];
                           
                            regobj.BSLicenseIssueDate = dtableUserInfo.Rows[0]["BSLicenseIssueDate"].ToString().Split()[0];
                            regobj.LicenseExpireDate = dtableUserInfo.Rows[0]["LicenseExpiryDate"].ToString().Split()[0];
                            regobj.BSLicenseExpireDate = dtableUserInfo.Rows[0]["BSLicenseExpiryDate"].ToString().Split()[0];
                            regobj.LicensePlaceOfIssue = dtableUserInfo.Rows[0]["LicensePlaceOfIssue"].ToString();

                            regobj.Passport = dtableUserInfo.Rows[0]["PassportNo"].ToString();
                            regobj.PassportPlaceOfIssue = dtableUserInfo.Rows[0]["PassportPlaceOfIssue"].ToString();
                            regobj.PassportIssueDate = dtableUserInfo.Rows[0]["PassportIssueDate"].ToString().Split()[0];
                            regobj.PassportExpireDate = dtableUserInfo.Rows[0]["PassportExpiryDate"].ToString().Split()[0];
                            
                            regobj.PassportImage = dtableUserInfo.Rows[0]["PassportImage"].ToString();
                            regobj.FrontImage = dtableUserInfo.Rows[0]["FrontImage"].ToString();
                            regobj.BackImage = dtableUserInfo.Rows[0]["BackImage"].ToString();

                        }


                        /*map old data to Agent*/
                        AgentTable OldAgent = new AgentTable();
                        MNClient OldMNClient = new MNClient();
                        MNClientContact OldMNClientContact = new MNClientContact();
                        MNClientKYC OldMNClientKYC = new MNClientKYC();
                        MNClientKYCDoc OldMNClientKYCDoc = new MNClientKYCDoc();
                       
                        OldMNClient.Address = regobj.Address;
                        OldMNClient.Name = regobj.Name;

                        //OldMNClientContact.UserName = regobj.ContactNumber1;
                        OldMNClientContact.EmailAddress = regobj.EmailAddress;
                        OldMNClientContact.ContactNumber1 = regobj.ContactNumber1;



                        OldMNClientKYC.FName = regobj.FName;
                        OldMNClientKYC.MName = regobj.MName;
                        OldMNClientKYC.LName = regobj.LName;
                        OldMNClientKYC.Gender = regobj.Gender;
                        OldMNClientKYC.DateOfBirth = DateTime.Parse(regobj.DOB.Trim()).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        OldMNClientKYC.BSDateOfBirth = regobj.BSDateOfBirth;
                        OldMNClientKYC.Nationality = regobj.Nationality;
                        OldMNClientKYC.Country = regobj.Country;
                        OldMNClientKYC.Occupation = regobj.Occupation;
                        OldMNClientKYC.MaritalStatus = regobj.MaritalStatus;
                        OldMNClientKYC.SpouseName = regobj.SpouseName;
                        OldMNClientKYC.FatherInLaw = regobj.FatherInLaw;
                        OldMNClientKYC.FathersName = regobj.FatherName;
                        OldMNClientKYC.MothersName = regobj.MotherName;
                        OldMNClientKYC.GFathersName = regobj.GrandFatherName;
                        OldMNClientKYC.PANNumber = regobj.PanNo;

                        OldMNClientKYC.PProvince = regobj.PProvince;
                        OldMNClientKYC.PDistrict = regobj.PDistrict;
                        OldMNClientKYC.PMunicipalityVDC = regobj.PMunicipalityVDC;
                        OldMNClientKYC.PHouseNo = regobj.PHouseNo;
                        OldMNClientKYC.PWardNo = regobj.PWardNo;
                        OldMNClientKYC.PStreet = regobj.PStreet;

                        OldMNClientKYC.CProvince = regobj.CProvince;
                        OldMNClientKYC.CDistrict = regobj.CDistrict;
                        OldMNClientKYC.CMunicipalityVDC = regobj.CMunicipalityVDC;
                        OldMNClientKYC.CHouseNo = regobj.CHouseNo;
                        OldMNClientKYC.CWardNo = regobj.CWardNo;
                        OldMNClientKYC.CStreet = regobj.CStreet;


                        OldMNClientKYC.CitizenshipNo = regobj.Citizenship;
                        OldMNClientKYC.CitizenPlaceOfIssue = regobj.CitizenshipPlaceOfIssue;
                        OldMNClientKYC.CitizenIssueDate = DateTime.Parse(regobj.CitizenshipIssueDate.Trim()).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        OldMNClientKYC.BSCitizenIssueDate = regobj.BSCitizenshipIssueDate;

                        OldMNClientKYC.LicenseNo = regobj.License;
                        OldMNClientKYC.LicensePlaceOfIssue = regobj.LicensePlaceOfIssue;
                        OldMNClientKYC.LicenseIssueDate = DateTime.Parse(regobj.LicenseIssueDate.Trim()).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        OldMNClientKYC.BSLicenseIssueDate = regobj.BSLicenseIssueDate;
                        OldMNClientKYC.LicenseExpiryDate = DateTime.Parse(regobj.LicenseExpireDate.Trim()).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        OldMNClientKYC.BSLicenseExpiryDate = regobj.BSLicenseExpireDate;

                        OldMNClientKYC.PassportNo = regobj.Passport;
                        OldMNClientKYC.PassportPlaceOfIssue = regobj.PassportPlaceOfIssue;
                        OldMNClientKYC.PassportIssueDate = DateTime.Parse(regobj.PassportIssueDate.Trim()).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        OldMNClientKYC.PassportExpiryDate = DateTime.Parse(regobj.PassportExpireDate.Trim()).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                        OldMNClientKYCDoc.DocType = regobj.Document;
                        OldMNClientKYCDoc.PassportImage = regobj.PassportImage;
                        OldMNClientKYCDoc.FrontImage = regobj.FrontImage;
                        OldMNClientKYCDoc.BackImage = regobj.BackImage;

                        /*map old data to Agent--END*/

                        OldAgent.MNClient = OldMNClient;
                        OldAgent.MNClientContact = OldMNClientContact;
                        OldAgent.MNClientKYC = OldMNClientKYC;
                        OldAgent.MNClientKYCDoc = OldMNClientKYCDoc;
                        
                        /*Difference compare*/
                        bool isUpdated = false;
                        CompareLogic compareLogic = new CompareLogic();
                        ComparisonConfig config = new ComparisonConfig();
                        config.MaxDifferences = int.MaxValue;
                        config.IgnoreCollectionOrder = false;
                        compareLogic.Config = config;
                        ComparisonResult result = compareLogic.Compare(OldAgent, NewAgent); //  firstparameter orginal,second parameter modified
                        List<MNMakerChecker> makerCheckers = new List<MNMakerChecker>();
                        if (!result.AreEqual)
                        {
                            isUpdated = true;
                            foreach (Difference diff in result.Differences)
                            {
                                MNMakerChecker makerchecker = new MNMakerChecker();
                                int index = diff.PropertyName.IndexOf('.');
                                makerchecker.ColumnName = diff.PropertyName.Substring(index + 1);
                                makerchecker.TableName = diff.ParentPropertyName;
                                makerchecker.OldValue = diff.Object1Value;
                                makerchecker.NewValue = diff.Object2Value;
                                makerchecker.Module = "AGENT";
                                makerCheckers.Add(makerchecker);
                            }
                        }


                        if (isUpdated)
                        {

                            string modifyingAdmin = (string)Session["UserName"];
                            string modifyingBranch = (string)Session["UserBranch"];
                            CusProfileUtils cUtil = new CusProfileUtils();
                            string ModifiedFieldXML = cUtil.GetMakerCheckerXMLStr(makerCheckers);
                            bool inserted = AgentUtils.InsertAgentMakerChecker(cClientCode, modifyingAdmin, modifyingBranch, ModifiedFieldXML);

                            displayMessage = inserted
                                                    ? "Agent Information for " + userInfoModify.FName + " has successfully been updated. Please go to Approve Modification and perform accordingly."
                                                    : "Error while updating Agent Information";
                            messageClass = inserted ? CssSetting.SuccessMessageClass : CssSetting.FailedMessageClass;

                        }
                        else
                        {
                            displayMessage = "Nothing Changed";
                            messageClass = CssSetting.SuccessMessageClass;
                        }




                        this.TempData["agenttmodify_messsage"] = displayMessage;
                        this.TempData["message_class"] = messageClass;
                        return RedirectToAction("ModificationRejectedList");
                    }
                    ViewBag.Bank = new SelectList(item, "Value", "Text", userInfoModify.BankNo);
                    return View();

                }
                else
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            catch (Exception ex)
            {
                displayMessage = "Error while updating Agent Information. Error cause::" + ex.Message;
                messageClass = CssSetting.FailedMessageClass;
                return RedirectToAction("ModificationRejectedList", new { id = Id });

            }
            finally
            {
                this.TempData["agentmodify_messsage"] = displayMessage;
                this.TempData["message_class"] = messageClass;
            }
        }

        #endregion



        #region Registration Rejected---

        [HttpGet]
        public ActionResult RegistrationRejectedList(string Key, string Value)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            string UserName = Value;

            if (this.TempData["agenttmodify_messsage"] != null)
            {
                this.ViewData["agenttmodify_messsage"] = this.TempData["agenttmodify_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            //Check Role link start    
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkRole(System.Reflection.MethodBase.GetCurrentMethod().Name, clientCode);
            //Check Role link end


            TempData["userType"] = userType;
            if (TempData["userType"] != null && checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;


                List<UserInfo> agentRegApprove = new List<UserInfo>();
                var agentInfo = AgentUtils.GetAgentRegReject("agent", UserName);



                ViewBag.Value = Value;
                return View(agentInfo);

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        
        // GET: Agent/AgentRegistration/5
        public ActionResult AgentRegistrationRejected(string id)
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
                DataSet DSet = ProfileUtils.GetAgentProfileInfoDS(id);

                DataTable dtableUserInfo = DSet.Tables["dtUserInfo"];
                DataTable dKycInfo = DSet.Tables["dtKycInfo"];
                DataTable dKycDoc = DSet.Tables["dtKycDoc"];

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
                //ViewBag.Wallet = item;
                // ViewBag.Bank = item;


                if (dtableUserInfo != null && dtableUserInfo.Rows.Count > 0)
                {
                    regobj.ClientCode = dtableUserInfo.Rows[0]["ClientCode"].ToString();
                    regobj.Name = dtableUserInfo.Rows[0]["Name"].ToString();
                    regobj.Address = dtableUserInfo.Rows[0]["Address"].ToString();

                    regobj.PProvince = dtableUserInfo.Rows[0]["PProvince"].ToString();
                    regobj.PDistrict = dtableUserInfo.Rows[0]["PDistrict"].ToString();
                    regobj.PMunicipalityVDC = dtableUserInfo.Rows[0]["PMunicipalityVDC"].ToString();
                    regobj.PWardNo = dtableUserInfo.Rows[0]["PWardNo"].ToString();
                    regobj.PHouseNo = dtableUserInfo.Rows[0]["PHouseNo"].ToString();
                    regobj.PStreet = dtableUserInfo.Rows[0]["PStreet"].ToString();

                    regobj.CProvince = dtableUserInfo.Rows[0]["CProvince"].ToString();
                    regobj.CDistrict = dtableUserInfo.Rows[0]["CDistrict"].ToString();
                    regobj.CMunicipalityVDC = dtableUserInfo.Rows[0]["CMunicipalityVDC"].ToString();
                    regobj.CWardNo = dtableUserInfo.Rows[0]["CWardNo"].ToString();
                    regobj.CHouseNo = dtableUserInfo.Rows[0]["CHouseNo"].ToString();
                    regobj.CStreet = dtableUserInfo.Rows[0]["CStreet"].ToString();



                    regobj.IsApproved = dtableUserInfo.Rows[0]["IsApproved"].ToString();
                    regobj.IsRejected = dtableUserInfo.Rows[0]["IsRejected"].ToString();
                    regobj.IsModified = dtableUserInfo.Rows[0]["IsModified"].ToString();

                    regobj.Status = dtableUserInfo.Rows[0]["Status"].ToString();
                    regobj.UserName = dtableUserInfo.Rows[0]["UserName"].ToString();
                    regobj.UserType = dtableUserInfo.Rows[0]["UserType"].ToString();
                    regobj.PIN = dtableUserInfo.Rows[0]["PIN"].ToString();
                    regobj.WalletNumber = dtableUserInfo.Rows[0]["WalletNumber"].ToString();

                    regobj.FName = dtableUserInfo.Rows[0]["FName"].ToString();
                    regobj.MName = dtableUserInfo.Rows[0]["MName"].ToString();
                    regobj.LName = dtableUserInfo.Rows[0]["LName"].ToString();
                    regobj.Gender = dtableUserInfo.Rows[0]["Gender"].ToString();
                    regobj.DOB = dtableUserInfo.Rows[0]["DateOfBirth"].ToString();
                    //regobj.DOB = DateTime.Parse(regobj.DOB.Trim()).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    regobj.DOB = DateTime.Parse(regobj.DOB.Trim()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                    regobj.BSDateOfBirth = dtableUserInfo.Rows[0]["BSDateOfBirth"].ToString();

                    regobj.EmailAddress = dtableUserInfo.Rows[0]["EmailAddress"].ToString();
                    regobj.Nationality = dtableUserInfo.Rows[0]["Nationality"].ToString();
                    regobj.Country = dtableUserInfo.Rows[0]["Country"].ToString();
                    regobj.Occupation = dtableUserInfo.Rows[0]["Occupation"].ToString();
                    regobj.MaritalStatus = dtableUserInfo.Rows[0]["MaritalStatus"].ToString();
                    regobj.SpouseName = dtableUserInfo.Rows[0]["SpouseName"].ToString();

                    regobj.FatherInLaw = dtableUserInfo.Rows[0]["FatherInLaw"].ToString();

                    regobj.FatherName = dtableUserInfo.Rows[0]["FathersName"].ToString();
                    regobj.MotherName = dtableUserInfo.Rows[0]["MothersName"].ToString();
                    regobj.GrandFatherName = dtableUserInfo.Rows[0]["GFathersName"].ToString();
                    regobj.PanNo = dtableUserInfo.Rows[0]["PANNumber"].ToString();


                    regobj.Document = dtableUserInfo.Rows[0]["DocType"].ToString();
                    regobj.Citizenship = dtableUserInfo.Rows[0]["CitizenshipNo"].ToString();
                    regobj.CitizenshipIssueDate = dtableUserInfo.Rows[0]["CitizenIssueDate"].ToString().Split()[0];
                    //regobj.CitizenshipIssueDate = DateTime.Parse(regobj.CitizenshipIssueDate.Trim()).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    regobj.CitizenshipIssueDate = DateTime.Parse(regobj.CitizenshipIssueDate.Trim()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

                    regobj.BSCitizenshipIssueDate = dtableUserInfo.Rows[0]["BSCitizenIssueDate"].ToString().Split()[0];
                    regobj.CitizenshipPlaceOfIssue = dtableUserInfo.Rows[0]["CitizenPlaceOfIssue"].ToString();

                    regobj.License = dtableUserInfo.Rows[0]["LicenseNo"].ToString();
                    regobj.LicenseIssueDate = dtableUserInfo.Rows[0]["LicenseIssueDate"].ToString().Split()[0];
                    //regobj.LicenseIssueDate = DateTime.Parse(regobj.LicenseIssueDate.Trim()).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    regobj.LicenseIssueDate = DateTime.Parse(regobj.LicenseIssueDate.Trim()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

                    regobj.BSLicenseIssueDate = dtableUserInfo.Rows[0]["BSLicenseIssueDate"].ToString().Split()[0];
                    regobj.LicenseExpireDate = dtableUserInfo.Rows[0]["LicenseExpiryDate"].ToString().Split()[0];
                    //regobj.LicenseExpireDate = DateTime.Parse(regobj.LicenseExpireDate.Trim()).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    regobj.LicenseExpireDate = DateTime.Parse(regobj.LicenseExpireDate.Trim()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

                    regobj.BSLicenseExpireDate = dtableUserInfo.Rows[0]["BSLicenseExpiryDate"].ToString().Split()[0];

                    regobj.LicensePlaceOfIssue = dtableUserInfo.Rows[0]["LicensePlaceOfIssue"].ToString();

                    regobj.Passport = dtableUserInfo.Rows[0]["PassportNo"].ToString();
                    regobj.PassportPlaceOfIssue = dtableUserInfo.Rows[0]["PassportPlaceOfIssue"].ToString();
                    regobj.PassportIssueDate = dtableUserInfo.Rows[0]["PassportIssueDate"].ToString().Split()[0];
                    //  regobj.PassportIssueDate = DateTime.Parse(regobj.PassportIssueDate.Trim()).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    regobj.PassportIssueDate = DateTime.Parse(regobj.PassportIssueDate.Trim()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

                    regobj.PassportExpireDate = dtableUserInfo.Rows[0]["PassportExpiryDate"].ToString().Split()[0];
                    //regobj.PassportExpireDate = DateTime.Parse(regobj.PassportExpireDate.Trim()).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    regobj.PassportExpireDate = DateTime.Parse(regobj.PassportExpireDate.Trim()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

                    regobj.PassportImage = dtableUserInfo.Rows[0]["PassportImage"].ToString();
                    regobj.FrontImage = dtableUserInfo.Rows[0]["FrontImage"].ToString();
                    regobj.BackImage = dtableUserInfo.Rows[0]["BackImage"].ToString();




                    //regobj.ContactNumber1 = dtableUserInfo.Rows[0]["ContactNumber1"].ToString();
                    //regobj.ContactNumber2 = dtableUserInfo.Rows[0]["ContactNumber2"].ToString();                  
                    //regobj.EmailAddress= dtableUserInfo.Rows[0]["EmailAddress"].ToString();                                        
                    //regobj.BankNo = dtableUserInfo.Rows[0]["BankNo"].ToString();
                    //regobj.BranchCode = dtableUserInfo.Rows[0]["BranchCode"].ToString();
                    //regobj.BankAccountNumber = dtableUserInfo.Rows[0]["BankAccountNumber"].ToString();

                }
                else
                {
                    this.TempData["custmodify_messsage"] = "User not found";
                    this.TempData["message_class"] = CssSetting.FailedMessageClass;
                    return RedirectToAction("Index");

                }

                //ViewBag.Bank = new SelectList(item,"Value","Text" ,regobj.BankNo);

                // start milayako 02
                ViewBag.PProvince = new SelectList(list, "Value", "Text", regobj.PProvince);
                ViewBag.CProvince = new SelectList(list, "Value", "Text", regobj.CProvince);


                //end milayako 02

                // start milayako 02
                ViewBag.PDistrictID = new SelectList(ProvinceToDistrict(regobj.PProvince), "Value", "Text", regobj.PDistrict);
                ViewBag.CDistrictID = new SelectList(ProvinceToDistrict(regobj.CProvince), "Value", "Text", regobj.CDistrict);

                //  ViewBag.PDistrictID = new SelectList(ProvinceToDistrict(regobj.PProvince), "Value", "Text", regobj.PDistrictID);
                //ViewBag.CDistrictID = new SelectList(ProvinceToDistrict(regobj.CProvince), "Value", "Text", regobj.CDistrictID);
                // END milayako 02
                return View(regobj);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        [HttpPost]
        public async Task<ActionResult> AgentRegistrationRejected(string id, string btnCommand, FormCollection collection, UserInfo model)
        {
            string Id = "";
            string displayMessage = "";
            string messageClass = "";
            UserInfo userInfo = new UserInfo();
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
                        //string cAddress = collection["Address"].ToString();
                        //string cName = collection["Name"].ToString();
                        string cWalletNumber = collection["WalletNumber"].ToString();
                        //string cIsApproved = collection["IsApproved"].ToString();
                        string cUserName = collection["txtUserName"].ToString();
                        string cFName = collection["txtFirstName"].ToString();
                        string cMName = collection["txtMiddleName"].ToString();
                        string cLName = collection["txtLastName"].ToString();
                        string cGender = collection["Gender"].ToString();
                        string cDOB = collection["DOB"].ToString();
                        string cBSDateOfBirth = collection["BSDateOfBirth"].ToString();
                        string cEmailAddress = collection["Email"].ToString();
                        string cNationality = collection["Nationality"].ToString();
                        string cCountry = collection["Country"].ToString();
                        string cFatherName = collection["txtFatherName"].ToString();
                        string cMotherName = collection["txtMotherName"].ToString();
                        string cMaritalStatus = collection["MaritalStatus"].ToString();
                        string cSpouseName = collection["SpouseName"].ToString();
                        //start father
                        string cFatherInLaw = collection["FatherInLaw"].ToString();

                        //end father
                        string cGrandFatherName = collection["txtGrandFatherName"].ToString();
                        string cOccupation = collection["Occupation"].ToString();
                        string cPanNo = collection["txtPanNo"].ToString();

                        string cPProvince = collection["PProvinceText"].ToString();
                        string cPDistrict = collection["PDistrictText"].ToString();
                        string cPVDC = collection["txtPVDC"].ToString();
                        string cPHouseNo = collection["txtPHouseNo"].ToString();
                        string cPWardNo = collection["txtPWardNo"].ToString();
                        string cPStreet = collection["PStreet"].ToString();

                        string cCProvince = collection["CProvinceText"].ToString();
                        string cCDistrict = collection["CDistrictText"].ToString();
                        string cCVDC = collection["txtCVDC"].ToString();
                        string cCHouseNo = collection["txtCHouseNo"].ToString();
                        string cCWardNo = collection["txtCWardNo"].ToString();
                        string cCStreet = collection["CStreet"].ToString();

                        string cDocument = collection["Document"].ToString();
                        string cCitizenship = collection["txtCitizenship"].ToString();
                        string cCitizenshipIssueDate = collection["CitizenshipIssueDate"].ToString();
                        string cBSCitizenshipIssueDate = collection["BSCitizenshipIssueDate"].ToString();
                        string cCitizenshipPlaceOfIssue = collection["txtCitizenshipPlaceOfIssue"].ToString();


                        string cLicense = collection["txtLicense"].ToString();
                        // string cLicenseIssueDate = collection["txtLicenseIssueDate"].ToString();
                        string cLicenseIssueDate = collection["LicenseIssueDate"].ToString();
                        string cBSLicenseIssueDate = collection["BSLicenseIssueDate"].ToString();
                        // string cLicenseExpireDate = collection["txtLicenseExpireDate"].ToString();
                        string cLicenseExpireDate = collection["LicenseExpireDate"].ToString();
                        string cBSLicenseExpireDate = collection["BSLicenseExpireDate"].ToString();
                        string cLicensePlaceOfIssue = collection["txtLicensePlaceOfIssue"].ToString();

                        string cPassport = collection["txtPassport"].ToString();
                        string cPassportIssueDate = collection["txtPassportIssueDate"].ToString();
                        string cPassportExpireDate = collection["txtPassportExpireDate"].ToString();
                        string cPassportPlaceOfIssue = collection["txtPassportPlaceOfIssue"].ToString();

                        //Images OLD Value//
                        string cPassportPhoto = collection["txtOldPP"].ToString();
                        string cFrontPhoto = collection["txtOldfront"].ToString();
                        string cBackPhoto = collection["txtOldback"].ToString();

                        userInfoModify.ClientCode = cClientCode;

                        //userInfoModify.Name = userInfoModify.FName + "" + userInfoModify.MName + "" + userInfoModify.LName ;   
                        userInfoModify.Name = cFName + " " + cMName + " " + cLName;
                        userInfoModify.WalletNumber = cWalletNumber;
                        userInfoModify.UserType = "agent";
                        userInfoModify.Status = "InActive";
                        userInfoModify.IsApproved = "UnApprove";
                        userInfoModify.IsRejected = "F";
                        userInfoModify.IsModified = "T";
                        userInfoModify.UserName = cUserName;
                        userInfoModify.Address = cPVDC + "," + getDistrictName(cPDistrict) + ", Province " + cPProvince;   /*+ cPWardNo + "," + cPHouseNo + "," + cPStreet*/
                        userInfoModify.FName = cFName;
                        userInfoModify.MName = cMName;
                        userInfoModify.LName = cLName;
                        userInfoModify.Gender = cGender;
                        userInfoModify.DOB = cDOB;
                        userInfoModify.BSDateOfBirth = cBSDateOfBirth;
                        userInfoModify.EmailAddress = cEmailAddress;
                        userInfoModify.Nationality = cNationality;
                        userInfoModify.Country = cCountry;
                        userInfoModify.Occupation = cOccupation;
                        userInfoModify.MaritalStatus = cMaritalStatus;
                        userInfoModify.SpouseName = cSpouseName;
                        //start father
                        userInfoModify.FatherInLaw = cFatherInLaw;
                        //end
                        userInfoModify.PanNo = cPanNo;
                        userInfoModify.FatherName = cFatherName;
                        userInfoModify.MotherName = cMotherName;
                        userInfoModify.GrandFatherName = cGrandFatherName;



                        userInfoModify.PProvince = cPProvince;
                        userInfoModify.PDistrict = cPDistrict;
                        userInfoModify.PVDC = cPVDC;
                        userInfoModify.PWardNo = cPWardNo;
                        userInfoModify.PHouseNo = cPHouseNo;
                        userInfoModify.PStreet = cPStreet;

                        userInfoModify.CProvince = cCProvince;
                        userInfoModify.CDistrict = cCDistrict;
                        userInfoModify.CVDC = cCVDC;
                        userInfoModify.CWardNo = cCWardNo;
                        userInfoModify.CHouseNo = cCHouseNo;
                        userInfoModify.CStreet = cCStreet;

                        userInfoModify.Document = cDocument;
                        userInfoModify.Citizenship = cCitizenship;
                        userInfoModify.CitizenshipIssueDate = cCitizenshipIssueDate;
                        userInfoModify.BSCitizenshipIssueDate = cBSCitizenshipIssueDate;
                        userInfoModify.CitizenshipPlaceOfIssue = cCitizenshipPlaceOfIssue;

                        userInfoModify.License = cLicense;
                        userInfoModify.LicenseIssueDate = cLicenseIssueDate;
                        userInfoModify.BSLicenseIssueDate = cBSLicenseIssueDate;
                        userInfoModify.LicenseExpireDate = cLicenseExpireDate;
                        userInfoModify.BSLicenseExpireDate = cBSLicenseExpireDate;
                        userInfoModify.LicensePlaceOfIssue = cLicensePlaceOfIssue;

                        userInfoModify.Passport = cPassport;
                        userInfoModify.PassportIssueDate = cPassportIssueDate;
                        userInfoModify.PassportExpireDate = cPassportExpireDate;
                        userInfoModify.PassportPlaceOfIssue = cPassportPlaceOfIssue;

                        //userInfoModify.PassportImage = cPassportPhoto;

                        //var location = Server.MapPath("~/Content/Upload");
                        if (model.PassportPhoto != null)
                        {
                            if (!string.IsNullOrEmpty(cPassportPhoto))
                            {
                                //var existingFile = Path.Combine(cPassportPhoto);
                                var existingFile = Request.MapPath(cPassportPhoto);
                                if (System.IO.File.Exists(existingFile))
                                {
                                    System.IO.File.Delete(existingFile);
                                }
                            }
                            //userInfoModify.PassportImage = string.Format("{0}", SaveAndReturnFileName(model.PassportPhoto, userInfoModify.UserName));
                            var PP = ReturnFileName(model.PassportPhoto, userInfoModify.UserName);
                            userInfoModify.PassportImage = ParseCv(model.PassportPhoto);
                            userInfoModify.PassportImageName = string.Format(PP);
                            //    userInfoModify.PassportImage = string.Format("~/Content/Upload/{0}", SaveAndReturnFileName(model.PassportPhoto, userInfoModify.UserName));
                        }
                        else
                        {
                            userInfoModify.PassportImageName = "";
                            //userInfoModify.PassportImage = cPassportPhoto;
                        }


                        if (model.Front != null)
                        {
                            if (!string.IsNullOrEmpty(cFrontPhoto))
                            {

                                var existingFrontFile = Request.MapPath(cFrontPhoto);
                                if (System.IO.File.Exists(existingFrontFile))
                                {
                                    System.IO.File.Delete(existingFrontFile);
                                }
                            }
                            var front = ReturnFileName(model.Front, userInfoModify.UserName);
                            userInfoModify.FrontImage = ParseCv(model.Front);
                            userInfoModify.FrontImageName = string.Format(front);
                            //userInfoModify.FrontImage = string.Format("{0}", SaveAndReturnFileName(model.Front, userInfoModify.UserName));

                            // userInfoModify.FrontImage = string.Format("~/Content/Upload/{0}", SaveAndReturnFileName(model.Front, userInfoModify.UserName));
                        }
                        else
                        {
                            //userInfoModify.FrontImage = cFrontPhoto;
                            userInfoModify.FrontImageName = "";
                        }


                        if (model.Back != null)
                        {
                            if (!string.IsNullOrEmpty(cBackPhoto))
                            {

                                var existingBackFile = Request.MapPath(cBackPhoto);
                                if (System.IO.File.Exists(existingBackFile))
                                {
                                    System.IO.File.Delete(existingBackFile);
                                }
                            }
                            var back = ReturnFileName(model.Back, userInfoModify.UserName);
                            userInfoModify.BackImage = ParseCv(model.Back);
                            userInfoModify.BackImageName = string.Format(back);
                            //userInfoModify.BackImage = string.Format("{0}", SaveAndReturnFileName(model.Back, userInfoModify.UserName));

                            // userInfoModify.BackImage = string.Format("~/Content/Upload/{0}", SaveAndReturnFileName(model.Back, userInfoModify.UserName));
                        }
                        else
                        {
                            //userInfoModify.BackImage = cBackPhoto;
                            userInfoModify.BackImageName = "";
                        }

                        await SavePhoto(userInfoModify);
                        string sPP = Session["PP"].ToString();
                        string sFront = Session["Front"].ToString();
                        string sBack = Session["Back"].ToString();

                        if (sPP != null && sPP != "")
                        {
                            userInfoModify.PassportImageName = Session["PP"].ToString();
                        }
                        else
                        {
                            userInfoModify.PassportImageName = cPassportPhoto;
                        }
                        if (sFront != null && sFront != "")
                        {
                            userInfoModify.FrontImageName = Session["Front"].ToString();
                        }
                        else
                        {
                            userInfoModify.FrontImageName = cFrontPhoto;
                        }
                        if (sBack != null && sBack != "")
                        {
                            userInfoModify.BackImageName = Session["Back"].ToString();
                        }
                        else
                        {
                            userInfoModify.BackImageName = cBackPhoto;
                        }

                        if ((cWalletNumber != "") && (cUserName != "") && (cFName != ""))
                        {

                            bool isUpdated = CustomerUtils.UpdateAgentInfo(userInfoModify);
                            displayMessage = isUpdated
                                                     ? "Changes will take effect after approval."
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
                        return RedirectToAction("RegistrationRejectedList");
                    }
                    ViewBag.Bank = new SelectList(item, "Value", "Text", userInfoModify.BankNo);
                    return View();

                }
                else
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            catch (Exception ex)
            {
                displayMessage = "Error while updating Agent Information. Error cause::" + ex.Message;
                messageClass = CssSetting.FailedMessageClass;
                return RedirectToAction("AgentModify", new { id = Id });

            }
            finally
            {
                this.TempData["agenttmodify_messsage"] = displayMessage;
                this.TempData["message_class"] = messageClass;
            }
        }

        #endregion

       
        #region "Agent Status" ---

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
                if (this.TempData["agent_messsage"] != null)
                {
                    this.ViewData["agent_messsage"] = this.TempData["agent_messsage"];
                    this.ViewData["message_class"] = this.TempData["message_class"];
                }
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

                if (userInfo.Name == "" || userInfo.Name == null)
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
                
                if (userInfo.Name != "")
                {
                    DataTable dtableCustomerStatusByMobileNo = AgentUtils.GetAgentProfileByMobileNo(userInfo.Name);
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
                
                return View(customerStatus);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        //View Agent Status
        [HttpGet]
        public ActionResult ViewAgentStatus(string clientCodeId)
        {
            if (string.IsNullOrEmpty(clientCodeId))
            {
                return RedirectToAction("Index", "Agent");
            }
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

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


                UserInfo userInfo = new UserInfo();
                DataTable dtblRegistration = ProfileUtils.GetAgentProfileInfo(clientCodeId);
                if (dtblRegistration.Rows.Count == 1)
                {
                    userInfo.ClientCode = dtblRegistration.Rows[0]["ClientCode"].ToString();
                    userInfo.Name = dtblRegistration.Rows[0]["Name"].ToString().Capitalize();
                    userInfo.Address = dtblRegistration.Rows[0]["Address"].ToString();
                    userInfo.UserName = dtblRegistration.Rows[0]["UserName"].ToString();
                    userInfo.WalletNumber = dtblRegistration.Rows[0]["WalletNumber"].ToString();

                    userInfo.FName = dtblRegistration.Rows[0]["FName"].ToString();
                    userInfo.MName = dtblRegistration.Rows[0]["MName"].ToString();
                    userInfo.LName = dtblRegistration.Rows[0]["LName"].ToString();
                    userInfo.DOB = dtblRegistration.Rows[0]["DateOfBirth"].ToString().Split()[0];
                    userInfo.BSDateOfBirth = dtblRegistration.Rows[0]["BSDateOfBirth"].ToString().Split()[0];
                    userInfo.EmailAddress = dtblRegistration.Rows[0]["EmailAddress"].ToString();
                    userInfo.Gender = dtblRegistration.Rows[0]["Gender"].ToString();
                    userInfo.Nationality = dtblRegistration.Rows[0]["Nationality"].ToString();
                    userInfo.Country = dtblRegistration.Rows[0]["Country"].ToString();
                    userInfo.FatherName = dtblRegistration.Rows[0]["FathersName"].ToString();
                    userInfo.MotherName = dtblRegistration.Rows[0]["MothersName"].ToString();
                    userInfo.MaritalStatus = dtblRegistration.Rows[0]["MaritalStatus"].ToString();
                    userInfo.SpouseName = dtblRegistration.Rows[0]["SpouseName"].ToString();

                    userInfo.FatherInLaw = dtblRegistration.Rows[0]["FatherInLaw"].ToString();

                    userInfo.GrandFatherName = dtblRegistration.Rows[0]["GFathersName"].ToString();
                    userInfo.Occupation = dtblRegistration.Rows[0]["Occupation"].ToString();
                    userInfo.PanNo = dtblRegistration.Rows[0]["PanNumber"].ToString();

                    userInfo.PProvince = dtblRegistration.Rows[0]["PProvince"].ToString();
                    userInfo.PDistrict = getDistrictName(dtblRegistration.Rows[0]["PDistrict"].ToString());
                    userInfo.PVDC = dtblRegistration.Rows[0]["PMunicipalityVDC"].ToString();
                    userInfo.PWardNo = dtblRegistration.Rows[0]["PWardNo"].ToString();
                    userInfo.PHouseNo = dtblRegistration.Rows[0]["PHouseNo"].ToString();
                    userInfo.PStreet = dtblRegistration.Rows[0]["PStreet"].ToString();

                    userInfo.CProvince = dtblRegistration.Rows[0]["CProvince"].ToString();
                    userInfo.CDistrict = getDistrictName(dtblRegistration.Rows[0]["CDistrict"].ToString());
                    userInfo.CVDC = dtblRegistration.Rows[0]["CMunicipalityVDC"].ToString();
                    userInfo.CWardNo = dtblRegistration.Rows[0]["CWardNo"].ToString();
                    userInfo.CHouseNo = dtblRegistration.Rows[0]["CHouseNo"].ToString();
                    userInfo.CStreet = dtblRegistration.Rows[0]["CStreet"].ToString();

                    userInfo.Document = dtblRegistration.Rows[0]["DocType"].ToString();

                    userInfo.Citizenship = dtblRegistration.Rows[0]["CitizenshipNo"].ToString();
                    userInfo.CitizenshipIssueDate = dtblRegistration.Rows[0]["CitizenIssueDate"].ToString().Split()[0];
                    userInfo.BSCitizenshipIssueDate = dtblRegistration.Rows[0]["BSCitizenIssueDate"].ToString().Split()[0];
                    userInfo.CitizenshipPlaceOfIssue = dtblRegistration.Rows[0]["CitizenPlaceOfIssue"].ToString();

                    userInfo.License = dtblRegistration.Rows[0]["LicenseNo"].ToString();
                    userInfo.LicenseIssueDate = dtblRegistration.Rows[0]["LicenseIssueDate"].ToString().Split()[0];
                    userInfo.BSLicenseIssueDate = dtblRegistration.Rows[0]["BSLicenseIssueDate"].ToString().Split()[0];
                    userInfo.LicenseExpireDate = dtblRegistration.Rows[0]["LicenseExpiryDate"].ToString().Split()[0];
                    userInfo.BSLicenseExpireDate = dtblRegistration.Rows[0]["BSLicenseExpiryDate"].ToString().Split()[0];
                    userInfo.LicensePlaceOfIssue = dtblRegistration.Rows[0]["LicensePlaceOfIssue"].ToString();

                    userInfo.Passport = dtblRegistration.Rows[0]["PassportNo"].ToString();
                    userInfo.PassportIssueDate = dtblRegistration.Rows[0]["PassportIssueDate"].ToString().Split()[0];
                    userInfo.PassportExpireDate = dtblRegistration.Rows[0]["PassportExpiryDate"].ToString().Split()[0];
                    userInfo.PassportPlaceOfIssue = dtblRegistration.Rows[0]["PassportPlaceOfIssue"].ToString();

                    userInfo.PassportImage = dtblRegistration.Rows[0]["PassportImage"].ToString();
                    userInfo.FrontImage = dtblRegistration.Rows[0]["FrontImage"].ToString();
                    userInfo.BackImage = dtblRegistration.Rows[0]["BackImage"].ToString();

                    userInfo.DOB = DateConvert(userInfo.DOB);

                    userInfo.CitizenshipIssueDate = DateConvert(userInfo.CitizenshipIssueDate);
                    userInfo.LicenseIssueDate = DateConvert(userInfo.LicenseIssueDate);
                    userInfo.LicenseExpireDate = DateConvert(userInfo.LicenseExpireDate);

                    userInfo.PassportIssueDate = DateConvert(userInfo.PassportIssueDate);
                    userInfo.PassportExpireDate = DateConvert(userInfo.PassportExpireDate);

                }
                return View(userInfo);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        #endregion

    }
}

