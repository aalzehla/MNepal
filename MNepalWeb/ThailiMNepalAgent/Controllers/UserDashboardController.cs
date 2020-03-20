using ThailiMNepalAgent.Models;
using ThailiMNepalAgent.Utilities;
using System.Data;
using System.Web.Mvc;
using System.Web.SessionState;
using System.Net.Http;
using ThailiMNepalAgent.Helper;
using System.IO;
using ThailiMNepalAgent.App_Start;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;

namespace ThailiMNepalAgent.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class UserDashboardController : Controller
    {
        // GET: UserDashboardContent
        public ActionResult Index()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            //ViewBag.bankBalance = (string)Session["bankBal"];

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;
                Session["bankbalance"] = "";
                ViewBag.BankBal = Session["bankbalance"];



                MNBalance availBaln = new MNBalance();
                DataTable dtableUser = AvailBalnUtils.GetAvailBaln(clientCode);
                if (dtableUser != null && dtableUser.Rows.Count > 0)
                {
                    availBaln.amount = dtableUser.Rows[0]["AvailBaln"].ToString();

                    ViewBag.AvailBalnAmount = availBaln.amount;
                }

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
            else
            {
                return RedirectToAction("Index","Login");
            }
        }

        

        //start milyako 03
        public async Task<ActionResult> BankQuery()
        {
            string username = (string)Session["LOGGED_USERNAME"];
            string pin = "";
            if ((username != "") || (username != null))
            {
                   
                string result = string.Empty;
                DataTable dtableMobileNo = CustomerUtils.GetUserProfileByMobileNo(username);
                if (dtableMobileNo.Rows.Count == 1)
                {
                   pin= dtableMobileNo.Rows[0]["PIN"].ToString();
                }
               
            }
            
            HttpResponseMessage _res = new HttpResponseMessage();

            string mobile = username; //mobile is username
         

            TraceIdGenerator _tig = new TraceIdGenerator();
            var tid = _tig.GenerateTraceID();

            using (HttpClient client = new HttpClient())
            {

                var action = "query.svc/balance?tid="+tid+"&sc=22&mobile="+mobile+"&sa=1&pin="+pin+"&src=web";
                var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                //var content = new FormUrlEncodedContent(new[]{
                //        new KeyValuePair<string, string>("tid", tid),
                //        new KeyValuePair<string,string>("sc","22"),
                //        new KeyValuePair<string, string>("mobile",mobile),
                //        new KeyValuePair<string, string>("sa", "1"),
                //        new KeyValuePair<string,string>("pin", pin),
                //        new KeyValuePair<string,string>("src","web")
                //    });
                try
                {
                    _res = await client.GetAsync(uri);
                string responseBody = _res.StatusCode.ToString() + " ," + await _res.Content.ReadAsStringAsync();
                _res.ReasonPhrase = responseBody;
                string errorMessage = string.Empty;
                int responseCode = 0;
                string message = string.Empty;
                string responsetext = string.Empty;
                string responsedateTime = string.Empty;
                bool result = false;
                string ava = string.Empty;
                string avatra = string.Empty;
                string avamsg = string.Empty;
              
                    if (_res.IsSuccessStatusCode)
                    {
                        result = true;
                        responseCode = (int)_res.StatusCode;
                        responsetext = await _res.Content.ReadAsStringAsync();
                        responsedateTime = await _res.Content.ReadAsStringAsync();
                        message = _res.Content.ReadAsStringAsync().Result;
                        string respmsg = "";
                        string dateTime = "";
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
                        this.Session["bankSyncTime"] = DateTime.Now;
                        dateTime = Session["bankSyncTime"].ToString();
                        this.Session["bankbal"] = respmsg;
                        ViewBag.AvailBankBalnAmount = (string)respmsg;
                        return Json(new { responseCode = responseCode, responseText = respmsg, responsedateTime = dateTime },
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
                            ViewBag.AvailBankBalnAmount = (string)item["StatusMessage"];
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
            }
        }
       

        public ActionResult WalletQuery()
        {
            string clientCode = (string)Session["LOGGEDUSER_ID"];

            MNBalance availBaln = new MNBalance();
            DataTable dtableUser = AvailBalnUtils.GetAvailBaln(clientCode);
            if (dtableUser != null && dtableUser.Rows.Count > 0)
            {
                string dateTime = "";
                availBaln.amount = dtableUser.Rows[0]["AvailBaln"].ToString();

                ViewBag.AvailBalnAmount = availBaln.amount;

                this.Session["walletSync"] = DateTime.Now;
                dateTime = Session["walletSync"].ToString();

                return Json(new { responseCode = 200, responseText = availBaln.amount, responsedateTime = dateTime },
                       JsonRequestBehavior.AllowGet);

            }
            return null;
        }

        //end milayako 03
    }
}