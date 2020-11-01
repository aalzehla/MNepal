using ThailiMNepalAgent.App_Start;
using ThailiMNepalAgent.Connection;
using ThailiMNepalAgent.Helper;
using ThailiMNepalAgent.Models;
using ThailiMNepalAgent.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace CustApp.Controllers
{
    public class ShareController : Controller
    {
        DAL objdal = new DAL();
        // GET: Share
        #region "GET: Share Index"
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

                ViewBag.DematName = DMATUtils.GetDematName();
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

        #region "POST: Demat CheckPayment"
        [HttpPost]

        public async Task<ActionResult> DMATPayment(DMAT dmat)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;



            this.ViewData["userType"] = this.TempData["userType"];
            ViewBag.UserType = this.TempData["userType"];
            ViewBag.Name = name;

            string retoken = dmat.TokenUnique;
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

                Session["DMATName"] = dmat.DematName;
                Session["DematNumber"] = dmat.DematCode;



                //api call here
                HttpResponseMessage _res = new HttpResponseMessage();
                string mobile = userName; //mobile is username
                TraceIdGenerator _tig = new TraceIdGenerator();
                var tid = _tig.GenerateTraceID();
                string tokenID = Session["TokenID"].ToString();

                //specify to use TLS 1.2 as default connection
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                DematGet dematGet = new DematGet();
                using (var client = new HttpClient())
                {
                    var action = "share.svc/checkpayment";
                    var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                    var content = new FormUrlEncodedContent(new[]{
                    new KeyValuePair<string,string>("BoId",Session["DematNumber"].ToString()),
                    new KeyValuePair<string, string>("bankCode",Session["DMATName"].ToString()),
                    new KeyValuePair<string, string>("mobile",mobile),
                    new KeyValuePair<string, string>("tId",tid),
                    new KeyValuePair<string, string>("tokenID",tokenID),
                    new KeyValuePair<string, string>("clientCode",clientCode),
                    new KeyValuePair<string, string>("paymentType","Demat"),

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
                                Session["timeStamp"] = myNames.timeStamp;
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
                            responsetext = JsonConvert.SerializeObject(@"Invalid BoId");
                            responsetext = responsetext.Replace("\"", string.Empty).Trim();
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
                return Json(new { responseCode = "400", responseText = "Please refresh the page again.", blockMessage = BlockMessage },
                            JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region "GET: Demat Details"
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

                string s_DMATName = (string)Session["DMATName"];
                string s_DMATNumber = (string)Session["DematNumber"];

                if ((s_DMATName == null) || (s_DMATNumber == null))
                {
                    return RedirectToAction("Index");
                }

                DMAT dMAT = new DMAT();
                dMAT.DematCode = s_DMATNumber;
                dMAT.DematName = s_DMATName;
                dMAT.UserName = userName;
                dMAT.ClientCode = clientCode;
                dMAT.TimeStamp = Session["timeStamp"].ToString();

                dMAT.refStan = getRetRef(dMAT);

                DMAT DematDetails = new DMAT();
                DataSet DMATSet = DMATUtils.GetDematDetails(dMAT);

                DataTable dResponse = DMATSet.Tables["dtResponse"];
                DataTable dPayment = DMATSet.Tables["dtPayment"];


                List<ListFees> ListDetails = new List<ListFees>();
                if (dResponse != null && dResponse.Rows.Count > 0)
                {
                    DematDetails.DematCode = dResponse.Rows[0]["BoId"].ToString();
                    DematDetails.DematName = dResponse.Rows[0]["DematName"].ToString();
                    DematDetails.TotalAmountDue = dResponse.Rows[0]["TotalAmount"].ToString();
                    DematDetails.Months = dResponse.Rows[0]["Fees"].ToString();

                    Session["DematCustomerName"] = DematDetails.DematName;

                    string[] lines = DematDetails.Months.Split(new[] { Environment.NewLine }, StringSplitOptions.None); //to split string to new line
                    lines = lines.Take(lines.Length - 1).ToArray();  //to remove last list which is empty 

                    //List<string> list = new List<string>(lines);

                    for (int i = 0; i < lines.Length; i++)
                    {
                        string Fees = lines[i];
                        ListDetails.Add(new ListFees
                        {
                            Description = Fees

                        });
                    }

                    ViewBag.ListDetails = ListDetails;



                }
                else
                {
                    return RedirectToAction("Index");
                }


                ViewBag.BankName = Session["DMATName"];
                if (ViewBag.BankName == "001")
                {
                    Session["DMATBankName"] = "NIBL ACE Capital";
                }

                ViewBag.BankName = Session["DMATBankName"];
                ViewBag.DematName = DematDetails.DematName;
                ViewBag.BoId = DematDetails.DematCode;
                ViewBag.TotalAmount = DematDetails.TotalAmountDue;
                ViewBag.Descriptions = DematDetails.Months;

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
            }

            return View();
        }
        #endregion

        #region "POST: Demat ExecutePayment"
        [HttpPost]
        public async Task<ActionResult> DematExecutePayment(DMAT demat)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            var DematBankName = Session["DMATName"];
            var DematNumber = Session["DematNumber"];
            var DematCustomerName = Session["DematCustomerName"];
            var ReferenceId = Session["ReferenceId"];

            TempData["userType"] = userType;



            this.ViewData["userType"] = this.TempData["userType"];
            ViewBag.UserType = this.TempData["userType"];
            ViewBag.Name = name;

            string retoken = demat.TokenUnique;
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

                string DMATNAME = (string)Session["DMATName"];
                string DmatNumber = (string)Session["DematNumber"];
                string TimeStamp = Session["timeStamp"].ToString();

                DMAT dematObj = new DMAT();
                dematObj.ClientCode = clientCode;
                dematObj.UserName = userName;
                dematObj.DematName = DMATNAME;
                dematObj.DematCode = DmatNumber;
                dematObj.TimeStamp = TimeStamp;
                dematObj.refStan = getRetRef(dematObj);

                DMAT regobj = new DMAT();

                DataSet DMATSet = DMATUtils.GetDematDetails(dematObj);


                DataTable dResponse = DMATSet.Tables["dtResponse"];
                DataTable dNWPayment = DMATSet.Tables["dtNWPayment"];
                if (dResponse != null && dResponse.Rows.Count > 0)
                {
                    regobj.DematCode = dResponse.Rows[0]["BoId"].ToString();
                    regobj.CustomerName = dResponse.Rows[0]["DematName"].ToString();
                    regobj.TotalAmountDue = dResponse.Rows[0]["TotalAmount"].ToString();
                    regobj.DematName = dResponse.Rows[0]["BankCode"].ToString();
                    regobj.TimeStamp = dResponse.Rows[0]["TimeStamp"].ToString();
                    regobj.ClientCode = dResponse.Rows[0]["ClientCode"].ToString();
                    regobj.retrievalReference = dResponse.Rows[0]["RetrievalRef"].ToString();
                    regobj.UserName = dResponse.Rows[0]["UserName"].ToString();

                }

                HttpResponseMessage _res = new HttpResponseMessage();
                string mobile = userName; //mobile is username
                TraceIdGenerator _tig = new TraceIdGenerator();
                var tid = _tig.GenerateTraceID();

                //specify to use TLS 1.2 as default connection
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                using (HttpClient client = new HttpClient())
                {
                    var destinationTestNumber = System.Configuration.ConfigurationManager.AppSettings["DestinationTestNumber"];
                    var destinationMerchantId = System.Configuration.ConfigurationManager.AppSettings["DestinationMerchantId"];
                    var action = "share.svc/executepayment";
                    var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                    string tokenID = Session["TokenID"].ToString();
                    var content = new FormUrlEncodedContent(new[]{
                         new KeyValuePair<string, string>("vid", destinationMerchantId),
                        new KeyValuePair<string, string>("BoId", dematObj.DematCode),
                        new KeyValuePair<string, string>("DematBank", dematObj.DematName),
                        new KeyValuePair<string, string>("mobile", mobile),
                        new KeyValuePair<string, string>("da",destinationTestNumber),//default
                        new KeyValuePair<string, string>("sc",demat.TransactionMedium),//user 00 10
                        new KeyValuePair<string, string>("amount",demat.amount),
                         new KeyValuePair<string, string>("pin",demat.TPin),//User
                        new KeyValuePair<string, string>("note", demat.Remarks),//User
                        new KeyValuePair<string, string>("tokenID",tokenID),//default
                        new KeyValuePair<string, string>("tid", tid),//default
                        new KeyValuePair<string, string>("RetrievalReference", regobj.retrievalReference),
                        new KeyValuePair<string, string>("ClientCode", clientCode),
                        new KeyValuePair<string, string>("TimeStamp", regobj.TimeStamp),
                        new KeyValuePair<string, string>("walletBalance", availBaln.amount),
                        new KeyValuePair<string, string>("paymentType", "Demat Payment"),
                        new KeyValuePair<string, string>("src","http") ////default
                });
                    _res = await client.PostAsync(new Uri(uri), content);
                    var a = _res.StatusCode;
                    var b = _res.IsSuccessStatusCode;

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
                                return Json(new { responseCode = responseCode, responseText = responsetext },
                            JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                dynamic item = JValue.Parse(message);

                                return Json(new { responseCode = responseCode, responseText = (string)item["StatusMessage"], blockMessage = BlockMessage },
                                JsonRequestBehavior.AllowGet);
                            }
                        }

                    }
                    catch (Exception ex)
                    {

                        return Json(new { responseCode = "400", responseText = ex.Message, blockMessage = BlockMessage },
                            JsonRequestBehavior.AllowGet);
                    }

                }
            }
            else
            {
                return Json(new { responseCode = "400", responseText = "Please refresh the page again.", blockMessage = BlockMessage },
                            JsonRequestBehavior.AllowGet);
            }

        }
        #endregion


        #region Get Demat Bank Name
        public string getBankName(string BankCode)
        {
            string DMBankName = "select DName from MNDemat where DCode='" + BankCode + "'";
            DataTable dt = new DataTable();
            dt = objdal.MyMethod(DMBankName);
            string BankName = string.Empty;
            foreach (DataRow row in dt.Rows)
            {
                BankName = row["DMBankName"].ToString();
            }
            return BankName;
        }
        #endregion


        #region Get RetrievalRef from Demat Request Table
        public string getRetRef(DMAT dMAT)
        {
            string Query_refStan = "select RetrievalRef from MNDematRequest where ClientCode='" + dMAT.ClientCode + "' AND UserName='" + dMAT.UserName + "' AND BankCode='" + dMAT.DematName + "' AND BoId='" + dMAT.DematCode + "' AND TimeStamp='" + dMAT.TimeStamp + "'";
            DataTable dt = new DataTable();
            dt = objdal.MyMethod(Query_refStan);
            string retRef = string.Empty;
            foreach (DataRow row in dt.Rows)
            {
                retRef = row["RetrievalRef"].ToString();
            }
            return retRef;
        }

        #endregion
    }
}