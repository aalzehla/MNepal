using MNepalProject.Connection;
using MNepalProject.Controllers;
using MNepalProject.Helper;
using MNepalProject.Models;
using MNepalProject.Services;
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
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
    public class subisu
    {
        // To use HTTP GET, add [WebGet] attribute. (Default ResponseFormat is WebMessageFormat.Json)
        // To create an operation that returns XML,
        //     add [WebGet(ResponseFormat=WebMessageFormat.Xml)],
        //     and include the following line in the operation body:
        //         WebOperationContext.Current.OutgoingResponse.ContentType = "text/xml";
        #region"Check Paypoint NepalWater"
        [OperationContract]
        [WebInvoke(Method = "POST",
                  ResponseFormat = WebMessageFormat.Json)]
        public string checkpayment(Stream input)
        {
            string PaypointPwd = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointPwd"];
            string PaypointUserID = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointUserID"];
            System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate { return true; }; //to prevent from SSL error
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            string tid = qs["tid"];
            string vid = qs["vid"]; //MerchantID
            //string sc = qs["sc"];
            string sc = "00";
            string mobile = qs["mobile"];

            //string da = "9840066836";//merchant in 30


            string da = System.Web.Configuration.WebConfigurationManager.AppSettings["DestinationNoForTestServer"];
            string note = "utility payment for subisu. Customer Name=" + qs["account"];//+ ". " + qs["note"];
            string src = qs["src"];
            string result = "";
            string sessionID = qs["tokenID"];
            string resultMessageResCP = "";

            string companyCode = qs["companyCode"];
            string serviceCode = qs["serviceCode"];
            string account = qs["account"];
            string special1 = qs["special1"];
            string special2 = "";
            string transactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");//"2019-11-22T11:11:02";
            long millisecondstrandId = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string transactionId = millisecondstrandId.ToString(); //"120163339819";
            string userId = PaypointUserID;
            //string userId = "MNepalLT";
            //string userPassword = "MNepalLT";
            string userPassword = PaypointPwd.Trim();
            string salePointType = "6";
            string ClientCode = qs["ClientCode"];
            //string paypointType = "NepalWater";//qs["paypointType"]
            string paypointType = "Subisu";//qs["paypointType"]
            string transactionType = string.Empty;

            PaypointModel reqCPPaypointSubisuInfo = new PaypointModel();
            PaypointModel resCPPaypointSubisuInfo = new PaypointModel();


            PaypointModel resPaypointSubisuPaymentInfo = new PaypointModel();//to store data of Response of CP only

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
                //for checkpayment link 
                //string URI = "https://test.paypoint.md:4445/PayPointWS/PayPointMSOperations.asmx/CheckPayment";

                //For checkpaypoint link in webconfig
                string URI = System.Web.Configuration.WebConfigurationManager.AppSettings["CPPaypointUrl"];

                string myParameters = "companyCode=" + companyCode + "&serviceCode=" + serviceCode +
                    "&account=" + account + "&special1=" + special1 + "&special2=" + special2 +
                    "&transactionDate=" + transactionDate + "&transactionId=" + transactionId +
                    "&userId=" + userId.Trim() + "&userPassword=" + userPassword.Trim() + "&salePointType=" + salePointType;

                //for checkpayment request insert in database
                reqCPPaypointSubisuInfo = new PaypointModel()
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

                using (WebClient wc = new WebClient())
                {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    var HtmlResult = wc.UploadString(URI, myParameters);// response from checkpayment

                    //                        string HtmlResult = @"<string xmlns=""http://tempuri.org/""><PPResponse Result=""000"" Key=""93eb6e47-2ab1-41f8-b6e8-b93e99fd1be5""><ResultMessage>Operation is succesfully completed </ResultMessage>
                    //<UtilityInfo><UtilityCode> 598 </UtilityCode></UtilityInfo><BillInfo><Bill><BillNumber> 93eb6e47-2ab1-41f8-b6e8-b93e99fd1be5 </BillNumber>
                    //<DueDate> 2019-11-26T07:13:38 </DueDate><Amount> 13900 </Amount><ReserveInfo> ISHWORI PD.SHRESTHA </ReserveInfo><BillParam><mask> 1 </mask>
                    //                        <commission type=""0"" val=""0.00"" op=""-"" paysource=""1""></commission></BillParam>" +
                    //                            "<RefStan> 21265641797314 </RefStan></Bill></BillInfo></PPResponse></string>";

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




                        //for checkpayment payaments response insert in database for nepal water

                        resPaypointSubisuPaymentInfo = new PaypointModel()
                        {
                            companyCodeReqCP = companyCode,
                            serviceCodeReqCP = serviceCode,
                            accountReqCP = account,
                            special1ReqCP = special1,
                            special2ReqCP = special2,
                            customerNameCP = qs["account"],
                            transactionDateReqCP = transactionDate,
                            transactionIdReqCP = transactionId,
                            userIdReqCP = userId,
                            userPasswordReqCP = userPassword,
                            salePointTypeReqCP = salePointType,
                            companyCodeCP = qs["companyCode"],

                            refStanReqCP = refStan,
                            amountReqCP = (Convert.ToInt32(amountpay) / 100).ToString(), //amount in paisa
                            billNumberReqCP = billNumber,
                            //retrievalReferenceReqCP = fundtransfer.tid,
                            retrievalReferenceReqCP = tid,
                            remarkReqCP = "Check Payment",
                            UserName = mobile,
                            ClientCode = ClientCode,
                            paypointType = paypointType
                        };

                        int resultsPayments = PaypointUtils.PaypointSubisuInfo(resPaypointSubisuPaymentInfo);

                    }

                    else
                    {
                        keyrlt = xElem.Attribute("Key").Value;
                        resultMessageResCP = xElem.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;
                    }
                    resultMessageResCP = xElem.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;

                    //for checkpayment response insert in database in subisu
                    resCPPaypointSubisuInfo = new PaypointModel()
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
                        amountResCP = (Convert.ToInt32(amountpay) / 100).ToString(),
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

            ///for  inserting CP PayPoint Data of  subisu 

            try
            {
                int resultsReqCP = PaypointUtils.RequestCPPaypointInfo(reqCPPaypointSubisuInfo);
                int resultsResCP = PaypointUtils.ResponseCPPaypointInfo(resCPPaypointSubisuInfo);


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
                    retrievalRef = resCPPaypointSubisuInfo.retrievalReferenceResCP,
                    refStanCK = resCPPaypointSubisuInfo.refStanResCP
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

        #region"execute Paypoint"
        [OperationContract]
        [WebInvoke(Method = "POST",
                  ResponseFormat = WebMessageFormat.Json)]
        public string executepayment(Stream input)
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
            string note = "utility payment for Subisu. Customer Name=" + qs["account"];//+ ". " + qs["note"];
            string src = qs["src"];
            string result = "";
            string sessionID = qs["tokenID"];
            string resultMessageResCP = "";

            string companyCode = qs["companyCode"]; //"720";//
                                                    //string serviceCode = qs["special1"]; //"1";// 
            string serviceCode = qs["serviceCode"]; //"11";// 
                                                    // string serviceCode = qs["special1s"]; //"11";//
            string account = qs["account"]; //"1234567";//            
            string special1 = qs["special1"]; //
            string special2 = ""; //
            string transactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");//"2019-11-22T11:11:02";
            long millisecondstrandId = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string transactionId = millisecondstrandId.ToString(); //"120163339819";
            string userId = PaypointUserID;
            //string userId = "MNepalLT";
            //string userPassword = "MNepalLT";
            string userPassword = PaypointPwd.Trim();
            string salePointType = "6";
            string transactionType = string.Empty;

            string amountpay = qs["amountpay"];//amount need to pay i.e amount in bill
            string exectransactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"); ;
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
            int amountpayInt = Convert.ToInt32(amountpay);
            PaypointModel reqEPPaypointSubisuInfo = new PaypointModel();
            PaypointModel resEPPaypointSubisuInfo = new PaypointModel();

            PaypointModel reqGTPaypointSubisuInfo = new PaypointModel();
            PaypointModel resGTPaypointSubisuInfo = new PaypointModel();

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

                            //if (!(UserNameCheck.IsValidUser(mobile)))
                            //{
                            //    // throw ex
                            //    statusCode = "400";
                            //    //message = "Transaction restricted to User";
                            //    message = "Transaction only for User";
                            //    failedmessage = message;
                            //}

                            Pin p = new Pin();
                            if (!p.validPIN(mobile, pin))
                            {
                                statusCode = "400";
                                message = "Invalid PIN";
                                failedmessage = message;

                                LoginUtils.SetPINTries(mobile, "BUWP");//add +1 in trypwd

                                if (LoginUtils.GetPINBlockTime(mobile)) //check if blocktime is greater than current time 
                                {
                                    message = LoginUtils.GetMessage("01");
                                    //message = "Invalid PIN! You have already attempt 3 times with wrong PIN,Please try again after 10 minutes";  //change
                                    statusCode = "417";
                                    MNFundTransfer mnlg = new MNFundTransfer();
                                    mnlg.ResponseStatus(HttpStatusCode.ExpectationFailed, message);
                                    failedmessage = message;

                                }
                            }
                            else
                            {
                                LoginUtils.SetPINTries(mobile, "RPT");
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
                                        fundtransfer.sourcechannel,"Subisu");
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

                        //if (!(UserNameCheck.IsValidUser(mobile)))
                        //{
                        //    // throw ex
                        //    statusCode = "400";
                        //    //message = "Transaction restricted to User";
                        //    message = "Transaction only for User";
                        //    failedmessage = message;
                        //}

                        Pin p = new Pin();
                        if (!p.validPIN(mobile, pin))
                        {
                            statusCode = "400";
                            message = "Invalid PIN";
                            failedmessage = message;

                            LoginUtils.SetPINTries(mobile, "BUWP");//add +1 in trypwd

                            if (LoginUtils.GetPINBlockTime(mobile)) //check if blocktime is greater than current time 
                            {
                                message = LoginUtils.GetMessage("01");
                                //message = "Invalid PIN! You have already attempt 3 times with wrong PIN,Please try again after 10 minutes";
                                statusCode = "417";
                                MNFundTransfer mnlg = new MNFundTransfer();
                                mnlg.ResponseStatus(HttpStatusCode.ExpectationFailed, message);
                                failedmessage = message;
                            }

                        }
                        else
                        {
                            LoginUtils.SetPINTries(mobile, "RPT");
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
                                    fundtransfer.sourcechannel,"Subisu");
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
                                    //if (companyCode == "598")
                                    //{
                                    //    special1 = "";
                                    //}
                                    //else
                                    //{
                                    //    special1 = special1.ToString();
                                    //}

                                    // string URIEXECPayment = "https://test.paypoint.md:4445/PayPointWS/PayPointMSOperations.asmx/ExecutePayment";//for EP Link 

                                    //For excutepaypoint link in webconfig
                                    string URIEXECPayment = System.Web.Configuration.WebConfigurationManager.AppSettings["EPPaypointUrl"];

                                    string execPayParameters = "companyCode=" + companyCode + "&serviceCode=" + serviceCode +
                                            "&account=" + account + "&special1=" + special1 + "&special2=" + special2 +
                                            "&transactionDate=" + exectransactionDate + "&transactionId=" + exectransactionId +
                                            "&refStan=" + refStan + "&amount=" + (Convert.ToInt32(amount) * 100).ToString() + "&billNumber=" + billNumber +
                                            "&userId=" + userId.Trim() + "&userPassword=" + userPassword.Trim() + "&salePointType=" + salePointType;

                                    //for executepayment request insert in database for wlink
                                    reqEPPaypointSubisuInfo = new PaypointModel()
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
                                        resEPPaypointSubisuInfo = new PaypointModel()
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
                                int resultsReqEP = PaypointUtils.RequestEPPaypointInfo(reqEPPaypointSubisuInfo);
                                int resultsResEP = PaypointUtils.ResponseEPPaypointInfo(resEPPaypointSubisuInfo);

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
                                        reqGTPaypointSubisuInfo = new PaypointModel()
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
                                            resGTPaypointSubisuInfo = new PaypointModel()
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

                                        int resultsReqGTP = PaypointUtils.RequestGTPaypointInfo(reqGTPaypointSubisuInfo);
                                        int resultsResGTP = PaypointUtils.ResponseGTPaypointInfo(resGTPaypointSubisuInfo);

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

                        //if (!(UserNameCheck.IsValidUser(mobile)))
                        //{
                        //    // throw ex
                        //    statusCode = "400";
                        //    //message = "Transaction restricted to User";
                        //    message = "Transaction only for User";
                        //    failedmessage = message;
                        //}

                        Pin p = new Pin();
                        if (!p.validPIN(mobile, pin))
                        {
                            statusCode = "400";
                            message = "Invalid PIN ";
                            failedmessage = message;

                            if (LoginUtils.GetPINBlockTime(mobile)) //check if blocktime is greater than current time 
                            {
                                message = LoginUtils.GetMessage("01");
                                //message = "Invalid PIN! You have already attempt 3 times with wrong PIN,Please try again after 10 minutes";
                                statusCode = "417";
                                MNFundTransfer mnlg = new MNFundTransfer();
                                mnlg.ResponseStatus(HttpStatusCode.ExpectationFailed, message);
                                failedmessage = message;
                            }
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

                            if (LoginUtils.GetPINBlockTime(mobile)) //check if blocktime is greater than current time 
                            {
                                message = LoginUtils.GetMessage("01");
                                //  message = "Invalid PIN! You have already attempt 3 times with wrong PIN,Please try again after 10 minutes";
                                statusCode = "417";
                                MNFundTransfer mnlg = new MNFundTransfer();
                                mnlg.ResponseStatus(HttpStatusCode.ExpectationFailed, message);
                                failedmessage = message;
                            }

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
                                                                    "Utility payment for Subisu." + " on date " +
                                                                    (validTransactionData.CreatedDate).ToString("dd/MM/yyyy")
                                                                + "." + "\n";
                                                messagereply += "Thank you. NIBL";

                                                var client = new WebClient();

                                                //SENDER
                                                if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                                                {
                                                    //FOR NCELL
                                                    var content = client.DownloadString(
                                                        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                                        + "977" + mobile + "&message=" + messagereply + "");
                                                }
                                                else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                                            || (mobile.Substring(0, 3) == "986"))
                                                {
                                                    //FOR NTC
                                                    var content = client.DownloadString(
                                                        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                                        + "977" + mobile + "&message=" + messagereply + "");
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
                                            "Utility payment for Subisu." + " on date " +
                                                (validTransactionData.CreatedDate).ToString("dd/MM/yyyy")
                                            + "." + "\n";
                            messagereply += "Thank you. NIBL";

                            var client = new WebClient();

                            //SENDER
                            if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                            {
                                //FOR NCELL
                                var content = client.DownloadString(
                                    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                    + "977" + mobile + "&message=" + messagereply + "");
                            }
                            else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                        || (mobile.Substring(0, 3) == "986"))
                            {
                                //FOR NTC
                                var content = client.DownloadString(
                                    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                    + "977" + mobile + "&message=" + messagereply + "");
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
            ///START Register For SMS
            //try
            //{
            //    int results = CustActivityUtils.RegisterCustActivityInfo(custsmsInfo);
            //    if (results > 0)
            //    {

            //        message = result;
            //    }
            //    else
            //    {
            //        message = result;
            //    }

            //}
            //catch (Exception ex)
            //{
            //    string ss = ex.Message;
            //    message = result;
            //}

            ///END SMS Register For SMS

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

        static void BackgroundTaskWithObject(Object stateInfo)
        {
            FundTransfer data = (FundTransfer)stateInfo;
            Console.WriteLine($"Hi {data.tid} from ThreadPool.");
            Thread.Sleep(1000);
        }
    }
}
