using MNepalWeb.App_Start;
using MNepalWeb.Helper;
using MNepalWeb.Models;
using MNepalWeb.UserModels;
using MNepalWeb.ViewModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace MNepalWeb.Controllers
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
                if (merchantcollege.Amount < 10 || merchantcollege.Amount > 9999)
                {
                    ModelState.AddModelError("Amount", "Amount is not valid.");
                }
                //if (merchantcollege.Amount % 10 != 0)
                //{
                //    ModelState.AddModelError("Amount", "Amount is not valid.");
                //}

                if (ModelState.IsValid)
                {
                    //api call here
                    HttpResponseMessage _res = new HttpResponseMessage();
                    TraceIdGenerator _tig = new TraceIdGenerator();
                    var tid = _tig.GenerateTraceID();

                    string mobile = userName; //mobile is username
                    using (HttpClient client = new HttpClient())
                    {

                        var action = "merchant/payment";
                        var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                        var content = new FormUrlEncodedContent(new[]
                       {
                         new KeyValuePair<string, string>("sc",merchantcollege.TransactionMedium),
                         new KeyValuePair<string, string>("vid",merchantcollege.CollegeName),
                         new KeyValuePair<string, string>("mobile",mobile),

                         new KeyValuePair<string, string>("sa",""),
                         new KeyValuePair<string, string>("prod","1"),
                         new KeyValuePair<string, string>("amount",merchantcollege.Amount.ToString()),
                         new KeyValuePair<string, string>("tid",tid),
                         new KeyValuePair<string, string>("pin",merchantcollege.TPin),
                         new KeyValuePair<string, string>("note","College Payment successful"),
                         new KeyValuePair<string, string>("src","http")

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
                            if (msg.StatusCode == "200")
                            {
                                ResponseMessage myNames = ser.Deserialize<ResponseMessage>(msg.StatusMessage);

                                MerchantPaymentVM UVM = new MerchantPaymentVM();
                                string av = myNames.AvailableBalance;
                                string amttransfer = myNames.AmountTransferredBalance;
                                UVM.Method = "200";
                                UVM.Message = "College Payment Successful of Amount NRs." + merchantcollege.Amount.ToString(); //myNames.Message;
                                UVM.Amount = myNames.AmountTransferredBalance;
                                UVM.Balance = myNames.AvailableBalance;
                                UVM.Sender = userName;
                                UVM.MName = merchantcollege.CollegeName;
                                UVM.Tid = tid;


                                return View("MerchantPaymentComplete", UVM);
                            }
                            else
                            {
                                this.ViewData["merchantpay_messsage"] = msg.StatusMessage;
                                this.ViewData["message_class"] = "failed_info";
                                return View(merchantcollege);
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
                            this.ViewData["merchantpay_messsage"] = StatusMessage;
                            this.ViewData["message_class"] = "failed_info";
                            return View(merchantcollege);
                        }

                    }
                }
                else
                {
                    this.ViewData["merchantpay_messsage"] = "Validation Error, Please recheck and submit";

                    this.ViewData["message_class"] = "failed_info";
                    return View(merchantcollege);
                }

            }
            catch (Exception ex)
            {
                result = false;
                errorMessage = ex.Message;
                this.ViewData["merchantpay_messsage"] = "Server Error, Please Contact administrator !";
                this.ViewData["message_class"] = "failed_info";

                return View(merchantcollege);
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

                ReportUserModel rep = new ReportUserModel();
                ViewBag.Restaurant = rep.GetMerchantListbyCategory("5").Select(x =>
                                  new SelectListItem()
                                  {
                                      Text = x.MName,
                                      Value = x.MId
                                  });

                //validation
                if (merchantrest.TPin.Length != 4)
                {
                    ModelState.AddModelError("TPin", "T-Pin should be 4 digits long.");
                }
                if (merchantrest.Amount < 10 || merchantrest.Amount > 9999)
                {
                    ModelState.AddModelError("Amount", "Amount is not valid.");
                }
                //if (merchantcollege.Amount % 10 != 0)
                //{
                //    ModelState.AddModelError("Amount", "Amount is not valid.");
                //}

                if (ModelState.IsValid)
                {
                    //api call here
                    HttpResponseMessage _res = new HttpResponseMessage();
                    TraceIdGenerator _tig = new TraceIdGenerator();
                    var tid = _tig.GenerateTraceID();

                    string mobile = userName; //mobile is username
                    using (HttpClient client = new HttpClient())
                    {

                        var action = "merchant/payment";
                        var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                        var content = new FormUrlEncodedContent(new[]
                       {
                         new KeyValuePair<string, string>("sc",merchantrest.TransactionMedium),
                         new KeyValuePair<string, string>("vid",merchantrest.RestaurantName),
                         new KeyValuePair<string, string>("mobile",mobile),

                         new KeyValuePair<string, string>("sa",""),
                         new KeyValuePair<string, string>("prod","1"),
                         new KeyValuePair<string, string>("amount",merchantrest.Amount.ToString()),
                         new KeyValuePair<string, string>("tid",tid),
                         new KeyValuePair<string, string>("pin",merchantrest.TPin),
                         new KeyValuePair<string, string>("note","Restaurant Payment successful"),
                         new KeyValuePair<string, string>("src","http")

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
                            if (msg.StatusCode == "200")
                            {
                                ResponseMessage myNames = ser.Deserialize<ResponseMessage>(msg.StatusMessage);

                                MerchantPaymentVM UVM = new MerchantPaymentVM();
                                string av = myNames.AvailableBalance;
                                string amttransfer = myNames.AmountTransferredBalance;
                                UVM.Method = "200";
                                UVM.Message = "Restaurant Payment Successful of Amount NRs." + merchantrest.Amount.ToString(); //myNames.Message;
                                UVM.Amount = myNames.AmountTransferredBalance;
                                UVM.Balance = myNames.AvailableBalance;
                                UVM.Sender = userName;
                                UVM.MName = merchantrest.RestaurantName;
                                UVM.Tid = tid;


                                return View("MerchantPaymentComplete", UVM);
                            }
                            else
                            {
                                this.ViewData["merchantpay_messsage"] = msg.StatusMessage;
                                this.ViewData["message_class"] = "failed_info";
                                return View(merchantrest);
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
                            this.ViewData["merchantpay_messsage"] = StatusMessage;
                            this.ViewData["message_class"] = "failed_info";
                            return View(merchantrest);
                        }

                    }
                }
                else
                {
                    this.ViewData["merchantpay_messsage"] = "Validation Error, Please recheck and submit";

                    this.ViewData["message_class"] = "failed_info";
                    return View(merchantrest);
                }

            }
            catch (Exception ex)
            {
                result = false;
                errorMessage = ex.Message;
                this.ViewData["merchantpay_messsage"] = "Server Error, Please Contact administrator !";
                this.ViewData["message_class"] = "failed_info";

                return View(merchantrest);
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
                if (merchantschool.Amount < 10 || merchantschool.Amount > 9999)
                {
                    ModelState.AddModelError("Amount", "Amount is not valid.");
                }
                //if (merchantcollege.Amount % 10 != 0)
                //{
                //    ModelState.AddModelError("Amount", "Amount is not valid.");
                //}


                if (ModelState.IsValid)
                {
                    //api call here
                    HttpResponseMessage _res = new HttpResponseMessage();
                    TraceIdGenerator _tig = new TraceIdGenerator();
                    var tid = _tig.GenerateTraceID();

                    string mobile = userName; //mobile is username
                    using (HttpClient client = new HttpClient())
                    {

                        var action = "merchant/payment";
                        var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                        var content = new FormUrlEncodedContent(new[]
                       {
                         new KeyValuePair<string, string>("sc",merchantschool.TransactionMedium),
                         new KeyValuePair<string, string>("vid",merchantschool.SchoolName),
                         new KeyValuePair<string, string>("mobile",mobile),

                         new KeyValuePair<string, string>("sa",""),
                         new KeyValuePair<string, string>("prod","1"),
                         new KeyValuePair<string, string>("amount",merchantschool.Amount.ToString()),
                         new KeyValuePair<string, string>("tid",tid),
                         new KeyValuePair<string, string>("pin",merchantschool.TPin),
                         new KeyValuePair<string, string>("note","College Payment successful"),
                         new KeyValuePair<string, string>("src","http")

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
                            if (msg.StatusCode == "200")
                            {
                                ResponseMessage myNames = ser.Deserialize<ResponseMessage>(msg.StatusMessage);

                                MerchantPaymentVM UVM = new MerchantPaymentVM();
                                string av = myNames.AvailableBalance;
                                string amttransfer = myNames.AmountTransferredBalance;
                                UVM.Method = "200";
                                UVM.Message = "School Payment Successful of Amount NRs." + merchantschool.Amount.ToString(); //myNames.Message;
                                UVM.Amount = myNames.AmountTransferredBalance;
                                UVM.Balance = myNames.AvailableBalance;
                                UVM.Sender = userName;
                                UVM.MName = merchantschool.SchoolName;
                                UVM.Tid = tid;


                                return View("MerchantPaymentComplete", UVM);
                            }
                            else
                            {
                                this.ViewData["merchantpay_messsage"] = msg.StatusMessage;
                                this.ViewData["message_class"] = "failed_info";
                                return View(merchantschool);
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
                            this.ViewData["merchantpay_messsage"] = StatusMessage;
                            this.ViewData["message_class"] = "failed_info";
                            return View(merchantschool);
                        }
                           
                    }

                   
                }
                else
                {
                    this.ViewData["merchantpay_messsage"] = "Validation Error, Please recheck and submit";

                    this.ViewData["message_class"] = "failed_info";
                    return View(merchantschool);
                }

            }
            catch (Exception ex)
            {
                result = false;
                errorMessage = ex.Message;
                this.ViewData["merchantpay_messsage"] = "Server Error, Please Contact administrator !";
                this.ViewData["message_class"] = "failed_info";

                return View(merchantschool);
            }

        }
        #endregion

    }
}