using MNepalProject.Connection;
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
using System.Threading;
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
    public class neareverse
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
            string serviceCode = qs["serviceCode"]; //"1";// 
            string account = qs["account"]; //"013.01.001";//
            string special1 = qs["special1"]; //"217";//
            string special2 = qs["special2"]; //"2300";// 
            string transactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");//"2019-11-22T11:11:02";
            long millisecondstrandId = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string transactionId = millisecondstrandId.ToString(); //"120163339819";
            string userId = "MNepalLT";
            string userPassword = "MNepalLT"; // qs["tokenID"];
            string salePointType = "6"; // qs["tokenID"];
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
                //                            //var content = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=2&Password=test12test&From=9797&To=" + "977" + mobile + "&Text=" + messagereply + "");
                //                            //SENDER
                //                            if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                //                            {
                //                                //FOR NCELL
                //                                var content = client.DownloadString(
                //                                    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                //                                    + "977" + mobile + "&Text=" + messagereply + "");
                //                            }
                //                            else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                //                                        || (mobile.Substring(0, 3) == "986"))
                //                            {
                //                                //FOR NTC
                //                                var content = client.DownloadString(
                //                                    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                //                                    + "977" + mobile + "&Text=" + messagereply + "");
                //                            }


                //                            //FOR DESTIONATION NUMBER RECEIVER
                //                            mobile = da;
                //                            if ((da.Substring(0, 3) == "980") || (da.Substring(0, 3) == "981")) //FOR NCELL
                //                            {
                //                                //FOR NCELL
                //                                var content = client.DownloadString(
                //                                    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                //                                    + "977" + da + "&Text=" + messagereplyDest + "");
                //                            }
                //                            else if ((da.Substring(0, 3) == "985") || (da.Substring(0, 3) == "984")
                //                                        || (da.Substring(0, 3) == "986"))
                //                            {
                //                                //FOR NTC
                //                                var content = client.DownloadString(
                //                                    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                //                                    + "977" + da + "&Text=" + messagereplyDest + "");
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

        #region"Check Paypoint NEA"
        [OperationContract]
        [WebInvoke(Method = "POST",
                  ResponseFormat = WebMessageFormat.Json)]

        public string checkpayment(Stream input)
        {
            string PaypointPwd = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointPwd"];
            string PaypointUserID = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointUserID"];


            string serviceCodeTestServer = "0";
            serviceCodeTestServer = System.Web.Configuration.WebConfigurationManager.AppSettings["serviceCodeTestServer"];


            System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate { return true; }; //to prevent from SSL error
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);


            //start cp request input
            string vid = qs["vid"]; //MerchantID  
            string mobile = qs["mobile"];
            string src = qs["src"];
            string sessionID = qs["tokenID"];
            string companyCode = qs["companyCode"]; //"598";//
            string special1 = qs["special1"]; //"217";//
            string serviceCode = qs["special1"]; //"1";// 

            if (serviceCodeTestServer == "1")
            {
                serviceCode = serviceCodeTestServer; //"1";//
            }

            string account = qs["account"]; //"013.01.001";//
            string special2 = qs["special2"]; //"2300";// 
            string tid = qs["tid"];
            string ClientCode = qs["ClientCode"];
            string paypointType = "1";

            //string transactionDate1 = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");//"2019-11-22T11:11:02";

            string transactionDate = qs["transactionDate"];

            //end cp request input




            string note = "utility payment for NEA. Customer ID=" + qs["special2"];//+ ". " + qs["note"];

            string result = "";
            string resultMessageResCP = "";

            long millisecondstrandId = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string transactionId = millisecondstrandId.ToString(); //"120163339819";

            string userId = PaypointUserID;
            string userPassword = PaypointPwd.Trim();
            string salePointType = "6";





            //start cp response default value
            string keyrltDefaultValue = qs["keyrltDefaultValue"]; //MerchantID  
            string billNumberDefaultValue = qs["billNumberDefaultValue"];
            string amountpayDefaultValue = qs["amountpayDefaultValue"];
            string refStanDefaultValue = qs["refStanDefaultValue"];
            string exectransactionDateDefaultValue = qs["exectransactionDateDefaultValue"]; //"598";//



            string customerNameDefaultValue = qs["customerNameDefaultValue"]; //"217";//
            string ResultDefaultValue = qs["ResultDefaultValue"]; //"1";//  
            string ResultMessageDefaultValue = qs["ResultMessageDefaultValue"]; //MerchantID 

             
            //end cp response default value


            string transactionType = string.Empty;

            PaypointModel reqCPPaypointInfo = new PaypointModel();
            PaypointModel resCPPaypointInfo = new PaypointModel();

            PaypointModel resPaypointPaymentInfo = new PaypointModel();

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
            //start all CP Transaction

            try
            {
                //string amountInPaisa = (Convert.ToInt32(amount) * 100).ToString();

                //for 30 paypoint checkpayment link
                //string URI = "https://test.paypoint.md:4445/PayPointWS/PayPointMSOperations.asmx/CheckPayment"; //test link for paypoint i.e for 27.111.30.126

                //For checkpaypoint link in webconfig
                //string URI = System.Web.Configuration.WebConfigurationManager.AppSettings["CPPaypointUrl"];


                string myParameters = "companyCode=" + companyCode + "&serviceCode=" + serviceCode +
                    "&account=" + account + "&special1=" + special1 + "&special2=" + special2 +
                    "&transactionDate=" + transactionDate + "&transactionId=" + transactionId +
                    "&userId=" + userId.Trim() + "&userPassword=" + userPassword.Trim() + "&salePointType=" + salePointType;

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
                                               //string exectransactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"); //Current DateTime

                string exectransactionDate;

                string rltCheckPaymt = "";
                string customerName = "";

                using (WebClient wc = new WebClient())
                {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    
                    //                        string HtmlResult = @"<string xmlns=""http://tempuri.org/""><PPResponse Result=""000"" Key=""93eb6e47-2ab1-41f8-b6e8-b93e99fd1be5""><ResultMessage>Operation is succesfully completed </ResultMessage>
                    //<UtilityInfo><UtilityCode> 598 </UtilityCode></UtilityInfo><BillInfo><Bill><BillNumber> 93eb6e47-2ab1-41f8-b6e8-b93e99fd1be5 </BillNumber>
                    //<DueDate> 2019-11-26T07:13:38 </DueDate><Amount> 13900 </Amount><ReserveInfo> ISHWORI PD.SHRESTHA </ReserveInfo><BillParam><mask> 1 </mask>
                    //                        <commission type=""0"" val=""0.00"" op=""-"" paysource=""1""></commission></BillParam>" +
                    //                            "<RefStan> 21265641797314 </RefStan></Bill></BillInfo></PPResponse></string>";

                    
                    rltCheckPaymt = ResultDefaultValue;

                    string keyrlt = "";// response.Attribute("Key").Value;


                    message = resultMessageResCP;
                    // if (xElem.Attribute("Result").Value == "000")
                    if (ResultDefaultValue == "000")
                    {
                        
                        //default value
                        keyrlt = keyrltDefaultValue;
                        billNumber = billNumberDefaultValue;
                        amountpay = amountpayDefaultValue;
                        refStan = refStanDefaultValue;
                        exectransactionDate = exectransactionDateDefaultValue;
                        customerName = customerNameDefaultValue;

                        ////to determine total number of months to pay
                        

                        

                        

                    }
                    else
                    {
                        //keyrlt = xElem.Attribute("Key").Value;
                        //resultMessageResCP = xElem.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;


                        keyrlt =keyrltDefaultValue;
                        resultMessageResCP = ResultMessageDefaultValue;


                    }
                    //resultMessageResCP = xElem.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;
                    resultMessageResCP = ResultMessageDefaultValue;


                     

                    refStan = refStanDefaultValue; //"2300";// 

                    billNumber = billNumberDefaultValue; //"2300";// 

                     
                     
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
                        //amountResCP = amountInPaisa, //amountpay,
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


                if (!(rltCheckPaymt == "000"))
                //if (rltCheckPaymt == "000")
                {

                    ///start for CP FAILED MESSSAGE

                    //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                    statusCode = "400";
                    //message = result;
                    message = resultMessageResCP;
                    failedmessage = message;

                    ///END for CP FAILED MESSSAGE
                     
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


            ///Start Insert PayPoint Data

            try
            {
                int resultsReqCP = PaypointUtils.RequestCPPaypointInfo(reqCPPaypointInfo);
                int resultsResCP = PaypointUtils.ResponseCPPaypointInfo(resCPPaypointInfo);



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

            ///End Insert PayPoint Data



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

        #region"execute Paypoint"
        [OperationContract]
        [WebInvoke(Method = "POST",
                  ResponseFormat = WebMessageFormat.Json)]
        public string executepayment(Stream input)
        {
            string PaypointPwd = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointPwd"];
            string PaypointUserID = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointUserID"];


            string serviceCodeTestServer = "0";
            serviceCodeTestServer = System.Web.Configuration.WebConfigurationManager.AppSettings["serviceCodeTestServer"];

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
              
            string da = System.Web.Configuration.WebConfigurationManager.AppSettings["DestinationNoForPaypoint"];

            string amount = qs["amount"];//amount paid by customer //in Rs
            string pin = qs["pin"];
             
            string transactionDate = qs["transactionDate"]; 
            string transactionDateNote= transactionDate.Replace("T", "  ");


            string note = "utility payment for NEA. Customer ID=" + qs["special2"] + " " + transactionDateNote;// + ". "+ qs["note"];
            string src = qs["src"];
            string result = "";
            string sessionID = qs["tokenID"];
            string resultMessageResCP = "";

            string companyCode = qs["companyCode"]; //"598";//

            string special1 = qs["special1"]; //"217";//
            string serviceCode = qs["special1"]; //"1";//  


            if (serviceCodeTestServer == "1")
            {
                serviceCode = serviceCodeTestServer; //"1";//
            }



            string account = qs["account"]; //"013.01.001";//

            string special2 = qs["special2"]; //"2300";// 
            //string transactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");//"2019-11-22T11:11:02";

            //transactionDate = "2020-05-13T13:45:08";// default value

            //"2300";// 

            long millisecondstrandId = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string transactionId = millisecondstrandId.ToString(); //"120163339819";
            //string userId = "MNepalL";
            string userId = PaypointUserID;
            //string userPassword = "9&l$0#%M";
            string userPassword = PaypointPwd.Trim();
            string salePointType = "6";
            //string resultMessage = qs["resultMessage"]; 
            string transactionType = string.Empty;

            string amountpay = qs["amountpay"];//amount need to pay i.e amount in bill//in paisa
                                               //string exectransactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                                               // exectransactionDate = "2020-05-13T13:45:08";// default value

            string exectransactionDate = qs["exectransactionDate"]; //"2300";// 



            string exectransactionId = millisecondstrandId.ToString();
            string refStan = qs["refStan"];
            string billNumber = qs["billNumber"];
            string rltCheckPaymt = qs["rltCheckPaymt"];
            string ClientCode = qs["ClientCode"];
            //string paypointType = qs["paypointType"];
            string paypointType = "1";
            string customerName = qs["customerName"];
            string walletBalance = qs["walletBalance"];//in Rs.
            //string bankBalance = qs["bankBalance"];//in Rs.
            string retrievalReference = qs["retrievalReference"];


            //for execute payment response default value
            string ResultEPDefaultValue = qs["ResultEPDefaultValue"]; //"598";//

            string ResultMessageEPDefaultValue = qs["ResultMessageEPDefaultValue"]; //"217";//
            string KeyEPDefaultValue = qs["KeyEPDefaultValue"]; //"1";//  

            //for get transaction response default value
             
            string BillNumberGTPDefaultValue = billNumber;



            string ResultGTPDefaultValue = qs["ResultGTPDefaultValue"];
            string KeyGTPDefaultValue = qs["KeyGTPDefaultValue"];
            string ResultMessageGTPDefaultValue = qs["ResultMessageGTPDefaultValue"];
            string StatusGTPDefaultValue = qs["StatusGTPDefaultValue"];





            int walletBalancePaisaInt = 0;
            //int bankBalancePaisaInt = 0;


            //Double walletBalanceInt3= Convert.ToDouble(walletBalance);
            //int walletBalanceInt4 = Convert.ToInt32(walletBalanceInt3)*100;

            walletBalancePaisaInt = Convert.ToInt32((Convert.ToDouble(walletBalance)) * 100);
            //bankBalancePaisaInt = Convert.ToInt32((Convert.ToDouble(bankBalance)) * 100);

            //int amountPaisaInt = Convert.ToInt32(amount) *100;
            int amountpayInt = Convert.ToInt32(amountpay);


            PaypointModel reqEPPaypointInfo = new PaypointModel();
            PaypointModel resEPPaypointInfo = new PaypointModel();

            PaypointModel reqGTPaypointInfo = new PaypointModel();
            PaypointModel resGTPaypointInfo = new PaypointModel();

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

            //TraceIdGenerator traceid = new TraceIdGenerator();
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
                //Amount check Start
                //start if wallet balance less then bill amount then show error msg
                if (walletBalancePaisaInt >= amountpayInt)
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
                                        //tid = traceid.GenerateUniqueTraceID();

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
            else if (sc == "10")
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


            //start comment gareko 14th may issue
            try
            {

                //start all EP  and GT transaction
                string compResultResp = "";

                if (statusCode == "200")
                {
                    try
                    {
                        string amountInPaisa = (Convert.ToInt32(amount) * 100).ToString();

                        if (rltCheckPaymt == "000")
                        {
                            int amountInPaisaInt = Convert.ToInt32(amountInPaisa);

                            long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                            exectransactionId = milliseconds.ToString();

                            //string compResultResp = "";
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

                                    //string URIEXECPayment = "https://test.paypoint.md:4445/PayPointWS/PayPointMSOperations.asmx/ExecutePayment";

                                    //For excutepaypoint link in webconfig
                                    //string URIEXECPayment = System.Web.Configuration.WebConfigurationManager.AppSettings["EPPaypointUrl"];

                                    string execPayParameters = "companyCode=" + companyCode + "&serviceCode=" + serviceCode +
                                            "&account=" + account + "&special1=" + special1 + "&special2=" + special2 +
                                            "&transactionDate=" + exectransactionDate + "&transactionId=" + exectransactionId +
                                            "&refStan=" + refStan + "&amount=" + amountInPaisa + "&billNumber=" + billNumber +
                                            "&userId=" + userId.Trim() + "&userPassword=" + userPassword.Trim() + "&salePointType=" + salePointType;

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
                                        //retrievalReferenceReqEP = fundtransfer.tid,
                                        //retrievalReferenceReqEP = tid,
                                        retrievalReferenceReqEP = retrievalReference,
                                        remarkReqEP = "Execute Payment",
                                        UserName = mobile,
                                        ClientCode = ClientCode,
                                        paypointType = paypointType,

                                    };
                                    //END ExecutePayment Request

                                    using (WebClient wcExecPay = new WebClient())
                                    {
                                        //wcExecPay.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                                        //string HtmlResultExecPay = wcExecPay.UploadString(URIEXECPayment, execPayParameters);




                                         

                                        if (ResultEPDefaultValue == "000")
                                        {
                                            compResultResp = ResultEPDefaultValue;
                                            keyExecRlt = KeyEPDefaultValue;
                                        }
                                        else if ((ResultEPDefaultValue == "011") || (ResultEPDefaultValue == "012"))
                                        {
                                            compResultResp = ResultEPDefaultValue;
                                            keyExecRlt = KeyEPDefaultValue;
                                        }
                                        else
                                        {
                                            compResultResp = ResultEPDefaultValue;

                                            keyExecRlt = KeyEPDefaultValue;
                                        }
                                        resultMessageResEP = ResultMessageEPDefaultValue;

                                        //for Response Execute Payment
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

                            ///Start Insert EP PayPoint Data

                            try
                            {

                                int resultsReqEP = PaypointUtils.RequestEPPaypointInfo(reqEPPaypointInfo);
                                int resultsResEP = PaypointUtils.ResponseEPPaypointInfo(resEPPaypointInfo);

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
                            ///End Insert EP PayPoint Data

                            //start gtp ko comment gareko
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
                                        gtBillNumber = billNumber;
                                        //string URIGetTran = "https://test.paypoint.md:4445/PayPointWS/PayPointMSOperations.asmx/GetTransaction";
                                        //For get transactionpaypoint link in webconfig
                                       // string URIGetTran = System.Web.Configuration.WebConfigurationManager.AppSettings["GTPPaypointUrl"];

                                        string GetTranParameters = "userLogin=" + userId + "&userPassword=" + userPassword + "&stan=" + "-1" + "&refStan=" + refStan + "&key=" + "" + "&billNumber=" + gtBillNumber;

                                        ////for get transaction payment request insert in database
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
                                            //retrievalReferenceReqGTP = fundtransfer.tid,
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
                                        //string statusResGTP = "";

                                        string billNumberResGTP = "";
                                        using (WebClient wcGetTran = new WebClient())
                                        {
                                            wcGetTran.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                                            //string HtmlResultGetTran = wcGetTran.UploadString(URIGetTran, GetTranParameters);

                                            
                                            if (ResultGTPDefaultValue == "000")
                                            {
                                                getTranResultResp = ResultGTPDefaultValue;
                                                keyGetTrancRlt = KeyGTPDefaultValue;

                                                 keyGetTrancRlt = qs["KeyGTPDefaultValue"];
                                                resultMessageResGTP = ResultMessageGTPDefaultValue;
                                                if (!(resultMessageResGTP == "No data"))
                                                {
                                                    billNumberResGTP = BillNumberGTPDefaultValue;

                                                    statusResGTP = StatusGTPDefaultValue;

                                                    //for get transaction payment status validation
                                                    if (!(statusResGTP == "1" || statusResGTP == "5" || statusResGTP == "11" || statusResGTP == "13"))
                                                    {
                                                        if (statusResGTP == "4" || statusResGTP == "6" || statusResGTP == "14" || statusResGTP == "16")
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
                                                resultMessageResGTP = ResultMessageGTPDefaultValue;

                                            }

                                            billNumberResGTP = BillNumberGTPDefaultValue;
                                            getTranResultResp = ResultGTPDefaultValue;
                                            keyGetTrancRlt = KeyGTPDefaultValue;
                                            //for get transaction payment response insert in database
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


                                    ///Start Insert GT PayPoint Data

                                    try
                                    {

                                        //int resultsReqEP = PaypointUtils.RequestEPPaypointInfo(reqEPPaypointInfo);
                                        //int resultsResEP = PaypointUtils.ResponseEPPaypointInfo(resEPPaypointInfo);
                                        //for get transaction payment request insert in database

                                        //string gtBillNumber = billNumber;
                                        //reqGTPaypointInfo = new PaypointModel()
                                        //{
                                        //    companyCodeReqGTP = companyCode,
                                        //    serviceCodeReqGTP = serviceCode,
                                        //    accountReqGTP = account,
                                        //    special1ReqGTP = special1,
                                        //    special2ReqGTP = special2,

                                        //    transactionDateReqGTP = transactionDate,
                                        //    transactionIdReqGTP = transactionId,
                                        //    userIdReqGTP = userId,
                                        //    userPasswordReqGTP = userPassword,
                                        //    salePointTypeReqGTP = salePointType,

                                        //    refStanReqGTP = refStan,
                                        //    amountReqGTP = amountInPaisa,
                                        //    billNumberReqGTP = gtBillNumber,
                                        //    //retrievalReferenceReqGTP = fundtransfer.tid,
                                        //    //retrievalReferenceReqGTP = tid,
                                        //    retrievalReferenceReqGTP = retrievalReference,
                                        //    remarkReqGTP = "Get Transaction Payment",
                                        //    UserName = mobile,
                                        //    ClientCode = ClientCode,
                                        //    paypointType = paypointType,
                                        //    //remarkReqGTP = "Get Transaction Payment"+keyExecRlt,

                                        //};
                                        int resultsReqGTP = PaypointUtils.RequestGTPaypointInfo(reqGTPaypointInfo);
                                        int resultsResGTP = PaypointUtils.ResponseGTPaypointInfo(resGTPaypointInfo);

                                        if ((resultsReqGTP > 0) && (resultsResGTP > 0)) //(resultsReqEP > 0) && (resultsResEP > 0) &&
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
                                    ///End Insert GT PayPoint Data
                                    ///
                                }

                            }
                            else //ELSE for (compResultResp == "000") i.e Error  in result of EP
                            {
                                //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                statusCode = "400";
                                //message = result;
                                message = resultMessageResEP;
                                failedmessage = message;
                            }
                            //end gtp ko comment gareko
                        }


                        else //ELSE FOR (rltCheckPaymt == "000")I.E Error  in result of CP
                        {
                            //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                            statusCode = "400";
                            // message = result;
                            message = resultMessageResCP;
                            // message = resultMessage;
                            //message = "Checkpaypoint failed";

                            failedmessage = message;
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
                //end all EP  and GT transaction

                //start reverse comment gareko
                //REverse Transaction 
                //if ((statusCode != "111") && (statusCode != "114") && (statusCode != "115") && (statusCode != "116") && (statusCode != "119") &&
                //    (statusCode != "121") && (statusCode != "163") && (statusCode != "180") && (statusCode != "181") && (statusCode != "182") &&
                //    (statusCode != "183") && (statusCode != "184") && (statusCode != "185") && (statusCode != "186") && (statusCode != "187") &&
                //    (statusCode != "188") && (statusCode != "189") && (statusCode != "190") && (statusCode != "800") && (statusCode != "902") &&
                //    (statusCode != "904") && (statusCode != "906") && (statusCode != "907") && (statusCode != "909") && (statusCode != "911") &&
                //    (statusCode != "913") && (statusCode != "90") && (statusCode != "91") && (statusCode != "94") && (statusCode != "95") &&
                //    (statusCode != "98") && (statusCode != "99") && (statusCodeBalance != "400") && (compResultResp != "000") && (statusCodeBalance != "400") && (statusCode != "200")
                //    )
                ////((statusCodeBalance != "400") && (compResultResp != "000")) || ((statusCodeBalance != "400") && (statusCode != "200")) ||
                ////if (statusCode != "116")
                //{
                //    //TraceIdGenerator traceRevid = new TraceIdGenerator();
                //    //tid = traceRevid.GenerateUniqueTraceID();
                //    tid = retrievalReference;
                //    if (sc == "00")
                //    {
                //        transactionType = "PayPoint Txfr to W2W";
                //    }
                //    else
                //    {
                //        sc = "01";
                //        transactionType = "PayPoint Txfr to W2B"; //B2W
                //    }

                //    FundTransfer fundtransferRev = new FundTransfer
                //    {
                //        tid = tid,
                //        sc = sc,
                //        mobile = da,//mobile
                //        da = mobile,//da
                //        amount = amount,
                //        pin = pin,
                //        note = "reverse " + note,
                //        sourcechannel = src
                //    };

                //    ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, fundtransfer);
                //    // CustActivityModel custsmsInfo = new CustActivityModel();

                //    //MNTransactionMaster validTransactionData = new MNTransactionMaster();

                //    if ((tid == null) || (sc == null) || (mobile == null) || (da == null) || (amount == null) || (pin == null) ||
                //    (src == null) || (double.Parse(amount) <= 0) || (vid == null))
                //    {
                //        // throw ex
                //        statusCode = "400";
                //        message = "Parameters Missing/Invalid";
                //        failedmessage = message;
                //    }
                //    if ((companyCode == null) || (companyCode == "") || (serviceCode == null) || (serviceCode == "") ||
                //        (account == null) || (special1 == null) || (special2 == null) || (transactionDate == null) || (transactionId == null) || (transactionId == "") ||
                //        (userId == null) || (userPassword == null) || (salePointType == null) || (userId == "") || (userPassword == "") || (salePointType == ""))
                //    {
                //        // throw ex
                //        statusCode = "400";
                //        message = "Parameters Missing/Invalid PayPoint";
                //        failedmessage = message;
                //    }
                //    else
                //    {
                //        //if (sc == "00")
                //        //{
                //        //    transactionType = "PayPoint Txfr to W2W";
                //        //}
                //        //else 
                //        //{
                //        //    sc = "01";
                //        //    transactionType = "PayPoint Txfr to W2B"; //B2W
                //        //}

                //        if (!(UserNameCheck.IsValidUser(mobile)))
                //        {
                //            // throw ex
                //            statusCode = "400";
                //            //message = "Transaction restricted to User";
                //            message = "Transaction only for User";
                //            failedmessage = message;
                //        }
                //        if (UserNameCheck.IsValidMerchant(da))
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
                //                MNFundTransfer mnft = new MNFundTransfer(tid, fundtransferRev.sc, fundtransferRev.mobile,
                //                    fundtransferRev.sa, fundtransferRev.amount, fundtransferRev.da, fundtransferRev.note, fundtransferRev.pin,
                //                    fundtransferRev.sourcechannel, "T", "PayPoint");
                //                var comfocuslog = new MNComAndFocusOneLog(mnft, DateTime.Now);
                //                var mncomfocuslog = new MNComAndFocusOneLogsController();
                //                //mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                //                result = mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                //                //end:Com focus one log//

                //                if (result == "Success")
                //                {
                //                    //NOTE:- may be need to validate before insert into reply typpe
                //                    //start:insert into reply type as HTTP//
                //                    var replyType = new MNReplyType(tid, "HTTP");
                //                    var mnreplyType = new MNReplyTypesController();
                //                    mnreplyType.InsertIntoReplyType(replyType);
                //                    //end:insert into reply type as HTTP//

                //                    MNMerchantsController getMerchantDetails = new MNMerchantsController();

                //                    string GetMerchantMobile = getMerchantDetails.PassVidToGetMerchantDetail(vid);
                //                    if (GetMerchantMobile != "" || GetMerchantMobile != null)
                //                    {
                //                        fundtransfer.da = GetMerchantMobile; //Set Destination Merchant Mobile number
                //                        GetMerchantName = getMerchantDetails.PassVIdToGetMerchantName(vid);
                //                    }
                //                    else
                //                    {
                //                        statusCode = "400";
                //                        message = "Destination Merchant Doesnot Exists";
                //                        mnft.Response = "Destination Merchant Doesnot Exists";
                //                        mnft.ResponseStatus(HttpStatusCode.BadRequest, mnft.Response);
                //                        result = mnft.Response;
                //                        failedmessage = message;
                //                    }


                //                    //start:insert into transaction master//
                //                    if (mnft.valid())
                //                    {
                //                        var transaction = new MNTransactionMaster(mnft);
                //                        var mntransaction = new MNTransactionsController();
                //                        //validTransactionData = mntransaction.Validate(transaction, mnft.pin);
                //                        validTransactionData = mntransaction.Validatepaypoint(transaction, mnft.pin);
                //                        result = validTransactionData.Response;
                //                        /*** ***/
                //                        ErrorMessage em = new ErrorMessage();

                //                        if (validTransactionData.Response == "Error")
                //                        {
                //                            mnft.Response = "error";
                //                            mnft.ResponseStatus(HttpStatusCode.InternalServerError,
                //                                "Internal server error - try again later, or contact support");
                //                            result = mnft.Response;
                //                            statusCode = "500";
                //                            message = "Internal server error - try again later, or contact support";
                //                            failedmessage = message;
                //                        }
                //                        else
                //                        {
                //                            if ((result == "Trace ID Repeated") || (result == "Limit Exceed")
                //                                || (result == "Invalid Source User") || (result == "Invalid Destination User")
                //                                || (result == "Invalid Product Request") || (result == "Please try again") || (result == ""))
                //                            {
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                //                                statusCode = "400";
                //                                message = result;
                //                                failedmessage = message;
                //                            }
                //                            if (result == "Invalid PIN")
                //                            {
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                //                                statusCode = "400";
                //                                message = result;
                //                                failedmessage = message;
                //                            }
                //                            if (result == "111")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_111;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "114")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_114;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "115")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_115;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "116")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_116;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "119")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_119;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "121")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_121;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "163")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_163;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "180")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_180;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "181")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_181;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "182")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_182;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "183")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_183;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "184")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_184;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "185")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_185;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "186")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_186;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "187")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_187;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "188")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_188;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "189")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_189;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "190")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_190;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "800")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_800;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "902")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_902;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "904")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_904;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "906")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_906;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "907")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_907;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "909")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_909;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "911")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_911;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "913")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_913;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "90")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_90;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "91")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_91;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "92")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_92;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "94")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_94;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "95")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_95;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "98")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_98;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            if (result == "99")
                //                            {
                //                                statusCode = result;
                //                                message = em.Error_99;
                //                                failedmessage = message;
                //                                mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                //                            }
                //                            else if (validTransactionData.ResponseCode == "OK")
                //                            {
                //                                statusCode = "200";
                //                                message = result;
                //                                mnft.ResponseStatus(HttpStatusCode.OK, message);

                //                            } //END ValidTransactionData.ResponseCode


                //                        } //END validTransactionData.Response WITHOUT MNDB ERROR
                //                          /*** ***/
                //                          /*** ***/

                //                        OutgoingWebResponseContext response1 = WebOperationContext.Current.OutgoingResponse;
                //                        if (response1.StatusCode == HttpStatusCode.OK)
                //                        {
                //                            string messagereply = "";
                //                            try
                //                            {
                //                                messagereply = "Dear " + CustCheckUtils.GetName(mobile) + "," + "\n";

                //                                messagereply += " You have successfully reverse  NPR " + validTransactionData.Amount
                //                                                + " to " +
                //                                                //GetMerchantName 
                //                                                "Utility payment for NEA." + " on date " +
                //                                                    (validTransactionData.CreatedDate).ToString("dd/MM/yyyy")
                //                                                + "." + "\n";
                //                                messagereply += "Thank you. MNepal";

                //                                var client = new WebClient();

                //                                //SENDER
                //                                if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                //                                {
                //                                    //FOR NCELL
                //                                    var content = client.DownloadString(
                //                                        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                //                                        + "977" + mobile + "&Text=" + messagereply + "");
                //                                }
                //                                else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                //                                            || (mobile.Substring(0, 3) == "986"))
                //                                {
                //                                    //FOR NTC
                //                                    var content = client.DownloadString(
                //                                        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                //                                        + "977" + mobile + "&Text=" + messagereply + "");
                //                                }

                //                                statusCode = "200";

                //                                var v = new
                //                                {
                //                                    StatusCode = Convert.ToInt32(statusCode),
                //                                    StatusMessage = result
                //                                };
                //                                result = JsonConvert.SerializeObject(v);

                //                            }
                //                            catch (Exception ex)
                //                            {
                //                                // throw ex
                //                                statusCode = "400";
                //                                message = ex.Message;
                //                            }


                //                            custsmsInfo = new CustActivityModel()
                //                            {
                //                                UserName = fundtransfer.mobile,
                //                                RequestMerchant = transactionType,
                //                                DestinationNo = fundtransfer.da,
                //                                Amount = validTransactionData.Amount.ToString(),
                //                                SMSStatus = "Success",
                //                                SMSSenderReply = messagereply,
                //                                ErrorMessage = "",
                //                            };


                //                        }
                //                        else if ((response1.StatusCode == HttpStatusCode.BadRequest) || (response1.StatusCode == HttpStatusCode.InternalServerError) || (statusCode != "200"))
                //                        {
                //                            custsmsInfo = new CustActivityModel()
                //                            {
                //                                UserName = mobile,
                //                                RequestMerchant = transactionType,
                //                                DestinationNo = fundtransfer.da,
                //                                Amount = validTransactionData.Amount.ToString(),
                //                                SMSStatus = "Failed",
                //                                SMSSenderReply = message,
                //                                ErrorMessage = failedmessage,
                //                            };

                //                        }
                //                        //end:insert into transaction master//

                //                    } //END:insert into transaction master//
                //                    else
                //                    {
                //                        mnft.Response = "error";
                //                        mnft.ResponseStatus(HttpStatusCode.BadRequest, "parameters missing/invalid"); //200 - OK
                //                        result = mnft.Response;
                //                        statusCode = "400";
                //                        message = "parameters missing/invalid";
                //                        failedmessage = message;

                //                        custsmsInfo = new CustActivityModel()
                //                        {
                //                            UserName = mobile,
                //                            RequestMerchant = transactionType,
                //                            DestinationNo = fundtransfer.da,
                //                            Amount = amount,
                //                            SMSStatus = "Failed",
                //                            SMSSenderReply = message,
                //                            ErrorMessage = failedmessage,
                //                        };
                //                    }

                //                } //END MNComAndFocusOneLogsController
                //                else
                //                {
                //                    statusCode = "400";
                //                    mnft.Response = "Data Insertion Failed in Check DB Connection: TraceID limit might have exceeded";
                //                    mnft.ResponseStatus(HttpStatusCode.InternalServerError, mnft.Response);
                //                    result = mnft.Response;
                //                    failedmessage = result;
                //                }

                //            } //END TRansLimit Check StatusCode N Message
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

                //        } //END IsValidMerchant


                //        //} //END Destination mobile No check

                //    }

                //}

                //end reverse comment gareko
                //end  transaction (change balance of customer)

                //start success bhayo bhanne sms pathaune
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
                                            "Utility payment for NEA." + " on date " +
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

                //end success bhayo bhanne sms pathaune

            }
            catch (Exception ex)
            {
                // throw ex
                statusCode = "400";
                message = ex.Message;
            }

            //end comment gareko 14th may issue




            

            //start if success send message to customer
            
            ///


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
        public string getDetailPayment(string userName, string clientCode, string tokenID, string account, string special1, string special2, string src, string rltCheckPaymt, string retrievalRef, string refStanGD) //Stream input
        {
            string PaypointPwd = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointPwd"];


            System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate { return true; }; //to prevent from SSL error
            ////StreamReader sr = new StreamReader(input);
            ////string s = sr.ReadToEnd();
            ////sr.Dispose();
            //NameValueCollection qs = HttpUtility.ParseQueryString(s);

            //string userName = qs["username"];
            //string clientCode = qs["clientCode"]; //clientCode

            //string src = qs["src"];
            string result = "";
            string sessionID = tokenID; // qs["tokenID"];
            string resultMessageResCP = "";

            ////string serviceCode = qs["serviceCode"]; //"1";// 
            //string account = qs["account"]; //"013.01.001";//
            //string special1 = qs["special1"]; //"217";//
            //string special2 = qs["special2"]; //"2300";//

            //rltCheckPaymt = "000";

            string totalAmount = string.Empty;
            string totalCount = string.Empty;
            string totalBAmount = string.Empty;
            string totalBCount = string.Empty;
            string balance = string.Empty;
            string message = string.Empty;
            string statusCode = string.Empty;
            string failedmessage = string.Empty;
            string GetMerchantName = string.Empty;
            string S_TotalAmountDue = string.Empty;
            double TotalAmountDueStr = 0.00;

            string customerNo = string.Empty;
            var resultList = new List<NEAFundTransfer>();

            //start all CP Transaction

            try
            {
                if (rltCheckPaymt == "000")
                {

                    //Session data set
                    string S_SCNo = account;// (string)Session["S_SCNo"];
                    string S_NEABranchName = special1; // (string)Session["S_NEABranchName"];
                    string S_CustomerID = special2; // (string)Session["S_CustomerID"];
                    if ((S_SCNo == null) || (S_NEABranchName == null) || (S_CustomerID == null) || (retrievalRef == null) || (refStanGD == null))
                    {
                        statusCode = "400";
                        message = resultMessageResCP;
                        failedmessage = message;

                    }
                    //End Session data set

                    NEAFundTransfer NEAObj = new NEAFundTransfer();
                    NEAObj.SCNo = S_SCNo;
                    NEAObj.NEABranchCode = S_NEABranchName;
                    NEAObj.CustomerID = S_CustomerID;
                    NEAObj.UserName = userName;
                    NEAObj.ClientCode = clientCode;
                    NEAObj.refStan = refStanGD;//PaypointUserModel.getrefStan(NEAObj);
                    NEAObj.retrievalReference = retrievalRef;

                    //Database Accessed
                    NEAFundTransfer regobj = new NEAFundTransfer();
                    DataSet DPaypointSet = PaypointUtils.GetNEADetails(NEAObj);
                    DataTable dResponse = DPaypointSet.Tables["dtResponse"];
                    DataSet DPaypointSetPay = PaypointUtils.GetNEADetailsPay(NEAObj);
                    DataTable dPayment = DPaypointSetPay.Tables["dtPayment"];
                    //End Database Accessed

                    string rowNo = (dPayment.Rows.Count).ToString();
                    int countROW = dPayment.Rows.Count;
                    List<NEAFundTransfer> ListDetails = new List<NEAFundTransfer>(countROW);
                    if (dResponse != null && dResponse.Rows.Count > 0)
                    {
                        regobj.SCNo = dResponse.Rows[0]["account"].ToString();
                        regobj.NEABranchCode = dResponse.Rows[0]["special1"].ToString();
                        regobj.CustomerID = dResponse.Rows[0]["special2"].ToString();
                        regobj.CustomerName = dResponse.Rows[0]["customerName"].ToString();
                        regobj.refStan = dResponse.Rows[0]["refStan"].ToString();
                        regobj.billNumber = dResponse.Rows[0]["billNumber"].ToString();
                        regobj.retrievalReference = dResponse.Rows[0]["retrievalReference"].ToString();
                        regobj.responseCode = dResponse.Rows[0]["responseCode"].ToString();
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
                            int S_TotalAmountDues = Convert.ToInt32(TotalAmountDue);
                            String[] Str_TotalAmountDue = TotalAmountDue.ToString().Split('.');
                            if (Str_TotalAmountDue.Length == 2)
                            {
                                S_TotalAmountDues = Convert.ToInt32(TotalAmountDue.ToString().Split('.')[0]) + 1;//adding 1 to decimal value
                            }
                            //end
                            S_TotalAmountDue = S_TotalAmountDues.ToString();
                            TotalAmountDueStr = TotalAmountDue;

                            //Payment table
                            //ViewBag.ListDetails = ListDetails;
                            //ListDetails = ListDetails;
                        }
                        else
                        {
                            //Converting paisa to rupee
                            regobj.totalAmount = dResponse.Rows[0]["amount"].ToString();
                            double TotalAmountDue = Convert.ToDouble(regobj.totalAmount.ToString());
                            TotalAmountDue = TotalAmountDue / 100;
                            //end Converting paisa to rupee
                            //splitting decimal values 
                            int S_TotalAmountDues = Convert.ToInt32(TotalAmountDue);
                            String[] Str_TotalAmountDue = TotalAmountDue.ToString().Split('.');
                            if (Str_TotalAmountDue.Length == 2)
                            {
                                S_TotalAmountDues = Convert.ToInt32(TotalAmountDue.ToString().Split('.')[0]) + 1;//adding 1 to decimal value
                            }
                            //end
                            S_TotalAmountDue = S_TotalAmountDues.ToString();
                            TotalAmountDueStr = TotalAmountDue;
                        }

                        UserInfo userInfo = new UserInfo();

                        MNBalance availBaln = new MNBalance();
                        DataTable dtableUser1 = PaypointUtils.GetAvailBaln(clientCode);
                        if (dtableUser1 != null && dtableUser1.Rows.Count > 0)
                        {
                            availBaln.amount = dtableUser1.Rows[0]["AvailBaln"].ToString();
                        }

                        statusCode = "200";
                        resultList.Add(new NEAFundTransfer
                        {
                            UserName = userName,
                            SCNo = regobj.SCNo,
                            NEABranchCode = regobj.NEABranchCode,
                            CustomerID = regobj.CustomerID,
                            CustomerName = regobj.CustomerName,
                            TotalAmountDue = regobj.TotalAmountDue,
                            refStan = regobj.refStan,
                            billNumber = regobj.billNumber,
                            retrievalReference = regobj.retrievalReference,
                            billAmount = regobj.totalAmount,
                            amount = availBaln.amount,
                            responseCode = regobj.responseCode,
                            nftList = ListDetails
                        });

                    }
                    else
                    {
                        statusCode = "400";
                        message = resultMessageResCP;
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

                    ///start for CP FAILED MESSSAGE

                    statusCode = "400";
                    //message = result;
                    message = "Check Payment Not Successfully";
                    failedmessage = message;

                    ///END for CP FAILED MESSSAGE

                }

            }
            catch (Exception ex)
            {
                message = result + ex + "Error Message ";
                //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                statusCode = "400";
                failedmessage = message;
            }


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






    }
}
