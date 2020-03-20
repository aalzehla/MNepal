using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Web;
using MNepalProject.Controllers;
using MNepalProject.Helper;
using MNepalProject.Models;
using WCF.MNepal.Utilities;
using WCF.MNepal.Helper;
using WCF.MNepal.Models;
using WCF.MNepal.ErrorMsg;

namespace WCF.MNepal
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class Topup
    {
        [OperationContract]
        [WebInvoke(Method = "POST",
                    ResponseFormat = WebMessageFormat.Json
                    )]

        public string Payment(Stream input)
        {
            //string tid, string sc, string vid, string mobile,string sa, string prod, string amount,string pin, string note, string src
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            string sc = qs["sc"];
            string vid = qs["vid"];
            string mobile = qs["mobile"];  // user's mobile number
            string destmobile = qs["destmobile"];  // destination's mobile number or topup no
            string amount = qs["amount"];
            string tid = qs["tid"];
            string pin = qs["pin"];
            string note = qs["note"];
            string src = qs["src"];

            string result = "";
            string getMerchantName = "";

            ReplyMessage replyMessage = new ReplyMessage();

            CustActivityModel custsmsInfo = new CustActivityModel();

            FundTransfer fundtransfer = new FundTransfer
            {
                tid = tid,
                sc = sc,
                mobile = mobile,
                amount = amount,
                pin = pin,
                sourcechannel = src
            };

            //Utility Payment / TOPUP
            if (sc == "31" || sc == "34")
            {
                fundtransfer.da = vid;
                fundtransfer.note = note;// + ":" + prod;
            }

            
            string totalCount = string.Empty;
            string totalBAmount = string.Empty;
            string totalBCount = string.Empty;
            string balance = string.Empty;
            string statusCode = "";
            string message = string.Empty;
            string failedmessage = string.Empty;
            

            MNTransactionMaster validTransactionData = new MNTransactionMaster();

            if ((tid == null) || (sc == null) || (mobile == null) || (vid == null) || (amount == null) || (pin == null) ||
                (src == null))
            {
                // throw ex
                statusCode = "400";
                message = "Parameters Missing/Invalid";
            }
            else
            {
                TransLimitCheck transLimitCheck = new TransLimitCheck();
                string resultTranLimit = transLimitCheck.LimitCheck(mobile, destmobile, amount, sc, pin, src);

                var jsonDataResult = JObject.Parse(resultTranLimit);
                statusCode = jsonDataResult["StatusCode"].ToString();
                string statusMsg = jsonDataResult["StatusMessage"].ToString();
                message = jsonDataResult["StatusMessage"].ToString();
                failedmessage = message;

                //Utility Payment / TOPUP
                if (sc == "31")
                {
                    getMerchantName = " Topup Wallet Check ";
                }
                if (sc == "34")
                {
                    getMerchantName = " Topup Bank Check ";
                }
                

                if ((statusCode == "200") && (statusMsg == "Success") && (statusCode != "400"))
                {
                    
                    TraceIdGenerator tig = new TraceIdGenerator();
                    tid = tig.GenerateUniqueTraceID(); //GenerateTraceID();
                    fundtransfer.tid = tid;

                    ////start:Com focus one log////

                    MNFundTransfer mnft = new MNFundTransfer(fundtransfer.tid, fundtransfer.sc, fundtransfer.mobile,
                        fundtransfer.sa, fundtransfer.amount, fundtransfer.da, fundtransfer.note, fundtransfer.pin,
                        fundtransfer.sourcechannel);
                    var comfocuslog = new MNComAndFocusOneLog(mnft, DateTime.Now);
                    var mncomfocuslog = new MNComAndFocusOneLogsController();
                    result = mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                    ////end:Com focus one log//

                    if (result == "Success")
                    {
                        //NOTE:- may be need to validate before insert into reply typpe
                        //start:insert into reply type as HTTP//
                        var replyType = new MNReplyType(fundtransfer.tid, "HTTP");
                        var mnreplyType = new MNReplyTypesController();
                        mnreplyType.InsertIntoReplyType(replyType);
                        //end:insert into reply type as HTTP//

                        MNMerchantsController getMerchantDetails = new MNMerchantsController();

                        string getMerchantMobile = getMerchantDetails.PassVidToGetMerchantDetail(vid);
                        if ((getMerchantMobile != "") || (getMerchantMobile != null))
                        {
                            fundtransfer.da = getMerchantMobile; //Set Destination Merchant Mobile number

                            getMerchantName = getMerchantDetails.PassVIdToGetMerchantName(vid);

                        }
                        else
                        {
                            replyMessage.Response = "Destination Merchant Doesnot Exists";
                            replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                        }

                        //fundtransfer.note =
                        string notedDestmobile = getMerchantName + ":" + destmobile;
                        MNFundTransfer mnftWithMerchant = new MNFundTransfer(fundtransfer.tid, fundtransfer.sc,
                            fundtransfer.mobile, fundtransfer.sa, fundtransfer.amount, fundtransfer.da, notedDestmobile,
                            fundtransfer.pin, fundtransfer.sourcechannel);

                        //MNTransactionMaster validTransactionData = new MNTransactionMaster();

                        //start:insert into transaction master//
                        if (mnft.valid() && mnftWithMerchant.valid())
                        {
                            var transaction = new MNTransactionMaster(mnftWithMerchant);
                            var mntransaction = new MNTransactionsController();
                            validTransactionData = mntransaction.Validate(transaction, mnftWithMerchant.pin);
                            result = validTransactionData.Response;

                            /********/
                            ErrorMessage em = new ErrorMessage();
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
                                    || (result == "Invalid Product Request") || (result == "") )
                                {
                                    statusCode = "400";
                                    message = result;
                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, "parameters missing/invalid");
                                    failedmessage = result + pin;
                                }
                                if (result == "Invalid PIN")
                                {
                                    statusCode = "400";
                                    message = result + " " + pin;
                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Pin");
                                    failedmessage = result + pin;
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
                                //    message = "Connection Failure from Gateway. Please Contact your Bank." + balance;
                                //    replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                //    failedmessage = result;
                                //}
                                else if (statusCode == "200")
                                {
                                    statusCode = "200";
                                    mnft.ResponseStatus(HttpStatusCode.OK, "Success");
                                    var v = new
                                    {
                                        StatusCode = Convert.ToInt32(statusCode),
                                        StatusMessage = result
                                    };
                                    result = JsonConvert.SerializeObject(v);
                                }

                            }
                        }
                        else
                        {
                            mnft.Response = "error";
                            mnft.ResponseStatus(HttpStatusCode.BadRequest, "parameters missing/invalid");
                            result = mnft.Response;
                            statusCode = "400";
                            message = "parameters missing/invalid";
                            failedmessage = message;
                        }

                        //end:insert into transaction master//
                    }

                    else
                    {
                        mnft.Response = "Data Insertion Failed in Check DB Connection: TraceID limit might have exceeded";
                        mnft.ResponseStatus(HttpStatusCode.InternalServerError, replyMessage.Response);
                        result = mnft.Response;
                        failedmessage = result;
                    }


                    OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        try
                        {
                            var jsonData = JObject.Parse(result);
                            var amtTransfer = jsonData["AmounttransferredBalance"];

                            //Sender
                            string messagereply = "Dear Customer," + "\n";
                            //messagereply += getMerchantName + " was successfully transferred with amount NPR " + validTransactionData.Amount //amtTransfer
                            //                + "." + "\n"; //validTransactionData.CreatedDate
                            messagereply += getMerchantName + " successful NPR "
                                            + validTransactionData.Amount + " for " + destmobile
                                            + "." + "\n";
                            messagereply += "Thank you. MNepal";

                            //Receiver
                            string messagereplyReceiver = "Dear Customer," + "\n";
                            messagereplyReceiver += getMerchantName + " was successfully transferred with amount NPR "
                                            + validTransactionData.Amount
                                            + "." + "\n";
                            messagereplyReceiver += "Thank you. MNepal";
                            
                            var client = new WebClient();

                            //var content = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To=" + "977" + mobile + "&Text=" + messagereply + "");

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

                            //For destmobile
                            string mobileReceiver = destmobile;
                            if ((mobileReceiver.Substring(0, 3) == "980") || (mobileReceiver.Substring(0, 3) == "981")) //FOR NCELL
                            {
                                //FOR NCELL
                                var content = client.DownloadString(
                                    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                    + "977" + mobileReceiver + "&Text=" + messagereplyReceiver + "");
                            }
                            else if ((mobileReceiver.Substring(0, 3) == "985") || (mobileReceiver.Substring(0, 3) == "984")
                                        || (mobileReceiver.Substring(0, 3) == "986"))
                            {
                                //FOR NTC
                                var content = client.DownloadString(
                                    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                    + "977" + mobileReceiver + "&Text=" + messagereplyReceiver + "");
                            }


                            custsmsInfo = new CustActivityModel()
                            {
                                UserName = mobile,
                                RequestMerchant = getMerchantName,
                                DestinationNo = destmobile,
                                Amount = validTransactionData.Amount.ToString(),
                                SMSStatus = "Success",
                                SMSSenderReply = messagereply,
                                ErrorMessage = "",
                            };
                            
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                    else if ((response.StatusCode == HttpStatusCode.BadRequest) || (response.StatusCode == HttpStatusCode.InternalServerError))
                    {
                        custsmsInfo = new CustActivityModel()
                        {
                            UserName = mobile,
                            RequestMerchant = getMerchantName,
                            DestinationNo = destmobile,
                            Amount = validTransactionData.Amount.ToString(),
                            SMSStatus = "Failed",
                            SMSSenderReply = "",
                            ErrorMessage = failedmessage + " " + pin,
                        };
                    }


                }
                else
                {
                    custsmsInfo = new CustActivityModel()
                    {
                        UserName = mobile,
                        RequestMerchant = getMerchantName,
                        DestinationNo = destmobile,
                        Amount = validTransactionData.Amount.ToString(),
                        SMSStatus = "Failed",
                        SMSSenderReply = "",
                        ErrorMessage = failedmessage,
                    };
                }


                try
                {
                    int results = CustActivityUtils.RegisterCustActivityInfo(custsmsInfo);
                    if (results > 0)
                    {
                        if (statusCode != "200")
                        {
                            result = message;
                            message = result;
                        }
                        else {
                            message = result;
                        }
                        
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

            if (statusCode == "")
            {
                result = result.ToString();
            }
            else if (statusCode != "200")
            {
                if (message == "")
                {
                    message = "Insufficient Balance";
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


    }
}
