using ThailiMNepalAgent.App_Start;
using ThailiMNepalAgent.Helper;
using ThailiMNepalAgent.Models;
using ThailiMNepalAgent.Utilities;
using ThailiMNepalAgent.ViewModel;
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

namespace ThailiMNepalAgent.Controllers
{
    public class UtilityPayController : Controller
    {                                                                                                                                               
        // GET: TopUp
        public ActionResult TopUp()
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

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        #region NT TopUp
        //GET NT TopUp
        [HttpGet]
        public ActionResult NTTopUp()
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

                if (this.ViewData["topup_messsage"] != null)
                {
                    this.ViewData["topup_messsage"] = this.TempData["topup_messsage"];
                    this.ViewData["message_class"] = this.TempData["message_class"];
                }

                //end milayako

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

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        //test//
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> NTTopUp (TopUpPay topup)
        {

            //start 01
           // bool result = false;
            //string errorMessage = string.Empty;
            //string message = string.Empty;
            
          //  try
           // {
                //end 01
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
            ///start 01
            //int responseCode = 0;
            // string responsetext = string.Empty;
            //end 01

            //end milayako

            ////validation
            //if (topup.MobileNumber.Length != 10)
            //{
            //    ModelState.AddModelError("MobileNumber", "Mobile Number should be 10 digits long.");
            //}
            //if (!topup.MobileNumber.Substring(0, 3).IsInCollection(ApplicationInitilize.NTCStarters))
            //{
            //    ModelState.AddModelError("MobileNumber", "Invalid NTC Number.");
            //}
            //if (topup.Amount < 10 || topup.Amount > 5000)
            //{
            //    ModelState.AddModelError("Amount", "Amount is not valid.");
            //}
            //if (topup.Amount % 10 != 0)
            //{
            //    ModelState.AddModelError("Amount", "Amount is not valid.");
            //}

            //start 01
            //  if (ModelState.IsValid)
            // {

            //end 01
            //api call here
            HttpResponseMessage _res = new HttpResponseMessage();
                    TraceIdGenerator _tig = new TraceIdGenerator();
                    var tid = _tig.GenerateTraceID();
            //string mobile = topup.MobileNumber; //mobile is username
            string mobile = userName; //mobile is username

                    using (HttpClient client = new HttpClient())
                    {
                       
                        var action = "topup.svc/payment";
                        var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                        var content = new FormUrlEncodedContent(new[]
                       {
                         new KeyValuePair<string, string>("sc",topup.TransactionMedium),
                         new KeyValuePair<string, string>("vid","2"),
                         new KeyValuePair<string, string>("mobile",mobile),

                         new KeyValuePair<string, string>("destmobile",topup.MobileNumber),
                        
                         new KeyValuePair<string, string>("amount",topup.Amount.ToString()),
                         new KeyValuePair<string, string>("tid",tid),
                         new KeyValuePair<string, string>("pin",topup.Pin),
                         new KeyValuePair<string, string>("note","Successfully topup to" + topup.MobileNumber),
                         new KeyValuePair<string, string>("src","http"),
                         new KeyValuePair<string,string>("tokenID",Session["TokenID"].ToString())

                       });
                        _res = await client.PostAsync(new Uri(uri), content);

                        string responseBody = _res.StatusCode.ToString() + " ," + await _res.Content.ReadAsStringAsync();
                          _res.ReasonPhrase = responseBody;

                        /// start 01
                        string errorMessage = string.Empty;
                        int responseCode = 0;
                        string message = string.Empty;
                        string responsetext = string.Empty;
                        bool result = false;
                        string ava = string.Empty;
                        string avatra = string.Empty;
                        string avamsg = string.Empty;
                        //end 01

                        try { 
                        
                        if (_res.IsSuccessStatusCode){
                            result = true;
                            responseCode = (int)_res.StatusCode;
                            responsetext = await _res.Content.ReadAsStringAsync();
                            //message = _res.Content.ReadAsStringAsync().Result;
                            //dynamic json = JValue.Parse(responsetext);
                            //// values require casting
                            //message = json.d;

                            message = _res.Content.ReadAsStringAsync().Result;
                                string respmsg = "";
                                if (!string.IsNullOrEmpty(message))
                                {
                                    JavaScriptSerializer ser = new JavaScriptSerializer();
//start 01
                            //string respmsg = "";
                            //end 01
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

                            //JsonParse msg = ser.Deserialize<JsonParse>(message);
                            //if (msg.StatusCode == "200")
                            //{
                            //    ResponseMessage myNames = ser.Deserialize<ResponseMessage>(msg.StatusMessage);

                            //    //UtilityViewModel UVM = new UtilityViewModel();
                            //    //string av = myNames.AvailableBalance;
                            //    //string amttransfer = myNames.AmountTransferredBalance;

                            //    //UVM.Method = "TOPUP";
                            //    //UVM.Message = "NT Postpaid TOPUP Successful ";//myNames.Message;
                            //    //UVM.Amount = myNames.AmountTransferredBalance;
                            //    //UVM.Balance= myNames.AvailableBalance;
                            //    //UVM.Sender = userName;
                            //    //UVM.Reciever = topup.MobileNumber;
                            //    //UVM.Tid = tid;


                            //    //return View("UtilityComplete", UVM);
                            //    this.ViewData["topup_messsage"] = msg.StatusMessage;
                            //    this.ViewData["message_class"] = "success_info";
                            //    return View(topup);
                            //}
                            //else
                            //{
                            //    this.ViewData["topup_messsage"] = msg.StatusMessage;
                            //    this.ViewData["message_class"] = "failed_info";
                            //    return View(topup);
                            //}


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

                        this.TempData["topup_messsage"] = result
                                                ? "Topup successfully." + message
                                                : "ERROR :: " + message;
                        this.TempData["message_class"] = result ? "success_info" : "failed_info";
                        return RedirectToAction("NTTopUp", "UtilityPay");

                    }
                }

                //start01
        //        else
        //        {
        //            result = false;
        //            message = "Validation Error, Please recheck and submit";
        //            //this.ViewData["topup_messsage"] = "Validation Error, Please recheck and submit";
        //            this.ViewData["message_class"] = "failed_info";
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        result = false;
        //        message = "Server Error, Please Contact administrator !" + ex.Message;
        //        this.ViewData["topup_messsage"] = "Server Error, Please Contact administrator !";
        //        this.ViewData["message_class"] = "failed_info";

                
        //    }

        //    this.TempData["topup_messsage"] = result
        //                                        ? "Topup successfully." + message
        //                                        : "ERROR :: " + message;
        //    this.TempData["message_class"] = result ? "success_info" : "failed_info";
        //    return RedirectToAction("NTTopUp", "UtilityPay");

        //}
        //end 01
        


        #endregion

        #region NCELL TopUp
        //GET NCELL TopUp
        [HttpGet]
        public ActionResult NCELLTopUp()
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

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpPost]
       // [ValidateAntiForgeryToken]
        public async Task<ActionResult> NCELLTopUp(TopUpPay topup)
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

                //end milayako
                ////validation
                //if (topup.MobileNumber.Length != 10)
                //{
                //    ModelState.AddModelError("MobileNumber", "Mobile Number should be 10 digits long.");
                //}
                //if(!topup.MobileNumber.Substring(0,3).IsInCollection(ApplicationInitilize.NcellStarters))
                //{
                //    ModelState.AddModelError("MobileNumber", "Invalid Ncell Mobile Number.");
                //}
                //if (topup.MobileNumber.Substring(0, 2) != "98")
                //{
                //    ModelState.AddModelError("MobileNumber", "Invalid Mobile Number.");
                //}
                //if (topup.Amount < 10 || topup.Amount > 5000)
                //{
                //    ModelState.AddModelError("Amount", "Amount is not valid.");
                //}
                //if (topup.Amount % 10 != 0)
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

                        var action = "topup.svc/payment";
                        var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                        var content = new FormUrlEncodedContent(new[]
                       {
                         new KeyValuePair<string, string>("sc",topup.TransactionMedium),
                         new KeyValuePair<string, string>("vid","10"),
                         new KeyValuePair<string, string>("mobile",mobile),

                         new KeyValuePair<string, string>("destmobile",topup.MobileNumber),

                         new KeyValuePair<string, string>("amount",topup.Amount.ToString()),
                         new KeyValuePair<string, string>("tid",tid),
                         new KeyValuePair<string, string>("pin",topup.Pin),
                         new KeyValuePair<string, string>("note","Successfully topup to" + topup.MobileNumber),
                         new KeyValuePair<string, string>("src","http"),
                         new KeyValuePair<string,string>("tokenID",Session["TokenID"].ToString())

                       });
                        _res = await client.PostAsync(new Uri(uri), content);

                string responseBody = _res.StatusCode.ToString() + " ," + await _res.Content.ReadAsStringAsync();
                _res.ReasonPhrase = responseBody;

                string  errorMessage = string.Empty;

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

                            //  dynamic json = JValue.Parse(responsetext);
                            // values require casting
                            // message = json.d;

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


                            //JsonParse msg = ser.Deserialize<JsonParse>(message);
                            //if (msg.StatusCode == "200")
                            //{
                            //    ResponseMessage myNames = ser.Deserialize<ResponseMessage>(msg.StatusMessage);

                            //    UtilityViewModel UVM = new UtilityViewModel();
                            //    string av = myNames.AvailableBalance;
                            //    string amttransfer = myNames.AmountTransferredBalance;
                            //    UVM.Method = "TOPUP";
                            //    UVM.Message = "NCELL TOPUP Successful ";//myNames.Message;
                            //    UVM.Amount = myNames.AmountTransferredBalance;
                            //    UVM.Balance = myNames.AvailableBalance;
                            //    UVM.Sender = userName;
                            //    UVM.Reciever = topup.MobileNumber;
                            //    UVM.Tid = tid;


                            //    return View("UtilityComplete", UVM);
                            //}
                            //else
                            //{
                            //    this.ViewData["topup_messsage"] = msg.StatusMessage;
                            //    this.ViewData["message_class"] = "failed_info";
                            //    return View(topup);
                            //}


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

                this.TempData["topup_messsage"] = result
                                        ? "Topup successfully." + message
                                        : "ERROR :: " + message;
                this.TempData["message_class"] = result ? "success_info" : "failed_info";
                return RedirectToAction("NCELLTopUp", "UtilityPay");

            }
        }
        #endregion

