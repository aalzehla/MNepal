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
    public class paypointkhanepani
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
            string ClientCode = qs["ClientCode"];
            string paypointType = "Khanepani";
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

        #region"Check Paypoint Khnaepani"
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
            // string da = "9841356370";//user in silver line
            //string da = "9801000004";// scool merchant in silver line

            // string da = "9840066836";//merchant in 30
            //string da = "9813999353";//user in 30
            //string da = "9841671238";//user in 30

            string da = System.Web.Configuration.WebConfigurationManager.AppSettings["DestinationNoForPaypoint"];


            //string amount = "5";
            //string pin = "1111";

            //string note = qs["note"];
            string note = "utility payment for Khanepani. Customer Code=" + qs["account"];//+ ". " + qs["note"];
            string src = qs["src"];
            string result = "";
            string sessionID = qs["tokenID"];
            string resultMessageResCP = "";

            string companyCode = qs["companyCode"]; //"598";//
                                                    //string serviceCode = qs["special1"]; //"1";// 
            string serviceCode = qs["serviceCode"]; //"1";// 
            string account = qs["account"]; //"013.01.001";//
            string special1 = qs["special1"]; //"217";//
            string special2 = qs["special2"]; //"2300";// 
            string transactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");//"2019-11-22T11:11:02";
            long millisecondstrandId = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string transactionId = millisecondstrandId.ToString(); //"120163339819";
            //string userId = "MNepalLT";
            string userId = PaypointUserID;
            //string userPassword = "MNepalLT";
            string userPassword = PaypointPwd.Trim();
            string salePointType = "6";
            string ClientCode = qs["ClientCode"];
            //string paypointType = "Khanepani";
            string paypointType = "2";
            string transactionType = string.Empty;

            PaypointModel reqCPPaypointKhanepaniInfo = new PaypointModel();
            PaypointModel resCPPaypointKhanepaniInfo = new PaypointModel();

            PaypointModel resPaypointKhanepaniInvoiceInfo = new PaypointModel();


            string totalAmount = string.Empty;
            string totalCount = string.Empty;
            string totalBAmount = string.Empty;
            string totalBCount = string.Empty;
            string balance = string.Empty;
            string message = string.Empty;
            string statusCode = string.Empty;
            string failedmessage = string.Empty;
            string customer_nameKI2 = string.Empty;
            string GetMerchantName = string.Empty;
            string retrievalRef = string.Empty;
            string refStanCK = string.Empty;

            string customerNo = string.Empty;
            TraceIdGenerator traceid = new TraceIdGenerator();
            tid = traceid.GenerateUniqueTraceID();

            //if (TokenGenerator.TokenChecker(sessionID, mobile, src) == false)
            //{
            //    // throw ex
            //    statusCode = "400";
            //    message = "Session expired. Please login again";
            //    failedmessage = message;
            //}
            //else
            //{

            //for CP transaction for khanepani

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
                reqCPPaypointKhanepaniInfo = new PaypointModel()
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

                string statusKI = "";// status value inside invoice of Khanepani
                string total_advance_amountKI = "";
                string customer_codeKI = "";
                string addressKI = "";
                string total_credit_sales_amountKI = "";

                string customer_nameKI = "";
                string current_month_duesKI = "";
                string mobile_numberKI = "";
                string total_duesKI = "";
                string previous_duesKI = "";

                string current_month_discountKI = "";
                string current_month_fineKI = "";


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
                        //customerName = xElem.Descendants().Elements("ReserveInfo").Where(x => x.Name == "ReserveInfo").SingleOrDefault().Value;

                        var invoice = xElem.Descendants().Elements("Invoice");


                        statusKI = invoice.Elements("status").SingleOrDefault().Value;
                        total_advance_amountKI = invoice.Elements("total_advance_amount").SingleOrDefault().Value;
                        customer_codeKI = invoice.Elements("customer_code").SingleOrDefault().Value;
                        addressKI = invoice.Elements("address").SingleOrDefault().Value;
                        total_credit_sales_amountKI = invoice.Elements("total_credit_sales_amount").SingleOrDefault().Value;

                        customer_nameKI = invoice.Elements("customer_name").SingleOrDefault().Value;
                        //customer_nameKI = "गोदावरी न.पा.- 2, महरा कटान";
                        current_month_duesKI = invoice.Elements("current_month_dues").SingleOrDefault().Value;
                        mobile_numberKI = invoice.Elements("mobile_number").SingleOrDefault().Value;
                        total_duesKI = invoice.Elements("total_dues").SingleOrDefault().Value;
                        previous_duesKI = invoice.Elements("previous_dues").SingleOrDefault().Value;

                        current_month_discountKI = invoice.Elements("current_month_discount").SingleOrDefault().Value;
                        current_month_fineKI = invoice.Elements("current_month_fine").SingleOrDefault().Value;


                        //for checkpayment khanepani invoice response insert in database

                        resPaypointKhanepaniInvoiceInfo = new PaypointModel()
                        {
                            statusKI = statusKI,
                            total_advance_amountKI = total_advance_amountKI,
                            customer_codeKI = customer_codeKI,
                            addressKI = addressKI,
                            total_credit_sales_amountKI = total_credit_sales_amountKI,

                            customer_nameKI = customer_nameKI,
                            current_month_duesKI = current_month_duesKI,
                            mobile_numberKI = mobile_numberKI,
                            total_duesKI = total_duesKI,
                            previous_duesKI = previous_duesKI,


                            current_month_discountKI = current_month_discountKI,
                            current_month_fineKI = current_month_fineKI,
                            refStan = refStan,
                            UserName = mobile,
                            ClientCode = ClientCode,

                        };

                        int resultsPayments = PaypointUtils.PaypointKhanepaniInvoiceInfo(resPaypointKhanepaniInvoiceInfo);

                    }
                    else
                    {
                        keyrlt = xElem.Attribute("Key").Value;
                        resultMessageResCP = xElem.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;
                    }
                    resultMessageResCP = xElem.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;
                    //customerName = xElem.Descendants().Elements("ReserveInfo").Where(x => x.Name == "ReserveInfo").SingleOrDefault().Value;

                    //for checkpayment response insert in database
                    resCPPaypointKhanepaniInfo = new PaypointModel()
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
                        customerNameCP = customer_nameKI,
                        UserName = mobile,
                        ClientCode = ClientCode,
                        paypointType = paypointType,
                        resultMessageResCP = resultMessageResCP,

                    };


                }


                if (!(rltCheckPaymt == "000"))
                {
                    //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK//pachi thapeko
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
                    customer_nameKI2 = customer_nameKI;
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

            ///for  inserting CP PayPoint Data of khanepani which is simlar to nea ,nepalwater

            try
            {
                int resultsReqCP = PaypointUtils.RequestCPPaypointInfo(reqCPPaypointKhanepaniInfo);
                int resultsResCP = PaypointUtils.ResponseCPPaypointInfo(resCPPaypointKhanepaniInfo);


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

            //}


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
                    customer_nameKI = customer_nameKI2,
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

            DelayForSec(15000);
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

            string da = System.Web.Configuration.WebConfigurationManager.AppSettings["DestinationNoForPaypoint"];


            string amount = qs["amount"];//amount paid by customer
            string pin = qs["pin"];
            string note = "Utility payment for Khanepani. Customer Code=" + qs["account"];//+ ". " + qs["note"];

            string src = qs["src"];
            string result = "";
            string sessionID = qs["tokenID"];
            string resultMessageResCP = "";

            string companyCode = qs["companyCode"]; //"761";//
            string serviceCode = qs["serviceCode"]; //"7";// 
            string account = qs["account"]; //"1212";//
            string special1 = qs["special1"]; //"12";//
            string special2 = qs["special2"]; //"";// 
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
            string paypointType = "2";
            string customerName = qs["customerName"];
            string walletBalance = qs["walletBalance"];//in Rs.
            string retrievalReference = qs["retrievalReference"];
            int walletBalancePaisaInt = 0;
            walletBalancePaisaInt = Convert.ToInt32((Convert.ToDouble(walletBalance)) * 100);
            int amountpayInt = Convert.ToInt32(amountpay);
            PaypointModel reqEPPaypointKhanepaniInfo = new PaypointModel();
            PaypointModel resEPPaypointKhanepaniInfo = new PaypointModel();

            PaypointModel reqGTPaypointKhanepaniInfo = new PaypointModel();
            PaypointModel resGTPaypointKhanepaniInfo = new PaypointModel();
            //start EGTP
            PaypointModel resGTAllPaypointKhanepaniInfo = new PaypointModel();
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


            //if (TokenGenerator.TokenChecker(sessionID, mobile, src) == false)
            //{
            //    // throw ex
            //    statusCode = "400";
            //    message = "Session expired. Please login again";
            //    failedmessage = message;
            //}
            //else
            //{

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
                                message = "Invalid PIN! You have already attempt 3 times with wrong PIN,Please try again after 1 hour";
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
                                message = "Invalid PIN! You have already attempt 3 times with wrong PIN,Please try again after 1 hour";
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
                string compResultResp = "";
                if (statusCode == "200")
                {
                    //start ep gt khanepani
                    try
                    {
                        string amountInPaisa = (Convert.ToInt32(amount) * 100).ToString();

                        if (rltCheckPaymt == "000")
                        {
                            //    if (amountpay != "0")
                            //    {
                            int amountInPaisaInt = Convert.ToInt32(amountInPaisa);
                            //int amountpayInt = Convert.ToInt32(amountpay);

                            //if (amountInPaisaInt > amountpayInt)
                            //{

                            //amountpay = (Convert.ToInt32(amount) * 100).ToString();

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

                                    // string URIEXECPayment = "https://test.paypoint.md:4445/PayPointWS/PayPointMSOperations.asmx/ExecutePayment";
                                    //For excutepaypoint link in webconfig
                                    string URIEXECPayment = System.Web.Configuration.WebConfigurationManager.AppSettings["EPPaypointUrl"];

                                    string execPayParameters = "companyCode=" + companyCode + "&serviceCode=" + serviceCode +
                                                "&account=" + account + "&special1=" + special1 + "&special2=" + special2 +
                                                "&transactionDate=" + exectransactionDate + "&transactionId=" + exectransactionId +
                                                "&refStan=" + refStan + "&amount=" + amountInPaisa + "&billNumber=" + billNumber +
                                                "&userId=" + userId.Trim() + "&userPassword=" + userPassword.Trim() + "&salePointType=" + salePointType;

                                    //for executepayment request insert in database
                                    reqEPPaypointKhanepaniInfo = new PaypointModel()
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
                                        //customerName = xElemEPay.Descendants().Elements("ReserveInfo").Where(x => x.Name == "ReserveInfo").SingleOrDefault().Value;
                                        //customerName = "";

                                        //for Response Execute Payment
                                        resEPPaypointKhanepaniInfo = new PaypointModel()
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
                                            //retrievalReferenceResEP = tid,
                                            retrievalReferenceResEP = retrievalReference,
                                            //retrievalReferenceResEP = fundtransfer.tid,
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
                                int resultsReqEP = PaypointUtils.RequestEPPaypointInfo(reqEPPaypointKhanepaniInfo);
                                int resultsResEP = PaypointUtils.ResponseEPPaypointInfo(resEPPaypointKhanepaniInfo);

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
                                        //string URIGetTran = "https://test.paypoint.md:4445/PayPointWS/PayPointMSOperations.asmx/GetTransaction";


                                        string GetTranParameters = "userLogin=" + userId + "&userPassword=" + userPassword + "&stan=" + "-1" + "&refStan=" + refStan + "&key=" + "" + "&billNumber=" + gtBillNumber;
                                        //For get transactionpaypoint link in webconfig
                                        string URIGetTran = System.Web.Configuration.WebConfigurationManager.AppSettings["GTPPaypointUrl"];


                                        //for get transaction payment request insert in database
                                        reqGTPaypointKhanepaniInfo = new PaypointModel()
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
                                                //FOR  get transaction response khanepani all

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
                                                resGTAllPaypointKhanepaniInfo = new PaypointModel()
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
                                                    Mode = "KPGTRes",

                                                };



                                                //end EGTP

                                                if (!(resultMessageResGTP == "No data"))
                                                {
                                                    billNumberResGTP = xElemGPay.Descendants().Elements("BillNumber").Where(x => x.Name == "BillNumber").SingleOrDefault().Value;

                                                    statusResGTP = xElemGPay.Descendants().Elements("Status").Where(x => x.Name == "Status").SingleOrDefault().Value;

                                                    //FOR get transaction status validation
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
                                                resultMessageResGTP = xElemGPay.Descendants().Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;

                                            }







                                            ////for get transaction payment response insert in database
                                            resGTPaypointKhanepaniInfo = new PaypointModel()
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

                                        int resultsReqGTP = PaypointUtils.RequestGTPaypointInfo(reqGTPaypointKhanepaniInfo);
                                        int resultsResGTP = PaypointUtils.ResponseGTPaypointInfo(resGTPaypointKhanepaniInfo);
                                        //START EGTP
                                        int resultsResGTPAll = PaypointUtils.ResponseGTAllPaypointInfo(resGTAllPaypointKhanepaniInfo);
                                        //END EGTP
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

                            //if (compResultResp == "000")
                            //{
                            //    statusCode = "200";
                            //    message = result;
                            //    mnft.ResponseStatus(HttpStatusCode.OK, message); //200 - OK
                            //}
                            else
                            {
                                //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                statusCode = "400";
                                //message = result;
                                message = resultMessageResEP;
                                failedmessage = message;
                            }


                            //}
                            //else //Error input amount not suffient then the response amount of CP
                            //{
                            //    mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                            //    statusCode = "400";
                            //    message = "Insufficient Funds";
                            //    failedmessage = message;
                            //}

                            //    }
                            //    else //Error  amount 0 in response of CP
                            //    {
                            //        mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                            //        statusCode = "400";

                            //        message = "You have no pending bill right now";
                            //        failedmessage = message;
                            //    }
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
                        //failedmessage = message;
                        failedmessage = "Please try again.";
                    }
                }
                //end ep gt khanepani

                //for sending sms  if success 
                DelayForSec(8000);
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
                                            "Utility payment for Khanepani." + " on date " +
                                                (validTransactionData.CreatedDate).ToString("dd/MM/yyyy")
                                            + "." + "\n";
                            messagereply += "Thank you. MNepal";

                            var client = new WebClient();

                            //SENDER
                            if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                            {
                                //FOR NCELL
                                //var content = client.DownloadString(
                                //    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                //    + "977" + mobile + "&Text=" + messagereply + "");
                                var content = client.DownloadString(
                                    SMSNCELL + "977" + mobile + "&Text=" + messagereply + "");
                            }
                            else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                        || (mobile.Substring(0, 3) == "986"))
                            {
                                //FOR NTC
                                //var content = client.DownloadString(
                                //    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                //    + "977" + mobile + "&Text=" + messagereply + "");
                                var content = client.DownloadString( 
                                    SMSNTC + "977" + mobile + "&Text=" + messagereply + "");
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
                                message = "Invalid PIN! You have already attempt 3 times with wrong PIN,Please try again after 1 hour";
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
                                message = "Invalid PIN! You have already attempt 3 times with wrong PIN,Please try again after 1 hour";
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
                                                                "Utility payment for Khanepani." + " on date " +
                                                                    (validTransactionData.CreatedDate).ToString("dd/MM/yyyy")
                                                                + "." + "\n";
                                                messagereply += "Thank you. MNepal";

                                                var client = new WebClient();

                                                //SENDER
                                                if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                                                {
                                                    //FOR NCELL
                                                    var content = client.DownloadString(
                                                        SMSNCELL
                                                        + "977" + mobile + "&Text=" + messagereply + "");
                                                    //"http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                                    //+ "977" + mobile + "&Text=" + messagereply + "");
                                                }
                                                else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                                            || (mobile.Substring(0, 3) == "986"))
                                                {
                                                    //FOR NTC
                                                    var content = client.DownloadString(
                                                        SMSNTC
                                                        + "977" + mobile + "&Text=" + messagereply + "");
                                                    //"http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                                    //    + "977" + mobile + "&Text=" + messagereply + "");
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
                                                                    "Utility payment for Khanepani." + " on date " +
                                                                    (validTransactionData.CreatedDate).ToString("dd/MM/yyyy")
                                                                + "." + "\n";
                                                messagereply += "Thank you. MNepal";

                                                var client = new WebClient();

                                                //SENDER
                                                if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                                                {
                                                    //FOR NCELL
                                                    //var content = client.DownloadString(
                                                    //    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                                    //    + "977" + mobile + "&Text=" + messagereply + "");
                                                    var content = client.DownloadString(
                                                                SMSNCELL
                                                                + "977" + mobile + "&Text=" + messagereply + "");
                                                }
                                                else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                                            || (mobile.Substring(0, 3) == "986"))
                                                {
                                                    //FOR NTC
                                                    //var content = client.DownloadString(
                                                    //    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                                    //    + "977" + mobile + "&Text=" + messagereply + "");
                                                    var content = client.DownloadString(
                                                                SMSNTC
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
        public string getDetailPayment(string userName, string clientCode, string tokenID, string khanepaniCounter, string customerID, string months, string src, string rltCheckPaymt, string retrievalRef, string refStanGD) //Stream input
        {
            string PaypointPwd = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointPwd"];
            System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate { return true; }; //to prevent from SSL error

            string result = "";
            string sessionID = tokenID;
            string resultMessageResCP = "";

            string balance = string.Empty;
            string message = string.Empty;
            string statusCode = string.Empty;
            string failedmessage = string.Empty;

            var resultList = new List<Khanepani>();

            //start all GD Transaction

            //Session data set
            string S_KhanepaniCounter = khanepaniCounter;
            string S_CustomerID = customerID;
            string S_Months = months;
            if ((S_KhanepaniCounter == null) || (S_CustomerID == null) || (S_Months == null) || (clientCode == null) || (retrievalRef == null) || (refStanGD == null))
            {
                statusCode = "400";
                message = resultMessageResCP;
                failedmessage = message;

            }
            //End Session data set

            try
            {
                if (rltCheckPaymt == "000")
                {
                    Khanepani KPObj = new Khanepani();
                    KPObj.KhanepaniCounter = S_KhanepaniCounter;
                    KPObj.CustomerID = S_CustomerID;
                    KPObj.UserName = userName;
                    KPObj.ClientCode = clientCode;
                    KPObj.refStan = refStanGD; //PaypointUserModel.getKPrefStan(KPObj);
                    KPObj.retrievalReference = retrievalRef;

                    //Database Accessed
                    Khanepani regobj = new Khanepani();
                    DataSet DPaypointSet = PaypointUtils.GetKPDetails(KPObj);
                    DataTable dResponse = DPaypointSet.Tables["dtResponse"];
                    DataSet DPaypointSetPay = PaypointUtils.GetKPDetailsPay(KPObj);
                    DataTable dKhanepaniInvoice = DPaypointSetPay.Tables["dtKhanepaniInvoice"];
                    //End Database Accessed
                    if (dResponse != null && dResponse.Rows.Count > 0)
                    {
                        regobj.CustomerID = dResponse.Rows[0]["account"].ToString();
                        regobj.TotalAmountDue = dResponse.Rows[0]["amount"].ToString();
                        regobj.KpBranchCode = dResponse.Rows[0]["serviceCode"].ToString();

                        regobj.refStan = dResponse.Rows[0]["refStan"].ToString();
                        regobj.billNumber = dResponse.Rows[0]["billNumber"].ToString();
                        regobj.retrievalReference = dResponse.Rows[0]["retrievalReference"].ToString();
                        regobj.responseCode = dResponse.Rows[0]["responseCode"].ToString();
                        if (dKhanepaniInvoice != null && dKhanepaniInvoice.Rows.Count > 0)
                        {
                            regobj.status = dKhanepaniInvoice.Rows[0]["status"].ToString();
                            regobj.total_advance_amount = dKhanepaniInvoice.Rows[0]["total_advance_amount"].ToString();
                            regobj.customer_code = dKhanepaniInvoice.Rows[0]["customer_code"].ToString();
                            regobj.address = dKhanepaniInvoice.Rows[0]["address"].ToString();
                            regobj.total_credit_sales_amount = dKhanepaniInvoice.Rows[0]["total_credit_sales_amount"].ToString();
                            regobj.customer_name = dKhanepaniInvoice.Rows[0]["customer_name"].ToString();
                            regobj.current_month_dues = dKhanepaniInvoice.Rows[0]["current_month_dues"].ToString();
                            regobj.mobile_number = dKhanepaniInvoice.Rows[0]["mobile_number"].ToString();
                            regobj.total_dues = dKhanepaniInvoice.Rows[0]["total_dues"].ToString();
                            regobj.previous_dues = dKhanepaniInvoice.Rows[0]["previous_dues"].ToString();
                            regobj.current_month_discount = dKhanepaniInvoice.Rows[0]["current_month_discount"].ToString();
                            regobj.current_month_fine = dKhanepaniInvoice.Rows[0]["current_month_fine"].ToString();
                        }
                        else
                        {
                            statusCode = "400";
                            message = "Khanepani Details Not found.";
                            failedmessage = message;
                        }


                        //string KpBranchName = getKpBranchName(regobj.KpBranchCode.ToString());
                        string KpBranchCode = regobj.KpBranchCode.ToString();
                        string CustomerID = regobj.CustomerID;
                        string CustomerName = regobj.CustomerName;
                        double TotalAmountDue = Convert.ToDouble(regobj.TotalAmountDue.ToString());
                        TotalAmountDue = TotalAmountDue / 100;
                        double TotalAmountDues = TotalAmountDue;

                        //Viewbag For details from Khanepani invoice
                        string status = regobj.status.ToString();
                        string total_advance_amount = regobj.total_advance_amount.ToString();
                        string customer_code = regobj.customer_code.ToString();
                        string address = regobj.address.ToString();
                        string total_credit_sales_amount = regobj.total_credit_sales_amount.ToString();
                        string customer_name = regobj.customer_name.ToString();
                        string current_month_dues = regobj.current_month_dues.ToString();
                        string mobile_number = regobj.mobile_number.ToString();
                        string total_dues = regobj.total_dues.ToString();
                        string previous_dues = regobj.previous_dues.ToString();
                        string current_month_discount = regobj.current_month_discount.ToString();
                        string current_month_fine = regobj.current_month_fine.ToString();

                        regobj.KhanepaniCounter = KPObj.KhanepaniCounter;

                        UserInfo userInfo = new UserInfo();

                        MNBalance availBaln = new MNBalance();
                        DataTable dtableUser1 = PaypointUtils.GetAvailBaln(clientCode);
                        if (dtableUser1 != null && dtableUser1.Rows.Count > 0)
                        {
                            availBaln.amount = dtableUser1.Rows[0]["AvailBaln"].ToString();
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

                        statusCode = "200";
                        resultList.Add(new Khanepani
                        {
                            UserName = userName,
                            KhanepaniCounter = regobj.KhanepaniCounter,
                            CustomerID = regobj.CustomerID,
                            CustomerName = regobj.customer_name,
                            TotalAmountDue = regobj.TotalAmountDue,
                            KpBranchCode = regobj.KpBranchCode,
                            refStan = regobj.refStan,
                            billNumber = regobj.billNumber,
                            retrievalReference = regobj.retrievalReference,
                            status = regobj.status,
                            total_advance_amount = regobj.total_advance_amount,
                            customer_code = regobj.customer_code,
                            address = regobj.address,
                            total_credit_sales_amount = regobj.total_credit_sales_amount,
                            current_month_dues = regobj.current_month_dues,
                            mobile_number = regobj.mobile_number,
                            total_dues = regobj.total_dues,
                            previous_dues = regobj.previous_dues,
                            current_month_discount = regobj.current_month_discount,
                            current_month_fine = regobj.current_month_fine,
                            amount = availBaln.amount,
                            responseCode = regobj.responseCode
                        });

                    }
                    else
                    {
                        statusCode = "400";
                        message = resultMessageResCP;
                        failedmessage = message;
                    }

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
