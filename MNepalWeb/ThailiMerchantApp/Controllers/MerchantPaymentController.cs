using ThailiMerchantApp.App_Start;
using ThailiMerchantApp.Helper;
using ThailiMerchantApp.Models;
using ThailiMerchantApp.UserModels;
using ThailiMerchantApp.ViewModel;
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
using ThailiMerchantApp.Utilities;

namespace ThailiMerchantApp.Controllers
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
                // int amt = System.Convert.ToInt32(merchantcollege.Amount) /*Int32.Parse(merchantcollege.Amount)*/;
                //if (amt < 10 || amt > 9999)
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
    #endregion
}
}