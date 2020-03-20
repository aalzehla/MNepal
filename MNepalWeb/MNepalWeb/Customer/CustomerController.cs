using KellermanSoftware.CompareNetObjects;
using MNepalWeb.App_Start;
using MNepalWeb.Helper;
using MNepalWeb.Models;
using MNepalWeb.Settings;
using MNepalWeb.UserModels;
using MNepalWeb.Utilities;
using MNepalWeb.ViewModel;
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

namespace MNepalWeb.Controllers
{

    public class CustomerController : Controller
    {
        DAL objdal = new DAL();

        #region "Customer Registration "

        // GET: Customer/Registration
        public ActionResult Registration()
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
                UserInfo UInfo = new UserInfo();
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

                ViewBag.User = "user";
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

                //For ProfileName
                List<MNCustProfile> ProfileList = new List<MNCustProfile>();
                List<SelectListItem> ItemProfile = new List<SelectListItem>();
                ProfileList = CusProfileUserModel.GetMNCustProfile().Where(x => x.ProfileStatus == 'A').ToList();

                foreach (MNCustProfile Cust in ProfileList)
                {
                    ItemProfile.Add(new SelectListItem
                    {
                        Text = Cust.ProfileCode,
                        Value = Cust.ProfileCode
                    });
                }

                ViewBag.ProfileList = new SelectList(ItemProfile.OrderBy(x => x.Text), "Value", "Text");
                ViewBag.Wallet = item[0].Text.ToString();
                ViewBag.WalletCode = item[0].Value.ToString();
                ViewBag.Bank = item.Where(x => x.Value != "0000").ToList();

                string bankCode = item[0].Value.ToString();
                string unique = TraceIdGenerator.GetUniqueWalletNo();
                ViewBag.WalletNumber = bankCode + t + unique;
                UInfo.UserType = "user";
                UInfo.ClientCode = t;
                UInfo.WalletNumber = bankCode + t + unique;
                UInfo.WalletName = item[0].Text.ToString(); //Wallet Name
                UInfo.WalletCode = item[0].Value.ToString(); //Wallet Code

                UInfo.StartDate = DateTime.Now.ToString("dd/MM/yyyy");
                //UInfo.EndDate = DateTime.Now.ToString("dd/MM/yyyy");

                return this.View(UInfo);
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
            list.Add(new SelectListItem { Text = "--Select District--", Value = "0" });
            foreach (DataRow row in dt.Rows)
            {

                list.Add(new SelectListItem { Text = Convert.ToString(row.ItemArray[1]), Value = Convert.ToString(row.ItemArray[0]) });

            }
            return Json(new SelectList(list, "Text", "Text", JsonRequestBehavior.AllowGet)).ToString();
        }
        //end milayako

        /// <param name="collection"></param>
        /// <returns></returns>
        // POST: Customer/Registration/1
        [HttpPost]
        public async Task<ActionResult> Registration(FormCollection collection, UserInfo model)
        {

            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            string t = string.Empty;

            //start milayako 02
            string provincestring2 = "select * from MNProvince";
            DataTable dt2 = new DataTable();
            dt2 = objdal.MyMethod(provincestring2);
            List<SelectListItem> list2 = new List<SelectListItem>();
            foreach (DataRow row in dt2.Rows)
            {
                list2.Add(new SelectListItem
                {
                    Text = Convert.ToString(row.ItemArray[1]),
                    Value = Convert.ToString(row.ItemArray[0])
                });

            }
            ViewBag.PProvince = list2;

            ViewBag.CProvince = list2;
            //end milayako 02
            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                UserInfo userInfo = new UserInfo();


                //userInfo.Address1 = txtAddress;
                userInfo.ClientCode = collection["ClientCode"].ToString();
                userInfo.AgentId = collection["AgentId"].ToString();
                userInfo.UserName = collection["UserName"].ToString();
                userInfo.UserType = collection["UserType"].ToString();
                userInfo.Password = CustomerUtils.GeneratePassword();//userInfo.UserType + "@123";                                   /*collection["txtPassword"].ToString()*/
                userInfo.Name = collection["Name"].ToString().Capitalize();
                //userInfo.Address = txtAddress + "," + txtWardNumber + "," + District + "," + Province;

                //start milayako 02
                userInfo.PStreet = collection["PStreet"].ToString();
                //userInfo.Name = collection["txtName"].ToString(); 
                userInfo.Gender = collection["txtGender"].ToString();
                userInfo.Nationality = collection["txtNationality"].ToString();
                userInfo.Country = collection["txtCountry"].ToString();
                userInfo.DOB = collection["DOB"].ToString();
                userInfo.BSDateOfBirth = collection["BSDateOfBirth"].ToString();
                userInfo.FatherName = collection["txtFatherName"].ToString();
                userInfo.MotherName = collection["txtMotherName"].ToString();
                userInfo.GrandFatherName = collection["txtGrandFatherName"].ToString();
                userInfo.Occupation = collection["txtOccupation"].ToString();
                userInfo.MaritalStatus = collection["txtMaritalStatus"].ToString();
                userInfo.SpouseName = collection["txtSpouseName"].ToString();
                userInfo.FatherInlawName = collection["txtFatherInLawsName"].ToString();
                userInfo.PProvince = collection["PProvince"].ToString();
                userInfo.PDistrictID = collection["PDistrictID"].ToString();
                userInfo.PMunicipalityVDC = collection["PMunicipalityVDC"].ToString();
                userInfo.PHouseNo = collection["PHouseNo"].ToString();
                userInfo.PWardNo = collection["PWardNo"].ToString();

                userInfo.CStreet = collection["CStreet"].ToString();
                userInfo.CProvince = collection["CProvince"].ToString();
                userInfo.CDistrictID = collection["CDistrictID"].ToString();
                userInfo.CMunicipalityVDC = collection["CMunicipalityVDC"].ToString();
                userInfo.CHouseNo = collection["CHouseNo"].ToString();
                userInfo.CWardNo = collection["CWardNo"].ToString();



                userInfo.Document = collection["txtDocument"].ToString();

                userInfo.Citizenship = collection["txtCitizenship"].ToString();
                userInfo.CitizenshipIssueDate = collection["CitizenshipIssueDate"].ToString();
                userInfo.BSCitizenshipIssueDate = collection["BSCitizenshipIssueDate"].ToString();
                userInfo.CitizenshipPlaceOfIssue = collection["txtCitizenshipPlaceOfIssue"].ToString();

                userInfo.License = collection["txtLicense"].ToString();
                userInfo.LicenseIssueDate = collection["LicenseIssueDate"].ToString();
                userInfo.BSLicenseIssueDate = collection["BSLicenseIssueDate"].ToString();
                userInfo.LicenseExpireDate = collection["LicenseExpireDate"].ToString();
                userInfo.BSLicenseExpireDate = collection["BSLicenseExpireDate"].ToString();
                userInfo.LicensePlaceOfIssue = collection["txtLicensePlaceOfIssue"].ToString();


                userInfo.Passport = collection["txtPassport"].ToString();
                userInfo.PassportIssueDate = collection["PassportIssueDate"].ToString();
                userInfo.PassportExpireDate = collection["PassportExpireDate"].ToString();
                userInfo.PassportPlaceOfIssue = collection["txtPassportPlaceOfIssue"].ToString();



                //emd milayako 02
                //current address as Default adderess
                userInfo.Address = userInfo.CHouseNo + "," + userInfo.CStreet + "," + userInfo.CWardNo + "," + userInfo.CDistrictID + "," + userInfo.CProvince;

                userInfo.UserType = collection["UserType"].ToString();
                userInfo.PIN = CustomerUtils.GeneratePin();
                userInfo.Status = "Active";//collection["txtStatus"].ToString();
                userInfo.IsApproved = collection["txtIsApproved"].ToString();
                userInfo.IsRejected = collection["txtIsRejected"].ToString();

                userInfo.ContactNumber1 = collection["ContactNumber1"].ToString();
                userInfo.ContactNumber2 = collection["ContactNumber2"].ToString();
                userInfo.EmailAddress = collection["Email"].ToString();
                userInfo.WalletName = collection["WalletName"].ToString();
                userInfo.WalletCode = collection["WalletCode"].ToString();

                userInfo.WBranchCode = collection["WBranchCode"].ToString();
                userInfo.WalletNumber = collection["txtWalletNumber"].ToString();
                userInfo.WIsDefault = collection["txtWIsDefault"].ToString();
                userInfo.BankNo = collection["BankNoBin"];
                userInfo.WBankCode = collection["WalletCode"];
                userInfo.IsDefault = collection["txtIsDefault"].ToString();
                userInfo.BankAccountNumber = collection["txtBankAccountNumber"].ToString();
                userInfo.ProfileCode = collection["CustProfile"].ToString();

                userInfo.Transaction = collection["Transaction"].ToString();
                //userInfo.DateRange = collection["DateRange"].ToString();
                if (collection.AllKeys.Contains("DateRange"))
                {
                    userInfo.DateRange = collection["DateRange"].ToString();
                    userInfo.StartDate = collection["StartDate"].ToString();
                    userInfo.EndDate = collection["EndDate"].ToString();

                }
                else
                {
                    userInfo.DateRange = "";
                    userInfo.StartDate = "";
                    userInfo.EndDate = "";
                }
                if (collection.AllKeys.Contains("LimitType"))
                {
                    userInfo.LimitType = collection["LimitType"].ToString();
                }
                else
                {
                    userInfo.LimitType = "";
                }

                userInfo.TransactionLimit = collection["TransactionLimit"].ToString();
                userInfo.TransactionCount = collection["TransactionCount"].ToString();
                userInfo.TransactionLimitMonthly = collection["TransactionLimitMonthly"].ToString();
                userInfo.TransactionLimitDaily = collection["TransactionLimitDaily"].ToString();


                userInfo.BranchCode = string.Empty;


                if (userInfo.BankAccountNumber != "")
                    userInfo.BranchCode = userInfo.BankAccountNumber.Substring(0, 3);
                userInfo.AdminUserName = (string)Session["UserName"];
                userInfo.AdminBranch = (string)Session["UserBranch"];
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
                string unique = TraceIdGenerator.GetUniqueWalletNo();
                ViewBag.WalletNumber = bankCode + t + unique;

                //For ProfileName
                List<MNCustProfile> ProfileList = new List<MNCustProfile>();
                List<SelectListItem> ItemProfile = new List<SelectListItem>();
                ProfileList = CusProfileUserModel.GetMNCustProfile().Where(x => x.ProfileStatus == 'A').ToList();

                foreach (MNCustProfile Cust in ProfileList)
                {
                    ItemProfile.Add(new SelectListItem
                    {
                        Text = Cust.ProfileCode,
                        Value = Cust.ProfileCode
                    });
                }

                ViewBag.ProfileList = new SelectList(ItemProfile.OrderBy(x => x.Text), "Value", "Text");
                string[] Accounts;
                List<TransactionInfo> TransAccount = new List<TransactionInfo>();

                if (!String.IsNullOrEmpty(collection["hidAccounts"].ToString()))
                {
                    Accounts = collection["hidAccounts"].ToString().Split(',');
                    Accounts = Accounts.Where(x => !string.IsNullOrEmpty(x.Trim())).ToArray();
                    foreach (string Account in Accounts)
                    {
                        TransactionInfo txnfo = new TransactionInfo();
                        txnfo.AcNumber = collection[String.Format("txtAcNumber{0}", Account)].ToString();
                        txnfo.Alias = collection[String.Format("txtAcAllias{0}", Account)].ToString();
                        txnfo.AcOwner = collection[String.Format("txtAcOwner{0}", Account)].ToString();
                        txnfo.AcType = collection[String.Format("txtAcType{0}", Account)].ToString();
                        txnfo.TBranchCode = collection[String.Format("txtBranchCode{0}", Account)].ToString();
                        if (collection.AllKeys.Contains(String.Format("chkPrimary{0}", Account)))
                        {
                            txnfo.IsPrimary = collection[String.Format("chkPrimary{0}", Account)].ToString();
                        }
                        else
                        {
                            txnfo.IsPrimary = "F";
                        }
                        if (collection.AllKeys.Contains(String.Format("chkTxnEnable{0}", Account)))
                        {
                            txnfo.TxnEnabled = collection[String.Format("chkTxnEnable{0}", Account)].ToString();
                        }
                        else
                        {
                            txnfo.TxnEnabled = "F";
                        }
                        //TransAccount.Add(new TransactionInfo
                        //{
                        //    AcNumber = collection[String.Format("txtAcNumber{0}", Account)].ToString(),
                        //    Alias = collection[String.Format("txtAcAllias{0}", Account)].ToString(),
                        //    AcOwner = collection[String.Format("txtAcOwner{0}", Account)].ToString(),
                        //    AcType = collection[String.Format("txtAcType{0}", Account)].ToString(),
                        //    TBranchCode = collection[String.Format("txtBranchCode{0}", Account)].ToString(),
                        //    if(collection.AllKeys.Contains(String.Format("chkPrimary{0}", Account)))true?"T":"F",
                        //    TxnEnabled=collection.AllKeys.Contains(String.Format("chkTxnEnable{0}", Account)) == true ? "T" : "F"
                        //});
                        if (txnfo.AcNumber != string.Empty || txnfo.AcNumber != "")
                            TransAccount.Add(txnfo);
                    }

                }

                CusProfileUtils cUtil = new CusProfileUtils();
                string AccXML = cUtil.GetServiceAccountXMLStr(TransAccount);
                userInfo.TxnAccounts = AccXML;

                //if (string.IsNullOrEmpty(collection["UserType"]))
                //{
                //    ModelState.AddModelError("UserType", "*Please enter UserType");
                //}
                if (string.IsNullOrEmpty(collection["UserName"]))
                {
                    ModelState.AddModelError("UserName", "Please enter UserName");
                }
                //if (string.IsNullOrEmpty(collection["txtPassword"]))
                //{
                //    ModelState.AddModelError("Password", "*Please enter Password");
                //}
                if (string.IsNullOrEmpty(collection["Name"]))
                {
                    ModelState.AddModelError("Name", "Please enter First Name");
                }
                if (string.IsNullOrEmpty(collection["DOB"]))
                {
                    ModelState.AddModelError("DOB", "Please enter your Date of Birth(AD)");
                }

                if (string.IsNullOrEmpty(collection["BSDateOfBirth"]))
                {
                    ModelState.AddModelError("BSDateOfBirth", "Please enter your Date of Birth(BS)");
                }
                if (string.IsNullOrEmpty(collection["txtFatherName"]))
                {
                    ModelState.AddModelError("txtFatherName", "Please enter YourFatherName");
                }
                if (string.IsNullOrEmpty(collection["txtMotherName"]))
                {
                    ModelState.AddModelError("txtMotherName", "Please enter Your MotherName");
                }
                if (string.IsNullOrEmpty(collection["txtGrandFatherName"]))
                {
                    ModelState.AddModelError("txtGrandFatherName", "Please enter Your GrandFatherName");
                }
                if (string.IsNullOrEmpty(collection["txtOccupation"]))
                {
                    ModelState.AddModelError("txtOccupation", "Please select Your Occupation");
                }
                if (string.IsNullOrEmpty(collection["txtMaritalStatus"]))
                {
                    ModelState.AddModelError("MaritalStatus", "Please select Your MaritalStatus");
                }
                if (collection["txtMaritalStatus"] != "UnMarried")
                {
                    if (string.IsNullOrEmpty(collection["txtSpouseName"]))
                    {
                        ModelState.AddModelError("txtSpouseName", "Please enter Your SpouseName");
                    }
                    if (string.IsNullOrEmpty(collection["txtFatherInLawsName"]))
                    {
                        ModelState.AddModelError("txtFatherInLawsName", "Please enter Your FatherInLawsName");
                    }
                }
                //start milayako 02
                if (string.IsNullOrEmpty(collection["PStreet"]))
                {
                    ModelState.AddModelError("PStreet", "Please enter Street");
                }
                if (string.IsNullOrEmpty(collection["PProvince"]))
                {
                    ModelState.AddModelError("PProvince", "Please enter Province");
                }
                if (string.IsNullOrEmpty(collection["PDistrictID"]))
                {
                    ModelState.AddModelError("PDistrictID", "Please enter District");
                }
                if (string.IsNullOrEmpty(collection["PMunicipalityVDC"]))
                {
                    ModelState.AddModelError("PMunicipalityVDC", "Please enter PVDC");
                }
                //if (string.IsNullOrEmpty(collection["PHouseNo"]))
                //{
                //    ModelState.AddModelError("PHouseNo", "Please enter PHouseNo");
                //}
                if (string.IsNullOrEmpty(collection["PWardNo"]))
                {
                    ModelState.AddModelError("PWardNo", "Please enter WardNo");
                }


                if (string.IsNullOrEmpty(collection["CStreet"]))
                {
                    ModelState.AddModelError("CStreet", "*Please enter Street");
                }
                if (string.IsNullOrEmpty(collection["CProvince"]))
                {
                    ModelState.AddModelError("CProvince", "*Please enter Province");
                }
                if (string.IsNullOrEmpty(collection["CDistrictID"]))
                {
                    ModelState.AddModelError("CDistrictID", "*Please enter District");
                }
                if (string.IsNullOrEmpty(collection["CMunicipalityVDC"]))
                {
                    ModelState.AddModelError("CMunicipalityVDC", "*Please enter CVDC");
                }
                //if (string.IsNullOrEmpty(collection["CHouseNo"]))
                //{
                //    ModelState.AddModelError("CHouseNo", "*Please enter CHouseNo");
                //}
                if (string.IsNullOrEmpty(collection["CWardNo"]))
                {
                    ModelState.AddModelError("CWardNo", "*Please enter CWardNo");
                }
                //if (string.IsNullOrEmpty(collection["txtDocument"]))
                //{
                //    ModelState.AddModelError("txtDocument", "txtDocument");
                //}
                //if (string.IsNullOrEmpty(collection["txtCitizenship"]))
                //{
                //    ModelState.AddModelError("txtCitizenship", "txtCitizenship");
                //}
                //if (string.IsNullOrEmpty(collection["txtCitizenshipPlaceOfIssue"]))
                //{
                //    ModelState.AddModelError("txtCitizenshipPlaceOfIssue", "txtCitizenshipPlaceOfIssue");
                //}
                //if (string.IsNullOrEmpty(collection["CitizenshipIssueDate"]))
                //{
                //    ModelState.AddModelError("CitizenshipIssueDate", "CitizenshipIssueDate");
                //}
                //if (string.IsNullOrEmpty(collection["txtLicense"]))
                //{
                //    ModelState.AddModelError("txtLicense", "txtLicense");
                //}
                //if (string.IsNullOrEmpty(collection["txtLicensePlaceOfIssue"]))
                //{
                //    ModelState.AddModelError("txtLicensePlaceOfIssue", "txtLicensePlaceOfIssue");
                //}
                //if (string.IsNullOrEmpty(collection["LicenseIssueDate"]))
                //{
                //    ModelState.AddModelError("LicenseIssueDate", "LicenseIssueDate");
                //}
                //if (string.IsNullOrEmpty(collection["LicenseExpireDate"]))
                //{
                //    ModelState.AddModelError("LicenseExpireDate", "LicenseExpireDate");
                //}

                //end milayako 02



                if (string.IsNullOrEmpty(collection["txtStatus"]))
                {
                    ModelState.AddModelError("Status", "*Please enter Status");
                }
                if (string.IsNullOrEmpty(collection["ContactNumber1"]))
                {
                    ModelState.AddModelError("ContactNumber", "*Please enter Contact Number");
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
                if (string.IsNullOrEmpty(collection["AgentId"]))
                {
                    ModelState.AddModelError("AgentId", "*Please enter AgentId");
                }
                if (string.IsNullOrEmpty(collection["txtIsDefault"]))
                {
                    ModelState.AddModelError("IsDefault", "*Please enter IsDefault");
                }

                if (model.PassportPhoto != null&&model.PassportPhoto.ContentLength > 0) { 
                userInfo.PassportImage = ParseCv(model.PassportPhoto);
                }
                //else
                //{

                //}
                if (model.Front != null && model.Front.ContentLength > 0)
                { userInfo.FrontImage = ParseCv(model.Front); }
                if (model.Back != null && model.Back.ContentLength > 0)
                { userInfo.BackImage = ParseCv(model.Back); }

                if (!ViewData.ModelState.IsValid)
                {
                    this.TempData["registration_message"] = " *Validation Error.";
                    this.TempData["message_class"] = "failed_info";
                    return View(userInfo);
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
                                var PP = ReturnFileName(model.PassportPhoto, userInfo.UserName);
                                var front = ReturnFileName(model.Front, userInfo.UserName);
                                var back = ReturnFileName(model.Back, userInfo.UserName);

                                userInfo.PassportImageName = string.Format(PP);
                                userInfo.FrontImageName = string.Format(front);
                                userInfo.BackImageName = string.Format(back);
                                await SavePhoto(userInfo);

                                userInfo.PassportImageName = Session["PP"].ToString();
                                userInfo.FrontImageName = Session["Front"].ToString();
                                userInfo.BackImageName = Session["Back"].ToString();

                                if (PP != null)
                                {
                                    userInfo.PassportImage = string.Format(PP);
                                }
                                else
                                {
                                    userInfo.PassportImage = "";
                                }
                                if (front != null)
                                {
                                    userInfo.FrontImage = string.Format(front);
                                }
                                else
                                {
                                    userInfo.FrontImage = "";
                                }
                                if (back != null)
                                {
                                    userInfo.BackImage = string.Format(back);
                                }
                                else
                                {
                                    userInfo.BackImage = "";
                                }


                                int results = RegisterUtils.RegisterUsersInfo(userInfo);
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
                this.TempData["registration_message"] = result
                                                ? "Registration information is successfully added.Please Check Registration Approval and perform accordingly"
                                                : "Error while inserting the information. ERROR :: "
                                                + errorMessage;
                this.TempData["message_class"] = result ? "success_info" : "failed_info";

                //For Bank


                if (result)
                    return RedirectToAction("Registration", "Customer");
                else
                    return View(userInfo);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
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


        #region "Customer List View"

        // GET: Customer/ListView
        [HttpGet]
        public ActionResult ListView()
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


        #region "Customer Modification"

        [HttpGet]
        public ActionResult CustomerModifyDetail(string SearchCol, string txtMobileNo, string txtName, string txtAccountNo)
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
                ViewBag.Message = "No Result Found !!";
                UserInfo userInfo = new UserInfo();
                userInfo.ContactNumber1 = txtMobileNo; //= collection["txtMobileNo"].ToString();
                userInfo.Name = txtName;// = collection["txtName"].ToString();
                userInfo.WalletNumber = txtAccountNo;// = collection["txtAccountNo"].ToString();

                List<UserInfo> CustomerStatus = new List<UserInfo>();

                if ((SearchCol == "All") || (userInfo.Name != "") && (userInfo.WalletNumber != "") && (userInfo.ContactNumber1 != ""))
                {
                    DataTable dtableCustomerStatusByAC = CustomerUtils.GetUserProfileByALL(userInfo.Name, userInfo.WalletNumber, userInfo.ContactNumber1);
                    if (dtableCustomerStatusByAC != null && dtableCustomerStatusByAC.Rows.Count > 0)
                    {
                        UserInfo regobj = new UserInfo();
                        regobj.ClientCode = dtableCustomerStatusByAC.Rows[0]["ClientCode"].ToString();
                        regobj.Name = dtableCustomerStatusByAC.Rows[0]["Name"].ToString();
                        regobj.Address = dtableCustomerStatusByAC.Rows[0]["Address"].ToString();
                        regobj.PIN = dtableCustomerStatusByAC.Rows[0]["PIN"].ToString();
                        regobj.Status = dtableCustomerStatusByAC.Rows[0]["Status"].ToString();
                        regobj.ContactNumber1 = dtableCustomerStatusByAC.Rows[0]["ContactNumber1"].ToString();
                        regobj.ContactNumber2 = dtableCustomerStatusByAC.Rows[0]["ContactNumber2"].ToString();
                        regobj.UserName = dtableCustomerStatusByAC.Rows[0]["UserName"].ToString();
                        regobj.UserType = dtableCustomerStatusByAC.Rows[0]["userType"].ToString();

                        CustomerStatus.Add(regobj);
                        ViewData["dtableCustomerStatus"] = dtableCustomerStatusByAC;
                    }
                }
                if ((SearchCol == "Name/Account") && (userInfo.Name != "") && (userInfo.WalletNumber != ""))
                {
                    DataTable dtableCustomerStatusByAC = CustomerUtils.GetUserProfileByAC(userInfo.WalletNumber);
                    if (dtableCustomerStatusByAC != null && dtableCustomerStatusByAC.Rows.Count > 0)
                    {
                        UserInfo regobj = new UserInfo();
                        regobj.ClientCode = dtableCustomerStatusByAC.Rows[0]["ClientCode"].ToString();
                        regobj.Name = dtableCustomerStatusByAC.Rows[0]["Name"].ToString();
                        regobj.Address = dtableCustomerStatusByAC.Rows[0]["Address"].ToString();
                        regobj.PIN = dtableCustomerStatusByAC.Rows[0]["PIN"].ToString();
                        regobj.Status = dtableCustomerStatusByAC.Rows[0]["Status"].ToString();
                        regobj.ContactNumber1 = dtableCustomerStatusByAC.Rows[0]["ContactNumber1"].ToString();
                        regobj.ContactNumber2 = dtableCustomerStatusByAC.Rows[0]["ContactNumber2"].ToString();
                        regobj.UserName = dtableCustomerStatusByAC.Rows[0]["UserName"].ToString();
                        regobj.UserType = dtableCustomerStatusByAC.Rows[0]["userType"].ToString();

                        CustomerStatus.Add(regobj);
                        ViewData["dtableCustomerStatus"] = dtableCustomerStatusByAC;
                    }
                }
                if ((userInfo.ContactNumber1 != "") && (SearchCol == "Mobile Number"))
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



                        regobj.UserBranchCode = dtableCustomerStatusByMobileNo.Rows[0]["UserBranchCode"].ToString();
                        regobj.ModifyingBranch = dtableCustomerStatusByMobileNo.Rows[0]["ModifyingBranch"].ToString();
                        regobj.IsModified = dtableCustomerStatusByMobileNo.Rows[0]["IsModified"].ToString();
                        regobj.IsRejected = dtableCustomerStatusByMobileNo.Rows[0]["IsRejected"].ToString();
                        regobj.IsApproved = dtableCustomerStatusByMobileNo.Rows[0]["IsApproved"].ToString();

                        string Message = "";
                        bool isActive = IsActive(regobj, out Message);
                        if (isActive)
                        {
                            CustomerStatus.Add(regobj);
                            ViewData["dtableCustomerStatus"] = dtableCustomerStatusByMobileNo;
                        }
                        else
                        {
                            ViewBag.Message = Message;
                        }
                    }
                }
                if ((userInfo.Name != "") && (SearchCol == "Full Name"))
                {
                    DataTable dtableCustomerStatusByName = CustomerUtils.GetUserProfileByName(userInfo.Name);
                    if (dtableCustomerStatusByName != null && dtableCustomerStatusByName.Rows.Count > 0)
                    {
                        UserInfo regobj = new UserInfo();
                        regobj.ClientCode = dtableCustomerStatusByName.Rows[0]["ClientCode"].ToString();
                        regobj.Name = dtableCustomerStatusByName.Rows[0]["Name"].ToString();
                        regobj.Address = dtableCustomerStatusByName.Rows[0]["Address"].ToString();
                        regobj.PIN = dtableCustomerStatusByName.Rows[0]["PIN"].ToString();
                        regobj.Status = dtableCustomerStatusByName.Rows[0]["Status"].ToString();
                        regobj.ContactNumber1 = dtableCustomerStatusByName.Rows[0]["ContactNumber1"].ToString();
                        regobj.ContactNumber2 = dtableCustomerStatusByName.Rows[0]["ContactNumber2"].ToString();
                        regobj.UserName = dtableCustomerStatusByName.Rows[0]["UserName"].ToString();
                        regobj.UserType = dtableCustomerStatusByName.Rows[0]["userType"].ToString();

                        CustomerStatus.Add(regobj);
                        ViewData["dtableCustomerStatus"] = dtableCustomerStatusByName;
                    }
                }
                if ((userInfo.WalletNumber != "") && (SearchCol == "Account Number"))
                {
                    DataTable dtableCustomerStatusByAC = CustomerUtils.GetUserProfileByAC(userInfo.WalletNumber);
                    if (dtableCustomerStatusByAC != null && dtableCustomerStatusByAC.Rows.Count > 0)
                    {
                        UserInfo regobj = new UserInfo();
                        regobj.ClientCode = dtableCustomerStatusByAC.Rows[0]["ClientCode"].ToString();
                        regobj.Name = dtableCustomerStatusByAC.Rows[0]["Name"].ToString();
                        regobj.Address = dtableCustomerStatusByAC.Rows[0]["Address"].ToString();
                        regobj.PIN = dtableCustomerStatusByAC.Rows[0]["PIN"].ToString();
                        regobj.Status = dtableCustomerStatusByAC.Rows[0]["Status"].ToString();
                        regobj.ContactNumber1 = dtableCustomerStatusByAC.Rows[0]["ContactNumber1"].ToString();
                        regobj.ContactNumber2 = dtableCustomerStatusByAC.Rows[0]["ContactNumber2"].ToString();
                        regobj.UserName = dtableCustomerStatusByAC.Rows[0]["UserName"].ToString();
                        regobj.UserType = dtableCustomerStatusByAC.Rows[0]["userType"].ToString();

                        CustomerStatus.Add(regobj);
                        ViewData["dtableCustomerStatus"] = dtableCustomerStatusByAC;
                    }
                }

                return View(CustomerStatus);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        // GET: Customer/ListView
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

                //FOR Bank
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


                List<MNCustProfile> ProfileList = new List<MNCustProfile>();
                List<SelectListItem> ItemProfile = new List<SelectListItem>();
                ProfileList = CusProfileUserModel.GetMNCustProfile().Where(x => x.ProfileStatus == 'A').ToList();

                foreach (MNCustProfile Cust in ProfileList)
                {
                    ItemProfile.Add(new SelectListItem
                    {
                        Text = Cust.ProfileCode,
                        Value = Cust.ProfileCode

                    });
                }

                UserInfo regobj = new UserInfo();
                DataSet DSet = ProfileUtils.GetUserProfileInfoDS(id);
                DateTime defaultdate = new DateTime(2006, 01, 01);
                string defaultdateBS = "2006-01-01";
                DataTable dtableUserInfo = DSet.Tables["dtUserInfo"];
                DataTable dTranstable = DSet.Tables["dtTransaction"];
                DataTable dtUserKYC = DSet.Tables["dtUserKYC"];
                DataTable dtUserKYCDoc = DSet.Tables["dtUserKYCDoc"];
                if (dtableUserInfo != null && dtableUserInfo.Rows.Count > 0)
                {
                    regobj.ClientCode = dtableUserInfo.Rows[0]["ClientCode"].ToString();
                    regobj.Name = dtableUserInfo.Rows[0]["Name"].ToString().Capitalize();
                    regobj.ClientStatus = dtableUserInfo.Rows[0]["ClientStatus"].ToString();
                    //start milayako 02
                    regobj.PStreet = dtableUserInfo.Rows[0]["PStreet"].ToString();
                    regobj.PProvince = dtableUserInfo.Rows[0]["PProvince"].ToString();
                    regobj.PDistrictID = dtableUserInfo.Rows[0]["PDistrictID"].ToString();
                    regobj.PMunicipalityVDC = dtableUserInfo.Rows[0]["PMunicipalityVDC"].ToString();
                    regobj.PHouseNo = dtableUserInfo.Rows[0]["PHouseNo"].ToString();
                    regobj.PWardNo = dtableUserInfo.Rows[0]["PWardNo"].ToString();

                    regobj.CStreet = dtableUserInfo.Rows[0]["CStreet"].ToString();
                    regobj.CProvince = dtableUserInfo.Rows[0]["CProvince"].ToString();
                    regobj.CDistrictID = dtableUserInfo.Rows[0]["CDistrictID"].ToString();
                    regobj.CMunicipalityVDC = dtableUserInfo.Rows[0]["CMunicipalityVDC"].ToString();
                    regobj.CHouseNo = dtableUserInfo.Rows[0]["CHouseNo"].ToString();
                    regobj.CWardNo = dtableUserInfo.Rows[0]["CWardNo"].ToString();
                    //NISCHAL
                    regobj.Gender = dtUserKYC.Rows[0]["Gender"].ToString();
                    regobj.Nationality = dtUserKYC.Rows[0]["Nationality"].ToString();
                    regobj.Occupation = dtUserKYC.Rows[0]["Occupation"].ToString();
                    regobj.MaritalStatus = dtUserKYC.Rows[0]["MaritalStatus"].ToString();
                    regobj.SpouseName = dtUserKYC.Rows[0]["SpouseName"].ToString();
                    regobj.FatherInlawName = dtUserKYC.Rows[0]["FatherInLaw"].ToString();
                    regobj.FatherName = dtUserKYC.Rows[0]["FathersName"].ToString();
                    regobj.MotherName = dtUserKYC.Rows[0]["MothersName"].ToString();
                    regobj.GrandFatherName = dtUserKYC.Rows[0]["GFathersName"].ToString();

                    if (dtUserKYC.Rows[0]["DateOfBirth"].ToString() == "" || dtUserKYC.Rows[0]["DateOfBirth"].ToString() == null)
                    {
                        regobj.DOB = null;
                    }
                    else
                    {
                        regobj.DOB = DateTime.Parse(dtUserKYC.Rows[0]["DateOfBirth"].ToString()).ToString("dd/MM/yyyy");
                    }


                    if (dtUserKYC.Rows[0]["BSDateOfBirth"].ToString() == "" || dtUserKYC.Rows[0]["BSDateOfBirth"].ToString() == null)
                    {
                        regobj.BSDateOfBirth = null;
                    }
                    else
                    {
                        regobj.BSDateOfBirth = dtUserKYC.Rows[0]["BSDateOfBirth"].ToString();
                    }


                    //regobj.DOB = DateTime.Parse(dtUserKYC.Rows[0]["DateOfBirth"].ToString()).ToString("dd MMM yyyy");
                    regobj.Country = dtUserKYC.Rows[0]["CountryCode"].ToString();
                    regobj.Document = dtUserKYCDoc.Rows[0]["DocType"].ToString();
                    regobj.Citizenship = dtUserKYC.Rows[0]["CitizenshipNo"].ToString();
                    regobj.CitizenshipPlaceOfIssue = dtUserKYC.Rows[0]["CitizenPlaceOfIssue"].ToString();
                    if (dtUserKYC.Rows[0]["CitizenIssueDate"].ToString() == "")
                    {
                        regobj.CitizenshipIssueDate = defaultdate.ToString();
                    }
                    else
                    {
                        regobj.CitizenshipIssueDate = DateTime.Parse(dtUserKYC.Rows[0]["CitizenIssueDate"].ToString()).ToString("dd/MM/yyyy");
                    }

                    if (dtUserKYC.Rows[0]["BSCitizenIssueDate"].ToString() == "")
                    {
                        regobj.BSCitizenshipIssueDate = defaultdateBS;
                    }
                    else
                    {
                        regobj.BSCitizenshipIssueDate = dtUserKYC.Rows[0]["BSCitizenIssueDate"].ToString();
                    }






                    regobj.License = dtUserKYC.Rows[0]["LicenseNo"].ToString();

                    if (dtUserKYC.Rows[0]["LicenseIssueDate"].ToString() == "")
                    {
                        regobj.LicenseIssueDate = defaultdate.ToString();
                    }
                    else
                    {
                        regobj.LicenseIssueDate = DateTime.Parse(dtUserKYC.Rows[0]["LicenseIssueDate"].ToString()).ToString("dd/MM/yyyy");
                    }


                    if (dtUserKYC.Rows[0]["BSLicenseIssueDate"].ToString() == "")
                    {
                        regobj.BSLicenseIssueDate = defaultdateBS;
                    }
                    else
                    {
                        regobj.BSLicenseIssueDate = dtUserKYC.Rows[0]["BSLicenseIssueDate"].ToString();
                    }

                    if (dtUserKYC.Rows[0]["LicenseExpiryDate"].ToString() == "")
                    {
                        regobj.LicenseExpireDate = defaultdate.ToString();
                    }
                    else
                    {
                        regobj.LicenseExpireDate = DateTime.Parse(dtUserKYC.Rows[0]["LicenseExpiryDate"].ToString()).ToString("dd/MM/yyyy");
                    }

                    if (dtUserKYC.Rows[0]["BSLicenseExpiryDate"].ToString() == "")
                    {
                        regobj.BSLicenseExpireDate = defaultdateBS;
                    }
                    else
                    {
                        regobj.BSLicenseExpireDate = dtUserKYC.Rows[0]["BSLicenseExpiryDate"].ToString();
                    }
                    regobj.LicensePlaceOfIssue = dtUserKYC.Rows[0]["LicensePlaceOfIssue"].ToString();







                    regobj.Passport = dtUserKYC.Rows[0]["PassportNo"].ToString();


                    if (dtUserKYC.Rows[0]["PassportIssueDate"].ToString() == "")
                    {
                        regobj.PassportIssueDate = defaultdate.ToString();
                    }
                    else
                    {
                        regobj.PassportIssueDate = DateTime.Parse(dtUserKYC.Rows[0]["PassportIssueDate"].ToString()).ToString("dd/MM/yyyy");
                    }

                    if (dtUserKYC.Rows[0]["PassportExpiryDate"].ToString() == "")
                    {
                        regobj.PassportExpireDate = defaultdate.ToString();
                    }
                    else
                    {
                        regobj.PassportExpireDate = DateTime.Parse(dtUserKYC.Rows[0]["PassportExpiryDate"].ToString()).ToString("dd/MM/yyyy");
                    }
                    regobj.PassportPlaceOfIssue = dtUserKYC.Rows[0]["PassportPlaceOfIssue"].ToString();






                    regobj.FrontImage = dtUserKYCDoc.Rows[0]["FrontImage"].ToString();
                    regobj.BackImage = dtUserKYCDoc.Rows[0]["BackImage"].ToString();
                    regobj.PassportImage = dtUserKYCDoc.Rows[0]["PassportImage"].ToString();
                    //End---NISCHAL---


                    //end milayako 02
                    //start delete garnu parne 02
                    regobj.Address = dtableUserInfo.Rows[0]["Address"].ToString();
                    //end delete garnu parne 02
                    regobj.Status = dtableUserInfo.Rows[0]["Status"].ToString();
                    regobj.ContactNumber1 = dtableUserInfo.Rows[0]["ContactNumber1"].ToString();
                    regobj.ContactNumber2 = dtableUserInfo.Rows[0]["ContactNumber2"].ToString();
                    regobj.UserName = dtableUserInfo.Rows[0]["UserName"].ToString();
                    regobj.UserType = dtableUserInfo.Rows[0]["userType"].ToString();
                    regobj.ProfileCode = dtableUserInfo.Rows[0]["ProfileCode"].ToString();
                    regobj.IsApproved = dtableUserInfo.Rows[0]["IsApproved"].ToString();
                    regobj.IsRejected = dtableUserInfo.Rows[0]["IsRejected"].ToString();
                    regobj.WalletNumber = dtableUserInfo.Rows[0]["WalletNumber"].ToString();
                    regobj.EmailAddress = dtableUserInfo.Rows[0]["EmailAddress"].ToString();
                    regobj.NewMobileNo = dtableUserInfo.Rows[0]["NewMobileNo"].ToString();
                    regobj.BankNo = dtableUserInfo.Rows[0]["BankNo"].ToString();
                    regobj.BranchCode = dtableUserInfo.Rows[0]["BranchCode"].ToString().Trim();
                    regobj.BankAccountNumber = dtableUserInfo.Rows[0]["BankAccountNumber"].ToString();
                    regobj.Remarks = dtableUserInfo.Rows[0]["Remarks"].ToString();
                    //regobj.Gender = dtableUserInfo.Rows[0]["Gender"].ToString();

                    //added//
                    regobj.StartDate = dtableUserInfo.Rows[0]["StartDate"].ToString();
                    regobj.EndDate = dtableUserInfo.Rows[0]["EndDate"].ToString();
                    regobj.Transaction = dtableUserInfo.Rows[0]["IndvTxn"].ToString();
                    regobj.DateRange = dtableUserInfo.Rows[0]["DateRange"].ToString();
                    regobj.TransactionLimit = dtableUserInfo.Rows[0]["TransactionLimit"].ToString();

                    regobj.LimitType = dtableUserInfo.Rows[0]["LimitType"].ToString();
                    regobj.TransactionCount = dtableUserInfo.Rows[0]["TransactionCount"].ToString();
                    regobj.TransactionLimitDaily = dtableUserInfo.Rows[0]["TransactionLimitDaily"].ToString();
                    regobj.TransactionLimitMonthly = dtableUserInfo.Rows[0]["TransactionLimitMonthly"].ToString();

                    //if (regobj.UserType.ToUpper() != "USER" || (regobj.IsModified != "F") || regobj.IsApproved != "Approve")
                    //{
                    //    this.TempData["custmodify_messsage"] = "User Not found";
                    //    this.TempData["message_class"] = CssSetting.FailedMessageClass;
                    //    return RedirectToAction("ListView");
                    //}


                }
                else
                {
                    this.TempData["custmodify_messsage"] = "User not found";
                    this.TempData["message_class"] = CssSetting.FailedMessageClass;
                    return RedirectToAction("ListView");

                }
                List<TransactionInfo> lst = new List<TransactionInfo>();
                if (dTranstable != null && dTranstable.Rows.Count > 0)
                {
                    foreach (DataRow row in dTranstable.Rows)
                    {
                        lst.Add(new TransactionInfo
                        {

                            AcNumber = row["AcNumber"].ToString(),
                            Alias = row["Alias"].ToString(),
                            AcOwner = row["AcOwner"].ToString(),
                            IsPrimary = row["IsPrimary"].ToString(),
                            AcType = row["AcType"].ToString(),
                            TxnEnabled = row["TxnEnabled"].ToString(),
                            TBranchCode = row["TBranchCode"].ToString()
                        });
                    }

                }
                regobj.Trans = lst;
                ViewBag.ProfileList = new SelectList(ItemProfile.OrderBy(x => x.Text), "Value", "Text", regobj.ProfileName);

                // start milayako 02
                ViewBag.PProvince = new SelectList(list, "Value", "Text", regobj.PProvince);
                ViewBag.CProvince = new SelectList(list, "Value", "Text", regobj.CProvince);


                //end milayako 02

                // start milayako 02
                ViewBag.PDistrictID = new SelectList(ProvinceToDistrict(regobj.PProvince), "Value", "Text", regobj.PDistrictID);
                ViewBag.CDistrictID = new SelectList(ProvinceToDistrict(regobj.CProvince), "Value", "Text", regobj.CDistrictID);
                // END milayako 02


                //ViewBag.AdminProfileName = new SelectList(itemProfile, "Value", "Text");

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
        public String ProvinceDistrictName(string id)
        {
            string districtstring = "select Name from MNDistrict where DistrictID ='" + id + "'";

            object dt1 = objdal.MyMethod(districtstring);

            return dt1.ToString();
        }

        public JsonResult GetDistrictModify(int id)
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
            return Json(new SelectList(list, "Value", "Text"));

        }

