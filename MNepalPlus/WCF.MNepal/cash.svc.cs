
using MNepalProject.Controllers;
using MNepalProject.Models;
using System;
using System.IO;
using Newtonsoft.Json;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using WCF.MNepal.Models;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Data;
using System.Web;
using WCF.MNepal.Utilities;
using WCF.MNepal.Helper;
using MNepalProject.Helper;
using System.Collections.Generic;
using System.Globalization;

namespace WCF.MNepal
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
    public class cash
    {

        [OperationContract]
        [WebInvoke(Method = "POST",
                  ResponseFormat = WebMessageFormat.Json
                  )]
        public string In(Stream input)
        {
            //string tid, string amobile, string umobile, string amount, string pin, string src
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            string tid = qs["tid"];
            string sc = qs["sc"];
            string amobile = qs["amobile"];
            string umobile = qs["umobile"];
            string amount = qs["amount"];
            string pin = qs["pin"];
            string src = qs["src"];
            string tokenID = qs["tokenID"];
            string result = "";

            //SMS
            string SMSNTC = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalNTCSMSServerUrl"];
            string SMSNCELL = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalSMSServerUrl"];

            CustActivityModel custsmsInfo = new CustActivityModel();
            ReplyMessage replyMessage = new ReplyMessage();

            TraceIdGenerator tig = new TraceIdGenerator();
            tid = tig.GenerateUniqueTraceID();

            FundTransfer fundtransfer = new FundTransfer
            {
                tid = tid,
                sc = sc,
                mobile = amobile,
                da = umobile,
                amount = amount,
                pin = pin,
                note = "",
                sourcechannel = src
            };

            string totalAmount = string.Empty;
            string totalCount = string.Empty;
            string balance = string.Empty;
            string message = string.Empty;
            string statusCode = "";
            string failedmessage = string.Empty;

            //start:Registered Customer Check Mobile
            string customerNo = string.Empty;
            if (umobile != "")
            {
                DataTable dtableUserCheck = CustCheckUtils.GetCustUserInfo(umobile);
                if (dtableUserCheck.Rows.Count == 0)
                {
                    customerNo = "0";
                }
                else if (dtableUserCheck.Rows.Count > 0)
                {
                    customerNo = umobile;
                }
            }
            //end:Registered Customer Check Mobile
            if (TokenGenerator.TokenChecker(tokenID, amobile, src) == false)
            {
                // throw ex
                statusCode = "400";
                message = "Session expired. Please login again";
                failedmessage = message;
            }
            else
            {
                //Cash In: 50
                if ((tid == null) || (sc == null) || (amobile == null) || (umobile == null) || (amount == null) || (pin == null) ||
                (src == null) || (double.Parse(amount) <= 0))
                {
                    // throw ex
                    statusCode = "400";
                    message = "Parameters Missing/Invalid";
                    failedmessage = message;
                }
                else
                {
                    if (customerNo == "0")
                    {
                        statusCode = "400";
                        message = "Unregistered Customer";
                        failedmessage = message;
                        result = failedmessage;
                        custsmsInfo = new CustActivityModel()
                        {
                            UserName = fundtransfer.mobile,
                            RequestMerchant = "Cash In",
                            DestinationNo = umobile,
                            Amount = amount,
                            SMSStatus = "Failed",
                            SMSSenderReply = "",
                            ErrorMessage = failedmessage,
                        };
                    }
                    if ((customerNo != "0") && (message == ""))
                    {

                        if (IsValidAgent(amobile))
                        {//Merchant check 
                            if (checkMerchantDestinationUsertype(umobile))
                            {
                                // throw ex
                                statusCode = "400";
                                message = "Transaction restricted to Merchant";
                                failedmessage = message;
                                custsmsInfo = new CustActivityModel()
                                {
                                    UserName = fundtransfer.mobile,
                                    RequestMerchant = "Cash In",
                                    DestinationNo = umobile,
                                    Amount = amount,
                                    SMSStatus = "Failed",
                                    SMSSenderReply = "",
                                    ErrorMessage = failedmessage,
                                };
                            }
                            else
                            {//Agent check 
                                if (!checkSourceAndDestinationUsertype(amobile, umobile))
                                {
                                    // throw ex
                                    statusCode = "400";
                                    message = "Transaction restricted to Agent";
                                    failedmessage = message;
                                 
                                }
                                else
                                {
                                    TransLimitCheck transLimitCheck = new TransLimitCheck();
                                    string resultTranLimit = transLimitCheck.LimitCheck(fundtransfer.mobile, umobile, amount, sc, pin, src);

                                    var jsonDataResult = JObject.Parse(resultTranLimit);
                                    statusCode = jsonDataResult["StatusCode"].ToString();
                                    string statusMsg = jsonDataResult["StatusMessage"].ToString();
                                    message = jsonDataResult["StatusMessage"].ToString();
                                    failedmessage = message;


                                    //start block msg 3 time pin attempt
                                    if (message == "Invalid PIN ")
                                    {
                                        LoginUtils.SetPINTries(amobile, "BUWP");//add +1 in trypwd

                                        if (LoginUtils.GetPINBlockTime(amobile)) //check if blocktime is greater than current time 
                                        {
                                            message = "Invalid PIN! You have already attempt 3 times with wrong PIN,Please try again after 1 hour";
                                            failedmessage = message;

                                        }


                                    }
                                    else
                                    {
                                        LoginUtils.SetPINTries(amobile, "RPT");

                                    }
                                    //end block msg 3 time pin attempt


                                    if ((statusCode == "200") && (statusMsg == "Success")) //&& (message == "")
                                    {
                                        //start: checking trace id
                                        do
                                        {
                                            TraceIdGenerator traceid = new TraceIdGenerator();
                                            tid = traceid.GenerateUniqueTraceID();

                                            bool traceIdCheck = false;
                                            traceIdCheck = TraceIdCheck.IsValidTraceId(tid);
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
                                        mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                        //end:Com focus one log//


                                        //NOTE:- may be need to validate before insert into reply typpe
                                        //start:insert into reply type as HTTP//
                                        var replyType = new MNReplyType(tid, "HTTP");
                                        var mnreplyType = new MNReplyTypesController();
                                        mnreplyType.InsertIntoReplyType(replyType);
                                        //end:insert into reply type as HTTP//


                                        //start:insert into transaction master//
                                        if (mnft.valid())
                                        {
                                            var transaction = new MNTransactionMaster(mnft);
                                            var mntransaction = new MNTransactionsController();
                                            MNTransactionMaster validTransactionData = mntransaction.Validate(transaction, mnft.pin);
                                            result = validTransactionData.Response;

                                            /*** ***/
                                            if (validTransactionData.Response == "Error")
                                            {
                                                mnft.Response = "error";
                                                mnft.ResponseStatus(HttpStatusCode.InternalServerError, "Internal server error - try again later, or contact support");
                                                result = mnft.Response;
                                                statusCode = "500";
                                                message = "Internal server error - try again later, or contact support";
                                                failedmessage = message;
                                            }
                                            else
                                            {
                                                if ((result == "Trace ID Repeated") || (result == "Limit Exceed")
                                                    || (result == "Invalid Source User") || (result == "Invalid Destination User")
                                                    || (result == "Invalid Product Request") || (result == ""))
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
                                                    message = result + " ";
                                                    failedmessage = message + " ";
                                                }
                                                if (result.Substring(0, 5) == "Error")
                                                {
                                                    statusCode = "400";
                                                    message = "Connection Failure from Gateway. Please Contact your Bank." + result;
                                                    mnft.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                                    failedmessage = result;
                                                }
                                                else if (statusCode == "200")
                                                {
                                                    mnft.ResponseStatus(HttpStatusCode.OK, message);
                                                    statusCode = "200";
                                                    message = result;

                                                    var v = new
                                                    {
                                                        StatusCode = Convert.ToInt32(statusCode),
                                                        StatusMessage = message
                                                    };
                                                    result = JsonConvert.SerializeObject(v);
                                                }
                                            }

                                            /*** ***/
                                        }
                                        else
                                        {
                                            mnft.Response = "error";
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, "parameters missing/invalid"); //200 - OK
                                            result = mnft.Response;
                                            statusCode = "400";
                                            message = "parameters missing/invalid";
                                            failedmessage = message;
                                        }
                                        //end:insert into transaction master//
                                        String mobile = String.Empty;

                                        OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                                        if (response.StatusCode == HttpStatusCode.OK)
                                        {
                                            try
                                            {
                                                string messagereply = "Dear " + CustCheckUtils.GetName(amobile) + "," + "\n";
                                                messagereply += "Your Cash In amount has been Debited by NPR " + amount + " \n";
                                                messagereply += "Thank-you. MNepal";

                                                mobile = amobile;
                                                var client = new WebClient();
                                                //var content = client.DownloadString(
                                                //    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To=" +
                                                //    "977" + mobile + "&Text=" + messagereply + "");

                                                if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                                                {
                                                    //FOR NCELL
                                                    //"http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                                    var content = client.DownloadString(
                                                        SMSNCELL
                                                        + "977" + mobile + "&Text=" + messagereply + "");
                                                }
                                                else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                                         || (mobile.Substring(0, 3) == "986"))
                                                {
                                                    //FOR NTC
                                                    //"http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                                    var content = client.DownloadString(
                                                        SMSNTC
                                                        + "977" + mobile + "&Text=" + messagereply + "");
                                                }

                                                string messagereplyUser = "Dear " + CustCheckUtils.GetName(umobile) + "," + "\n";
                                                messagereplyUser += "Your Cash In amount has been Credited by NPR " + amount + "\n";
                                                messagereplyUser += "Thank-you. MNepal";

                                                if ((umobile.Substring(0, 3) == "980") || (umobile.Substring(0, 3) == "981")) //FOR NCELL
                                                {
                                                    //FOR NCELL
                                                    //"http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                                    var content = client.DownloadString(
                                                        SMSNCELL
                                                        + "977" + umobile + "&Text=" + messagereplyUser + "");
                                                }
                                                else if ((umobile.Substring(0, 3) == "985") || (umobile.Substring(0, 3) == "984")
                                                         || (umobile.Substring(0, 3) == "986"))
                                                {
                                                    //FOR NTC
                                                    //"http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                                    var content = client.DownloadString(
                                                        SMSNTC
                                                        + "977" + umobile + "&Text=" + messagereplyUser + "");
                                                }

                                                custsmsInfo = new CustActivityModel()
                                                {
                                                    UserName = amobile,
                                                    RequestMerchant = "Cash In",
                                                    DestinationNo = umobile,
                                                    Amount = amount,
                                                    SMSStatus = "Success",
                                                    SMSSenderReply = messagereply,
                                                    ErrorMessage = "",
                                                };


                                            }
                                            catch (Exception ex)
                                            {
                                                // throw ex
                                                statusCode = "400";
                                                message = ex.Message;
                                            }
                                        }
                                        else if ((response.StatusCode == HttpStatusCode.BadRequest) || (response.StatusCode == HttpStatusCode.InternalServerError))
                                        {
                                            custsmsInfo = new CustActivityModel()
                                            {
                                                UserName = fundtransfer.mobile,
                                                RequestMerchant = "Cash In",
                                                DestinationNo = umobile,
                                                Amount = amount,
                                                SMSStatus = "Failed",
                                                SMSSenderReply = "",
                                                ErrorMessage = failedmessage,
                                            };

                                        }
                                    }
                                    else
                                    {

                                        replyMessage.Response = failedmessage;
                                        replyMessage.ResponseStatus(HttpStatusCode.BadRequest, failedmessage);
                                        result = replyMessage.Response;
                                        statusCode = "400";
                                        message = replyMessage.Response;
                                        failedmessage = message;

                                        custsmsInfo = new CustActivityModel()
                                        {
                                            UserName = fundtransfer.mobile,
                                            RequestMerchant = "Cash In",
                                            DestinationNo = umobile,
                                            Amount = amount,
                                            SMSStatus = "Failed",
                                            SMSSenderReply = "",
                                            ErrorMessage = failedmessage,
                                        };
                                    }
                                }
                            }
                        }
                        else
                        {
                            failedmessage = "Agent not Registered";
                            replyMessage.Response = failedmessage;
                            replyMessage.ResponseStatus(HttpStatusCode.BadRequest, failedmessage);
                            result = replyMessage.Response;
                            statusCode = "400";
                            message = replyMessage.Response;
                            failedmessage = message;
                            
                        }

                    }

                    //Register For SMS
                    try
                    {
                        if (message != "Transaction restricted to Agent" && message != "Transaction restricted to Merchant") { 
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

                    }
                    catch (Exception ex)
                    {
                        string ss = ex.Message;
                        message = result;
                    }
                    /*END SMS Register*/


                }
            }
            if (statusCode == "")
            {
                result = result.ToString();
            }
            else if (statusCode != "200")
            {
                if (message == "")
                {
                    message = result; //"Insufficient Balance";
                }
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = message
                };
                result = JsonConvert.SerializeObject(v);
            }

            return result;


            /*
            ReplyMessage replyMessage = new ReplyMessage();
            replyMessage.Response = "Cash In Done Successfully. Thank You";
            replyMessage.ResponseStatus(HttpStatusCode.OK, replyMessage.Response);
            JObject jsonObject = new JObject();
            jsonObject["message"] = replyMessage.Response;
            return jsonObject["message"].ToString();
          */

        }


        [OperationContract]
        [WebInvoke(Method = "POST",
                  ResponseFormat = WebMessageFormat.Json
                  )]

        public string Out(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            ReplyMessage replyMessage = new ReplyMessage();
            CustActivityModel custsmsInfo = new CustActivityModel();

            string tid = qs["tid"];
            string sc = qs["sc"];
            string amobile = qs["amobile"];
            string umobile = qs["umobile"];
            string amount = qs["amount"];
            string pin = qs["pin"];
            string src = qs["src"];

            //SMS
            string SMSNTC = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalNTCSMSServerUrl"];
            string SMSNCELL = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalSMSServerUrl"];

            TraceIdGenerator tig = new TraceIdGenerator();
            tid = tig.GenerateUniqueTraceID();

            string result = "";
            FundTransfer fundtransfer = new FundTransfer
            {
                tid = tid,
                sc = sc,
                mobile = umobile,
                da = amobile,
                amount = amount,
                pin = pin,
                note = "",
                sourcechannel = src
            };

            string totalAmount = string.Empty;
            string totalCount = string.Empty;
            string balance = string.Empty;
            string statusCode = string.Empty;
            string message = string.Empty;
            string failedmessage = string.Empty;

            //Cash Out: 51  //Wallet
            if ((tid == null) || (sc == null) || (amobile == null) || (umobile == null) || (amount == null) || (pin == null) ||
                (src == null))
            {
                // throw ex
                statusCode = "400";
                message = "Parameters Missing/Invalid";
            }
            else
            {
                TransLimitCheck transLimitCheck = new TransLimitCheck();
                string resultTranLimit = transLimitCheck.LimitCheck(umobile, umobile, amount, sc, pin, src);

                var jsonDataResult = JObject.Parse(resultTranLimit);
                statusCode = jsonDataResult["StatusCode"].ToString();
                string statusMsg = jsonDataResult["StatusMessage"].ToString();
                message = jsonDataResult["StatusMessage"].ToString();
                failedmessage = message;

                if ((statusCode == "200") && (statusMsg == "Success"))
                {

                    string agentNo = string.Empty;
                    if (amobile == null)
                    {
                        agentNo = "0";
                    }
                    else if ((amobile != "") || (amobile != null))
                    {
                        DataTable dtableAgentCheck = AgentCheckUtils.GetAgentInfo(amobile);
                        if (dtableAgentCheck.Rows.Count == 0)
                        {
                            agentNo = "0";
                        }
                        else if (dtableAgentCheck.Rows.Count > 0)
                        {
                            agentNo = amobile;
                        }
                    }

                    if ((agentNo != "0") && (message == "Success"))
                    {
                        //start: checking trace id
                        do
                        {
                            TraceIdGenerator traceid = new TraceIdGenerator();
                            tid = traceid.GenerateUniqueTraceID();
                            fundtransfer.tid = tid;

                            bool traceIdCheck = false;
                            traceIdCheck = TraceIdCheck.IsValidTraceId(tid);
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
                        MNFundTransfer mnft = new MNFundTransfer(fundtransfer.tid, fundtransfer.sc, fundtransfer.mobile,
                            fundtransfer.sa, fundtransfer.amount, fundtransfer.da, fundtransfer.note, fundtransfer.pin,
                            fundtransfer.sourcechannel);
                        var comfocuslog = new MNComAndFocusOneLog(mnft, DateTime.Now);
                        var mncomfocuslog = new MNComAndFocusOneLogsController();
                        mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                        //end:Com focus one log//


                        //NOTE:- may be need to validate before insert into reply typpe
                        //start:insert into reply type as HTTP//
                        var replyType = new MNReplyType(fundtransfer.tid, "HTTP");
                        var mnreplyType = new MNReplyTypesController();
                        mnreplyType.InsertIntoReplyType(replyType);
                        //end:insert into reply type as HTTP//


                        //start:insert into transaction master//
                        if (mnft.valid())
                        {
                            var transaction = new MNTransactionMaster(mnft);
                            var mntransaction = new MNTransactionsController();
                            MNTransactionMaster validTransactionData = mntransaction.Validate(transaction, mnft.pin);
                            result = validTransactionData.Response;

                            /*** ***/
                            if (validTransactionData.Response == "Error")
                            {
                                mnft.Response = "error";
                                mnft.ResponseStatus(HttpStatusCode.BadRequest, "Internal server error - try again later, or contact support");
                                result = mnft.Response;
                                statusCode = "500";
                                message = "Internal server error - try again later, or contact support";
                            }
                            else
                            {
                                if ((result == "Trace ID Repeated") || (result == "Limit Exceed") || (result == "Invalid PIN")
                                    || (result == "Invalid Source User") || (result == "Invalid Destination User")
                                    || (result == "Invalid Product Request") || (result == ""))
                                {
                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                    statusCode = "400";
                                    message = result + " " + pin;
                                    failedmessage = result + " " + pin;
                                }
                                if (result.Substring(0, 5) == "Error")
                                {
                                    statusCode = "400";
                                    message = "Connection Failure from Gateway. Please Contact your Bank." + result;
                                    mnft.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                    failedmessage = result;
                                }
                                else if (statusCode == "200")
                                {
                                    mnft.ResponseStatus(HttpStatusCode.OK, result); //200 - OK
                                    statusCode = "200";
                                    message = result;
                                }

                            }
                            var v = new
                            {
                                StatusCode = Convert.ToInt32(statusCode),
                                StatusMessage = message
                            };
                            result = JsonConvert.SerializeObject(v);
                            /*** ***/
                        }
                        else
                        {
                            mnft.Response = "error";
                            mnft.ResponseStatus(HttpStatusCode.BadRequest, "parameters missing/invalid"); //200 - OK
                            result = mnft.Response;
                            statusCode = "400";
                            message = "parameters missing/invalid";
                            failedmessage = message;
                        }
                        //end:insert into transaction master//

                        string mobile = string.Empty;

                        OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            try
                            {
                                //string messagereply = "Dear Customer," + "\n";

                                
                                string messagereply = "Dear " + CustCheckUtils.GetName(umobile) + "," + "\n";// pachi milayako
                                messagereply += "Your Cash Out amount has been Debited NPR " + amount + "to " + amobile + "\n";
                                messagereply += "-MNepal";

                                mobile = umobile;
                                var client = new WebClient();
                                //var content = client.DownloadString(
                                //    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To=" +
                                //    "977" + mobile + "&Text=" + messagereply + "");

                                if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                                {
                                    //FOR NCELL
                                    //"http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                    var content = client.DownloadString(
                                        SMSNCELL
                                        + "977" + mobile + "&Text=" + messagereply + "");
                                }
                                else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                         || (mobile.Substring(0, 3) == "986"))
                                {
                                    //FOR NTC
                                    //"http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                    var content = client.DownloadString(
                                        SMSNTC
                                        + "977" + mobile + "&Text=" + messagereply + "");
                                }

                                string messagereplyAgent = "Dear Customer," + "\n";
                                messagereplyAgent += "Your Cash Out amount has been Credited NPR " + amount + "\n";
                                messagereplyAgent += "-MNepal";

                                //var contentAgent = client.DownloadString(
                                //    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To=" +
                                //    "977" + amobile + "&Text=" + messagereplyAgent + "");

                                if ((amobile.Substring(0, 3) == "980") || (amobile.Substring(0, 3) == "981")) //FOR NCELL
                                {
                                    //FOR NCELL
                                    //"http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                    var content = client.DownloadString(
                                        SMSNCELL
                                        + "977" + amobile + "&Text=" + messagereplyAgent + "");
                                }
                                else if ((amobile.Substring(0, 3) == "985") || (amobile.Substring(0, 3) == "984")
                                         || (amobile.Substring(0, 3) == "986"))
                                {
                                    //FOR NTC
                                    //"http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                    var content = client.DownloadString(
                                        SMSNTC
                                        + "977" + amobile + "&Text=" + messagereplyAgent + "");
                                }

                                custsmsInfo = new CustActivityModel()
                                {
                                    UserName = mobile,
                                    RequestMerchant = "Cash Out",
                                    DestinationNo = amobile,
                                    Amount = amount,
                                    SMSStatus = "Success",
                                    SMSSenderReply = messagereply,
                                    ErrorMessage = "",
                                };

                            }
                            catch (Exception ex)
                            {
                                message = ex.Message;
                            }
                        }
                        else if ((response.StatusCode == HttpStatusCode.BadRequest) || (response.StatusCode == HttpStatusCode.InternalServerError))
                        {
                            replyMessage.Response = failedmessage;
                            replyMessage.ResponseStatus(HttpStatusCode.BadRequest, failedmessage);
                            result = replyMessage.Response;
                            statusCode = "400";
                            message = replyMessage.Response;
                            failedmessage = message;

                            custsmsInfo = new CustActivityModel()
                            {
                                UserName = umobile,
                                RequestMerchant = "Cash Out",
                                DestinationNo = amobile,
                                Amount = amount.ToString(),
                                SMSStatus = "Failed",
                                SMSSenderReply = "",
                                ErrorMessage = failedmessage,
                            };

                        }

                    }

                }
                else
                {

                    replyMessage.Response = failedmessage;
                    replyMessage.ResponseStatus(HttpStatusCode.BadRequest, failedmessage);
                    result = replyMessage.Response;
                    statusCode = "400";
                    message = replyMessage.Response;
                    failedmessage = message;

                    custsmsInfo = new CustActivityModel()
                    {
                        UserName = umobile,
                        RequestMerchant = "Cash Out",
                        DestinationNo = amobile,
                        Amount = amount.ToString(),
                        SMSStatus = "Failed",
                        SMSSenderReply = "",
                        ErrorMessage = failedmessage,
                    };
                }

                //Register For SMS
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
                /*END SMS Register*/

            }

            if (statusCode == "")
            {
                result = result.ToString();
            }
            else if (statusCode != "200")
            {
                if (message == "")
                {
                    message = result; //"Insufficient Balance";
                }
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = message
                };
                result = JsonConvert.SerializeObject(v);
            }
            return result;

        }

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        public string CustOut(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();

            NameValueCollection qs = HttpUtility.ParseQueryString(s);
            string tid = qs["tid"];
            string sc = qs["sc"];
            //string amobile = qs["amobile"];
            //string umobile = qs["umobile"];
            string amount = qs["amount"];
            string pin = qs["pin"];
            string src = qs["src"];
            string userName = qs["umobile"];
            string tokenID = qs["tokenID"];

            //SMS
            string SMSNTC = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalNTCSMSServerUrl"];
            string SMSNCELL = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalSMSServerUrl"];


            CustActivityModel custsmsInfo = new CustActivityModel();
            ReplyMessage replyMessage = new ReplyMessage();
            string result = "";
            string code = string.Empty;
            string statusCode = string.Empty;
            string message = string.Empty;
            //start: checking trace id
            do
            {
                TraceIdGenerator traceid = new TraceIdGenerator();
                tid = traceid.GenerateUniqueTraceID();


                bool traceIdCheck = false;
                traceIdCheck = TraceIdCheck.IsValidTraceId(tid);
                if (traceIdCheck == true)
                {
                    result = "Trace ID Repeated";
                }
                else
                {
                    result = "false";
                }

            } while (result == "Trace ID Repeated");
            if (TokenGenerator.TokenChecker(tokenID, userName, src) == false)
            {
                // throw ex
                statusCode = "400";
                message = "Session expired. Please login again";

            }
            else
            {
                if ((userName == null) || (src == null) || (tid == null) || (sc == null) /*|| (amobile == null)*/ || (amount == null) || (pin == null))
                {
                    statusCode = "400";
                    message = "Parameters Missing/Invalid";
                }
                else
                {
                    try
                    {
                        if (!IsRightPin(pin, userName))
                        {
                            statusCode = "400";
                            message = "Invalid pin";

                            //start block msg 3 time pin attempt
                            if (message == "Invalid pin")
                            {
                                LoginUtils.SetPINTries(userName, "BUWP");//add +1 in trypwd

                                if (LoginUtils.GetPINBlockTime(userName)) //check if blocktime is greater than current time 
                                {
                                    message = "Invalid PIN! You have already attempt 3 times with wrong PIN,Please try again after 1 hour";
                                }

                            }
                            else
                            {
                                LoginUtils.SetPINTries(userName, "RPT");

                            }
                            //end block msg 3 time pin attempt
                            replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
                        }
                        else
                        {
                            LoginUtils.SetPINTries(userName, "RPT");
                            TransLimitCheck transLimitCheck = new TransLimitCheck();
                            string resultTranLimit = transLimitCheck.LimitCheck(userName, " ", amount, sc, pin, src);

                            var jsonDataResult = JObject.Parse(resultTranLimit);
                            statusCode = jsonDataResult["StatusCode"].ToString();
                            string statusMsg = jsonDataResult["StatusMessage"].ToString();
                            message = jsonDataResult["StatusMessage"].ToString();


                            if (IsValidUserName(userName) && (statusMsg == "Success"))
                            {
                                TraceIdGenerator otp = new TraceIdGenerator();
                                code = otp.GetUniqueOTPKey();
                                statusCode = "200";
                                message = "Verfication code :" + code + " Please show this code to your agent to complete withdraw /n ";

                            }
                            else
                            {
                                statusCode = "400";
                                if (statusMsg != "Success")
                                {
                                    result = message;
                                    replyMessage.Response = message;
                                }
                                else
                                {
                                    result = "The number is not registered !!";
                                    replyMessage.Response = "The number is not registered !!";
                                }

                                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                                message = result;
                            }
                            //}
                            //else {
                            //    statusCode = "400";
                            //    result = "Agent is not registered !!";
                            //    replyMessage.Response = "The number is not registered !!";
                            //    replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                            //    message = result;
                            //}

                        }
                    }
                    catch (Exception ex)
                    {
                        statusCode = "400";
                        result = "Please Contact to the administrator !!" + ex;
                        //replyMessage.Response = result;
                        //replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                        message = result;
                        var v = new
                        {
                            StatusCode = Convert.ToInt32(statusCode),
                            StatusMessage = message
                        };
                        return result = JsonConvert.SerializeObject(v);
                    }
                }

                OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    try
                    {
                        CashOut cashOutInfo = new CashOut()
                        {
                            //AgentMobileNo = amobile,
                            CustomerMobileNo = userName,
                            Purpose = "Customer Cash out",
                            TraceID = tid,
                            RequestTokenCode = code,
                            Amount = amount.ToString(),
                            BeneficialName = "",
                            ClientCode = "",
                            Status = null,
                            Remarks = null,
                            TokenID = code,
                            PIN = pin,
                            ServiceCode = sc,
                            SourceChannel = src,

                        };
                        CashOutUtils.InsertCashOutInfo(cashOutInfo);

                        //string messagereply = "Dear Agent," + "\n";
                        //messagereply += userName + " has requested for cash-out of Rs. " + amount + " from your terminal. Please enter the token provided by the client to proceed or decline. ";
                        //messagereply += "Thank-you. -MNepal";

                        var client = new WebClient();
                        
                        //SMSLog log = new SMSLog();
                        //log.SentBy = mobile;
                        //log.Purpose = "Customer Cash-out";
                        //log.UserName = userName;
                        //log.Message = messagereply;
                        //CustomerUtils.LogSMS(log);

                        string customermessagereply = "Dear " + CustCheckUtils.GetName(userName) + "," + "\n";
                        customermessagereply += "Your Verification/Transaction code " + code + " has been granted successfully for cash-out of Rs. " + amount + ". Please provide the token code to the agent to proceed.";
                        customermessagereply += "Thank-you. -MNepal";

                        var cclient = new WebClient();
                        string cmobile = "";
                        cmobile = userName;

                        if ((cmobile.Substring(0, 3) == "980") || (cmobile.Substring(0, 3) == "981")) //FOR NCELL
                        {
                            //FOR NCELL
                            //"http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                            var contents = client.DownloadString( SMSNCELL
                            + "977" + cmobile + "&Text=" + customermessagereply + "");

                            statusCode = "200";
                            message = code;

                        }
                        else if ((cmobile.Substring(0, 3) == "985") || (cmobile.Substring(0, 3) == "984")
                            || (cmobile.Substring(0, 3) == "986"))
                        {
                            //FOR NTC
                            //"http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                            var contents = client.DownloadString( SMSNTC
                                + "977" + cmobile + "&Text=" + customermessagereply + "");

                            statusCode = "200";
                            message = code;
                        }
                        else
                        {
                            //FOR SMARTCELL
                            //var contents = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                            //    + "977" + mobile + "&Text=" + messagereply + "");
                            statusCode = "400";
                            message = "Network Operator is not available !! ";
                        }

                        custsmsInfo = new CustActivityModel()
                        {
                            UserName = userName,
                            RequestMerchant = "Cash Out to "/* + amobile*/,
                            DestinationNo = "",
                            Amount = amount,
                            SMSStatus = "Success",
                            SMSSenderReply = message,
                            ErrorMessage = "",
                        };

                        SMSLog llog = new SMSLog();
                        llog.SentBy = cmobile;
                        llog.Purpose = "Customer Cash-out";
                        llog.UserName = userName;
                        llog.Message = customermessagereply;
                        CustomerUtils.LogSMS(llog);

                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                else if ((response.StatusCode == HttpStatusCode.BadRequest) || (statusCode != "200"))
                {
                    custsmsInfo = new CustActivityModel()
                    {
                        UserName = userName,
                        RequestMerchant = "Cash Out to "/* + amobile*/,
                        DestinationNo = "",
                        Amount = amount,
                        SMSStatus = "Failed",
                        SMSSenderReply = "",
                        ErrorMessage = message,
                    };
                }

                //Register For SMS
                try
                {
                    int results = CustActivityUtils.RegisterCustActivityInfo(custsmsInfo);
                    if (results > 0)
                    {
                        if (statusCode != "200")
                        {
                            //custresult = message;
                            //message = result;
                        }
                        else
                        {
                            //message = result;
                        }
                    }
                    else
                    {
                        //message = result;
                    }

                }
                catch (Exception ex)
                {
                    string ss = ex.Message;
                    message = result;
                }
                ///*END SMS Register*/
            }
            if (statusCode != "200")
            {

                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = message,
                    BankBaln = ""
                };
                result = JsonConvert.SerializeObject(v);
            }
            if (statusCode == "200")
            {

                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = message,
                    BankBaln = ""
                };
                result = JsonConvert.SerializeObject(v);
            }

            return result;
        }

        [OperationContract]
        [WebInvoke(Method = "POST",
                ResponseFormat = WebMessageFormat.Json
                )]
        public string AgentCashOut(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            ReplyMessage replyMessage = new ReplyMessage();
            CustActivityModel custsmsInfo = new CustActivityModel();

            string tid = qs["tid"];
            string sc = qs["sc"];
            string amobile = qs["amobile"];
            string umobile = qs["umobile"];
            string amount = qs["amount"];
            string pin = qs["pin"];
            string transactioncode = qs["transactioncode"];
            string src = qs["src"];
            string status = qs["status"];
            string note = qs["note"];
            string tablestatus = "";
            string tokenID = qs["tokenID"];

            //SMS
            string SMSNTC = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalNTCSMSServerUrl"];
            string SMSNCELL = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalSMSServerUrl"];

            TraceIdGenerator tig = new TraceIdGenerator();
            tid = tig.GenerateUniqueTraceID();

            string result = "";
            FundTransfer fundtransfer = new FundTransfer
            {
                tid = tid,
                sc = sc,
                mobile = umobile,
                da = amobile,
                amount = amount,
                pin = pin,
                transactioncode = transactioncode,
                note = "",
                sourcechannel = src
            };

            string totalAmount = string.Empty;
            string totalCount = string.Empty;
            string balance = string.Empty;
            string statusCode = string.Empty;
            string message = string.Empty;
            string failedmessage = string.Empty;
            string referenceNo = string.Empty;

            if (TokenGenerator.TokenChecker(tokenID, amobile, src) == false)
            {
                // throw ex
                statusCode = "400";
                message = "Session expired. Please login again";

            }
            else
            {
                //Cash Out: 51  //Wallet
                if ((tid == null) || (sc == null) || (amobile == null) || (umobile == null) || (amount == null) || (pin == null) ||
                (src == null) || (status == null) || (transactioncode == null))
                {
                    // throw ex
                    statusCode = "400";
                    message = "Parameters Missing/Invalid";
                    replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);

                }
               else if (checkMerchantDestinationUsertype(umobile))
                {
                    // throw ex
                    statusCode = "400";
                    message = "Transaction restricted to Merchant";
                    failedmessage = message;

                }
                else if (!IsValidUserName(umobile))
                {
                    // throw ex
                    statusCode = "400";
                    message = "Unregistered Customer";
                    failedmessage = message;

                }
                else if (!checkSourceAndDestinationUsertype(amobile, umobile)) //Agent check 
                {
                    // throw ex
                    statusCode = "400";
                    message = "Transaction restricted to Agent";
                    failedmessage = message;

                }           
                else if (amobile != null)
                {
                    DataTable dtableTransactionCodeCheck = CashOutUtils.GetCashOutInfo(amobile, umobile, transactioncode, amount);
                    if (dtableTransactionCodeCheck.Rows.Count == 0)
                    {
                        statusCode = "400";
                        message = "Transaction code or requested amount doesnot match. ";
                        replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);

                    }
                    if (dtableTransactionCodeCheck.Rows.Count > 0)
                    {
                        if (dtableTransactionCodeCheck.Rows[0]["SenderMobileNo"].ToString() != umobile)
                        {
                            statusCode = "400";
                            message = "No Transaction Request from " + umobile;
                            replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);

                        }
                        if (dtableTransactionCodeCheck.Rows[0]["RequestTokenCode"].ToString() != transactioncode)
                        {
                            statusCode = "400";
                            message = "Transaction Code doesnot match.";
                            replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                        }
                        if (DateTime.Compare((DateTime)dtableTransactionCodeCheck.Rows[0]["TokenExpiryDate"], DateTime.Now) < 0)
                        {
                            statusCode = "400";
                            message = "The token is expired.";
                            replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                        }
                        if (dtableTransactionCodeCheck.Rows[0]["Status"].ToString() == "0")
                        {
                            statusCode = "400";
                            message = "Cash out was already rejected by Agent";
                            replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                        }
                        if (dtableTransactionCodeCheck.Rows[0]["Status"].ToString() == "1")
                        {
                            statusCode = "400";
                            message = "This transaction code is already used. ";
                            replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                        }
                        tablestatus = dtableTransactionCodeCheck.Rows[0]["Status"].ToString();
                        referenceNo = dtableTransactionCodeCheck.Rows[0]["TraceID"].ToString();
                    }
                    if (status == "0" && tablestatus == "" && statusCode != "400")
                    {
                        if (!IsRightPin(pin, amobile))
                        {
                            statusCode = "400";
                            message = "Invalid pin";
                            replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                        }
                        else
                        {
                            CashOut cashOutInfo = new CashOut()
                            {
                                AgentMobileNo = amobile,
                                CustomerMobileNo = umobile,
                                TraceID = tid,
                                RequestTokenCode = transactioncode,
                                Amount = amount.ToString(),
                                BeneficialName = "",
                                ClientCode = "",
                                Status = status,
                                Remarks = "Reject Cash out by Agent",
                                PIN = pin,
                                ServiceCode = sc,
                                SourceChannel = src,
                            };
                            CashOutUtils.UpdateCashOut(cashOutInfo);
                            statusCode = "200";
                            message = "Cash Out Rejected";
                            replyMessage.ResponseStatus(HttpStatusCode.OK, message);

                            string messagereply = "Dear " + CustCheckUtils.GetName(umobile) + "," + "\n";
                            messagereply += "Your Request has been rejected by the agent. Please, contact the agent for further information.\n";
                            messagereply += "-MNepal";

                            string mobile = umobile;
                            var client = new WebClient();
                            //var content = client.DownloadString(
                            //    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To=" +
                            //    "977" + mobile + "&Text=" + messagereply + "");

                            if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                            {
                                //FOR NCELL
                                //"http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                var content = client.DownloadString(
                                    SMSNCELL
                                    + "977" + mobile + "&Text=" + messagereply + "");
                            }
                            else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                     || (mobile.Substring(0, 3) == "986"))
                            {
                                //FOR NTC
                                //"http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                var content = client.DownloadString(
                                    SMSNTC
                                    + "977" + mobile + "&Text=" + messagereply + "");
                            }
                            var v = new
                            {
                                StatusCode = Convert.ToInt32(statusCode),
                                StatusMessage = message,
                                BankBaln = ""
                            };
                            result = JsonConvert.SerializeObject(v);
                            return result;
                        }
                    }
                }

                if ((statusCode != "400") && tablestatus == "")
                {
                    CashOutTransLimitCheck transLimitCheck = new CashOutTransLimitCheck();
                    string resultTranLimit = transLimitCheck.CashOutLimitCheck(umobile, amobile, amount, sc, pin, src);

                    var jsonDataResult = JObject.Parse(resultTranLimit);
                    statusCode = jsonDataResult["StatusCode"].ToString();
                    string statusMsg = jsonDataResult["StatusMessage"].ToString();
                    message = jsonDataResult["StatusMessage"].ToString();
                    failedmessage = message;


                    //start block msg 3 time pin attempt
                    if (message == "Invalid pin ")
                    {
                        LoginUtils.SetPINTries(amobile, "BUWP");//add +1 in trypwd

                        if (LoginUtils.GetPINBlockTime(amobile)) //check if blocktime is greater than current time 
                        {
                            message = "Invalid PIN! You have already attempt 3 times with wrong PIN,Please try again after 1 hour";
                            failedmessage = message;

                        }


                    }
                    else
                    {
                        LoginUtils.SetPINTries(amobile, "RPT");

                    }
                    //end block msg 3 time pin attempt

                    if ((statusCode == "200") && (statusMsg == "Success"))
                    {

                        string agentNo = string.Empty;
                        //string custNo = string.Empty;
                        //string custPin = string.Empty;
                        if (amobile == null)
                        {
                            agentNo = "0";
                        }
                        else if ((amobile != "") || (amobile != null))
                        {
                            DataTable dtableAgentCheck = AgentCheckUtils.GetAgentInfo(amobile);
                            if (dtableAgentCheck.Rows.Count == 0)
                            {
                                agentNo = "0";
                            }
                            else if (dtableAgentCheck.Rows.Count > 0)
                            {
                                agentNo = amobile;
                            }
                        }

                        //if (umobile == null)
                        //{
                        //    custNo = "0";
                        //}
                        //else if ((umobile != "") || (umobile != null))
                        //{
                        //    DataTable dtableUsertCheck = CustCheckUtils.GetCustUserInfo(umobile);
                        //    if (dtableUsertCheck.Rows.Count == 0)
                        //    {
                        //        custNo = "0";
                        //        custPin = "0";
                        //    }
                        //    else if (dtableUsertCheck.Rows.Count > 0)
                        //    {
                        //        custNo = umobile;
                        //        custPin = dtableUsertCheck.Rows[0]["PIN"].ToString();
                        //    }
                        //}

                        if ((agentNo != "0") && (message == "Success"))
                        {
                            //start: checking trace id
                            do
                            {
                                TraceIdGenerator traceid = new TraceIdGenerator();
                                tid = traceid.GenerateUniqueTraceID();
                                fundtransfer.tid = tid;

                                bool traceIdCheck = false;
                                traceIdCheck = TraceIdCheck.IsValidTraceId(tid);
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
                            if (referenceNo != "")
                            {
                                fundtransfer.tid = referenceNo;
                            }
                            //start:Com focus one log///
                            MNFundTransfer mnft = new MNFundTransfer(fundtransfer.tid, fundtransfer.sc, fundtransfer.mobile,
                                fundtransfer.sa, fundtransfer.amount, fundtransfer.da, fundtransfer.transactioncode, fundtransfer.pin,
                                fundtransfer.sourcechannel);
                            var comfocuslog = new MNComAndFocusOneLog(mnft, DateTime.Now);
                            var mncomfocuslog = new MNComAndFocusOneLogsController();
                            mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                            //end:Com focus one log//


                            //NOTE:- may be need to validate before insert into reply typpe
                            //start:insert into reply type as HTTP//
                            var replyType = new MNReplyType(fundtransfer.tid, "HTTP");
                            var mnreplyType = new MNReplyTypesController();
                            mnreplyType.InsertIntoReplyType(replyType);
                            //end:insert into reply type as HTTP//


                            //start:insert into transaction master//
                            if (mnft.valid())
                            {
                                var transaction = new MNTransactionMaster(mnft);
                                var mntransaction = new MNTransactionsController();
                                MNTransactionMaster validTransactionData = mntransaction.Validate(transaction, mnft.pin);
                                result = validTransactionData.Response;

                                /*** ***/
                                if (validTransactionData.Response == "Error")
                                {
                                    mnft.Response = "error";
                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, "Internal server error - try again later, or contact support");
                                    result = mnft.Response;
                                    statusCode = "500";
                                    message = "Internal server error - try again later, or contact support";
                                }
                                else
                                {
                                    if ((result == "Trace ID Repeated") || (result == "Limit Exceed") || (result == "Invalid PIN")
                                        || (result == "Invalid Source User") || (result == "Invalid Destination User")
                                        || (result == "Invalid Product Request") || (result == ""))
                                    {
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                        statusCode = "400";
                                        message = result + " " + pin;
                                        failedmessage = result + " " + pin;
                                    }
                                    if (result.Substring(0, 5) == "Error")
                                    {
                                        statusCode = "400";
                                        message = "Connection Failure from Gateway. Please Contact your Bank." + result;
                                        mnft.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                        failedmessage = result;
                                    }
                                    else if (statusCode == "200")
                                    {
                                        mnft.ResponseStatus(HttpStatusCode.OK, result); //200 - OK
                                        statusCode = "200";
                                        message = result;
                                    }

                                }
                                var v = new
                                {
                                    StatusCode = Convert.ToInt32(statusCode),
                                    StatusMessage = message
                                };
                                result = JsonConvert.SerializeObject(v);
                                /*** ***/
                            }
                            else
                            {
                                mnft.Response = "error";
                                mnft.ResponseStatus(HttpStatusCode.BadRequest, "parameters missing/invalid"); //200 - OK
                                result = mnft.Response;
                                statusCode = "400";
                                message = "parameters missing/invalid";
                                failedmessage = message;
                            }
                            //end:insert into transaction master//

                            string mobile = string.Empty;

                            OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                try
                                {
                                    CashOut cashOutInfo = new CashOut()
                                    {
                                        AgentMobileNo = amobile,
                                        CustomerMobileNo = umobile,
                                        TraceID = tid,
                                        RequestTokenCode = transactioncode,
                                        Amount = amount.ToString(),
                                        BeneficialName = "",
                                        ClientCode = "",
                                        Status = status,
                                        Remarks = "Accept Cash out by Agent",
                                        PIN = pin,
                                        ServiceCode = sc,
                                        SourceChannel = src,
                                    };
                                    CashOutUtils.UpdateCashOut(cashOutInfo);
                                    string messagereply = "Dear " + CustCheckUtils.GetName(umobile) + "," + "\n";
                                    messagereply += "You’ve successfully withdrawn Rs " + amount + " from  " + amobile + "\n";
                                    messagereply += "-MNepal";

                                    mobile = umobile;
                                    var client = new WebClient();
                                    //var content = client.DownloadString(
                                    //    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To=" +
                                    //    "977" + mobile + "&Text=" + messagereply + "");

                                    if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                                    {
                                        //FOR NCELL
                                        //"http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                        var content = client.DownloadString(
                                            SMSNCELL
                                            + "977" + mobile + "&Text=" + messagereply + "");
                                    }
                                    else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                             || (mobile.Substring(0, 3) == "986"))
                                    {
                                        //FOR NTC
                                        //"http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                        var content = client.DownloadString(
                                            SMSNTC
                                            + "977" + mobile + "&Text=" + messagereply + "");
                                    }

                                    string messagereplyAgent = "Dear " + CustCheckUtils.GetName(amobile) + "," + "\n";
                                    messagereplyAgent += "Your Cash Out amount has been credited by NPR " + amount + "\n";
                                    messagereplyAgent += "-MNepal";

                                    //var contentAgent = client.DownloadString(
                                    //    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To=" +
                                    //    "977" + amobile + "&Text=" + messagereplyAgent + "");

                                    if ((amobile.Substring(0, 3) == "980") || (amobile.Substring(0, 3) == "981")) //FOR NCELL
                                    {
                                        //FOR NCELL
                                        //"http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                        var content = client.DownloadString(
                                            SMSNCELL
                                            + "977" + amobile + "&Text=" + messagereplyAgent + "");
                                    }
                                    else if ((amobile.Substring(0, 3) == "985") || (amobile.Substring(0, 3) == "984")
                                             || (amobile.Substring(0, 3) == "986"))
                                    {
                                        //FOR NTC
                                        //"http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                        var content = client.DownloadString(
                                            SMSNTC
                                            + "977" + amobile + "&Text=" + messagereplyAgent + "");
                                    }



                                    custsmsInfo = new CustActivityModel()
                                    {
                                        UserName = mobile,
                                        RequestMerchant = "Cash Out",
                                        DestinationNo = amobile,
                                        Amount = amount,
                                        SMSStatus = "Success",
                                        SMSSenderReply = messagereply,
                                        ErrorMessage = "",
                                    };
                                }
                                catch (Exception ex)
                                {
                                    message = ex.Message;
                                }
                            }
                            else if ((response.StatusCode == HttpStatusCode.BadRequest) || (response.StatusCode == HttpStatusCode.InternalServerError))
                            {
                                replyMessage.Response = failedmessage;
                                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, failedmessage);
                                result = replyMessage.Response;
                                statusCode = "400";
                                message = replyMessage.Response;
                                failedmessage = message;

                                custsmsInfo = new CustActivityModel()
                                {
                                    UserName = umobile,
                                    RequestMerchant = "Cash Out",
                                    DestinationNo = amobile,
                                    Amount = amount.ToString(),
                                    SMSStatus = "Failed",
                                    SMSSenderReply = "",
                                    ErrorMessage = failedmessage,
                                };

                            }

                        }

                    }
                    else
                    {

                        replyMessage.Response = failedmessage;
                        replyMessage.ResponseStatus(HttpStatusCode.BadRequest, failedmessage);
                        result = replyMessage.Response;
                        statusCode = "400";
                        message = replyMessage.Response;
                        failedmessage = message;

                        custsmsInfo = new CustActivityModel()
                        {
                            UserName = umobile,
                            RequestMerchant = "Cash Out",
                            DestinationNo = amobile,
                            Amount = amount.ToString(),
                            SMSStatus = "Failed",
                            SMSSenderReply = "",
                            ErrorMessage = failedmessage,
                        };
                    }

                    ////Register For SMS
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
                    /*END SMS Register*/

                }
            }

            if (statusCode == "")
            {
                result = result.ToString();
            }
            else if (statusCode != "200")
            {
                if (message == "")
                {
                    message = result; //"Insufficient Balance";
                }
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = message,
                    BankBaln = ""
                };
                result = JsonConvert.SerializeObject(v);
            }
            return result;
            

        }

        //old Agent Cash Out
        public string AgentCashOutOld(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            ReplyMessage replyMessage = new ReplyMessage();
            CustActivityModel custsmsInfo = new CustActivityModel();

            string tid = qs["tid"];
            string sc = qs["sc"];
            string amobile = qs["amobile"];
            string umobile = qs["umobile"];
            string amount = qs["amount"];
            string pin = qs["pin"];
            string transactioncode = qs["transactioncode"];
            string src = qs["src"];
            string status = qs["status"];
            string note = qs["note"];
            string tablestatus = "";

            TraceIdGenerator tig = new TraceIdGenerator();
            tid = tig.GenerateUniqueTraceID();

            string result = "";
            FundTransfer fundtransfer = new FundTransfer();
            fundtransfer.tid = tid;
            fundtransfer.sc = sc;
            fundtransfer.mobile = amobile;
            fundtransfer.da = umobile;
            fundtransfer.amount = amount;
            fundtransfer.pin = pin;
            fundtransfer.transactioncode = transactioncode;
            fundtransfer.note = "";
            fundtransfer.sourcechannel = src;

            string totalAmount = string.Empty;
            string totalCount = string.Empty;
            string balance = string.Empty;
            string statusCode = string.Empty;
            string message = string.Empty;
            string failedmessage = string.Empty;



            //Cash Out: 51  //Wallet
            if ((tid == null) || (sc == null) || (amobile == null) || (umobile == null) || (amount == null) || (pin == null) ||
                (src == null) || (status == null))
            {
                // throw ex
                statusCode = "400";
                message = "Parameters Missing/Invalid";
                replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);

            }
            if (amobile != null)
            {
                DataTable dtableTransactionCodeCheck = CashOutUtils.GetCashOutInfo(amobile, umobile, transactioncode, amount);
                if (dtableTransactionCodeCheck.Rows.Count == 0)
                {
                    statusCode = "400";
                    message = "Transaction Code doesnot match.";
                    replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);

                }
                if (dtableTransactionCodeCheck.Rows.Count > 0)
                {
                    if (dtableTransactionCodeCheck.Rows[0]["SenderMobileNo"].ToString() != umobile)
                    {
                        statusCode = "400";
                        message = "No Transaction Request from " + umobile;
                        replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);

                    }
                    if (dtableTransactionCodeCheck.Rows[0]["RequestTokenCode"].ToString() != transactioncode)
                    {
                        statusCode = "400";
                        message = "Transaction Code doesnot match.";
                        replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                    }
                    if (DateTime.Compare((DateTime)dtableTransactionCodeCheck.Rows[0]["TokenExpiryDate"], DateTime.Now) < 0)
                    {
                        statusCode = "400";
                        message = "The token is expired.";
                        replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                    }
                    if (dtableTransactionCodeCheck.Rows[0]["Status"].ToString() == "0")
                    {
                        statusCode = "400";
                        message = "Cash out was already rejected by Agent";
                        replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                    }
                    if (dtableTransactionCodeCheck.Rows[0]["Status"].ToString() == "1")
                    {
                        statusCode = "400";
                        message = "This transaction code is already used. ";
                        replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                    }
                    tablestatus = dtableTransactionCodeCheck.Rows[0]["Status"].ToString();
                }
                if (status == "0" && tablestatus == "")
                {
                    if (!IsRightPin(pin, amobile))
                    {
                        statusCode = "400";
                        message = "Invalid pin" + pin;
                        replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                    }
                    else
                    {
                        CashOut cashOutInfo = new CashOut()
                        {
                            AgentMobileNo = amobile,
                            CustomerMobileNo = umobile,
                            TraceID = tid,
                            RequestTokenCode = transactioncode,
                            Amount = amount.ToString(),
                            BeneficialName = "",
                            ClientCode = "",
                            Status = status,
                            Remarks = "Reject Cash out by Agent",
                            PIN = pin,
                            ServiceCode = sc,
                            SourceChannel = src,
                        };
                        CashOutUtils.UpdateCashOut(cashOutInfo);
                        statusCode = "200";
                        message = "Cash Out Rejected";
                        replyMessage.ResponseStatus(HttpStatusCode.OK, message);

                        //string messagereply = "Dear Customer," + "\n";
                        string messagereply = "Dear " + CustCheckUtils.GetName(umobile) + "," + "\n";//pachi milayako
                        messagereply += "Your Request has been rejected by the agent. Please, contact the agent for further information.\n";
                        messagereply += "-MNepal";

                        string mobile = umobile;
                        var client = new WebClient();
                        //var content = client.DownloadString(
                        //    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To=" +
                        //    "977" + mobile + "&Text=" + messagereply + "");

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
                        var v = new
                        {
                            StatusCode = Convert.ToInt32(statusCode),
                            StatusMessage = message,
                            BankBaln = ""
                        };
                        result = JsonConvert.SerializeObject(v);
                        return result;
                    }
                }
            }
            if ((statusCode != "400") && tablestatus == "")
            {
                TransLimitCheck transLimitCheck = new TransLimitCheck();
                string resultTranLimit = transLimitCheck.LimitCheck(amobile, umobile, amount, sc, pin, src);

                var jsonDataResult = JObject.Parse(resultTranLimit);
                statusCode = jsonDataResult["StatusCode"].ToString();
                string statusMsg = jsonDataResult["StatusMessage"].ToString();
                message = jsonDataResult["StatusMessage"].ToString();
                failedmessage = message;

                if ((statusCode == "200") && (statusMsg == "Success"))
                {

                    string agentNo = string.Empty;
                    if (amobile == null)
                    {
                        agentNo = "0";
                    }
                    else if ((amobile != "") || (amobile != null))
                    {
                        DataTable dtableAgentCheck = AgentCheckUtils.GetAgentInfo(amobile);
                        if (dtableAgentCheck.Rows.Count == 0)
                        {
                            agentNo = "0";
                        }
                        else if (dtableAgentCheck.Rows.Count > 0)
                        {
                            agentNo = amobile;
                        }
                    }

                    if ((agentNo != "0") && (message == "Success"))
                    {
                        //start: checking trace id
                        do
                        {
                            TraceIdGenerator traceid = new TraceIdGenerator();
                            tid = traceid.GenerateUniqueTraceID();
                            fundtransfer.tid = tid;

                            bool traceIdCheck = false;
                            traceIdCheck = TraceIdCheck.IsValidTraceId(tid);
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
                        MNFundTransfer mnft = new MNFundTransfer(fundtransfer.tid, fundtransfer.sc, fundtransfer.mobile,
                            fundtransfer.sa, fundtransfer.amount, fundtransfer.da, fundtransfer.transactioncode, fundtransfer.pin,
                            fundtransfer.sourcechannel);
                        var comfocuslog = new MNComAndFocusOneLog(mnft, DateTime.Now);
                        var mncomfocuslog = new MNComAndFocusOneLogsController();
                        mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                        //end:Com focus one log//


                        //NOTE:- may be need to validate before insert into reply typpe
                        //start:insert into reply type as HTTP//
                        var replyType = new MNReplyType(fundtransfer.tid, "HTTP");
                        var mnreplyType = new MNReplyTypesController();
                        mnreplyType.InsertIntoReplyType(replyType);
                        //end:insert into reply type as HTTP//


                        //start:insert into transaction master//
                        if (mnft.valid())
                        {
                            var transaction = new MNTransactionMaster(mnft);
                            var mntransaction = new MNTransactionsController();
                            MNTransactionMaster validTransactionData = mntransaction.Validate(transaction, mnft.pin);
                            result = validTransactionData.Response;

                            /*** ***/
                            if (validTransactionData.Response == "Error")
                            {
                                mnft.Response = "error";
                                mnft.ResponseStatus(HttpStatusCode.BadRequest, "Internal server error - try again later, or contact support");
                                result = mnft.Response;
                                statusCode = "500";
                                message = "Internal server error - try again later, or contact support";
                            }
                            else
                            {
                                if ((result == "Trace ID Repeated") || (result == "Limit Exceed") || (result == "Invalid PIN")
                                    || (result == "Invalid Source User") || (result == "Invalid Destination User")
                                    || (result == "Invalid Product Request") || (result == ""))
                                {
                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                    statusCode = "400";
                                    message = result + " " + pin;
                                    failedmessage = result + " " + pin;
                                }
                                if (result.Substring(0, 5) == "Error")
                                {
                                    statusCode = "400";
                                    message = "Connection Failure from Gateway. Please Contact your Bank." + result;
                                    mnft.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                    failedmessage = result;
                                }
                                else if (statusCode == "200")
                                {
                                    mnft.ResponseStatus(HttpStatusCode.OK, result); //200 - OK
                                    statusCode = "200";
                                    message = result;
                                }

                            }
                            var v = new
                            {
                                StatusCode = Convert.ToInt32(statusCode),
                                StatusMessage = message
                            };
                            result = JsonConvert.SerializeObject(v);
                            /*** ***/
                        }
                        else
                        {
                            mnft.Response = "error";
                            mnft.ResponseStatus(HttpStatusCode.BadRequest, "parameters missing/invalid"); //200 - OK
                            result = mnft.Response;
                            statusCode = "400";
                            message = "parameters missing/invalid";
                            failedmessage = message;
                        }
                        //end:insert into transaction master//

                        string mobile = string.Empty;

                        OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            try
                            {
                                CashOut cashOutInfo = new CashOut()
                                {
                                    AgentMobileNo = amobile,
                                    CustomerMobileNo = umobile,
                                    TraceID = tid,
                                    RequestTokenCode = transactioncode,
                                    Amount = amount.ToString(),
                                    BeneficialName = "",
                                    ClientCode = "",
                                    Status = status,
                                    Remarks = "Accept Cash out by Agent",
                                    PIN = pin,
                                    ServiceCode = sc,
                                    SourceChannel = src,
                                };
                                CashOutUtils.UpdateCashOut(cashOutInfo);
                                //string messagereply = "Dear Customer," + "\n";
                                string messagereply = "Dear " + CustCheckUtils.GetName(umobile) + "," + "\n";//pachi milayako
                                messagereply += "You've sucessfully withdrawn Rs." + amount + " from Agent: " + amobile + "\n";
                                messagereply += "-MNepal";

                                mobile = umobile;
                                var client = new WebClient();
                                //var content = client.DownloadString(
                                //    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To=" +
                                //    "977" + mobile + "&Text=" + messagereply + "");

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

                                string messagereplyAgent = "Dear Agent," + "\n";
                                messagereplyAgent += "Your Cash Out amount has been debited by NPR " + amount + "\n";
                                messagereplyAgent += "-MNepal";

                                //var contentAgent = client.DownloadString(
                                //    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To=" +
                                //    "977" + amobile + "&Text=" + messagereplyAgent + "");

                                if ((amobile.Substring(0, 3) == "980") || (amobile.Substring(0, 3) == "981")) //FOR NCELL
                                {
                                    //FOR NCELL
                                    var content = client.DownloadString(
                                        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                        + "977" + amobile + "&Text=" + messagereplyAgent + "");
                                }
                                else if ((amobile.Substring(0, 3) == "985") || (amobile.Substring(0, 3) == "984")
                                         || (amobile.Substring(0, 3) == "986"))
                                {
                                    //FOR NTC
                                    var content = client.DownloadString(
                                        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                        + "977" + amobile + "&Text=" + messagereplyAgent + "");
                                }



                                custsmsInfo = new CustActivityModel()
                                {
                                    UserName = mobile,
                                    RequestMerchant = "Cash Out",
                                    DestinationNo = amobile,
                                    Amount = amount,
                                    SMSStatus = "Success",
                                    SMSSenderReply = messagereply,
                                    ErrorMessage = "",
                                };
                            }
                            catch (Exception ex)
                            {
                                message = ex.Message;
                            }
                        }
                        else if ((response.StatusCode == HttpStatusCode.BadRequest) || (response.StatusCode == HttpStatusCode.InternalServerError))
                        {
                            replyMessage.Response = failedmessage;
                            replyMessage.ResponseStatus(HttpStatusCode.BadRequest, failedmessage);
                            result = replyMessage.Response;
                            statusCode = "400";
                            message = replyMessage.Response;
                            failedmessage = message;

                            custsmsInfo = new CustActivityModel()
                            {
                                UserName = umobile,
                                RequestMerchant = "Cash Out",
                                DestinationNo = amobile,
                                Amount = amount.ToString(),
                                SMSStatus = "Failed",
                                SMSSenderReply = "",
                                ErrorMessage = failedmessage,
                            };

                        }

                    }

                }
                else
                {

                    replyMessage.Response = failedmessage;
                    replyMessage.ResponseStatus(HttpStatusCode.BadRequest, failedmessage);
                    result = replyMessage.Response;
                    statusCode = "400";
                    message = replyMessage.Response;
                    failedmessage = message;

                    custsmsInfo = new CustActivityModel()
                    {
                        UserName = umobile,
                        RequestMerchant = "Cash Out",
                        DestinationNo = amobile,
                        Amount = amount.ToString(),
                        SMSStatus = "Failed",
                        SMSSenderReply = "",
                        ErrorMessage = failedmessage,
                    };
                }

                ////Register For SMS
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
                /*END SMS Register*/

            }


            if (statusCode == "")
            {
                result = result.ToString();
            }
            else if (statusCode != "200")
            {
                if (message == "")
                {
                    message = result; //"Insufficient Balance";
                }
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = message,
                    BankBaln = ""
                };
                result = JsonConvert.SerializeObject(v);
            }
            return result;


            /*ReplyMessage replyMessage = new ReplyMessage();
            replyMessage.Response = "Cash Out Done Successfully. Thank You";
            replyMessage.ResponseStatus(HttpStatusCode.OK, replyMessage.Response);

            JObject jsonObject = new JObject();
            jsonObject["message"] = replyMessage.Response;
            return jsonObject["message"].ToString();*/

        }

        public bool IsValidUserName(string username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                DataTable dtCheckUserName = CustCheckUtils.GetCustUserInfo(username);
                if (dtCheckUserName.Rows.Count == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        public bool IsValidAgent(string amobile)
        {
            if (!string.IsNullOrEmpty(amobile))
            {
                DataTable dtableAgentCheck = AgentCheckUtils.GetAgentInfo(amobile);
                if (dtableAgentCheck.Rows.Count == 1)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsRightPin(string pin, string mobile)
        {
            DataTable dtableResult = PinUtils.GetPinInfo(mobile);
            if (dtableResult != null)
            {
                foreach (DataRow dtableUser in dtableResult.Rows)
                {
                    String validPIN = dtableUser["PIN"].ToString();
                    if (!validPIN.Equals(pin))
                    {
                        string errorpin = pin;

                        return false;
                    }

                }
            }
            return true;
        }
        #region Checking SOurce and Destination Numbers
        public bool checkSourceAndDestinationUsertype(string source, string destination)
        {

            if (UserNameCheck.IsValidAgent(source) && UserNameCheck.IsValidUserNameForgotPassword(destination))
            {
                return true;
            }

            return false;
        }

        public bool checkMerchantDestinationUsertype(string destination)
        {
            if (CustCheckUtils.GetMerchantUserCheckInfo(destination))
            {
                return true;
            }
            return false;
        }
        #endregion
        //Merchant check 


        //FOR cashoutlist
        #region"Cash Out List"
        [OperationContract]
        [WebGet]
        public string CashOutList(string SenderMobileNo,string TokenID,string RecipientMobileNo,string Amount, string Status)
             
        {
            string result = "";
            string message = string.Empty;
            string statusCode = string.Empty;
            string CreatedDate,ExpiryDate;
             
             
            ReplyMessage replyMessage = new ReplyMessage();

            if (string.IsNullOrEmpty(SenderMobileNo))
            {
                // throw ex
                statusCode = "400";
                message = "Parameters Missing/Invalid";
                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
            }
            else
            {
                //start: check customer KYC detail
                var resultList = new List<CashOutList>();
                DataTable dtableResult = CashOutUtils.GetCashOutList(SenderMobileNo, TokenID, RecipientMobileNo, Amount,Status);
               // if (dtableResult.Rows.Count == 1)
                    if (dtableResult.Rows.Count > 0)
                    {
                    foreach (DataRow dtableUser in dtableResult.Rows)
                    {



                        TokenID = dtableUser["TokenID"].ToString();
                        RecipientMobileNo = dtableUser["RecipientMobileNo"].ToString();
                        CreatedDate = dtableUser["TokenCreatedDate"].ToString();
                        ExpiryDate = dtableUser["TokenExpiryDate"].ToString();
                        Amount = dtableUser["Amount"].ToString();
                        Status = dtableUser["Status"].ToString();
                         


                        resultList.Add(new CashOutList
                        {


                            TokenID = TokenID,
                            RecipientMobileNo = RecipientMobileNo,
                            CreatedDate = CreatedDate,
                            ExpiryDate = ExpiryDate,
                            Amount = Amount,
                            Status = Status

                        });

                    }

                    string sJSONResponse = JsonConvert.SerializeObject(resultList);

                    result = sJSONResponse;

                    statusCode = "200";
                    replyMessage.Response = result;
                    message = replyMessage.Response;
                }
                else
                {
                    // throw ex
                    statusCode = "400";
                    
                    result = "No record found.";
                    replyMessage.Response = "No record found";
                    message = replyMessage.Response;
                }
                //end: check customer KYC detail
            }

            OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    var v = new
                    {
                        StatusCode = Convert.ToInt32(statusCode),
                        StatusMessage = result
                    };
                    result = JsonConvert.SerializeObject(v);

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = result
                };
                result = JsonConvert.SerializeObject(v);
            }

            return result;
        }

        #endregion

        #region Cancel CashOut By Customer
        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        public string CustCashOutCancel(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);
            string tid;
            string pin = qs["pin"];
            string src = qs["src"];
            string userName = qs["umobile"];
            string tokenID = qs["tokenID"];
            string requestTokenCode = qs["requestTokenCode"];
            CustActivityModel custsmsInfo = new CustActivityModel();
            ReplyMessage replyMessage = new ReplyMessage();
            string result = "";
            string code = string.Empty;
            string statusCode = string.Empty;
            string message = string.Empty;
            //start: checking trace id
            do
            {
                TraceIdGenerator traceid = new TraceIdGenerator();
                tid = traceid.GenerateUniqueTraceID();
                bool traceIdCheck = false;
                traceIdCheck = TraceIdCheck.IsValidTraceId(tid);
                if (traceIdCheck == true)
                {
                    result = "Trace ID Repeated";
                }
                else
                {
                    result = "false";
                }
 
                 


            } while (result == "Trace ID Repeated");
            if (TokenGenerator.TokenChecker(tokenID, userName, src) == false)
            {
                // throw ex
                statusCode = "400";
                message = "Session expired. Please login again";
            }
            else
            {
                if ((userName == null) || (src == null) || (tid == null) || (pin == null) || (requestTokenCode == null))
                {
                    statusCode = "400";
                    message = "Parameters Missing/Invalid";
                }
                else
                {
                    try
                    {
                       

                        //checking RequestTokenCode 
                        DataTable dtableUserCheck = CashOutUtils.CheckRequestToken(requestTokenCode, userName);
                        if (dtableUserCheck.Rows.Count == 0)
                        {

                            statusCode = "400";
                            message = "Invalid token code";
                            replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
                        }



                        else if (!IsRightPin(pin, userName))
                        {
                            statusCode = "400";
                            message = "Invalid pin";
 //start block msg 3 time pin attempt
                            if (message == "Invalid pin")
                            {
                                LoginUtils.SetPINTries(userName, "BUWP");//add +1 in trypwd

                                if (LoginUtils.GetPINBlockTime(userName)) //check if blocktime is greater than current time 
                                {
                                    message = "Invalid PIN! You have already attempt 3 times with wrong PIN,Please try again after 1 hour";
                                   // failedmessage = message;

                                }


                            }
                            else
                            {
                                LoginUtils.SetPINTries(userName, "RPT");

                            }
                            //end block msg 3 time pin attempt
                            replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
                        }

                        else
                        {
                            LoginUtils.SetPINTries(userName, "RPT");
                            string statusMsg = "";
                            if (IsValidUserName(userName))
                            {
                                CashOut cashOutInfo = new CashOut()
                                {
                                    AgentMobileNo = "",
                                    CustomerMobileNo = userName,
                                    TraceID = tid,
                                    RequestTokenCode = requestTokenCode,
                                    Amount = "",
                                    BeneficialName = "",
                                    ClientCode = "",
                                    Status = "0",
                                    Remarks = "Cancel Cash out by Customer",
                                    PIN = pin,
                                    ServiceCode = "",
                                    SourceChannel = src,
                                };
                                CashOutUtils.UpdateCashOut(cashOutInfo);
                                statusCode = "200";
                                message = "Cash Out Rejected";
                                replyMessage.ResponseStatus(HttpStatusCode.OK, message);
                                statusMsg = "Success";

                            }
                            else
                            {
                                statusCode = "400";
                                if (statusMsg != "Success")
                                {
                                    result = message;
                                    replyMessage.Response = message;
                                }
                                else
                                {
                                    result = "The number is not registered !!";
                                    replyMessage.Response = "The number is not registered !!";
                                }

                                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                                message = result;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        statusCode = "400";
                        result = "Please Contact to the administrator !!" + ex;
                        message = result;
                        var vv = new
                        {
                            StatusCode = Convert.ToInt32(statusCode),
                            StatusMessage = message
                        };
                        return result = JsonConvert.SerializeObject(vv);
                    }
                }
                OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    //    try
                    //    {
                    //        var client = new WebClient();
                    //        string customermessagereply = "Dear " + CustCheckUtils.GetName(userName) + "," + "\n";
                    //        customermessagereply += "Your CashOut has been cancelled.\n";
                    //        customermessagereply += "-MNepal";

                    //        var cclient = new WebClient();
                    //        string cmobile = "";
                    //        cmobile = userName;

                    //        if ((cmobile.Substring(0, 3) == "980") || (cmobile.Substring(0, 3) == "981")) //FOR NCELL
                    //        {
                    //            //FOR NCELL
                    //            var contents = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                    //            + "977" + cmobile + "&Text=" + customermessagereply + "");

                    //            statusCode = "200";
                    //            message = code;

                    //        }
                    //        else if ((cmobile.Substring(0, 3) == "985") || (cmobile.Substring(0, 3) == "984")
                    //            || (cmobile.Substring(0, 3) == "986"))
                    //        {
                    //            //FOR NTC
                    //            var contents = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                    //                + "977" + cmobile + "&Text=" + customermessagereply + "");

                    //            statusCode = "200";
                    //            message = code;
                    //        }
                    //        else
                    //        {
                    //            //FOR SMARTCELL
                    //            //var contents = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                    //            //    + "977" + mobile + "&Text=" + messagereply + "");
                    //            statusCode = "400";
                    //            message = "Network Operator is not available !! ";
                    //        }

                    custsmsInfo = new CustActivityModel()
                    {
                        UserName = userName,
                        RequestMerchant = "Cash Out to "/* + amobile*/,
                        DestinationNo = "",
                        Amount = "",
                        SMSStatus = "Success",
                        SMSSenderReply = message,
                        ErrorMessage = "",
                    };

                    //        SMSLog llog = new SMSLog();
                    //        llog.SentBy = cmobile;
                    //        llog.Purpose = "Customer Cash-out";
                    //        llog.UserName = userName;
                    //        llog.Message = customermessagereply;
                    //        CustomerUtils.LogSMS(llog);

                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        throw ex;
                    //    }
                }
                else if ((response.StatusCode == HttpStatusCode.BadRequest) || (statusCode != "200"))
                {
                    custsmsInfo = new CustActivityModel()
                    {
                        UserName = userName,
                        RequestMerchant = "Cash Out to "/* + amobile*/,
                        DestinationNo = "",
                        Amount = "",
                        SMSStatus = "Failed",
                        SMSSenderReply = "",
                        ErrorMessage = message,
                    };
                }

                //Register For SMS
                try
                {
                    int results = CustActivityUtils.RegisterCustActivityInfo(custsmsInfo);
                    if (results > 0)
                    {
                        if (statusCode != "200") { } else { }
                    }
                    else
                    {

                    }

                }
                catch (Exception ex)
                {
                    string ss = ex.Message;
                    message = result;
                }
                ///*END SMS Register*/
            }
            var v = new
            {
                StatusCode = Convert.ToInt32(statusCode),
                StatusMessage = message
            };
            result = JsonConvert.SerializeObject(v);
            return result;
        }
        #endregion


      




    }


}
