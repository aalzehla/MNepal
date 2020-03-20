
using MNepalProject.Controllers;
using MNepalProject.Models;
using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Net;
using System.Collections.Specialized;
using System.Data;
using System.Web;
using MNepalWCF.Utilities;
using MNepalWCF.Helper;
using MNepalProject.Helper;
using MNepalWCF.Models;

namespace MNepalWCF
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
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

            string result = "";

            CustActivityModel custsmsInfo = new CustActivityModel();
            ReplyMessage replyMessage = new ReplyMessage();

            TraceIdGenerator tig = new TraceIdGenerator();
            tid = tig.GenerateUniqueTraceID();

            FundTransfer fundtransfer = new FundTransfer();
            fundtransfer.tid = tid;
            fundtransfer.sc = sc;
            fundtransfer.mobile = amobile;
            fundtransfer.da = umobile;
            fundtransfer.amount = amount;
            fundtransfer.pin = pin;
            fundtransfer.note = "";
            fundtransfer.sourcechannel = src;
            
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

            //Cash In: 50
            if ((tid == null) || (sc == null) || (amobile == null) || (umobile == null) || (amount == null) || (pin == null) ||
                (src == null))
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
                    message = "Unregister Customer";
                    failedmessage = message;
                }
                if ((customerNo != "0") && (message == ""))
                {
                    TranxLimitCheck transLimitCheck = new TranxLimitCheck();
                    string resultTranLimit = transLimitCheck.LimitCheck(fundtransfer.mobile, umobile, amount, sc, pin, src);

                    var jsonDataResult = JObject.Parse(resultTranLimit);
                    statusCode = jsonDataResult["StatusCode"].ToString();
                    string statusMsg = jsonDataResult["StatusMessage"].ToString();
                    message = jsonDataResult["StatusMessage"].ToString();
                    failedmessage = message;

                    if ((statusCode == "200") && (statusMsg == "Success")) //&& (message == "")
                    {

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
                                    message = result + " " + pin;
                                    failedmessage = message + " " + pin;
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
                                string messagereply = "Dear Customer," + "\n";
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

                                string messagereplyUser = "Dear Customer," + "\n";
                                messagereplyUser += "Your Cash In amount has been Credited by NPR " + amount + "\n";
                                messagereplyUser += "Thank-you. MNepal";

                                if ((umobile.Substring(0, 3) == "980") || (umobile.Substring(0, 3) == "981")) //FOR NCELL
                                {
                                    //FOR NCELL
                                    var content = client.DownloadString(
                                        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                        + "977" + umobile + "&Text=" + messagereplyUser + "");
                                }
                                else if ((umobile.Substring(0, 3) == "985") || (umobile.Substring(0, 3) == "984")
                                         || (umobile.Substring(0, 3) == "986"))
                                {
                                    //FOR NTC
                                    var content = client.DownloadString(
                                        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
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

            TraceIdGenerator tig = new TraceIdGenerator();
            tid = tig.GenerateUniqueTraceID();

            string result = "";
            FundTransfer fundtransfer = new FundTransfer();
            fundtransfer.tid = tid;
            fundtransfer.sc = sc;
            fundtransfer.mobile = umobile;
            fundtransfer.da = amobile;
            fundtransfer.amount = amount;
            fundtransfer.pin = pin;
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
                                    || (result == "Invalid Product Request") || (result == "") )
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
                                string messagereply = "Dear Customer," + "\n";
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

                                string messagereplyAgent = "Dear Customer," + "\n";
                                messagereplyAgent += "Your Cash Out amount has been Credited NPR " + amount + "\n";
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
            
            
            /*ReplyMessage replyMessage = new ReplyMessage();
            replyMessage.Response = "Cash Out Done Successfully. Thank You";
            replyMessage.ResponseStatus(HttpStatusCode.OK, replyMessage.Response);

            JObject jsonObject = new JObject();
            jsonObject["message"] = replyMessage.Response;
            return jsonObject["message"].ToString();*/

        }


    }


}
