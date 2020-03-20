using MNepalWeb.Helper;
using MNepalWeb.Models;
using MNepalWeb.Utilities;
using MNepalWeb.ViewModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.SessionState;

namespace MNepalWeb.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class RemitController : Controller
    {
        #region "Request Token Code "
        public ActionResult GetRequestTokenCode(string recipientNo)
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

                ServiceCodeInfo scobj = new ServiceCodeInfo();
                MNRemit remitInfo = new MNRemit();
                string BeneficialName = string.Empty;
                string RequestTokenCode = string.Empty;
                string Amount = string.Empty;
                string TokenID = string.Empty;
                string Note = string.Empty;

                if (recipientNo != null)
                {
                    DataTable dtable = RemitUtils.GetRemitInformation(recipientNo);
                    if (dtable != null && dtable.Rows.Count > 0)
                    {
                        remitInfo.BeneficialName = dtable.Rows[0]["BeneficialName"].ToString();
                        remitInfo.RequestTokenCode = dtable.Rows[0]["RequestTokenCode"].ToString();
                        remitInfo.Amount = dtable.Rows[0]["Amount"].ToString();
                        remitInfo.TokenID = dtable.Rows[0]["TokenID"].ToString();
                        remitInfo.Purpose = dtable.Rows[0]["Purpose"].ToString();
                    }
                }
                ViewBag.BeneficialName = remitInfo.BeneficialName;
                ViewBag.RequestTokenCode = remitInfo.RequestTokenCode;
                ViewBag.Amount = remitInfo.Amount;
                ViewBag.Note = remitInfo.Purpose;

                return Json(remitInfo, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        #endregion

        // GET: Remit
        public ActionResult Index()
        {
            return View();
        }


        #region "Request Token"

        // GET: Remit/RequestToken
        public ActionResult RequestToken()
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

                if (this.TempData["remit_messsage"] != null)
                {
                    this.ViewData["remit_messsage"] = this.TempData["remit_messsage"];
                    this.ViewData["message_class"] = this.TempData["message_class"];
                }

                ViewBag.SenderMobileNo = userName;

                ViewBag.ClientCode = clientCode;

                string code = TraceIdGenerator.GetUniqueKey();
                ViewBag.Code = code;

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


        // POST: Remit/RequestToken
        [HttpPost]
        public async Task<ActionResult> RequestToken(FormCollection collection, MNFundTransfer _ft)
        {
            var user=HttpContext.User;
            MNRemit remitInfo = new MNRemit();
            remitInfo.TraceID = collection["txtTraceID"].ToString();
            remitInfo.SenderMobileNo = collection["txtSenderMobileNo"].ToString();
            remitInfo.RecipientMobileNo = collection["txtRecipientNo"].ToString();
            remitInfo.BeneficialName = collection["txtBeneficialName"].ToString();
            remitInfo.RequestTokenCode = collection["txtRequestTokenCode"].ToString();
            remitInfo.Amount = collection["txtAmount"].ToString();
            remitInfo.Purpose = collection["txtNote"].ToString();
            remitInfo.ClientCode = collection["txtClientCode"].ToString();
            remitInfo.PIN = collection["txtPin"].ToString();
            remitInfo.TokenID = "";

            remitInfo.ServiceCode = collection["txtServiceCode"].ToString();
            remitInfo.SourceChannel = collection["txtSrc"].ToString();

            if (string.IsNullOrEmpty(remitInfo.Amount)) 
            {
                ModelState.AddModelError("Amount", "*Please enter Amount");
            }

            if (!ViewData.ModelState.IsValid)
            {
                this.TempData["registration_message"] = " *Validation Error.";
                this.TempData["message_class"] = "failed_info";
                return View();
            }

            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;
            this.ViewData["userType"] = this.TempData["userType"];
            ViewBag.UserType = this.TempData["userType"];



            bool result = false;
            string errorMessage = string.Empty;
            if ((remitInfo.TraceID != "") && (remitInfo.Amount != ""))
            {
                try
                {
                    int results = RemitUtils.InsertRemitInfo(remitInfo);
                    if (results > 0)
                    {
                        result = true;

                        HttpResponseMessage _res = new HttpResponseMessage();
                        //string tid = _ft.tid;
                        _ft.sc = "40";

                        //TraceIdGenerator _tig = new TraceIdGenerator();
                        //var tid = _tig.GenerateTraceID();

                        string tid = remitInfo.TraceID;
                        _ft.mobile = remitInfo.SenderMobileNo;
                        string mobile = _ft.mobile; //mobile is username
                        _ft.amount = remitInfo.Amount;
                        _ft.da = remitInfo.RecipientMobileNo;
                        _ft.pin = remitInfo.PIN;
                        _ft.note = remitInfo.Purpose;
                        _ft.sourcechannel = remitInfo.SourceChannel;

                        //new added
                        _ft.sa = "";
                        _ft.RequestTokenCode =remitInfo.RequestTokenCode;
                        _ft.ClientCode = "1";
                        _ft.BeneficialName = remitInfo.BeneficialName;
                      
                        /*tid=689622
                        &mobile=9803200158
                        &sc=40
                        &sa=
                        &da=9801207825
                        &amount=2
                        &code=5426
                        &note=
                        &pin=1234
                        &src=http*/

                        string Query = "";
                        using (HttpClient client = new HttpClient())
                        {
                            
                            var uri = "http://27.111.30.126/MNepal.WCF/remit/token?";
                            var content = new FormUrlEncodedContent(new[]
                            {
                                new KeyValuePair<string, string>("tid", tid),
                                new KeyValuePair<string,string>("sc",_ft.sc),
                                new KeyValuePair<string, string>("mobile",mobile),
                                new KeyValuePair<string, string>("amount", _ft.amount),
                                new KeyValuePair<string,string>("da",_ft.da),
                                new KeyValuePair<string,string>("pin",_ft.pin),
                                new KeyValuePair<string, string>("note", _ft.note),
                                new KeyValuePair<string,string>("src","http"),
                                new KeyValuePair<string, string>("code", _ft.RequestTokenCode),
                                new KeyValuePair<string,string>("sa",_ft.sa),
                                new KeyValuePair<string, string>("clientCode",_ft.ClientCode),
                                new KeyValuePair<string, string>("bn",_ft.BeneficialName)

                            });
                            
                            Query = content.ReadAsStringAsync().Result;
                            var URL = new Uri(uri + Query);
                            _res = await client.GetAsync(URL);
                            
                            int responseCode =(int)_res.StatusCode;
                            string message = string.Empty;


                            string responsetext = string.Empty;

                            string responseBody = _res.ReasonPhrase.ToString();// + " ," + await _res.Content.ReadAsStringAsync();
                           // _res.ReasonPhrase = responseBody;

                            if (_res.IsSuccessStatusCode)
                            {
                                RemitViewModel Rvm = new RemitViewModel();
                                responseCode = (int)_res.StatusCode;
                                responsetext = await _res.Content.ReadAsStringAsync();
                                //message = _res.Content.ReadAsStringAsync().Result;
                                dynamic json = JValue.Parse(responsetext);
                                // values require casting
                                message = json.d;

                                dynamic item = JValue.Parse(message);
                                int code = (int)item["StatusCode"];
                                JavaScriptSerializer ser = new JavaScriptSerializer();
                                ResponseMessage myNames = ser.Deserialize<ResponseMessage>((string)item["StatusMessage"]);
                                
                                ser.Serialize(myNames);

                                string av = myNames.AvailableBalance;
                                string msg = myNames.Message;
                                string amttransfer = myNames.AmountTransferredBalance;
                                Rvm.Message = myNames.Message;
                                Rvm.Balance = myNames.AvailableBalance;
                                Rvm.TokenAmount = myNames.AmountTransferredBalance;
                                Rvm.Sender = remitInfo.SenderMobileNo;
                                Rvm.Reciever = remitInfo.RecipientMobileNo;
                                Rvm.Token = myNames.RequestedToken;
                                Rvm.Method = "TRQ";

                                return View("RemitComplete",Rvm);


                            }
                            else
                            {
                                responseCode = (int)_res.StatusCode;
                                responsetext = await _res.Content.ReadAsStringAsync();

                                dynamic json = JValue.Parse(responsetext);
                                //values require casting
                                message = json.d;
                                dynamic item = JValue.Parse(message);
                             

                                result = false;
                                this.TempData["remit_messsage"] = "Error while inserting the information. ERROR :: "+(string)item["StatusMessage"];
                                this.TempData["message_class"] = "failed_info";
                                return this.RedirectToAction("RequestToken");

                            }
                         
                        }

                    }
                    else
                    {
                        result = false;
                        this.TempData["remit_messsage"] = result
                                                       ? "Remit information is successfully added."
                                                       : "Error while inserting the information. ERROR :: "
                                                         +"Could not insert Remit info.";
                        this.TempData["message_class"] = result ? "success_info" : "failed_info";
                            return this.RedirectToAction("RequestToken");

                    }
                }
                catch (Exception ex)
                {

                    result = false;
                    errorMessage = ex.Message;
                    this.TempData["remit_messsage"] = result
                                                      ? "Remit information is successfully added."
                                                      : "Error while inserting the information. ERROR :: "
                                                        + errorMessage;
                    this.TempData["message_class"] = result ? "success_info" : "failed_info";

                    return this.RedirectToAction("RequestToken");
                }
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

            //this.TempData["remit_messsage"] = result
            //                                  ? "Remit information is successfully added."
            //                                  : "Error while inserting the information. ERROR :: "
            //                                    + errorMessage;
            //this.TempData["message_class"] = result ? "success_info" : "failed_info";

            //return this.RedirectToAction("RequestToken");
        }



        //public ActionResult RemitComplete(RemitViewModel vm)
        //{
        //    return View();
        //}

        #endregion


        #region "TokenRedeem"
        // GET: Remit/TokenRedeem
        public ActionResult TokenRedeem()
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

                if (this.TempData["remit_messsage"] != null)
                {
                    this.ViewData["remit_messsage"] = this.TempData["remit_messsage"];
                    this.ViewData["message_class"] = this.TempData["message_class"];
                }

                ViewBag.AgentMobileNo = userName;

                //string code = TraceIdGenerator.GetUniqueKey();
                //@ViewBag.Code = code;

                //string tokenID = TraceIdGenerator.GetUniqueKey();
                //@ViewBag.TokenID = tokenID;

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


        // POST: Remit/TokenRedeem
        [HttpPost]
        public async Task<ActionResult> TokenRedeem(MNFundTransfer _ft)
        {
            bool result = false;
            string errorMessage;
            try
            {

                string userName = (string)Session["LOGGED_USERNAME"];
                string clientCode = (string)Session["LOGGEDUSER_ID"];
                string name = (string)Session["LOGGEDUSER_NAME"];
                string userType = (string)Session["LOGGED_USERTYPE"];

                TempData["userType"] = userType;
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];


                HttpResponseMessage _res = new HttpResponseMessage();
                //string tid = _ft.tid;
                string mobile = _ft.mobile; //mobile is username
                _ft.sc = "40";

                TraceIdGenerator _tig = new TraceIdGenerator();
                var tid = _tig.GenerateTraceID();

                using (HttpClient client = new HttpClient())
                {
                    var uri = "http://27.111.30.126/MNepal.WCF/remit/redeem";
                    var content = new FormUrlEncodedContent(new[]
                   {
                    new KeyValuePair<string, string>("tid", tid),
                    new KeyValuePair<string,string>("sc",_ft.sc),
                    new KeyValuePair<string, string>("agentmobile",mobile),
                    new KeyValuePair<string, string>("tokenId", _ft.RequestTokenCode),
                    new KeyValuePair<string,string>("photoId",""),
                    new KeyValuePair<string,string>("c",_ft.pin),
                    //new KeyValuePair<string,string>("pin",_ft.pin),
                    //new KeyValuePair<string, string>("note", _ft.note),
                    new KeyValuePair<string,string>("src",_ft.sourcechannel),
                    new KeyValuePair<string,string>("code",_ft.sa),
                    new KeyValuePair<string,string>("remitCustSender",_ft.RemitCusSender),
                    new KeyValuePair<string,string>("remitReceiver",_ft.RemitReceiver),
                    new KeyValuePair<string, string>("amount", _ft.amount),
                    //new KeyValuePair<string, string>("description","")

                });
                    _res = await client.PostAsync(new Uri(uri), content);

                    string responseBody = _res.StatusCode.ToString() + " ," + await _res.Content.ReadAsStringAsync();
                    _res.ReasonPhrase = responseBody;

                    errorMessage = string.Empty;

                    int responseCode = 0;
                    string message = string.Empty;
                    string responsetext = string.Empty;

                    if (_res.IsSuccessStatusCode)
                    {
                        responseCode = (int)_res.StatusCode;
                        responsetext = await _res.Content.ReadAsStringAsync();
                        //message = _res.Content.ReadAsStringAsync().Result;
                        dynamic json = JValue.Parse(responsetext);
                        // values require casting
                        message = json.d;

                        JavaScriptSerializer ser = new JavaScriptSerializer();

                        JsonParse msg = ser.Deserialize<JsonParse>(message);

                        ResponseMessage myNames = ser.Deserialize<ResponseMessage>(msg.StatusMessage);
                        RemitViewModel Rvm = new RemitViewModel();
                        string av = myNames.AvailableBalance;
                       string amttransfer = myNames.AmountTransferredBalance;
                        Rvm.Method = "TRD";
                        Rvm.Message = myNames.Message;
                        Rvm.TokenAmount = myNames.AmountTransferredBalance;
                        return View("RemitComplete", Rvm);

                    }
                    else
                    {
                        responseCode = (int)_res.StatusCode;
                        responsetext = await _res.Content.ReadAsStringAsync();

                        dynamic json = JValue.Parse(responsetext);
                        // values require casting
                        message = json.d;
                        result = false;
                        this.TempData["remit_messsage"] = result
                                                       ? "Remit information is successfully added."
                                                       : "Error while inserting the information. ERROR :: "
                                                         + message;
                        this.TempData["message_class"] = result ? "success_info" : "failed_info";
                        return this.RedirectToAction("TokenRedeem");
                    }

                    //return Json(new { responseCode = responseCode, responseText = message },
                    //    JsonRequestBehavior.AllowGet);
                }
            }
            catch(Exception ex)
            {
                result = false;
                errorMessage = ex.Message;
                this.TempData["remit_messsage"] = result
                                                  ? "Remit information is successfully added."
                                                  : "Error while inserting the information. ERROR :: "
                                                    + errorMessage;
                this.TempData["message_class"] = result ? "success_info" : "failed_info";

                return this.RedirectToAction("TokenRedeem");
            }

        }

        #endregion


        

    }
}