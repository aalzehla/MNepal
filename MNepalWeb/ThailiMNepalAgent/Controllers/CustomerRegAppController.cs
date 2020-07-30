using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ThailiMNepalAgent.Helper;
using ThailiMNepalAgent.Models;
using ThailiMNepalAgent.Settings;
using ThailiMNepalAgent.Utilities;
using System.Data;
using System.Globalization;
using System.IO;
using System.Web.Helpers;
using System.Web.SessionState;

namespace ThailiMNepalAgent.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class CustomerRegAppController : Controller
    {
        #region "Customer CreateRegistration"
        DAL objdal = new DAL();//rakhnu parne

        // GET: CustomerApp
        public ActionResult CreateRegistration()
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

        //start 00

        // POST: Customer/CreateRegistration
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


                    //userInfo.UserName = collection["txtUserName"].ToString();
                    //userInfo.ContactNumber1 = collection["txtContactNumber1"].ToString();
                    userInfo.FName = collection["txtFirstName"].ToString();
                    userInfo.MName = collection["txtMiddleName"].ToString();
                    userInfo.LName = collection["txtLastName"].ToString();


                    userInfo.Name = userInfo.FName + " " + userInfo.MName + " " + userInfo.LName;
                    userInfo.Gender = collection["txtGender"].ToString();
                    userInfo.DOB = collection["DOB"].ToString();


                    userInfo.Nationality = collection["txtNationality"].ToString();
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




                    userInfo.GrandFatherName = collection["txtGrandFatherName"].ToString();
                    //userInfo.FatherInlawName = collection["txtFatherInlawName"].ToString();
                    userInfo.Occupation = collection["txtOccupation"].ToString();


                    //userInfo.EmailAddress = collection["txtEmail"].ToString();
                    userInfo.PanNo = collection["txtPanNo"].ToString();

                    //userInfo.Photo1 = collection["txtPhoto1"].ToString();
                    //userInfo.Country = collection["txtCountry"].ToString();
                    //userInfo.ContactNumber2 = collection["txtContactNumber2"].ToString();


                    userInfo.PProvince = collection["PProvince"].ToString();
                    userInfo.PDistrict = collection["PDistrictID"].ToString();
                    userInfo.PVDC = collection["txtPVDC"].ToString();
                    userInfo.PHouseNo = collection["txtPHouseNo"].ToString();
                    userInfo.PWardNo = collection["txtPWardNo"].ToString();
                    userInfo.PStreet = collection["PStreet"].ToString();
                    userInfo.Address = userInfo.PProvince + "," + userInfo.PDistrict + "," + userInfo.PVDC + "," + userInfo.PWardNo + "," + userInfo.PHouseNo + "," + userInfo.PStreet;


                    userInfo.CProvince = collection["CProvince"].ToString();
                    userInfo.CDistrict = collection["CDistrictID"].ToString();
                    userInfo.CVDC = collection["txtCVDC"].ToString();
                    userInfo.CHouseNo = collection["txtCHouseNo"].ToString();
                    userInfo.CWardNo = collection["txtCWardNo"].ToString();
                    userInfo.CStreet = collection["CStreet"].ToString();

                    userInfo.Citizenship = collection["txtCitizenship"].ToString();

                    userInfo.CitizenshipIssueDate = collection["CitizenshipIssueDate"].ToString();


                    userInfo.CitizenshipPlaceOfIssue = collection["txtCitizenshipPlaceOfIssue"].ToString();

                    userInfo.License = "";

                    userInfo.LicenseIssueDate = "";

                    userInfo.LicensePlaceOfIssue = "";

                    userInfo.LicenseExpireDate = "";



                    if (collection.AllKeys.Contains("PassportIssueDate"))
                    {

                        userInfo.Passport = collection["txtPassport"].ToString();
                    }
                    else
                    {
                        userInfo.Passport = "";
                    }


                    //if (collection.AllKeys.Contains("PassportIssueDate"))
                    //{
                    //    userInfo.PassportIssueDate = DateTime.ParseExact(collection["PassportIssueDate"].ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture)
                    //                            .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    //    //userInfo.PassportIssueDate = collection["PassportIssueDate"].ToString();
                    //}
                    //else
                    //{
                    //    userInfo.PassportIssueDate = "";
                    //}
                    if (!string.IsNullOrEmpty(userInfo.PassportIssueDate))
                    {
                        DateTime.ParseExact(userInfo.PassportIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                               .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        userInfo.PassportIssueDate = "";
                    }

                    if (!string.IsNullOrEmpty(userInfo.PassportExpireDate))
                    {
                        DateTime.ParseExact(userInfo.PassportExpireDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                               .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        userInfo.PassportExpireDate = "";
                    }

                    if (collection.AllKeys.Contains("PassportIssueDate"))
                    {
                        userInfo.PassportPlaceOfIssue = collection["txtPassportPlaceOfIssue"].ToString();
                    }
                    else
                    {
                        userInfo.PassportPlaceOfIssue = "";
                    }


                    //if (collection.AllKeys.Contains("PassportExpireDate"))
                    //{
                    //    userInfo.PassportExpireDate =DateTime.ParseExact(collection["PassportExpireDate"].ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture)
                    //                            .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

                    //}
                    //else
                    //{
                    //    userInfo.PassportExpireDate = "";
                    //}


                    userInfo.Document = collection["txtDocument"].ToString();
                    userInfo.UserName = collection["txtUserName"].ToString();
                    userInfo.UserType = "user";
                    userInfo.Status = "Active";
                    userInfo.IsApproved = "UnApprove";
                    userInfo.IsRejected = "F";
                    string mobile = userInfo.UserName;



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
                                    var PP = SaveAndReturnFileName(model.PassportPhoto, mobile);
                                    var front = SaveAndReturnFileName(model.Front, mobile);
                                    var back = SaveAndReturnFileName(model.Back, mobile);

                                    userInfo.PassportImage = string.Format("~/Content/Upload/{0}", PP);
                                    userInfo.FrontImage = string.Format("~/Content/Upload/{0}", front);
                                    userInfo.BackImage = string.Format("~/Content/Upload/{0}", back);

                                    int results = RegisterUtils.CustomerRegAppRegisterCustomerInfo(userInfo);
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
       
        //end  00
    }
}

//
#endregion