        #region ADSL TopUp
        //GET NCELL TopUp
        [HttpGet]
        public ActionResult ADSLTopUp()
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

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpPost]
       // [ValidateAntiForgeryToken]
        public async Task<ActionResult> ADSLTopUp(TopUpPay topup)
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

                //end milayako

                ////validation
                //if (topup.MobileNumber.Length != 9)
                //{
                //    ModelState.AddModelError("MobileNumber", "Landline Number should be 9 digits long.");
                //}
                //if (!topup.MobileNumber.Substring(0, 2).IsInCollection(ApplicationInitilize.ADSLStarters))
                //{
                //    ModelState.AddModelError("MobileNumber", "Invalid ADSL Number.");
                //}
                //if (topup.Amount < 10 || topup.Amount > 5000)
                //{
                //    ModelState.AddModelError("Amount", "Amount is not valid.");
                //}
                //if (topup.Amount % 10 != 0)
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

                        var action = "topup.svc/payment";
                        var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                        var content = new FormUrlEncodedContent(new[]
                       {
                         new KeyValuePair<string, string>("sc",topup.TransactionMedium),
                         new KeyValuePair<string, string>("vid","1"),
                         new KeyValuePair<string, string>("mobile",mobile),
                         new KeyValuePair<string, string>("selectADSL",topup.SelectADSL),

                         new KeyValuePair<string, string>("destmobile",topup.MobileNumber),

                         new KeyValuePair<string, string>("amount",topup.Amount.ToString()),
                         new KeyValuePair<string, string>("tid",tid),
                         new KeyValuePair<string, string>("pin",topup.Pin),
                         new KeyValuePair<string, string>("note","Successfully topup to" + topup.MobileNumber),
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
                            //message = _res.Content.ReadAsStringAsync().Result;
                            //dynamic json = JValue.Parse(responsetext);
                            // values require casting
                            // message = json.d;
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
                            //JsonParse msg = ser.Deserialize<JsonParse>(message);
                            //if (msg.StatusCode == "200")
                            //{
                            //    ResponseMessage myNames = ser.Deserialize<ResponseMessage>(msg.StatusMessage);

                            //    UtilityViewModel UVM = new UtilityViewModel();
                            //    string av = myNames.AvailableBalance;
                            //    string amttransfer = myNames.AmountTransferredBalance;
                            //    UVM.Method = "TOPUP";
                            //    UVM.Message = "ADSL TOPUP Successful ";//myNames.Message;
                            //    UVM.Amount = myNames.AmountTransferredBalance;
                            //    UVM.Balance = myNames.AvailableBalance;
                            //    UVM.Sender = userName;
                            //    UVM.Reciever = topup.MobileNumber;
                            //    UVM.Tid = tid;


                            //    return View("UtilityComplete", UVM);
                            //}
                            //else
                            //{
                            //    this.ViewData["topup_messsage"] = msg.StatusMessage;
                            //    this.ViewData["message_class"] = "failed_info";
                            //    return View(topup);
                            //}


                        }
                        else
                        {
                            result = false;
                            string StatusMessage = "";
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

                this.TempData["topup_messsage"] = result
                                        ? "Topup successfully." + message
                                        : "ERROR :: " + message;
                this.TempData["message_class"] = result ? "success_info" : "failed_info";
                return RedirectToAction("ADSLTopUp", "UtilityPay");

            }
        }
        #endregion
        
