using ThailiMerchantApp.Helper;
using ThailiMerchantApp.Models;
using ThailiMerchantApp.Settings;
using ThailiMerchantApp.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.SessionState;

namespace ThailiMerchantApp.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class SignupController : Controller
    {
        // GET: Signup

        public ActionResult Index()
        {
            if (this.TempData["forget_message"] != null)
            {
                this.ViewData["forget_message"] = this.TempData["forget_message"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Index(FormCollection collection)
        {
            string userName = collection["txtUserName"] ?? string.Empty;
            //string verify = collection["txtVerify"] ?? string.Empty;
            string displayMessage = null;
            string messageClass = null;
            string code = string.Empty;
            string mobile = string.Empty;
            bool result = false;

            try
            {
                if (!IsValidUserName(userName))
                {
                    //code = TraceIdGenerator.GetUniqueKey();
                    //string messagereply = "Dear Customer," + "\n";
                    //messagereply += " Your Verification Code is " + code
                    //    + "." + "\n" + "Close this message and enter code to verify account.";
                    //messagereply += "-MNepal";

                    var client = new WebClient();

                    mobile = userName;
                    //var content = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                    //    + "977" + mobile + "&Text=" + messagereply + "");

                    //if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                    //{
                    //    //FOR NCELL
                    //    var contents = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                    //    + "977" + mobile + "&Text=" + messagereply + "");
                    //}
                    //else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                    //    || (mobile.Substring(0, 3) == "986"))
                    //{
                    //    //FOR NTC
                    //    var contents = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                    //        + "977" + mobile + "&Text=" + messagereply + "");
                    //}

                    //SMSLog log = new SMSLog();
                    //log.SentBy = mobile;
                    //log.Purpose = "Self Registration";
                    //log.UserName = userName;
                    //log.Message = messagereply;
                    //CustomerUtils.LogSMS(log);

                    Session["Mobile"] = mobile;
                    //Session["Code"] = code;
                    return RedirectToAction("CustomerKYC1");
                }
                else
                {
                    displayMessage = "\n The number is registered !!";
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
        public ActionResult GetCheckingUserName(string username)
        {
            if ((username != "") || (username != null))
            {

                string result = string.Empty;
                //if (username.Substring(0, 2) == "98")
                //{
                //    DataTable dtableMobileNo = RegisterUtils.GetCheckMobileNo(username);
                //    if (dtableMobileNo.Rows.Count == 0)
                //    {
                //        result = "Not Registered";
                //        return Json(result, JsonRequestBehavior.AllowGet);
                //    }
                //    else
                //    {
                //        result = "Success";
                //        return Json(result, JsonRequestBehavior.AllowGet); 
                //    }
                //}

                DataTable dtCheckUserName = RegisterUtils.GetCheckUserName(username);
                if (dtCheckUserName.Rows.Count == 0)
                {
                    result = "Not Registered";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    result = "Success";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }




            }
            return RedirectToAction("CustomerKYC1");
        }

        // GET: Signup/Verify
        public ActionResult Verify()
        {
            if (this.TempData["recover_message"] != null)
            {
                this.TempData["recover_message"] = this.TempData["recover_message"];
                this.TempData["message_class"] = this.TempData["message_class"];
            }
            string userName = (string)Session["Mobile"];
            string code = (string)Session["Code"];
            return View("Verify");
        }


        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Verify(FormCollection collection)
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
                    return RedirectToAction("CustomerKYC1");
                }
                else
                {
                    displayMessage = "Enter the code received via SMS";
                    messageClass = CssSetting.FailedMessageClass;
                }
            }
            catch (Exception ex)
            {
                displayMessage = ex.Message + "\n Please Contact to the administrator !!";
                messageClass = CssSetting.FailedMessageClass;
            }
            this.TempData["recover_msg"] = result
                                                  ? "" + displayMessage
                                                  :  displayMessage;
            this.TempData["message_class"] = result ? "success_info" : "failed_info";
            return View("Verify");
        }
        DAL objdal = new DAL();
        // GET: Signup/CustomerKYC
        public ActionResult CustomerKYC()
        {
            string mobile = (string)Session["Mobile"];

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
            if (this.TempData["registration_message"] != null)
            {
                this.TempData["changepwd_message"] = this.TempData["changepwd_message"];
                this.TempData["message_class"] = this.TempData["message_class"];
            }
            CustomerSRInfo srobj = new CustomerSRInfo();
            return View(srobj);
            
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

        // POST: Signup/CustomerKYC
        [AcceptVerbs(HttpVerbs.Post)]
        [HttpPost]
        public ActionResult CustomerKYC(FormCollection collection, CustomerSRInfo model)
        {
            

            CustomerSRInfo srInfo = new CustomerSRInfo();
            try
            {
                
                string mobile = (string)Session["Mobile"];
                string code = (string)Session["Code"];

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


                //string PDistrict, PProvincestr, CDistrict, CProvincestr, t = string.Empty;

                if ((mobile != "null"))
                {

                    

                    // //START milayako
                    //PDistrict = collection["PDistrict"].ToString();

                    // string Pdistrictstring = "SELECT DistrictID FROM MNDistrict WHERE Name ='" + PDistrict + "'";
                    // DataTable Pdt1 = new DataTable();
                    // Pdt1 = objdal.MyMethod(Pdistrictstring);
                    // List<SelectListItem> listDistrict = new List<SelectListItem>();
                    // string districtID = string.Empty;
                    // foreach (DataRow row in Pdt1.Rows)
                    // {
                    //     districtID = row["DistrictID"].ToString();

                    // }
                    // //string districtID = listDistrict.ToString();
                    // string PtestDistrictID = districtID;

                    // PProvincestr = collection["PProvince"].ToString();

                    // string Pprovincestring = "SELECT * FROM MNProvince WHERE ProvinceID='" + PProvincestr + "'";
                    // DataTable Pdt = new DataTable();
                    // Pdt = objdal.MyMethod(Pprovincestring);
                    // List<SelectListItem> Plist = new List<SelectListItem>();
                    // foreach (DataRow row in Pdt.Rows)
                    // {
                    //     Plist.Add(new SelectListItem
                    //     {
                    //         Text = Convert.ToString(row.ItemArray[1]),
                    //         Value = Convert.ToString(row.ItemArray[0])\



                    //     });

                    // }
                    // string PProvince = Plist[0].Text.ToString();


                    // //end milayako2

                    // //START milayako
                    // CDistrict = collection["CDistrict"].ToString();

                    // string Cdistrictstring = "SELECT DistrictID FROM MNDistrict WHERE Name ='" + CDistrict + "'";
                    // DataTable Cdt1 = new DataTable();
                    // Cdt1 = objdal.MyMethod(Cdistrictstring);
                    // List<SelectListItem> ClistDistrict = new List<SelectListItem>();
                    // string CdistrictID = string.Empty;
                    // foreach (DataRow row in Cdt1.Rows)
                    // {
                    //     districtID = row["DistrictID"].ToString();

                    // }
                    // //string districtID = listDistrict.ToString();
                    // string CtestDistrictID = districtID;

                    // CProvincestr = collection["PProvince"].ToString();

                    // string Cprovincestring = "SELECT * FROM MNProvince WHERE ProvinceID='" + CProvincestr + "'";
                    // DataTable Cdt = new DataTable();
                    // Cdt = objdal.MyMethod(Cprovincestring);
                    // List<SelectListItem> Clist = new List<SelectListItem>();
                    // foreach (DataRow row in Cdt.Rows)
                    // {
                    //     Clist.Add(new SelectListItem
                    //     {
                    //         Text = Convert.ToString(row.ItemArray[1]),
                    //         Value = Convert.ToString(row.ItemArray[0])
                    //     });

                    // }
                    // string CProvince = Clist[0].Text.ToString();


                    // //end milayako2
                    srInfo.UserName = mobile;

                    srInfo.FName = collection["txtFirstName"].ToString();
                    srInfo.MName = collection["txtMiddleName"].ToString();
                    srInfo.LName = collection["txtLastName"].ToString();
                   

                    //srInfo.Name = srInfo.FName + " " + srInfo.MName + " " + srInfo.LName;
                    srInfo.Gender = collection["txtGender"].ToString();
                    srInfo.DOB = collection["DOB"].ToString();

                    srInfo.Nationality = collection["txtNationality"].ToString();
                    srInfo.FatherName = collection["txtFatherName"].ToString();
                    srInfo.MotherName = collection["txtMotherName"].ToString();
                    srInfo.SpouseName = collection["SpouseName"].ToString();
                    srInfo.MaritalStatus = collection["MaritalStatus"].ToString();


                    srInfo.GrandFatherName = collection["txtGrandFatherName"].ToString();
                    srInfo.FatherInlawName = collection["txtFatherInlawName"].ToString();
                    srInfo.Occupation = collection["txtOccupation"].ToString();


                    srInfo.EmailAddress = collection["txtEmail"].ToString();
                    srInfo.PanNo = collection["txtPanNo"].ToString();

                    
                    srInfo.Country = collection["txtCountry"].ToString();

                    
                    srInfo.PProvince = collection["PProvince"].ToString();
                    srInfo.PDistrict = collection["PDistrict"].ToString();
                    //srInfo.PDistrict = districtID.ToString();

                    //srInfo.PZone = collection["txtPZone"].ToString();
                    //srInfo.PDistrict = collection["txtPDistrict"].ToString();
                    srInfo.PVDC = collection["txtPVDC"].ToString();
                    srInfo.PHouseNo = collection["txtPHouseNo"].ToString();
                    srInfo.PWardNo = collection["txtPWardNo"].ToString();
                    srInfo.PStreet = collection["PStreet"].ToString();
                    //srInfo.PAddress = srInfo.PHouseNo + " " + srInfo.PWardNo + " " + srInfo.PDistrict + " " + srInfo.PVDC + " " + srInfo.PZone;


                    srInfo.CProvince = collection["CProvince"].ToString();
                    srInfo.CDistrict = collection["CDistrict"].ToString();
                    //srInfo.CDistrict = districtID.ToString();

                    //srInfo.CZone = collection["txtCZone"].ToString();
                    //srInfo.CDistrict = collection["txtCDistrict"].ToString();
                    srInfo.CVDC = collection["txtCVDC"].ToString();
                    srInfo.CHouseNo = collection["txtCHouseNo"].ToString();
                    srInfo.CWardNo = collection["txtCWardNo"].ToString();
                    srInfo.CStreet = collection["CStreet"].ToString();
                    //srInfo.CAddress = srInfo.CHouseNo + " " + srInfo.CWardNo + " " + srInfo.CDistrict + " " + srInfo.CVDC + " " + srInfo.CZone;
                    //srInfo.Address = srInfo.CAddress;

                    srInfo.Citizenship = collection["txtCitizenship"].ToString();
                    srInfo.CitizenshipIssueDate = collection["CitizenshipIssueDate"].ToString();
                    srInfo.CitizenshipPlaceOfIssue = collection["txtCitizenshipPlaceOfIssue"].ToString();

                    srInfo.License = collection["txtLicense"].ToString();
                    //test//
                    if (!string.IsNullOrEmpty(srInfo.LicenseIssueDate))
                    {
                        DateTime.ParseExact(srInfo.LicenseIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        srInfo.LicenseIssueDate = "";
                    }
                    srInfo.LicensePlaceOfIssue = collection["txtLicensePlaceOfIssue"].ToString();

                    if (!string.IsNullOrEmpty(srInfo.LicenseExpireDate))
                    {
                        DateTime.ParseExact(srInfo.LicenseExpireDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        srInfo.LicenseExpireDate = "";
                    }


                    srInfo.Passport = collection["txtPassport"].ToString();

                    if (!string.IsNullOrEmpty(srInfo.PassportIssueDate))
                    {
                        DateTime.ParseExact(srInfo.PassportIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        srInfo.PassportIssueDate = "";
                    }

                    srInfo.PassportPlaceOfIssue = collection["txtPassportPlaceOfIssue"].ToString();

                    if (!string.IsNullOrEmpty(srInfo.PassportExpireDate))
                    {
                        DateTime.ParseExact(srInfo.PassportExpireDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        srInfo.PassportExpireDate = "";
                    }


                    srInfo.Document = collection["txtDocument"].ToString();


                    srInfo.UserType = "user";
                    srInfo.Status = "Active";
                    //srInfo.IsApproved = "UnApprove";
                    srInfo.IsRejected = "F";
                    srInfo.BranchCode = "004";
                    srInfo.OTPCode = code;
                    srInfo.Source = "http";
                    


                    if (!ViewData.ModelState.IsValid)
                    {
                        this.ViewData["registration_message"] = " *Validation Error.";
                        this.ViewData["message_class"] = "failed_info";
                        return View();
                    }

                    bool result = false;
                    string errorMessage = string.Empty;


                    if ((srInfo.UserName != "") && (srInfo.Password != ""))
                    {
                        DataTable dtableMobileNo = RegisterUtils.GetCheckMobileNo(srInfo.UserName);
                        if (dtableMobileNo.Rows.Count == 0)
                        {
                            if (collection.AllKeys.Any())
                            {
                                try
                                {
                                    var PP = SaveAndReturnFileName(model.PassportPhoto, mobile);
                                    var front = SaveAndReturnFileName(model.Front, mobile);
                                    var back = SaveAndReturnFileName(model.Back, mobile);

                                    srInfo.PassportImage = string.Format("~/Content/Upload/{0}", PP);
                                    srInfo.FrontImage = string.Format("~/Content/Upload/{0}", front); 
                                    srInfo.BackImage = string.Format("~/Content/Upload/{0}", back);

                                    int results = RegisterUtils.CustomerSelfReg(srInfo);
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
                                        TempData["login_message"] = "Registration information is successfully added.Please wait for approval";
                                        TempData["message_class"] = CssSetting.SuccessMessageClass;
                                        return RedirectToAction("Index", "Login");
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
                                                  ? "Registration information is successfully added."
                                                  : "Error while inserting the information. ERROR :: "
                                                    + errorMessage;
                    this.ViewData["message_class"] = result ? "success_info" : "failed_info";

                   


                    return View(srInfo);
                }
                else
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            catch (Exception ex)
            {
                return View(srInfo);
            }
        }

        public string SaveAndReturnFileName(HttpPostedFileBase file,string mobile)
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
                                "_" + Guid.NewGuid().ToString().Substring(0, 4)+ Path.GetExtension(fileName);
            var path = Path.Combine(Server.MapPath("~/Content/Upload/"), randomFileName);
            img.Save(path);
            return randomFileName;
        }

        private bool IsImage(HttpPostedFileBase file)
        {
            //Checks for image type... you could also do filename extension checks and other things
            return ((file != null) && System.Text.RegularExpressions.Regex.IsMatch(file.ContentType, "image/\\S+") && (file.ContentLength > 0));
        }

        
        #region customerKYC1
        

        //start kyc1
        public ActionResult CustomerKYC1()
        {
            string mobile = (string)Session["Mobile"];
             
            if (this.TempData["registration_message"] != null)
            {
                this.TempData["changepwd_message"] = this.TempData["changepwd_message"];
                this.TempData["message_class"] = this.TempData["message_class"];
            }
            CustomerSRInfo srobj = new CustomerSRInfo();
            return View(srobj);

        }

         
        // POST: Signup/CustomerKYC1
        [AcceptVerbs(HttpVerbs.Post)]
        [HttpPost]
        public ActionResult CustomerKYC1(FormCollection collection)
        {
            
            CustomerSRInfo srInfo = new CustomerSRInfo();
            try
            {

                string mobile = (string)Session["Mobile"];
                string code = (string)Session["Code"];
                
               
                if ((mobile != null))
                {

                    srInfo.UserName = mobile;
                    srInfo.FName = collection["txtFirstName"].ToString();
                    srInfo.MName = collection["txtMiddleName"].ToString();
                    srInfo.LName = collection["txtLastName"].ToString();
                    //srInfo.Name = srInfo.FName + " " + srInfo.MName + " " + srInfo.LName;
                    srInfo.Gender = collection["txtGender"].ToString();
                    srInfo.Nationality = collection["txtNationality"].ToString();
                    srInfo.DOB = collection["DOB"].ToString();
                    srInfo.BSDateOfBirth = collection["BSDateOfBirth"].ToString();
                    // srInfo.Country = collection["txtDateADBS"].ToString();

                    srInfo.Country = collection["txtCountry"].ToString();
                    

                    srInfo.UserType = "user";
                    srInfo.Status = "Active";
                    //srInfo.IsApproved = "UnApprove";
                    srInfo.IsRejected = "F";
                    srInfo.BranchCode = "004";
                    srInfo.OTPCode = code;
                    srInfo.Source = "http";



                    if (!ViewData.ModelState.IsValid)
                    {
                        this.ViewData["registration_message"] = " *Validation Error.";
                        this.ViewData["message_class"] = "failed_info";
                        return View();
                    }

                    bool result = false;
                    string errorMessage = string.Empty;


                    if ((srInfo.UserName != "") && (srInfo.Password != ""))
                    {
                        //Session["UserName"] = srInfo.UserName;
                        //Session["FName"] = srInfo.FName;
                        TempData["CustomerKYC"] = srInfo;
                        return RedirectToAction("CustomerKYC2");
                        
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




                    return View(srInfo);
                }
                else
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            catch (Exception ex)
            {
                return View();
            }
        }
        #endregion

        #region CustomerKYC2
        
        [HttpGet]
        public ActionResult CustomerKYC2()
        {
            if (TempData["CustomerKYC"]==null) {
                return RedirectToAction("CustomerKYC1");
            }
            string mobile = (string)Session["Mobile"];

            //string username = (string)Session["UserName"];
            //string fname = (string)Session["FName"];
          
            if (this.TempData["registration_message"] != null)
            {
                this.TempData["changepwd_message"] = this.TempData["changepwd_message"];
                this.TempData["message_class"] = this.TempData["message_class"];
            }
            
            return View(TempData["CustomerKYC"]);

        }

        
        // POST: Signup/CustomerKYC2
        [AcceptVerbs(HttpVerbs.Post)]
        [HttpPost]
        public ActionResult CustomerKYC2(FormCollection collection)
        {
          
            CustomerSRInfo srInfo = new CustomerSRInfo();
            try
            {
                
                string mobile = (string)Session["Mobile"];

                string code = (string)Session["Code"];
                
                if ((mobile != null))
                {
                    srInfo.UserName = mobile;

                    srInfo.FName = collection["txtFirstName"].ToString();
                    srInfo.MName = collection["txtMiddleName"].ToString();
                    srInfo.LName = collection["txtLastName"].ToString();
                    //srInfo.Name = srInfo.FName + " " + srInfo.MName + " " + srInfo.LName;
                    srInfo.Gender = collection["txtGender"].ToString();
                    srInfo.Nationality = collection["txtNationality"].ToString();
                    srInfo.DOB = collection["DOB"].ToString();
                    srInfo.BSDateOfBirth = collection["BSDateOfBirth"].ToString();
                    // srInfo.Country = collection["txtDateADBS"].ToString();
                    srInfo.Country = collection["txtCountry"].ToString();
                    
                    //kyc2
                    srInfo.FatherName = collection["txtFatherName"].ToString();
                    srInfo.MotherName = collection["txtMotherName"].ToString();
                    srInfo.GrandFatherName = collection["txtGrandFatherName"].ToString();
                    srInfo.MaritalStatus = collection["MaritalStatus"].ToString();
                    srInfo.SpouseName = collection["SpouseName"].ToString();
                    srInfo.FatherInlawName = collection["txtFatherInlawName"].ToString();
                    srInfo.Occupation = collection["txtOccupation"].ToString();
                    srInfo.EmailAddress = collection["txtEmail"].ToString();
                    srInfo.PanNo = collection["txtPanNo"].ToString();
                    
                    srInfo.UserType = "user";
                    srInfo.Status = "Active";
                    //srInfo.IsApproved = "UnApprove";
                    srInfo.IsRejected = "F";
                    srInfo.BranchCode = "004";
                    srInfo.OTPCode = code;
                    srInfo.Source = "http";



                    if (!ViewData.ModelState.IsValid)
                    {
                        this.ViewData["registration_message"] = " *Validation Error.";
                        this.ViewData["message_class"] = "failed_info";
                        return View(srInfo);
                    }

                    bool result = false;
                    string errorMessage = string.Empty;


                    if ((srInfo.UserName != "") && (srInfo.Password != ""))
                    {
                        //Session["UserName"] = srInfo.UserName;
                        //Session["FName"] = srInfo.FName;
                        TempData["CustomerKYC"] = srInfo;
                        return RedirectToAction("CustomerKYC3");

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




                    return View(srInfo);
                }
                else
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            catch (Exception ex)
            {
                bool result = false;
                this.ViewData["registration_message"] = result
                                                 ? "Registration information is successfully added."
                                                 : "Error while inserting the information. ERROR : "
                                                   + ex.Message;
                this.ViewData["message_class"] = result ? "success_info" : "failed_info";
                return View(srInfo);
            }
        }

        //end post kyc2
        //end kyc2
        #endregion

        #region CustomerKYC3
        [HttpGet]
        public ActionResult CustomerKYC3()
        {
            if (TempData["CustomerKYC"] == null)
            {
                return RedirectToAction("CustomerKYC1");
            }
            //string mobile = (string)Session["Mobile"];
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
            if (this.TempData["registration_message"] != null)
            {
                this.TempData["changepwd_message"] = this.TempData["changepwd_message"];
                this.TempData["message_class"] = this.TempData["message_class"];
            }

            return View(TempData["CustomerKYC"]);

        }


        
        [AcceptVerbs(HttpVerbs.Post)]
        [HttpPost]
        public ActionResult CustomerKYC3(FormCollection collection)
        {
            
            CustomerSRInfo srInfo = new CustomerSRInfo();
            try
            {
                string mobile = (string)Session["Mobile"];
                string code = (string)Session["Code"];

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
                if ((mobile != null))
                {

                    srInfo.UserName = mobile;

                    srInfo.FName = collection["txtFirstName"].ToString();
                    srInfo.MName = collection["txtMiddleName"].ToString();
                    srInfo.LName = collection["txtLastName"].ToString();
                    //srInfo.Name = srInfo.FName + " " + srInfo.MName + " " + srInfo.LName;
                    srInfo.Gender = collection["txtGender"].ToString();
                    srInfo.Nationality = collection["txtNationality"].ToString();
                    srInfo.DOB = collection["DOB"].ToString();
                    srInfo.BSDateOfBirth = collection["BSDateOfBirth"].ToString();
                    // srInfo.Country = collection["txtDateADBS"].ToString();
                    srInfo.Country = collection["txtCountry"].ToString();

                    //kyc2
                    srInfo.FatherName = collection["txtFatherName"].ToString();
                    srInfo.MotherName = collection["txtMotherName"].ToString();
                    srInfo.GrandFatherName = collection["txtGrandFatherName"].ToString();
                    srInfo.MaritalStatus = collection["MaritalStatus"].ToString();
                    srInfo.SpouseName = collection["SpouseName"].ToString();
                    srInfo.FatherInlawName = collection["txtFatherInlawName"].ToString();
                    srInfo.Occupation = collection["txtOccupation"].ToString();
                    srInfo.EmailAddress = collection["txtEmail"].ToString();
                    srInfo.PanNo = collection["txtPanNo"].ToString();
                    //kyc3
                    srInfo.PProvince = collection["PProvince"].ToString();
                    srInfo.PDistrict = collection["PDistrict"].ToString();
                    //srInfo.PDistrict = districtID.ToString();

                    //srInfo.PZone = collection["txtPZone"].ToString();
                    //srInfo.PDistrict = collection["txtPDistrict"].ToString();
                    srInfo.PVDC = collection["txtPVDC"].ToString();
                    srInfo.PHouseNo = collection["txtPHouseNo"].ToString();
                    srInfo.PWardNo = collection["txtPWardNo"].ToString();
                    srInfo.PStreet = collection["PStreet"].ToString();
                    //srInfo.PAddress = srInfo.PHouseNo + " " + srInfo.PWardNo + " " + srInfo.PDistrict + " " + srInfo.PVDC + " " + srInfo.PZone;
                    
                    srInfo.CProvince = collection["CProvince"].ToString();
                    srInfo.CDistrict = collection["CDistrict"].ToString();
                    //srInfo.CDistrict = districtID.ToString();
                    //srInfo.CZone = collection["txtCZone"].ToString();
                    //srInfo.CDistrict = collection["txtCDistrict"].ToString();
                    srInfo.CVDC = collection["txtCVDC"].ToString();
                    srInfo.CHouseNo = collection["txtCHouseNo"].ToString();
                    srInfo.CWardNo = collection["txtCWardNo"].ToString();
                    srInfo.CStreet = collection["CStreet"].ToString();
                    //srInfo.CAddress = srInfo.CHouseNo + " " + srInfo.CWardNo + " " + srInfo.CDistrict + " " + srInfo.CVDC + " " + srInfo.CZone;
                    //srInfo.Address = srInfo.CAddress;

                    
                    srInfo.UserType = "user";
                    srInfo.Status = "Active";
                    //srInfo.IsApproved = "UnApprove";
                    srInfo.IsRejected = "F";
                    srInfo.BranchCode = "004";
                    srInfo.OTPCode = code;
                    srInfo.Source = "http";



                    if (!ViewData.ModelState.IsValid)
                    {
                        this.ViewData["registration_message"] = " *Validation Error.";
                        this.ViewData["message_class"] = "failed_info";
                        return View(srInfo);
                    }

                    bool result = false;
                    string errorMessage = string.Empty;


                    if ((srInfo.UserName != "") && (srInfo.Password != ""))
                    {
                        //Session["UserName"] = srInfo.UserName;
                        //Session["FName"] = srInfo.FName;
                        TempData["CustomerKYC"] = srInfo;
                        return RedirectToAction("CustomerKYC4");

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




                    return View(srInfo);
                }
                else
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            catch (Exception ex)
            {
                return View(srInfo);
            }
        }


        #endregion

        #region CustomerKYC4
       
        [HttpGet]
        public ActionResult CustomerKYC4()
        {
            if (TempData["CustomerKYC"] == null)
            {
                return RedirectToAction("CustomerKYC1");
            }
            if (this.TempData["registration_message"] != null)
            {
                this.TempData["changepwd_message"] = this.TempData["changepwd_message"];
                this.TempData["message_class"] = this.TempData["message_class"];
            }

            return View(TempData["CustomerKYC"]);

        }



        [AcceptVerbs(HttpVerbs.Post)]
        [HttpPost]
        public ActionResult CustomerKYC4(FormCollection collection, CustomerSRInfo model)
        {


            CustomerSRInfo srInfo = new CustomerSRInfo();
            try
            {
                string mobile = (string)Session["Mobile"];
                string code = (string)Session["Code"];

               
                if ((mobile != null))
                {
                    srInfo.UserName = mobile;

                    srInfo.FName = collection["txtFirstName"].ToString();
                    srInfo.MName = collection["txtMiddleName"].ToString();
                    srInfo.LName = collection["txtLastName"].ToString();
                    //srInfo.Name = srInfo.FName + " " + srInfo.MName + " " + srInfo.LName;
                    srInfo.Gender = collection["txtGender"].ToString();
                    srInfo.Nationality = collection["txtNationality"].ToString();
                    srInfo.DOB = collection["DOB"].ToString();
                    srInfo.BSDateOfBirth = collection["BSDateOfBirth"].ToString();
                    // srInfo.Country = collection["txtDateADBS"].ToString();
                    srInfo.Country = collection["txtCountry"].ToString();
                    
                    //kyc2
                    srInfo.FatherName = collection["txtFatherName"].ToString();
                    srInfo.MotherName = collection["txtMotherName"].ToString();
                    srInfo.GrandFatherName = collection["txtGrandFatherName"].ToString();
                    srInfo.MaritalStatus = collection["MaritalStatus"].ToString();
                    srInfo.SpouseName = collection["SpouseName"].ToString();
                    srInfo.FatherInlawName = collection["txtFatherInlawName"].ToString();
                    srInfo.Occupation = collection["txtOccupation"].ToString();
                    srInfo.EmailAddress = collection["txtEmail"].ToString();
                    srInfo.PanNo = collection["txtPanNo"].ToString();
                    
                    //kyc3
                    srInfo.PProvince = collection["PProvince"].ToString();
                    srInfo.PDistrict = collection["PDistrict"].ToString();
                    //srInfo.PDistrict = districtID.ToString();
                    //srInfo.PZone = collection["txtPZone"].ToString();
                    //srInfo.PDistrict = collection["txtPDistrict"].ToString();
                    srInfo.PVDC = collection["txtPVDC"].ToString();
                    srInfo.PHouseNo = collection["txtPHouseNo"].ToString();
                    srInfo.PWardNo = collection["txtPWardNo"].ToString();
                    srInfo.PStreet = collection["PStreet"].ToString();
                    //srInfo.PAddress = srInfo.PHouseNo + " " + srInfo.PWardNo + " " + srInfo.PDistrict + " " + srInfo.PVDC + " " + srInfo.PZone;
                    
                    srInfo.CProvince = collection["CProvince"].ToString();
                    srInfo.CDistrict = collection["CDistrict"].ToString();
                    //srInfo.CDistrict = districtID.ToString();
                    //srInfo.CZone = collection["txtCZone"].ToString();
                    //srInfo.CDistrict = collection["txtCDistrict"].ToString();
                    srInfo.CVDC = collection["txtCVDC"].ToString();
                    srInfo.CHouseNo = collection["txtCHouseNo"].ToString();
                    srInfo.CWardNo = collection["txtCWardNo"].ToString();
                    srInfo.CStreet = collection["CStreet"].ToString();
                    //srInfo.CAddress = srInfo.CHouseNo + " " + srInfo.CWardNo + " " + srInfo.CDistrict + " " + srInfo.CVDC + " " + srInfo.CZone;
                    //srInfo.Address = srInfo.CAddress;
                    
                    //kyc4
                    srInfo.Citizenship = collection["txtCitizenship"].ToString();
                    srInfo.CitizenshipIssueDate = collection["CitizenshipIssueDate"].ToString();
                    srInfo.CitizenshipPlaceOfIssue = collection["txtCitizenshipPlaceOfIssue"].ToString();
                    if (!string.IsNullOrEmpty(srInfo.CitizenshipIssueDate))
                    {
                        srInfo.CitizenshipIssueDate = DateTime.ParseExact(srInfo.CitizenshipIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        srInfo.CitizenshipIssueDate = "";
                    }

                    srInfo.License = collection["txtLicense"].ToString();
                    srInfo.LicenseIssueDate = collection["LicenseIssueDate"].ToString();
                    srInfo.LicenseExpireDate = collection["LicenseExpireDate"].ToString();
                    if (!string.IsNullOrEmpty(srInfo.LicenseIssueDate))
                    {
                        srInfo.LicenseIssueDate = DateTime.ParseExact(srInfo.LicenseIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        srInfo.LicenseIssueDate = "";
                    }
                    srInfo.LicensePlaceOfIssue = collection["txtLicensePlaceOfIssue"].ToString();

                    if (!string.IsNullOrEmpty(srInfo.LicenseExpireDate))
                    {
                        srInfo.LicenseExpireDate = DateTime.ParseExact(srInfo.LicenseExpireDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        srInfo.LicenseExpireDate = "";
                    }


                    srInfo.Passport = collection["txtPassport"].ToString();
                    srInfo.PassportIssueDate = collection["PassportIssueDate"].ToString();
                    srInfo.PassportExpireDate = collection["PassportExpireDate"].ToString();
                    if (!string.IsNullOrEmpty(srInfo.PassportIssueDate))
                    {
                        srInfo.PassportIssueDate = DateTime.ParseExact(srInfo.PassportIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        srInfo.PassportIssueDate = "";
                    }

                    srInfo.PassportPlaceOfIssue = collection["txtPassportPlaceOfIssue"].ToString();

                    if (!string.IsNullOrEmpty(srInfo.PassportExpireDate))
                    {
                        srInfo.PassportExpireDate = DateTime.ParseExact(srInfo.PassportExpireDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        srInfo.PassportExpireDate = "";
                    }


                    srInfo.Document = collection["txtDocument"].ToString();


                    srInfo.UserType = "user";
                    srInfo.Status = "Active";
                    //srInfo.IsApproved = "UnApprove";
                    srInfo.IsRejected = "F";
                    srInfo.BranchCode = "004";
                    //srInfo.OTPCode = code;
                    srInfo.Source = "http";

                    
                    if (!ViewData.ModelState.IsValid)
                    {
                        this.ViewData["registration_message"] = " *Validation Error.";
                        this.ViewData["message_class"] = "failed_info";
                        return View();
                    }

                    bool result = false;
                    string errorMessage = string.Empty;
                    if ((srInfo.UserName != "") && (srInfo.Password != ""))
                    {
                        DataTable dtableMobileNo = RegisterUtils.GetCheckMobileNo(srInfo.UserName);
                        if (dtableMobileNo.Rows.Count == 0)
                        {
                            if (collection.AllKeys.Any())
                            {
                                try
                                {
                                    var PP = SaveAndReturnFileName(model.PassportPhoto, mobile);
                                    var front = SaveAndReturnFileName(model.Front, mobile);
                                    var back = SaveAndReturnFileName(model.Back, mobile);

                                    srInfo.PassportImage = string.Format(PP);
                                    srInfo.FrontImage = string.Format(front);
                                    srInfo.BackImage = string.Format(back);

                                    int results = RegisterUtils.CustomerSelfReg(srInfo);
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
                                        TempData["agent_message"] = "Registration information is successfully added.Please wait for approval";
                                        TempData["message_class"] = CssSetting.SuccessMessageClass;
                                        return RedirectToAction("../AgentDashboard/Index");
                                          
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
                                                  ? "Registration information is successfully added."
                                                  : "Error while inserting the information. ERROR :: "
                                                    + errorMessage;
                    this.ViewData["message_class"] = result ? "success_info" : "failed_info";




                    return View(srInfo);
                }
                else
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            catch (Exception ex)
            {
                
                return View(srInfo);
            }

        }


        #endregion

        #region CustomerKYC Finish
        public ActionResult CustomerKYCFinish()
        {
            

            if (this.TempData["registration_message"] != null)
            {
                this.TempData["changepwd_message"] = this.TempData["changepwd_message"];
                this.TempData["message_class"] = this.TempData["message_class"];
            }
           
            return View();

        }
        #endregion


        public ActionResult CustomerKYC5()
        {
            string mobile = (string)Session["Mobile"];

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
            if (this.TempData["registration_message"] != null)
            {
                this.TempData["changepwd_message"] = this.TempData["changepwd_message"];
                this.TempData["message_class"] = this.TempData["message_class"];
            }
            CustomerSRInfo srobj = new CustomerSRInfo();
            return View(srobj);

        }


        // POST: Signup/CustomerKYC5
        [AcceptVerbs(HttpVerbs.Post)]
        [HttpPost]
        public ActionResult CustomerKYC5(FormCollection collection, CustomerSRInfo model)
        {


            CustomerSRInfo srInfo = new CustomerSRInfo();
            try
            {

                string mobile = (string)Session["Mobile"];
                string code = (string)Session["Code"];

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

                //string PDistrict, PProvincestr, CDistrict, CProvincestr, t = string.Empty;

                if ((mobile != "null"))
                {



                    // //START milayako
                    //PDistrict = collection["PDistrict"].ToString();

                    // string Pdistrictstring = "SELECT DistrictID FROM MNDistrict WHERE Name ='" + PDistrict + "'";
                    // DataTable Pdt1 = new DataTable();
                    // Pdt1 = objdal.MyMethod(Pdistrictstring);
                    // List<SelectListItem> listDistrict = new List<SelectListItem>();
                    // string districtID = string.Empty;
                    // foreach (DataRow row in Pdt1.Rows)
                    // {
                    //     districtID = row["DistrictID"].ToString();

                    // }
                    // //string districtID = listDistrict.ToString();
                    // string PtestDistrictID = districtID;

                    // PProvincestr = collection["PProvince"].ToString();

                    // string Pprovincestring = "SELECT * FROM MNProvince WHERE ProvinceID='" + PProvincestr + "'";
                    // DataTable Pdt = new DataTable();
                    // Pdt = objdal.MyMethod(Pprovincestring);
                    // List<SelectListItem> Plist = new List<SelectListItem>();
                    // foreach (DataRow row in Pdt.Rows)
                    // {
                    //     Plist.Add(new SelectListItem
                    //     {
                    //         Text = Convert.ToString(row.ItemArray[1]),
                    //         Value = Convert.ToString(row.ItemArray[0])\



                    //     });

                    // }
                    // string PProvince = Plist[0].Text.ToString();


                    // //end milayako2

                    // //START milayako
                    // CDistrict = collection["CDistrict"].ToString();

                    // string Cdistrictstring = "SELECT DistrictID FROM MNDistrict WHERE Name ='" + CDistrict + "'";
                    // DataTable Cdt1 = new DataTable();
                    // Cdt1 = objdal.MyMethod(Cdistrictstring);
                    // List<SelectListItem> ClistDistrict = new List<SelectListItem>();
                    // string CdistrictID = string.Empty;
                    // foreach (DataRow row in Cdt1.Rows)
                    // {
                    //     districtID = row["DistrictID"].ToString();

                    // }
                    // //string districtID = listDistrict.ToString();
                    // string CtestDistrictID = districtID;

                    // CProvincestr = collection["PProvince"].ToString();

                    // string Cprovincestring = "SELECT * FROM MNProvince WHERE ProvinceID='" + CProvincestr + "'";
                    // DataTable Cdt = new DataTable();
                    // Cdt = objdal.MyMethod(Cprovincestring);
                    // List<SelectListItem> Clist = new List<SelectListItem>();
                    // foreach (DataRow row in Cdt.Rows)
                    // {
                    //     Clist.Add(new SelectListItem
                    //     {
                    //         Text = Convert.ToString(row.ItemArray[1]),
                    //         Value = Convert.ToString(row.ItemArray[0])
                    //     });

                    // }
                    // string CProvince = Clist[0].Text.ToString();


                    // //end milayako2
                    srInfo.UserName = mobile;

                    srInfo.FName = collection["txtFirstName"].ToString();
                    srInfo.MName = collection["txtMiddleName"].ToString();
                    srInfo.LName = collection["txtLastName"].ToString();


                    //srInfo.Name = srInfo.FName + " " + srInfo.MName + " " + srInfo.LName;
                    srInfo.Gender = collection["txtGender"].ToString();
                    srInfo.DOB = collection["DOB"].ToString();
                    srInfo.BSDateOfBirth = collection["BSDateOfBirth"].ToString();
                    //srInfo.DateADBS = collection["txtDateADBS"].ToString();

                    srInfo.Nationality = collection["txtNationality"].ToString();
                    srInfo.FatherName = collection["txtFatherName"].ToString();
                    srInfo.MotherName = collection["txtMotherName"].ToString();
                    srInfo.SpouseName = collection["SpouseName"].ToString();
                    srInfo.MaritalStatus = collection["MaritalStatus"].ToString();


                    srInfo.GrandFatherName = collection["txtGrandFatherName"].ToString();
                    srInfo.FatherInlawName = collection["txtFatherInlawName"].ToString();
                    srInfo.Occupation = collection["txtOccupation"].ToString();


                    srInfo.EmailAddress = collection["txtEmail"].ToString();
                    srInfo.PanNo = collection["txtPanNo"].ToString();


                    srInfo.Country = collection["txtCountry"].ToString();


                    srInfo.PProvince = collection["PProvince"].ToString();
                    srInfo.PDistrict = collection["PDistrict"].ToString();
                    //srInfo.PDistrict = districtID.ToString();

                    //srInfo.PZone = collection["txtPZone"].ToString();
                    //srInfo.PDistrict = collection["txtPDistrict"].ToString();
                    srInfo.PVDC = collection["txtPVDC"].ToString();
                    srInfo.PHouseNo = collection["txtPHouseNo"].ToString();
                    srInfo.PWardNo = collection["txtPWardNo"].ToString();
                    srInfo.PStreet = collection["PStreet"].ToString();
                    //srInfo.PAddress = srInfo.PHouseNo + " " + srInfo.PWardNo + " " + srInfo.PDistrict + " " + srInfo.PVDC + " " + srInfo.PZone;


                    srInfo.CProvince = collection["CProvince"].ToString();
                    srInfo.CDistrict = collection["CDistrict"].ToString();
                    //srInfo.CDistrict = districtID.ToString();

                    //srInfo.CZone = collection["txtCZone"].ToString();
                    //srInfo.CDistrict = collection["txtCDistrict"].ToString();
                    srInfo.CVDC = collection["txtCVDC"].ToString();
                    srInfo.CHouseNo = collection["txtCHouseNo"].ToString();
                    srInfo.CWardNo = collection["txtCWardNo"].ToString();
                    srInfo.CStreet = collection["CStreet"].ToString();
                    //srInfo.CAddress = srInfo.CHouseNo + " " + srInfo.CWardNo + " " + srInfo.CDistrict + " " + srInfo.CVDC + " " + srInfo.CZone;
                    //srInfo.Address = srInfo.CAddress;

                    srInfo.Citizenship = collection["txtCitizenship"].ToString();
                    srInfo.CitizenshipIssueDate = collection["CitizenshipIssueDate"].ToString();
                    srInfo.CitizenshipPlaceOfIssue = collection["txtCitizenshipPlaceOfIssue"].ToString();


                    srInfo.License = collection["txtLicense"].ToString();
                    //test//
                    if (!string.IsNullOrEmpty(srInfo.LicenseIssueDate))
                    {
                        DateTime.ParseExact(srInfo.LicenseIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        srInfo.LicenseIssueDate = "";
                    }
                    srInfo.LicensePlaceOfIssue = collection["txtLicensePlaceOfIssue"].ToString();

                    if (!string.IsNullOrEmpty(srInfo.LicenseExpireDate))
                    {
                        DateTime.ParseExact(srInfo.LicenseExpireDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        srInfo.LicenseExpireDate = "";
                    }


                    srInfo.Passport = collection["txtPassport"].ToString();

                    if (!string.IsNullOrEmpty(srInfo.PassportIssueDate))
                    {
                        DateTime.ParseExact(srInfo.PassportIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        srInfo.PassportIssueDate = "";
                    }

                    srInfo.PassportPlaceOfIssue = collection["txtPassportPlaceOfIssue"].ToString();

                    if (!string.IsNullOrEmpty(srInfo.PassportExpireDate))
                    {
                        DateTime.ParseExact(srInfo.PassportExpireDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        srInfo.PassportExpireDate = "";
                    }


                    srInfo.Document = collection["txtDocument"].ToString();


                    srInfo.UserType = "user";
                    srInfo.Status = "Active";
                    //srInfo.IsApproved = "UnApprove";
                    srInfo.IsRejected = "F";
                    srInfo.BranchCode = "004";
                    srInfo.OTPCode = code;
                    srInfo.Source = "http";



                    if (!ViewData.ModelState.IsValid)
                    {
                        this.ViewData["registration_message"] = " *Validation Error.";
                        this.ViewData["message_class"] = "failed_info";
                        return View();
                    }

                    bool result = false;
                    string errorMessage = string.Empty;


                    if ((srInfo.UserName != "") && (srInfo.Password != ""))
                    {
                        DataTable dtableMobileNo = RegisterUtils.GetCheckMobileNo(srInfo.UserName);
                        if (dtableMobileNo.Rows.Count == 0)
                        {
                            if (collection.AllKeys.Any())
                            {
                                try
                                {
                                    var PP = SaveAndReturnFileName(model.PassportPhoto, mobile);
                                    var front = SaveAndReturnFileName(model.Front, mobile);
                                    var back = SaveAndReturnFileName(model.Back, mobile);

                                    srInfo.PassportImage = string.Format("~/Content/Upload/{0}", PP);
                                    srInfo.FrontImage = string.Format("~/Content/Upload/{0}", front);
                                    srInfo.BackImage = string.Format("~/Content/Upload/{0}", back);

                                    int results = RegisterUtils.CustomerSelfReg(srInfo);
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
                                        TempData["login_message"] = "Registration information is successfully added.Please wait for approval";
                                        TempData["message_class"] = CssSetting.SuccessMessageClass;
                                        return RedirectToAction("Index", "Login");
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
                                                  ? "Registration information is successfully added."
                                                  : "Error while inserting the information. ERROR :: "
                                                    + errorMessage;
                    this.ViewData["message_class"] = result ? "success_info" : "failed_info";




                    return View(srInfo);
                }
                else
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            catch (Exception ex)
            {
                return View(srInfo);
            }
        }

        public ActionResult ResendCode(FormCollection collection)
        {
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

            try
            {
                if (!IsValidUserName(userName))
                {
                    code = TraceIdGenerator.GetUniqueKey();
                    string messagereply = "Dear Customer," + "\n";
                    messagereply += " Your Verification Code is " + code
                        + "." + "\n" + "Close this message and enter code to verify account.";
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
                    log.Purpose = "Self Registration";
                    log.UserName = userName;
                    log.Message = messagereply;
                    CustomerUtils.LogSMS(log);

                    Session["Mobile"] = mobile;
                    Session["Code"] = code;
                    return RedirectToAction("Verify");
                }
                else
                {
                    displayMessage = "\n The number is registered !!";
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