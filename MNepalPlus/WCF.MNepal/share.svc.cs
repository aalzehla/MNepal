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
using System.Text;
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

    public class share
    {
        #region"Check Demat Payment"
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

            string BoId = qs["BoId"];
            string bankCode = qs["bankCode"];
            string userName = qs["mobile"];
            string tId = qs["tId"];
            string tokenId = qs["tokenID"];
            string paymentType = qs["paymentType"];
            string clientCode = qs["clientCode"];
            string sc = "00";

            string destinationNumber = System.Web.Configuration.WebConfigurationManager.AppSettings["DestinationNoForTestServer"];  //9849931345
            string note = "payment for Demat account. Demat Number=" + BoId;
            string result = "";
            string resultMessageResCP = "";
            string transactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");//"2019-11-22T11:11:02";
            long millisecondstrandId = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string transactionId = millisecondstrandId.ToString(); //"120163339819";
            string userId = PaypointUserID;
            //string userId = "MNepalLT";
            //string userPassword = "MNepalLT";
            string userPassword = PaypointPwd.Trim();
            string transactionType = string.Empty;
            string tid = "";

            string totalAmount = "";
            string message = "";
            string statusCode = "";
            string retrievalRef = "";
            string refStanCk = "";
            string merchantName = "";
            string customerNo = "";

            TraceIdGenerator traceid = new TraceIdGenerator();
            tid = traceid.GenerateUniqueTraceID();

            //for checkpayment for Demat account
            try
            {
                string DematPendingPayments = System.Web.Configuration.WebConfigurationManager.AppSettings["DematPendingPayments"];
                string parameter = "Id=" + BoId;

                string URL = DematPendingPayments + BoId;

                //for checkpayment insert in database
                DematModel reqCheckPaymentPDematInfo = new DematModel();
                DematModel resCheckPaymentDematInfo = new DematModel();

                //reqCPDematInfo = new DematModel()
                //{
                //    BoId = BoId,
                //    DematName = 
                //}

                string billNumber = "";
                string amountpay = "";
                string refStan = "";
                string exectransactionId = ""; //Unique
                string exectransactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"); //Current DateTime
                string rltCheckPaymt = "";
                string customerName = "";

                using (WebClient client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    var AuthUsername = System.Web.Configuration.WebConfigurationManager.AppSettings["AuthUsername"];
                    var AuthPassword = System.Web.Configuration.WebConfigurationManager.AppSettings["AuthPassword"];
                    //client.Credentials = new NetworkCredential(AuthUsername, AuthPassword);
                    //var byteArray = new UTF8Encoding().GetBytes(client.Credentials);
                    string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(AuthUsername + ":" + AuthPassword));

                    client.Headers[HttpRequestHeader.Authorization] = string.Format("Basic {0}", credentials);


                    var JsonData = client.DownloadString(URL);  //response from checkpayment

                    Dematobject demt = JsonConvert.DeserializeObject<Dematobject>(JsonData);

                    var DematId = demt.BoId;
                    var DematName = demt.DematName;
                    var TotalAmount = demt.TotalAmount;

                    //Fees array from response
                    //Fees array to list
                    //List of array to string builder
                    List<Fee> fee = new List<Fee>();
                    string stringBuilder = "";
                    foreach (var item in demt.Fees)
                    {
                        string concatenateFees = "";
                        Fee fees = new Fee();
                        fees.FiscalYear = item.FiscalYear;
                        fees.Description = item.Description;
                        fees.Amount = item.Amount;
                        fee.Add(fees);

                        //string builder
                        concatenateFees = "Fiscal Year: " + fees.FiscalYear + " " + "Description: " + fees.Description + " " + "Amount: " + fees.Amount;
                        stringBuilder = stringBuilder + concatenateFees + Environment.NewLine;
                    }



                    resCheckPaymentDematInfo = new DematModel()
                    {
                        BoId = DematId,
                        DematName = DematName,
                        TotalAmount = TotalAmount.ToString(),
                        Fees = stringBuilder,
                        ClientCode = clientCode,
                        RetrievalRef = tid,
                        UserName = userName,
                        BankCode = bankCode,
                        TimeStamp = Constants.getTimeStamp()

                    };

                    int resultPayments = DematUtils.DematPaymentInfo(resCheckPaymentDematInfo);

                    var v = new
                    {
                        statusCode = Convert.ToInt32("200"),
                        statusMessage = "Success",
                        retrievalRef = resCheckPaymentDematInfo.RetrievalRef,
                        tranId = tId,
                        timeStamp = resCheckPaymentDematInfo.TimeStamp,
                        boId = BoId,
                        fees = fee,
                        dematName = DematName

                    };

                    result = JsonConvert.SerializeObject(v);
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
            return result;
        }

        // Add more operations here and mark them with [OperationContract]
        #endregion

        #region"execute DematPayment"
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
            string vid = qs["vid"];
            string BoId = qs["BoId"];
            string special1 = "";
            string account = qs["account"];
            string DematBank = qs["DematBank"];
            string mobile = qs["mobile"];
            string sc = qs["sc"];
            string amount = qs["amount"];
            string pin = qs["pin"];
            pin = HashAlgo.Hash(pin);
            string note = "Demat Payment for BoId=" + qs["BoId"];//+ ". " + qs["note"];
            string ToKenId = qs["tokenID"];
            string tid = qs["tid"];
            string RetrievalReference = qs["RetrievalReference"];
            string ClientCode = qs["ClientCode"];
            string TimeStamp = qs["TimeStamp"];
            string WalletBalance = qs["walletBalance"];
            string da = qs["da"];
            string paymentType = qs["paymentType"];
            string src = qs["src"];
            string resultMessageResCP = "";

            string destinationNumber = System.Web.Configuration.WebConfigurationManager.AppSettings["DestinationNoForTestServer"];

            DematModel reqEPDematInfo = new DematModel();
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
            string userId = PaypointUserID;
            string userPassword = PaypointPwd.Trim();
            string transactionType = string.Empty;
            string result = "";

            double walletBalance = Convert.ToDouble(WalletBalance);
            double amountpayInt = Convert.ToDouble(amount);

            //int walletBalancePaisaInt = 0;
            //walletBalancePaisaInt = Convert.ToInt32((Convert.ToDouble(walletBalance)) * 100);
            //int amountpayInt = Convert.ToDouble(amount);

            string transactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");//"2019-11-22T11:11:02";
            long millisecondstrandId = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string transactionId = millisecondstrandId.ToString(); //"120163339819";
            string exectransactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            string exectransactionId = millisecondstrandId.ToString();

            // TraceIdGenerator traceid = new TraceIdGenerator();
            //tid = traceid.GenerateUniqueTraceID();
            tid = RetrievalReference;
            FundTransfer fundtransfer = new FundTransfer
            {
                tid = tid,
                sc = sc,
                mobile = mobile,
                da = destinationNumber,
                amount = amount,
                pin = pin,
                note = note
                //sourcechannel = src
            };

            if (sc == "00")
            {
                //if (walletBalance >= amountpayInt)// if wallet balance less then bill amount then show error msg
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
                    ((double.Parse(amount) <= 0)))
                    {
                        // throw ex
                        statusCode = "400";
                        message = "Parameters Missing/Invalid";
                        failedmessage = message;
                    }
                    if ((BoId == null) || (DematBank == "") || (mobile == null) || (transactionDate == null) || (transactionId == null) ||
                        (ClientCode == null) || (mobile == "") || (paymentType == "") || (userPassword == null))
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
                            transactionType = "Demat Txfr to W2W";
                        }
                        else if (sc == "10")
                        {
                            transactionType = "Demat Txfr to B2W"; //B2W
                        }

                        //change here
                        //if (!(UserNameCheck.IsValidUser(mobile)))
                        //{
                        //    // throw ex
                        //    statusCode = "400";
                        //    //message = "Transaction restricted to User";
                        //    message = "Transaction only for User";
                        //    failedmessage = message;
                        //}

                        //change starts here
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
                                    fundtransfer.sourcechannel, "Share");   //change
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
                        ((double.Parse(amount) <= 0)))
                    {
                        // throw ex
                        statusCode = "400";
                        message = "Parameters Missing/Invalid";
                        failedmessage = message;
                    }
                    if ((BoId == null) || (DematBank == "") || (mobile == null) || (transactionDate == null) || (transactionId == null) ||
                        (ClientCode == null) || (mobile == "") || (paymentType == "") || (userPassword == null))
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
                            transactionType = "Demat Txfr to W2W";
                        }
                        else if (sc == "10")
                        {
                            transactionType = "Demat Txfr to B2W"; //B2W
                        }

                        //change here
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
                                    fundtransfer.sourcechannel, "Share");  //change here
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


                                        var transactionpaypoint = new MNTransactionMaster(mnft);  //change here
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

            try
            {
                string compResultResp = "";
                if (statusCode == "200")
                {
                    try
                    {
                        string amountInPaisa = ((double.Parse(amount)) * 100).ToString();
                        //if (rltCheckPaymt == "000")  //change
                        //{
                            int amountInPaisaInt = Convert.ToInt32(amountInPaisa);

                            long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                            exectransactionId = milliseconds.ToString();

                            string keyExecRlt = "";
                            string resultMessageResEP = "";
                            do
                            {
                                if ((BoId == null) || (DematBank == "") || (mobile == null) || (transactionDate == null) || (transactionId == null) ||
                                (ClientCode == null) || (mobile == "") || (paymentType == "") || (userPassword == null))
                                {
                                    // throw ex
                                    statusCode = "400";
                                    message = "Parameters Missing/Invalid PayPoint";
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
                                    //    special1 = special1.ToString();   //change
                                    //}


                                    //for demat configuration
                                    Constants constants = new Constants();
                                    DematModel dmatPayment = new DematModel();
                                    var API_Key = System.Configuration.ConfigurationManager.AppSettings["DematAPIKey"];
                                    DateTime aDate = DateTime.Now;
                                    var getTimeStamp = aDate.ToString("yyyy-MM-ddTHH:mm:ss");

                                    //For excutepaypoint link in webconfig
                                    string URIEXECPayment = System.Web.Configuration.WebConfigurationManager.AppSettings["DematPayment"];
                                    var getHashValue = Constants.ComputeSha256Hash(BoId + "-" + float.Parse(amount) + "-" + getTimeStamp + "-" + RetrievalReference + "-" + API_Key);

                                    string execPayParameters = "BoId=" + BoId + "&Amount=" + amount + "&ReferenceId=" + RetrievalReference + "&HashValue=" + getHashValue + "&TimeStamp=" + getTimeStamp;
                                    //end demat configuration

                                    using (WebClient client = new WebClient())
                                    {
                                        client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                                        var AuthUsername = System.Web.Configuration.WebConfigurationManager.AppSettings["AuthUsername"];
                                        var AuthPassword = System.Web.Configuration.WebConfigurationManager.AppSettings["AuthPassword"];
                                        //client.Credentials = new NetworkCredential(AuthUsername, AuthPassword);
                                        //var byteArray = new UTF8Encoding().GetBytes(client.Credentials);
                                        string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(AuthUsername + ":" + AuthPassword));
                                        client.Headers[HttpRequestHeader.Authorization] = string.Format("Basic {0}", credentials);
                                        string response = client.UploadString(URIEXECPayment, execPayParameters);

                                        compResultResp = response;

                                        reqEPDematInfo = new DematModel()
                                        {
                                            BoId = BoId,
                                            TotalAmount = amount,
                                            TimeStamp = getTimeStamp,
                                            RetrievalRef = RetrievalReference,
                                            UserName = mobile,
                                            Status = "Paid",
                                            BankCode = DematBank,
                                            ClientCode = ClientCode
                                        };


                                    }
                                }

                            } while ((compResultResp == "011") || (compResultResp == "012"));

                            //for inserting Execute Payment of Demat into database
                            try
                            {
                                int resultsReqEP = DematUtils.DematExecutePaymentInfo(reqEPDematInfo);

                                if ((resultsReqEP > 0))
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

                            //for sending sms  if success 
                            if (compResultResp == "\"000\"")
                            {
                                OutgoingWebResponseContext response2 = WebOperationContext.Current.OutgoingResponse;
                                if (response2.StatusCode == HttpStatusCode.OK)
                                {
                                    string messagereply = "";
                                    try
                                    {
                                        messagereply = "Dear " + CustCheckUtils.GetName(mobile) + "," + "\n";

                                        messagereply += "You have successfully paid NPR " + validTransactionData.Amount
                                                        + " to " + BoId + " " +
                                                        //GetMerchantName 
                                                        "payment for Demat Account" + " on date " +
                                                            (validTransactionData.CreatedDate).ToString("dd/MM/yyyy")
                                                        + "." + "\n";
                                        messagereply += "Thank you. NIBL Thaili";

                                        var client = new WebClient();

                                        //SENDER
                                        if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                                        {
                                            //FOR NCELL
                                            var content = client.DownloadString(
                                                SMSNCELL + "977" + mobile + "&message=" + messagereply + "");
                                        }
                                        else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                                    || (mobile.Substring(0, 3) == "986"))
                                        {
                                            //FOR NTC
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
                        //}
                        //else
                        //{
                        //    //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                        //    statusCode = "400";
                        //    //message = result;
                        //    message = resultMessageResCP;
                        //    failedmessage = message;
                        //}


                    }
                    catch (Exception ex)
                    {
                        message = result + ex + "Error Message ";
                        //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                        statusCode = "400";
                        failedmessage = "Please try again.";
                    }
                }

                //Reverse transacation
                if ((statusCode != "111") && (statusCode != "114") && (statusCode != "115") && (statusCode != "116") && (statusCode != "119") &&
                            (statusCode != "121") && (statusCode != "163") && (statusCode != "180") && (statusCode != "181") && (statusCode != "182") &&
                            (statusCode != "183") && (statusCode != "184") && (statusCode != "185") && (statusCode != "186") && (statusCode != "187") &&
                            (statusCode != "188") && (statusCode != "189") && (statusCode != "190") && (statusCode != "800") && (statusCode != "902") &&
                            (statusCode != "904") && (statusCode != "906") && (statusCode != "907") && (statusCode != "909") && (statusCode != "911") &&
                            (statusCode != "913") && (statusCode != "90") && (statusCode != "91") && (statusCode != "94") && (statusCode != "95") &&
                            (statusCode != "98") && (statusCode != "99") && (statusCodeBalance != "400") && (compResultResp != "000") && (statusCodeBalance != "400")
                            && (statusCode != "200") && (statusCode != "508")
                            )
                {
                    //TraceIdGenerator traceRevid = new TraceIdGenerator();
                    //tid = traceRevid.GenerateUniqueTraceID();
                    tid = RetrievalReference;
                    if (sc == "00")
                    {
                        transactionType = "Demat Txfr to W2W";
                    }
                    else
                    {
                        sc = "01";
                        transactionType = "Demat Txfr to W2B"; //B2W
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

                    if ((tid == null) || (sc == null) || (mobile == null) || (da == null) || (amount == null) || (pin == null) ||
                  ((double.Parse(amount) <= 0)))
                    {
                        // throw ex
                        statusCode = "400";
                        message = "Parameters Missing/Invalid";
                        failedmessage = message;
                    }
                    if ((BoId == null) || (DematBank == "") || (mobile == null) || (transactionDate == null) || (transactionId == null) ||
                        (ClientCode == null) || (mobile == "") || (paymentType == "") || (userPassword == null))
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

                        //change here
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
                                       fundtransferRev.sourcechannel, "T", "Demat Bank");
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
                                                                    "Demat Payment." + " on date " +
                                                                    (validTransactionData.CreatedDate).ToString("dd/MM/yyyy")
                                                                + "." + "\n";
                                                messagereply += "Thank you. NIBL Thaili";

                                                var client = new WebClient();

                                                //SENDER
                                                if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                                                {
                                                    //FOR NCELL
                                                    var content = client.DownloadString(
                                                        SMSNCELL + "977" + mobile + "&message=" + messagereply + "");
                                                }
                                                else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                                            || (mobile.Substring(0, 3) == "986"))
                                                {
                                                    //FOR NTC
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
                                            "Utility payment for Demat." + " on date " +
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


            ///END SMS Register For SMS
            /// if (statusCodeBalance == "400")
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
