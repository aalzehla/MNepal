using CustApp.App_Start;
using CustApp.Helper;
using CustApp.Models;
using CustApp.UserModels;
using CustApp.ViewModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Data;
using CustApp.Utilities;

namespace CustApp.Controllers
{
    public class MerchantPaymentController : Controller
    {
        public ActionResult MerchantIndex()
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

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }


        }

        #region College

        // GET: MerchantCollege
        [HttpGet]
        public ActionResult MerchantCollege()
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

                ViewBag.year = DateTime.Now.Year.ToString();
                ViewBag.nextYear = (DateTime.Now.Year + 1).ToString();

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

                ReportUserModel rep = new ReportUserModel();
                ViewBag.College = rep.GetMerchantListbyCategory("4").Select(x =>
                                  new SelectListItem()
                                  {
                                      Text = x.MName,
                                      Value = x.MId
                                  });
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }


        }

        // POST: MerchantPayment/MerchantCollege
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MerchantCollege(MerchantCollege merchantcollege)
        {

            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            this.ViewData["userType"] = this.TempData["userType"];
            ViewBag.UserType = this.TempData["userType"];
            ViewBag.Name = name;


            string retoken = merchantcollege.TokenUnique;
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

                ReportUserModel rep = new ReportUserModel();
                ViewBag.College = rep.GetMerchantListbyCategory("4").Select(x =>
                                  new SelectListItem()
                                  {
                                      Text = x.MName,
                                      Value = x.MId
                                  });

                //validation
                if (merchantcollege.TPin.Length != 4)
                {
                    ModelState.AddModelError("TPin", "T-Pin should be 4 digits long.");
                }
                //if (merchantcollege.Amount < 10 || merchantcollege.Amount > 9999)
                //{
                //    ModelState.AddModelError("Amount", "Amount is not valid.");
                //}
                //if (merchantcollege.Amount % 10 != 0)
                //{
                //    ModelState.AddModelError("Amount", "Amount is not valid.");
                //}

                //api call here
                HttpResponseMessage _res = new HttpResponseMessage();
                TraceIdGenerator _tig = new TraceIdGenerator();
                var tid = _tig.GenerateTraceID();

                string mobile = userName; //mobile is username
                using (HttpClient client = new HttpClient())
                {

                    var action = "merchant.svc/payment";
                    var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                    var content = new FormUrlEncodedContent(new[]
                   {
                         new KeyValuePair<string, string>("sc",merchantcollege.TransactionMedium),
                         new KeyValuePair<string, string>("vid",merchantcollege.CollegeName),
                         new KeyValuePair<string, string>("mobile",mobile),

                         new KeyValuePair<string, string>("billNo",merchantcollege.BillNumber.ToString()),
                         new KeyValuePair<string, string>("studName",merchantcollege.StudentName.ToString()),
                         new KeyValuePair<string, string>("merchantType","college"),

                         new KeyValuePair<string, string>("class",merchantcollege.Class),
                         new KeyValuePair<string, string>("year",merchantcollege.Year.ToString()),
                         new KeyValuePair<string, string>("month",merchantcollege.Month),
                         new KeyValuePair<string, string>("rollNo",merchantcollege.RollNumber.ToString()),
                         new KeyValuePair<string, string>("remarks",merchantcollege.Remarks),

                         new KeyValuePair<string, string>("sa",""),
                         new KeyValuePair<string, string>("prod","1"),
                         new KeyValuePair<string, string>("amount",merchantcollege.Amount),
                         new KeyValuePair<string, string>("tid",tid),
                         new KeyValuePair<string, string>("pin",merchantcollege.TPin),
                         new KeyValuePair<string, string>("note","College Payment successful"),
                         new KeyValuePair<string, string>("src","http"),
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
                                //start 01
                                //  message = (string)item["StatusMessage"];
                                //end 01
                                return Json(new { responseCode = responseCode, responseText = (string)item["StatusMessage"] },
                                JsonRequestBehavior.AllowGet);
                            }

                            //    string StatusMessage = "";
                            //responseCode = (int)_res.StatusCode;
                            //responsetext = await _res.Content.ReadAsStringAsync();
                            //if (responseCode == 404)
                            //{
                            //    StatusMessage = "Could not connect to the server";
                            //}
                            //else
                            //{
                            //    dynamic json = JValue.Parse(responsetext);
                            //    // values require casting
                            //    message = json.d;
                            //    result = false;
                            //    JavaScriptSerializer ser = new JavaScriptSerializer();
                            //    JsonParse msg = ser.Deserialize<JsonParse>(message);
                            //    StatusMessage = msg.StatusMessage;


                            //}
                            //this.ViewData["topup_messsage"] = StatusMessage;
                            //this.ViewData["message_class"] = "failed_info";
                            //return View(topup);
                        }

                    }
                    catch (Exception ex)
                    {
                        return Json(new { responseCode = "400", responseText = ex.Message },
                        JsonRequestBehavior.AllowGet);
                    }

                    this.TempData["merchantpay_messsage"] = result
                                            ? "Payment successfully." + message
                                            : "ERROR :: " + message;
                    this.TempData["message_class"] = result ? "success_info" : "failed_info";
                    return RedirectToAction("MerchantCollege", "MerchantPayment");

                }

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        #endregion


        #region Restaurant

        [HttpGet]
        public ActionResult MerchantRestaurant()
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

                ReportUserModel rep = new ReportUserModel();
                ViewBag.Restaurant = rep.GetMerchantListbyCategory("5").Select(x =>
                                  new SelectListItem()
                                  {
                                      Text = x.MName,
                                      Value = x.MId
                                  });

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }


        }

        // POST: MerchantPayment/MerchantRestaurant
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MerchantRestaurant(MerchantRestaurant merchantrest)
        {
            //bool result = false;
            //string errorMessage;
            //try
            //{
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            this.ViewData["userType"] = this.TempData["userType"];
            ViewBag.UserType = this.TempData["userType"];
            ViewBag.Name = name;


            string retoken = merchantrest.TokenUnique;
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
                //api call here
                HttpResponseMessage _res = new HttpResponseMessage();
                TraceIdGenerator _tig = new TraceIdGenerator();
                var tid = _tig.GenerateTraceID();

                string mobile = userName; //mobile is username
                using (HttpClient client = new HttpClient())
                {

                    var action = "merchant.svc/payment";
                    var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                    var content = new FormUrlEncodedContent(new[]
                   {
                         new KeyValuePair<string, string>("sc",merchantrest.TransactionMedium),
                         new KeyValuePair<string, string>("vid",merchantrest.RestaurantName),
                         new KeyValuePair<string, string>("mobile",mobile),

                         new KeyValuePair<string, string>("billNo",merchantrest.BillNumber.ToString()),
                         new KeyValuePair<string, string>("merchantType","restaurant"),
                         new KeyValuePair<string, string>("remarks",merchantrest.Remarks),

                         new KeyValuePair<string, string>("sa",""),
                         new KeyValuePair<string, string>("prod","1"),
                         new KeyValuePair<string, string>("amount",merchantrest.Amount),
                         new KeyValuePair<string, string>("tid",tid),
                         new KeyValuePair<string, string>("pin",merchantrest.TPin),
                         new KeyValuePair<string, string>("note","Restaurant Payment successful"),
                         new KeyValuePair<string, string>("src","http"),
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
                                //start 01
                                //  message = (string)item["StatusMessage"];
                                //end 01
                                return Json(new { responseCode = responseCode, responseText = (string)item["StatusMessage"] },
                                JsonRequestBehavior.AllowGet);
                            }

                            //    string StatusMessage = "";
                            //responseCode = (int)_res.StatusCode;
                            //responsetext = await _res.Content.ReadAsStringAsync();
                            //if (responseCode == 404)
                            //{
                            //    StatusMessage = "Could not connect to the server";
                            //}
                            //else
                            //{
                            //    dynamic json = JValue.Parse(responsetext);
                            //    // values require casting
                            //    message = json.d;
                            //    result = false;
                            //    JavaScriptSerializer ser = new JavaScriptSerializer();
                            //    JsonParse msg = ser.Deserialize<JsonParse>(message);
                            //    StatusMessage = msg.StatusMessage;


                            //}
                            //this.ViewData["topup_messsage"] = StatusMessage;
                            //this.ViewData["message_class"] = "failed_info";
                            //return View(topup);
                        }

                    }
                    catch (Exception ex)
                    {
                        return Json(new { responseCode = "400", responseText = ex.Message },
                        JsonRequestBehavior.AllowGet);
                    }

                    this.TempData["merchantpay_messsage"] = result
                                            ? "Payment successfully." + message
                                            : "ERROR :: " + message;
                    this.TempData["message_class"] = result ? "success_info" : "failed_info";
                    return RedirectToAction("MerchantRestaurant", "MerchantPayment");

                }

            }
            else
            {
                return Json(new { responseCode = "400", responseText = "Please refresh the page again." },
                            JsonRequestBehavior.AllowGet);
            }

        }


        #endregion


        #region School

        [HttpGet]
        public ActionResult MerchantSchool()
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

                ViewBag.year = DateTime.Now.Year.ToString();
                ViewBag.nextYear = (DateTime.Now.Year + 1).ToString();

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

                ReportUserModel rep = new ReportUserModel();
                ViewBag.School = rep.GetMerchantListbyCategory("3").Select(x =>
                                  new SelectListItem()
                                  {
                                      Text = x.MName,
                                      Value = x.MId
                                  });
                return View();


            }
            else
            {
                return RedirectToAction("Index", "Login");
            }


        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> MerchantSchool(MerchantSchool merchantschool)
        {

            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            this.ViewData["userType"] = this.TempData["userType"];
            ViewBag.UserType = this.TempData["userType"];
            ViewBag.Name = name;


            string retoken = merchantschool.TokenUnique;
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

                ReportUserModel rep = new ReportUserModel();
                ViewBag.School = rep.GetMerchantListbyCategory("3").Select(x =>
                                  new SelectListItem()
                                  {
                                      Text = x.MName,
                                      Value = x.MId
                                  });

                //validation
                if (merchantschool.TPin.Length != 4)
                {
                    ModelState.AddModelError("TPin", "T-Pin should be 4 digits long.");
                }
                //if (merchantschool.Amount < 10 || merchantschool.Amount > 9999)
                //{
                //    ModelState.AddModelError("Amount", "Amount is not valid.");
                //}
                //if (merchantcollege.Amount % 10 != 0)
                //{
                //    ModelState.AddModelError("Amount", "Amount is not valid.");
                //}



                //api call here
                HttpResponseMessage _res = new HttpResponseMessage();
                TraceIdGenerator _tig = new TraceIdGenerator();
                var tid = _tig.GenerateTraceID();

                string mobile = userName; //mobile is username
                using (HttpClient client = new HttpClient())
                {

                    var action = "merchant.svc/payment";
                    var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                    var content = new FormUrlEncodedContent(new[]
                   {
                         new KeyValuePair<string, string>("sc",merchantschool.TransactionMedium),
                         new KeyValuePair<string, string>("vid",merchantschool.SchoolName),
                         new KeyValuePair<string, string>("mobile",mobile),
                         new KeyValuePair<string, string>("sa",""),
                         new KeyValuePair<string, string>("prod","1"),
                         new KeyValuePair<string, string>("amount",merchantschool.Amount),
                         new KeyValuePair<string, string>("tid",tid),

                         new KeyValuePair<string, string>("billNo",merchantschool.BillNumber.ToString()),
                         new KeyValuePair<string, string>("studName",merchantschool.StudentName.ToString()),
                         new KeyValuePair<string, string>("merchantType","school"),

                         new KeyValuePair<string, string>("class",merchantschool.Class),
                         new KeyValuePair<string, string>("year",merchantschool.Year.ToString()),
                         new KeyValuePair<string, string>("month",merchantschool.Month),
                         new KeyValuePair<string, string>("rollNo",merchantschool.RollNumber.ToString()),
                         new KeyValuePair<string, string>("remarks",merchantschool.Remarks),

                         new KeyValuePair<string, string>("pin",merchantschool.TPin),
                         new KeyValuePair<string, string>("note","College Payment successful"),
                         new KeyValuePair<string, string>("src","http"),
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
                                //start 01
                                //  message = (string)item["StatusMessage"];
                                //end 01
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

                    this.TempData["merchantpay_messsage"] = result
                                            ? "Payment successfully." + message
                                            : "ERROR :: " + message;
                    this.TempData["message_class"] = result ? "success_info" : "failed_info";
                    return RedirectToAction("MerchantSchool", "MerchantPayment");

                }

            }
            else
            {
                return Json(new { responseCode = "400", responseText = "Please refresh the page again." },
                             JsonRequestBehavior.AllowGet);
            }
        }
        
        
        #endregion


        #region Insurance

        [HttpGet]
        public ActionResult MerchantInsurance()
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

                ViewBag.year = DateTime.Now.Year.ToString();
                ViewBag.nextYear = (DateTime.Now.Year + 1).ToString();

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

                ReportUserModel rep = new ReportUserModel();
                ViewBag.Insurance = rep.GetMerchantListbyCategory("8").Select(x =>
                                  new SelectListItem()
                                  {
                                      Text = x.MName,
                                      Value = x.MId
                                  });
                return View();


            }
            else
            {
                return RedirectToAction("Index", "Login");
            }


        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> MerchantInsurance(MerchantInsurance merchantInsurance)
        {

            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            this.ViewData["userType"] = this.TempData["userType"];
            ViewBag.UserType = this.TempData["userType"];
            ViewBag.Name = name;


            string retoken = merchantInsurance.TokenUnique;
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

                ReportUserModel rep = new ReportUserModel();
                ViewBag.School = rep.GetMerchantListbyCategory("3").Select(x =>
                                  new SelectListItem()
                                  {
                                      Text = x.MName,
                                      Value = x.MId
                                  });

                //validation
                if (merchantInsurance.TPin.Length != 4)
                {
                    ModelState.AddModelError("TPin", "T-Pin should be 4 digits long.");
                }
                //if (merchantschool.Amount < 10 || merchantschool.Amount > 9999)
                //{
                //    ModelState.AddModelError("Amount", "Amount is not valid.");
                //}
                //if (merchantcollege.Amount % 10 != 0)
                //{
                //    ModelState.AddModelError("Amount", "Amount is not valid.");
                //}



                //api call here
                HttpResponseMessage _res = new HttpResponseMessage();
                TraceIdGenerator _tig = new TraceIdGenerator();
                var tid = _tig.GenerateTraceID();

                string mobile = userName; //mobile is username
                using (HttpClient client = new HttpClient())
                {

                    var action = "merchant.svc/payment";
                    var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                    var content = new FormUrlEncodedContent(new[]
                   {
                         new KeyValuePair<string, string>("sc",merchantInsurance.TransactionMedium),
                         new KeyValuePair<string, string>("vid",merchantInsurance.InsuranceName),
                         new KeyValuePair<string, string>("mobile",mobile),
                         new KeyValuePair<string, string>("sa",""),
                         new KeyValuePair<string, string>("prod","1"),
                         new KeyValuePair<string, string>("amount",merchantInsurance.Amount),
                         new KeyValuePair<string, string>("tid",tid),

                         new KeyValuePair<string, string>("billNo",merchantInsurance.PolicyNumber),
                         new KeyValuePair<string, string>("studName",merchantInsurance.CustomerName),
                         new KeyValuePair<string, string>("merchantType","insurance"),

                         new KeyValuePair<string, string>("class",merchantInsurance.Type),
                         new KeyValuePair<string, string>("year",merchantInsurance.CustomerAddress),
                         new KeyValuePair<string, string>("month",merchantInsurance.EmailAddress),
                         new KeyValuePair<string, string>("rollNo",merchantInsurance.MobileNumber),
                         new KeyValuePair<string, string>("remarks",merchantInsurance.Remarks),
                         new KeyValuePair<string, string>("agentName",merchantInsurance.AgentName),


                         new KeyValuePair<string, string>("pin",merchantInsurance.TPin),
                         new KeyValuePair<string, string>("note","Insurance Payment successful"),
                         new KeyValuePair<string, string>("src","http"),
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
                                //start 01
                                //  message = (string)item["StatusMessage"];
                                //end 01
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

                    this.TempData["merchantpay_messsage"] = result
                                            ? "Payment successfully." + message
                                            : "ERROR :: " + message;
                    this.TempData["message_class"] = result ? "success_info" : "failed_info";
                    return RedirectToAction("MerchantSchool", "MerchantPayment");

                }

            }
            else
            {
                return Json(new { responseCode = "400", responseText = "Please refresh the page again." },
                            JsonRequestBehavior.AllowGet);
            }

        }
        

        #endregion

    }
}