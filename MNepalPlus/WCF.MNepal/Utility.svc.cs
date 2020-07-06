using MNepalProject.Controllers;
using MNepalProject.Helper;
using MNepalProject.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using WCF.MNepal.ErrorMsg;
using WCF.MNepal.Helper;
using WCF.MNepal.Models;
using WCF.MNepal.Utilities;

namespace WCF.MNepal
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class Utility
    {
        #region"Check NCell Payment"
        [OperationContract]        
        [WebInvoke(Method = "POST",
                  ResponseFormat = WebMessageFormat.Json)]
        public string Ncellcheckpayment(Stream input)
        {
            string PaypointPwd = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointPwd"];
            string PaypointUserID = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointUserID"];
            System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate { return true; }; //to prevent from SSL error
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            string tid = qs["tid"];
            string vid = qs["vid"];
            string sc = "00";
            string mobile = qs["mobile"];
            string da = System.Web.Configuration.WebConfigurationManager.AppSettings["DestinationNoForTestServer"];
            string note = "Utility payment for Ncell. Mobile Number=" + qs["account"];
            string src = qs["src"];
            string result = "";
            string sessionID = qs["tokenID"];
            string companyCode = qs["companyCode"];
            string serviceCode = qs["serviceCode"];
            string account = qs["account"];
            string special1 = qs["special1"];
            string special2 = "";
            string transactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            long millisecondstrandId = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string transactionId = millisecondstrandId.ToString();
            string userId = PaypointUserID;
            string userPassword = PaypointPwd.Trim();
            string salePointType = "6";
            string ClientCode = qs["ClientCode"];
            string paypointType = qs["paypointType"];
            string transactionType = string.Empty;
            string resultMessageResCP = "";

            PaypointModel reqCPPaypointNcellInfo = new PaypointModel();//to store data request of CP which is also commmon in nea
            PaypointModel resCPPaypointNcellInfo = new PaypointModel();//to store data response of CP which is also commmon in nea

            //PaypointModel resPaypointPaymentInfo = new PaypointModel();
            PaypointModel resPaypointNcellPaymentInfo = new PaypointModel();//to store data of Response of CP only

            string totalAmount = string.Empty;
            string totalCount = string.Empty;
            string totalBAmount = string.Empty;
            string totalBCount = string.Empty;
            string balance = string.Empty;
            string message = string.Empty;
            string statusCode = string.Empty;
            string failedmessage = string.Empty;
            string GetMerchantName = string.Empty;
            string retrievalRef = string.Empty;
            string refStanCK = string.Empty;

            string customerNo = string.Empty;
            TraceIdGenerator traceid = new TraceIdGenerator();
            tid = traceid.GenerateUniqueTraceID();

            //for CP transaction for nepal water
            try
            {
                string URI = System.Web.Configuration.WebConfigurationManager.AppSettings["CPPaypointUrl"];

                string myParameters = "companyCode=" + companyCode + "&serviceCode=" + serviceCode +
                    "&account=" + account + "&special1=" + special1 + "&special2=" + special2 +
                    "&transactionDate=" + transactionDate + "&transactionId=" + transactionId +
                    "&userId=" + userId.Trim() + "&userPassword=" + userPassword.Trim() + "&salePointType=" + salePointType;

                //for checkpayment request insert in database
                reqCPPaypointNcellInfo = new PaypointModel()
                {
                    companyCodeReqCP = companyCode,
                    serviceCodeReqCP = serviceCode,
                    accountReqCP = account,
                    special1ReqCP = special1,
                    special2ReqCP = special2,

                    transactionDateReqCP = transactionDate,
                    transactionIdReqCP = transactionId,
                    userIdReqCP = userId,
                    userPasswordReqCP = userPassword,
                    salePointTypeReqCP = salePointType,

                    refStanReqCP = "",
                    amountReqCP = "",//amountInPaisa
                    billNumberReqCP = "",
                    //retrievalReferenceReqCP = fundtransfer.tid,
                    retrievalReferenceReqCP = tid,
                    remarkReqCP = "Check Payment",
                    UserName = mobile,
                    ClientCode = ClientCode,
                    paypointType = paypointType,

                };

                string billNumber = "";
                string amountpay = "";
                string refStan = "";
                string exectransactionId = ""; //Unique
                string exectransactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"); //Current DateTime
                string rltCheckPaymt = "";
                string customerName = "";
                string mask = "";

                using (WebClient wc = new WebClient())
                {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    var HtmlResult = wc.UploadString(URI, myParameters);// response from checkpayment

                    XmlDocument xmlDoc = new XmlDocument();

                    xmlDoc.LoadXml(HtmlResult);

                    XmlNodeList test = xmlDoc.GetElementsByTagName("*");
                    string results = test[0].InnerText;
                    string HtmlResult1 = results;

                    //for getting key value from check payment
                    var reader = new StringReader(HtmlResult1);
                    var xdoc = XDocument.Load(reader);

                    XDocument docParse = XDocument.Parse(xdoc.ToString());
                    IEnumerable<XElement> responses = docParse.Descendants();

                    var xElem = XElement.Parse(docParse.ToString());

                    rltCheckPaymt = xElem.Attribute("Result").Value;
                    string keyrlt = "";// response.Attribute("Key").Value;
                    var billAmount = "";

                    message = resultMessageResCP;
                    if (xElem.Attribute("Result").Value == "000")
                    {
                        keyrlt = xElem.Attribute("Key").Value;
                        billNumber = xElem.Descendants().Elements("BillNumber").Where(x => x.Name == "BillNumber").SingleOrDefault().Value;
                        amountpay = xElem.Descendants().Elements("Amount").Where(x => x.Name == "Amount").SingleOrDefault().Value;
                        refStan = xElem.Descendants().Elements("RefStan").Where(x => x.Name == "RefStan").SingleOrDefault().Value;
                        exectransactionDate = xElem.Descendants().Elements("DueDate").Where(x => x.Name == "DueDate").SingleOrDefault().Value;
                        customerName = xElem.Descendants().Elements("ReserveInfo").Where(x => x.Name == "ReserveInfo").SingleOrDefault().Value;
                        mask = xElem.Descendants().Elements("mask").Where(x => x.Name == "mask").SingleOrDefault().Value;
                        if(amountpay == "0")
                        {
                            billAmount = special1;
                        }
                        else
                        {
                            billAmount = amountpay;
                        }

                        resPaypointNcellPaymentInfo = new PaypointModel()
                        {
                            billNumber = billNumber,
                            refStan = refStan,
                            amount = billAmount,
                            transactionDate = exectransactionDate,
                            customerName = account,
                            companyCode = companyCode,
                            UserName = mobile,
                            ClientCode = ClientCode,
                            serviceCode = serviceCode,
                            paypointType = paypointType

                        };



                        //end list of package

                        int resultsPayments = PaypointUtils.PaypointUtilityNCellInfo(resPaypointNcellPaymentInfo);
                    }
                    else
                    {
                        keyrlt = xElem.Attribute("Key").Value;
                        resultMessageResCP = xElem.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;
                    }
                    resultMessageResCP = xElem.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;

                    //for checkpayment response insert in database in nepal water
                    resCPPaypointNcellInfo = new PaypointModel()
                    {
                        companyCodeResCP = companyCode,
                        serviceCodeResCP = serviceCode,
                        accountResCP = account,
                        special1ResCP = special1,
                        special2ResCP = special2,

                        transactionDateResCP = transactionDate,
                        transactionIdResCP = transactionId,
                        userIdResCP = userId,
                        userPasswordResCP = userPassword,
                        salePointTypeResCP = salePointType,

                        refStanResCP = refStan,
                        amountResCP = amountpay,
                        billNumberResCP = billNumber,
                        //retrievalReferenceResCP = fundtransfer.tid,
                        retrievalReferenceResCP = tid,
                        responseCodeResCP = rltCheckPaymt,
                        descriptionResCP = "Check Payment " + keyrlt,
                        customerNameCP = customerName,
                        UserName = mobile,
                        ClientCode = ClientCode,
                        paypointType = paypointType,
                        resultMessageResCP = resultMessageResCP,

                    };
                }


                if (!(rltCheckPaymt == "000"))//show error when CP response is not 000 
                {
                    statusCode = "400";
                    // message = result;
                    message = resultMessageResCP;
                    failedmessage = message;

                }
                else
                {
                    statusCode = "200";
                    message = resultMessageResCP;
                    failedmessage = message;
                }
            }
            catch (Exception ex)
            {
                message = result + ex + "Error Message ";
                //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                statusCode = "400";
                // failedmessage = message;
                failedmessage = resultMessageResCP;

            }

            ///for  inserting CP PayPoint Data of Nepal Water which is simlar to nea 

            try
            {
                int resultsReqCP = PaypointUtils.RequestCPPaypointInfo(reqCPPaypointNcellInfo);
                int resultsResCP = PaypointUtils.ResponseCPPaypointInfo(resCPPaypointNcellInfo);


                if ((resultsReqCP > 0) && (resultsResCP > 0))

                {
                    message = result;
                }
                else
                {
                    message = result;
                }

            }
            catch (Exception ex)
            {
                string ss = ex.Message;
                message = result;
            }

            if (statusCode == "")
            {
                result = result.ToString();
            }
            else if (statusCode == "200")
            {
                if (message == "")
                {
                    if (string.IsNullOrEmpty(result))
                    {
                        result = failedmessage; // "Could not process your request,Please Try again.";
                    }
                    message = result;
                }
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = failedmessage,
                    retrievalRef = resCPPaypointNcellInfo.retrievalReferenceResCP,
                    refStanCK = resCPPaypointNcellInfo.refStanResCP,
                };
                result = JsonConvert.SerializeObject(v);
            }
            else if (statusCode != "200")
            {
                if (message == "")
                {
                    if (string.IsNullOrEmpty(result))
                    {
                        result = failedmessage; // "Could not process your request,Please Try again.";
                    }
                    message = result;
                }
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = failedmessage,
                    retrievalRef = "",
                    refStanCK = ""
                };
                result = JsonConvert.SerializeObject(v);
            }
            return result;

        }
        #endregion

        #region"NCell execute Paypoint"
        [OperationContract]
        [WebInvoke(Method = "POST",
                  ResponseFormat = WebMessageFormat.Json)]
        public string NCellExecutePayment(Stream input)
        {
            string PaypointPwd = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointPwd"];
            string PaypointUserID = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointUserID"];

            System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);
            MNTransactionMaster validTransactionData = new MNTransactionMaster();
            CustActivityModel custsmsInfo = new CustActivityModel();

            string tid = qs["tid"];
            string vid = qs["vid"]; //MerchantID
            string sc = qs["sc"];
            string mobile = qs["mobile"];
            string da = System.Web.Configuration.WebConfigurationManager.AppSettings["DestinationNoForTestServer"];


            string amount = qs["amount"];//amount paid by customer 
            string pin = qs["pin"];
            string note = "utility payment for NCell. Customer Name=" + qs["account"];//+ ". " + qs["note"];
            string src = qs["src"];
            string result = "";
            string sessionID = qs["tokenID"];
            string resultMessageResCP = "";
            string companyCode = qs["companyCode"];
            string serviceCode = qs["serviceCode"];
            string account = qs["account"];
            string special1 = qs["special1"]; //
            string special2 = ""; //
            string transactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");//"2019-11-22T11:11:02";
            long millisecondstrandId = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string transactionId = millisecondstrandId.ToString(); //"120163339819";
            string userId = PaypointUserID;
            string userPassword = PaypointPwd.Trim();
            string salePointType = "6";
            string transactionType = string.Empty;
            string amountpay = qs["amountpay"];//amount need to pay i.e amount in bill
            string exectransactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            string exectransactionId = millisecondstrandId.ToString();
            string refStan = qs["refStan"];
            string billNumber = qs["billNumber"];
            string rltCheckPaymt = qs["rltCheckPaymt"];
            string ClientCode = qs["ClientCode"];
            string paypointType = qs["paypointType"];
            //string paypointType = "Nepal Water";
            //string paypointType = "3";

            string customerName = qs["customerName"];
            string walletBalance = qs["walletBalance"];//in Rs.
            string retrievalReference = qs["retrievalReference"];
            int walletBalancePaisaInt = 0;
            walletBalancePaisaInt = Convert.ToInt32((Convert.ToDouble(walletBalance)) * 100);
            int amountpayInt = Convert.ToInt32(float.Parse(amount)) * 100;

            PaypointModel reqEPPaypointDishHomeInfo = new PaypointModel();
            PaypointModel resEPPaypointDishHomeInfo = new PaypointModel();

            PaypointModel reqGTPaypointDishHomeInfo = new PaypointModel();
            PaypointModel resGTPaypointDishHomeInfo = new PaypointModel();

            string totalAmount = string.Empty;
            string totalCount = string.Empty;
            string totalBAmount = string.Empty;
            string totalBCount = string.Empty;
            string balance = string.Empty;
            string message = string.Empty;
            string statusCode = string.Empty;
            string failedmessage = string.Empty;
            string GetMerchantName = string.Empty;
            string statusCodeBalance = string.Empty;

            string customerNo = string.Empty;

            // TraceIdGenerator traceid = new TraceIdGenerator();
            //tid = traceid.GenerateUniqueTraceID();
            tid = retrievalReference;

            FundTransfer fundtransfer = new FundTransfer
            {
                tid = tid,
                sc = sc,
                mobile = mobile,
                da = da,
                amount = amount,
                pin = pin,
                note = note,
                sourcechannel = src
            };
            if (sc == "00")
            {
                if (walletBalancePaisaInt >= amountpayInt)// if wallet balance less then bill amount then show error msg
                {
                    //First transaction MNRequest N Response
                    try
                    {

                        //FundTransfer fundtransfer = new FundTransfer
                        //{
                        //    tid = tid,
                        //    sc = sc,
                        //    mobile = mobile,
                        //    da = da,
                        //    amount = amount,
                        //    pin = pin,
                        //    note = note,
                        //    sourcechannel = src
                        //};

                        ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, fundtransfer);
                        //CustActivityModel custsmsInfo = new CustActivityModel();

                        //MNTransactionMaster validTransactionData = new MNTransactionMaster();


                        //if (TokenGenerator.TokenChecker(sessionID, mobile, src) == false)
                        //{
                        //    // throw ex
                        //    statusCode = "400";
                        //    message = "Session expired. Please login again";
                        //    failedmessage = message;
                        //}
                        //else
                        //{
                        if ((tid == null) || (sc == null) || (mobile == null) || (da == null) || (amount == null) || (pin == null) ||
                        (src == null) || (double.Parse(amount) <= 0) || (vid == null))
                        {
                            // throw ex
                            statusCode = "400";
                            message = "Parameters Missing/Invalid";
                            failedmessage = message;
                        }
                        if ((companyCode == null) || (companyCode == "") || (serviceCode == null) || (serviceCode == "") ||
                            (account == null) || (special1 == null) || (special2 == null) || (transactionDate == null) || (transactionId == null) || (transactionId == "") ||
                            (userId == null) || (userPassword == null) || (salePointType == null) || (userId == "") || (userPassword == "") || (salePointType == ""))
                        {
                            // throw ex
                            statusCode = "400";
                            message = "Parameters Missing/Invalid PayPoint";
                            failedmessage = message;
                        }
                        else
                        {
                            if (sc == "00")
                            {
                                transactionType = "PayPoint Txfr to W2W";
                            }
                            else if (sc == "10")
                            {
                                transactionType = "PayPoint Txfr to B2W"; //B2W
                            }

                            if (!(UserNameCheck.IsValidUser(mobile)))
                            {
                                // throw ex
                                statusCode = "400";
                                //message = "Transaction restricted to User";
                                message = "Transaction only for User";
                                failedmessage = message;
                            }
                            if (UserNameCheck.IsValidMerchant(da))
                            {

                                TransLimitCheck transLimitCheck = new TransLimitCheck();
                                string resultTranLimit = transLimitCheck.LimitCheck(mobile, da, amount, sc, pin, src);

                                var jsonDataResult = JObject.Parse(resultTranLimit);
                                statusCode = jsonDataResult["StatusCode"].ToString();
                                string statusMsg = jsonDataResult["StatusMessage"].ToString();
                                message = jsonDataResult["StatusMessage"].ToString();
                                failedmessage = message;

                                if ((statusCode == "200") && (message == "Success"))
                                {
                                    //start: checking trace id
                                    do
                                    {
                                        //TraceIdGenerator traceid = new TraceIdGenerator();
                                        // tid = traceid.GenerateUniqueTraceID();

                                        fundtransfer.tid = tid;
                                        bool traceIdCheck = false;
                                        traceIdCheck = TraceIdCheck.IsValidTraceId(fundtransfer.tid);
                                        if (traceIdCheck == true)
                                        {
                                            result = "Trace ID Repeated";
                                        }
                                        else
                                        {
                                            result = "false";
                                        }

                                    } while (result == "Trace ID Repeated");
                                    //End: TraceId

                                    //start:Com focus one log///
                                    MNFundTransfer mnft = new MNFundTransfer(tid, fundtransfer.sc, fundtransfer.mobile,
                                        fundtransfer.sa, fundtransfer.amount, fundtransfer.da, fundtransfer.note, fundtransfer.pin,
                                        fundtransfer.sourcechannel);
                                    var comfocuslog = new MNComAndFocusOneLog(mnft, DateTime.Now);
                                    var mncomfocuslog = new MNComAndFocusOneLogsController();
                                    //mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                    result = mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                    //end:Com focus one log//

                                    if (result == "Success")
                                    {
                                        //NOTE:- may be need to validate before insert into reply typpe
                                        //start:insert into reply type as HTTP//
                                        var replyType = new MNReplyType(tid, "HTTP");
                                        var mnreplyType = new MNReplyTypesController();
                                        mnreplyType.InsertIntoReplyType(replyType);
                                        //end:insert into reply type as HTTP//

                                        MNMerchantsController getMerchantDetails = new MNMerchantsController();

                                        string GetMerchantMobile = getMerchantDetails.PassVidToGetMerchantDetail(vid);
                                        if (GetMerchantMobile != "" || GetMerchantMobile != null)
                                        {
                                            fundtransfer.da = GetMerchantMobile; //Set Destination Merchant Mobile number
                                            GetMerchantName = getMerchantDetails.PassVIdToGetMerchantName(vid);
                                        }
                                        else
                                        {
                                            statusCode = "400";
                                            message = "Destination Merchant Doesnot Exists";
                                            mnft.Response = "Destination Merchant Doesnot Exists";
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, mnft.Response);
                                            result = mnft.Response;
                                            failedmessage = message;
                                        }


                                        //start:insert into transaction master//
                                        if (mnft.valid())
                                        {
                                            //var transaction = new MNTransactionMaster(mnft);
                                            //var mntransaction = new MNTransactionsController();
                                            //validTransactionData = mntransaction.Validate(transaction, mnft.pin);

                                            var transactionpaypoint = new MNTransactionMaster(mnft);
                                            var mntransactionpaypoint = new MNTransactionsController();
                                            validTransactionData = mntransactionpaypoint.Validatepaypoint(transactionpaypoint, mnft.pin);

                                            result = validTransactionData.Response;
                                            /*** ***/
                                            ErrorMessage em = new ErrorMessage();

                                            if (validTransactionData.Response == "Error")
                                            {
                                                mnft.Response = "error";
                                                mnft.ResponseStatus(HttpStatusCode.InternalServerError,
                                                    "Internal server error - try again later, or contact support");
                                                result = mnft.Response;
                                                statusCode = "500";
                                                message = "Internal server error - try again later, or contact support";
                                                failedmessage = message;
                                            }
                                            else
                                            {
                                                if ((result == "Trace ID Repeated") || (result == "Limit Exceed")
                                                    || (result == "Invalid Source User") || (result == "Invalid Destination User")
                                                    || (result == "Invalid Product Request") || (result == "Please try again") || (result == ""))
                                                {
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                    statusCode = "400";
                                                    message = result;
                                                    failedmessage = message;
                                                }
                                                if (result == "Invalid PIN")
                                                {
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                    statusCode = "400";
                                                    message = result;
                                                    failedmessage = message;
                                                }
                                                if (result == "111")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_111;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "114")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_114;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "115")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_115;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "116")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_116;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "119")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_119;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "121")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_121;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "163")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_163;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "180")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_180;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "181")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_181;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "182")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_182;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "183")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_183;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "184")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_184;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "185")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_185;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "186")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_186;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "187")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_187;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "188")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_188;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "189")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_189;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "190")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_190;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "800")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_800;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "902")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_902;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "904")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_904;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "906")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_906;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "907")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_907;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "909")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_909;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "911")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_911;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "913")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_913;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "90")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_90;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "91")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_91;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "92")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_92;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "94")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_94;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "95")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_95;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "98")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_98;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "99")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_99;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "508")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_508;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                else if (validTransactionData.ResponseCode == "OK")
                                                {
                                                    statusCode = "200";
                                                    message = result;
                                                    mnft.ResponseStatus(HttpStatusCode.OK, message);

                                                } //END ValidTransactionData.ResponseCode


                                            } //END validTransactionData.Response WITHOUT MNDB ERROR

                                            //start comment outgoing
                                            //OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                                            //if (response.StatusCode == HttpStatusCode.OK)
                                            //{
                                            //    string messagereply = "";
                                            //    try
                                            //    {
                                            //        //messagereply = "Dear " + CustCheckUtils.GetName(mobile) + "," + "\n";

                                            //        //messagereply += " You have successfully paid NPR " + validTransactionData.Amount
                                            //        //                + " to " + GetMerchantName + " on date " +
                                            //        //                    (validTransactionData.CreatedDate).ToString("dd/MM/yyyy")
                                            //        //                + "." + "\n";
                                            //        //messagereply += "Thank you. MNepal";

                                            //        //var client = new WebClient();

                                            //        ////SENDER
                                            //        //if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                                            //        //{
                                            //        //    //FOR NCELL
                                            //        //    var content = client.DownloadString(
                                            //        //        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                            //        //        + "977" + mobile + "&Text=" + messagereply + "");
                                            //        //}
                                            //        //else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                            //        //            || (mobile.Substring(0, 3) == "986"))
                                            //        //{
                                            //        //    //FOR NTC
                                            //        //    var content = client.DownloadString(
                                            //        //        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                            //        //        + "977" + mobile + "&Text=" + messagereply + "");
                                            //        //}

                                            //        statusCode = "200";
                                            //        var v = new
                                            //        {
                                            //            StatusCode = Convert.ToInt32(statusCode),
                                            //            StatusMessage = result
                                            //        };
                                            //        result = JsonConvert.SerializeObject(v);

                                            //    }
                                            //    catch (Exception ex)
                                            //    {
                                            //        // throw ex
                                            //        statusCode = "400";
                                            //        message = ex.Message;
                                            //    }


                                            //    custsmsInfo = new CustActivityModel()
                                            //    {
                                            //        UserName = fundtransfer.mobile,
                                            //        RequestMerchant = transactionType,
                                            //        DestinationNo = fundtransfer.da,
                                            //        Amount = validTransactionData.Amount.ToString(),
                                            //        SMSStatus = "Success",
                                            //        SMSSenderReply = messagereply,
                                            //        ErrorMessage = "",
                                            //    };


                                            //}
                                            //else if ((response.StatusCode == HttpStatusCode.BadRequest) || (response.StatusCode == HttpStatusCode.InternalServerError) || (statusCode != "200"))
                                            //{
                                            //    custsmsInfo = new CustActivityModel()
                                            //    {
                                            //        UserName = mobile,
                                            //        RequestMerchant = transactionType,
                                            //        DestinationNo = fundtransfer.da,
                                            //        Amount = validTransactionData.Amount.ToString(),
                                            //        SMSStatus = "Failed",
                                            //        SMSSenderReply = message,
                                            //        ErrorMessage = failedmessage,
                                            //    };

                                            //}

                                            //end comment outgoint
                                            //end:insert into transaction master//

                                        } //END:insert into transaction master//
                                        else
                                        {
                                            mnft.Response = "error";
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, "parameters missing/invalid"); //200 - OK
                                            result = mnft.Response;
                                            statusCode = "400";
                                            message = "parameters missing/invalid";
                                            failedmessage = message;

                                            custsmsInfo = new CustActivityModel()
                                            {
                                                UserName = mobile,
                                                RequestMerchant = transactionType,
                                                DestinationNo = fundtransfer.da,
                                                Amount = amount,
                                                SMSStatus = "Failed",
                                                SMSSenderReply = message,
                                                ErrorMessage = failedmessage,
                                            };
                                        }

                                    } //END MNComAndFocusOneLogsController
                                    else
                                    {
                                        statusCode = "400";
                                        mnft.Response = "Data Insertion Failed in Check DB Connection: TraceID limit might have exceeded";
                                        mnft.ResponseStatus(HttpStatusCode.InternalServerError, mnft.Response);
                                        result = mnft.Response;
                                        failedmessage = result;
                                    }

                                } //END TRansLimit Check StatusCode N Message
                                else
                                {
                                    custsmsInfo = new CustActivityModel()
                                    {
                                        UserName = mobile,
                                        RequestMerchant = transactionType,
                                        DestinationNo = fundtransfer.da,
                                        Amount = amount,
                                        SMSStatus = "Failed",
                                        SMSSenderReply = message,
                                        ErrorMessage = failedmessage,
                                    };
                                }

                            } //END IsValidMerchant

                            //} //END Destination mobile No check

                        }

                    }
                    catch (Exception ex)
                    {
                        message = result + ex + "Error Message ";
                        //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                        statusCode = "400";
                        //failedmessage = message;
                        failedmessage = "Please try again.";
                    }

                }
                else  //else for  if wallet balance less then bill amount then show error msg
                {
                    statusCodeBalance = "400";
                    message = "Insufficient Balance";
                    failedmessage = message;

                }
            }
            else if (sc == "10")//for bank payment in nepal water
            {
                //if (bankBalancePaisaInt >= amountpayInt)
                //{
                //First transaction MNRequest N Response
                try
                {

                    //FundTransfer fundtransfer = new FundTransfer
                    //{
                    //    tid = tid,
                    //    sc = sc,
                    //    mobile = mobile,
                    //    da = da,
                    //    amount = amount,
                    //    pin = pin,
                    //    note = note,
                    //    sourcechannel = src
                    //};

                    ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, fundtransfer);
                    //CustActivityModel custsmsInfo = new CustActivityModel();

                    //MNTransactionMaster validTransactionData = new MNTransactionMaster();


                    //if (TokenGenerator.TokenChecker(sessionID, mobile, src) == false)
                    //{
                    //    // throw ex
                    //    statusCode = "400";
                    //    message = "Session expired. Please login again";
                    //    failedmessage = message;
                    //}
                    //else
                    //{
                    if ((tid == null) || (sc == null) || (mobile == null) || (da == null) || (amount == null) || (pin == null) ||
                    (src == null) || (double.Parse(amount) <= 0) || (vid == null))
                    {
                        // throw ex
                        statusCode = "400";
                        message = "Parameters Missing/Invalid";
                        failedmessage = message;
                    }
                    if ((companyCode == null) || (companyCode == "") || (serviceCode == null) || (serviceCode == "") ||
                        (account == null) || (special1 == null) || (special2 == null) || (transactionDate == null) || (transactionId == null) || (transactionId == "") ||
                        (userId == null) || (userPassword == null) || (salePointType == null) || (userId == "") || (userPassword == "") || (salePointType == ""))
                    {
                        // throw ex
                        statusCode = "400";
                        message = "Parameters Missing/Invalid PayPoint";
                        failedmessage = message;
                    }
                    else
                    {
                        if (sc == "00")
                        {
                            transactionType = "PayPoint Txfr to W2W";
                        }
                        else if (sc == "10")
                        {
                            transactionType = "PayPoint Txfr to B2W"; //B2W
                        }

                        if (!(UserNameCheck.IsValidUser(mobile)))
                        {
                            // throw ex
                            statusCode = "400";
                            //message = "Transaction restricted to User";
                            message = "Transaction only for User";
                            failedmessage = message;
                        }
                        if (UserNameCheck.IsValidMerchant(da))
                        {

                            TransLimitCheck transLimitCheck = new TransLimitCheck();
                            string resultTranLimit = transLimitCheck.LimitCheck(mobile, da, amount, sc, pin, src);

                            var jsonDataResult = JObject.Parse(resultTranLimit);
                            statusCode = jsonDataResult["StatusCode"].ToString();
                            string statusMsg = jsonDataResult["StatusMessage"].ToString();
                            message = jsonDataResult["StatusMessage"].ToString();
                            failedmessage = message;

                            if ((statusCode == "200") && (message == "Success"))
                            {
                                //start: checking trace id
                                do
                                {
                                    //TraceIdGenerator traceid = new TraceIdGenerator();
                                    // tid = traceid.GenerateUniqueTraceID();
                                    fundtransfer.tid = tid;

                                    bool traceIdCheck = false;
                                    traceIdCheck = TraceIdCheck.IsValidTraceId(fundtransfer.tid);
                                    if (traceIdCheck == true)
                                    {
                                        result = "Trace ID Repeated";
                                    }
                                    else
                                    {
                                        result = "false";
                                    }

                                } while (result == "Trace ID Repeated");
                                //End: TraceId

                                //start:Com focus one log///
                                MNFundTransfer mnft = new MNFundTransfer(tid, fundtransfer.sc, fundtransfer.mobile,
                                    fundtransfer.sa, fundtransfer.amount, fundtransfer.da, fundtransfer.note, fundtransfer.pin,
                                    fundtransfer.sourcechannel);
                                var comfocuslog = new MNComAndFocusOneLog(mnft, DateTime.Now);
                                var mncomfocuslog = new MNComAndFocusOneLogsController();
                                //mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                result = mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                //end:Com focus one log//

                                if (result == "Success")
                                {
                                    //NOTE:- may be need to validate before insert into reply typpe
                                    //start:insert into reply type as HTTP//
                                    var replyType = new MNReplyType(tid, "HTTP");
                                    var mnreplyType = new MNReplyTypesController();
                                    mnreplyType.InsertIntoReplyType(replyType);
                                    //end:insert into reply type as HTTP//

                                    MNMerchantsController getMerchantDetails = new MNMerchantsController();

                                    string GetMerchantMobile = getMerchantDetails.PassVidToGetMerchantDetail(vid);
                                    if (GetMerchantMobile != "" || GetMerchantMobile != null)
                                    {
                                        fundtransfer.da = GetMerchantMobile; //Set Destination Merchant Mobile number
                                        GetMerchantName = getMerchantDetails.PassVIdToGetMerchantName(vid);
                                    }
                                    else
                                    {
                                        statusCode = "400";
                                        message = "Destination Merchant Doesnot Exists";
                                        mnft.Response = "Destination Merchant Doesnot Exists";
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, mnft.Response);
                                        result = mnft.Response;
                                        failedmessage = message;
                                    }


                                    //start:insert into transaction master//
                                    if (mnft.valid())
                                    {
                                        //var transaction = new MNTransactionMaster(mnft);
                                        //var mntransaction = new MNTransactionsController();
                                        //validTransactionData = mntransaction.Validate(transaction, mnft.pin);


                                        var transactionpaypoint = new MNTransactionMaster(mnft);
                                        var mntransactionpaypoint = new MNTransactionsController();
                                        validTransactionData = mntransactionpaypoint.Validatepaypoint(transactionpaypoint, mnft.pin);
                                        result = validTransactionData.Response;
                                        /*** ***/
                                        ErrorMessage em = new ErrorMessage();

                                        if (validTransactionData.Response == "Error")
                                        {
                                            mnft.Response = "error";
                                            mnft.ResponseStatus(HttpStatusCode.InternalServerError,
                                                "Internal server error - try again later, or contact support");
                                            result = mnft.Response;
                                            statusCode = "500";
                                            message = "Internal server error - try again later, or contact support";
                                            failedmessage = message;
                                        }
                                        else
                                        {
                                            if ((result == "Trace ID Repeated") || (result == "Limit Exceed")
                                                || (result == "Invalid Source User") || (result == "Invalid Destination User")
                                                || (result == "Invalid Product Request") || (result == "Please try again") || (result == ""))
                                            {
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                statusCode = "400";
                                                message = result;
                                                failedmessage = message;
                                            }
                                            if (result == "Invalid PIN")
                                            {
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                statusCode = "400";
                                                message = result;
                                                failedmessage = message;
                                            }
                                            if (result == "111")
                                            {
                                                statusCode = result;
                                                message = em.Error_111;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "114")
                                            {
                                                statusCode = result;
                                                message = em.Error_114;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "115")
                                            {
                                                statusCode = result;
                                                message = em.Error_115;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "116")
                                            {
                                                statusCode = result;
                                                message = em.Error_116;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "119")
                                            {
                                                statusCode = result;
                                                message = em.Error_119;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "121")
                                            {
                                                statusCode = result;
                                                message = em.Error_121;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "163")
                                            {
                                                statusCode = result;
                                                message = em.Error_163;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "180")
                                            {
                                                statusCode = result;
                                                message = em.Error_180;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "181")
                                            {
                                                statusCode = result;
                                                message = em.Error_181;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "182")
                                            {
                                                statusCode = result;
                                                message = em.Error_182;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "183")
                                            {
                                                statusCode = result;
                                                message = em.Error_183;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "184")
                                            {
                                                statusCode = result;
                                                message = em.Error_184;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "185")
                                            {
                                                statusCode = result;
                                                message = em.Error_185;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "186")
                                            {
                                                statusCode = result;
                                                message = em.Error_186;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "187")
                                            {
                                                statusCode = result;
                                                message = em.Error_187;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "188")
                                            {
                                                statusCode = result;
                                                message = em.Error_188;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "189")
                                            {
                                                statusCode = result;
                                                message = em.Error_189;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "190")
                                            {
                                                statusCode = result;
                                                message = em.Error_190;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "800")
                                            {
                                                statusCode = result;
                                                message = em.Error_800;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "902")
                                            {
                                                statusCode = result;
                                                message = em.Error_902;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "904")
                                            {
                                                statusCode = result;
                                                message = em.Error_904;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "906")
                                            {
                                                statusCode = result;
                                                message = em.Error_906;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "907")
                                            {
                                                statusCode = result;
                                                message = em.Error_907;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "909")
                                            {
                                                statusCode = result;
                                                message = em.Error_909;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "911")
                                            {
                                                statusCode = result;
                                                message = em.Error_911;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "913")
                                            {
                                                statusCode = result;
                                                message = em.Error_913;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "90")
                                            {
                                                statusCode = result;
                                                message = em.Error_90;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "91")
                                            {
                                                statusCode = result;
                                                message = em.Error_91;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "92")
                                            {
                                                statusCode = result;
                                                message = em.Error_92;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "94")
                                            {
                                                statusCode = result;
                                                message = em.Error_94;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "95")
                                            {
                                                statusCode = result;
                                                message = em.Error_95;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "98")
                                            {
                                                statusCode = result;
                                                message = em.Error_98;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "99")
                                            {
                                                statusCode = result;
                                                message = em.Error_99;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "508")
                                            {
                                                statusCode = result;
                                                message = em.Error_508;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            else if (validTransactionData.ResponseCode == "OK")
                                            {
                                                statusCode = "200";
                                                message = result;
                                                mnft.ResponseStatus(HttpStatusCode.OK, message);

                                            } //END ValidTransactionData.ResponseCode


                                        } //END validTransactionData.Response WITHOUT MNDB ERROR

                                        //end:insert into transaction master//

                                    } //END:insert into transaction master//
                                    else
                                    {
                                        mnft.Response = "error";
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, "parameters missing/invalid"); //200 - OK
                                        result = mnft.Response;
                                        statusCode = "400";
                                        message = "parameters missing/invalid";
                                        failedmessage = message;

                                        custsmsInfo = new CustActivityModel()
                                        {
                                            UserName = mobile,
                                            RequestMerchant = transactionType,
                                            DestinationNo = fundtransfer.da,
                                            Amount = amount,
                                            SMSStatus = "Failed",
                                            SMSSenderReply = message,
                                            ErrorMessage = failedmessage,
                                        };
                                    }

                                } //END MNComAndFocusOneLogsController
                                else
                                {
                                    statusCode = "400";
                                    mnft.Response = "Data Insertion Failed in Check DB Connection: TraceID limit might have exceeded";
                                    mnft.ResponseStatus(HttpStatusCode.InternalServerError, mnft.Response);
                                    result = mnft.Response;
                                    failedmessage = result;
                                }

                            } //END TRansLimit Check StatusCode N Message
                            else
                            {
                                custsmsInfo = new CustActivityModel()
                                {
                                    UserName = mobile,
                                    RequestMerchant = transactionType,
                                    DestinationNo = fundtransfer.da,
                                    Amount = amount,
                                    SMSStatus = "Failed",
                                    SMSSenderReply = message,
                                    ErrorMessage = failedmessage,
                                };
                            }

                        } //END IsValidMerchant


                        //} //END Destination mobile No check

                    }

                }
                catch (Exception ex)
                {
                    message = result + ex + "Error Message ";
                    //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                    statusCode = "400";
                    //failedmessage = message;
                    failedmessage = "Please try again.";
                }
                //}
                //else  //else for  if wallet balance less then bill amount then show error msg
                //{
                //    statusCodeBalance = "400";
                //    message = "Insufficient Balance";
                //    failedmessage = message;

                //}
            }
            try
            {

                //for  all EP  and GT transaction
                string compResultResp = "";

                if (statusCode == "200")
                {
                    try
                    {
                        //string amountInPaisa = ((double.Parse(amount)) * 100).ToString();

                        if (rltCheckPaymt == "000")//go if CP Response is 000
                        {
                            //int amountInPaisaInt = Convert.ToInt32(amountInPaisa);

                            long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                            exectransactionId = milliseconds.ToString();

                            string keyExecRlt = "";
                            string resultMessageResEP = "";
                            do
                            {
                                if ((companyCode == null) || (serviceCode == null) || (account == null) ||
                                (exectransactionDate == null) || (exectransactionId == null) ||
                                (refStan == null) || (amount == null) || (billNumber == null) ||
                                (userId == null) || (userPassword == null) || (salePointType == null))
                                {
                                    // throw ex
                                    statusCode = "400";
                                    message = "Parameters Missing/Invalid";
                                    failedmessage = message;
                                }
                                else
                                {
                                    if (companyCode == "598")
                                    {
                                        special1 = "";
                                    }
                                    else
                                    {
                                        special1 = special1.ToString();
                                    }

                                    // string URIEXECPayment = "https://test.paypoint.md:4445/PayPointWS/PayPointMSOperations.asmx/ExecutePayment";//for EP Link 

                                    //For excutepaypoint link in webconfig
                                    string URIEXECPayment = System.Web.Configuration.WebConfigurationManager.AppSettings["EPPaypointUrl"];

                                    string execPayParameters = "companyCode=" + companyCode + "&serviceCode=" + serviceCode +
                                            "&account=" + account + "&special1=" + special1 + "&special2=" + special2 +
                                            "&transactionDate=" + exectransactionDate + "&transactionId=" + exectransactionId +
                                            "&refStan=" + refStan + "&amount=" + Convert.ToInt32(float.Parse(amount) * 100).ToString() + "&billNumber=" + billNumber +
                                            "&userId=" + userId.Trim() + "&userPassword=" + userPassword.Trim() + "&salePointType=" + salePointType;

                                    //for executepayment request insert in database for wlink
                                    reqEPPaypointDishHomeInfo = new PaypointModel()
                                    {
                                        companyCodeReqEP = companyCode,
                                        serviceCodeReqEP = serviceCode,
                                        accountReqEP = account,
                                        special1ReqEP = special1,
                                        special2ReqEP = special2,

                                        transactionDateReqEP = exectransactionDate,
                                        transactionIdReqEP = exectransactionId,
                                        userIdReqEP = userId,
                                        userPasswordReqEP = userPassword,
                                        salePointTypeReqEP = salePointType,

                                        refStanReqEP = refStan,
                                        amountReqEP = amount,
                                        billNumberReqEP = billNumber,
                                        //retrievalReferenceReqEP = fundtransfer.tid,
                                        //retrievalReferenceReqEP = tid,
                                        retrievalReferenceReqEP = retrievalReference,
                                        remarkReqEP = "Execute Payment",
                                        UserName = mobile,
                                        ClientCode = ClientCode,
                                        paypointType = paypointType,

                                    };

                                    using (WebClient wcExecPay = new WebClient())
                                    {
                                        wcExecPay.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                                        string HtmlResultExecPay = wcExecPay.UploadString(URIEXECPayment, execPayParameters);

                                        XmlDocument xmlEDoc = new XmlDocument();
                                        xmlEDoc.LoadXml(HtmlResultExecPay);

                                        XmlNodeList nodeEPay = xmlEDoc.GetElementsByTagName("*");
                                        string resultEPay = nodeEPay[0].InnerText;
                                        string HtmlEPayResult = resultEPay;

                                        //for determining  checkpayment key and result
                                        var readerEPay = new StringReader(HtmlEPayResult);
                                        var xdocEPay = XDocument.Load(readerEPay);

                                        XDocument docEPay = XDocument.Parse(xdocEPay.ToString());

                                        var xElemEPay = XElement.Parse(xdocEPay.ToString());

                                        if (xElemEPay.Attribute("Result").Value == "000")
                                        {
                                            compResultResp = xElemEPay.Attribute("Result").Value;
                                            keyExecRlt = xElemEPay.Attribute("Key").Value;
                                        }
                                        else if ((xElemEPay.Attribute("Result").Value == "011") || (xElemEPay.Attribute("Result").Value == "012"))
                                        {
                                            compResultResp = xElemEPay.Attribute("Result").Value;
                                            keyExecRlt = xElemEPay.Attribute("Key").Value;
                                        }
                                        else
                                        {
                                            compResultResp = xElemEPay.Attribute("Result").Value;
                                            keyExecRlt = xElemEPay.Attribute("Key").Value;
                                        }
                                        resultMessageResEP = xElemEPay.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;



                                        //for Response Execute Payment
                                        resEPPaypointDishHomeInfo = new PaypointModel()
                                        {
                                            companyCodeResEP = companyCode,
                                            serviceCodeResEP = serviceCode,
                                            accountResEP = account,
                                            special1ResEP = special1,
                                            special2ResEP = special2,

                                            transactionDateResEP = transactionDate,
                                            transactionIdResEP = transactionId,
                                            userIdResEP = userId,

                                            userPasswordResEP = userPassword,
                                            salePointTypeResEP = salePointType,

                                            refStanResEP = refStan,
                                            amountResEP = amount,
                                            billNumberResEP = billNumber,
                                            //retrievalReferenceResEP = fundtransfer.tid,
                                            //retrievalReferenceResEP = tid,
                                            retrievalReferenceResEP = retrievalReference,
                                            responseCodeResEP = compResultResp,
                                            descriptionResEP = "Execute Payment" + keyExecRlt,
                                            UserName = mobile,
                                            ClientCode = ClientCode,
                                            paypointType = paypointType,
                                            resultMessageResEP = resultMessageResEP,
                                            customerNameResEP = customerName,
                                        };
                                    }
                                }

                            } while ((compResultResp == "011") || (compResultResp == "012"));

                            ///for  Inserting EP PayPoint Data for wlink

                            try
                            {
                                int resultsReqEP = PaypointUtils.RequestEPPaypointInfo(reqEPPaypointDishHomeInfo);
                                int resultsResEP = PaypointUtils.ResponseEPPaypointInfo(resEPPaypointDishHomeInfo);

                                if ((resultsReqEP > 0) && (resultsResEP > 0))
                                {
                                    message = result;
                                }
                                else
                                {
                                    message = result;
                                }

                            }
                            catch (Exception ex)
                            {
                                string ss = ex.Message;
                                message = result;
                            }

                            if (compResultResp == "000")
                            {
                                if ((refStan == null) || (userId == null) || (userPassword == null) || (salePointType == null))
                                {
                                    // throw ex
                                    statusCode = "400";
                                    message = "Parameters Missing/Invalid";
                                    failedmessage = message;
                                }
                                else if (compResultResp != "")
                                {
                                    string statusResGTP = "1";
                                    do
                                    {
                                        string key = "";
                                        string gtBillNumber = "";
                                        // string URIGetTran = "https://test.paypoint.md:4445/PayPointWS/PayPointMSOperations.asmx/GetTransaction";//for GT paypoint transactionlink

                                        //For get transactionpaypoint link in webconfig
                                        string URIGetTran = System.Web.Configuration.WebConfigurationManager.AppSettings["GTPPaypointUrl"];

                                        string GetTranParameters = "userLogin=" + userId + "&userPassword=" + userPassword + "&stan=" + "-1" + "&refStan=" + refStan + "&key=" + "" + "&billNumber=" + gtBillNumber;

                                        //for get transaction payment request insert in database
                                        reqGTPaypointDishHomeInfo = new PaypointModel()
                                        {
                                            companyCodeReqGTP = companyCode,
                                            serviceCodeReqGTP = serviceCode,
                                            accountReqGTP = account,
                                            special1ReqGTP = special1,
                                            special2ReqGTP = special2,

                                            transactionDateReqGTP = transactionDate,
                                            transactionIdReqGTP = transactionId,
                                            userIdReqGTP = userId,
                                            userPasswordReqGTP = userPassword,
                                            salePointTypeReqGTP = salePointType,

                                            refStanReqGTP = refStan,
                                            amountReqGTP = amount,
                                            billNumberReqGTP = gtBillNumber,
                                            // retrievalReferenceReqGTP = fundtransfer.tid,
                                            //retrievalReferenceReqGTP = tid,
                                            retrievalReferenceReqGTP = retrievalReference,
                                            remarkReqGTP = "Get Transaction Payment",
                                            UserName = mobile,
                                            ClientCode = ClientCode,
                                            paypointType = paypointType,
                                            //remarkReqGTP = "Get Transaction Payment"+keyExecRlt,

                                        };
                                        string getTranResultResp = "";
                                        string keyGetTrancRlt = "";
                                        string resultMessageResGTP = "";
                                        string billNumberResGTP = "";
                                        using (WebClient wcGetTran = new WebClient())
                                        {
                                            wcGetTran.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                                            string HtmlResultGetTran = wcGetTran.UploadString(URIGetTran, GetTranParameters);

                                            XmlDocument xmlEDoc = new XmlDocument();
                                            xmlEDoc.LoadXml(HtmlResultGetTran);

                                            XmlNodeList nodeEPay = xmlEDoc.GetElementsByTagName("*");
                                            string resultEPay = nodeEPay[0].InnerText;
                                            string HtmlEPayResult = resultEPay;

                                            //for determing excutepayment key and result
                                            var readerEPay = new StringReader(HtmlEPayResult);
                                            var xdocEPay = XDocument.Load(readerEPay);

                                            XDocument docEPay = XDocument.Parse(xdocEPay.ToString());
                                            var xElemGPay = XElement.Parse(docEPay.ToString());
                                            if (xElemGPay.Attribute("Result").Value == "000")
                                            {
                                                getTranResultResp = xElemGPay.Attribute("Result").Value;
                                                keyGetTrancRlt = xElemGPay.Attribute("Key").Value;
                                                resultMessageResGTP = xElemGPay.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;
                                                if (!(resultMessageResGTP == "No data"))
                                                {
                                                    billNumberResGTP = xElemGPay.Descendants().Elements("BillNumber").Where(x => x.Name == "BillNumber").SingleOrDefault().Value;
                                                    statusResGTP = xElemGPay.Descendants().Elements("Status").Where(x => x.Name == "Status").SingleOrDefault().Value;

                                                    //for get transaction payment status validation
                                                    if (!(statusResGTP == "1" || statusResGTP == "5" || statusResGTP == "11" || statusResGTP == "13"))
                                                    {
                                                        if (statusResGTP == "4" || statusResGTP == "6" || statusResGTP == "14" || statusResGTP == "16")
                                                        {
                                                            // mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                            statusCode = "400";
                                                            //message = result;
                                                            message = "Failed";
                                                            failedmessage = message;
                                                            resultMessageResGTP = "failed";
                                                        }

                                                    }
                                                    else
                                                    {
                                                        statusCode = "200";
                                                        resultMessageResGTP = "sucess";
                                                    }
                                                }
                                                else
                                                {
                                                    //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                    statusCode = "400";
                                                    //message = result;
                                                    message = "Failed";
                                                    failedmessage = message;
                                                    resultMessageResGTP = "failed";
                                                }
                                            }
                                            else
                                            {
                                                resultMessageResGTP = xElemGPay.Descendants().Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;

                                            }


                                            //end get transaction payment status validation

                                            ////for get transaction payment response insert in database
                                            resGTPaypointDishHomeInfo = new PaypointModel()
                                            {
                                                companyCodeResGTP = companyCode,
                                                serviceCodeResGTP = serviceCode,
                                                accountResGTP = account,
                                                special1ResGTP = special1,
                                                special2ResGTP = special2,

                                                transactionDateResGTP = transactionDate,
                                                transactionIdResGTP = transactionId,
                                                userIdResGTP = userId,
                                                userPasswordResGTP = userPassword,
                                                salePointTypeResGTP = salePointType,

                                                refStanResGTP = refStan,
                                                amountResGTP = amount, //amountpay,
                                                billNumberResGTP = billNumberResGTP,
                                                //retrievalReferenceResGTP = fundtransfer.tid,
                                                //retrievalReferenceResGTP = tid,
                                                retrievalReferenceResGTP = retrievalReference,
                                                responseCodeResGTP = getTranResultResp,
                                                descriptionResGTP = "Get Transaction Payment " + keyGetTrancRlt,
                                                UserName = mobile,
                                                ClientCode = ClientCode,
                                                paypointType = paypointType,
                                                resultMessageResGTP = resultMessageResGTP,
                                                customerNameResGTP = customerName,

                                            };
                                        }
                                    } while (statusResGTP == "10" || statusResGTP == "15" || statusResGTP == "20" || statusResGTP == "21" || statusResGTP == "99" || statusResGTP == "12" || statusResGTP == "2" || statusResGTP == "0");

                                    ///for  Inserting GT PayPoint Data

                                    try
                                    {

                                        int resultsReqGTP = PaypointUtils.RequestGTPaypointInfo(reqGTPaypointDishHomeInfo);
                                        int resultsResGTP = PaypointUtils.ResponseGTPaypointInfo(resGTPaypointDishHomeInfo);

                                        if ((resultsReqGTP > 0) && (resultsResGTP > 0))
                                        {
                                            message = result;
                                        }
                                        else
                                        {
                                            message = result;
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        string ss = ex.Message;
                                        message = result;
                                    }

                                }

                            }

                            else
                            {
                                //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                statusCode = "400";
                                //message = result;
                                message = resultMessageResEP;
                                failedmessage = message;
                            }

                        }
                        else //ELSE FOR (rltCheckPaymt == "000")I.E Error  in result of CP
                        {
                            //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                            statusCode = "400";
                            // message = result;
                            message = resultMessageResCP;

                            failedmessage = message;

                            ////start:Com focus one log///
                            //MNFundTransfer mnft1 = new MNFundTransfer(tid, fundtransfer.sc, fundtransfer.da,
                            //    fundtransfer.sa, fundtransfer.amount, fundtransfer.mobile, fundtransfer.note, fundtransfer.pin,
                            //    fundtransfer.sourcechannel);
                            //var comfocuslog1 = new MNComAndFocusOneLog(mnft1, DateTime.Now);
                            //var mncomfocuslog1 = new MNComAndFocusOneLogsController();
                            //result = mncomfocuslog1.InsertIntoComFocusOne(comfocuslog1);
                            ////end:Com focus one log//

                            //if (mnft1.valid())
                            //{
                            //    var transaction1 = new MNTransactionMaster(mnft1);
                            //    var mntransaction1 = new MNTransactionsController();
                            //    validTransactionData = mntransaction1.Validate(transaction1, mnft1.pin);
                            //    result = validTransactionData.Response;

                            //}
                        }
                        //end excute dekhi get ko sabai comment gareko
                    }
                    catch (Exception ex)
                    {
                        message = result + ex + "Error Message ";
                        //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                        statusCode = "400";
                        failedmessage = "Please try again.";
                    }
                }

                //REverse Transaction 
                if ((statusCode != "111") && (statusCode != "114") && (statusCode != "115") && (statusCode != "116") && (statusCode != "119") &&
                    (statusCode != "121") && (statusCode != "163") && (statusCode != "180") && (statusCode != "181") && (statusCode != "182") &&
                    (statusCode != "183") && (statusCode != "184") && (statusCode != "185") && (statusCode != "186") && (statusCode != "187") &&
                    (statusCode != "188") && (statusCode != "189") && (statusCode != "190") && (statusCode != "800") && (statusCode != "902") &&
                    (statusCode != "904") && (statusCode != "906") && (statusCode != "907") && (statusCode != "909") && (statusCode != "911") &&
                    (statusCode != "913") && (statusCode != "90") && (statusCode != "91") && (statusCode != "94") && (statusCode != "95") &&
                    (statusCode != "98") && (statusCode != "99") && (statusCodeBalance != "400") && (compResultResp != "000") && (statusCodeBalance != "400")
                    && (statusCode != "200") && (statusCode != "508")
                    )
                //((statusCodeBalance != "400") && (compResultResp != "000")) || ((statusCodeBalance != "400") && (statusCode != "200")) ||
                //if (statusCode != "116")
                {
                    //TraceIdGenerator traceRevid = new TraceIdGenerator();
                    //tid = traceRevid.GenerateUniqueTraceID();
                    tid = retrievalReference;
                    if (sc == "00")
                    {
                        transactionType = "PayPoint Txfr to W2W";
                    }
                    else
                    {
                        sc = "01";
                        transactionType = "PayPoint Txfr to W2B"; //B2W
                    }
                    FundTransfer fundtransferRev = new FundTransfer
                    {
                        tid = tid,
                        sc = sc,
                        mobile = da,//mobile
                        da = mobile,//da
                        amount = amount,
                        pin = pin,
                        note = "reverse " + note,
                        sourcechannel = src
                    };
                    ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, fundtransfer);
                    //CustActivityModel custsmsInfo = new CustActivityModel();


                    // MNTransactionMaster validTransactionData = new MNTransactionMaster();


                    //if (TokenGenerator.TokenChecker(sessionID, mobile, src) == false)
                    //{
                    //    // throw ex
                    //    statusCode = "400";
                    //    message = "Session expired. Please login again";
                    //    failedmessage = message;
                    //}
                    //else
                    //{
                    if ((tid == null) || (sc == null) || (mobile == null) || (da == null) || (amount == null) || (pin == null) ||
                    (src == null) || (double.Parse(amount) <= 0) || (vid == null))
                    {
                        // throw ex
                        statusCode = "400";
                        message = "Parameters Missing/Invalid";
                        failedmessage = message;
                    }
                    if ((companyCode == null) || (companyCode == "") || (serviceCode == null) || (serviceCode == "") ||
                        (account == null) || (special1 == null) || (special2 == null) || (transactionDate == null) || (transactionId == null) || (transactionId == "") ||
                        (userId == null) || (userPassword == null) || (salePointType == null) || (userId == "") || (userPassword == "") || (salePointType == ""))
                    {
                        // throw ex
                        statusCode = "400";
                        message = "Parameters Missing/Invalid PayPoint";
                        failedmessage = message;
                    }
                    else
                    {
                        //if (sc == "00")
                        //{
                        //    transactionType = "PayPoint Txfr to W2W";
                        //}
                        //else 
                        //{
                        //    sc = "01";
                        //    transactionType = "PayPoint Txfr to W2B"; //B2W
                        //}

                        if (!(UserNameCheck.IsValidUser(mobile)))
                        {
                            // throw ex
                            statusCode = "400";
                            //message = "Transaction restricted to User";
                            message = "Transaction only for User";
                            failedmessage = message;
                        }
                        if (UserNameCheck.IsValidMerchant(da))
                        {

                            TransLimitCheck transLimitCheck = new TransLimitCheck();
                            string resultTranLimit = transLimitCheck.LimitCheck(mobile, da, amount, sc, pin, src);

                            var jsonDataResult = JObject.Parse(resultTranLimit);
                            statusCode = jsonDataResult["StatusCode"].ToString();
                            string statusMsg = jsonDataResult["StatusMessage"].ToString();
                            message = jsonDataResult["StatusMessage"].ToString();
                            failedmessage = message;

                            if ((statusCode == "200") && (message == "Success"))
                            {
                                //start: checking trace id
                                do
                                {
                                    TraceIdGenerator traceid = new TraceIdGenerator();
                                    tid = traceid.GenerateUniqueTraceID();
                                    fundtransfer.tid = tid;

                                    bool traceIdCheck = false;
                                    traceIdCheck = TraceIdCheck.IsValidTraceId(fundtransfer.tid);
                                    if (traceIdCheck == true)
                                    {
                                        result = "Trace ID Repeated";
                                    }
                                    else
                                    {
                                        result = "false";
                                    }

                                } while (result == "Trace ID Repeated");
                                //End: TraceId

                                //start:Com focus one log///
                                MNFundTransfer mnft = new MNFundTransfer(tid, fundtransferRev.sc, fundtransferRev.mobile,
                                       fundtransferRev.sa, fundtransferRev.amount, fundtransferRev.da, fundtransferRev.note, fundtransferRev.pin,
                                       fundtransferRev.sourcechannel, "T", "PayPoint");
                                var comfocuslog = new MNComAndFocusOneLog(mnft, DateTime.Now);
                                var mncomfocuslog = new MNComAndFocusOneLogsController();
                                //mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                result = mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                //end:Com focus one log//

                                if (result == "Success")
                                {
                                    //NOTE:- may be need to validate before insert into reply typpe
                                    //start:insert into reply type as HTTP//
                                    var replyType = new MNReplyType(tid, "HTTP");
                                    var mnreplyType = new MNReplyTypesController();
                                    mnreplyType.InsertIntoReplyType(replyType);
                                    //end:insert into reply type as HTTP//

                                    MNMerchantsController getMerchantDetails = new MNMerchantsController();

                                    string GetMerchantMobile = getMerchantDetails.PassVidToGetMerchantDetail(vid);
                                    if (GetMerchantMobile != "" || GetMerchantMobile != null)
                                    {
                                        fundtransfer.da = GetMerchantMobile; //Set Destination Merchant Mobile number
                                        GetMerchantName = getMerchantDetails.PassVIdToGetMerchantName(vid);
                                    }
                                    else
                                    {
                                        statusCode = "400";
                                        message = "Destination Merchant Doesnot Exists";
                                        mnft.Response = "Destination Merchant Doesnot Exists";
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, mnft.Response);
                                        result = mnft.Response;
                                        failedmessage = message;
                                    }


                                    //start:insert into transaction master//
                                    if (mnft.valid())
                                    {
                                        var transaction = new MNTransactionMaster(mnft);
                                        var mntransaction = new MNTransactionsController();
                                        //validTransactionData = mntransaction.Validate(transaction, mnft.pin);
                                        validTransactionData = mntransaction.Validatepaypoint(transaction, mnft.pin);
                                        result = validTransactionData.Response;

                                        /*** ***/
                                        ErrorMessage em = new ErrorMessage();

                                        if (validTransactionData.Response == "Error")
                                        {
                                            mnft.Response = "error";
                                            mnft.ResponseStatus(HttpStatusCode.InternalServerError,
                                                "Internal server error - try again later, or contact support");
                                            result = mnft.Response;
                                            statusCode = "500";
                                            message = "Internal server error - try again later, or contact support";
                                            failedmessage = message;
                                        }
                                        else
                                        {
                                            if ((result == "Trace ID Repeated") || (result == "Limit Exceed")
                                                || (result == "Invalid Source User") || (result == "Invalid Destination User")
                                                || (result == "Invalid Product Request") || (result == "Please try again") || (result == ""))
                                            {
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                statusCode = "400";
                                                message = result;
                                                failedmessage = message;
                                            }
                                            if (result == "Invalid PIN")
                                            {
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                statusCode = "400";
                                                message = result;
                                                failedmessage = message;
                                            }
                                            if (result == "111")
                                            {
                                                statusCode = result;
                                                message = em.Error_111;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "114")
                                            {
                                                statusCode = result;
                                                message = em.Error_114;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "115")
                                            {
                                                statusCode = result;
                                                message = em.Error_115;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "116")
                                            {
                                                statusCode = result;
                                                message = em.Error_116;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "119")
                                            {
                                                statusCode = result;
                                                message = em.Error_119;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "121")
                                            {
                                                statusCode = result;
                                                message = em.Error_121;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "163")
                                            {
                                                statusCode = result;
                                                message = em.Error_163;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "180")
                                            {
                                                statusCode = result;
                                                message = em.Error_180;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "181")
                                            {
                                                statusCode = result;
                                                message = em.Error_181;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "182")
                                            {
                                                statusCode = result;
                                                message = em.Error_182;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "183")
                                            {
                                                statusCode = result;
                                                message = em.Error_183;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "184")
                                            {
                                                statusCode = result;
                                                message = em.Error_184;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "185")
                                            {
                                                statusCode = result;
                                                message = em.Error_185;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "186")
                                            {
                                                statusCode = result;
                                                message = em.Error_186;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "187")
                                            {
                                                statusCode = result;
                                                message = em.Error_187;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "188")
                                            {
                                                statusCode = result;
                                                message = em.Error_188;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "189")
                                            {
                                                statusCode = result;
                                                message = em.Error_189;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "190")
                                            {
                                                statusCode = result;
                                                message = em.Error_190;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "800")
                                            {
                                                statusCode = result;
                                                message = em.Error_800;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "902")
                                            {
                                                statusCode = result;
                                                message = em.Error_902;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "904")
                                            {
                                                statusCode = result;
                                                message = em.Error_904;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "906")
                                            {
                                                statusCode = result;
                                                message = em.Error_906;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "907")
                                            {
                                                statusCode = result;
                                                message = em.Error_907;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "909")
                                            {
                                                statusCode = result;
                                                message = em.Error_909;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "911")
                                            {
                                                statusCode = result;
                                                message = em.Error_911;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "913")
                                            {
                                                statusCode = result;
                                                message = em.Error_913;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "90")
                                            {
                                                statusCode = result;
                                                message = em.Error_90;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "91")
                                            {
                                                statusCode = result;
                                                message = em.Error_91;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "92")
                                            {
                                                statusCode = result;
                                                message = em.Error_92;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "94")
                                            {
                                                statusCode = result;
                                                message = em.Error_94;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "95")
                                            {
                                                statusCode = result;
                                                message = em.Error_95;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "98")
                                            {
                                                statusCode = result;
                                                message = em.Error_98;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "99")
                                            {
                                                statusCode = result;
                                                message = em.Error_99;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            else if (validTransactionData.ResponseCode == "OK")
                                            {
                                                statusCode = "200";
                                                message = result;
                                                mnft.ResponseStatus(HttpStatusCode.OK, message);

                                            } //END ValidTransactionData.ResponseCode


                                        } //END validTransactionData.Response WITHOUT MNDB ERROR
                                        /*** ***/

                                        OutgoingWebResponseContext response1 = WebOperationContext.Current.OutgoingResponse;
                                        if (response1.StatusCode == HttpStatusCode.OK)
                                        {
                                            string messagereply = "";
                                            try
                                            {
                                                messagereply = "Dear " + CustCheckUtils.GetName(mobile) + "," + "\n";

                                                messagereply += " You have successfully reverse  NPR " + validTransactionData.Amount
                                                                    + " to " +
                                                                    //GetMerchantName 
                                                                    "Utility payment for Dish Home Direct." + " on date " +
                                                                    (validTransactionData.CreatedDate).ToString("dd/MM/yyyy")
                                                                + "." + "\n";
                                                messagereply += "Thank you. MNepal";

                                                var client = new WebClient();

                                                //SENDER
                                                if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                                                {
                                                    //FOR NCELL
                                                    var content = client.DownloadString(
                                                        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                                        + "977" + mobile + "&Text=" + messagereply + "");
                                                }
                                                else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                                            || (mobile.Substring(0, 3) == "986"))
                                                {
                                                    //FOR NTC
                                                    var content = client.DownloadString(
                                                        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                                        + "977" + mobile + "&Text=" + messagereply + "");
                                                }

                                                statusCode = "200";
                                                var v = new
                                                {
                                                    StatusCode = Convert.ToInt32(statusCode),
                                                    StatusMessage = result
                                                };
                                                result = JsonConvert.SerializeObject(v);

                                            }
                                            catch (Exception ex)
                                            {
                                                // throw ex
                                                statusCode = "400";
                                                message = ex.Message;
                                            }


                                            custsmsInfo = new CustActivityModel()
                                            {
                                                UserName = fundtransfer.mobile,
                                                RequestMerchant = transactionType,
                                                DestinationNo = fundtransfer.da,
                                                Amount = validTransactionData.Amount.ToString(),
                                                SMSStatus = "Success",
                                                SMSSenderReply = messagereply,
                                                ErrorMessage = "",
                                            };


                                        }
                                        else if ((response1.StatusCode == HttpStatusCode.BadRequest) || (response1.StatusCode == HttpStatusCode.InternalServerError) || (statusCode != "200"))
                                        {
                                            custsmsInfo = new CustActivityModel()
                                            {
                                                UserName = mobile,
                                                RequestMerchant = transactionType,
                                                DestinationNo = fundtransfer.da,
                                                Amount = validTransactionData.Amount.ToString(),
                                                SMSStatus = "Failed",
                                                SMSSenderReply = message,
                                                ErrorMessage = failedmessage,
                                            };

                                        }
                                        //end:insert into transaction master//

                                    } //END:insert into transaction master//
                                    else
                                    {
                                        mnft.Response = "error";
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, "parameters missing/invalid"); //200 - OK
                                        result = mnft.Response;
                                        statusCode = "400";
                                        message = "parameters missing/invalid";
                                        failedmessage = message;

                                        custsmsInfo = new CustActivityModel()
                                        {
                                            UserName = mobile,
                                            RequestMerchant = transactionType,
                                            DestinationNo = fundtransfer.da,
                                            Amount = amount,
                                            SMSStatus = "Failed",
                                            SMSSenderReply = message,
                                            ErrorMessage = failedmessage,
                                        };
                                    }

                                } //END MNComAndFocusOneLogsController
                                else
                                {
                                    statusCode = "400";
                                    mnft.Response = "Data Insertion Failed in Check DB Connection: TraceID limit might have exceeded";
                                    mnft.ResponseStatus(HttpStatusCode.InternalServerError, mnft.Response);
                                    result = mnft.Response;
                                    failedmessage = result;
                                }

                            } //END TRansLimit Check StatusCode N Message
                            else
                            {
                                custsmsInfo = new CustActivityModel()
                                {
                                    UserName = mobile,
                                    RequestMerchant = transactionType,
                                    DestinationNo = fundtransfer.da,
                                    Amount = amount,
                                    SMSStatus = "Failed",
                                    SMSSenderReply = message,
                                    ErrorMessage = failedmessage,
                                };
                            }

                        } //END IsValidMerchant

                        //} //END Destination mobile No check

                    }
                }
                //} //tokengenerator ko closing bracket

                //for sending sms  if success 
                if (compResultResp == "000")
                {
                    OutgoingWebResponseContext response2 = WebOperationContext.Current.OutgoingResponse;
                    if (response2.StatusCode == HttpStatusCode.OK)
                    {
                        string messagereply = "";
                        try
                        {
                            messagereply = "Dear " + CustCheckUtils.GetName(mobile) + "," + "\n";

                            messagereply += " You have successfully paid NPR " + validTransactionData.Amount
                                            + " to " + " account name: " + account + ". " +
                                            //GetMerchantName 
                                            "Utility payment for Dish Home Direct (Pinless)." + " on date " +
                                                (validTransactionData.CreatedDate).ToString("dd/MM/yyyy")
                                            + "." + "\n";
                            messagereply += "Thank you. MNepal";

                            var client = new WebClient();

                            //SENDER
                            if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                            {
                                //FOR NCELL
                                var content = client.DownloadString(
                                    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                    + "977" + mobile + "&Text=" + messagereply + "");
                            }
                            else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                        || (mobile.Substring(0, 3) == "986"))
                            {
                                //FOR NTC
                                var content = client.DownloadString(
                                    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                    + "977" + mobile + "&Text=" + messagereply + "");
                            }

                            statusCode = "200";
                            var v = new
                            {
                                StatusCode = Convert.ToInt32(statusCode),
                                StatusMessage = result
                            };
                            result = JsonConvert.SerializeObject(v);

                        }
                        catch (Exception ex)
                        {
                            // throw ex
                            statusCode = "400";
                            message = ex.Message;
                        }


                        custsmsInfo = new CustActivityModel()
                        {
                            UserName = fundtransfer.mobile,
                            RequestMerchant = transactionType,
                            DestinationNo = fundtransfer.da,
                            Amount = validTransactionData.Amount.ToString(),
                            SMSStatus = "Success",
                            SMSSenderReply = messagereply,
                            ErrorMessage = "",
                        };


                    }
                    else if ((response2.StatusCode == HttpStatusCode.BadRequest) || (response2.StatusCode == HttpStatusCode.InternalServerError) || (statusCode != "200"))
                    {
                        custsmsInfo = new CustActivityModel()
                        {
                            UserName = mobile,
                            RequestMerchant = transactionType,
                            DestinationNo = fundtransfer.da,
                            Amount = validTransactionData.Amount.ToString(),
                            SMSStatus = "Failed",
                            SMSSenderReply = message,
                            ErrorMessage = failedmessage,
                        };

                    }
                }

            }
            catch (Exception ex)
            {
                // throw ex
                statusCode = "400";
                message = ex.Message;
            }
            if (statusCodeBalance == "400")
            {
                statusCode = "400";
            }
            if (statusCode == "")
            {
                result = result.ToString();
            }
            //else if (statusCode != "200")
            else if ((statusCode != "200") || (statusCodeBalance == "400"))
            {
                if (message == "")
                {
                    if (string.IsNullOrEmpty(result))
                    {
                        result = failedmessage; // "Could not process your request,Please Try again.";
                    }
                    message = result;
                }
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = failedmessage
                };
                result = JsonConvert.SerializeObject(v);
            }
            return result;
        }
        #endregion

        #region"Check NTC Payment"
        [OperationContract]
        [WebInvoke(Method = "POST",
                  ResponseFormat = WebMessageFormat.Json)]
        public string NTCcheckpayment(Stream input)
        {
            string PaypointPwd = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointPwd"];
            string PaypointUserID = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointUserID"];
            System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate { return true; }; //to prevent from SSL error
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            string tid = qs["tid"];
            string vid = qs["vid"];
            string sc = "00";
            string mobile = qs["mobile"];
            string da = System.Web.Configuration.WebConfigurationManager.AppSettings["DestinationNoForTestServer"];
            string src = qs["src"];
            string result = "";
            string sessionID = qs["tokenID"];
            string companyCode = qs["companyCode"];
            string serviceCode = qs["serviceCode"];
            string account = qs["account"];
            string special1 = qs["special1"];
            string special2 = "";
            string transactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            long millisecondstrandId = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string transactionId = millisecondstrandId.ToString();
            string userId = PaypointUserID;
            string userPassword = PaypointPwd.Trim();
            string salePointType = "6";
            string ClientCode = qs["ClientCode"];
            string paypointType = qs["paypointType"];
            string transactionType = string.Empty;
            string resultMessageResCP = "";
            string note = "Utility payment for NTC of service code " + serviceCode +". Mobile Number=" + qs["account"];

            PaypointModel reqCPPaypointNTCInfo = new PaypointModel();//to store data request of CP which is also commmon in nea
            PaypointModel resCPPaypointNTCInfo = new PaypointModel();//to store data response of CP which is also commmon in nea

            //PaypointModel resPaypointPaymentInfo = new PaypointModel();
            PaypointModel resPaypointNTCPaymentInfo = new PaypointModel();//to store data of Response of CP only

            string totalAmount = string.Empty;
            string totalCount = string.Empty;
            string totalBAmount = string.Empty;
            string totalBCount = string.Empty;
            string balance = string.Empty;
            string message = string.Empty;
            string statusCode = string.Empty;
            string failedmessage = string.Empty;
            string GetMerchantName = string.Empty;
            string retrievalRef = string.Empty;
            string refStanCK = string.Empty;

            string customerNo = string.Empty;
            TraceIdGenerator traceid = new TraceIdGenerator();
            tid = traceid.GenerateUniqueTraceID();


            List<Packages> pkg = new List<Packages>();
            //for CP transaction for nepal water
            try
            {
                string URI = System.Web.Configuration.WebConfigurationManager.AppSettings["CPPaypointUrl"];

                string myParameters = "companyCode=" + companyCode + "&serviceCode=" + serviceCode +
                    "&account=" + account + "&special1=" + (Convert.ToInt32(special1)*100).ToString() + "&special2=" + special2 +
                    "&transactionDate=" + transactionDate + "&transactionId=" + transactionId +
                    "&userId=" + userId.Trim() + "&userPassword=" + userPassword.Trim() + "&salePointType=" + salePointType;

                //for checkpayment request insert in database
                reqCPPaypointNTCInfo = new PaypointModel()
                {
                    companyCodeReqCP = companyCode,
                    serviceCodeReqCP = serviceCode,
                    accountReqCP = account,
                    special1ReqCP = special1,
                    special2ReqCP = special2,

                    transactionDateReqCP = transactionDate,
                    transactionIdReqCP = transactionId,
                    userIdReqCP = userId,
                    userPasswordReqCP = userPassword,
                    salePointTypeReqCP = salePointType,

                    refStanReqCP = "",
                    amountReqCP = "",//amountInPaisa
                    billNumberReqCP = "",
                    //retrievalReferenceReqCP = fundtransfer.tid,
                    retrievalReferenceReqCP = tid,
                    remarkReqCP = "Check Payment",
                    UserName = mobile,
                    ClientCode = ClientCode,
                    paypointType = paypointType,

                };

                string billNumber = "";
                string amountpay = "";
                string refStan = "";
                string exectransactionId = ""; //Unique
                string exectransactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"); //Current DateTime
                string rltCheckPaymt = "";
                string customerName = "";
                string mask = "";

                using (WebClient wc = new WebClient())
                {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    var HtmlResult = wc.UploadString(URI, myParameters);// response from checkpayment

                    XmlDocument xmlDoc = new XmlDocument();

                    xmlDoc.LoadXml(HtmlResult);

                    XmlNodeList test = xmlDoc.GetElementsByTagName("*");
                    string results = test[0].InnerText;
                    string HtmlResult1 = results;

                    //for getting key value from check payment
                    var reader = new StringReader(HtmlResult1);
                    var xdoc = XDocument.Load(reader);

                    XDocument docParse = XDocument.Parse(xdoc.ToString());
                    IEnumerable<XElement> responses = docParse.Descendants();

                    var xElem = XElement.Parse(docParse.ToString());

                    rltCheckPaymt = xElem.Attribute("Result").Value;
                    string keyrlt = "";// response.Attribute("Key").Value;


                    message = resultMessageResCP;
                    if (xElem.Attribute("Result").Value == "000")
                    {
                        keyrlt = xElem.Attribute("Key").Value;
                        billNumber = xElem.Descendants().Elements("BillNumber").Where(x => x.Name == "BillNumber").SingleOrDefault().Value;
                        amountpay = xElem.Descendants().Elements("Amount").Where(x => x.Name == "Amount").SingleOrDefault().Value;
                        refStan = xElem.Descendants().Elements("RefStan").Where(x => x.Name == "RefStan").SingleOrDefault().Value;
                        exectransactionDate = xElem.Descendants().Elements("DueDate").Where(x => x.Name == "DueDate").SingleOrDefault().Value;
                        customerName = xElem.Descendants().Elements("ReserveInfo").Where(x => x.Name == "ReserveInfo").SingleOrDefault().Value;
                        mask = xElem.Descendants().Elements("mask").Where(x => x.Name == "mask").SingleOrDefault().Value;

                        if (serviceCode == "0")
                        {
                            amountpay = special1;
                        }

                        Packages packages = new Packages();
                        string stringBuilderDescriptions = "";
                        string stringBuilderAmounts = "";
                        if (mask == "3" && amountpay == "0")
                        {
                            
                            var package = xElem.Descendants("packages").SingleOrDefault();
                            //var packageList = package.Descendants("package").ToList();

                            XmlDocument xmlDoc1 = new XmlDocument();
                            xmlDoc1.LoadXml(package.ToString());

                            XmlNodeList xmlNodeList = xmlDoc1.SelectNodes("/packages/package");

                            foreach (XmlNode xmlNode in xmlNodeList)
                            {
                                packages.Description = xmlNode.OuterXml; /*xmlNode.InnerText;*/
                                packages.Amount = xmlNode.Attributes["val"].Value;
                                pkg.Add(packages);
                                stringBuilderDescriptions = stringBuilderDescriptions + packages.Description + Environment.NewLine;

                                var charsToRemove = new string[] { "<", ">", "/", "/>", "\"" };
                                foreach (var c in charsToRemove)
                                {
                                    stringBuilderDescriptions = stringBuilderDescriptions.Replace(c, string.Empty);
                                }
                                stringBuilderAmounts = stringBuilderAmounts + packages.Amount + Environment.NewLine;

                            }

                        }
                        resPaypointNTCPaymentInfo = new PaypointModel()
                        {
                            description = stringBuilderDescriptions,
                            amountP = stringBuilderAmounts,
                            billNumber = billNumber,
                            refStan = refStan,
                            amount = amountpay,
                            transactionDate = exectransactionDate,
                            customerName = account,
                            companyCode = companyCode,
                            UserName = mobile,
                            ClientCode = ClientCode,
                            serviceCode = serviceCode

                        };
                        //end list of package

                        int resultsPayments = PaypointUtils.PaypointUtilityNTCInfo(resPaypointNTCPaymentInfo);
                        
                            
                    }
                    else
                    {
                        keyrlt = xElem.Attribute("Key").Value;
                        resultMessageResCP = xElem.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;
                    }
                    resultMessageResCP = xElem.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;

                    //for checkpayment response insert in database in nepal water
                    resCPPaypointNTCInfo = new PaypointModel()
                    {
                        companyCodeResCP = companyCode,
                        serviceCodeResCP = serviceCode,
                        accountResCP = account,
                        special1ResCP = special1,
                        special2ResCP = special2,

                        transactionDateResCP = transactionDate,
                        transactionIdResCP = transactionId,
                        userIdResCP = userId,
                        userPasswordResCP = userPassword,
                        salePointTypeResCP = salePointType,

                        refStanResCP = refStan,
                        amountResCP = amountpay,
                        billNumberResCP = billNumber,
                        //retrievalReferenceResCP = fundtransfer.tid,
                        retrievalReferenceResCP = tid,
                        responseCodeResCP = rltCheckPaymt,
                        descriptionResCP = "Check Payment " + keyrlt,
                        customerNameCP = customerName,
                        UserName = mobile,
                        ClientCode = ClientCode,
                        paypointType = paypointType,
                        resultMessageResCP = resultMessageResCP,

                    };
                }


                if (!(rltCheckPaymt == "000"))//show error when CP response is not 000 
                {
                    statusCode = "400";
                    // message = result;
                    message = resultMessageResCP;
                    failedmessage = message;

                }
                else
                {
                    statusCode = "200";
                    message = resultMessageResCP;
                    failedmessage = message;
                }
            }
            catch (Exception ex)
            {
                message = result + ex + "Error Message ";
                //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                statusCode = "400";
                // failedmessage = message;
                failedmessage = resultMessageResCP;

            }

            ///for  inserting CP PayPoint Data of Nepal Water which is simlar to nea 

            try
            {
                int resultsReqCP = PaypointUtils.RequestCPPaypointInfo(reqCPPaypointNTCInfo);
                int resultsResCP = PaypointUtils.ResponseCPPaypointInfo(resCPPaypointNTCInfo);


                if ((resultsReqCP > 0) && (resultsResCP > 0))

                {
                    message = result;
                }
                else
                {
                    message = result;
                }

            }
            catch (Exception ex)
            {
                string ss = ex.Message;
                message = result;
            }

            if (statusCode == "")
            {
                result = result.ToString();
            }
            else if (statusCode == "200")
            {
                if (message == "")
                {
                    if (string.IsNullOrEmpty(result))
                    {
                        result = failedmessage; // "Could not process your request,Please Try again.";
                    }
                    message = result;
                }
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = failedmessage,
                    retrievalRef = resCPPaypointNTCInfo.retrievalReferenceResCP,
                    refStanCK = resCPPaypointNTCInfo.refStanResCP,
                    description = pkg
                };
                result = JsonConvert.SerializeObject(v);
            }
            else if (statusCode != "200")
            {
                if (message == "")
                {
                    if (string.IsNullOrEmpty(result))
                    {
                        result = failedmessage; // "Could not process your request,Please Try again.";
                    }
                    message = result;
                }
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = failedmessage,
                    retrievalRef = "",
                    refStanCK = ""
                };
                result = JsonConvert.SerializeObject(v);
            }
            return result;

        }
        #endregion

        #region"NTC execute Paypoint"
        [OperationContract]
        [WebInvoke(Method = "POST",
                  ResponseFormat = WebMessageFormat.Json)]
        public string NTCExecutePayment(Stream input)
        {
            string PaypointPwd = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointPwd"];
            string PaypointUserID = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointUserID"];

            System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);
            MNTransactionMaster validTransactionData = new MNTransactionMaster();
            CustActivityModel custsmsInfo = new CustActivityModel();

            string tid = qs["tid"];
            string vid = qs["vid"]; //MerchantID
            string sc = qs["sc"];
            string mobile = qs["mobile"];
            string da = System.Web.Configuration.WebConfigurationManager.AppSettings["DestinationNoForTestServer"];


            string amount = qs["amount"];//amount paid by customer 
            string pin = qs["pin"];
            string note = "utility payment for NTC. Customer Name=" + qs["account"];//+ ". " + qs["note"];
            string src = qs["src"];
            string result = "";
            string sessionID = qs["tokenID"];
            string resultMessageResCP = "";
            string companyCode = qs["companyCode"];
            string serviceCode = qs["serviceCode"];
            string account = qs["account"];
            string special1 = qs["special1"]; //
            string special2 = ""; //
            string transactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");//"2019-11-22T11:11:02";
            long millisecondstrandId = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string transactionId = millisecondstrandId.ToString(); //"120163339819";
            string userId = PaypointUserID;
            string userPassword = PaypointPwd.Trim();
            string salePointType = "6";
            string transactionType = string.Empty;
            string amountpay = qs["amountpay"];//amount need to pay i.e amount in bill
            string exectransactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            string exectransactionId = millisecondstrandId.ToString();
            string refStan = qs["refStan"];
            string billNumber = qs["billNumber"];
            string rltCheckPaymt = qs["rltCheckPaymt"];
            string ClientCode = qs["ClientCode"];
            string paypointType = qs["paypointType"];
            //string paypointType = "Nepal Water";
            //string paypointType = "3";

            string customerName = qs["customerName"];
            string walletBalance = qs["walletBalance"];//in Rs.
            string retrievalReference = qs["retrievalReference"];
            int walletBalancePaisaInt = 0;
            walletBalancePaisaInt = Convert.ToInt32((Convert.ToDouble(walletBalance)) * 100);
            int amountpayInt = Convert.ToInt32(float.Parse(amount)) * 100;

            PaypointModel reqEPPaypointDishHomeInfo = new PaypointModel();
            PaypointModel resEPPaypointDishHomeInfo = new PaypointModel();

            PaypointModel reqGTPaypointDishHomeInfo = new PaypointModel();
            PaypointModel resGTPaypointDishHomeInfo = new PaypointModel();

            string totalAmount = string.Empty;
            string totalCount = string.Empty;
            string totalBAmount = string.Empty;
            string totalBCount = string.Empty;
            string balance = string.Empty;
            string message = string.Empty;
            string statusCode = string.Empty;
            string failedmessage = string.Empty;
            string GetMerchantName = string.Empty;
            string statusCodeBalance = string.Empty;

            string customerNo = string.Empty;

            // TraceIdGenerator traceid = new TraceIdGenerator();
            //tid = traceid.GenerateUniqueTraceID();
            tid = retrievalReference;

            FundTransfer fundtransfer = new FundTransfer
            {
                tid = tid,
                sc = sc,
                mobile = mobile,
                da = da,
                amount = amount,
                pin = pin,
                note = note,
                sourcechannel = src
            };
            if (sc == "00")
            {
                if (walletBalancePaisaInt >= amountpayInt)// if wallet balance less then bill amount then show error msg
                {
                    //First transaction MNRequest N Response
                    try
                    {

                        //FundTransfer fundtransfer = new FundTransfer
                        //{
                        //    tid = tid,
                        //    sc = sc,
                        //    mobile = mobile,
                        //    da = da,
                        //    amount = amount,
                        //    pin = pin,
                        //    note = note,
                        //    sourcechannel = src
                        //};

                        ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, fundtransfer);
                        //CustActivityModel custsmsInfo = new CustActivityModel();

                        //MNTransactionMaster validTransactionData = new MNTransactionMaster();


                        //if (TokenGenerator.TokenChecker(sessionID, mobile, src) == false)
                        //{
                        //    // throw ex
                        //    statusCode = "400";
                        //    message = "Session expired. Please login again";
                        //    failedmessage = message;
                        //}
                        //else
                        //{
                        if ((tid == null) || (sc == null) || (mobile == null) || (da == null) || (amount == null) || (pin == null) ||
                        (src == null) || (double.Parse(amount) <= 0) || (vid == null))
                        {
                            // throw ex
                            statusCode = "400";
                            message = "Parameters Missing/Invalid";
                            failedmessage = message;
                        }
                        if ((companyCode == null) || (companyCode == "") || (serviceCode == null) || (serviceCode == "") ||
                            (account == null) || (special1 == null) || (special2 == null) || (transactionDate == null) || (transactionId == null) || (transactionId == "") ||
                            (userId == null) || (userPassword == null) || (salePointType == null) || (userId == "") || (userPassword == "") || (salePointType == ""))
                        {
                            // throw ex
                            statusCode = "400";
                            message = "Parameters Missing/Invalid PayPoint";
                            failedmessage = message;
                        }
                        else
                        {
                            if (sc == "00")
                            {
                                transactionType = "PayPoint Txfr to W2W";
                            }
                            else if (sc == "10")
                            {
                                transactionType = "PayPoint Txfr to B2W"; //B2W
                            }

                            if (!(UserNameCheck.IsValidUser(mobile)))
                            {
                                // throw ex
                                statusCode = "400";
                                //message = "Transaction restricted to User";
                                message = "Transaction only for User";
                                failedmessage = message;
                            }
                            if (UserNameCheck.IsValidMerchant(da))
                            {

                                TransLimitCheck transLimitCheck = new TransLimitCheck();
                                string resultTranLimit = transLimitCheck.LimitCheck(mobile, da, amount, sc, pin, src);

                                var jsonDataResult = JObject.Parse(resultTranLimit);
                                statusCode = jsonDataResult["StatusCode"].ToString();
                                string statusMsg = jsonDataResult["StatusMessage"].ToString();
                                message = jsonDataResult["StatusMessage"].ToString();
                                failedmessage = message;

                                if ((statusCode == "200") && (message == "Success"))
                                {
                                    //start: checking trace id
                                    do
                                    {
                                        //TraceIdGenerator traceid = new TraceIdGenerator();
                                        // tid = traceid.GenerateUniqueTraceID();

                                        fundtransfer.tid = tid;
                                        bool traceIdCheck = false;
                                        traceIdCheck = TraceIdCheck.IsValidTraceId(fundtransfer.tid);
                                        if (traceIdCheck == true)
                                        {
                                            result = "Trace ID Repeated";
                                        }
                                        else
                                        {
                                            result = "false";
                                        }

                                    } while (result == "Trace ID Repeated");
                                    //End: TraceId

                                    //start:Com focus one log///
                                    MNFundTransfer mnft = new MNFundTransfer(tid, fundtransfer.sc, fundtransfer.mobile,
                                        fundtransfer.sa, fundtransfer.amount, fundtransfer.da, fundtransfer.note, fundtransfer.pin,
                                        fundtransfer.sourcechannel);
                                    var comfocuslog = new MNComAndFocusOneLog(mnft, DateTime.Now);
                                    var mncomfocuslog = new MNComAndFocusOneLogsController();
                                    //mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                    result = mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                    //end:Com focus one log//

                                    if (result == "Success")
                                    {
                                        //NOTE:- may be need to validate before insert into reply typpe
                                        //start:insert into reply type as HTTP//
                                        var replyType = new MNReplyType(tid, "HTTP");
                                        var mnreplyType = new MNReplyTypesController();
                                        mnreplyType.InsertIntoReplyType(replyType);
                                        //end:insert into reply type as HTTP//

                                        MNMerchantsController getMerchantDetails = new MNMerchantsController();

                                        string GetMerchantMobile = getMerchantDetails.PassVidToGetMerchantDetail(vid);
                                        if (GetMerchantMobile != "" || GetMerchantMobile != null)
                                        {
                                            fundtransfer.da = GetMerchantMobile; //Set Destination Merchant Mobile number
                                            GetMerchantName = getMerchantDetails.PassVIdToGetMerchantName(vid);
                                        }
                                        else
                                        {
                                            statusCode = "400";
                                            message = "Destination Merchant Doesnot Exists";
                                            mnft.Response = "Destination Merchant Doesnot Exists";
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, mnft.Response);
                                            result = mnft.Response;
                                            failedmessage = message;
                                        }


                                        //start:insert into transaction master//
                                        if (mnft.valid())
                                        {
                                            //var transaction = new MNTransactionMaster(mnft);
                                            //var mntransaction = new MNTransactionsController();
                                            //validTransactionData = mntransaction.Validate(transaction, mnft.pin);

                                            var transactionpaypoint = new MNTransactionMaster(mnft);
                                            var mntransactionpaypoint = new MNTransactionsController();
                                            validTransactionData = mntransactionpaypoint.Validatepaypoint(transactionpaypoint, mnft.pin);

                                            result = validTransactionData.Response;
                                            /*** ***/
                                            ErrorMessage em = new ErrorMessage();

                                            if (validTransactionData.Response == "Error")
                                            {
                                                mnft.Response = "error";
                                                mnft.ResponseStatus(HttpStatusCode.InternalServerError,
                                                    "Internal server error - try again later, or contact support");
                                                result = mnft.Response;
                                                statusCode = "500";
                                                message = "Internal server error - try again later, or contact support";
                                                failedmessage = message;
                                            }
                                            else
                                            {
                                                if ((result == "Trace ID Repeated") || (result == "Limit Exceed")
                                                    || (result == "Invalid Source User") || (result == "Invalid Destination User")
                                                    || (result == "Invalid Product Request") || (result == "Please try again") || (result == ""))
                                                {
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                    statusCode = "400";
                                                    message = result;
                                                    failedmessage = message;
                                                }
                                                if (result == "Invalid PIN")
                                                {
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                    statusCode = "400";
                                                    message = result;
                                                    failedmessage = message;
                                                }
                                                if (result == "111")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_111;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "114")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_114;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "115")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_115;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "116")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_116;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "119")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_119;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "121")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_121;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "163")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_163;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "180")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_180;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "181")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_181;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "182")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_182;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "183")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_183;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "184")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_184;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "185")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_185;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "186")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_186;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "187")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_187;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "188")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_188;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "189")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_189;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "190")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_190;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "800")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_800;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "902")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_902;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "904")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_904;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "906")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_906;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "907")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_907;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "909")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_909;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "911")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_911;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "913")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_913;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "90")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_90;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "91")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_91;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "92")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_92;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "94")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_94;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "95")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_95;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "98")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_98;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "99")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_99;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "508")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_508;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                else if (validTransactionData.ResponseCode == "OK")
                                                {
                                                    statusCode = "200";
                                                    message = result;
                                                    mnft.ResponseStatus(HttpStatusCode.OK, message);

                                                } //END ValidTransactionData.ResponseCode


                                            } //END validTransactionData.Response WITHOUT MNDB ERROR

                                            //start comment outgoing
                                            //OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                                            //if (response.StatusCode == HttpStatusCode.OK)
                                            //{
                                            //    string messagereply = "";
                                            //    try
                                            //    {
                                            //        //messagereply = "Dear " + CustCheckUtils.GetName(mobile) + "," + "\n";

                                            //        //messagereply += " You have successfully paid NPR " + validTransactionData.Amount
                                            //        //                + " to " + GetMerchantName + " on date " +
                                            //        //                    (validTransactionData.CreatedDate).ToString("dd/MM/yyyy")
                                            //        //                + "." + "\n";
                                            //        //messagereply += "Thank you. MNepal";

                                            //        //var client = new WebClient();

                                            //        ////SENDER
                                            //        //if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                                            //        //{
                                            //        //    //FOR NCELL
                                            //        //    var content = client.DownloadString(
                                            //        //        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                            //        //        + "977" + mobile + "&Text=" + messagereply + "");
                                            //        //}
                                            //        //else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                            //        //            || (mobile.Substring(0, 3) == "986"))
                                            //        //{
                                            //        //    //FOR NTC
                                            //        //    var content = client.DownloadString(
                                            //        //        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                            //        //        + "977" + mobile + "&Text=" + messagereply + "");
                                            //        //}

                                            //        statusCode = "200";
                                            //        var v = new
                                            //        {
                                            //            StatusCode = Convert.ToInt32(statusCode),
                                            //            StatusMessage = result
                                            //        };
                                            //        result = JsonConvert.SerializeObject(v);

                                            //    }
                                            //    catch (Exception ex)
                                            //    {
                                            //        // throw ex
                                            //        statusCode = "400";
                                            //        message = ex.Message;
                                            //    }


                                            //    custsmsInfo = new CustActivityModel()
                                            //    {
                                            //        UserName = fundtransfer.mobile,
                                            //        RequestMerchant = transactionType,
                                            //        DestinationNo = fundtransfer.da,
                                            //        Amount = validTransactionData.Amount.ToString(),
                                            //        SMSStatus = "Success",
                                            //        SMSSenderReply = messagereply,
                                            //        ErrorMessage = "",
                                            //    };


                                            //}
                                            //else if ((response.StatusCode == HttpStatusCode.BadRequest) || (response.StatusCode == HttpStatusCode.InternalServerError) || (statusCode != "200"))
                                            //{
                                            //    custsmsInfo = new CustActivityModel()
                                            //    {
                                            //        UserName = mobile,
                                            //        RequestMerchant = transactionType,
                                            //        DestinationNo = fundtransfer.da,
                                            //        Amount = validTransactionData.Amount.ToString(),
                                            //        SMSStatus = "Failed",
                                            //        SMSSenderReply = message,
                                            //        ErrorMessage = failedmessage,
                                            //    };

                                            //}

                                            //end comment outgoint
                                            //end:insert into transaction master//

                                        } //END:insert into transaction master//
                                        else
                                        {
                                            mnft.Response = "error";
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, "parameters missing/invalid"); //200 - OK
                                            result = mnft.Response;
                                            statusCode = "400";
                                            message = "parameters missing/invalid";
                                            failedmessage = message;

                                            custsmsInfo = new CustActivityModel()
                                            {
                                                UserName = mobile,
                                                RequestMerchant = transactionType,
                                                DestinationNo = fundtransfer.da,
                                                Amount = amount,
                                                SMSStatus = "Failed",
                                                SMSSenderReply = message,
                                                ErrorMessage = failedmessage,
                                            };
                                        }

                                    } //END MNComAndFocusOneLogsController
                                    else
                                    {
                                        statusCode = "400";
                                        mnft.Response = "Data Insertion Failed in Check DB Connection: TraceID limit might have exceeded";
                                        mnft.ResponseStatus(HttpStatusCode.InternalServerError, mnft.Response);
                                        result = mnft.Response;
                                        failedmessage = result;
                                    }

                                } //END TRansLimit Check StatusCode N Message
                                else
                                {
                                    custsmsInfo = new CustActivityModel()
                                    {
                                        UserName = mobile,
                                        RequestMerchant = transactionType,
                                        DestinationNo = fundtransfer.da,
                                        Amount = amount,
                                        SMSStatus = "Failed",
                                        SMSSenderReply = message,
                                        ErrorMessage = failedmessage,
                                    };
                                }

                            } //END IsValidMerchant

                            //} //END Destination mobile No check

                        }

                    }
                    catch (Exception ex)
                    {
                        message = result + ex + "Error Message ";
                        //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                        statusCode = "400";
                        //failedmessage = message;
                        failedmessage = "Please try again.";
                    }

                }
                else  //else for  if wallet balance less then bill amount then show error msg
                {
                    statusCodeBalance = "400";
                    message = "Insufficient Balance";
                    failedmessage = message;

                }
            }
            else if (sc == "10")//for bank payment in nepal water
            {
                //if (bankBalancePaisaInt >= amountpayInt)
                //{
                //First transaction MNRequest N Response
                try
                {

                    //FundTransfer fundtransfer = new FundTransfer
                    //{
                    //    tid = tid,
                    //    sc = sc,
                    //    mobile = mobile,
                    //    da = da,
                    //    amount = amount,
                    //    pin = pin,
                    //    note = note,
                    //    sourcechannel = src
                    //};

                    ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, fundtransfer);
                    //CustActivityModel custsmsInfo = new CustActivityModel();

                    //MNTransactionMaster validTransactionData = new MNTransactionMaster();


                    //if (TokenGenerator.TokenChecker(sessionID, mobile, src) == false)
                    //{
                    //    // throw ex
                    //    statusCode = "400";
                    //    message = "Session expired. Please login again";
                    //    failedmessage = message;
                    //}
                    //else
                    //{
                    if ((tid == null) || (sc == null) || (mobile == null) || (da == null) || (amount == null) || (pin == null) ||
                    (src == null) || (double.Parse(amount) <= 0) || (vid == null))
                    {
                        // throw ex
                        statusCode = "400";
                        message = "Parameters Missing/Invalid";
                        failedmessage = message;
                    }
                    if ((companyCode == null) || (companyCode == "") || (serviceCode == null) || (serviceCode == "") ||
                        (account == null) || (special1 == null) || (special2 == null) || (transactionDate == null) || (transactionId == null) || (transactionId == "") ||
                        (userId == null) || (userPassword == null) || (salePointType == null) || (userId == "") || (userPassword == "") || (salePointType == ""))
                    {
                        // throw ex
                        statusCode = "400";
                        message = "Parameters Missing/Invalid PayPoint";
                        failedmessage = message;
                    }
                    else
                    {
                        if (sc == "00")
                        {
                            transactionType = "PayPoint Txfr to W2W";
                        }
                        else if (sc == "10")
                        {
                            transactionType = "PayPoint Txfr to B2W"; //B2W
                        }

                        if (!(UserNameCheck.IsValidUser(mobile)))
                        {
                            // throw ex
                            statusCode = "400";
                            //message = "Transaction restricted to User";
                            message = "Transaction only for User";
                            failedmessage = message;
                        }
                        if (UserNameCheck.IsValidMerchant(da))
                        {

                            TransLimitCheck transLimitCheck = new TransLimitCheck();
                            string resultTranLimit = transLimitCheck.LimitCheck(mobile, da, amount, sc, pin, src);

                            var jsonDataResult = JObject.Parse(resultTranLimit);
                            statusCode = jsonDataResult["StatusCode"].ToString();
                            string statusMsg = jsonDataResult["StatusMessage"].ToString();
                            message = jsonDataResult["StatusMessage"].ToString();
                            failedmessage = message;

                            if ((statusCode == "200") && (message == "Success"))
                            {
                                //start: checking trace id
                                do
                                {
                                    //TraceIdGenerator traceid = new TraceIdGenerator();
                                    // tid = traceid.GenerateUniqueTraceID();
                                    fundtransfer.tid = tid;

                                    bool traceIdCheck = false;
                                    traceIdCheck = TraceIdCheck.IsValidTraceId(fundtransfer.tid);
                                    if (traceIdCheck == true)
                                    {
                                        result = "Trace ID Repeated";
                                    }
                                    else
                                    {
                                        result = "false";
                                    }

                                } while (result == "Trace ID Repeated");
                                //End: TraceId

                                //start:Com focus one log///
                                MNFundTransfer mnft = new MNFundTransfer(tid, fundtransfer.sc, fundtransfer.mobile,
                                    fundtransfer.sa, fundtransfer.amount, fundtransfer.da, fundtransfer.note, fundtransfer.pin,
                                    fundtransfer.sourcechannel);
                                var comfocuslog = new MNComAndFocusOneLog(mnft, DateTime.Now);
                                var mncomfocuslog = new MNComAndFocusOneLogsController();
                                //mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                result = mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                //end:Com focus one log//

                                if (result == "Success")
                                {
                                    //NOTE:- may be need to validate before insert into reply typpe
                                    //start:insert into reply type as HTTP//
                                    var replyType = new MNReplyType(tid, "HTTP");
                                    var mnreplyType = new MNReplyTypesController();
                                    mnreplyType.InsertIntoReplyType(replyType);
                                    //end:insert into reply type as HTTP//

                                    MNMerchantsController getMerchantDetails = new MNMerchantsController();

                                    string GetMerchantMobile = getMerchantDetails.PassVidToGetMerchantDetail(vid);
                                    if (GetMerchantMobile != "" || GetMerchantMobile != null)
                                    {
                                        fundtransfer.da = GetMerchantMobile; //Set Destination Merchant Mobile number
                                        GetMerchantName = getMerchantDetails.PassVIdToGetMerchantName(vid);
                                    }
                                    else
                                    {
                                        statusCode = "400";
                                        message = "Destination Merchant Doesnot Exists";
                                        mnft.Response = "Destination Merchant Doesnot Exists";
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, mnft.Response);
                                        result = mnft.Response;
                                        failedmessage = message;
                                    }


                                    //start:insert into transaction master//
                                    if (mnft.valid())
                                    {
                                        //var transaction = new MNTransactionMaster(mnft);
                                        //var mntransaction = new MNTransactionsController();
                                        //validTransactionData = mntransaction.Validate(transaction, mnft.pin);


                                        var transactionpaypoint = new MNTransactionMaster(mnft);
                                        var mntransactionpaypoint = new MNTransactionsController();
                                        validTransactionData = mntransactionpaypoint.Validatepaypoint(transactionpaypoint, mnft.pin);
                                        result = validTransactionData.Response;
                                        /*** ***/
                                        ErrorMessage em = new ErrorMessage();

                                        if (validTransactionData.Response == "Error")
                                        {
                                            mnft.Response = "error";
                                            mnft.ResponseStatus(HttpStatusCode.InternalServerError,
                                                "Internal server error - try again later, or contact support");
                                            result = mnft.Response;
                                            statusCode = "500";
                                            message = "Internal server error - try again later, or contact support";
                                            failedmessage = message;
                                        }
                                        else
                                        {
                                            if ((result == "Trace ID Repeated") || (result == "Limit Exceed")
                                                || (result == "Invalid Source User") || (result == "Invalid Destination User")
                                                || (result == "Invalid Product Request") || (result == "Please try again") || (result == ""))
                                            {
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                statusCode = "400";
                                                message = result;
                                                failedmessage = message;
                                            }
                                            if (result == "Invalid PIN")
                                            {
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                statusCode = "400";
                                                message = result;
                                                failedmessage = message;
                                            }
                                            if (result == "111")
                                            {
                                                statusCode = result;
                                                message = em.Error_111;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "114")
                                            {
                                                statusCode = result;
                                                message = em.Error_114;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "115")
                                            {
                                                statusCode = result;
                                                message = em.Error_115;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "116")
                                            {
                                                statusCode = result;
                                                message = em.Error_116;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "119")
                                            {
                                                statusCode = result;
                                                message = em.Error_119;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "121")
                                            {
                                                statusCode = result;
                                                message = em.Error_121;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "163")
                                            {
                                                statusCode = result;
                                                message = em.Error_163;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "180")
                                            {
                                                statusCode = result;
                                                message = em.Error_180;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "181")
                                            {
                                                statusCode = result;
                                                message = em.Error_181;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "182")
                                            {
                                                statusCode = result;
                                                message = em.Error_182;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "183")
                                            {
                                                statusCode = result;
                                                message = em.Error_183;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "184")
                                            {
                                                statusCode = result;
                                                message = em.Error_184;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "185")
                                            {
                                                statusCode = result;
                                                message = em.Error_185;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "186")
                                            {
                                                statusCode = result;
                                                message = em.Error_186;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "187")
                                            {
                                                statusCode = result;
                                                message = em.Error_187;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "188")
                                            {
                                                statusCode = result;
                                                message = em.Error_188;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "189")
                                            {
                                                statusCode = result;
                                                message = em.Error_189;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "190")
                                            {
                                                statusCode = result;
                                                message = em.Error_190;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "800")
                                            {
                                                statusCode = result;
                                                message = em.Error_800;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "902")
                                            {
                                                statusCode = result;
                                                message = em.Error_902;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "904")
                                            {
                                                statusCode = result;
                                                message = em.Error_904;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "906")
                                            {
                                                statusCode = result;
                                                message = em.Error_906;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "907")
                                            {
                                                statusCode = result;
                                                message = em.Error_907;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "909")
                                            {
                                                statusCode = result;
                                                message = em.Error_909;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "911")
                                            {
                                                statusCode = result;
                                                message = em.Error_911;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "913")
                                            {
                                                statusCode = result;
                                                message = em.Error_913;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "90")
                                            {
                                                statusCode = result;
                                                message = em.Error_90;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "91")
                                            {
                                                statusCode = result;
                                                message = em.Error_91;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "92")
                                            {
                                                statusCode = result;
                                                message = em.Error_92;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "94")
                                            {
                                                statusCode = result;
                                                message = em.Error_94;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "95")
                                            {
                                                statusCode = result;
                                                message = em.Error_95;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "98")
                                            {
                                                statusCode = result;
                                                message = em.Error_98;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "99")
                                            {
                                                statusCode = result;
                                                message = em.Error_99;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "508")
                                            {
                                                statusCode = result;
                                                message = em.Error_508;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            else if (validTransactionData.ResponseCode == "OK")
                                            {
                                                statusCode = "200";
                                                message = result;
                                                mnft.ResponseStatus(HttpStatusCode.OK, message);

                                            } //END ValidTransactionData.ResponseCode


                                        } //END validTransactionData.Response WITHOUT MNDB ERROR

                                        //end:insert into transaction master//

                                    } //END:insert into transaction master//
                                    else
                                    {
                                        mnft.Response = "error";
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, "parameters missing/invalid"); //200 - OK
                                        result = mnft.Response;
                                        statusCode = "400";
                                        message = "parameters missing/invalid";
                                        failedmessage = message;

                                        custsmsInfo = new CustActivityModel()
                                        {
                                            UserName = mobile,
                                            RequestMerchant = transactionType,
                                            DestinationNo = fundtransfer.da,
                                            Amount = amount,
                                            SMSStatus = "Failed",
                                            SMSSenderReply = message,
                                            ErrorMessage = failedmessage,
                                        };
                                    }

                                } //END MNComAndFocusOneLogsController
                                else
                                {
                                    statusCode = "400";
                                    mnft.Response = "Data Insertion Failed in Check DB Connection: TraceID limit might have exceeded";
                                    mnft.ResponseStatus(HttpStatusCode.InternalServerError, mnft.Response);
                                    result = mnft.Response;
                                    failedmessage = result;
                                }

                            } //END TRansLimit Check StatusCode N Message
                            else
                            {
                                custsmsInfo = new CustActivityModel()
                                {
                                    UserName = mobile,
                                    RequestMerchant = transactionType,
                                    DestinationNo = fundtransfer.da,
                                    Amount = amount,
                                    SMSStatus = "Failed",
                                    SMSSenderReply = message,
                                    ErrorMessage = failedmessage,
                                };
                            }

                        } //END IsValidMerchant


                        //} //END Destination mobile No check

                    }

                }
                catch (Exception ex)
                {
                    message = result + ex + "Error Message ";
                    //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                    statusCode = "400";
                    //failedmessage = message;
                    failedmessage = "Please try again.";
                }
                //}
                //else  //else for  if wallet balance less then bill amount then show error msg
                //{
                //    statusCodeBalance = "400";
                //    message = "Insufficient Balance";
                //    failedmessage = message;

                //}
            }
            try
            {

                //for  all EP  and GT transaction
                string compResultResp = "";

                if (statusCode == "200")
                {
                    try
                    {
                        //string amountInPaisa = ((double.Parse(amount)) * 100).ToString();

                        if (rltCheckPaymt == "000")//go if CP Response is 000
                        {
                            //int amountInPaisaInt = Convert.ToInt32(amountInPaisa);

                            long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                            exectransactionId = milliseconds.ToString();

                            string keyExecRlt = "";
                            string resultMessageResEP = "";
                            do
                            {
                                if ((companyCode == null) || (serviceCode == null) || (account == null) ||
                                (exectransactionDate == null) || (exectransactionId == null) ||
                                (refStan == null) || (amount == null) || (billNumber == null) ||
                                (userId == null) || (userPassword == null) || (salePointType == null))
                                {
                                    // throw ex
                                    statusCode = "400";
                                    message = "Parameters Missing/Invalid";
                                    failedmessage = message;
                                }
                                else
                                {
                                    if (companyCode == "598")
                                    {
                                        special1 = "";
                                    }
                                    else
                                    {
                                        special1 = special1.ToString();
                                    }

                                    // string URIEXECPayment = "https://test.paypoint.md:4445/PayPointWS/PayPointMSOperations.asmx/ExecutePayment";//for EP Link 

                                    //For excutepaypoint link in webconfig
                                    string URIEXECPayment = System.Web.Configuration.WebConfigurationManager.AppSettings["EPPaypointUrl"];

                                    string execPayParameters = "companyCode=" + companyCode + "&serviceCode=" + serviceCode +
                                            "&account=" + account + "&special1=" + special1 + "&special2=" + special2 +
                                            "&transactionDate=" + exectransactionDate + "&transactionId=" + exectransactionId +
                                            "&refStan=" + refStan + "&amount=" + Convert.ToInt32(float.Parse(amount) * 100).ToString() + "&billNumber=" + billNumber +
                                            "&userId=" + userId.Trim() + "&userPassword=" + userPassword.Trim() + "&salePointType=" + salePointType;

                                    //for executepayment request insert in database for wlink
                                    reqEPPaypointDishHomeInfo = new PaypointModel()
                                    {
                                        companyCodeReqEP = companyCode,
                                        serviceCodeReqEP = serviceCode,
                                        accountReqEP = account,
                                        special1ReqEP = special1,
                                        special2ReqEP = special2,

                                        transactionDateReqEP = exectransactionDate,
                                        transactionIdReqEP = exectransactionId,
                                        userIdReqEP = userId,
                                        userPasswordReqEP = userPassword,
                                        salePointTypeReqEP = salePointType,

                                        refStanReqEP = refStan,
                                        amountReqEP = amount,
                                        billNumberReqEP = billNumber,
                                        //retrievalReferenceReqEP = fundtransfer.tid,
                                        //retrievalReferenceReqEP = tid,
                                        retrievalReferenceReqEP = retrievalReference,
                                        remarkReqEP = "Execute Payment",
                                        UserName = mobile,
                                        ClientCode = ClientCode,
                                        paypointType = paypointType,

                                    };

                                    using (WebClient wcExecPay = new WebClient())
                                    {
                                        wcExecPay.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                                        string HtmlResultExecPay = wcExecPay.UploadString(URIEXECPayment, execPayParameters);

                                        XmlDocument xmlEDoc = new XmlDocument();
                                        xmlEDoc.LoadXml(HtmlResultExecPay);

                                        XmlNodeList nodeEPay = xmlEDoc.GetElementsByTagName("*");
                                        string resultEPay = nodeEPay[0].InnerText;
                                        string HtmlEPayResult = resultEPay;

                                        //for determining  checkpayment key and result
                                        var readerEPay = new StringReader(HtmlEPayResult);
                                        var xdocEPay = XDocument.Load(readerEPay);

                                        XDocument docEPay = XDocument.Parse(xdocEPay.ToString());

                                        var xElemEPay = XElement.Parse(xdocEPay.ToString());

                                        if (xElemEPay.Attribute("Result").Value == "000")
                                        {
                                            compResultResp = xElemEPay.Attribute("Result").Value;
                                            keyExecRlt = xElemEPay.Attribute("Key").Value;
                                        }
                                        else if ((xElemEPay.Attribute("Result").Value == "011") || (xElemEPay.Attribute("Result").Value == "012"))
                                        {
                                            compResultResp = xElemEPay.Attribute("Result").Value;
                                            keyExecRlt = xElemEPay.Attribute("Key").Value;
                                        }
                                        else
                                        {
                                            compResultResp = xElemEPay.Attribute("Result").Value;
                                            keyExecRlt = xElemEPay.Attribute("Key").Value;
                                        }
                                        resultMessageResEP = xElemEPay.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;



                                        //for Response Execute Payment
                                        resEPPaypointDishHomeInfo = new PaypointModel()
                                        {
                                            companyCodeResEP = companyCode,
                                            serviceCodeResEP = serviceCode,
                                            accountResEP = account,
                                            special1ResEP = special1,
                                            special2ResEP = special2,

                                            transactionDateResEP = transactionDate,
                                            transactionIdResEP = transactionId,
                                            userIdResEP = userId,

                                            userPasswordResEP = userPassword,
                                            salePointTypeResEP = salePointType,

                                            refStanResEP = refStan,
                                            amountResEP = amount,
                                            billNumberResEP = billNumber,
                                            //retrievalReferenceResEP = fundtransfer.tid,
                                            //retrievalReferenceResEP = tid,
                                            retrievalReferenceResEP = retrievalReference,
                                            responseCodeResEP = compResultResp,
                                            descriptionResEP = "Execute Payment" + keyExecRlt,
                                            UserName = mobile,
                                            ClientCode = ClientCode,
                                            paypointType = paypointType,
                                            resultMessageResEP = resultMessageResEP,
                                            customerNameResEP = customerName,
                                        };
                                    }
                                }

                            } while ((compResultResp == "011") || (compResultResp == "012"));

                            ///for  Inserting EP PayPoint Data for wlink

                            try
                            {
                                int resultsReqEP = PaypointUtils.RequestEPPaypointInfo(reqEPPaypointDishHomeInfo);
                                int resultsResEP = PaypointUtils.ResponseEPPaypointInfo(resEPPaypointDishHomeInfo);

                                if ((resultsReqEP > 0) && (resultsResEP > 0))
                                {
                                    message = result;
                                }
                                else
                                {
                                    message = result;
                                }

                            }
                            catch (Exception ex)
                            {
                                string ss = ex.Message;
                                message = result;
                            }

                            if (compResultResp == "000")
                            {
                                if ((refStan == null) || (userId == null) || (userPassword == null) || (salePointType == null))
                                {
                                    // throw ex
                                    statusCode = "400";
                                    message = "Parameters Missing/Invalid";
                                    failedmessage = message;
                                }
                                else if (compResultResp != "")
                                {
                                    string statusResGTP = "1";
                                    do
                                    {
                                        string key = "";
                                        string gtBillNumber = "";
                                        // string URIGetTran = "https://test.paypoint.md:4445/PayPointWS/PayPointMSOperations.asmx/GetTransaction";//for GT paypoint transactionlink

                                        //For get transactionpaypoint link in webconfig
                                        string URIGetTran = System.Web.Configuration.WebConfigurationManager.AppSettings["GTPPaypointUrl"];

                                        string GetTranParameters = "userLogin=" + userId + "&userPassword=" + userPassword + "&stan=" + "-1" + "&refStan=" + refStan + "&key=" + "" + "&billNumber=" + gtBillNumber;

                                        //for get transaction payment request insert in database
                                        reqGTPaypointDishHomeInfo = new PaypointModel()
                                        {
                                            companyCodeReqGTP = companyCode,
                                            serviceCodeReqGTP = serviceCode,
                                            accountReqGTP = account,
                                            special1ReqGTP = special1,
                                            special2ReqGTP = special2,

                                            transactionDateReqGTP = transactionDate,
                                            transactionIdReqGTP = transactionId,
                                            userIdReqGTP = userId,
                                            userPasswordReqGTP = userPassword,
                                            salePointTypeReqGTP = salePointType,

                                            refStanReqGTP = refStan,
                                            amountReqGTP = amount,
                                            billNumberReqGTP = gtBillNumber,
                                            // retrievalReferenceReqGTP = fundtransfer.tid,
                                            //retrievalReferenceReqGTP = tid,
                                            retrievalReferenceReqGTP = retrievalReference,
                                            remarkReqGTP = "Get Transaction Payment",
                                            UserName = mobile,
                                            ClientCode = ClientCode,
                                            paypointType = paypointType,
                                            //remarkReqGTP = "Get Transaction Payment"+keyExecRlt,

                                        };
                                        string getTranResultResp = "";
                                        string keyGetTrancRlt = "";
                                        string resultMessageResGTP = "";
                                        string billNumberResGTP = "";
                                        using (WebClient wcGetTran = new WebClient())
                                        {
                                            wcGetTran.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                                            string HtmlResultGetTran = wcGetTran.UploadString(URIGetTran, GetTranParameters);

                                            XmlDocument xmlEDoc = new XmlDocument();
                                            xmlEDoc.LoadXml(HtmlResultGetTran);

                                            XmlNodeList nodeEPay = xmlEDoc.GetElementsByTagName("*");
                                            string resultEPay = nodeEPay[0].InnerText;
                                            string HtmlEPayResult = resultEPay;

                                            //for determing excutepayment key and result
                                            var readerEPay = new StringReader(HtmlEPayResult);
                                            var xdocEPay = XDocument.Load(readerEPay);

                                            XDocument docEPay = XDocument.Parse(xdocEPay.ToString());
                                            var xElemGPay = XElement.Parse(docEPay.ToString());
                                            if (xElemGPay.Attribute("Result").Value == "000")
                                            {
                                                getTranResultResp = xElemGPay.Attribute("Result").Value;
                                                keyGetTrancRlt = xElemGPay.Attribute("Key").Value;
                                                resultMessageResGTP = xElemGPay.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;
                                                if (!(resultMessageResGTP == "No data"))
                                                {
                                                    billNumberResGTP = xElemGPay.Descendants().Elements("BillNumber").Where(x => x.Name == "BillNumber").SingleOrDefault().Value;
                                                    statusResGTP = xElemGPay.Descendants().Elements("Status").Where(x => x.Name == "Status").SingleOrDefault().Value;

                                                    //for get transaction payment status validation
                                                    if (!(statusResGTP == "1" || statusResGTP == "5" || statusResGTP == "11" || statusResGTP == "13"))
                                                    {
                                                        if (statusResGTP == "4" || statusResGTP == "6" || statusResGTP == "14" || statusResGTP == "16")
                                                        {
                                                            // mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                            statusCode = "400";
                                                            //message = result;
                                                            message = "Failed";
                                                            failedmessage = message;
                                                            resultMessageResGTP = "failed";
                                                        }

                                                    }
                                                    else
                                                    {
                                                        statusCode = "200";
                                                        resultMessageResGTP = "sucess";
                                                    }
                                                }
                                                else
                                                {
                                                    //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                    statusCode = "400";
                                                    //message = result;
                                                    message = "Failed";
                                                    failedmessage = message;
                                                    resultMessageResGTP = "failed";
                                                }
                                            }
                                            else
                                            {
                                                resultMessageResGTP = xElemGPay.Descendants().Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;

                                            }


                                            //end get transaction payment status validation

                                            ////for get transaction payment response insert in database
                                            resGTPaypointDishHomeInfo = new PaypointModel()
                                            {
                                                companyCodeResGTP = companyCode,
                                                serviceCodeResGTP = serviceCode,
                                                accountResGTP = account,
                                                special1ResGTP = special1,
                                                special2ResGTP = special2,

                                                transactionDateResGTP = transactionDate,
                                                transactionIdResGTP = transactionId,
                                                userIdResGTP = userId,
                                                userPasswordResGTP = userPassword,
                                                salePointTypeResGTP = salePointType,

                                                refStanResGTP = refStan,
                                                amountResGTP = amount, //amountpay,
                                                billNumberResGTP = billNumberResGTP,
                                                //retrievalReferenceResGTP = fundtransfer.tid,
                                                //retrievalReferenceResGTP = tid,
                                                retrievalReferenceResGTP = retrievalReference,
                                                responseCodeResGTP = getTranResultResp,
                                                descriptionResGTP = "Get Transaction Payment " + keyGetTrancRlt,
                                                UserName = mobile,
                                                ClientCode = ClientCode,
                                                paypointType = paypointType,
                                                resultMessageResGTP = resultMessageResGTP,
                                                customerNameResGTP = customerName,

                                            };
                                        }
                                    } while (statusResGTP == "10" || statusResGTP == "15" || statusResGTP == "20" || statusResGTP == "21" || statusResGTP == "99" || statusResGTP == "12" || statusResGTP == "2" || statusResGTP == "0");

                                    ///for  Inserting GT PayPoint Data

                                    try
                                    {

                                        int resultsReqGTP = PaypointUtils.RequestGTPaypointInfo(reqGTPaypointDishHomeInfo);
                                        int resultsResGTP = PaypointUtils.ResponseGTPaypointInfo(resGTPaypointDishHomeInfo);

                                        if ((resultsReqGTP > 0) && (resultsResGTP > 0))
                                        {
                                            message = result;
                                        }
                                        else
                                        {
                                            message = result;
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        string ss = ex.Message;
                                        message = result;
                                    }

                                }

                            }

                            else
                            {
                                //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                statusCode = "400";
                                //message = result;
                                message = resultMessageResEP;
                                failedmessage = message;
                            }

                        }
                        else //ELSE FOR (rltCheckPaymt == "000")I.E Error  in result of CP
                        {
                            //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                            statusCode = "400";
                            // message = result;
                            message = resultMessageResCP;

                            failedmessage = message;

                            ////start:Com focus one log///
                            //MNFundTransfer mnft1 = new MNFundTransfer(tid, fundtransfer.sc, fundtransfer.da,
                            //    fundtransfer.sa, fundtransfer.amount, fundtransfer.mobile, fundtransfer.note, fundtransfer.pin,
                            //    fundtransfer.sourcechannel);
                            //var comfocuslog1 = new MNComAndFocusOneLog(mnft1, DateTime.Now);
                            //var mncomfocuslog1 = new MNComAndFocusOneLogsController();
                            //result = mncomfocuslog1.InsertIntoComFocusOne(comfocuslog1);
                            ////end:Com focus one log//

                            //if (mnft1.valid())
                            //{
                            //    var transaction1 = new MNTransactionMaster(mnft1);
                            //    var mntransaction1 = new MNTransactionsController();
                            //    validTransactionData = mntransaction1.Validate(transaction1, mnft1.pin);
                            //    result = validTransactionData.Response;

                            //}
                        }
                        //end excute dekhi get ko sabai comment gareko
                    }
                    catch (Exception ex)
                    {
                        message = result + ex + "Error Message ";
                        //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                        statusCode = "400";
                        failedmessage = "Please try again.";
                    }
                }

                //REverse Transaction 
                if ((statusCode != "111") && (statusCode != "114") && (statusCode != "115") && (statusCode != "116") && (statusCode != "119") &&
                    (statusCode != "121") && (statusCode != "163") && (statusCode != "180") && (statusCode != "181") && (statusCode != "182") &&
                    (statusCode != "183") && (statusCode != "184") && (statusCode != "185") && (statusCode != "186") && (statusCode != "187") &&
                    (statusCode != "188") && (statusCode != "189") && (statusCode != "190") && (statusCode != "800") && (statusCode != "902") &&
                    (statusCode != "904") && (statusCode != "906") && (statusCode != "907") && (statusCode != "909") && (statusCode != "911") &&
                    (statusCode != "913") && (statusCode != "90") && (statusCode != "91") && (statusCode != "94") && (statusCode != "95") &&
                    (statusCode != "98") && (statusCode != "99") && (statusCodeBalance != "400") && (compResultResp != "000") && (statusCodeBalance != "400")
                    && (statusCode != "200") && (statusCode != "508")
                    )
                //((statusCodeBalance != "400") && (compResultResp != "000")) || ((statusCodeBalance != "400") && (statusCode != "200")) ||
                //if (statusCode != "116")
                {
                    //TraceIdGenerator traceRevid = new TraceIdGenerator();
                    //tid = traceRevid.GenerateUniqueTraceID();
                    tid = retrievalReference;
                    if (sc == "00")
                    {
                        transactionType = "PayPoint Txfr to W2W";
                    }
                    else
                    {
                        sc = "01";
                        transactionType = "PayPoint Txfr to W2B"; //B2W
                    }
                    FundTransfer fundtransferRev = new FundTransfer
                    {
                        tid = tid,
                        sc = sc,
                        mobile = da,//mobile
                        da = mobile,//da
                        amount = amount,
                        pin = pin,
                        note = "reverse " + note,
                        sourcechannel = src
                    };
                    ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, fundtransfer);
                    //CustActivityModel custsmsInfo = new CustActivityModel();


                    // MNTransactionMaster validTransactionData = new MNTransactionMaster();


                    //if (TokenGenerator.TokenChecker(sessionID, mobile, src) == false)
                    //{
                    //    // throw ex
                    //    statusCode = "400";
                    //    message = "Session expired. Please login again";
                    //    failedmessage = message;
                    //}
                    //else
                    //{
                    if ((tid == null) || (sc == null) || (mobile == null) || (da == null) || (amount == null) || (pin == null) ||
                    (src == null) || (double.Parse(amount) <= 0) || (vid == null))
                    {
                        // throw ex
                        statusCode = "400";
                        message = "Parameters Missing/Invalid";
                        failedmessage = message;
                    }
                    if ((companyCode == null) || (companyCode == "") || (serviceCode == null) || (serviceCode == "") ||
                        (account == null) || (special1 == null) || (special2 == null) || (transactionDate == null) || (transactionId == null) || (transactionId == "") ||
                        (userId == null) || (userPassword == null) || (salePointType == null) || (userId == "") || (userPassword == "") || (salePointType == ""))
                    {
                        // throw ex
                        statusCode = "400";
                        message = "Parameters Missing/Invalid PayPoint";
                        failedmessage = message;
                    }
                    else
                    {
                        //if (sc == "00")
                        //{
                        //    transactionType = "PayPoint Txfr to W2W";
                        //}
                        //else 
                        //{
                        //    sc = "01";
                        //    transactionType = "PayPoint Txfr to W2B"; //B2W
                        //}

                        if (!(UserNameCheck.IsValidUser(mobile)))
                        {
                            // throw ex
                            statusCode = "400";
                            //message = "Transaction restricted to User";
                            message = "Transaction only for User";
                            failedmessage = message;
                        }
                        if (UserNameCheck.IsValidMerchant(da))
                        {

                            TransLimitCheck transLimitCheck = new TransLimitCheck();
                            string resultTranLimit = transLimitCheck.LimitCheck(mobile, da, amount, sc, pin, src);

                            var jsonDataResult = JObject.Parse(resultTranLimit);
                            statusCode = jsonDataResult["StatusCode"].ToString();
                            string statusMsg = jsonDataResult["StatusMessage"].ToString();
                            message = jsonDataResult["StatusMessage"].ToString();
                            failedmessage = message;

                            if ((statusCode == "200") && (message == "Success"))
                            {
                                //start: checking trace id
                                do
                                {
                                    TraceIdGenerator traceid = new TraceIdGenerator();
                                    tid = traceid.GenerateUniqueTraceID();
                                    fundtransfer.tid = tid;

                                    bool traceIdCheck = false;
                                    traceIdCheck = TraceIdCheck.IsValidTraceId(fundtransfer.tid);
                                    if (traceIdCheck == true)
                                    {
                                        result = "Trace ID Repeated";
                                    }
                                    else
                                    {
                                        result = "false";
                                    }

                                } while (result == "Trace ID Repeated");
                                //End: TraceId

                                //start:Com focus one log///
                                MNFundTransfer mnft = new MNFundTransfer(tid, fundtransferRev.sc, fundtransferRev.mobile,
                                       fundtransferRev.sa, fundtransferRev.amount, fundtransferRev.da, fundtransferRev.note, fundtransferRev.pin,
                                       fundtransferRev.sourcechannel, "T", "PayPoint");
                                var comfocuslog = new MNComAndFocusOneLog(mnft, DateTime.Now);
                                var mncomfocuslog = new MNComAndFocusOneLogsController();
                                //mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                result = mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                //end:Com focus one log//

                                if (result == "Success")
                                {
                                    //NOTE:- may be need to validate before insert into reply typpe
                                    //start:insert into reply type as HTTP//
                                    var replyType = new MNReplyType(tid, "HTTP");
                                    var mnreplyType = new MNReplyTypesController();
                                    mnreplyType.InsertIntoReplyType(replyType);
                                    //end:insert into reply type as HTTP//

                                    MNMerchantsController getMerchantDetails = new MNMerchantsController();

                                    string GetMerchantMobile = getMerchantDetails.PassVidToGetMerchantDetail(vid);
                                    if (GetMerchantMobile != "" || GetMerchantMobile != null)
                                    {
                                        fundtransfer.da = GetMerchantMobile; //Set Destination Merchant Mobile number
                                        GetMerchantName = getMerchantDetails.PassVIdToGetMerchantName(vid);
                                    }
                                    else
                                    {
                                        statusCode = "400";
                                        message = "Destination Merchant Doesnot Exists";
                                        mnft.Response = "Destination Merchant Doesnot Exists";
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, mnft.Response);
                                        result = mnft.Response;
                                        failedmessage = message;
                                    }


                                    //start:insert into transaction master//
                                    if (mnft.valid())
                                    {
                                        var transaction = new MNTransactionMaster(mnft);
                                        var mntransaction = new MNTransactionsController();
                                        //validTransactionData = mntransaction.Validate(transaction, mnft.pin);
                                        validTransactionData = mntransaction.Validatepaypoint(transaction, mnft.pin);
                                        result = validTransactionData.Response;

                                        /*** ***/
                                        ErrorMessage em = new ErrorMessage();

                                        if (validTransactionData.Response == "Error")
                                        {
                                            mnft.Response = "error";
                                            mnft.ResponseStatus(HttpStatusCode.InternalServerError,
                                                "Internal server error - try again later, or contact support");
                                            result = mnft.Response;
                                            statusCode = "500";
                                            message = "Internal server error - try again later, or contact support";
                                            failedmessage = message;
                                        }
                                        else
                                        {
                                            if ((result == "Trace ID Repeated") || (result == "Limit Exceed")
                                                || (result == "Invalid Source User") || (result == "Invalid Destination User")
                                                || (result == "Invalid Product Request") || (result == "Please try again") || (result == ""))
                                            {
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                statusCode = "400";
                                                message = result;
                                                failedmessage = message;
                                            }
                                            if (result == "Invalid PIN")
                                            {
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                statusCode = "400";
                                                message = result;
                                                failedmessage = message;
                                            }
                                            if (result == "111")
                                            {
                                                statusCode = result;
                                                message = em.Error_111;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "114")
                                            {
                                                statusCode = result;
                                                message = em.Error_114;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "115")
                                            {
                                                statusCode = result;
                                                message = em.Error_115;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "116")
                                            {
                                                statusCode = result;
                                                message = em.Error_116;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "119")
                                            {
                                                statusCode = result;
                                                message = em.Error_119;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "121")
                                            {
                                                statusCode = result;
                                                message = em.Error_121;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "163")
                                            {
                                                statusCode = result;
                                                message = em.Error_163;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "180")
                                            {
                                                statusCode = result;
                                                message = em.Error_180;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "181")
                                            {
                                                statusCode = result;
                                                message = em.Error_181;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "182")
                                            {
                                                statusCode = result;
                                                message = em.Error_182;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "183")
                                            {
                                                statusCode = result;
                                                message = em.Error_183;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "184")
                                            {
                                                statusCode = result;
                                                message = em.Error_184;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "185")
                                            {
                                                statusCode = result;
                                                message = em.Error_185;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "186")
                                            {
                                                statusCode = result;
                                                message = em.Error_186;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "187")
                                            {
                                                statusCode = result;
                                                message = em.Error_187;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "188")
                                            {
                                                statusCode = result;
                                                message = em.Error_188;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "189")
                                            {
                                                statusCode = result;
                                                message = em.Error_189;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "190")
                                            {
                                                statusCode = result;
                                                message = em.Error_190;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "800")
                                            {
                                                statusCode = result;
                                                message = em.Error_800;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "902")
                                            {
                                                statusCode = result;
                                                message = em.Error_902;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "904")
                                            {
                                                statusCode = result;
                                                message = em.Error_904;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "906")
                                            {
                                                statusCode = result;
                                                message = em.Error_906;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "907")
                                            {
                                                statusCode = result;
                                                message = em.Error_907;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "909")
                                            {
                                                statusCode = result;
                                                message = em.Error_909;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "911")
                                            {
                                                statusCode = result;
                                                message = em.Error_911;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "913")
                                            {
                                                statusCode = result;
                                                message = em.Error_913;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "90")
                                            {
                                                statusCode = result;
                                                message = em.Error_90;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "91")
                                            {
                                                statusCode = result;
                                                message = em.Error_91;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "92")
                                            {
                                                statusCode = result;
                                                message = em.Error_92;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "94")
                                            {
                                                statusCode = result;
                                                message = em.Error_94;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "95")
                                            {
                                                statusCode = result;
                                                message = em.Error_95;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "98")
                                            {
                                                statusCode = result;
                                                message = em.Error_98;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "99")
                                            {
                                                statusCode = result;
                                                message = em.Error_99;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            else if (validTransactionData.ResponseCode == "OK")
                                            {
                                                statusCode = "200";
                                                message = result;
                                                mnft.ResponseStatus(HttpStatusCode.OK, message);

                                            } //END ValidTransactionData.ResponseCode


                                        } //END validTransactionData.Response WITHOUT MNDB ERROR
                                        /*** ***/

                                        OutgoingWebResponseContext response1 = WebOperationContext.Current.OutgoingResponse;
                                        if (response1.StatusCode == HttpStatusCode.OK)
                                        {
                                            string messagereply = "";
                                            try
                                            {
                                                messagereply = "Dear " + CustCheckUtils.GetName(mobile) + "," + "\n";

                                                messagereply += " You have successfully reverse  NPR " + validTransactionData.Amount
                                                                    + " to " +
                                                                    //GetMerchantName 
                                                                    "Utility payment for Dish Home Direct." + " on date " +
                                                                    (validTransactionData.CreatedDate).ToString("dd/MM/yyyy")
                                                                + "." + "\n";
                                                messagereply += "Thank you. MNepal";

                                                var client = new WebClient();

                                                //SENDER
                                                if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                                                {
                                                    //FOR NCELL
                                                    var content = client.DownloadString(
                                                        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                                        + "977" + mobile + "&Text=" + messagereply + "");
                                                }
                                                else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                                            || (mobile.Substring(0, 3) == "986"))
                                                {
                                                    //FOR NTC
                                                    var content = client.DownloadString(
                                                        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                                        + "977" + mobile + "&Text=" + messagereply + "");
                                                }

                                                statusCode = "200";
                                                var v = new
                                                {
                                                    StatusCode = Convert.ToInt32(statusCode),
                                                    StatusMessage = result
                                                };
                                                result = JsonConvert.SerializeObject(v);

                                            }
                                            catch (Exception ex)
                                            {
                                                // throw ex
                                                statusCode = "400";
                                                message = ex.Message;
                                            }


                                            custsmsInfo = new CustActivityModel()
                                            {
                                                UserName = fundtransfer.mobile,
                                                RequestMerchant = transactionType,
                                                DestinationNo = fundtransfer.da,
                                                Amount = validTransactionData.Amount.ToString(),
                                                SMSStatus = "Success",
                                                SMSSenderReply = messagereply,
                                                ErrorMessage = "",
                                            };


                                        }
                                        else if ((response1.StatusCode == HttpStatusCode.BadRequest) || (response1.StatusCode == HttpStatusCode.InternalServerError) || (statusCode != "200"))
                                        {
                                            custsmsInfo = new CustActivityModel()
                                            {
                                                UserName = mobile,
                                                RequestMerchant = transactionType,
                                                DestinationNo = fundtransfer.da,
                                                Amount = validTransactionData.Amount.ToString(),
                                                SMSStatus = "Failed",
                                                SMSSenderReply = message,
                                                ErrorMessage = failedmessage,
                                            };

                                        }
                                        //end:insert into transaction master//

                                    } //END:insert into transaction master//
                                    else
                                    {
                                        mnft.Response = "error";
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, "parameters missing/invalid"); //200 - OK
                                        result = mnft.Response;
                                        statusCode = "400";
                                        message = "parameters missing/invalid";
                                        failedmessage = message;

                                        custsmsInfo = new CustActivityModel()
                                        {
                                            UserName = mobile,
                                            RequestMerchant = transactionType,
                                            DestinationNo = fundtransfer.da,
                                            Amount = amount,
                                            SMSStatus = "Failed",
                                            SMSSenderReply = message,
                                            ErrorMessage = failedmessage,
                                        };
                                    }

                                } //END MNComAndFocusOneLogsController
                                else
                                {
                                    statusCode = "400";
                                    mnft.Response = "Data Insertion Failed in Check DB Connection: TraceID limit might have exceeded";
                                    mnft.ResponseStatus(HttpStatusCode.InternalServerError, mnft.Response);
                                    result = mnft.Response;
                                    failedmessage = result;
                                }

                            } //END TRansLimit Check StatusCode N Message
                            else
                            {
                                custsmsInfo = new CustActivityModel()
                                {
                                    UserName = mobile,
                                    RequestMerchant = transactionType,
                                    DestinationNo = fundtransfer.da,
                                    Amount = amount,
                                    SMSStatus = "Failed",
                                    SMSSenderReply = message,
                                    ErrorMessage = failedmessage,
                                };
                            }

                        } //END IsValidMerchant

                        //} //END Destination mobile No check

                    }
                }
                //} //tokengenerator ko closing bracket

                //for sending sms  if success 
                if (compResultResp == "000")
                {
                    OutgoingWebResponseContext response2 = WebOperationContext.Current.OutgoingResponse;
                    if (response2.StatusCode == HttpStatusCode.OK)
                    {
                        string messagereply = "";
                        try
                        {
                            messagereply = "Dear " + CustCheckUtils.GetName(mobile) + "," + "\n";

                            messagereply += " You have successfully paid NPR " + validTransactionData.Amount
                                            + " to " + " account name: " + account + ". " +
                                            //GetMerchantName 
                                            "Utility payment for Dish Home Direct (Pinless)." + " on date " +
                                                (validTransactionData.CreatedDate).ToString("dd/MM/yyyy")
                                            + "." + "\n";
                            messagereply += "Thank you. MNepal";

                            var client = new WebClient();

                            //SENDER
                            if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                            {
                                //FOR NCELL
                                var content = client.DownloadString(
                                    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                    + "977" + mobile + "&Text=" + messagereply + "");
                            }
                            else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                        || (mobile.Substring(0, 3) == "986"))
                            {
                                //FOR NTC
                                var content = client.DownloadString(
                                    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                    + "977" + mobile + "&Text=" + messagereply + "");
                            }

                            statusCode = "200";
                            var v = new
                            {
                                StatusCode = Convert.ToInt32(statusCode),
                                StatusMessage = result
                            };
                            result = JsonConvert.SerializeObject(v);

                        }
                        catch (Exception ex)
                        {
                            // throw ex
                            statusCode = "400";
                            message = ex.Message;
                        }


                        custsmsInfo = new CustActivityModel()
                        {
                            UserName = fundtransfer.mobile,
                            RequestMerchant = transactionType,
                            DestinationNo = fundtransfer.da,
                            Amount = validTransactionData.Amount.ToString(),
                            SMSStatus = "Success",
                            SMSSenderReply = messagereply,
                            ErrorMessage = "",
                        };


                    }
                    else if ((response2.StatusCode == HttpStatusCode.BadRequest) || (response2.StatusCode == HttpStatusCode.InternalServerError) || (statusCode != "200"))
                    {
                        custsmsInfo = new CustActivityModel()
                        {
                            UserName = mobile,
                            RequestMerchant = transactionType,
                            DestinationNo = fundtransfer.da,
                            Amount = validTransactionData.Amount.ToString(),
                            SMSStatus = "Failed",
                            SMSSenderReply = message,
                            ErrorMessage = failedmessage,
                        };

                    }
                }

            }
            catch (Exception ex)
            {
                // throw ex
                statusCode = "400";
                message = ex.Message;
            }
            if (statusCodeBalance == "400")
            {
                statusCode = "400";
            }
            if (statusCode == "")
            {
                result = result.ToString();
            }
            //else if (statusCode != "200")
            else if ((statusCode != "200") || (statusCodeBalance == "400"))
            {
                if (message == "")
                {
                    if (string.IsNullOrEmpty(result))
                    {
                        result = failedmessage; // "Could not process your request,Please Try again.";
                    }
                    message = result;
                }
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = failedmessage
                };
                result = JsonConvert.SerializeObject(v);
            }
            return result;
        }
        #endregion

        #region"Check NTC CDMA Payment"
        [OperationContract]
        [WebInvoke(Method = "POST",
                  ResponseFormat = WebMessageFormat.Json)]
        public string NTCCDMAcheckpayment(Stream input)
        {
            string PaypointPwd = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointPwd"];
            string PaypointUserID = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointUserID"];
            System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate { return true; }; //to prevent from SSL error
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            string tid = qs["tid"];
            string vid = qs["vid"];
            string sc = "00";
            string mobile = qs["mobile"];
            string da = System.Web.Configuration.WebConfigurationManager.AppSettings["DestinationNoForTestServer"];
            string src = qs["src"];
            string result = "";
            string sessionID = qs["tokenID"];
            string companyCode = qs["companyCode"];
            string serviceCode = qs["serviceCode"];
            string account = qs["account"];
            string special1 = qs["special1"];
            string special2 = "";
            string transactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            long millisecondstrandId = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string transactionId = millisecondstrandId.ToString();
            string userId = PaypointUserID;
            string userPassword = PaypointPwd.Trim();
            string salePointType = "6";
            string ClientCode = qs["ClientCode"];
            string paypointType = qs["paypointType"];
            string transactionType = string.Empty;
            string resultMessageResCP = "";
            string note = "Utility payment for NTC of service code " + serviceCode + ". Mobile Number=" + qs["account"];

            PaypointModel reqCPPaypointNTCInfo = new PaypointModel();//to store data request of CP which is also commmon in nea
            PaypointModel resCPPaypointNTCInfo = new PaypointModel();//to store data response of CP which is also commmon in nea

            //PaypointModel resPaypointPaymentInfo = new PaypointModel();
            PaypointModel resPaypointNTCPaymentInfo = new PaypointModel();//to store data of Response of CP only

            string totalAmount = string.Empty;
            string totalCount = string.Empty;
            string totalBAmount = string.Empty;
            string totalBCount = string.Empty;
            string balance = string.Empty;
            string message = string.Empty;
            string statusCode = string.Empty;
            string failedmessage = string.Empty;
            string GetMerchantName = string.Empty;
            string retrievalRef = string.Empty;
            string refStanCK = string.Empty;

            string customerNo = string.Empty;
            TraceIdGenerator traceid = new TraceIdGenerator();
            tid = traceid.GenerateUniqueTraceID();


            if(serviceCode == "1")
            {
                special1 = "200";
            }
            if (serviceCode == "2")
            {
                special1 = "500";
            }
            if (serviceCode == "3")
            {
                special1 = "1000";
            }
            if (serviceCode == "10")
            {
                special1 = "10";
            }
            if (serviceCode == "1")
            {
                special1 = "200";
            }
            if (serviceCode == "4")
            {
                special1 = "50";
            }
            if (serviceCode == "5")
            {
                special1 = "100";
            }
            //for CP transaction for nepal water
            try
            {
                string URI = System.Web.Configuration.WebConfigurationManager.AppSettings["CPPaypointUrl"];

                string myParameters = "companyCode=" + companyCode + "&serviceCode=" + serviceCode +
                    "&account=" + account + "&special1=" + (Convert.ToInt32(special1) * 100).ToString() + "&special2=" + special2 +
                    "&transactionDate=" + transactionDate + "&transactionId=" + transactionId +
                    "&userId=" + userId.Trim() + "&userPassword=" + userPassword.Trim() + "&salePointType=" + salePointType;

                //for checkpayment request insert in database
                reqCPPaypointNTCInfo = new PaypointModel()
                {
                    companyCodeReqCP = companyCode,
                    serviceCodeReqCP = serviceCode,
                    accountReqCP = account,
                    special1ReqCP = special1,
                    special2ReqCP = special2,

                    transactionDateReqCP = transactionDate,
                    transactionIdReqCP = transactionId,
                    userIdReqCP = userId,
                    userPasswordReqCP = userPassword,
                    salePointTypeReqCP = salePointType,

                    refStanReqCP = "",
                    amountReqCP = "",//amountInPaisa
                    billNumberReqCP = "",
                    //retrievalReferenceReqCP = fundtransfer.tid,
                    retrievalReferenceReqCP = tid,
                    remarkReqCP = "Check Payment",
                    UserName = mobile,
                    ClientCode = ClientCode,
                    paypointType = paypointType,

                };

                string billNumber = "";
                string amountpay = "";
                string refStan = "";
                string exectransactionId = ""; //Unique
                string exectransactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"); //Current DateTime
                string rltCheckPaymt = "";
                string customerName = "";
                string mask = "";

                using (WebClient wc = new WebClient())
                {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    var HtmlResult = wc.UploadString(URI, myParameters);// response from checkpayment

                    XmlDocument xmlDoc = new XmlDocument();

                    xmlDoc.LoadXml(HtmlResult);

                    XmlNodeList test = xmlDoc.GetElementsByTagName("*");
                    string results = test[0].InnerText;
                    string HtmlResult1 = results;

                    //for getting key value from check payment
                    var reader = new StringReader(HtmlResult1);
                    var xdoc = XDocument.Load(reader);

                    XDocument docParse = XDocument.Parse(xdoc.ToString());
                    IEnumerable<XElement> responses = docParse.Descendants();

                    var xElem = XElement.Parse(docParse.ToString());

                    rltCheckPaymt = xElem.Attribute("Result").Value;
                    string keyrlt = "";// response.Attribute("Key").Value;


                    message = resultMessageResCP;
                    if (xElem.Attribute("Result").Value == "000")
                    {
                        keyrlt = xElem.Attribute("Key").Value;
                        billNumber = xElem.Descendants().Elements("BillNumber").Where(x => x.Name == "BillNumber").SingleOrDefault().Value;
                        amountpay = xElem.Descendants().Elements("Amount").Where(x => x.Name == "Amount").SingleOrDefault().Value;
                        refStan = xElem.Descendants().Elements("RefStan").Where(x => x.Name == "RefStan").SingleOrDefault().Value;
                        exectransactionDate = xElem.Descendants().Elements("DueDate").Where(x => x.Name == "DueDate").SingleOrDefault().Value;
                        customerName = xElem.Descendants().Elements("ReserveInfo").Where(x => x.Name == "ReserveInfo").SingleOrDefault().Value;
                        mask = xElem.Descendants().Elements("mask").Where(x => x.Name == "mask").SingleOrDefault().Value;

                       
                        resPaypointNTCPaymentInfo = new PaypointModel()
                        {
                            billNumber = billNumber,
                            refStan = refStan,
                            amount = amountpay,
                            transactionDate = exectransactionDate,
                            customerName = account,
                            companyCode = companyCode,
                            UserName = mobile,
                            ClientCode = ClientCode,
                            serviceCode = serviceCode

                        };
                        //end list of package

                        int resultsPayments = PaypointUtils.PaypointUtilityNTCCDMAInfo(resPaypointNTCPaymentInfo);


                    }
                    else
                    {
                        keyrlt = xElem.Attribute("Key").Value;
                        resultMessageResCP = xElem.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;
                    }
                    resultMessageResCP = xElem.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;

                    //for checkpayment response insert in database in nepal water
                    resCPPaypointNTCInfo = new PaypointModel()
                    {
                        companyCodeResCP = companyCode,
                        serviceCodeResCP = serviceCode,
                        accountResCP = account,
                        special1ResCP = special1,
                        special2ResCP = special2,

                        transactionDateResCP = transactionDate,
                        transactionIdResCP = transactionId,
                        userIdResCP = userId,
                        userPasswordResCP = userPassword,
                        salePointTypeResCP = salePointType,

                        refStanResCP = refStan,
                        amountResCP = amountpay,
                        billNumberResCP = billNumber,
                        //retrievalReferenceResCP = fundtransfer.tid,
                        retrievalReferenceResCP = tid,
                        responseCodeResCP = rltCheckPaymt,
                        descriptionResCP = "Check Payment " + keyrlt,
                        customerNameCP = customerName,
                        UserName = mobile,
                        ClientCode = ClientCode,
                        paypointType = paypointType,
                        resultMessageResCP = resultMessageResCP,

                    };
                }


                if (!(rltCheckPaymt == "000"))//show error when CP response is not 000 
                {
                    statusCode = "400";
                    // message = result;
                    message = resultMessageResCP;
                    failedmessage = message;

                }
                else
                {
                    statusCode = "200";
                    message = resultMessageResCP;
                    failedmessage = message;
                }
            }
            catch (Exception ex)
            {
                message = result + ex + "Error Message ";
                //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                statusCode = "400";
                // failedmessage = message;
                failedmessage = resultMessageResCP;

            }

            ///for  inserting CP PayPoint Data of Nepal Water which is simlar to nea 

            try
            {
                int resultsReqCP = PaypointUtils.RequestCPPaypointInfo(reqCPPaypointNTCInfo);
                int resultsResCP = PaypointUtils.ResponseCPPaypointInfo(resCPPaypointNTCInfo);


                if ((resultsReqCP > 0) && (resultsResCP > 0))

                {
                    message = result;
                }
                else
                {
                    message = result;
                }

            }
            catch (Exception ex)
            {
                string ss = ex.Message;
                message = result;
            }

            if (statusCode == "")
            {
                result = result.ToString();
            }
            else if (statusCode == "200")
            {
                if (message == "")
                {
                    if (string.IsNullOrEmpty(result))
                    {
                        result = failedmessage; // "Could not process your request,Please Try again.";
                    }
                    message = result;
                }
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = failedmessage,
                    retrievalRef = resCPPaypointNTCInfo.retrievalReferenceResCP,
                    refStanCK = resCPPaypointNTCInfo.refStanResCP
                };
                result = JsonConvert.SerializeObject(v);
            }
            else if (statusCode != "200")
            {
                if (message == "")
                {
                    if (string.IsNullOrEmpty(result))
                    {
                        result = failedmessage; // "Could not process your request,Please Try again.";
                    }
                    message = result;
                }
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = failedmessage,
                    retrievalRef = "",
                    refStanCK = ""
                };
                result = JsonConvert.SerializeObject(v);
            }
            return result;

        }
        #endregion

        #region"NTC CDMA execute Paypoint"
        [OperationContract]
        [WebInvoke(Method = "POST",
                  ResponseFormat = WebMessageFormat.Json)]
        public string NTCCDMAExecutePayment(Stream input)
        {
            string PaypointPwd = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointPwd"];
            string PaypointUserID = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointUserID"];

            System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);
            MNTransactionMaster validTransactionData = new MNTransactionMaster();
            CustActivityModel custsmsInfo = new CustActivityModel();

            string tid = qs["tid"];
            string vid = qs["vid"]; //MerchantID
            string sc = qs["sc"];
            string mobile = qs["mobile"];
            string da = System.Web.Configuration.WebConfigurationManager.AppSettings["DestinationNoForTestServer"];


            string amount = qs["amount"];//amount paid by customer 
            string pin = qs["pin"];
            string note = "utility payment for NTC CDMA. Customer Name=" + qs["account"];//+ ". " + qs["note"];
            string src = qs["src"];
            string result = "";
            string sessionID = qs["tokenID"];
            string resultMessageResCP = "";
            string companyCode = qs["companyCode"];
            string serviceCode = qs["serviceCode"];
            string account = qs["account"];
            string special1 = qs["special1"]; //
            string special2 = ""; //
            string transactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");//"2019-11-22T11:11:02";
            long millisecondstrandId = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string transactionId = millisecondstrandId.ToString(); //"120163339819";
            string userId = PaypointUserID;
            string userPassword = PaypointPwd.Trim();
            string salePointType = "6";
            string transactionType = string.Empty;
            string amountpay = qs["amountpay"];//amount need to pay i.e amount in bill
            string exectransactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            string exectransactionId = millisecondstrandId.ToString();
            string refStan = qs["refStan"];
            string billNumber = qs["billNumber"];
            string rltCheckPaymt = qs["rltCheckPaymt"];
            string ClientCode = qs["ClientCode"];
            string paypointType = qs["paypointType"];
            //string paypointType = "Nepal Water";
            //string paypointType = "3";

            string customerName = qs["customerName"];
            string walletBalance = qs["walletBalance"];//in Rs.
            string retrievalReference = qs["retrievalReference"];
            int walletBalancePaisaInt = 0;
            walletBalancePaisaInt = Convert.ToInt32((Convert.ToDouble(walletBalance)) * 100);
            int amountpayInt = Convert.ToInt32(float.Parse(amount)) * 100;

            PaypointModel reqEPPaypointDishHomeInfo = new PaypointModel();
            PaypointModel resEPPaypointDishHomeInfo = new PaypointModel();

            PaypointModel reqGTPaypointDishHomeInfo = new PaypointModel();
            PaypointModel resGTPaypointDishHomeInfo = new PaypointModel();

            string totalAmount = string.Empty;
            string totalCount = string.Empty;
            string totalBAmount = string.Empty;
            string totalBCount = string.Empty;
            string balance = string.Empty;
            string message = string.Empty;
            string statusCode = string.Empty;
            string failedmessage = string.Empty;
            string GetMerchantName = string.Empty;
            string statusCodeBalance = string.Empty;

            string customerNo = string.Empty;

            // TraceIdGenerator traceid = new TraceIdGenerator();
            //tid = traceid.GenerateUniqueTraceID();
            tid = retrievalReference;

            FundTransfer fundtransfer = new FundTransfer
            {
                tid = tid,
                sc = sc,
                mobile = mobile,
                da = da,
                amount = amount,
                pin = pin,
                note = note,
                sourcechannel = src
            };
            if (sc == "00")
            {
                if (walletBalancePaisaInt >= amountpayInt)// if wallet balance less then bill amount then show error msg
                {
                    //First transaction MNRequest N Response
                    try
                    {

                        //FundTransfer fundtransfer = new FundTransfer
                        //{
                        //    tid = tid,
                        //    sc = sc,
                        //    mobile = mobile,
                        //    da = da,
                        //    amount = amount,
                        //    pin = pin,
                        //    note = note,
                        //    sourcechannel = src
                        //};

                        ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, fundtransfer);
                        //CustActivityModel custsmsInfo = new CustActivityModel();

                        //MNTransactionMaster validTransactionData = new MNTransactionMaster();


                        //if (TokenGenerator.TokenChecker(sessionID, mobile, src) == false)
                        //{
                        //    // throw ex
                        //    statusCode = "400";
                        //    message = "Session expired. Please login again";
                        //    failedmessage = message;
                        //}
                        //else
                        //{
                        if ((tid == null) || (sc == null) || (mobile == null) || (da == null) || (amount == null) || (pin == null) ||
                        (src == null) || (double.Parse(amount) <= 0) || (vid == null))
                        {
                            // throw ex
                            statusCode = "400";
                            message = "Parameters Missing/Invalid";
                            failedmessage = message;
                        }
                        if ((companyCode == null) || (companyCode == "") || (serviceCode == null) || (serviceCode == "") ||
                            (account == null) || (special1 == null) || (special2 == null) || (transactionDate == null) || (transactionId == null) || (transactionId == "") ||
                            (userId == null) || (userPassword == null) || (salePointType == null) || (userId == "") || (userPassword == "") || (salePointType == ""))
                        {
                            // throw ex
                            statusCode = "400";
                            message = "Parameters Missing/Invalid PayPoint";
                            failedmessage = message;
                        }
                        else
                        {
                            if (sc == "00")
                            {
                                transactionType = "PayPoint Txfr to W2W";
                            }
                            else if (sc == "10")
                            {
                                transactionType = "PayPoint Txfr to B2W"; //B2W
                            }

                            if (!(UserNameCheck.IsValidUser(mobile)))
                            {
                                // throw ex
                                statusCode = "400";
                                //message = "Transaction restricted to User";
                                message = "Transaction only for User";
                                failedmessage = message;
                            }
                            if (UserNameCheck.IsValidMerchant(da))
                            {

                                TransLimitCheck transLimitCheck = new TransLimitCheck();
                                string resultTranLimit = transLimitCheck.LimitCheck(mobile, da, amount, sc, pin, src);

                                var jsonDataResult = JObject.Parse(resultTranLimit);
                                statusCode = jsonDataResult["StatusCode"].ToString();
                                string statusMsg = jsonDataResult["StatusMessage"].ToString();
                                message = jsonDataResult["StatusMessage"].ToString();
                                failedmessage = message;

                                if ((statusCode == "200") && (message == "Success"))
                                {
                                    //start: checking trace id
                                    do
                                    {
                                        //TraceIdGenerator traceid = new TraceIdGenerator();
                                        // tid = traceid.GenerateUniqueTraceID();

                                        fundtransfer.tid = tid;
                                        bool traceIdCheck = false;
                                        traceIdCheck = TraceIdCheck.IsValidTraceId(fundtransfer.tid);
                                        if (traceIdCheck == true)
                                        {
                                            result = "Trace ID Repeated";
                                        }
                                        else
                                        {
                                            result = "false";
                                        }

                                    } while (result == "Trace ID Repeated");
                                    //End: TraceId

                                    //start:Com focus one log///
                                    MNFundTransfer mnft = new MNFundTransfer(tid, fundtransfer.sc, fundtransfer.mobile,
                                        fundtransfer.sa, fundtransfer.amount, fundtransfer.da, fundtransfer.note, fundtransfer.pin,
                                        fundtransfer.sourcechannel);
                                    var comfocuslog = new MNComAndFocusOneLog(mnft, DateTime.Now);
                                    var mncomfocuslog = new MNComAndFocusOneLogsController();
                                    //mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                    result = mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                    //end:Com focus one log//

                                    if (result == "Success")
                                    {
                                        //NOTE:- may be need to validate before insert into reply typpe
                                        //start:insert into reply type as HTTP//
                                        var replyType = new MNReplyType(tid, "HTTP");
                                        var mnreplyType = new MNReplyTypesController();
                                        mnreplyType.InsertIntoReplyType(replyType);
                                        //end:insert into reply type as HTTP//

                                        MNMerchantsController getMerchantDetails = new MNMerchantsController();

                                        string GetMerchantMobile = getMerchantDetails.PassVidToGetMerchantDetail(vid);
                                        if (GetMerchantMobile != "" || GetMerchantMobile != null)
                                        {
                                            fundtransfer.da = GetMerchantMobile; //Set Destination Merchant Mobile number
                                            GetMerchantName = getMerchantDetails.PassVIdToGetMerchantName(vid);
                                        }
                                        else
                                        {
                                            statusCode = "400";
                                            message = "Destination Merchant Doesnot Exists";
                                            mnft.Response = "Destination Merchant Doesnot Exists";
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, mnft.Response);
                                            result = mnft.Response;
                                            failedmessage = message;
                                        }


                                        //start:insert into transaction master//
                                        if (mnft.valid())
                                        {
                                            //var transaction = new MNTransactionMaster(mnft);
                                            //var mntransaction = new MNTransactionsController();
                                            //validTransactionData = mntransaction.Validate(transaction, mnft.pin);

                                            var transactionpaypoint = new MNTransactionMaster(mnft);
                                            var mntransactionpaypoint = new MNTransactionsController();
                                            validTransactionData = mntransactionpaypoint.Validatepaypoint(transactionpaypoint, mnft.pin);

                                            result = validTransactionData.Response;
                                            /*** ***/
                                            ErrorMessage em = new ErrorMessage();

                                            if (validTransactionData.Response == "Error")
                                            {
                                                mnft.Response = "error";
                                                mnft.ResponseStatus(HttpStatusCode.InternalServerError,
                                                    "Internal server error - try again later, or contact support");
                                                result = mnft.Response;
                                                statusCode = "500";
                                                message = "Internal server error - try again later, or contact support";
                                                failedmessage = message;
                                            }
                                            else
                                            {
                                                if ((result == "Trace ID Repeated") || (result == "Limit Exceed")
                                                    || (result == "Invalid Source User") || (result == "Invalid Destination User")
                                                    || (result == "Invalid Product Request") || (result == "Please try again") || (result == ""))
                                                {
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                    statusCode = "400";
                                                    message = result;
                                                    failedmessage = message;
                                                }
                                                if (result == "Invalid PIN")
                                                {
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                    statusCode = "400";
                                                    message = result;
                                                    failedmessage = message;
                                                }
                                                if (result == "111")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_111;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "114")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_114;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "115")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_115;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "116")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_116;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "119")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_119;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "121")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_121;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "163")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_163;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "180")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_180;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "181")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_181;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "182")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_182;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "183")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_183;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "184")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_184;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "185")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_185;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "186")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_186;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "187")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_187;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "188")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_188;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "189")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_189;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "190")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_190;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "800")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_800;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "902")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_902;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "904")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_904;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "906")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_906;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "907")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_907;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "909")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_909;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "911")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_911;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "913")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_913;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "90")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_90;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "91")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_91;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "92")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_92;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "94")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_94;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "95")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_95;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "98")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_98;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "99")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_99;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "508")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_508;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                else if (validTransactionData.ResponseCode == "OK")
                                                {
                                                    statusCode = "200";
                                                    message = result;
                                                    mnft.ResponseStatus(HttpStatusCode.OK, message);

                                                } //END ValidTransactionData.ResponseCode


                                            } //END validTransactionData.Response WITHOUT MNDB ERROR

                                            //start comment outgoing
                                            //OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                                            //if (response.StatusCode == HttpStatusCode.OK)
                                            //{
                                            //    string messagereply = "";
                                            //    try
                                            //    {
                                            //        //messagereply = "Dear " + CustCheckUtils.GetName(mobile) + "," + "\n";

                                            //        //messagereply += " You have successfully paid NPR " + validTransactionData.Amount
                                            //        //                + " to " + GetMerchantName + " on date " +
                                            //        //                    (validTransactionData.CreatedDate).ToString("dd/MM/yyyy")
                                            //        //                + "." + "\n";
                                            //        //messagereply += "Thank you. MNepal";

                                            //        //var client = new WebClient();

                                            //        ////SENDER
                                            //        //if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                                            //        //{
                                            //        //    //FOR NCELL
                                            //        //    var content = client.DownloadString(
                                            //        //        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                            //        //        + "977" + mobile + "&Text=" + messagereply + "");
                                            //        //}
                                            //        //else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                            //        //            || (mobile.Substring(0, 3) == "986"))
                                            //        //{
                                            //        //    //FOR NTC
                                            //        //    var content = client.DownloadString(
                                            //        //        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                            //        //        + "977" + mobile + "&Text=" + messagereply + "");
                                            //        //}

                                            //        statusCode = "200";
                                            //        var v = new
                                            //        {
                                            //            StatusCode = Convert.ToInt32(statusCode),
                                            //            StatusMessage = result
                                            //        };
                                            //        result = JsonConvert.SerializeObject(v);

                                            //    }
                                            //    catch (Exception ex)
                                            //    {
                                            //        // throw ex
                                            //        statusCode = "400";
                                            //        message = ex.Message;
                                            //    }


                                            //    custsmsInfo = new CustActivityModel()
                                            //    {
                                            //        UserName = fundtransfer.mobile,
                                            //        RequestMerchant = transactionType,
                                            //        DestinationNo = fundtransfer.da,
                                            //        Amount = validTransactionData.Amount.ToString(),
                                            //        SMSStatus = "Success",
                                            //        SMSSenderReply = messagereply,
                                            //        ErrorMessage = "",
                                            //    };


                                            //}
                                            //else if ((response.StatusCode == HttpStatusCode.BadRequest) || (response.StatusCode == HttpStatusCode.InternalServerError) || (statusCode != "200"))
                                            //{
                                            //    custsmsInfo = new CustActivityModel()
                                            //    {
                                            //        UserName = mobile,
                                            //        RequestMerchant = transactionType,
                                            //        DestinationNo = fundtransfer.da,
                                            //        Amount = validTransactionData.Amount.ToString(),
                                            //        SMSStatus = "Failed",
                                            //        SMSSenderReply = message,
                                            //        ErrorMessage = failedmessage,
                                            //    };

                                            //}

                                            //end comment outgoint
                                            //end:insert into transaction master//

                                        } //END:insert into transaction master//
                                        else
                                        {
                                            mnft.Response = "error";
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, "parameters missing/invalid"); //200 - OK
                                            result = mnft.Response;
                                            statusCode = "400";
                                            message = "parameters missing/invalid";
                                            failedmessage = message;

                                            custsmsInfo = new CustActivityModel()
                                            {
                                                UserName = mobile,
                                                RequestMerchant = transactionType,
                                                DestinationNo = fundtransfer.da,
                                                Amount = amount,
                                                SMSStatus = "Failed",
                                                SMSSenderReply = message,
                                                ErrorMessage = failedmessage,
                                            };
                                        }

                                    } //END MNComAndFocusOneLogsController
                                    else
                                    {
                                        statusCode = "400";
                                        mnft.Response = "Data Insertion Failed in Check DB Connection: TraceID limit might have exceeded";
                                        mnft.ResponseStatus(HttpStatusCode.InternalServerError, mnft.Response);
                                        result = mnft.Response;
                                        failedmessage = result;
                                    }

                                } //END TRansLimit Check StatusCode N Message
                                else
                                {
                                    custsmsInfo = new CustActivityModel()
                                    {
                                        UserName = mobile,
                                        RequestMerchant = transactionType,
                                        DestinationNo = fundtransfer.da,
                                        Amount = amount,
                                        SMSStatus = "Failed",
                                        SMSSenderReply = message,
                                        ErrorMessage = failedmessage,
                                    };
                                }

                            } //END IsValidMerchant

                            //} //END Destination mobile No check

                        }

                    }
                    catch (Exception ex)
                    {
                        message = result + ex + "Error Message ";
                        //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                        statusCode = "400";
                        //failedmessage = message;
                        failedmessage = "Please try again.";
                    }

                }
                else  //else for  if wallet balance less then bill amount then show error msg
                {
                    statusCodeBalance = "400";
                    message = "Insufficient Balance";
                    failedmessage = message;

                }
            }
            else if (sc == "10")//for bank payment in nepal water
            {
                //if (bankBalancePaisaInt >= amountpayInt)
                //{
                //First transaction MNRequest N Response
                try
                {

                    //FundTransfer fundtransfer = new FundTransfer
                    //{
                    //    tid = tid,
                    //    sc = sc,
                    //    mobile = mobile,
                    //    da = da,
                    //    amount = amount,
                    //    pin = pin,
                    //    note = note,
                    //    sourcechannel = src
                    //};

                    ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, fundtransfer);
                    //CustActivityModel custsmsInfo = new CustActivityModel();

                    //MNTransactionMaster validTransactionData = new MNTransactionMaster();


                    //if (TokenGenerator.TokenChecker(sessionID, mobile, src) == false)
                    //{
                    //    // throw ex
                    //    statusCode = "400";
                    //    message = "Session expired. Please login again";
                    //    failedmessage = message;
                    //}
                    //else
                    //{
                    if ((tid == null) || (sc == null) || (mobile == null) || (da == null) || (amount == null) || (pin == null) ||
                    (src == null) || (double.Parse(amount) <= 0) || (vid == null))
                    {
                        // throw ex
                        statusCode = "400";
                        message = "Parameters Missing/Invalid";
                        failedmessage = message;
                    }
                    if ((companyCode == null) || (companyCode == "") || (serviceCode == null) || (serviceCode == "") ||
                        (account == null) || (special1 == null) || (special2 == null) || (transactionDate == null) || (transactionId == null) || (transactionId == "") ||
                        (userId == null) || (userPassword == null) || (salePointType == null) || (userId == "") || (userPassword == "") || (salePointType == ""))
                    {
                        // throw ex
                        statusCode = "400";
                        message = "Parameters Missing/Invalid PayPoint";
                        failedmessage = message;
                    }
                    else
                    {
                        if (sc == "00")
                        {
                            transactionType = "PayPoint Txfr to W2W";
                        }
                        else if (sc == "10")
                        {
                            transactionType = "PayPoint Txfr to B2W"; //B2W
                        }

                        if (!(UserNameCheck.IsValidUser(mobile)))
                        {
                            // throw ex
                            statusCode = "400";
                            //message = "Transaction restricted to User";
                            message = "Transaction only for User";
                            failedmessage = message;
                        }
                        if (UserNameCheck.IsValidMerchant(da))
                        {

                            TransLimitCheck transLimitCheck = new TransLimitCheck();
                            string resultTranLimit = transLimitCheck.LimitCheck(mobile, da, amount, sc, pin, src);

                            var jsonDataResult = JObject.Parse(resultTranLimit);
                            statusCode = jsonDataResult["StatusCode"].ToString();
                            string statusMsg = jsonDataResult["StatusMessage"].ToString();
                            message = jsonDataResult["StatusMessage"].ToString();
                            failedmessage = message;

                            if ((statusCode == "200") && (message == "Success"))
                            {
                                //start: checking trace id
                                do
                                {
                                    //TraceIdGenerator traceid = new TraceIdGenerator();
                                    // tid = traceid.GenerateUniqueTraceID();
                                    fundtransfer.tid = tid;

                                    bool traceIdCheck = false;
                                    traceIdCheck = TraceIdCheck.IsValidTraceId(fundtransfer.tid);
                                    if (traceIdCheck == true)
                                    {
                                        result = "Trace ID Repeated";
                                    }
                                    else
                                    {
                                        result = "false";
                                    }

                                } while (result == "Trace ID Repeated");
                                //End: TraceId

                                //start:Com focus one log///
                                MNFundTransfer mnft = new MNFundTransfer(tid, fundtransfer.sc, fundtransfer.mobile,
                                    fundtransfer.sa, fundtransfer.amount, fundtransfer.da, fundtransfer.note, fundtransfer.pin,
                                    fundtransfer.sourcechannel);
                                var comfocuslog = new MNComAndFocusOneLog(mnft, DateTime.Now);
                                var mncomfocuslog = new MNComAndFocusOneLogsController();
                                //mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                result = mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                //end:Com focus one log//

                                if (result == "Success")
                                {
                                    //NOTE:- may be need to validate before insert into reply typpe
                                    //start:insert into reply type as HTTP//
                                    var replyType = new MNReplyType(tid, "HTTP");
                                    var mnreplyType = new MNReplyTypesController();
                                    mnreplyType.InsertIntoReplyType(replyType);
                                    //end:insert into reply type as HTTP//

                                    MNMerchantsController getMerchantDetails = new MNMerchantsController();

                                    string GetMerchantMobile = getMerchantDetails.PassVidToGetMerchantDetail(vid);
                                    if (GetMerchantMobile != "" || GetMerchantMobile != null)
                                    {
                                        fundtransfer.da = GetMerchantMobile; //Set Destination Merchant Mobile number
                                        GetMerchantName = getMerchantDetails.PassVIdToGetMerchantName(vid);
                                    }
                                    else
                                    {
                                        statusCode = "400";
                                        message = "Destination Merchant Doesnot Exists";
                                        mnft.Response = "Destination Merchant Doesnot Exists";
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, mnft.Response);
                                        result = mnft.Response;
                                        failedmessage = message;
                                    }


                                    //start:insert into transaction master//
                                    if (mnft.valid())
                                    {
                                        //var transaction = new MNTransactionMaster(mnft);
                                        //var mntransaction = new MNTransactionsController();
                                        //validTransactionData = mntransaction.Validate(transaction, mnft.pin);


                                        var transactionpaypoint = new MNTransactionMaster(mnft);
                                        var mntransactionpaypoint = new MNTransactionsController();
                                        validTransactionData = mntransactionpaypoint.Validatepaypoint(transactionpaypoint, mnft.pin);
                                        result = validTransactionData.Response;
                                        /*** ***/
                                        ErrorMessage em = new ErrorMessage();

                                        if (validTransactionData.Response == "Error")
                                        {
                                            mnft.Response = "error";
                                            mnft.ResponseStatus(HttpStatusCode.InternalServerError,
                                                "Internal server error - try again later, or contact support");
                                            result = mnft.Response;
                                            statusCode = "500";
                                            message = "Internal server error - try again later, or contact support";
                                            failedmessage = message;
                                        }
                                        else
                                        {
                                            if ((result == "Trace ID Repeated") || (result == "Limit Exceed")
                                                || (result == "Invalid Source User") || (result == "Invalid Destination User")
                                                || (result == "Invalid Product Request") || (result == "Please try again") || (result == ""))
                                            {
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                statusCode = "400";
                                                message = result;
                                                failedmessage = message;
                                            }
                                            if (result == "Invalid PIN")
                                            {
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                statusCode = "400";
                                                message = result;
                                                failedmessage = message;
                                            }
                                            if (result == "111")
                                            {
                                                statusCode = result;
                                                message = em.Error_111;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "114")
                                            {
                                                statusCode = result;
                                                message = em.Error_114;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "115")
                                            {
                                                statusCode = result;
                                                message = em.Error_115;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "116")
                                            {
                                                statusCode = result;
                                                message = em.Error_116;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "119")
                                            {
                                                statusCode = result;
                                                message = em.Error_119;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "121")
                                            {
                                                statusCode = result;
                                                message = em.Error_121;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "163")
                                            {
                                                statusCode = result;
                                                message = em.Error_163;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "180")
                                            {
                                                statusCode = result;
                                                message = em.Error_180;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "181")
                                            {
                                                statusCode = result;
                                                message = em.Error_181;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "182")
                                            {
                                                statusCode = result;
                                                message = em.Error_182;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "183")
                                            {
                                                statusCode = result;
                                                message = em.Error_183;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "184")
                                            {
                                                statusCode = result;
                                                message = em.Error_184;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "185")
                                            {
                                                statusCode = result;
                                                message = em.Error_185;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "186")
                                            {
                                                statusCode = result;
                                                message = em.Error_186;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "187")
                                            {
                                                statusCode = result;
                                                message = em.Error_187;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "188")
                                            {
                                                statusCode = result;
                                                message = em.Error_188;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "189")
                                            {
                                                statusCode = result;
                                                message = em.Error_189;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "190")
                                            {
                                                statusCode = result;
                                                message = em.Error_190;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "800")
                                            {
                                                statusCode = result;
                                                message = em.Error_800;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "902")
                                            {
                                                statusCode = result;
                                                message = em.Error_902;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "904")
                                            {
                                                statusCode = result;
                                                message = em.Error_904;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "906")
                                            {
                                                statusCode = result;
                                                message = em.Error_906;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "907")
                                            {
                                                statusCode = result;
                                                message = em.Error_907;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "909")
                                            {
                                                statusCode = result;
                                                message = em.Error_909;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "911")
                                            {
                                                statusCode = result;
                                                message = em.Error_911;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "913")
                                            {
                                                statusCode = result;
                                                message = em.Error_913;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "90")
                                            {
                                                statusCode = result;
                                                message = em.Error_90;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "91")
                                            {
                                                statusCode = result;
                                                message = em.Error_91;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "92")
                                            {
                                                statusCode = result;
                                                message = em.Error_92;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "94")
                                            {
                                                statusCode = result;
                                                message = em.Error_94;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "95")
                                            {
                                                statusCode = result;
                                                message = em.Error_95;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "98")
                                            {
                                                statusCode = result;
                                                message = em.Error_98;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "99")
                                            {
                                                statusCode = result;
                                                message = em.Error_99;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "508")
                                            {
                                                statusCode = result;
                                                message = em.Error_508;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            else if (validTransactionData.ResponseCode == "OK")
                                            {
                                                statusCode = "200";
                                                message = result;
                                                mnft.ResponseStatus(HttpStatusCode.OK, message);

                                            } //END ValidTransactionData.ResponseCode


                                        } //END validTransactionData.Response WITHOUT MNDB ERROR

                                        //end:insert into transaction master//

                                    } //END:insert into transaction master//
                                    else
                                    {
                                        mnft.Response = "error";
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, "parameters missing/invalid"); //200 - OK
                                        result = mnft.Response;
                                        statusCode = "400";
                                        message = "parameters missing/invalid";
                                        failedmessage = message;

                                        custsmsInfo = new CustActivityModel()
                                        {
                                            UserName = mobile,
                                            RequestMerchant = transactionType,
                                            DestinationNo = fundtransfer.da,
                                            Amount = amount,
                                            SMSStatus = "Failed",
                                            SMSSenderReply = message,
                                            ErrorMessage = failedmessage,
                                        };
                                    }

                                } //END MNComAndFocusOneLogsController
                                else
                                {
                                    statusCode = "400";
                                    mnft.Response = "Data Insertion Failed in Check DB Connection: TraceID limit might have exceeded";
                                    mnft.ResponseStatus(HttpStatusCode.InternalServerError, mnft.Response);
                                    result = mnft.Response;
                                    failedmessage = result;
                                }

                            } //END TRansLimit Check StatusCode N Message
                            else
                            {
                                custsmsInfo = new CustActivityModel()
                                {
                                    UserName = mobile,
                                    RequestMerchant = transactionType,
                                    DestinationNo = fundtransfer.da,
                                    Amount = amount,
                                    SMSStatus = "Failed",
                                    SMSSenderReply = message,
                                    ErrorMessage = failedmessage,
                                };
                            }

                        } //END IsValidMerchant


                        //} //END Destination mobile No check

                    }

                }
                catch (Exception ex)
                {
                    message = result + ex + "Error Message ";
                    //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                    statusCode = "400";
                    //failedmessage = message;
                    failedmessage = "Please try again.";
                }
                //}
                //else  //else for  if wallet balance less then bill amount then show error msg
                //{
                //    statusCodeBalance = "400";
                //    message = "Insufficient Balance";
                //    failedmessage = message;

                //}
            }
            try
            {

                //for  all EP  and GT transaction
                string compResultResp = "";

                if (statusCode == "200")
                {
                    try
                    {
                        //string amountInPaisa = ((double.Parse(amount)) * 100).ToString();

                        if (rltCheckPaymt == "000")//go if CP Response is 000
                        {
                            //int amountInPaisaInt = Convert.ToInt32(amountInPaisa);

                            long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                            exectransactionId = milliseconds.ToString();

                            string keyExecRlt = "";
                            string resultMessageResEP = "";
                            do
                            {
                                if ((companyCode == null) || (serviceCode == null) || (account == null) ||
                                (exectransactionDate == null) || (exectransactionId == null) ||
                                (refStan == null) || (amount == null) || (billNumber == null) ||
                                (userId == null) || (userPassword == null) || (salePointType == null))
                                {
                                    // throw ex
                                    statusCode = "400";
                                    message = "Parameters Missing/Invalid";
                                    failedmessage = message;
                                }
                                else
                                {
                                    if (companyCode == "598")
                                    {
                                        special1 = "";
                                    }
                                    else
                                    {
                                        special1 = special1.ToString();
                                    }

                                    // string URIEXECPayment = "https://test.paypoint.md:4445/PayPointWS/PayPointMSOperations.asmx/ExecutePayment";//for EP Link 

                                    //For excutepaypoint link in webconfig
                                    string URIEXECPayment = System.Web.Configuration.WebConfigurationManager.AppSettings["EPPaypointUrl"];

                                    string execPayParameters = "companyCode=" + companyCode + "&serviceCode=" + serviceCode +
                                            "&account=" + account + "&special1=" + special1 + "&special2=" + special2 +
                                            "&transactionDate=" + exectransactionDate + "&transactionId=" + exectransactionId +
                                            "&refStan=" + refStan + "&amount=" + Convert.ToInt32(float.Parse(amount) * 100).ToString() + "&billNumber=" + billNumber +
                                            "&userId=" + userId.Trim() + "&userPassword=" + userPassword.Trim() + "&salePointType=" + salePointType;

                                    //for executepayment request insert in database for wlink
                                    reqEPPaypointDishHomeInfo = new PaypointModel()
                                    {
                                        companyCodeReqEP = companyCode,
                                        serviceCodeReqEP = serviceCode,
                                        accountReqEP = account,
                                        special1ReqEP = special1,
                                        special2ReqEP = special2,

                                        transactionDateReqEP = exectransactionDate,
                                        transactionIdReqEP = exectransactionId,
                                        userIdReqEP = userId,
                                        userPasswordReqEP = userPassword,
                                        salePointTypeReqEP = salePointType,

                                        refStanReqEP = refStan,
                                        amountReqEP = amount,
                                        billNumberReqEP = billNumber,
                                        //retrievalReferenceReqEP = fundtransfer.tid,
                                        //retrievalReferenceReqEP = tid,
                                        retrievalReferenceReqEP = retrievalReference,
                                        remarkReqEP = "Execute Payment",
                                        UserName = mobile,
                                        ClientCode = ClientCode,
                                        paypointType = paypointType,

                                    };

                                    using (WebClient wcExecPay = new WebClient())
                                    {
                                        wcExecPay.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                                        string HtmlResultExecPay = wcExecPay.UploadString(URIEXECPayment, execPayParameters);

                                        XmlDocument xmlEDoc = new XmlDocument();
                                        xmlEDoc.LoadXml(HtmlResultExecPay);

                                        XmlNodeList nodeEPay = xmlEDoc.GetElementsByTagName("*");
                                        string resultEPay = nodeEPay[0].InnerText;
                                        string HtmlEPayResult = resultEPay;

                                        //for determining  checkpayment key and result
                                        var readerEPay = new StringReader(HtmlEPayResult);
                                        var xdocEPay = XDocument.Load(readerEPay);

                                        XDocument docEPay = XDocument.Parse(xdocEPay.ToString());

                                        var xElemEPay = XElement.Parse(xdocEPay.ToString());

                                        if (xElemEPay.Attribute("Result").Value == "000")
                                        {
                                            compResultResp = xElemEPay.Attribute("Result").Value;
                                            keyExecRlt = xElemEPay.Attribute("Key").Value;
                                        }
                                        else if ((xElemEPay.Attribute("Result").Value == "011") || (xElemEPay.Attribute("Result").Value == "012"))
                                        {
                                            compResultResp = xElemEPay.Attribute("Result").Value;
                                            keyExecRlt = xElemEPay.Attribute("Key").Value;
                                        }
                                        else
                                        {
                                            compResultResp = xElemEPay.Attribute("Result").Value;
                                            keyExecRlt = xElemEPay.Attribute("Key").Value;
                                        }
                                        resultMessageResEP = xElemEPay.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;



                                        //for Response Execute Payment
                                        resEPPaypointDishHomeInfo = new PaypointModel()
                                        {
                                            companyCodeResEP = companyCode,
                                            serviceCodeResEP = serviceCode,
                                            accountResEP = account,
                                            special1ResEP = special1,
                                            special2ResEP = special2,

                                            transactionDateResEP = transactionDate,
                                            transactionIdResEP = transactionId,
                                            userIdResEP = userId,

                                            userPasswordResEP = userPassword,
                                            salePointTypeResEP = salePointType,

                                            refStanResEP = refStan,
                                            amountResEP = amount,
                                            billNumberResEP = billNumber,
                                            //retrievalReferenceResEP = fundtransfer.tid,
                                            //retrievalReferenceResEP = tid,
                                            retrievalReferenceResEP = retrievalReference,
                                            responseCodeResEP = compResultResp,
                                            descriptionResEP = "Execute Payment" + keyExecRlt,
                                            UserName = mobile,
                                            ClientCode = ClientCode,
                                            paypointType = paypointType,
                                            resultMessageResEP = resultMessageResEP,
                                            customerNameResEP = customerName,
                                        };
                                    }
                                }

                            } while ((compResultResp == "011") || (compResultResp == "012"));

                            ///for  Inserting EP PayPoint Data for wlink

                            try
                            {
                                int resultsReqEP = PaypointUtils.RequestEPPaypointInfo(reqEPPaypointDishHomeInfo);
                                int resultsResEP = PaypointUtils.ResponseEPPaypointInfo(resEPPaypointDishHomeInfo);

                                if ((resultsReqEP > 0) && (resultsResEP > 0))
                                {
                                    message = result;
                                }
                                else
                                {
                                    message = result;
                                }

                            }
                            catch (Exception ex)
                            {
                                string ss = ex.Message;
                                message = result;
                            }

                            if (compResultResp == "000")
                            {
                                if ((refStan == null) || (userId == null) || (userPassword == null) || (salePointType == null))
                                {
                                    // throw ex
                                    statusCode = "400";
                                    message = "Parameters Missing/Invalid";
                                    failedmessage = message;
                                }
                                else if (compResultResp != "")
                                {
                                    string statusResGTP = "1";
                                    do
                                    {
                                        string key = "";
                                        string gtBillNumber = "";
                                        // string URIGetTran = "https://test.paypoint.md:4445/PayPointWS/PayPointMSOperations.asmx/GetTransaction";//for GT paypoint transactionlink

                                        //For get transactionpaypoint link in webconfig
                                        string URIGetTran = System.Web.Configuration.WebConfigurationManager.AppSettings["GTPPaypointUrl"];

                                        string GetTranParameters = "userLogin=" + userId + "&userPassword=" + userPassword + "&stan=" + "-1" + "&refStan=" + refStan + "&key=" + "" + "&billNumber=" + gtBillNumber;

                                        //for get transaction payment request insert in database
                                        reqGTPaypointDishHomeInfo = new PaypointModel()
                                        {
                                            companyCodeReqGTP = companyCode,
                                            serviceCodeReqGTP = serviceCode,
                                            accountReqGTP = account,
                                            special1ReqGTP = special1,
                                            special2ReqGTP = special2,

                                            transactionDateReqGTP = transactionDate,
                                            transactionIdReqGTP = transactionId,
                                            userIdReqGTP = userId,
                                            userPasswordReqGTP = userPassword,
                                            salePointTypeReqGTP = salePointType,

                                            refStanReqGTP = refStan,
                                            amountReqGTP = amount,
                                            billNumberReqGTP = gtBillNumber,
                                            // retrievalReferenceReqGTP = fundtransfer.tid,
                                            //retrievalReferenceReqGTP = tid,
                                            retrievalReferenceReqGTP = retrievalReference,
                                            remarkReqGTP = "Get Transaction Payment",
                                            UserName = mobile,
                                            ClientCode = ClientCode,
                                            paypointType = paypointType,
                                            //remarkReqGTP = "Get Transaction Payment"+keyExecRlt,

                                        };
                                        string getTranResultResp = "";
                                        string keyGetTrancRlt = "";
                                        string resultMessageResGTP = "";
                                        string billNumberResGTP = "";
                                        using (WebClient wcGetTran = new WebClient())
                                        {
                                            wcGetTran.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                                            string HtmlResultGetTran = wcGetTran.UploadString(URIGetTran, GetTranParameters);

                                            XmlDocument xmlEDoc = new XmlDocument();
                                            xmlEDoc.LoadXml(HtmlResultGetTran);

                                            XmlNodeList nodeEPay = xmlEDoc.GetElementsByTagName("*");
                                            string resultEPay = nodeEPay[0].InnerText;
                                            string HtmlEPayResult = resultEPay;

                                            //for determing excutepayment key and result
                                            var readerEPay = new StringReader(HtmlEPayResult);
                                            var xdocEPay = XDocument.Load(readerEPay);

                                            XDocument docEPay = XDocument.Parse(xdocEPay.ToString());
                                            var xElemGPay = XElement.Parse(docEPay.ToString());
                                            if (xElemGPay.Attribute("Result").Value == "000")
                                            {
                                                getTranResultResp = xElemGPay.Attribute("Result").Value;
                                                keyGetTrancRlt = xElemGPay.Attribute("Key").Value;
                                                resultMessageResGTP = xElemGPay.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;
                                                if (!(resultMessageResGTP == "No data"))
                                                {
                                                    billNumberResGTP = xElemGPay.Descendants().Elements("BillNumber").Where(x => x.Name == "BillNumber").SingleOrDefault().Value;
                                                    statusResGTP = xElemGPay.Descendants().Elements("Status").Where(x => x.Name == "Status").SingleOrDefault().Value;

                                                    //for get transaction payment status validation
                                                    if (!(statusResGTP == "1" || statusResGTP == "5" || statusResGTP == "11" || statusResGTP == "13"))
                                                    {
                                                        if (statusResGTP == "4" || statusResGTP == "6" || statusResGTP == "14" || statusResGTP == "16")
                                                        {
                                                            // mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                            statusCode = "400";
                                                            //message = result;
                                                            message = "Failed";
                                                            failedmessage = message;
                                                            resultMessageResGTP = "failed";
                                                        }

                                                    }
                                                    else
                                                    {
                                                        statusCode = "200";
                                                        resultMessageResGTP = "sucess";
                                                    }
                                                }
                                                else
                                                {
                                                    //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                    statusCode = "400";
                                                    //message = result;
                                                    message = "Failed";
                                                    failedmessage = message;
                                                    resultMessageResGTP = "failed";
                                                }
                                            }
                                            else
                                            {
                                                resultMessageResGTP = xElemGPay.Descendants().Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;

                                            }


                                            //end get transaction payment status validation

                                            ////for get transaction payment response insert in database
                                            resGTPaypointDishHomeInfo = new PaypointModel()
                                            {
                                                companyCodeResGTP = companyCode,
                                                serviceCodeResGTP = serviceCode,
                                                accountResGTP = account,
                                                special1ResGTP = special1,
                                                special2ResGTP = special2,

                                                transactionDateResGTP = transactionDate,
                                                transactionIdResGTP = transactionId,
                                                userIdResGTP = userId,
                                                userPasswordResGTP = userPassword,
                                                salePointTypeResGTP = salePointType,

                                                refStanResGTP = refStan,
                                                amountResGTP = amount, //amountpay,
                                                billNumberResGTP = billNumberResGTP,
                                                //retrievalReferenceResGTP = fundtransfer.tid,
                                                //retrievalReferenceResGTP = tid,
                                                retrievalReferenceResGTP = retrievalReference,
                                                responseCodeResGTP = getTranResultResp,
                                                descriptionResGTP = "Get Transaction Payment " + keyGetTrancRlt,
                                                UserName = mobile,
                                                ClientCode = ClientCode,
                                                paypointType = paypointType,
                                                resultMessageResGTP = resultMessageResGTP,
                                                customerNameResGTP = customerName,

                                            };
                                        }
                                    } while (statusResGTP == "10" || statusResGTP == "15" || statusResGTP == "20" || statusResGTP == "21" || statusResGTP == "99" || statusResGTP == "12" || statusResGTP == "2" || statusResGTP == "0");

                                    ///for  Inserting GT PayPoint Data

                                    try
                                    {

                                        int resultsReqGTP = PaypointUtils.RequestGTPaypointInfo(reqGTPaypointDishHomeInfo);
                                        int resultsResGTP = PaypointUtils.ResponseGTPaypointInfo(resGTPaypointDishHomeInfo);

                                        if ((resultsReqGTP > 0) && (resultsResGTP > 0))
                                        {
                                            message = result;
                                        }
                                        else
                                        {
                                            message = result;
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        string ss = ex.Message;
                                        message = result;
                                    }

                                }

                            }

                            else
                            {
                                //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                statusCode = "400";
                                //message = result;
                                message = resultMessageResEP;
                                failedmessage = message;
                            }

                        }
                        else //ELSE FOR (rltCheckPaymt == "000")I.E Error  in result of CP
                        {
                            //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                            statusCode = "400";
                            // message = result;
                            message = resultMessageResCP;

                            failedmessage = message;

                            ////start:Com focus one log///
                            //MNFundTransfer mnft1 = new MNFundTransfer(tid, fundtransfer.sc, fundtransfer.da,
                            //    fundtransfer.sa, fundtransfer.amount, fundtransfer.mobile, fundtransfer.note, fundtransfer.pin,
                            //    fundtransfer.sourcechannel);
                            //var comfocuslog1 = new MNComAndFocusOneLog(mnft1, DateTime.Now);
                            //var mncomfocuslog1 = new MNComAndFocusOneLogsController();
                            //result = mncomfocuslog1.InsertIntoComFocusOne(comfocuslog1);
                            ////end:Com focus one log//

                            //if (mnft1.valid())
                            //{
                            //    var transaction1 = new MNTransactionMaster(mnft1);
                            //    var mntransaction1 = new MNTransactionsController();
                            //    validTransactionData = mntransaction1.Validate(transaction1, mnft1.pin);
                            //    result = validTransactionData.Response;

                            //}
                        }
                        //end excute dekhi get ko sabai comment gareko
                    }
                    catch (Exception ex)
                    {
                        message = result + ex + "Error Message ";
                        //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                        statusCode = "400";
                        failedmessage = "Please try again.";
                    }
                }

                //REverse Transaction 
                if ((statusCode != "111") && (statusCode != "114") && (statusCode != "115") && (statusCode != "116") && (statusCode != "119") &&
                    (statusCode != "121") && (statusCode != "163") && (statusCode != "180") && (statusCode != "181") && (statusCode != "182") &&
                    (statusCode != "183") && (statusCode != "184") && (statusCode != "185") && (statusCode != "186") && (statusCode != "187") &&
                    (statusCode != "188") && (statusCode != "189") && (statusCode != "190") && (statusCode != "800") && (statusCode != "902") &&
                    (statusCode != "904") && (statusCode != "906") && (statusCode != "907") && (statusCode != "909") && (statusCode != "911") &&
                    (statusCode != "913") && (statusCode != "90") && (statusCode != "91") && (statusCode != "94") && (statusCode != "95") &&
                    (statusCode != "98") && (statusCode != "99") && (statusCodeBalance != "400") && (compResultResp != "000") && (statusCodeBalance != "400")
                    && (statusCode != "200") && (statusCode != "508")
                    )
                //((statusCodeBalance != "400") && (compResultResp != "000")) || ((statusCodeBalance != "400") && (statusCode != "200")) ||
                //if (statusCode != "116")
                {
                    //TraceIdGenerator traceRevid = new TraceIdGenerator();
                    //tid = traceRevid.GenerateUniqueTraceID();
                    tid = retrievalReference;
                    if (sc == "00")
                    {
                        transactionType = "PayPoint Txfr to W2W";
                    }
                    else
                    {
                        sc = "01";
                        transactionType = "PayPoint Txfr to W2B"; //B2W
                    }
                    FundTransfer fundtransferRev = new FundTransfer
                    {
                        tid = tid,
                        sc = sc,
                        mobile = da,//mobile
                        da = mobile,//da
                        amount = amount,
                        pin = pin,
                        note = "reverse " + note,
                        sourcechannel = src
                    };
                    ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, fundtransfer);
                    //CustActivityModel custsmsInfo = new CustActivityModel();


                    // MNTransactionMaster validTransactionData = new MNTransactionMaster();


                    //if (TokenGenerator.TokenChecker(sessionID, mobile, src) == false)
                    //{
                    //    // throw ex
                    //    statusCode = "400";
                    //    message = "Session expired. Please login again";
                    //    failedmessage = message;
                    //}
                    //else
                    //{
                    if ((tid == null) || (sc == null) || (mobile == null) || (da == null) || (amount == null) || (pin == null) ||
                    (src == null) || (double.Parse(amount) <= 0) || (vid == null))
                    {
                        // throw ex
                        statusCode = "400";
                        message = "Parameters Missing/Invalid";
                        failedmessage = message;
                    }
                    if ((companyCode == null) || (companyCode == "") || (serviceCode == null) || (serviceCode == "") ||
                        (account == null) || (special1 == null) || (special2 == null) || (transactionDate == null) || (transactionId == null) || (transactionId == "") ||
                        (userId == null) || (userPassword == null) || (salePointType == null) || (userId == "") || (userPassword == "") || (salePointType == ""))
                    {
                        // throw ex
                        statusCode = "400";
                        message = "Parameters Missing/Invalid PayPoint";
                        failedmessage = message;
                    }
                    else
                    {
                        //if (sc == "00")
                        //{
                        //    transactionType = "PayPoint Txfr to W2W";
                        //}
                        //else 
                        //{
                        //    sc = "01";
                        //    transactionType = "PayPoint Txfr to W2B"; //B2W
                        //}

                        if (!(UserNameCheck.IsValidUser(mobile)))
                        {
                            // throw ex
                            statusCode = "400";
                            //message = "Transaction restricted to User";
                            message = "Transaction only for User";
                            failedmessage = message;
                        }
                        if (UserNameCheck.IsValidMerchant(da))
                        {

                            TransLimitCheck transLimitCheck = new TransLimitCheck();
                            string resultTranLimit = transLimitCheck.LimitCheck(mobile, da, amount, sc, pin, src);

                            var jsonDataResult = JObject.Parse(resultTranLimit);
                            statusCode = jsonDataResult["StatusCode"].ToString();
                            string statusMsg = jsonDataResult["StatusMessage"].ToString();
                            message = jsonDataResult["StatusMessage"].ToString();
                            failedmessage = message;

                            if ((statusCode == "200") && (message == "Success"))
                            {
                                //start: checking trace id
                                do
                                {
                                    TraceIdGenerator traceid = new TraceIdGenerator();
                                    tid = traceid.GenerateUniqueTraceID();
                                    fundtransfer.tid = tid;

                                    bool traceIdCheck = false;
                                    traceIdCheck = TraceIdCheck.IsValidTraceId(fundtransfer.tid);
                                    if (traceIdCheck == true)
                                    {
                                        result = "Trace ID Repeated";
                                    }
                                    else
                                    {
                                        result = "false";
                                    }

                                } while (result == "Trace ID Repeated");
                                //End: TraceId

                                //start:Com focus one log///
                                MNFundTransfer mnft = new MNFundTransfer(tid, fundtransferRev.sc, fundtransferRev.mobile,
                                       fundtransferRev.sa, fundtransferRev.amount, fundtransferRev.da, fundtransferRev.note, fundtransferRev.pin,
                                       fundtransferRev.sourcechannel, "T", "PayPoint");
                                var comfocuslog = new MNComAndFocusOneLog(mnft, DateTime.Now);
                                var mncomfocuslog = new MNComAndFocusOneLogsController();
                                //mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                result = mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                //end:Com focus one log//

                                if (result == "Success")
                                {
                                    //NOTE:- may be need to validate before insert into reply typpe
                                    //start:insert into reply type as HTTP//
                                    var replyType = new MNReplyType(tid, "HTTP");
                                    var mnreplyType = new MNReplyTypesController();
                                    mnreplyType.InsertIntoReplyType(replyType);
                                    //end:insert into reply type as HTTP//

                                    MNMerchantsController getMerchantDetails = new MNMerchantsController();

                                    string GetMerchantMobile = getMerchantDetails.PassVidToGetMerchantDetail(vid);
                                    if (GetMerchantMobile != "" || GetMerchantMobile != null)
                                    {
                                        fundtransfer.da = GetMerchantMobile; //Set Destination Merchant Mobile number
                                        GetMerchantName = getMerchantDetails.PassVIdToGetMerchantName(vid);
                                    }
                                    else
                                    {
                                        statusCode = "400";
                                        message = "Destination Merchant Doesnot Exists";
                                        mnft.Response = "Destination Merchant Doesnot Exists";
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, mnft.Response);
                                        result = mnft.Response;
                                        failedmessage = message;
                                    }


                                    //start:insert into transaction master//
                                    if (mnft.valid())
                                    {
                                        var transaction = new MNTransactionMaster(mnft);
                                        var mntransaction = new MNTransactionsController();
                                        //validTransactionData = mntransaction.Validate(transaction, mnft.pin);
                                        validTransactionData = mntransaction.Validatepaypoint(transaction, mnft.pin);
                                        result = validTransactionData.Response;

                                        /*** ***/
                                        ErrorMessage em = new ErrorMessage();

                                        if (validTransactionData.Response == "Error")
                                        {
                                            mnft.Response = "error";
                                            mnft.ResponseStatus(HttpStatusCode.InternalServerError,
                                                "Internal server error - try again later, or contact support");
                                            result = mnft.Response;
                                            statusCode = "500";
                                            message = "Internal server error - try again later, or contact support";
                                            failedmessage = message;
                                        }
                                        else
                                        {
                                            if ((result == "Trace ID Repeated") || (result == "Limit Exceed")
                                                || (result == "Invalid Source User") || (result == "Invalid Destination User")
                                                || (result == "Invalid Product Request") || (result == "Please try again") || (result == ""))
                                            {
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                statusCode = "400";
                                                message = result;
                                                failedmessage = message;
                                            }
                                            if (result == "Invalid PIN")
                                            {
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                statusCode = "400";
                                                message = result;
                                                failedmessage = message;
                                            }
                                            if (result == "111")
                                            {
                                                statusCode = result;
                                                message = em.Error_111;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "114")
                                            {
                                                statusCode = result;
                                                message = em.Error_114;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "115")
                                            {
                                                statusCode = result;
                                                message = em.Error_115;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "116")
                                            {
                                                statusCode = result;
                                                message = em.Error_116;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "119")
                                            {
                                                statusCode = result;
                                                message = em.Error_119;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "121")
                                            {
                                                statusCode = result;
                                                message = em.Error_121;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "163")
                                            {
                                                statusCode = result;
                                                message = em.Error_163;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "180")
                                            {
                                                statusCode = result;
                                                message = em.Error_180;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "181")
                                            {
                                                statusCode = result;
                                                message = em.Error_181;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "182")
                                            {
                                                statusCode = result;
                                                message = em.Error_182;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "183")
                                            {
                                                statusCode = result;
                                                message = em.Error_183;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "184")
                                            {
                                                statusCode = result;
                                                message = em.Error_184;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "185")
                                            {
                                                statusCode = result;
                                                message = em.Error_185;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "186")
                                            {
                                                statusCode = result;
                                                message = em.Error_186;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "187")
                                            {
                                                statusCode = result;
                                                message = em.Error_187;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "188")
                                            {
                                                statusCode = result;
                                                message = em.Error_188;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "189")
                                            {
                                                statusCode = result;
                                                message = em.Error_189;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "190")
                                            {
                                                statusCode = result;
                                                message = em.Error_190;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "800")
                                            {
                                                statusCode = result;
                                                message = em.Error_800;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "902")
                                            {
                                                statusCode = result;
                                                message = em.Error_902;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "904")
                                            {
                                                statusCode = result;
                                                message = em.Error_904;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "906")
                                            {
                                                statusCode = result;
                                                message = em.Error_906;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "907")
                                            {
                                                statusCode = result;
                                                message = em.Error_907;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "909")
                                            {
                                                statusCode = result;
                                                message = em.Error_909;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "911")
                                            {
                                                statusCode = result;
                                                message = em.Error_911;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "913")
                                            {
                                                statusCode = result;
                                                message = em.Error_913;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "90")
                                            {
                                                statusCode = result;
                                                message = em.Error_90;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "91")
                                            {
                                                statusCode = result;
                                                message = em.Error_91;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "92")
                                            {
                                                statusCode = result;
                                                message = em.Error_92;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "94")
                                            {
                                                statusCode = result;
                                                message = em.Error_94;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "95")
                                            {
                                                statusCode = result;
                                                message = em.Error_95;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "98")
                                            {
                                                statusCode = result;
                                                message = em.Error_98;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "99")
                                            {
                                                statusCode = result;
                                                message = em.Error_99;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            else if (validTransactionData.ResponseCode == "OK")
                                            {
                                                statusCode = "200";
                                                message = result;
                                                mnft.ResponseStatus(HttpStatusCode.OK, message);

                                            } //END ValidTransactionData.ResponseCode


                                        } //END validTransactionData.Response WITHOUT MNDB ERROR
                                        /*** ***/

                                        OutgoingWebResponseContext response1 = WebOperationContext.Current.OutgoingResponse;
                                        if (response1.StatusCode == HttpStatusCode.OK)
                                        {
                                            string messagereply = "";
                                            try
                                            {
                                                messagereply = "Dear " + CustCheckUtils.GetName(mobile) + "," + "\n";

                                                messagereply += " You have successfully reverse  NPR " + validTransactionData.Amount
                                                                    + " to " +
                                                                    //GetMerchantName 
                                                                    "Utility payment for Dish Home Direct." + " on date " +
                                                                    (validTransactionData.CreatedDate).ToString("dd/MM/yyyy")
                                                                + "." + "\n";
                                                messagereply += "Thank you. MNepal";

                                                var client = new WebClient();

                                                //SENDER
                                                if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                                                {
                                                    //FOR NCELL
                                                    var content = client.DownloadString(
                                                        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                                        + "977" + mobile + "&Text=" + messagereply + "");
                                                }
                                                else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                                            || (mobile.Substring(0, 3) == "986"))
                                                {
                                                    //FOR NTC
                                                    var content = client.DownloadString(
                                                        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                                        + "977" + mobile + "&Text=" + messagereply + "");
                                                }

                                                statusCode = "200";
                                                var v = new
                                                {
                                                    StatusCode = Convert.ToInt32(statusCode),
                                                    StatusMessage = result
                                                };
                                                result = JsonConvert.SerializeObject(v);

                                            }
                                            catch (Exception ex)
                                            {
                                                // throw ex
                                                statusCode = "400";
                                                message = ex.Message;
                                            }


                                            custsmsInfo = new CustActivityModel()
                                            {
                                                UserName = fundtransfer.mobile,
                                                RequestMerchant = transactionType,
                                                DestinationNo = fundtransfer.da,
                                                Amount = validTransactionData.Amount.ToString(),
                                                SMSStatus = "Success",
                                                SMSSenderReply = messagereply,
                                                ErrorMessage = "",
                                            };


                                        }
                                        else if ((response1.StatusCode == HttpStatusCode.BadRequest) || (response1.StatusCode == HttpStatusCode.InternalServerError) || (statusCode != "200"))
                                        {
                                            custsmsInfo = new CustActivityModel()
                                            {
                                                UserName = mobile,
                                                RequestMerchant = transactionType,
                                                DestinationNo = fundtransfer.da,
                                                Amount = validTransactionData.Amount.ToString(),
                                                SMSStatus = "Failed",
                                                SMSSenderReply = message,
                                                ErrorMessage = failedmessage,
                                            };

                                        }
                                        //end:insert into transaction master//

                                    } //END:insert into transaction master//
                                    else
                                    {
                                        mnft.Response = "error";
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, "parameters missing/invalid"); //200 - OK
                                        result = mnft.Response;
                                        statusCode = "400";
                                        message = "parameters missing/invalid";
                                        failedmessage = message;

                                        custsmsInfo = new CustActivityModel()
                                        {
                                            UserName = mobile,
                                            RequestMerchant = transactionType,
                                            DestinationNo = fundtransfer.da,
                                            Amount = amount,
                                            SMSStatus = "Failed",
                                            SMSSenderReply = message,
                                            ErrorMessage = failedmessage,
                                        };
                                    }

                                } //END MNComAndFocusOneLogsController
                                else
                                {
                                    statusCode = "400";
                                    mnft.Response = "Data Insertion Failed in Check DB Connection: TraceID limit might have exceeded";
                                    mnft.ResponseStatus(HttpStatusCode.InternalServerError, mnft.Response);
                                    result = mnft.Response;
                                    failedmessage = result;
                                }

                            } //END TRansLimit Check StatusCode N Message
                            else
                            {
                                custsmsInfo = new CustActivityModel()
                                {
                                    UserName = mobile,
                                    RequestMerchant = transactionType,
                                    DestinationNo = fundtransfer.da,
                                    Amount = amount,
                                    SMSStatus = "Failed",
                                    SMSSenderReply = message,
                                    ErrorMessage = failedmessage,
                                };
                            }

                        } //END IsValidMerchant

                        //} //END Destination mobile No check

                    }
                }
                //} //tokengenerator ko closing bracket

                //for sending sms  if success 
                if (compResultResp == "000")
                {
                    OutgoingWebResponseContext response2 = WebOperationContext.Current.OutgoingResponse;
                    if (response2.StatusCode == HttpStatusCode.OK)
                    {
                        string messagereply = "";
                        try
                        {
                            messagereply = "Dear " + CustCheckUtils.GetName(mobile) + "," + "\n";

                            messagereply += " You have successfully paid NPR " + validTransactionData.Amount
                                            + " to " + " account name: " + account + ". " +
                                            //GetMerchantName 
                                            "Utility payment for Dish Home Direct (Pinless)." + " on date " +
                                                (validTransactionData.CreatedDate).ToString("dd/MM/yyyy")
                                            + "." + "\n";
                            messagereply += "Thank you. MNepal";

                            var client = new WebClient();

                            //SENDER
                            if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                            {
                                //FOR NCELL
                                var content = client.DownloadString(
                                    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                    + "977" + mobile + "&Text=" + messagereply + "");
                            }
                            else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                        || (mobile.Substring(0, 3) == "986"))
                            {
                                //FOR NTC
                                var content = client.DownloadString(
                                    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                    + "977" + mobile + "&Text=" + messagereply + "");
                            }

                            statusCode = "200";
                            var v = new
                            {
                                StatusCode = Convert.ToInt32(statusCode),
                                StatusMessage = result
                            };
                            result = JsonConvert.SerializeObject(v);

                        }
                        catch (Exception ex)
                        {
                            // throw ex
                            statusCode = "400";
                            message = ex.Message;
                        }


                        custsmsInfo = new CustActivityModel()
                        {
                            UserName = fundtransfer.mobile,
                            RequestMerchant = transactionType,
                            DestinationNo = fundtransfer.da,
                            Amount = validTransactionData.Amount.ToString(),
                            SMSStatus = "Success",
                            SMSSenderReply = messagereply,
                            ErrorMessage = "",
                        };


                    }
                    else if ((response2.StatusCode == HttpStatusCode.BadRequest) || (response2.StatusCode == HttpStatusCode.InternalServerError) || (statusCode != "200"))
                    {
                        custsmsInfo = new CustActivityModel()
                        {
                            UserName = mobile,
                            RequestMerchant = transactionType,
                            DestinationNo = fundtransfer.da,
                            Amount = validTransactionData.Amount.ToString(),
                            SMSStatus = "Failed",
                            SMSSenderReply = message,
                            ErrorMessage = failedmessage,
                        };

                    }
                }

            }
            catch (Exception ex)
            {
                // throw ex
                statusCode = "400";
                message = ex.Message;
            }
            if (statusCodeBalance == "400")
            {
                statusCode = "400";
            }
            if (statusCode == "")
            {
                result = result.ToString();
            }
            //else if (statusCode != "200")
            else if ((statusCode != "200") || (statusCodeBalance == "400"))
            {
                if (message == "")
                {
                    if (string.IsNullOrEmpty(result))
                    {
                        result = failedmessage; // "Could not process your request,Please Try again.";
                    }
                    message = result;
                }
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = failedmessage
                };
                result = JsonConvert.SerializeObject(v);
            }
            return result;
        }
        #endregion

        #region"Check SmartCell TopUp Payment"
        [OperationContract]
        [WebInvoke(Method = "POST",
                  ResponseFormat = WebMessageFormat.Json)]
        public string SmartCellcheckpayment(Stream input)
        {
            string PaypointPwd = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointPwd"];
            string PaypointUserID = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointUserID"];
            System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate { return true; }; //to prevent from SSL error
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            string tid = qs["tid"];
            string vid = qs["vid"];
            string sc = "00";
            string mobile = qs["mobile"];
            string da = System.Web.Configuration.WebConfigurationManager.AppSettings["DestinationNoForTestServer"];
            string src = qs["src"];
            string result = "";
            string sessionID = qs["tokenID"];
            string companyCode = qs["companyCode"];
            string serviceCode = qs["serviceCode"];
            string account = qs["account"];
            string special1 = qs["special1"];
            string special2 = "";
            string transactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            long millisecondstrandId = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string transactionId = millisecondstrandId.ToString();
            string userId = PaypointUserID;
            string userPassword = PaypointPwd.Trim();
            string salePointType = "6";
            string ClientCode = qs["ClientCode"];
            string paypointType = qs["paypointType"];
            string transactionType = string.Empty;
            string resultMessageResCP = "";
            string note = "Utility payment for SmartCell of service code " + serviceCode + ". Mobile Number=" + qs["account"];

            PaypointModel reqCPPaypointNTCInfo = new PaypointModel();//to store data request of CP which is also commmon in nea
            PaypointModel resCPPaypointNTCInfo = new PaypointModel();//to store data response of CP which is also commmon in nea

            //PaypointModel resPaypointPaymentInfo = new PaypointModel();
            PaypointModel resPaypointNTCPaymentInfo = new PaypointModel();//to store data of Response of CP only

            string totalAmount = string.Empty;
            string totalCount = string.Empty;
            string totalBAmount = string.Empty;
            string totalBCount = string.Empty;
            string balance = string.Empty;
            string message = string.Empty;
            string statusCode = string.Empty;
            string failedmessage = string.Empty;
            string GetMerchantName = string.Empty;
            string retrievalRef = string.Empty;
            string refStanCK = string.Empty;

            string customerNo = string.Empty;
            TraceIdGenerator traceid = new TraceIdGenerator();
            tid = traceid.GenerateUniqueTraceID();


            if (serviceCode == "1")
            {
                special1 = "50";
            }
            if (serviceCode == "2")
            {
                special1 = "100";
            }
            if (serviceCode == "3")
            {
                special1 = "200";
            }
            if (serviceCode == "10")
            {
                special1 = "10";
            }
            if (serviceCode == "0")
            {
                special1 = "20";
            }
            if (serviceCode == "4")
            {
                special1 = "500";
            }
            if (serviceCode == "5")
            {
                special1 = "1000";
            }
            //for CP transaction for nepal water
            try
            {
                string URI = System.Web.Configuration.WebConfigurationManager.AppSettings["CPPaypointUrl"];

                string myParameters = "companyCode=" + companyCode + "&serviceCode=" + serviceCode +
                    "&account=" + account + "&special1=" + (Convert.ToInt32(special1) * 100).ToString() + "&special2=" + special2 +
                    "&transactionDate=" + transactionDate + "&transactionId=" + transactionId +
                    "&userId=" + userId.Trim() + "&userPassword=" + userPassword.Trim() + "&salePointType=" + salePointType;

                //for checkpayment request insert in database
                reqCPPaypointNTCInfo = new PaypointModel()
                {
                    companyCodeReqCP = companyCode,
                    serviceCodeReqCP = serviceCode,
                    accountReqCP = account,
                    special1ReqCP = special1,
                    special2ReqCP = special2,

                    transactionDateReqCP = transactionDate,
                    transactionIdReqCP = transactionId,
                    userIdReqCP = userId,
                    userPasswordReqCP = userPassword,
                    salePointTypeReqCP = salePointType,

                    refStanReqCP = "",
                    amountReqCP = "",//amountInPaisa
                    billNumberReqCP = "",
                    //retrievalReferenceReqCP = fundtransfer.tid,
                    retrievalReferenceReqCP = tid,
                    remarkReqCP = "Check Payment",
                    UserName = mobile,
                    ClientCode = ClientCode,
                    paypointType = paypointType,

                };

                string billNumber = "";
                string amountpay = "";
                string refStan = "";
                string exectransactionId = ""; //Unique
                string exectransactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"); //Current DateTime
                string rltCheckPaymt = "";
                string customerName = "";
                string mask = "";

                using (WebClient wc = new WebClient())
                {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    var HtmlResult = wc.UploadString(URI, myParameters);// response from checkpayment

                    XmlDocument xmlDoc = new XmlDocument();

                    xmlDoc.LoadXml(HtmlResult);

                    XmlNodeList test = xmlDoc.GetElementsByTagName("*");
                    string results = test[0].InnerText;
                    string HtmlResult1 = results;

                    //for getting key value from check payment
                    var reader = new StringReader(HtmlResult1);
                    var xdoc = XDocument.Load(reader);

                    XDocument docParse = XDocument.Parse(xdoc.ToString());
                    IEnumerable<XElement> responses = docParse.Descendants();

                    var xElem = XElement.Parse(docParse.ToString());

                    rltCheckPaymt = xElem.Attribute("Result").Value;
                    string keyrlt = "";// response.Attribute("Key").Value;


                    message = resultMessageResCP;
                    if (xElem.Attribute("Result").Value == "000")
                    {
                        keyrlt = xElem.Attribute("Key").Value;
                        billNumber = xElem.Descendants().Elements("BillNumber").Where(x => x.Name == "BillNumber").SingleOrDefault().Value;
                        amountpay = xElem.Descendants().Elements("Amount").Where(x => x.Name == "Amount").SingleOrDefault().Value;
                        refStan = xElem.Descendants().Elements("RefStan").Where(x => x.Name == "RefStan").SingleOrDefault().Value;
                        exectransactionDate = xElem.Descendants().Elements("DueDate").Where(x => x.Name == "DueDate").SingleOrDefault().Value;
                        customerName = xElem.Descendants().Elements("ReserveInfo").Where(x => x.Name == "ReserveInfo").SingleOrDefault().Value;
                        mask = xElem.Descendants().Elements("mask").Where(x => x.Name == "mask").SingleOrDefault().Value;


                        resPaypointNTCPaymentInfo = new PaypointModel()
                        {
                            billNumber = billNumber,
                            refStan = refStan,
                            amount = amountpay,
                            transactionDate = exectransactionDate,
                            customerName = account,
                            companyCode = companyCode,
                            UserName = mobile,
                            ClientCode = ClientCode,
                            serviceCode = serviceCode

                        };
                        //end list of package

                        int resultsPayments = PaypointUtils.PaypointUtilitySmartCellTopUpInfo(resPaypointNTCPaymentInfo);


                    }
                    else
                    {
                        keyrlt = xElem.Attribute("Key").Value;
                        resultMessageResCP = xElem.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;
                    }
                    resultMessageResCP = xElem.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;

                    //for checkpayment response insert in database in nepal water
                    resCPPaypointNTCInfo = new PaypointModel()
                    {
                        companyCodeResCP = companyCode,
                        serviceCodeResCP = serviceCode,
                        accountResCP = account,
                        special1ResCP = special1,
                        special2ResCP = special2,

                        transactionDateResCP = transactionDate,
                        transactionIdResCP = transactionId,
                        userIdResCP = userId,
                        userPasswordResCP = userPassword,
                        salePointTypeResCP = salePointType,

                        refStanResCP = refStan,
                        amountResCP = amountpay,
                        billNumberResCP = billNumber,
                        //retrievalReferenceResCP = fundtransfer.tid,
                        retrievalReferenceResCP = tid,
                        responseCodeResCP = rltCheckPaymt,
                        descriptionResCP = "Check Payment " + keyrlt,
                        customerNameCP = customerName,
                        UserName = mobile,
                        ClientCode = ClientCode,
                        paypointType = paypointType,
                        resultMessageResCP = resultMessageResCP,

                    };
                }


                if (!(rltCheckPaymt == "000"))//show error when CP response is not 000 
                {
                    statusCode = "400";
                    // message = result;
                    message = resultMessageResCP;
                    failedmessage = message;

                }
                else
                {
                    statusCode = "200";
                    message = resultMessageResCP;
                    failedmessage = message;
                }
            }
            catch (Exception ex)
            {
                message = result + ex + "Error Message ";
                //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                statusCode = "400";
                // failedmessage = message;
                failedmessage = resultMessageResCP;

            }

            ///for  inserting CP PayPoint Data of Nepal Water which is simlar to nea 

            try
            {
                int resultsReqCP = PaypointUtils.RequestCPPaypointInfo(reqCPPaypointNTCInfo);
                int resultsResCP = PaypointUtils.ResponseCPPaypointInfo(resCPPaypointNTCInfo);


                if ((resultsReqCP > 0) && (resultsResCP > 0))

                {
                    message = result;
                }
                else
                {
                    message = result;
                }

            }
            catch (Exception ex)
            {
                string ss = ex.Message;
                message = result;
            }

            if (statusCode == "")
            {
                result = result.ToString();
            }
            else if (statusCode == "200")
            {
                if (message == "")
                {
                    if (string.IsNullOrEmpty(result))
                    {
                        result = failedmessage; // "Could not process your request,Please Try again.";
                    }
                    message = result;
                }
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = failedmessage,
                    retrievalRef = resCPPaypointNTCInfo.retrievalReferenceResCP,
                    refStanCK = resCPPaypointNTCInfo.refStanResCP
                };
                result = JsonConvert.SerializeObject(v);
            }
            else if (statusCode != "200")
            {
                if (message == "")
                {
                    if (string.IsNullOrEmpty(result))
                    {
                        result = failedmessage; // "Could not process your request,Please Try again.";
                    }
                    message = result;
                }
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = failedmessage,
                    retrievalRef = "",
                    refStanCK = ""
                };
                result = JsonConvert.SerializeObject(v);
            }
            return result;

        }
        #endregion

        #region"SmartCell TopUp ExecutePayment Paypoint"
        [OperationContract]
        [WebInvoke(Method = "POST",
                  ResponseFormat = WebMessageFormat.Json)]
        public string SmartCellTopUpExecutePayment(Stream input)
        {
            string PaypointPwd = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointPwd"];
            string PaypointUserID = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointUserID"];

            System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);
            MNTransactionMaster validTransactionData = new MNTransactionMaster();
            CustActivityModel custsmsInfo = new CustActivityModel();

            string tid = qs["tid"];
            string vid = qs["vid"]; //MerchantID
            string sc = qs["sc"];
            string mobile = qs["mobile"];
            string da = System.Web.Configuration.WebConfigurationManager.AppSettings["DestinationNoForTestServer"];


            string amount = qs["amount"];//amount paid by customer 
            string pin = qs["pin"];
            string note = "utility payment for SmartCell TopUp. Customer Name=" + qs["account"];//+ ". " + qs["note"];
            string src = qs["src"];
            string result = "";
            string sessionID = qs["tokenID"];
            string resultMessageResCP = "";
            string companyCode = qs["companyCode"];
            string serviceCode = qs["serviceCode"];
            string account = qs["account"];
            string special1 = qs["special1"]; //
            string special2 = ""; //
            string transactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");//"2019-11-22T11:11:02";
            long millisecondstrandId = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string transactionId = millisecondstrandId.ToString(); //"120163339819";
            string userId = PaypointUserID;
            string userPassword = PaypointPwd.Trim();
            string salePointType = "6";
            string transactionType = string.Empty;
            string amountpay = qs["amountpay"];//amount need to pay i.e amount in bill
            string exectransactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            string exectransactionId = millisecondstrandId.ToString();
            string refStan = qs["refStan"];
            string billNumber = qs["billNumber"];
            string rltCheckPaymt = qs["rltCheckPaymt"];
            string ClientCode = qs["ClientCode"];
            string paypointType = qs["paypointType"];
            //string paypointType = "Nepal Water";
            //string paypointType = "3";

            string customerName = qs["customerName"];
            string walletBalance = qs["walletBalance"];//in Rs.
            string retrievalReference = qs["retrievalReference"];
            int walletBalancePaisaInt = 0;
            walletBalancePaisaInt = Convert.ToInt32((Convert.ToDouble(walletBalance)) * 100);
            int amountpayInt = Convert.ToInt32(float.Parse(amount)) * 100;

            PaypointModel reqEPPaypointDishHomeInfo = new PaypointModel();
            PaypointModel resEPPaypointDishHomeInfo = new PaypointModel();

            PaypointModel reqGTPaypointDishHomeInfo = new PaypointModel();
            PaypointModel resGTPaypointDishHomeInfo = new PaypointModel();

            string totalAmount = string.Empty;
            string totalCount = string.Empty;
            string totalBAmount = string.Empty;
            string totalBCount = string.Empty;
            string balance = string.Empty;
            string message = string.Empty;
            string statusCode = string.Empty;
            string failedmessage = string.Empty;
            string GetMerchantName = string.Empty;
            string statusCodeBalance = string.Empty;

            string customerNo = string.Empty;

            // TraceIdGenerator traceid = new TraceIdGenerator();
            //tid = traceid.GenerateUniqueTraceID();
            tid = retrievalReference;

            FundTransfer fundtransfer = new FundTransfer
            {
                tid = tid,
                sc = sc,
                mobile = mobile,
                da = da,
                amount = amount,
                pin = pin,
                note = note,
                sourcechannel = src
            };
            if (sc == "00")
            {
                if (walletBalancePaisaInt >= amountpayInt)// if wallet balance less then bill amount then show error msg
                {
                    //First transaction MNRequest N Response
                    try
                    {

                        //FundTransfer fundtransfer = new FundTransfer
                        //{
                        //    tid = tid,
                        //    sc = sc,
                        //    mobile = mobile,
                        //    da = da,
                        //    amount = amount,
                        //    pin = pin,
                        //    note = note,
                        //    sourcechannel = src
                        //};

                        ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, fundtransfer);
                        //CustActivityModel custsmsInfo = new CustActivityModel();

                        //MNTransactionMaster validTransactionData = new MNTransactionMaster();


                        //if (TokenGenerator.TokenChecker(sessionID, mobile, src) == false)
                        //{
                        //    // throw ex
                        //    statusCode = "400";
                        //    message = "Session expired. Please login again";
                        //    failedmessage = message;
                        //}
                        //else
                        //{
                        if ((tid == null) || (sc == null) || (mobile == null) || (da == null) || (amount == null) || (pin == null) ||
                        (src == null) || (double.Parse(amount) <= 0) || (vid == null))
                        {
                            // throw ex
                            statusCode = "400";
                            message = "Parameters Missing/Invalid";
                            failedmessage = message;
                        }
                        if ((companyCode == null) || (companyCode == "") || (serviceCode == null) || (serviceCode == "") ||
                            (account == null) || (special1 == null) || (special2 == null) || (transactionDate == null) || (transactionId == null) || (transactionId == "") ||
                            (userId == null) || (userPassword == null) || (salePointType == null) || (userId == "") || (userPassword == "") || (salePointType == ""))
                        {
                            // throw ex
                            statusCode = "400";
                            message = "Parameters Missing/Invalid PayPoint";
                            failedmessage = message;
                        }
                        else
                        {
                            if (sc == "00")
                            {
                                transactionType = "PayPoint Txfr to W2W";
                            }
                            else if (sc == "10")
                            {
                                transactionType = "PayPoint Txfr to B2W"; //B2W
                            }

                            if (!(UserNameCheck.IsValidUser(mobile)))
                            {
                                // throw ex
                                statusCode = "400";
                                //message = "Transaction restricted to User";
                                message = "Transaction only for User";
                                failedmessage = message;
                            }
                            if (UserNameCheck.IsValidMerchant(da))
                            {

                                TransLimitCheck transLimitCheck = new TransLimitCheck();
                                string resultTranLimit = transLimitCheck.LimitCheck(mobile, da, amount, sc, pin, src);

                                var jsonDataResult = JObject.Parse(resultTranLimit);
                                statusCode = jsonDataResult["StatusCode"].ToString();
                                string statusMsg = jsonDataResult["StatusMessage"].ToString();
                                message = jsonDataResult["StatusMessage"].ToString();
                                failedmessage = message;

                                if ((statusCode == "200") && (message == "Success"))
                                {
                                    //start: checking trace id
                                    do
                                    {
                                        //TraceIdGenerator traceid = new TraceIdGenerator();
                                        // tid = traceid.GenerateUniqueTraceID();

                                        fundtransfer.tid = tid;
                                        bool traceIdCheck = false;
                                        traceIdCheck = TraceIdCheck.IsValidTraceId(fundtransfer.tid);
                                        if (traceIdCheck == true)
                                        {
                                            result = "Trace ID Repeated";
                                        }
                                        else
                                        {
                                            result = "false";
                                        }

                                    } while (result == "Trace ID Repeated");
                                    //End: TraceId

                                    //start:Com focus one log///
                                    MNFundTransfer mnft = new MNFundTransfer(tid, fundtransfer.sc, fundtransfer.mobile,
                                        fundtransfer.sa, fundtransfer.amount, fundtransfer.da, fundtransfer.note, fundtransfer.pin,
                                        fundtransfer.sourcechannel);
                                    var comfocuslog = new MNComAndFocusOneLog(mnft, DateTime.Now);
                                    var mncomfocuslog = new MNComAndFocusOneLogsController();
                                    //mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                    result = mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                    //end:Com focus one log//

                                    if (result == "Success")
                                    {
                                        //NOTE:- may be need to validate before insert into reply typpe
                                        //start:insert into reply type as HTTP//
                                        var replyType = new MNReplyType(tid, "HTTP");
                                        var mnreplyType = new MNReplyTypesController();
                                        mnreplyType.InsertIntoReplyType(replyType);
                                        //end:insert into reply type as HTTP//

                                        MNMerchantsController getMerchantDetails = new MNMerchantsController();

                                        string GetMerchantMobile = getMerchantDetails.PassVidToGetMerchantDetail(vid);
                                        if (GetMerchantMobile != "" || GetMerchantMobile != null)
                                        {
                                            fundtransfer.da = GetMerchantMobile; //Set Destination Merchant Mobile number
                                            GetMerchantName = getMerchantDetails.PassVIdToGetMerchantName(vid);
                                        }
                                        else
                                        {
                                            statusCode = "400";
                                            message = "Destination Merchant Doesnot Exists";
                                            mnft.Response = "Destination Merchant Doesnot Exists";
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, mnft.Response);
                                            result = mnft.Response;
                                            failedmessage = message;
                                        }


                                        //start:insert into transaction master//
                                        if (mnft.valid())
                                        {
                                            //var transaction = new MNTransactionMaster(mnft);
                                            //var mntransaction = new MNTransactionsController();
                                            //validTransactionData = mntransaction.Validate(transaction, mnft.pin);

                                            var transactionpaypoint = new MNTransactionMaster(mnft);
                                            var mntransactionpaypoint = new MNTransactionsController();
                                            validTransactionData = mntransactionpaypoint.Validatepaypoint(transactionpaypoint, mnft.pin);

                                            result = validTransactionData.Response;
                                            /*** ***/
                                            ErrorMessage em = new ErrorMessage();

                                            if (validTransactionData.Response == "Error")
                                            {
                                                mnft.Response = "error";
                                                mnft.ResponseStatus(HttpStatusCode.InternalServerError,
                                                    "Internal server error - try again later, or contact support");
                                                result = mnft.Response;
                                                statusCode = "500";
                                                message = "Internal server error - try again later, or contact support";
                                                failedmessage = message;
                                            }
                                            else
                                            {
                                                if ((result == "Trace ID Repeated") || (result == "Limit Exceed")
                                                    || (result == "Invalid Source User") || (result == "Invalid Destination User")
                                                    || (result == "Invalid Product Request") || (result == "Please try again") || (result == ""))
                                                {
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                    statusCode = "400";
                                                    message = result;
                                                    failedmessage = message;
                                                }
                                                if (result == "Invalid PIN")
                                                {
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                    statusCode = "400";
                                                    message = result;
                                                    failedmessage = message;
                                                }
                                                if (result == "111")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_111;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "114")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_114;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "115")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_115;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "116")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_116;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "119")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_119;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "121")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_121;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "163")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_163;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "180")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_180;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "181")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_181;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "182")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_182;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "183")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_183;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "184")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_184;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "185")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_185;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "186")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_186;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "187")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_187;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "188")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_188;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "189")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_189;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "190")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_190;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "800")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_800;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "902")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_902;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "904")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_904;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "906")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_906;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "907")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_907;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "909")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_909;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "911")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_911;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "913")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_913;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "90")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_90;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "91")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_91;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "92")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_92;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "94")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_94;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "95")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_95;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "98")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_98;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "99")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_99;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "508")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_508;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                else if (validTransactionData.ResponseCode == "OK")
                                                {
                                                    statusCode = "200";
                                                    message = result;
                                                    mnft.ResponseStatus(HttpStatusCode.OK, message);

                                                } //END ValidTransactionData.ResponseCode


                                            } //END validTransactionData.Response WITHOUT MNDB ERROR

                                            //start comment outgoing
                                            //OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                                            //if (response.StatusCode == HttpStatusCode.OK)
                                            //{
                                            //    string messagereply = "";
                                            //    try
                                            //    {
                                            //        //messagereply = "Dear " + CustCheckUtils.GetName(mobile) + "," + "\n";

                                            //        //messagereply += " You have successfully paid NPR " + validTransactionData.Amount
                                            //        //                + " to " + GetMerchantName + " on date " +
                                            //        //                    (validTransactionData.CreatedDate).ToString("dd/MM/yyyy")
                                            //        //                + "." + "\n";
                                            //        //messagereply += "Thank you. MNepal";

                                            //        //var client = new WebClient();

                                            //        ////SENDER
                                            //        //if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                                            //        //{
                                            //        //    //FOR NCELL
                                            //        //    var content = client.DownloadString(
                                            //        //        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                            //        //        + "977" + mobile + "&Text=" + messagereply + "");
                                            //        //}
                                            //        //else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                            //        //            || (mobile.Substring(0, 3) == "986"))
                                            //        //{
                                            //        //    //FOR NTC
                                            //        //    var content = client.DownloadString(
                                            //        //        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                            //        //        + "977" + mobile + "&Text=" + messagereply + "");
                                            //        //}

                                            //        statusCode = "200";
                                            //        var v = new
                                            //        {
                                            //            StatusCode = Convert.ToInt32(statusCode),
                                            //            StatusMessage = result
                                            //        };
                                            //        result = JsonConvert.SerializeObject(v);

                                            //    }
                                            //    catch (Exception ex)
                                            //    {
                                            //        // throw ex
                                            //        statusCode = "400";
                                            //        message = ex.Message;
                                            //    }


                                            //    custsmsInfo = new CustActivityModel()
                                            //    {
                                            //        UserName = fundtransfer.mobile,
                                            //        RequestMerchant = transactionType,
                                            //        DestinationNo = fundtransfer.da,
                                            //        Amount = validTransactionData.Amount.ToString(),
                                            //        SMSStatus = "Success",
                                            //        SMSSenderReply = messagereply,
                                            //        ErrorMessage = "",
                                            //    };


                                            //}
                                            //else if ((response.StatusCode == HttpStatusCode.BadRequest) || (response.StatusCode == HttpStatusCode.InternalServerError) || (statusCode != "200"))
                                            //{
                                            //    custsmsInfo = new CustActivityModel()
                                            //    {
                                            //        UserName = mobile,
                                            //        RequestMerchant = transactionType,
                                            //        DestinationNo = fundtransfer.da,
                                            //        Amount = validTransactionData.Amount.ToString(),
                                            //        SMSStatus = "Failed",
                                            //        SMSSenderReply = message,
                                            //        ErrorMessage = failedmessage,
                                            //    };

                                            //}

                                            //end comment outgoint
                                            //end:insert into transaction master//

                                        } //END:insert into transaction master//
                                        else
                                        {
                                            mnft.Response = "error";
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, "parameters missing/invalid"); //200 - OK
                                            result = mnft.Response;
                                            statusCode = "400";
                                            message = "parameters missing/invalid";
                                            failedmessage = message;

                                            custsmsInfo = new CustActivityModel()
                                            {
                                                UserName = mobile,
                                                RequestMerchant = transactionType,
                                                DestinationNo = fundtransfer.da,
                                                Amount = amount,
                                                SMSStatus = "Failed",
                                                SMSSenderReply = message,
                                                ErrorMessage = failedmessage,
                                            };
                                        }

                                    } //END MNComAndFocusOneLogsController
                                    else
                                    {
                                        statusCode = "400";
                                        mnft.Response = "Data Insertion Failed in Check DB Connection: TraceID limit might have exceeded";
                                        mnft.ResponseStatus(HttpStatusCode.InternalServerError, mnft.Response);
                                        result = mnft.Response;
                                        failedmessage = result;
                                    }

                                } //END TRansLimit Check StatusCode N Message
                                else
                                {
                                    custsmsInfo = new CustActivityModel()
                                    {
                                        UserName = mobile,
                                        RequestMerchant = transactionType,
                                        DestinationNo = fundtransfer.da,
                                        Amount = amount,
                                        SMSStatus = "Failed",
                                        SMSSenderReply = message,
                                        ErrorMessage = failedmessage,
                                    };
                                }

                            } //END IsValidMerchant

                            //} //END Destination mobile No check

                        }

                    }
                    catch (Exception ex)
                    {
                        message = result + ex + "Error Message ";
                        //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                        statusCode = "400";
                        //failedmessage = message;
                        failedmessage = "Please try again.";
                    }

                }
                else  //else for  if wallet balance less then bill amount then show error msg
                {
                    statusCodeBalance = "400";
                    message = "Insufficient Balance";
                    failedmessage = message;

                }
            }
            else if (sc == "10")//for bank payment in nepal water
            {
                //if (bankBalancePaisaInt >= amountpayInt)
                //{
                //First transaction MNRequest N Response
                try
                {

                    //FundTransfer fundtransfer = new FundTransfer
                    //{
                    //    tid = tid,
                    //    sc = sc,
                    //    mobile = mobile,
                    //    da = da,
                    //    amount = amount,
                    //    pin = pin,
                    //    note = note,
                    //    sourcechannel = src
                    //};

                    ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, fundtransfer);
                    //CustActivityModel custsmsInfo = new CustActivityModel();

                    //MNTransactionMaster validTransactionData = new MNTransactionMaster();


                    //if (TokenGenerator.TokenChecker(sessionID, mobile, src) == false)
                    //{
                    //    // throw ex
                    //    statusCode = "400";
                    //    message = "Session expired. Please login again";
                    //    failedmessage = message;
                    //}
                    //else
                    //{
                    if ((tid == null) || (sc == null) || (mobile == null) || (da == null) || (amount == null) || (pin == null) ||
                    (src == null) || (double.Parse(amount) <= 0) || (vid == null))
                    {
                        // throw ex
                        statusCode = "400";
                        message = "Parameters Missing/Invalid";
                        failedmessage = message;
                    }
                    if ((companyCode == null) || (companyCode == "") || (serviceCode == null) || (serviceCode == "") ||
                        (account == null) || (special1 == null) || (special2 == null) || (transactionDate == null) || (transactionId == null) || (transactionId == "") ||
                        (userId == null) || (userPassword == null) || (salePointType == null) || (userId == "") || (userPassword == "") || (salePointType == ""))
                    {
                        // throw ex
                        statusCode = "400";
                        message = "Parameters Missing/Invalid PayPoint";
                        failedmessage = message;
                    }
                    else
                    {
                        if (sc == "00")
                        {
                            transactionType = "PayPoint Txfr to W2W";
                        }
                        else if (sc == "10")
                        {
                            transactionType = "PayPoint Txfr to B2W"; //B2W
                        }

                        if (!(UserNameCheck.IsValidUser(mobile)))
                        {
                            // throw ex
                            statusCode = "400";
                            //message = "Transaction restricted to User";
                            message = "Transaction only for User";
                            failedmessage = message;
                        }
                        if (UserNameCheck.IsValidMerchant(da))
                        {

                            TransLimitCheck transLimitCheck = new TransLimitCheck();
                            string resultTranLimit = transLimitCheck.LimitCheck(mobile, da, amount, sc, pin, src);

                            var jsonDataResult = JObject.Parse(resultTranLimit);
                            statusCode = jsonDataResult["StatusCode"].ToString();
                            string statusMsg = jsonDataResult["StatusMessage"].ToString();
                            message = jsonDataResult["StatusMessage"].ToString();
                            failedmessage = message;

                            if ((statusCode == "200") && (message == "Success"))
                            {
                                //start: checking trace id
                                do
                                {
                                    //TraceIdGenerator traceid = new TraceIdGenerator();
                                    // tid = traceid.GenerateUniqueTraceID();
                                    fundtransfer.tid = tid;

                                    bool traceIdCheck = false;
                                    traceIdCheck = TraceIdCheck.IsValidTraceId(fundtransfer.tid);
                                    if (traceIdCheck == true)
                                    {
                                        result = "Trace ID Repeated";
                                    }
                                    else
                                    {
                                        result = "false";
                                    }

                                } while (result == "Trace ID Repeated");
                                //End: TraceId

                                //start:Com focus one log///
                                MNFundTransfer mnft = new MNFundTransfer(tid, fundtransfer.sc, fundtransfer.mobile,
                                    fundtransfer.sa, fundtransfer.amount, fundtransfer.da, fundtransfer.note, fundtransfer.pin,
                                    fundtransfer.sourcechannel);
                                var comfocuslog = new MNComAndFocusOneLog(mnft, DateTime.Now);
                                var mncomfocuslog = new MNComAndFocusOneLogsController();
                                //mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                result = mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                //end:Com focus one log//

                                if (result == "Success")
                                {
                                    //NOTE:- may be need to validate before insert into reply typpe
                                    //start:insert into reply type as HTTP//
                                    var replyType = new MNReplyType(tid, "HTTP");
                                    var mnreplyType = new MNReplyTypesController();
                                    mnreplyType.InsertIntoReplyType(replyType);
                                    //end:insert into reply type as HTTP//

                                    MNMerchantsController getMerchantDetails = new MNMerchantsController();

                                    string GetMerchantMobile = getMerchantDetails.PassVidToGetMerchantDetail(vid);
                                    if (GetMerchantMobile != "" || GetMerchantMobile != null)
                                    {
                                        fundtransfer.da = GetMerchantMobile; //Set Destination Merchant Mobile number
                                        GetMerchantName = getMerchantDetails.PassVIdToGetMerchantName(vid);
                                    }
                                    else
                                    {
                                        statusCode = "400";
                                        message = "Destination Merchant Doesnot Exists";
                                        mnft.Response = "Destination Merchant Doesnot Exists";
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, mnft.Response);
                                        result = mnft.Response;
                                        failedmessage = message;
                                    }


                                    //start:insert into transaction master//
                                    if (mnft.valid())
                                    {
                                        //var transaction = new MNTransactionMaster(mnft);
                                        //var mntransaction = new MNTransactionsController();
                                        //validTransactionData = mntransaction.Validate(transaction, mnft.pin);


                                        var transactionpaypoint = new MNTransactionMaster(mnft);
                                        var mntransactionpaypoint = new MNTransactionsController();
                                        validTransactionData = mntransactionpaypoint.Validatepaypoint(transactionpaypoint, mnft.pin);
                                        result = validTransactionData.Response;
                                        /*** ***/
                                        ErrorMessage em = new ErrorMessage();

                                        if (validTransactionData.Response == "Error")
                                        {
                                            mnft.Response = "error";
                                            mnft.ResponseStatus(HttpStatusCode.InternalServerError,
                                                "Internal server error - try again later, or contact support");
                                            result = mnft.Response;
                                            statusCode = "500";
                                            message = "Internal server error - try again later, or contact support";
                                            failedmessage = message;
                                        }
                                        else
                                        {
                                            if ((result == "Trace ID Repeated") || (result == "Limit Exceed")
                                                || (result == "Invalid Source User") || (result == "Invalid Destination User")
                                                || (result == "Invalid Product Request") || (result == "Please try again") || (result == ""))
                                            {
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                statusCode = "400";
                                                message = result;
                                                failedmessage = message;
                                            }
                                            if (result == "Invalid PIN")
                                            {
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                statusCode = "400";
                                                message = result;
                                                failedmessage = message;
                                            }
                                            if (result == "111")
                                            {
                                                statusCode = result;
                                                message = em.Error_111;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "114")
                                            {
                                                statusCode = result;
                                                message = em.Error_114;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "115")
                                            {
                                                statusCode = result;
                                                message = em.Error_115;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "116")
                                            {
                                                statusCode = result;
                                                message = em.Error_116;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "119")
                                            {
                                                statusCode = result;
                                                message = em.Error_119;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "121")
                                            {
                                                statusCode = result;
                                                message = em.Error_121;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "163")
                                            {
                                                statusCode = result;
                                                message = em.Error_163;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "180")
                                            {
                                                statusCode = result;
                                                message = em.Error_180;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "181")
                                            {
                                                statusCode = result;
                                                message = em.Error_181;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "182")
                                            {
                                                statusCode = result;
                                                message = em.Error_182;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "183")
                                            {
                                                statusCode = result;
                                                message = em.Error_183;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "184")
                                            {
                                                statusCode = result;
                                                message = em.Error_184;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "185")
                                            {
                                                statusCode = result;
                                                message = em.Error_185;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "186")
                                            {
                                                statusCode = result;
                                                message = em.Error_186;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "187")
                                            {
                                                statusCode = result;
                                                message = em.Error_187;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "188")
                                            {
                                                statusCode = result;
                                                message = em.Error_188;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "189")
                                            {
                                                statusCode = result;
                                                message = em.Error_189;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "190")
                                            {
                                                statusCode = result;
                                                message = em.Error_190;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "800")
                                            {
                                                statusCode = result;
                                                message = em.Error_800;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "902")
                                            {
                                                statusCode = result;
                                                message = em.Error_902;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "904")
                                            {
                                                statusCode = result;
                                                message = em.Error_904;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "906")
                                            {
                                                statusCode = result;
                                                message = em.Error_906;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "907")
                                            {
                                                statusCode = result;
                                                message = em.Error_907;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "909")
                                            {
                                                statusCode = result;
                                                message = em.Error_909;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "911")
                                            {
                                                statusCode = result;
                                                message = em.Error_911;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "913")
                                            {
                                                statusCode = result;
                                                message = em.Error_913;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "90")
                                            {
                                                statusCode = result;
                                                message = em.Error_90;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "91")
                                            {
                                                statusCode = result;
                                                message = em.Error_91;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "92")
                                            {
                                                statusCode = result;
                                                message = em.Error_92;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "94")
                                            {
                                                statusCode = result;
                                                message = em.Error_94;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "95")
                                            {
                                                statusCode = result;
                                                message = em.Error_95;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "98")
                                            {
                                                statusCode = result;
                                                message = em.Error_98;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "99")
                                            {
                                                statusCode = result;
                                                message = em.Error_99;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "508")
                                            {
                                                statusCode = result;
                                                message = em.Error_508;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            else if (validTransactionData.ResponseCode == "OK")
                                            {
                                                statusCode = "200";
                                                message = result;
                                                mnft.ResponseStatus(HttpStatusCode.OK, message);

                                            } //END ValidTransactionData.ResponseCode


                                        } //END validTransactionData.Response WITHOUT MNDB ERROR

                                        //end:insert into transaction master//

                                    } //END:insert into transaction master//
                                    else
                                    {
                                        mnft.Response = "error";
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, "parameters missing/invalid"); //200 - OK
                                        result = mnft.Response;
                                        statusCode = "400";
                                        message = "parameters missing/invalid";
                                        failedmessage = message;

                                        custsmsInfo = new CustActivityModel()
                                        {
                                            UserName = mobile,
                                            RequestMerchant = transactionType,
                                            DestinationNo = fundtransfer.da,
                                            Amount = amount,
                                            SMSStatus = "Failed",
                                            SMSSenderReply = message,
                                            ErrorMessage = failedmessage,
                                        };
                                    }

                                } //END MNComAndFocusOneLogsController
                                else
                                {
                                    statusCode = "400";
                                    mnft.Response = "Data Insertion Failed in Check DB Connection: TraceID limit might have exceeded";
                                    mnft.ResponseStatus(HttpStatusCode.InternalServerError, mnft.Response);
                                    result = mnft.Response;
                                    failedmessage = result;
                                }

                            } //END TRansLimit Check StatusCode N Message
                            else
                            {
                                custsmsInfo = new CustActivityModel()
                                {
                                    UserName = mobile,
                                    RequestMerchant = transactionType,
                                    DestinationNo = fundtransfer.da,
                                    Amount = amount,
                                    SMSStatus = "Failed",
                                    SMSSenderReply = message,
                                    ErrorMessage = failedmessage,
                                };
                            }

                        } //END IsValidMerchant


                        //} //END Destination mobile No check

                    }

                }
                catch (Exception ex)
                {
                    message = result + ex + "Error Message ";
                    //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                    statusCode = "400";
                    //failedmessage = message;
                    failedmessage = "Please try again.";
                }
                //}
                //else  //else for  if wallet balance less then bill amount then show error msg
                //{
                //    statusCodeBalance = "400";
                //    message = "Insufficient Balance";
                //    failedmessage = message;

                //}
            }
            try
            {

                //for  all EP  and GT transaction
                string compResultResp = "";

                if (statusCode == "200")
                {
                    try
                    {
                        //string amountInPaisa = ((double.Parse(amount)) * 100).ToString();

                        if (rltCheckPaymt == "000")//go if CP Response is 000
                        {
                            //int amountInPaisaInt = Convert.ToInt32(amountInPaisa);

                            long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                            exectransactionId = milliseconds.ToString();

                            string keyExecRlt = "";
                            string resultMessageResEP = "";
                            do
                            {
                                if ((companyCode == null) || (serviceCode == null) || (account == null) ||
                                (exectransactionDate == null) || (exectransactionId == null) ||
                                (refStan == null) || (amount == null) || (billNumber == null) ||
                                (userId == null) || (userPassword == null) || (salePointType == null))
                                {
                                    // throw ex
                                    statusCode = "400";
                                    message = "Parameters Missing/Invalid";
                                    failedmessage = message;
                                }
                                else
                                {
                                    if (companyCode == "598")
                                    {
                                        special1 = "";
                                    }
                                    else
                                    {
                                        special1 = special1.ToString();
                                    }

                                    // string URIEXECPayment = "https://test.paypoint.md:4445/PayPointWS/PayPointMSOperations.asmx/ExecutePayment";//for EP Link 

                                    //For excutepaypoint link in webconfig
                                    string URIEXECPayment = System.Web.Configuration.WebConfigurationManager.AppSettings["EPPaypointUrl"];

                                    string execPayParameters = "companyCode=" + companyCode + "&serviceCode=" + serviceCode +
                                            "&account=" + account + "&special1=" + special1 + "&special2=" + special2 +
                                            "&transactionDate=" + exectransactionDate + "&transactionId=" + exectransactionId +
                                            "&refStan=" + refStan + "&amount=" + Convert.ToInt32(float.Parse(amount) * 100).ToString() + "&billNumber=" + billNumber +
                                            "&userId=" + userId.Trim() + "&userPassword=" + userPassword.Trim() + "&salePointType=" + salePointType;

                                    //for executepayment request insert in database for wlink
                                    reqEPPaypointDishHomeInfo = new PaypointModel()
                                    {
                                        companyCodeReqEP = companyCode,
                                        serviceCodeReqEP = serviceCode,
                                        accountReqEP = account,
                                        special1ReqEP = special1,
                                        special2ReqEP = special2,

                                        transactionDateReqEP = exectransactionDate,
                                        transactionIdReqEP = exectransactionId,
                                        userIdReqEP = userId,
                                        userPasswordReqEP = userPassword,
                                        salePointTypeReqEP = salePointType,

                                        refStanReqEP = refStan,
                                        amountReqEP = amount,
                                        billNumberReqEP = billNumber,
                                        //retrievalReferenceReqEP = fundtransfer.tid,
                                        //retrievalReferenceReqEP = tid,
                                        retrievalReferenceReqEP = retrievalReference,
                                        remarkReqEP = "Execute Payment",
                                        UserName = mobile,
                                        ClientCode = ClientCode,
                                        paypointType = paypointType,

                                    };

                                    using (WebClient wcExecPay = new WebClient())
                                    {
                                        wcExecPay.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                                        string HtmlResultExecPay = wcExecPay.UploadString(URIEXECPayment, execPayParameters);

                                        XmlDocument xmlEDoc = new XmlDocument();
                                        xmlEDoc.LoadXml(HtmlResultExecPay);

                                        XmlNodeList nodeEPay = xmlEDoc.GetElementsByTagName("*");
                                        string resultEPay = nodeEPay[0].InnerText;
                                        string HtmlEPayResult = resultEPay;

                                        //for determining  checkpayment key and result
                                        var readerEPay = new StringReader(HtmlEPayResult);
                                        var xdocEPay = XDocument.Load(readerEPay);

                                        XDocument docEPay = XDocument.Parse(xdocEPay.ToString());

                                        var xElemEPay = XElement.Parse(xdocEPay.ToString());

                                        if (xElemEPay.Attribute("Result").Value == "000")
                                        {
                                            compResultResp = xElemEPay.Attribute("Result").Value;
                                            keyExecRlt = xElemEPay.Attribute("Key").Value;
                                        }
                                        else if ((xElemEPay.Attribute("Result").Value == "011") || (xElemEPay.Attribute("Result").Value == "012"))
                                        {
                                            compResultResp = xElemEPay.Attribute("Result").Value;
                                            keyExecRlt = xElemEPay.Attribute("Key").Value;
                                        }
                                        else
                                        {
                                            compResultResp = xElemEPay.Attribute("Result").Value;
                                            keyExecRlt = xElemEPay.Attribute("Key").Value;
                                        }
                                        resultMessageResEP = xElemEPay.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;



                                        //for Response Execute Payment
                                        resEPPaypointDishHomeInfo = new PaypointModel()
                                        {
                                            companyCodeResEP = companyCode,
                                            serviceCodeResEP = serviceCode,
                                            accountResEP = account,
                                            special1ResEP = special1,
                                            special2ResEP = special2,

                                            transactionDateResEP = transactionDate,
                                            transactionIdResEP = transactionId,
                                            userIdResEP = userId,

                                            userPasswordResEP = userPassword,
                                            salePointTypeResEP = salePointType,

                                            refStanResEP = refStan,
                                            amountResEP = amount,
                                            billNumberResEP = billNumber,
                                            //retrievalReferenceResEP = fundtransfer.tid,
                                            //retrievalReferenceResEP = tid,
                                            retrievalReferenceResEP = retrievalReference,
                                            responseCodeResEP = compResultResp,
                                            descriptionResEP = "Execute Payment" + keyExecRlt,
                                            UserName = mobile,
                                            ClientCode = ClientCode,
                                            paypointType = paypointType,
                                            resultMessageResEP = resultMessageResEP,
                                            customerNameResEP = customerName,
                                        };
                                    }
                                }

                            } while ((compResultResp == "011") || (compResultResp == "012"));

                            ///for  Inserting EP PayPoint Data for wlink

                            try
                            {
                                int resultsReqEP = PaypointUtils.RequestEPPaypointInfo(reqEPPaypointDishHomeInfo);
                                int resultsResEP = PaypointUtils.ResponseEPPaypointInfo(resEPPaypointDishHomeInfo);

                                if ((resultsReqEP > 0) && (resultsResEP > 0))
                                {
                                    message = result;
                                }
                                else
                                {
                                    message = result;
                                }

                            }
                            catch (Exception ex)
                            {
                                string ss = ex.Message;
                                message = result;
                            }

                            if (compResultResp == "000")
                            {
                                if ((refStan == null) || (userId == null) || (userPassword == null) || (salePointType == null))
                                {
                                    // throw ex
                                    statusCode = "400";
                                    message = "Parameters Missing/Invalid";
                                    failedmessage = message;
                                }
                                else if (compResultResp != "")
                                {
                                    string statusResGTP = "1";
                                    do
                                    {
                                        string key = "";
                                        string gtBillNumber = "";
                                        // string URIGetTran = "https://test.paypoint.md:4445/PayPointWS/PayPointMSOperations.asmx/GetTransaction";//for GT paypoint transactionlink

                                        //For get transactionpaypoint link in webconfig
                                        string URIGetTran = System.Web.Configuration.WebConfigurationManager.AppSettings["GTPPaypointUrl"];

                                        string GetTranParameters = "userLogin=" + userId + "&userPassword=" + userPassword + "&stan=" + "-1" + "&refStan=" + refStan + "&key=" + "" + "&billNumber=" + gtBillNumber;

                                        //for get transaction payment request insert in database
                                        reqGTPaypointDishHomeInfo = new PaypointModel()
                                        {
                                            companyCodeReqGTP = companyCode,
                                            serviceCodeReqGTP = serviceCode,
                                            accountReqGTP = account,
                                            special1ReqGTP = special1,
                                            special2ReqGTP = special2,

                                            transactionDateReqGTP = transactionDate,
                                            transactionIdReqGTP = transactionId,
                                            userIdReqGTP = userId,
                                            userPasswordReqGTP = userPassword,
                                            salePointTypeReqGTP = salePointType,

                                            refStanReqGTP = refStan,
                                            amountReqGTP = amount,
                                            billNumberReqGTP = gtBillNumber,
                                            // retrievalReferenceReqGTP = fundtransfer.tid,
                                            //retrievalReferenceReqGTP = tid,
                                            retrievalReferenceReqGTP = retrievalReference,
                                            remarkReqGTP = "Get Transaction Payment",
                                            UserName = mobile,
                                            ClientCode = ClientCode,
                                            paypointType = paypointType,
                                            //remarkReqGTP = "Get Transaction Payment"+keyExecRlt,

                                        };
                                        string getTranResultResp = "";
                                        string keyGetTrancRlt = "";
                                        string resultMessageResGTP = "";
                                        string billNumberResGTP = "";
                                        using (WebClient wcGetTran = new WebClient())
                                        {
                                            wcGetTran.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                                            string HtmlResultGetTran = wcGetTran.UploadString(URIGetTran, GetTranParameters);

                                            XmlDocument xmlEDoc = new XmlDocument();
                                            xmlEDoc.LoadXml(HtmlResultGetTran);

                                            XmlNodeList nodeEPay = xmlEDoc.GetElementsByTagName("*");
                                            string resultEPay = nodeEPay[0].InnerText;
                                            string HtmlEPayResult = resultEPay;

                                            //for determing excutepayment key and result
                                            var readerEPay = new StringReader(HtmlEPayResult);
                                            var xdocEPay = XDocument.Load(readerEPay);

                                            XDocument docEPay = XDocument.Parse(xdocEPay.ToString());
                                            var xElemGPay = XElement.Parse(docEPay.ToString());
                                            if (xElemGPay.Attribute("Result").Value == "000")
                                            {
                                                getTranResultResp = xElemGPay.Attribute("Result").Value;
                                                keyGetTrancRlt = xElemGPay.Attribute("Key").Value;
                                                resultMessageResGTP = xElemGPay.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;
                                                if (!(resultMessageResGTP == "No data"))
                                                {
                                                    billNumberResGTP = xElemGPay.Descendants().Elements("BillNumber").Where(x => x.Name == "BillNumber").SingleOrDefault().Value;
                                                    statusResGTP = xElemGPay.Descendants().Elements("Status").Where(x => x.Name == "Status").SingleOrDefault().Value;

                                                    //for get transaction payment status validation
                                                    if (!(statusResGTP == "1" || statusResGTP == "5" || statusResGTP == "11" || statusResGTP == "13"))
                                                    {
                                                        if (statusResGTP == "4" || statusResGTP == "6" || statusResGTP == "14" || statusResGTP == "16")
                                                        {
                                                            // mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                            statusCode = "400";
                                                            //message = result;
                                                            message = "Failed";
                                                            failedmessage = message;
                                                            resultMessageResGTP = "failed";
                                                        }

                                                    }
                                                    else
                                                    {
                                                        statusCode = "200";
                                                        resultMessageResGTP = "sucess";
                                                    }
                                                }
                                                else
                                                {
                                                    //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                    statusCode = "400";
                                                    //message = result;
                                                    message = "Failed";
                                                    failedmessage = message;
                                                    resultMessageResGTP = "failed";
                                                }
                                            }
                                            else
                                            {
                                                resultMessageResGTP = xElemGPay.Descendants().Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;

                                            }


                                            //end get transaction payment status validation

                                            ////for get transaction payment response insert in database
                                            resGTPaypointDishHomeInfo = new PaypointModel()
                                            {
                                                companyCodeResGTP = companyCode,
                                                serviceCodeResGTP = serviceCode,
                                                accountResGTP = account,
                                                special1ResGTP = special1,
                                                special2ResGTP = special2,

                                                transactionDateResGTP = transactionDate,
                                                transactionIdResGTP = transactionId,
                                                userIdResGTP = userId,
                                                userPasswordResGTP = userPassword,
                                                salePointTypeResGTP = salePointType,

                                                refStanResGTP = refStan,
                                                amountResGTP = amount, //amountpay,
                                                billNumberResGTP = billNumberResGTP,
                                                //retrievalReferenceResGTP = fundtransfer.tid,
                                                //retrievalReferenceResGTP = tid,
                                                retrievalReferenceResGTP = retrievalReference,
                                                responseCodeResGTP = getTranResultResp,
                                                descriptionResGTP = "Get Transaction Payment " + keyGetTrancRlt,
                                                UserName = mobile,
                                                ClientCode = ClientCode,
                                                paypointType = paypointType,
                                                resultMessageResGTP = resultMessageResGTP,
                                                customerNameResGTP = customerName,

                                            };
                                        }
                                    } while (statusResGTP == "10" || statusResGTP == "15" || statusResGTP == "20" || statusResGTP == "21" || statusResGTP == "99" || statusResGTP == "12" || statusResGTP == "2" || statusResGTP == "0");

                                    ///for  Inserting GT PayPoint Data

                                    try
                                    {

                                        int resultsReqGTP = PaypointUtils.RequestGTPaypointInfo(reqGTPaypointDishHomeInfo);
                                        int resultsResGTP = PaypointUtils.ResponseGTPaypointInfo(resGTPaypointDishHomeInfo);

                                        if ((resultsReqGTP > 0) && (resultsResGTP > 0))
                                        {
                                            message = result;
                                        }
                                        else
                                        {
                                            message = result;
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        string ss = ex.Message;
                                        message = result;
                                    }

                                }

                            }

                            else
                            {
                                //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                statusCode = "400";
                                //message = result;
                                message = resultMessageResEP;
                                failedmessage = message;
                            }

                        }
                        else //ELSE FOR (rltCheckPaymt == "000")I.E Error  in result of CP
                        {
                            //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                            statusCode = "400";
                            // message = result;
                            message = resultMessageResCP;

                            failedmessage = message;

                            ////start:Com focus one log///
                            //MNFundTransfer mnft1 = new MNFundTransfer(tid, fundtransfer.sc, fundtransfer.da,
                            //    fundtransfer.sa, fundtransfer.amount, fundtransfer.mobile, fundtransfer.note, fundtransfer.pin,
                            //    fundtransfer.sourcechannel);
                            //var comfocuslog1 = new MNComAndFocusOneLog(mnft1, DateTime.Now);
                            //var mncomfocuslog1 = new MNComAndFocusOneLogsController();
                            //result = mncomfocuslog1.InsertIntoComFocusOne(comfocuslog1);
                            ////end:Com focus one log//

                            //if (mnft1.valid())
                            //{
                            //    var transaction1 = new MNTransactionMaster(mnft1);
                            //    var mntransaction1 = new MNTransactionsController();
                            //    validTransactionData = mntransaction1.Validate(transaction1, mnft1.pin);
                            //    result = validTransactionData.Response;

                            //}
                        }
                        //end excute dekhi get ko sabai comment gareko
                    }
                    catch (Exception ex)
                    {
                        message = result + ex + "Error Message ";
                        //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                        statusCode = "400";
                        failedmessage = "Please try again.";
                    }
                }

                //REverse Transaction 
                if ((statusCode != "111") && (statusCode != "114") && (statusCode != "115") && (statusCode != "116") && (statusCode != "119") &&
                    (statusCode != "121") && (statusCode != "163") && (statusCode != "180") && (statusCode != "181") && (statusCode != "182") &&
                    (statusCode != "183") && (statusCode != "184") && (statusCode != "185") && (statusCode != "186") && (statusCode != "187") &&
                    (statusCode != "188") && (statusCode != "189") && (statusCode != "190") && (statusCode != "800") && (statusCode != "902") &&
                    (statusCode != "904") && (statusCode != "906") && (statusCode != "907") && (statusCode != "909") && (statusCode != "911") &&
                    (statusCode != "913") && (statusCode != "90") && (statusCode != "91") && (statusCode != "94") && (statusCode != "95") &&
                    (statusCode != "98") && (statusCode != "99") && (statusCodeBalance != "400") && (compResultResp != "000") && (statusCodeBalance != "400")
                    && (statusCode != "200") && (statusCode != "508")
                    )
                //((statusCodeBalance != "400") && (compResultResp != "000")) || ((statusCodeBalance != "400") && (statusCode != "200")) ||
                //if (statusCode != "116")
                {
                    //TraceIdGenerator traceRevid = new TraceIdGenerator();
                    //tid = traceRevid.GenerateUniqueTraceID();
                    tid = retrievalReference;
                    if (sc == "00")
                    {
                        transactionType = "PayPoint Txfr to W2W";
                    }
                    else
                    {
                        sc = "01";
                        transactionType = "PayPoint Txfr to W2B"; //B2W
                    }
                    FundTransfer fundtransferRev = new FundTransfer
                    {
                        tid = tid,
                        sc = sc,
                        mobile = da,//mobile
                        da = mobile,//da
                        amount = amount,
                        pin = pin,
                        note = "reverse " + note,
                        sourcechannel = src
                    };
                    ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, fundtransfer);
                    //CustActivityModel custsmsInfo = new CustActivityModel();


                    // MNTransactionMaster validTransactionData = new MNTransactionMaster();


                    //if (TokenGenerator.TokenChecker(sessionID, mobile, src) == false)
                    //{
                    //    // throw ex
                    //    statusCode = "400";
                    //    message = "Session expired. Please login again";
                    //    failedmessage = message;
                    //}
                    //else
                    //{
                    if ((tid == null) || (sc == null) || (mobile == null) || (da == null) || (amount == null) || (pin == null) ||
                    (src == null) || (double.Parse(amount) <= 0) || (vid == null))
                    {
                        // throw ex
                        statusCode = "400";
                        message = "Parameters Missing/Invalid";
                        failedmessage = message;
                    }
                    if ((companyCode == null) || (companyCode == "") || (serviceCode == null) || (serviceCode == "") ||
                        (account == null) || (special1 == null) || (special2 == null) || (transactionDate == null) || (transactionId == null) || (transactionId == "") ||
                        (userId == null) || (userPassword == null) || (salePointType == null) || (userId == "") || (userPassword == "") || (salePointType == ""))
                    {
                        // throw ex
                        statusCode = "400";
                        message = "Parameters Missing/Invalid PayPoint";
                        failedmessage = message;
                    }
                    else
                    {
                        //if (sc == "00")
                        //{
                        //    transactionType = "PayPoint Txfr to W2W";
                        //}
                        //else 
                        //{
                        //    sc = "01";
                        //    transactionType = "PayPoint Txfr to W2B"; //B2W
                        //}

                        if (!(UserNameCheck.IsValidUser(mobile)))
                        {
                            // throw ex
                            statusCode = "400";
                            //message = "Transaction restricted to User";
                            message = "Transaction only for User";
                            failedmessage = message;
                        }
                        if (UserNameCheck.IsValidMerchant(da))
                        {

                            TransLimitCheck transLimitCheck = new TransLimitCheck();
                            string resultTranLimit = transLimitCheck.LimitCheck(mobile, da, amount, sc, pin, src);

                            var jsonDataResult = JObject.Parse(resultTranLimit);
                            statusCode = jsonDataResult["StatusCode"].ToString();
                            string statusMsg = jsonDataResult["StatusMessage"].ToString();
                            message = jsonDataResult["StatusMessage"].ToString();
                            failedmessage = message;

                            if ((statusCode == "200") && (message == "Success"))
                            {
                                //start: checking trace id
                                do
                                {
                                    TraceIdGenerator traceid = new TraceIdGenerator();
                                    tid = traceid.GenerateUniqueTraceID();
                                    fundtransfer.tid = tid;

                                    bool traceIdCheck = false;
                                    traceIdCheck = TraceIdCheck.IsValidTraceId(fundtransfer.tid);
                                    if (traceIdCheck == true)
                                    {
                                        result = "Trace ID Repeated";
                                    }
                                    else
                                    {
                                        result = "false";
                                    }

                                } while (result == "Trace ID Repeated");
                                //End: TraceId

                                //start:Com focus one log///
                                MNFundTransfer mnft = new MNFundTransfer(tid, fundtransferRev.sc, fundtransferRev.mobile,
                                       fundtransferRev.sa, fundtransferRev.amount, fundtransferRev.da, fundtransferRev.note, fundtransferRev.pin,
                                       fundtransferRev.sourcechannel, "T", "PayPoint");
                                var comfocuslog = new MNComAndFocusOneLog(mnft, DateTime.Now);
                                var mncomfocuslog = new MNComAndFocusOneLogsController();
                                //mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                result = mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                //end:Com focus one log//

                                if (result == "Success")
                                {
                                    //NOTE:- may be need to validate before insert into reply typpe
                                    //start:insert into reply type as HTTP//
                                    var replyType = new MNReplyType(tid, "HTTP");
                                    var mnreplyType = new MNReplyTypesController();
                                    mnreplyType.InsertIntoReplyType(replyType);
                                    //end:insert into reply type as HTTP//

                                    MNMerchantsController getMerchantDetails = new MNMerchantsController();

                                    string GetMerchantMobile = getMerchantDetails.PassVidToGetMerchantDetail(vid);
                                    if (GetMerchantMobile != "" || GetMerchantMobile != null)
                                    {
                                        fundtransfer.da = GetMerchantMobile; //Set Destination Merchant Mobile number
                                        GetMerchantName = getMerchantDetails.PassVIdToGetMerchantName(vid);
                                    }
                                    else
                                    {
                                        statusCode = "400";
                                        message = "Destination Merchant Doesnot Exists";
                                        mnft.Response = "Destination Merchant Doesnot Exists";
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, mnft.Response);
                                        result = mnft.Response;
                                        failedmessage = message;
                                    }


                                    //start:insert into transaction master//
                                    if (mnft.valid())
                                    {
                                        var transaction = new MNTransactionMaster(mnft);
                                        var mntransaction = new MNTransactionsController();
                                        //validTransactionData = mntransaction.Validate(transaction, mnft.pin);
                                        validTransactionData = mntransaction.Validatepaypoint(transaction, mnft.pin);
                                        result = validTransactionData.Response;

                                        /*** ***/
                                        ErrorMessage em = new ErrorMessage();

                                        if (validTransactionData.Response == "Error")
                                        {
                                            mnft.Response = "error";
                                            mnft.ResponseStatus(HttpStatusCode.InternalServerError,
                                                "Internal server error - try again later, or contact support");
                                            result = mnft.Response;
                                            statusCode = "500";
                                            message = "Internal server error - try again later, or contact support";
                                            failedmessage = message;
                                        }
                                        else
                                        {
                                            if ((result == "Trace ID Repeated") || (result == "Limit Exceed")
                                                || (result == "Invalid Source User") || (result == "Invalid Destination User")
                                                || (result == "Invalid Product Request") || (result == "Please try again") || (result == ""))
                                            {
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                statusCode = "400";
                                                message = result;
                                                failedmessage = message;
                                            }
                                            if (result == "Invalid PIN")
                                            {
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                statusCode = "400";
                                                message = result;
                                                failedmessage = message;
                                            }
                                            if (result == "111")
                                            {
                                                statusCode = result;
                                                message = em.Error_111;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "114")
                                            {
                                                statusCode = result;
                                                message = em.Error_114;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "115")
                                            {
                                                statusCode = result;
                                                message = em.Error_115;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "116")
                                            {
                                                statusCode = result;
                                                message = em.Error_116;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "119")
                                            {
                                                statusCode = result;
                                                message = em.Error_119;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "121")
                                            {
                                                statusCode = result;
                                                message = em.Error_121;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "163")
                                            {
                                                statusCode = result;
                                                message = em.Error_163;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "180")
                                            {
                                                statusCode = result;
                                                message = em.Error_180;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "181")
                                            {
                                                statusCode = result;
                                                message = em.Error_181;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "182")
                                            {
                                                statusCode = result;
                                                message = em.Error_182;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "183")
                                            {
                                                statusCode = result;
                                                message = em.Error_183;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "184")
                                            {
                                                statusCode = result;
                                                message = em.Error_184;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "185")
                                            {
                                                statusCode = result;
                                                message = em.Error_185;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "186")
                                            {
                                                statusCode = result;
                                                message = em.Error_186;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "187")
                                            {
                                                statusCode = result;
                                                message = em.Error_187;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "188")
                                            {
                                                statusCode = result;
                                                message = em.Error_188;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "189")
                                            {
                                                statusCode = result;
                                                message = em.Error_189;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "190")
                                            {
                                                statusCode = result;
                                                message = em.Error_190;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "800")
                                            {
                                                statusCode = result;
                                                message = em.Error_800;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "902")
                                            {
                                                statusCode = result;
                                                message = em.Error_902;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "904")
                                            {
                                                statusCode = result;
                                                message = em.Error_904;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "906")
                                            {
                                                statusCode = result;
                                                message = em.Error_906;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "907")
                                            {
                                                statusCode = result;
                                                message = em.Error_907;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "909")
                                            {
                                                statusCode = result;
                                                message = em.Error_909;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "911")
                                            {
                                                statusCode = result;
                                                message = em.Error_911;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "913")
                                            {
                                                statusCode = result;
                                                message = em.Error_913;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "90")
                                            {
                                                statusCode = result;
                                                message = em.Error_90;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "91")
                                            {
                                                statusCode = result;
                                                message = em.Error_91;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "92")
                                            {
                                                statusCode = result;
                                                message = em.Error_92;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "94")
                                            {
                                                statusCode = result;
                                                message = em.Error_94;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "95")
                                            {
                                                statusCode = result;
                                                message = em.Error_95;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "98")
                                            {
                                                statusCode = result;
                                                message = em.Error_98;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "99")
                                            {
                                                statusCode = result;
                                                message = em.Error_99;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            else if (validTransactionData.ResponseCode == "OK")
                                            {
                                                statusCode = "200";
                                                message = result;
                                                mnft.ResponseStatus(HttpStatusCode.OK, message);

                                            } //END ValidTransactionData.ResponseCode


                                        } //END validTransactionData.Response WITHOUT MNDB ERROR
                                        /*** ***/

                                        OutgoingWebResponseContext response1 = WebOperationContext.Current.OutgoingResponse;
                                        if (response1.StatusCode == HttpStatusCode.OK)
                                        {
                                            string messagereply = "";
                                            try
                                            {
                                                messagereply = "Dear " + CustCheckUtils.GetName(mobile) + "," + "\n";

                                                messagereply += " You have successfully reverse  NPR " + validTransactionData.Amount
                                                                    + " to " +
                                                                    //GetMerchantName 
                                                                    "Utility payment for Dish Home Direct." + " on date " +
                                                                    (validTransactionData.CreatedDate).ToString("dd/MM/yyyy")
                                                                + "." + "\n";
                                                messagereply += "Thank you. MNepal";

                                                var client = new WebClient();

                                                //SENDER
                                                if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                                                {
                                                    //FOR NCELL
                                                    var content = client.DownloadString(
                                                        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                                        + "977" + mobile + "&Text=" + messagereply + "");
                                                }
                                                else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                                            || (mobile.Substring(0, 3) == "986"))
                                                {
                                                    //FOR NTC
                                                    var content = client.DownloadString(
                                                        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                                        + "977" + mobile + "&Text=" + messagereply + "");
                                                }

                                                statusCode = "200";
                                                var v = new
                                                {
                                                    StatusCode = Convert.ToInt32(statusCode),
                                                    StatusMessage = result
                                                };
                                                result = JsonConvert.SerializeObject(v);

                                            }
                                            catch (Exception ex)
                                            {
                                                // throw ex
                                                statusCode = "400";
                                                message = ex.Message;
                                            }


                                            custsmsInfo = new CustActivityModel()
                                            {
                                                UserName = fundtransfer.mobile,
                                                RequestMerchant = transactionType,
                                                DestinationNo = fundtransfer.da,
                                                Amount = validTransactionData.Amount.ToString(),
                                                SMSStatus = "Success",
                                                SMSSenderReply = messagereply,
                                                ErrorMessage = "",
                                            };


                                        }
                                        else if ((response1.StatusCode == HttpStatusCode.BadRequest) || (response1.StatusCode == HttpStatusCode.InternalServerError) || (statusCode != "200"))
                                        {
                                            custsmsInfo = new CustActivityModel()
                                            {
                                                UserName = mobile,
                                                RequestMerchant = transactionType,
                                                DestinationNo = fundtransfer.da,
                                                Amount = validTransactionData.Amount.ToString(),
                                                SMSStatus = "Failed",
                                                SMSSenderReply = message,
                                                ErrorMessage = failedmessage,
                                            };

                                        }
                                        //end:insert into transaction master//

                                    } //END:insert into transaction master//
                                    else
                                    {
                                        mnft.Response = "error";
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, "parameters missing/invalid"); //200 - OK
                                        result = mnft.Response;
                                        statusCode = "400";
                                        message = "parameters missing/invalid";
                                        failedmessage = message;

                                        custsmsInfo = new CustActivityModel()
                                        {
                                            UserName = mobile,
                                            RequestMerchant = transactionType,
                                            DestinationNo = fundtransfer.da,
                                            Amount = amount,
                                            SMSStatus = "Failed",
                                            SMSSenderReply = message,
                                            ErrorMessage = failedmessage,
                                        };
                                    }

                                } //END MNComAndFocusOneLogsController
                                else
                                {
                                    statusCode = "400";
                                    mnft.Response = "Data Insertion Failed in Check DB Connection: TraceID limit might have exceeded";
                                    mnft.ResponseStatus(HttpStatusCode.InternalServerError, mnft.Response);
                                    result = mnft.Response;
                                    failedmessage = result;
                                }

                            } //END TRansLimit Check StatusCode N Message
                            else
                            {
                                custsmsInfo = new CustActivityModel()
                                {
                                    UserName = mobile,
                                    RequestMerchant = transactionType,
                                    DestinationNo = fundtransfer.da,
                                    Amount = amount,
                                    SMSStatus = "Failed",
                                    SMSSenderReply = message,
                                    ErrorMessage = failedmessage,
                                };
                            }

                        } //END IsValidMerchant

                        //} //END Destination mobile No check

                    }
                }
                //} //tokengenerator ko closing bracket

                //for sending sms  if success 
                if (compResultResp == "000")
                {
                    OutgoingWebResponseContext response2 = WebOperationContext.Current.OutgoingResponse;
                    if (response2.StatusCode == HttpStatusCode.OK)
                    {
                        string messagereply = "";
                        try
                        {
                            messagereply = "Dear " + CustCheckUtils.GetName(mobile) + "," + "\n";

                            messagereply += " You have successfully paid NPR " + validTransactionData.Amount
                                            + " to " + " account name: " + account + ". " +
                                            //GetMerchantName 
                                            "Utility payment for Dish Home Direct (Pinless)." + " on date " +
                                                (validTransactionData.CreatedDate).ToString("dd/MM/yyyy")
                                            + "." + "\n";
                            messagereply += "Thank you. MNepal";

                            var client = new WebClient();

                            //SENDER
                            if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                            {
                                //FOR NCELL
                                var content = client.DownloadString(
                                    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                    + "977" + mobile + "&Text=" + messagereply + "");
                            }
                            else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                        || (mobile.Substring(0, 3) == "986"))
                            {
                                //FOR NTC
                                var content = client.DownloadString(
                                    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                    + "977" + mobile + "&Text=" + messagereply + "");
                            }

                            statusCode = "200";
                            var v = new
                            {
                                StatusCode = Convert.ToInt32(statusCode),
                                StatusMessage = result
                            };
                            result = JsonConvert.SerializeObject(v);

                        }
                        catch (Exception ex)
                        {
                            // throw ex
                            statusCode = "400";
                            message = ex.Message;
                        }


                        custsmsInfo = new CustActivityModel()
                        {
                            UserName = fundtransfer.mobile,
                            RequestMerchant = transactionType,
                            DestinationNo = fundtransfer.da,
                            Amount = validTransactionData.Amount.ToString(),
                            SMSStatus = "Success",
                            SMSSenderReply = messagereply,
                            ErrorMessage = "",
                        };


                    }
                    else if ((response2.StatusCode == HttpStatusCode.BadRequest) || (response2.StatusCode == HttpStatusCode.InternalServerError) || (statusCode != "200"))
                    {
                        custsmsInfo = new CustActivityModel()
                        {
                            UserName = mobile,
                            RequestMerchant = transactionType,
                            DestinationNo = fundtransfer.da,
                            Amount = validTransactionData.Amount.ToString(),
                            SMSStatus = "Failed",
                            SMSSenderReply = message,
                            ErrorMessage = failedmessage,
                        };

                    }
                }

            }
            catch (Exception ex)
            {
                // throw ex
                statusCode = "400";
                message = ex.Message;
            }
            if (statusCodeBalance == "400")
            {
                statusCode = "400";
            }
            if (statusCode == "")
            {
                result = result.ToString();
            }
            //else if (statusCode != "200")
            else if ((statusCode != "200") || (statusCodeBalance == "400"))
            {
                if (message == "")
                {
                    if (string.IsNullOrEmpty(result))
                    {
                        result = failedmessage; // "Could not process your request,Please Try again.";
                    }
                    message = result;
                }
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = failedmessage
                };
                result = JsonConvert.SerializeObject(v);
            }
            return result;
        }
        #endregion

        #region"Check SmartCell EPIN Payment"
        [OperationContract]
        [WebInvoke(Method = "POST",
                  ResponseFormat = WebMessageFormat.Json)]
        public string SmartCellEPINcheckpayment(Stream input)
        {
            string PaypointPwd = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointPwd"];
            string PaypointUserID = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointUserID"];
            System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate { return true; }; //to prevent from SSL error
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            string tid = qs["tid"];
            string vid = qs["vid"];
            string sc = "00";
            string mobile = qs["mobile"];
            string da = System.Web.Configuration.WebConfigurationManager.AppSettings["DestinationNoForTestServer"];
            string src = qs["src"];
            string result = "";
            string sessionID = qs["tokenID"];
            string companyCode = qs["companyCode"];
            string serviceCode = qs["serviceCode"];
            string account = qs["account"];
            string special1 = qs["special1"];
            string special2 = "";
            string transactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            long millisecondstrandId = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string transactionId = millisecondstrandId.ToString();
            string userId = PaypointUserID;
            string userPassword = PaypointPwd.Trim();
            string salePointType = "6";
            string ClientCode = qs["ClientCode"];
            string paypointType = qs["paypointType"];
            string transactionType = string.Empty;
            string resultMessageResCP = "";
            string note = "Utility payment for SmartCell EPIN of service code " + serviceCode + ". Mobile Number=" + qs["account"];

            PaypointModel reqCPPaypointNTCInfo = new PaypointModel();//to store data request of CP which is also commmon in nea
            PaypointModel resCPPaypointNTCInfo = new PaypointModel();//to store data response of CP which is also commmon in nea

            //PaypointModel resPaypointPaymentInfo = new PaypointModel();
            PaypointModel resPaypointNTCPaymentInfo = new PaypointModel();//to store data of Response of CP only

            string totalAmount = string.Empty;
            string totalCount = string.Empty;
            string totalBAmount = string.Empty;
            string totalBCount = string.Empty;
            string balance = string.Empty;
            string message = string.Empty;
            string statusCode = string.Empty;
            string failedmessage = string.Empty;
            string GetMerchantName = string.Empty;
            string retrievalRef = string.Empty;
            string refStanCK = string.Empty;

            string customerNo = string.Empty;
            TraceIdGenerator traceid = new TraceIdGenerator();
            tid = traceid.GenerateUniqueTraceID();


            if (serviceCode == "1")
            {
                special1 = "50";
            }
            if (serviceCode == "2")
            {
                special1 = "100";
            }
            if (serviceCode == "3")
            {
                special1 = "200";
            }
            if (serviceCode == "10")
            {
                special1 = "10";
            }
            if (serviceCode == "4")
            {
                special1 = "500";
            }
           
            //for CP transaction for nepal water
            try
            {
                string URI = System.Web.Configuration.WebConfigurationManager.AppSettings["CPPaypointUrl"];

                string myParameters = "companyCode=" + companyCode + "&serviceCode=" + serviceCode +
                    "&account=" + account + "&special1=" + (Convert.ToInt32(special1) * 100).ToString() + "&special2=" + special2 +
                    "&transactionDate=" + transactionDate + "&transactionId=" + transactionId +
                    "&userId=" + userId.Trim() + "&userPassword=" + userPassword.Trim() + "&salePointType=" + salePointType;

                //for checkpayment request insert in database
                reqCPPaypointNTCInfo = new PaypointModel()
                {
                    companyCodeReqCP = companyCode,
                    serviceCodeReqCP = serviceCode,
                    accountReqCP = account,
                    special1ReqCP = special1,
                    special2ReqCP = special2,

                    transactionDateReqCP = transactionDate,
                    transactionIdReqCP = transactionId,
                    userIdReqCP = userId,
                    userPasswordReqCP = userPassword,
                    salePointTypeReqCP = salePointType,

                    refStanReqCP = "",
                    amountReqCP = "",//amountInPaisa
                    billNumberReqCP = "",
                    //retrievalReferenceReqCP = fundtransfer.tid,
                    retrievalReferenceReqCP = tid,
                    remarkReqCP = "Check Payment",
                    UserName = mobile,
                    ClientCode = ClientCode,
                    paypointType = paypointType,

                };

                string billNumber = "";
                string amountpay = "";
                string refStan = "";
                string exectransactionId = ""; //Unique
                string exectransactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"); //Current DateTime
                string rltCheckPaymt = "";
                string customerName = "";
                string mask = "";

                using (WebClient wc = new WebClient())
                {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    var HtmlResult = wc.UploadString(URI, myParameters);// response from checkpayment

                    XmlDocument xmlDoc = new XmlDocument();

                    xmlDoc.LoadXml(HtmlResult);

                    XmlNodeList test = xmlDoc.GetElementsByTagName("*");
                    string results = test[0].InnerText;
                    string HtmlResult1 = results;

                    //for getting key value from check payment
                    var reader = new StringReader(HtmlResult1);
                    var xdoc = XDocument.Load(reader);

                    XDocument docParse = XDocument.Parse(xdoc.ToString());
                    IEnumerable<XElement> responses = docParse.Descendants();

                    var xElem = XElement.Parse(docParse.ToString());

                    rltCheckPaymt = xElem.Attribute("Result").Value;
                    string keyrlt = "";// response.Attribute("Key").Value;


                    message = resultMessageResCP;
                    if (xElem.Attribute("Result").Value == "000")
                    {
                        keyrlt = xElem.Attribute("Key").Value;
                        billNumber = xElem.Descendants().Elements("BillNumber").Where(x => x.Name == "BillNumber").SingleOrDefault().Value;
                        amountpay = xElem.Descendants().Elements("Amount").Where(x => x.Name == "Amount").SingleOrDefault().Value;
                        refStan = xElem.Descendants().Elements("RefStan").Where(x => x.Name == "RefStan").SingleOrDefault().Value;
                        exectransactionDate = xElem.Descendants().Elements("DueDate").Where(x => x.Name == "DueDate").SingleOrDefault().Value;
                        customerName = xElem.Descendants().Elements("ReserveInfo").Where(x => x.Name == "ReserveInfo").SingleOrDefault().Value;
                        mask = xElem.Descendants().Elements("mask").Where(x => x.Name == "mask").SingleOrDefault().Value;


                        resPaypointNTCPaymentInfo = new PaypointModel()
                        {
                            billNumber = billNumber,
                            refStan = refStan,
                            amount = amountpay,
                            transactionDate = exectransactionDate,
                            customerName = account,
                            companyCode = companyCode,
                            UserName = mobile,
                            ClientCode = ClientCode,
                            serviceCode = serviceCode

                        };
                        //end list of package

                        int resultsPayments = PaypointUtils.PaypointUtilitySmartCellEPINInfo(resPaypointNTCPaymentInfo);


                    }
                    else
                    {
                        keyrlt = xElem.Attribute("Key").Value;
                        resultMessageResCP = xElem.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;
                    }
                    resultMessageResCP = xElem.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;

                    //for checkpayment response insert in database in nepal water
                    resCPPaypointNTCInfo = new PaypointModel()
                    {
                        companyCodeResCP = companyCode,
                        serviceCodeResCP = serviceCode,
                        accountResCP = account,
                        special1ResCP = special1,
                        special2ResCP = special2,

                        transactionDateResCP = transactionDate,
                        transactionIdResCP = transactionId,
                        userIdResCP = userId,
                        userPasswordResCP = userPassword,
                        salePointTypeResCP = salePointType,

                        refStanResCP = refStan,
                        amountResCP = amountpay,
                        billNumberResCP = billNumber,
                        //retrievalReferenceResCP = fundtransfer.tid,
                        retrievalReferenceResCP = tid,
                        responseCodeResCP = rltCheckPaymt,
                        descriptionResCP = "Check Payment " + keyrlt,
                        customerNameCP = customerName,
                        UserName = mobile,
                        ClientCode = ClientCode,
                        paypointType = paypointType,
                        resultMessageResCP = resultMessageResCP,

                    };
                }


                if (!(rltCheckPaymt == "000"))//show error when CP response is not 000 
                {
                    statusCode = "400";
                    // message = result;
                    message = resultMessageResCP;
                    failedmessage = message;

                }
                else
                {
                    statusCode = "200";
                    message = resultMessageResCP;
                    failedmessage = message;
                }
            }
            catch (Exception ex)
            {
                message = result + ex + "Error Message ";
                //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                statusCode = "400";
                // failedmessage = message;
                failedmessage = resultMessageResCP;

            }

            ///for  inserting CP PayPoint Data of Nepal Water which is simlar to nea 

            try
            {
                int resultsReqCP = PaypointUtils.RequestCPPaypointInfo(reqCPPaypointNTCInfo);
                int resultsResCP = PaypointUtils.ResponseCPPaypointInfo(resCPPaypointNTCInfo);


                if ((resultsReqCP > 0) && (resultsResCP > 0))

                {
                    message = result;
                }
                else
                {
                    message = result;
                }

            }
            catch (Exception ex)
            {
                string ss = ex.Message;
                message = result;
            }

            if (statusCode == "")
            {
                result = result.ToString();
            }
            else if (statusCode == "200")
            {
                if (message == "")
                {
                    if (string.IsNullOrEmpty(result))
                    {
                        result = failedmessage; // "Could not process your request,Please Try again.";
                    }
                    message = result;
                }
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = failedmessage,
                    retrievalRef = resCPPaypointNTCInfo.retrievalReferenceResCP,
                    refStanCK = resCPPaypointNTCInfo.refStanResCP
                };
                result = JsonConvert.SerializeObject(v);
            }
            else if (statusCode != "200")
            {
                if (message == "")
                {
                    if (string.IsNullOrEmpty(result))
                    {
                        result = failedmessage; // "Could not process your request,Please Try again.";
                    }
                    message = result;
                }
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = failedmessage,
                    retrievalRef = "",
                    refStanCK = ""
                };
                result = JsonConvert.SerializeObject(v);
            }
            return result;

        }
        #endregion

        #region"SmartCell EPIN ExecutePayment Paypoint"
        [OperationContract]
        [WebInvoke(Method = "POST",
                  ResponseFormat = WebMessageFormat.Json)]
        public string SmartCellEPINExecutePayment(Stream input)
        {
            string PaypointPwd = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointPwd"];
            string PaypointUserID = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointUserID"];

            System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);
            MNTransactionMaster validTransactionData = new MNTransactionMaster();
            CustActivityModel custsmsInfo = new CustActivityModel();

            string tid = qs["tid"];
            string vid = qs["vid"]; //MerchantID
            string sc = qs["sc"];
            string mobile = qs["mobile"];
            string da = System.Web.Configuration.WebConfigurationManager.AppSettings["DestinationNoForTestServer"];


            string amount = qs["amount"];//amount paid by customer 
            string pin = qs["pin"];
            string note = "utility payment for SmartCell EPIN. Customer Name=" + qs["account"];//+ ". " + qs["note"];
            string src = qs["src"];
            string result = "";
            string sessionID = qs["tokenID"];
            string resultMessageResCP = "";
            string companyCode = qs["companyCode"];
            string serviceCode = qs["serviceCode"];
            string account = qs["account"];
            string special1 = qs["special1"]; //
            string special2 = ""; //
            string transactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");//"2019-11-22T11:11:02";
            long millisecondstrandId = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string transactionId = millisecondstrandId.ToString(); //"120163339819";
            string userId = PaypointUserID;
            string userPassword = PaypointPwd.Trim();
            string salePointType = "6";
            string transactionType = string.Empty;
            string amountpay = qs["amountpay"];//amount need to pay i.e amount in bill
            string exectransactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            string exectransactionId = millisecondstrandId.ToString();
            string refStan = qs["refStan"];
            string billNumber = qs["billNumber"];
            string rltCheckPaymt = qs["rltCheckPaymt"];
            string ClientCode = qs["ClientCode"];
            string paypointType = qs["paypointType"];
            //string paypointType = "Nepal Water";
            //string paypointType = "3";

            string customerName = qs["customerName"];
            string walletBalance = qs["walletBalance"];//in Rs.
            string retrievalReference = qs["retrievalReference"];
            int walletBalancePaisaInt = 0;
            walletBalancePaisaInt = Convert.ToInt32((Convert.ToDouble(walletBalance)) * 100);
            int amountpayInt = Convert.ToInt32(float.Parse(amount)) * 100;

            PaypointModel reqEPPaypointDishHomeInfo = new PaypointModel();
            PaypointModel resEPPaypointDishHomeInfo = new PaypointModel();

            PaypointModel reqGTPaypointDishHomeInfo = new PaypointModel();
            PaypointModel resGTPaypointDishHomeInfo = new PaypointModel();

            string totalAmount = string.Empty;
            string totalCount = string.Empty;
            string totalBAmount = string.Empty;
            string totalBCount = string.Empty;
            string balance = string.Empty;
            string message = string.Empty;
            string statusCode = string.Empty;
            string failedmessage = string.Empty;
            string GetMerchantName = string.Empty;
            string statusCodeBalance = string.Empty;

            string customerNo = string.Empty;

            // TraceIdGenerator traceid = new TraceIdGenerator();
            //tid = traceid.GenerateUniqueTraceID();
            tid = retrievalReference;

            FundTransfer fundtransfer = new FundTransfer
            {
                tid = tid,
                sc = sc,
                mobile = mobile,
                da = da,
                amount = amount,
                pin = pin,
                note = note,
                sourcechannel = src
            };
            if (sc == "00")
            {
                if (walletBalancePaisaInt >= amountpayInt)// if wallet balance less then bill amount then show error msg
                {
                    //First transaction MNRequest N Response
                    try
                    {

                        //FundTransfer fundtransfer = new FundTransfer
                        //{
                        //    tid = tid,
                        //    sc = sc,
                        //    mobile = mobile,
                        //    da = da,
                        //    amount = amount,
                        //    pin = pin,
                        //    note = note,
                        //    sourcechannel = src
                        //};

                        ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, fundtransfer);
                        //CustActivityModel custsmsInfo = new CustActivityModel();

                        //MNTransactionMaster validTransactionData = new MNTransactionMaster();


                        //if (TokenGenerator.TokenChecker(sessionID, mobile, src) == false)
                        //{
                        //    // throw ex
                        //    statusCode = "400";
                        //    message = "Session expired. Please login again";
                        //    failedmessage = message;
                        //}
                        //else
                        //{
                        if ((tid == null) || (sc == null) || (mobile == null) || (da == null) || (amount == null) || (pin == null) ||
                        (src == null) || (double.Parse(amount) <= 0) || (vid == null))
                        {
                            // throw ex
                            statusCode = "400";
                            message = "Parameters Missing/Invalid";
                            failedmessage = message;
                        }
                        if ((companyCode == null) || (companyCode == "") || (serviceCode == null) || (serviceCode == "") ||
                            (account == null) || (special1 == null) || (special2 == null) || (transactionDate == null) || (transactionId == null) || (transactionId == "") ||
                            (userId == null) || (userPassword == null) || (salePointType == null) || (userId == "") || (userPassword == "") || (salePointType == ""))
                        {
                            // throw ex
                            statusCode = "400";
                            message = "Parameters Missing/Invalid PayPoint";
                            failedmessage = message;
                        }
                        else
                        {
                            if (sc == "00")
                            {
                                transactionType = "PayPoint Txfr to W2W";
                            }
                            else if (sc == "10")
                            {
                                transactionType = "PayPoint Txfr to B2W"; //B2W
                            }

                            if (!(UserNameCheck.IsValidUser(mobile)))
                            {
                                // throw ex
                                statusCode = "400";
                                //message = "Transaction restricted to User";
                                message = "Transaction only for User";
                                failedmessage = message;
                            }
                            if (UserNameCheck.IsValidMerchant(da))
                            {

                                TransLimitCheck transLimitCheck = new TransLimitCheck();
                                string resultTranLimit = transLimitCheck.LimitCheck(mobile, da, amount, sc, pin, src);

                                var jsonDataResult = JObject.Parse(resultTranLimit);
                                statusCode = jsonDataResult["StatusCode"].ToString();
                                string statusMsg = jsonDataResult["StatusMessage"].ToString();
                                message = jsonDataResult["StatusMessage"].ToString();
                                failedmessage = message;

                                if ((statusCode == "200") && (message == "Success"))
                                {
                                    //start: checking trace id
                                    do
                                    {
                                        //TraceIdGenerator traceid = new TraceIdGenerator();
                                        // tid = traceid.GenerateUniqueTraceID();

                                        fundtransfer.tid = tid;
                                        bool traceIdCheck = false;
                                        traceIdCheck = TraceIdCheck.IsValidTraceId(fundtransfer.tid);
                                        if (traceIdCheck == true)
                                        {
                                            result = "Trace ID Repeated";
                                        }
                                        else
                                        {
                                            result = "false";
                                        }

                                    } while (result == "Trace ID Repeated");
                                    //End: TraceId

                                    //start:Com focus one log///
                                    MNFundTransfer mnft = new MNFundTransfer(tid, fundtransfer.sc, fundtransfer.mobile,
                                        fundtransfer.sa, fundtransfer.amount, fundtransfer.da, fundtransfer.note, fundtransfer.pin,
                                        fundtransfer.sourcechannel);
                                    var comfocuslog = new MNComAndFocusOneLog(mnft, DateTime.Now);
                                    var mncomfocuslog = new MNComAndFocusOneLogsController();
                                    //mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                    result = mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                    //end:Com focus one log//

                                    if (result == "Success")
                                    {
                                        //NOTE:- may be need to validate before insert into reply typpe
                                        //start:insert into reply type as HTTP//
                                        var replyType = new MNReplyType(tid, "HTTP");
                                        var mnreplyType = new MNReplyTypesController();
                                        mnreplyType.InsertIntoReplyType(replyType);
                                        //end:insert into reply type as HTTP//

                                        MNMerchantsController getMerchantDetails = new MNMerchantsController();

                                        string GetMerchantMobile = getMerchantDetails.PassVidToGetMerchantDetail(vid);
                                        if (GetMerchantMobile != "" || GetMerchantMobile != null)
                                        {
                                            fundtransfer.da = GetMerchantMobile; //Set Destination Merchant Mobile number
                                            GetMerchantName = getMerchantDetails.PassVIdToGetMerchantName(vid);
                                        }
                                        else
                                        {
                                            statusCode = "400";
                                            message = "Destination Merchant Doesnot Exists";
                                            mnft.Response = "Destination Merchant Doesnot Exists";
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, mnft.Response);
                                            result = mnft.Response;
                                            failedmessage = message;
                                        }


                                        //start:insert into transaction master//
                                        if (mnft.valid())
                                        {
                                            //var transaction = new MNTransactionMaster(mnft);
                                            //var mntransaction = new MNTransactionsController();
                                            //validTransactionData = mntransaction.Validate(transaction, mnft.pin);

                                            var transactionpaypoint = new MNTransactionMaster(mnft);
                                            var mntransactionpaypoint = new MNTransactionsController();
                                            validTransactionData = mntransactionpaypoint.Validatepaypoint(transactionpaypoint, mnft.pin);

                                            result = validTransactionData.Response;
                                            /*** ***/
                                            ErrorMessage em = new ErrorMessage();

                                            if (validTransactionData.Response == "Error")
                                            {
                                                mnft.Response = "error";
                                                mnft.ResponseStatus(HttpStatusCode.InternalServerError,
                                                    "Internal server error - try again later, or contact support");
                                                result = mnft.Response;
                                                statusCode = "500";
                                                message = "Internal server error - try again later, or contact support";
                                                failedmessage = message;
                                            }
                                            else
                                            {
                                                if ((result == "Trace ID Repeated") || (result == "Limit Exceed")
                                                    || (result == "Invalid Source User") || (result == "Invalid Destination User")
                                                    || (result == "Invalid Product Request") || (result == "Please try again") || (result == ""))
                                                {
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                    statusCode = "400";
                                                    message = result;
                                                    failedmessage = message;
                                                }
                                                if (result == "Invalid PIN")
                                                {
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                    statusCode = "400";
                                                    message = result;
                                                    failedmessage = message;
                                                }
                                                if (result == "111")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_111;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "114")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_114;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "115")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_115;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "116")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_116;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "119")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_119;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "121")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_121;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "163")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_163;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "180")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_180;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "181")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_181;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "182")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_182;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "183")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_183;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "184")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_184;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "185")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_185;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "186")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_186;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "187")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_187;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "188")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_188;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "189")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_189;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "190")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_190;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "800")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_800;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "902")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_902;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "904")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_904;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "906")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_906;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "907")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_907;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "909")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_909;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "911")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_911;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "913")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_913;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "90")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_90;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "91")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_91;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "92")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_92;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "94")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_94;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "95")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_95;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "98")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_98;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "99")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_99;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                if (result == "508")
                                                {
                                                    statusCode = result;
                                                    message = em.Error_508;
                                                    failedmessage = message;
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                                }
                                                else if (validTransactionData.ResponseCode == "OK")
                                                {
                                                    statusCode = "200";
                                                    message = result;
                                                    mnft.ResponseStatus(HttpStatusCode.OK, message);

                                                } //END ValidTransactionData.ResponseCode


                                            } //END validTransactionData.Response WITHOUT MNDB ERROR

                                            //start comment outgoing
                                            //OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                                            //if (response.StatusCode == HttpStatusCode.OK)
                                            //{
                                            //    string messagereply = "";
                                            //    try
                                            //    {
                                            //        //messagereply = "Dear " + CustCheckUtils.GetName(mobile) + "," + "\n";

                                            //        //messagereply += " You have successfully paid NPR " + validTransactionData.Amount
                                            //        //                + " to " + GetMerchantName + " on date " +
                                            //        //                    (validTransactionData.CreatedDate).ToString("dd/MM/yyyy")
                                            //        //                + "." + "\n";
                                            //        //messagereply += "Thank you. MNepal";

                                            //        //var client = new WebClient();

                                            //        ////SENDER
                                            //        //if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                                            //        //{
                                            //        //    //FOR NCELL
                                            //        //    var content = client.DownloadString(
                                            //        //        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                            //        //        + "977" + mobile + "&Text=" + messagereply + "");
                                            //        //}
                                            //        //else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                            //        //            || (mobile.Substring(0, 3) == "986"))
                                            //        //{
                                            //        //    //FOR NTC
                                            //        //    var content = client.DownloadString(
                                            //        //        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                            //        //        + "977" + mobile + "&Text=" + messagereply + "");
                                            //        //}

                                            //        statusCode = "200";
                                            //        var v = new
                                            //        {
                                            //            StatusCode = Convert.ToInt32(statusCode),
                                            //            StatusMessage = result
                                            //        };
                                            //        result = JsonConvert.SerializeObject(v);

                                            //    }
                                            //    catch (Exception ex)
                                            //    {
                                            //        // throw ex
                                            //        statusCode = "400";
                                            //        message = ex.Message;
                                            //    }


                                            //    custsmsInfo = new CustActivityModel()
                                            //    {
                                            //        UserName = fundtransfer.mobile,
                                            //        RequestMerchant = transactionType,
                                            //        DestinationNo = fundtransfer.da,
                                            //        Amount = validTransactionData.Amount.ToString(),
                                            //        SMSStatus = "Success",
                                            //        SMSSenderReply = messagereply,
                                            //        ErrorMessage = "",
                                            //    };


                                            //}
                                            //else if ((response.StatusCode == HttpStatusCode.BadRequest) || (response.StatusCode == HttpStatusCode.InternalServerError) || (statusCode != "200"))
                                            //{
                                            //    custsmsInfo = new CustActivityModel()
                                            //    {
                                            //        UserName = mobile,
                                            //        RequestMerchant = transactionType,
                                            //        DestinationNo = fundtransfer.da,
                                            //        Amount = validTransactionData.Amount.ToString(),
                                            //        SMSStatus = "Failed",
                                            //        SMSSenderReply = message,
                                            //        ErrorMessage = failedmessage,
                                            //    };

                                            //}

                                            //end comment outgoint
                                            //end:insert into transaction master//

                                        } //END:insert into transaction master//
                                        else
                                        {
                                            mnft.Response = "error";
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, "parameters missing/invalid"); //200 - OK
                                            result = mnft.Response;
                                            statusCode = "400";
                                            message = "parameters missing/invalid";
                                            failedmessage = message;

                                            custsmsInfo = new CustActivityModel()
                                            {
                                                UserName = mobile,
                                                RequestMerchant = transactionType,
                                                DestinationNo = fundtransfer.da,
                                                Amount = amount,
                                                SMSStatus = "Failed",
                                                SMSSenderReply = message,
                                                ErrorMessage = failedmessage,
                                            };
                                        }

                                    } //END MNComAndFocusOneLogsController
                                    else
                                    {
                                        statusCode = "400";
                                        mnft.Response = "Data Insertion Failed in Check DB Connection: TraceID limit might have exceeded";
                                        mnft.ResponseStatus(HttpStatusCode.InternalServerError, mnft.Response);
                                        result = mnft.Response;
                                        failedmessage = result;
                                    }

                                } //END TRansLimit Check StatusCode N Message
                                else
                                {
                                    custsmsInfo = new CustActivityModel()
                                    {
                                        UserName = mobile,
                                        RequestMerchant = transactionType,
                                        DestinationNo = fundtransfer.da,
                                        Amount = amount,
                                        SMSStatus = "Failed",
                                        SMSSenderReply = message,
                                        ErrorMessage = failedmessage,
                                    };
                                }

                            } //END IsValidMerchant

                            //} //END Destination mobile No check

                        }

                    }
                    catch (Exception ex)
                    {
                        message = result + ex + "Error Message ";
                        //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                        statusCode = "400";
                        //failedmessage = message;
                        failedmessage = "Please try again.";
                    }

                }
                else  //else for  if wallet balance less then bill amount then show error msg
                {
                    statusCodeBalance = "400";
                    message = "Insufficient Balance";
                    failedmessage = message;

                }
            }
            else if (sc == "10")//for bank payment in nepal water
            {
                //if (bankBalancePaisaInt >= amountpayInt)
                //{
                //First transaction MNRequest N Response
                try
                {

                    //FundTransfer fundtransfer = new FundTransfer
                    //{
                    //    tid = tid,
                    //    sc = sc,
                    //    mobile = mobile,
                    //    da = da,
                    //    amount = amount,
                    //    pin = pin,
                    //    note = note,
                    //    sourcechannel = src
                    //};

                    ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, fundtransfer);
                    //CustActivityModel custsmsInfo = new CustActivityModel();

                    //MNTransactionMaster validTransactionData = new MNTransactionMaster();


                    //if (TokenGenerator.TokenChecker(sessionID, mobile, src) == false)
                    //{
                    //    // throw ex
                    //    statusCode = "400";
                    //    message = "Session expired. Please login again";
                    //    failedmessage = message;
                    //}
                    //else
                    //{
                    if ((tid == null) || (sc == null) || (mobile == null) || (da == null) || (amount == null) || (pin == null) ||
                    (src == null) || (double.Parse(amount) <= 0) || (vid == null))
                    {
                        // throw ex
                        statusCode = "400";
                        message = "Parameters Missing/Invalid";
                        failedmessage = message;
                    }
                    if ((companyCode == null) || (companyCode == "") || (serviceCode == null) || (serviceCode == "") ||
                        (account == null) || (special1 == null) || (special2 == null) || (transactionDate == null) || (transactionId == null) || (transactionId == "") ||
                        (userId == null) || (userPassword == null) || (salePointType == null) || (userId == "") || (userPassword == "") || (salePointType == ""))
                    {
                        // throw ex
                        statusCode = "400";
                        message = "Parameters Missing/Invalid PayPoint";
                        failedmessage = message;
                    }
                    else
                    {
                        if (sc == "00")
                        {
                            transactionType = "PayPoint Txfr to W2W";
                        }
                        else if (sc == "10")
                        {
                            transactionType = "PayPoint Txfr to B2W"; //B2W
                        }

                        if (!(UserNameCheck.IsValidUser(mobile)))
                        {
                            // throw ex
                            statusCode = "400";
                            //message = "Transaction restricted to User";
                            message = "Transaction only for User";
                            failedmessage = message;
                        }
                        if (UserNameCheck.IsValidMerchant(da))
                        {

                            TransLimitCheck transLimitCheck = new TransLimitCheck();
                            string resultTranLimit = transLimitCheck.LimitCheck(mobile, da, amount, sc, pin, src);

                            var jsonDataResult = JObject.Parse(resultTranLimit);
                            statusCode = jsonDataResult["StatusCode"].ToString();
                            string statusMsg = jsonDataResult["StatusMessage"].ToString();
                            message = jsonDataResult["StatusMessage"].ToString();
                            failedmessage = message;

                            if ((statusCode == "200") && (message == "Success"))
                            {
                                //start: checking trace id
                                do
                                {
                                    //TraceIdGenerator traceid = new TraceIdGenerator();
                                    // tid = traceid.GenerateUniqueTraceID();
                                    fundtransfer.tid = tid;

                                    bool traceIdCheck = false;
                                    traceIdCheck = TraceIdCheck.IsValidTraceId(fundtransfer.tid);
                                    if (traceIdCheck == true)
                                    {
                                        result = "Trace ID Repeated";
                                    }
                                    else
                                    {
                                        result = "false";
                                    }

                                } while (result == "Trace ID Repeated");
                                //End: TraceId

                                //start:Com focus one log///
                                MNFundTransfer mnft = new MNFundTransfer(tid, fundtransfer.sc, fundtransfer.mobile,
                                    fundtransfer.sa, fundtransfer.amount, fundtransfer.da, fundtransfer.note, fundtransfer.pin,
                                    fundtransfer.sourcechannel);
                                var comfocuslog = new MNComAndFocusOneLog(mnft, DateTime.Now);
                                var mncomfocuslog = new MNComAndFocusOneLogsController();
                                //mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                result = mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                //end:Com focus one log//

                                if (result == "Success")
                                {
                                    //NOTE:- may be need to validate before insert into reply typpe
                                    //start:insert into reply type as HTTP//
                                    var replyType = new MNReplyType(tid, "HTTP");
                                    var mnreplyType = new MNReplyTypesController();
                                    mnreplyType.InsertIntoReplyType(replyType);
                                    //end:insert into reply type as HTTP//

                                    MNMerchantsController getMerchantDetails = new MNMerchantsController();

                                    string GetMerchantMobile = getMerchantDetails.PassVidToGetMerchantDetail(vid);
                                    if (GetMerchantMobile != "" || GetMerchantMobile != null)
                                    {
                                        fundtransfer.da = GetMerchantMobile; //Set Destination Merchant Mobile number
                                        GetMerchantName = getMerchantDetails.PassVIdToGetMerchantName(vid);
                                    }
                                    else
                                    {
                                        statusCode = "400";
                                        message = "Destination Merchant Doesnot Exists";
                                        mnft.Response = "Destination Merchant Doesnot Exists";
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, mnft.Response);
                                        result = mnft.Response;
                                        failedmessage = message;
                                    }


                                    //start:insert into transaction master//
                                    if (mnft.valid())
                                    {
                                        //var transaction = new MNTransactionMaster(mnft);
                                        //var mntransaction = new MNTransactionsController();
                                        //validTransactionData = mntransaction.Validate(transaction, mnft.pin);


                                        var transactionpaypoint = new MNTransactionMaster(mnft);
                                        var mntransactionpaypoint = new MNTransactionsController();
                                        validTransactionData = mntransactionpaypoint.Validatepaypoint(transactionpaypoint, mnft.pin);
                                        result = validTransactionData.Response;
                                        /*** ***/
                                        ErrorMessage em = new ErrorMessage();

                                        if (validTransactionData.Response == "Error")
                                        {
                                            mnft.Response = "error";
                                            mnft.ResponseStatus(HttpStatusCode.InternalServerError,
                                                "Internal server error - try again later, or contact support");
                                            result = mnft.Response;
                                            statusCode = "500";
                                            message = "Internal server error - try again later, or contact support";
                                            failedmessage = message;
                                        }
                                        else
                                        {
                                            if ((result == "Trace ID Repeated") || (result == "Limit Exceed")
                                                || (result == "Invalid Source User") || (result == "Invalid Destination User")
                                                || (result == "Invalid Product Request") || (result == "Please try again") || (result == ""))
                                            {
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                statusCode = "400";
                                                message = result;
                                                failedmessage = message;
                                            }
                                            if (result == "Invalid PIN")
                                            {
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                statusCode = "400";
                                                message = result;
                                                failedmessage = message;
                                            }
                                            if (result == "111")
                                            {
                                                statusCode = result;
                                                message = em.Error_111;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "114")
                                            {
                                                statusCode = result;
                                                message = em.Error_114;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "115")
                                            {
                                                statusCode = result;
                                                message = em.Error_115;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "116")
                                            {
                                                statusCode = result;
                                                message = em.Error_116;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "119")
                                            {
                                                statusCode = result;
                                                message = em.Error_119;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "121")
                                            {
                                                statusCode = result;
                                                message = em.Error_121;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "163")
                                            {
                                                statusCode = result;
                                                message = em.Error_163;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "180")
                                            {
                                                statusCode = result;
                                                message = em.Error_180;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "181")
                                            {
                                                statusCode = result;
                                                message = em.Error_181;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "182")
                                            {
                                                statusCode = result;
                                                message = em.Error_182;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "183")
                                            {
                                                statusCode = result;
                                                message = em.Error_183;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "184")
                                            {
                                                statusCode = result;
                                                message = em.Error_184;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "185")
                                            {
                                                statusCode = result;
                                                message = em.Error_185;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "186")
                                            {
                                                statusCode = result;
                                                message = em.Error_186;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "187")
                                            {
                                                statusCode = result;
                                                message = em.Error_187;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "188")
                                            {
                                                statusCode = result;
                                                message = em.Error_188;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "189")
                                            {
                                                statusCode = result;
                                                message = em.Error_189;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "190")
                                            {
                                                statusCode = result;
                                                message = em.Error_190;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "800")
                                            {
                                                statusCode = result;
                                                message = em.Error_800;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "902")
                                            {
                                                statusCode = result;
                                                message = em.Error_902;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "904")
                                            {
                                                statusCode = result;
                                                message = em.Error_904;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "906")
                                            {
                                                statusCode = result;
                                                message = em.Error_906;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "907")
                                            {
                                                statusCode = result;
                                                message = em.Error_907;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "909")
                                            {
                                                statusCode = result;
                                                message = em.Error_909;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "911")
                                            {
                                                statusCode = result;
                                                message = em.Error_911;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "913")
                                            {
                                                statusCode = result;
                                                message = em.Error_913;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "90")
                                            {
                                                statusCode = result;
                                                message = em.Error_90;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "91")
                                            {
                                                statusCode = result;
                                                message = em.Error_91;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "92")
                                            {
                                                statusCode = result;
                                                message = em.Error_92;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "94")
                                            {
                                                statusCode = result;
                                                message = em.Error_94;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "95")
                                            {
                                                statusCode = result;
                                                message = em.Error_95;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "98")
                                            {
                                                statusCode = result;
                                                message = em.Error_98;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "99")
                                            {
                                                statusCode = result;
                                                message = em.Error_99;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "508")
                                            {
                                                statusCode = result;
                                                message = em.Error_508;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            else if (validTransactionData.ResponseCode == "OK")
                                            {
                                                statusCode = "200";
                                                message = result;
                                                mnft.ResponseStatus(HttpStatusCode.OK, message);

                                            } //END ValidTransactionData.ResponseCode


                                        } //END validTransactionData.Response WITHOUT MNDB ERROR

                                        //end:insert into transaction master//

                                    } //END:insert into transaction master//
                                    else
                                    {
                                        mnft.Response = "error";
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, "parameters missing/invalid"); //200 - OK
                                        result = mnft.Response;
                                        statusCode = "400";
                                        message = "parameters missing/invalid";
                                        failedmessage = message;

                                        custsmsInfo = new CustActivityModel()
                                        {
                                            UserName = mobile,
                                            RequestMerchant = transactionType,
                                            DestinationNo = fundtransfer.da,
                                            Amount = amount,
                                            SMSStatus = "Failed",
                                            SMSSenderReply = message,
                                            ErrorMessage = failedmessage,
                                        };
                                    }

                                } //END MNComAndFocusOneLogsController
                                else
                                {
                                    statusCode = "400";
                                    mnft.Response = "Data Insertion Failed in Check DB Connection: TraceID limit might have exceeded";
                                    mnft.ResponseStatus(HttpStatusCode.InternalServerError, mnft.Response);
                                    result = mnft.Response;
                                    failedmessage = result;
                                }

                            } //END TRansLimit Check StatusCode N Message
                            else
                            {
                                custsmsInfo = new CustActivityModel()
                                {
                                    UserName = mobile,
                                    RequestMerchant = transactionType,
                                    DestinationNo = fundtransfer.da,
                                    Amount = amount,
                                    SMSStatus = "Failed",
                                    SMSSenderReply = message,
                                    ErrorMessage = failedmessage,
                                };
                            }

                        } //END IsValidMerchant


                        //} //END Destination mobile No check

                    }

                }
                catch (Exception ex)
                {
                    message = result + ex + "Error Message ";
                    //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                    statusCode = "400";
                    //failedmessage = message;
                    failedmessage = "Please try again.";
                }
                //}
                //else  //else for  if wallet balance less then bill amount then show error msg
                //{
                //    statusCodeBalance = "400";
                //    message = "Insufficient Balance";
                //    failedmessage = message;

                //}
            }
            try
            {

                //for  all EP  and GT transaction
                string compResultResp = "";

                if (statusCode == "200")
                {
                    try
                    {
                        //string amountInPaisa = ((double.Parse(amount)) * 100).ToString();

                        if (rltCheckPaymt == "000")//go if CP Response is 000
                        {
                            //int amountInPaisaInt = Convert.ToInt32(amountInPaisa);

                            long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                            exectransactionId = milliseconds.ToString();

                            string keyExecRlt = "";
                            string resultMessageResEP = "";
                            do
                            {
                                if ((companyCode == null) || (serviceCode == null) || (account == null) ||
                                (exectransactionDate == null) || (exectransactionId == null) ||
                                (refStan == null) || (amount == null) || (billNumber == null) ||
                                (userId == null) || (userPassword == null) || (salePointType == null))
                                {
                                    // throw ex
                                    statusCode = "400";
                                    message = "Parameters Missing/Invalid";
                                    failedmessage = message;
                                }
                                else
                                {
                                    if (companyCode == "598")
                                    {
                                        special1 = "";
                                    }
                                    else
                                    {
                                        special1 = special1.ToString();
                                    }

                                    // string URIEXECPayment = "https://test.paypoint.md:4445/PayPointWS/PayPointMSOperations.asmx/ExecutePayment";//for EP Link 

                                    //For excutepaypoint link in webconfig
                                    string URIEXECPayment = System.Web.Configuration.WebConfigurationManager.AppSettings["EPPaypointUrl"];

                                    string execPayParameters = "companyCode=" + companyCode + "&serviceCode=" + serviceCode +
                                            "&account=" + account + "&special1=" + special1 + "&special2=" + special2 +
                                            "&transactionDate=" + exectransactionDate + "&transactionId=" + exectransactionId +
                                            "&refStan=" + refStan + "&amount=" + Convert.ToInt32(float.Parse(amount) * 100).ToString() + "&billNumber=" + billNumber +
                                            "&userId=" + userId.Trim() + "&userPassword=" + userPassword.Trim() + "&salePointType=" + salePointType;

                                    //for executepayment request insert in database for wlink
                                    reqEPPaypointDishHomeInfo = new PaypointModel()
                                    {
                                        companyCodeReqEP = companyCode,
                                        serviceCodeReqEP = serviceCode,
                                        accountReqEP = account,
                                        special1ReqEP = special1,
                                        special2ReqEP = special2,

                                        transactionDateReqEP = exectransactionDate,
                                        transactionIdReqEP = exectransactionId,
                                        userIdReqEP = userId,
                                        userPasswordReqEP = userPassword,
                                        salePointTypeReqEP = salePointType,

                                        refStanReqEP = refStan,
                                        amountReqEP = amount,
                                        billNumberReqEP = billNumber,
                                        //retrievalReferenceReqEP = fundtransfer.tid,
                                        //retrievalReferenceReqEP = tid,
                                        retrievalReferenceReqEP = retrievalReference,
                                        remarkReqEP = "Execute Payment",
                                        UserName = mobile,
                                        ClientCode = ClientCode,
                                        paypointType = paypointType,

                                    };

                                    using (WebClient wcExecPay = new WebClient())
                                    {
                                        wcExecPay.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                                        string HtmlResultExecPay = wcExecPay.UploadString(URIEXECPayment, execPayParameters);

                                        XmlDocument xmlEDoc = new XmlDocument();
                                        xmlEDoc.LoadXml(HtmlResultExecPay);

                                        XmlNodeList nodeEPay = xmlEDoc.GetElementsByTagName("*");
                                        string resultEPay = nodeEPay[0].InnerText;
                                        string HtmlEPayResult = resultEPay;

                                        //for determining  checkpayment key and result
                                        var readerEPay = new StringReader(HtmlEPayResult);
                                        var xdocEPay = XDocument.Load(readerEPay);

                                        XDocument docEPay = XDocument.Parse(xdocEPay.ToString());

                                        var xElemEPay = XElement.Parse(xdocEPay.ToString());

                                        if (xElemEPay.Attribute("Result").Value == "000")
                                        {
                                            compResultResp = xElemEPay.Attribute("Result").Value;
                                            keyExecRlt = xElemEPay.Attribute("Key").Value;
                                        }
                                        else if ((xElemEPay.Attribute("Result").Value == "011") || (xElemEPay.Attribute("Result").Value == "012"))
                                        {
                                            compResultResp = xElemEPay.Attribute("Result").Value;
                                            keyExecRlt = xElemEPay.Attribute("Key").Value;
                                        }
                                        else
                                        {
                                            compResultResp = xElemEPay.Attribute("Result").Value;
                                            keyExecRlt = xElemEPay.Attribute("Key").Value;
                                        }
                                        resultMessageResEP = xElemEPay.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;



                                        //for Response Execute Payment
                                        resEPPaypointDishHomeInfo = new PaypointModel()
                                        {
                                            companyCodeResEP = companyCode,
                                            serviceCodeResEP = serviceCode,
                                            accountResEP = account,
                                            special1ResEP = special1,
                                            special2ResEP = special2,

                                            transactionDateResEP = transactionDate,
                                            transactionIdResEP = transactionId,
                                            userIdResEP = userId,

                                            userPasswordResEP = userPassword,
                                            salePointTypeResEP = salePointType,

                                            refStanResEP = refStan,
                                            amountResEP = amount,
                                            billNumberResEP = billNumber,
                                            //retrievalReferenceResEP = fundtransfer.tid,
                                            //retrievalReferenceResEP = tid,
                                            retrievalReferenceResEP = retrievalReference,
                                            responseCodeResEP = compResultResp,
                                            descriptionResEP = "Execute Payment" + keyExecRlt,
                                            UserName = mobile,
                                            ClientCode = ClientCode,
                                            paypointType = paypointType,
                                            resultMessageResEP = resultMessageResEP,
                                            customerNameResEP = customerName,
                                        };
                                    }
                                }

                            } while ((compResultResp == "011") || (compResultResp == "012"));

                            ///for  Inserting EP PayPoint Data for wlink

                            try
                            {
                                int resultsReqEP = PaypointUtils.RequestEPPaypointInfo(reqEPPaypointDishHomeInfo);
                                int resultsResEP = PaypointUtils.ResponseEPPaypointInfo(resEPPaypointDishHomeInfo);

                                if ((resultsReqEP > 0) && (resultsResEP > 0))
                                {
                                    message = result;
                                }
                                else
                                {
                                    message = result;
                                }

                            }
                            catch (Exception ex)
                            {
                                string ss = ex.Message;
                                message = result;
                            }

                            if (compResultResp == "000")
                            {
                                if ((refStan == null) || (userId == null) || (userPassword == null) || (salePointType == null))
                                {
                                    // throw ex
                                    statusCode = "400";
                                    message = "Parameters Missing/Invalid";
                                    failedmessage = message;
                                }
                                else if (compResultResp != "")
                                {
                                    string statusResGTP = "1";
                                    do
                                    {
                                        string key = "";
                                        string gtBillNumber = "";
                                        // string URIGetTran = "https://test.paypoint.md:4445/PayPointWS/PayPointMSOperations.asmx/GetTransaction";//for GT paypoint transactionlink

                                        //For get transactionpaypoint link in webconfig
                                        string URIGetTran = System.Web.Configuration.WebConfigurationManager.AppSettings["GTPPaypointUrl"];

                                        string GetTranParameters = "userLogin=" + userId + "&userPassword=" + userPassword + "&stan=" + "-1" + "&refStan=" + refStan + "&key=" + "" + "&billNumber=" + gtBillNumber;

                                        //for get transaction payment request insert in database
                                        reqGTPaypointDishHomeInfo = new PaypointModel()
                                        {
                                            companyCodeReqGTP = companyCode,
                                            serviceCodeReqGTP = serviceCode,
                                            accountReqGTP = account,
                                            special1ReqGTP = special1,
                                            special2ReqGTP = special2,

                                            transactionDateReqGTP = transactionDate,
                                            transactionIdReqGTP = transactionId,
                                            userIdReqGTP = userId,
                                            userPasswordReqGTP = userPassword,
                                            salePointTypeReqGTP = salePointType,

                                            refStanReqGTP = refStan,
                                            amountReqGTP = amount,
                                            billNumberReqGTP = gtBillNumber,
                                            // retrievalReferenceReqGTP = fundtransfer.tid,
                                            //retrievalReferenceReqGTP = tid,
                                            retrievalReferenceReqGTP = retrievalReference,
                                            remarkReqGTP = "Get Transaction Payment",
                                            UserName = mobile,
                                            ClientCode = ClientCode,
                                            paypointType = paypointType,
                                            //remarkReqGTP = "Get Transaction Payment"+keyExecRlt,

                                        };
                                        string getTranResultResp = "";
                                        string keyGetTrancRlt = "";
                                        string resultMessageResGTP = "";
                                        string billNumberResGTP = "";
                                        using (WebClient wcGetTran = new WebClient())
                                        {
                                            wcGetTran.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                                            string HtmlResultGetTran = wcGetTran.UploadString(URIGetTran, GetTranParameters);

                                            XmlDocument xmlEDoc = new XmlDocument();
                                            xmlEDoc.LoadXml(HtmlResultGetTran);

                                            XmlNodeList nodeEPay = xmlEDoc.GetElementsByTagName("*");
                                            string resultEPay = nodeEPay[0].InnerText;
                                            string HtmlEPayResult = resultEPay;

                                            //for determing excutepayment key and result
                                            var readerEPay = new StringReader(HtmlEPayResult);
                                            var xdocEPay = XDocument.Load(readerEPay);

                                            XDocument docEPay = XDocument.Parse(xdocEPay.ToString());
                                            var xElemGPay = XElement.Parse(docEPay.ToString());
                                            if (xElemGPay.Attribute("Result").Value == "000")
                                            {
                                                getTranResultResp = xElemGPay.Attribute("Result").Value;
                                                keyGetTrancRlt = xElemGPay.Attribute("Key").Value;
                                                resultMessageResGTP = xElemGPay.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;
                                                if (!(resultMessageResGTP == "No data"))
                                                {
                                                    billNumberResGTP = xElemGPay.Descendants().Elements("BillNumber").Where(x => x.Name == "BillNumber").SingleOrDefault().Value;
                                                    statusResGTP = xElemGPay.Descendants().Elements("Status").Where(x => x.Name == "Status").SingleOrDefault().Value;

                                                    //for get transaction payment status validation
                                                    if (!(statusResGTP == "1" || statusResGTP == "5" || statusResGTP == "11" || statusResGTP == "13"))
                                                    {
                                                        if (statusResGTP == "4" || statusResGTP == "6" || statusResGTP == "14" || statusResGTP == "16")
                                                        {
                                                            // mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                            statusCode = "400";
                                                            //message = result;
                                                            message = "Failed";
                                                            failedmessage = message;
                                                            resultMessageResGTP = "failed";
                                                        }

                                                    }
                                                    else
                                                    {
                                                        statusCode = "200";
                                                        resultMessageResGTP = "sucess";
                                                    }
                                                }
                                                else
                                                {
                                                    //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                    statusCode = "400";
                                                    //message = result;
                                                    message = "Failed";
                                                    failedmessage = message;
                                                    resultMessageResGTP = "failed";
                                                }
                                            }
                                            else
                                            {
                                                resultMessageResGTP = xElemGPay.Descendants().Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;

                                            }


                                            //end get transaction payment status validation

                                            ////for get transaction payment response insert in database
                                            resGTPaypointDishHomeInfo = new PaypointModel()
                                            {
                                                companyCodeResGTP = companyCode,
                                                serviceCodeResGTP = serviceCode,
                                                accountResGTP = account,
                                                special1ResGTP = special1,
                                                special2ResGTP = special2,

                                                transactionDateResGTP = transactionDate,
                                                transactionIdResGTP = transactionId,
                                                userIdResGTP = userId,
                                                userPasswordResGTP = userPassword,
                                                salePointTypeResGTP = salePointType,

                                                refStanResGTP = refStan,
                                                amountResGTP = amount, //amountpay,
                                                billNumberResGTP = billNumberResGTP,
                                                //retrievalReferenceResGTP = fundtransfer.tid,
                                                //retrievalReferenceResGTP = tid,
                                                retrievalReferenceResGTP = retrievalReference,
                                                responseCodeResGTP = getTranResultResp,
                                                descriptionResGTP = "Get Transaction Payment " + keyGetTrancRlt,
                                                UserName = mobile,
                                                ClientCode = ClientCode,
                                                paypointType = paypointType,
                                                resultMessageResGTP = resultMessageResGTP,
                                                customerNameResGTP = customerName,

                                            };
                                        }
                                    } while (statusResGTP == "10" || statusResGTP == "15" || statusResGTP == "20" || statusResGTP == "21" || statusResGTP == "99" || statusResGTP == "12" || statusResGTP == "2" || statusResGTP == "0");

                                    ///for  Inserting GT PayPoint Data

                                    try
                                    {

                                        int resultsReqGTP = PaypointUtils.RequestGTPaypointInfo(reqGTPaypointDishHomeInfo);
                                        int resultsResGTP = PaypointUtils.ResponseGTPaypointInfo(resGTPaypointDishHomeInfo);

                                        if ((resultsReqGTP > 0) && (resultsResGTP > 0))
                                        {
                                            message = result;
                                        }
                                        else
                                        {
                                            message = result;
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        string ss = ex.Message;
                                        message = result;
                                    }

                                }

                            }

                            else
                            {
                                //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                statusCode = "400";
                                //message = result;
                                message = resultMessageResEP;
                                failedmessage = message;
                            }

                        }
                        else //ELSE FOR (rltCheckPaymt == "000")I.E Error  in result of CP
                        {
                            //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                            statusCode = "400";
                            // message = result;
                            message = resultMessageResCP;

                            failedmessage = message;

                            ////start:Com focus one log///
                            //MNFundTransfer mnft1 = new MNFundTransfer(tid, fundtransfer.sc, fundtransfer.da,
                            //    fundtransfer.sa, fundtransfer.amount, fundtransfer.mobile, fundtransfer.note, fundtransfer.pin,
                            //    fundtransfer.sourcechannel);
                            //var comfocuslog1 = new MNComAndFocusOneLog(mnft1, DateTime.Now);
                            //var mncomfocuslog1 = new MNComAndFocusOneLogsController();
                            //result = mncomfocuslog1.InsertIntoComFocusOne(comfocuslog1);
                            ////end:Com focus one log//

                            //if (mnft1.valid())
                            //{
                            //    var transaction1 = new MNTransactionMaster(mnft1);
                            //    var mntransaction1 = new MNTransactionsController();
                            //    validTransactionData = mntransaction1.Validate(transaction1, mnft1.pin);
                            //    result = validTransactionData.Response;

                            //}
                        }
                        //end excute dekhi get ko sabai comment gareko
                    }
                    catch (Exception ex)
                    {
                        message = result + ex + "Error Message ";
                        //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                        statusCode = "400";
                        failedmessage = "Please try again.";
                    }
                }

                //REverse Transaction 
                if ((statusCode != "111") && (statusCode != "114") && (statusCode != "115") && (statusCode != "116") && (statusCode != "119") &&
                    (statusCode != "121") && (statusCode != "163") && (statusCode != "180") && (statusCode != "181") && (statusCode != "182") &&
                    (statusCode != "183") && (statusCode != "184") && (statusCode != "185") && (statusCode != "186") && (statusCode != "187") &&
                    (statusCode != "188") && (statusCode != "189") && (statusCode != "190") && (statusCode != "800") && (statusCode != "902") &&
                    (statusCode != "904") && (statusCode != "906") && (statusCode != "907") && (statusCode != "909") && (statusCode != "911") &&
                    (statusCode != "913") && (statusCode != "90") && (statusCode != "91") && (statusCode != "94") && (statusCode != "95") &&
                    (statusCode != "98") && (statusCode != "99") && (statusCodeBalance != "400") && (compResultResp != "000") && (statusCodeBalance != "400")
                    && (statusCode != "200") && (statusCode != "508")
                    )
                //((statusCodeBalance != "400") && (compResultResp != "000")) || ((statusCodeBalance != "400") && (statusCode != "200")) ||
                //if (statusCode != "116")
                {
                    //TraceIdGenerator traceRevid = new TraceIdGenerator();
                    //tid = traceRevid.GenerateUniqueTraceID();
                    tid = retrievalReference;
                    if (sc == "00")
                    {
                        transactionType = "PayPoint Txfr to W2W";
                    }
                    else
                    {
                        sc = "01";
                        transactionType = "PayPoint Txfr to W2B"; //B2W
                    }
                    FundTransfer fundtransferRev = new FundTransfer
                    {
                        tid = tid,
                        sc = sc,
                        mobile = da,//mobile
                        da = mobile,//da
                        amount = amount,
                        pin = pin,
                        note = "reverse " + note,
                        sourcechannel = src
                    };
                    ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, fundtransfer);
                    //CustActivityModel custsmsInfo = new CustActivityModel();


                    // MNTransactionMaster validTransactionData = new MNTransactionMaster();


                    //if (TokenGenerator.TokenChecker(sessionID, mobile, src) == false)
                    //{
                    //    // throw ex
                    //    statusCode = "400";
                    //    message = "Session expired. Please login again";
                    //    failedmessage = message;
                    //}
                    //else
                    //{
                    if ((tid == null) || (sc == null) || (mobile == null) || (da == null) || (amount == null) || (pin == null) ||
                    (src == null) || (double.Parse(amount) <= 0) || (vid == null))
                    {
                        // throw ex
                        statusCode = "400";
                        message = "Parameters Missing/Invalid";
                        failedmessage = message;
                    }
                    if ((companyCode == null) || (companyCode == "") || (serviceCode == null) || (serviceCode == "") ||
                        (account == null) || (special1 == null) || (special2 == null) || (transactionDate == null) || (transactionId == null) || (transactionId == "") ||
                        (userId == null) || (userPassword == null) || (salePointType == null) || (userId == "") || (userPassword == "") || (salePointType == ""))
                    {
                        // throw ex
                        statusCode = "400";
                        message = "Parameters Missing/Invalid PayPoint";
                        failedmessage = message;
                    }
                    else
                    {
                        //if (sc == "00")
                        //{
                        //    transactionType = "PayPoint Txfr to W2W";
                        //}
                        //else 
                        //{
                        //    sc = "01";
                        //    transactionType = "PayPoint Txfr to W2B"; //B2W
                        //}

                        if (!(UserNameCheck.IsValidUser(mobile)))
                        {
                            // throw ex
                            statusCode = "400";
                            //message = "Transaction restricted to User";
                            message = "Transaction only for User";
                            failedmessage = message;
                        }
                        if (UserNameCheck.IsValidMerchant(da))
                        {

                            TransLimitCheck transLimitCheck = new TransLimitCheck();
                            string resultTranLimit = transLimitCheck.LimitCheck(mobile, da, amount, sc, pin, src);

                            var jsonDataResult = JObject.Parse(resultTranLimit);
                            statusCode = jsonDataResult["StatusCode"].ToString();
                            string statusMsg = jsonDataResult["StatusMessage"].ToString();
                            message = jsonDataResult["StatusMessage"].ToString();
                            failedmessage = message;

                            if ((statusCode == "200") && (message == "Success"))
                            {
                                //start: checking trace id
                                do
                                {
                                    TraceIdGenerator traceid = new TraceIdGenerator();
                                    tid = traceid.GenerateUniqueTraceID();
                                    fundtransfer.tid = tid;

                                    bool traceIdCheck = false;
                                    traceIdCheck = TraceIdCheck.IsValidTraceId(fundtransfer.tid);
                                    if (traceIdCheck == true)
                                    {
                                        result = "Trace ID Repeated";
                                    }
                                    else
                                    {
                                        result = "false";
                                    }

                                } while (result == "Trace ID Repeated");
                                //End: TraceId

                                //start:Com focus one log///
                                MNFundTransfer mnft = new MNFundTransfer(tid, fundtransferRev.sc, fundtransferRev.mobile,
                                       fundtransferRev.sa, fundtransferRev.amount, fundtransferRev.da, fundtransferRev.note, fundtransferRev.pin,
                                       fundtransferRev.sourcechannel, "T", "PayPoint");
                                var comfocuslog = new MNComAndFocusOneLog(mnft, DateTime.Now);
                                var mncomfocuslog = new MNComAndFocusOneLogsController();
                                //mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                result = mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                //end:Com focus one log//

                                if (result == "Success")
                                {
                                    //NOTE:- may be need to validate before insert into reply typpe
                                    //start:insert into reply type as HTTP//
                                    var replyType = new MNReplyType(tid, "HTTP");
                                    var mnreplyType = new MNReplyTypesController();
                                    mnreplyType.InsertIntoReplyType(replyType);
                                    //end:insert into reply type as HTTP//

                                    MNMerchantsController getMerchantDetails = new MNMerchantsController();

                                    string GetMerchantMobile = getMerchantDetails.PassVidToGetMerchantDetail(vid);
                                    if (GetMerchantMobile != "" || GetMerchantMobile != null)
                                    {
                                        fundtransfer.da = GetMerchantMobile; //Set Destination Merchant Mobile number
                                        GetMerchantName = getMerchantDetails.PassVIdToGetMerchantName(vid);
                                    }
                                    else
                                    {
                                        statusCode = "400";
                                        message = "Destination Merchant Doesnot Exists";
                                        mnft.Response = "Destination Merchant Doesnot Exists";
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, mnft.Response);
                                        result = mnft.Response;
                                        failedmessage = message;
                                    }


                                    //start:insert into transaction master//
                                    if (mnft.valid())
                                    {
                                        var transaction = new MNTransactionMaster(mnft);
                                        var mntransaction = new MNTransactionsController();
                                        //validTransactionData = mntransaction.Validate(transaction, mnft.pin);
                                        validTransactionData = mntransaction.Validatepaypoint(transaction, mnft.pin);
                                        result = validTransactionData.Response;

                                        /*** ***/
                                        ErrorMessage em = new ErrorMessage();

                                        if (validTransactionData.Response == "Error")
                                        {
                                            mnft.Response = "error";
                                            mnft.ResponseStatus(HttpStatusCode.InternalServerError,
                                                "Internal server error - try again later, or contact support");
                                            result = mnft.Response;
                                            statusCode = "500";
                                            message = "Internal server error - try again later, or contact support";
                                            failedmessage = message;
                                        }
                                        else
                                        {
                                            if ((result == "Trace ID Repeated") || (result == "Limit Exceed")
                                                || (result == "Invalid Source User") || (result == "Invalid Destination User")
                                                || (result == "Invalid Product Request") || (result == "Please try again") || (result == ""))
                                            {
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                statusCode = "400";
                                                message = result;
                                                failedmessage = message;
                                            }
                                            if (result == "Invalid PIN")
                                            {
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                statusCode = "400";
                                                message = result;
                                                failedmessage = message;
                                            }
                                            if (result == "111")
                                            {
                                                statusCode = result;
                                                message = em.Error_111;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "114")
                                            {
                                                statusCode = result;
                                                message = em.Error_114;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "115")
                                            {
                                                statusCode = result;
                                                message = em.Error_115;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "116")
                                            {
                                                statusCode = result;
                                                message = em.Error_116;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "119")
                                            {
                                                statusCode = result;
                                                message = em.Error_119;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "121")
                                            {
                                                statusCode = result;
                                                message = em.Error_121;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "163")
                                            {
                                                statusCode = result;
                                                message = em.Error_163;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "180")
                                            {
                                                statusCode = result;
                                                message = em.Error_180;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "181")
                                            {
                                                statusCode = result;
                                                message = em.Error_181;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "182")
                                            {
                                                statusCode = result;
                                                message = em.Error_182;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "183")
                                            {
                                                statusCode = result;
                                                message = em.Error_183;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "184")
                                            {
                                                statusCode = result;
                                                message = em.Error_184;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "185")
                                            {
                                                statusCode = result;
                                                message = em.Error_185;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "186")
                                            {
                                                statusCode = result;
                                                message = em.Error_186;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "187")
                                            {
                                                statusCode = result;
                                                message = em.Error_187;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "188")
                                            {
                                                statusCode = result;
                                                message = em.Error_188;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "189")
                                            {
                                                statusCode = result;
                                                message = em.Error_189;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "190")
                                            {
                                                statusCode = result;
                                                message = em.Error_190;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "800")
                                            {
                                                statusCode = result;
                                                message = em.Error_800;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "902")
                                            {
                                                statusCode = result;
                                                message = em.Error_902;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "904")
                                            {
                                                statusCode = result;
                                                message = em.Error_904;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "906")
                                            {
                                                statusCode = result;
                                                message = em.Error_906;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "907")
                                            {
                                                statusCode = result;
                                                message = em.Error_907;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "909")
                                            {
                                                statusCode = result;
                                                message = em.Error_909;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "911")
                                            {
                                                statusCode = result;
                                                message = em.Error_911;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "913")
                                            {
                                                statusCode = result;
                                                message = em.Error_913;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "90")
                                            {
                                                statusCode = result;
                                                message = em.Error_90;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "91")
                                            {
                                                statusCode = result;
                                                message = em.Error_91;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "92")
                                            {
                                                statusCode = result;
                                                message = em.Error_92;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "94")
                                            {
                                                statusCode = result;
                                                message = em.Error_94;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "95")
                                            {
                                                statusCode = result;
                                                message = em.Error_95;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "98")
                                            {
                                                statusCode = result;
                                                message = em.Error_98;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            if (result == "99")
                                            {
                                                statusCode = result;
                                                message = em.Error_99;
                                                failedmessage = message;
                                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                            }
                                            else if (validTransactionData.ResponseCode == "OK")
                                            {
                                                statusCode = "200";
                                                message = result;
                                                mnft.ResponseStatus(HttpStatusCode.OK, message);

                                            } //END ValidTransactionData.ResponseCode


                                        } //END validTransactionData.Response WITHOUT MNDB ERROR
                                        /*** ***/

                                        OutgoingWebResponseContext response1 = WebOperationContext.Current.OutgoingResponse;
                                        if (response1.StatusCode == HttpStatusCode.OK)
                                        {
                                            string messagereply = "";
                                            try
                                            {
                                                messagereply = "Dear " + CustCheckUtils.GetName(mobile) + "," + "\n";

                                                messagereply += " You have successfully reverse  NPR " + validTransactionData.Amount
                                                                    + " to " +
                                                                    //GetMerchantName 
                                                                    "Utility payment for Dish Home Direct." + " on date " +
                                                                    (validTransactionData.CreatedDate).ToString("dd/MM/yyyy")
                                                                + "." + "\n";
                                                messagereply += "Thank you. MNepal";

                                                var client = new WebClient();

                                                //SENDER
                                                if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                                                {
                                                    //FOR NCELL
                                                    var content = client.DownloadString(
                                                        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                                        + "977" + mobile + "&Text=" + messagereply + "");
                                                }
                                                else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                                            || (mobile.Substring(0, 3) == "986"))
                                                {
                                                    //FOR NTC
                                                    var content = client.DownloadString(
                                                        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                                        + "977" + mobile + "&Text=" + messagereply + "");
                                                }

                                                statusCode = "200";
                                                var v = new
                                                {
                                                    StatusCode = Convert.ToInt32(statusCode),
                                                    StatusMessage = result
                                                };
                                                result = JsonConvert.SerializeObject(v);

                                            }
                                            catch (Exception ex)
                                            {
                                                // throw ex
                                                statusCode = "400";
                                                message = ex.Message;
                                            }


                                            custsmsInfo = new CustActivityModel()
                                            {
                                                UserName = fundtransfer.mobile,
                                                RequestMerchant = transactionType,
                                                DestinationNo = fundtransfer.da,
                                                Amount = validTransactionData.Amount.ToString(),
                                                SMSStatus = "Success",
                                                SMSSenderReply = messagereply,
                                                ErrorMessage = "",
                                            };


                                        }
                                        else if ((response1.StatusCode == HttpStatusCode.BadRequest) || (response1.StatusCode == HttpStatusCode.InternalServerError) || (statusCode != "200"))
                                        {
                                            custsmsInfo = new CustActivityModel()
                                            {
                                                UserName = mobile,
                                                RequestMerchant = transactionType,
                                                DestinationNo = fundtransfer.da,
                                                Amount = validTransactionData.Amount.ToString(),
                                                SMSStatus = "Failed",
                                                SMSSenderReply = message,
                                                ErrorMessage = failedmessage,
                                            };

                                        }
                                        //end:insert into transaction master//

                                    } //END:insert into transaction master//
                                    else
                                    {
                                        mnft.Response = "error";
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, "parameters missing/invalid"); //200 - OK
                                        result = mnft.Response;
                                        statusCode = "400";
                                        message = "parameters missing/invalid";
                                        failedmessage = message;

                                        custsmsInfo = new CustActivityModel()
                                        {
                                            UserName = mobile,
                                            RequestMerchant = transactionType,
                                            DestinationNo = fundtransfer.da,
                                            Amount = amount,
                                            SMSStatus = "Failed",
                                            SMSSenderReply = message,
                                            ErrorMessage = failedmessage,
                                        };
                                    }

                                } //END MNComAndFocusOneLogsController
                                else
                                {
                                    statusCode = "400";
                                    mnft.Response = "Data Insertion Failed in Check DB Connection: TraceID limit might have exceeded";
                                    mnft.ResponseStatus(HttpStatusCode.InternalServerError, mnft.Response);
                                    result = mnft.Response;
                                    failedmessage = result;
                                }

                            } //END TRansLimit Check StatusCode N Message
                            else
                            {
                                custsmsInfo = new CustActivityModel()
                                {
                                    UserName = mobile,
                                    RequestMerchant = transactionType,
                                    DestinationNo = fundtransfer.da,
                                    Amount = amount,
                                    SMSStatus = "Failed",
                                    SMSSenderReply = message,
                                    ErrorMessage = failedmessage,
                                };
                            }

                        } //END IsValidMerchant

                        //} //END Destination mobile No check

                    }
                }
                //} //tokengenerator ko closing bracket

                //for sending sms  if success 
                if (compResultResp == "000")
                {
                    OutgoingWebResponseContext response2 = WebOperationContext.Current.OutgoingResponse;
                    if (response2.StatusCode == HttpStatusCode.OK)
                    {
                        string messagereply = "";
                        try
                        {
                            messagereply = "Dear " + CustCheckUtils.GetName(mobile) + "," + "\n";

                            messagereply += " You have successfully paid NPR " + validTransactionData.Amount
                                            + " to " + " account name: " + account + ". " +
                                            //GetMerchantName 
                                            "Utility payment for Dish Home Direct (Pinless)." + " on date " +
                                                (validTransactionData.CreatedDate).ToString("dd/MM/yyyy")
                                            + "." + "\n";
                            messagereply += "Thank you. MNepal";

                            var client = new WebClient();

                            //SENDER
                            if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                            {
                                //FOR NCELL
                                var content = client.DownloadString(
                                    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                    + "977" + mobile + "&Text=" + messagereply + "");
                            }
                            else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                        || (mobile.Substring(0, 3) == "986"))
                            {
                                //FOR NTC
                                var content = client.DownloadString(
                                    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                    + "977" + mobile + "&Text=" + messagereply + "");
                            }

                            statusCode = "200";
                            var v = new
                            {
                                StatusCode = Convert.ToInt32(statusCode),
                                StatusMessage = result
                            };
                            result = JsonConvert.SerializeObject(v);

                        }
                        catch (Exception ex)
                        {
                            // throw ex
                            statusCode = "400";
                            message = ex.Message;
                        }


                        custsmsInfo = new CustActivityModel()
                        {
                            UserName = fundtransfer.mobile,
                            RequestMerchant = transactionType,
                            DestinationNo = fundtransfer.da,
                            Amount = validTransactionData.Amount.ToString(),
                            SMSStatus = "Success",
                            SMSSenderReply = messagereply,
                            ErrorMessage = "",
                        };


                    }
                    else if ((response2.StatusCode == HttpStatusCode.BadRequest) || (response2.StatusCode == HttpStatusCode.InternalServerError) || (statusCode != "200"))
                    {
                        custsmsInfo = new CustActivityModel()
                        {
                            UserName = mobile,
                            RequestMerchant = transactionType,
                            DestinationNo = fundtransfer.da,
                            Amount = validTransactionData.Amount.ToString(),
                            SMSStatus = "Failed",
                            SMSSenderReply = message,
                            ErrorMessage = failedmessage,
                        };

                    }
                }

            }
            catch (Exception ex)
            {
                // throw ex
                statusCode = "400";
                message = ex.Message;
            }
            if (statusCodeBalance == "400")
            {
                statusCode = "400";
            }
            if (statusCode == "")
            {
                result = result.ToString();
            }
            //else if (statusCode != "200")
            else if ((statusCode != "200") || (statusCodeBalance == "400"))
            {
                if (message == "")
                {
                    if (string.IsNullOrEmpty(result))
                    {
                        result = failedmessage; // "Could not process your request,Please Try again.";
                    }
                    message = result;
                }
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = failedmessage
                };
                result = JsonConvert.SerializeObject(v);
            }
            return result;
        }
        #endregion

        #region"Check UTL Payment"
        [OperationContract]
        [WebInvoke(Method = "POST",
                  ResponseFormat = WebMessageFormat.Json)]
        public string UTLcheckpayment(Stream input)
        {
            string PaypointPwd = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointPwd"];
            string PaypointUserID = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointUserID"];
            System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate { return true; }; //to prevent from SSL error
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            string tid = qs["tid"];
            string vid = qs["vid"];
            string sc = "00";
            string mobile = qs["mobile"];
            string da = System.Web.Configuration.WebConfigurationManager.AppSettings["DestinationNoForTestServer"];
            string note = "Utility payment for UTL. Mobile Number=" + qs["account"];
            string src = qs["src"];
            string result = "";
            string sessionID = qs["tokenID"];
            string companyCode = qs["companyCode"];
            string serviceCode = qs["serviceCode"];
            string account = qs["account"];
            string special1 = qs["special1"];
            string special2 = "";
            string transactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            long millisecondstrandId = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string transactionId = millisecondstrandId.ToString();
            string userId = PaypointUserID;
            string userPassword = PaypointPwd.Trim();
            string salePointType = "6";
            string ClientCode = qs["ClientCode"];
            string paypointType = qs["paypointType"];
            string transactionType = string.Empty;
            string resultMessageResCP = "";

            PaypointModel reqCPPaypointUTLInfo = new PaypointModel();//to store data request of CP which is also commmon in nea
            PaypointModel resCPPaypointUTLInfo = new PaypointModel();//to store data response of CP which is also commmon in nea

            //PaypointModel resPaypointPaymentInfo = new PaypointModel();
            PaypointModel resPaypointUTLPaymentInfo = new PaypointModel();//to store data of Response of CP only

            string totalAmount = string.Empty;
            string totalCount = string.Empty;
            string totalBAmount = string.Empty;
            string totalBCount = string.Empty;
            string balance = string.Empty;
            string message = string.Empty;
            string statusCode = string.Empty;
            string failedmessage = string.Empty;
            string GetMerchantName = string.Empty;
            string retrievalRef = string.Empty;
            string refStanCK = string.Empty;

            string customerNo = string.Empty;
            TraceIdGenerator traceid = new TraceIdGenerator();
            tid = traceid.GenerateUniqueTraceID();

            List<Packages> pkg = new List<Packages>();

            //for CP transaction for nepal water
            try
            {
                string URI = System.Web.Configuration.WebConfigurationManager.AppSettings["CPPaypointUrl"];

                string myParameters = "companyCode=" + companyCode + "&serviceCode=" + serviceCode +
                    "&account=" + account + "&special1=" + (Convert.ToInt32(special1) * 100).ToString() + "&special2=" + special2 +
                    "&transactionDate=" + transactionDate + "&transactionId=" + transactionId +
                    "&userId=" + userId.Trim() + "&userPassword=" + userPassword.Trim() + "&salePointType=" + salePointType;

                //for checkpayment request insert in database
                reqCPPaypointUTLInfo = new PaypointModel()
                {
                    companyCodeReqCP = companyCode,
                    serviceCodeReqCP = serviceCode,
                    accountReqCP = account,
                    special1ReqCP = special1,
                    special2ReqCP = special2,

                    transactionDateReqCP = transactionDate,
                    transactionIdReqCP = transactionId,
                    userIdReqCP = userId,
                    userPasswordReqCP = userPassword,
                    salePointTypeReqCP = salePointType,

                    refStanReqCP = "",
                    amountReqCP = "",//amountInPaisa
                    billNumberReqCP = "",
                    //retrievalReferenceReqCP = fundtransfer.tid,
                    retrievalReferenceReqCP = tid,
                    remarkReqCP = "Check Payment",
                    UserName = mobile,
                    ClientCode = ClientCode,
                    paypointType = paypointType,

                };

                string billNumber = "";
                string amountpay = "";
                string refStan = "";
                string exectransactionId = ""; //Unique
                string exectransactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"); //Current DateTime
                string rltCheckPaymt = "";
                string customerName = "";
                string mask = "";

                using (WebClient wc = new WebClient())
                {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    var HtmlResult = wc.UploadString(URI, myParameters);// response from checkpayment

                    XmlDocument xmlDoc = new XmlDocument();

                    xmlDoc.LoadXml(HtmlResult);

                    XmlNodeList test = xmlDoc.GetElementsByTagName("*");
                    string results = test[0].InnerText;
                    string HtmlResult1 = results;

                    //for getting key value from check payment
                    var reader = new StringReader(HtmlResult1);
                    var xdoc = XDocument.Load(reader);

                    XDocument docParse = XDocument.Parse(xdoc.ToString());
                    IEnumerable<XElement> responses = docParse.Descendants();

                    var xElem = XElement.Parse(docParse.ToString());

                    rltCheckPaymt = xElem.Attribute("Result").Value;
                    string keyrlt = "";// response.Attribute("Key").Value;
                    var billAmount = "";

                    message = resultMessageResCP;
                    if (xElem.Attribute("Result").Value == "000")
                    {
                        keyrlt = xElem.Attribute("Key").Value;
                        billNumber = xElem.Descendants().Elements("BillNumber").Where(x => x.Name == "BillNumber").SingleOrDefault().Value;
                        amountpay = xElem.Descendants().Elements("Amount").Where(x => x.Name == "Amount").SingleOrDefault().Value;
                        refStan = xElem.Descendants().Elements("RefStan").Where(x => x.Name == "RefStan").SingleOrDefault().Value;
                        exectransactionDate = xElem.Descendants().Elements("DueDate").Where(x => x.Name == "DueDate").SingleOrDefault().Value;
                        customerName = xElem.Descendants().Elements("ReserveInfo").Where(x => x.Name == "ReserveInfo").SingleOrDefault().Value;
                        mask = xElem.Descendants().Elements("mask").Where(x => x.Name == "mask").SingleOrDefault().Value;
                        if (amountpay == "0")
                        {
                            billAmount = special1;
                        }
                        else
                        {
                            billAmount = amountpay;
                        }

                        Packages packages = new Packages();
                        if (mask == "0" || mask == "6")
                        {

                            var commission = xElem.Descendants("BillParam").SingleOrDefault();
                            //var packageList = package.Descendants("package").ToList();

                            XmlDocument xmlDoc1 = new XmlDocument();
                            xmlDoc1.LoadXml(commission.ToString());

                            XmlNodeList xmlNodeList = xmlDoc1.SelectNodes("/BillParam/commission");

                            string stringBuilderDescriptions = "";
                            string stringBuilderCommission = "";
                            foreach (XmlNode xmlNode in xmlNodeList)
                            {
                                packages.Description = xmlNode.OuterXml; /*xmlNode.InnerText;*/
                                packages.commissionAmount = xmlNode.Attributes["val"].Value;
                                pkg.Add(packages);
                                stringBuilderDescriptions = stringBuilderDescriptions + packages.Description + Environment.NewLine;
                                stringBuilderCommission = packages.commissionAmount;

                            }

                            resPaypointUTLPaymentInfo = new PaypointModel()
                            {
                                billNumber = billNumber,
                                refStan = refStan,
                                amount = billAmount,
                                description = stringBuilderDescriptions,
                                commissionAmount = stringBuilderCommission,
                                transactionDate = exectransactionDate,
                                customerName = account,
                                companyCode = companyCode,
                                UserName = mobile,
                                ClientCode = ClientCode,
                                serviceCode = serviceCode,
                                paypointType = paypointType

                            };

                        }

                        //end list of package

                        int resultsPayments = PaypointUtils.PaypointUtilityUTLInfo(resPaypointUTLPaymentInfo);
                    }
                    else
                    {
                        keyrlt = xElem.Attribute("Key").Value;
                        resultMessageResCP = xElem.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;
                    }
                    resultMessageResCP = xElem.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;

                    //for checkpayment response insert in database in nepal water
                    resCPPaypointUTLInfo = new PaypointModel()
                    {
                        companyCodeResCP = companyCode,
                        serviceCodeResCP = serviceCode,
                        accountResCP = account,
                        special1ResCP = special1,
                        special2ResCP = special2,

                        transactionDateResCP = transactionDate,
                        transactionIdResCP = transactionId,
                        userIdResCP = userId,
                        userPasswordResCP = userPassword,
                        salePointTypeResCP = salePointType,

                        refStanResCP = refStan,
                        amountResCP = amountpay,
                        billNumberResCP = billNumber,
                        //retrievalReferenceResCP = fundtransfer.tid,
                        retrievalReferenceResCP = tid,
                        responseCodeResCP = rltCheckPaymt,
                        descriptionResCP = "Check Payment " + keyrlt,
                        customerNameCP = customerName,
                        UserName = mobile,
                        ClientCode = ClientCode,
                        paypointType = paypointType,
                        resultMessageResCP = resultMessageResCP,

                    };
                }


                if (!(rltCheckPaymt == "000"))//show error when CP response is not 000 
                {
                    statusCode = "400";
                    // message = result;
                    message = resultMessageResCP;
                    failedmessage = message;

                }
                else
                {
                    statusCode = "200";
                    message = resultMessageResCP;
                    failedmessage = message;
                }
            }
            catch (Exception ex)
            {
                message = result + ex + "Error Message ";
                //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                statusCode = "400";
                // failedmessage = message;
                failedmessage = resultMessageResCP;

            }

            ///for  inserting CP PayPoint Data of Nepal Water which is simlar to nea 

            try
            {
                int resultsReqCP = PaypointUtils.RequestCPPaypointInfo(reqCPPaypointUTLInfo);
                int resultsResCP = PaypointUtils.ResponseCPPaypointInfo(resCPPaypointUTLInfo);


                if ((resultsReqCP > 0) && (resultsResCP > 0))

                {
                    message = result;
                }
                else
                {
                    message = result;
                }

            }
            catch (Exception ex)
            {
                string ss = ex.Message;
                message = result;
            }

            if (statusCode == "")
            {
                result = result.ToString();
            }
            else if (statusCode == "200")
            {
                if (message == "")
                {
                    if (string.IsNullOrEmpty(result))
                    {
                        result = failedmessage; // "Could not process your request,Please Try again.";
                    }
                    message = result;
                }
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = failedmessage,
                    retrievalRef = resCPPaypointUTLInfo.retrievalReferenceResCP,
                    refStanCK = resCPPaypointUTLInfo.refStanResCP
                };
                result = JsonConvert.SerializeObject(v);
            }
            else if (statusCode != "200")
            {
                if (message == "")
                {
                    if (string.IsNullOrEmpty(result))
                    {
                        result = failedmessage; // "Could not process your request,Please Try again.";
                    }
                    message = result;
                }
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = failedmessage,
                    retrievalRef = "",
                    refStanCK = ""
                };
                result = JsonConvert.SerializeObject(v);
            }
            return result;

        }
        #endregion

        static void BackgroundTaskWithObject(Object stateInfo)
        {
            FundTransfer data = (FundTransfer)stateInfo;
            Console.WriteLine($"Hi {data.tid} from ThreadPool.");
            Thread.Sleep(1000);
        }


    }
}
