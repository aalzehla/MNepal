using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ThailiMNepalAgent.Models;
using ThailiMNepalAgent.Utilities;
using System.Web.Mvc;
using System.Data;
using System.Net.Http;
using ThailiMNepalAgent.Helper;
using System.IO;
using ThailiMNepalAgent.App_Start;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;


namespace ThailiMNepalAgent.Controllers
{
    public class NEAController : Controller
    {
        //string BankBalance;
        DAL objdal = new DAL();
        #region "GET: NEAPayment"
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


                ViewBag.NEA = PaypointUtils.GetNEAName();
                ViewBag.SenderMobileNo = userName;

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
        
        #region "POST: NEACheckPayment"
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> NEAPayment(NEAFundTransfer _NEAft)
        {

            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;



            this.ViewData["userType"] = this.TempData["userType"];
            ViewBag.UserType = this.TempData["userType"];
            ViewBag.Name = name;


            string retoken = _NEAft.TokenUnique;
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
                //START Session for User Input Data
                Session["S_SCNo"] = _NEAft.SCNo;
                Session["S_NEABranchName"] = _NEAft.NEABranchName;
                Session["S_CustomerID"] = _NEAft.CustomerID;
                //END Session
                HttpResponseMessage _res = new HttpResponseMessage();
                string mobile = userName; //mobile is username
                TraceIdGenerator _tig = new TraceIdGenerator();
                var tid = _tig.GenerateTraceID();

                using (HttpClient client = new HttpClient())
                {
                    var action = "paypoint.svc/checkpayment";
                    var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                    string tokenID = Session["TokenID"].ToString();
                    var content = new FormUrlEncodedContent(new[]{
                    new KeyValuePair<string, string>("vid", "14"),//default
                        new KeyValuePair<string, string>("mobile", mobile),
                        new KeyValuePair<string,string>("src","gprs"), ////default
                        new KeyValuePair<string,string>("tokenID",Session["TokenID"].ToString()),//default
                        new KeyValuePair<string, string>("companyCode", "598"),//default
                        new KeyValuePair<string,string>("serviceCode", "1"),//default
                        new KeyValuePair<string, string>("account",  _NEAft.SCNo),//user
                        new KeyValuePair<string, string>("special1",_NEAft.NEABranchName),//user
                        new KeyValuePair<string,string>("special2", _NEAft.CustomerID),//user
                        new KeyValuePair<string, string>("tid", tid),//default
                        new KeyValuePair<string, string>("ClientCode", clientCode),
                        new KeyValuePair<string, string>("paypointType", "NEA"),

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

        #region "GET: NEADetails"
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
                //Session data set
                string S_SCNo = (string)Session["S_SCNo"];
                string S_NEABranchName = (string)Session["S_NEABranchName"];
                string S_CustomerID = (string)Session["S_CustomerID"];
                if ((S_SCNo == null) || (S_NEABranchName == null) || (S_CustomerID == null))
                {
                    return RedirectToAction("Index");

                }
                //End Session data set

                NEAFundTransfer NEAObj = new NEAFundTransfer();
                NEAObj.SCNo = S_SCNo;
                NEAObj.NEABranchCode = S_NEABranchName;
                NEAObj.CustomerID = S_CustomerID;
                NEAObj.UserName = userName;
                NEAObj.ClientCode = clientCode;
                NEAObj.refStan = getrefStan(NEAObj);

                //Database Accessed
                NEAFundTransfer regobj = new NEAFundTransfer();
                DataSet DPaypointSet = PaypointUtils.GetNEADetails(NEAObj);
                DataTable dResponse = DPaypointSet.Tables["dtResponse"];
                DataTable dPayment = DPaypointSet.Tables["dtPayment"];
                //End Database Accessed

                ViewBag.rowNo = dPayment.Rows.Count;
                int countROW = dPayment.Rows.Count;
                List<NEAFundTransfer> ListDetails = new List<NEAFundTransfer>(countROW);
                if (dResponse != null && dResponse.Rows.Count > 0)
                {
                    regobj.SCNo = dResponse.Rows[0]["account"].ToString();
                    regobj.NEABranchCode = dResponse.Rows[0]["special1"].ToString();
                    regobj.CustomerID = dResponse.Rows[0]["special2"].ToString();
                    regobj.CustomerName = dResponse.Rows[0]["customerName"].ToString();
                    //regobj.TotalAmountDue = dResponse.Rows[0]["amount"].ToString();
                    if (dPayment != null && dPayment.Rows.Count > 0)
                    {
                        //For Payments months 
                        for (int i = 0; i < countROW; i++)
                        {
                            //Converting paisa to rupee
                            double NPRbillAmount = Convert.ToDouble(dPayment.Rows[i]["billAmount"].ToString());
                            NPRbillAmount = NPRbillAmount / 100;

                            double NPRamount = Convert.ToDouble(dPayment.Rows[i]["amount"].ToString());
                            NPRamount = NPRamount / 100;
                            //end Converting paisa to rupee
                            string description = dPayment.Rows[i]["description"].ToString();

                            // using the method to split
                            char[] spearator = { ':' };
                            String[] S_description = description.Split(spearator);

                            ListDetails.Add(new NEAFundTransfer
                            {
                                billDate = dPayment.Rows[i]["billDate"].ToString(),
                                description = S_description[1].ToString(),//Number of Days
                                status = dPayment.Rows[i]["status"].ToString(),
                                destination = dPayment.Rows[i]["destination"].ToString(),
                                //totalAmount = dPayment.Rows[i]["totalAmount"].ToString(),
                                billAmount = NPRbillAmount.ToString(),
                                amount = NPRamount.ToString()
                            });
                        }
                        //Converting paisa to rupee
                        regobj.totalAmount = dPayment.Rows[0]["totalAmount"].ToString();
                        double TotalAmountDue = Convert.ToDouble(regobj.totalAmount.ToString());
                        TotalAmountDue = TotalAmountDue / 100;
                        //end Converting paisa to rupee

                        //splitting decimal values 
                        int S_TotalAmountDue = Convert.ToInt32(TotalAmountDue);
                        String[] Str_TotalAmountDue = TotalAmountDue.ToString().Split('.');
                        if (Str_TotalAmountDue.Length == 2)
                        {
                            S_TotalAmountDue = Convert.ToInt32(TotalAmountDue.ToString().Split('.')[0]) + 1;//adding 1 to decimal value
                        }
                        //end
                        ViewBag.S_TotalAmountDue = S_TotalAmountDue;
                        ViewBag.TotalAmountDue = TotalAmountDue;

                        //Payment table
                        ViewBag.ListDetails = ListDetails;
                    }
                    else
                    {
                        //Converting paisa to rupee
                        regobj.totalAmount = dResponse.Rows[0]["amount"].ToString();
                        double TotalAmountDue = Convert.ToDouble(regobj.totalAmount.ToString());
                        TotalAmountDue = TotalAmountDue / 100;
                        //end Converting paisa to rupee
                        //splitting decimal values 
                        int S_TotalAmountDue = Convert.ToInt32(TotalAmountDue);
                        String[] Str_TotalAmountDue = TotalAmountDue.ToString().Split('.');
                        if (Str_TotalAmountDue.Length == 2)
                        {
                            S_TotalAmountDue = Convert.ToInt32(TotalAmountDue.ToString().Split('.')[0])+1;//adding 1 to decimal value
                        }
                        //end
                        ViewBag.S_TotalAmountDue = S_TotalAmountDue;
                        ViewBag.TotalAmountDue = TotalAmountDue;
                    }

                }
                else
                {
                    return RedirectToAction("Index");
                }

                ViewBag.SCNo = regobj.SCNo;
                ViewBag.NEABranchName = getNEABranchName(regobj.NEABranchCode.ToString());
                ViewBag.NEABranchCode = regobj.NEABranchCode.ToString();
                ViewBag.CustomerID = regobj.CustomerID;
                ViewBag.CustomerName = regobj.CustomerName;

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

        #region "POST: NEA ExecutePayment"
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> NEAExecutePayment(NEAFundTransfer _NEAft)
        {

            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;



            this.ViewData["userType"] = this.TempData["userType"];
            ViewBag.UserType = this.TempData["userType"];
            ViewBag.Name = name;


            string retoken = _NEAft.TokenUnique;
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

                string S_SCNo = (string)Session["S_SCNo"];
                string S_NEABranchName = (string)Session["S_NEABranchName"];
                string S_CustomerID = (string)Session["S_CustomerID"];
                NEAFundTransfer NEAObj = new NEAFundTransfer();
                NEAObj.SCNo = S_SCNo;
                NEAObj.NEABranchCode = S_NEABranchName;
                NEAObj.CustomerID = S_CustomerID;
                NEAObj.UserName = userName;
                NEAObj.ClientCode = clientCode;

                NEAFundTransfer regobj = new NEAFundTransfer();
                //DataSet dtableUserInfo = PaypointUtils.GetNEADetails(NEAObj);
                DataSet DPaypointSet = PaypointUtils.GetNEADetails(NEAObj);
                DataTable dResponse = DPaypointSet.Tables["dtResponse"];
                DataTable dPayment = DPaypointSet.Tables["dtPayment"];
                regobj.CustomerName = null;
                if (dResponse != null && dResponse.Rows.Count > 0)
                {
                    regobj.SCNo = dResponse.Rows[0]["account"].ToString();
                    regobj.NEABranchName = dResponse.Rows[0]["special1"].ToString();
                    regobj.CustomerID = dResponse.Rows[0]["special2"].ToString();
                    regobj.CustomerName = dResponse.Rows[0]["customerName"].ToString();
                    regobj.TotalAmountDue = dResponse.Rows[0]["amount"].ToString();
                    regobj.refStan = dResponse.Rows[0]["refStan"].ToString();
                    regobj.billNumber = dResponse.Rows[0]["billNumber"].ToString();
                    regobj.responseCode = dResponse.Rows[0]["responseCode"].ToString();
                    regobj.retrievalReference = dResponse.Rows[0]["retrievalReference"].ToString();

                }

                HttpResponseMessage _res = new HttpResponseMessage();
                string mobile = userName; //mobile is username
                TraceIdGenerator _tig = new TraceIdGenerator();
                var tid = _tig.GenerateTraceID();

                //await BankQuery();
                //string bankBal = BankBalance;

                using (HttpClient client = new HttpClient())
                {
                    var action = "paypoint.svc/executepayment";
                    var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                    string tokenID = Session["TokenID"].ToString();
                    var content = new FormUrlEncodedContent(new[]{
                        new KeyValuePair<string, string>("vid", "14"),//default
                        new KeyValuePair<string,string>("sc",_NEAft.TransactionMedium),//user 00 10
                        new KeyValuePair<string, string>("mobile", mobile),
                        new KeyValuePair<string, string>("amount",_NEAft.amount),//user
                        new KeyValuePair<string,string>("da","9840066836"),//default
                        new KeyValuePair<string,string>("pin",_NEAft.TPin),//User
                        new KeyValuePair<string, string>("note", "Execute "+_NEAft.Remarks),//User
                        new KeyValuePair<string,string>("src","gprs"), ////default
                        new KeyValuePair<string,string>("tokenID",Session["TokenID"].ToString()),//default
                        new KeyValuePair<string, string>("companyCode", "598"),//default
                        new KeyValuePair<string,string>("serviceCode", "1"),//default
                        new KeyValuePair<string, string>("account",  regobj.SCNo),//user regobj.SCNo
                        new KeyValuePair<string, string>("special1",regobj.NEABranchName),//user
                        new KeyValuePair<string,string>("special2", regobj.CustomerID),//user
                        new KeyValuePair<string, string>("tid", tid),//default
                        new KeyValuePair<string, string>("amountpay", regobj.TotalAmountDue),//database
                        new KeyValuePair<string, string>("refStan", regobj.refStan),//Database
                        new KeyValuePair<string, string>("billNumber", regobj.billNumber),//Database
                        new KeyValuePair<string, string>("rltCheckPaymt", regobj.responseCode),//Database
                        new KeyValuePair<string, string>("ClientCode", clientCode),
                        new KeyValuePair<string, string>("paypointType", "NEA"),
                        new KeyValuePair<string, string>("customerName", regobj.CustomerName),
                        new KeyValuePair<string, string>("walletBalance", availBaln.amount),
                        //new KeyValuePair<string, string>("bankBalance", bankBal),
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
                            Session.Remove("S_SCNo");
                            Session.Remove("S_NEABranchName");
                            Session.Remove("S_CustomerID");

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

        #region Get NEA Branch Name
        public string getNEABranchName(string BranchCode)
        {
            string NEABranchName = "select NEABranchName from MNNEALocation where NEABranchCode='" + BranchCode + "'";
            DataTable dt = new DataTable();
            dt = objdal.MyMethod(NEABranchName);
            string BranchName = string.Empty;
            foreach (DataRow row in dt.Rows)
            {
                BranchName = row["NEABranchName"].ToString();
            }
            return BranchName;
        }
        #endregion

        #region Get NEA refStan From Response Table
        public string getrefStan(NEAFundTransfer NEAObj)
        {
            // string Query_refStan = "select refStan from MNPaypointResponse where account='" + NEAObj.SCNo + "' AND special1='" + NEAObj.NEABranchCode + "' AND special2='" + NEAObj.CustomerID + "' AND ClientCode='" + NEAObj.ClientCode + "' AND UserName='" + NEAObj.UserName + "'";
            string Query_refStan = "select refStan from MNPaypointResponse where account='" + NEAObj.SCNo + "' AND special1='" + NEAObj.NEABranchCode + "' AND special2='" + NEAObj.CustomerID + "' AND ClientCode='" + NEAObj.ClientCode + "' AND UserName='" + NEAObj.UserName + "'  order by transactionDate";


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

        //#region CheckBankBalance
        //public async Task<ActionResult> BankQuery()
        //{
        //    string username = (string)Session["LOGGED_USERNAME"];
        //    string pin = "";
        //    if ((username != "") || (username != null))
        //    {

        //        string result = string.Empty;
        //        DataTable dtableMobileNo = CustomerUtils.GetUserProfileByMobileNo(username);
        //        if (dtableMobileNo.Rows.Count == 1)
        //        {
        //            pin = dtableMobileNo.Rows[0]["PIN"].ToString();
        //        }

        //    }

        //    HttpResponseMessage _res = new HttpResponseMessage();

        //    string mobile = username; //mobile is username


        //    TraceIdGenerator _tig = new TraceIdGenerator();
        //    var tid = _tig.GenerateTraceID();

        //    using (HttpClient client = new HttpClient())
        //    {

        //        var action = "query.svc/balance?tid=" + tid + "&sc=22&mobile=" + mobile + "&sa=1&pin=" + pin + "&src=web";
        //        var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
        //        //var content = new FormUrlEncodedContent(new[]{
        //        //        new KeyValuePair<string, string>("tid", tid),
        //        //        new KeyValuePair<string,string>("sc","22"),
        //        //        new KeyValuePair<string, string>("mobile",mobile),
        //        //        new KeyValuePair<string, string>("sa", "1"),
        //        //        new KeyValuePair<string,string>("pin", pin),
        //        //        new KeyValuePair<string,string>("src","web")
        //        //    });
        //        try
        //        {
        //            _res = await client.GetAsync(uri);
        //            string responseBody = _res.StatusCode.ToString() + " ," + await _res.Content.ReadAsStringAsync();
        //            _res.ReasonPhrase = responseBody;
        //            string errorMessage = string.Empty;
        //            int responseCode = 0;
        //            string message = string.Empty;
        //            string responsetext = string.Empty;
        //            string responsedateTime = string.Empty;
        //            bool result = false;
        //            string ava = string.Empty;
        //            string avatra = string.Empty;
        //            string avamsg = string.Empty;

        //            if (_res.IsSuccessStatusCode)
        //            {
        //                result = true;
        //                responseCode = (int)_res.StatusCode;
        //                responsetext = await _res.Content.ReadAsStringAsync();
        //                responsedateTime = await _res.Content.ReadAsStringAsync();
        //                message = _res.Content.ReadAsStringAsync().Result;
        //                string respmsg = "";
        //                string dateTime = "";
        //                if (!string.IsNullOrEmpty(message))
        //                {
        //                    JavaScriptSerializer ser = new JavaScriptSerializer();
        //                    var json = ser.Deserialize<JsonParse>(responsetext);
        //                    message = json.d;
        //                    JsonParse myNames = ser.Deserialize<JsonParse>(json.d);
        //                    int code = Convert.ToInt32(myNames.StatusCode);
        //                    respmsg = myNames.StatusMessage;
        //                    if (code != responseCode)
        //                    {
        //                        responseCode = code;
        //                    }
        //                }
        //                this.Session["bankSyncTime"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt");
        //                dateTime = Session["bankSyncTime"].ToString();
        //                this.Session["bankbal"] = respmsg;
        //                BankBalance = (string)respmsg;
        //                ViewBag.AvailBankBalnAmount = (string)respmsg;
        //                return Json(new { responseCode = responseCode, responseText = respmsg, responsedateTime = dateTime },
        //                JsonRequestBehavior.AllowGet);
        //            }
        //            else
        //            {
        //                result = false;
        //                responseCode = (int)_res.StatusCode;
        //                responsetext = await _res.Content.ReadAsStringAsync();
        //                dynamic json = JValue.Parse(responsetext);
        //                message = json.d;
        //                if (message == null)
        //                {
        //                    return Json(new { responseCode = responseCode, responseText = responsetext },
        //                JsonRequestBehavior.AllowGet);

        //                }
        //                else
        //                {
        //                    dynamic item = JValue.Parse(message);
        //                    ViewBag.AvailBankBalnAmount = (string)item["StatusMessage"];
        //                    return Json(new { responseCode = responseCode, responseText = (string)item["StatusMessage"] },
        //                    JsonRequestBehavior.AllowGet);

        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            return Json(new { responseCode = "400", responseText = ex.Message },
        //                JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}
        //#endregion 
    }
}