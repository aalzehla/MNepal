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
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using WCF.MNepal.ErrorMsg;
using WCF.MNepal.Helper;
using WCF.MNepal.Models;

using WCF.MNepal.UserModels;
using WCF.MNepal.Utilities;
namespace WCF.MNepal
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
    public class paypointnepalwater
    {
        [OperationContract]
        [WebInvoke(Method = "POST",
                  ResponseFormat = WebMessageFormat.Json)]
        #region"check ,execute,get transaction yautao ma bhayako"
        public string request(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            string tid = qs["tid"];
            string vid = qs["vid"]; //MerchantID
            string sc = qs["sc"];
            string mobile = qs["mobile"];
            string da = qs["da"];
            string amount = qs["amount"];
            string pin = qs["pin"];
            string note = qs["note"];
            string src = qs["src"];
            string result = "";
            string sessionID = qs["tokenID"];
            string resultMessageResCP = "";

            string companyCode = qs["companyCode"]; //"598";//
            string serviceCode = qs["serviceCode"]; //"11";// 
            string account = qs["account"]; //"1234567";//
            string special1 = qs["special1"]; //"";//
            string special2 = qs["special2"]; //"";// 
            string transactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");//"2019-11-22T11:11:02";
            long millisecondstrandId = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string transactionId = millisecondstrandId.ToString(); //"120163339819";
            string userId = "MNepalLT";
            string userPassword = "MNepalLT"; // qs["tokenID"];
            string salePointType = "6"; // qs["tokenID"];
            string ClientCode = qs["ClientCode"];
            string paypointType = "Nepal Water";
            string transactionType = string.Empty;

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

            ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, fundtransfer);
            CustActivityModel custsmsInfo = new CustActivityModel();

            PaypointModel reqCPPaypointInfo = new PaypointModel();
            PaypointModel resCPPaypointInfo = new PaypointModel();

            PaypointModel reqEPPaypointInfo = new PaypointModel();
            PaypointModel resEPPaypointInfo = new PaypointModel();

            PaypointModel reqGTPaypointInfo = new PaypointModel();
            PaypointModel resGTPaypointInfo = new PaypointModel();

            MNTransactionMaster validTransactionData = new MNTransactionMaster();

            string totalAmount = string.Empty;
            string totalCount = string.Empty;
            string totalBAmount = string.Empty;
            string totalBCount = string.Empty;
            string balance = string.Empty;
            string message = string.Empty;
            string statusCode = string.Empty;
            string failedmessage = string.Empty;
            string GetMerchantName = string.Empty;

            string customerNo = string.Empty;
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
                                var transaction = new MNTransactionMaster(mnft);
                                var mntransaction = new MNTransactionsController();
                                validTransactionData = mntransaction.Validate(transaction, mnft.pin);
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

                                        ///PayPont Integration For REquest and REsponse

                                        try
                                        {
                                            string amountInPaisa = (Convert.ToInt32(amount) * 100).ToString();

                                            //for checkpayment
                                            string URI = "https://test.paypoint.md:4445/PayPointWS/PayPointMSOperations.asmx/CheckPayment";
                                            string myParameters = "companyCode=" + companyCode + "&serviceCode=" + serviceCode +
                                                "&account=" + account + "&special1=" + special1 + "&special2=" + special2 +
                                                "&transactionDate=" + transactionDate + "&transactionId=" + transactionId +
                                                "&userId=" + userId + "&userPassword=" + userPassword + "&salePointType=" + salePointType;

                                            //for checkpayment request insert in database
                                            reqCPPaypointInfo = new PaypointModel()
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
                                                amountReqCP = amountInPaisa,
                                                billNumberReqCP = "",
                                                retrievalReferenceReqCP = fundtransfer.tid,
                                                remarkReqCP = "Check Payment",

                                            };

                                            string billNumber = "";
                                            string amountpay = "";
                                            string refStan = "";
                                            string exectransactionId = ""; //Unique
                                            string exectransactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"); //Current DateTime
                                            string rltCheckPaymt = "";

                                            using (WebClient wc = new WebClient())
                                            {
                                                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                                                var HtmlResult = wc.UploadString(URI, myParameters);

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
                                                }
                                                else
                                                {
                                                    keyrlt = xElem.Attribute("Key").Value;
                                                    resultMessageResCP = xElem.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;
                                                }

                                                //for checkpayment response insert in database
                                                resCPPaypointInfo = new PaypointModel()
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
                                                    amountResCP = amountInPaisa, //amountpay,
                                                    billNumberResCP = billNumber,
                                                    retrievalReferenceResCP = fundtransfer.tid,
                                                    responseCodeResCP = rltCheckPaymt,
                                                    descriptionResCP = "Check Payment " + keyrlt,

                                                };
                                                //END CheckPayment Response

                                            }

                                            if ((rltCheckPaymt == "000"))
                                            {
                                                if (amountpay != "0")
                                                {
                                                    int amountInPaisaInt = Convert.ToInt32(amountInPaisa);
                                                    int amountpayInt = Convert.ToInt32(amountpay);

                                                    if (amountInPaisaInt > amountpayInt)
                                                    {

                                                        amountpay = (Convert.ToInt32(amount) * 100).ToString();

                                                        long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                                                        exectransactionId = milliseconds.ToString();

                                                        string compResultResp = "";
                                                        string keyExecRlt = "";

                                                        do
                                                        {
                                                            if ((companyCode == null) || (serviceCode == null) || (account == null) ||
                                                            (exectransactionDate == null) || (exectransactionId == null) ||
                                                            (refStan == null) || (amountInPaisa == null) || (billNumber == null) ||
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

                                                                string URIEXECPayment = "https://test.paypoint.md:4445/PayPointWS/PayPointMSOperations.asmx/ExecutePayment";
                                                                string execPayParameters = "companyCode=" + companyCode + "&serviceCode=" + serviceCode +
                                                                        "&account=" + account + "&special1=" + special1 + "&special2=" + special2 +
                                                                        "&transactionDate=" + exectransactionDate + "&transactionId=" + exectransactionId +
                                                                        "&refStan=" + refStan + "&amount=" + amountInPaisa + "&billNumber=" + billNumber +
                                                                        "&userId=" + userId + "&userPassword=" + userPassword + "&salePointType=" + salePointType;

                                                                //for executepayment request insert in database
                                                                reqEPPaypointInfo = new PaypointModel()
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
                                                                    amountReqEP = amountInPaisa,
                                                                    billNumberReqEP = billNumber,
                                                                    retrievalReferenceReqEP = fundtransfer.tid,
                                                                    remarkReqEP = "Execute Payment",

                                                                };
                                                                //END ExecutePayment Request

                                                                using (WebClient wcExecPay = new WebClient())
                                                                {
                                                                    wcExecPay.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                                                                    string HtmlResultExecPay = wcExecPay.UploadString(URIEXECPayment, execPayParameters);

                                                                    XmlDocument xmlEDoc = new XmlDocument();
                                                                    xmlEDoc.LoadXml(HtmlResultExecPay);

                                                                    XmlNodeList nodeEPay = xmlEDoc.GetElementsByTagName("*");
                                                                    string resultEPay = nodeEPay[0].InnerText;
                                                                    string HtmlEPayResult = resultEPay;

                                                                    //start for key check
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
                                                                    //end for key check

                                                                    //Start Response Execute Payment
                                                                    resEPPaypointInfo = new PaypointModel()
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
                                                                        amountResEP = amountInPaisa,
                                                                        billNumberResEP = billNumber,
                                                                        retrievalReferenceResEP = fundtransfer.tid,
                                                                        responseCodeResEP = compResultResp,
                                                                        descriptionResEP = "Execute Payment" + keyExecRlt,
                                                                    };
                                                                    //End Response Execute Payment

                                                                }


                                                            }

                                                        } while ((compResultResp == "011") || (compResultResp == "012"));


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
                                                                string key = "";
                                                                string gtBillNumber = "";
                                                                string URIGetTran = "https://test.paypoint.md:4445/PayPointWS/PayPointMSOperations.asmx/GetTransaction";
                                                                //string GetTranParameters = "userLogin=" + userId + "&userPassword=" + userPassword + "&stan=" + "-1&refStan=" + refStan + "&key=" + key + "&billNumber=" + gtBillNumber;
                                                                string GetTranParameters = "userLogin=" + userId + "&userPassword=" + userPassword + "&stan=" + "-1&refStan=" + refStan + "&key=" + "" + "&billNumber=" + gtBillNumber;


                                                                //for get transaction payment request insert in database
                                                                reqGTPaypointInfo = new PaypointModel()
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
                                                                    amountReqGTP = amountInPaisa,
                                                                    billNumberReqGTP = gtBillNumber,
                                                                    ////////////////////////////////////////////////////////////////////////////////////////////
                                                                    retrievalReferenceReqGTP = fundtransfer.tid,
                                                                    remarkReqGTP = "Get Transaction Payment",
                                                                    //remarkReqGTP = "Get Transaction Payment"+keyExecRlt,


                                                                };
                                                                string getTranResultResp = "";
                                                                string keyGetTrancRlt = "";

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

                                                                    //start for key check
                                                                    var readerEPay = new StringReader(HtmlEPayResult);
                                                                    var xdocEPay = XDocument.Load(readerEPay);

                                                                    XDocument docEPay = XDocument.Parse(xdocEPay.ToString());
                                                                    var xElemGPay = XElement.Parse(docEPay.ToString());
                                                                    if (xElemGPay.Attribute("Result").Value == "000")
                                                                    {
                                                                        getTranResultResp = xElemGPay.Attribute("Result").Value;
                                                                        keyGetTrancRlt = xElemGPay.Attribute("Key").Value;
                                                                        billNumberResGTP = xElemGPay.Descendants().Elements("BillNumber").Where(x => x.Name == "BillNumber").SingleOrDefault().Value;

                                                                    }
                                                                    //end for key check


                                                                    ////for get transaction payment response insert in database
                                                                    resGTPaypointInfo = new PaypointModel()
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
                                                                        amountResGTP = amountInPaisa, //amountpay,
                                                                        billNumberResGTP = billNumberResGTP,
                                                                        retrievalReferenceResGTP = fundtransfer.tid,
                                                                        responseCodeResGTP = getTranResultResp,
                                                                        descriptionResGTP = "Get Transaction Payment " + keyGetTrancRlt,

                                                                    };
                                                                }

                                                            }

                                                        }

                                                        //if (compResultResp == "000")
                                                        //{
                                                        //    statusCode = "200";
                                                        //    message = result;
                                                        //    mnft.ResponseStatus(HttpStatusCode.OK, message); //200 - OK
                                                        //}
                                                        else
                                                        {
                                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                            statusCode = "400";
                                                            message = result;
                                                            failedmessage = message;
                                                        }


                                                    }
                                                    else //Error input amount not suffient then the response amount of CP
                                                    {
                                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                        statusCode = "400";
                                                        message = "Insufficient Funds";
                                                        failedmessage = message;
                                                    }

                                                }
                                                else //Error  amount 0 in response of CP
                                                {
                                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                    statusCode = "400";

                                                    message = "You have no pending bill right now";
                                                    failedmessage = message;
                                                }
                                            }
                                            else //Error  in result of CP
                                            {
                                                //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                                statusCode = "400";
                                                // message = result;
                                                message = resultMessageResCP;

                                                failedmessage = message;

                                                //start:Com focus one log///
                                                MNFundTransfer mnft1 = new MNFundTransfer(tid, fundtransfer.sc, fundtransfer.da,
                                                    fundtransfer.sa, fundtransfer.amount, fundtransfer.mobile, fundtransfer.note, fundtransfer.pin,
                                                    fundtransfer.sourcechannel);
                                                var comfocuslog1 = new MNComAndFocusOneLog(mnft1, DateTime.Now);
                                                var mncomfocuslog1 = new MNComAndFocusOneLogsController();
                                                result = mncomfocuslog1.InsertIntoComFocusOne(comfocuslog1);
                                                //end:Com focus one log//



                                                if (mnft1.valid())
                                                {
                                                    var transaction1 = new MNTransactionMaster(mnft1);
                                                    var mntransaction1 = new MNTransactionsController();
                                                    validTransactionData = mntransaction1.Validate(transaction1, mnft1.pin);
                                                    result = validTransactionData.Response;

                                                }
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            message = result + ex + "Error Message ";
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                            statusCode = "400";
                                            failedmessage = message;
                                        }

                                        ///END PayPoint Integration For Request N Response


                                    } //END ValidTransactionData.ResponseCode


                                } //END validTransactionData.Response WITHOUT MNDB ERROR
                                  /*** ***/

                                OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                                if (response.StatusCode == HttpStatusCode.OK)
                                {
                                    string messagereply = "";
                                    try
                                    {
                                        messagereply = "Dear " + CustCheckUtils.GetName(mobile) + "," + "\n";

                                        messagereply += " You have successfully paid NPR " + validTransactionData.Amount
                                                        + " to " + GetMerchantName + " on date " +
                                                            (validTransactionData.CreatedDate).ToString("dd/MM/yyyy")
                                                        + "." + "\n";
                                        messagereply += "Thank you. NIBL Thaili";

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
                                else if ((response.StatusCode == HttpStatusCode.BadRequest) || (response.StatusCode == HttpStatusCode.InternalServerError) || (statusCode != "200"))
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

                //else
                //{

                //    if (checkMerchantDestinationUsertype(da))
                //    {
                //        // throw ex
                //        statusCode = "400";
                //        message = "Transaction restricted to Merchant";
                //        failedmessage = message;
                //    }
                //    else
                //    {
                //        //Agent check 
                //        if (!checkSourceAndDestinationUsertype(mobile, da))
                //        {
                //            // throw ex
                //            statusCode = "400";
                //            message = "Transaction restricted to Agent";
                //            failedmessage = message;
                //        }
                //        else
                //        {
                //            TransLimitCheck transLimitCheck = new TransLimitCheck();
                //            string resultTranLimit = transLimitCheck.LimitCheck(mobile, da, amount, sc, pin, src);

                //            var jsonDataResult = JObject.Parse(resultTranLimit);
                //            statusCode = jsonDataResult["StatusCode"].ToString();
                //            string statusMsg = jsonDataResult["StatusMessage"].ToString();
                //            message = jsonDataResult["StatusMessage"].ToString();
                //            failedmessage = message;

                //            if ((statusCode == "200") && (message == "Success"))
                //            {
                //                //start: checking trace id
                //                do
                //                {
                //                    TraceIdGenerator traceid = new TraceIdGenerator();
                //                    tid = traceid.GenerateUniqueTraceID();
                //                    fundtransfer.tid = tid;

                //                    bool traceIdCheck = false;
                //                    traceIdCheck = TraceIdCheck.IsValidTraceId(fundtransfer.tid);
                //                    if (traceIdCheck == true)
                //                    {
                //                        result = "Trace ID Repeated";
                //                    }
                //                    else
                //                    {
                //                        result = "false";
                //                    }

                //                } while (result == "Trace ID Repeated");
                //                //End: TraceId

                //                //start:Com focus one log///
                //                MNFundTransfer mnft = new MNFundTransfer(tid, fundtransfer.sc, fundtransfer.mobile,
                //                    fundtransfer.sa, fundtransfer.amount, fundtransfer.da, fundtransfer.note, fundtransfer.pin,
                //                    fundtransfer.sourcechannel);
                //                var comfocuslog = new MNComAndFocusOneLog(mnft, DateTime.Now);
                //                var mncomfocuslog = new MNComAndFocusOneLogsController();
                //                mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                //                //end:Com focus one log//


                //                //NOTE:- may be need to validate before insert into reply typpe
                //                //start:insert into reply type as HTTP//
                //                var replyType = new MNReplyType(tid, "HTTP");
                //                var mnreplyType = new MNReplyTypesController();
                //                mnreplyType.InsertIntoReplyType(replyType);
                //                //end:insert into reply type as HTTP//

                //                //MNTransactionMaster validTransactionData = new MNTransactionMaster();

                //                //start:insert into transaction master//
                //                if (mnft.valid())
                //                {
                //                    var transaction = new MNTransactionMaster(mnft);
                //                    var mntransaction = new MNTransactionsController();
                //                    validTransactionData = mntransaction.Validate(transaction, mnft.pin);
                //                    result = validTransactionData.Response;
                //                    /*** ***/
                //                    ErrorMessage em = new ErrorMessage();

                //                    if (validTransactionData.Response == "Error")
                //                    {
                //                        mnft.Response = "error";
                //                        mnft.ResponseStatus(HttpStatusCode.InternalServerError,
                //                            "Internal server error - try again later, or contact support");
                //                        result = mnft.Response;
                //                        statusCode = "500";
                //                        message = "Internal server error - try again later, or contact support";
                //                        failedmessage = message;
                //                    }
                //                    else
                //                    {
                //                        if ((result == "Trace ID Repeated") || (result == "Limit Exceed")
                //                            || (result == "Invalid Source User") || (result == "Invalid Destination User")
                //                            || (result == "Invalid Product Request") || (result == "Please try again") || (result == ""))
                //                        {
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                //                            statusCode = "400";
                //                            message = result;
                //                            failedmessage = message;
                //                        }
                //                        if (result == "Invalid PIN")
                //                        {
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                //                            statusCode = "400";
                //                            message = result;
                //                            failedmessage = message;
                //                        }
                //                        if (result == "111")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_111;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "114")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_114;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "115")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_115;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "116")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_116;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "119")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_119;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "121")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_121;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "163")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_163;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "180")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_180;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "181")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_181;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "182")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_182;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "183")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_183;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "184")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_184;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "185")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_185;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "186")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_186;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "187")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_187;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "188")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_188;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "189")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_189;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "190")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_190;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "800")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_800;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "902")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_902;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "904")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_904;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "906")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_906;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "907")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_907;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "909")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_909;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "911")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_911;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "913")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_913;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "90")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_90;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "91")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_91;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "92")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_92;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "94")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_94;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "95")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_95;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "98")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_98;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        if (result == "99")
                //                        {
                //                            statusCode = result;
                //                            message = em.Error_99;
                //                            failedmessage = message;
                //                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                        }
                //                        else if (validTransactionData.ResponseCode == "OK")
                //                        {
                //                            statusCode = "200";
                //                            message = result;
                //                            mnft.ResponseStatus(HttpStatusCode.OK, message);
                //                        }


                //                    }
                //                    /*** ***/

                //                    OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                //                    if (response.StatusCode == HttpStatusCode.OK)
                //                    {

                //                        string messagereplyDest = "";
                //                        string messagereply = "";

                //                        if (sc == "00")
                //                        {
                //                            transactionType = "W2W";

                //                            messagereplyDest = "Dear " + CustCheckUtils.GetName(da) + "," + "\n";
                //                            messagereplyDest +=
                //                                "You have received NPR " + //transactiontype + " transaction was successful with amount NPR " +
                //                                validTransactionData.Amount + " in your Wallet from " + mobile + " on date " +
                //                                (validTransactionData.CreatedDate).ToString("dd/MM/yyyy") +
                //                                "." + "\n";
                //                            messagereplyDest += "Thank you, MNepal";
                //                        }
                //                        else if (sc == "01")
                //                        {
                //                            transactionType = "W2B";

                //                            messagereplyDest = "Dear " + CustCheckUtils.GetName(da) + "," + "\n";
                //                            messagereplyDest +=
                //                            "Your bank account has been deposited by NPR " + //transactiontype + " transaction was successful with amount NPR " +
                //                            validTransactionData.Amount + " from " + mobile + " on date " +
                //                            (validTransactionData.CreatedDate).ToString("dd/MM/yyyy") +
                //                            "." + "\n";
                //                            messagereplyDest += "Thank you, MNepal";
                //                        }
                //                        else if (sc == "10")
                //                        {
                //                            transactionType = "B2W";

                //                            messagereplyDest = "Dear " + CustCheckUtils.GetName(da) + "," + "\n";

                //                            if (mobile == da)
                //                            {

                //                                messagereplyDest +=
                //                                "You have received NPR " + //transactiontype + " transaction was successful with amount NPR " +
                //                                validTransactionData.Amount + " from Bank A/C to your Wallet on date " +
                //                                (validTransactionData.CreatedDate).ToString("dd/MM/yyyy") +
                //                                "." + "\n";
                //                                messagereplyDest += "Thank you, MNepal";

                //                            }
                //                            else
                //                            {
                //                                messagereplyDest +=
                //                                    "You have received NPR " + //transactiontype + " transaction was successful with amount NPR " +
                //                                    validTransactionData.Amount + " from " + mobile + " to your Wallet on date " +
                //                                    (validTransactionData.CreatedDate).ToString("dd/MM/yyyy") +
                //                                    "." + "\n";
                //                                messagereplyDest += "Thank you, MNepal";
                //                            }
                //                        }
                //                        else if (sc == "11")
                //                        {
                //                            transactionType = "B2B";

                //                            messagereplyDest = "Dear " + CustCheckUtils.GetName(da) + "," + "\n";
                //                            messagereplyDest +=
                //                            "Your bank account has been deposited by NPR " + //transactiontype + " transaction was successful with amount NPR " +
                //                            validTransactionData.Amount + " from " + mobile +
                //                            " on date " + (validTransactionData.CreatedDate).ToString("dd/MM/yyyy") +
                //                            "." + "\n";
                //                            messagereplyDest += "Thank you, MNepal";
                //                        }

                //                        try
                //                        {
                //                            messagereply = "Dear " + CustCheckUtils.GetName(mobile) + "," + "\n";
                //                            //messagereply += transactiontype + " transaction was successful with amount NPR" +
                //                            //                validTransactionData.Amount + " on date " + validTransactionData.CreatedDate +
                //                            //                "." + "\n";
                //                            //messagereply += "You have send NPR " +
                //                            //                validTransactionData.Amount + " on date " +
                //                            //                validTransactionData.CreatedDate +
                //                            //                "." + "\n";
                //                            //messagereply += "Thank you, MNepal";

                //                            if (sc == "00") //W2W
                //                            {
                //                                messagereply += "You have successfully transferred NPR " + //send
                //                                            validTransactionData.Amount + " to " + da + " on date " +
                //                                            (validTransactionData.CreatedDate).ToString("dd/MM/yyyy") +
                //                                            "." + "\n";

                //                            }
                //                            else if (sc == "01") //W2B
                //                            {
                //                                messagereply += "You have successfully transferred NPR " + //send
                //                                            validTransactionData.Amount + " to " + da + " bank account on date " +
                //                                            (validTransactionData.CreatedDate).ToString("dd/MM/yyyy") +
                //                                            "." + "\n";
                //                            }
                //                            else if (sc == "10") //B2W
                //                            {
                //                                messagereply += "You have successfully transferred NPR " + //send
                //                                            validTransactionData.Amount + " from your bank account to "
                //                                            + da + " on date " +
                //                                            (validTransactionData.CreatedDate).ToString("dd/MM/yyyy") +
                //                                            "." + "\n";
                //                            }
                //                            else if (sc == "11") //B2B
                //                            {

                //                                messagereply += "You have successfully transferred NPR " + //send
                //                                                validTransactionData.Amount + " from your bank account to "
                //                                                + da + " bank account on date " +
                //                                                (validTransactionData.CreatedDate).ToString("dd/MM/yyyy") +
                //                                                "." + "\n";
                //                            }

                //                            messagereply += "Thank you, MNepal";

                //                            var client = new WebClient();
                //                            //var content = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=2&Password=test12test&From=9797&To=" + "977" + mobile + "&message=" + messagereply + "");
                //                            //SENDER
                //                            if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                //                            {
                //                                //FOR NCELL
                //                                var content = client.DownloadString(
                //                                    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                //                                    + "977" + mobile + "&message=" + messagereply + "");
                //                            }
                //                            else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                //                                        || (mobile.Substring(0, 3) == "986"))
                //                            {
                //                                //FOR NTC
                //                                var content = client.DownloadString(
                //                                    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                //                                    + "977" + mobile + "&message=" + messagereply + "");
                //                            }


                //                            //FOR DESTIONATION NUMBER RECEIVER
                //                            mobile = da;
                //                            if ((da.Substring(0, 3) == "980") || (da.Substring(0, 3) == "981")) //FOR NCELL
                //                            {
                //                                //FOR NCELL
                //                                var content = client.DownloadString(
                //                                    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                //                                    + "977" + da + "&message=" + messagereplyDest + "");
                //                            }
                //                            else if ((da.Substring(0, 3) == "985") || (da.Substring(0, 3) == "984")
                //                                        || (da.Substring(0, 3) == "986"))
                //                            {
                //                                //FOR NTC
                //                                var content = client.DownloadString(
                //                                    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                //                                    + "977" + da + "&message=" + messagereplyDest + "");
                //                            }

                //                            statusCode = "200";
                //                            var v = new
                //                            {
                //                                StatusCode = Convert.ToInt32(statusCode),
                //                                StatusMessage = result
                //                            };
                //                            result = JsonConvert.SerializeObject(v);
                //                        }
                //                        catch (Exception ex)
                //                        {
                //                            // throw ex
                //                            statusCode = "400";
                //                            message = ex.Message;
                //                        }


                //                        custsmsInfo = new CustActivityModel()
                //                        {
                //                            UserName = fundtransfer.mobile,
                //                            RequestMerchant = transactionType,
                //                            DestinationNo = fundtransfer.da,
                //                            Amount = validTransactionData.Amount.ToString(),
                //                            SMSStatus = "Success",
                //                            SMSSenderReply = messagereply,
                //                            ErrorMessage = "",
                //                        };


                //                    }
                //                    else if ((response.StatusCode == HttpStatusCode.BadRequest) || (response.StatusCode == HttpStatusCode.InternalServerError) || (statusCode != "200"))
                //                    {
                //                        custsmsInfo = new CustActivityModel()
                //                        {
                //                            UserName = mobile,
                //                            RequestMerchant = transactionType,
                //                            DestinationNo = fundtransfer.da,
                //                            Amount = validTransactionData.Amount.ToString(),
                //                            SMSStatus = "Failed",
                //                            SMSSenderReply = message,
                //                            ErrorMessage = failedmessage,
                //                        };

                //                    }
                //                    //end:insert into transaction master//

                //                }
                //                else
                //                {
                //                    mnft.Response = "error";
                //                    mnft.ResponseStatus(HttpStatusCode.BadRequest, "parameters missing/invalid"); //200 - OK
                //                    result = mnft.Response;
                //                    statusCode = "400";
                //                    message = "parameters missing/invalid";
                //                    failedmessage = message;

                //                    custsmsInfo = new CustActivityModel()
                //                    {
                //                        UserName = mobile,
                //                        RequestMerchant = transactionType,
                //                        DestinationNo = fundtransfer.da,
                //                        Amount = amount,
                //                        SMSStatus = "Failed",
                //                        SMSSenderReply = message,
                //                        ErrorMessage = failedmessage,
                //                    };
                //                }



                //            }
                //            else
                //            {
                //                custsmsInfo = new CustActivityModel()
                //                {
                //                    UserName = mobile,
                //                    RequestMerchant = transactionType,
                //                    DestinationNo = fundtransfer.da,
                //                    Amount = amount,
                //                    SMSStatus = "Failed",
                //                    SMSSenderReply = message,
                //                    ErrorMessage = failedmessage,
                //                };


                //            }

                //        }
                //    }
                //}

                //} //END Destination mobile No check

                //else
                //{
                //    failedmessage = "Destination mobile Not Registered";
                //    custsmsInfo = new CustActivityModel()
                //    {
                //        UserName = mobile,
                //        RequestMerchant = transactionType,
                //        DestinationNo = fundtransfer.da,
                //        Amount = amount,
                //        SMSStatus = "Failed",
                //        SMSSenderReply = message,
                //        ErrorMessage = failedmessage,
                //    };

                //    var v = new
                //    {
                //        StatusCode = Convert.ToInt32(400),
                //        StatusMessage = "Destination mobile Not Registered"
                //    };
                //    result = JsonConvert.SerializeObject(v);
                //}


                //if ((companyCode == null) || (serviceCode == null) || (account == null) || (special1 == null) || (special2 == null) || (transactionDate == null) ||
                //(transactionId == null) || (userId == null) || (userPassword == null) || (salePointType == null))
                //{
                //    // throw ex
                //    statusCode = "400";
                //    message = "Parameters Missing/Invalid";
                //    failedmessage = message;
                //}
                //else
                //{


                //    try
                //    {
                //        //for checkpayment
                //        string URI = "https://test.paypoint.md:4445/PayPointWS/PayPointMSOperations.asmx/CheckPayment";
                //        string myParameters = "companyCode=" + companyCode + "&serviceCode=" + serviceCode +
                //            "&account=" + account + "&special1=" + special1 + "&special2=" + special2 +
                //            "&transactionDate=" + transactionDate + "&transactionId=" + transactionId +
                //            "&userId=" + userId + "&userPassword=" + userPassword + "&salePointType=" + salePointType;

                //        string billNumber = "";
                //        string amountpay = "";
                //        string refStan = "";
                //        string exectransactionId = ""; //Unique
                //        string exectransactionDate = "";//Current DateTime
                //        string rltCheckPaymt = "";

                //        using (WebClient wc = new WebClient())
                //        {
                //            wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                //            var HtmlResult = wc.UploadString(URI, myParameters);

                //            //                        string HtmlResult = @"<string xmlns=""http://tempuri.org/""><PPResponse Result=""000"" Key=""93eb6e47-2ab1-41f8-b6e8-b93e99fd1be5""><ResultMessage>Operation is succesfully completed </ResultMessage>
                //            //<UtilityInfo><UtilityCode> 598 </UtilityCode></UtilityInfo><BillInfo><Bill><BillNumber> 93eb6e47-2ab1-41f8-b6e8-b93e99fd1be5 </BillNumber>
                //            //<DueDate> 2019-11-26T07:13:38 </DueDate><Amount> 13900 </Amount><ReserveInfo> ISHWORI PD.SHRESTHA </ReserveInfo><BillParam><mask> 1 </mask>
                //            //                        <commission type=""0"" val=""0.00"" op=""-"" paysource=""1""></commission></BillParam>" +
                //            //                            "<RefStan> 21265641797314 </RefStan></Bill></BillInfo></PPResponse></string>";

                //            XmlDocument xmlDoc = new XmlDocument();
                //            xmlDoc.LoadXml(HtmlResult);

                //            XmlNodeList test = xmlDoc.GetElementsByTagName("*");
                //            string results = test[0].InnerText;
                //            string HtmlResult1 = results;

                //            //start for key check
                //            var reader = new StringReader(HtmlResult1);
                //            var xdoc = XDocument.Load(reader);

                //            XDocument docParse = XDocument.Parse(xdoc.ToString());
                //            IEnumerable<XElement> responses = docParse.Descendants();

                //            var xElem = XElement.Parse(docParse.ToString());


                //            //foreach (XElement response in responses)
                //            //{
                //            rltCheckPaymt = xElem.Attribute("Result").Value;
                //            string keyrlt = "";// response.Attribute("Key").Value;

                //            if (xElem.Attribute("Result").Value == "000")
                //            {
                //                keyrlt = xElem.Attribute("Key").Value;
                //                billNumber = xElem.Descendants().Elements("BillNumber").Where(x => x.Name == "BillNumber").SingleOrDefault().Value;
                //                amountpay = xElem.Descendants().Elements("Amount").Where(x => x.Name == "Amount").SingleOrDefault().Value;
                //                refStan = xElem.Descendants().Elements("RefStan").Where(x => x.Name == "RefStan").SingleOrDefault().Value;
                //                exectransactionDate = xElem.Descendants().Elements("DueDate").Where(x => x.Name == "DueDate").SingleOrDefault().Value;
                //            }
                //            //}
                //            //end for key check

                //            //var reader = new StringReader(HtmlResult);
                //            //var xdoc = XDocument.Load(reader);

                //            //XDocument doc = XDocument.Parse(xdoc.ToString());

                //            //XNamespace ns = "http://tempuri.org/";
                //            //IEnumerable<XElement> responses = doc.Descendants(ns + "string");

                //            //foreach (XElement response in responses)
                //            //{
                //            //    foreach (XElement elementn in response.Nodes())
                //            //    {
                //            //        string rlt = elementn.Attribute("Result").Value;
                //            //        string keyrlt = elementn.Attribute("Key").Value;

                //            //        if (elementn.Attribute("Result").Value == "000")
                //            //        {
                //            //            for (int i = 0; i < elementn.Elements().Count(); ++i)
                //            //            {
                //            //                var row = elementn.Elements().ElementAt(i);

                //            //            }
                //            //            //OR
                //            //            foreach (var submenu in elementn.Elements())
                //            //            {
                //            //                string sm = submenu.ToString();
                //            //            }
                //            //        }

                //            //    }
                //            //}
                //        }

                //        if (rltCheckPaymt == "000")
                //        {
                //            amountpay = (Convert.ToInt32(amount) * 100).ToString();

                //            long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                //            exectransactionId = milliseconds.ToString();

                //            if ((companyCode == null) || (serviceCode == null) || (account == null) ||
                //                (exectransactionDate == null) || (exectransactionId == null) ||
                //                (refStan == null) || (amountpay == null) || (billNumber == null) ||
                //                (userId == null) || (userPassword == null) || (salePointType == null))
                //            {
                //                // throw ex
                //                statusCode = "400";
                //                message = "Parameters Missing/Invalid";
                //                failedmessage = message;
                //            }
                //            else
                //            {
                //                if (companyCode == "598")
                //                {
                //                    special1 = "";
                //                }
                //                else
                //                {
                //                    special1 = special1.ToString();
                //                }
                //                string URIEXECPayment = "https://test.paypoint.md:4445/PayPointWS/PayPointMSOperations.asmx/ExecutePayment";
                //                string execPayParameters = "companyCode=" + companyCode + "&serviceCode=" + serviceCode +
                //                        "&account=" + account + "&special1=" + special1 + "&special2=" + special2 +
                //                        "&transactionDate=" + exectransactionDate + "&transactionId=" + exectransactionId +
                //                        "&refStan=" + refStan + "&amount=" + amountpay + "&billNumber=" + billNumber +
                //                        "&userId=" + userId + "&userPassword=" + userPassword + "&salePointType=" + salePointType;

                //                string compResultResp = "";
                //                string keyExecRlt = "";

                //                using (WebClient wcExecPay = new WebClient())
                //                {
                //                    wcExecPay.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                //                    string HtmlResultExecPay = wcExecPay.UploadString(URIEXECPayment, execPayParameters);

                //                    XmlDocument xmlEDoc = new XmlDocument();
                //                    xmlEDoc.LoadXml(HtmlResultExecPay);

                //                    XmlNodeList nodeEPay = xmlEDoc.GetElementsByTagName("*");
                //                    string resultEPay = nodeEPay[0].InnerText;
                //                    string HtmlEPayResult = resultEPay;

                //                    //start for key check
                //                    var readerEPay = new StringReader(HtmlEPayResult);
                //                    var xdocEPay = XDocument.Load(readerEPay);

                //                    XDocument docEPay = XDocument.Parse(xdocEPay.ToString());
                //                    //IEnumerable<XElement> responseEPay = docEPay.Descendants();
                //                    //foreach (XElement response in responseEPay)
                //                    //{
                //                    //    compResultResp = response.Attribute("Result").Value;
                //                    //    keyExecRlt = response.Attribute("Key").Value;

                //                    //}

                //                    var xElemEPay = XElement.Parse(xdocEPay.ToString());
                //                    if (xElemEPay.Attribute("Result").Value == "000")
                //                    {
                //                        compResultResp = xElemEPay.Attribute("Result").Value;
                //                        keyExecRlt = xElemEPay.Attribute("Key").Value;
                //                    }
                //                    //end for key check

                //                }

                //                if (compResultResp == "000")
                //                {
                //                    if ((refStan == null) || (userId == null) || (userPassword == null) || (salePointType == null))
                //                    {
                //                        // throw ex
                //                        statusCode = "400";
                //                        message = "Parameters Missing/Invalid";
                //                        failedmessage = message;
                //                    }
                //                    else if (compResultResp != "")
                //                    {
                //                        string key = "";
                //                        string gtBillNumber = "";
                //                        string URIGetTran = "https://test.paypoint.md:4445/PayPointWS/PayPointMSOperations.asmx/GetTransaction";
                //                        string GetTranParameters = "userLogin=" + userId + "&userPassword=" + userPassword + "&stan=-1&refStan=" + refStan + "key=" + key + "&billNumber=" + gtBillNumber;

                //                        string getTranResultResp = "";
                //                        string keyGetTrancRlt = "";

                //                        using (WebClient wcGetTran = new WebClient())
                //                        {
                //                            wcGetTran.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                //                            string HtmlResultGetTran = wcGetTran.UploadString(URIGetTran, GetTranParameters);

                //                            XmlDocument xmlEDoc = new XmlDocument();
                //                            xmlEDoc.LoadXml(HtmlResultGetTran);

                //                            XmlNodeList nodeEPay = xmlEDoc.GetElementsByTagName("*");
                //                            string resultEPay = nodeEPay[0].InnerText;
                //                            string HtmlEPayResult = resultEPay;

                //                            //start for key check
                //                            var readerEPay = new StringReader(HtmlEPayResult);
                //                            var xdocEPay = XDocument.Load(readerEPay);

                //                            XDocument docEPay = XDocument.Parse(xdocEPay.ToString());
                //                            //IEnumerable<XElement> responseEPay = docEPay.Descendants();
                //                            //foreach (XElement response in responseEPay)
                //                            //{
                //                            //    compResultResp = response.Attribute("Result").Value;
                //                            //    keyExecRlt = response.Attribute("Key").Value;
                //                            //}
                //                            var xElemGPay = XElement.Parse(docEPay.ToString());
                //                            if (xElemGPay.Attribute("Result").Value == "000")
                //                            {
                //                                getTranResultResp = xElemGPay.Attribute("Result").Value;
                //                                keyGetTrancRlt = xElemGPay.Attribute("Key").Value;
                //                            }
                //                            //end for key check

                //                        }

                //                    }

                //                }


                //            }

                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        message = ex + "Error Message ";
                //    }

                //}

            }
            //}

            ///Start Insert PayPoint Data

            try
            {
                int resultsReqCP = PaypointUtils.RequestCPPaypointInfo(reqCPPaypointInfo);
                int resultsResCP = PaypointUtils.ResponseCPPaypointInfo(resCPPaypointInfo);

                int resultsReqEP = PaypointUtils.RequestEPPaypointInfo(reqEPPaypointInfo);
                int resultsResEP = PaypointUtils.ResponseEPPaypointInfo(resEPPaypointInfo);

                int resultsReqGTP = PaypointUtils.RequestGTPaypointInfo(reqGTPaypointInfo);
                int resultsResGTP = PaypointUtils.ResponseGTPaypointInfo(resGTPaypointInfo);

                if ((resultsReqCP > 0) && (resultsResCP > 0) && (resultsReqEP > 0) && (resultsResEP > 0))
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

            ///End Insert PayPoint Data

            ///START Register For SMS
            try
            {
                int results = CustActivityUtils.RegisterCustActivityInfo(custsmsInfo);
                if (results > 0)
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

            ///END SMS Register For SMS


            if (statusCode == "")
            {
                result = result.ToString();
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
                    StatusMessage = failedmessage
                };
                result = JsonConvert.SerializeObject(v);
            }
            return result;

        }

        static void BackgroundTaskWithObject(Object stateInfo)
        {
            FundTransfer data = (FundTransfer)stateInfo;
            Console.WriteLine($"Hi {data.tid} from ThreadPool.");
            Thread.Sleep(1000);
        }

        #endregion

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
            //string da = qs["da"];
            //string da = "9877777777";//new merchant made in silver line
            //string da = "9841356370";//user in silver line
            //string da = "9801000004";// scool merchant in silver line

            //string da = "9840066836";//merchant in 30
            //string da = "9813999353";//user in 30
            //string da = "9841671238";//user in 30

            //string da = "9801055303";//merchant  naresh in silver line

            string da = System.Web.Configuration.WebConfigurationManager.AppSettings["DestinationNoForPaypoint"];


            //string amount = "5";
            //string pin = "1111";

            //string note = qs["note"];
            //string note = "paypoint from " + qs["mobile"] + " to Nepal Water . Customer Code=" + qs["account"] + ". " + qs["note"];
            string note = "utility payment for Nepal Water. Customer Code=" + qs["account"];//+ ". " + qs["note"];

            string src = qs["src"];
            string result = "";
            string sessionID = qs["tokenID"];
            string resultMessageResCP = "";

            string companyCode = qs["companyCode"]; //"598";//
            string serviceCode = qs["serviceCode"]; //"1";// 
                                                    //string serviceCode = qs["special1"]; //"1";// 
                                                    // string serviceCode = qs["special1"]; //"1";// 
            string account = qs["account"]; //"013.01.001";//
            //string special1 = qs["special1"]; //"217";//
            //string special2 = qs["special2"]; //"2300";//
            string special1 = "";
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
            string paypointType = "3";//qs["paypointType"]
            string transactionType = string.Empty;

            PaypointModel reqCPPaypointNepalWaterInfo = new PaypointModel();//to store data request of CP which is also commmon in nea
            PaypointModel resCPPaypointNepalWaterInfo = new PaypointModel();//to store data response of CP which is also commmon in nea

            //PaypointModel resPaypointPaymentInfo = new PaypointModel();
            PaypointModel resPaypointNepalWaterPaymentInfo = new PaypointModel();//to store data of Response of CP only

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
                reqCPPaypointNepalWaterInfo = new PaypointModel()
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
                        customerName = xElem.Descendants().Elements("ReserveInfo").Where(x => x.Name == "ReserveInfo").SingleOrDefault().Value;


                        //for  determining total number of months to pay
                        int i_count = 0;
                        //int p = 1;
                        //for (p = 1; p < 10; p++)
                            int p = 0;
                        for (p = 0; p < 10; p++)

                        {

                            string q = p.ToString();

                            IEnumerable<XElement> elements2 =
                                                     from e in xElem.Descendants().Elements("paymentex")
                                                     where ((string)e.Element("_i")) == q
                                                     orderby ((string)e.Element("_i"))
                                                     select e;
                            //IEnumerable<XElement> elements3 =
                            //                         from e in xElem.Descendants().Elements("payment")
                            //                         where ((string)e.Element("_i")) == q
                            //                         orderby ((string)e.Element("_i"))
                            //                         select e;
                            if (elements2.Any())
                            {
                                i_count++;//total number of months to pay
                            }
                        }


                        string[] descriptionP = new string[i_count];
                        string[] billDateP = new string[i_count];
                        string[] billAmountP = new string[i_count];
                        string[] amountP = new string[i_count];
                        string[] totalAmountP = new string[i_count];

                        string[] statusP = new string[i_count];
                        string[] amountfactP = new string[i_count];
                        string[] amountmaskP = new string[i_count];
                        string[] amountmaxP = new string[i_count];
                        string[] amountminP = new string[i_count];

                        string[] amountstepP = new string[i_count];
                        string[] codservP = new string[i_count];
                        string[] commissionP = new string[i_count];
                        string[] commisvalueP = new string[i_count];
                        string[] destinationP = new string[i_count];

                        string[] fioP = new string[i_count];
                        string[] iP = new string[i_count];
                        string[] idP = new string[i_count];
                        string[] jP = new string[i_count];
                        string[] requestIdP = new string[i_count];

                        string[] show_counterP = new string[i_count];
                        string[] legatNumberP = new string[i_count];
                        string[] discountAmountP = new string[i_count];
                        string[] counterRentP = new string[i_count];
                        string[] fineAmountP = new string[i_count];

                        string[] billDateFromP = new string[i_count];
                        string[] billDateToP = new string[i_count];
                        //start storing in database 0 value for null response i.e for  i_count=0
                        if (i_count == 0)
                        {
                            //for checkpayment payaments response insert in database

                            resPaypointNepalWaterPaymentInfo = new PaypointModel()
                            {
                                descriptionP = "0",
                                billDateP = "0",
                                billAmountP = "0",
                                amountP = "0",
                                totalAmountP = "0",

                                statusP = "0",
                                amountfactP = "0",
                                amountmaskP = "0",
                                amountmaxP = "0",
                                amountminP = "0",

                                amountstepP = "0",
                                codservP = "0",
                                commissionP = "0",
                                commisvalueP = "0",
                                destinationP = "0",

                                fioP = "0",
                                iP = "0",
                                idP = "0",
                                jP = "0",
                                requestIdP = "0",
                                show_counterP = "0",
                                i_countP = "0",

                                legatNumberP = "0",
                                discountAmountP = "0",
                                counterRentP = "0",
                                fineAmountP = "0",
                                billDateFromP = "0",
                                billDateToP = "0",
                                UserName = mobile,
                                ClientCode = ClientCode,
                            };

                            int resultsPayments = PaypointUtils.PaypointNepalWaterPaymentInfo(resPaypointNepalWaterPaymentInfo);


                        }
                        //end storing in database 0 value for null response i.e for  i_count=0

                        int k;
                        for (k = 0; k < i_count; k++)
                        {
                            //int n = k + 1;
                            //string m = n.ToString();
                            string m = k.ToString();

                            IEnumerable<XElement> elements1 =
                                                 from e in xElem.Descendants().Elements("paymentex")
                                                 where ((string)e.Element("_i")) == m
                                                 orderby ((string)e.Element("_i"))
                                                 select e;


                            descriptionP[k] = elements1.Elements("_description").SingleOrDefault().Value;
                            billDateP[k] = elements1.Elements("_billDate").SingleOrDefault().Value;
                            billAmountP[k] = elements1.Elements("_billAmount").SingleOrDefault().Value;
                            amountP[k] = elements1.Elements("_amount").SingleOrDefault().Value;
                            totalAmountP[k] = elements1.Elements("_totalAmount").SingleOrDefault().Value;

                            statusP[k] = elements1.Elements("_status").SingleOrDefault().Value;
                            amountfactP[k] = elements1.Elements("_amountfact").SingleOrDefault().Value;
                            amountmaskP[k] = elements1.Elements("_amountmask").SingleOrDefault().Value;
                            amountmaxP[k] = elements1.Elements("_amountmax").SingleOrDefault().Value;
                            amountminP[k] = elements1.Elements("_amountmin").SingleOrDefault().Value;

                            amountstepP[k] = elements1.Elements("_amountstep").SingleOrDefault().Value;
                            codservP[k] = elements1.Elements("_codserv").SingleOrDefault().Value;
                            commissionP[k] = elements1.Elements("_commission").SingleOrDefault().Value;
                            commisvalueP[k] = elements1.Elements("_commisvalue").SingleOrDefault().Value;
                            destinationP[k] = elements1.Elements("_destination").SingleOrDefault().Value;

                            fioP[k] = elements1.Elements("_fio").SingleOrDefault().Value;

                            iP[k] = elements1.Elements("_i").SingleOrDefault().Value;
                            idP[k] = elements1.Elements("_id").SingleOrDefault().Value;
                            jP[k] = elements1.Elements("_j").SingleOrDefault().Value;
                            requestIdP[k] = elements1.Elements("_requestId").SingleOrDefault().Value;
                            show_counterP[k] = elements1.Elements("_show_counter").SingleOrDefault().Value;

                            legatNumberP[k] = elements1.Elements("_legatNumber").SingleOrDefault().Value;
                            discountAmountP[k] = elements1.Elements("_discountAmount").SingleOrDefault().Value;
                            counterRentP[k] = elements1.Elements("_counterRent").SingleOrDefault().Value;
                            fineAmountP[k] = elements1.Elements("_fineAmount").SingleOrDefault().Value;
                            billDateFromP[k] = elements1.Elements("_billDateFrom").SingleOrDefault().Value;
                            billDateToP[k] = elements1.Elements("_billDateTo").SingleOrDefault().Value;

                            customerName = elements1.Elements("_fio").SingleOrDefault().Value;

                            //for checkpayment payaments response insert in database for nepal water

                            resPaypointNepalWaterPaymentInfo = new PaypointModel()
                            {
                                descriptionP = descriptionP[k],
                                billDateP = billDateP[k],
                                billAmountP = billAmountP[k],
                                amountP = amountP[k],
                                totalAmountP = totalAmountP[k],

                                statusP = statusP[k],
                                amountfactP = amountfactP[k],
                                amountmaskP = amountmaskP[k],
                                amountmaxP = amountmaxP[k],
                                amountminP = amountminP[k],

                                amountstepP = amountstepP[k],
                                codservP = codservP[k],
                                commissionP = commissionP[k],
                                commisvalueP = commisvalueP[k],
                                destinationP = destinationP[k],

                                fioP = fioP[k],
                                iP = iP[k],
                                idP = idP[k],
                                jP = jP[k],
                                requestIdP = requestIdP[k],
                                show_counterP = show_counterP[k],
                                i_countP = i_count.ToString(),

                                legatNumberP = legatNumberP[k],
                                discountAmountP = discountAmountP[k],
                                counterRentP = counterRentP[k],
                                fineAmountP = fineAmountP[k],
                                billDateFromP = billDateFromP[k],
                                billDateToP = billDateToP[k],
                                UserName = mobile,
                                ClientCode = ClientCode,

                            };

                            int resultsPayments = PaypointUtils.PaypointNepalWaterPaymentInfo(resPaypointNepalWaterPaymentInfo);

                        }
                    }
                    else
                    {
                        keyrlt = xElem.Attribute("Key").Value;
                        resultMessageResCP = xElem.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;
                    }
                    resultMessageResCP = xElem.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;

                    //for checkpayment response insert in database in nepal water
                    resCPPaypointNepalWaterInfo = new PaypointModel()
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
                    retrievalRef = tid;
                    refStanCK = refStan;
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
                int resultsReqCP = PaypointUtils.RequestCPPaypointInfo(reqCPPaypointNepalWaterInfo);
                int resultsResCP = PaypointUtils.ResponseCPPaypointInfo(resCPPaypointNepalWaterInfo);


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
                    retrievalRef = tid,
                    refStan = refStanCK
                    //retrievalRef = resCPPaypointNepalWaterInfo.retrievalReferenceResCP,
                    //refStanCK = resCPPaypointNepalWaterInfo.refStanResCP
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
                    StatusMessage = failedmessage
                    //retrievalRef = "",
                    //   refStanCK = ""
                };
                result = JsonConvert.SerializeObject(v);
            }

          //  DelayForSec(15000);
            return result;

        }

        #endregion


        #region"execute Paypoint"
        [OperationContract]
        [WebInvoke(Method = "POST",
                  ResponseFormat = WebMessageFormat.Json)]
        public string executepayment(Stream input)
        {
            //SMS
            string SMSNTC = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalNTCSMSServerUrl"];
            string SMSNCELL = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalSMSServerUrl"];

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
            //string da = qs["da"];
            //string da = "9877777777";//new merchant made in silver line
            //string da = "9841356370";//user in silver line
            //string da = "9801000004";// scool merchant in silver line

            //string da = "9840066836";//merchant in 30
            //string da = "9813999353";//user in 30
            //string da = "9841671238";//user in 30
            //string da = "9801055303";//merchant  naresh in silver line
            string da = System.Web.Configuration.WebConfigurationManager.AppSettings["DestinationNoForPaypoint"];


            string amount = qs["amount"];//amount paid by customer 
            string pin = qs["pin"];
            pin = HashAlgo.Hash(pin);
            string note = "utility payment for Nepal Water. Customer Code=" + qs["account"];//+ ". " + qs["note"];
            string src = qs["src"];
            string result = "";
            string sessionID = qs["tokenID"];
            string resultMessageResCP = "";

            string companyCode = qs["companyCode"]; //"720";//
                                                    //string serviceCode = qs["special1"]; //"1";// 
            string serviceCode = qs["serviceCode"]; //"11";// 
                                                    // string serviceCode = qs["special1s"]; //"11";//
            string account = qs["account"]; //"1234567";//            
            string special1 = ""; //
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
            //string paypointType = qs["paypointType"];
            //string paypointType = "Nepal Water";
            string paypointType = "3";

            string customerName = qs["customerName"];
            string walletBalance = qs["walletBalance"];//in Rs.
            string retrievalReference = qs["retrievalReference"];
            int walletBalancePaisaInt = 0;
            walletBalancePaisaInt = Convert.ToInt32((Convert.ToDouble(walletBalance)) * 100);
            int amountpayInt = Convert.ToInt32(amountpay);
            PaypointModel reqEPPaypointNepalWaterInfo = new PaypointModel();
            PaypointModel resEPPaypointNepalWaterInfo = new PaypointModel();

            PaypointModel reqGTPaypointNepalWaterInfo = new PaypointModel();
            PaypointModel resGTPaypointNepalWaterInfo = new PaypointModel();

            //START EGTP
            PaypointModel resGTAllPaypointNepalWaterInfo = new PaypointModel();
            //END EGTP
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
                pin  = pin,
                note = note,
                sourcechannel = src
            };


            if (sc == "00")//for wallet NEA payment
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
                                // message = "Invalid PIN! You have already attempt 3 times with wrong PIN,Please try again after 10 minutes";
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
                                    fundtransfer.sourcechannel,"Nepal Water");
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
                                        var transactionpaypoint = new MNTransactionMaster(mnft,account);
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
                                            else if (validTransactionData.ResponseCode == "OK")
                                            {
                                                LoginUtils.SetPINTries(mobile, "RPT");
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
                                    fundtransfer.sourcechannel,"Nepal Water");
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
                                        var transactionpaypoint = new MNTransactionMaster(mnft,account);
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
                                            else if (validTransactionData.ResponseCode == "OK")
                                            {
                                                LoginUtils.SetPINTries(mobile, "RPT");
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

            }
            try
            {

                //for  all EP  and GT transaction
                string compResultResp = "";

                if (statusCode == "200")
                {
                    try
                    {
                        string amountInPaisa = ((double.Parse(amount)) * 100).ToString();

                        if (rltCheckPaymt == "000")//go if CP Response is 000
                        {
                            int amountInPaisaInt = Convert.ToInt32(amountInPaisa);

                            long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                            exectransactionId = milliseconds.ToString();

                            string keyExecRlt = "";
                            string resultMessageResEP = "";
                            do
                            {
                                if ((companyCode == null) || (serviceCode == null) || (account == null) ||
                                (exectransactionDate == null) || (exectransactionId == null) ||
                                (refStan == null) || (amountInPaisa == null) || (billNumber == null) ||
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
                                            "&refStan=" + refStan + "&amount=" + amountInPaisa + "&billNumber=" + billNumber +
                                            "&userId=" + userId.Trim() + "&userPassword=" + userPassword.Trim() + "&salePointType=" + salePointType;

                                    //for executepayment request insert in database for nepalwater
                                    reqEPPaypointNepalWaterInfo = new PaypointModel()
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
                                        amountReqEP = amountInPaisa,
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
                                        resEPPaypointNepalWaterInfo = new PaypointModel()
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
                                            amountResEP = amountInPaisa,
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

                            ///for  Inserting EP PayPoint Data for nepalwater

                            try
                            {
                                int resultsReqEP = PaypointUtils.RequestEPPaypointInfo(reqEPPaypointNepalWaterInfo);
                                int resultsResEP = PaypointUtils.ResponseEPPaypointInfo(resEPPaypointNepalWaterInfo);

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
                                        reqGTPaypointNepalWaterInfo = new PaypointModel()
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
                                            amountReqGTP = amountInPaisa,
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


                                                //START EGTP
                                                //FOR  gt res all
                                                string ResultResGTPAll = getTranResultResp;
                                                string ResponseKeyResGTPAll = keyGetTrancRlt;
                                                string RequestKeyResGTPAll = xElemGPay.Descendants().Elements("RequestKey").Where(x => x.Name == "RequestKey").SingleOrDefault().Value;
                                                string StanResGTPAll = xElemGPay.Descendants().Elements("Stan").Where(x => x.Name == "Stan").SingleOrDefault().Value;
                                                string RefStanResGTPAll = xElemGPay.Descendants().Elements("RefStan").Where(x => x.Name == "RefStan").SingleOrDefault().Value;


                                                string ExternalStanResGTPAll = xElemGPay.Descendants().Elements("ExternalStan").Where(x => x.Name == "ExternalStan").SingleOrDefault().Value;
                                                string CompanyIDResGTPAll = xElemGPay.Descendants().Elements("Company").Where(x => x.Name == "Company").SingleOrDefault().Value;
                                                string CompanyNameResGTPAll = xElemGPay.Descendants().Elements("Name").Where(x => x.Name == "Name").SingleOrDefault().Value;
                                                string ServiceCodeResGTPAll = xElemGPay.Descendants().Elements("ServiceCode").Where(x => x.Name == "ServiceCode").SingleOrDefault().Value;
                                                string ServiceNameResGTPAll = xElemGPay.Descendants().Elements("ServiceName").Where(x => x.Name == "ServiceName").SingleOrDefault().Value;

                                                string AccountResGTPAll = xElemGPay.Descendants().Elements("Account").Where(x => x.Name == "Account").SingleOrDefault().Value;
                                                string CurrencyResGTPAll = xElemGPay.Descendants().Elements("Currency").Where(x => x.Name == "Currency").SingleOrDefault().Value;
                                                string CurrencyCodeResGTPAll = xElemGPay.Descendants().Elements("CurrencyCode").Where(x => x.Name == "CurrencyCode").SingleOrDefault().Value;
                                                string AmountResGTPAll = xElemGPay.Descendants().Elements("Amount").Where(x => x.Name == "Amount").SingleOrDefault().Value;
                                                string CommissionAmountResGTPAll = xElemGPay.Descendants().Elements("CommissionAmount").Where(x => x.Name == "CommissionAmount").SingleOrDefault().Value;

                                                string BillNumberResGTPAll = xElemGPay.Descendants().Elements("BillNumber").Where(x => x.Name == "BillNumber").SingleOrDefault().Value;
                                                string UserLoginResGTPAll = xElemGPay.Descendants().Elements("UserLogin").Where(x => x.Name == "UserLogin").SingleOrDefault().Value;
                                                string SalesPointTypeResGTPAll = xElemGPay.Descendants().Elements("SalesPointType").Where(x => x.Name == "SalesPointType").SingleOrDefault().Value;
                                                string StatusResGTPAll = xElemGPay.Descendants().Elements("Status").Where(x => x.Name == "Status").SingleOrDefault().Value;
                                                string RegDateResGTPAll = xElemGPay.Descendants().Elements("RegDate").Where(x => x.Name == "RegDate").SingleOrDefault().Value;

                                                string PaymentIdResGTPAll = xElemGPay.Descendants().Elements("PaymentId").Where(x => x.Name == "PaymentId").SingleOrDefault().Value;
                                                string DealerIdResGTPAll = xElemGPay.Descendants().Elements("DealerId").Where(x => x.Name == "DealerId").SingleOrDefault().Value;
                                                string DealerNameResGTPAll = xElemGPay.Descendants().Elements("DealerName").Where(x => x.Name == "DealerName").SingleOrDefault().Value;
                                                string ResponseCodeResGTPAll = xElemGPay.Descendants().Elements("ResponseCode").Where(x => x.Name == "ResponseCode").SingleOrDefault().Value;
                                                string PaySourceTypeResGTPAll = xElemGPay.Descendants().Elements("PaySourceType").Where(x => x.Name == "PaySourceType").SingleOrDefault().Value;

                                                string CityResGTPAll = xElemGPay.Descendants().Elements("City").Where(x => x.Name == "City").SingleOrDefault().Value;
                                                string AddressResGTPAll = xElemGPay.Descendants().Elements("Address").Where(x => x.Name == "Address").SingleOrDefault().Value;
                                                string CloseDateResGTPAll = xElemGPay.Descendants().Elements("CloseDate").Where(x => x.Name == "CloseDate").SingleOrDefault().Value;
                                                string ProblemResGTPAll = xElemGPay.Descendants().Elements("Problem").Where(x => x.Name == "Problem").SingleOrDefault().Value;



                                                ////for get transaction payment response all insert in database
                                                resGTAllPaypointNepalWaterInfo = new PaypointModel()
                                                {


                                                    ResultResGTPAll = ResultResGTPAll,
                                                    ResponseKeyResGTPAll = ResponseKeyResGTPAll,
                                                    RequestKeyResGTPAll = RequestKeyResGTPAll,
                                                    StanResGTPAll = StanResGTPAll,
                                                    RefStanResGTPAll = RefStanResGTPAll,

                                                    ExternalStanResGTPAll = ExternalStanResGTPAll,
                                                    CompanyIDResGTPAll = CompanyIDResGTPAll,
                                                    CompanyNameResGTPAll = CompanyNameResGTPAll,
                                                    ServiceCodeResGTPAll = ServiceCodeResGTPAll,
                                                    ServiceNameResGTPAll = ServiceNameResGTPAll,

                                                    AccountResGTPAll = AccountResGTPAll,
                                                    CurrencyResGTPAll = CurrencyResGTPAll,
                                                    CurrencyCodeResGTPAll = CurrencyCodeResGTPAll,
                                                    AmountResGTPAll = AmountResGTPAll,
                                                    CommissionAmountResGTPAll = CommissionAmountResGTPAll,

                                                    BillNumberResGTPAll = BillNumberResGTPAll,
                                                    UserLoginResGTPAll = UserLoginResGTPAll,
                                                    SalesPointTypeResGTPAll = SalesPointTypeResGTPAll,
                                                    StatusResGTPAll = StatusResGTPAll,
                                                    RegDateResGTPAll = RegDateResGTPAll,

                                                    PaymentIdResGTPAll = PaymentIdResGTPAll,
                                                    DealerIdResGTPAll = DealerIdResGTPAll,
                                                    DealerNameResGTPAll = DealerNameResGTPAll,
                                                    ResponseCodeResGTPAll = ResponseCodeResGTPAll,
                                                    PaySourceTypeResGTPAll = PaySourceTypeResGTPAll,

                                                    CityResGTPAll = CityResGTPAll,
                                                    AddressResGTPAll = AddressResGTPAll,
                                                    CloseDateResGTPAll = CloseDateResGTPAll,
                                                    ProblemResGTPAll = ProblemResGTPAll,
                                                    UserName = mobile,

                                                    ClientCode = ClientCode,

                                                    Mode = "NWGTRes",
                                                };




                                                //END EGTP



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
                                            resGTPaypointNepalWaterInfo = new PaypointModel()
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
                                                amountResGTP = amountInPaisa, //amountpay,
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

                                        int resultsReqGTP = PaypointUtils.RequestGTPaypointInfo(reqGTPaypointNepalWaterInfo);
                                        int resultsResGTP = PaypointUtils.ResponseGTPaypointInfo(resGTPaypointNepalWaterInfo);


                                        //START EGTP
                                        int resultsResGTPAll = PaypointUtils.ResponseGTAllPaypointInfo(resGTAllPaypointNepalWaterInfo);
                                        //EGTP 
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

               // DelayForSec(8000);
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
                                            + " to " +
                                            //GetMerchantName 
                                            "Utility payment for Nepal Water." + " on date " +
                                                (validTransactionData.CreatedDate).ToString("dd/MM/yyyy")
                                            + "." + "\n";
                            messagereply += "Thank you. NIBL Thaili";

                            var client = new WebClient();

                            //SENDER
                            if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                            {
                                //FOR NCELL
                                //var content = client.DownloadString(
                                //    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                //    + "977" + mobile + "&message=" + messagereply + "");
                                var content = client.DownloadString(
                                               SMSNCELL + "977" + mobile + "&message=" + messagereply + "");
                            }
                            else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                        || (mobile.Substring(0, 3) == "986"))
                            {
                                //FOR NTC
                                //var content = client.DownloadString(
                                //    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                //    + "977" + mobile + "&message=" + messagereply + "");
                                var content = client.DownloadString(
                                               SMSNTC + "977" + mobile + "&message=" + messagereply + "");
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

                //REverse Transaction 
                if ((statusCode != "111") && (statusCode != "114") && (statusCode != "115") && (statusCode != "116") && (statusCode != "119") &&
                    (statusCode != "121") && (statusCode != "163") && (statusCode != "180") && (statusCode != "181") && (statusCode != "182") &&
                    (statusCode != "183") && (statusCode != "184") && (statusCode != "185") && (statusCode != "186") && (statusCode != "187") &&
                    (statusCode != "188") && (statusCode != "189") && (statusCode != "190") && (statusCode != "800") && (statusCode != "902") &&
                    (statusCode != "904") && (statusCode != "906") && (statusCode != "907") && (statusCode != "909") && (statusCode != "911") &&
                    (statusCode != "913") && (statusCode != "90") && (statusCode != "91") && (statusCode != "94") && (statusCode != "95") &&
                    (statusCode != "98") && (statusCode != "99") && (statusCodeBalance != "400") && (compResultResp != "000") && (statusCodeBalance != "400") && (statusCode != "200")
                    )
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
                                          //for reverse aagadi success sms pathaune

                                        OutgoingWebResponseContext response2 = WebOperationContext.Current.OutgoingResponse;
                                        if (response2.StatusCode == HttpStatusCode.OK)
                                        {
                                            string messagereply = "";
                                            try
                                            {
                                                messagereply = "Dear " + CustCheckUtils.GetName(mobile) + "," + "\n";

                                                messagereply += " You have successfully paid NPR " + validTransactionData.Amount
                                                                + " to " +
                                                                //GetMerchantName 
                                                                "Utility payment for Nepal Water." + " on date " +
                                                                    (validTransactionData.CreatedDate).ToString("dd/MM/yyyy")
                                                                + "." + "\n";
                                                messagereply += "Thank you. NIBL Thaili";

                                                var client = new WebClient();

                                                //SENDER
                                                if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                                                {
                                                    //FOR NCELL
                                                    //var content = client.DownloadString(
                                                    //    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                                    //    + "977" + mobile + "&message=" + messagereply + "");
                                                    var content = client.DownloadString(
                                                        SMSNCELL + "977" + mobile + "&message=" + messagereply + "");
                                                }
                                                else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                                            || (mobile.Substring(0, 3) == "986"))
                                                {
                                                    //FOR NTC
                                                    //var content = client.DownloadString(
                                                    //    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                                    //    + "977" + mobile + "&message=" + messagereply + "");
                                                    var content = client.DownloadString(
                                                        SMSNTC + "977" + mobile + "&message=" + messagereply + "");
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
                                                                    "Utility payment for Nepal Water." + " on date " +
                                                                    (validTransactionData.CreatedDate).ToString("dd/MM/yyyy")
                                                                + "." + "\n";
                                                messagereply += "Thank you. NIBL Thaili";

                                                var client = new WebClient();

                                                //SENDER
                                                if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                                                {
                                                    //FOR NCELL
                                                    //var content = client.DownloadString(
                                                    //    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                                    //    + "977" + mobile + "&message=" + messagereply + "");
                                                    var content = client.DownloadString(
                                                        SMSNCELL + "977" + mobile + "&message=" + messagereply + "");
                                                }
                                                else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                                            || (mobile.Substring(0, 3) == "986"))
                                                {
                                                    //FOR NTC
                                                    //var content = client.DownloadString(
                                                    //    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                                    //    + "977" + mobile + "&message=" + messagereply + "");
                                                    var content = client.DownloadString(
                                                        SMSNTC + "977" + mobile + "&message=" + messagereply + "");
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


        #region "GetDetails"

        [OperationContract]
        [WebInvoke(Method = "GET",
                  ResponseFormat = WebMessageFormat.Json)]
        public string getDetailPayment(string userName, string clientCode, string tokenID, string serviceCode, string account, string special1, string special2, string src, string rltCheckPaymt, string retrievalRef, string refStanGD) //Stream input
        {
            string PaypointPwd = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointPwd"];
            System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate { return true; }; //to prevent from SSL error

            string result = "";
            string sessionID = tokenID;

            string message = string.Empty;
            string statusCode = string.Empty;
            string failedmessage = string.Empty;
            var resultList = new List<NepalWater>();

            //start all GD Transaction

            try
            {
                if (rltCheckPaymt == "000")
                {

                    //Session data set
                    string S_NWCounter = serviceCode; //"serviceCode"- NW.NWCounter
                    string S_CustomerID = account; //"account", NW.CustomerID
                    if ((S_NWCounter == null) || (S_CustomerID == null) || (retrievalRef == null) || (refStanGD == null))
                    {
                        statusCode = "400";
                        message = "Parameter Missing:Please Try Again!";
                        failedmessage = message;
                    }

                    NepalWater NWObj = new NepalWater();
                    NWObj.NWCounter = S_NWCounter;
                    NWObj.CustomerID = S_CustomerID;
                    NWObj.UserName = userName;
                    NWObj.ClientCode = clientCode;
                    NWObj.refStan = refStanGD;//PaypointUserModel.getrefStan(NEAObj);
                    NWObj.retrievalReference = retrievalRef;

                    //Database Accessed
                    NepalWater regobj = new NepalWater();
                    DataSet DPaypointSet = PaypointUtils.GetNWDetails(NWObj);
                    DataTable dResponse = DPaypointSet.Tables["dtResponse"];
                    DataSet DPaypointSetPay = PaypointUtils.GetNWDetailsPay(NWObj);
                    DataTable dNWPayment = DPaypointSetPay.Tables["dtPayment"];
                    //End Database Accessed

                    string rowNo = (dNWPayment.Rows.Count).ToString();
                    int countROW = dNWPayment.Rows.Count;
                    List<NepalWater> ListDetails = new List<NepalWater>(countROW);
                    if (dResponse != null && dResponse.Rows.Count > 0)
                    {
                        regobj.CustomerID = dResponse.Rows[0]["account"].ToString();
                        regobj.TotalAmountDue = dResponse.Rows[0]["amount"].ToString();
                        regobj.NWBranchCode = dResponse.Rows[0]["serviceCode"].ToString();
                        regobj.CustomerName = dResponse.Rows[0]["customerName"].ToString();
                        regobj.refStan = dResponse.Rows[0]["refStan"].ToString();
                        regobj.billNumber = dResponse.Rows[0]["billNumber"].ToString();
                        regobj.retrievalReference = dResponse.Rows[0]["retrievalReference"].ToString();
                        regobj.responseCode = dResponse.Rows[0]["responseCode"].ToString();

                        if (dNWPayment != null && dNWPayment.Rows.Count > 0)
                        {
                            //For Payments months 
                            for (int i = 0; i < countROW; i++)
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
                        }
                        else
                        {
                            statusCode = "400";
                            message = "Payment Details Not Found.";
                            failedmessage = message;
                        }

                        //ViewBag.NWBranchName = getNWBranchName(regobj.NWBranchCode.ToString());
                        string NWBranchCode = regobj.NWBranchCode.ToString();
                        string CustomerID = regobj.CustomerID;
                        double TotalAmountDue = Convert.ToDouble(regobj.TotalAmountDue.ToString());
                        TotalAmountDue = TotalAmountDue / 100;

                        //Viewbag For details from Nepal Water
                        string description = regobj.description.ToString();
                        string CustomerName = regobj.CustomerName;
                        string billDate = regobj.billDate.ToString();

                        double billAmount = Convert.ToDouble(regobj.billAmount.ToString());
                        billAmount = billAmount / 100;

                        double totalAmount = Convert.ToDouble(regobj.totalAmount.ToString());
                        totalAmount = totalAmount / 100;

                        string status = regobj.status.ToString();
                        string amountfact = regobj.amountfact.ToString();
                        string amountmask = regobj.amountmask.ToString();
                        string amountmax = regobj.amountmax.ToString();
                        string amountmin = regobj.amountmin.ToString();
                        string amountstep = regobj.amountstep.ToString();
                        string codserv = regobj.codserv.ToString();
                        string commission = regobj.commission.ToString();
                        string commisvalue = regobj.commisvalue.ToString();
                        string destination = regobj.destination.ToString();
                        string requestId = regobj.requestId.ToString();
                        string showCounter = regobj.showCounter.ToString();
                        string iCount = regobj.iCount.ToString();
                        string legatNumber = regobj.legatNumber.ToString();

                        double discountAmount = Convert.ToDouble(regobj.discountAmount.ToString());
                        discountAmount = discountAmount / 100;


                        double fineAmount = Convert.ToDouble(regobj.fineAmount.ToString());
                        fineAmount = fineAmount / 100;

                        string counterRent = regobj.counterRent.ToString();
                        string billDateFrom = regobj.billDateFrom.ToString();
                        string billDateTo = regobj.billDateTo.ToString();

                        UserInfo userInfo = new UserInfo();

                        MNBalance availBaln = new MNBalance();
                        DataTable dtableUser1 = PaypointUtils.GetAvailBaln(clientCode);
                        if (dtableUser1 != null && dtableUser1.Rows.Count > 0)
                        {
                            availBaln.amount = dtableUser1.Rows[0]["AvailBaln"].ToString();
                        }

                        statusCode = "200";
                        resultList.Add(new NepalWater
                        {
                            UserName = userName,
                            NWBranchCode = regobj.NWBranchCode,
                            CustomerID = regobj.CustomerID,
                            CustomerName = regobj.CustomerName,
                            TotalAmountDue = regobj.TotalAmountDue,
                            description = regobj.description,
                            billDate = regobj.billDate.ToString(),
                            billAmount = regobj.billAmount.ToString(),
                            totalAmount = regobj.totalAmount.ToString(),
                            status = regobj.status,

                            amountfact = regobj.amountfact.ToString(),
                            amountmask = regobj.amountmask.ToString(),
                            amountmax = regobj.amountmax.ToString(),
                            amountmin = regobj.amountmin.ToString(),
                            amountstep = regobj.amountstep.ToString(),
                            codserv = regobj.codserv.ToString(),
                            commission = regobj.commission.ToString(),
                            commisvalue = regobj.commisvalue.ToString(),
                            destination = regobj.destination.ToString(),
                            requestId = regobj.requestId.ToString(),
                            showCounter = regobj.showCounter.ToString(),
                            iCount = regobj.iCount.ToString(),
                            legatNumber = regobj.legatNumber.ToString(),

                            discountAmount = regobj.discountAmount.ToString(),
                            fineAmount = regobj.fineAmount.ToString(),

                            counterRent = regobj.counterRent.ToString(),
                            billDateFrom = regobj.billDateFrom.ToString(),
                            billDateTo = regobj.billDateTo.ToString(),

                            refStan = regobj.refStan,
                            billNumber = regobj.billNumber,
                            retrievalReference = regobj.retrievalReference,
                            amount = availBaln.amount,
                            responseCode = regobj.responseCode
                        });

                        // string sJSONResponse = JsonConvert.SerializeObject(resultList);

                        // result = sJSONResponse;

                        // statusCode = "200"; //200 - OK
                        // message = result;

                    }
                    else
                    {
                        statusCode = "400";
                        message = "Response Not Found";
                        failedmessage = message;
                    }

                    ////Check KYC
                    //DataTable dtableUserCheckKYC = ProfileUtils.CheckKYC(userName);
                    //if (dtableUserCheckKYC != null && dtableUserCheckKYC.Rows.Count > 0)
                    //{
                    //    userInfo.hasKYC = dtableUserCheckKYC.Rows[0]["hasKYC"].ToString();
                    //    userInfo.IsRejected = dtableUserCheckKYC.Rows[0]["IsRejected"].ToString();

                    //    ViewBag.IsRejected = userInfo.IsRejected;

                    //    ViewBag.hasKYC = userInfo.hasKYC;
                    //}

                    string sJSONResponse = JsonConvert.SerializeObject(resultList);

                    result = sJSONResponse;

                    statusCode = "200"; //200 - OK
                    message = result;
                }
                else if (!(rltCheckPaymt == "000"))
                {
                    ///start for GD FAILED MESSSAGE

                    statusCode = "400";
                    message = "Check Payment Not Successfully";
                    failedmessage = message;

                    ///END for GD FAILED MESSSAGE
                }

            }
            catch (Exception ex)
            {
                message = result + ex + "Error Message ";
                //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                statusCode = "400";
                failedmessage = message;
            }

            //end all GD Transaction

            if (statusCode == "")
            {
                result = result.ToString();
            }
            else if (statusCode == "200")
            {
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = result
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
                    StatusMessage = failedmessage
                };
                result = JsonConvert.SerializeObject(v);
            }
            return result;

        }


        #endregion

        private async void DelayForSec(int delaysec)
        {
            var t = Task.Run(async delegate
            {
                //await Task.Delay(30000);//30 sec
                await Task.Delay(delaysec);//30 sec
                //return 30;
            });
            t.Wait();

        }
    }




}
