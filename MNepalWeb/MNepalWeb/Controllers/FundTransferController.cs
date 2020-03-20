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
using MNepalWeb.App_Start;

namespace MNepalWeb.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class FundTransferController : Controller
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

        // GET: FundTransfer
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

            using (HttpClient client = new HttpClient())    {

                var action = "ft/request";
                var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                var content = new FormUrlEncodedContent(new[]{
                        new KeyValuePair<string, string>("tid", tid),
                        new KeyValuePair<string,string>("sc",_ft.sc),
                        new KeyValuePair<string, string>("mobile",mobile),
                        new KeyValuePair<string, string>("amount", _ft.amount),
                        new KeyValuePair<string,string>("da",_ft.da),
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
                bool result = false;
                string ava = string.Empty;
                string avatra = string.Empty;
                string avamsg = string.Empty;
                try{
                    if (_res.IsSuccessStatusCode){
                        result = true;
                        responseCode = (int)_res.StatusCode;
                        responsetext = await _res.Content.ReadAsStringAsync();
                        message = _res.Content.ReadAsStringAsync().Result;
                        string respmsg = "";
                        if (!string.IsNullOrEmpty(message)){
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
                    else {
                        result = false;
                        responseCode = (int)_res.StatusCode;
                        responsetext = await _res.Content.ReadAsStringAsync();
                        dynamic json = JValue.Parse(responsetext);
                        message = json.d;
                        dynamic item = JValue.Parse(message);

                        return Json(new { responseCode = responseCode, responseText = (string)item["StatusMessage"] },
                        JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex){
                    return Json(new { responseCode = "400", responseText = ex.Message },
                        JsonRequestBehavior.AllowGet);
                }
                this.TempData["fundTransfer_messsage"] = result
                                                ? "Fund Transfer successfully." + message
                                                : "ERROR :: " + message;
                this.TempData["message_class"] = result ? "success_info" : "failed_info";

                return RedirectToAction("Index", "FundTransfer");
            }


        }

        #endregion

    }
}