        //To get the Province name in text format
        public string getProvinceName(string id)
        {
            string provincestring = "select Name from MNProvince where ProvinceID='" + id + "'";


            DataTable dt = new DataTable();
            dt = objdal.MyMethod(provincestring);
            string name = string.Empty;
            foreach (DataRow row in dt.Rows)
            {
                name = row["Name"].ToString();


            }
            return name;


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
        [HttpPost]
        public async Task<ActionResult> Modification(string btnCommand, FormCollection collection, UserInfo model)
        {

            DateTime defaultdate = new DateTime(2006, 01, 01);
            string defaultdateBS = "2006-01-01";
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

                    string cClientCode = collection["ClientCode"].ToString();
                    var testobj = TempData["CustModification" + cClientCode] as UserInfo;

                    //start milayako 02
                    string PStreet = collection["PStreet"].ToString();
                    string PProvince = collection["PProvince"].ToString();
                    string PDistrictID = collection["PDistrictID"].ToString();
                    string PMunicipalityVDC = collection["PMunicipalityVDC"].ToString();
                    string PHouseNo = collection["PHouseNo"].ToString();
                    string PWardNo = collection["PWardNo"].ToString();

                    string CStreet = collection["CStreet"].ToString();
                    string CProvince = collection["CProvince"].ToString();
                    string CDistrictID = collection["CDistrictID"].ToString();
                    string CMunicipalityVDC = collection["CMunicipalityVDC"].ToString();
                    string CHouseNo = collection["CHouseNo"].ToString();
                    string CWardNo = collection["CWardNo"].ToString();
                    //end milayako 02                   


                    string cStatus = collection["Status"].ToString();
                    string cContactNumber1 = collection["ContactNumber1"].ToString();
                    string cContactNumber2 = collection["ContactNumber2"].ToString();
                    string cEmailAddress = collection["EmailAddress"].ToString();
                    //string cNewMobileNo = collection["NewMobileNo"].ToString();
                    //NISCHAL
                    string cGender = collection["txtGender"].ToString();

                    string cDOB = collection["DOB"].ToString();
                    string cBSDateOfBirth = collection["BSDateOfBirth"].ToString();

                    string cNationality = collection["txtNationality"].ToString();
                    string cCountry = collection["txtCountry"].ToString();
                    string cFatherName = collection["txtFatherName"].ToString();
                    string cMotherName = collection["txtMotherName"].ToString();
                    string cGrandFatherName = collection["txtGrandFatherName"].ToString();
                    string cOccupation = collection["txtOccupation"].ToString();
                    string cMaritalStatus = collection["txtMaritalStatus"].ToString();
                    string cSpouseName = collection["txtSpouseName"].ToString();
                    string cFatherInLawName = collection["txtFatherInLawsName"].ToString();

                    string cDocument = collection["txtDocument"].ToString();
                    string cCitizenship = collection["txtCitizenship"].ToString();
                    string cCitizenshipPlaceOfIssue = collection["txtCitizenshipPlaceOfIssue"].ToString();
                    string cCitizenshipIssueDate;
                    string cBSCitizenshipIssueDate;
                    if (collection["CitizenshipIssueDate"].ToString() == null)
                    {
                        cCitizenshipIssueDate = defaultdate.ToString();
                    }
                    else
                    {
                        cCitizenshipIssueDate = collection["CitizenshipIssueDate"].ToString();
                    }
                    if (collection["BSCitizenshipIssueDate"].ToString() == null)
                    {
                        cBSCitizenshipIssueDate = defaultdateBS;
                    }
                    else
                    {
                        cBSCitizenshipIssueDate = collection["BSCitizenshipIssueDate"].ToString();
                    }





                    string cLicense = collection["txtLicense"].ToString();
                    string cLicensePlaceOfIssue = collection["txtLicensePlaceOfIssue"].ToString();
                    string cLicenseIssueDate;
                    string cBSLicenseIssueDate;
                    if (collection["LicenseIssueDate"].ToString() == null)
                    {
                        cLicenseIssueDate = defaultdate.ToString();
                    }
                    else
                    {
                        cLicenseIssueDate = collection["LicenseIssueDate"].ToString();
                    }


                    if (collection["BSLicenseIssueDate"].ToString() == null)
                    {
                        cBSLicenseIssueDate = defaultdateBS;
                    }
                    else
                    {
                        cBSLicenseIssueDate = collection["BSLicenseIssueDate"].ToString();
                    }
                    string cLicenseExpireDate;
                    string cBSLicenseExpireDate;
                    if (collection["LicenseExpireDate"].ToString() == null)
                    {
                        cLicenseExpireDate = defaultdate.ToString();
                    }
                    else
                    {
                        cLicenseExpireDate = collection["LicenseExpireDate"].ToString();
                    }
                    if (collection["BSLicenseExpireDate"].ToString() == null)
                    {
                        cBSLicenseExpireDate = defaultdateBS;
                    }
                    else
                    {
                        cBSLicenseExpireDate = collection["BSLicenseExpireDate"].ToString();
                    }



                    string cPassport = collection["txtPassport"].ToString();
                    string cPassportPlaceOfIssue = collection["txtPassportPlaceOfIssue"].ToString();
                    string cPassportIssueDate;
                    if (collection["PassportIssueDate"].ToString() == null)
                    {
                        cPassportIssueDate = defaultdate.ToString();
                    }
                    else
                    {
                        cPassportIssueDate = collection["PassportIssueDate"].ToString();
                    }

                    string cPassportExpireDate;
                    if (collection["PassportExpireDate"].ToString() == null)
                    {
                        cPassportExpireDate = defaultdate.ToString();
                    }
                    else
                    {
                        cPassportExpireDate = collection["PassportExpireDate"].ToString();
                    }



                    string cClientStatus = collection["ClientStatus"].ToString();
                    string cBranchCode = collection["BranchCode"].ToString();
                    string cBankNo = collection["BankNo"].ToString();
                    string cBankAccountNumber = collection["BankAccountNumber"].ToString();
                    string cIsApproved = collection["IsApproved"].ToString();
                    string cProfileCode = collection["ProfileCode"].ToString();
                    string cUserType = collection["UserType"].ToString();
                    string cUserName = collection["UserName"].ToString();
                    string cName = collection["Name"].ToString();
                    string cIsRejected = "F";
                    if (collection.AllKeys.Contains("IsRejected"))
                    {
                        cIsRejected = collection["IsRejected"].ToString();
                    }
                    string cTransaction = "Dis";
                    if (collection.AllKeys.Contains("Transaction"))
                    {
                        cTransaction = collection["Transaction"].ToString();
                    }


                    string cDateRange = "", cStartDate = "", cEndDate = "", cTransactionLimit = string.Empty;

                    if (collection.AllKeys.Contains("DateRange"))
                    {
                        cDateRange = collection["DateRange"].ToString();
                    }
                    cStartDate = collection["StartDate"].ToString();
                    cEndDate = collection["EndDate"].ToString();
                    string cLimitType = "", cTransactionCount = "", cTransactionLimitMonthly = "", cTransactionLimitDaily = "";
                    if (collection.AllKeys.Contains("LimitType"))
                    {
                        cLimitType = collection["LimitType"].ToString();
                    }
                    cTransactionCount = collection["TransactionCount"].ToString();
                    cTransactionLimitMonthly = collection["TransactionLimitMonthly"].ToString();
                    cTransactionLimitDaily = collection["TransactionLimitDaily"].ToString();
                    cTransactionLimit = collection["TransactionLimit"].ToString();
                    if (cContactNumber1 != cUserName)
                    {
                        DataTable dtableMobileNo = RegisterUtils.GetCheckMobileNo(cUserName);

                        if (dtableMobileNo.Rows.Count != 0)
                        {

                            this.TempData["custmodify_messsage"] = "Already Registered Mobile Number";
                            this.TempData["message_class"] = CssSetting.FailedMessageClass;
                            return RedirectToAction("Modification", "Customer", new { id = cClientCode });
                        }
                    }

                    UserInfo userInfoModify = new UserInfo();
                    userInfoModify.ClientCode = cClientCode;
                    userInfoModify.Name = cName;
                    userInfoModify.ClientStatus = cClientStatus;

                    //start milayako 02
                    userInfoModify.PStreet = PStreet;
                    userInfoModify.PProvince = PProvince;
                    userInfoModify.PDistrictID = PDistrictID;
                    userInfoModify.PMunicipalityVDC = PMunicipalityVDC;
                    userInfoModify.PHouseNo = PHouseNo;
                    userInfoModify.PWardNo = PWardNo;

                    userInfoModify.CStreet = CStreet;
                    userInfoModify.CProvince = CProvince;
                    userInfoModify.CDistrictID = CDistrictID;
                    userInfoModify.CMunicipalityVDC = CMunicipalityVDC;
                    userInfoModify.CHouseNo = CHouseNo;
                    userInfoModify.CWardNo = CWardNo;
                    //NISCHAL
                    userInfoModify.Gender = cGender;
                    userInfoModify.DOB = cDOB;
                    userInfoModify.BSDateOfBirth = cBSDateOfBirth;
                    userInfoModify.Nationality = cNationality;
                    userInfoModify.Country = cCountry;
                    userInfoModify.FatherName = cFatherName;
                    userInfoModify.MotherName = cMotherName;
                    userInfoModify.GrandFatherName = cGrandFatherName;
                    userInfoModify.Occupation = cOccupation;
                    userInfoModify.MaritalStatus = cMaritalStatus;
                    userInfoModify.SpouseName = cSpouseName;
                    userInfoModify.FatherInlawName = cFatherInLawName;
                    userInfoModify.Document = cDocument;
                    userInfoModify.Citizenship = cCitizenship;
                    userInfoModify.CitizenshipPlaceOfIssue = cCitizenshipPlaceOfIssue;
                    userInfoModify.CitizenshipIssueDate = cCitizenshipIssueDate;
                    userInfoModify.BSCitizenshipIssueDate = cBSCitizenshipIssueDate;



                    userInfoModify.License = cLicense;
                    userInfoModify.LicensePlaceOfIssue = cLicensePlaceOfIssue;
                    userInfoModify.LicenseIssueDate = cLicenseIssueDate;
                    userInfoModify.BSLicenseIssueDate = cBSLicenseIssueDate;
                    userInfoModify.LicenseExpireDate = cLicenseExpireDate;
                    userInfoModify.BSLicenseExpireDate = cBSLicenseExpireDate;



                    userInfoModify.Passport = cPassport;
                    userInfoModify.PassportIssueDate = cPassportIssueDate;
                    userInfoModify.PassportExpireDate = cPassportExpireDate;
                    userInfoModify.PassportPlaceOfIssue = cPassportPlaceOfIssue;
                    //end milayako 02


                    //userInfoModify.Status = cStatus;
                    userInfoModify.ContactNumber1 = cUserName;
                    userInfoModify.ContactNumber2 = cContactNumber2;
                    userInfoModify.UserType = cUserType;
                    userInfoModify.UserName = cUserName;
                    userInfoModify.ProfileCode = cProfileCode;
                    //userInfoModify.IsApproved = cIsApproved;
                    //userInfoModify.IsRejected = cIsRejected;
                    userInfoModify.EmailAddress = cEmailAddress;
                    //userInfoModify.NewMobileNo = cNewMobileNo;

                    //Added//
                    userInfoModify.Transaction = cTransaction;
                    userInfoModify.DateRange = cDateRange;
                    userInfoModify.StartDate = cStartDate;
                    userInfoModify.EndDate = cEndDate;
                    userInfoModify.LimitType = cLimitType;
                    userInfoModify.TransactionLimit = cTransactionLimit;
                    userInfoModify.TransactionCount = cTransactionCount;
                    userInfoModify.TransactionLimitMonthly = cTransactionLimitMonthly;
                    userInfoModify.TransactionLimitDaily = cTransactionLimitDaily;
                    userInfoModify.BankNo = cBankNo;
                    if (cBankAccountNumber.Trim() != "")
                        userInfoModify.BranchCode = cBankAccountNumber.Substring(0, 3).Trim();
                    else
                        userInfoModify.BranchCode = "";
                    userInfoModify.BankAccountNumber = cBankAccountNumber;


                    userInfoModify.AdminBranch = (string)Session["UserBranch"];
                    userInfoModify.AdminUserName = (string)Session["UserName"];

                    string[] Accounts;
                    List<TransactionInfo> TransAccount = new List<TransactionInfo>();

                    if (!String.IsNullOrEmpty(collection["hidAccounts"].ToString()))
                    {
                        Accounts = collection["hidAccounts"].ToString().Split(',');
                        Accounts = Accounts.Where(x => !string.IsNullOrEmpty(x.Trim())).ToArray();
                        foreach (string Account in Accounts)
                        {
                            TransactionInfo txnfo = new TransactionInfo();
                            txnfo.AcNumber = collection[String.Format("txtAcNumber{0}", Account)].ToString().Trim();
                            txnfo.Alias = collection[String.Format("txtAllias{0}", Account)].ToString().Trim();
                            txnfo.AcOwner = collection[String.Format("txtAcOwner{0}", Account)].ToString().Trim();
                            txnfo.AcType = collection[String.Format("txtAcType{0}", Account)].ToString().Trim();
                            txnfo.TBranchCode = collection[String.Format("txtBranchCode{0}", Account)].ToString().Trim();
                            if (collection.AllKeys.Contains(String.Format("chkPrimary{0}", Account)))
                            {
                                txnfo.IsPrimary = collection[String.Format("chkPrimary{0}", Account)].ToString().Trim();
                            }
                            else
                            {
                                txnfo.IsPrimary = "F";
                            }
                            if (collection.AllKeys.Contains(String.Format("chkTxnEnable{0}", Account)))
                            {
                                txnfo.TxnEnabled = collection[String.Format("chkTxnEnable{0}", Account)].ToString().Trim();
                            }
                            else
                            {
                                txnfo.TxnEnabled = "F";
                            }
                            if (txnfo.AcNumber != "" || txnfo.AcNumber != string.Empty)
                                TransAccount.Add(txnfo);
                        }

                    }
                    //var PP = SaveAndReturnFileName(model.PassportPhoto, userInfoModify.UserName);
                    //var front = SaveAndReturnFileName(model.Front, userInfoModify.UserName);
                    //var back = SaveAndReturnFileName(model.Back, userInfoModify.UserName);

                    var PP = ReturnFileName(model.PassportPhoto, userInfoModify.UserName);
                    var front = ReturnFileName(model.Front, userInfoModify.UserName);
                    var back = ReturnFileName(model.Back, userInfoModify.UserName);

                    //userInfoModify.PassportImageName = string.Format(PP);
                    //userInfoModify.FrontImageName = string.Format(front);
                    //userInfoModify.BackImageName = string.Format(back);

                    if (PP != null)
                    {
                        userInfoModify.PassportImage = ParseCv(model.PassportPhoto);
                        userInfoModify.PassportImageName = string.Format(PP);
                    }
                    else
                    {
                        userInfoModify.PassportImageName = "";
                    }
                    if (front != null)
                    {
                        userInfoModify.FrontImage = ParseCv(model.Front);
                        userInfoModify.FrontImageName = string.Format(front);
                    }
                    else
                    {
                        userInfoModify.FrontImageName = "";
                    }
                    if (back != null)
                    {
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

                    if (sPP != null)
                    {
                        userInfoModify.PassportImageName = Session["PP"].ToString();
                    }
                    else
                    {
                        userInfoModify.BackImageName = "";
                    }
                    if (sFront != null)
                    {
                        userInfoModify.FrontImageName = Session["Front"].ToString();
                    }
                    else
                    {
                        userInfoModify.BackImageName = "";
                    }
                    if (sBack != null)
                    {
                        userInfoModify.BackImageName = Session["Back"].ToString();
                    }
                    else
                    {
                        userInfoModify.BackImageName = "";
                    }
                    //userInfoModify.PassportImageName = Session["PP"].ToString();
                    //userInfoModify.FrontImageName = Session["Front"].ToString();
                    //userInfoModify.BackImageName = Session["Back"].ToString();

                    MNUser cust = new MNUser();
                    MNClient mnclient = new MNClient();
                    MNClientKYC mNClientKYC = new MNClientKYC();
                    MNClientKYCDoc mNClientKYCDoc = new MNClientKYCDoc();
                    mnclient.ClientCode = userInfoModify.ClientCode;
                    mnclient.Name = userInfoModify.Name;

                    //start milayako 02
                    mnclient.PStreet = userInfoModify.PStreet;
                    mnclient.PProvince = userInfoModify.PProvince;
                    mnclient.PDistrictID = userInfoModify.PDistrictID;
                    mnclient.PMunicipalityVDC = userInfoModify.PMunicipalityVDC;
                    mnclient.PHouseNo = userInfoModify.PHouseNo;
                    mnclient.PWardNo = userInfoModify.PWardNo;

                    mnclient.CStreet = userInfoModify.CStreet;
                    mnclient.CProvince = userInfoModify.CProvince;
                    mnclient.CDistrictID = userInfoModify.CDistrictID;
                    mnclient.CMunicipalityVDC = userInfoModify.CMunicipalityVDC;
                    mnclient.CHouseNo = userInfoModify.CHouseNo;
                    mnclient.CWardNo = userInfoModify.CWardNo;
                    //end milayako 02
                    //NISCHAL
                    mNClientKYC.Gender = userInfoModify.Gender;
                    mNClientKYC.DateOfBirth = DateTime.Parse(userInfoModify.DOB).ToString("dd/MM/yyyy");
                    mNClientKYC.BSDateOfBirth = userInfoModify.BSDateOfBirth;

                    mNClientKYC.Nationality = userInfoModify.Nationality;
                    mNClientKYC.Country = userInfoModify.Country;
                    mNClientKYC.FathersName = userInfoModify.FatherName;
                    mNClientKYC.MothersName = userInfoModify.MotherName;
                    mNClientKYC.GFathersName = userInfoModify.GrandFatherName;
                    mNClientKYC.Occupation = userInfoModify.Occupation;
                    mNClientKYC.MaritalStatus = userInfoModify.MaritalStatus;
                    mNClientKYC.SpouseName = userInfoModify.SpouseName;
                    mNClientKYC.FatherInLaw = userInfoModify.FatherInlawName;
                    mNClientKYCDoc.DocType = userInfoModify.Document;


                    mNClientKYC.CitizenshipNo = userInfoModify.Citizenship;
                    mNClientKYC.CitizenPlaceOfIssue = userInfoModify.CitizenshipPlaceOfIssue;
                    mNClientKYC.CitizenIssueDate = DateTime.Parse(userInfoModify.CitizenshipIssueDate).ToString("dd/MM/yyyy");
                    mNClientKYC.BSCitizenIssueDate = userInfoModify.BSCitizenshipIssueDate;



                    mNClientKYC.LicenseNo = userInfoModify.License;
                    mNClientKYC.LicensePlaceOfIssue = userInfoModify.LicensePlaceOfIssue;
                    mNClientKYC.LicenseIssueDate = DateTime.Parse(userInfoModify.LicenseIssueDate).ToString("dd/MM/yyyy");
                    mNClientKYC.BSLicenseIssueDate = userInfoModify.BSLicenseIssueDate;
                    mNClientKYC.LicenseExpiryDate = DateTime.Parse(userInfoModify.LicenseExpireDate).ToString("dd/MM/yyyy");
                    mNClientKYC.BSLicenseExpiryDate = userInfoModify.BSLicenseExpireDate;



                    mNClientKYC.PassportNo = userInfoModify.Passport;
                    mNClientKYC.PassportPlaceOfIssue = userInfoModify.PassportPlaceOfIssue;
                    mNClientKYC.PassportIssueDate = DateTime.Parse(userInfoModify.PassportIssueDate).ToString("dd/MM/yyyy");
                    mNClientKYC.PassportExpiryDate = DateTime.Parse(userInfoModify.PassportExpireDate).ToString("dd/MM/yyyy");
                    mNClientKYCDoc.PassportImage = userInfoModify.PassportImageName;
                    mNClientKYCDoc.FrontImage = userInfoModify.FrontImageName;
                    mNClientKYCDoc.BackImage = userInfoModify.BackImageName;
                    //---NISCHAL---

                    mnclient.ProfileCode = userInfoModify.ProfileCode;

                    mnclient.IndvTxn = userInfoModify.Transaction;
                    mnclient.DateRange = userInfoModify.DateRange;
                    mnclient.StartDate = userInfoModify.StartDate;
                    mnclient.EndDate = userInfoModify.EndDate;
                    mnclient.TransactionLimit = userInfoModify.TransactionLimit;
                    mnclient.LimitType = userInfoModify.LimitType;
                    mnclient.TransactionLimitDaily = userInfoModify.TransactionLimitDaily;
                    mnclient.TransactionLimitMonthly = userInfoModify.TransactionLimitMonthly;
                    mnclient.TransactionCount = userInfoModify.TransactionCount;

                    MNClientContact mnclientcontact = new MNClientContact();
                    mnclientcontact.ContactNumber1 = userInfoModify.ContactNumber1;
                    mnclientcontact.ContactNumber2 = userInfoModify.ContactNumber2;
                    mnclientcontact.EmailAddress = userInfoModify.EmailAddress;

                    MNClientExt mnclientext = new MNClientExt();
                    mnclientext.UserName = userInfoModify.UserName;
                    mnclientext.userType = userInfoModify.UserType;

                    MNBankAccountMap mnbankacmap = new MNBankAccountMap();
                    mnbankacmap.BIN = userInfoModify.BankNo;
                    mnbankacmap.BranchCode = userInfoModify.BranchCode.Trim();
                    mnbankacmap.BankAccountNumber = userInfoModify.BankAccountNumber;

                    cust.MNClient = mnclient;
                    cust.MNClientContact = mnclientcontact;
                    cust.MNClientExt = mnclientext;
                    cust.MNBankAccountMap = mnbankacmap;
                    cust.MNClientKYC = mNClientKYC;
                    cust.MNClientKYCDoc = mNClientKYCDoc;

                    DataTable NewTransAcs = ExtraUtility.ToDataTable<TransactionInfo>(TransAccount.ToList());

                    MNUser oldCust = new MNUser();
                    UserInfo regobj = new UserInfo();
                    DataSet DSet = ProfileUtils.GetUserProfileInfoDS(cClientCode);
                    DataTable dtableUserInfo = DSet.Tables["dtUserInfo"];
                    DataTable dTranstable = DSet.Tables["dtTransaction"];
                    DataTable dtUserKYC = DSet.Tables["dtUserKYC"];
                    DataTable dtUserKYCDoc = DSet.Tables["dtUserKYCDoc"];
                    DataTable oldTransAcs = new DataTable();
                    if (dtableUserInfo != null && dtableUserInfo.Rows.Count > 0)
                    {
                        regobj.ClientCode = dtableUserInfo.Rows[0]["ClientCode"].ToString();
                        regobj.Name = dtableUserInfo.Rows[0]["Name"].ToString().Capitalize();
                        regobj.ClientStatus = dtableUserInfo.Rows[0]["ClientStatus"].ToString();


                        // start milayako 02
                        regobj.PStreet = dtableUserInfo.Rows[0]["PStreet"].ToString();
                        regobj.PProvince = dtableUserInfo.Rows[0]["PProvince"].ToString();
                        regobj.PDistrictID = dtableUserInfo.Rows[0]["PDistrictID"].ToString();
                        regobj.PMunicipalityVDC = dtableUserInfo.Rows[0]["PMunicipalityVDC"].ToString();
                        regobj.PHouseNo = dtableUserInfo.Rows[0]["PHouseNo"].ToString();
                        regobj.PWardNo = dtableUserInfo.Rows[0]["PWardNo"].ToString();

                        regobj.CStreet = dtableUserInfo.Rows[0]["CStreet"].ToString();
                        regobj.CProvince = dtableUserInfo.Rows[0]["CProvince"].ToString();
                        regobj.CDistrictID = dtableUserInfo.Rows[0]["CDistrictID"].ToString();
                        regobj.CMunicipalityVDC = dtableUserInfo.Rows[0]["CMunicipalityVDC"].ToString();
                        regobj.CHouseNo = dtableUserInfo.Rows[0]["CHouseNo"].ToString();
                        regobj.CWardNo = dtableUserInfo.Rows[0]["CWardNo"].ToString();
                        //end milayako 02
                        //NISCHAL

                        regobj.Gender = dtUserKYC.Rows[0]["Gender"].ToString();
                        regobj.Nationality = dtUserKYC.Rows[0]["Nationality"].ToString();
                        regobj.Occupation = dtUserKYC.Rows[0]["Occupation"].ToString();
                        regobj.MaritalStatus = dtUserKYC.Rows[0]["MaritalStatus"].ToString();
                        regobj.SpouseName = dtUserKYC.Rows[0]["SpouseName"].ToString();
                        regobj.FatherInlawName = dtUserKYC.Rows[0]["FatherInLaw"].ToString();
                        regobj.FatherName = dtUserKYC.Rows[0]["FathersName"].ToString();
                        regobj.MotherName = dtUserKYC.Rows[0]["MothersName"].ToString();
                        regobj.GrandFatherName = dtUserKYC.Rows[0]["GFathersName"].ToString();
                        regobj.DateOfBirth = DateTime.Parse(dtUserKYC.Rows[0]["DateOfBirth"].ToString()).ToString("dd/MM/yyyy");
                        regobj.BSDateOfBirth = dtUserKYC.Rows[0]["BSDateOfBirth"].ToString();

                        regobj.Country = dtUserKYC.Rows[0]["CountryCode"].ToString();
                        regobj.Document = dtUserKYCDoc.Rows[0]["DocType"].ToString();
                        regobj.Citizenship = dtUserKYC.Rows[0]["CitizenshipNo"].ToString();
                        regobj.CitizenshipPlaceOfIssue = dtUserKYC.Rows[0]["CitizenPlaceOfIssue"].ToString();

                        regobj.CitizenshipIssueDate = DateTime.Parse(dtUserKYC.Rows[0]["CitizenIssueDate"].ToString()).ToString("dd/MM/yyyy");
                        regobj.BSCitizenshipIssueDate = dtUserKYC.Rows[0]["BSCitizenIssueDate"].ToString();

                        regobj.License = dtUserKYC.Rows[0]["LicenseNo"].ToString();
                        regobj.LicenseIssueDate = DateTime.Parse(dtUserKYC.Rows[0]["LicenseIssueDate"].ToString()).ToString("dd/MM/yyyy");
                        regobj.BSLicenseIssueDate = dtUserKYC.Rows[0]["BSLicenseIssueDate"].ToString();
                        regobj.LicenseExpireDate = DateTime.Parse(dtUserKYC.Rows[0]["LicenseExpiryDate"].ToString()).ToString("dd/MM/yyyy");
                        regobj.BSLicenseExpireDate = dtUserKYC.Rows[0]["BSLicenseExpiryDate"].ToString();
                        regobj.LicensePlaceOfIssue = dtUserKYC.Rows[0]["LicensePlaceOfIssue"].ToString();



                        regobj.Passport = dtUserKYC.Rows[0]["PassportNo"].ToString();
                        regobj.PassportIssueDate = DateTime.Parse(dtUserKYC.Rows[0]["PassportIssueDate"].ToString()).ToString("dd/MM/yyyy");
                        regobj.PassportExpireDate = DateTime.Parse(dtUserKYC.Rows[0]["PassportExpiryDate"].ToString()).ToString("dd/MM/yyyy");

                        regobj.PassportPlaceOfIssue = dtUserKYC.Rows[0]["PassportPlaceOfIssue"].ToString();
                        regobj.PassportImage = dtUserKYCDoc.Rows[0]["PassportImage"].ToString();
                        regobj.FrontImage = dtUserKYCDoc.Rows[0]["FrontImage"].ToString();
                        regobj.BackImage = dtUserKYCDoc.Rows[0]["BackImage"].ToString();

                        //---NISCHAL---



                        regobj.Status = dtableUserInfo.Rows[0]["Status"].ToString();
                        regobj.ContactNumber1 = dtableUserInfo.Rows[0]["ContactNumber1"].ToString();
                        regobj.ContactNumber2 = dtableUserInfo.Rows[0]["ContactNumber2"].ToString();
                        regobj.UserName = dtableUserInfo.Rows[0]["UserName"].ToString();
                        regobj.UserType = dtableUserInfo.Rows[0]["userType"].ToString();
                        regobj.ProfileCode = dtableUserInfo.Rows[0]["ProfileCode"].ToString();
                        regobj.IsApproved = dtableUserInfo.Rows[0]["IsApproved"].ToString();
                        regobj.IsRejected = dtableUserInfo.Rows[0]["IsRejected"].ToString();
                        regobj.WalletNumber = dtableUserInfo.Rows[0]["WalletNumber"].ToString();
                        regobj.EmailAddress = dtableUserInfo.Rows[0]["EmailAddress"].ToString();
                        regobj.NewMobileNo = dtableUserInfo.Rows[0]["NewMobileNo"].ToString();

                        regobj.BankNo = dtableUserInfo.Rows[0]["BankNo"].ToString();
                        regobj.BranchCode = dtableUserInfo.Rows[0]["BranchCode"].ToString();
                        regobj.BankAccountNumber = dtableUserInfo.Rows[0]["BankAccountNumber"].ToString();
                        // regobj.COC = dtableUserInfo.Rows[0]["COC"].ToString();

                        regobj.StartDate = dtableUserInfo.Rows[0]["StartDate"].ToString();
                        regobj.EndDate = dtableUserInfo.Rows[0]["EndDate"].ToString();
                        regobj.Transaction = dtableUserInfo.Rows[0]["IndvTxn"].ToString();
                        regobj.DateRange = dtableUserInfo.Rows[0]["DateRange"].ToString();
                        regobj.TransactionLimit = dtableUserInfo.Rows[0]["TransactionLimit"].ToString();

                        regobj.LimitType = dtableUserInfo.Rows[0]["LimitType"].ToString();
                        regobj.TransactionCount = dtableUserInfo.Rows[0]["TransactionCount"].ToString();
                        regobj.TransactionLimitDaily = dtableUserInfo.Rows[0]["TransactionLimitDaily"].ToString();
                        regobj.TransactionLimitMonthly = dtableUserInfo.Rows[0]["TransactionLimitMonthly"].ToString();

                        List<TransactionInfo> lst = new List<TransactionInfo>();

                        if (dTranstable != null && dTranstable.Rows.Count > 0)
                        {
                            foreach (DataRow row in dTranstable.Rows)
                            {
                                lst.Add(new TransactionInfo
                                {

                                    AcNumber = row["AcNumber"].ToString().Trim(),
                                    Alias = row["Alias"].ToString().Trim(),
                                    AcOwner = row["AcOwner"].ToString().Trim(),
                                    IsPrimary = row["IsPrimary"].ToString().Trim(),
                                    AcType = row["AcType"].ToString().Trim(),
                                    TxnEnabled = row["TxnEnabled"].ToString().Trim(),
                                    TBranchCode = row["TBranchCode"].ToString().Trim()
                                });
                            }

                        }
                        regobj.Trans = lst;

                        MNClient omnclient = new MNClient();
                        MNClientKYC omnClientKYC = new MNClientKYC();
                        MNClientKYCDoc omnClientKYCDoc = new MNClientKYCDoc();
                        omnclient.ClientCode = regobj.ClientCode;
                        omnclient.Name = regobj.Name;

                        //start milayako 02
                        omnclient.PStreet = regobj.PStreet;
                        omnclient.PProvince = regobj.PProvince;
                        omnclient.PDistrictID = regobj.PDistrictID;
                        omnclient.PMunicipalityVDC = regobj.PMunicipalityVDC;
                        omnclient.PHouseNo = regobj.PHouseNo;
                        omnclient.PWardNo = regobj.PWardNo;

                        omnclient.CStreet = regobj.CStreet;
                        omnclient.CProvince = regobj.CProvince;
                        omnclient.CDistrictID = regobj.CDistrictID;
                        omnclient.CMunicipalityVDC = regobj.CMunicipalityVDC;
                        omnclient.CHouseNo = regobj.CHouseNo;
                        omnclient.CWardNo = regobj.CWardNo;


                        //NISCHAL
                        omnClientKYC.Gender = regobj.Gender;
                        omnClientKYC.DateOfBirth = regobj.DateOfBirth;
                        omnClientKYC.BSDateOfBirth = regobj.BSDateOfBirth;
                        omnClientKYC.Nationality = regobj.Nationality;
                        omnClientKYC.Country = regobj.Country;
                        omnClientKYC.FathersName = regobj.FatherName;
                        omnClientKYC.MothersName = regobj.MotherName;
                        omnClientKYC.GFathersName = regobj.GrandFatherName;
                        omnClientKYC.Occupation = regobj.Occupation;
                        omnClientKYC.MaritalStatus = regobj.MaritalStatus;
                        omnClientKYC.SpouseName = regobj.SpouseName;
                        omnClientKYC.FatherInLaw = regobj.FatherInlawName;
                        omnClientKYCDoc.DocType = regobj.Document;
                        omnClientKYC.CitizenshipNo = regobj.Citizenship;
                        omnClientKYC.CitizenPlaceOfIssue = regobj.CitizenshipPlaceOfIssue;
                        omnClientKYC.CitizenIssueDate = regobj.CitizenshipIssueDate;
                        omnClientKYC.BSCitizenIssueDate = regobj.BSCitizenshipIssueDate;
                        omnClientKYC.LicenseNo = regobj.License;
                        omnClientKYC.LicensePlaceOfIssue = regobj.LicensePlaceOfIssue;
                        omnClientKYC.LicenseIssueDate = regobj.LicenseIssueDate;
                        omnClientKYC.BSLicenseIssueDate = regobj.BSLicenseIssueDate;
                        omnClientKYC.LicenseExpiryDate = regobj.LicenseExpireDate;
                        omnClientKYC.BSLicenseExpiryDate = regobj.BSLicenseExpireDate;


                        omnClientKYC.PassportNo = regobj.Passport;
                        omnClientKYC.PassportPlaceOfIssue = regobj.PassportPlaceOfIssue;
                        omnClientKYC.PassportIssueDate = regobj.PassportIssueDate;
                        omnClientKYC.PassportExpiryDate = regobj.PassportExpireDate;

                        omnClientKYCDoc.PassportImage = regobj.PassportImage;
                        omnClientKYCDoc.FrontImage = regobj.FrontImage;
                        omnClientKYCDoc.BackImage = regobj.BackImage;
                        //---NISCHAL---
                        //end milayako 02


                        //omnclient.ClientStatus = regobj.ClientStatus;
                        // omnclient.Status = regobj.Status;
                        //omnclient.MobileNo = regobj.NewMobileNo;
                        omnclient.ProfileCode = regobj.ProfileCode;
                        //omnclient.IsApproved = regobj.ProfileCode;
                        //omnclient.IsRejected = regobj.ProfileCode;

                        omnclient.IndvTxn = regobj.Transaction;

                        omnclient.DateRange = regobj.DateRange;
                        omnclient.StartDate = regobj.StartDate;
                        omnclient.EndDate = regobj.EndDate;
                        omnclient.TransactionLimit = regobj.TransactionLimit;
                        omnclient.LimitType = regobj.LimitType;
                        omnclient.TransactionLimitDaily = regobj.TransactionLimitDaily;
                        omnclient.TransactionLimitMonthly = regobj.TransactionLimitMonthly;
                        omnclient.TransactionCount = regobj.TransactionCount;



                        MNClientContact omnclientcontact = new MNClientContact();
                        omnclientcontact.ContactNumber1 = regobj.ContactNumber1;
                        omnclientcontact.ContactNumber2 = regobj.ContactNumber2;
                        omnclientcontact.EmailAddress = regobj.EmailAddress;

                        MNClientExt omnclientext = new MNClientExt();
                        omnclientext.UserName = regobj.UserName;
                        omnclientext.userType = regobj.UserType;

                        MNBankAccountMap omnbankacmap = new MNBankAccountMap();
                        omnbankacmap.BIN = regobj.BankNo;
                        omnbankacmap.BranchCode = regobj.BranchCode.Trim();
                        omnbankacmap.BankAccountNumber = regobj.BankAccountNumber;

                        oldCust.MNClient = omnclient;
                        oldCust.MNClientContact = omnclientcontact;
                        oldCust.MNClientExt = omnclientext;
                        oldCust.MNBankAccountMap = omnbankacmap;
                        oldCust.MNClientKYC = omnClientKYC;
                        oldCust.MNClientKYCDoc = omnClientKYCDoc;

                        oldTransAcs = ExtraUtility.ToDataTable<TransactionInfo>(regobj.Trans.ToList());
                        // oldCust.MNTransactionAccounts = regobj.Trans.ToList();
                        if (mNClientKYCDoc.PassportImage == "")
                        {
                            mNClientKYCDoc.PassportImage = omnClientKYCDoc.PassportImage;
                        }
                        if (mNClientKYCDoc.FrontImage == "")
                        {
                            mNClientKYCDoc.FrontImage = omnClientKYCDoc.FrontImage;
                        }
                        if (mNClientKYCDoc.BackImage == "")
                        {
                            mNClientKYCDoc.BackImage = omnClientKYCDoc.BackImage;
                        }

                    }
                    bool isUpdated = false;
                    CompareLogic compareLogic = new CompareLogic();
                    ComparisonConfig config = new ComparisonConfig();
                    config.MaxDifferences = int.MaxValue;
                    config.IgnoreCollectionOrder = false;
                    compareLogic.Config = config;
                    ComparisonResult result = compareLogic.Compare(oldCust, cust); //  firstparameter orginal,second parameter modified
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
                            makerchecker.Module = "CUST";
                            makerCheckers.Add(makerchecker);
                        }
                    }

                    DataTable DataToDelete = new DataTable();
                    DataTable DataToAdd = new DataTable();

                    if (oldTransAcs.Rows.Count >= 0)
                    {
                        var ToDelete = oldTransAcs.AsEnumerable().Except(NewTransAcs.AsEnumerable(), DataRowComparer.Default);
                        var ToAdd = NewTransAcs.AsEnumerable().Except(oldTransAcs.AsEnumerable(), DataRowComparer.Default);
                        if (ToDelete.Count() > 0)
                        {
                            DataToDelete = ToDelete.CopyToDataTable();
                            isUpdated = true;
                        }
                        if (ToAdd.Count() > 0)
                        {
                            DataToAdd = ToAdd.CopyToDataTable();
                            isUpdated = true;
                        }
                    }

                    List<TransactionInfo> TxnAcToAdd = ExtraUtility.DatatableToListClass<TransactionInfo>(DataToAdd);
                    List<TransactionInfo> TxnAcToDelete = ExtraUtility.DatatableToListClass<TransactionInfo>(DataToDelete);
                    CusProfileUtils cUtil = new CusProfileUtils();

                    string ModifiedFieldXML = cUtil.GetMakerCheckerXMLStr(makerCheckers);
                    string TxnAcToAddXML = cUtil.GetServiceAccountXMLStr(TxnAcToAdd);
                    string TxnAcToDeleteXML = cUtil.GetServiceAccountXMLStr(TxnAcToDelete);

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


                        bool inserted = CustomerUtils.InsertMakerChecker(cClientCode, modifyingAdmin, modifyingBranch, ModifiedFieldXML, TxnAcToDeleteXML, TxnAcToAddXML);

                        displayMessage = inserted
                                                 ? "Changes will take effect after approval in Approve Modified"
                                                 : "Error updating customer, Please try again";
                        messageClass = inserted ? CssSetting.SuccessMessageClass : CssSetting.FailedMessageClass;
                    }
                    else
                    {
                        displayMessage = "Nothing Changed";
                        messageClass = CssSetting.SuccessMessageClass;
                    }
                    this.TempData["custmodify_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;
                    return RedirectToAction("ListView");
                }
                return View();

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpGet]
        public ActionResult GetFeatureDetail(string Profile)
        {
            CusProfileViewModel model = new CusProfileViewModel();
            model = CusProfileUserModel.GetCustProfileDetails(Profile, false);
            if (model == null)
                return View(new CusProfileViewModel());
            else
            {

                return View(model);
            }

        }




        #endregion


        #region "Customer Status"


        // GET: Customer/CustomerStatus
        [HttpGet]
        public ActionResult CustomerStatus()
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


        [HttpGet]
        public ActionResult CustomerDetail(string SearchCol, string txtMobileNo, string txtName, string txtAccountNo)
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
                userInfo.ContactNumber1 = txtMobileNo; //= collection["txtMobileNo"].ToString();
                userInfo.Name = txtName;// = collection["txtName"].ToString();
                userInfo.WalletNumber = txtAccountNo;// = collection["txtAccountNo"].ToString();

                List<UserInfo> CustomerStatus = new List<UserInfo>();

                if ((SearchCol == "All") || (userInfo.Name == null) && (userInfo.WalletNumber == null) && (userInfo.ContactNumber1 == null))
                {
                    DataTable dtableCustomerStatusByALL = CustomerUtils.GetUserProfileByALLA(userInfo.Name, userInfo.WalletNumber, userInfo.ContactNumber1);
                    if (dtableCustomerStatusByALL != null && dtableCustomerStatusByALL.Rows.Count > 0)
                    {
                        UserInfo regobj = new UserInfo();
                        regobj.ClientCode = dtableCustomerStatusByALL.Rows[0]["ClientCode"].ToString();
                        regobj.Name = dtableCustomerStatusByALL.Rows[0]["Name"].ToString();
                        regobj.Address = dtableCustomerStatusByALL.Rows[0]["Address"].ToString();
                        regobj.PIN = dtableCustomerStatusByALL.Rows[0]["PIN"].ToString();
                        regobj.Status = dtableCustomerStatusByALL.Rows[0]["Status"].ToString();
                        regobj.ContactNumber1 = dtableCustomerStatusByALL.Rows[0]["ContactNumber1"].ToString();
                        regobj.ContactNumber2 = dtableCustomerStatusByALL.Rows[0]["ContactNumber2"].ToString();
                        regobj.UserName = dtableCustomerStatusByALL.Rows[0]["UserName"].ToString();
                        regobj.UserType = dtableCustomerStatusByALL.Rows[0]["userType"].ToString();

                        CustomerStatus.Add(regobj);
                        ViewData["dtableCustomerStatus"] = dtableCustomerStatusByALL;
                    }
                }
                if ((SearchCol == "Name/Account") && (userInfo.Name != "") && (userInfo.WalletNumber != ""))
                {
                    DataTable dtableCustomerStatusByAC = CustomerUtils.GetUserProfileByNameACA(userInfo.Name, userInfo.WalletNumber);
                    if (dtableCustomerStatusByAC != null && dtableCustomerStatusByAC.Rows.Count > 0)
                    {
                        UserInfo regobj = new UserInfo();
                        regobj.ClientCode = dtableCustomerStatusByAC.Rows[0]["ClientCode"].ToString();
                        regobj.Name = dtableCustomerStatusByAC.Rows[0]["Name"].ToString();
                        regobj.Address = dtableCustomerStatusByAC.Rows[0]["Address"].ToString();
                        regobj.PIN = dtableCustomerStatusByAC.Rows[0]["PIN"].ToString();
                        regobj.Status = dtableCustomerStatusByAC.Rows[0]["Status"].ToString();
                        regobj.ContactNumber1 = dtableCustomerStatusByAC.Rows[0]["ContactNumber1"].ToString();
                        regobj.ContactNumber2 = dtableCustomerStatusByAC.Rows[0]["ContactNumber2"].ToString();
                        regobj.UserName = dtableCustomerStatusByAC.Rows[0]["UserName"].ToString();
                        regobj.UserType = dtableCustomerStatusByAC.Rows[0]["userType"].ToString();

                        CustomerStatus.Add(regobj);
                        ViewData["dtableCustomerStatus"] = dtableCustomerStatusByAC;
                    }
                }
                if ((SearchCol == "Mobile Number") && (userInfo.ContactNumber1 != ""))
                {
                    DataTable dtableCustomerStatusByMobileNo = CustomerUtils.GetUserProfileByMobileNoA(userInfo.ContactNumber1);
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

                        CustomerStatus.Add(regobj);
                        ViewData["dtableCustomerStatus"] = dtableCustomerStatusByMobileNo;
                    }
                }
                if ((SearchCol == "Full Name") && (userInfo.Name != ""))
                {
                    DataTable dtableCustomerStatusByName = CustomerUtils.GetUserProfileByNameA(userInfo.Name);
                    if (dtableCustomerStatusByName != null && dtableCustomerStatusByName.Rows.Count > 0)
                    {
                        UserInfo regobj = new UserInfo();
                        regobj.ClientCode = dtableCustomerStatusByName.Rows[0]["ClientCode"].ToString();
                        regobj.Name = dtableCustomerStatusByName.Rows[0]["Name"].ToString();
                        regobj.Address = dtableCustomerStatusByName.Rows[0]["Address"].ToString();
                        regobj.PIN = dtableCustomerStatusByName.Rows[0]["PIN"].ToString();
                        regobj.Status = dtableCustomerStatusByName.Rows[0]["Status"].ToString();
                        regobj.ContactNumber1 = dtableCustomerStatusByName.Rows[0]["ContactNumber1"].ToString();
                        regobj.ContactNumber2 = dtableCustomerStatusByName.Rows[0]["ContactNumber2"].ToString();
                        regobj.UserName = dtableCustomerStatusByName.Rows[0]["UserName"].ToString();
                        regobj.UserType = dtableCustomerStatusByName.Rows[0]["userType"].ToString();

                        CustomerStatus.Add(regobj);
                        ViewData["dtableCustomerStatus"] = dtableCustomerStatusByName;
                    }
                }
                if ((SearchCol == "Account Number") && (userInfo.WalletNumber != ""))
                {
                    DataTable dtableCustomerStatusByAC = CustomerUtils.GetUserProfileByACA(userInfo.WalletNumber);
                    if (dtableCustomerStatusByAC != null && dtableCustomerStatusByAC.Rows.Count > 0)
                    {
                        UserInfo regobj = new UserInfo();
                        regobj.ClientCode = dtableCustomerStatusByAC.Rows[0]["ClientCode"].ToString();
                        regobj.Name = dtableCustomerStatusByAC.Rows[0]["Name"].ToString();
                        regobj.Address = dtableCustomerStatusByAC.Rows[0]["Address"].ToString();
                        regobj.PIN = dtableCustomerStatusByAC.Rows[0]["PIN"].ToString();
                        regobj.Status = dtableCustomerStatusByAC.Rows[0]["Status"].ToString();
                        regobj.ContactNumber1 = dtableCustomerStatusByAC.Rows[0]["ContactNumber1"].ToString();
                        regobj.ContactNumber2 = dtableCustomerStatusByAC.Rows[0]["ContactNumber2"].ToString();
                        regobj.UserName = dtableCustomerStatusByAC.Rows[0]["UserName"].ToString();
                        regobj.UserType = dtableCustomerStatusByAC.Rows[0]["userType"].ToString();

                        CustomerStatus.Add(regobj);
                        ViewData["dtableCustomerStatus"] = dtableCustomerStatusByAC;
                    }
                }

                return View(CustomerStatus);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        #endregion


        #region "OverView Registration"

        public ActionResult ViewRegistration(string clientCodeID)
        {
            DateTime defaultdate = new DateTime(2006, 01, 01);
            string defaultdateBS = "2006-01-01";
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

                DataSet dsBank = BankUtils.GetDataSetPopulateBankCode();


                UserInfo regobj = new UserInfo();
                DataSet DSet = ProfileUtils.GetUserProfileInfoDS(clientCodeID);
                DataTable dtableUserInfo = DSet.Tables["dtUserInfo"];
                DataTable dtUserKYC = DSet.Tables["dtUserKYC"];
                DataTable dtUserKYCDoc = DSet.Tables["dtUserKYCDoc"];
                DataTable dTranstable = DSet.Tables["dtTransaction"];

                if (dtableUserInfo != null && dtableUserInfo.Rows.Count > 0)
                {
                    regobj.ClientCode = dtableUserInfo.Rows[0]["ClientCode"].ToString();
                    regobj.Name = dtableUserInfo.Rows[0]["Name"].ToString();

                    regobj.ClientStatus = dtableUserInfo.Rows[0]["ClientStatus"].ToString();

                    //start milayako 02
                    regobj.PStreet = dtableUserInfo.Rows[0]["PStreet"].ToString();
                    regobj.PProvince = dtableUserInfo.Rows[0]["PProvince"].ToString();
                    regobj.PDistrictID = getDistrictName(dtableUserInfo.Rows[0]["PDistrictID"].ToString()); //get district name from district id

                    regobj.PMunicipalityVDC = dtableUserInfo.Rows[0]["PMunicipalityVDC"].ToString();
                    regobj.PHouseNo = dtableUserInfo.Rows[0]["PHouseNo"].ToString();
                    regobj.PWardNo = dtableUserInfo.Rows[0]["PWardNo"].ToString();

                    regobj.CStreet = dtableUserInfo.Rows[0]["CStreet"].ToString();
                    regobj.CProvince = dtableUserInfo.Rows[0]["CProvince"].ToString();
                    regobj.CDistrictID = getDistrictName(dtableUserInfo.Rows[0]["CDistrictID"].ToString()); //get district name from district id

                    regobj.CMunicipalityVDC = dtableUserInfo.Rows[0]["CMunicipalityVDC"].ToString();
                    regobj.CHouseNo = dtableUserInfo.Rows[0]["CHouseNo"].ToString();
                    regobj.CWardNo = dtableUserInfo.Rows[0]["CWardNo"].ToString();
                    regobj.CustStatus = dtableUserInfo.Rows[0]["CWardNo"].ToString();



                    //NISCHAL
                    regobj.Gender = dtUserKYC.Rows[0]["Gender"].ToString();
                    regobj.Nationality = dtUserKYC.Rows[0]["Nationality"].ToString();
                    if (dtUserKYC.Rows[0]["DateOfBirth"].ToString() == "" || dtUserKYC.Rows[0]["DateOfBirth"].ToString() == null)
                    {
                        regobj.DOB = null;
                    }
                    else
                    {
                        regobj.DOB = DateTime.Parse(dtUserKYC.Rows[0]["DateOfBirth"].ToString()).ToString("dd/MM/yyyy");
                    }


                    if (dtUserKYC.Rows[0]["BSDateOfBirth"].ToString() == "" || dtUserKYC.Rows[0]["BSDateOfBirth"].ToString() == null)
                    {
                        regobj.BSDateOfBirth = null;
                    }
                    else
                    {
                        regobj.BSDateOfBirth = dtUserKYC.Rows[0]["BSDateOfBirth"].ToString();
                    }
                    //regobj.DOB = DateTime.Parse(dtUserKYC.Rows[0]["DateOfBirth"].ToString()).ToString("dd MMM yyyy");
                    regobj.Country = dtUserKYC.Rows[0]["CountryCode"].ToString();
                    regobj.FatherName = dtUserKYC.Rows[0]["FathersName"].ToString();
                    regobj.MotherName = dtUserKYC.Rows[0]["MothersName"].ToString();
                    regobj.GrandFatherName = dtUserKYC.Rows[0]["GFathersName"].ToString();
                    regobj.Occupation = dtUserKYC.Rows[0]["Occupation"].ToString();
                    regobj.MaritalStatus = dtUserKYC.Rows[0]["MaritalStatus"].ToString();
                    regobj.SpouseName = dtUserKYC.Rows[0]["SpouseName"].ToString();
                    regobj.FatherInlawName = dtUserKYC.Rows[0]["FatherInLaw"].ToString();
                    regobj.Document = dtUserKYCDoc.Rows[0]["DocType"].ToString();
                    regobj.Citizenship = dtUserKYC.Rows[0]["CitizenshipNo"].ToString();
                    if (dtUserKYC.Rows[0]["CitizenIssueDate"].ToString() == "")
                    {
                        regobj.CitizenshipIssueDate = defaultdate.ToString();
                    }
                    else
                    {
                        regobj.CitizenshipIssueDate = DateTime.Parse(dtUserKYC.Rows[0]["CitizenIssueDate"].ToString()).ToString("dd/MM/yyyy");
                    }


                    if (dtUserKYC.Rows[0]["BSCitizenIssueDate"].ToString() == "")
                    {
                        regobj.BSCitizenshipIssueDate = defaultdateBS;
                    }
                    else
                    {
                        regobj.BSCitizenshipIssueDate = dtUserKYC.Rows[0]["BSCitizenIssueDate"].ToString();
                    }

                    regobj.License = dtUserKYC.Rows[0]["LicenseNo"].ToString();

                    if (dtUserKYC.Rows[0]["LicenseIssueDate"].ToString() == "")
                    {
                        regobj.LicenseIssueDate = defaultdate.ToString();
                    }
                    else
                    {
                        regobj.LicenseIssueDate = DateTime.Parse(dtUserKYC.Rows[0]["LicenseIssueDate"].ToString()).ToString("dd/MM/yyyy");
                    }


                    if (dtUserKYC.Rows[0]["BSLicenseIssueDate"].ToString() == "")
                    {
                        regobj.BSLicenseIssueDate = defaultdateBS;
                    }
                    else
                    {
                        regobj.BSLicenseIssueDate = dtUserKYC.Rows[0]["BSLicenseIssueDate"].ToString();
                    }

                    if (dtUserKYC.Rows[0]["LicenseExpiryDate"].ToString() == "")
                    {
                        regobj.LicenseExpireDate = defaultdate.ToString();
                    }
                    else
                    {
                        regobj.LicenseExpireDate = DateTime.Parse(dtUserKYC.Rows[0]["LicenseExpiryDate"].ToString()).ToString("dd/MM/yyyy");
                    }


                    if (dtUserKYC.Rows[0]["BSLicenseExpiryDate"].ToString() == "")
                    {
                        regobj.BSLicenseExpireDate = defaultdateBS;
                    }
                    else
                    {
                        regobj.BSLicenseExpireDate = dtUserKYC.Rows[0]["BSLicenseExpiryDate"].ToString();
                    }

                    if (dtUserKYC.Rows[0]["PassportIssueDate"].ToString() == "")
                    {
                        regobj.PassportIssueDate = defaultdate.ToString();
                    }
                    else
                    {
                        regobj.PassportIssueDate = DateTime.Parse(dtUserKYC.Rows[0]["PassportIssueDate"].ToString()).ToString("dd/MM/yyyy");
                    }

                    if (dtUserKYC.Rows[0]["PassportExpiryDate"].ToString() == "")
                    {
                        regobj.PassportExpireDate = defaultdate.ToString();
                    }
                    else
                    {
                        regobj.PassportExpireDate = DateTime.Parse(dtUserKYC.Rows[0]["PassportExpiryDate"].ToString()).ToString("dd/MM/yyyy");
                    }

                    regobj.CitizenshipPlaceOfIssue = dtUserKYC.Rows[0]["CitizenPlaceOfIssue"].ToString();
                    regobj.Passport = dtUserKYC.Rows[0]["PassportNo"].ToString();
                    regobj.PassportPlaceOfIssue = dtUserKYC.Rows[0]["PassportPlaceOfIssue"].ToString();            
                    regobj.License = dtUserKYC.Rows[0]["LicenseNo"].ToString();
                    regobj.LicensePlaceOfIssue = dtUserKYC.Rows[0]["LicensePlaceOfIssue"].ToString();                    
                    regobj.PassportImage = dtUserKYCDoc.Rows[0]["PassportImage"].ToString();
                    regobj.FrontImage = dtUserKYCDoc.Rows[0]["FrontImage"].ToString();
                    regobj.BackImage = dtUserKYCDoc.Rows[0]["BackImage"].ToString();
                    //NISCHAL---END

                    //end milayako 02
                    //start delete garnu parne 02

                    regobj.Address = dtableUserInfo.Rows[0]["Address"].ToString();
                    //end delete garnu parne 02

                    regobj.PIN = dtableUserInfo.Rows[0]["PIN"].ToString();
                    regobj.Status = dtableUserInfo.Rows[0]["Status"].ToString();
                    regobj.ContactNumber1 = dtableUserInfo.Rows[0]["ContactNumber1"].ToString();
                    regobj.ContactNumber2 = dtableUserInfo.Rows[0]["ContactNumber2"].ToString();
                    regobj.UserName = dtableUserInfo.Rows[0]["UserName"].ToString();
                    regobj.UserType = dtableUserInfo.Rows[0]["userType"].ToString();
                    //regobj.ProfileName = dtableUserInfo.Rows[0]["ProfileName"].ToString();
                    regobj.ProfileCode = dtableUserInfo.Rows[0]["ProfileCode"].ToString();
                    regobj.IsApproved = dtableUserInfo.Rows[0]["IsApproved"].ToString();
                    regobj.IsRejected = dtableUserInfo.Rows[0]["IsRejected"].ToString();
                    regobj.WalletNumber = dtableUserInfo.Rows[0]["WalletNumber"].ToString();
                    regobj.EmailAddress = dtableUserInfo.Rows[0]["EmailAddress"].ToString();
                    regobj.NewMobileNo = dtableUserInfo.Rows[0]["NewMobileNo"].ToString();

                    //regobj.BankNo = 
                    regobj.BranchCode = dtableUserInfo.Rows[0]["BranchCode"].ToString();
                    regobj.BankAccountNumber = dtableUserInfo.Rows[0]["BankAccountNumber"].ToString();
                    regobj.COC = dtableUserInfo.Rows[0]["COC"].ToString();


                    //edit

                    regobj.CreatedDate = dtableUserInfo.Rows[0]["CreatedDate"].ToString();
                    regobj.UserBranchCode = dtableUserInfo.Rows[0]["UserBranchCode"].ToString();
                    regobj.IsModified = dtableUserInfo.Rows[0]["IsModified"].ToString();
                    regobj.CreatedBy = dtableUserInfo.Rows[0]["CreatedBy"].ToString();
                    regobj.ApprovedBy = dtableUserInfo.Rows[0]["ApprovedBy"].ToString();
                    regobj.ModifyingBranch = dtableUserInfo.Rows[0]["ModifyingBranch"].ToString();
                    regobj.ModifyingAdmin = dtableUserInfo.Rows[0]["ModifiedBy"].ToString();
                    regobj.RejectedBy = dtableUserInfo.Rows[0]["RejectedBy"].ToString();

                    /////
                    regobj.Transaction = dtableUserInfo.Rows[0]["IndvTxn"].ToString();
                    regobj.DateRange = dtableUserInfo.Rows[0]["DateRange"].ToString();
                    regobj.StartDate = dtableUserInfo.Rows[0]["StartDate"].ToString();
                    regobj.EndDate = dtableUserInfo.Rows[0]["EndDate"].ToString();
                    regobj.TransactionLimit = dtableUserInfo.Rows[0]["TransactionLimit"].ToString();


                    regobj.LimitType = dtableUserInfo.Rows[0]["LimitType"].ToString();
                    regobj.TransactionCount = dtableUserInfo.Rows[0]["TransactionCount"].ToString();
                    regobj.TransactionLimitDaily = dtableUserInfo.Rows[0]["TransactionLimitDaily"].ToString();
                    regobj.TransactionLimitMonthly = dtableUserInfo.Rows[0]["TransactionLimitMonthly"].ToString();
                    regobj.TransactionLimit = dtableUserInfo.Rows[0]["TransactionLimit"].ToString();
                    regobj.SelfRegistered = dtableUserInfo.Rows[0]["SelfRegistered"].ToString();
                    ////

                }
                DataSet dsBranch = BranchUtils.GetDataSetPopulateBranchName();
                DataTable bank = dsBranch.Tables[0];
                var BranchDictnary = bank.AsEnumerable().ToDictionary<DataRow, string, string>(row => row.Field<string>(0),
                                                                                row => row.Field<string>(1));
                BranchDictnary.Add("", "");

                /*Testimg for customer status*/
                string Message = string.Empty;
                //if (regobj.IsModified == "T")
                //{
                //    regobj.Status = "Modified";
                //}
                //if (regobj.IsRejected == "T")
                //{
                //    regobj.Status = "Rejected";
                //}


                if (regobj.Status == "Active" && regobj.IsApproved == "Approve")
                {
                    Message = "Active";
                }
                else if (regobj.Status == "Active" && regobj.IsApproved == "UnApprove")
                {
                    if (regobj.IsRejected == "T")
                    {
                        Message = "Rejected in :" + BranchDictnary[regobj.UserBranchCode];
                    }
                    else
                    {
                        Message = "Waiting for approval: " + BranchDictnary[regobj.UserBranchCode];
                    }
                }
                else if (regobj.Status == "Blocked" && regobj.IsApproved == "Approve")
                {
                    Message = "Blocked";
                }
                else if (regobj.Status == "Active" && regobj.IsApproved == "UnApprove" && regobj.IsModified == "T")
                {
                    if (regobj.IsRejected == "T")
                    {
                        Message = "Rejected in :" + BranchDictnary[regobj.ModifyingBranch];
                    }
                    else
                    {
                        Message = "Pending approval in :" + BranchDictnary[regobj.ModifyingBranch];
                    }
                }
                else if (regobj.Status == "Active" && regobj.IsApproved == "UnApprove" && regobj.IsModified != "T" && regobj.IsRejected == "T")
                {
                    Message = "Rejected in :" + BranchDictnary[regobj.UserBranchCode];
                }
                else if (regobj.Status == "PASR" && regobj.IsApproved == "UnApprove")
                {
                    Message = "Customer is pending password reset approval in :" + BranchDictnary[regobj.ModifyingBranch];
                }
                else if (regobj.Status == "PINR" && regobj.IsApproved == "UnApprove")
                {
                    Message = " Pin reset in progress : " + BranchDictnary[regobj.ModifyingBranch];
                }
                else if (regobj.Status == "PPR" && regobj.IsApproved == "UnApprove")
                {
                    Message = "Pending PIN/password reset approval in :" + BranchDictnary[regobj.ModifyingBranch];
                }
                else if (regobj.Status == "Blocked" && regobj.IsApproved == "Blocked")
                {
                    Message = "Customer block in progress: " + BranchDictnary[regobj.ModifyingBranch];
                }
                else if (regobj.Status == "Active" && regobj.IsApproved == "Blocked")
                {
                    //customer unblock in progress: branch_name
                    Message = "Customer unblock in progress: " + BranchDictnary[regobj.ModifyingBranch];
                }

                ViewBag.StatusMessage = Message;

                /*end Testing*/





                DataRow[] rows = dsBank.Tables[0].Select(string.Format("BankCode ='{0}'", dtableUserInfo.Rows[0]["BankNo"].ToString()));
                if (rows.Length > 0)
                    regobj.BankNo = rows[0]["BankName"].ToString();
                else
                    regobj.BankNo = "";

                List<TransactionInfo> lst = new List<TransactionInfo>();

                if (dTranstable != null && dTranstable.Rows.Count > 0)
                {
                    foreach (DataRow row in dTranstable.Rows)
                    {
                        lst.Add(new TransactionInfo
                        {

                            AcNumber = row["AcNumber"].ToString(),
                            Alias = row["Alias"].ToString(),
                            AcOwner = row["AcOwner"].ToString(),
                            IsPrimary = row["IsPrimary"].ToString(),
                            AcType = row["AcType"].ToString(),
                            TxnEnabled = row["TxnEnabled"].ToString(),
                            TBranchCode = row["TBranchCode"].ToString()
                        });
                    }

                }
                regobj.Trans = lst;
                return View(regobj);



            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

            //return View();
        }

        #endregion


        #region "Customer RejectedList"


        // GET: Customer/RejectedList
        public ActionResult RejectedList()
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


                ApproveRJViewModel dtObj = new ApproveRJViewModel();

                /// <summary>
                /// UNAPPROVE REJECTED LIST OF CUSTOMER
                /// </summary>
                /// 
                DataTable UnapproveRejected = new DataTable();
                // dtObj.UNApproveRejectTable = ProfileUtils.GetUnApproveRJCustomerProfile(userType);
                string BranchCode = (string)Session["UserBranch"];
                bool COC = (bool)Session["COC"];
                UnapproveRejected = ProfileUtils.GetUnApproveRJCustomerProfile(userType);
                if (!COC)
                {
                    var filtered = from myrow in UnapproveRejected.AsEnumerable()
                                   where myrow.Field<string>("UserBranchCode") == BranchCode
                                   select myrow;
                    if (filtered.Count() > 0)
                        dtObj.UNApproveRejectTable = filtered.CopyToDataTable();
                }
                else
                {
                    dtObj.UNApproveRejectTable = UnapproveRejected;
                }


                //DataTable dtableUNApproveReject = ProfileUtils.GetUnApproveRJCustomerProfile();
                if (dtObj.UNApproveRejectTable != null && dtObj.UNApproveRejectTable.Rows.Count > 0)
                {
                    UserInfo regobj = new UserInfo();
                    regobj.Name = dtObj.UNApproveRejectTable.Rows[0]["Name"].ToString();
                    regobj.ClientCode = dtObj.UNApproveRejectTable.Rows[0]["ClientCode"].ToString();
                    regobj.Address = dtObj.UNApproveRejectTable.Rows[0]["Address"].ToString();
                    regobj.PIN = dtObj.UNApproveRejectTable.Rows[0]["PIN"].ToString();
                    regobj.Status = dtObj.UNApproveRejectTable.Rows[0]["Status"].ToString();
                    regobj.ContactNumber1 = dtObj.UNApproveRejectTable.Rows[0]["ContactNumber1"].ToString();
                    regobj.ContactNumber2 = dtObj.UNApproveRejectTable.Rows[0]["ContactNumber2"].ToString();
                    regobj.UserName = dtObj.UNApproveRejectTable.Rows[0]["UserName"].ToString();
                    regobj.UserType = dtObj.UNApproveRejectTable.Rows[0]["userType"].ToString();
                    regobj.IsApproved = dtObj.UNApproveRejectTable.Rows[0]["IsApproved"].ToString();
                    regobj.IsRejected = dtObj.UNApproveRejectTable.Rows[0]["IsRejected"].ToString();

                    ViewData["UNApproveReject"] = dtObj.UNApproveRejectTable;//dtableUNApproveReject;

                }
                else
                {
                    dtObj.UNApproveRejectTable = null;
                    //dtableUNApproveReject = null;
                    ViewData["UNApproveReject"] = dtObj.UNApproveRejectTable;//dtableUNApproveReject;
                }

                /// <summary>
                /// APPROVE REJECTED LIST OF CUSTOMER
                /// </summary>
                /// 

                DataTable ApproveRejected = ProfileUtils.GetApproveRJCustomerProfile(userType);
                DataTable dtableApproveReject = new DataTable();
                if (!COC)
                {
                    var filtered = from myrow in ApproveRejected.AsEnumerable()
                                   where myrow.Field<string>("ModifyingBranch") == BranchCode
                                   select myrow;
                    if (filtered.Count() > 0)
                        dtableApproveReject = filtered.CopyToDataTable();
                }
                else
                {
                    dtableApproveReject = ApproveRejected;
                }
                //dtObj.ApproveRejectTable = ProfileUtils.GetApproveRJCustomerProfile();
                if (dtableApproveReject != null && dtableApproveReject.Rows.Count > 0)
                {
                    UserInfo regobj = new UserInfo();
                    regobj.Name = dtableApproveReject.Rows[0]["Name"].ToString();
                    regobj.ClientCode = dtableApproveReject.Rows[0]["ClientCode"].ToString();
                    regobj.Address = dtableApproveReject.Rows[0]["Address"].ToString();
                    regobj.PIN = dtableApproveReject.Rows[0]["PIN"].ToString();
                    regobj.Status = dtableApproveReject.Rows[0]["Status"].ToString();
                    regobj.ContactNumber1 = dtableApproveReject.Rows[0]["ContactNumber1"].ToString();
                    regobj.ContactNumber2 = dtableApproveReject.Rows[0]["ContactNumber2"].ToString();
                    regobj.UserName = dtableApproveReject.Rows[0]["UserName"].ToString();
                    regobj.UserType = dtableApproveReject.Rows[0]["userType"].ToString();
                    regobj.IsApproved = dtableApproveReject.Rows[0]["IsApproved"].ToString();
                    regobj.IsRejected = dtableApproveReject.Rows[0]["IsRejected"].ToString();
                    dtObj.ApproveRejectTable = dtableApproveReject;
                    ViewData["ApproveReject"] = dtObj.ApproveRejectTable;//dtableApproveReject;
                }
                else
                {
                    dtObj.ApproveRejectTable = null;
                    //dtableApproveReject = null;
                    ViewData["ApproveReject"] = dtObj.ApproveRejectTable;// dtableApproveReject;

                    //return View(dtableApproveReject);
                }
                return View(dtObj);

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        #endregion


        #region "Customer PinResetList"

        // GET: Customer/PinResetList
        [HttpGet]
        public ActionResult PinResetList()
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


        #region "Customer PinReset"

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


        // GET: Customer/PinReset
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

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;
                string AdminUserName = (string)Session["UserName"];
                string AdminBranch = (string)Session["UserBranch"];
                bool COC = (bool)Session["COC"];
                var model = PinUtils.GetPinResetList(AdminBranch, COC, MobileNumber);
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
                bool COC = (bool)Session["COC"];
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
                        Message = string.Format("Dear {0},\n Your new T-Pin is {1}.\n Thank You -MNepal", info.Name, info.PIN);
                        resp = "T-Pin reset Successful for " + info.UserName;

                    }
                    else if (info.Status == "PASR")
                    {
                        info.Password = CustomerUtils.GeneratePassword();
                        Mode = "PASS";
                        Message = string.Format("Dear {0},\n Your new Password is {1}.\n Thank You -MNepal", info.Name, info.Password);
                        resp = "Password reset Successful for " + info.UserName;
                    }
                    else if (info.Status == "PPR")
                    {
                        info.PIN = CustomerUtils.GeneratePin();
                        info.Password = CustomerUtils.GeneratePassword();
                        Mode = "BOTH";
                        Message = string.Format("Dear {0},\n Your new T-Pin is {1} and Password is {2}.\n Thank You -MNepal", info.Name, info.PIN, info.Password);
                        resp = "T-Pin and Password reset Successful for " + info.UserName;
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
                            StatusMessage = "Error Approving customer pin"
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


        #region "Customer Delete UserList"

        // GET: Customer/DeleteUserList
        [HttpGet]
        public ActionResult DeleteUserList()
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


        #region "Customer Delete User"


        [HttpGet]
        public ActionResult DeleteUserDetail(string txtMobileNo)
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
                        DataTable dtableCustomerStatusByMobileNo = CustomerUtils.GetUserProfileByMobileNoA(userInfo.ContactNumber1);
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


        // GET: Customer/DeleteUser
        public ActionResult DeleteUser(string id)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (this.TempData["deleteUser_messsage"] != null)
            {
                this.ViewData["deleteUser_messsage"] = this.TempData["deleteUser_messsage"];
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

        //POST: Customer/DeleteUser/1
        [HttpPost]
        public ActionResult DeleteUser(string btnCommand, FormCollection collection)
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
                    string cClientCode = collection["ClientCode"].ToString();
                    string newStatus = collection["Status"].ToString();
                    string OldStatus = collection["OldStatus"].ToString();

                    UserInfo userInfo = new UserInfo();
                    userInfo.ClientCode = cClientCode;
                    userInfo.Status = newStatus;
                    string AdminUserName = Session["UserName"].ToString();
                    string AdminBranchCode = Session["UserBranch"].ToString();
                    if (OldStatus != newStatus)
                    {
                        if (newStatus != "")
                        {
                            bool isUpdated = DeleteUtils.ChangeUserInfo(cClientCode, newStatus, AdminBranchCode, AdminUserName);
                            displayMessage = isUpdated
                                                     ? "Status has successfully been updated."
                                                     : "Error while updating Status";
                            messageClass = isUpdated ? CssSetting.SuccessMessageClass : CssSetting.FailedMessageClass;
                            this.TempData["deleteUser_messsage"] = displayMessage;
                            this.TempData["message_class"] = messageClass;
                            return RedirectToAction("DeleteUserList");
                        }
                        else
                        {
                            displayMessage = "Status is Empty";
                            messageClass = CssSetting.FailedMessageClass;
                        }


                    }
                    else
                    {

                    }
                    this.TempData["deleteUser_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;
                    return RedirectToAction("DeleteUser");
                }
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        #endregion
        //View / approve blocked / active
        [HttpGet]
        public ActionResult StatusChangedList(string MobileNumber)
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
                string BranchCode = Session["UserBranch"].ToString();
                bool COC = (bool)Session["COC"];
                var model = CustomerUtils.GetUserStatus(BranchCode, COC, MobileNumber);
                return View(model);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpGet]
        public ContentResult StatusApprove(string ClientCode, string Name)
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
                bool COC = (bool)Session["COC"];
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

                    string Mode = "";
                    string Message = string.Empty;
                    string resp = string.Empty;

                    isUpdated = CustomerUtils.CustStatusApprove(info.ClientCode);
                    if (isUpdated)
                    {
                        resp = "Client Status approved successfully.";
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
                            StatusMessage = "Error Approving customer status"
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
                bool COC = (bool)Session["COC"];
                bool isUpdated = false;

                UserInfo info = new UserInfo();
                DataTable dtableUserInfo = ProfileUtils.GetUserProfileInfo(ClientCode);
                if (dtableUserInfo != null && dtableUserInfo.Rows.Count > 0)
                {
                    info.ClientCode = dtableUserInfo.Rows[0]["ClientCode"].ToString();
                    info.Name = dtableUserInfo.Rows[0]["Name"].ToString();
                    info.Address = dtableUserInfo.Rows[0]["Address"].ToString();
                    info.Status = dtableUserInfo.Rows[0]["Status"].ToString();
                    if (info.Status == "Blocked")
                    {
                        info.Status = "Active";
                    }
                    else
                    {
                        info.Status = "Blocked";
                    }
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

                    isUpdated = CustomerUtils.CustStatusReject(info.ClientCode, info.Status);
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


        #region "Customer Details"
        public ActionResult CustomerDetails()
        {
            //{ string userName = (string)Session["LOGGED_USERNAME"];
            //    string clientCode = (string)Session["LOGGEDUSER_ID"];
            //    string name = (string)Session["LOGGEDUSER_NAME"];
            //    string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = "superadmin";
            this.ViewData["userType"] = this.TempData["userType"];
            //if (TempData["userType"] != null)
            //{
            //    this.ViewData["userType"] = this.TempData["userType"];
            //    ViewBag.UserType = this.TempData["userType"];
            //    ViewBag.Name = name;
            //}

            return View();
        }

        #endregion

        [HttpGet]
        #region "Customer Profile Insert"
        public ActionResult CreateCusProfile()
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

                if (this.TempData["cusprofile_message"] != null)
                {
                    this.ViewData["cusprofile_message"] = this.TempData["cusprofile_message"];
                    this.ViewData["message_class"] = this.TempData["message_class"];
                }
                CusProfileViewModel Vm = new CusProfileViewModel();
                Vm.MNFeatures = CusProfileUserModel.GetMNFeature();


                return this.View(Vm);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        #endregion

        [HttpPost]
        #region "Customer Profile Insert"
        public ActionResult CreateCusProfile(FormCollection collection)
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

                    CusProfileViewModel cusProfile = new CusProfileViewModel();
                    cusProfile.m_ProfileCode = collection["txtProfileCode"].ToString();
                    cusProfile.m_ProfileDesc = collection["txtProfileDesc"].ToString();
                    cusProfile.m_ProfileStatus = collection["txtPStatus"].ToString();
                    cusProfile.m_RenewPeriod = Convert.ToInt32(collection["txtRenewPeriod"].ToString().Trim() == "" ? "0" : collection["txtRenewPeriod"].ToString().Trim());

                    //cusProfile.m_Registration = Convert.ToDecimal(collection["txtRegistration"].ToString().Trim() == "" ? "0" : collection["txtRegistration"].ToString().Trim());
                    //cusProfile.m_ReNew = Convert.ToDecimal(collection["txtRenewal"].ToString().Trim() == "" ? "0" : collection["txtRenewal"].ToString().Trim());
                    //cusProfile.m_PinReset = Convert.ToDecimal(collection["txtPIN"].ToString().Trim() == "" ? "0" : collection["txtPIN"].ToString().Trim());
                    //cusProfile.m_ChargeAccount = collection["txtCharge"].ToString();
                    /*
                    cusProfile.m_HasCharge = collection["txtHasCharge"].ToString();
                    cusProfile.m_IsDrAlert = collection["txtIsDrAlert"].ToString();
                    cusProfile.m_IsCrAlert = collection["txtIsCrAlert"].ToString();*/
                    //cusProfile.m_MinDrAlertAmt = Convert.ToDecimal(collection["txtMinDrAlertAmt"].ToString().Trim() == "" ? "0" : collection["txtMinDrAlertAmt"].ToString().Trim());
                    //cusProfile.m_MinCrAlertAmt = Convert.ToDecimal(collection["txtMinCrAlertAmt"].ToString().Trim() == "" ? "0" : collection["txtMinCrAlertAmt"].ToString().Trim());
                    cusProfile.m_ReNew = collection["txtRenewal"].ToString();// Convert.ToDecimal(collection["txtRenewal"].ToString().Trim() == "" ? "0" : collection["txtRenewal"].ToString().Trim());
                    cusProfile.m_ChargeAccount = collection["txtCharge"].ToString();
                    if (collection.AllKeys.Contains("txtHasCharge"))
                    {
                        cusProfile.m_HasCharge = collection["txtHasCharge"].ToString();
                        cusProfile.m_Registration = collection["txtRegistration"].ToString();// Convert.ToDecimal(collection["txtRegistration"].ToString().Trim() == "" ? "0" : collection["txtRegistration"].ToString().Trim());
                        cusProfile.m_PinReset = collection["txtPIN"].ToString();// Convert.ToDecimal(collection["txtPIN"].ToString().Trim() == "" ? "0" : collection["txtPIN"].ToString().Trim());
                    }
                    else
                    {
                        cusProfile.m_HasCharge = "F";
                        cusProfile.m_Registration = "0";

                        cusProfile.m_PinReset = "0";

                    }
                    if (collection.AllKeys.Contains("txtIsDrAlert"))
                    {
                        cusProfile.m_IsDrAlert = collection["txtIsDrAlert"].ToString();
                        cusProfile.m_MinDrAlertAmt = Convert.ToDecimal(collection["txtMinDrAlertAmt"].ToString().Trim() == "" ? "0" : collection["txtMinDrAlertAmt"].ToString().Trim());
                    }
                    else
                    {
                        cusProfile.m_IsDrAlert = "F";
                        cusProfile.m_MinDrAlertAmt = 0;
                    }
                    if (collection.AllKeys.Contains("txtIsCrAlert"))
                    {
                        cusProfile.m_IsCrAlert = collection["txtIsCrAlert"].ToString();
                        cusProfile.m_MinCrAlertAmt = Convert.ToDecimal(collection["txtMinCrAlertAmt"].ToString().Trim() == "" ? "0" : collection["txtMinCrAlertAmt"].ToString().Trim());
                    }
                    else
                    {
                        cusProfile.m_IsCrAlert = "F";
                        cusProfile.m_MinCrAlertAmt = 0;
                    }
                    if (collection.AllKeys.Contains("txtAutoRenew"))
                    {
                        cusProfile.m_AutoRenew = collection["txtAutoRenew"].ToString();
                    }
                    else
                    {
                        cusProfile.m_AutoRenew = "F";
                    }
                    /**/
                    string[] features;
                    List<MNFeatureMasterVM> MNFeature = new List<MNFeatureMasterVM>();

                    if (!String.IsNullOrEmpty(collection["hidFeatures"].ToString()))
                    {
                        features = collection["hidFeatures"].ToString().Split(',');
                        features = features.Where(x => !string.IsNullOrEmpty(x.Trim())).ToArray();

                        foreach (string feature in features)
                        {


                            MNFeature.Add(new MNFeatureMasterVM
                            {
                                FeatureCode = feature,
                                TxnCount = collection[String.Format("txtTxnCount{0}", feature)].ToString(),
                                PerDayTxnAmt = collection[String.Format("txtPerDayTxnAmt{0}", feature)].ToString(),
                                PerTxnAmt = collection[String.Format("txtPerTxnAmt{0}", feature)].ToString()

                            });

                        }

                    }

                    if (string.IsNullOrEmpty(collection["txtProfileCode"]))
                    {
                        ModelState.AddModelError("m_ProfileCode", "*Please enter Code");
                    }
                    if (string.IsNullOrEmpty(collection["txtProfileDesc"]))
                    {
                        ModelState.AddModelError("m_ProfileDesc", "*Please enter Description");
                    }
                    if (string.IsNullOrEmpty(collection["txtPStatus"]))
                    {
                        ModelState.AddModelError("m_ProfileStatus", "*Please enter Profile Status");
                    }
                    if (string.IsNullOrEmpty(collection["txtRenewPeriod"]))
                    {
                        ModelState.AddModelError("m_RenewPeriod", "*Please enter Renew Period");
                    }

                    if (string.IsNullOrEmpty(collection["txtAutoRenew"]))
                    {
                        ModelState.AddModelError("m_AutoRenew ", "*Please enter Auto Renew");
                    }
                    bool result = false;
                    string errorMessage = string.Empty;

                    if ((cusProfile.m_ProfileCode != "") && (cusProfile.m_ProfileDesc != "") && (cusProfile.m_ProfileStatus != ""))
                    {
                        if (collection.AllKeys.Any())
                        {
                            try
                            {

                                //int resultmsg = CusProfileUtils.GetChargesXMLStr(cusProfile);
                                CusProfileUtils cusutils = new CusProfileUtils();
                                List<MNProfileChargeClass> chargeList = new List<MNProfileChargeClass>();
                                chargeList.Add(new MNProfileChargeClass { ProfileCode = cusProfile.m_ProfileCode, Registration = cusProfile.m_Registration, ReNew = cusProfile.m_ReNew, PinReset = cusProfile.PinReset, ChargeAccount = cusProfile.m_ChargeAccount });

                                string chargeXML = cusutils.GetChargesXMLStr(chargeList);

                                List<MNTxnLimitClass> txnLimitList = new List<MNTxnLimitClass>();
                                foreach (var item in MNFeature)
                                {
                                    txnLimitList.Add(new MNTxnLimitClass
                                    {
                                        ProfileCode = cusProfile.m_ProfileCode,
                                        FeatureCode = item.FeatureCode,
                                        TxnCount = item.TxnCount,
                                        PerTxnAmt = item.PerTxnAmt,
                                        PerDayAmt = item.PerDayTxnAmt
                                    });
                                }
                                //txnLimitList.Add(new MNTxnLimitClass { ProfileCode = cusProfile.m_ProfileCode, FeatureCode = "01", TxnCount = 10, PerTxnAmt = 500, PerDayAmt = 10000 });
                                //txnLimitList.Add(new MNTxnLimitClass { ProfileCode = cusProfile.m_ProfileCode, FeatureCode = "02", TxnCount = 10, PerTxnAmt = 600, PerDayAmt = 12000 });

                                string txnLimitXML = cusutils.GetTxnLimitXMLStr(txnLimitList);

                                //List<MNContactClass> contactList = new List<MNContactClass>();
                                //contactList.Add(new MNContactClass { ClientCode = "00112345", Address = "Kathmandu", ContactNum1 = "9841123456", ContactNum2 = "", EmailAddress = "email@gmail.com" });

                                //string contactXML = newMNUtil.GetContactXMLStr(contactList);



                                MNProfileClass newProfile = new MNProfileClass();
                                newProfile.ProfileCode = cusProfile.m_ProfileCode;
                                newProfile.ProfileDesc = cusProfile.m_ProfileDesc;
                                newProfile.AutoRenew = cusProfile.m_AutoRenew;
                                newProfile.RenewPeriod = cusProfile.m_RenewPeriod;
                                newProfile.ProfileStatus = cusProfile.m_ProfileStatus;
                                newProfile.Charge = chargeXML;
                                newProfile.TxnLimit = txnLimitXML;
                                newProfile.HasCharge = cusProfile.m_HasCharge;
                                newProfile.IsDrAlert = cusProfile.m_IsDrAlert;
                                newProfile.IsCrAlert = cusProfile.m_IsCrAlert;
                                newProfile.MinDrAlertAmt = cusProfile.m_MinDrAlertAmt;
                                newProfile.MinCrAlertAmt = cusProfile.m_MinCrAlertAmt;

                                string retMessage = string.Empty;

                                //int retValue = DAL.AddNewCustProfile(newProfile, ConnectionString, out retMessage);

                                //CusProfileUserModel.GetCustProfileDetails(cusProfile.m_ProfileCode, ConnectionString);
                                int retValue = CusProfileUserModel.AddNewCustProfile(newProfile, out retMessage);

                                if (retValue == 100)

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
                    else
                    {
                        result = false;
                    }

                    this.TempData["cusprofile_message"] = result
                                                  ? "Account Type information is successfully added."
                                                  : "Error while inserting the information. ERROR :: "
                                                    + errorMessage;
                    this.TempData["message_class"] = result ? "success_info" : "failed_info";

                    return this.RedirectToAction("CreateCusProfile");
                }
                else
                {
                    return RedirectToAction("Index", "Login");
                }

            }

            catch (Exception ex)
            {
                CusProfileViewModel Vm = new CusProfileViewModel();
                Vm.MNFeatures = CusProfileUserModel.GetMNFeature();
                return View(Vm);
            }

        }

        #endregion


        #region "ManageCostumerProfile"

        public ActionResult ListCostumerProfile()
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
                return View(CusProfileUserModel.GetMNCustProfile());
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }

        #endregion


        #region "EditCostumerProfile"
        [HttpGet]
        public ActionResult EditCostumerProfile(string Id)
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
                List<MNCustProfile> ProfileList = new List<MNCustProfile>();
                List<SelectListItem> ItemProfile = new List<SelectListItem>();
                ProfileList = CusProfileUserModel.GetMNCustProfile().Where(x => x.ProfileStatus == 'A').ToList();

                foreach (MNCustProfile Cust in ProfileList)
                {
                    ItemProfile.Add(new SelectListItem
                    {
                        Text = Cust.ProfileCode,
                        Value = Cust.ProfileCode
                    });
                }

                ViewBag.ProfileList = new SelectList(ItemProfile, "Value", "Text");

                var model = CusProfileUserModel.GetCustProfileDetails(Id, true);
                if (model == null)
                {
                    this.TempData["cusprofile_message"] = "Error Fetching Info For this Profile";

                    this.TempData["message_class"] = "failed_info";

                    return RedirectToAction("ListCostumerProfile", "Customer");
                }

                return View(model);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        [HttpPost]
        public ActionResult EditCostumerProfile(FormCollection collection)
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

                CusProfileViewModel cusProfile = new CusProfileViewModel();
                cusProfile.m_ProfileCode = collection["txtProfileCode"].ToString();
                cusProfile.m_ProfileDesc = collection["txtProfileDesc"].ToString();
                cusProfile.m_ProfileStatus = collection["txtPStatus"].ToString();
                cusProfile.m_RenewPeriod = Convert.ToInt32(collection["txtRenewPeriod"].ToString().Trim() == "" ? "0" : collection["txtRenewPeriod"].ToString().Trim());
                //cusProfile.m_Registration = Convert.ToDecimal(collection["txtRegistration"].ToString().Trim() == "" ? "0" : collection["txtRegistration"].ToString().Trim());
                //cusProfile.m_ReNew = Convert.ToDecimal(collection["txtRenewal"].ToString().Trim() == "" ? "0" : collection["txtRenewal"].ToString().Trim());
                //cusProfile.m_PinReset = Convert.ToDecimal(collection["txtPIN"].ToString().Trim() == "" ? "0" : collection["txtPIN"].ToString().Trim());
                //cusProfile.m_ChargeAccount = collection["txtCharge"].ToString();
                //cusProfile.m_MinDrAlertAmt = Convert.ToDecimal(collection["txtMinDrAlertAmt"].ToString().Trim() == "" ? "0" : collection["txtMinDrAlertAmt"].ToString().Trim());
                //cusProfile.m_MinCrAlertAmt = Convert.ToDecimal(collection["txtMinCrAlertAmt"].ToString().Trim() == "" ? "0" : collection["txtMinCrAlertAmt"].ToString().Trim());
                cusProfile.m_ReNew = collection["txtRenewal"].ToString(); //Convert.ToDecimal(collection["txtRenewal"].ToString().Trim() == "" ? "0" : collection["txtRenewal"].ToString().Trim());
                cusProfile.m_ChargeAccount = collection["txtCharge"].ToString();
                if (collection.AllKeys.Contains("txtHasCharge"))
                {
                    cusProfile.m_HasCharge = collection["txtHasCharge"].ToString();
                    cusProfile.m_Registration = collection["txtRegistration"].ToString();// Convert.ToDecimal(collection["txtRegistration"].ToString().Trim() == "" ? "0" : collection["txtRegistration"].ToString().Trim());
                    cusProfile.m_PinReset = collection["txtPIN"].ToString();// Convert.ToDecimal(collection["txtPIN"].ToString().Trim() == "" ? "0" : collection["txtPIN"].ToString().Trim());
                }
                else
                {
                    cusProfile.m_HasCharge = "F";
                    cusProfile.m_Registration = "0";
                    cusProfile.m_PinReset = "0";

                }
                if (collection.AllKeys.Contains("txtIsDrAlert"))
                {
                    cusProfile.m_IsDrAlert = collection["txtIsDrAlert"].ToString();
                    cusProfile.m_MinDrAlertAmt = Convert.ToDecimal(collection["txtMinDrAlertAmt"].ToString().Trim() == "" ? "0" : collection["txtMinDrAlertAmt"].ToString().Trim());
                }
                else
                {
                    cusProfile.m_IsDrAlert = "F";
                    cusProfile.m_MinDrAlertAmt = 0;
                }
                if (collection.AllKeys.Contains("txtIsCrAlert"))
                {
                    cusProfile.m_IsCrAlert = collection["txtIsCrAlert"].ToString();
                    cusProfile.m_MinCrAlertAmt = Convert.ToDecimal(collection["txtMinCrAlertAmt"].ToString().Trim() == "" ? "0" : collection["txtMinCrAlertAmt"].ToString().Trim());
                }
                else
                {

                    cusProfile.m_IsCrAlert = "F";
                    cusProfile.m_MinCrAlertAmt = 0;
                }
                if (collection.AllKeys.Contains("txtAutoRenew"))
                {
                    cusProfile.m_AutoRenew = collection["txtAutoRenew"].ToString();
                }
                else
                {
                    cusProfile.m_AutoRenew = "F";
                }
                /**/
                string[] features;
                List<MNFeatureMasterVM> MNFeature = new List<MNFeatureMasterVM>();

                if (!String.IsNullOrEmpty(collection["hidFeatures"].ToString()))
                {
                    features = collection["hidFeatures"].ToString().Split(',');
                    features = features.Where(x => !string.IsNullOrEmpty(x.Trim())).ToArray();

                    foreach (string feature in features)
                    {


                        MNFeature.Add(new MNFeatureMasterVM
                        {
                            FeatureCode = feature,
                            TxnCount = collection[String.Format("txtTxnCount{0}", feature)].ToString(),
                            PerDayTxnAmt = collection[String.Format("txtPerDayTxnAmt{0}", feature)].ToString(),
                            PerTxnAmt = collection[String.Format("txtPerTxnAmt{0}", feature)].ToString()

                        });

                    }

                }

                if (string.IsNullOrEmpty(collection["txtProfileCode"]))
                {
                    ModelState.AddModelError("m_ProfileCode", "*Please enter Code");
                }
                if (string.IsNullOrEmpty(collection["txtProfileDesc"]))
                {
                    ModelState.AddModelError("m_ProfileDesc", "*Please enter Description");
                }
                if (string.IsNullOrEmpty(collection["txtPStatus"]))
                {
                    ModelState.AddModelError("m_ProfileStatus", "*Please enter Profile Status");
                }
                if (string.IsNullOrEmpty(collection["txtRenewPeriod"]))
                {
                    ModelState.AddModelError("m_RenewPeriod", "*Please enter Renew Period");
                }

                if (string.IsNullOrEmpty(collection["txtAutoRenew"]))
                {
                    ModelState.AddModelError("m_AutoRenew ", "*Please enter Auto Renew");
                }
                bool result = false;
                string errorMessage = string.Empty;

                if ((cusProfile.m_ProfileCode != "") && (cusProfile.m_ProfileDesc != "") && (cusProfile.m_ProfileStatus != ""))
                {
                    if (collection.AllKeys.Any())
                    {
                        try
                        {

                            //int resultmsg = CusProfileUtils.GetChargesXMLStr(cusProfile);
                            CusProfileUtils cusutils = new CusProfileUtils();
                            List<MNProfileChargeClass> chargeList = new List<MNProfileChargeClass>();
                            chargeList.Add(new MNProfileChargeClass { ProfileCode = cusProfile.m_ProfileCode, Registration = cusProfile.m_Registration, ReNew = cusProfile.m_ReNew, PinReset = cusProfile.PinReset, ChargeAccount = cusProfile.m_ChargeAccount });

                            string chargeXML = cusutils.GetChargesXMLStr(chargeList);

                            List<MNTxnLimitClass> txnLimitList = new List<MNTxnLimitClass>();
                            foreach (var item in MNFeature)
                            {
                                txnLimitList.Add(new MNTxnLimitClass
                                {
                                    ProfileCode = cusProfile.m_ProfileCode,
                                    FeatureCode = item.FeatureCode,
                                    TxnCount = item.TxnCount,
                                    PerTxnAmt = item.PerTxnAmt,
                                    PerDayAmt = item.PerDayTxnAmt
                                });
                            }
                            //txnLimitList.Add(new MNTxnLimitClass { ProfileCode = cusProfile.m_ProfileCode, FeatureCode = "01", TxnCount = 10, PerTxnAmt = 500, PerDayAmt = 10000 });
                            //txnLimitList.Add(new MNTxnLimitClass { ProfileCode = cusProfile.m_ProfileCode, FeatureCode = "02", TxnCount = 10, PerTxnAmt = 600, PerDayAmt = 12000 });

                            string txnLimitXML = cusutils.GetTxnLimitXMLStr(txnLimitList);

                            //List<MNContactClass> contactList = new List<MNContactClass>();
                            //contactList.Add(new MNContactClass { ClientCode = "00112345", Address = "Kathmandu", ContactNum1 = "9841123456", ContactNum2 = "", EmailAddress = "email@gmail.com" });

                            //string contactXML = newMNUtil.GetContactXMLStr(contactList);



                            MNProfileClass newProfile = new MNProfileClass();
                            newProfile.ProfileCode = cusProfile.m_ProfileCode;
                            newProfile.ProfileDesc = cusProfile.m_ProfileDesc;
                            newProfile.AutoRenew = cusProfile.m_AutoRenew;
                            newProfile.RenewPeriod = cusProfile.m_RenewPeriod;
                            newProfile.ProfileStatus = cusProfile.m_ProfileStatus;
                            newProfile.Charge = chargeXML;
                            newProfile.TxnLimit = txnLimitXML;
                            newProfile.HasCharge = cusProfile.m_HasCharge;
                            newProfile.IsDrAlert = cusProfile.m_IsDrAlert;
                            newProfile.IsCrAlert = cusProfile.m_IsCrAlert;
                            newProfile.MinDrAlertAmt = cusProfile.m_MinDrAlertAmt;
                            newProfile.MinCrAlertAmt = cusProfile.m_MinCrAlertAmt;

                            string retMessage = string.Empty;

                            //int retValue = DAL.AddNewCustProfile(newProfile, ConnectionString, out retMessage);

                            //CusProfileUserModel.GetCustProfileDetails(cusProfile.m_ProfileCode, ConnectionString);
                            int retValue = CusProfileUserModel.UpdateCustProfile(newProfile, out retMessage);

                            if (retValue == 100)

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
                else
                {
                    result = false;
                }

                this.TempData["cusprofile_message"] = result
                                              ? "Customer Profile Successfully Updated"
                                              : "Error while updating the information. ERROR :: "
                                                + errorMessage;
                this.TempData["message_class"] = result ? "success_info" : "failed_info";
                if (result)
                    return this.RedirectToAction("ListCostumerProfile", "Customer");

                return this.RedirectToAction("EditCostumerProfile", new { Id = cusProfile.m_ProfileCode });
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }

        #endregion


        #region "Customer Approve "

        /// <summary>
        /// APPROVE REMAIN LIST OF CUSTOMER
        /// </summary>
        /// 




        // GET: Customer/ApproveList
        [HttpGet]
        public ActionResult ApproveList(string Key, string Value)
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

                bool COC = (bool)Session["COC"];
                TempData["COC"] = COC;

                if (this.TempData["custapprove_messsage"] != null)
                {
                    this.ViewData["custapprove_messsage"] = this.TempData["custapprove_messsage"];
                    this.ViewData["message_class"] = this.TempData["message_class"];
                }

                List<UserInfo> custUnApproveObj = new List<UserInfo>();
                UserInfo userInfo = new UserInfo();
                DataTable dtblUnapproveCus = new DataTable();
                DataTable dtblCus = CustomerUtils.GetUnApproveCustProfile("user");

                //bool COC = Session["COC"] == null ? false : (bool)Session["COC"];
                string UserBranchCode = (string)Session["UserBranch"];
                ViewBag.Value = Value;
                if (Key == "Mobile")
                {
                    if (!string.IsNullOrEmpty(Value))
                    {
                        EnumerableRowCollection<DataRow> row;
                        if (COC == true)
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
                    if (COC == false)
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
                    ViewBag.COC = COC; //test
                    custUnApproveObj.Add(userInfo);


                }

                var rowbankregistred = dtblUnapproveCus.AsEnumerable().Where(r => r.Field<string>("SelfRegistered") == "F");
                var selfregistred = dtblUnapproveCus.AsEnumerable().Where(r => r.Field<string>("SelfRegistered") == "T");

                ViewData["dtableCustForApproval"] = new DataTable();
                ViewData["dtableCustForApprovalSelf"] = new DataTable();
                if (rowbankregistred.Any())
                    ViewData["dtableCustForApproval"] = rowbankregistred.CopyToDataTable();
                if (selfregistred.Any())
                    ViewData["dtableCustForApprovalSelf"] = selfregistred.CopyToDataTable();

                return View(userInfo);

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpPost]
        public ActionResult Approve(UserInfo model, string btnApprove)
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
                    userInfoApproval.Remarks = model.Remarks;
                    int ret = CustomerUtils.CustInfoReject(userInfoApproval);
                    if (ret == 100)
                    {
                        displayMessage = "Customer " + model.Name + " has been Rejected. Please Check Rejectlist and perform accordingly";
                        messageClass = CssSetting.SuccessMessageClass;
                    }
                    else if (ret == -1 || ret != 100)
                    {
                        displayMessage = "Error while rejecting Customer " + model.Name;
                        messageClass = CssSetting.FailedMessageClass;
                    }

                    this.TempData["custapprove_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;

                    return RedirectToAction("ApproveList");

                }
                else if (btnApprove.ToUpper() == "APPROVE")
                {
                    userInfoApproval.ApprovedBy = userName;
                    int ret = CustomerUtils.CustInfoApproval(userInfoApproval);
                    if (ret == 100)
                    {
                        UserInfo userInfo = new UserInfo();

                        DataSet DSet = ProfileUtils.GetUserProfileInfoDS(model.ClientCode);
                        DataTable dtableUserInfo = DSet.Tables["dtUserInfo"];
                        string Pin = dtableUserInfo.Rows[0]["PIN"].ToString();
                        string Password = dtableUserInfo.Rows[0]["Password"].ToString();
                        userInfo.ClientCode = dtableUserInfo.Rows[0]["ClientCode"].ToString();
                        userInfo.Password = CustomerUtils.GeneratePassword();
                        bool passChange = PasswordUtils.ResetPassword(userInfo);

                        displayMessage = "Customer Information for  user " + model.Name + " has successfully been approved.";


                        //SMS
                        SMSUtils SMS = new SMSUtils();

                        string Message = string.Format("Dear {0},\n Your new T-Pin is {1} and Password is {2}.\n Thank You -MNepal", model.Name, Pin, userInfo.Password);
                        //string Message = "Dear Customer," + "\n";
                        //Message += " Your T-Pin is " + Pin + " and Password is " + Password
                        //    + "." + "\n" + "Thank You";
                        //Message += "-MNepal";

                        SMSLog Log = new SMSLog();

                        Log.UserName = model.UserName;
                        Log.Purpose = "NR"; //New Registration
                        Log.SentBy = userInfoApproval.ApprovedBy;
                        Log.Message = "Your T-Pin is " + ExtraUtility.Encrypt(Pin) + " and Password is " + ExtraUtility.Encrypt(Password); //encrypted when logging
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

                    return RedirectToAction("ApproveList");
                }
                else
                {

                    this.TempData["custapprove_messsage"] = "Unauthorized execution";
                    this.TempData["message_class"] = CssSetting.FailedMessageClass;

                    return RedirectToAction("ApproveList");
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

        #region ViewApprove

        public ActionResult ViewApproveDetail(string id)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            //ViewBag.Province = list;


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

                ViewBag.Bank = item;
                List<MNCustProfile> ProfileList = new List<MNCustProfile>();
                List<SelectListItem> ItemProfile = new List<SelectListItem>();
                ProfileList = CusProfileUserModel.GetMNCustProfile().Where(x => x.ProfileStatus == 'A').ToList();

                foreach (MNCustProfile Cust in ProfileList)
                {
                    ItemProfile.Add(new SelectListItem
                    {
                        Text = Cust.ProfileCode,
                        Value = Cust.ProfileCode

                    });
                }

                UserInfo regobj = new UserInfo();
                DataSet DSet = ProfileUtils.GetUserProfileInfoDS(id);
                DataTable dtableUserInfo = DSet.Tables["dtUserInfo"];
                DataTable dTranstable = DSet.Tables["dtTransaction"];
                DataTable dtUserKYC = DSet.Tables["dtUserKYC"];
                DataTable dtUserKYCDoc = DSet.Tables["dtUserKYCDoc"];

                if (dtableUserInfo != null && dtableUserInfo.Rows.Count > 0)
                {
                    regobj.ClientCode = dtableUserInfo.Rows[0]["ClientCode"].ToString();
                    regobj.Name = dtableUserInfo.Rows[0]["Name"].ToString();

                    //regobj.FName = dtableUserInfo.Rows[0]["FName"].ToString();
                    //regobj.MName = dtableUserInfo.Rows[0]["MName"].ToString();
                    //regobj.LName = dtableUserInfo.Rows[0]["LName"].ToString();
                    regobj.ClientStatus = dtableUserInfo.Rows[0]["ClientStatus"].ToString();
                    //regobj.Gender = dtableUserInfo.Rows[0]["Gender"].ToString();

                    //start milayako 02
                    regobj.PStreet = dtableUserInfo.Rows[0]["PStreet"].ToString();
                    regobj.PProvince = getProvinceName(dtableUserInfo.Rows[0]["PProvince"].ToString());
                    regobj.PDistrictID = getDistrictName(dtableUserInfo.Rows[0]["PDistrictID"].ToString());
                    regobj.PMunicipalityVDC = dtableUserInfo.Rows[0]["PMunicipalityVDC"].ToString();
                    regobj.PHouseNo = dtableUserInfo.Rows[0]["PHouseNo"].ToString();
                    regobj.PWardNo = dtableUserInfo.Rows[0]["PWardNo"].ToString();


                    regobj.CStreet = dtableUserInfo.Rows[0]["CStreet"].ToString();
                    regobj.CProvince = getProvinceName(dtableUserInfo.Rows[0]["CProvince"].ToString());
                    regobj.CDistrictID = getDistrictName(dtableUserInfo.Rows[0]["CDistrictID"].ToString());
                    regobj.CMunicipalityVDC = dtableUserInfo.Rows[0]["CMunicipalityVDC"].ToString();
                    regobj.CHouseNo = dtableUserInfo.Rows[0]["CHouseNo"].ToString();
                    regobj.CWardNo = dtableUserInfo.Rows[0]["CWardNo"].ToString();
                    //end milayako 02


                    //start delete garnu parne 02
                    regobj.Address = dtableUserInfo.Rows[0]["Address"].ToString();
                    //end delete garnu parne 02


                    regobj.PIN = dtableUserInfo.Rows[0]["PIN"].ToString();
                    regobj.Status = dtableUserInfo.Rows[0]["Status"].ToString();
                    regobj.ContactNumber1 = dtableUserInfo.Rows[0]["ContactNumber1"].ToString();
                    regobj.ContactNumber2 = dtableUserInfo.Rows[0]["ContactNumber2"].ToString();
                    regobj.UserName = dtableUserInfo.Rows[0]["UserName"].ToString();
                    regobj.UserType = dtableUserInfo.Rows[0]["userType"].ToString();
                    //regobj.ProfileName = dtableUserInfo.Rows[0]["ProfileName"].ToString();
                    regobj.ProfileCode = dtableUserInfo.Rows[0]["ProfileCode"].ToString();
                    regobj.IsApproved = dtableUserInfo.Rows[0]["IsApproved"].ToString();
                    regobj.IsRejected = dtableUserInfo.Rows[0]["IsRejected"].ToString();
                    regobj.WalletNumber = dtableUserInfo.Rows[0]["WalletNumber"].ToString();
                    regobj.EmailAddress = dtableUserInfo.Rows[0]["EmailAddress"].ToString();
                    regobj.NewMobileNo = dtableUserInfo.Rows[0]["NewMobileNo"].ToString();

                    regobj.BankNo = dtableUserInfo.Rows[0]["BankNo"].ToString();
                    regobj.BranchCode = dtableUserInfo.Rows[0]["BranchCode"].ToString();
                    regobj.BankAccountNumber = dtableUserInfo.Rows[0]["BankAccountNumber"].ToString();
                    regobj.COC = dtableUserInfo.Rows[0]["COC"].ToString();


                    regobj.Transaction = dtableUserInfo.Rows[0]["IndvTxn"].ToString();
                    regobj.DateRange = dtableUserInfo.Rows[0]["DateRange"].ToString();
                    regobj.StartDate = dtableUserInfo.Rows[0]["StartDate"].ToString();
                    regobj.EndDate = dtableUserInfo.Rows[0]["EndDate"].ToString();
                    regobj.LimitType = dtableUserInfo.Rows[0]["LimitType"].ToString();
                    regobj.TransactionCount = dtableUserInfo.Rows[0]["TransactionCount"].ToString();
                    regobj.TransactionLimitDaily = dtableUserInfo.Rows[0]["TransactionLimitDaily"].ToString();
                    regobj.TransactionLimitMonthly = dtableUserInfo.Rows[0]["TransactionLimitMonthly"].ToString();
                    regobj.TransactionLimit = dtableUserInfo.Rows[0]["TransactionLimit"].ToString();

                    regobj.SelfRegistered = dtableUserInfo.Rows[0]["SelfRegistered"].ToString();

                    regobj.Gender = dtUserKYC.Rows[0]["Gender"].ToString();
                    regobj.Nationality = dtUserKYC.Rows[0]["Nationality"].ToString();

                    regobj.DOB = DateTime.Parse(dtUserKYC.Rows[0]["DateOfBirth"].ToString()).ToString("dd/MM/yyyy");

                    regobj.BSDateOfBirth = dtUserKYC.Rows[0]["BSDateOfBirth"].ToString();
                    regobj.Country = dtUserKYC.Rows[0]["CountryCode"].ToString();
                    regobj.FatherName = dtUserKYC.Rows[0]["FathersName"].ToString();
                    regobj.MotherName = dtUserKYC.Rows[0]["MothersName"].ToString();
                    regobj.GrandFatherName = dtUserKYC.Rows[0]["GFathersName"].ToString();
                    regobj.Occupation = dtUserKYC.Rows[0]["Occupation"].ToString();
                    regobj.MaritalStatus = dtUserKYC.Rows[0]["MaritalStatus"].ToString();
                    regobj.SpouseName = dtUserKYC.Rows[0]["SpouseName"].ToString();
                    regobj.FatherInlawName = dtUserKYC.Rows[0]["FatherInLaw"].ToString();
                    regobj.Document = dtUserKYCDoc.Rows[0]["DocType"].ToString();

                    regobj.Citizenship = dtUserKYC.Rows[0]["CitizenshipNo"].ToString();
                    regobj.CitizenshipIssueDate = DateTime.Parse(dtUserKYC.Rows[0]["CitizenIssueDate"].ToString()).ToString("dd/MM/yyyy");
                    regobj.BSCitizenshipIssueDate = dtUserKYC.Rows[0]["BSCitizenIssueDate"].ToString();
                    regobj.CitizenshipPlaceOfIssue = dtUserKYC.Rows[0]["CitizenPlaceOfIssue"].ToString();


                    regobj.Passport = dtUserKYC.Rows[0]["PassportNo"].ToString();
                    regobj.PassportPlaceOfIssue = dtUserKYC.Rows[0]["PassportPlaceOfIssue"].ToString();
                    regobj.PassportIssueDate = DateTime.Parse(dtUserKYC.Rows[0]["PassportIssueDate"].ToString()).ToString("dd/MM/yyyy");
                    regobj.PassportExpireDate = DateTime.Parse(dtUserKYC.Rows[0]["PassportExpiryDate"].ToString()).ToString("dd/MM/yyyy");



                    regobj.License = dtUserKYC.Rows[0]["LicenseNo"].ToString();
                    regobj.LicensePlaceOfIssue = dtUserKYC.Rows[0]["LicensePlaceOfIssue"].ToString();
                    regobj.LicenseIssueDate = DateTime.Parse(dtUserKYC.Rows[0]["LicenseIssueDate"].ToString()).ToString("dd/MM/yyyy");
                    regobj.BSLicenseIssueDate = dtUserKYC.Rows[0]["BSLicenseIssueDate"].ToString();
                    regobj.LicenseExpireDate = DateTime.Parse(dtUserKYC.Rows[0]["LicenseExpiryDate"].ToString()).ToString("dd/MM/yyyy");
                    regobj.BSLicenseExpireDate = dtUserKYC.Rows[0]["BSLicenseExpiryDate"].ToString();

                    regobj.FrontImage = dtUserKYCDoc.Rows[0]["FrontImage"].ToString();
                    regobj.BackImage = dtUserKYCDoc.Rows[0]["BackImage"].ToString();
                    regobj.PassportImage = dtUserKYCDoc.Rows[0]["PassportImage"].ToString();
                    ViewBag.FrontImage = regobj.FrontImage;
                    ViewBag.BackImage = regobj.BackImage;
                    ViewBag.PassportImage = regobj.PassportImage;
                    //regobj.CustStatus = dtUserKYCDoc.Rows[0]["CustStatus"].ToString();
                }

                List<TransactionInfo> lst = new List<TransactionInfo>();

                if (dTranstable != null && dTranstable.Rows.Count > 0)
                {
                    foreach (DataRow row in dTranstable.Rows)
                    {
                        lst.Add(new TransactionInfo
                        {

                            AcNumber = row["AcNumber"].ToString(),
                            Alias = row["Alias"].ToString(),
                            AcOwner = row["AcOwner"].ToString(),
                            IsPrimary = row["IsPrimary"].ToString(),
                            AcType = row["AcType"].ToString(),
                            TxnEnabled = row["TxnEnabled"].ToString(),
                            TBranchCode = row["TBranchCode"].ToString()
                        });
                    }

                }
                regobj.Trans = lst;


                ViewBag.ProfileList = new SelectList(ItemProfile, "Value", "Text", regobj.ProfileName);

                ViewBag.Province = new SelectList(new List<SelectListItem>(), "Value", "Text", regobj.Province);


                ViewBag.District = new SelectList(new List<SelectListItem>(), "Value", "Text", regobj.District);

                //ViewBag.AdminProfileName = new SelectList(itemProfile, "Value", "Text");
                return View(regobj);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        public ActionResult ViewRegisteredDetail(string id)
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
            //ViewBag.Province = list;


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

                ViewBag.Bank = item;
                List<MNCustProfile> ProfileList = new List<MNCustProfile>();
                List<SelectListItem> ItemProfile = new List<SelectListItem>();
                ProfileList = CusProfileUserModel.GetMNCustProfile().Where(x => x.ProfileStatus == 'A').ToList();

                foreach (MNCustProfile Cust in ProfileList)
                {
                    ItemProfile.Add(new SelectListItem
                    {
                        Text = Cust.ProfileCode,
                        Value = Cust.ProfileCode

                    });
                }

                UserInfo regobj = new UserInfo();
                DataSet DSet = ProfileUtils.GetUserProfileInfoDS(id);
                DataTable dtableUserInfo = DSet.Tables["dtUserInfo"];
                DataTable dTranstable = DSet.Tables["dtTransaction"];

                if (dtableUserInfo != null && dtableUserInfo.Rows.Count > 0)
                {
                    regobj.ClientCode = dtableUserInfo.Rows[0]["ClientCode"].ToString();
                    regobj.Name = dtableUserInfo.Rows[0]["Name"].ToString();
                    //regobj.FName = dtableUserInfo.Rows[0]["FName"].ToString();
                    //regobj.MName = dtableUserInfo.Rows[0]["MName"].ToString();
                    //regobj.LName = dtableUserInfo.Rows[0]["LName"].ToString();
                    regobj.ClientStatus = dtableUserInfo.Rows[0]["ClientStatus"].ToString();
                    //regobj.Gender = dtableUserInfo.Rows[0]["Gender"].ToString();

                    regobj.Address = dtableUserInfo.Rows[0]["Address"].ToString();
                    regobj.WardNumber = dtableUserInfo.Rows[0]["WardNumber"].ToString();
                    regobj.Province = dtableUserInfo.Rows[0]["Province"].ToString();
                    regobj.District = dtableUserInfo.Rows[0]["District"].ToString();

                    regobj.PIN = dtableUserInfo.Rows[0]["PIN"].ToString();
                    regobj.Status = dtableUserInfo.Rows[0]["Status"].ToString();
                    regobj.ContactNumber1 = dtableUserInfo.Rows[0]["ContactNumber1"].ToString();
                    regobj.ContactNumber2 = dtableUserInfo.Rows[0]["ContactNumber2"].ToString();
                    regobj.UserName = dtableUserInfo.Rows[0]["UserName"].ToString();
                    regobj.UserType = dtableUserInfo.Rows[0]["userType"].ToString();
                    //regobj.ProfileName = dtableUserInfo.Rows[0]["ProfileName"].ToString();
                    regobj.ProfileCode = dtableUserInfo.Rows[0]["ProfileCode"].ToString();
                    regobj.IsApproved = dtableUserInfo.Rows[0]["IsApproved"].ToString();
                    regobj.IsRejected = dtableUserInfo.Rows[0]["IsRejected"].ToString();
                    regobj.WalletNumber = dtableUserInfo.Rows[0]["WalletNumber"].ToString();
                    regobj.EmailAddress = dtableUserInfo.Rows[0]["EmailAddress"].ToString();
                    regobj.NewMobileNo = dtableUserInfo.Rows[0]["NewMobileNo"].ToString();

                    regobj.BankNo = dtableUserInfo.Rows[0]["BankNo"].ToString();
                    regobj.BranchCode = dtableUserInfo.Rows[0]["BranchCode"].ToString();
                    regobj.BankAccountNumber = dtableUserInfo.Rows[0]["BankAccountNumber"].ToString();
                    regobj.COC = dtableUserInfo.Rows[0]["COC"].ToString();


                    regobj.Transaction = dtableUserInfo.Rows[0]["IndvTxn"].ToString();
                    regobj.DateRange = dtableUserInfo.Rows[0]["DateRange"].ToString();
                    regobj.StartDate = dtableUserInfo.Rows[0]["StartDate"].ToString();
                    regobj.EndDate = dtableUserInfo.Rows[0]["EndDate"].ToString();
                    regobj.LimitType = dtableUserInfo.Rows[0]["LimitType"].ToString();
                    regobj.TransactionCount = dtableUserInfo.Rows[0]["TransactionCount"].ToString();
                    regobj.TransactionLimitDaily = dtableUserInfo.Rows[0]["TransactionLimitDaily"].ToString();
                    regobj.TransactionLimitMonthly = dtableUserInfo.Rows[0]["TransactionLimitMonthly"].ToString();
                    regobj.TransactionLimit = dtableUserInfo.Rows[0]["TransactionLimit"].ToString();

                    regobj.SelfRegistered = dtableUserInfo.Rows[0]["SelfRegistered"].ToString();


                }

                List<TransactionInfo> lst = new List<TransactionInfo>();

                if (dTranstable != null && dTranstable.Rows.Count > 0)
                {
                    foreach (DataRow row in dTranstable.Rows)
                    {
                        lst.Add(new TransactionInfo
                        {

                            AcNumber = row["AcNumber"].ToString(),
                            Alias = row["Alias"].ToString(),
                            AcOwner = row["AcOwner"].ToString(),
                            IsPrimary = row["IsPrimary"].ToString(),
                            AcType = row["AcType"].ToString(),
                            TxnEnabled = row["TxnEnabled"].ToString(),
                            TBranchCode = row["TBranchCode"].ToString()
                        });
                    }

                }
                regobj.Trans = lst;


                ViewBag.ProfileList = new SelectList(ItemProfile, "Value", "Text", regobj.ProfileName);

                ViewBag.Province = new SelectList(list, "Value", "Text", regobj.Province);

                string districtstring = "select * from MNDistrict WHERE ProvinceID = " + regobj.Province;
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

                ViewBag.District = new SelectList(list1, "Value", "Text", regobj.District);

                //ViewBag.AdminProfileName = new SelectList(itemProfile, "Value", "Text");
                return View(regobj);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        //3
        public JsonResult GetDistrictApprove(int id)
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
            return Json(new SelectList(list, "Value", "Text"));

        }

        public string getProviceApprove(int id)
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

        /// <summary>
        /// 3
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ViewModified(string Key, string Value)
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
                DataTable dtblCus = CustomerUtils.GetModifiedCustomer("user");
                ViewBag.Value = Value;
                bool COC = Session["COC"] == null ? false : (bool)Session["COC"];
                string UserBranchCode = (string)Session["UserBranch"];
                if (Key == "Mobile")
                {
                    if (!string.IsNullOrEmpty(Value))
                    {
                        EnumerableRowCollection<DataRow> row;
                        if (COC)
                        {
                            row = dtblCus.AsEnumerable().Where(r => r.Field<string>("UserName") == Value);
                        }
                        else
                        {
                            row = dtblCus.AsEnumerable().Where(r => r.Field<string>("UserName") == Value).Where(r => r.Field<string>("ModifyingBranch") == UserBranchCode);
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
                        row = dtblCus.AsEnumerable().Where(r => r.Field<string>("ModifyingBranch") == UserBranchCode);
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
                    foreach (DataRow row in dtblUnapproveCus.Rows)
                    {
                        custUnApproveObj.Add(new UserInfo
                        {
                            ClientCode = row["ClientCode"].ToString(),
                            ContactNumber1 = row["ContactNumber1"].ToString(),
                            Name = row["Name"].ToString(),
                            Address = row["Address"].ToString(),
                            BankAccountNumber = row["BankAccountNumber"].ToString(),
                            ModifyingBranch = row["BranchName"].ToString(),
                            ModifyingAdmin = row["ModifiedBy"].ToString()

                        });
                    }
                }

                return View(custUnApproveObj);

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        public ActionResult ViewModifyDetail(string id)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

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

                if (string.IsNullOrEmpty(id))
                {
                    this.TempData["custapprove_messsage"] = "User Not found";
                    this.TempData["message_class"] = CssSetting.FailedMessageClass;
                    return RedirectToAction("ViewModified");
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

                ViewBag.Bank = item;
                List<MNCustProfile> ProfileList = new List<MNCustProfile>();
                List<SelectListItem> ItemProfile = new List<SelectListItem>();
                ProfileList = CusProfileUserModel.GetMNCustProfile().Where(x => x.ProfileStatus == 'A').ToList();

                foreach (MNCustProfile Cust in ProfileList)
                {
                    ItemProfile.Add(new SelectListItem
                    {
                        Text = Cust.ProfileCode,
                        Value = Cust.ProfileCode

                    });
                }

                UserInfo regobj = new UserInfo();
                DataSet DSet = ProfileUtils.GetUserProfileInfoDS(id.ToString());

                DataTable dtableUserInfo = DSet.Tables["dtUserInfo"];
                DataTable dTranstable = DSet.Tables["dtTransaction"];
                DataTable dtUserKYC = DSet.Tables["dtUserKYC"]; //NISCHAL
                DataTable dtUserKYCDoc = DSet.Tables["dtUserKYCDoc"]; //NISCHAL


                if (dtableUserInfo != null && dtableUserInfo.Rows.Count > 0)
                {
                    regobj.ClientCode = dtableUserInfo.Rows[0]["ClientCode"].ToString();
                    regobj.Name = dtableUserInfo.Rows[0]["Name"].ToString();
                    //regobj.FName = dtableUserInfo.Rows[0]["FName"].ToString();
                    //regobj.MName = dtableUserInfo.Rows[0]["MName"].ToString();
                    //regobj.LName = dtableUserInfo.Rows[0]["LName"].ToString();
                    regobj.ClientStatus = dtableUserInfo.Rows[0]["ClientStatus"].ToString();
                    //regobj.Gender = dtableUserInfo.Rows[0]["Gender"].ToString();


                    //start milayako 02
                    regobj.PStreet = dtableUserInfo.Rows[0]["PStreet"].ToString();
                    regobj.PProvince = getProvinceName(dtableUserInfo.Rows[0]["PProvince"].ToString());
                    regobj.PDistrictID = getDistrictName(dtableUserInfo.Rows[0]["PDistrictID"].ToString());
                    regobj.PMunicipalityVDC = dtableUserInfo.Rows[0]["PMunicipalityVDC"].ToString();
                    regobj.PHouseNo = dtableUserInfo.Rows[0]["PHouseNo"].ToString();
                    regobj.PWardNo = dtableUserInfo.Rows[0]["PWardNo"].ToString();

                    regobj.CStreet = dtableUserInfo.Rows[0]["CStreet"].ToString();
                    regobj.CProvince = getProvinceName(dtableUserInfo.Rows[0]["CProvince"].ToString());
                    regobj.CDistrictID = getDistrictName(dtableUserInfo.Rows[0]["CDistrictID"].ToString());
                    regobj.CMunicipalityVDC = dtableUserInfo.Rows[0]["CMunicipalityVDC"].ToString();
                    regobj.CHouseNo = dtableUserInfo.Rows[0]["CHouseNo"].ToString();
                    regobj.CWardNo = dtableUserInfo.Rows[0]["CWardNo"].ToString();
                    //end milayako 02

                    //NISCHAL
                    regobj.Gender = dtUserKYC.Rows[0]["Gender"].ToString();
                    regobj.Nationality = dtUserKYC.Rows[0]["Nationality"].ToString();

                    regobj.DOB = DateTime.Parse(dtUserKYC.Rows[0]["DateOfBirth"].ToString()).ToString("dd/MM/yyyy");
                    regobj.BSDateOfBirth = dtUserKYC.Rows[0]["BSDateOfBirth"].ToString();

                    regobj.Country = dtUserKYC.Rows[0]["CountryCode"].ToString();
                    regobj.FatherName = dtUserKYC.Rows[0]["FathersName"].ToString();
                    regobj.MotherName = dtUserKYC.Rows[0]["MothersName"].ToString();
                    regobj.GrandFatherName = dtUserKYC.Rows[0]["GFathersName"].ToString();
                    regobj.Occupation = dtUserKYC.Rows[0]["Occupation"].ToString();
                    regobj.MaritalStatus = dtUserKYC.Rows[0]["MaritalStatus"].ToString();
                    regobj.SpouseName = dtUserKYC.Rows[0]["SpouseName"].ToString();
                    regobj.FatherInlawName = dtUserKYC.Rows[0]["FatherInLaw"].ToString();
                    regobj.Document = dtUserKYCDoc.Rows[0]["DocType"].ToString();
                    regobj.Citizenship = dtUserKYC.Rows[0]["CitizenshipNo"].ToString();
                    regobj.CitizenshipIssueDate = DateTime.Parse(dtUserKYC.Rows[0]["CitizenIssueDate"].ToString()).ToString("dd/MM/yyyy");
                    regobj.BSCitizenshipIssueDate = dtUserKYC.Rows[0]["BSCitizenIssueDate"].ToString();

                    regobj.CitizenshipPlaceOfIssue = dtUserKYC.Rows[0]["CitizenPlaceOfIssue"].ToString();
                    regobj.Passport = dtUserKYC.Rows[0]["PassportNo"].ToString();
                    regobj.PassportPlaceOfIssue = dtUserKYC.Rows[0]["PassportPlaceOfIssue"].ToString();
                    regobj.PassportIssueDate = DateTime.Parse(dtUserKYC.Rows[0]["PassportIssueDate"].ToString()).ToString("dd/MM/yyyy");
                    regobj.PassportExpireDate = DateTime.Parse(dtUserKYC.Rows[0]["PassportExpiryDate"].ToString()).ToString("dd/MM/yyyy");
                    regobj.License = dtUserKYC.Rows[0]["LicenseNo"].ToString();
                    regobj.LicensePlaceOfIssue = dtUserKYC.Rows[0]["LicensePlaceOfIssue"].ToString();
                    regobj.LicenseIssueDate = DateTime.Parse(dtUserKYC.Rows[0]["LicenseIssueDate"].ToString()).ToString("dd/MM/yyyy");
                    regobj.BSLicenseIssueDate = dtUserKYC.Rows[0]["BSLicenseIssueDate"].ToString();

                    regobj.LicenseExpireDate = DateTime.Parse(dtUserKYC.Rows[0]["LicenseExpiryDate"].ToString()).ToString("dd/MM/yyyy");
                    regobj.BSLicenseExpireDate = dtUserKYC.Rows[0]["BSLicenseExpiryDate"].ToString();

                    regobj.FrontImage = dtUserKYCDoc.Rows[0]["FrontImage"].ToString();
                    regobj.BackImage = dtUserKYCDoc.Rows[0]["BackImage"].ToString();
                    regobj.PassportImage = dtUserKYCDoc.Rows[0]["PassportImage"].ToString();
                    //NISCHAL---END

                    //start delete garnu parne 02
                    regobj.Address = dtableUserInfo.Rows[0]["Address"].ToString();
                    //end delete garnu parne 02


                    regobj.PIN = dtableUserInfo.Rows[0]["PIN"].ToString();
                    regobj.Status = dtableUserInfo.Rows[0]["Status"].ToString();
                    regobj.ContactNumber1 = dtableUserInfo.Rows[0]["ContactNumber1"].ToString();
                    regobj.ContactNumber2 = dtableUserInfo.Rows[0]["ContactNumber2"].ToString();
                    regobj.UserName = dtableUserInfo.Rows[0]["UserName"].ToString();
                    regobj.UserType = dtableUserInfo.Rows[0]["userType"].ToString();
                    //regobj.ProfileName = dtableUserInfo.Rows[0]["ProfileName"].ToString();
                    regobj.ProfileCode = dtableUserInfo.Rows[0]["ProfileCode"].ToString();
                    regobj.IsApproved = dtableUserInfo.Rows[0]["IsApproved"].ToString();
                    regobj.IsRejected = dtableUserInfo.Rows[0]["IsRejected"].ToString();
                    regobj.IsModified = dtableUserInfo.Rows[0]["IsModified"].ToString();
                    regobj.WalletNumber = dtableUserInfo.Rows[0]["WalletNumber"].ToString();
                    regobj.EmailAddress = dtableUserInfo.Rows[0]["EmailAddress"].ToString();
                    regobj.NewMobileNo = dtableUserInfo.Rows[0]["NewMobileNo"].ToString();

                    regobj.BankNo = dtableUserInfo.Rows[0]["BankNo"].ToString();
                    regobj.BranchCode = dtableUserInfo.Rows[0]["BranchCode"].ToString();
                    regobj.BankAccountNumber = dtableUserInfo.Rows[0]["BankAccountNumber"].ToString();
                    regobj.COC = dtableUserInfo.Rows[0]["COC"].ToString();

                    /////
                    regobj.Transaction = dtableUserInfo.Rows[0]["IndvTxn"].ToString();
                    regobj.DateRange = dtableUserInfo.Rows[0]["DateRange"].ToString();
                    regobj.StartDate = dtableUserInfo.Rows[0]["StartDate"].ToString();
                    regobj.EndDate = dtableUserInfo.Rows[0]["EndDate"].ToString();

                    regobj.LimitType = dtableUserInfo.Rows[0]["LimitType"].ToString();
                    regobj.TransactionCount = dtableUserInfo.Rows[0]["TransactionCount"].ToString();
                    regobj.TransactionLimitDaily = dtableUserInfo.Rows[0]["TransactionLimitDaily"].ToString();
                    regobj.TransactionLimitMonthly = dtableUserInfo.Rows[0]["TransactionLimitMonthly"].ToString();
                    regobj.TransactionLimit = dtableUserInfo.Rows[0]["TransactionLimit"].ToString();


                    if (regobj.UserType.ToUpper() != "USER" || regobj.IsModified != "T" || regobj.IsApproved != "UnApprove" || regobj.IsRejected != "F")
                    {
                        this.TempData["custapprove_messsage"] = "User Not found";
                        this.TempData["message_class"] = CssSetting.FailedMessageClass;
                        return RedirectToAction("ViewModified");
                    }
                    /*
                                        WHERE UserType = @UserType AND IsNull(IsModified,'F')= 'T' AND IsApproved = 'UnApprove'*/

                    ////
                }
                else
                {
                    this.TempData["custapprove_messsage"] = "User Not found";
                    this.TempData["message_class"] = CssSetting.FailedMessageClass;
                    return RedirectToAction("ViewModified");
                }
                List<TransactionInfo> lst = new List<TransactionInfo>();

                if (dTranstable != null && dTranstable.Rows.Count > 0)
                {
                    foreach (DataRow row in dTranstable.Rows)
                    {
                        lst.Add(new TransactionInfo
                        {

                            AcNumber = row["AcNumber"].ToString(),
                            Alias = row["Alias"].ToString(),
                            AcOwner = row["AcOwner"].ToString(),
                            IsPrimary = row["IsPrimary"].ToString(),
                            AcType = row["AcType"].ToString(),
                            TxnEnabled = row["TxnEnabled"].ToString(),
                            TBranchCode = row["TBranchCode"].ToString()
                        });
                    }

                }
                regobj.Trans = lst;
                ViewBag.ProfileList = new SelectList(ItemProfile, "Value", "Text", regobj.ProfileName);

                //Get chaned Values
                DataSet DSetMakerchecker = ProfileUtils.GetCustModifiedValue(id);
                List<MNMakerChecker> ModifiedValues = ExtraUtility.DatatableToListClass<MNMakerChecker>(DSetMakerchecker.Tables["MNMakerChecker"]);
                List<InMemMNTransactionAccount> ModifiedTransAccs = ExtraUtility.DatatableToListClass<InMemMNTransactionAccount>(DSetMakerchecker.Tables["InMemMNTransactionAccount"]);
                regobj.MakerChecker = ModifiedValues;
                regobj.MakerCheckerTransAccounts = ModifiedTransAccs.ToList();


                var Inserted = ModifiedTransAccs.Where(x => x.Action == "I").ToList();
                var Deleted = ModifiedTransAccs.Where(x => x.Action == "D").ToList();
                HashSet<string> insertedMainCodes = new HashSet<string>(Inserted.Select(x => x.AcNumber));




                //ViewBag.AdminProfileName = new SelectList(itemProfile, "Value", "Text");

                return View(regobj);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }



        [HttpPost]
        public ActionResult ApproveModify(UserInfo model, string btnApprove)
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
                    userInfoApproval.Remarks = model.Remarks;
                    userInfoApproval.AdminUserName = (string)Session["UserName"];
                    userInfoApproval.AdminBranch = (string)Session["UserBranch"];

                    bool rejected = CustomerUtils.RejectCustModified(userInfoApproval);
                    if (rejected)
                    {
                        displayMessage = "Customer " + model.Name + " has been Rejected. Please Check Rejectlist and perform accordingly";
                        messageClass = CssSetting.SuccessMessageClass;
                    }
                    else
                    {
                        displayMessage = "Error while rejecting Customer " + model.Name;
                        messageClass = CssSetting.FailedMessageClass;
                    }

                    this.TempData["custapprove_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;

                    return RedirectToAction("ViewModified");

                }
                else if (btnApprove.ToUpper() == "APPROVE")
                {
                    userInfoApproval.AdminUserName = (string)Session["UserName"];
                    userInfoApproval.AdminBranch = (string)Session["UserBranch"];
                    bool Approved = CustomerUtils.ApproveCustModified(userInfoApproval);
                    if (Approved)
                    {
                        displayMessage = "modification Information for  user " + model.Name + " has successfully been approved.";
                        messageClass = CssSetting.SuccessMessageClass;
                    }
                    else
                    {
                        displayMessage = "Error while approving Customer Information";
                        messageClass = CssSetting.FailedMessageClass;
                    }

                    this.TempData["custapprove_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;

                    return RedirectToAction("ViewModified");
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

        [HttpGet]
        public ActionResult ViewRejected(string id)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            //start milayako 02
            string provincestring2 = "select * from MNProvince";
            DataTable dt2 = new DataTable();
            dt2 = objdal.MyMethod(provincestring2);
            List<SelectListItem> list2 = new List<SelectListItem>();
            foreach (DataRow row in dt2.Rows)
            {
                list2.Add(new SelectListItem
                {
                    Text = Convert.ToString(row.ItemArray[1]),
                    Value = Convert.ToString(row.ItemArray[0])
                });

            }
            ViewBag.PProvince = list2;

            ViewBag.CProvince = list2;
            //end milayako 02 

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

                //FOR Bank
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

                //TEST

                List<MNCustProfile> ProfileList = new List<MNCustProfile>();
                List<SelectListItem> ItemProfile = new List<SelectListItem>();
                ProfileList = CusProfileUserModel.GetMNCustProfile().Where(x => x.ProfileStatus == 'A').ToList();

                foreach (MNCustProfile Cust in ProfileList)
                {
                    ItemProfile.Add(new SelectListItem
                    {
                        Text = Cust.ProfileCode,
                        Value = Cust.ProfileCode

                    });
                }

                UserInfo regobj = new UserInfo();
                DataSet DSet = ProfileUtils.GetUserProfileInfoDS(id.ToString());
                DataTable dtableUserInfo = DSet.Tables["dtUserInfo"];
                DataTable dTranstable = DSet.Tables["dtTransaction"];
                DataTable dtUserKYC = DSet.Tables["dtUserKYC"];
                DataTable dtUserKYCDoc = DSet.Tables["dtUserKYCDoc"];

                if (dtableUserInfo != null && dtableUserInfo.Rows.Count > 0)
                {
                    regobj.ClientCode = dtableUserInfo.Rows[0]["ClientCode"].ToString();
                    regobj.Name = dtableUserInfo.Rows[0]["Name"].ToString();
                    //regobj.FName = dtableUserInfo.Rows[0]["FName"].ToString().Capitalize();
                    //regobj.MName = dtableUserInfo.Rows[0]["MName"].ToString().Capitalize();
                    //regobj.LName = dtableUserInfo.Rows[0]["LName"].ToString().Capitalize();
                    regobj.ClientStatus = dtableUserInfo.Rows[0]["ClientStatus"].ToString();
                    ////regobj.Gender = dtableUserInfo.Rows[0]["Gender"].ToString();

                    //start milayako 02
                    regobj.PStreet = dtableUserInfo.Rows[0]["PStreet"].ToString();
                    regobj.PProvince = dtableUserInfo.Rows[0]["PProvince"].ToString();
                    regobj.PDistrictID = dtableUserInfo.Rows[0]["PDistrictID"].ToString();
                    regobj.PMunicipalityVDC = dtableUserInfo.Rows[0]["PMunicipalityVDC"].ToString();
                    regobj.PHouseNo = dtableUserInfo.Rows[0]["PHouseNo"].ToString();
                    regobj.PWardNo = dtableUserInfo.Rows[0]["PWardNo"].ToString();

                    regobj.CStreet = dtableUserInfo.Rows[0]["CStreet"].ToString();
                    regobj.CProvince = dtableUserInfo.Rows[0]["CProvince"].ToString();
                    regobj.CDistrictID = dtableUserInfo.Rows[0]["CDistrictID"].ToString();
                    regobj.CMunicipalityVDC = dtableUserInfo.Rows[0]["CMunicipalityVDC"].ToString();
                    regobj.CHouseNo = dtableUserInfo.Rows[0]["CHouseNo"].ToString();
                    regobj.CWardNo = dtableUserInfo.Rows[0]["CWardNo"].ToString();


                    regobj.Gender = dtUserKYC.Rows[0]["Gender"].ToString();
                    regobj.Nationality = dtUserKYC.Rows[0]["Nationality"].ToString();

                    regobj.DOB = DateTime.Parse(dtUserKYC.Rows[0]["DateOfBirth"].ToString()).ToString("dd/MM/yyyy");
                    regobj.BSDateOfBirth = dtUserKYC.Rows[0]["BSDateOfBirth"].ToString();
                    regobj.Country = dtUserKYC.Rows[0]["CountryCode"].ToString();
                    regobj.FatherName = dtUserKYC.Rows[0]["FathersName"].ToString();
                    regobj.MotherName = dtUserKYC.Rows[0]["MothersName"].ToString();
                    regobj.GrandFatherName = dtUserKYC.Rows[0]["GFathersName"].ToString();
                    regobj.Occupation = dtUserKYC.Rows[0]["Occupation"].ToString();
                    regobj.MaritalStatus = dtUserKYC.Rows[0]["MaritalStatus"].ToString();
                    regobj.SpouseName = dtUserKYC.Rows[0]["SpouseName"].ToString();
                    regobj.FatherInlawName = dtUserKYC.Rows[0]["FatherInLaw"].ToString();
                    regobj.Document = dtUserKYCDoc.Rows[0]["DocType"].ToString();
                    regobj.Citizenship = dtUserKYC.Rows[0]["CitizenshipNo"].ToString();
                    regobj.CitizenshipIssueDate = DateTime.Parse(dtUserKYC.Rows[0]["CitizenIssueDate"].ToString()).ToString("dd/MM/yyyy");
                    regobj.BSCitizenshipIssueDate = dtUserKYC.Rows[0]["BSCitizenIssueDate"].ToString();

                    regobj.CitizenshipPlaceOfIssue = dtUserKYC.Rows[0]["CitizenPlaceOfIssue"].ToString();
                    regobj.Passport = dtUserKYC.Rows[0]["PassportNo"].ToString();
                    regobj.PassportPlaceOfIssue = dtUserKYC.Rows[0]["PassportPlaceOfIssue"].ToString();
                    regobj.PassportIssueDate = DateTime.Parse(dtUserKYC.Rows[0]["PassportIssueDate"].ToString()).ToString("dd/MM/yyyy");
                    regobj.PassportExpireDate = DateTime.Parse(dtUserKYC.Rows[0]["PassportExpiryDate"].ToString()).ToString("dd/MM/yyyy");
                    regobj.License = dtUserKYC.Rows[0]["LicenseNo"].ToString();
                    regobj.LicensePlaceOfIssue = dtUserKYC.Rows[0]["LicensePlaceOfIssue"].ToString();
                    regobj.LicenseIssueDate = DateTime.Parse(dtUserKYC.Rows[0]["LicenseIssueDate"].ToString()).ToString("dd/MM/yyyy");
                    regobj.BSLicenseIssueDate = dtUserKYC.Rows[0]["BSLicenseIssueDate"].ToString();

                    regobj.LicenseExpireDate = DateTime.Parse(dtUserKYC.Rows[0]["LicenseExpiryDate"].ToString()).ToString("dd/MM/yyyy");
                    regobj.BSLicenseExpireDate = dtUserKYC.Rows[0]["BSLicenseExpiryDate"].ToString();

                    regobj.FrontImage = dtUserKYCDoc.Rows[0]["FrontImage"].ToString();
                    regobj.BackImage = dtUserKYCDoc.Rows[0]["BackImage"].ToString();
                    regobj.PassportImage = dtUserKYCDoc.Rows[0]["PassportImage"].ToString();
                    Session["PassportImage"] = regobj.PassportImage;
                    Session["FrontImage"] = regobj.FrontImage;
                    Session["BackImage"] = regobj.BackImage;

                    //NISCHAL---END
                    //end milayako 02
                    //start delete garnu parne 02
                    regobj.Address = dtableUserInfo.Rows[0]["Address"].ToString();
                    //end delete garnu parne 02

                    regobj.PIN = dtableUserInfo.Rows[0]["PIN"].ToString();
                    regobj.Status = dtableUserInfo.Rows[0]["Status"].ToString();
                    regobj.ContactNumber1 = dtableUserInfo.Rows[0]["ContactNumber1"].ToString();
                    regobj.ContactNumber2 = dtableUserInfo.Rows[0]["ContactNumber2"].ToString();
                    regobj.UserName = dtableUserInfo.Rows[0]["UserName"].ToString();
                    regobj.UserType = dtableUserInfo.Rows[0]["userType"].ToString();
                    //regobj.ProfileName = dtableUserInfo.Rows[0]["ProfileName"].ToString();
                    regobj.ProfileCode = dtableUserInfo.Rows[0]["ProfileCode"].ToString();
                    regobj.IsApproved = dtableUserInfo.Rows[0]["IsApproved"].ToString();
                    regobj.IsRejected = dtableUserInfo.Rows[0]["IsRejected"].ToString();
                    regobj.WalletNumber = dtableUserInfo.Rows[0]["WalletNumber"].ToString();
                    regobj.EmailAddress = dtableUserInfo.Rows[0]["EmailAddress"].ToString();
                    regobj.NewMobileNo = dtableUserInfo.Rows[0]["NewMobileNo"].ToString();

                    regobj.BankNo = dtableUserInfo.Rows[0]["BankNo"].ToString();
                    regobj.BranchCode = dtableUserInfo.Rows[0]["BranchCode"].ToString();
                    regobj.BankAccountNumber = dtableUserInfo.Rows[0]["BankAccountNumber"].ToString();
                    regobj.COC = dtableUserInfo.Rows[0]["COC"].ToString();
                    regobj.Remarks = dtableUserInfo.Rows[0]["Remarks"].ToString();
                    //added//
                    regobj.StartDate = dtableUserInfo.Rows[0]["StartDate"].ToString();
                    regobj.EndDate = dtableUserInfo.Rows[0]["EndDate"].ToString();
                    regobj.Transaction = dtableUserInfo.Rows[0]["IndvTxn"].ToString();
                    regobj.DateRange = dtableUserInfo.Rows[0]["DateRange"].ToString();
                    regobj.TransactionLimit = dtableUserInfo.Rows[0]["TransactionLimit"].ToString();
                    regobj.LimitType = dtableUserInfo.Rows[0]["LimitType"].ToString();
                    regobj.TransactionCount = dtableUserInfo.Rows[0]["TransactionCount"].ToString();
                    regobj.TransactionLimitDaily = dtableUserInfo.Rows[0]["TransactionLimitDaily"].ToString();
                    regobj.TransactionLimitMonthly = dtableUserInfo.Rows[0]["TransactionLimitMonthly"].ToString();


                }

                List<TransactionInfo> lst = new List<TransactionInfo>();

                if (dTranstable != null && dTranstable.Rows.Count > 0)
                {
                    foreach (DataRow row in dTranstable.Rows)
                    {
                        lst.Add(new TransactionInfo
                        {

                            AcNumber = row["AcNumber"].ToString(),
                            Alias = row["Alias"].ToString(),
                            AcOwner = row["AcOwner"].ToString(),
                            IsPrimary = row["IsPrimary"].ToString(),
                            AcType = row["AcType"].ToString(),
                            TxnEnabled = row["TxnEnabled"].ToString(),
                            TBranchCode = row["TBranchCode"].ToString()
                        });
                    }

                }
                regobj.Trans = lst;
                ViewBag.ProfileList = new SelectList(ItemProfile, "Value", "Text", regobj.ProfileName);
                //ViewBag.AdminProfileName = new SelectList(itemProfile, "Value", "Text");

                // start milayako 02
                ViewBag.PProvince = new SelectList(list2, "Value", "Text", regobj.PProvince);
                ViewBag.CProvince = new SelectList(list2, "Value", "Text", regobj.CProvince);


                //end milayako 02

                // start milayako 02
                ViewBag.PDistrictID = new SelectList(ProvinceToDistrict(regobj.PProvince), "Value", "Text", regobj.PDistrictID);
                ViewBag.CDistrictID = new SelectList(ProvinceToDistrict(regobj.CProvince), "Value", "Text", regobj.CDistrictID);
                // END milayako 02

                return View(regobj);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpPost]
        public async Task<ActionResult> ModifyRejected(string btnCommand, FormCollection collection, UserInfo model)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            string cPPImage = Session["PassportImage"].ToString();
            string cFrontImage = Session["FrontImage"].ToString();
            string cBackImage = Session["BackImage"].ToString();
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
                    string cClientCode = collection["ClientCode"].ToString();
                    //string cAddress = collection["Address"].ToString();
                    string cStatus = collection["Status"].ToString();
                    string cContactNumber1 = collection["ContactNumber1"].ToString();
                    string cContactNumber2 = collection["ContactNumber2"].ToString();
                    //string cWalletNumber = collection["WalletNumber"].ToString();
                    string cEmailAddress = collection["EmailAddress"].ToString();
                    string cNewMobileNo = collection["NewMobileNo"].ToString();
                    //string cFName = collection["FName"].ToString().Capitalize();
                    //string cMName = collection["MName"].ToString().Capitalize();
                    //string cLName = collection["LName"].ToString().Capitalize();
                    string cClientStatus = collection["ClientStatus"].ToString();
                    string cCHouseNo = collection["CHouseNo"].ToString();
                    string cCStreet = collection["CStreet"].ToString();
                    string cCWardNo = collection["CWardNo"].ToString();
                    string cCMunicipalityVDC = collection["CMunicipalityVDC"].ToString();
                    string cCDistrictID = collection["CDistrictID"].ToString();
                    string cCProvince = collection["CProvince"].ToString();

                    string cPHouseNo = collection["PHouseNo"].ToString();
                    string cPStreet = collection["PStreet"].ToString();
                    string cPWardNo = collection["PWardNo"].ToString();
                    string cPMunicipalityVDC = collection["PMunicipalityVDC"].ToString();
                    string cPDistrictID = collection["PDistrictID"].ToString();
                    string cPProvince = collection["PProvince"].ToString();

                    //---NISCHAL
                    string cDOB = collection["DOB"].ToString();
                    string cGender = collection["txtGender"].ToString();
                    string cCountry = collection["txtCountry"].ToString();
                    string cNationality = collection["txtNationality"].ToString();
                    string cFatherName = collection["txtFatherName"].ToString();
                    string cMotherName = collection["txtMotherName"].ToString();
                    string cGrandFatherName = collection["txtGrandFatherName"].ToString();
                    string cOccupation = collection["txtOccupation"].ToString();
                    string cMaritalStatus = collection["txtMaritalStatus"].ToString();
                    string cSpouseName = collection["txtSpouseName"].ToString();
                    string cFatherInLawsName = collection["txtFatherInLawsName"].ToString();

                    string cDocument = collection["txtDocument"].ToString();
                    string cCitizenship = collection["txtCitizenship"].ToString();
                    string cCitizenshipPlaceOfIssue = collection["txtCitizenshipPlaceOfIssue"].ToString();
                    string cCitizenshipIssueDate = collection["CitizenshipIssueDate"].ToString();
                    string cLicense = collection["txtLicense"].ToString();
                    string cLicensePlaceOfIssue = collection["txtLicensePlaceOfIssue"].ToString();
                    string cLicenseIssueDate = collection["LicenseIssueDate"].ToString();
                    string cLicenseExpireDate = collection["LicenseExpireDate"].ToString();
                    string cPassport = collection["txtPassport"].ToString();
                    string cPassportPlaceOfIssue = collection["txtPassportPlaceOfIssue"].ToString();
                    string cPassportIssueDate = collection["PassportIssueDate"].ToString();
                    string cPassportExpireDate = collection["PassportExpireDate"].ToString();
                    //---NISCHAL

                    string cBranchCode = collection["BranchCode"].ToString();
                    string cBankNo = collection["BankNo"].ToString();
                    string cBankAccountNumber = collection["BankAccountNumber"].ToString();
                    string cIsApproved = collection["IsApproved"].ToString();
                    //string cIsRejected = collection["IsRejected"].ToString();
                    string cPIN = collection["PIN"].ToString();
                    //string cGender = collection["Gender"].ToString();
                    //string cProfileName = collection["ProfileName"].ToString();
                    string cProfileCode = collection["ProfileCode"].ToString();
                    string cUserType = collection["UserType"].ToString();
                    string cUserName = collection["UserName"].ToString();
                    string cName = collection["Name"].ToString();

                    //string cCOC = "F";
                    //if (collection.AllKeys.Contains("COC"))
                    //{
                    //    cCOC = collection["COC"].ToString();
                    //}

                    //added//
                    string cTransaction = "Dis";
                    if (collection.AllKeys.Contains("Transaction"))
                    {
                        cTransaction = collection["Transaction"].ToString();
                    }
                    string cDateRange = "", cStartDate = "", cEndDate = "", cTransactionLimit = string.Empty;
                    if (collection.AllKeys.Contains("DateRange"))
                    {
                        cDateRange = collection["DateRange"].ToString();
                        cStartDate = collection["StartDate"].ToString();
                        cEndDate = collection["EndDate"].ToString();

                    }
                    string cLimitType = "", cTransactionCount = "", cTransactionLimitMonthly = "", cTransactionLimitDaily = "";
                    if (collection.AllKeys.Contains("LimitType"))
                    {
                        cLimitType = collection["LimitType"].ToString();
                        cTransactionCount = collection["TransactionCount"].ToString();
                        cTransactionLimitMonthly = collection["TransactionLimitMonthly"].ToString();
                        cTransactionLimitDaily = collection["TransactionLimitDaily"].ToString();
                        cTransactionLimit = collection["TransactionLimit"].ToString();
                    }



                    string cIsRejected = "F";
                    if (collection.AllKeys.Contains("IsRejected"))
                    {
                        cIsRejected = collection["IsRejected"].ToString();
                    }

                    UserInfo userInfoModify = new UserInfo();
                    userInfoModify.ClientCode = cClientCode;
                    //userInfoModify.Address = cAddress;
                    userInfoModify.Status = cStatus;
                    userInfoModify.ClientStatus = cClientStatus;

                    userInfoModify.CProvince = cCProvince;
                    userInfoModify.CDistrictID = cCDistrictID;
                    userInfoModify.CMunicipalityVDC = cCMunicipalityVDC;
                    userInfoModify.CWardNo = cCWardNo;
                    userInfoModify.CHouseNo = cCHouseNo;
                    userInfoModify.CStreet = cCStreet;

                    userInfoModify.PProvince = cPProvince;
                    userInfoModify.PDistrictID = cPDistrictID;
                    userInfoModify.PMunicipalityVDC = cPMunicipalityVDC;
                    userInfoModify.PWardNo = cPWardNo;
                    userInfoModify.PHouseNo = cPHouseNo;
                    userInfoModify.PStreet = cPStreet;
                    //NISCHAL
                    userInfoModify.Gender = cGender;
                    userInfoModify.DOB = cDOB;
                    userInfoModify.Nationality = cNationality;
                    userInfoModify.Country = cCountry;
                    userInfoModify.FatherName = cFatherName;
                    userInfoModify.MotherName = cMotherName;
                    userInfoModify.GrandFatherName = cGrandFatherName;
                    userInfoModify.Occupation = cOccupation;
                    userInfoModify.MaritalStatus = cMaritalStatus;
                    userInfoModify.SpouseName = cSpouseName;
                    userInfoModify.FatherInlawName = cFatherInLawsName;
                    userInfoModify.Document = cDocument;
                    userInfoModify.Citizenship = cCitizenship;
                    userInfoModify.CitizenshipPlaceOfIssue = cCitizenshipPlaceOfIssue;
                    userInfoModify.CitizenshipIssueDate = cCitizenshipIssueDate;
                    userInfoModify.License = cLicense;
                    userInfoModify.LicensePlaceOfIssue = cLicensePlaceOfIssue;
                    userInfoModify.LicenseIssueDate = cLicenseIssueDate;
                    userInfoModify.LicenseExpireDate = cLicenseExpireDate;
                    userInfoModify.Passport = cPassport;
                    userInfoModify.PassportIssueDate = cPassportIssueDate;
                    userInfoModify.PassportExpireDate = cPassportExpireDate;
                    userInfoModify.PassportPlaceOfIssue = cPassportPlaceOfIssue;
                    //end milayako 02


                    userInfoModify.ContactNumber1 = cContactNumber1;
                    userInfoModify.ContactNumber2 = cContactNumber2;

                    if (cBankAccountNumber.Trim() != "")
                        userInfoModify.BranchCode = cBankAccountNumber.Substring(0, 3);
                    else
                        userInfoModify.BranchCode = "";
                    userInfoModify.BankAccountNumber = cBankAccountNumber;
                    userInfoModify.BankNo = cBankNo;
                    userInfoModify.IsApproved = cIsApproved;
                    userInfoModify.IsRejected = cIsRejected;
                    userInfoModify.PIN = cPIN;

                    userInfoModify.ProfileCode = cProfileCode;
                    userInfoModify.UserType = cUserType;
                    userInfoModify.UserName = cUserName;
                    userInfoModify.Name = cName;
                    userInfoModify.EmailAddress = cEmailAddress;
                    userInfoModify.NewMobileNo = cNewMobileNo;
                    //Added//
                    userInfoModify.Transaction = cTransaction;
                    userInfoModify.DateRange = cDateRange;
                    userInfoModify.StartDate = cStartDate;
                    userInfoModify.EndDate = cEndDate;
                    userInfoModify.TransactionLimit = cTransactionLimit;
                    userInfoModify.LimitType = cLimitType;

                    userInfoModify.TransactionCount = cTransactionCount;
                    userInfoModify.TransactionLimitMonthly = cTransactionLimitMonthly;
                    userInfoModify.TransactionLimitDaily = cTransactionLimitDaily;

                    //var PP = SaveAndReturnFileName(model.PassportPhoto, userInfoModify.UserName);
                    //var front = SaveAndReturnFileName(model.Front, userInfoModify.UserName);
                    //var back = SaveAndReturnFileName(model.Back, userInfoModify.UserName);


                    var PP = ReturnFileName(model.PassportPhoto, userInfoModify.UserName);
                    var front = ReturnFileName(model.Front, userInfoModify.UserName);
                    var back = ReturnFileName(model.Back, userInfoModify.UserName);
                    if (PP != null)
                    {
                        userInfoModify.PassportImage = ParseCv(model.PassportPhoto);
                        userInfoModify.PassportImageName = string.Format(PP);
                    }
                    else
                    {
                        userInfoModify.PassportImageName = cPPImage;
                    }
                    if (front != null)
                    {
                        userInfoModify.FrontImage = ParseCv(model.Front);
                        userInfoModify.FrontImageName = string.Format(front);
                    }
                    else
                    {
                        userInfoModify.FrontImageName = cFrontImage;
                    }
                    if (back != null)
                    {
                        userInfoModify.BackImage = ParseCv(model.Back);
                        userInfoModify.BackImageName = string.Format(back);
                    }
                    else
                    {
                        userInfoModify.BackImageName = cBackImage;
                    }
                    await SavePhoto(userInfoModify);
                    string sPP = Session["PP"].ToString();
                    string sFront = Session["Front"].ToString();
                    string sBack = Session["Back"].ToString();

                    if (sPP != null)
                    {
                        userInfoModify.PassportImageName = Session["PP"].ToString();
                    }
                    else
                    {
                        userInfoModify.BackImageName = "";
                    }
                    if (sFront != null)
                    {
                        userInfoModify.FrontImageName = Session["Front"].ToString();
                    }
                    else
                    {
                        userInfoModify.BackImageName = "";
                    }
                    if (sBack != null)
                    {
                        userInfoModify.BackImageName = Session["Back"].ToString();
                    }
                    else
                    {
                        userInfoModify.BackImageName = "";
                    }
                    //userInfoModify.PassportImageName = Session["PP"].ToString();
                    //userInfoModify.FrontImageName = Session["Front"].ToString();
                    //userInfoModify.BackImageName = Session["Back"].ToString();

                    if (userInfoModify.Transaction == "Dis")
                    {
                        userInfoModify.DateRange = "";
                        userInfoModify.StartDate = "";
                        userInfoModify.EndDate = "";
                        userInfoModify.TransactionLimit = "";
                    }

                    if (userInfoModify.LimitType == "U")
                    {
                        userInfoModify.TransactionCount = "";
                        userInfoModify.TransactionLimit = "";
                        userInfoModify.TransactionLimitMonthly = "";
                        userInfoModify.TransactionLimitDaily = "";
                    }
                    userInfoModify.AdminBranch = (string)Session["UserBranch"];
                    userInfoModify.AdminUserName = (string)Session["UserName"];
                    string[] Accounts;
                    List<TransactionInfo> TransAccount = new List<TransactionInfo>();

                    if (!String.IsNullOrEmpty(collection["hidAccounts"].ToString()))
                    {
                        Accounts = collection["hidAccounts"].ToString().Split(',');
                        Accounts = Accounts.Where(x => !string.IsNullOrEmpty(x.Trim())).ToArray();
                        foreach (string Account in Accounts)
                        {
                            TransactionInfo txnfo = new TransactionInfo();
                            txnfo.AcNumber = collection[String.Format("txtAcNumber{0}", Account)].ToString();
                            txnfo.Alias = collection[String.Format("txtAllias{0}", Account)].ToString();
                            txnfo.AcOwner = collection[String.Format("txtAcOwner{0}", Account)].ToString();
                            txnfo.AcType = collection[String.Format("txtAcType{0}", Account)].ToString();
                            txnfo.TBranchCode = collection[String.Format("txtBranchCode{0}", Account)].ToString();
                            if (collection.AllKeys.Contains(String.Format("chkPrimary{0}", Account)))
                            {
                                txnfo.IsPrimary = collection[String.Format("chkPrimary{0}", Account)].ToString();
                            }
                            else
                            {
                                txnfo.IsPrimary = "F";
                            }
                            if (collection.AllKeys.Contains(String.Format("chkTxnEnable{0}", Account)))
                            {
                                txnfo.TxnEnabled = collection[String.Format("chkTxnEnable{0}", Account)].ToString();
                            }
                            else
                            {
                                txnfo.TxnEnabled = "F";
                            }
                            //TransAccount.Add(new TransactionInfo
                            //{
                            //    AcNumber = collection[String.Format("txtAcNumber{0}", Account)].ToString(),
                            //    Alias = collection[String.Format("txtAcAllias{0}", Account)].ToString(),
                            //    AcOwner = collection[String.Format("txtAcOwner{0}", Account)].ToString(),
                            //    AcType = collection[String.Format("txtAcType{0}", Account)].ToString(),
                            //    TBranchCode = collection[String.Format("txtBranchCode{0}", Account)].ToString(),
                            //    if(collection.AllKeys.Contains(String.Format("chkPrimary{0}", Account)))true?"T":"F",
                            //    TxnEnabled=collection.AllKeys.Contains(String.Format("chkTxnEnable{0}", Account)) == true ? "T" : "F"
                            //});
                            if (txnfo.AcNumber != "" || txnfo.AcNumber != string.Empty)
                                TransAccount.Add(txnfo);
                        }

                    }
                    CusProfileUtils cUtil = new CusProfileUtils();
                    string AccXML = cUtil.GetServiceAccountXMLStr(TransAccount);
                    userInfoModify.TxnAccounts = AccXML;

                    if ((cUserName != "") && (cUserType != "") /*&& (cIsRejected != "")*/
                        && (cIsApproved != "") && (cContactNumber1 != ""))
                    {
                        bool isUpdated = CustomerUtils.UpdateRejectedCustomerUserInfo(userInfoModify);
                        displayMessage = isUpdated
                                                 ? "Customer Information has successfully been Updated and Sent for approval."
                                                 : "Error while updating Customer Information";
                        messageClass = isUpdated ? CssSetting.SuccessMessageClass : CssSetting.FailedMessageClass;
                    }
                    else
                    {
                        displayMessage = "Required Field is Empty";
                        messageClass = CssSetting.FailedMessageClass;
                    }
                    this.TempData["custmodify_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;
                    return RedirectToAction("RejectedList");
                }
                return View();

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        #region Customer KYC Form
        [HttpGet]
        public ActionResult CustomerKyc(string Id)
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
            }
            if (string.IsNullOrEmpty(Id))
            {
                return RedirectToAction("");
            }
            KYC kyc = new KYC();
            kyc.ClientCode = Id;
            for (int i = 0; i < 3; i++)
            {
                kyc.Occupations.Add(new Occupation());
            }
            return View(kyc);
        }

        [HttpPost]
        public ActionResult CustomerKyc(KYC Kyc, HttpPostedFileBase IdenitificationImage, HttpPostedFileBase Photo)
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
            }
            var path = Server.MapPath("~/ClientContent/" + Kyc.ClientCode);
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path); //Create directory if it doesn't exist
            }
            if (IdenitificationImage != null)
            {
                string IdName = IdenitificationImage.FileName;
                var ext = IdName.Substring(IdName.LastIndexOf('.'));
                string IdImage = Kyc.ClientCode + "-IdImage-" + IdName;
                string IdPath = System.IO.Path.Combine(path, IdImage);
                IdenitificationImage.SaveAs(IdPath);
            }
            if (Photo != null)
            {
                string PhotoName = Photo.FileName;
                var Photoext = PhotoName.Substring(PhotoName.LastIndexOf('.'));
                string IdPhoto = Kyc.ClientCode + "-IdPhoto-" + PhotoName;
                string IdPath = System.IO.Path.Combine(path, IdPhoto);
                Photo.SaveAs(IdPath);
            }
            return View(Kyc);
        }

        #endregion

        public ActionResult Renew(string UserName)
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


                if (TempData["Custmessage"] != null)
                {
                    ViewData["Custmessage"] = TempData["Custmessage"];
                    ViewData["messageclass"] = TempData["messageclass"];
                }
                //var model=CustomerUtils.GetUserProfileByName()
                // CustomerUserModel model = new CustomerUserModel();

                //var displayModel = model.GetCustCharge(Id);
                //return View(displayModel);
                ViewBag.MobileNumber = UserName;
                List<UserInfo> users = new List<UserInfo>();
                users = CustomerUtils.GetRejectedUser(UserName);
                return View(users);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }

        [HttpGet]
        public ActionResult RenewCust(string Id)
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

                var usermodel = CustomerUtils.GetCustCharge(Id);
                if (usermodel != null)
                {
                    if (usermodel.UserInfo.Status.ToUpper() != "EXPIRED")
                    {
                        this.TempData["Custmessage"] = "The customer's service period has not yet expired.";
                        this.TempData["messageclass"] = CssSetting.FailedMessageClass;
                        return RedirectToAction("Renew", "Customer");

                    }
                    else
                    {
                        return View(usermodel);
                    }
                }
                else
                {
                    this.TempData["Custmessage"] = "Unable to fetch the customer data.";
                    this.TempData["messageclass"] = CssSetting.FailedMessageClass;
                    return RedirectToAction("Renew", "Customer");

                }

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpPost]
        public async Task<ActionResult> RenewCust(ChargeVM model)
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

                var displayModel = CustomerUtils.GetCustCharge(model.UserInfo.ClientCode);
                displayModel.ChargeReg = model.ChargeReg;
                displayModel.ChargeRenew = model.ChargeRenew;
                displayModel.ChargePinReset = model.ChargePinReset;
                decimal RegChargeAmt = 0, RenewChargeAmt = 0, PinResetChargeAmt = 0;

                //string ChargeDescription = "";
                //if (displayModel.IsCharged)
                //{
                //    //Error message
                //    this.ViewData["Custmessage"] = "There was problem deducting customer charge.Please try again.";
                //    this.ViewData["messageclass"] = CssSetting.FailedMessageClass;
                //    return View(displayModel);
                //}

                decimal Total = RegChargeAmt + RenewChargeAmt + PinResetChargeAmt;
                //var TraceId = DateTime.Now.ToString();
                HttpResponseMessage _res = new HttpResponseMessage();
                TraceIdGenerator _tig = new TraceIdGenerator();
                var tid = _tig.GenerateTraceID();
                string sc = "11";
                string da = "9899999999";
                string src = "http";
                string note = "Renew Charge";
                int amount = (int)Decimal.Parse(displayModel.ProfileCharge.ReNew == "" ? "0" : displayModel.ProfileCharge.ReNew);


                if (CustomerUtils.ChangeCustStaus(displayModel.UserInfo.ClientCode, "Active") == 1)
                {
                    using (HttpClient client = new HttpClient())
                    {

                        var action = "ft/request";
                        var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                        var content = new FormUrlEncodedContent(new[]{
                        new KeyValuePair<string, string>("tid", tid),
                        new KeyValuePair<string,string>("sc",sc),
                        new KeyValuePair<string, string>("mobile",displayModel.UserInfo.UserName),
                        new KeyValuePair<string, string>("amount", amount.ToString()),
                        new KeyValuePair<string,string>("da",da),
                        new KeyValuePair<string,string>("pin",displayModel.UserInfo.PIN),
                        new KeyValuePair<string, string>("note",note),
                        new KeyValuePair<string,string>("src",src)
                    });
                        _res = await client.PostAsync(new Uri(uri), content);

                        if (_res.IsSuccessStatusCode)
                        {

                            if (CustomerUtils.CustRenew(displayModel.UserInfo.ClientCode, displayModel.UserInfo.UserName, amount.ToString(), Session["LOGGED_USERNAME"].ToString(), tid) == 100)
                            {
                                this.TempData["Custmessage"] = "Customer Renewed successfully.";
                                this.TempData["messageclass"] = CssSetting.SuccessMessageClass;
                                return RedirectToAction("Renew", "Customer");
                            }
                            else
                            {
                                this.TempData["Custmessage"] = "Charge was deducted,but there was problem processing the customer. Please retry again.";
                                this.TempData["messageclass"] = CssSetting.FailedMessageClass;
                                return RedirectToAction("Renew", "Customer");
                            }

                        }
                        else
                        {

                            //unable to deduct charge
                            CustomerUtils.ChangeCustStaus(displayModel.UserInfo.ClientCode, "Expired");

                            this.ViewData["Custmessage"] = "There was problem deducting customer charge.Please try again.";
                            this.ViewData["messageclass"] = CssSetting.FailedMessageClass;
                            return View(displayModel);
                        }



                    }
                }
                else
                {
                    this.ViewData["Custmessage"] = "There was problem deducting customer charge.Please try again.";
                    this.ViewData["messageclass"] = CssSetting.FailedMessageClass;
                    return View(displayModel);
                }

                // return RedirectToAction("Renew", "Customer");
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }

        /// <summary>
        /// Customer Charge Deduction and Charge History
        /// </summary>
        //public ActionResult DeductCharge(string Id)
        //{

        //    string userName = (string)Session["LOGGED_USERNAME"];
        //    string clientCode = (string)Session["LOGGEDUSER_ID"];
        //    string name = (string)Session["LOGGEDUSER_NAME"];
        //    string userType = (string)Session["LOGGED_USERTYPE"];
        //    TempData["userType"] = userType;

        //    if (TempData["userType"] != null)
        //    {
        //        this.ViewData["userType"] = this.TempData["userType"];
        //        ViewBag.UserType = this.TempData["userType"];
        //        ViewBag.Name = name;
        //        CustomerUserModel model = new CustomerUserModel();
        //        var displayModel = model.GetCustCharge(Id);
        //        return View(displayModel);
        //    }
        //    else
        //    {
        //        return RedirectToAction("Index","Login");
        //    }

        //}

        //[HttpPost]
        //public ActionResult DeductCharge(ChargeVM model)
        //{

        //    string userName = (string)Session["LOGGED_USERNAME"];
        //    string clientCode = (string)Session["LOGGEDUSER_ID"];
        //    string name = (string)Session["LOGGEDUSER_NAME"];
        //    string userType = (string)Session["LOGGED_USERTYPE"];
        //    TempData["userType"] = userType;

        //    if (TempData["userType"] != null)
        //    {
        //        this.ViewData["userType"] = this.TempData["userType"];
        //        ViewBag.UserType = this.TempData["userType"];
        //        ViewBag.Name = name;
        //        CustomerUserModel data = new CustomerUserModel();
        //        var displayModel = data.GetCustCharge(model.UserInfo.ClientCode);
        //        displayModel.ChargeReg = model.ChargeReg;
        //        displayModel.ChargeRenew = model.ChargeRenew;
        //        displayModel.ChargePinReset = model.ChargePinReset;
        //        decimal RegChargeAmt=0, RenewChargeAmt=0, PinResetChargeAmt=0;
        //        string ChargeDescription = "";
        //        if(displayModel.IsCharged)
        //        {
        //            //Error message
        //            return View(displayModel);
        //        }
        //        if(model.ChargeReg)
        //        {
        //            RegChargeAmt = Convert.ToDecimal(displayModel.ProfileCharge.Registration == "" ? "0" : displayModel.ProfileCharge.Registration);
        //            ChargeDescription = "Registration Charge |";
        //        }
        //        if (model.ChargeRenew)
        //        {
        //            RenewChargeAmt = Convert.ToDecimal(displayModel.ProfileCharge.ReNew == "" ? "0" : displayModel.ProfileCharge.ReNew);
        //            ChargeDescription = ChargeDescription + " Renew Charge |";
        //        }
        //        if (model.ChargePinReset)
        //        {
        //            PinResetChargeAmt = Convert.ToDecimal(displayModel.ProfileCharge.PinReset == "" ? "0" : displayModel.ProfileCharge.PinReset);
        //            ChargeDescription = ChargeDescription + " Pinreset Charge ";
        //        }
        //        decimal Total=RegChargeAmt + RenewChargeAmt + PinResetChargeAmt;
        //        //var TraceId = DateTime.Now.ToString();
        //        using (HttpClient client = new HttpClient())
        //        {

        //            var action = "ft/request";
        //            var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
        //            var content = new FormUrlEncodedContent(new[]{
        //                new KeyValuePair<string, string>("tid", ""), //traceid
        //                new KeyValuePair<string,string>("sc",""), //Service code
        //                new KeyValuePair<string, string>("mobile",""), //mobile number
        //                new KeyValuePair<string, string>("amount",""), //amount
        //                new KeyValuePair<string,string>("da",""),  //destination account
        //                new KeyValuePair<string,string>("pin",""), //pin
        //                new KeyValuePair<string, string>("note", ""), //transaction note
        //                new KeyValuePair<string,string>("src","") //source web
        //            });
        //        }


        //        return View(displayModel);
        //    }
        //    else
        //    {
        //        return RedirectToAction("Index", "Login");
        //    }

        //}

        public List<T> ListDifference<T>(List<T> source, List<T> destination)
        {

            if (source.Count <= 0 || destination.Count <= 0)
            {
                return new List<T>();
            }
            List<T> diff = new List<T>();
            foreach (var listItem in source)
            {
                if (!destination.Contains(listItem))
                {
                    diff.Add(listItem);
                }
            }


            return diff;
        }

        bool IsActive(UserInfo regobj, out string Message)
        {
            Message = string.Empty;
            bool Active = false;
            DataSet dsBranch = BranchUtils.GetDataSetPopulateBranchName();
            DataTable bank = dsBranch.Tables[0];
            var BranchDictnary = bank.AsEnumerable().ToDictionary<DataRow, string, string>(row => row.Field<string>(0),
                                                                            row => row.Field<string>(1));
            if (regobj.Status == "Active" && regobj.IsApproved == "Approve")
            {
                Active = true;
            }
            else if (regobj.Status == "Active" && regobj.IsApproved == "UnApprove")
            {
                if (regobj.IsRejected == "T")
                {
                    Message = "Rejected in :" + BranchDictnary[regobj.UserBranchCode];
                }
                else
                {
                    Message = "Waiting for approval: " + BranchDictnary[regobj.UserBranchCode];
                }
                Active = false;
            }
            else if (regobj.Status == "Blocked" && regobj.IsApproved == "Approve")
            {
                Message = "Blocked";
                Active = false;
            }
            else if (regobj.Status == "Active" && regobj.IsApproved == "UnApprove" && regobj.IsModified == "T")
            {
                if (regobj.IsRejected == "T")
                {
                    Message = "Rejected in :" + BranchDictnary[regobj.ModifyingBranch];
                }
                else
                {
                    Message = "Pending approval in :" + BranchDictnary[regobj.ModifyingBranch];
                }
                Active = false;
            }
            else if (regobj.Status == "Active" && regobj.IsApproved == "UnApprove" && regobj.IsModified != "T" && regobj.IsRejected == "T")
            {
                Message = "Rejected in :" + BranchDictnary[regobj.UserBranchCode];
                Active = false;
            }
            else if (regobj.Status == "PASR" && regobj.IsApproved == "UnApprove")
            {
                Message = "Customer is pending password reset approval in :" + BranchDictnary[regobj.ModifyingBranch];
                Active = false;
            }
            else if (regobj.Status == "PINR" && regobj.IsApproved == "UnApprove")
            {
                Message = "Pending pin reset approval in :" + BranchDictnary[regobj.ModifyingBranch];
                Active = false;
            }
            else if (regobj.Status == "PPR" && regobj.IsApproved == "UnApprove")
            {
                Message = "Pending PIN/password reset approval in :" + BranchDictnary[regobj.ModifyingBranch];
                Active = false;
            }
            else if (regobj.Status == "Blocked" && regobj.IsApproved == "Blocked")
            {
                Message = "Customer block in progress: " + BranchDictnary[regobj.ModifyingBranch];
                Active = false;
            }
            else if (regobj.Status == "Active" && regobj.IsApproved == "Blocked")
            {
                //customer unblock in progress: branch_name
                Message = "Customer unblock in progress: " + BranchDictnary[regobj.ModifyingBranch];
                Active = false;
            }
            return Active;
        }

        private void FundTransfer()
        {

            using (HttpClient client = new HttpClient())
            {

                var action = "ft/request";
                var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                var content = new FormUrlEncodedContent(new[]{
                        new KeyValuePair<string, string>("tid", ""), //traceid
                        new KeyValuePair<string,string>("sc",""), //Service code
                        new KeyValuePair<string, string>("mobile",""), //mobile number
                        new KeyValuePair<string, string>("amount",""), //amount
                        new KeyValuePair<string,string>("da",""),  //destination account
                        new KeyValuePair<string,string>("pin",""), //pin
                        new KeyValuePair<string, string>("note", ""), //transaction note
                        new KeyValuePair<string,string>("src","") //source web
                    });
            }
        }

        //[HttpGet]
        //public ActionResult TESTModification(string id)
        //{
        //    string userName = (string)Session["LOGGED_USERNAME"];
        //    string clientCode = (string)Session["LOGGEDUSER_ID"];
        //    string name = (string)Session["LOGGEDUSER_NAME"];
        //    string userType = (string)Session["LOGGED_USERTYPE"];

        //    if (this.TempData["custmodify_messsage"] != null)
        //    {
        //        this.ViewData["custmodify_messsage"] = this.TempData["custmodify_messsage"];
        //        this.ViewData["message_class"] = this.TempData["message_class"];
        //    }

        //    TempData["userType"] = userType;

        //    if (TempData["userType"] != null)
        //    {
        //        this.ViewData["userType"] = this.TempData["userType"];
        //        ViewBag.UserType = this.TempData["userType"];
        //        ViewBag.Name = name;

        //        //FOR Bank
        //        List<SelectListItem> item = new List<SelectListItem>();
        //        DataSet dsBank = BankUtils.GetDataSetPopulateBankCode();
        //        ViewBag.bank = dsBank.Tables[0];
        //        foreach (DataRow dr in ViewBag.bank.Rows)
        //        {
        //            item.Add(new SelectListItem
        //            {
        //                Text = @dr["BankName"].ToString(),
        //                Value = @dr["BankCode"].ToString()
        //            });
        //        }
        //        ViewBag.Bank = item.Where(x => x.Value != "0000").ToList();
        //        //ViewBag.Bank = item;

        //        List<MNCustProfile> ProfileList = new List<MNCustProfile>();
        //        List<SelectListItem> ItemProfile = new List<SelectListItem>();
        //        ProfileList = CusProfileUserModel.GetMNCustProfile().Where(x => x.ProfileStatus == 'A').ToList();

        //        foreach (MNCustProfile Cust in ProfileList)
        //        {
        //            ItemProfile.Add(new SelectListItem
        //            {
        //                Text = Cust.ProfileCode,
        //                Value = Cust.ProfileCode

        //            });
        //        }
        //        UserInfo regobj = new UserInfo();
        //        //DataSet DSet = ProfileUtils.GetUserProfileInfoDS(id.ToString());
        //        var CustomerInfo = CustomerUtils.GetByCode(id);
        //        ViewBag.ProfileList = new SelectList(ItemProfile.OrderBy(x => x.Text), "Value", "Text", regobj.ProfileName);
        //        //ViewBag.AdminProfileName = new SelectList(itemProfile, "Value", "Text");
        //        TempData["testobj"] = regobj;
        //        return View(CustomerInfo);
        //    }
        //    else
        //    {
        //        return RedirectToAction("Index", "Login");
        //    }
        //}

        //[HttpPost]
        //public ActionResult TESTModification(Customer model)
        //{

        //    return View(model);
        //}

        //test//
        #region Customer Rejected List using search

        // GET: Customer/CustomerRejectedUnApproved
        [HttpGet]
        public ActionResult CustomerRejectedUnApproved()
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

                ApproveRJViewModel dtObj = new ApproveRJViewModel();

                DataTable UnapproveRejected = new DataTable();

                string BranchCode = (string)Session["UserBranch"];
                bool COC = (bool)Session["COC"];
                UnapproveRejected = ProfileUtils.GetUnApproveRJCustomerProfile(userType);
                if (!COC)
                {
                    var filtered = from myrow in UnapproveRejected.AsEnumerable()
                                   where myrow.Field<string>("UserBranchCode") == BranchCode
                                   select myrow;
                    if (filtered.Count() > 0)
                        dtObj.UNApproveRejectTable = filtered.CopyToDataTable();
                }
                else
                {
                    dtObj.UNApproveRejectTable = UnapproveRejected;
                }
                List<UserInfo> CustomerUNApproveReject = new List<UserInfo>();

                DataTable dtableCustomerRej = CustomerUtils.GetRejectedCustomer();
                if (dtableCustomerRej != null && dtableCustomerRej.Rows.Count > 0)
                {
                    UserInfo regobj = new UserInfo();
                    regobj.ClientCode = dtableCustomerRej.Rows[0]["ClientCode"].ToString();
                    regobj.Name = dtableCustomerRej.Rows[0]["Name"].ToString();
                    regobj.Address = dtableCustomerRej.Rows[0]["Address"].ToString();
                    regobj.PIN = dtableCustomerRej.Rows[0]["PIN"].ToString();
                    regobj.Status = dtableCustomerRej.Rows[0]["Status"].ToString();
                    regobj.ContactNumber1 = dtableCustomerRej.Rows[0]["ContactNumber1"].ToString();
                    regobj.ContactNumber2 = dtableCustomerRej.Rows[0]["ContactNumber2"].ToString();
                    regobj.UserName = dtableCustomerRej.Rows[0]["UserName"].ToString();
                    regobj.UserType = dtableCustomerRej.Rows[0]["userBranchName"].ToString();
                    regobj.IsApproved = dtableCustomerRej.Rows[0]["BankAccountNumber"].ToString();
                    regobj.IsRejected = dtableCustomerRej.Rows[0]["CreatedBy"].ToString();

                    CustomerUNApproveReject.Add(regobj);
                    ViewData["dtableCustomerUNApproveReject"] = dtableCustomerRej;
                }

                return View(CustomerUNApproveReject);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }

        [HttpGet]
        public ActionResult CusRejUnApproved(string SearchCol, string txtMobileNo, string txtName, string txtAccountNo)
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
                userInfo.ContactNumber1 = txtMobileNo; //= collection["txtMobileNo"].ToString();
                userInfo.Name = txtName;// = collection["txtName"].ToString();
                userInfo.WalletNumber = txtAccountNo;// = collection["txtAccountNo"].ToString();


                ApproveRJViewModel dtObj = new ApproveRJViewModel();

                /// <summary>
                /// UNAPPROVE REJECTED LIST OF CUSTOMER
                /// </summary>
                /// 
                DataTable UnapproveRejected = new DataTable();

                string BranchCode = (string)Session["UserBranch"];
                bool COC = (bool)Session["COC"];
                UnapproveRejected = ProfileUtils.GetUnApproveRJCustomerProfile(userType);
                if (!COC)
                {
                    var filtered = from myrow in UnapproveRejected.AsEnumerable()
                                   where myrow.Field<string>("UserBranchCode") == BranchCode
                                   select myrow;
                    if (filtered.Count() > 0)
                        dtObj.UNApproveRejectTable = filtered.CopyToDataTable();
                }
                else
                {
                    dtObj.UNApproveRejectTable = UnapproveRejected;
                }
                List<UserInfo> CustomerUNApproveReject = new List<UserInfo>();



                if ((SearchCol == "Mobile Number") && (userInfo.ContactNumber1 != ""))
                {
                    DataTable dtableCustomerStatusByMobileNo = CustomerUtils.GetCusRejUnByMobileNo(userInfo.ContactNumber1, userType);
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
                        regobj.IsApproved = dtableCustomerStatusByMobileNo.Rows[0]["IsApproved"].ToString();
                        regobj.IsRejected = dtableCustomerStatusByMobileNo.Rows[0]["IsRejected"].ToString();

                        CustomerUNApproveReject.Add(regobj);
                        ViewData["dtableCustomerUNApproveReject"] = dtableCustomerStatusByMobileNo;
                    }
                }
                if ((SearchCol == "First Name") && (userInfo.Name != ""))
                {
                    DataTable dtableCustomerStatusByName = CustomerUtils.GetCusRejUnByName(userInfo.Name, userType);
                    if (dtableCustomerStatusByName != null && dtableCustomerStatusByName.Rows.Count > 0)
                    {
                        UserInfo regobj = new UserInfo();
                        regobj.ClientCode = dtableCustomerStatusByName.Rows[0]["ClientCode"].ToString();
                        regobj.Name = dtableCustomerStatusByName.Rows[0]["Name"].ToString();
                        regobj.Address = dtableCustomerStatusByName.Rows[0]["Address"].ToString();
                        regobj.PIN = dtableCustomerStatusByName.Rows[0]["PIN"].ToString();
                        regobj.Status = dtableCustomerStatusByName.Rows[0]["Status"].ToString();
                        regobj.ContactNumber1 = dtableCustomerStatusByName.Rows[0]["ContactNumber1"].ToString();
                        regobj.ContactNumber2 = dtableCustomerStatusByName.Rows[0]["ContactNumber2"].ToString();
                        regobj.UserName = dtableCustomerStatusByName.Rows[0]["UserName"].ToString();
                        regobj.UserType = dtableCustomerStatusByName.Rows[0]["userType"].ToString();
                        regobj.IsApproved = dtableCustomerStatusByName.Rows[0]["IsApproved"].ToString();
                        regobj.IsRejected = dtableCustomerStatusByName.Rows[0]["IsRejected"].ToString();


                        CustomerUNApproveReject.Add(regobj);
                        ViewData["dtableCustomerUNApproveReject"] = dtableCustomerStatusByName;
                    }
                }
                if ((SearchCol == "Account Number") && (userInfo.WalletNumber != ""))
                {
                    DataTable dtableCustomerStatusByAC = CustomerUtils.GetCusRejUnByAcNumber(userInfo.WalletNumber, userType);
                    if (dtableCustomerStatusByAC != null && dtableCustomerStatusByAC.Rows.Count > 0)
                    {
                        UserInfo regobj = new UserInfo();
                        regobj.ClientCode = dtableCustomerStatusByAC.Rows[0]["ClientCode"].ToString();
                        regobj.Name = dtableCustomerStatusByAC.Rows[0]["Name"].ToString();
                        regobj.Address = dtableCustomerStatusByAC.Rows[0]["Address"].ToString();
                        regobj.PIN = dtableCustomerStatusByAC.Rows[0]["PIN"].ToString();
                        regobj.Status = dtableCustomerStatusByAC.Rows[0]["Status"].ToString();
                        regobj.ContactNumber1 = dtableCustomerStatusByAC.Rows[0]["ContactNumber1"].ToString();
                        regobj.ContactNumber2 = dtableCustomerStatusByAC.Rows[0]["ContactNumber2"].ToString();
                        regobj.UserName = dtableCustomerStatusByAC.Rows[0]["UserName"].ToString();
                        regobj.UserType = dtableCustomerStatusByAC.Rows[0]["userType"].ToString();
                        regobj.IsApproved = dtableCustomerStatusByAC.Rows[0]["IsApproved"].ToString();
                        regobj.IsRejected = dtableCustomerStatusByAC.Rows[0]["IsRejected"].ToString();

                        CustomerUNApproveReject.Add(regobj);
                        ViewData["dtableCustomerUNApproveReject"] = dtableCustomerStatusByAC;
                    }
                }

                return View(CustomerUNApproveReject);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        //For Approved Customer//
        // GET: Customer/CustomerRejectedUnApproved
        [HttpGet]
        public ActionResult CustomerRejectedApproved()
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

                ApproveRJViewModel dtObj = new ApproveRJViewModel();

                DataTable ApproveRejected = new DataTable();

                string BranchCode = (string)Session["UserBranch"];
                bool COC = (bool)Session["COC"];
                ApproveRejected = ProfileUtils.GetUnApproveRJCustomerProfile(userType);
                DataTable dtableApproveReject = new DataTable();
                if (!COC)
                {
                    var filtered = from myrow in ApproveRejected.AsEnumerable()
                                   where myrow.Field<string>("ModifyingBranch") == BranchCode
                                   select myrow;
                    if (filtered.Count() > 0)
                        dtableApproveReject = filtered.CopyToDataTable();
                }
                else
                {
                    dtableApproveReject = ApproveRejected;
                }
                List<UserInfo> CustomerApproveReject = new List<UserInfo>();

                DataTable dtableCustomerApproveRej = CustomerUtils.GetCustRejAp();
                if (dtableCustomerApproveRej != null && dtableCustomerApproveRej.Rows.Count > 0)
                {
                    UserInfo regobj = new UserInfo();
                    regobj.ClientCode = dtableCustomerApproveRej.Rows[0]["ClientCode"].ToString();
                    regobj.Name = dtableCustomerApproveRej.Rows[0]["Name"].ToString();
                    regobj.Address = dtableCustomerApproveRej.Rows[0]["Address"].ToString();
                    regobj.PIN = dtableCustomerApproveRej.Rows[0]["PIN"].ToString();
                    regobj.Status = dtableCustomerApproveRej.Rows[0]["Status"].ToString();
                    regobj.ContactNumber1 = dtableCustomerApproveRej.Rows[0]["ContactNumber1"].ToString();
                    regobj.ContactNumber2 = dtableCustomerApproveRej.Rows[0]["ContactNumber2"].ToString();
                    regobj.UserName = dtableCustomerApproveRej.Rows[0]["UserName"].ToString();
                    regobj.UserType = dtableCustomerApproveRej.Rows[0]["userBranchName"].ToString();
                    regobj.IsApproved = dtableCustomerApproveRej.Rows[0]["BankAccountNumber"].ToString();
                    regobj.IsRejected = dtableCustomerApproveRej.Rows[0]["CreatedBy"].ToString();

                    CustomerApproveReject.Add(regobj);
                    ViewData["dtableCustomerApproveReject"] = dtableCustomerApproveRej;
                }

                return View(CustomerApproveReject);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        [HttpGet]
        public ActionResult CusRejApproved(string SearchCol, string txtMobileNo, string txtName, string txtAccountNo)
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
                userInfo.ContactNumber1 = txtMobileNo; //= collection["txtMobileNo"].ToString();
                userInfo.Name = txtName;// = collection["txtName"].ToString();
                userInfo.WalletNumber = txtAccountNo;// = collection["txtAccountNo"].ToString();


                ApproveRJViewModel dtObj = new ApproveRJViewModel();

                /// <summary>
                /// APPROVE REJECTED LIST OF CUSTOMER
                /// </summary>
                /// 
                string BranchCode = (string)Session["UserBranch"];
                bool COC = (bool)Session["COC"];
                DataTable ApproveRejected = ProfileUtils.GetApproveRJCustomerProfile(userType);
                DataTable dtableApproveReject = new DataTable();
                if (!COC)
                {
                    var filtered = from myrow in ApproveRejected.AsEnumerable()
                                   where myrow.Field<string>("ModifyingBranch") == BranchCode
                                   select myrow;
                    if (filtered.Count() > 0)
                        dtableApproveReject = filtered.CopyToDataTable();
                }
                else
                {
                    dtableApproveReject = ApproveRejected;
                }
                List<UserInfo> CustomerApproveReject = new List<UserInfo>();



                if ((SearchCol == "Mobile Number") && (userInfo.ContactNumber1 != ""))
                {
                    DataTable dtableCustomerStatusByMobileNo = CustomerUtils.GetCusRejApByMobileNo(userInfo.ContactNumber1, userType);
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
                        regobj.IsApproved = dtableCustomerStatusByMobileNo.Rows[0]["IsApproved"].ToString();
                        regobj.IsRejected = dtableCustomerStatusByMobileNo.Rows[0]["IsRejected"].ToString();

                        CustomerApproveReject.Add(regobj);
                        ViewData["dtableCustomerApproveReject"] = dtableCustomerStatusByMobileNo;
                    }
                }
                if ((SearchCol == "First Name") && (userInfo.Name != ""))
                {
                    DataTable dtableCustomerStatusByName = CustomerUtils.GetCusRejApByName(userInfo.Name, userType);
                    if (dtableCustomerStatusByName != null && dtableCustomerStatusByName.Rows.Count > 0)
                    {
                        UserInfo regobj = new UserInfo();
                        regobj.ClientCode = dtableCustomerStatusByName.Rows[0]["ClientCode"].ToString();
                        regobj.Name = dtableCustomerStatusByName.Rows[0]["Name"].ToString();
                        regobj.Address = dtableCustomerStatusByName.Rows[0]["Address"].ToString();
                        regobj.PIN = dtableCustomerStatusByName.Rows[0]["PIN"].ToString();
                        regobj.Status = dtableCustomerStatusByName.Rows[0]["Status"].ToString();
                        regobj.ContactNumber1 = dtableCustomerStatusByName.Rows[0]["ContactNumber1"].ToString();
                        regobj.ContactNumber2 = dtableCustomerStatusByName.Rows[0]["ContactNumber2"].ToString();
                        regobj.UserName = dtableCustomerStatusByName.Rows[0]["UserName"].ToString();
                        regobj.UserType = dtableCustomerStatusByName.Rows[0]["userType"].ToString();
                        regobj.IsApproved = dtableCustomerStatusByName.Rows[0]["IsApproved"].ToString();
                        regobj.IsRejected = dtableCustomerStatusByName.Rows[0]["IsRejected"].ToString();


                        CustomerApproveReject.Add(regobj);
                        ViewData["dtableCustomerApproveReject"] = dtableCustomerStatusByName;
                    }
                }
                if ((SearchCol == "Account Number") && (userInfo.WalletNumber != ""))
                {
                    DataTable dtableCustomerStatusByAC = CustomerUtils.GetCusRejApByAcNumber(userInfo.WalletNumber, userType);
                    if (dtableCustomerStatusByAC != null && dtableCustomerStatusByAC.Rows.Count > 0)
                    {
                        UserInfo regobj = new UserInfo();
                        regobj.ClientCode = dtableCustomerStatusByAC.Rows[0]["ClientCode"].ToString();
                        regobj.Name = dtableCustomerStatusByAC.Rows[0]["Name"].ToString();
                        regobj.Address = dtableCustomerStatusByAC.Rows[0]["Address"].ToString();
                        regobj.PIN = dtableCustomerStatusByAC.Rows[0]["PIN"].ToString();
                        regobj.Status = dtableCustomerStatusByAC.Rows[0]["Status"].ToString();
                        regobj.ContactNumber1 = dtableCustomerStatusByAC.Rows[0]["ContactNumber1"].ToString();
                        regobj.ContactNumber2 = dtableCustomerStatusByAC.Rows[0]["ContactNumber2"].ToString();
                        regobj.UserName = dtableCustomerStatusByAC.Rows[0]["UserName"].ToString();
                        regobj.UserType = dtableCustomerStatusByAC.Rows[0]["userType"].ToString();
                        regobj.IsApproved = dtableCustomerStatusByAC.Rows[0]["IsApproved"].ToString();
                        regobj.IsRejected = dtableCustomerStatusByAC.Rows[0]["IsRejected"].ToString();

                        CustomerApproveReject.Add(regobj);
                        ViewData["dtableCustomerApproveReject"] = dtableCustomerStatusByAC;
                    }
                }

                return View(CustomerApproveReject);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }



        #endregion

        #region Self Registration Customer Approval
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
                    srInfo.CitizenshipPlaceOfIssue = dtableSrInfo.Rows[0]["CitizenPlaceOfIssue"].ToString();

                    srInfo.License = dtableSrInfo.Rows[0]["LicenseNo"].ToString();

                    srInfo.LicensePlaceOfIssue = dtableSrInfo.Rows[0]["LicensePlaceOfIssue"].ToString();
                    srInfo.LicenseIssueDate = dtableSrInfo.Rows[0]["LicenseIssueDate"].ToString();
                    srInfo.LicenseExpireDate = dtableSrInfo.Rows[0]["LicenseExpiryDate"].ToString();


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

                }


                return View(srInfo);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        #endregion

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

        #region LinkBankAccount

        // GET: Customer/ApproveList
        [HttpGet]
        public ActionResult BankLinkApproveList(string Key, string Value)
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

                bool COC = (bool)Session["COC"];
                TempData["COC"] = COC;

                if (this.TempData["bankLinkapprove_messsage"] != null)
                {
                    this.ViewData["bankLinkapprove_messsage"] = this.TempData["bankLinkapprove_messsage"];
                    this.ViewData["message_class"] = this.TempData["message_class"];
                }

                List<UserInfo> custUnApproveObj = new List<UserInfo>();
                UserInfo userInfo = new UserInfo();
                userInfo.ClientCode = clientCode;
                DataTable dtblUnapproveCus = new DataTable();
                DataTable dtblCus = CustomerUtils.GetBankLinkApproveDetailInfo(userInfo);

                //bool COC = Session["COC"] == null ? false : (bool)Session["COC"];
                string UserBranchCode = (string)Session["UserBranch"];
                ViewBag.Value = Value;
                if (Key == "Mobile")
                {
                    if (!string.IsNullOrEmpty(Value))
                    {
                        EnumerableRowCollection<DataRow> row;
                        //if (COC == true)
                        //{
                        //    row = dtblCus.AsEnumerable().Where(r => r.Field<string>("MobileNumber") == Value);
                        //}
                        //else
                        //{
                        //    row = dtblCus.AsEnumerable().Where(r => r.Field<string>("MobileNumber") == Value).Where(r => r.Field<string>("UserBranchCode") == UserBranchCode);
                        //}
                        row = dtblCus.AsEnumerable().Where(r => r.Field<string>("MobileNumber") == Value).Where(r => r.Field<string>("UserBranchCode") == UserBranchCode);
                        if (row.Any())
                            dtblUnapproveCus = row.CopyToDataTable();
                    }

                }
                else
                {
                    EnumerableRowCollection<DataRow> row;
                    if (COC == false)
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
                    userInfo.AcNumber = dtblUnapproveCus.Rows[0]["AccountNumber"].ToString();
                    string DOB = dtblUnapproveCus.Rows[0]["DateOfBirth"].ToString();
                    DateTime dob = DateTime.Parse(DOB);
                    userInfo.DOB = dob.ToString("dd MMM yyyy");


                    ViewBag.CustClientCode = userInfo.ClientCode;
                    ViewBag.ContactNumber1 = userInfo.ContactNumber1;
                    ViewBag.CName = userInfo.Name;
                    ViewBag.AcNumber = userInfo.AcNumber;
                    ViewBag.DOB = userInfo.DOB;
                    ViewBag.SearchValue = Value;
                    ViewBag.COC = COC; //test
                    custUnApproveObj.Add(userInfo);


                }

                //  var rowbankregistred = dtblUnapproveCus.AsEnumerable().Where(r => r.Field<string>("SelfRegistered") == "F");
                var selfregistred = dtblUnapproveCus.AsEnumerable();

                ViewData["dtableCustForApproval"] = new DataTable();
                ViewData["dtableCustForApprovalSelf"] = new DataTable();
                //if (rowbankregistred.Any())
                //    ViewData["dtableCustForApproval"] = rowbankregistred.CopyToDataTable();
                if (selfregistred.Any())
                    ViewData["dtableCustForApprovalSelf"] = selfregistred.CopyToDataTable();

                return View(userInfo);

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpPost]
        public ActionResult BankLinkApprove(UserInfo model, string btnApprove, CustomerSRInfo info, string Remarks)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            bool COC = (bool)Session["COC"];
            TempData["COC"] = COC;

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                string displayMessage = null;
                string messageClass = null;

                List<UserInfo> custUnApproveObj = new List<UserInfo>();
                UserInfo userInfoBankLinkApproval = new UserInfo();
                userInfoBankLinkApproval.ClientCode = model.ClientCode;
                DataTable dtblUnapproveCus = new DataTable();
                DataTable dtblCus = CustomerUtils.GetAccNo(userInfoBankLinkApproval);

                //bool COC = Session["COC"] == null ? false : (bool)Session["COC"];
                string UserBranchCode = (string)Session["UserBranch"];

                EnumerableRowCollection<DataRow> row;
                if (COC == false)
                {
                    row = dtblCus.AsEnumerable().Where(r => r.Field<string>("UserBranchCode") == UserBranchCode);
                    if (row.Any())
                        dtblUnapproveCus = row.CopyToDataTable();
                }
                else
                {
                    dtblUnapproveCus = dtblCus;
                }

                if (dtblUnapproveCus.Rows.Count > 0)
                {
                    userInfoBankLinkApproval.ClientCode = dtblUnapproveCus.Rows[0]["ClientCode"].ToString();
                    userInfoBankLinkApproval.ContactNumber1 = dtblUnapproveCus.Rows[0]["MobileNumber"].ToString();
                    userInfoBankLinkApproval.Name = dtblUnapproveCus.Rows[0]["Name"].ToString();
                    userInfoBankLinkApproval.AcNumber = dtblUnapproveCus.Rows[0]["AccountNumber"].ToString();
                    userInfoBankLinkApproval.DOB = dtblUnapproveCus.Rows[0]["DateOfBirth"].ToString();

                    ViewBag.CustClientCode = userInfoBankLinkApproval.ClientCode;
                    ViewBag.ContactNumber1 = userInfoBankLinkApproval.ContactNumber1;
                    ViewBag.CName = userInfoBankLinkApproval.Name;
                    ViewBag.AcNumber = userInfoBankLinkApproval.AcNumber;
                    ViewBag.DOB = userInfoBankLinkApproval.DOB;
                    ViewBag.COC = COC; //test
                    custUnApproveObj.Add(userInfoBankLinkApproval);
                }

                //UserInfo userInfoBankLinkApproval = new UserInfo();
                // userInfoBankLinkApproval.ClientCode = model.ClientCode;
                // DataTable dtableSrInfo = CustomerUtils.GetAccNo(userInfoBankLinkApproval);

                // userInfoBankLinkApproval.AcNumber = model.AcNumber;
                string accNo = ViewBag.AcNumber;
                userInfoBankLinkApproval.UserName = userName;

                if (btnApprove.ToUpper() == "REJECT")
                {
                    userInfoBankLinkApproval.Remarks = info.Remarks;
                    int ret = CustomerUtils.BankLinkReject(userInfoBankLinkApproval);
                    if (ret == 100)
                    {
                        displayMessage = "Customer " + userInfoBankLinkApproval.Name + " bank link has been Rejected.";

                        //SMS
                        SMSUtils SMS = new SMSUtils();

                        string Message = string.Format("Dear {0},\n Your Bank Link is rejected.\n Thank You -MNepal", userInfoBankLinkApproval.Name);

                        SMSLog Log = new SMSLog();

                        Log.UserName = userInfoBankLinkApproval.ContactNumber1;
                        Log.Purpose = "BLA"; //New Registration
                        Log.SentBy = userInfoBankLinkApproval.ApprovedBy;
                        Log.Message = "Bank Link successfully rejected";
                        //Log SMS
                        //CustomerUtils.LogSMS(Log);
                        SMS.SendSMS(Message, userInfoBankLinkApproval.ContactNumber1);

                        messageClass = CssSetting.SuccessMessageClass;
                    }
                    else if (ret == -1 || ret != 100)
                    {
                        displayMessage = "Error while rejecting Customer's Bank Link request. " + userInfoBankLinkApproval.Name;
                        messageClass = CssSetting.FailedMessageClass;
                    }

                    this.TempData["bankLinkapprove_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;

                    return RedirectToAction("BankLinkApproveList");

                }
                else if (btnApprove.ToUpper() == "APPROVE")
                {
                    userInfoBankLinkApproval.ApprovedBy = userName;
                    ///for date
                    userInfoBankLinkApproval.ApprovedDate = DateTime.Now.ToString("yyyy-MM-dd");

                    String branchCode = accNo.Substring(0, 3);
                    userInfoBankLinkApproval.BranchCode = branchCode;
                    int ret = CustomerUtils.BankLinkApprove(userInfoBankLinkApproval);
                    if (ret == 100)
                    {
                        displayMessage = "Customer Information for  user " + userInfoBankLinkApproval.Name + " has successfully been approved.";

                        //SMS
                        SMSUtils SMS = new SMSUtils();

                        string Message = string.Format("Dear {0},\n Your Bank Link is approved.\n Thank You -MNepal", userInfoBankLinkApproval.Name);

                        SMSLog Log = new SMSLog();

                        Log.UserName = userInfoBankLinkApproval.ContactNumber1;
                        Log.Purpose = "BLA"; //New Registration
                        Log.SentBy = userInfoBankLinkApproval.ApprovedBy;
                        Log.Message = "Bank Link successfully approved";
                        //Log SMS
                        //CustomerUtils.LogSMS(Log);
                        SMS.SendSMS(Message, userInfoBankLinkApproval.ContactNumber1);

                        messageClass = CssSetting.SuccessMessageClass;
                    }
                    else if (ret == -1 || ret != 100)
                    {
                        displayMessage = "Error while approving Bank Link";
                        messageClass = CssSetting.FailedMessageClass;
                    }

                    this.TempData["bankLinkapprove_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;

                    return RedirectToAction("BankLinkApproveList");
                }
                else
                {
                    this.TempData["bankLinkapprove_messsage"] = "Unauthorized execution";
                    this.TempData["message_class"] = CssSetting.FailedMessageClass;

                    return RedirectToAction("BankLinkApproveList");
                }
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }

        public ActionResult ViewBankLinkDetail(string id)
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

                UserInfo userInfo = new UserInfo();
                userInfo.ClientCode = id;
                DataTable dtblCus = CustomerUtils.GetBankLinkApproveDetailInformation(userInfo);
                DataTable dtblUnapproveCus = new DataTable();
                dtblUnapproveCus = dtblCus;

                if (dtblUnapproveCus.Rows.Count > 0)
                {
                    userInfo.ClientCode = dtblUnapproveCus.Rows[0]["ClientCode"].ToString();
                    userInfo.ContactNumber1 = dtblUnapproveCus.Rows[0]["MobileNumber"].ToString();
                    userInfo.Name = dtblUnapproveCus.Rows[0]["Name"].ToString();
                    userInfo.AcNumber = dtblUnapproveCus.Rows[0]["AccountNumber"].ToString();
                    string DOB = dtblUnapproveCus.Rows[0]["DateOfBirth"].ToString();
                    DateTime dob = DateTime.Parse(DOB);
                    userInfo.DOB = dob.ToString("dd MMM yyyy");


                    ViewBag.CustClientCode = userInfo.ClientCode;
                    ViewBag.ContactNumber1 = userInfo.ContactNumber1;
                    ViewBag.CName = userInfo.Name;
                    ViewBag.AcNumber = userInfo.AcNumber;
                    ViewBag.DOB = userInfo.DOB;
                }
                DataTable dtableGetUserInfo = ProfileUtils.GetCustomerName(ViewBag.ContactNumber1);
                if (dtableGetUserInfo != null && dtableGetUserInfo.Rows.Count > 0)
                {
                    userInfo.FName = dtableGetUserInfo.Rows[0]["FName"].ToString();
                    userInfo.MName = dtableGetUserInfo.Rows[0]["MName"].ToString();
                    userInfo.LName = dtableGetUserInfo.Rows[0]["LName"].ToString();

                    ViewBag.FName = userInfo.FName;
                    ViewBag.MName = userInfo.MName;
                    ViewBag.LName = userInfo.LName;
                }

                CustomerSRInfo srInfo = new CustomerSRInfo();
                srInfo.ClientCode = id;
                srInfo.DOB = ViewBag.DOB;

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
                    srInfo.DOB = DateTime.Parse(dtableSrInfo.Rows[0]["DateOfBirth"].ToString()).ToString("dd/MM/yyyy");
                    //srInfo.DOB = dtableSrInfo.Rows[0]["DateOfBirth"].ToString().Split()[0];
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
                    srInfo.CitizenshipIssueDate = DateTime.Parse(dtableSrInfo.Rows[0]["CitizenIssueDate"].ToString()).ToString("dd/MM/yyyy");
                    //srInfo.CitizenshipIssueDate = dtableSrInfo.Rows[0]["CitizenIssueDate"].ToString().Split()[0];
                    srInfo.BSCitizenshipIssueDate = dtableSrInfo.Rows[0]["BSCitizenIssueDate"].ToString();
                    srInfo.CitizenshipPlaceOfIssue = dtableSrInfo.Rows[0]["CitizenPlaceOfIssue"].ToString();


                    srInfo.License = dtableSrInfo.Rows[0]["LicenseNo"].ToString();
                    srInfo.LicensePlaceOfIssue = dtableSrInfo.Rows[0]["LicensePlaceOfIssue"].ToString();
                    srInfo.LicenseIssueDate = DateTime.Parse(dtableSrInfo.Rows[0]["LicenseIssueDate"].ToString()).ToString("dd/MM/yyyy");
                    //srInfo.LicenseIssueDate = dtableSrInfo.Rows[0]["LicenseIssueDate"].ToString().Split()[0];
                    srInfo.BSLicenseIssueDate = dtableSrInfo.Rows[0]["BSLicenseIssueDate"].ToString();
                    srInfo.LicenseExpireDate = DateTime.Parse(dtableSrInfo.Rows[0]["LicenseExpiryDate"].ToString()).ToString("dd/MM/yyyy");
                    //srInfo.LicenseExpireDate = dtableSrInfo.Rows[0]["LicenseExpiryDate"].ToString().Split()[0];
                    srInfo.BSLicenseExpireDate = dtableSrInfo.Rows[0]["BSLicenseExpiryDate"].ToString();

                    srInfo.Passport = dtableSrInfo.Rows[0]["PassportNo"].ToString();
                    srInfo.PassportPlaceOfIssue = dtableSrInfo.Rows[0]["PassportPlaceOfIssue"].ToString();
                    srInfo.PassportIssueDate = DateTime.Parse(dtableSrInfo.Rows[0]["PassportIssueDate"].ToString()).ToString("dd/MM/yyyy");
                    //srInfo.PassportIssueDate = dtableSrInfo.Rows[0]["PassportIssueDate"].ToString().Split()[0];
                    //srInfo.PassportExpireDate = dtableSrInfo.Rows[0]["PassportExpiryDate"].ToString().Split()[0];
                    srInfo.PassportExpireDate = DateTime.Parse(dtableSrInfo.Rows[0]["PassportExpiryDate"].ToString()).ToString("dd/MM/yyyy");

                    srInfo.Document = dtableSrInfo.Rows[0]["DocType"].ToString();



                    srInfo.BranchCode = "004";


                    srInfo.FrontImage = dtableSrInfo.Rows[0]["FrontImage"].ToString();
                    srInfo.BackImage = dtableSrInfo.Rows[0]["BackImage"].ToString();
                    srInfo.PassportImage = dtableSrInfo.Rows[0]["PassportImage"].ToString();
                    srInfo.CustStatus = dtableSrInfo.Rows[0]["CustStatus"].ToString();

                }


                return View(srInfo);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
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
    }
}