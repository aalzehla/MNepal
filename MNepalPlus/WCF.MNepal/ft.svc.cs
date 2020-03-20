using MNepalProject.Controllers;
using MNepalProject.Models;
using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.IO;
using System.Collections.Specialized;
using System.Data;
using System.Web;
using MNepalProject.DAL;
using WCF.MNepal.Utilities;
using MNepalProject.Helper;
using Newtonsoft.Json;
using WCF.MNepal.Models;
using WCF.MNepal.Helper;
using Newtonsoft.Json.Linq;
using WCF.MNepal.ErrorMsg;
using System.Threading;
using System.Web.Services.Description;

namespace WCF.MNepal
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
    public class ft
    {
        [OperationContract]
        [WebInvoke(Method = "POST",
                  ResponseFormat = WebMessageFormat.Json)]
        public string request(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            string tid = qs["tid"];

            //string[] querySegments = s.Split('&');
            //string[] querySegments = s.Split(new string[] { "&amp;" }, StringSplitOptions.None);
            //foreach (string segment in querySegments)
            //{
            //    string[] parts = segment.Split('=');
            //    if (parts.Length > 0)
            //    {
            //        string key = parts[0].Trim();//new char[] { '?', ' ' });
            //        string val = parts[1].Trim();

            //        qs.Add(key, val);
            //    }
            //}

            string sc = qs["sc"];
            string mobile = qs["mobile"];
            string da = qs["da"];
            string amount = qs["amount"];
            string pin = qs["pin"];
            string note = qs["note"];
            string src = qs["src"];
            string result = "";
            string sessionID = qs["tokenID"];

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
            MNTransactionMaster validTransactionData = new MNTransactionMaster();

            string totalAmount = string.Empty;
            string totalCount = string.Empty;
            string totalBAmount = string.Empty;
            string totalBCount = string.Empty;
            string balance = string.Empty;
            string message = string.Empty;
            string statusCode = string.Empty;
            string failedmessage = string.Empty;

            string customerNo = string.Empty;
            if (TokenGenerator.TokenChecker(sessionID, mobile, src) == false)
            {
                // throw ex
                statusCode = "400";
                message = "Session expired. Please login again";
                failedmessage = message;
            }
            else
            {
                if ((tid == null) || (sc == null) || (mobile == null) || (da == null) || (amount == null) || (pin == null) ||
                    (src == null) || (double.Parse(amount) <= 0))
                {
                    // throw ex
                    statusCode = "400";
                    message = "Parameters Missing/Invalid";
                    failedmessage = message;
                }
                else
                {

                    //start:Registered Customer Check Mobile
                    if (da != "")
                    {
                        DataTable dtableUserCheck = CustCheckUtils.GetCustUserInfo(da);
                        if (dtableUserCheck.Rows.Count == 0)
                        {
                            customerNo = "0";
                        }
                        else if (dtableUserCheck.Rows.Count > 0)
                        {
                            customerNo = da;
                        }
                    }
                    //end:Registered Customer Check Mobile

                    if (sc == "00")
                    {
                        transactionType = "Fund Transfer to W2W";
                    }
                    else if (sc == "01")
                    {
                        transactionType = "Fund Transfer to W2B";
                    }
                    else if (sc == "10")
                    {
                        transactionType = "Fund Transfer to B2W"; //B2W
                    }
                    else if (sc == "11")
                    {
                        transactionType = "Fund Transfer to Bank";//B2B
                    }

                    if ((customerNo != "0") && (message == "") && (statusCode != "400"))
                    { //Merchant check 





                        if (UserNameCheck.IsValidMerchant(mobile))
                        {
                            if (UserNameCheck.IsValidAgent(da))
                            {

                                // throw ex
                                statusCode = "400";
                                message = "Transaction restricted to Agent";
                                failedmessage = message;
                            }
                            if (UserNameCheck.IsValidUser(da))
                            {
                                // throw ex
                                statusCode = "400";
                                message = "Transaction restricted to User";
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
                                    mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                    //end:Com focus one log//


                                    //NOTE:- may be need to validate before insert into reply typpe
                                    //start:insert into reply type as HTTP//
                                    var replyType = new MNReplyType(tid, "HTTP");
                                    var mnreplyType = new MNReplyTypesController();
                                    mnreplyType.InsertIntoReplyType(replyType);
                                    //end:insert into reply type as HTTP//

                                    //MNTransactionMaster validTransactionData = new MNTransactionMaster();

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
                                            }


                                        }
                                        /*** ***/

                                        OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                                        if (response.StatusCode == HttpStatusCode.OK)
                                        {

                                            string messagereplyDest = "";
                                            string messagereply = "";

                                            if (sc == "00")
                                            {
                                                transactionType = "W2W";

                                                messagereplyDest = "Dear " + CustCheckUtils.GetName(da) + "," + "\n";
                                                messagereplyDest +=
                                                    "You have received NPR " + //transactiontype + " transaction was successful with amount NPR " +
                                                    validTransactionData.Amount + " in your Wallet from " + mobile + " on date " +
                                                    (validTransactionData.CreatedDate).ToString("dd/MM/yyyy") +
                                                    "." + "\n";
                                                messagereplyDest += "Thank you, MNepal";
                                            }
                                            else if (sc == "01")
                                            {
                                                transactionType = "W2B";

                                                messagereplyDest = "Dear " + CustCheckUtils.GetName(da) + "," + "\n";
                                                messagereplyDest +=
                                                "Your bank account has been deposited by NPR " + //transactiontype + " transaction was successful with amount NPR " +
                                                validTransactionData.Amount + " from " + mobile + " on date " +
                                                (validTransactionData.CreatedDate).ToString("dd/MM/yyyy") +
                                                "." + "\n";
                                                messagereplyDest += "Thank you, MNepal";
                                            }
                                            else if (sc == "10")
                                            {
                                                transactionType = "B2W";

                                                messagereplyDest = "Dear " + CustCheckUtils.GetName(da) + "," + "\n";

                                                if (mobile == da)
                                                {

                                                    messagereplyDest +=
                                                    "You have received NPR " + //transactiontype + " transaction was successful with amount NPR " +
                                                    validTransactionData.Amount + " from Bank A/C to your Wallet on date " +
                                                    (validTransactionData.CreatedDate).ToString("dd/MM/yyyy") +
                                                    "." + "\n";
                                                    messagereplyDest += "Thank you, MNepal";

                                                }
                                                else
                                                {
                                                    messagereplyDest +=
                                                        "You have received NPR " + //transactiontype + " transaction was successful with amount NPR " +
                                                        validTransactionData.Amount + " from " + mobile + " to your Wallet on date " +
                                                        (validTransactionData.CreatedDate).ToString("dd/MM/yyyy") +
                                                        "." + "\n";
                                                    messagereplyDest += "Thank you, MNepal";
                                                }
                                            }
                                            else if (sc == "11")
                                            {
                                                transactionType = "B2B";

                                                messagereplyDest = "Dear " + CustCheckUtils.GetName(da) + "," + "\n";
                                                messagereplyDest +=
                                                "Your bank account has been deposited by NPR " + //transactiontype + " transaction was successful with amount NPR " +
                                                validTransactionData.Amount + " from " + mobile +
                                                " on date " + (validTransactionData.CreatedDate).ToString("dd/MM/yyyy") +
                                                "." + "\n";
                                                messagereplyDest += "Thank you, MNepal";
                                            }

                                            try
                                            {
                                                messagereply = "Dear " + CustCheckUtils.GetName(mobile) + "," + "\n";
                                                //messagereply += transactiontype + " transaction was successful with amount NPR" +
                                                //                validTransactionData.Amount + " on date " + validTransactionData.CreatedDate +
                                                //                "." + "\n";
                                                //messagereply += "You have send NPR " +
                                                //                validTransactionData.Amount + " on date " +
                                                //                validTransactionData.CreatedDate +
                                                //                "." + "\n";
                                                //messagereply += "Thank you, MNepal";

                                                if (sc == "00") //W2W
                                                {
                                                    messagereply += "You have successfully transferred NPR " + //send
                                                                validTransactionData.Amount + " to " + da + " on date " +
                                                                (validTransactionData.CreatedDate).ToString("dd/MM/yyyy") +
                                                                "." + "\n";

                                                }
                                                else if (sc == "01") //W2B
                                                {
                                                    messagereply += "You have successfully transferred NPR " + //send
                                                                validTransactionData.Amount + " to " + da + " bank account on date " +
                                                                (validTransactionData.CreatedDate).ToString("dd/MM/yyyy") +
                                                                "." + "\n";
                                                }
                                                else if (sc == "10") //B2W
                                                {
                                                    messagereply += "You have successfully transferred NPR " + //send
                                                                validTransactionData.Amount + " from your bank account to "
                                                                + da + " on date " +
                                                                (validTransactionData.CreatedDate).ToString("dd/MM/yyyy") +
                                                                "." + "\n";
                                                }
                                                else if (sc == "11") //B2B
                                                {

                                                    messagereply += "You have successfully transferred NPR " + //send
                                                                    validTransactionData.Amount + " from your bank account to "
                                                                    + da + " bank account on date " +
                                                                    (validTransactionData.CreatedDate).ToString("dd/MM/yyyy") +
                                                                    "." + "\n";
                                                }

                                                messagereply += "Thank you, MNepal";

                                                var client = new WebClient();
                                                //var content = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=2&Password=test12test&From=9797&To=" + "977" + mobile + "&Text=" + messagereply + "");
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


                                                //FOR DESTIONATION NUMBER RECEIVER
                                                mobile = da;
                                                if ((da.Substring(0, 3) == "980") || (da.Substring(0, 3) == "981")) //FOR NCELL
                                                {
                                                    //FOR NCELL
                                                    var content = client.DownloadString(
                                                        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                                        + "977" + da + "&Text=" + messagereplyDest + "");
                                                }
                                                else if ((da.Substring(0, 3) == "985") || (da.Substring(0, 3) == "984")
                                                            || (da.Substring(0, 3) == "986"))
                                                {
                                                    //FOR NTC
                                                    var content = client.DownloadString(
                                                        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                                        + "977" + da + "&Text=" + messagereplyDest + "");
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

                                    }
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



                                }
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


                            }
                        }

                        else
                        {

                            if (checkMerchantDestinationUsertype(da))
                            {
                                // throw ex
                                statusCode = "400";
                                message = "Transaction restricted to Merchant";
                                failedmessage = message;
                            }
                            else
                            {//Agent check 
                                if (!checkSourceAndDestinationUsertype(mobile, da))
                                {
                                    // throw ex
                                    statusCode = "400";
                                    message = "Transaction restricted to Agent";
                                    failedmessage = message;
                                }
                                else
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
                                        mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                                        //end:Com focus one log//


                                        //NOTE:- may be need to validate before insert into reply typpe
                                        //start:insert into reply type as HTTP//
                                        var replyType = new MNReplyType(tid, "HTTP");
                                        var mnreplyType = new MNReplyTypesController();
                                        mnreplyType.InsertIntoReplyType(replyType);
                                        //end:insert into reply type as HTTP//

                                        //MNTransactionMaster validTransactionData = new MNTransactionMaster();

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
                                                }


                                            }
                                            /*** ***/

                                            OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                                            if (response.StatusCode == HttpStatusCode.OK)
                                            {

                                                string messagereplyDest = "";
                                                string messagereply = "";

                                                if (sc == "00")
                                                {
                                                    transactionType = "W2W";

                                                    messagereplyDest = "Dear " + CustCheckUtils.GetName(da) + "," + "\n";
                                                    messagereplyDest +=
                                                        "You have received NPR " + //transactiontype + " transaction was successful with amount NPR " +
                                                        validTransactionData.Amount + " in your Wallet from " + mobile + " on date " +
                                                        (validTransactionData.CreatedDate).ToString("dd/MM/yyyy") +
                                                        "." + "\n";
                                                    messagereplyDest += "Thank you, MNepal";
                                                }
                                                else if (sc == "01")
                                                {
                                                    transactionType = "W2B";

                                                    messagereplyDest = "Dear " + CustCheckUtils.GetName(da) + "," + "\n";
                                                    messagereplyDest +=
                                                    "Your bank account has been deposited by NPR " + //transactiontype + " transaction was successful with amount NPR " +
                                                    validTransactionData.Amount + " from " + mobile + " on date " +
                                                    (validTransactionData.CreatedDate).ToString("dd/MM/yyyy") +
                                                    "." + "\n";
                                                    messagereplyDest += "Thank you, MNepal";
                                                }
                                                else if (sc == "10")
                                                {
                                                    transactionType = "B2W";

                                                    messagereplyDest = "Dear " + CustCheckUtils.GetName(da) + "," + "\n";

                                                    if (mobile == da)
                                                    {

                                                        messagereplyDest +=
                                                        "You have received NPR " + //transactiontype + " transaction was successful with amount NPR " +
                                                        validTransactionData.Amount + " from Bank A/C to your Wallet on date " +
                                                        (validTransactionData.CreatedDate).ToString("dd/MM/yyyy") +
                                                        "." + "\n";
                                                        messagereplyDest += "Thank you, MNepal";

                                                    }
                                                    else
                                                    {
                                                        messagereplyDest +=
                                                            "You have received NPR " + //transactiontype + " transaction was successful with amount NPR " +
                                                            validTransactionData.Amount + " from " + mobile + " to your Wallet on date " +
                                                            (validTransactionData.CreatedDate).ToString("dd/MM/yyyy") +
                                                            "." + "\n";
                                                        messagereplyDest += "Thank you, MNepal";
                                                    }
                                                }
                                                else if (sc == "11")
                                                {
                                                    transactionType = "B2B";

                                                    messagereplyDest = "Dear " + CustCheckUtils.GetName(da) + "," + "\n";
                                                    messagereplyDest +=
                                                    "Your bank account has been deposited by NPR " + //transactiontype + " transaction was successful with amount NPR " +
                                                    validTransactionData.Amount + " from " + mobile +
                                                    " on date " + (validTransactionData.CreatedDate).ToString("dd/MM/yyyy") +
                                                    "." + "\n";
                                                    messagereplyDest += "Thank you, MNepal";
                                                }

                                                try
                                                {
                                                    messagereply = "Dear " + CustCheckUtils.GetName(mobile) + "," + "\n";
                                                    //messagereply += transactiontype + " transaction was successful with amount NPR" +
                                                    //                validTransactionData.Amount + " on date " + validTransactionData.CreatedDate +
                                                    //                "." + "\n";
                                                    //messagereply += "You have send NPR " +
                                                    //                validTransactionData.Amount + " on date " +
                                                    //                validTransactionData.CreatedDate +
                                                    //                "." + "\n";
                                                    //messagereply += "Thank you, MNepal";

                                                    if (sc == "00") //W2W
                                                    {
                                                        messagereply += "You have successfully transferred NPR " + //send
                                                                    validTransactionData.Amount + " to " + da + " on date " +
                                                                    (validTransactionData.CreatedDate).ToString("dd/MM/yyyy") +
                                                                    "." + "\n";

                                                    }
                                                    else if (sc == "01") //W2B
                                                    {
                                                        messagereply += "You have successfully transferred NPR " + //send
                                                                    validTransactionData.Amount + " to " + da + " bank account on date " +
                                                                    (validTransactionData.CreatedDate).ToString("dd/MM/yyyy") +
                                                                    "." + "\n";
                                                    }
                                                    else if (sc == "10") //B2W
                                                    {
                                                        messagereply += "You have successfully transferred NPR " + //send
                                                                    validTransactionData.Amount + " from your bank account to "
                                                                    + da + " on date " +
                                                                    (validTransactionData.CreatedDate).ToString("dd/MM/yyyy") +
                                                                    "." + "\n";
                                                    }
                                                    else if (sc == "11") //B2B
                                                    {

                                                        messagereply += "You have successfully transferred NPR " + //send
                                                                        validTransactionData.Amount + " from your bank account to "
                                                                        + da + " bank account on date " +
                                                                        (validTransactionData.CreatedDate).ToString("dd/MM/yyyy") +
                                                                        "." + "\n";
                                                    }

                                                    messagereply += "Thank you, MNepal";

                                                    var client = new WebClient();
                                                    //var content = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=2&Password=test12test&From=9797&To=" + "977" + mobile + "&Text=" + messagereply + "");
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


                                                    //FOR DESTIONATION NUMBER RECEIVER
                                                    mobile = da;
                                                    if ((da.Substring(0, 3) == "980") || (da.Substring(0, 3) == "981")) //FOR NCELL
                                                    {
                                                        //FOR NCELL
                                                        var content = client.DownloadString(
                                                            "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                                            + "977" + da + "&Text=" + messagereplyDest + "");
                                                    }
                                                    else if ((da.Substring(0, 3) == "985") || (da.Substring(0, 3) == "984")
                                                                || (da.Substring(0, 3) == "986"))
                                                    {
                                                        //FOR NTC
                                                        var content = client.DownloadString(
                                                            "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                                            + "977" + da + "&Text=" + messagereplyDest + "");
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

                                        }
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



                                    }
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

                                }
                            }
                        }
                    }
                    else
                    {
                        failedmessage = "Destination mobile Not Registered";
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

                        var v = new
                        {
                            StatusCode = Convert.ToInt32(400),
                            StatusMessage = "Destination mobile Not Registered"
                        };
                        result = JsonConvert.SerializeObject(v);
                    }

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
        private string InsertIntoReplyType(string tid, string type)
        {
            string result = type;
            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;
            string sql = "Insert into MNReplyType values ('" + type + "','" + tid + "')";
            try
            {
                using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(conStr))
                {
                    con.Open();
                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, con))
                        command.ExecuteNonQuery();
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                result = ex.ToString();
            }
            return type;
        }
        static void BackgroundTaskWithObject(Object stateInfo)
        {
            FundTransfer data = (FundTransfer)stateInfo;
            Console.WriteLine($"Hi {data.tid} from ThreadPool.");
            Thread.Sleep(1000);
        }

        #region LoadWallet Request 
        [OperationContract]
        [WebInvoke(Method = "POST",
                   ResponseFormat = WebMessageFormat.Json)]
        public string loadwalletrequest(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            SoapTransaction soapTransaction = new SoapTransaction();

            soapTransaction.BID = qs["BID"].Trim();
            soapTransaction.PRN = qs["PREF"].Trim();
            soapTransaction.Amount = qs["AMOUNT"].Trim();
            soapTransaction.BankID = qs["BankId"].Trim();
            soapTransaction.AMT1 = qs["AMT1"].Trim();
            soapTransaction.FirstName = qs["FirstName"].Trim();
            soapTransaction.LastName = qs["LastName"].Trim();
            soapTransaction.CustRefType = qs["CustRefType"].Trim();
            soapTransaction.ITC = soapTransaction.CustRefType;
            soapTransaction.Email = qs["EmailID"].Trim();
            soapTransaction.UID = qs["UID"].Trim();


            string totalAmount = string.Empty;
            string totalCount = string.Empty;
            string totalBAmount = string.Empty;
            string totalBCount = string.Empty;
            string balance = string.Empty;
            string message = string.Empty;
            string statusCode = string.Empty;
            string failedmessage = string.Empty;
            string customerNo = string.Empty;

            string tid = "";
            string sc = "00";
            string da = GetMobileNumberFromPRN(soapTransaction.PRN); //get Mobile Number from Preference Number
            string mobile = "";
            //string mobile = qs["mobile"];
            //string da = qs["da"];
            string amount = soapTransaction.Amount;
            string pin = "";
            string note = "";
            string src = "";
            string result = "";
            string transactionType = "EBanking Load Wallet Request";


            CustActivityModel custsmsInfo = new CustActivityModel();
            FundTransfer fundtransfer = new FundTransfer
            {
                da = da,
                sc = sc,
                mobile = mobile,
                amount = amount,
                pin = pin,
                note = note,
            };
            ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, fundtransfer);
            MNTransactionMaster validTransactionData = new MNTransactionMaster();
            if (checkEbankingRequest(soapTransaction))
            {

                if ((sc == null) || (mobile == null) || (da == null) || (da == "") || (amount == null) || (double.Parse(amount) <= 0) || (soapTransaction.PRN == null))
                {
                    // throw ex
                    statusCode = "400";
                    message = "Parameters Missing/Invalid";
                    failedmessage = message;
                }
                else
                {
                    bool verifyTransaction = VerifyTransaction(soapTransaction); //check Transaction from Bank
                    if (verifyTransaction == true)
                    {

                        //start:Registered Customer Check Mobile
                        if (da != "")
                        {
                            DataTable dtableUserCheck = CustCheckUtils.GetCustUserInfo(da);
                            if (dtableUserCheck.Rows.Count == 0)
                            {
                                customerNo = "0";
                            }
                            else if (dtableUserCheck.Rows.Count > 0)
                            {
                                customerNo = da;
                            }
                        }
                        //end:Registered Customer Check Mobile

                        if (sc == "00")
                        {
                            transactionType = "Fund Transfer to W2W";
                        }
                        if ((customerNo != "0") && (message == "") && (statusCode != "400"))
                        {

                            tid = getEbankingRetrievalRef(soapTransaction);
                            //start: checking trace id
                            //do
                            //{
                            //    TraceIdGenerator traceid = new TraceIdGenerator();
                            //    tid = traceid.GenerateUniqueTraceID();
                            //    fundtransfer.tid = tid;

                            //    bool traceIdCheck = false;
                            //    traceIdCheck = TraceIdCheck.IsValidTraceId(fundtransfer.tid);
                            //    if (traceIdCheck == true)
                            //    {
                            //        result = "Trace ID Repeated";
                            //    }
                            //    else
                            //    {
                            //        result = "false";
                            //    }

                            //} while (result == "Trace ID Repeated");
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

                            //MNTransactionMaster validTransactionData = new MNTransactionMaster();

                            //start:insert into transaction master//
                            if ((sc != null) || (mobile != null) || (da != null) || (amount != null))
                            {
                                var transaction = new MNTransactionMaster(mnft);
                                var mntransaction = new MNTransactionsController();
                                validTransactionData = mntransaction.LoadWalletValidate(transaction, mnft.pin);
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
                                        message = result + pin;
                                        failedmessage = message;
                                    }
                                    if (result == "111")
                                    {
                                        statusCode = result;
                                        message = em.Error_111 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "114")
                                    {
                                        statusCode = result;
                                        message = em.Error_114 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "115")
                                    {
                                        statusCode = result;
                                        message = em.Error_115 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "116")
                                    {
                                        statusCode = result;
                                        message = em.Error_116 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "119")
                                    {
                                        statusCode = result;
                                        message = em.Error_119 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "121")
                                    {
                                        statusCode = result;
                                        message = em.Error_121 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "163")
                                    {
                                        statusCode = result;
                                        message = em.Error_163 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "180")
                                    {
                                        statusCode = result;
                                        message = em.Error_180 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "181")
                                    {
                                        statusCode = result;
                                        message = em.Error_181 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "182")
                                    {
                                        statusCode = result;
                                        message = em.Error_182 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "183")
                                    {
                                        statusCode = result;
                                        message = em.Error_183 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "184")
                                    {
                                        statusCode = result;
                                        message = em.Error_184 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "185")
                                    {
                                        statusCode = result;
                                        message = em.Error_185 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "186")
                                    {
                                        statusCode = result;
                                        message = em.Error_186 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "187")
                                    {
                                        statusCode = result;
                                        message = em.Error_187 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "188")
                                    {
                                        statusCode = result;
                                        message = em.Error_188 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "189")
                                    {
                                        statusCode = result;
                                        message = em.Error_189 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "190")
                                    {
                                        statusCode = result;
                                        message = em.Error_190 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "800")
                                    {
                                        statusCode = result;
                                        message = em.Error_800 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "902")
                                    {
                                        statusCode = result;
                                        message = em.Error_902 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "904")
                                    {
                                        statusCode = result;
                                        message = em.Error_904 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "906")
                                    {
                                        statusCode = result;
                                        message = em.Error_906 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "907")
                                    {
                                        statusCode = result;
                                        message = em.Error_907 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "909")
                                    {
                                        statusCode = result;
                                        message = em.Error_909 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "911")
                                    {
                                        statusCode = result;
                                        message = em.Error_911 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "913")
                                    {
                                        statusCode = result;
                                        message = em.Error_913 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "90")
                                    {
                                        statusCode = result;
                                        message = em.Error_90 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "91")
                                    {
                                        statusCode = result;
                                        message = em.Error_91 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "92")
                                    {
                                        statusCode = result;
                                        message = em.Error_92 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "94")
                                    {
                                        statusCode = result;
                                        message = em.Error_94 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "95")
                                    {
                                        statusCode = result;
                                        message = em.Error_95 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "98")
                                    {
                                        statusCode = result;
                                        message = em.Error_98 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "99")
                                    {
                                        statusCode = result;
                                        message = em.Error_99 + " " + result;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    else if (validTransactionData.ResponseCode == "OK")
                                    {
                                        statusCode = "200";
                                        message = result;
                                        mnft.ResponseStatus(HttpStatusCode.OK, message);
                                    }


                                }
                                /*** ***/

                                OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                                if (response.StatusCode == HttpStatusCode.OK)
                                {

                                    string messagereplyDest = "";
                                    string messagereply = "";

                                    if (sc == "00")
                                    {
                                        transactionType = "W2W";

                                        messagereplyDest = "Dear Customer," + "\n";
                                        messagereplyDest +=
                                            "You have received NPR " + //transactiontype + " transaction was successful with amount NPR " +
                                            validTransactionData.Amount + " in your Wallet from " + mobile + " on date " +
                                            (validTransactionData.CreatedDate).ToString("dd/MM/yyyy") +
                                            "." + "\n";
                                        messagereplyDest += "Thank you, MNepal";
                                    }


                                    try
                                    {
                                        messagereply = "Dear " + CustCheckUtils.GetName(da) + "," + "\n";
                                        //messagereply += transactiontype + " transaction was successful with amount NPR" +
                                        //                validTransactionData.Amount + " on date " + validTransactionData.CreatedDate +
                                        //                "." + "\n";
                                        //messagereply += "You have send NPR " +
                                        //                validTransactionData.Amount + " on date " +
                                        //                validTransactionData.CreatedDate +
                                        //                "." + "\n";
                                        //messagereply += "Thank you, MNepal";

                                        if (sc == "00") //W2W
                                        {
                                            messagereply += "You have successfully transferred NPR " + //send
                                                        validTransactionData.Amount + " to " + da + " on date " +
                                                        (validTransactionData.CreatedDate).ToString("dd/MM/yyyy") +
                                                        "." + "\n";

                                        }

                                        messagereply += "Thank you, MNepal";

                                        var client = new WebClient();
                                        //var content = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=2&Password=test12test&From=9797&To=" + "977" + mobile + "&Text=" + messagereply + "");
                                        //SENDER
                                        //if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                                        //{
                                        //    //FOR NCELL
                                        //    var content = client.DownloadString(
                                        //        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                        //        + "977" + mobile + "&Text=" + messagereply + "");
                                        //}
                                        //else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                        //            || (mobile.Substring(0, 3) == "986"))
                                        //{
                                        //    //FOR NTC
                                        //    var content = client.DownloadString(
                                        //        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                        //        + "977" + mobile + "&Text=" + messagereply + "");
                                        //}


                                        //FOR DESTIONATION NUMBER RECEIVER
                                        mobile = da;
                                        if ((da.Substring(0, 3) == "980") || (da.Substring(0, 3) == "981")) //FOR NCELL
                                        {
                                            //FOR NCELL
                                            var content = client.DownloadString(
                                                "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                                + "977" + da + "&Text=" + messagereplyDest + "");
                                        }
                                        else if ((da.Substring(0, 3) == "985") || (da.Substring(0, 3) == "984")
                                                    || (da.Substring(0, 3) == "986"))
                                        {
                                            //FOR NTC
                                            var content = client.DownloadString(
                                                "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                                + "977" + da + "&Text=" + messagereplyDest + "");
                                        }

                                        statusCode = "200";
                                        var v = new
                                        {
                                            StatusCode = Convert.ToInt32(statusCode),
                                            StatusMessage = result
                                        };
                                        result = JsonConvert.SerializeObject(v);
                                        CustCheckUtils.InsertEBankingResponse(soapTransaction, statusCode);  //Insert value  into Response Table


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
                            }
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

                        }
                        else
                        {
                            failedmessage = "Destination mobile Not Registered";
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

                            var v = new
                            {
                                StatusCode = Convert.ToInt32(400),
                                StatusMessage = "Destination mobile Not Registered"
                            };
                            result = JsonConvert.SerializeObject(v);
                        }
                    }
                    else
                    {
                        // throw ex
                        statusCode = "400";
                        message = "Transaction Not Verified.";
                        failedmessage = message;

                        custsmsInfo = new CustActivityModel()
                        {
                            UserName = fundtransfer.da,
                            RequestMerchant = transactionType,
                            DestinationNo = fundtransfer.da,
                            Amount = amount,
                            SMSStatus = "Failed",
                            SMSSenderReply = message,
                            ErrorMessage = failedmessage,
                        };
                    }
                }
            } //Check Transaction from db
            else
            {
                statusCode = "400";
                message = "Transaction Doesnot Match.";
                failedmessage = message;

                custsmsInfo = new CustActivityModel()
                {
                    UserName = fundtransfer.da,
                    RequestMerchant = transactionType,
                    DestinationNo = fundtransfer.da,
                    Amount = amount,
                    SMSStatus = "Failed",
                    SMSSenderReply = message,
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

            string thailiUrl = System.Web.Configuration.WebConfigurationManager.AppSettings["ThailiApp"];
            string thailiAgentUrl = System.Web.Configuration.WebConfigurationManager.AppSettings["ThailiAgentApp"];
            if (UserNameCheck.IsValidAgent(da))
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.Redirect;
                WebOperationContext.Current.OutgoingResponse.Headers.Add("Location", thailiAgentUrl);
            }
            else
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.Redirect;
                WebOperationContext.Current.OutgoingResponse.Headers.Add("Location", thailiUrl);
            }

            return result;
        }

        protected bool VerifyTransaction(SoapTransaction soapTransaction)
        {
            bool returnResult = false;
            WebReference.VerifyHeader objVerifyHeader = new WebReference.VerifyHeader();
            WebReference.VerifyThisSoapClient objVerifyThis = new WebReference.VerifyThisSoapClient();

            objVerifyHeader.H1 = System.Web.Configuration.WebConfigurationManager.AppSettings["H1"];
            objVerifyHeader.H2 = System.Web.Configuration.WebConfigurationManager.AppSettings["H2"];
            objVerifyHeader.H3 = System.Web.Configuration.WebConfigurationManager.AppSettings["H3"];

            string BID = soapTransaction.BID;
            string ITC = soapTransaction.ITC;
            string PRN = soapTransaction.PRN;
            string Amount = soapTransaction.Amount;

            string result = objVerifyThis.VerifyTxn(objVerifyHeader, BID, ITC, PRN, Amount);
            if (result == "Success for BID <" + BID + ">")
            {
                returnResult = true;
            }
            else if (result == "Failed")
            {
                returnResult = false;
            }
            else
            {
                returnResult = false;
            }
            return returnResult;
        }

        #region Test Verification
        [OperationContract]
        [WebGet]
        protected string Verify1Transaction()
        {
            bool returnResult = false;
            WebReference.VerifyHeader objVerifyHeader = new WebReference.VerifyHeader();
            WebReference.VerifyThisSoapClient objVerifyThis = new WebReference.VerifyThisSoapClient();

            objVerifyHeader.H1 = "000000000286";
            objVerifyHeader.H2 = "thaili";
            objVerifyHeader.H3 = "thaili";

            //objVerifyHeader.H1 = System.Web.Configuration.WebConfigurationManager.AppSettings["H1"];
            //objVerifyHeader.H2 = System.Web.Configuration.WebConfigurationManager.AppSettings["H2"];
            //objVerifyHeader.H3 = System.Web.Configuration.WebConfigurationManager.AppSettings["H3"];

            //string BID = "9690";
            //string ITC = "4774070";
            //string PRN = "4774070";
            //string Amount = "337";

            string BID = "11671";
            string ITC = "0001100";
            string PRN = "9798491550";
            string Amount = "15";

            string result = objVerifyThis.VerifyTxn(objVerifyHeader, BID, ITC, PRN, Amount);

            //if (result == "Success for BID <" + BID + ">")
            //{
            //    returnResult = true;
            //}
            //else if (result == "Failed")
            //{
            //    returnResult = false;
            //}
            //else
            //{
            //    returnResult = false;
            //}
            //return returnResult;
            return result;
        }
        #endregion
        public string GetMobileNumberFromPRN(string PRN)
        {
            return CustCheckUtils.GetMobileNumberFromPRN(PRN);
        }

        public bool checkEbankingRequest(SoapTransaction transaction)
        {
            bool result = false;
            string CITC = "";
            string CPRN = "";
            string CAmount = "";
            DataTable dtcheckRequest = CustCheckUtils.GetEBankingRequest(transaction.PRN);
            if (dtcheckRequest.Rows.Count == 1)
            {
                foreach (DataRow row in dtcheckRequest.Rows)
                {

                    CPRN = dtcheckRequest.Rows[0]["PaymentReferenceNumber"].ToString();
                    CAmount = dtcheckRequest.Rows[0]["Amount"].ToString();
                    CITC = dtcheckRequest.Rows[0]["ItemCode"].ToString();

                }
                if (transaction.PRN == CPRN && transaction.Amount == CAmount && transaction.CustRefType == CITC)
                {
                    result = true;
                }
            }
            else
            {
                result = false;
            }
            return result;
        }

        public string getEbankingRetrievalRef(SoapTransaction transaction)
        {
            string retRef;
            DataTable dtcheckRequest = CustCheckUtils.GetEBankingRequest(transaction.PRN);
            if (dtcheckRequest.Rows.Count == 1)
            {
                retRef = dtcheckRequest.Rows[0]["retrievalReference"].ToString();
            }
            else
            {
                retRef = " ";
            }
            return retRef;
        }

        #endregion
        #region Checking SOurce and Destination Numbers
        public bool checkSourceAndDestinationUsertype(string source, string destination)
        {

            if (UserNameCheck.IsValidUserNameForgotPassword(source) && UserNameCheck.IsValidUserNameForgotPassword(destination))
            {
                return true;
            }
            if (UserNameCheck.IsValidAgent(source) && UserNameCheck.IsValidUserName(destination))
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
    }
}
