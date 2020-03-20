using MNepalWeb.App_Start;
using MNepalWeb.Helper;
using MNepalWeb.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.SessionState;

namespace MNepalWeb.Controllers
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
                    int code = (int)item["StatusCode"];
                    message= (string)item["StatusMessage"];
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
        [HttpPost]
        public async Task<ActionResult> Out(MNFundTransfer _ft)
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

        #endregion


    }
}