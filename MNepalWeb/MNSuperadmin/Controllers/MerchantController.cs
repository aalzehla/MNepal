using MNSuperadmin.App_Start;
using MNSuperadmin.Helper;
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
    public class MerchantController : Controller
    {

        DAL objdal = new DAL();


        #region "All Merchant Details"
        // GET: Merchant/Index
        [HttpGet]
        public ActionResult MerchantStatus()
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

        #region "GET: All Merchant Details"
        // GET: Merchant/MerchantDetail
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
                    DataTable dtableStatusByAll = MerchantUtils.GetMerchantUserProfileByALL(userInfo.Name, userInfo.WalletNumber, userInfo.ContactNumber1);
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
                    DataTable dtableStatusByName = MerchantUtils.GetMerchantUserProfileByName(userInfo.Name);
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

        #region "Merchant Details"
        // GET: Merchant/MerchantDetails
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

        #region "GET: View Merchant Details/ this is triggered when the search option is clicked"
        // GET: Merchant/Modification
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

                DataTable dtableUserInfo = MerchantUtils.GetMerchantProfileInfo(id.ToString());

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
                    regobj.MerchantCategory = getMerchantCategory(id);
                    regobj.PDistrictID = getDistrictName(dtableUserInfo.Rows[0]["PDistrictID"].ToString());
                    regobj.WebsiteName = dtableUserInfo.Rows[0]["WebsiteName"].ToString();
                    regobj.PProvince = getProvince(dtableUserInfo.Rows[0]["PProvince"].ToString());

                    regobj.FName = dtableUserInfo.Rows[0]["FName"].ToString();
                    regobj.MName = dtableUserInfo.Rows[0]["MName"].ToString();
                    regobj.LName = dtableUserInfo.Rows[0]["LName"].ToString();
                    regobj.BusinessName = dtableUserInfo.Rows[0]["BusinessName"].ToString();
                    regobj.RegistrationNumber = dtableUserInfo.Rows[0]["RegistrationNumber"].ToString();
                    regobj.VATNumber = dtableUserInfo.Rows[0]["VATNumber"].ToString();
                    regobj.PanNo = dtableUserInfo.Rows[0]["PANNumber"].ToString();
                    regobj.PStreet = dtableUserInfo.Rows[0]["PStreet"].ToString();
                    regobj.PVDC = dtableUserInfo.Rows[0]["PMunicipalityVDC"].ToString();
                    regobj.PHouseNo = dtableUserInfo.Rows[0]["PHouseNo"].ToString();
                    regobj.PWardNo = dtableUserInfo.Rows[0]["PWardNo"].ToString();
                    regobj.LandlineNumber = dtableUserInfo.Rows[0]["LandlineNumber"].ToString();
                    regobj.EmailAddress = dtableUserInfo.Rows[0]["EmailAddress"].ToString();

                    regobj.BankNo = dtableUserInfo.Rows[0]["BankNo"].ToString();
                    regobj.BranchCode = dtableUserInfo.Rows[0]["BranchCode"].ToString();
                    regobj.BankAccountNumber = dtableUserInfo.Rows[0]["BankAccountNumber"].ToString();

                    regobj.Citizenship = dtableUserInfo.Rows[0]["CitizenshipNo"].ToString();
                    regobj.CitizenshipPlaceOfIssue = dtableUserInfo.Rows[0]["CitizenPlaceOfIssue"].ToString();
                    regobj.CitizenshipIssueDate = dtableUserInfo.Rows[0]["CitizenIssueDate"].ToString().Split()[0];
                    regobj.CitizenshipIssueDate = DateTime.Parse(regobj.CitizenshipIssueDate.Trim()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                    regobj.BSCitizenshipIssueDate = dtableUserInfo.Rows[0]["BSCitizenIssueDate"].ToString().Split()[0];

                    regobj.PassportImage = dtableUserInfo.Rows[0]["PassportImage"].ToString();
                    regobj.FrontImage = dtableUserInfo.Rows[0]["FrontImage"].ToString();
                    regobj.BackImage = dtableUserInfo.Rows[0]["BackImage"].ToString();
                    regobj.RegCertificateImage = dtableUserInfo.Rows[0]["RegCertImage"].ToString();
                    regobj.TaxClearFrontImage = dtableUserInfo.Rows[0]["TaxClearFrontImage"].ToString();
                    regobj.TaxClearBackImage = dtableUserInfo.Rows[0]["TaxClearBackImage"].ToString();
                }
                return View(regobj);
            }

            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        #endregion
        
        #region "GET Registration"
        //GET: Merchant/MerchantRegistration
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
        
        #region "POST: Registration"
        //POST: Merchant/MerchantRegistration
        [HttpPost]
        public async Task<ActionResult> Registration(FormCollection collection, UserInfo model)
        {
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

                string tid;
                TraceIdGenerator traceid = new TraceIdGenerator();
                tid = traceid.GenerateUniqueTraceID();
                ViewBag.retrievalReference = tid;

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
                    userInfo.PIN = CustomerUtils.GeneratePin();
                    userInfo.PanNo = collection["txtPanNo"].ToString();
                    userInfo.Name = collection["txtBusinessName"].ToString(); /*+ " " + MName + " " + LName;*/
                    userInfo.FName = collection["txtFirstName"].ToString();
                    userInfo.MName = collection["txtMiddleName"].ToString();
                    userInfo.LName = collection["txtLastName"].ToString();
                    userInfo.BusinessName = collection["txtBusinessName"].ToString();
                    userInfo.RegistrationNumber = collection["txtRegistrationNumber"].ToString();
                    userInfo.VATNumber = collection["txtVATNumber"].ToString();
                    userInfo.WebsiteName = collection["txtWebsiteName"].ToString();
                    userInfo.PStreet = collection["PStreet"].ToString();
                    userInfo.PVDC = collection["txtPVDC"].ToString();
                    userInfo.PHouseNo = collection["txtPHouseNo"].ToString();
                    userInfo.PWardNo = collection["txtPWardNo"].ToString();
                    userInfo.PProvince = collection["PProvince"].ToString();
                    userInfo.PDistrictID = collection["PDistrictID"].ToString();
                    userInfo.LandlineNumber = collection["txtLandlineNumber"].ToString();
                    userInfo.EmailAddress = collection["Email"].ToString();

                    userInfo.Citizenship = collection["txtCitizenship"].ToString();
                    userInfo.CitizenshipPlaceOfIssue = collection["txtCitizenshipPlaceOfIssue"].ToString();
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
                        userInfo.BSCitizenshipIssueDate = "2062-09-17";
                    }


                    userInfo.Address = userInfo.PVDC + "," + userInfo.PDistrictID + "," + userInfo.PProvince;
                    //userInfo.MerchantType = collection["MerchantCategory"].ToString();
                    string MerchantCategory = collection["MerchantCategory"].ToString();
                    userInfo.PIN = CustomerUtils.GeneratePin().ToString();
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
                    userInfo.retrievalReference = ViewBag.retrievalReference;

                    userInfo.BankNo = collection["BankNoBin"];
                    userInfo.BankAccountNumber = collection["txtBankAccountNumber"].ToString();

                    userInfo.PassportImage = ParseCv(model.PassportPhoto);
                    userInfo.FrontImage = ParseCv(model.TaxClearFront);
                    userInfo.BackImage = ParseCv(model.TaxClearBack);
                    userInfo.RegCertificateImage = ParseCv(model.RegCertificatePhoto);
                    userInfo.TaxClearFrontImage = ParseCv(model.TaxClearFront);
                    userInfo.TaxClearBackImage = ParseCv(model.TaxClearBack);

                    //userInfo.Document = collection["txtDocument"].ToString();
                    string mobile = userInfo.UserName;

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

                                    var PP = ReturnFileName(model.PassportPhoto, mobile);
                                    var front = ReturnFileName(model.Front, mobile);
                                    var back = ReturnFileName(model.Back, mobile);
                                    var regCerti = ReturnFileName(model.RegCertificatePhoto, mobile);
                                    var taxFront = ReturnFileName(model.TaxClearFront, mobile);
                                    var taxBack = ReturnFileName(model.TaxClearBack, mobile);

                                    userInfo.PassportImageName = string.Format(PP);
                                    userInfo.FrontImageName = string.Format(front);
                                    userInfo.BackImageName = string.Format(back);
                                    userInfo.RegCertificatePhotoName = string.Format(regCerti);
                                    userInfo.TaxClearFrontName = string.Format(taxFront);
                                    userInfo.TaxClearBackName = string.Format(taxBack);

                                    await SavePhoto(userInfo);

                                    userInfo.PassportImageName = Session["PP"].ToString();
                                    userInfo.FrontImageName = Session["Front"].ToString();
                                    userInfo.BackImageName = Session["Back"].ToString();
                                    userInfo.RegCertificatePhotoName = Session["RegCerti"].ToString();
                                    userInfo.TaxClearFrontName = Session["TaxClearFront"].ToString();
                                    userInfo.TaxClearBackName = Session["TaxClearBack"].ToString();

                                    int results = MerchantUtils.RegisterMerchantInfo(userInfo, MerchantCategory);
                                    if (results > 0)
                                    {
                                        result = true;
                                        SMSUtils util = new SMSUtils();
                                        string Message = "Dear " + userInfo.FName + "," + "\n";
                                        Message += " Your Merchant Registration request has been queued for the approval. You'll be notified shortly."
                                            + "." + "\n" + "Thank You";
                                        Message += "-MNepal";
                                        util.SendSMS(Message, userInfo.ContactNumber1);

                                        int ret = MerchantUtils.InsertResponseQuickSelfReg(userInfo.UserName, tid, "200", "New Merchant Registration");

                                        //int resultmsg = RegisterUtils.CreateWalletAcInfo(userInfo);
                                        //if (resultmsg == 100)
                                        //{
                                        //    result = true;
                                        //}
                                        //else
                                        //{
                                        //    result = false;

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
        
        #region "GET: ApproveRegistration---List "
        [HttpGet]
        public ActionResult ApproveRegistration(string Value)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            string UserName = Value;

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
                userobj = MerchantUtils.GetRegisteredMerchantList(IsModified, UserName);
                //DataTable dtableUNApproveReject = ProfileUtils.GetUnApproveRJCustomerProfile();



                return View(userobj);

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        #endregion
        
        #region "Approve/Reject (button page) Merchant"
        // GET: Merchant/Modification
        [HttpGet]
        public ActionResult ApproveReject(string id)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];


            TempData["userType"] = userType;

            if (TempData["userType"] != null && id != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                UserInfo regobj = new UserInfo();

                DataTable dtableUserInfo = MerchantUtils.GetMerchantProfileInfo(id.ToString());

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
                    regobj.MerchantCategory = getMerchantCategory(id);
                    regobj.PDistrictID = getDistrictName(dtableUserInfo.Rows[0]["PDistrictID"].ToString());

                    regobj.PProvince = getProvince(dtableUserInfo.Rows[0]["PProvince"].ToString());

                    regobj.FName = dtableUserInfo.Rows[0]["FName"].ToString();
                    regobj.MName = dtableUserInfo.Rows[0]["MName"].ToString();
                    regobj.LName = dtableUserInfo.Rows[0]["LName"].ToString();
                    regobj.BusinessName = dtableUserInfo.Rows[0]["BusinessName"].ToString();
                    regobj.RegistrationNumber = dtableUserInfo.Rows[0]["RegistrationNumber"].ToString();
                    regobj.VATNumber = dtableUserInfo.Rows[0]["VATNumber"].ToString();
                    regobj.PanNo = dtableUserInfo.Rows[0]["PANNumber"].ToString();
                    regobj.PStreet = dtableUserInfo.Rows[0]["PStreet"].ToString();
                    regobj.PVDC = dtableUserInfo.Rows[0]["PMunicipalityVDC"].ToString();
                    regobj.PHouseNo = dtableUserInfo.Rows[0]["PHouseNo"].ToString();
                    regobj.PWardNo = dtableUserInfo.Rows[0]["PWardNo"].ToString();
                    regobj.LandlineNumber = dtableUserInfo.Rows[0]["LandlineNumber"].ToString();
                    regobj.EmailAddress = dtableUserInfo.Rows[0]["EmailAddress"].ToString();
                    regobj.WebsiteName = dtableUserInfo.Rows[0]["WebsiteName"].ToString();
                    regobj.BankNo = dtableUserInfo.Rows[0]["BankNo"].ToString();
                    regobj.BranchCode = dtableUserInfo.Rows[0]["BranchCode"].ToString();
                    regobj.BankAccountNumber = dtableUserInfo.Rows[0]["BankAccountNumber"].ToString();

                    regobj.Citizenship = dtableUserInfo.Rows[0]["CitizenshipNo"].ToString();
                    regobj.CitizenshipPlaceOfIssue = dtableUserInfo.Rows[0]["CitizenPlaceOfIssue"].ToString();
                    regobj.CitizenshipIssueDate = dtableUserInfo.Rows[0]["CitizenIssueDate"].ToString().Split()[0];
                    regobj.CitizenshipIssueDate = DateTime.Parse(regobj.CitizenshipIssueDate.Trim()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                    regobj.BSCitizenshipIssueDate = dtableUserInfo.Rows[0]["BSCitizenIssueDate"].ToString().Split()[0];
                    
                    regobj.PassportImage = dtableUserInfo.Rows[0]["PassportImage"].ToString();
                    regobj.FrontImage = dtableUserInfo.Rows[0]["FrontImage"].ToString();
                    regobj.BackImage = dtableUserInfo.Rows[0]["BackImage"].ToString();
                    regobj.RegCertificateImage = dtableUserInfo.Rows[0]["RegCertImage"].ToString();
                    regobj.TaxClearFrontImage = dtableUserInfo.Rows[0]["TaxClearFrontImage"].ToString();
                    regobj.TaxClearBackImage = dtableUserInfo.Rows[0]["TaxClearBackImage"].ToString();


                }
                return View(regobj);
            }

            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        #endregion
        
        #region "Post Approve/Reject Button Value"
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


                        displayMessage = "Merchant" + model.Name + " has been Rejected. Please Check Registration Rejected and perform accordingly";
                        messageClass = CssSetting.SuccessMessageClass;




                    }
                    else if (ret == -1 || ret != 100)
                    {
                        displayMessage = "Error while rejecting Merchant " + model.Name;
                        messageClass = CssSetting.FailedMessageClass;
                    }

                    this.TempData["merchant_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;
                    return RedirectToAction("ApproveRegistration");

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
                        DataTable dtblRegistration = MerchantUtils.GetMerchantProfileInfo(model.ClientCode);
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
                        //userInfo.PIN = CustomerUtils.GeneratePin();
                        if (userInfo.EmailAddress != "" && userInfo.EmailAddress != string.Empty)
                        {
                            string Subject = "New Merchant Registration";
                            string MailSubject = "<span style='font-size:15px;'><h4>Dear Merchant " + userInfo.Name + ",</h4>";
                            MailSubject += "A new Merchant Account has been created  for you ";
                            MailSubject += "and you have been issued with a new temporary password.<br/>";
                            MailSubject += "<br/><b>Username: " + userInfo.UserName + "</b> <br/>";
                            MailSubject += "<br/><b>Password: " + userInfo.Password + "</b> <br/>";
                            MailSubject += "<br/><b>PIN: " + userInfo.PIN + "</b> <br/>";
                            MailSubject += "                    (Please change your password after login.)<br/>";
                            MailSubject += "<br/>Thank You <br/>";
                            MailSubject += "<br/>-MNepal </span><br/>";
                            MailSubject += @"<br/>Note: This is confidential mail, Please do not share with other. Your credential purpose is for MNepal Administration Console only.<br/>";
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
                            string Message = string.Format("Dear Merchant " + userInfo.Name + ", Your Merchant Registration has been approved." + "\n Your new T-Pin is " + userInfo.PIN + " and Password is " + userInfo.Password + ".\n Thank You -MNepal");

                            SMSLog Log = new SMSLog();

                            Log.UserName = userInfo.UserName;
                            Log.Purpose = "NR"; //New Registration
                            Log.SentBy = userInfoApproval.ApprovedBy;
                            Log.Message = "Your T-Pin is " + ExtraUtility.Encrypt(userInfo.PIN) + "" + " and Password is " + ExtraUtility.Encrypt(userInfo.Password); //encrypted when logging
                                                                                                                                                                      //Log SMS
                            CustomerUtils.LogSMS(Log);

                            SMS.SendSMS(Message, userInfo.UserName);

                        }
                        displayMessage = "Registration Information for Merchant " + userInfo.Name + " has successfully been approved. Please go to Modification Page for further changes.";
                        messageClass = CssSetting.SuccessMessageClass;
                    }
                    else if (ret == -1 || ret != 100)
                    {
                        displayMessage = "Error while approving Agent Information";
                        messageClass = CssSetting.FailedMessageClass;
                    }

                    this.TempData["merchant_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;
                    return RedirectToAction("ApproveRegistration");
                }
                else
                {

                    this.TempData["agentapprove_messsage"] = "Unauthorized execution";
                    this.TempData["message_class"] = CssSetting.FailedMessageClass;

                    return RedirectToAction("RegistrationRejected", "Merchant");
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
        
        #region "GET: RegistrationRejected--- List "
        // GET: Merchant/Index
        [HttpGet]
        public ActionResult RegistrationRejected(string Value)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            string UserName = Value;
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
                userobj = MerchantUtils.GetMerchantRejectedList(IsModified, UserName);
                //DataTable dtableUNApproveReject = ProfileUtils.GetUnApproveRJCustomerProfile();



                return View(userobj);

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        #endregion

        #region "GET: RejectedModification-- Page "
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

            //Merchant Category selectlist Begin
            string merchantstring = "SELECT * FROM MNMerchant (NOLOCK)";
            DataTable dt1 = new DataTable();
            dt1 = objdal.MyMethod(merchantstring);
            List<SelectListItem> list1 = new List<SelectListItem>();
            foreach (DataRow row in dt1.Rows)
            {
                list1.Add(new SelectListItem
                {
                    Text = Convert.ToString(row.ItemArray[1]),
                    Value = Convert.ToString(row.ItemArray[0])
                });
            }
            ViewBag.MerchantCategory = list1;//MerchantUtils.GetMerchantsType();
            //Merchant Category selectlist End

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

                DataTable dtableUserInfo = MerchantUtils.GetMerchantProfileInfo(id.ToString());

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
                    regobj.MerchantCategory = getMerchantCategory(id);
                    regobj.PDistrictID = dtableUserInfo.Rows[0]["PDistrictID"].ToString();
                    regobj.PProvince = dtableUserInfo.Rows[0]["PProvince"].ToString();

                    regobj.FName = dtableUserInfo.Rows[0]["FName"].ToString();
                    regobj.MName = dtableUserInfo.Rows[0]["MName"].ToString();
                    regobj.LName = dtableUserInfo.Rows[0]["LName"].ToString();
                    regobj.BusinessName = dtableUserInfo.Rows[0]["BusinessName"].ToString();
                    regobj.RegistrationNumber = dtableUserInfo.Rows[0]["RegistrationNumber"].ToString();
                    regobj.VATNumber = dtableUserInfo.Rows[0]["VATNumber"].ToString();
                    regobj.PStreet = dtableUserInfo.Rows[0]["PStreet"].ToString();
                    regobj.PVDC = dtableUserInfo.Rows[0]["PMunicipalityVDC"].ToString();
                    regobj.PHouseNo = dtableUserInfo.Rows[0]["PHouseNo"].ToString();
                    regobj.PWardNo = dtableUserInfo.Rows[0]["PWardNo"].ToString();
                    regobj.PanNo = dtableUserInfo.Rows[0]["PANNumber"].ToString();
                    regobj.WebsiteName = dtableUserInfo.Rows[0]["WebsiteName"].ToString();
                    regobj.LandlineNumber = dtableUserInfo.Rows[0]["LandlineNumber"].ToString();
                    regobj.EmailAddress = dtableUserInfo.Rows[0]["EmailAddress"].ToString();

                    regobj.Citizenship = dtableUserInfo.Rows[0]["CitizenshipNo"].ToString();
                    regobj.CitizenshipPlaceOfIssue = dtableUserInfo.Rows[0]["CitizenPlaceOfIssue"].ToString();
                    regobj.CitizenshipIssueDate = dtableUserInfo.Rows[0]["CitizenIssueDate"].ToString().Split()[0];
                    regobj.CitizenshipIssueDate = DateTime.Parse(regobj.CitizenshipIssueDate.Trim()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                    regobj.BSCitizenshipIssueDate = dtableUserInfo.Rows[0]["BSCitizenIssueDate"].ToString().Split()[0];


                    regobj.BankNo = dtableUserInfo.Rows[0]["BankNo"].ToString();
                    regobj.BranchCode = dtableUserInfo.Rows[0]["BranchCode"].ToString();
                    regobj.BankAccountNumber = dtableUserInfo.Rows[0]["BankAccountNumber"].ToString();

                    regobj.PassportImage = dtableUserInfo.Rows[0]["PassportImage"].ToString();
                    regobj.FrontImage = dtableUserInfo.Rows[0]["FrontImage"].ToString();
                    regobj.BackImage = dtableUserInfo.Rows[0]["BackImage"].ToString();
                    regobj.RegCertificateImage = dtableUserInfo.Rows[0]["RegCertImage"].ToString();
                    regobj.TaxClearFrontImage = dtableUserInfo.Rows[0]["TaxClearFrontImage"].ToString();
                    regobj.TaxClearBackImage = dtableUserInfo.Rows[0]["TaxClearBackImage"].ToString();
                    
                    ViewBag.PProvince = new SelectList(list, "Value", "Text", regobj.PProvince);
                    ViewBag.PDistrictID = new SelectList(ProvinceToDistrict(regobj.PProvince), "Value", "Text", regobj.PDistrictID);
                    ViewBag.MerchantCategory = new SelectList(list1, "Value", "Text", getMerchantCategoryId(id));

                    regobj.Remarks = dtableUserInfo.Rows[0]["Remarks"].ToString();

                }
                return View(regobj);
            }

            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        #endregion
        
        #region "POST: RejectedModification-- page"
        //POST: Merchant/Modification/1
        [HttpPost]
        public async Task<ActionResult> RejectedModification(string btnCommand, FormCollection collection, UserInfo model)
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
                        string MerchantCategory = collection["MerchantCategory"].ToString();
                        userInfoModify.PProvince = collection["PProvince"].ToString();
                        userInfoModify.PDistrictID = collection["PDistrictID"].ToString();
                        //string cPIN = collection["PIN"].ToString();

                        userInfoModify.UserName = model.UserName.ToString();
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
                        //userInfoModify.PIN = cPIN; //(cPIN != "") && && (cAddress != "")

                        userInfoModify.FName = collection["txtFirstName"].ToString();
                        userInfoModify.MName = collection["txtMiddleName"].ToString();
                        userInfoModify.LName = collection["txtLastName"].ToString();
                        userInfoModify.BusinessName = collection["txtBusinessName"].ToString();
                        userInfoModify.RegistrationNumber = collection["txtRegistrationNumber"].ToString();
                        userInfoModify.VATNumber = collection["txtVATNumber"].ToString();
                        userInfoModify.PanNo = collection["txtPanNo"].ToString();
                        userInfoModify.PStreet = collection["PStreet"].ToString();
                        userInfoModify.PVDC = collection["txtPVDC"].ToString();
                        userInfoModify.PHouseNo = collection["txtPHouseNo"].ToString();
                        userInfoModify.PWardNo = collection["txtPWardNo"].ToString();
                        userInfoModify.PProvince = collection["PProvince"].ToString();
                        userInfoModify.PDistrictID = collection["PDistrictID"].ToString();
                        userInfoModify.LandlineNumber = collection["txtLandlineNumber"].ToString();
                        userInfoModify.EmailAddress = collection["Email"].ToString();
                        userInfoModify.WebsiteName = collection["txtWebsiteName"].ToString();

                        userInfoModify.Citizenship = collection["txtCitizenship"].ToString();
                        userInfoModify.CitizenshipPlaceOfIssue = collection["txtCitizenshipPlaceOfIssue"].ToString();
                        userInfoModify.CitizenshipIssueDate = collection["CitizenshipIssueDate"].ToString();
                        userInfoModify.BSCitizenshipIssueDate = collection["BSCitizenshipIssueDate"].ToString();

                        string cPPImage = collection["txtOldPP"].ToString();
                        string cFrontImage = collection["txtOldfront"].ToString();
                        string cBackImage = collection["txtOldback"].ToString();
                        string cRegCertiImage = collection["txtOldRegCerti"].ToString();
                        string cTaxClearFrontImage = collection["txtOldTaxClearFront"].ToString();
                        string cTaxClearBackImage = collection["txtOldTaxClearBack"].ToString();

                        userInfoModify.Name = userInfoModify.BusinessName; // + " " + userInfoModify.MName + " " + userInfoModify.LName;
                        userInfoModify.Address = userInfoModify.PVDC + " " + userInfoModify.PDistrictID + " " + userInfoModify.PProvince;
                        userInfoModify.AdminBranch = (string)Session["UserBranch"];

                        var PP = ReturnFileName(model.PassportPhoto, userInfoModify.UserName);
                        var front = ReturnFileName(model.Front, userInfoModify.UserName);
                        var back = ReturnFileName(model.Back, userInfoModify.UserName);
                        var regCert = ReturnFileName(model.RegCertificatePhoto, userInfoModify.UserName);
                        var taxClearFront = ReturnFileName(model.TaxClearFront, userInfoModify.UserName);
                        var taxDoc = ReturnFileName(model.TaxClearBack, userInfoModify.UserName);

                        if (PP != null)
                        {
                            if (!string.IsNullOrEmpty(cPPImage))
                            {
                                //var existingFile = Path.Combine(cPassportPhoto);
                                var existingFile = Request.MapPath(cPPImage);
                                if (System.IO.File.Exists(existingFile))
                                {
                                    System.IO.File.Delete(existingFile);
                                }
                            }
                            userInfoModify.PassportImage = ParseCv(model.PassportPhoto);
                            userInfoModify.PassportImageName = string.Format(PP);
                        }
                        else
                        {
                            userInfoModify.PassportImageName = "";
                        }
                        if (front != null)
                        {
                            if (!string.IsNullOrEmpty(cFrontImage))
                            {
                                //var existingFile = Path.Combine(cPassportPhoto);
                                var existingFile = Request.MapPath(cFrontImage);
                                if (System.IO.File.Exists(existingFile))
                                {
                                    System.IO.File.Delete(existingFile);
                                }
                            }
                            userInfoModify.FrontImage = ParseCv(model.Front);
                            userInfoModify.FrontImageName = string.Format(front);
                        }
                        else
                        {
                            userInfoModify.FrontImageName = "";
                        }
                        if (back != null)
                        {
                            if (!string.IsNullOrEmpty(cBackImage))
                            {
                                //var existingFile = Path.Combine(cPassportPhoto);
                                var existingFile = Request.MapPath(cBackImage);
                                if (System.IO.File.Exists(existingFile))
                                {
                                    System.IO.File.Delete(existingFile);
                                }
                            }
                            userInfoModify.BackImage = ParseCv(model.Back);
                            userInfoModify.BackImageName = string.Format(back);
                        }
                        else
                        {
                            userInfoModify.BackImageName = "";
                        }
                        if (regCert != null)
                        {
                            if (!string.IsNullOrEmpty(cRegCertiImage))
                            {
                                //var existingFile = Path.Combine(cPassportPhoto);
                                var existingFile = Request.MapPath(cRegCertiImage);
                                if (System.IO.File.Exists(existingFile))
                                {
                                    System.IO.File.Delete(existingFile);
                                }
                            }
                            userInfoModify.RegCertificateImage = ParseCv(model.RegCertificatePhoto);
                            userInfoModify.RegCertificatePhotoName = string.Format(regCert);
                        }
                        else
                        {
                            userInfoModify.RegCertificatePhotoName = "";
                        }

                        if (taxClearFront != null)
                        {
                            if (!string.IsNullOrEmpty(cTaxClearFrontImage))
                            {
                                //var existingFile = Path.Combine(cPassportPhoto);
                                var existingFile = Request.MapPath(cTaxClearFrontImage);
                                if (System.IO.File.Exists(existingFile))
                                {
                                    System.IO.File.Delete(existingFile);
                                }
                            }
                            userInfoModify.TaxClearFrontImage = ParseCv(model.TaxClearFront);
                            userInfoModify.TaxClearFrontName = string.Format(taxClearFront);
                        }
                        else
                        {
                            userInfoModify.TaxClearFrontName = "";
                        }
                        if (taxDoc != null)
                        {
                            if (!string.IsNullOrEmpty(cTaxClearBackImage))
                            {
                                //var existingFile = Path.Combine(cPassportPhoto);
                                var existingFile = Request.MapPath(cTaxClearBackImage);
                                if (System.IO.File.Exists(existingFile))
                                {
                                    System.IO.File.Delete(existingFile);
                                }
                            }
                            userInfoModify.TaxClearBackImage = ParseCv(model.TaxClearBack);
                            userInfoModify.TaxClearBackName = string.Format(taxDoc);
                        }
                        else
                        {
                            userInfoModify.TaxClearBackName = "";
                        }

                        await SavePhoto(userInfoModify);

                        string sPP = Session["PP"].ToString();
                        string sFront = Session["Front"].ToString();
                        string sBack = Session["Back"].ToString();
                        string sRegCert = Session["RegCerti"].ToString();
                        string sTaxClearFront = Session["TaxClearFront"].ToString();
                        string sTaxClearBack = Session["TaxClearBack"].ToString();


                        if (sPP != "")
                        {
                            userInfoModify.PassportImageName = Session["PP"].ToString();
                        }
                        else
                        {
                            userInfoModify.PassportImageName = cPPImage;
                        }
                        if (sFront != "")
                        {
                            userInfoModify.FrontImageName = Session["Front"].ToString();
                        }
                        else
                        {
                            userInfoModify.FrontImageName = cFrontImage;
                        }
                        if (sBack != "")
                        {
                            userInfoModify.BackImageName = Session["Back"].ToString();
                        }
                        else
                        {
                            userInfoModify.BackImageName = cBackImage;
                        }
                        if (sRegCert != "")
                        {
                            userInfoModify.RegCertificatePhotoName = Session["RegCerti"].ToString();
                        }
                        else
                        {
                            userInfoModify.RegCertificatePhotoName = cRegCertiImage;
                        }
                        if (sTaxClearFront != "")
                        {
                            userInfoModify.TaxClearFrontName = Session["TaxClearFront"].ToString();
                        }
                        else
                        {
                            userInfoModify.TaxClearFrontName = cTaxClearFrontImage;
                        }
                        if (sTaxClearBack != "")
                        {
                            userInfoModify.TaxClearBackName = Session["TaxClearBack"].ToString();
                        }
                        else
                        {
                            userInfoModify.TaxClearBackName = cTaxClearBackImage;
                        }

                        if ((cIsRejected != "") && (cIsApproved != "")
                            && (cContactNumber1 != "") && (cStatus != ""))
                        {
                            bool isUpdated = MerchantUtils.UpdateRejectedMerchantInfo(userInfoModify,MerchantCategory);
                            displayMessage = isUpdated
                                                     ? "Merchant Information has successfully been updated. Please go to Approve Registration."
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

                        return RedirectToAction("RegistrationRejected");
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
        public ActionResult Modification(string Key, string Value)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            string UserName = Value;

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

                string IsModified = "T";
                List<UserInfo> userobj = new List<UserInfo>();


                userobj = MerchantUtils.GetMerchantApprovedList(IsModified, UserName);


                ViewBag.ProfileName = new SelectList(itemProfile, "Value", "Text", itemProfile).OrderBy(x => x.Text);

                return View(userobj);

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        #endregion
        
        #region "GET: Merchant Modification"
        // GET: Merchant/Modification
        [HttpGet]
        public ActionResult Modify(string id)
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

            //Merchant Category selectlist Begin
            string merchantstring = "SELECT * FROM MNMerchant (NOLOCK)";
            DataTable dt1 = new DataTable();
            dt1 = objdal.MyMethod(merchantstring);
            List<SelectListItem> list1 = new List<SelectListItem>();
            foreach (DataRow row in dt1.Rows)
            {
                list1.Add(new SelectListItem
                {
                    Text = Convert.ToString(row.ItemArray[1]),
                    Value = Convert.ToString(row.ItemArray[0])
                });
            }
            ViewBag.MerchantCategory = list1;//MerchantUtils.GetMerchantsType();
            //Merchant Category selectlist End

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

                DataTable dtableUserInfo = MerchantUtils.GetMerchantProfileInfo(id.ToString());

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
                    regobj.MerchantCategory = getMerchantCategory(id);
                    regobj.FName = dtableUserInfo.Rows[0]["FName"].ToString();
                    regobj.MName = dtableUserInfo.Rows[0]["MName"].ToString();
                    regobj.LName = dtableUserInfo.Rows[0]["LName"].ToString();
                    regobj.BusinessName = dtableUserInfo.Rows[0]["BusinessName"].ToString();
                    regobj.RegistrationNumber = dtableUserInfo.Rows[0]["RegistrationNumber"].ToString();
                    regobj.VATNumber = dtableUserInfo.Rows[0]["VATNumber"].ToString();
                    regobj.PStreet = dtableUserInfo.Rows[0]["PStreet"].ToString();
                    regobj.PVDC = dtableUserInfo.Rows[0]["PMunicipalityVDC"].ToString();
                    regobj.PHouseNo = dtableUserInfo.Rows[0]["PHouseNo"].ToString();
                    regobj.PWardNo = dtableUserInfo.Rows[0]["PWardNo"].ToString();
                    regobj.PanNo = dtableUserInfo.Rows[0]["PANNumber"].ToString();
                    regobj.WebsiteName = dtableUserInfo.Rows[0]["WebsiteName"].ToString();
                    regobj.LandlineNumber = dtableUserInfo.Rows[0]["LandlineNumber"].ToString();
                    regobj.EmailAddress = dtableUserInfo.Rows[0]["EmailAddress"].ToString();

                    regobj.Citizenship = dtableUserInfo.Rows[0]["CitizenshipNo"].ToString();
                    regobj.CitizenshipPlaceOfIssue = dtableUserInfo.Rows[0]["CitizenPlaceOfIssue"].ToString();
                    regobj.CitizenshipIssueDate = dtableUserInfo.Rows[0]["CitizenIssueDate"].ToString().Split()[0];
                    regobj.CitizenshipIssueDate = DateTime.Parse(regobj.CitizenshipIssueDate.Trim()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                    regobj.BSCitizenshipIssueDate = dtableUserInfo.Rows[0]["BSCitizenIssueDate"].ToString().Split()[0];

                    regobj.PassportImage = dtableUserInfo.Rows[0]["PassportImage"].ToString();
                    regobj.FrontImage = dtableUserInfo.Rows[0]["FrontImage"].ToString();
                    regobj.BackImage = dtableUserInfo.Rows[0]["BackImage"].ToString();
                    regobj.RegCertificateImage = dtableUserInfo.Rows[0]["RegCertImage"].ToString();
                    regobj.TaxClearFrontImage = dtableUserInfo.Rows[0]["TaxClearFrontImage"].ToString();
                    regobj.TaxClearBackImage = dtableUserInfo.Rows[0]["TaxClearBackImage"].ToString();

                    regobj.BankNo = dtableUserInfo.Rows[0]["BankNo"].ToString();
                    regobj.BranchCode = dtableUserInfo.Rows[0]["BranchCode"].ToString();
                    regobj.BankAccountNumber = dtableUserInfo.Rows[0]["BankAccountNumber"].ToString();

                    ViewBag.PProvince = new SelectList(list, "Value", "Text", regobj.PProvince);
                    ViewBag.PDistrictID = new SelectList(ProvinceToDistrict(regobj.PProvince), "Value", "Text", regobj.PDistrictID);
                    ViewBag.MerchantCategory = new SelectList(list1, "Value", "Text", getMerchantCategoryId(id));
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
        //POST: Merchant/Modification/1
        [HttpPost]
        public async Task<ActionResult>Modify(string btnCommand, string id, FormCollection collection, UserInfo model)
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

                        ViewBag.Bank = item;

                        string cClientCode = collection["ClientCode"].ToString();

                        string cStatus = collection["txtStatus"].ToString();
                        string cContactNumber1 = collection["ContactNumber1"].ToString();
                        string cContactNumber2 = collection["ContactNumber2"].ToString();

                        string cBranchCode = collection["BranchCode"].ToString();
                        string cBankNo = collection["BankNo"].ToString();
                        string cBankAccountNumber = collection["BankAccountNumber"].ToString();
                        string cIsApproved = collection["txtIsApproved"].ToString();
                        string cIsRejected = collection["txtIsRejected"].ToString();
                        userInfoModify.MerchantCategory = collection["MerchantCategory"].ToString();

                        userInfoModify.PProvince = collection["PProvince"].ToString();
                        userInfoModify.PDistrictID = collection["PDistrictID"].ToString();

                        userInfoModify.IsModified = "T";

                        userInfoModify.ClientCode = cClientCode;

                        userInfoModify.EmailAddress = "";

                        userInfoModify.Status = cStatus;
                        userInfoModify.ContactNumber1 = cContactNumber1;
                        userInfoModify.ContactNumber2 = cContactNumber2;

                        userInfoModify.BankNo = cBankNo;
                        userInfoModify.BranchCode = cBankAccountNumber.Substring(0, 3);
                        userInfoModify.BankAccountNumber = cBankAccountNumber;
                        userInfoModify.IsApproved = cIsApproved;

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
                        userInfoModify.PanNo = collection["txtPanNo"].ToString();

                        userInfoModify.Citizenship = collection["txtCitizenship"].ToString();
                        userInfoModify.CitizenshipPlaceOfIssue = collection["txtCitizenshipPlaceOfIssue"].ToString();
                        userInfoModify.CitizenshipIssueDate = collection["CitizenshipIssueDate"].ToString();                        
                        userInfoModify.BSCitizenshipIssueDate = collection["BSCitizenshipIssueDate"].ToString();

                        string cPPImage = collection["txtOldPP"].ToString();
                        string cFrontImage = collection["txtOldfront"].ToString();
                        string cBackImage = collection["txtOldback"].ToString();
                        string cRegCertiImage = collection["txtOldRegCerti"].ToString();
                        string cTaxClearFrontImage = collection["txtOldTaxClearFront"].ToString();
                        string cTaxClearBackImage = collection["txtOldTaxClearBack"].ToString();

                        userInfoModify.WebsiteName = collection["txtWebsiteName"].ToString();
                        userInfoModify.Name = userInfoModify.BusinessName; //+ " " + userInfoModify.MName + " " + userInfoModify.LName;
                        userInfoModify.Address = userInfoModify.PVDC + " " + userInfoModify.PDistrictID + " " + userInfoModify.PProvince;

                        var PP = ReturnFileName(model.PassportPhoto, userInfoModify.UserName);
                        var front = ReturnFileName(model.Front, userInfoModify.UserName);
                        var back = ReturnFileName(model.Back, userInfoModify.UserName);
                        var regCert = ReturnFileName(model.RegCertificatePhoto, userInfoModify.UserName);
                        var taxClearFront = ReturnFileName(model.TaxClearFront, userInfoModify.UserName);
                        var taxDoc = ReturnFileName(model.TaxClearBack, userInfoModify.UserName);

                        if (PP != null)
                        {
                            if (!string.IsNullOrEmpty(cPPImage))
                            {
                                //var existingFile = Path.Combine(cPassportPhoto);
                                var existingFile = Request.MapPath(cPPImage);
                                if (System.IO.File.Exists(existingFile))
                                {
                                    System.IO.File.Delete(existingFile);
                                }
                            }
                            userInfoModify.PassportImage = ParseCv(model.PassportPhoto);
                            userInfoModify.PassportImageName = string.Format(PP);
                        }
                        else
                        {
                            userInfoModify.PassportImageName = "";
                        }
                        if (front != null)
                        {
                            if (!string.IsNullOrEmpty(cFrontImage))
                            {
                                //var existingFile = Path.Combine(cPassportPhoto);
                                var existingFile = Request.MapPath(cFrontImage);
                                if (System.IO.File.Exists(existingFile))
                                {
                                    System.IO.File.Delete(existingFile);
                                }
                            }
                            userInfoModify.FrontImage = ParseCv(model.Front);
                            userInfoModify.FrontImageName = string.Format(front);
                        }
                        else
                        {
                            userInfoModify.FrontImageName = "";
                        }
                        if (back != null)
                        {
                            if (!string.IsNullOrEmpty(cBackImage))
                            {
                                //var existingFile = Path.Combine(cPassportPhoto);
                                var existingFile = Request.MapPath(cBackImage);
                                if (System.IO.File.Exists(existingFile))
                                {
                                    System.IO.File.Delete(existingFile);
                                }
                            }
                            userInfoModify.BackImage = ParseCv(model.Back);
                            userInfoModify.BackImageName = string.Format(back);
                        }
                        else
                        {
                            userInfoModify.BackImageName = "";
                        }
                        if (regCert != null)
                        {
                            if (!string.IsNullOrEmpty(cRegCertiImage))
                            {
                                //var existingFile = Path.Combine(cPassportPhoto);
                                var existingFile = Request.MapPath(cRegCertiImage);
                                if (System.IO.File.Exists(existingFile))
                                {
                                    System.IO.File.Delete(existingFile);
                                }
                            }
                            userInfoModify.RegCertificateImage = ParseCv(model.RegCertificatePhoto);
                            userInfoModify.RegCertificatePhotoName = string.Format(regCert);
                        }
                        else
                        {
                            userInfoModify.RegCertificatePhotoName = "";
                        }

                        if (taxClearFront != null)
                        {
                            if (!string.IsNullOrEmpty(cTaxClearFrontImage))
                            {
                                //var existingFile = Path.Combine(cPassportPhoto);
                                var existingFile = Request.MapPath(cTaxClearFrontImage);
                                if (System.IO.File.Exists(existingFile))
                                {
                                    System.IO.File.Delete(existingFile);
                                }
                            }
                            userInfoModify.TaxClearFrontImage = ParseCv(model.TaxClearFront);
                            userInfoModify.TaxClearFrontName = string.Format(taxClearFront);
                        }
                        else
                        {
                            userInfoModify.TaxClearFrontName = "";
                        }
                        if (taxDoc != null)
                        {
                            if (!string.IsNullOrEmpty(cTaxClearBackImage))
                            {
                                //var existingFile = Path.Combine(cPassportPhoto);
                                var existingFile = Request.MapPath(cTaxClearBackImage);
                                if (System.IO.File.Exists(existingFile))
                                {
                                    System.IO.File.Delete(existingFile);
                                }
                            }
                            userInfoModify.TaxClearBackImage = ParseCv(model.TaxClearBack);
                            userInfoModify.TaxClearBackName = string.Format(taxDoc);
                        }
                        else
                        {
                            userInfoModify.TaxClearBackName = "";
                        }

                        await SavePhoto(userInfoModify);

                        string sPP = Session["PP"].ToString();
                        string sFront = Session["Front"].ToString();
                        string sBack = Session["Back"].ToString();
                        string sRegCert = Session["RegCerti"].ToString();
                        string sTaxClearFront = Session["TaxClearFront"].ToString();
                        string sTaxClearBack = Session["TaxClearBack"].ToString();


                        if (sPP != "")
                        {
                            userInfoModify.PassportImageName = Session["PP"].ToString();
                        }
                        else
                        {
                            userInfoModify.PassportImageName = cPPImage;
                        }
                        if (sFront != "")
                        {
                            userInfoModify.FrontImageName = Session["Front"].ToString();
                        }
                        else
                        {
                            userInfoModify.FrontImageName = cFrontImage;
                        }
                        if (sBack != "")
                        {
                            userInfoModify.BackImageName = Session["Back"].ToString();
                        }
                        else
                        {
                            userInfoModify.BackImageName = cBackImage;
                        }
                        if (sRegCert != "")
                        {
                            userInfoModify.RegCertificatePhotoName = Session["RegCerti"].ToString();
                        }
                        else
                        {
                            userInfoModify.RegCertificatePhotoName = cRegCertiImage;
                        }
                        if (sTaxClearFront != "")
                        {
                            userInfoModify.TaxClearFrontName = Session["TaxClearFront"].ToString();
                        }
                        else
                        {
                            userInfoModify.TaxClearFrontName = cTaxClearFrontImage;
                        }
                        if (sTaxClearBack != "")
                        {
                            userInfoModify.TaxClearBackName = Session["TaxClearBack"].ToString();
                        }
                        else
                        {
                            userInfoModify.TaxClearBackName = cTaxClearBackImage;
                        }

                        /*map new data to Merchant*/
                        MerchantTables NewMerch = new MerchantTables();
                        MNMerchants NewData = new MNMerchants();
                        MNClient NewMNClient = new MNClient();
                        MNClientContact NewMNClientContact = new MNClientContact();
                        MNClientKYC NewMNClientKYC = new MNClientKYC();
                        MNMerchantKYCDoc NewMNMerchantKYCDoc = new MNMerchantKYCDoc();
                        MNBankAccountMap NewMNBankAccountMap = new MNBankAccountMap();

                        NewData.mname = userInfoModify.BusinessName;
                        NewData.ClientCode = userInfoModify.ClientCode;
                        NewData.BusinessName = userInfoModify.BusinessName;
                        NewData.RegistrationNumber = userInfoModify.RegistrationNumber;
                        NewData.VATNumber = userInfoModify.VATNumber;
                        NewData.WebsiteName = userInfoModify.WebsiteName;
                        NewData.LandlineNumber = userInfoModify.LandlineNumber;
                        NewData.catid = userInfoModify.MerchantCategory;

                        NewMNClient.PStreet = userInfoModify.PStreet;
                        NewMNClient.PMunicipalityVDC = userInfoModify.PVDC;
                        NewMNClient.PHouseNo = userInfoModify.PHouseNo;
                        NewMNClient.PWardNo = userInfoModify.PWardNo;
                        NewMNClient.PProvince = userInfoModify.PProvince;
                        NewMNClient.PDistrictID = userInfoModify.PDistrictID;
                        NewMNClient.Address = userInfoModify.Address;
                        NewMNClient.Name = userInfoModify.Name;

                        //NewMNClientContact.UserName = userInfoModify.UserName;
                        NewMNClientContact.EmailAddress = userInfoModify.EmailAddress;
                        NewMNClientContact.ContactNumber1 = userInfoModify.ContactNumber1;
                        NewMNClientContact.ContactNumber2 = userInfoModify.ContactNumber2;


                        NewMNBankAccountMap.BIN = userInfoModify.BankNo;
                        NewMNBankAccountMap.BranchCode = userInfoModify.BranchCode.Trim();
                        NewMNBankAccountMap.BankAccountNumber = userInfoModify.BankAccountNumber;

                        NewMNClientKYC.FName = userInfoModify.FName;
                        NewMNClientKYC.MName = userInfoModify.MName;
                        NewMNClientKYC.LName = userInfoModify.LName;
                        NewMNClientKYC.CitizenshipNo = userInfoModify.Citizenship;
                        NewMNClientKYC.PANNumber = userInfoModify.PanNo;
                        NewMNClientKYC.CitizenPlaceOfIssue = userInfoModify.CitizenshipPlaceOfIssue;
                        NewMNClientKYC.CitizenIssueDate =  DateTime.ParseExact(userInfoModify.CitizenshipIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        //NewMNClientKYC.CitizenIssueDate = userInfoModify.CitizenshipIssueDate;
                        NewMNClientKYC.BSCitizenIssueDate = userInfoModify.BSCitizenshipIssueDate;

                        NewMNMerchantKYCDoc.PassportImage = userInfoModify.PassportImageName;
                        NewMNMerchantKYCDoc.FrontImage = userInfoModify.FrontImageName;
                        NewMNMerchantKYCDoc.BackImage = userInfoModify.BackImageName;
                        NewMNMerchantKYCDoc.RegCertImage = userInfoModify.RegCertificatePhotoName;
                        NewMNMerchantKYCDoc.TaxClearFrontImage = userInfoModify.TaxClearFrontName;
                        NewMNMerchantKYCDoc.TaxClearBackImage = userInfoModify.TaxClearBackName;
                        /*map NEW data to Merchant--END*/

                        NewMerch.MNMerchants = NewData;
                        NewMerch.MNClient = NewMNClient;
                        NewMerch.MNClientContact = NewMNClientContact;
                        NewMerch.MNClientKYC = NewMNClientKYC;
                        NewMerch.MNMerchantKYCDoc = NewMNMerchantKYCDoc;
                        NewMerch.MNBankAccountMap = NewMNBankAccountMap;



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
                        DataTable dtableUserInfo = MerchantUtils.GetMerchantProfileInfo(id.ToString());
                        if (dtableUserInfo != null && dtableUserInfo.Rows.Count > 0)
                        {
                            regobj.ClientCode = dtableUserInfo.Rows[0]["ClientCode"].ToString();
                            regobj.PIN = dtableUserInfo.Rows[0]["PIN"].ToString();
                            regobj.Status = dtableUserInfo.Rows[0]["Status"].ToString();
                            regobj.UserName = dtableUserInfo.Rows[0]["UserName"].ToString();
                            regobj.UserType = dtableUserInfo.Rows[0]["userType"].ToString();
                            regobj.IsApproved = dtableUserInfo.Rows[0]["IsApproved"].ToString();
                            regobj.IsRejected = dtableUserInfo.Rows[0]["IsRejected"].ToString();
                            regobj.WalletNumber = dtableUserInfo.Rows[0]["WalletNumber"].ToString();
                            regobj.MerchantCategory = getMerchantCategoryId(id);

                            regobj.Name = dtableUserInfo.Rows[0]["Name"].ToString();
                            regobj.FName = dtableUserInfo.Rows[0]["FName"].ToString();
                            regobj.MName = dtableUserInfo.Rows[0]["MName"].ToString();
                            regobj.LName = dtableUserInfo.Rows[0]["LName"].ToString();

                            regobj.Address = dtableUserInfo.Rows[0]["Address"].ToString();
                            regobj.PStreet = dtableUserInfo.Rows[0]["PStreet"].ToString();
                            regobj.PVDC = dtableUserInfo.Rows[0]["PMunicipalityVDC"].ToString();
                            regobj.PHouseNo = dtableUserInfo.Rows[0]["PHouseNo"].ToString();
                            regobj.PWardNo = dtableUserInfo.Rows[0]["PWardNo"].ToString();
                            regobj.PDistrictID = dtableUserInfo.Rows[0]["PDistrictID"].ToString();
                            regobj.PProvince = dtableUserInfo.Rows[0]["PProvince"].ToString();

                            regobj.ContactNumber1 = dtableUserInfo.Rows[0]["ContactNumber1"].ToString();
                            regobj.ContactNumber2 = dtableUserInfo.Rows[0]["ContactNumber2"].ToString();
                            regobj.LandlineNumber = dtableUserInfo.Rows[0]["LandlineNumber"].ToString();
                            regobj.EmailAddress = dtableUserInfo.Rows[0]["EmailAddress"].ToString();

                            regobj.BusinessName = dtableUserInfo.Rows[0]["BusinessName"].ToString();
                            regobj.RegistrationNumber = dtableUserInfo.Rows[0]["RegistrationNumber"].ToString();
                            regobj.VATNumber = dtableUserInfo.Rows[0]["VATNumber"].ToString();
                            regobj.PanNo = dtableUserInfo.Rows[0]["PANNumber"].ToString();
                            regobj.WebsiteName = dtableUserInfo.Rows[0]["WebsiteName"].ToString();


                            regobj.Citizenship = dtableUserInfo.Rows[0]["CitizenshipNo"].ToString();
                            regobj.CitizenshipPlaceOfIssue = dtableUserInfo.Rows[0]["CitizenPlaceOfIssue"].ToString();
                            regobj.CitizenshipIssueDate = dtableUserInfo.Rows[0]["CitizenIssueDate"].ToString().Split()[0];
                            regobj.CitizenshipIssueDate = DateTime.Parse(regobj.CitizenshipIssueDate.Trim()).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                            //regobj.CitizenshipIssueDate = DateTime.Parse(regobj.CitizenshipIssueDate.Trim()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                            regobj.BSCitizenshipIssueDate = dtableUserInfo.Rows[0]["BSCitizenIssueDate"].ToString().Split()[0];

                            regobj.PassportImage = dtableUserInfo.Rows[0]["PassportImage"].ToString();
                            regobj.FrontImage = dtableUserInfo.Rows[0]["FrontImage"].ToString();
                            regobj.BackImage = dtableUserInfo.Rows[0]["BackImage"].ToString();
                            regobj.RegCertificateImage = dtableUserInfo.Rows[0]["RegCertImage"].ToString();
                            regobj.TaxClearFrontImage = dtableUserInfo.Rows[0]["TaxClearFrontImage"].ToString();
                            regobj.TaxClearBackImage = dtableUserInfo.Rows[0]["TaxClearBackImage"].ToString();

                            regobj.BankNo = dtableUserInfo.Rows[0]["BankNo"].ToString();
                            regobj.BranchCode = dtableUserInfo.Rows[0]["BranchCode"].ToString();
                            regobj.BankAccountNumber = dtableUserInfo.Rows[0]["BankAccountNumber"].ToString();

                            ViewBag.PProvince = new SelectList(list, "Value", "Text", regobj.PProvince);
                            ViewBag.PDistrictID = new SelectList(ProvinceToDistrict(regobj.PProvince), "Value", "Text", regobj.PDistrictID);

                        }

                        /*map old data to Merchant*/
                        MerchantTables OldMerch = new MerchantTables();
                        MNMerchants OldData = new MNMerchants();
                        MNClient OldMNClient = new MNClient();
                        MNClientContact OldMNClientContact = new MNClientContact();
                        MNClientKYC OldMNClientKYC = new MNClientKYC();
                        MNMerchantKYCDoc OldMNMerchantKYCDoc = new MNMerchantKYCDoc();
                        MNBankAccountMap OldMNBankAccountMap = new MNBankAccountMap();

                        OldData.mname = regobj.BusinessName;
                        OldData.ClientCode = regobj.ClientCode;
                        OldData.BusinessName = regobj.BusinessName;
                        OldData.RegistrationNumber = regobj.RegistrationNumber;
                        OldData.VATNumber = regobj.VATNumber;
                        OldData.WebsiteName = regobj.WebsiteName;
                        OldData.LandlineNumber = regobj.LandlineNumber;
                        OldData.catid = regobj.MerchantCategory;

                        OldMNClient.PStreet = regobj.PStreet;
                        OldMNClient.PMunicipalityVDC = regobj.PVDC;
                        OldMNClient.PHouseNo = regobj.PHouseNo;
                        OldMNClient.PWardNo = regobj.PWardNo;
                        OldMNClient.PProvince = regobj.PProvince;
                        OldMNClient.PDistrictID = regobj.PDistrictID;
                        OldMNClient.Address = regobj.Address;
                        OldMNClient.Name = regobj.Name;

                        //OldMNClientContact.UserName = regobj.ContactNumber1;
                        OldMNClientContact.EmailAddress = regobj.EmailAddress;
                        OldMNClientContact.ContactNumber1 = regobj.ContactNumber1;
                        OldMNClientContact.ContactNumber2 = regobj.ContactNumber2;

                        OldMNBankAccountMap.BIN = regobj.BankNo;
                        OldMNBankAccountMap.BranchCode = regobj.BranchCode.Trim();
                        OldMNBankAccountMap.BankAccountNumber = regobj.BankAccountNumber;

                        OldMNClientKYC.FName = regobj.FName;
                        OldMNClientKYC.MName = regobj.MName;
                        OldMNClientKYC.LName = regobj.LName;
                        OldMNClientKYC.CitizenshipNo = regobj.Citizenship;
                        OldMNClientKYC.PANNumber = regobj.PanNo;
                        OldMNClientKYC.CitizenPlaceOfIssue = regobj.CitizenshipPlaceOfIssue;
                        //OldMNClientKYC.CitizenIssueDate = DateTime.ParseExact(regobj.CitizenshipIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        OldMNClientKYC.CitizenIssueDate = regobj.CitizenshipIssueDate;
                        //OldMNClientKYC.CitizenIssueDate = DateTime.ParseExact(OldMNClientKYC.CitizenIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        OldMNClientKYC.BSCitizenIssueDate = regobj.BSCitizenshipIssueDate;

                        OldMNMerchantKYCDoc.PassportImage = regobj.PassportImage;
                        OldMNMerchantKYCDoc.FrontImage = regobj.FrontImage;
                        OldMNMerchantKYCDoc.BackImage = regobj.BackImage;
                        OldMNMerchantKYCDoc.RegCertImage = regobj.RegCertificateImage;
                        OldMNMerchantKYCDoc.TaxClearFrontImage = regobj.TaxClearFrontImage;
                        OldMNMerchantKYCDoc.TaxClearBackImage = regobj.TaxClearBackImage;
                        /*map old data to Merchant--END*/

                        OldMerch.MNMerchants = OldData;
                        OldMerch.MNClient = OldMNClient;
                        OldMerch.MNClientContact = OldMNClientContact;
                        OldMerch.MNClientKYC = OldMNClientKYC;
                        OldMerch.MNMerchantKYCDoc = OldMNMerchantKYCDoc;
                        OldMerch.MNBankAccountMap = OldMNBankAccountMap;


                        /*Difference compare*/
                        bool isUpdated = false;
                        CompareLogic compareLogic = new CompareLogic();
                        ComparisonConfig config = new ComparisonConfig();
                        config.MaxDifferences = int.MaxValue;
                        config.IgnoreCollectionOrder = false;
                        compareLogic.Config = config;
                        ComparisonResult result = compareLogic.Compare(OldMerch, NewMerch); //  firstparameter orginal,second parameter modified
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
                                makerchecker.Module = "SUPERADMIN";
                                makerCheckers.Add(makerchecker);
                            }
                        }


                        if (isUpdated)
                        {

                            string modifyingAdmin = (string)Session["UserName"];
                            string modifyingBranch = (string)Session["UserBranch"];
                            CusProfileUtils cUtil = new CusProfileUtils();
                            string ModifiedFieldXML = cUtil.GetMakerCheckerXMLStr(makerCheckers);
                            bool inserted = MerchantUtils.InsertMerchantMakerChecker(cClientCode, modifyingAdmin, modifyingBranch, ModifiedFieldXML);

                            displayMessage = inserted
                                                    ? "Merchant Information for " + userInfoModify.Name + " has successfully been updated. Please go to Approve Modification and perform accordingly."
                                                    : "Error while updating Merchant Information";
                            messageClass = inserted ? CssSetting.SuccessMessageClass : CssSetting.FailedMessageClass;

                        }
                        else
                        {
                            displayMessage = "Nothing Changed";
                            messageClass = CssSetting.SuccessMessageClass;
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
        
        #region "GET: ApproveModification--- List"
        [HttpGet]
        public ActionResult ApproveModification(string Value)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            String UserName = Value;

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

                string IsModified = "T";
                List<UserInfo> userobj = new List<UserInfo>();

                /// <summary>
                /// UNAPPROVE REJECTED LIST OF Merchant
                /// 
                userobj = MerchantUtils.GetModifiedMerchantList(IsModified, UserName);
                //DataTable dtableUNApproveReject = ProfileUtils.GetUnApproveRJCustomerProfile();



                return View(userobj);

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        #endregion
        
        #region "Approve/Reject Merchant"
        // GET: Merchant/Modification
        [HttpGet]
        public ActionResult ApproveRejectModify(string id)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

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
            

            //Merchant Category selectlist Begin
            string merchantstring = "SELECT * FROM MNMerchant (NOLOCK)";
            DataTable dt1 = new DataTable();
            dt1 = objdal.MyMethod(merchantstring);
            List<SelectListItem> list1 = new List<SelectListItem>();
            foreach (DataRow row in dt1.Rows)
            {
                list1.Add(new SelectListItem
                {
                    Text = Convert.ToString(row.ItemArray[1]),
                    Value = Convert.ToString(row.ItemArray[0])
                });
            }
            ViewBag.MerchantCategory = list1;//MerchantUtils.GetMerchantsType();
                                             //Merchant Category selectlist End


            if (TempData["userType"] != null && id != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                
                UserInfo regobj = new UserInfo();
                
                DataTable dtableUserInfo = MerchantUtils.GetMerchantProfileInfo(id.ToString());

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
                    regobj.MerchantCategory = getMerchantCategory(id);
                    regobj.PDistrictID = getDistrictName(dtableUserInfo.Rows[0]["PDistrictID"].ToString());
                    regobj.PProvince = getProvince(dtableUserInfo.Rows[0]["PProvince"].ToString());
                    regobj.PanNo = dtableUserInfo.Rows[0]["PANNumber"].ToString();
                    regobj.WebsiteName = dtableUserInfo.Rows[0]["WebsiteName"].ToString();
                    regobj.FName = dtableUserInfo.Rows[0]["FName"].ToString();
                    regobj.MName = dtableUserInfo.Rows[0]["MName"].ToString();
                    regobj.LName = dtableUserInfo.Rows[0]["LName"].ToString();
                    regobj.BusinessName = dtableUserInfo.Rows[0]["BusinessName"].ToString();
                    regobj.RegistrationNumber = dtableUserInfo.Rows[0]["RegistrationNumber"].ToString();
                    regobj.VATNumber = dtableUserInfo.Rows[0]["VATNumber"].ToString();
                    regobj.PStreet = dtableUserInfo.Rows[0]["PStreet"].ToString();
                    regobj.PVDC = dtableUserInfo.Rows[0]["PMunicipalityVDC"].ToString();
                    regobj.PHouseNo = dtableUserInfo.Rows[0]["PHouseNo"].ToString();
                    regobj.PWardNo = dtableUserInfo.Rows[0]["PWardNo"].ToString();
                    regobj.LandlineNumber = dtableUserInfo.Rows[0]["LandlineNumber"].ToString();
                    regobj.EmailAddress = dtableUserInfo.Rows[0]["EmailAddress"].ToString();

                    regobj.Citizenship = dtableUserInfo.Rows[0]["CitizenshipNo"].ToString();
                    regobj.CitizenshipPlaceOfIssue = dtableUserInfo.Rows[0]["CitizenPlaceOfIssue"].ToString();
                    regobj.CitizenshipIssueDate = dtableUserInfo.Rows[0]["CitizenIssueDate"].ToString().Split()[0];
                    regobj.CitizenshipIssueDate = DateTime.Parse(regobj.CitizenshipIssueDate.Trim()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                    regobj.BSCitizenshipIssueDate = dtableUserInfo.Rows[0]["BSCitizenIssueDate"].ToString().Split()[0];
                    
                    regobj.PassportImage = dtableUserInfo.Rows[0]["PassportImage"].ToString();
                    regobj.FrontImage = dtableUserInfo.Rows[0]["FrontImage"].ToString();
                    regobj.BackImage = dtableUserInfo.Rows[0]["BackImage"].ToString();
                    regobj.RegCertificateImage = dtableUserInfo.Rows[0]["RegCertImage"].ToString();
                    regobj.TaxClearFrontImage = dtableUserInfo.Rows[0]["TaxClearFrontImage"].ToString();
                    regobj.TaxClearBackImage = dtableUserInfo.Rows[0]["TaxClearBackImage"].ToString();

                    regobj.BankNo = dtableUserInfo.Rows[0]["BankNo"].ToString();
                    regobj.BranchCode = dtableUserInfo.Rows[0]["BranchCode"].ToString();
                    regobj.BankAccountNumber = dtableUserInfo.Rows[0]["BankAccountNumber"].ToString();
                    regobj.MerchantCategory = getMerchantCategory(id);
                    regobj.Remarks = dtableUserInfo.Rows[0]["Remarks"].ToString();

                    ViewBag.PProvince = new SelectList(list, "Value", "Text", regobj.PProvince);
                    ViewBag.PDistrictID = new SelectList(ProvinceToDistrict(regobj.PProvince), "Value", "Text", regobj.PDistrictID);
                    ViewBag.MerchantCategory = new SelectList(list1, "Value", "Text", getMerchantCategoryId(id));
                }

                DataSet DSetMakerchecker = MerchantUtils.GetSuperAdminMerchantModifiedValue(id.ToString());
                List<MNMakerChecker> ModifiedValues = ExtraUtility.DatatableToListClass<MNMakerChecker>(DSetMakerchecker.Tables["MNMakerChecker"]);
                regobj.MakerChecker = ModifiedValues;
                
                return View(regobj);
            }

            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        #endregion
        
        #region "Approve/Reject Merchant Modification when the approve/reject button is triggered"
        [HttpPost]
        public ActionResult ApproveMerchantModify(UserInfo model, string btnApprove)
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
                    userInfoApproval.AdminUserName = (string)Session["LOGGED_USERNAME"];
                    userInfoApproval.AdminBranch = (string)Session["LOGGEDUSER_ID"];

                    bool rejected = MerchantUtils.RejectMerchantModified(userInfoApproval);
                    if (rejected)
                    {
                        displayMessage = "Merchant has been Rejected. Please Check Rejectlist and perform accordingly";
                        messageClass = CssSetting.SuccessMessageClass;
                    }
                    else
                    {
                        displayMessage = "Error while rejecting Merchant " + model.Name;
                        messageClass = CssSetting.FailedMessageClass;
                    }

                    this.TempData["merchant_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;
                    return RedirectToAction("ApproveModification");

                }
                else if (btnApprove.ToUpper() == "APPROVE")
                {

                    userInfoApproval.AdminUserName = (string)Session["UserName"];
                    

                    bool Approved = MerchantUtils.MerchantModifyApprove(userInfoApproval);
                    if (Approved)
                    {
                        displayMessage = "Modification Information for Merchant name " + model.Name + " has successfully been approved.";
                        messageClass = CssSetting.SuccessMessageClass;
                    }
                    else
                    {
                        displayMessage = "Error while approving Merchant Information";
                        messageClass = CssSetting.FailedMessageClass;
                    }

                    this.TempData["merchant_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;
                    return RedirectToAction("ApproveModification");
                }
                else
                {

                    this.TempData["agentapprove_messsage"] = "Unauthorized execution";
                    this.TempData["message_class"] = CssSetting.FailedMessageClass;

                    return RedirectToAction("RegistrationRejected", "Merchant");
                }
            }            
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        #endregion
        
        #region "GET: Rejected Lists of Merchant "
        // GET: Merchant/Index
        [HttpGet]
        public ActionResult ModificationRejected(string Value)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            string UserName = Value;
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

                
                userobj = MerchantUtils.GetMerchantModificationRejectList(IsModified, UserName);
                
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
        public ActionResult RejectedModify(string id)
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

            //Merchant Category selectlist Begin
            string merchantstring = "SELECT * FROM MNMerchant (NOLOCK)";
            DataTable dt1 = new DataTable();
            dt1 = objdal.MyMethod(merchantstring);
            List<SelectListItem> list1 = new List<SelectListItem>();
            foreach (DataRow row in dt1.Rows)
            {
                list1.Add(new SelectListItem
                {
                    Text = Convert.ToString(row.ItemArray[1]),
                    Value = Convert.ToString(row.ItemArray[0])
                });
            }
            ViewBag.MerchantCategory = list1;//MerchantUtils.GetMerchantsType();
            //Merchant Category selectlist End

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

                DataTable dtableUserInfo = MerchantUtils.GetMerchantProfileInfo(id.ToString());

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
                    regobj.MerchantCategory = getMerchantCategory(id);
                    regobj.FName = dtableUserInfo.Rows[0]["FName"].ToString();
                    regobj.MName = dtableUserInfo.Rows[0]["MName"].ToString();
                    regobj.LName = dtableUserInfo.Rows[0]["LName"].ToString();
                    regobj.BusinessName = dtableUserInfo.Rows[0]["BusinessName"].ToString();
                    regobj.RegistrationNumber = dtableUserInfo.Rows[0]["RegistrationNumber"].ToString();
                    regobj.VATNumber = dtableUserInfo.Rows[0]["VATNumber"].ToString();
                    regobj.PStreet = dtableUserInfo.Rows[0]["PStreet"].ToString();
                    regobj.PVDC = dtableUserInfo.Rows[0]["PMunicipalityVDC"].ToString();
                    regobj.PHouseNo = dtableUserInfo.Rows[0]["PHouseNo"].ToString();
                    regobj.PWardNo = dtableUserInfo.Rows[0]["PWardNo"].ToString();
                    regobj.PanNo = dtableUserInfo.Rows[0]["PANNumber"].ToString();
                    regobj.WebsiteName = dtableUserInfo.Rows[0]["WebsiteName"].ToString();
                    regobj.LandlineNumber = dtableUserInfo.Rows[0]["LandlineNumber"].ToString();
                    regobj.EmailAddress = dtableUserInfo.Rows[0]["EmailAddress"].ToString();
                    regobj.MerchantCategory = getMerchantCategory(id);
                    regobj.CitizenshipPlaceOfIssue = dtableUserInfo.Rows[0]["CitizenPlaceOfIssue"].ToString();
                    regobj.Citizenship = dtableUserInfo.Rows[0]["CitizenshipNo"].ToString();
                    regobj.CitizenshipIssueDate = dtableUserInfo.Rows[0]["CitizenIssueDate"].ToString().Split()[0];
                    regobj.CitizenshipIssueDate = DateTime.Parse(regobj.CitizenshipIssueDate.Trim()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                    regobj.BSCitizenshipIssueDate = dtableUserInfo.Rows[0]["BSCitizenIssueDate"].ToString().Split()[0];

                    regobj.PassportImage = dtableUserInfo.Rows[0]["PassportImage"].ToString();
                    regobj.FrontImage = dtableUserInfo.Rows[0]["FrontImage"].ToString();
                    regobj.BackImage = dtableUserInfo.Rows[0]["BackImage"].ToString();
                    regobj.RegCertificateImage = dtableUserInfo.Rows[0]["RegCertImage"].ToString();
                    regobj.TaxClearFrontImage = dtableUserInfo.Rows[0]["TaxClearFrontImage"].ToString();
                    regobj.TaxClearBackImage = dtableUserInfo.Rows[0]["TaxClearBackImage"].ToString();

                    regobj.BankNo = dtableUserInfo.Rows[0]["BankNo"].ToString();
                    regobj.BranchCode = dtableUserInfo.Rows[0]["BranchCode"].ToString();
                    regobj.BankAccountNumber = dtableUserInfo.Rows[0]["BankAccountNumber"].ToString();

                    ViewBag.PProvince = new SelectList(list, "Value", "Text", regobj.PProvince);
                    ViewBag.PDistrictID = new SelectList(ProvinceToDistrict(regobj.PProvince), "Value", "Text", regobj.PDistrictID);
                    ViewBag.MerchantCategory = new SelectList(list1, "Value", "Text", getMerchantCategoryId(id));

                    regobj.Remarks = dtableUserInfo.Rows[0]["Remarks"].ToString();

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
       
        [HttpPost]
        public async Task<ActionResult> RejectedModify(string btnCommand, string id, FormCollection collection, UserInfo model)
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
                        ViewBag.Bank = item;

                        string cClientCode = collection["ClientCode"].ToString();
                        string cStatus = collection["txtStatus"].ToString();
                        string cContactNumber1 = collection["ContactNumber1"].ToString();
                        string cContactNumber2 = collection["ContactNumber2"].ToString();
                        string cBranchCode = collection["BranchCode"].ToString();
                        string cBankNo = collection["BankNo"].ToString();
                        string cBankAccountNumber = collection["BankAccountNumber"].ToString();
                        string cIsApproved = collection["txtIsApproved"].ToString();
                        string cIsRejected = collection["txtIsRejected"].ToString();
                        userInfoModify.MerchantCategory = collection["MerchantCategory"].ToString();

                        userInfoModify.PProvince = collection["PProvince"].ToString();
                        userInfoModify.PDistrictID = collection["PDistrictID"].ToString();

                        userInfoModify.IsModified = "T";

                        userInfoModify.ClientCode = cClientCode;

                        userInfoModify.EmailAddress = "";

                        userInfoModify.Status = cStatus;
                        userInfoModify.ContactNumber1 = cContactNumber1;
                        userInfoModify.ContactNumber2 = cContactNumber2;

                        userInfoModify.BankNo = cBankNo;
                        userInfoModify.BranchCode = cBankAccountNumber.Substring(0, 3);
                        userInfoModify.BankAccountNumber = cBankAccountNumber;
                        userInfoModify.IsApproved = cIsApproved;

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
                        userInfoModify.PanNo = collection["txtPanNo"].ToString();

                        userInfoModify.Citizenship = collection["txtCitizenship"].ToString();
                        userInfoModify.CitizenshipPlaceOfIssue = collection["txtCitizenshipPlaceOfIssue"].ToString();
                        userInfoModify.CitizenshipIssueDate = collection["CitizenshipIssueDate"].ToString();
                        userInfoModify.BSCitizenshipIssueDate = collection["BSCitizenshipIssueDate"].ToString();

                        string cPPImage = collection["txtOldPP"].ToString();
                        string cFrontImage = collection["txtOldfront"].ToString();
                        string cBackImage = collection["txtOldback"].ToString();
                        string cRegCertiImage = collection["txtOldRegCerti"].ToString();
                        string cTaxClearFrontImage = collection["txtOldTaxClearFront"].ToString();
                        string cTaxClearBackImage = collection["txtOldTaxClearBack"].ToString();

                        userInfoModify.WebsiteName = collection["txtWebsiteName"].ToString();
                        userInfoModify.Name = userInfoModify.BusinessName; //+ " " + userInfoModify.MName + " " + userInfoModify.LName;
                        userInfoModify.Address = userInfoModify.PVDC + " " + userInfoModify.PDistrictID + " " + userInfoModify.PProvince;

                        var PP = ReturnFileName(model.PassportPhoto, userInfoModify.UserName);
                        var front = ReturnFileName(model.Front, userInfoModify.UserName);
                        var back = ReturnFileName(model.Back, userInfoModify.UserName);
                        var regCert = ReturnFileName(model.RegCertificatePhoto, userInfoModify.UserName);
                        var taxClearFront = ReturnFileName(model.TaxClearFront, userInfoModify.UserName);
                        var taxDoc = ReturnFileName(model.TaxClearBack, userInfoModify.UserName);

                        if (PP != null)
                        {
                            if (!string.IsNullOrEmpty(cPPImage))
                            {
                                //var existingFile = Path.Combine(cPassportPhoto);
                                var existingFile = Request.MapPath(cPPImage);
                                if (System.IO.File.Exists(existingFile))
                                {
                                    System.IO.File.Delete(existingFile);
                                }
                            }
                            userInfoModify.PassportImage = ParseCv(model.PassportPhoto);
                            userInfoModify.PassportImageName = string.Format(PP);
                        }
                        else
                        {
                            userInfoModify.PassportImageName = "";
                        }
                        if (front != null)
                        {
                            if (!string.IsNullOrEmpty(cFrontImage))
                            {
                                //var existingFile = Path.Combine(cPassportPhoto);
                                var existingFile = Request.MapPath(cFrontImage);
                                if (System.IO.File.Exists(existingFile))
                                {
                                    System.IO.File.Delete(existingFile);
                                }
                            }
                            userInfoModify.FrontImage = ParseCv(model.Front);
                            userInfoModify.FrontImageName = string.Format(front);
                        }
                        else
                        {
                            userInfoModify.FrontImageName = "";
                        }
                        if (back != null)
                        {
                            if (!string.IsNullOrEmpty(cBackImage))
                            {
                                //var existingFile = Path.Combine(cPassportPhoto);
                                var existingFile = Request.MapPath(cBackImage);
                                if (System.IO.File.Exists(existingFile))
                                {
                                    System.IO.File.Delete(existingFile);
                                }
                            }
                            userInfoModify.BackImage = ParseCv(model.Back);
                            userInfoModify.BackImageName = string.Format(back);
                        }
                        else
                        {
                            userInfoModify.BackImageName = "";
                        }
                        if (regCert != null)
                        {
                            if (!string.IsNullOrEmpty(cRegCertiImage))
                            {
                                //var existingFile = Path.Combine(cPassportPhoto);
                                var existingFile = Request.MapPath(cRegCertiImage);
                                if (System.IO.File.Exists(existingFile))
                                {
                                    System.IO.File.Delete(existingFile);
                                }
                            }
                            userInfoModify.RegCertificateImage = ParseCv(model.RegCertificatePhoto);
                            userInfoModify.RegCertificatePhotoName = string.Format(regCert);
                        }
                        else
                        {
                            userInfoModify.RegCertificatePhotoName = "";
                        }

                        if (taxClearFront != null)
                        {
                            if (!string.IsNullOrEmpty(cTaxClearFrontImage))
                            {
                                //var existingFile = Path.Combine(cPassportPhoto);
                                var existingFile = Request.MapPath(cTaxClearFrontImage);
                                if (System.IO.File.Exists(existingFile))
                                {
                                    System.IO.File.Delete(existingFile);
                                }
                            }
                            userInfoModify.TaxClearFrontImage = ParseCv(model.TaxClearFront);
                            userInfoModify.TaxClearFrontName = string.Format(taxClearFront);
                        }
                        else
                        {
                            userInfoModify.TaxClearFrontName = "";
                        }
                        if (taxDoc != null)
                        {
                            if (!string.IsNullOrEmpty(cTaxClearBackImage))
                            {
                                //var existingFile = Path.Combine(cPassportPhoto);
                                var existingFile = Request.MapPath(cTaxClearBackImage);
                                if (System.IO.File.Exists(existingFile))
                                {
                                    System.IO.File.Delete(existingFile);
                                }
                            }
                            userInfoModify.TaxClearBackImage = ParseCv(model.TaxClearBack);
                            userInfoModify.TaxClearBackName = string.Format(taxDoc);
                        }
                        else
                        {
                            userInfoModify.TaxClearBackName = "";
                        }

                        await SavePhoto(userInfoModify);

                        string sPP = Session["PP"].ToString();
                        string sFront = Session["Front"].ToString();
                        string sBack = Session["Back"].ToString();
                        string sRegCert = Session["RegCerti"].ToString();
                        string sTaxClearFront = Session["TaxClearFront"].ToString();
                        string sTaxClearBack = Session["TaxClearBack"].ToString();

                        if (sPP != "")
                        {
                            userInfoModify.PassportImageName = Session["PP"].ToString();
                        }
                        else
                        {
                            userInfoModify.PassportImageName = cPPImage;
                        }
                        if (sFront != "")
                        {
                            userInfoModify.FrontImageName = Session["Front"].ToString();
                        }
                        else
                        {
                            userInfoModify.FrontImageName = cFrontImage;
                        }
                        if (sBack != "")
                        {
                            userInfoModify.BackImageName = Session["Back"].ToString();
                        }
                        else
                        {
                            userInfoModify.BackImageName = cBackImage;
                        }
                        if (sRegCert != "")
                        {
                            userInfoModify.RegCertificatePhotoName = Session["RegCerti"].ToString();
                        }
                        else
                        {
                            userInfoModify.RegCertificatePhotoName = cRegCertiImage;
                        }
                        if (sTaxClearFront != "")
                        {
                            userInfoModify.TaxClearFrontName = Session["TaxClearFront"].ToString();
                        }
                        else
                        {
                            userInfoModify.TaxClearFrontName = cTaxClearFrontImage;
                        }
                        if (sTaxClearBack != "")
                        {
                            userInfoModify.TaxClearBackName = Session["TaxClearBack"].ToString();
                        }
                        else
                        {
                            userInfoModify.TaxClearBackName = cTaxClearBackImage;
                        }

                        /*map new data to Merchant*/
                        MerchantTables NewMerch = new MerchantTables();
                        MNMerchants NewData = new MNMerchants();
                        MNClient NewMNClient = new MNClient();
                        MNClientContact NewMNClientContact = new MNClientContact();
                        MNClientKYC NewMNClientKYC = new MNClientKYC();
                        MNMerchantKYCDoc NewMNMerchantKYCDoc = new MNMerchantKYCDoc();
                        MNBankAccountMap NewMNBankAccountMap = new MNBankAccountMap();

                        NewData.mname = userInfoModify.BusinessName;
                        NewData.ClientCode = userInfoModify.ClientCode;
                        NewData.BusinessName = userInfoModify.BusinessName;
                        NewData.RegistrationNumber = userInfoModify.RegistrationNumber;
                        NewData.VATNumber = userInfoModify.VATNumber;
                        NewData.WebsiteName = userInfoModify.WebsiteName;
                        NewData.LandlineNumber = userInfoModify.LandlineNumber;
                        NewData.catid = userInfoModify.MerchantCategory;

                        NewMNClient.PStreet = userInfoModify.PStreet;
                        NewMNClient.PMunicipalityVDC = userInfoModify.PVDC;
                        NewMNClient.PHouseNo = userInfoModify.PHouseNo;
                        NewMNClient.PWardNo = userInfoModify.PWardNo;
                        NewMNClient.PProvince = userInfoModify.PProvince;
                        NewMNClient.PDistrictID = userInfoModify.PDistrictID;
                        NewMNClient.Address = userInfoModify.Address;
                        NewMNClient.Name = userInfoModify.Name;

                        //NewMNClientContact.UserName = userInfoModify.UserName;
                        NewMNClientContact.EmailAddress = userInfoModify.EmailAddress;
                        NewMNClientContact.ContactNumber1 = userInfoModify.ContactNumber1;
                        NewMNClientContact.ContactNumber2 = userInfoModify.ContactNumber2;


                        NewMNBankAccountMap.BIN = userInfoModify.BankNo;
                        NewMNBankAccountMap.BranchCode = userInfoModify.BranchCode.Trim();
                        NewMNBankAccountMap.BankAccountNumber = userInfoModify.BankAccountNumber;

                        NewMNClientKYC.FName = userInfoModify.FName;
                        NewMNClientKYC.MName = userInfoModify.MName;
                        NewMNClientKYC.LName = userInfoModify.LName;
                        NewMNClientKYC.CitizenshipNo = userInfoModify.Citizenship;
                        NewMNClientKYC.PANNumber = userInfoModify.PanNo;
                        NewMNClientKYC.CitizenPlaceOfIssue = userInfoModify.CitizenshipPlaceOfIssue;
                        NewMNClientKYC.CitizenIssueDate = DateTime.ParseExact(userInfoModify.CitizenshipIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        //NewMNClientKYC.CitizenIssueDate = userInfoModify.CitizenshipIssueDate;
                        NewMNClientKYC.BSCitizenIssueDate = userInfoModify.BSCitizenshipIssueDate;

                        NewMNMerchantKYCDoc.PassportImage = userInfoModify.PassportImageName;
                        NewMNMerchantKYCDoc.FrontImage = userInfoModify.FrontImageName;
                        NewMNMerchantKYCDoc.BackImage = userInfoModify.BackImageName;
                        NewMNMerchantKYCDoc.RegCertImage = userInfoModify.RegCertificatePhotoName;
                        NewMNMerchantKYCDoc.TaxClearFrontImage = userInfoModify.TaxClearFrontName;
                        NewMNMerchantKYCDoc.TaxClearBackImage = userInfoModify.TaxClearBackName;
                        /*map NEW data to Merchant--END*/

                        NewMerch.MNMerchants = NewData;
                        NewMerch.MNClient = NewMNClient;
                        NewMerch.MNClientContact = NewMNClientContact;
                        NewMerch.MNClientKYC = NewMNClientKYC;
                        NewMerch.MNMerchantKYCDoc = NewMNMerchantKYCDoc;
                        NewMerch.MNBankAccountMap = NewMNBankAccountMap;



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
                        DataTable dtableUserInfo = MerchantUtils.GetMerchantProfileInfo(id.ToString());
                        if (dtableUserInfo != null && dtableUserInfo.Rows.Count > 0)
                        {
                            regobj.ClientCode = dtableUserInfo.Rows[0]["ClientCode"].ToString();
                            regobj.PIN = dtableUserInfo.Rows[0]["PIN"].ToString();
                            regobj.Status = dtableUserInfo.Rows[0]["Status"].ToString();
                            regobj.UserName = dtableUserInfo.Rows[0]["UserName"].ToString();
                            regobj.UserType = dtableUserInfo.Rows[0]["userType"].ToString();
                            regobj.IsApproved = dtableUserInfo.Rows[0]["IsApproved"].ToString();
                            regobj.IsRejected = dtableUserInfo.Rows[0]["IsRejected"].ToString();
                            regobj.WalletNumber = dtableUserInfo.Rows[0]["WalletNumber"].ToString();
                            regobj.MerchantCategory = getMerchantCategoryId(id);

                            regobj.Name = dtableUserInfo.Rows[0]["Name"].ToString();
                            regobj.FName = dtableUserInfo.Rows[0]["FName"].ToString();
                            regobj.MName = dtableUserInfo.Rows[0]["MName"].ToString();
                            regobj.LName = dtableUserInfo.Rows[0]["LName"].ToString();

                            regobj.Address = dtableUserInfo.Rows[0]["Address"].ToString();
                            regobj.PStreet = dtableUserInfo.Rows[0]["PStreet"].ToString();
                            regobj.PVDC = dtableUserInfo.Rows[0]["PMunicipalityVDC"].ToString();
                            regobj.PHouseNo = dtableUserInfo.Rows[0]["PHouseNo"].ToString();
                            regobj.PWardNo = dtableUserInfo.Rows[0]["PWardNo"].ToString();
                            regobj.PDistrictID = dtableUserInfo.Rows[0]["PDistrictID"].ToString();
                            regobj.PProvince = dtableUserInfo.Rows[0]["PProvince"].ToString();

                            regobj.ContactNumber1 = dtableUserInfo.Rows[0]["ContactNumber1"].ToString();
                            regobj.ContactNumber2 = dtableUserInfo.Rows[0]["ContactNumber2"].ToString();
                            regobj.LandlineNumber = dtableUserInfo.Rows[0]["LandlineNumber"].ToString();
                            regobj.EmailAddress = dtableUserInfo.Rows[0]["EmailAddress"].ToString();

                            regobj.BusinessName = dtableUserInfo.Rows[0]["BusinessName"].ToString();
                            regobj.RegistrationNumber = dtableUserInfo.Rows[0]["RegistrationNumber"].ToString();
                            regobj.VATNumber = dtableUserInfo.Rows[0]["VATNumber"].ToString();
                            regobj.PanNo = dtableUserInfo.Rows[0]["PANNumber"].ToString();
                            regobj.WebsiteName = dtableUserInfo.Rows[0]["WebsiteName"].ToString();


                            regobj.Citizenship = dtableUserInfo.Rows[0]["CitizenshipNo"].ToString();
                            regobj.CitizenshipPlaceOfIssue = dtableUserInfo.Rows[0]["CitizenPlaceOfIssue"].ToString();
                            regobj.CitizenshipIssueDate = dtableUserInfo.Rows[0]["CitizenIssueDate"].ToString().Split()[0];
                            regobj.CitizenshipIssueDate = DateTime.Parse(regobj.CitizenshipIssueDate.Trim()).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                            //regobj.CitizenshipIssueDate = DateTime.Parse(regobj.CitizenshipIssueDate.Trim()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                            regobj.BSCitizenshipIssueDate = dtableUserInfo.Rows[0]["BSCitizenIssueDate"].ToString().Split()[0];

                            regobj.PassportImage = dtableUserInfo.Rows[0]["PassportImage"].ToString();
                            regobj.FrontImage = dtableUserInfo.Rows[0]["FrontImage"].ToString();
                            regobj.BackImage = dtableUserInfo.Rows[0]["BackImage"].ToString();
                            regobj.RegCertificateImage = dtableUserInfo.Rows[0]["RegCertImage"].ToString();
                            regobj.TaxClearFrontImage = dtableUserInfo.Rows[0]["TaxClearFrontImage"].ToString();
                            regobj.TaxClearBackImage = dtableUserInfo.Rows[0]["TaxClearBackImage"].ToString();

                            regobj.BankNo = dtableUserInfo.Rows[0]["BankNo"].ToString();
                            regobj.BranchCode = dtableUserInfo.Rows[0]["BranchCode"].ToString();
                            regobj.BankAccountNumber = dtableUserInfo.Rows[0]["BankAccountNumber"].ToString();

                            ViewBag.PProvince = new SelectList(list, "Value", "Text", regobj.PProvince);
                            ViewBag.PDistrictID = new SelectList(ProvinceToDistrict(regobj.PProvince), "Value", "Text", regobj.PDistrictID);

                        }

                        /*map old data to Merchant*/
                        MerchantTables OldMerch = new MerchantTables();
                        MNMerchants OldData = new MNMerchants();
                        MNClient OldMNClient = new MNClient();
                        MNClientContact OldMNClientContact = new MNClientContact();
                        MNClientKYC OldMNClientKYC = new MNClientKYC();
                        MNMerchantKYCDoc OldMNMerchantKYCDoc = new MNMerchantKYCDoc();
                        MNBankAccountMap OldMNBankAccountMap = new MNBankAccountMap();

                        OldData.mname = regobj.BusinessName;
                        OldData.ClientCode = regobj.ClientCode;
                        OldData.BusinessName = regobj.BusinessName;
                        OldData.RegistrationNumber = regobj.RegistrationNumber;
                        OldData.VATNumber = regobj.VATNumber;
                        OldData.WebsiteName = regobj.WebsiteName;
                        OldData.LandlineNumber = regobj.LandlineNumber;
                        OldData.catid = regobj.MerchantCategory;

                        OldMNClient.PStreet = regobj.PStreet;
                        OldMNClient.PMunicipalityVDC = regobj.PVDC;
                        OldMNClient.PHouseNo = regobj.PHouseNo;
                        OldMNClient.PWardNo = regobj.PWardNo;
                        OldMNClient.PProvince = regobj.PProvince;
                        OldMNClient.PDistrictID = regobj.PDistrictID;
                        OldMNClient.Address = regobj.Address;
                        OldMNClient.Name = regobj.Name;

                        //OldMNClientContact.UserName = regobj.ContactNumber1;
                        OldMNClientContact.EmailAddress = regobj.EmailAddress;
                        OldMNClientContact.ContactNumber1 = regobj.ContactNumber1;
                        OldMNClientContact.ContactNumber2 = regobj.ContactNumber2;

                        OldMNBankAccountMap.BIN = regobj.BankNo;
                        OldMNBankAccountMap.BranchCode = regobj.BranchCode.Trim();
                        OldMNBankAccountMap.BankAccountNumber = regobj.BankAccountNumber;

                        OldMNClientKYC.FName = regobj.FName;
                        OldMNClientKYC.MName = regobj.MName;
                        OldMNClientKYC.LName = regobj.LName;
                        OldMNClientKYC.CitizenshipNo = regobj.Citizenship;
                        OldMNClientKYC.PANNumber = regobj.PanNo;
                        OldMNClientKYC.CitizenPlaceOfIssue = regobj.CitizenshipPlaceOfIssue;
                        //OldMNClientKYC.CitizenIssueDate = DateTime.ParseExact(regobj.CitizenshipIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        OldMNClientKYC.CitizenIssueDate = regobj.CitizenshipIssueDate;
                        //OldMNClientKYC.CitizenIssueDate = DateTime.ParseExact(OldMNClientKYC.CitizenIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        OldMNClientKYC.BSCitizenIssueDate = regobj.BSCitizenshipIssueDate;

                        OldMNMerchantKYCDoc.PassportImage = regobj.PassportImage;
                        OldMNMerchantKYCDoc.FrontImage = regobj.FrontImage;
                        OldMNMerchantKYCDoc.BackImage = regobj.BackImage;
                        OldMNMerchantKYCDoc.RegCertImage = regobj.RegCertificateImage;
                        OldMNMerchantKYCDoc.TaxClearFrontImage = regobj.TaxClearFrontImage;
                        OldMNMerchantKYCDoc.TaxClearBackImage = regobj.TaxClearBackImage;
                        /*map old data to Merchant--END*/

                        OldMerch.MNMerchants = OldData;
                        OldMerch.MNClient = OldMNClient;
                        OldMerch.MNClientContact = OldMNClientContact;
                        OldMerch.MNClientKYC = OldMNClientKYC;
                        OldMerch.MNMerchantKYCDoc = OldMNMerchantKYCDoc;
                        OldMerch.MNBankAccountMap = OldMNBankAccountMap;


                        /*Difference compare*/
                        bool isUpdated = false;
                        CompareLogic compareLogic = new CompareLogic();
                        ComparisonConfig config = new ComparisonConfig();
                        config.MaxDifferences = int.MaxValue;
                        config.IgnoreCollectionOrder = false;
                        compareLogic.Config = config;
                        ComparisonResult result = compareLogic.Compare(OldMerch, NewMerch); //  firstparameter orginal,second parameter modified
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
                                makerchecker.Module = "SUPERADMIN";
                                makerCheckers.Add(makerchecker);
                            }
                        }


                        if (isUpdated)
                        {
                            string modifyingAdmin = (string)Session["UserName"];
                            string modifyingBranch = (string)Session["UserBranch"];
                            CusProfileUtils cUtil = new CusProfileUtils();
                            string ModifiedFieldXML = cUtil.GetMakerCheckerXMLStr(makerCheckers);
                            bool inserted = MerchantUtils.InsertMerchantMakerChecker(cClientCode, modifyingAdmin, modifyingBranch, ModifiedFieldXML);

                            displayMessage = inserted
                                                    ? "Merchant Information for " + userInfoModify.Name + " has successfully been updated. Please go to Approve Modification and perform accordingly."
                                                    : "Error while updating Merchant Information";
                            messageClass = inserted ? CssSetting.SuccessMessageClass : CssSetting.FailedMessageClass;

                        }
                        else
                        {
                            displayMessage = "Nothing Changed";
                            messageClass = CssSetting.SuccessMessageClass;
                        }

                        this.TempData["merchant_messsage"] = displayMessage;
                        this.TempData["message_class"] = messageClass;

                        return RedirectToAction("ModificationRejected");
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


        #region Merchant Deactivation/Reactivation Search
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
        public ActionResult MerchantSearchInfo(string SearchCol, string txtMobileNo, string txtName, string txtAccountNo)
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

                if ((SearchCol == "Mobile Number"))
                {
                    DataTable dtableMerchantStatusByAc = MerchantUtils.GetMerchantUserProfileMobileOrName(userInfo.Name, userInfo.WalletNumber, userInfo.ContactNumber1);

                    if (dtableMerchantStatusByAc != null && dtableMerchantStatusByAc.Rows.Count > 0)
                    {
                        UserInfo regobj = new UserInfo();
                        regobj.ClientCode = dtableMerchantStatusByAc.Rows[0]["ClientCode"].ToString();
                        regobj.Name = dtableMerchantStatusByAc.Rows[0]["Name"].ToString();
                        regobj.Address = dtableMerchantStatusByAc.Rows[0]["Address"].ToString();
                        regobj.PIN = dtableMerchantStatusByAc.Rows[0]["PIN"].ToString();
                        regobj.Status = dtableMerchantStatusByAc.Rows[0]["Status"].ToString();
                        regobj.ContactNumber1 = dtableMerchantStatusByAc.Rows[0]["ContactNumber1"].ToString();
                        regobj.ContactNumber2 = dtableMerchantStatusByAc.Rows[0]["ContactNumber2"].ToString();
                        regobj.UserName = dtableMerchantStatusByAc.Rows[0]["UserName"].ToString();
                        regobj.UserType = dtableMerchantStatusByAc.Rows[0]["userType"].ToString();

                        customerStatus.Add(regobj);
                        ViewData["dtableMerchantStatus"] = dtableMerchantStatusByAc;
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


        #region Merchant Deactivation/Reactivation

        [HttpGet]
        public ActionResult MerchantStatusChange(string id)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            if (this.TempData["Merchantmodify_messsage"] != null)
            {
                this.ViewData["Merchantmodify_messsage"] = this.TempData["Merchantmodify_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                UserInfo regobj = new UserInfo();

                DataTable dtableUserInfo = MerchantUtils.GetMerchantProfileInfo(id.ToString());

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

        public ActionResult MerchantStatusChange(string btnCommand, FormCollection collection)
        {
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
                            bool isUpdated = MerchantUtils.UpdateMerchantStatus(cClientCode, cStatus, SAdminBranchCode, SAdminUserName, cBlockRemarks);
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

                        this.TempData["Merchantmodify_messsage"] = displayMessage;
                        this.TempData["message_class"] = messageClass;
                        return RedirectToAction("MerchantStatusChange");
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
                return RedirectToAction("MerchantStatusChange");

            }
            finally
            {
                this.TempData["Merchantmodify_messsage"] = displayMessage;
                this.TempData["message_class"] = messageClass;
            }
        }
        #endregion


        #region Approve Deactivation/Reactivation of Merchant
        [HttpGet]
        public ActionResult MerchantStatusChangedList(string MobileNumber)
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
                //string BranchCode = Session["UserBranch"].ToString();

                var model = MerchantUtils.GetMerchantStatus(MobileNumber);
                return View(model);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        //for Approve 
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
                bool isUpdated = false;

                UserInfo info = new UserInfo();
                DataTable dtableUserInfo = MerchantUtils.GetMerchantProfileInfo(ClientCode);
                if (dtableUserInfo != null && dtableUserInfo.Rows.Count > 0)
                {
                    info.ClientCode = dtableUserInfo.Rows[0]["ClientCode"].ToString();
                    info.Name = dtableUserInfo.Rows[0]["Name"].ToString();
                    info.Address = dtableUserInfo.Rows[0]["Address"].ToString();
                    info.Status = dtableUserInfo.Rows[0]["Status"].ToString();
                    info.UserName = dtableUserInfo.Rows[0]["UserName"].ToString();

                    //for name only
                    string[] tokens = info.Name.Split(' ');
                    string FirstName = tokens[0];

                    //added charmin
                    string Message = string.Empty;
                    if (info.Status == "Blocked")
                    {
                        info.Status = "Active";

                        /*Sms Message for block msg */

                        Message = "Dear" + " " + FirstName + ",\n";
                        Message += "Your account " + info.UserName + " for Merchant has been Unblocked, for detail please contact Administrator"
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
                        Message += "Your account 98******** for Merchant has been Blocked, for detail please contact Administrator"
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
                    Message = string.Empty;
                    string resp = string.Empty;
                    isUpdated = MerchantUtils.MerchantStatusReject(info.ClientCode, info.Status);

                    if (isUpdated)
                    {
                        resp = "Merchant Status approved successfully.";
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
                            StatusMessage = "Error Approving Merchant status"
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

        //for Reject 
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
                DataTable dtableUserInfo = MerchantUtils.GetMerchantProfileInfo(ClientCode);
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


                    string Message = string.Empty;
                    string resp = string.Empty;

                    isUpdated = MerchantUtils.MerchantStatusReject(info.ClientCode, info.Status);
                    if (isUpdated)
                    {
                        resp = "Merchant Status rejected successfully.";
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
                            StatusMessage = "Error rejecting Merchant status"
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


        #region "Merchant Pin/Password reset by superadmin"

        // GET: Merchant/PinResetList
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

                    List<UserInfo> MerchantList = new List<UserInfo>();


                    if (userInfo.ContactNumber1 != null)
                    {
                        DataTable dtableMerchantStatusByAc = MerchantUtils.GetMerchantUserProfileMobileOrName(userInfo.Name, userInfo.WalletNumber, userInfo.ContactNumber1);
                        if (dtableMerchantStatusByAc != null && dtableMerchantStatusByAc.Rows.Count > 0)
                        {
                            UserInfo regobj = new UserInfo();
                            regobj.ClientCode = dtableMerchantStatusByAc.Rows[0]["ClientCode"].ToString();
                            regobj.Name = dtableMerchantStatusByAc.Rows[0]["Name"].ToString();
                            regobj.Address = dtableMerchantStatusByAc.Rows[0]["Address"].ToString();
                            regobj.PIN = dtableMerchantStatusByAc.Rows[0]["PIN"].ToString();
                            regobj.Status = dtableMerchantStatusByAc.Rows[0]["Status"].ToString();
                            regobj.ContactNumber1 = dtableMerchantStatusByAc.Rows[0]["ContactNumber1"].ToString();
                            regobj.ContactNumber2 = dtableMerchantStatusByAc.Rows[0]["ContactNumber2"].ToString();
                            regobj.UserName = dtableMerchantStatusByAc.Rows[0]["UserName"].ToString();
                            regobj.UserType = dtableMerchantStatusByAc.Rows[0]["userType"].ToString();

                            MerchantList.Add(regobj);
                            ViewData["dtableMerchantList"] = dtableMerchantStatusByAc;
                        }
                    }
                    return View(MerchantList);
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
                return RedirectToAction("PinResetList", "Merchant");
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
                        return RedirectToAction("PinResetList", "Merchant");
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
                var model = MerchantUtils.GetPinResetList(AdminBranch, boolCOC, MobileNumber);
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
                        Message = string.Format("Dear {0},\n Your new T-Pin is {1}.\n Thank You -MNepal", info.Name, info.PIN);
                        resp = "T-Pin reset Successful for " + info.UserName;

                        //for email pin only

                        string Subject = "Merchant T-Pin Reset";

                        string MailSubject = "<span style='font-size:15px;'><h4>Dear " + info.Name + ",</h4>";
                        MailSubject += "Your T-Pin reset request for Merchant " + info.UserName + " has been acknowledged and you have been issued with a new temporary T-Pin.<br/>";

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
                        Message = string.Format("Dear {0},\n Your new Password is {1}.\n Thank You -MNepal", info.Name, info.Password);
                        resp = "Password reset Successful for " + info.UserName;

                        //for email password only

                        string Subject = "Merchant Password Reset";

                        string MailSubject = "<span style='font-size:15px;'><h4>Dear " + info.Name + ",</h4>";
                        MailSubject += "Your password reset request for Merchant " + info.UserName + " has been acknowledged and you have been issued with a new temporary password.<br/>";

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
                        Message = string.Format("Dear {0},\n Your new T-Pin is {1} and Password is {2}.\n Thank You -MNepal", info.Name, info.PIN, info.Password);
                        resp = "T-Pin and Password reset Successful for " + info.UserName.Split()[0];

                        //for email both T-Pin and password

                        string Subject = "Merchant Password/T-Pin Reset";

                        string MailSubject = "<span style='font-size:15px;'><h4>Dear " + info.Name + ",</h4>";
                        MailSubject += "Your password and T-Pin reset request for Merchant " + info.UserName + " has been acknowledged and you have been issued with a new temporary password and T-Pin.<br/>";

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
                            StatusMessage = "Error Approving Merchant pin"
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
                             StatusMessage = "Reverted Pin/password request for Merchant " + info.Name
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

        public string getMerchantCategory(string id)
        {
            string catagoryName = "select Name from MNMerchant where Id = (select catid from MNMerchants where ClientCode='" + id + "')";
            DataTable dt = new DataTable();
            dt = objdal.MyMethod(catagoryName);
            string name = string.Empty;
            foreach (DataRow row in dt.Rows)
            {
                name = row["Name"].ToString();
            }
            return name;
        }
        public string getMerchantCategoryId(string id)
        {
            string catagoryName = "select catid from MNMerchants where ClientCode='" + id + "'";
            DataTable dt = new DataTable();
            dt = objdal.MyMethod(catagoryName);
            string id1 = string.Empty;
            foreach (DataRow row in dt.Rows)
            {
                id1 = row["catid"].ToString();
            }
            return id1;
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

        #region FOR IMAGE
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
                    var action = "selfReg.svc/SaveImageMerchantAPI";
                    var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                    var content = new FormUrlEncodedContent(new[]{
                        new KeyValuePair<string, string>("mobile", srInfo.UserName),
                        new KeyValuePair<string,string>("frontImage", srInfo.FrontImage),
                        new KeyValuePair<string,string>("backImage",srInfo.BackImage),
                        new KeyValuePair<string,string>("ppImage", srInfo.PassportImage),
                        new KeyValuePair<string,string>("regCertiImage", srInfo.RegCertificateImage),
                        new KeyValuePair<string,string>("taxClearFrontImage", srInfo.TaxClearFrontImage),
                        new KeyValuePair<string,string>("taxClearBackImage", srInfo.TaxClearBackImage),
                        new KeyValuePair<string,string>("frontImageName", srInfo.FrontImageName),
                        new KeyValuePair<string,string>("backImageName", srInfo.BackImageName),
                        new KeyValuePair<string,string>("ppImageName", srInfo.PassportImageName),
                        new KeyValuePair<string,string>("regCertiImageName", srInfo.RegCertificatePhotoName),
                        new KeyValuePair<string,string>("taxClearFrontImageName", srInfo.TaxClearFrontName),
                        new KeyValuePair<string,string>("taxClearBackImageName", srInfo.TaxClearBackName)
                        //new KeyValuePair<string,string>("DocumentType", srInfo.Document),
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
                                Session["RegCerti"] = myNames.regcerti;
                                Session["TaxClearFront"] = myNames.taxClearFront;
                                Session["TaxClearBack"] = myNames.taxClearBack;

                                if (code != responseCode)
                                {
                                    responseCode = code;
                                }
                            }
                            return Json(new { responseCode = responseCode, responseText = respmsg/*, PP = PP*/ },
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

        #endregion
    }
}