        #region NT Landline
        //GET NTLandline
        [HttpGet]
        public ActionResult NTLandline()
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

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]

        public async Task<ActionResult> NTLandline(TopUpPay topup)
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

                //end milayako

                ////validation
                //if (topup.MobileNumber.Length != 9)
                //{
                //    ModelState.AddModelError("MobileNumber", "Landline Number should be 9 digits long.");
                //}
                //if (!topup.MobileNumber.Substring(0, 2).IsInCollection(ApplicationInitilize.ADSLStarters))
                //{
                //    ModelState.AddModelError("MobileNumber", "Invalid ADSL Number.");
                //}
                //if (topup.Amount < 10 || topup.Amount > 5000)
                //{
                //    ModelState.AddModelError("Amount", "Amount is not valid.");
                //}
                //if (topup.Amount % 10 != 0)
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

                        var action = "topup.svc/payment";
                        var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                        var content = new FormUrlEncodedContent(new[]
                       {
                         new KeyValuePair<string, string>("sc",topup.TransactionMedium),
                         new KeyValuePair<string, string>("vid","7"),
                         new KeyValuePair<string, string>("mobile",mobile),

                         new KeyValuePair<string, string>("destmobile",topup.MobileNumber),

                         new KeyValuePair<string, string>("amount",topup.Amount.ToString()),
                         new KeyValuePair<string, string>("tid",tid),
                         new KeyValuePair<string, string>("pin",topup.Pin),
                         new KeyValuePair<string, string>("note","Successfully topup to" + topup.MobileNumber),
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

                this.TempData["topup_messsage"] = result
                                        ? "Topup successfully." + message
                                        : "ERROR :: " + message;
                this.TempData["message_class"] = result ? "success_info" : "failed_info";
                return RedirectToAction("NTLandline", "UtilityPay");
            }

        }

        #endregion

        //Recharge Card
        // GET: Recharge
        #region Recharge
        public ActionResult Recharge()
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

        #endregion

        #region BroadLink
        public ActionResult BroadLink()
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

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<ActionResult> BroadLink(TopUpPay topup)
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

                //validation

                if (topup.Amount < 10 || topup.Amount > 5000)
                {
                    ModelState.AddModelError("Amount", "Amount is not valid.");
                }
                if (topup.Amount % 10 != 0)
                {
                    ModelState.AddModelError("Amount", "Amount is not valid.");
                }
                if (ModelState.IsValid)
                {
                    //api call here
                    HttpResponseMessage _res = new HttpResponseMessage();
                    TraceIdGenerator _tig = new TraceIdGenerator();
                    var tid = _tig.GenerateTraceID();

                    string mobile = userName; //mobile is username
                    using (HttpClient client = new HttpClient())
                    {

                        var action = "coupon.svc/telco";
                        var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                        var content = new FormUrlEncodedContent(new[]
                       {
                         new KeyValuePair<string, string>("sc",topup.TransactionMedium),
                         new KeyValuePair<string, string>("vid","14"),
                         new KeyValuePair<string, string>("mobile",mobile),
                                                 
                         new KeyValuePair<string, string>("amount",topup.Amount.ToString()),
                         new KeyValuePair<string, string>("qty","1"),
                         new KeyValuePair<string, string>("tid",tid),
                         new KeyValuePair<string, string>("pin",topup.Pin),
                         new KeyValuePair<string, string>("note","BroadLink Successfully recharged"),
                         new KeyValuePair<string, string>("src","http"),
                        new KeyValuePair<string,string>("tokenID",Session["TokenID"].ToString())

                       });
                        _res = await client.PostAsync(new Uri(uri), content);

                        //string responseBody = _res.StatusCode.ToString() + " ," + await _res.Content.ReadAsStringAsync();
                        //  _res.ReasonPhrase = responseBody;

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
                            if (msg.StatusCode == "200")
                            {
                                ResponseMessage myNames = ser.Deserialize<ResponseMessage>(msg.StatusMessage);

                                UtilityViewModel UVM = new UtilityViewModel();
                                string av = myNames.AvailableBalance;
                                string amttransfer = myNames.AmountTransferredBalance;
                                UVM.Method = "RECHARGE";
                                UVM.Message = "Broadlink " + myNames.Message;
                                UVM.Amount = myNames.AmountTransferredBalance;
                                UVM.Balance = myNames.AvailableBalance;
                                UVM.Sender = userName;
                                UVM.Reciever = topup.MobileNumber;
                                UVM.Tid = tid;


                                return View("UtilityComplete", UVM);
                            }
                            else
                            {
                                this.ViewData["recharge_message"] = msg.StatusMessage;
                                this.ViewData["message_class"] = "failed_info";
                                return View(topup);
                            }


                        }
                        else
                        {
                            string StatusMessage = "";
                            responseCode = (int)_res.StatusCode;
                            responsetext = await _res.Content.ReadAsStringAsync();
                            if (responseCode == 404)
                            {
                                StatusMessage = "Could not connect to the server";
                            }
                            else
                            {
                                dynamic json = JValue.Parse(responsetext);
                                // values require casting
                                message = json.d;
                                result = false;
                                JavaScriptSerializer ser = new JavaScriptSerializer();
                                JsonParse msg = ser.Deserialize<JsonParse>(message);
                                StatusMessage = msg.StatusMessage;


                            }
                            this.ViewData["recharge_message"] = StatusMessage;
                            this.ViewData["message_class"] = "failed_info";
                            return View(topup);
                        }

                    }
                }
                else
                {
                    this.ViewData["recharge_message"] = "Validation Error, Please recheck and submit";

                    this.ViewData["message_class"] = "failed_info";
                    return View(topup);
                }

            }
            catch (Exception ex)
            {
                result = false;
                errorMessage = ex.Message;
                this.ViewData["recharge_message"] = "Server Error, Please Contact administrator !";
                this.ViewData["message_class"] = "failed_info";

                return View(topup);
            }

        }

        #endregion

        #region Dish Home
        public ActionResult DishHome()
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

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<ActionResult> DishHome(TopUpPay topup)
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

                //validation

                if (topup.Amount < 10 || topup.Amount > 5000)
                {
                    ModelState.AddModelError("Amount", "Amount is not valid.");
                }
                if (topup.Amount % 10 != 0)
                {
                    ModelState.AddModelError("Amount", "Amount is not valid.");
                }
                if (ModelState.IsValid)
                {
                    //api call here
                    HttpResponseMessage _res = new HttpResponseMessage();
                    TraceIdGenerator _tig = new TraceIdGenerator();
                    var tid = _tig.GenerateTraceID();

                    string mobile = userName; //mobile is username
                    using (HttpClient client = new HttpClient())
                    {

                        var action = "coupon.svc/telco";
                        var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                        var content = new FormUrlEncodedContent(new[]
                       {
                         new KeyValuePair<string, string>("sc",topup.TransactionMedium),
                         new KeyValuePair<string, string>("vid","13"),
                         new KeyValuePair<string, string>("mobile",mobile),

                         new KeyValuePair<string, string>("amount",topup.Amount.ToString()),
                         new KeyValuePair<string, string>("qty","1"),
                         new KeyValuePair<string, string>("tid",tid),
                         new KeyValuePair<string, string>("pin",topup.Pin),
                         new KeyValuePair<string, string>("note","DishHome Successfully recharged"),
                         new KeyValuePair<string, string>("src","http"),
                        new KeyValuePair<string,string>("tokenID",Session["TokenID"].ToString())

                       });
                        _res = await client.PostAsync(new Uri(uri), content);

                        //string responseBody = _res.StatusCode.ToString() + " ," + await _res.Content.ReadAsStringAsync();
                        //  _res.ReasonPhrase = responseBody;

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
                            if (msg.StatusCode == "200")
                            {
                                ResponseMessage myNames = ser.Deserialize<ResponseMessage>(msg.StatusMessage);

                                UtilityViewModel UVM = new UtilityViewModel();
                                string av = myNames.AvailableBalance;
                                string amttransfer = myNames.AmountTransferredBalance;
                                UVM.Method = "RECHARGE";
                                UVM.Message = "DishHome " + myNames.Message;//"DishHome Recharge Successful "
                                UVM.Amount = myNames.AmountTransferredBalance;
                                UVM.Balance = myNames.AvailableBalance;
                                UVM.Sender = userName;
                                UVM.Reciever = topup.MobileNumber;
                                UVM.Tid = tid;


                                return View("UtilityComplete", UVM);
                            }
                            else
                            {
                                this.ViewData["recharge_message"] = msg.StatusMessage;
                                this.ViewData["message_class"] = "failed_info";
                                return View(topup);
                            }


                        }
                        else
                        {
                            string StatusMessage = "";
                            responseCode = (int)_res.StatusCode;
                            responsetext = await _res.Content.ReadAsStringAsync();
                            if (responseCode == 404)
                            {
                                StatusMessage = "Could not connect to the server";
                            }
                            else
                            {
                                dynamic json = JValue.Parse(responsetext);
                                // values require casting
                                message = json.d;
                                result = false;
                                JavaScriptSerializer ser = new JavaScriptSerializer();
                                JsonParse msg = ser.Deserialize<JsonParse>(message);
                                StatusMessage = msg.StatusMessage;


                            }
                            this.ViewData["recharge_message"] = StatusMessage;
                            this.ViewData["message_class"] = "failed_info";
                            return View(topup);
                        }

                    }
                }
                else
                {
                    this.ViewData["recharge_message"] = "Validation Error, Please recheck and submit";

                    this.ViewData["message_class"] = "failed_info";
                    return View(topup);
                }

            }
            catch (Exception ex)
            {
                result = false;
                errorMessage = ex.Message;
                this.ViewData["recharge_message"] = "Server Error, Please Contact administrator !";
                this.ViewData["message_class"] = "failed_info";

                return View(topup);
            }

        }


        #endregion

        #region NTC CDMA
        public ActionResult NTCCDMA()
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

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<ActionResult> NTCCDMA(TopUpPay topup)
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

                //validation

                if (topup.Amount < 10 || topup.Amount > 5000)
                {
                    ModelState.AddModelError("Amount", "Amount is not valid.");
                }
                if (topup.Amount % 10 != 0)
                {
                    ModelState.AddModelError("Amount", "Amount is not valid.");
                }
                if (ModelState.IsValid)
                {
                    //api call here
                    HttpResponseMessage _res = new HttpResponseMessage();
                    TraceIdGenerator _tig = new TraceIdGenerator();
                    var tid = _tig.GenerateTraceID();

                    string mobile = userName; //mobile is username
                    using (HttpClient client = new HttpClient())
                    {

                        var action = "coupon.svc/telco";
                        var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                        var content = new FormUrlEncodedContent(new[]
                       {
                         new KeyValuePair<string, string>("sc",topup.TransactionMedium),
                         new KeyValuePair<string, string>("vid","12"),
                         new KeyValuePair<string, string>("mobile",mobile),

                         new KeyValuePair<string, string>("amount",topup.Amount.ToString()),
                         new KeyValuePair<string, string>("qty","1"),
                         new KeyValuePair<string, string>("tid",tid),
                         new KeyValuePair<string, string>("pin",topup.Pin),
                         new KeyValuePair<string, string>("note","NTC CDMA Successfully recharged"),
                         new KeyValuePair<string, string>("src","http"),
                        new KeyValuePair<string,string>("tokenID",Session["TokenID"].ToString())

                       });
                        _res = await client.PostAsync(new Uri(uri), content);

                        //string responseBody = _res.StatusCode.ToString() + " ," + await _res.Content.ReadAsStringAsync();
                        //  _res.ReasonPhrase = responseBody;

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
                            if (msg.StatusCode == "200")
                            {
                                ResponseMessage myNames = ser.Deserialize<ResponseMessage>(msg.StatusMessage);

                                UtilityViewModel UVM = new UtilityViewModel();
                                string av = myNames.AvailableBalance;
                                string amttransfer = myNames.AmountTransferredBalance;
                                UVM.Method = "RECHARGE";
                                UVM.Message = "NTC CDMA " + myNames.Message;
                                UVM.Amount = myNames.AmountTransferredBalance;
                                UVM.Balance = myNames.AvailableBalance;
                                UVM.Sender = userName;
                                UVM.Reciever = topup.MobileNumber;
                                UVM.Tid = tid;
                               

                                return View("UtilityComplete", UVM);
                            }
                            else
                            {
                                this.ViewData["recharge_message"] = msg.StatusMessage;
                                this.ViewData["message_class"] = "failed_info";
                                return View(topup);
                            }


                        }
                        else
                        {
                            string StatusMessage = "";
                            responseCode = (int)_res.StatusCode;
                            responsetext = await _res.Content.ReadAsStringAsync();
                            if (responseCode == 404)
                            {
                                StatusMessage = "Could not connect to the server";
                            }
                            else
                            {
                                dynamic json = JValue.Parse(responsetext);
                                // values require casting
                                message = json.d;
                                result = false;
                                JavaScriptSerializer ser = new JavaScriptSerializer();
                                JsonParse msg = ser.Deserialize<JsonParse>(message);
                                StatusMessage = msg.StatusMessage;


                            }
                            this.ViewData["recharge_message"] = StatusMessage;
                            this.ViewData["message_class"] = "failed_info";
                            return View(topup);
                        }

                    }
                }
                else
                {
                    this.ViewData["recharge_message"] = "Validation Error, Please recheck and submit";

                    this.ViewData["message_class"] = "failed_info";
                    return View(topup);
                }

            }
            catch (Exception ex)
            {
                result = false;
                errorMessage = ex.Message;
                this.ViewData["recharge_message"] = "Server Error, Please Contact administrator !";
                this.ViewData["message_class"] = "failed_info";

                return View(topup);
            }

        }
        #endregion

        #region NTC GSM
        public ActionResult NTCGSM()
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

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<ActionResult> NTCGSM(TopUpPay topup)
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

                //validation

                if (topup.Amount < 10 || topup.Amount > 5000)
                {
                    ModelState.AddModelError("Amount", "Amount is not valid.");
                }
                if (topup.Amount % 10 != 0)
                {
                    ModelState.AddModelError("Amount", "Amount is not valid.");
                }
                if (ModelState.IsValid)
                {
                    //api call here
                    HttpResponseMessage _res = new HttpResponseMessage();
                    TraceIdGenerator _tig = new TraceIdGenerator();
                    var tid = _tig.GenerateTraceID();

                    string mobile = userName; //mobile is username
                    using (HttpClient client = new HttpClient())
                    {

                        var action = "coupon.svc/telco";
                        var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                        var content = new FormUrlEncodedContent(new[]
                       {
                         new KeyValuePair<string, string>("sc",topup.TransactionMedium),
                         new KeyValuePair<string, string>("vid","11"),
                         new KeyValuePair<string, string>("mobile",mobile),

                         new KeyValuePair<string, string>("amount",topup.Amount.ToString()),
                         new KeyValuePair<string, string>("qty","1"),
                         new KeyValuePair<string, string>("tid",tid),
                         new KeyValuePair<string, string>("pin",topup.Pin),
                         new KeyValuePair<string, string>("note","NTC GSM Successfully recharged"),
                         new KeyValuePair<string, string>("src","http"),
                         new KeyValuePair<string,string>("tokenID",Session["TokenID"].ToString())

                       });
                        _res = await client.PostAsync(new Uri(uri), content);

                        //string responseBody = _res.StatusCode.ToString() + " ," + await _res.Content.ReadAsStringAsync();
                        //  _res.ReasonPhrase = responseBody;

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
                            if (msg.StatusCode == "200")
                            {
                                ResponseMessage myNames = ser.Deserialize<ResponseMessage>(msg.StatusMessage);

                                UtilityViewModel UVM = new UtilityViewModel();
                                string av = myNames.AvailableBalance;
                                string amttransfer = myNames.AmountTransferredBalance;
                                UVM.Method = "RECHARGE";
                                UVM.Message = "NTC GSM " + myNames.Message;
                                UVM.Amount = myNames.AmountTransferredBalance;
                                UVM.Balance = myNames.AvailableBalance;
                                UVM.Sender = userName;
                                UVM.Reciever = topup.MobileNumber;
                                UVM.Tid = tid;


                                return View("UtilityComplete", UVM);
                            }
                            else
                            {
                                this.ViewData["recharge_message"] = msg.StatusMessage;
                                this.ViewData["message_class"] = "failed_info";
                                return View(topup);
                            }


                        }
                        else
                        {
                            string StatusMessage = "";
                            responseCode = (int)_res.StatusCode;
                            responsetext = await _res.Content.ReadAsStringAsync();
                            if (responseCode == 404)
                            {
                                StatusMessage = "Could not connect to the server";
                            }
                            else
                            {
                                dynamic json = JValue.Parse(responsetext);
                                // values require casting
                                message = json.d;
                                result = false;
                                JavaScriptSerializer ser = new JavaScriptSerializer();
                                JsonParse msg = ser.Deserialize<JsonParse>(message);
                                StatusMessage = msg.StatusMessage;


                            }
                            this.ViewData["recharge_message"] = StatusMessage;
                            this.ViewData["message_class"] = "failed_info";
                            return View(topup);
                        }

                    }
                }
                else
                {
                    this.ViewData["recharge_message"] = "Validation Error, Please recheck and submit";

                    this.ViewData["message_class"] = "failed_info";
                    return View(topup);
                }

            }
            catch (Exception ex)
            {
                result = false;
                errorMessage = ex.Message;
                this.ViewData["recharge_message"] = "Server Error, Please Contact administrator !";
                this.ViewData["message_class"] = "failed_info";

                return View(topup);
            }

        }


        #endregion

        //start milyako

        #region NewRecharge
        public ActionResult NewRecharge()
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

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpPost]
        public async Task<ActionResult> NewRecharge(TopUpPay topup)
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


            //api call here
            HttpResponseMessage _res = new HttpResponseMessage();
            TraceIdGenerator _tig = new TraceIdGenerator();
            var tid = _tig.GenerateTraceID();

            string mobile = userName; //mobile is username
            using (HttpClient client = new HttpClient())
            {

                var action = "coupon.svc/telco";
                var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                var content = new FormUrlEncodedContent(new[]
               {
                         new KeyValuePair<string, string>("sc",topup.TransactionMedium),
                         new KeyValuePair<string, string>("vid",topup.SelectRecharge),
                         new KeyValuePair<string, string>("mobile",mobile),

                         new KeyValuePair<string, string>("amount",topup.Amount.ToString()),
                         new KeyValuePair<string, string>("qty","1"),
                         new KeyValuePair<string, string>("tid",tid),
                         new KeyValuePair<string, string>("pin",topup.Pin),
                         new KeyValuePair<string, string>("note","BroadLink Successfully recharged"),
                         new KeyValuePair<string, string>("src","http"),
                        new KeyValuePair<string,string>("tokenID",Session["TokenID"].ToString())

                       });
                _res = await client.PostAsync(new Uri(uri), content);

                string responseBody = _res.StatusCode.ToString() + " ," + await _res.Content.ReadAsStringAsync();
                //string responseBody = _res.StatusCode.ToString() + " ," + _res.Content.ReadAsStringAsync();
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
                        int rechargePin = 0;
                        //message = _res.Content.ReadAsStringAsync().Result;
                        // dynamic json = JValue.Parse(responsetext);
                        // values require casting
                        //  message = json.d;

                        message = _res.Content.ReadAsStringAsync().Result;
                        string respmsg = "";
                        string couponNo = "";

                        if (!string.IsNullOrEmpty(message))
                        {

                            JavaScriptSerializer ser = new JavaScriptSerializer();

                            var jsonresp = ser.Deserialize<JsonParse>(responsetext);
                            message = jsonresp.d;

                            JsonParse myNames = ser.Deserialize<JsonParse>(jsonresp.d);
                            int code = Convert.ToInt32(myNames.StatusCode);
                            respmsg = myNames.StatusMessage;
                            if (code == 200)
                            {
                                var details = JObject.Parse(respmsg);
                                couponNo = details["couponnumber"].ToString();
                                ViewBag.couponNo = couponNo;
                            }
                            if (code != responseCode)
                            {
                                responseCode = code;
                            }
                        }
                        return Json(new { responseCode = responseCode, responseText = respmsg, rechargePin = couponNo },
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
                return RedirectToAction("NewRecharge", "UtilityPay");

            }
        }

        #endregion


        //end milayako

        ///start milayako
        #region Electricity
        public ActionResult Electricity()
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

        [HttpPost]
        public async Task<ActionResult> Electricity(TopUpPay topup)
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


            //api call here
            HttpResponseMessage _res = new HttpResponseMessage();
            TraceIdGenerator _tig = new TraceIdGenerator();
            var tid = _tig.GenerateTraceID();

            string mobile = userName; //mobile is username
            using (HttpClient client = new HttpClient())
            {

                var action = "coupon.svc/telco";
                var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                var content = new FormUrlEncodedContent(new[]
               {
                         new KeyValuePair<string, string>("sc",topup.TransactionMedium),
                         new KeyValuePair<string, string>("vid",topup.SelectRecharge),
                         new KeyValuePair<string, string>("mobile",mobile),

                         new KeyValuePair<string, string>("amount",topup.Amount.ToString()),
                         new KeyValuePair<string, string>("qty","1"),
                         new KeyValuePair<string, string>("tid",tid),
                         new KeyValuePair<string, string>("pin",topup.Pin),
                         new KeyValuePair<string, string>("note","BroadLink Successfully recharged"),
                         new KeyValuePair<string, string>("src","http")

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
                return RedirectToAction("Electricity", "UtilityPay");

            }
        }

        #endregion

        #region Khanepani
        public ActionResult Khanepani()
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

        [HttpPost]
        public async Task<ActionResult> Khanepani(TopUpPay topup)
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


            //api call here
            HttpResponseMessage _res = new HttpResponseMessage();
            TraceIdGenerator _tig = new TraceIdGenerator();
            var tid = _tig.GenerateTraceID();

            string mobile = userName; //mobile is username
            using (HttpClient client = new HttpClient())
            {

                var action = "coupon.svc/telco";
                var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                var content = new FormUrlEncodedContent(new[]
               {
                         new KeyValuePair<string, string>("sc",topup.TransactionMedium),
                         new KeyValuePair<string, string>("vid",topup.SelectRecharge),
                         new KeyValuePair<string, string>("mobile",mobile),

                         new KeyValuePair<string, string>("amount",topup.Amount.ToString()),
                         new KeyValuePair<string, string>("qty","1"),
                         new KeyValuePair<string, string>("tid",tid),
                         new KeyValuePair<string, string>("pin",topup.Pin),
                         new KeyValuePair<string, string>("note","BroadLink Successfully recharged"),
                         new KeyValuePair<string, string>("src","http")

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
                return RedirectToAction("Khanepani", "UtilityPay");

            }
        }

        #endregion
        //end milayako

    }
}