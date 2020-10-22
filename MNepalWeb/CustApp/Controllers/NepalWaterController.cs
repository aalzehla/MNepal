using CustApp.App_Start;
using CustApp.Helper;
using CustApp.Models;
using CustApp.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace CustApp.Controllers
{
    public class NepalWaterController : Controller
    {
        DAL objdal = new DAL();
        // GET: NepalWater
        #region "GET: Nepal Water Index"
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

                ViewBag.NepalWater = PaypointUtils.GetNepalWaterName();
                ViewBag.SenderMobileNo = userName;

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

        #region "POST: NepalWater CheckPayment"
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> NepalWaterPayment(NepalWater NW)
        {

            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;



            this.ViewData["userType"] = this.TempData["userType"];
            ViewBag.UserType = this.TempData["userType"];
            ViewBag.Name = name;


            string retoken = NW.TokenUnique;
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

                Session["NWCounter"] = NW.NWCounter;
                Session["CustomerID"] = NW.CustomerID;

                //api call here
                HttpResponseMessage _res = new HttpResponseMessage();
                string mobile = userName; //mobile is username
                TraceIdGenerator _tig = new TraceIdGenerator();
                var tid = _tig.GenerateTraceID();
                string tokenID = Session["TokenID"].ToString();

                //specify to use TLS 1.2 as default connection
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                using (HttpClient client = new HttpClient())
                {
                    var action = "paypointnepalwater.svc/checkpayment";
                    var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);

                    var content = new FormUrlEncodedContent(new[]{
                        new KeyValuePair<string, string>("vid", "130"),
                        new KeyValuePair<string, string>("mobile", mobile),
                        new KeyValuePair<string, string>("src","http"),
                        new KeyValuePair<string, string>("tokenID",tokenID),
                        new KeyValuePair<string, string>("companyCode", "720"),
                        new KeyValuePair<string, string>("serviceCode", NW.NWCounter),
                        new KeyValuePair<string, string>("account", NW.CustomerID),
                        new KeyValuePair<string, string>("special1",""),
                        new KeyValuePair<string, string>("special2",""),
                        new KeyValuePair<string, string>("tid", tid),
                        new KeyValuePair<string, string>("ClientCode", clientCode),
                        new KeyValuePair<string, string>("paypointType", "NepalWater"),


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

        #region "GET: NepalWater Details"
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

                string S_NWCounter = (string)Session["NWCounter"];
                string S_CustomerID = (string)Session["CustomerID"];
                if ((S_NWCounter == null) || (S_CustomerID == null))
                {
                    return RedirectToAction("Index");
                }

                NepalWater NWObj = new NepalWater();
                NWObj.NWCounter = S_NWCounter;
                NWObj.CustomerID = S_CustomerID;
                NWObj.UserName = userName;
                NWObj.ClientCode = clientCode;
                NWObj.refStan = getrefStan(NWObj);
                NepalWater regobj = new NepalWater();
                DataSet DPaypointSet = PaypointUtils.GetNWDetails(NWObj);
                DataTable dResponse = DPaypointSet.Tables["dtResponse"];
                DataTable dNWPayment = DPaypointSet.Tables["dtNWPayment"];
                if (dResponse != null && dResponse.Rows.Count > 0)
                {
                    regobj.CustomerID = dResponse.Rows[0]["account"].ToString();
                    //regobj.CustomerName = dResponse.Rows[0]["customerName"].ToString();
                    regobj.TotalAmountDue = dResponse.Rows[0]["amount"].ToString();
                    regobj.NWBranchCode = dResponse.Rows[0]["serviceCode"].ToString();
                    if (dNWPayment != null && dNWPayment.Rows.Count > 0)
                    {
                        regobj.description = dNWPayment.Rows[0]["description"].ToString();
                        regobj.CustomerName = dNWPayment.Rows[0]["fio"].ToString();
                        regobj.billDate = dNWPayment.Rows[0]["billDate"].ToString();
                        regobj.billAmount = dNWPayment.Rows[0]["billAmount"].ToString();
                        regobj.totalAmount = dNWPayment.Rows[0]["totalAmount"].ToString();
                        regobj.status = dNWPayment.Rows[0]["status"].ToString();
                        regobj.amountfact = dNWPayment.Rows[0]["amountfact"].ToString();
                        regobj.amountmask = dNWPayment.Rows[0]["amountmask"].ToString();
                        regobj.amountmax = dNWPayment.Rows[0]["amountmax"].ToString();
                        regobj.amountmin = dNWPayment.Rows[0]["amountmin"].ToString();
                        regobj.amountstep = dNWPayment.Rows[0]["amountstep"].ToString();
                        regobj.codserv = dNWPayment.Rows[0]["codserv"].ToString();
                        regobj.commission = dNWPayment.Rows[0]["commission"].ToString();
                        regobj.commisvalue = dNWPayment.Rows[0]["commisvalue"].ToString();
                        regobj.destination = dNWPayment.Rows[0]["destination"].ToString();
                        regobj.requestId = dNWPayment.Rows[0]["requestId"].ToString();
                        regobj.showCounter = dNWPayment.Rows[0]["showCounter"].ToString();
                        regobj.iCount = dNWPayment.Rows[0]["iCount"].ToString();
                        regobj.legatNumber = dNWPayment.Rows[0]["legatNumber"].ToString();
                        regobj.discountAmount = dNWPayment.Rows[0]["discountAmount"].ToString();
                        regobj.counterRent = dNWPayment.Rows[0]["counterRent"].ToString();
                        regobj.fineAmount = dNWPayment.Rows[0]["fineAmount"].ToString();
                        regobj.billDateFrom = dNWPayment.Rows[0]["billDateFrom"].ToString();
                        regobj.billDateTo = dNWPayment.Rows[0]["billDateTo"].ToString();
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
                ViewBag.NWBranchName = getNWBranchName(regobj.NWBranchCode.ToString());
                ViewBag.NWBranchCode = regobj.NWBranchCode.ToString();
                ViewBag.CustomerID = regobj.CustomerID;
                double TotalAmountDue = Convert.ToDouble(regobj.TotalAmountDue.ToString());
                TotalAmountDue = TotalAmountDue / 100;
                ViewBag.TotalAmountDue = TotalAmountDue;

                //Viewbag For details from Nepal Water
                ViewBag.description = regobj.description.ToString();
                ViewBag.CustomerName = regobj.CustomerName;
                ViewBag.billDate = regobj.billDate.ToString();

                double billAmount = Convert.ToDouble(regobj.billAmount.ToString());
                billAmount = billAmount / 100;
                ViewBag.billAmount = billAmount;

                double totalAmount = Convert.ToDouble(regobj.totalAmount.ToString());
                totalAmount = totalAmount / 100;
                ViewBag.totalAmount = totalAmount;

                ViewBag.status = regobj.status.ToString();
                ViewBag.amountfact = regobj.amountfact.ToString();
                ViewBag.amountmask = regobj.amountmask.ToString();
                ViewBag.amountmax = regobj.amountmax.ToString();
                ViewBag.amountmin = regobj.amountmin.ToString();
                ViewBag.amountstep = regobj.amountstep.ToString();
                ViewBag.codserv = regobj.codserv.ToString();
                ViewBag.commission = regobj.commission.ToString();
                ViewBag.commisvalue = regobj.commisvalue.ToString();
                ViewBag.destination = regobj.destination.ToString();
                ViewBag.requestId = regobj.requestId.ToString();
                ViewBag.showCounter = regobj.showCounter.ToString();
                ViewBag.iCount = regobj.iCount.ToString();
                ViewBag.legatNumber = regobj.legatNumber.ToString();

                double discountAmount = Convert.ToDouble(regobj.discountAmount.ToString());
                discountAmount = discountAmount / 100;
                ViewBag.discountAmount = discountAmount;


                double fineAmount = Convert.ToDouble(regobj.fineAmount.ToString());
                fineAmount = fineAmount / 100;
                ViewBag.fineAmount = fineAmount;

                ViewBag.counterRent = regobj.counterRent.ToString();
                ViewBag.billDateFrom = regobj.billDateFrom.ToString();
                ViewBag.billDateTo = regobj.billDateTo.ToString();


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

        #region "POST: Nepal Water ExecutePayment"
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> NWExecutePayment(NepalWater NW)
        {

            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;



            this.ViewData["userType"] = this.TempData["userType"];
            ViewBag.UserType = this.TempData["userType"];
            ViewBag.Name = name;


            string retoken = NW.TokenUnique;
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
            string BlockMessage = LoginUtils.GetMessage("01");
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

                string S_NWCounter = (string)Session["NWCounter"];
                string S_CustomerID = (string)Session["CustomerID"];

                NepalWater NWObj = new NepalWater();
                NWObj.NWCounter = S_NWCounter;
                NWObj.CustomerID = S_CustomerID;
                NWObj.UserName = userName;
                NWObj.ClientCode = clientCode;
                NWObj.refStan = getrefStan(NWObj);
                NepalWater regobj = new NepalWater();

                DataSet DPaypointSet = PaypointUtils.GetNWDetails(NWObj);
                DataTable dResponse = DPaypointSet.Tables["dtResponse"];
                DataTable dNWPayment = DPaypointSet.Tables["dtNWPayment"];
                if (dResponse != null && dResponse.Rows.Count > 0)
                {
                    regobj.CustomerID = dResponse.Rows[0]["account"].ToString();
                    regobj.CustomerName = dResponse.Rows[0]["customerName"].ToString();
                    regobj.TotalAmountDue = dResponse.Rows[0]["amount"].ToString();
                    regobj.NWBranchCode = dResponse.Rows[0]["serviceCode"].ToString();
                    regobj.refStan = dResponse.Rows[0]["refStan"].ToString();
                    regobj.billNumber = dResponse.Rows[0]["billNumber"].ToString();
                    regobj.responseCode = dResponse.Rows[0]["responseCode"].ToString();
                    regobj.retrievalReference = dResponse.Rows[0]["retrievalReference"].ToString();
                }

                HttpResponseMessage _res = new HttpResponseMessage();
                string mobile = userName; //mobile is username
                TraceIdGenerator _tig = new TraceIdGenerator();
                var tid = _tig.GenerateTraceID();

                //specify to use TLS 1.2 as default connection
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                using (HttpClient client = new HttpClient())
                {
                    var action = "paypointnepalwater.svc/executepayment";
                    var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                    string tokenID = Session["TokenID"].ToString();
                    var content = new FormUrlEncodedContent(new[]{
                        new KeyValuePair<string, string>("vid", "130"),//default
                        new KeyValuePair<string, string>("sc",NW.TransactionMedium),//user 00 10
                        new KeyValuePair<string, string>("mobile", mobile),
                        new KeyValuePair<string, string>("amount",NW.amount),//user
                        new KeyValuePair<string, string>("da","9840066836"),//default
                        new KeyValuePair<string, string>("pin",NW.TPin),//User
                        new KeyValuePair<string, string>("note", "Execute "+NW.Remarks),//User
                        new KeyValuePair<string, string>("src","http"), ////default
                        new KeyValuePair<string, string>("tokenID",tokenID),//default
                        new KeyValuePair<string, string>("companyCode", "720"),//default
                        new KeyValuePair<string, string>("serviceCode", regobj.NWBranchCode),//default
                        new KeyValuePair<string, string>("account",  regobj.CustomerID),//user
                        new KeyValuePair<string, string>("special1",""),//user
                        new KeyValuePair<string, string>("special2", ""),//user
                        new KeyValuePair<string, string>("tid", tid),//default
                        new KeyValuePair<string, string>("amountpay", regobj.TotalAmountDue),//database
                        new KeyValuePair<string, string>("refStan", regobj.refStan),//Database
                        new KeyValuePair<string, string>("billNumber", regobj.billNumber),//Database
                        new KeyValuePair<string, string>("rltCheckPaymt", regobj.responseCode),//Database
                        new KeyValuePair<string, string>("ClientCode", clientCode),
                        new KeyValuePair<string, string>("paypointType", "NepalWater"),
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
                   // string BlockMessage = LoginUtils.GetMessage("01");
                    try
                    {
                        if (_res.IsSuccessStatusCode)
                        {
                            //Session value removed
                            Session.Remove("NWCounter");
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
                           // return Json(new { responseCode = responseCode, responseText = respmsg },
                             return Json(new { responseCode = responseCode, responseText = respmsg, blockMessage = BlockMessage },
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
                                //  return Json(new { responseCode = responseCode, responseText = responsetext },
                                return Json(new { responseCode = responseCode, responseText = responsetext, blockMessage = BlockMessage },
                             JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                dynamic item = JValue.Parse(message);

                                // return Json(new { responseCode = responseCode, responseText = (string)item["StatusMessage"] },
                                return Json(new { responseCode = responseCode, responseText = (string)item["StatusMessage"], blockMessage = BlockMessage },
                           JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // return Json(new { responseCode = "400", responseText = ex.Message },
                        return Json(new { responseCode = "400", responseText = ex.Message, blockMessage = BlockMessage },
                            JsonRequestBehavior.AllowGet);
                    }

                }

            }
            else
            {
                //return Json(new { responseCode = "400", responseText = "Please refresh the page again." },
                return Json(new { responseCode = "400", responseText = "Please refresh the page again.", blockMessage = BlockMessage },
                            JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Get NepalWater Branch Name
        public string getNWBranchName(string BranchCode)
        {
            string NwBranchName = "SELECT NwBranchName FROM MNNepalWaterLocation (NOLOCK) where NwBranchCode='" + BranchCode + "'";
            DataTable dt = new DataTable();
            dt = objdal.MyMethod(NwBranchName);
            string BranchName = string.Empty;
            foreach (DataRow row in dt.Rows)
            {
                BranchName = row["NwBranchName"].ToString();
            }
            return BranchName;
        }
        #endregion

        #region Get Nepal Water refStan From Response Table
        public string getrefStan(NepalWater NWObj)
        {
            string Query_refStan = "select refStan from MNPaypointResponse where account='" + NWObj.CustomerID + "' AND serviceCode='" + NWObj.NWCounter + "' AND ClientCode='" + NWObj.ClientCode + "' AND UserName='" + NWObj.UserName + "'  order by transactionDate";
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