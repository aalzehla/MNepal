using CustApp.Models;
using CustApp.Utilities;
using System.Data;
using System.Web.Mvc;
using System.Web.SessionState;
using System.Net.Http;
using CustApp.Helper;
using System.IO;
using CustApp.App_Start;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Configuration;
using System.Text;
using static CustApp.Models.Notifications;
using Newtonsoft.Json;
using System.Collections;

namespace CustApp.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class UserDashboardController : Controller
    {
        // GET: UserDashboardContent
        [HttpGet]
        public ActionResult Index()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            //ViewBag.bankBalance = (string)Session["bankBal"];

            if (this.TempData["user_message"] != null)
            {
                this.ViewData["user_message"] = this.TempData["user_message"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            TempData["userType"] = userType;

            //Check all three variables Session1, Session2, Cookie. If all the three are not null then procces futher
            if (Session["LOGGED_USERNAME"] != null && Session["AuthToken"] != null && Request.Cookies["AuthToken"] != null)
            {
                //Second Check, if Cookies we created has the same value as second session we've created
                //if (Request.Cookies["AuthToken"].Value == Session["AuthToken"].ToString())
                if (Session["AuthToken"].ToString().Equals(
                           Request.Cookies["AuthToken"].Value))
                {

                    if (TempData["userType"] != null)
                    {
                        DataTable dtableUserCheckFirstLogin = ProfileUtils.IsFirstLogin(clientCode);
                        if (dtableUserCheckFirstLogin != null && dtableUserCheckFirstLogin.Rows.Count > 0)
                        {
                            ViewBag.IsFirstLogin = dtableUserCheckFirstLogin.Rows[0]["IsFirstLogin"].ToString();
                            ViewBag.PinChanged = dtableUserCheckFirstLogin.Rows[0]["PinChanged"].ToString();
                            ViewBag.PassChanged = dtableUserCheckFirstLogin.Rows[0]["PassChanged"].ToString();
                        }

                        if (TempData["userType"] != null && ViewBag.IsFirstLogin == "F" && ViewBag.PinChanged == "T" && ViewBag.PassChanged == "T")
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
                            Session["LOGGED_USERNAME"] = null;
                            return RedirectToAction("Index", "Login");
                        }
                    }
                    else
                    {
                        return RedirectToAction("Index", "Login");
                    }

                }
                else
                {
                    Session["LOGGED_USERNAME"] = null;
                    return RedirectToAction("Index", "Login");
                }
            }
            else
            {
                Session["LOGGED_USERNAME"] = null;
                return RedirectToAction("Index", "Login");
            }

        }

        [HttpGet]
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
                string IsRejected = string.Empty;
                UserInfo userInfo = new UserInfo();

                //Check KYC
                DataTable dtableUserCheckKYC = ProfileUtils.CheckKYC(userName);
                if (dtableUserCheckKYC != null && dtableUserCheckKYC.Rows.Count > 0)
                {
                    userInfo.hasKYC = dtableUserCheckKYC.Rows[0]["hasKYC"].ToString();
                    userInfo.IsRejected = dtableUserCheckKYC.Rows[0]["IsRejected"].ToString();

                    ViewBag.hasKYC = userInfo.hasKYC;
                    ViewBag.IsRejected = userInfo.IsRejected;
                    hasKYC = ViewBag.hasKYC;
                    IsRejected = ViewBag.IsRejected;

                }
                return Json(hasKYC, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        //start milyako
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
                    pin = dtableMobileNo.Rows[0]["PIN"].ToString();
                }

            }

            HttpResponseMessage _res = new HttpResponseMessage();

            string mobile = username; //mobile is username


            TraceIdGenerator _tig = new TraceIdGenerator();
            var tid = _tig.GenerateTraceID();

            using (HttpClient client = new HttpClient())
            {

                var action = "query.svc/balance?tid=" + tid + "&sc=22&mobile=" + mobile + "&sa=1&pin=" + pin + "&src=web";
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
                        this.Session["bankSyncTime"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt");
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
        //end milayako

        [HttpGet]
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

                this.Session["walletSync"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt"); ;
                dateTime = Session["walletSync"].ToString();

                return Json(new { responseCode = 200, responseText = availBaln.amount, responsedateTime = dateTime },
                       JsonRequestBehavior.AllowGet);

            }
            return null;
        }

        [HttpGet]
        public ActionResult CheckKYCRejected()
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
                string IsRejected = string.Empty;
                var remarks = string.Empty;
                UserInfo userInfo = new UserInfo();

                //Check KYC
                DataTable dtableUserCheckKYC = ProfileUtils.CheckKYC(userName);
                if (dtableUserCheckKYC != null && dtableUserCheckKYC.Rows.Count > 0)
                {
                    userInfo.hasKYC = dtableUserCheckKYC.Rows[0]["hasKYC"].ToString();
                    userInfo.IsRejected = dtableUserCheckKYC.Rows[0]["IsRejected"].ToString();
                    userInfo.Remarks = dtableUserCheckKYC.Rows[0]["Remarks"].ToString();

                    ViewBag.hasKYC = userInfo.hasKYC;
                    ViewBag.IsRejected = userInfo.IsRejected;
                    ViewBag.Remarks = userInfo.Remarks;
                    remarks = userInfo.Remarks;
                    IsRejected = ViewBag.IsRejected;

                }
                var result = new { IsRejected, remarks };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpGet]
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

       
    }
}