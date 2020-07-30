using CustApp.App_Start;
using CustApp.Helper;
using CustApp.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.SessionState;
using System.Data;
using CustApp.Utilities;
using CustApp.UserModels;
using System.Linq;
using System.Dynamic;
using Newtonsoft.Json;

namespace CustApp.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class CashController : Controller
    {
        #region "Cash In"
        // GET: Cash/In
        public ActionResult In()
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

                if (this.TempData["TranTransfer_messsage"] != null)
                {
                    this.ViewData["TranTransfer_messsage"] = this.TempData["TranTransfer_messsage"];
                    this.ViewData["message_class"] = this.TempData["message_class"];
                }

                ViewBag.SenderMobileNo = userName;

                //int id = TraceIdGenerator.GetID() + 1;
                //string stringid = (id).ToString();//this.GetID() + 1
                //string traceID = stringid.PadLeft(11, '0') + 'W';
                //ViewBag.TraceID = traceID;

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        // POST: Cash/In
        [HttpPost]
        public async Task<ActionResult> In(MNFundTransfer _ft)
        {
            HttpResponseMessage _res = new HttpResponseMessage();
            //string tid = _ft.tid;
            string mobile = _ft.mobile; //mobile is username
            string cashinsc = "50";

            TraceIdGenerator _tig = new TraceIdGenerator();
            var tid = _tig.GenerateTraceID();

            using (HttpClient client = new HttpClient())
            {
                //var uri = "http://27.111.30.126/MNepal.WCF/cash/In";

                var action = "cash/In";
                var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                var content = new FormUrlEncodedContent(new[]
                        {
                    new KeyValuePair<string, string>("tid", tid),
                    new KeyValuePair<string,string>("sc",cashinsc),
                    new KeyValuePair<string, string>("amobile",mobile),
                    new KeyValuePair<string, string>("amount", _ft.amount),
                    new KeyValuePair<string,string>("umobile",_ft.da),
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

                string ava = string.Empty;
                string avatra = string.Empty;
                string avamsg = string.Empty;

                if (_res.IsSuccessStatusCode)
                {
                    responseCode = (int)_res.StatusCode;
                    responsetext = await _res.Content.ReadAsStringAsync();
                    //message = _res.Content.ReadAsStringAsync().Result;
                    dynamic json = JValue.Parse(responsetext);
                    // values require casting
                    string message1 = json.d;
                    dynamic item = JValue.Parse(message1);
                    int code = (int)item["StatusCode"];
                    message = (string)item["StatusMessage"];
                    JavaScriptSerializer ser = new JavaScriptSerializer();
                    ResponseMessage myNames = ser.Deserialize<ResponseMessage>((string)item["StatusMessage"]);

                    ser.Serialize(myNames);
                    ava = myNames.AvailableBalance.ToString();
                    avatra = myNames.AmountTransferredBalance.ToString();
                    avamsg = myNames.Message.ToString();
                }
                else
                {
                    responseCode = (int)_res.StatusCode;
                    responsetext = await _res.Content.ReadAsStringAsync();

                    dynamic json = JValue.Parse(responsetext);
                    // values require casting
                    string message1 = json.d;
                    dynamic item = JValue.Parse(message1);
                    message = (string)item["StatusMessage"];
                }

                return Json(new { responseCode = responseCode, responseText = message },
                    JsonRequestBehavior.AllowGet);
            }


        }

        #endregion

        #region "Cash Out"
        //GET: Cash/Out
        public ActionResult Out()
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
                UserInfo userInfo = new UserInfo();

                //Check KYC
                DataTable dtableUserCheckKYC = ProfileUtils.CheckKYC(userName);
                if (dtableUserCheckKYC != null && dtableUserCheckKYC.Rows.Count > 0)
                {
                    userInfo.hasKYC = dtableUserCheckKYC.Rows[0]["hasKYC"].ToString();
                    userInfo.IsRejected = dtableUserCheckKYC.Rows[0]["IsRejected"].ToString();

                    ViewBag.IsRejected = userInfo.IsRejected;
                    ViewBag.hasKYC = userInfo.hasKYC;
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

                if (this.TempData["TranTransfer_messsage"] != null)
                {
                    this.ViewData["TranTransfer_messsage"] = this.TempData["TranTransfer_messsage"];
                    this.ViewData["message_class"] = this.TempData["message_class"];
                }

                ViewBag.SenderMobileNo = userName;

                //int id = TraceIdGenerator.GetID() + 1;
                //string stringid = (id).ToString();//this.GetID() + 1
                //string traceID = stringid.PadLeft(11, '0') + 'W';
                //ViewBag.TraceID = traceID;

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        // POST: Cash/Out
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> Out(MNFundTransfer _ft)
        {
            try
            {
                string retoken = _ft.TokenUnique;
                string reqToken = "";
                DataTable dtableVToken = ReqTokenUtils.GetReqToken(retoken);
                if (dtableVToken != null && dtableVToken.Rows.Count > 0)
                {
                    reqToken = dtableVToken.Rows[0]["ReqVerifyToken"].ToString();
                }
                else if (dtableVToken.Rows.Count == 0)
                {
                    reqToken = "0";
                }
                if (reqToken == "0")
                {
                    ReqTokenUtils.InsertReqToken(retoken);

                    HttpResponseMessage _res = new HttpResponseMessage();
                    //string tid = _ft.tid;
                    string mobile = _ft.mobile; //mobile is username
                    string cashoutsc = "51"; //Cash out SC: 51

                    TraceIdGenerator _tig = new TraceIdGenerator();
                    var tid = _tig.GenerateTraceID();

                    using (HttpClient client = new HttpClient())
                    {
                        //var uri = "http://27.111.30.126/MNepal.WCF/cash/Out";
                        //var action = "cash/Out";
                        //start milayako 03
                        var action = "cash.svc/CustOut";
                        //end milayako 03
                        var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                        var content = new FormUrlEncodedContent(new[]
                                {
                    new KeyValuePair<string, string>("tid", tid),
                    new KeyValuePair<string,string>("sc",cashoutsc),
                    //new KeyValuePair<string, string>("amobile",_ft.da),
                    new KeyValuePair<string, string>("amount", _ft.amount),
                    new KeyValuePair<string,string>("umobile",mobile),
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

                        string ava = string.Empty;
                        string avatra = string.Empty;
                        string avamsg = string.Empty;

                        if (_res.IsSuccessStatusCode)
                        {
                            responseCode = (int)_res.StatusCode;
                            responsetext = await _res.Content.ReadAsStringAsync();
                            //message = _res.Content.ReadAsStringAsync().Result;
                            dynamic json = JValue.Parse(responsetext);
                            // values require casting
                            string message1 = json.d;
                            dynamic item = JValue.Parse(message1);
                            message = (string)item["StatusMessage"];
                            responseCode = item["StatusCode"];

                            //JavaScriptSerializer ser = new JavaScriptSerializer();
                            //ResponseMessage myNames = ser.Deserialize<ResponseMessage>((string)item["StatusMessage"]);
                            //ser.Serialize(myNames);
                            //ava = myNames.AvailableBalance.ToString();
                            //avatra = myNames.AmountTransferredBalance.ToString();
                            //avamsg = myNames.Message.ToString();
                        }
                        else
                        {
                            responseCode = (int)_res.StatusCode;
                            responsetext = await _res.Content.ReadAsStringAsync();

                            dynamic json = JValue.Parse(responsetext);
                            // values require casting
                            string message1 = json.d;
                            dynamic item = JValue.Parse(message1);
                            message = (string)item["StatusMessage"];
                        }

                        return Json(new { responseCode = responseCode, responseText = message },
                            JsonRequestBehavior.AllowGet);
                    }

                }
                else
                {
                    return Json(new { responseCode = "400", responseText = "Please refresh the page again." },
                            JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
                throw;
            }

        }

        #endregion
        
        public ActionResult GetCheckingAgent(string Username)
        {
            if ((Username != "") || (Username != null))
            {
                string result = string.Empty;
                DataTable dtableMobileNo = RegisterUtils.GetCheckAgentMobileNo(Username);
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
        
        #region "Cash Out By Agent"
        //GET: Cash/OutAgent1
        public ActionResult OutAgent1()
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
                if (this.TempData["TranTransfer_messsage"] != null)
                {
                    this.ViewData["TranTransfer_messsage"] = this.TempData["TranTransfer_messsage"];
                    this.ViewData["message_class"] = this.TempData["message_class"];
                }

                ViewBag.AgentMobileNo = userName;

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        // POST: Cash/OutAgent1
        [HttpPost]
        public async Task<ActionResult> OutAgent1(MNFundTransfer _ft)
        {


            //end milayako 
            try
            {
                HttpResponseMessage _res = new HttpResponseMessage();

                string agentMobile = _ft.agentUserName; //mobile is username
                string custMobile = _ft.da; //customer username
                string transactionCode = _ft.transactionCode; //mobile is username

                int responseCode = 0;
                string message = string.Empty;
                string responsetext = string.Empty;

                if ((agentMobile != "") && (custMobile != "") && (transactionCode != ""))
                {
                    responseCode = (int)_res.StatusCode;
                    message = "success";

                    Session["LOGGED_CUSTOMERMOBILE"] = custMobile;
                    Session["LOGGED_TRANSACTIONCODE"] = transactionCode;

                }
                else
                {
                    responseCode = (int)_res.StatusCode;
                    responsetext = "Please Enter the required Field";

                    dynamic json = JValue.Parse(responsetext);
                    // values require casting
                    string message1 = json.d;
                    dynamic item = JValue.Parse(message1);
                    message = (string)item["StatusMessage"];
                }

                return Json(new { responseCode = responseCode, responseText = message },
                        JsonRequestBehavior.AllowGet);


                //string cashoutsc = "51"; //Cash out SC: 51
                //TraceIdGenerator _tig = new TraceIdGenerator();
                //var tid = _tig.GenerateTraceID();

                //using (HttpClient client = new HttpClient())
                //{
                //    //var uri = "http://27.111.30.126/MNepal.WCF/cash/Out";
                //    var action = "cash/Out";
                //    var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                //    var content = new FormUrlEncodedContent(new[]
                //    {
                //        new KeyValuePair<string, string>("tid", tid),
                //        new KeyValuePair<string,string>("sc",cashoutsc),
                //        new KeyValuePair<string, string>("amobile",_ft.da),
                //        new KeyValuePair<string, string>("amount", _ft.amount),
                //        new KeyValuePair<string,string>("umobile",mobile),
                //        new KeyValuePair<string,string>("pin",_ft.pin),
                //        new KeyValuePair<string, string>("note", _ft.note),
                //        new KeyValuePair<string,string>("src",_ft.sourcechannel)
                //    });

                //    _res = await client.PostAsync(new Uri(uri), content);

                //    string responseBody = _res.StatusCode.ToString() + " ," + await _res.Content.ReadAsStringAsync();
                //    _res.ReasonPhrase = responseBody;

                //    string errorMessage = string.Empty;

                //    int responseCode = 0;
                //    string message = string.Empty;
                //    string responsetext = string.Empty;

                //    string ava = string.Empty;
                //    string avatra = string.Empty;
                //    string avamsg = string.Empty;

                //    if (_res.IsSuccessStatusCode)
                //    {
                //        responseCode = (int)_res.StatusCode;
                //        responsetext = await _res.Content.ReadAsStringAsync();
                //        //message = _res.Content.ReadAsStringAsync().Result;
                //        dynamic json = JValue.Parse(responsetext);
                //        // values require casting
                //        string message1 = json.d;
                //        dynamic item = JValue.Parse(message1);
                //        message = (string)item["StatusMessage"];


                //        JavaScriptSerializer ser = new JavaScriptSerializer();
                //        ResponseMessage myNames = ser.Deserialize<ResponseMessage>((string)item["StatusMessage"]);
                //        ser.Serialize(myNames);
                //        ava = myNames.AvailableBalance.ToString();
                //        avatra = myNames.AmountTransferredBalance.ToString();
                //        avamsg = myNames.Message.ToString();
                //    }
                //    else
                //    {
                //        responseCode = (int)_res.StatusCode;
                //        responsetext = await _res.Content.ReadAsStringAsync();

                //        dynamic json = JValue.Parse(responsetext);
                //        // values require casting
                //        string message1 = json.d;
                //        dynamic item = JValue.Parse(message1);
                //        message = (string)item["StatusMessage"];
                //    }

                //    return Json(new { responseCode = responseCode, responseText = message },
                //        JsonRequestBehavior.AllowGet);
                //}
            }
            catch
            {
                throw;
            }

        }

        //start 
        //GET: Cash/OutAgent2
        public ActionResult OutAgent2()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            ///start milayako 
            string custuserMobile = (string)Session["LOGGED_CUSTOMERMOBILE"];
            string transactionCode = (string)Session["LOGGED_TRANSACTIONCODE"];

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

                //end milayako

                if (this.TempData["TranTransfer_messsage"] != null)
                {
                    this.ViewData["TranTransfer_messsage"] = this.TempData["TranTransfer_messsage"];
                    this.ViewData["message_class"] = this.TempData["message_class"];
                }

                ViewBag.AgentMobileNo = userName;
                ViewBag.custuserMobile = custuserMobile;
                ViewBag.transactionCode = transactionCode;



                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        // POST: Cash/OutAgent2
        [HttpPost]
        public async Task<ActionResult> OutAgent2(MNFundTransfer _ft)
        {
            try
            {
                HttpResponseMessage _res = new HttpResponseMessage();
                //string tid = _ft.tid;
                string mobile = _ft.mobile; //mobile is username
                string cashoutsc = "51"; //Cash out SC: 51

                TraceIdGenerator _tig = new TraceIdGenerator();
                var tid = _tig.GenerateTraceID();

                using (HttpClient client = new HttpClient())
                {
                    //var uri = "http://27.111.30.126/MNepal.WCF/cash/Out";
                    var action = "cash/Out";
                    var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                    var content = new FormUrlEncodedContent(new[]
                            {
                    new KeyValuePair<string, string>("tid", tid),
                    new KeyValuePair<string,string>("sc",cashoutsc),
                    new KeyValuePair<string, string>("amobile",_ft.da),
                    new KeyValuePair<string, string>("amount", _ft.amount),
                    new KeyValuePair<string,string>("umobile",mobile),
                    new KeyValuePair<string,string>("pin",_ft.pin),
                    new KeyValuePair<string, string>("note", _ft.note),
                    new KeyValuePair<string,string>("src",_ft.sourcechannel)
                });

                    _res = await client.PostAsync(new Uri(uri), content);

                    string responseBody = _res.StatusCode.ToString() + " ," + await _res.Content.ReadAsStringAsync();
                    _res.ReasonPhrase = responseBody;

                    string errorMessage = string.Empty;

                    int responseCode = 0;
                    string message = string.Empty;
                    string responsetext = string.Empty;

                    string ava = string.Empty;
                    string avatra = string.Empty;
                    string avamsg = string.Empty;

                    if (_res.IsSuccessStatusCode)
                    {
                        responseCode = (int)_res.StatusCode;
                        responsetext = await _res.Content.ReadAsStringAsync();
                        //message = _res.Content.ReadAsStringAsync().Result;
                        dynamic json = JValue.Parse(responsetext);
                        // values require casting
                        string message1 = json.d;
                        dynamic item = JValue.Parse(message1);
                        message = (string)item["StatusMessage"];


                        JavaScriptSerializer ser = new JavaScriptSerializer();
                        ResponseMessage myNames = ser.Deserialize<ResponseMessage>((string)item["StatusMessage"]);
                        ser.Serialize(myNames);
                        ava = myNames.AvailableBalance.ToString();
                        avatra = myNames.AmountTransferredBalance.ToString();
                        avamsg = myNames.Message.ToString();
                    }
                    else
                    {
                        responseCode = (int)_res.StatusCode;
                        responsetext = await _res.Content.ReadAsStringAsync();

                        dynamic json = JValue.Parse(responsetext);
                        // values require casting
                        string message1 = json.d;
                        dynamic item = JValue.Parse(message1);
                        message = (string)item["StatusMessage"];
                    }

                    return Json(new { responseCode = responseCode, responseText = message },
                        JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
                throw;
            }

        }
        //end


        //start 
        //GET: Cash/OutAgent3
        public ActionResult OutAgent3()
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

                //end milayako

                if (this.TempData["TranTransfer_messsage"] != null)
                {
                    this.ViewData["TranTransfer_messsage"] = this.TempData["TranTransfer_messsage"];
                    this.ViewData["message_class"] = this.TempData["message_class"];
                }

                ViewBag.AgentMobileNo = userName;

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        // POST: Cash/OutAgent3
        [HttpPost]
        public async Task<ActionResult> OutAgent3(MNFundTransfer _ft)
        {
            try
            {
                HttpResponseMessage _res = new HttpResponseMessage();
                //string tid = _ft.tid;
                string mobile = _ft.mobile; //mobile is username
                string cashoutsc = "51"; //Cash out SC: 51

                TraceIdGenerator _tig = new TraceIdGenerator();
                var tid = _tig.GenerateTraceID();

                using (HttpClient client = new HttpClient())
                {
                    //var uri = "http://27.111.30.126/MNepal.WCF/cash/Out";
                    var action = "cash/Out";
                    var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                    var content = new FormUrlEncodedContent(new[]
                            {
                    new KeyValuePair<string, string>("tid", tid),
                    new KeyValuePair<string,string>("sc",cashoutsc),
                    new KeyValuePair<string, string>("amobile",_ft.da),
                    new KeyValuePair<string, string>("amount", _ft.amount),
                    new KeyValuePair<string,string>("umobile",mobile),
                    new KeyValuePair<string,string>("pin",_ft.pin),
                    new KeyValuePair<string, string>("note", _ft.note),
                    new KeyValuePair<string,string>("src",_ft.sourcechannel)
                });

                    _res = await client.PostAsync(new Uri(uri), content);

                    string responseBody = _res.StatusCode.ToString() + " ," + await _res.Content.ReadAsStringAsync();
                    _res.ReasonPhrase = responseBody;

                    string errorMessage = string.Empty;

                    int responseCode = 0;
                    string message = string.Empty;
                    string responsetext = string.Empty;

                    string ava = string.Empty;
                    string avatra = string.Empty;
                    string avamsg = string.Empty;

                    if (_res.IsSuccessStatusCode)
                    {
                        responseCode = (int)_res.StatusCode;
                        responsetext = await _res.Content.ReadAsStringAsync();
                        //message = _res.Content.ReadAsStringAsync().Result;
                        dynamic json = JValue.Parse(responsetext);
                        // values require casting
                        string message1 = json.d;
                        dynamic item = JValue.Parse(message1);
                        message = (string)item["StatusMessage"];


                        JavaScriptSerializer ser = new JavaScriptSerializer();
                        ResponseMessage myNames = ser.Deserialize<ResponseMessage>((string)item["StatusMessage"]);
                        ser.Serialize(myNames);
                        ava = myNames.AvailableBalance.ToString();
                        avatra = myNames.AmountTransferredBalance.ToString();
                        avamsg = myNames.Message.ToString();
                    }
                    else
                    {
                        responseCode = (int)_res.StatusCode;
                        responsetext = await _res.Content.ReadAsStringAsync();

                        dynamic json = JValue.Parse(responsetext);
                        // values require casting
                        string message1 = json.d;
                        dynamic item = JValue.Parse(message1);
                        message = (string)item["StatusMessage"];
                    }

                    return Json(new { responseCode = responseCode, responseText = message },
                        JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
                throw;
            }

        }
        //end
        #endregion

        #region New Cash Out
        [HttpGet]
        public ActionResult CashOutNew()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            //Balance//
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
            return View();



        }
        #endregion

        #region Cash Out List
        //GET: Cash/CashOutList
        public ActionResult CashOutList()
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

                    ViewBag.IsRejected = userInfo.IsRejected;
                    ViewBag.hasKYC = userInfo.hasKYC;
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

                if (this.TempData["TranTransfer_messsage"] != null)
                {
                    this.ViewData["TranTransfer_messsage"] = this.TempData["TranTransfer_messsage"];
                    this.ViewData["message_class"] = this.TempData["message_class"];
                }

                ViewBag.SenderMobileNo = userName;

                //int id = TraceIdGenerator.GetID() + 1;
                //string stringid = (id).ToString();//this.GetID() + 1
                //string traceID = stringid.PadLeft(11, '0') + 'W';
                //ViewBag.TraceID = traceID;

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        public ContentResult CashOutListRejectTable(DataTableAjaxPostModel model, string change)
        {
            if (!Request.IsAjaxRequest())
            {
                Response.Write("Invalid Execution");
                return Content("");
            }
            string userName = (string)Session["LOGGED_USERNAME"];
            if (userName != null)
            {
                int filteredResultsCount;
                int totalResultsCount;
                String ParaChanged = "T";

                CustReport ac = new CustReport();
                ac.UserName = userName;
                ac.Status = null;

                string convert;
                ParaChanged = change;
                
                var result = new List<CashOutList>();
                if (Session["CashOutList"] != null && ParaChanged == "F")
                {
                    result = Session["CashOutList"] as List<CashOutList>;
                }
                else
                {
                    ReportUserModel rep = new ReportUserModel();
                    result = rep.GetCashOutListApproved(ac);
                    Session["CashOutList"] = result;
                }

                var res = FilterAndSort<CashOutList>(model, result, out totalResultsCount, out filteredResultsCount);
                var obj = new List<dynamic>();
                foreach (var item in res)
                {
                    dynamic resultset = new ExpandoObject();
                    resultset.TokenID = item.TokenID;
                    //resultset.Date = item.Date.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);//"dd /MM/yyyy", CultureInfo.InvariantCulture);

                    //resultset.RecipientMobileNo = item.RecipientMobileNo;
                    resultset.Amount = item.Amount;
                    resultset.TokenCreatedDate = item.CreatedDate.ToString();
                    resultset.TokenExpiryDate = item.ExpiryDate;
                    resultset.Status = "Pending";
                    obj.Add(resultset);

                }
                convert = JsonConvert.SerializeObject(new
                {

                    draw = model.draw,
                    recordsTotal = totalResultsCount,
                    recordsFiltered = filteredResultsCount,
                    data = obj
                });

                return Content(convert, "application/json");
            }
            else
            {
                return Content("Index", "Login");
            }
        }

        // POST: Cash/OutAgentDeny
        [HttpPost]
        public async Task<ActionResult> CashOutReject(string token, string ActionClick, string pin)
        {

            HttpResponseMessage _res = new HttpResponseMessage();
            string tokenID = token; //token is Token ID
            string userName = (string)Session["LOGGED_USERNAME"];
            string tpin = pin;
            if (userName != null)
            {
                TraceIdGenerator _tig = new TraceIdGenerator();
                var tid = _tig.GenerateTraceID();

                using (HttpClient client = new HttpClient())
                {
                    //var uri = "http://27.111.30.126/MNepal.WCF/cash/Out";
                    // var uri = "http://localhost:51397/cash.svc/AgentCashOut";
                    var action = "cash.svc/CustCashOutCancel";
                    var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                    var content = new FormUrlEncodedContent(new[]
                            {
                        new KeyValuePair<string, string>("tid", tid),
                        new KeyValuePair<string,string>("pin",tpin),
                        new KeyValuePair<string,string>("src","http"),
                        new KeyValuePair<string, string>("umobile",userName),
                        new KeyValuePair<string,string>("tokenID",Session["TokenID"].ToString()),
                        new KeyValuePair<string, string>("requestTokenCode", tokenID)
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

                        if (_res.IsSuccessStatusCode /*&& (_ft.status.Equals('1'))*/)
                        {
                            result = true;
                            responseCode = (int)_res.StatusCode;
                            responsetext = await _res.Content.ReadAsStringAsync();
                            //message = _res.Content.ReadAsStringAsync().Result;
                            // dynamic json = JValue.Parse(responsetext);
                            // values require casting
                            //  message = json.d;

                            message = _res.Content.ReadAsStringAsync().Result;
                            string respmsg = "";

                            if (!string.IsNullOrEmpty(message))
                            {

                                JavaScriptSerializer ser = new JavaScriptSerializer();

                                var jsonresp = ser.Deserialize<JsonParse>(responsetext);
                                message = jsonresp.d;

                                JsonParse myNames = ser.Deserialize<JsonParse>(jsonresp.d);
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
                    this.TempData["recharge_message"] = result
                                               ? "Recharge successfully." + message
                                               : "ERROR :: " + message;
                    this.TempData["message_class"] = result ? "success_info" : "failed_info";
                    return RedirectToAction("OutAgent3", "Cash");
                }
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
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
        public ActionResult GetCheckingAgentName(string Username)
        {
            if ((Username != "") || (Username != null))
            {
                string result = string.Empty;
                DataTable dtableMobileNo = RegisterUtils.GetCheckAgentMobileNo(Username);
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
        
    }
}