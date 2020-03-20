using MNepalWeb.App_Start;
using MNepalWeb.Helper;
using MNepalWeb.Models;
using MNepalWeb.Utilities;
using MNepalWeb.ViewModel;
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

namespace MNepalWeb.Controllers
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

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        #region NT TopUp
        //GET NT TopUp
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

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        //test//
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> NTTopUp (TopUpPay topup)
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
                //validation
                if (topup.MobileNumber.Length != 10)
                {
                    ModelState.AddModelError("MobileNumber", "Mobile Number should be 10 digits long.");
                }
                if (!topup.MobileNumber.Substring(0, 3).IsInCollection(ApplicationInitilize.NTCStarters))
                {
                    ModelState.AddModelError("MobileNumber", "Invalid NTC Number.");
                }
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
                       
                        var action = "topup/payment";
                        var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                        var content = new FormUrlEncodedContent(new[]
                       {
                         new KeyValuePair<string, string>("sc",topup.TransactionMedium),
                         new KeyValuePair<string, string>("vid","2"),
                         new KeyValuePair<string, string>("mobile",mobile),

                         new KeyValuePair<string, string>("destmobile",topup.MobileNumber),
                        
                         new KeyValuePair<string, string>("amount",topup.Amount.ToString()),
                         new KeyValuePair<string, string>("tid",tid),
                         new KeyValuePair<string, string>("pin",topup.PIN),
                         new KeyValuePair<string, string>("note","Successfully topup to" + topup.MobileNumber),
                         new KeyValuePair<string, string>("src","http")

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
                                UVM.Method = "TOPUP";
                                UVM.Message = "NT Postpaid TOPUP Successful ";//myNames.Message;
                                UVM.Amount = myNames.AmountTransferredBalance;
                                UVM.Balance= myNames.AvailableBalance;
                                UVM.Sender = userName;
                                UVM.Reciever = topup.MobileNumber;
                                UVM.Tid = tid;


                                return View("UtilityComplete", UVM);
                            }
                            else
                            {
                                this.ViewData["topup_messsage"] = msg.StatusMessage;
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
                            this.ViewData["topup_messsage"] = StatusMessage;
                            this.ViewData["message_class"] = "failed_info";
                            return View(topup);
                        }

                    }
                }
                else
                {
                    this.ViewData["topup_messsage"] = "Validation Error, Please recheck and submit";
                                                          
                    this.ViewData["message_class"] = "failed_info";
                    return View(topup);
                }

            }
            catch (Exception ex)
            {
                result = false;
                errorMessage = ex.Message;
                this.ViewData["topup_messsage"] = "Server Error, Please Contact administrator !";
                this.ViewData["message_class"] = "failed_info";

                return View(topup);
            }

        }
        //test//


        #endregion

       #region NCELL TopUp
        //GET NCELL TopUp
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

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> NCELLTopUp(TopUpPay topup)
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
                //validation
                if (topup.MobileNumber.Length != 10)
                {
                    ModelState.AddModelError("MobileNumber", "Mobile Number should be 10 digits long.");
                }
                if(!topup.MobileNumber.Substring(0,3).IsInCollection(ApplicationInitilize.NcellStarters))
                {
                    ModelState.AddModelError("MobileNumber", "Invalid Ncell Mobile Number.");
                }
                if (topup.MobileNumber.Substring(0, 2) != "98")
                {
                    ModelState.AddModelError("MobileNumber", "Invalid Mobile Number.");
                }
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

                        var action = "topup/payment";
                        var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                        var content = new FormUrlEncodedContent(new[]
                       {
                         new KeyValuePair<string, string>("sc",topup.TransactionMedium),
                         new KeyValuePair<string, string>("vid","10"),
                         new KeyValuePair<string, string>("mobile",mobile),

                         new KeyValuePair<string, string>("destmobile",topup.MobileNumber),

                         new KeyValuePair<string, string>("amount",topup.Amount.ToString()),
                         new KeyValuePair<string, string>("tid",tid),
                         new KeyValuePair<string, string>("pin",topup.PIN),
                         new KeyValuePair<string, string>("note","Successfully topup to" + topup.MobileNumber),
                         new KeyValuePair<string, string>("src","http")

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
                                UVM.Method = "TOPUP";
                                UVM.Message = "NCELL TOPUP Successful ";//myNames.Message;
                                UVM.Amount = myNames.AmountTransferredBalance;
                                UVM.Balance = myNames.AvailableBalance;
                                UVM.Sender = userName;
                                UVM.Reciever = topup.MobileNumber;
                                UVM.Tid = tid;


                                return View("UtilityComplete", UVM);
                            }
                            else
                            {
                                this.ViewData["topup_messsage"] = msg.StatusMessage;
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
                            this.ViewData["topup_messsage"] = StatusMessage;
                            this.ViewData["message_class"] = "failed_info";
                            return View(topup);
                        }

                    }
                }
                else
                {
                    this.ViewData["topup_messsage"] = "Validation Error, Please recheck and submit";

                    this.ViewData["message_class"] = "failed_info";
                    return View(topup);
                }

            }
            catch (Exception ex)
            {
                result = false;
                errorMessage = ex.Message;
                this.ViewData["topup_messsage"] = "Server Error, Please Contact administrator !";
                this.ViewData["message_class"] = "failed_info";

                return View(topup);
            }

        }

        #endregion

        #region ADSL TopUp
        //GET NCELL TopUp
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

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ADSLTopUp(TopUpPay topup)
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
                //validation
                if (topup.MobileNumber.Length != 9)
                {
                    ModelState.AddModelError("MobileNumber", "Landline Number should be 9 digits long.");
                }
                if (!topup.MobileNumber.Substring(0, 2).IsInCollection(ApplicationInitilize.ADSLStarters))
                {
                    ModelState.AddModelError("MobileNumber", "Invalid ADSL Number.");
                }
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

                        var action = "topup/payment";
                        var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                        var content = new FormUrlEncodedContent(new[]
                       {
                         new KeyValuePair<string, string>("sc",topup.TransactionMedium),
                         new KeyValuePair<string, string>("vid","1"),
                         new KeyValuePair<string, string>("mobile",mobile),

                         new KeyValuePair<string, string>("destmobile",topup.MobileNumber),

                         new KeyValuePair<string, string>("amount",topup.Amount.ToString()),
                         new KeyValuePair<string, string>("tid",tid),
                         new KeyValuePair<string, string>("pin",topup.PIN),
                         new KeyValuePair<string, string>("note","Successfully topup to" + topup.MobileNumber),
                         new KeyValuePair<string, string>("src","http")

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
                                UVM.Method = "TOPUP";
                                UVM.Message = "ADSL TOPUP Successful ";//myNames.Message;
                                UVM.Amount = myNames.AmountTransferredBalance;
                                UVM.Balance = myNames.AvailableBalance;
                                UVM.Sender = userName;
                                UVM.Reciever = topup.MobileNumber;
                                UVM.Tid = tid;


                                return View("UtilityComplete", UVM);
                            }
                            else
                            {
                                this.ViewData["topup_messsage"] = msg.StatusMessage;
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
                            this.ViewData["topup_messsage"] = StatusMessage;
                            this.ViewData["message_class"] = "failed_info";
                            return View(topup);
                        }

                    }
                }
                else
                {
                    this.ViewData["topup_messsage"] = "Validation Error, Please recheck and submit";

                    this.ViewData["message_class"] = "failed_info";
                    return View(topup);
                }

            }
            catch (Exception ex)
            {
                result = false;
                errorMessage = ex.Message;
                this.ViewData["topup_messsage"] = "Server Error, Please Contact administrator !";
                this.ViewData["message_class"] = "failed_info";

                return View(topup);
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

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<ActionResult> NTLandline(TopUpPay topup)
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
                //validation
                if (topup.MobileNumber.Length != 9)
                {
                    ModelState.AddModelError("MobileNumber", "Landline Number should be 9 digits long.");
                }
                if (!topup.MobileNumber.Substring(0, 2).IsInCollection(ApplicationInitilize.ADSLStarters))
                {
                    ModelState.AddModelError("MobileNumber", "Invalid ADSL Number.");
                }
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

                        var action = "topup/payment";
                        var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                        var content = new FormUrlEncodedContent(new[]
                       {
                         new KeyValuePair<string, string>("sc",topup.TransactionMedium),
                         new KeyValuePair<string, string>("vid","7"),
                         new KeyValuePair<string, string>("mobile",mobile),

                         new KeyValuePair<string, string>("destmobile",topup.MobileNumber),

                         new KeyValuePair<string, string>("amount",topup.Amount.ToString()),
                         new KeyValuePair<string, string>("tid",tid),
                         new KeyValuePair<string, string>("pin",topup.PIN),
                         new KeyValuePair<string, string>("note","Successfully topup to" + topup.MobileNumber),
                         new KeyValuePair<string, string>("src","http")

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
                                UVM.Method = "TOPUP";
                                UVM.Message = "Landline TOPUP Successful ";//myNames.Message;
                                UVM.Amount = myNames.AmountTransferredBalance;
                                UVM.Balance = myNames.AvailableBalance;
                                UVM.Sender = userName;
                                UVM.Reciever = topup.MobileNumber;
                                UVM.Tid = tid;


                                return View("UtilityComplete", UVM);
                            }
                            else
                            {
                                this.ViewData["topup_messsage"] = msg.StatusMessage;
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
                            this.ViewData["topup_messsage"] = StatusMessage;
                            this.ViewData["message_class"] = "failed_info";
                            return View(topup);
                        }

                    }
                }
                else
                {
                    this.ViewData["topup_messsage"] = "Validation Error, Please recheck and submit";

                    this.ViewData["message_class"] = "failed_info";
                    return View(topup);
                }

            }
            catch (Exception ex)
            {
                result = false;
                errorMessage = ex.Message;
                this.ViewData["topup_messsage"] = "Server Error, Please Contact administrator !";
                this.ViewData["message_class"] = "failed_info";

                return View(topup);
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

                        var action = "coupon/telco";
                        var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                        var content = new FormUrlEncodedContent(new[]
                       {
                         new KeyValuePair<string, string>("sc",topup.TransactionMedium),
                         new KeyValuePair<string, string>("vid","14"),
                         new KeyValuePair<string, string>("mobile",mobile),
                                                 
                         new KeyValuePair<string, string>("amount",topup.Amount.ToString()),
                         new KeyValuePair<string, string>("qty","1"),
                         new KeyValuePair<string, string>("tid",tid),
                         new KeyValuePair<string, string>("pin",topup.PIN),
                         new KeyValuePair<string, string>("note","BroadLink Successfully recharged"),
                         new KeyValuePair<string, string>("src","http")

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

                        var action = "coupon/telco";
                        var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                        var content = new FormUrlEncodedContent(new[]
                       {
                         new KeyValuePair<string, string>("sc",topup.TransactionMedium),
                         new KeyValuePair<string, string>("vid","13"),
                         new KeyValuePair<string, string>("mobile",mobile),

                         new KeyValuePair<string, string>("amount",topup.Amount.ToString()),
                         new KeyValuePair<string, string>("qty","1"),
                         new KeyValuePair<string, string>("tid",tid),
                         new KeyValuePair<string, string>("pin",topup.PIN),
                         new KeyValuePair<string, string>("note","DishHome Successfully recharged"),
                         new KeyValuePair<string, string>("src","http")

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

                        var action = "coupon/telco";
                        var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                        var content = new FormUrlEncodedContent(new[]
                       {
                         new KeyValuePair<string, string>("sc",topup.TransactionMedium),
                         new KeyValuePair<string, string>("vid","12"),
                         new KeyValuePair<string, string>("mobile",mobile),

                         new KeyValuePair<string, string>("amount",topup.Amount.ToString()),
                         new KeyValuePair<string, string>("qty","1"),
                         new KeyValuePair<string, string>("tid",tid),
                         new KeyValuePair<string, string>("pin",topup.PIN),
                         new KeyValuePair<string, string>("note","NTC CDMA Successfully recharged"),
                         new KeyValuePair<string, string>("src","http")

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

                        var action = "coupon/telco";
                        var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                        var content = new FormUrlEncodedContent(new[]
                       {
                         new KeyValuePair<string, string>("sc",topup.TransactionMedium),
                         new KeyValuePair<string, string>("vid","11"),
                         new KeyValuePair<string, string>("mobile",mobile),

                         new KeyValuePair<string, string>("amount",topup.Amount.ToString()),
                         new KeyValuePair<string, string>("qty","1"),
                         new KeyValuePair<string, string>("tid",tid),
                         new KeyValuePair<string, string>("pin",topup.PIN),
                         new KeyValuePair<string, string>("note","NTC GSM Successfully recharged"),
                         new KeyValuePair<string, string>("src","http")

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



    }
}