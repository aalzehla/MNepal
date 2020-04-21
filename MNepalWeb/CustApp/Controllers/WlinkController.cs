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
    public class WlinkController : Controller
    {
        DAL objdal = new DAL();
        // GET: Wlink
        #region Get Wlink
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

                //ViewBag.NepalWater = PaypointUtils.GetNepalWaterName();
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

             

        #region "POST: Wlink CheckPayment"
        [HttpPost]

        public async Task<ActionResult> WlinkCheckPayment(ISP isp)
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

            Session["CustomerName"] = isp.CustomerName;
            //api call here

            HttpResponseMessage _res = new HttpResponseMessage();
            string mobile = userName; //mobile is username
            TraceIdGenerator _tig = new TraceIdGenerator();
            var tid = _tig.GenerateTraceID();
            string tokenID = Session["TokenID"].ToString();

            using (HttpClient client = new HttpClient())
            {
                var action = "wlink.svc/checkpayment";
                var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);

                var content = new FormUrlEncodedContent(new[]{
                        new KeyValuePair<string, string>("vid", "130"),
                        new KeyValuePair<string, string>("mobile", mobile),
                        new KeyValuePair<string, string>("src","http"),
                        new KeyValuePair<string, string>("tokenID",tokenID),
                        new KeyValuePair<string, string>("companyCode", "597"),
                        new KeyValuePair<string, string>("serviceCode", "0"),
                        new KeyValuePair<string, string>("account", isp.CustomerName),
                        new KeyValuePair<string, string>("special1",""),
                        new KeyValuePair<string, string>("special2",""),
                        new KeyValuePair<string, string>("tid", tid),
                        new KeyValuePair<string, string>("ClientCode", clientCode),
                        new KeyValuePair<string, string>("paypointType", "Worldlink"),


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

        #region "GET: Wlink Details"
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
                //string S_NWCounter = (string)Session["NWCounter"];
                string S_CustomerName = (string)Session["CustomerName"];
                if ((S_CustomerName == null))
                {
                    return RedirectToAction("Index");
                }

                ISP iSP = new ISP();
                //NWObj.NWCounter = S_NWCounter;
                iSP.CustomerName = S_CustomerName;
                iSP.UserName = userName;
                iSP.ClientCode = clientCode;
                iSP.refStan = getrefStan(iSP);

                MNPayPointISPPayments regobj = new MNPayPointISPPayments();
                DataSet DPaypointSet = PaypointUtils.GetWlinkDetails(iSP);
                DataTable dResponse = DPaypointSet.Tables["dtResponse"];
                DataTable dWlinkPayment = DPaypointSet.Tables["dtNWPayment"];

                List<ISP> ListDetails = new List<ISP>();
                if (dResponse != null && dResponse.Rows.Count > 0)
                {
                    regobj.CustomerID = dResponse.Rows[0]["account"].ToString();
                    regobj.CustomerName = dResponse.Rows[0]["customerName"].ToString();
                    regobj.TotalAmountDue = dResponse.Rows[0]["amount"].ToString();
                    regobj.NWBranchCode = dResponse.Rows[0]["serviceCode"].ToString();
                    regobj.payPointType = dResponse.Rows[0]["paypointType"].ToString();
                    regobj.description = dResponse.Rows[0]["description"].ToString();

                    if (dWlinkPayment != null && dWlinkPayment.Rows.Count > 0)
                    {
                        regobj.billDate = dWlinkPayment.Rows[0]["billDate"].ToString();
                        regobj.billAmount = dWlinkPayment.Rows[0]["billAmount"].ToString();
                        regobj.PackageRemainingDays = dWlinkPayment.Rows[0]["RemainingDays"].ToString();

                        if (dWlinkPayment.Rows[0]["PackageAmount"].ToString()!=null && dWlinkPayment.Rows[0]["PackageId"].ToString()!=null &&
                            dWlinkPayment.Rows[0]["Descriptions"].ToString()!=null)
                        {
                            try
                            {
                                regobj.PackageAmount = dWlinkPayment.Rows[0]["PackageAmount"].ToString();
                                regobj.PackageId = dWlinkPayment.Rows[0]["PackageId"].ToString();
                                regobj.PackageId = dWlinkPayment.Rows[0]["Descriptions"].ToString();


                                string[] lines = regobj.description.Split(new[] { Environment.NewLine }, StringSplitOptions.None); //to split string to new line
                                lines = lines.Take(lines.Length - 1).ToArray();  //to remove last list which is empty 

                                string[] lines1 = regobj.PackageAmount.Split(new[] { Environment.NewLine }, StringSplitOptions.None); //to split string to new line
                                lines1 = lines1.Take(lines1.Length - 1).ToArray();  //to remove last list which is empty 


                                string[] lines2 = regobj.PackageId.Split(new[] { Environment.NewLine }, StringSplitOptions.None); //to split string to new line
                                lines2 = lines2.Take(lines2.Length - 1).ToArray();  //to remove last list which is empty 


                                //List<string> list = new List<string>(lines);

                                for (int i = 0; i < lines.Length; i++)
                                {
                                    string Packages = lines[i];
                                    ListDetails.Add(new ISP
                                    {
                                        Description = Packages,
                                        PackageAmount = lines1[i],
                                        PackageId = lines2[i]

                                    });
                                }

                            }
                            catch (Exception ex)
                            {

                                throw ex;
                            }
                        }


                        ViewBag.ListDetails = ListDetails;

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
                ViewBag.NWBranchCode = regobj.NWBranchCode.ToString();
                ViewBag.CustomerID = regobj.CustomerID;
                ViewBag.TotalAmountDue = regobj.TotalAmountDue;

                //Viewbag For details from Wlink
                ViewBag.description = regobj.description.ToString();
                ViewBag.CustomerName = regobj.CustomerName;
                ViewBag.billDate = regobj.billDate.ToString();
                ViewBag.paypointType = regobj.payPointType.ToString();
                ViewBag.remainingDays = regobj.PackageRemainingDays.ToString();


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

        #region "POST: Wlink ExecutePayment"
        [HttpPost]
        public async Task<ActionResult> WlinkExecutePayment(ISP iSP)
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
            string S_CustomerID = (string)Session["CustomerName"];

            ISP NWObj = new ISP();
            //NWObj.NWCounter = S_NWCounter;
            NWObj.CustomerName = S_CustomerID;
            NWObj.UserName = userName;
            NWObj.ClientCode = clientCode;
            NWObj.refStan = getrefStan(NWObj);
            ISP regobj = new ISP();

            DataSet DPaypointSet = PaypointUtils.GetWlinkDetails(NWObj);

            DataTable dResponse = DPaypointSet.Tables["dtResponse"];
            DataTable dNWPayment = DPaypointSet.Tables["dtNWPayment"];
            if (dResponse != null && dResponse.Rows.Count > 0)
            {
                regobj.CustomerID = dResponse.Rows[0]["account"].ToString();
                regobj.CustomerName = dResponse.Rows[0]["customerName"].ToString();
                regobj.TotalAmountDue = dResponse.Rows[0]["amount"].ToString();
                regobj.ServiceCode = dResponse.Rows[0]["serviceCode"].ToString();
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
                var action = "wlink.svc/executepayment";
                var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                string tokenID = Session["TokenID"].ToString();
                var content = new FormUrlEncodedContent(new[]{
                        new KeyValuePair<string, string>("vid", destinationMerchantId),//default
                        new KeyValuePair<string, string>("sc",iSP.TransactionMedium),//user 00 10
                        new KeyValuePair<string, string>("mobile", mobile),
                        new KeyValuePair<string, string>("amount",iSP.amount),//user
                        new KeyValuePair<string, string>("da",destinationTestNumber),//default
                        new KeyValuePair<string, string>("pin",iSP.TPin),//User
                        new KeyValuePair<string, string>("note", "Execute "+iSP.Remarks),//User
                        new KeyValuePair<string, string>("src","http"), ////default
                        new KeyValuePair<string, string>("tokenID",tokenID),//default
                        new KeyValuePair<string, string>("companyCode", "597"),//default
                        new KeyValuePair<string, string>("serviceCode", regobj.ServiceCode),//default
                        new KeyValuePair<string, string>("account",  regobj.CustomerID),//user
                        new KeyValuePair<string, string>("special1",""),//user
                        new KeyValuePair<string, string>("special2", ""),//user
                        new KeyValuePair<string, string>("tid", tid),//default
                        new KeyValuePair<string, string>("amountpay", regobj.TotalAmountDue),//database
                        new KeyValuePair<string, string>("refStan", regobj.refStan),//Database
                        new KeyValuePair<string, string>("billNumber", regobj.billNumber),//Database
                        new KeyValuePair<string, string>("rltCheckPaymt", regobj.responseCode),//Database
                        new KeyValuePair<string, string>("ClientCode", clientCode),
                        new KeyValuePair<string, string>("paypointType", "Worldlink"),
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

        #region Get Wlink refStan From Response Table
        public string getrefStan(ISP isp)
        {
            string Query_refStan = "select refStan from MNPaypointResponse where account='" + isp.CustomerName + "' AND ClientCode='" + isp.ClientCode + "' AND UserName='" + isp.UserName + "'";
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