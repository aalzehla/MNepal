using ThailiMNepalAgent.App_Start;
using ThailiMNepalAgent.Helper;
using ThailiMNepalAgent.Models;
using ThailiMNepalAgent.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace ThailiMNepalAgent.Controllers
{
    public class KhanepaniController : Controller
    {
        DAL objdal = new DAL();
        // GET: Khanepani
        #region "GET: Khanepani Index"
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

                ViewBag.Khanepani = PaypointUtils.GetKhanepaniName();
                ViewBag.SenderMobileNo = userName;

                int id = TraceIdGenerator.GetID() + 1;
                string stringid = (id).ToString();//this.GetID() + 1
                string traceID = stringid.PadLeft(11, '0') + 'W';
                ViewBag.TraceID = traceID;

                List<SelectListItem> Months = new List<SelectListItem>();
                Months.Add(new SelectListItem { Text = "Baisakh", Value = "1" });
                Months.Add(new SelectListItem { Text = "Jestha", Value = "2" });
                Months.Add(new SelectListItem { Text = "Ashadh", Value = "3" });
                Months.Add(new SelectListItem { Text = "Shrawan", Value = "4" });
                Months.Add(new SelectListItem { Text = "Bhadra", Value = "5" });
                Months.Add(new SelectListItem { Text = "Ashoj", Value = "6" });
                Months.Add(new SelectListItem { Text = "Kartik", Value = "7" });
                Months.Add(new SelectListItem { Text = "Mangsir", Value = "8" });
                Months.Add(new SelectListItem { Text = "Poush", Value = "9" });
                Months.Add(new SelectListItem { Text = "Magh", Value = "10" });
                Months.Add(new SelectListItem { Text = "Falgun", Value = "11" });
                Months.Add(new SelectListItem { Text = "Chaitra", Value = "12" });

                ViewBag.Months = Months;

                UserInfo userInfo = new UserInfo();


                MNBalance availBaln = new MNBalance();
                DataTable dtableUser1 = AvailBalnUtils.GetAvailBaln(clientCode);
                if (dtableUser1 != null && dtableUser1.Rows.Count > 0)
                {
                    availBaln.amount = dtableUser1.Rows[0]["AvailBaln"].ToString();

                    ViewBag.AvailBalnAmount = availBaln.amount;
                }


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

                //For Profile Picture
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
                return RedirectToAction("Index", "Login");
            }
        }
        #endregion

        #region "POST: Khanepani CheckPayment"
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> KhanepaniPayment(Khanepani KP)
        {

            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;



            this.ViewData["userType"] = this.TempData["userType"];
            ViewBag.UserType = this.TempData["userType"];
            ViewBag.Name = name;


            string retoken = KP.TokenUnique;
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

                //For Profile Picture
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

                Session["KhanepaniCounter"] = KP.KhanepaniCounter;
                Session["CustomerID"] = KP.CustomerID;
                Session["Months"] = KP.Months;

                //api call here
                HttpResponseMessage _res = new HttpResponseMessage();
                string mobile = userName; //mobile is username
                TraceIdGenerator _tig = new TraceIdGenerator();
                var tid = _tig.GenerateTraceID();
                string tokenID = Session["TokenID"].ToString();

                using (HttpClient client = new HttpClient())
                {
                    var action = "paypointkhanepani.svc/checkpayment";
                    var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);

                    var content = new FormUrlEncodedContent(new[]{
                        new KeyValuePair<string, string>("vid", "130"),
                        new KeyValuePair<string, string>("mobile", mobile),
                        new KeyValuePair<string, string>("src","http"),
                        new KeyValuePair<string, string>("tokenID",tokenID),
                        new KeyValuePair<string, string>("companyCode", "761"),
                        new KeyValuePair<string, string>("serviceCode", KP.KhanepaniCounter),
                        new KeyValuePair<string, string>("account", KP.CustomerID),
                        new KeyValuePair<string, string>("special1",KP.Months), //months
                        new KeyValuePair<string, string>("special2", ""),
                        new KeyValuePair<string, string>("tid", tid),
                        new KeyValuePair<string, string>("ClientCode", clientCode),
                        new KeyValuePair<string, string>("paypointType", "Khanepani"),


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
                            string customer_nameKI2 = "";
                            if (!string.IsNullOrEmpty(message))
                            {
                                JavaScriptSerializer ser = new JavaScriptSerializer();
                                var json = ser.Deserialize<JsonParse>(responsetext);
                                message = json.d;
                                JsonParse myNames = ser.Deserialize<JsonParse>(json.d);
                                int code = Convert.ToInt32(myNames.StatusCode);
                                respmsg = myNames.StatusMessage;
                                customer_nameKI2 = myNames.customer_nameKI;
                                //for sending the customername to excutepayment get page
                                Session["customer_nameKI2"] = customer_nameKI2;
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
                }

            }
            else
            {
                return Json(new { responseCode = "400", responseText = "Please refresh the page again." },
                            JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region "GET: Khanepani Details"
        public ActionResult Details()
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

                ViewBag.SenderMobileNo = userName;

                string S_KhanepaniCounter = (string)Session["KhanepaniCounter"];
                string S_CustomerID = (string)Session["CustomerID"];
                string S_Months = (string)Session["Months"];
                if ((S_KhanepaniCounter == null) || (S_CustomerID == null) || (S_Months == null))
                {
                    return RedirectToAction("Index");
                }

                Khanepani KPObj = new Khanepani();
                KPObj.KhanepaniCounter = S_KhanepaniCounter;
                KPObj.CustomerID = S_CustomerID;
                KPObj.UserName = userName;
                KPObj.ClientCode = clientCode;
                KPObj.refStan = getrefStan(KPObj);
                Khanepani regobj = new Khanepani();
                DataSet DPaypointSet = PaypointUtils.GetKPDetails(KPObj);
                DataTable dResponse = DPaypointSet.Tables["dtResponse"];
                DataTable dKhanepaniInvoice = DPaypointSet.Tables["dtKhanepaniInvoice"];
                if (dResponse != null && dResponse.Rows.Count > 0)
                {
                    regobj.CustomerID = dResponse.Rows[0]["account"].ToString();
                    //regobj.CustomerName = dResponse.Rows[0]["customerName"].ToString();
                    regobj.TotalAmountDue = dResponse.Rows[0]["amount"].ToString();
                    regobj.KpBranchCode = dResponse.Rows[0]["serviceCode"].ToString();
                    if (dKhanepaniInvoice != null && dKhanepaniInvoice.Rows.Count > 0)
                    {
                        regobj.status = dKhanepaniInvoice.Rows[0]["status"].ToString();
                        regobj.total_advance_amount = dKhanepaniInvoice.Rows[0]["total_advance_amount"].ToString();
                        regobj.customer_code = dKhanepaniInvoice.Rows[0]["customer_code"].ToString();
                        regobj.address = dKhanepaniInvoice.Rows[0]["address"].ToString();
                        regobj.total_credit_sales_amount = dKhanepaniInvoice.Rows[0]["total_credit_sales_amount"].ToString();
                        //regobj.customer_name = dKhanepaniInvoice.Rows[0]["customer_name"].ToString();
                        regobj.customer_name = (string)Session["customer_nameKI2"];// receiving value from CP post
                        regobj.current_month_dues = dKhanepaniInvoice.Rows[0]["current_month_dues"].ToString();
                        regobj.mobile_number = dKhanepaniInvoice.Rows[0]["mobile_number"].ToString();
                        regobj.total_dues = dKhanepaniInvoice.Rows[0]["total_dues"].ToString();
                        regobj.previous_dues = dKhanepaniInvoice.Rows[0]["previous_dues"].ToString();
                        regobj.current_month_discount = dKhanepaniInvoice.Rows[0]["current_month_discount"].ToString();
                        regobj.current_month_fine = dKhanepaniInvoice.Rows[0]["current_month_fine"].ToString();
                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    return RedirectToAction("Index");
                }
                ViewBag.KpBranchName = getKpBranchName(regobj.KpBranchCode.ToString());
                ViewBag.KpBranchCode = regobj.KpBranchCode.ToString();
                ViewBag.CustomerID = regobj.CustomerID;
                ViewBag.CustomerName = regobj.CustomerName;
                double TotalAmountDue = Convert.ToDouble(regobj.TotalAmountDue.ToString());
                TotalAmountDue = TotalAmountDue / 100;
                ViewBag.TotalAmountDue = TotalAmountDue;

                //Viewbag For details from Khanepani invoice
                ViewBag.status = regobj.status.ToString();
                ViewBag.total_advance_amount = regobj.total_advance_amount.ToString();
                ViewBag.customer_code = regobj.customer_code.ToString();
                ViewBag.address = regobj.address.ToString();
                ViewBag.total_credit_sales_amount = regobj.total_credit_sales_amount.ToString();
                ViewBag.customer_name = regobj.customer_name.ToString();
                ViewBag.current_month_dues = regobj.current_month_dues.ToString();
                ViewBag.mobile_number = regobj.mobile_number.ToString();
                ViewBag.total_dues = regobj.total_dues.ToString();
                ViewBag.previous_dues = regobj.previous_dues.ToString();
                ViewBag.current_month_discount = regobj.current_month_discount.ToString();
                ViewBag.current_month_fine = regobj.current_month_fine.ToString();


                int id = TraceIdGenerator.GetID() + 1;
                string stringid = (id).ToString();//this.GetID() + 1
                string traceID = stringid.PadLeft(11, '0') + 'W';
                ViewBag.TraceID = traceID;

                UserInfo userInfo = new UserInfo();


                MNBalance availBaln = new MNBalance();
                DataTable dtableUser1 = AvailBalnUtils.GetAvailBaln(clientCode);
                if (dtableUser1 != null && dtableUser1.Rows.Count > 0)
                {
                    availBaln.amount = dtableUser1.Rows[0]["AvailBaln"].ToString();

                    ViewBag.AvailBalnAmount = availBaln.amount;
                }


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

                //For Profile Picture
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
                return RedirectToAction("Index", "Login");
            }
        }
        #endregion

        #region "POST: Khanepani ExecutePayment"
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> KPExecutePayment(Khanepani KP)
        {

            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;



            this.ViewData["userType"] = this.TempData["userType"];
            ViewBag.UserType = this.TempData["userType"];
            ViewBag.Name = name;


            string retoken = KP.TokenUnique;
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

                //For Profile Picture
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

                string S_KhanepaniCounter = (string)Session["KhanepaniCounter"];
                string S_CustomerID = (string)Session["CustomerID"];
                string S_Months = (string)Session["Months"];

                Khanepani KPObj = new Khanepani();
                KPObj.KhanepaniCounter = S_KhanepaniCounter;
                KPObj.CustomerID = S_CustomerID;
                KPObj.UserName = userName;
                KPObj.ClientCode = clientCode;
                KPObj.refStan = getrefStan(KPObj);
                Khanepani regobj = new Khanepani();

                DataSet DPaypointSet = PaypointUtils.GetKPDetails(KPObj);
                DataTable dResponse = DPaypointSet.Tables["dtResponse"];
                DataTable dKhanepaniInvoice = DPaypointSet.Tables["dtKhanepaniInvoice"];
                if (dResponse != null && dResponse.Rows.Count > 0)
                {
                    regobj.CustomerID = dResponse.Rows[0]["account"].ToString();
                    regobj.CustomerName = dResponse.Rows[0]["customerName"].ToString();
                    regobj.TotalAmountDue = dResponse.Rows[0]["amount"].ToString();
                    regobj.KpBranchCode = dResponse.Rows[0]["serviceCode"].ToString();
                    regobj.Months = dResponse.Rows[0]["special1"].ToString();
                    regobj.refStan = dResponse.Rows[0]["refStan"].ToString();
                    regobj.billNumber = dResponse.Rows[0]["billNumber"].ToString();
                    regobj.responseCode = dResponse.Rows[0]["responseCode"].ToString();
                    regobj.retrievalReference = dResponse.Rows[0]["retrievalReference"].ToString();
                }

                HttpResponseMessage _res = new HttpResponseMessage();
                string mobile = userName; //mobile is username
                TraceIdGenerator _tig = new TraceIdGenerator();
                var tid = _tig.GenerateTraceID();

                using (HttpClient client = new HttpClient())
                {
                    var action = "paypointkhanepani.svc/executepayment";
                    var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                    string tokenID = Session["TokenID"].ToString();
                    var content = new FormUrlEncodedContent(new[]{
                        new KeyValuePair<string, string>("vid", "14"),//default
                        new KeyValuePair<string, string>("sc",KP.TransactionMedium),//user 00 10
                        new KeyValuePair<string, string>("mobile", mobile),
                        new KeyValuePair<string, string>("amount",KP.amount),//user
                        new KeyValuePair<string, string>("da","9840066836"),//default
                        new KeyValuePair<string, string>("pin",KP.TPin),//User
                        new KeyValuePair<string, string>("note", "Execute "+KP.Remarks),//User
                        new KeyValuePair<string, string>("src","gprs"), ////default
                        new KeyValuePair<string, string>("tokenID",tokenID),//default
                        new KeyValuePair<string, string>("companyCode", "761"),//default
                        new KeyValuePair<string, string>("serviceCode", regobj.KpBranchCode),//default
                        new KeyValuePair<string, string>("account",  regobj.CustomerID),//user
                        new KeyValuePair<string, string>("special1",regobj.Months),//user
                        new KeyValuePair<string, string>("special2", ""),//user
                        new KeyValuePair<string, string>("tid", tid),//default
                        new KeyValuePair<string, string>("amountpay", regobj.TotalAmountDue),//database
                        new KeyValuePair<string, string>("refStan", regobj.refStan),//Database  regobj.refStan
                        new KeyValuePair<string, string>("billNumber", regobj.billNumber),//Database regobj.billNumber
                        new KeyValuePair<string, string>("rltCheckPaymt", regobj.responseCode),//Database
                        new KeyValuePair<string, string>("ClientCode", clientCode),
                        new KeyValuePair<string, string>("paypointType", "Khanepani"),
                        new KeyValuePair<string, string>("customerName", regobj.CustomerName),
                        new KeyValuePair<string, string>("walletBalance", availBaln.amount),
                        new KeyValuePair<string, string>("retrievalReference", regobj.retrievalReference),//Database

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
                            //Session value removed
                            Session.Remove("KhanepaniCounter");
                            Session.Remove("CustomerID");

                            result = true;
                            responseCode = (int)_res.StatusCode;
                            responsetext = await _res.Content.ReadAsStringAsync();
                            message = _res.Content.ReadAsStringAsync().Result;
                            string respmsg = "";
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

                }

            }
            else
            {
                return Json(new { responseCode = "400", responseText = "Please refresh the page again." },
                            JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region Get Khanepani Branch Name
        public string getKpBranchName(string BranchCode)
        {
            string KpBranchName = "select KpBranchName from MNKhanepaniLocation where KpBranchCode='" + BranchCode + "'";
            DataTable dt = new DataTable();
            dt = objdal.MyMethod(KpBranchName);
            string BranchName = string.Empty;
            foreach (DataRow row in dt.Rows)
            {
                BranchName = row["KpBranchName"].ToString();
            }
            return BranchName;
        }
        #endregion

        #region Get Khanepani refStan From Response Table
        public string getrefStan(Khanepani KPObj)
        {
            string Query_refStan = "select refStan from MNPaypointResponse where account='" + KPObj.CustomerID + "' AND serviceCode='" + KPObj.KhanepaniCounter + "' AND ClientCode='" + KPObj.ClientCode + "' AND UserName='" + KPObj.UserName + "'  order by transactionDate";
            DataTable dt = new DataTable();
            dt = objdal.MyMethod(Query_refStan);
            string refStan = string.Empty;
            foreach (DataRow row in dt.Rows)
            {
                refStan = row["refStan"].ToString();
            }
            return refStan;
        }
        #endregion
    }
}