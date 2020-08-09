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
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace CustApp.Controllers
{
    public class UtilityController : Controller
    {
        DAL objdal = new DAL();

        #region GET: NCellTopUp
        public ActionResult NCellTopUp()
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

                ViewBag.NCellServices = PaypointUtils.GetNCellServices();
                ViewBag.NCellPackages = PaypointUtils.GetNCellPackages();

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

        #region "POST: NCellTopUp CheckPayment"
        [HttpPost]
        public async Task<ActionResult> NCellTopUpCheck(ISP iSP)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;



            this.ViewData["userType"] = this.TempData["userType"];
            ViewBag.UserType = this.TempData["userType"];
            ViewBag.Name = name;


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

            Session["MobileNumber"] = iSP.mobile;
            Session["ServiceCode"] = iSP.ServiceCode;

            //api call here
            HttpResponseMessage _res = new HttpResponseMessage();
            string mobile = userName; //mobile is username
            TraceIdGenerator _tig = new TraceIdGenerator();
            var tid = _tig.GenerateTraceID();
            string tokenID = Session["TokenID"].ToString();
            var paypointType = "";
            var special1 = "";
            if(iSP.ServiceCode=="0")
            {
                paypointType = "NCell TopUp";
                special1 = iSP.amount;

            }
            if(iSP.ServiceCode=="3")
            {
                if (iSP.PackageAmount == "Per Day 300MB - 98.99 Rs")
                {
                    special1 = "Per_Day_300MB";
                    paypointType = special1;
                }
                if (iSP.PackageAmount == "Per Day 500MB - 498.83 Rs")
                {
                    special1 = "Per_Day_500MB";
                    paypointType = special1;
                }
            }
            using (HttpClient client = new HttpClient())
            {
                var action = "Utility.svc/Ncellcheckpayment";
                var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);

                var content = new FormUrlEncodedContent(new[]{
                        new KeyValuePair<string, string>("vid", "130"),
                        new KeyValuePair<string, string>("mobile", mobile),
                        new KeyValuePair<string, string>("src","http"),
                        new KeyValuePair<string, string>("tokenID",tokenID),
                        new KeyValuePair<string, string>("companyCode", "78"),
                        new KeyValuePair<string, string>("serviceCode", iSP.ServiceCode),
                        new KeyValuePair<string, string>("account", iSP.mobile),
                        new KeyValuePair<string, string>("special1", special1),
                        new KeyValuePair<string, string>("special2",""),
                        new KeyValuePair<string, string>("tid", tid),
                        new KeyValuePair<string, string>("ClientCode", clientCode),
                        new KeyValuePair<string, string>("paypointType", paypointType),


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
        #endregion

        #region "GET: Ncell Details"
        public ActionResult NCellDetails()
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


                string S_CustomerID = (string)Session["MobileNumber"];
                string S_Amount = (string)Session["Amount"];
                if ((S_CustomerID == null) && (S_Amount == null))
                {
                    return RedirectToAction("Index");
                }
                ISP iSP = new ISP();

                iSP.CustomerID = S_CustomerID;
                iSP.UserName = userName;
                iSP.ClientCode = clientCode;
                iSP.refStan = getrefStan(iSP);

                ISP regobj = new ISP();
                DataSet DPaypointSet = PaypointUtils.GetNcellDetails(iSP);
                DataTable dResponse = DPaypointSet.Tables["dtResponse"];
                DataTable dDHPayment = DPaypointSet.Tables["dtNWPayment"];

                
                if (dResponse != null && dResponse.Rows.Count > 0)
                {
                    regobj.CustomerID = dResponse.Rows[0]["account"].ToString();
                    regobj.TotalAmountDue = dResponse.Rows[0]["amount"].ToString();
                    regobj.NWBranchCode = dResponse.Rows[0]["serviceCode"].ToString();
                    if (dDHPayment != null && dDHPayment.Rows.Count > 0)
                    {
                        regobj.BillDate = dDHPayment.Rows[0]["billDate"].ToString();
                        regobj.amount = dDHPayment.Rows[0]["billAmount"].ToString();
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

                ViewBag.CustomerID = regobj.CustomerID;
                if (Session["ServiceCode"].ToString() == "3")
                {
                    ViewBag.BillAmount = ((float.Parse(regobj.amount)) / 100).ToString();
                }
                else
                {
                    ViewBag.BillAmount = regobj.amount;
                }
                
                ViewBag.CustomerName = regobj.CustomerName;
                ViewBag.billDate = regobj.BillDate.ToString();

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

        #region "POST: NCell ExecutePayment"
        [HttpPost]
        public async Task<ActionResult> NCellExecutePayment(NepalWater NW)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;



            this.ViewData["userType"] = this.TempData["userType"];
            ViewBag.UserType = this.TempData["userType"];
            ViewBag.Name = name;


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


            ISP iSP = new ISP();
            iSP.NWCounter = S_NWCounter;
            iSP.CustomerID = S_CustomerID;
            iSP.UserName = userName;
            iSP.ClientCode = clientCode;
            iSP.refStan = getrefStan(iSP);
            NepalWater regobj = new NepalWater();

            DataSet DPaypointSet = PaypointUtils.GetDishHomeDetails(iSP);
            DataTable dResponse = DPaypointSet.Tables["dtResponse"];
            DataTable dNWPayment = DPaypointSet.Tables["dtNWPayment"];
            if (dResponse != null && dResponse.Rows.Count > 0)
            {
                regobj.CustomerID = dResponse.Rows[0]["account"].ToString();
                regobj.CustomerName = dResponse.Rows[0]["customerName"].ToString();
                regobj.TotalAmountDue = NW.amount;
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
                var destinationTestNumber = System.Configuration.ConfigurationManager.AppSettings["DestinationTestNumber"];
                var destinationMerchantId = System.Configuration.ConfigurationManager.AppSettings["DestinationMerchantIdPaypoint"];
                var action = "Utility.svc/NCellExecutePayment";
                var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                string tokenID = Session["TokenID"].ToString();
                var content = new FormUrlEncodedContent(new[]{
                        new KeyValuePair<string, string>("vid", destinationMerchantId),//default
                        new KeyValuePair<string, string>("sc",NW.TransactionMedium),//user 00 10
                        new KeyValuePair<string, string>("mobile", mobile),
                        new KeyValuePair<string, string>("amount",NW.amount),//user
                        new KeyValuePair<string, string>("da",destinationTestNumber),//default
                        new KeyValuePair<string, string>("pin",NW.TPin),//User
                        new KeyValuePair<string, string>("note", "Execute "+ NW.Remarks),//User
                        new KeyValuePair<string, string>("src","http"), ////default
                        new KeyValuePair<string, string>("tokenID",tokenID),//default
                        new KeyValuePair<string, string>("companyCode", "78"),//default
                        new KeyValuePair<string, string>("serviceCode", "0"),//default
                        new KeyValuePair<string, string>("account",  regobj.CustomerID),//user
                        new KeyValuePair<string, string>("special1", regobj.CustomerID),//user
                        new KeyValuePair<string, string>("special2", ""),//user
                        new KeyValuePair<string, string>("tid", tid),//default
                        new KeyValuePair<string, string>("amountpay", regobj.TotalAmountDue),//database
                        new KeyValuePair<string, string>("refStan", regobj.refStan),//Database
                        new KeyValuePair<string, string>("billNumber", regobj.billNumber),//Database
                        new KeyValuePair<string, string>("rltCheckPaymt", regobj.responseCode),//Database
                        new KeyValuePair<string, string>("ClientCode", clientCode),
                        new KeyValuePair<string, string>("paypointType", "NCell TopUp"),
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
                        //Session value remove
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
        #endregion


        #region GET: NTC TopUp
        public ActionResult NTCTopUp()
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

                ViewBag.NTCServices = PaypointUtils.GetNTCServices();
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

        #region "POST: NTCTopUp CheckPayment"
        [HttpPost]
        public async Task<ActionResult> NTCTopUpCheck(ISP iSP)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;



            this.ViewData["userType"] = this.TempData["userType"];
            ViewBag.UserType = this.TempData["userType"];
            ViewBag.Name = name;


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

            Session["CustomerID"] = iSP.CustomerID;
            Session["ServiceCode"] = iSP.ServiceCode;

            //api call here
            HttpResponseMessage _res = new HttpResponseMessage();
            string mobile = userName; //mobile is username
            TraceIdGenerator _tig = new TraceIdGenerator();
            var tid = _tig.GenerateTraceID();
            string tokenID = Session["TokenID"].ToString();
            using (HttpClient client = new HttpClient())
            {
                var action = "Utility.svc/NTCcheckpayment";
                var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);

                var content = new FormUrlEncodedContent(new[]{
                        new KeyValuePair<string, string>("vid", "130"),
                        new KeyValuePair<string, string>("mobile", mobile),
                        new KeyValuePair<string, string>("src","http"),
                        new KeyValuePair<string, string>("tokenID",tokenID),
                        new KeyValuePair<string, string>("companyCode", "585"),
                        new KeyValuePair<string, string>("serviceCode", iSP.ServiceCode),
                        new KeyValuePair<string, string>("account", iSP.CustomerID),
                        new KeyValuePair<string, string>("special1",iSP.amount),
                        new KeyValuePair<string, string>("special2",""),
                        new KeyValuePair<string, string>("tid", tid),
                        new KeyValuePair<string, string>("ClientCode", clientCode),
                        new KeyValuePair<string, string>("paypointType", "NTC TopUp"),


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
        #endregion

        #region "GET: NTC Details"
        public ActionResult NTCTopUpDetails()
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


                string S_CustomerID = (string)Session["CustomerID"];
                string S_Amount = (string)Session["Amount"];
                string S_ServiceCode = (string)Session["ServiceCode"];
                string serviceName = "";
                if(S_ServiceCode == "0")
                {
                    serviceName = "AIRTIME (prepaid)";
                }
                else if(S_ServiceCode == "1")
                {
                    serviceName = "GSM (Postpaid)";
                }
                else if (S_ServiceCode == "2")
                {
                    serviceName = "PSTN (Landline)";
                }
                else if (S_ServiceCode == "3")
                {
                    serviceName = "ADSLU - Internet";
                }
                else if (S_ServiceCode == "4")
                {
                    serviceName = "ADSLV - Internet";
                }
                else if (S_ServiceCode == "5")
                {
                    serviceName = "CDMA (pinless)";
                }
                else if (S_ServiceCode == "6")
                {
                    serviceName = "NTFTTH - Internet";
                }
                else if (S_ServiceCode == "7")
                {
                    serviceName = "NTWIMAX - Internet";
                }
                if ((S_CustomerID == null) && (S_Amount == null) && (S_ServiceCode == null))
                {
                    return RedirectToAction("Index");
                }
                ISP iSP = new ISP();

                iSP.CustomerID = S_CustomerID;
                iSP.UserName = userName;
                iSP.ClientCode = clientCode;
                iSP.refStan = getrefStan(iSP);
                MNPayPointISPPayments regobj = new MNPayPointISPPayments();

                DataSet DPaypointSet = PaypointUtils.GetNTCDetails(iSP);
                DataTable dResponse = DPaypointSet.Tables["dtResponse"];
                DataTable dDHPayment = DPaypointSet.Tables["dtNWPayment"];

                List<ISP> ListDetails = new List<ISP>();
                if (dResponse != null && dResponse.Rows.Count > 0)
                {
                    regobj.CustomerID = dResponse.Rows[0]["account"].ToString();
                    regobj.TotalAmountDue = dResponse.Rows[0]["amount"].ToString();
                    regobj.NWBranchCode = dResponse.Rows[0]["serviceCode"].ToString();
                    if (dDHPayment != null && dDHPayment.Rows.Count > 0)
                    {
                        regobj.description = dDHPayment.Rows[0]["Description"].ToString();
                        regobj.PackageAmount = dDHPayment.Rows[0]["PackageAmount"].ToString();
                        regobj.billDate = dDHPayment.Rows[0]["billDate"].ToString();

                        string[] lines = regobj.description.Split(new[] { Environment.NewLine }, StringSplitOptions.None); //to split string to new line
                        lines = lines.Take(lines.Length - 1).ToArray();  //to remove last list which is empty 

                        string[] lines1 = regobj.PackageAmount.Split(new[] { Environment.NewLine }, StringSplitOptions.None); //to split string to new line
                        lines1 = lines1.Take(lines1.Length - 1).ToArray();  //to remove last list which is empty 

                        for (int i = 0; i < lines.Length; i++)
                        {
                            string Packages = lines[i];
                            ListDetails.Add(new ISP
                            {
                                Description = Packages,
                                PackageAmount = lines1[i]
                            });
                        }



                        ViewBag.ListDetails = ListDetails;

                    }
                    else
                    {
                        return RedirectToAction("NTCTopUp");
                    }
                }
                else
                {
                    return RedirectToAction("NTCTopUp");
                }

                ViewBag.ServiceNumber = regobj.CustomerID;
                //ViewBag.BillAmount = S_Amount;
                if(S_ServiceCode =="0")
                {
                ViewBag.BillAmount = regobj.TotalAmountDue;
                }
                else
                {
                    ViewBag.BillAmount = (Convert.ToInt32(regobj.TotalAmountDue) / 100).ToString();
                }
                ViewBag.ServiceName = serviceName;
                ViewBag.billDate = regobj.billDate.ToString();

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

        #region "POST: NTC ExecutePayment"
        [HttpPost]
        public async Task<ActionResult> NTCExecutePayment(NepalWater NW)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            string S_ServiceCode = (string)Session["ServiceCode"];

            TempData["userType"] = userType;



            this.ViewData["userType"] = this.TempData["userType"];
            ViewBag.UserType = this.TempData["userType"];
            ViewBag.Name = name;


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


            ISP iSP = new ISP();
            iSP.NWCounter = S_NWCounter;
            iSP.CustomerID = S_CustomerID;
            iSP.UserName = userName;
            iSP.ClientCode = clientCode;
            iSP.refStan = getrefStan(iSP);
            NepalWater regobj = new NepalWater();

            DataSet DPaypointSet = PaypointUtils.GetDishHomeDetails(iSP);
            DataTable dResponse = DPaypointSet.Tables["dtResponse"];
            DataTable dNWPayment = DPaypointSet.Tables["dtNWPayment"];
            if (dResponse != null && dResponse.Rows.Count > 0)
            {
                regobj.CustomerID = dResponse.Rows[0]["account"].ToString();
                regobj.CustomerName = dResponse.Rows[0]["customerName"].ToString();
                regobj.TotalAmountDue = NW.amount;
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
                var destinationTestNumber = System.Configuration.ConfigurationManager.AppSettings["DestinationTestNumber"];
                var destinationMerchantId = System.Configuration.ConfigurationManager.AppSettings["DestinationMerchantIdPaypoint"];
                var action = "Utility.svc/NTCExecutePayment";
                var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                string tokenID = Session["TokenID"].ToString();
                var content = new FormUrlEncodedContent(new[]{
                        new KeyValuePair<string, string>("vid", destinationMerchantId),//default
                        new KeyValuePair<string, string>("sc",NW.TransactionMedium),//user 00 10
                        new KeyValuePair<string, string>("mobile", mobile),
                        new KeyValuePair<string, string>("amount",NW.amount),//user
                        new KeyValuePair<string, string>("da",destinationTestNumber),//default
                        new KeyValuePair<string, string>("pin",NW.TPin),//User
                        new KeyValuePair<string, string>("note", "Execute "+ NW.Remarks),//User
                        new KeyValuePair<string, string>("src","http"), ////default
                        new KeyValuePair<string, string>("tokenID",tokenID),//default
                        new KeyValuePair<string, string>("companyCode", "585"),//default
                        new KeyValuePair<string, string>("serviceCode", S_ServiceCode),//default
                        new KeyValuePair<string, string>("account",  regobj.CustomerID),//user
                        new KeyValuePair<string, string>("special1", regobj.CustomerID),//user
                        new KeyValuePair<string, string>("special2", ""),//user
                        new KeyValuePair<string, string>("tid", tid),//default
                        new KeyValuePair<string, string>("amountpay", regobj.TotalAmountDue),//database
                        new KeyValuePair<string, string>("refStan", regobj.refStan),//Database
                        new KeyValuePair<string, string>("billNumber", regobj.billNumber),//Database
                        new KeyValuePair<string, string>("rltCheckPaymt", regobj.responseCode),//Database
                        new KeyValuePair<string, string>("ClientCode", clientCode),
                        new KeyValuePair<string, string>("paypointType", "NTC TopUp"),
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
                        //Session value remove
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
        #endregion

        #region GET: NTC CDMA Voucher
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

                ViewBag.NTCCDMAServices = PaypointUtils.GetNTCCDMAServices();
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

        #region "POST: NTCCDMA CheckPayment"
        [HttpPost]
        public async Task<ActionResult> NTCCDMACheck(ISP iSP)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;



            this.ViewData["userType"] = this.TempData["userType"];
            ViewBag.UserType = this.TempData["userType"];
            ViewBag.Name = name;


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

            Session["CustomerID"] = iSP.CustomerID;
            Session["Amount"] = iSP.amount;
            Session["ServiceCode"] = iSP.ServiceCode;

            //api call here
            HttpResponseMessage _res = new HttpResponseMessage();
            string mobile = userName; //mobile is username
            TraceIdGenerator _tig = new TraceIdGenerator();
            var tid = _tig.GenerateTraceID();
            string tokenID = Session["TokenID"].ToString();
            using (HttpClient client = new HttpClient())
            {
                var action = "Utility.svc/NTCCDMAcheckpayment";
                var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);

                var content = new FormUrlEncodedContent(new[]{
                        new KeyValuePair<string, string>("vid", "130"),
                        new KeyValuePair<string, string>("mobile", mobile),
                        new KeyValuePair<string, string>("src","http"),
                        new KeyValuePair<string, string>("tokenID",tokenID),
                        new KeyValuePair<string, string>("companyCode", "588"),
                        new KeyValuePair<string, string>("serviceCode", iSP.ServiceCode),
                        new KeyValuePair<string, string>("account", iSP.CustomerID),
                        new KeyValuePair<string, string>("special1",""),
                        new KeyValuePair<string, string>("special2",""),
                        new KeyValuePair<string, string>("tid", tid),
                        new KeyValuePair<string, string>("ClientCode", clientCode),
                        new KeyValuePair<string, string>("paypointType", "NTC CDMA"),


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
        #endregion

        #region "GET: NTC CDMA Details"
        public ActionResult NTCCDMADetails()
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


                string S_CustomerID = (string)Session["CustomerID"];
                string S_ServiceCode = (string)Session["ServiceCode"];
                string serviceName = "";
               
                if ((S_CustomerID == null) && (S_ServiceCode == null))
                {
                    return RedirectToAction("Index");
                }
                ISP iSP = new ISP();

                iSP.CustomerID = S_CustomerID;
                iSP.UserName = userName;
                iSP.ClientCode = clientCode;
                iSP.refStan = getrefStan(iSP);
                MNPayPointISPPayments regobj = new MNPayPointISPPayments();

                DataSet DPaypointSet = PaypointUtils.GetNTCCDMADetails(iSP);
                DataTable dResponse = DPaypointSet.Tables["dtResponse"];
                DataTable dDHPayment = DPaypointSet.Tables["dtNWPayment"];

                List<ISP> ListDetails = new List<ISP>();
                if (dResponse != null && dResponse.Rows.Count > 0)
                {
                    regobj.CustomerID = dResponse.Rows[0]["account"].ToString();
                    regobj.TotalAmountDue = dResponse.Rows[0]["amount"].ToString();
                    regobj.NWBranchCode = dResponse.Rows[0]["serviceCode"].ToString();
                    if (dDHPayment != null && dDHPayment.Rows.Count > 0)
                    {
                        regobj.billAmount =(Convert.ToInt32(dDHPayment.Rows[0]["BillAmount"].ToString())/100).ToString();
                        regobj.billDate = dDHPayment.Rows[0]["BillDate"].ToString();

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

                ViewBag.ServiceNumber = regobj.CustomerID;
                ViewBag.ServiceName = serviceName;
                ViewBag.billDate = regobj.billDate;
                ViewBag.Amount = regobj.billAmount;

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

        #region "POST: NTC CDMA ExecutePayment"
        [HttpPost]
        public async Task<ActionResult> NTCCDMAExecutePayment(NepalWater NW)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            string S_ServiceCode = (string)Session["ServiceCode"];

            TempData["userType"] = userType;



            this.ViewData["userType"] = this.TempData["userType"];
            ViewBag.UserType = this.TempData["userType"];
            ViewBag.Name = name;


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


            ISP iSP = new ISP();
            iSP.NWCounter = S_NWCounter;
            iSP.CustomerID = S_CustomerID;
            iSP.UserName = userName;
            iSP.ClientCode = clientCode;
            iSP.refStan = getrefStan(iSP);
            NepalWater regobj = new NepalWater();

            DataSet DPaypointSet = PaypointUtils.GetDishHomeDetails(iSP);
            DataTable dResponse = DPaypointSet.Tables["dtResponse"];
            DataTable dNWPayment = DPaypointSet.Tables["dtNWPayment"];
            if (dResponse != null && dResponse.Rows.Count > 0)
            {
                regobj.CustomerID = dResponse.Rows[0]["account"].ToString();
                regobj.CustomerName = dResponse.Rows[0]["customerName"].ToString();
                regobj.TotalAmountDue = NW.amount;
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
                var destinationTestNumber = System.Configuration.ConfigurationManager.AppSettings["DestinationTestNumber"];
                var destinationMerchantId = System.Configuration.ConfigurationManager.AppSettings["DestinationMerchantIdPaypoint"];
                var action = "Utility.svc/NTCCDMAExecutePayment";
                var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                string tokenID = Session["TokenID"].ToString();
                var content = new FormUrlEncodedContent(new[]{
                        new KeyValuePair<string, string>("vid", destinationMerchantId),//default
                        new KeyValuePair<string, string>("sc",NW.TransactionMedium),//user 00 10
                        new KeyValuePair<string, string>("mobile", mobile),
                        new KeyValuePair<string, string>("amount",NW.amount),//user
                        new KeyValuePair<string, string>("da",destinationTestNumber),//default
                        new KeyValuePair<string, string>("pin",NW.TPin),//User
                        new KeyValuePair<string, string>("note", "Execute "+ NW.Remarks),//User
                        new KeyValuePair<string, string>("src","http"), ////default
                        new KeyValuePair<string, string>("tokenID",tokenID),//default
                        new KeyValuePair<string, string>("companyCode", "588"),//default
                        new KeyValuePair<string, string>("serviceCode", S_ServiceCode),//default
                        new KeyValuePair<string, string>("account",  regobj.CustomerID),//user
                        new KeyValuePair<string, string>("special1", regobj.CustomerID),//user
                        new KeyValuePair<string, string>("special2", ""),//user
                        new KeyValuePair<string, string>("tid", tid),//default
                        new KeyValuePair<string, string>("amountpay", regobj.TotalAmountDue),//database
                        new KeyValuePair<string, string>("refStan", regobj.refStan),//Database
                        new KeyValuePair<string, string>("billNumber", regobj.billNumber),//Database
                        new KeyValuePair<string, string>("rltCheckPaymt", regobj.responseCode),//Database
                        new KeyValuePair<string, string>("ClientCode", clientCode),
                        new KeyValuePair<string, string>("paypointType", "NTC TopUp"),
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
                        //Session value remove
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
        #endregion

        #region GET: SmartCell TopUp
        public ActionResult SmartCellTopUp()
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

                ViewBag.SmartCellTopUpServices = PaypointUtils.GetSmartCellServices();
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

        #region "POST: SmartCell CheckPayment"
        [HttpPost]
        public async Task<ActionResult> SmartCellTopUpCheck(ISP iSP)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;



            this.ViewData["userType"] = this.TempData["userType"];
            ViewBag.UserType = this.TempData["userType"];
            ViewBag.Name = name;


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

            Session["CustomerID"] = iSP.CustomerID;
            Session["Amount"] = iSP.amount;
            Session["ServiceCode"] = iSP.ServiceCode;

            //api call here
            HttpResponseMessage _res = new HttpResponseMessage();
            string mobile = userName; //mobile is username
            TraceIdGenerator _tig = new TraceIdGenerator();
            var tid = _tig.GenerateTraceID();
            string tokenID = Session["TokenID"].ToString();
            using (HttpClient client = new HttpClient())
            {
                var action = "Utility.svc/SmartCellcheckpayment";
                var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);

                var content = new FormUrlEncodedContent(new[]{
                        new KeyValuePair<string, string>("vid", "130"),
                        new KeyValuePair<string, string>("mobile", mobile),
                        new KeyValuePair<string, string>("src","http"),
                        new KeyValuePair<string, string>("tokenID",tokenID),
                        new KeyValuePair<string, string>("companyCode", "709"),
                        new KeyValuePair<string, string>("serviceCode", iSP.ServiceCode),
                        new KeyValuePair<string, string>("account", iSP.CustomerID),
                        new KeyValuePair<string, string>("special1",""),
                        new KeyValuePair<string, string>("special2",""),
                        new KeyValuePair<string, string>("tid", tid),
                        new KeyValuePair<string, string>("ClientCode", clientCode),
                        new KeyValuePair<string, string>("paypointType", "SmartCell TopUp"),


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
        #endregion

        #region "GET: SmartCell TopUp Details"
        public ActionResult SmartCellTopUpDetails()
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


                string S_CustomerID = (string)Session["CustomerID"];
                string S_ServiceCode = (string)Session["ServiceCode"];
                string serviceName = "";

                if ((S_CustomerID == null) && (S_ServiceCode == null))
                {
                    return RedirectToAction("Index");
                }
                ISP iSP = new ISP();

                iSP.CustomerID = S_CustomerID;
                iSP.UserName = userName;
                iSP.ClientCode = clientCode;
                iSP.refStan = getrefStan(iSP);
                MNPayPointISPPayments regobj = new MNPayPointISPPayments();

                DataSet DPaypointSet = PaypointUtils.GetSmartCellTopUpDetails(iSP);
                DataTable dResponse = DPaypointSet.Tables["dtResponse"];
                DataTable dDHPayment = DPaypointSet.Tables["dtNWPayment"];

                List<ISP> ListDetails = new List<ISP>();
                if (dResponse != null && dResponse.Rows.Count > 0)
                {
                    regobj.CustomerID = dResponse.Rows[0]["account"].ToString();
                    regobj.TotalAmountDue = dResponse.Rows[0]["amount"].ToString();
                    regobj.NWBranchCode = dResponse.Rows[0]["serviceCode"].ToString();
                    if (dDHPayment != null && dDHPayment.Rows.Count > 0)
                    {
                        regobj.billAmount = (Convert.ToInt32(dDHPayment.Rows[0]["BillAmount"].ToString()) / 100).ToString();
                        regobj.billDate = dDHPayment.Rows[0]["BillDate"].ToString();

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

                ViewBag.ServiceNumber = regobj.CustomerID;
                ViewBag.ServiceName = serviceName;
                ViewBag.billDate = regobj.billDate;
                ViewBag.Amount = regobj.billAmount;

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

        #region "POST: SmartCell ExecutePayment"
        [HttpPost]
        public async Task<ActionResult> SmartCellExecutePayment(NepalWater NW)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            string S_ServiceCode = (string)Session["ServiceCode"];

            TempData["userType"] = userType;



            this.ViewData["userType"] = this.TempData["userType"];
            ViewBag.UserType = this.TempData["userType"];
            ViewBag.Name = name;


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


            ISP iSP = new ISP();
            iSP.NWCounter = S_NWCounter;
            iSP.CustomerID = S_CustomerID;
            iSP.UserName = userName;
            iSP.ClientCode = clientCode;
            iSP.refStan = getrefStan(iSP);
            NepalWater regobj = new NepalWater();

            DataSet DPaypointSet = PaypointUtils.GetDishHomeDetails(iSP);
            DataTable dResponse = DPaypointSet.Tables["dtResponse"];
            DataTable dNWPayment = DPaypointSet.Tables["dtNWPayment"];
            if (dResponse != null && dResponse.Rows.Count > 0)
            {
                regobj.CustomerID = dResponse.Rows[0]["account"].ToString();
                regobj.CustomerName = dResponse.Rows[0]["customerName"].ToString();
                regobj.TotalAmountDue = NW.amount;
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
                var destinationTestNumber = System.Configuration.ConfigurationManager.AppSettings["DestinationTestNumber"];
                var destinationMerchantId = System.Configuration.ConfigurationManager.AppSettings["DestinationMerchantIdPaypoint"];
                var action = "Utility.svc/SmartCellTopUpExecutePayment";
                var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                string tokenID = Session["TokenID"].ToString();
                var content = new FormUrlEncodedContent(new[]{
                        new KeyValuePair<string, string>("vid", destinationMerchantId),//default
                        new KeyValuePair<string, string>("sc",NW.TransactionMedium),//user 00 10
                        new KeyValuePair<string, string>("mobile", mobile),
                        new KeyValuePair<string, string>("amount",NW.amount),//user
                        new KeyValuePair<string, string>("da",destinationTestNumber),//default
                        new KeyValuePair<string, string>("pin",NW.TPin),//User
                        new KeyValuePair<string, string>("note", "Execute "+ NW.Remarks),//User
                        new KeyValuePair<string, string>("src","http"), ////default
                        new KeyValuePair<string, string>("tokenID",tokenID),//default
                        new KeyValuePair<string, string>("companyCode", "709"),//default
                        new KeyValuePair<string, string>("serviceCode", S_ServiceCode),//default
                        new KeyValuePair<string, string>("account",  regobj.CustomerID),//user
                        new KeyValuePair<string, string>("special1", regobj.CustomerID),//user
                        new KeyValuePair<string, string>("special2", ""),//user
                        new KeyValuePair<string, string>("tid", tid),//default
                        new KeyValuePair<string, string>("amountpay", regobj.TotalAmountDue),//database
                        new KeyValuePair<string, string>("refStan", regobj.refStan),//Database
                        new KeyValuePair<string, string>("billNumber", regobj.billNumber),//Database
                        new KeyValuePair<string, string>("rltCheckPaymt", regobj.responseCode),//Database
                        new KeyValuePair<string, string>("ClientCode", clientCode),
                        new KeyValuePair<string, string>("paypointType", "SmartCell TopUp"),
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
                        //Session value remove
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
        #endregion

        #region GET: SmartCell EPIN
        public ActionResult SmartCellEPIN()
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

                ViewBag.SmartCellTopUpServices = PaypointUtils.GetSmartCellEPINServices();
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

        #region "POST: SmartCell EPIN CheckPayment"
        [HttpPost]
        public async Task<ActionResult> SmartCellEPINCheck(ISP iSP)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;



            this.ViewData["userType"] = this.TempData["userType"];
            ViewBag.UserType = this.TempData["userType"];
            ViewBag.Name = name;


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

            Session["CustomerID"] = iSP.CustomerID;
            Session["Amount"] = iSP.amount;
            Session["ServiceCode"] = iSP.ServiceCode;

            //api call here
            HttpResponseMessage _res = new HttpResponseMessage();
            string mobile = userName; //mobile is username
            TraceIdGenerator _tig = new TraceIdGenerator();
            var tid = _tig.GenerateTraceID();
            string tokenID = Session["TokenID"].ToString();
            using (HttpClient client = new HttpClient())
            {
                var action = "Utility.svc/SmartCellEPINcheckpayment";
                var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);

                var content = new FormUrlEncodedContent(new[]{
                        new KeyValuePair<string, string>("vid", "130"),
                        new KeyValuePair<string, string>("mobile", mobile),
                        new KeyValuePair<string, string>("src","http"),
                        new KeyValuePair<string, string>("tokenID",tokenID),
                        new KeyValuePair<string, string>("companyCode", "587"),
                        new KeyValuePair<string, string>("serviceCode", iSP.ServiceCode),
                        new KeyValuePair<string, string>("account", iSP.CustomerID),
                        new KeyValuePair<string, string>("special1",""),
                        new KeyValuePair<string, string>("special2",""),
                        new KeyValuePair<string, string>("tid", tid),
                        new KeyValuePair<string, string>("ClientCode", clientCode),
                        new KeyValuePair<string, string>("paypointType", "SmartCell EPIN"),


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
        #endregion

        #region "GET: SmartCell EPIN Details"
        public ActionResult SmartCellEPINCheckDetails()
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


                string S_CustomerID = (string)Session["CustomerID"];
                string S_ServiceCode = (string)Session["ServiceCode"];
                string serviceName = "";

                if ((S_CustomerID == null) && (S_ServiceCode == null))
                {
                    return RedirectToAction("Index");
                }
                ISP iSP = new ISP();

                iSP.CustomerID = S_CustomerID;
                iSP.UserName = userName;
                iSP.ClientCode = clientCode;
                iSP.refStan = getrefStan(iSP);
                MNPayPointISPPayments regobj = new MNPayPointISPPayments();

                DataSet DPaypointSet = PaypointUtils.GetSmartCellEPINDetails(iSP);
                DataTable dResponse = DPaypointSet.Tables["dtResponse"];
                DataTable dDHPayment = DPaypointSet.Tables["dtNWPayment"];

                List<ISP> ListDetails = new List<ISP>();
                if (dResponse != null && dResponse.Rows.Count > 0)
                {
                    regobj.CustomerID = dResponse.Rows[0]["account"].ToString();
                    regobj.TotalAmountDue = dResponse.Rows[0]["amount"].ToString();
                    regobj.NWBranchCode = dResponse.Rows[0]["serviceCode"].ToString();
                    if (dDHPayment != null && dDHPayment.Rows.Count > 0)
                    {
                        regobj.billAmount = (Convert.ToInt32(dDHPayment.Rows[0]["BillAmount"].ToString()) / 100).ToString();
                        regobj.billDate = dDHPayment.Rows[0]["BillDate"].ToString();

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

                ViewBag.ServiceNumber = regobj.CustomerID;
                ViewBag.ServiceName = serviceName;
                ViewBag.billDate = regobj.billDate;
                ViewBag.Amount = regobj.billAmount;

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

        #region "POST: SmartCell EPIN ExecutePayment"
        [HttpPost]
        public async Task<ActionResult> SmartCellEPINExecutePayment(NepalWater NW)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            string S_ServiceCode = (string)Session["ServiceCode"];

            TempData["userType"] = userType;



            this.ViewData["userType"] = this.TempData["userType"];
            ViewBag.UserType = this.TempData["userType"];
            ViewBag.Name = name;


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


            ISP iSP = new ISP();
            iSP.NWCounter = S_NWCounter;
            iSP.CustomerID = S_CustomerID;
            iSP.UserName = userName;
            iSP.ClientCode = clientCode;
            iSP.refStan = getrefStan(iSP);
            NepalWater regobj = new NepalWater();

            DataSet DPaypointSet = PaypointUtils.GetDishHomeDetails(iSP);
            DataTable dResponse = DPaypointSet.Tables["dtResponse"];
            DataTable dNWPayment = DPaypointSet.Tables["dtNWPayment"];
            if (dResponse != null && dResponse.Rows.Count > 0)
            {
                regobj.CustomerID = dResponse.Rows[0]["account"].ToString();
                regobj.CustomerName = dResponse.Rows[0]["customerName"].ToString();
                regobj.TotalAmountDue = NW.amount;
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
                var destinationTestNumber = System.Configuration.ConfigurationManager.AppSettings["DestinationTestNumber"];
                var destinationMerchantId = System.Configuration.ConfigurationManager.AppSettings["DestinationMerchantIdPaypoint"];
                var action = "Utility.svc/SmartCellEPINExecutePayment";
                var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                string tokenID = Session["TokenID"].ToString();
                var content = new FormUrlEncodedContent(new[]{
                        new KeyValuePair<string, string>("vid", destinationMerchantId),//default
                        new KeyValuePair<string, string>("sc",NW.TransactionMedium),//user 00 10
                        new KeyValuePair<string, string>("mobile", mobile),
                        new KeyValuePair<string, string>("amount",NW.amount),//user
                        new KeyValuePair<string, string>("da",destinationTestNumber),//default
                        new KeyValuePair<string, string>("pin",NW.TPin),//User
                        new KeyValuePair<string, string>("note", "Execute "+ NW.Remarks),//User
                        new KeyValuePair<string, string>("src","http"), ////default
                        new KeyValuePair<string, string>("tokenID",tokenID),//default
                        new KeyValuePair<string, string>("companyCode", "587"),//default
                        new KeyValuePair<string, string>("serviceCode", S_ServiceCode),//default
                        new KeyValuePair<string, string>("account",  regobj.CustomerID),//user
                        new KeyValuePair<string, string>("special1", regobj.CustomerID),//user
                        new KeyValuePair<string, string>("special2", ""),//user
                        new KeyValuePair<string, string>("tid", tid),//default
                        new KeyValuePair<string, string>("amountpay", regobj.TotalAmountDue),//database
                        new KeyValuePair<string, string>("refStan", regobj.refStan),//Database
                        new KeyValuePair<string, string>("billNumber", regobj.billNumber),//Database
                        new KeyValuePair<string, string>("rltCheckPaymt", regobj.responseCode),//Database
                        new KeyValuePair<string, string>("ClientCode", clientCode),
                        new KeyValuePair<string, string>("paypointType", "SmartCell EPIN"),
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
                        //Session value remove
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
        #endregion

        //UTL
        #region GET: UTL
        public ActionResult UTL()
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

                ViewBag.DHServiceCode = PaypointUtils.GetUTLServiceCode();
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

        #region "POST: UTL CheckPayment"
        [HttpPost]

        public async Task<ActionResult> UTLCheckPayment(ISP iSP)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;



            this.ViewData["userType"] = this.TempData["userType"];
            ViewBag.UserType = this.TempData["userType"];
            ViewBag.Name = name;


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

            Session["CustomerID"] = iSP.CustomerID;

            //api call here
            HttpResponseMessage _res = new HttpResponseMessage();
            string mobile = userName; //mobile is username
            TraceIdGenerator _tig = new TraceIdGenerator();
            var tid = _tig.GenerateTraceID();
            string tokenID = Session["TokenID"].ToString();
            using (HttpClient client = new HttpClient())
            {
                var action = "Utility.svc/UTLCheckpayment";
                var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);

                var content = new FormUrlEncodedContent(new[]{
                        new KeyValuePair<string, string>("vid", "130"),
                        new KeyValuePair<string, string>("mobile", mobile),
                        new KeyValuePair<string, string>("src","http"),
                        new KeyValuePair<string, string>("tokenID",tokenID),
                        new KeyValuePair<string, string>("companyCode", "582"),
                        new KeyValuePair<string, string>("serviceCode", "0"),
                        new KeyValuePair<string, string>("account", iSP.CustomerID),
                        new KeyValuePair<string, string>("special1", iSP.amount),
                        new KeyValuePair<string, string>("special2",""),
                        new KeyValuePair<string, string>("tid", tid),
                        new KeyValuePair<string, string>("ClientCode", clientCode),
                        new KeyValuePair<string, string>("paypointType", "UTL"),


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
        #endregion

        #region "GET: UTL Details"
        public ActionResult UTLDetails()
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

                string S_CustomerID = (string)Session["CustomerID"];
                string S_DHServiceCode = (string)Session["DHServiceCode"];
                if ((S_CustomerID == null))
                {
                    return RedirectToAction("UTL");
                }
                ISP iSP = new ISP();

                iSP.CustomerID = S_CustomerID;
                iSP.UserName = userName;
                iSP.ClientCode = clientCode;
                iSP.refStan = getrefStan(iSP);

                ISP regobj = new ISP();
                DataSet DPaypointSet = PaypointUtils.GetUTLDetails(iSP);
                DataTable dResponse = DPaypointSet.Tables["dtResponse"];
                DataTable dDHPayment = DPaypointSet.Tables["dtNWPayment"];


                if (dResponse != null && dResponse.Rows.Count > 0)
                {
                    regobj.CustomerID = dResponse.Rows[0]["account"].ToString();
                    regobj.TotalAmountDue = dResponse.Rows[0]["amount"].ToString();
                    regobj.DHServiceCode = dResponse.Rows[0]["serviceCode"].ToString();
                    if (dDHPayment != null && dDHPayment.Rows.Count > 0)
                    {
                        //regobj.Description = dDHPayment.Rows[0]["Descriptions"].ToString();
                        //regobj.amount = dDHPayment.Rows[0]["PackageAmount"].ToString();
                        //regobj.Bonus = dDHPayment.Rows[0]["Bonus"].ToString();


                        //string[] lines = regobj.Description.Split(new[] { Environment.NewLine }, StringSplitOptions.None); //to split string to new line
                        //lines = lines.Take(lines.Length - 1).ToArray();  //to remove last list which is empty 

                        //string[] lines1 = regobj.amount.Split(new[] { Environment.NewLine }, StringSplitOptions.None); //to split string to new line
                        //lines1 = lines1.Take(lines1.Length - 1).ToArray();  //to remove last list which is empty 


                        //string[] lines2 = regobj.Bonus.Split(new[] { Environment.NewLine }, StringSplitOptions.None); //to split string to new line
                        //lines2 = lines2.Take(lines2.Length - 1).ToArray();  //to remove last list which is empty 



                        regobj.BillDate = dDHPayment.Rows[0]["billDate"].ToString();
                        regobj.amount = dDHPayment.Rows[0]["billAmount"].ToString();
                    }
                    else
                    {
                        return RedirectToAction("UTL");
                    }
                }
                else
                {
                    return RedirectToAction("UTL");
                }

                ViewBag.CustomerID = regobj.CustomerID;
                ViewBag.BillAmount = (Convert.ToInt32(regobj.amount) / 100).ToString();
                ViewBag.billDate = regobj.BillDate.ToString();

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

        #region "POST: UTL ExecutePayment"
        [HttpPost]
        public async Task<ActionResult> UTLExecutePayment(NepalWater NW)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;



            this.ViewData["userType"] = this.TempData["userType"];
            ViewBag.UserType = this.TempData["userType"];
            ViewBag.Name = name;


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


            ISP iSP = new ISP();
            iSP.NWCounter = S_NWCounter;
            iSP.CustomerID = S_CustomerID;
            iSP.UserName = userName;
            iSP.ClientCode = clientCode;
            iSP.refStan = getrefStan(iSP);
            NepalWater regobj = new NepalWater();

            DataSet DPaypointSet = PaypointUtils.GetDishHomeDetails(iSP);
            DataTable dResponse = DPaypointSet.Tables["dtResponse"];
            DataTable dNWPayment = DPaypointSet.Tables["dtNWPayment"];
            if (dResponse != null && dResponse.Rows.Count > 0)
            {
                regobj.CustomerID = dResponse.Rows[0]["account"].ToString();
                regobj.CustomerName = dResponse.Rows[0]["customerName"].ToString();
                regobj.TotalAmountDue = NW.amount;
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
                var destinationTestNumber = System.Configuration.ConfigurationManager.AppSettings["DestinationTestNumber"];
                var destinationMerchantId = System.Configuration.ConfigurationManager.AppSettings["DestinationMerchantIdPaypoint"];
                var action = "Utility.svc/NCellExecutePayment";
                var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                string tokenID = Session["TokenID"].ToString();
                var content = new FormUrlEncodedContent(new[]{
                        new KeyValuePair<string, string>("vid", destinationMerchantId),//default
                        new KeyValuePair<string, string>("sc",NW.TransactionMedium),//user 00 10
                        new KeyValuePair<string, string>("mobile", mobile),
                        new KeyValuePair<string, string>("amount",NW.amount),//user
                        new KeyValuePair<string, string>("da",destinationTestNumber),//default
                        new KeyValuePair<string, string>("pin",NW.TPin),//User
                        new KeyValuePair<string, string>("note", "Execute "+ NW.Remarks),//User
                        new KeyValuePair<string, string>("src","http"), ////default
                        new KeyValuePair<string, string>("tokenID",tokenID),//default
                        new KeyValuePair<string, string>("companyCode", "78"),//default
                        new KeyValuePair<string, string>("serviceCode", "0"),//default
                        new KeyValuePair<string, string>("account",  regobj.CustomerID),//user
                        new KeyValuePair<string, string>("special1", regobj.CustomerID),//user
                        new KeyValuePair<string, string>("special2", ""),//user
                        new KeyValuePair<string, string>("tid", tid),//default
                        new KeyValuePair<string, string>("amountpay", regobj.TotalAmountDue),//database
                        new KeyValuePair<string, string>("refStan", regobj.refStan),//Database
                        new KeyValuePair<string, string>("billNumber", regobj.billNumber),//Database
                        new KeyValuePair<string, string>("rltCheckPaymt", regobj.responseCode),//Database
                        new KeyValuePair<string, string>("ClientCode", clientCode),
                        new KeyValuePair<string, string>("paypointType", "NCell TopUp"),
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
                        //Session value remove
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
        #endregion



        #region Get refStan From Response Table
        public string getrefStan(ISP iSP)
        {
            string Query_refStan = "select refStan from MNPaypointResponse where account='" + iSP.CustomerID + "' AND ClientCode='" + iSP.ClientCode + "' AND UserName='" + iSP.UserName + "'";
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