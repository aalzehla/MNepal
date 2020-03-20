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
using MNepalWCF.Utilities;
using MNepalProject.Helper;
using MNepalWCF.Models;
using MNepalWCF.ErrorMsg;
using WCF.MNepal.Helper;

namespace MNepalWCF
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class ft
    {
        [OperationContract]
        [WebInvoke(Method = "POST",
                  
                  ResponseFormat = WebMessageFormat.Json
                  )]

        public string request(Stream input)//
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
            string transactionType = string.Empty;
            FundTransfer fundtransfer = new FundTransfer();
            fundtransfer.tid = tid;
            fundtransfer.sc = sc;
            fundtransfer.mobile = mobile;
            fundtransfer.da = da;
            fundtransfer.amount = amount;
            fundtransfer.pin = pin;
            fundtransfer.note = note;
            fundtransfer.sourcechannel = src;

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

            if ((tid == "null") || (sc == "null") || (mobile == "null") || (da == "null") || (amount == "null") || (pin == "null") ||
                (src == "null"))
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
                        //start:Trace ID///
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
                        //End: Tace ID//

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
                                if (result == "116")
                                {
                                    statusCode = result;
                                    message = em.Error_116 + " " + result;
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
                                if (result == "911")
                                {
                                    statusCode = result;
                                    message = em.Error_911 + " " + result;
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
                                //if (result.Substring(0, 5) == "Error")
                                //{
                                //    statusCode = "400";
                                //    message = "Connection Failure from Gateway. Please Contact your Bank." + result;
                                //    mnft.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                //    failedmessage = result;
                                //}
                                else if (result == "200")
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
                                        validTransactionData.CreatedDate +
                                        "." + "\n";
                                    messagereplyDest += "Thank you, MNepal";
                                }
                                else if (sc == "01")
                                {
                                    transactionType = "W2B";

                                    messagereplyDest = "Dear Customer," + "\n";
                                    messagereplyDest +=
                                        "Your bank account has been deposited by NPR " + //transactiontype + " transaction was successful with amount NPR " +
                                        validTransactionData.Amount + " from " + mobile + " on date " +
                                        validTransactionData.CreatedDate +
                                        "." + "\n";
                                    messagereplyDest += "Thank you, MNepal";
                                }
                                else if (sc == "10")
                                {
                                    transactionType = "B2W";

                                    messagereplyDest = "Dear Customer," + "\n";

                                    if (mobile == da)
                                    {

                                        messagereplyDest +=
                                        "You have received NPR " + //transactiontype + " transaction was successful with amount NPR " +
                                        validTransactionData.Amount + " from Bank A/C to your Wallet on date " +
                                        validTransactionData.CreatedDate +
                                        "." + "\n";
                                        messagereplyDest += "Thank you, MNepal";

                                    }
                                    else
                                    {
                                        messagereplyDest +=
                                            "You have received NPR " + //transactiontype + " transaction was successful with amount NPR " +
                                            validTransactionData.Amount + " from " + mobile + " to your Wallet on date " +
                                            validTransactionData.CreatedDate +
                                            "." + "\n";
                                        messagereplyDest += "Thank you, MNepal";
                                    }
                                }
                                else if (sc == "11")
                                {
                                    transactionType = "B2B";

                                    messagereplyDest = "Dear Customer," + "\n";
                                    messagereplyDest +=
                                        "Your bank account has been deposited by NPR " + //transactiontype + " transaction was successful with amount NPR " +
                                        validTransactionData.Amount + " from " + mobile +
                                        " on date " + validTransactionData.CreatedDate +
                                        "." + "\n";
                                    messagereplyDest += "Thank you, MNepal";
                                }

                                try
                                {
                                    messagereply = "Dear Customer," + "\n";
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
                                                    validTransactionData.CreatedDate +
                                                    "." + "\n";

                                    }
                                    else if (sc == "01") //W2B
                                    {
                                        messagereply += "You have successfully transferred NPR " + //send
                                                    validTransactionData.Amount + " to " + da + " bank account on date " +
                                                    validTransactionData.CreatedDate +
                                                    "." + "\n";
                                    }
                                    else if (sc == "10") //B2W
                                    {
                                        messagereply += "You have successfully transferred NPR " + //send
                                                    validTransactionData.Amount + " from your bank account to "
                                                    + da + " on date " +
                                                    validTransactionData.CreatedDate +
                                                    "." + "\n";
                                    }
                                    else if (sc == "11") //B2B
                                    {

                                        messagereply += "You have successfully transferred NPR " + //send
                                                        validTransactionData.Amount + " from your bank account to "
                                                        + da + " bank account on date " +
                                                        validTransactionData.CreatedDate +
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
                            else if ((response.StatusCode == HttpStatusCode.BadRequest) || (response.StatusCode == HttpStatusCode.InternalServerError))
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
                    if(string.IsNullOrEmpty(result))
                    {
                        result = failedmessage; // "Could not process your request,Please Try again.";
                    }
                    message = result;
                }
                var v = new {
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

        

    }
}
