using ThailiMNepalAgent.Helper;
using ThailiMNepalAgent.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.SessionState;
using ThailiMNepalAgent.App_Start;
using ThailiMNepalAgent.Utilities;
using System.Data;
using System.Linq;
using ThailiMNepalAgent.Settings;

namespace ThailiMNepalAgent.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class LoadWalletController : Controller
    {
        #region "Service Code "
        public ActionResult GetServiceCode(string txtFundTransferType)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                ViewData["userType"] = TempData["userType"];
                ViewBag.UserType = TempData["userType"];
                ViewBag.Name = name;

                ServiceCodeInfo scobj = new ServiceCodeInfo();
                string serviceCode = string.Empty;
                if (txtFundTransferType != null)
                {
                    if (txtFundTransferType == "Wallet to Wallet")
                    {
                        scobj.ServiceCode = "00";//walletToWallet
                        serviceCode = scobj.ServiceCode;
                    }
                    if (txtFundTransferType == "Wallet to Bank")
                    {
                        scobj.ServiceCode = "01";//walletToBank
                        serviceCode = scobj.ServiceCode;
                    }
                    if (txtFundTransferType == "Bank to Wallet")
                    {
                        scobj.ServiceCode = "10";//bankToWallet
                        serviceCode = scobj.ServiceCode;
                    }
                    if (txtFundTransferType == "Bank to Bank")
                    {
                        scobj.ServiceCode = "11";//bankToBank
                        serviceCode = scobj.ServiceCode;
                    }
                }
                ViewBag.ServiceCode = serviceCode;
                return Json(serviceCode, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        #endregion

        #region "Fund Transfer"
        //start milyako
        public ActionResult LoadWalletBW()
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

                ///start milayako
                MNBalance availBaln = new MNBalance();
                DataTable dtableUser1 = AvailBalnUtils.GetAvailBaln(clientCode);
                if (dtableUser1 != null && dtableUser1.Rows.Count > 0)
                {
                    availBaln.amount = dtableUser1.Rows[0]["AvailBaln"].ToString();

                    ViewBag.AvailBalnAmount = availBaln.amount;
                }
                //For Profile Pic//
                UserInfo userInfo = new UserInfo();
                DataSet DSet = ProfileUtils.GetCusDetailProfileInfoDS(clientCode);
                DataTable dKYC = DSet.Tables["dtKycDetail"];
                DataTable dDoc = DSet.Tables["dtKycDoc"];
                if (dKYC != null && dKYC.Rows.Count > 0)
                {
                    userInfo.CustStatus = dKYC.Rows[0]["CustStatus"].ToString();
                    ViewBag.CustStatus = userInfo.CustStatus;
                }
                if (dDoc != null && dDoc.Rows.Count > 0)
                {
                    userInfo.PassportImage = dDoc.Rows[0]["PassportImage"].ToString();
                    ViewBag.PassportImage = userInfo.PassportImage;
                }
                //end milayako

                //Check KYC
                DataTable dtableUserCheckKYC = ProfileUtils.CheckKYC(userName);
                if (dtableUserCheckKYC != null && dtableUserCheckKYC.Rows.Count > 0)
                {
                    userInfo.hasKYC = dtableUserCheckKYC.Rows[0]["hasKYC"].ToString();
                    userInfo.IsRejected = dtableUserCheckKYC.Rows[0]["IsRejected"].ToString();

                    ViewBag.hasKYC = userInfo.hasKYC;
                    ViewBag.IsRejected = userInfo.IsRejected;
                }

                //Check Link Bank Account
                DataTable dtableUserCheckLinkBankAcc = ProfileUtils.CheckLinkBankAcc(userName);
                if (dtableUserCheckLinkBankAcc != null && dtableUserCheckLinkBankAcc.Rows.Count > 0)
                {
                    userInfo.BankAccountNumber = dtableUserCheckLinkBankAcc.Rows[0]["HasBankKYC"].ToString();

                    ViewBag.HasBankKYC = userInfo.BankAccountNumber;
                }

                if (this.ViewData["fundTransfer_messsage"] != null)
                {
                    this.ViewData["fundTransfer_messsage"] = this.TempData["fundTransfer_messsage"];
                    this.ViewData["message_class"] = this.TempData["message_class"];
                }

                ViewBag.SenderMobileNo = userName;

                int id = TraceIdGenerator.GetID() + 1;
                string stringid = (id).ToString();//this.GetID() + 1
                string traceID = stringid.PadLeft(11, '0') + 'W';
                ViewBag.TraceID = traceID;

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        //end milayako
        // GET: FundTransfer
        //public ActionResult Index()
        //{
        //    if (this.TempData["login_message"] != null)
        //    {
        //        this.ViewData["login_message"] = this.TempData["login_message"];
        //        this.ViewData["message_class"] = this.TempData["message_class"];
        //    }

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

        //        ///start milayako
        //        MNBalance availBaln = new MNBalance();
        //        DataTable dtableUser1 = AvailBalnUtils.GetAvailBaln(clientCode);
        //        if (dtableUser1 != null && dtableUser1.Rows.Count > 0)
        //        {
        //            availBaln.amount = dtableUser1.Rows[0]["AvailBaln"].ToString();

        //            ViewBag.AvailBalnAmount = availBaln.amount;
        //        }

        //        //For Profile Pic//
        //        UserInfo userInfo = new UserInfo();
        //        DataSet DSet = ProfileUtils.GetCusDetailProfileInfoDS(clientCode);
        //        DataTable dKYC = DSet.Tables["dtKycDetail"];
        //        DataTable dDoc = DSet.Tables["dtKycDoc"];
        //        if (dKYC != null && dKYC.Rows.Count > 0)
        //        {
        //            userInfo.CustStatus = dKYC.Rows[0]["CustStatus"].ToString();
        //            ViewBag.CustStatus = userInfo.CustStatus;
        //        }
        //        if (dDoc != null && dDoc.Rows.Count > 0)
        //        {
        //            userInfo.PassportImage = dDoc.Rows[0]["PassportImage"].ToString();
        //            ViewBag.PassportImage = userInfo.PassportImage;
        //        }
        //        //end milayako

        //        //Check KYC
        //        DataTable dtableUserCheckKYC = ProfileUtils.CheckKYC(userName);
        //        if (dtableUserCheckKYC != null && dtableUserCheckKYC.Rows.Count > 0)
        //        {
        //            userInfo.hasKYC = dtableUserCheckKYC.Rows[0]["hasKYC"].ToString();
        //            userInfo.IsRejected = dtableUserCheckKYC.Rows[0]["IsRejected"].ToString();

        //            ViewBag.hasKYC = userInfo.hasKYC;
        //            ViewBag.IsRejected = userInfo.IsRejected;
        //        }

        //        //Check Link Bank Account
        //        DataTable dtableUserCheckLinkBankAcc = ProfileUtils.CheckLinkBankAcc(userName);
        //        if (dtableUserCheckLinkBankAcc != null && dtableUserCheckLinkBankAcc.Rows.Count > 0)
        //        {
        //            userInfo.BankAccountNumber = dtableUserCheckLinkBankAcc.Rows[0]["HasBankKYC"].ToString();

        //            ViewBag.HasBankKYC = userInfo.BankAccountNumber;
        //        }


        //        if (this.ViewData["fundTransfer_messsage"] != null)
        //        {
        //            this.ViewData["fundTransfer_messsage"] = this.TempData["fundTransfer_messsage"];
        //            this.ViewData["message_class"] = this.TempData["message_class"];
        //        }

        //        ViewBag.SenderMobileNo = userName;

        //        int id = TraceIdGenerator.GetID() + 1;
        //        string stringid = (id).ToString();//this.GetID() + 1
        //        string traceID = stringid.PadLeft(11, '0') + 'W';
        //        ViewBag.TraceID = traceID;

        //        return View();
        //    }
        //    else
        //    {
        //        return RedirectToAction("Index", "Login");
        //    }
        //}

        //POST: FundTransfer/FundTransferTran
        [HttpPost]
        public async Task<ActionResult> FundTransferTran(MNFundTransfer _ft)
        //MNFundTransfer _ft
        {


            string userName = (string)Session["LOGGED_USERNAME"];

            if (_ft.sc == "00" || _ft.sc == "11")
            {
                if (_ft.da == userName)
                {
                    return Json(new { responseCode = "", responseText = "Fund transfer is not allowed to same user" },
                       JsonRequestBehavior.AllowGet);
                }
            }


            HttpResponseMessage _res = new HttpResponseMessage();

            string mobile = _ft.mobile; //mobile is username

            TraceIdGenerator _tig = new TraceIdGenerator();
            var tid = _tig.GenerateTraceID();

            using (HttpClient client = new HttpClient())
            {

                var action = "ft.svc/request";
                var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                var content = new FormUrlEncodedContent(new[]{
                        new KeyValuePair<string, string>("tid", tid),
                        new KeyValuePair<string,string>("sc",_ft.sc),
                        new KeyValuePair<string, string>("mobile",mobile),
                        new KeyValuePair<string, string>("amount", _ft.amount),
                        new KeyValuePair<string,string>("da",_ft.da),
                        new KeyValuePair<string,string>("pin",_ft.pin),
                        new KeyValuePair<string, string>("note", _ft.note),
                        new KeyValuePair<string,string>("src",_ft.sourcechannel),
                        new KeyValuePair<string,string>("tokenID",Session["TokenID"].ToString())
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
                            if (code != responseCode)
                            {
                                responseCode = code;
                            }
                        }
                        return Json(new { responseCode = responseCode, responseText = respmsg },
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
                                                ? "Fund Transfer successfully." + message
                                                : "ERROR :: " + message;
                this.TempData["message_class"] = result ? "success_info" : "failed_info";

                return RedirectToAction("Index", "LoadWallet");
            }


        }



        #endregion


        #region "E Banking"
        public ActionResult EBanking()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            //for ticks PaymentReferenceNumber   
            DateTime currentDate = DateTime.Now;
            long elapsedTicks = currentDate.Ticks;
            string elapsedTick = Convert.ToString(elapsedTicks);
            ViewBag.PaymentReferenceNumber = elapsedTick.Substring((elapsedTick.Length - 10), 10); ;

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;
                ViewBag.userName = userName;
                ///start milayako
                MNBalance availBaln = new MNBalance();
                DataTable dtableUser1 = AvailBalnUtils.GetAvailBaln(clientCode);
                if (dtableUser1 != null && dtableUser1.Rows.Count > 0)
                {
                    availBaln.amount = dtableUser1.Rows[0]["AvailBaln"].ToString();

                    ViewBag.AvailBalnAmount = availBaln.amount;
                }

                UserInfo userInfo = new UserInfo();

                //Check KYC
                DataTable dtableUserCheckKYC = ProfileUtils.CheckKYC(userName);
                if (dtableUserCheckKYC != null && dtableUserCheckKYC.Rows.Count > 0)
                {
                    userInfo.hasKYC = dtableUserCheckKYC.Rows[0]["hasKYC"].ToString();
                    userInfo.IsRejected = dtableUserCheckKYC.Rows[0]["IsRejected"].ToString();

                    ViewBag.hasKYC = userInfo.hasKYC;
                    ViewBag.IsRejected = userInfo.IsRejected;

                }

                //Check Link Bank Account
                DataTable dtableUserCheckLinkBankAcc = ProfileUtils.CheckLinkBankAcc(userName);
                if (dtableUserCheckLinkBankAcc != null && dtableUserCheckLinkBankAcc.Rows.Count > 0)
                {
                    userInfo.BankAccountNumber = dtableUserCheckLinkBankAcc.Rows[0]["HasBankKYC"].ToString();

                    ViewBag.HasBankKYC = userInfo.BankAccountNumber;
                }

                //For Profile Pic//
                DataSet DSet = ProfileUtils.GetCusDetailProfileInfoDS(clientCode);
                DataTable dKYC = DSet.Tables["dtKycDetail"];
                DataTable dDoc = DSet.Tables["dtKycDoc"];
                if (dKYC != null && dKYC.Rows.Count > 0)
                {
                    userInfo.CustStatus = dKYC.Rows[0]["CustStatus"].ToString();
                    ViewBag.CustStatus = userInfo.CustStatus;
                }
                if (dDoc != null && dDoc.Rows.Count > 0)
                {
                    userInfo.PassportImage = dDoc.Rows[0]["PassportImage"].ToString();
                    ViewBag.PassportImage = userInfo.PassportImage;
                }
                //end milayako


                if (this.ViewData["fundTransfer_messsage"] != null)
                {
                    this.ViewData["fundTransfer_messsage"] = this.TempData["fundTransfer_messsage"];
                    this.ViewData["message_class"] = this.TempData["message_class"];
                }

                ViewBag.SenderMobileNo = userName;

                int id = TraceIdGenerator.GetID() + 1;
                string stringid = (id).ToString();//this.GetID() + 1
                string traceID = stringid.PadLeft(11, '0') + 'W';
                ViewBag.TraceID = traceID;

                CustomerSRInfo srobj = new CustomerSRInfo();
                return View(srobj);

                //return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [HttpPost]
        public ActionResult EBanking(FormCollection collection)
        {
            string clientCode = (string)Session["LOGGEDUSER_ID"];

            string tid;
            TraceIdGenerator traceid = new TraceIdGenerator();
            tid = traceid.GenerateUniqueTraceID();
            ViewBag.retrievalReference = tid;

            CustomerSRInfo srInfo = new CustomerSRInfo();
            try
            {
                string mobile = (string)Session["Mobile"];
                string code = (string)Session["Code"];


                srInfo.UserName = mobile;
                ///for date
                srInfo.EBDate = DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt");


                srInfo.ClientCode = clientCode;
                srInfo.PaymentReferenceNumber = collection["txtPaymentReferenceNumber"].ToString();
                srInfo.ItemCode = collection["txtItemCode"].ToString();
                srInfo.Amount = collection["txtAmount"].ToString();
                srInfo.Remarks = collection["txtNote"].ToString();
                srInfo.retrievalReference = ViewBag.retrievalReference;

                srInfo.UserType = "user";
                srInfo.IsRejected = "F";
                srInfo.Source = "http";

                srInfo.Password = CustomerUtils.GeneratePassword();
                srInfo.PIN = CustomerUtils.GeneratePin();

                ///for value pass from EBanking to EBankingLink
                Session["LOGGED_PAYMENTREFERENCENUMBER"] = srInfo.PaymentReferenceNumber;
                Session["LOGGED_ITEMCODE"] = srInfo.ItemCode;
                Session["LOGGED_AMOUNT"] = srInfo.Amount;
                Session["LOGGED_Remarks"] = srInfo.Remarks;
                Session["LOGGED_retrievalReference"] = srInfo.retrievalReference;

                if (!ViewData.ModelState.IsValid)
                {
                    this.ViewData["registration_message"] = " *Validation Error.";
                    this.ViewData["message_class"] = "failed_info";
                    return View();
                }

                bool result = false;
                string errorMessage = string.Empty;

                if (collection.AllKeys.Any())
                {
                    try
                    {

                        int results = RegisterUtils.EBanking(srInfo);
                        if (results > 0)
                        {
                            result = true;

                            TempData["login_message"] = "We are happy to have you as a member of Thaili.";
                            TempData["message_class"] = CssSetting.SuccessMessageClass;

                            return RedirectToAction("EBankingLink");

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


                this.ViewData["registration_message"] = result
                                              ? "Registration information is successfully added."
                                              : "Error while inserting the information. ERROR :: "
                                                + errorMessage;
                this.ViewData["message_class"] = result ? "success_info" : "failed_info";

                return View(srInfo);

            }
            catch (Exception ex)
            {

                return View(srInfo);
            }
        }

        #endregion

        #region "E Banking direct link"
        public ActionResult EBankingLink()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            //FOR value passing from EBanking to EBankingLink
            string PaymentReferenceNumber = (string)Session["LOGGED_PAYMENTREFERENCENUMBER"];
            string ItemCode = (string)Session["LOGGED_ITEMCODE"];
            string Amount = (string)Session["LOGGED_AMOUNT"];
            string Remarks = (string)Session["LOGGED_Remarks"];

            ViewBag.PaymentReferenceNumber = PaymentReferenceNumber;
            ViewBag.ItemCode = ItemCode;
            ViewBag.Amount = Amount;
            ViewBag.Remarks = Remarks;

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;
                ViewBag.userName = userName;
                ///start milayako
                MNBalance availBaln = new MNBalance();
                DataTable dtableUser1 = AvailBalnUtils.GetAvailBaln(clientCode);
                if (dtableUser1 != null && dtableUser1.Rows.Count > 0)
                {
                    availBaln.amount = dtableUser1.Rows[0]["AvailBaln"].ToString();

                    ViewBag.AvailBalnAmount = availBaln.amount;
                }

                UserInfo userInfo = new UserInfo();

                //Check KYC
                DataTable dtableUserCheckKYC = ProfileUtils.CheckKYC(userName);
                if (dtableUserCheckKYC != null && dtableUserCheckKYC.Rows.Count > 0)
                {
                    userInfo.hasKYC = dtableUserCheckKYC.Rows[0]["hasKYC"].ToString();
                    userInfo.IsRejected = dtableUserCheckKYC.Rows[0]["IsRejected"].ToString();

                    ViewBag.hasKYC = userInfo.hasKYC;
                    ViewBag.IsRejected = userInfo.IsRejected;

                }

                //Check Link Bank Account
                DataTable dtableUserCheckLinkBankAcc = ProfileUtils.CheckLinkBankAcc(userName);
                if (dtableUserCheckLinkBankAcc != null && dtableUserCheckLinkBankAcc.Rows.Count > 0)
                {
                    userInfo.BankAccountNumber = dtableUserCheckLinkBankAcc.Rows[0]["HasBankKYC"].ToString();

                    ViewBag.HasBankKYC = userInfo.BankAccountNumber;
                }

                //For Profile Pic//
                DataSet DSet = ProfileUtils.GetCusDetailProfileInfoDS(clientCode);
                DataTable dKYC = DSet.Tables["dtKycDetail"];
                DataTable dDoc = DSet.Tables["dtKycDoc"];
                if (dKYC != null && dKYC.Rows.Count > 0)
                {
                    userInfo.CustStatus = dKYC.Rows[0]["CustStatus"].ToString();
                    ViewBag.CustStatus = userInfo.CustStatus;
                }
                if (dDoc != null && dDoc.Rows.Count > 0)
                {
                    userInfo.PassportImage = dDoc.Rows[0]["PassportImage"].ToString();
                    ViewBag.PassportImage = userInfo.PassportImage;
                }
                //end milayako


                if (this.ViewData["fundTransfer_messsage"] != null)
                {
                    this.ViewData["fundTransfer_messsage"] = this.TempData["fundTransfer_messsage"];
                    this.ViewData["message_class"] = this.TempData["message_class"];
                }

                ViewBag.SenderMobileNo = userName;

                int id = TraceIdGenerator.GetID() + 1;
                string stringid = (id).ToString();//this.GetID() + 1
                string traceID = stringid.PadLeft(11, '0') + 'W';
                ViewBag.TraceID = traceID;

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        #endregion

        //#region Withdraw
        //public ActionResult Withdraw()
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

        //        ///start milayako
        //        MNBalance availBaln = new MNBalance();
        //        DataTable dtableUser1 = AvailBalnUtils.GetAvailBaln(clientCode);
        //        if (dtableUser1 != null && dtableUser1.Rows.Count > 0)
        //        {
        //            availBaln.amount = dtableUser1.Rows[0]["AvailBaln"].ToString();

        //            ViewBag.AvailBalnAmount = availBaln.amount;
        //        }
        //        //For Profile Pic//
        //        UserInfo userInfo = new UserInfo();
        //        DataSet DSet = ProfileUtils.GetCusDetailProfileInfoDS(clientCode);
        //        DataTable dKYC = DSet.Tables["dtKycDetail"];
        //        DataTable dDoc = DSet.Tables["dtKycDoc"];
        //        if (dKYC != null && dKYC.Rows.Count > 0)
        //        {
        //            userInfo.CustStatus = dKYC.Rows[0]["CustStatus"].ToString();
        //            ViewBag.CustStatus = userInfo.CustStatus;
        //        }
        //        if (dDoc != null && dDoc.Rows.Count > 0)
        //        {
        //            userInfo.PassportImage = dDoc.Rows[0]["PassportImage"].ToString();
        //            ViewBag.PassportImage = userInfo.PassportImage;
        //        }
        //        //end milayako

        //        //Check KYC
        //        DataTable dtableUserCheckKYC = ProfileUtils.CheckKYC(userName);
        //        if (dtableUserCheckKYC != null && dtableUserCheckKYC.Rows.Count > 0)
        //        {
        //            userInfo.hasKYC = dtableUserCheckKYC.Rows[0]["hasKYC"].ToString();
        //            userInfo.IsRejected = dtableUserCheckKYC.Rows[0]["IsRejected"].ToString();

        //            ViewBag.hasKYC = userInfo.hasKYC;
        //            ViewBag.IsRejected = userInfo.IsRejected;
        //        }

        //        //Check Link Bank Account
        //        DataTable dtableUserCheckLinkBankAcc = ProfileUtils.CheckLinkBankAcc(userName);
        //        if (dtableUserCheckLinkBankAcc != null && dtableUserCheckLinkBankAcc.Rows.Count > 0)
        //        {
        //            userInfo.BankAccountNumber = dtableUserCheckLinkBankAcc.Rows[0]["HasBankKYC"].ToString();

        //            ViewBag.HasBankKYC = userInfo.BankAccountNumber;
        //        }

        //        if (this.ViewData["fundTransfer_messsage"] != null)
        //        {
        //            this.ViewData["fundTransfer_messsage"] = this.TempData["fundTransfer_messsage"];
        //            this.ViewData["message_class"] = this.TempData["message_class"];
        //        }

        //        ViewBag.SenderMobileNo = userName;

        //        int id = TraceIdGenerator.GetID() + 1;
        //        string stringid = (id).ToString();//this.GetID() + 1
        //        string traceID = stringid.PadLeft(11, '0') + 'W';
        //        ViewBag.TraceID = traceID;

        //        return View();
        //    }
        //    else
        //    {
        //        return RedirectToAction("Index", "Login");
        //    }
        //}
        //#endregion
        /////
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

        public ActionResult GetHasKYC()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                ViewData["userType"] = TempData["userType"];
                ViewBag.UserType = TempData["userType"];
                ViewBag.Name = name;
                string hasKYC = string.Empty;
                UserInfo userInfo = new UserInfo();

                //Check KYC
                DataTable dtableUserCheckKYC = ProfileUtils.CheckKYC(userName);
                if (dtableUserCheckKYC != null && dtableUserCheckKYC.Rows.Count > 0)
                {
                    userInfo.hasKYC = dtableUserCheckKYC.Rows[0]["hasKYC"].ToString();

                    ViewBag.hasKYC = userInfo.hasKYC;
                    hasKYC = ViewBag.hasKYC;

                }
                return Json(hasKYC, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        public ActionResult GetLnkBankAcc()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                ViewData["userType"] = TempData["userType"];
                ViewBag.UserType = TempData["userType"];
                ViewBag.Name = name;
                string HasBankKYC = string.Empty;
                UserInfo userInfo = new UserInfo();

                //Check Link Bank Account
                DataTable dtableUserCheckLinkBankAcc = ProfileUtils.CheckLinkBankAcc(userName);
                if (dtableUserCheckLinkBankAcc != null && dtableUserCheckLinkBankAcc.Rows.Count > 0)
                {
                    userInfo.BankAccountNumber = dtableUserCheckLinkBankAcc.Rows[0]["HasBankKYC"].ToString();

                    ViewBag.HasBankKYC = userInfo.BankAccountNumber;
                    HasBankKYC = ViewBag.HasBankKYC;

                }
                return Json(HasBankKYC, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        //
    }
}