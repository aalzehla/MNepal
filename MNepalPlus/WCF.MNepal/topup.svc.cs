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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WCF.MNepal.Utilities;
using WCF.MNepal.Helper;
using WCF.MNepal.Models;
using WCF.MNepal.ErrorMsg;
using System.Threading;
using System.Threading.Tasks;
using System.Data;

namespace WCF.MNepal
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
    public class topup
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
            string sessionID = qs["tokenID"];
            string selectADSL = qs["selectADSL"]; //Unlimited //VolumeBased

            //SMS
            string SMSNTC = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalNTCSMSServerUrl"];
            string SMSNCELL = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalSMSServerUrl"];

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
                vid = vid,
                sourcechannel = src
            };

            ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, fundtransfer);
            MNTransactionMaster validTransactionData = new MNTransactionMaster();
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
            string merchantpin = string.Empty;

            MNTransactionMaster VerifyTopupMerchant = new MNTransactionMaster();

            //if (TokenGenerator.TokenChecker(sessionID, mobile, src) == false)
            //{
            //    // throw ex
            //    statusCode = "400";
            //    message = "Session expired. Please login again";
            //    failedmessage = message;
            //}
            //else
            //{
                if ((tid == null) || (sc == null) || (mobile == null) || (vid == null) || (amount == null) || (pin == null) ||
                (src == null) || (double.Parse(amount) <= 0))
            {
                // throw ex
                statusCode = "400";
                message = "Parameters Missing/Invalid";
            }
                else if ((vid == "1" || vid == "7") && !checkLandline(destmobile))
                {
                    statusCode = "400";
                    message = "Please enter valid Number";
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

                    //start block msg 3 time pin attempt
                    if (message == "Invalid PIN ")
                    {
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
                    //end block msg 3 time pin attempt

                    //Utility Payment / TOPUP
                    if (sc == "31")
                    {
                        getMerchantName = " Topup Wallet Check ";
                    }
                    if (sc == "34")
                    {
                        getMerchantName = " Topup Bank Check ";
                    }

                    //start comment gareko

                    if ((statusCode == "200") && (statusMsg == "Success") && (statusCode != "400"))
                    {
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
                                                                     // fundtransfer.da = "9840062001";
                                                                     //fundtransfer.da = "9803200158";
                                string da2 = fundtransfer.da;

                                getMerchantName = getMerchantDetails.PassVIdToGetMerchantName(vid);
                                if (getMerchantName.Equals("NT Topup"))
                                {
                                    getMerchantName = "NTC";
                                }

                                if (getMerchantName.Equals("NT ADSL"))
                                {
                                    getMerchantName = "ADSL";
                                }

                                if (getMerchantName.Equals("NT PSTN"))
                                {
                                    getMerchantName = "Landline";
                                }

                            }
                            else
                            {
                                statusCode = "500";
                                replyMessage.Response = "Destination Merchant Doesnot Exists";
                                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                                result = replyMessage.Response;
                                message = replyMessage.Response;
                                failedmessage = message;
                            }

                            //fundtransfer.note =
                            //string notedDestmobile = getMerchantName + ":" + destmobile;
                            string notedDestmobile = "Topup for " + getMerchantName + " - " + destmobile;
                            string description1 = "";
                            if (vid == "2")//NTTOPUP
                            {
                                description1 = "Thaili NT TopUp(" + destmobile + ")";
                            }
                            MNFundTransfer mnftWithMerchant = new MNFundTransfer(fundtransfer.tid, fundtransfer.sc,
                                fundtransfer.mobile, fundtransfer.sa, fundtransfer.amount, fundtransfer.da, notedDestmobile,
                                fundtransfer.pin, fundtransfer.sourcechannel);

                            merchantpin = mnftWithMerchant.pin;

                            //start:insert into transaction master//
                            if (mnft.valid() && mnftWithMerchant.valid())
                            {
                                var transaction = new MNTransactionMaster(mnftWithMerchant, vid, description1);
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
                                        || (result == "Invalid Product Request") || (result == "Please try again") || (result == ""))
                                    {

                                        statusCode = "400";
                                        message = result;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, "parameters missing/invalid");
                                        failedmessage = result;
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
                                        message = em.Error_111/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "114")
                                    {
                                        statusCode = result;
                                        message = em.Error_114/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "115")
                                    {
                                        statusCode = result;
                                        message = em.Error_115/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "116")
                                    {
                                        statusCode = result;
                                        message = em.Error_116/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "119")
                                    {
                                        statusCode = result;
                                        message = em.Error_119/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "121")
                                    {
                                        statusCode = result;
                                        message = em.Error_121/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "163")
                                    {
                                        statusCode = result;
                                        message = em.Error_163/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "180")
                                    {
                                        statusCode = result;
                                        message = em.Error_180/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "181")
                                    {
                                        statusCode = result;
                                        message = em.Error_181/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "182")
                                    {
                                        statusCode = result;
                                        message = em.Error_182/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "183")
                                    {
                                        statusCode = result;
                                        message = em.Error_183/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "184")
                                    {
                                        statusCode = result;
                                        message = em.Error_184/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "185")
                                    {
                                        statusCode = result;
                                        message = em.Error_185/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "186")
                                    {
                                        statusCode = result;
                                        message = em.Error_186/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "187")
                                    {
                                        statusCode = result;
                                        message = em.Error_187/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "188")
                                    {
                                        statusCode = result;
                                        message = em.Error_188/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "189")
                                    {
                                        statusCode = result;
                                        message = em.Error_189/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "190")
                                    {
                                        statusCode = result;
                                        message = em.Error_190/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "800")
                                    {
                                        statusCode = result;
                                        message = em.Error_800/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "902")
                                    {
                                        statusCode = result;
                                        message = em.Error_902/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "904")
                                    {
                                        statusCode = result;
                                        message = em.Error_904/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "906")
                                    {
                                        statusCode = result;
                                        message = em.Error_906/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "907")
                                    {
                                        statusCode = result;
                                        message = em.Error_907/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "909")
                                    {
                                        statusCode = result;
                                        message = em.Error_909/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "911")
                                    {
                                        statusCode = result;
                                        message = em.Error_911/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "913")
                                    {
                                        statusCode = result;
                                        message = em.Error_913/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "90")
                                    {
                                        statusCode = result;
                                        message = em.Error_90/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "91")
                                    {
                                        statusCode = result;
                                        message = em.Error_91/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "92")
                                    {
                                        statusCode = result;
                                        message = em.Error_92/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "94")
                                    {
                                        statusCode = result;
                                        message = em.Error_94/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "95")
                                    {
                                        statusCode = result;
                                        message = em.Error_95/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "98")
                                    {
                                        statusCode = result;
                                        message = em.Error_98/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
                                    if (result == "99")
                                    {
                                        statusCode = result;
                                        message = em.Error_99/* + " " + result*/;
                                        failedmessage = message;
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                    }
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

                        //start sms comment gareko
                        OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            try
                            {
                                var jsonData = JObject.Parse(result);
                                var amtTransfer = jsonData["AmounttransferredBalance"];

                                //Sender
                                string messagereply = "Dear " + CustCheckUtils.GetName(mobile) + "," + "\n";
                                //messagereply += getMerchantName + " was successfully transferred with amount NPR " + validTransactionData.Amount //amtTransfer
                                //                + "." + "\n"; //validTransactionData.CreatedDate
                                messagereply += getMerchantName + " successful NPR "
                                                + validTransactionData.Amount + "." + "\n"; //destmobile
                                messagereply += "Thank you. MNepal";

                                //Receiver
                                 string messagereplyReceiver = "Dear " + CustCheckUtils.GetName(destmobile) + "," + "\n";
                                //string messagereplyReceiver = "Dear Customer," + "\n";
                                messagereplyReceiver += getMerchantName + " was successfully transferred with amount NPR "
                                                + validTransactionData.Amount
                                                + "." + "\n";
                                messagereplyReceiver += "Thank you. MNepal";

                                var client = new WebClient();

                                //var content = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To=" + "977" + mobile + "&Text=" + messagereply + "");

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

                                /*//For destmobile
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
                                }*/


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
                        else if ((response.StatusCode == HttpStatusCode.BadRequest) || (response.StatusCode == HttpStatusCode.InternalServerError) || (statusCode != "200"))
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
                        ////end sms comment gareko

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

                    //end comment gareko



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
                            else
                            {
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


                /*For Status Code*/
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

                string revStatus = string.Empty;

                //FOR NCELL
                if (vid == "10" && statusCode == "200")
                {
                    MerchantTransaction merchantTransaction = new MerchantTransaction(vid, "NCELL", destmobile, Convert.ToInt32(validTransactionData.Amount), validTransactionData.SourceMobile, validTransactionData.TraceId, "P", validTransactionData.CreatedDate, "", "", mobile, validTransactionData.createdTimeDate);

                    PassDataToMerchantTransactionController(merchantTransaction); //insert merchantdetail to db and status to pending
                                                                                  // string topUpResult =TopupMerchantVerfication(merchantTransaction);
                    Task<string> topUpResult = TopupMerchantVerfication(merchantTransaction); // call soapRequest function

                    string topupId = "";
                    string topupStatus = "";
                    string topupResult = "";

                    for (int i = 0; i < 6; i++)
                    {
                        Console.WriteLine("Sleep for 1 second!");

                        topupId = topUpResult.Id.ToString();
                        topupStatus = topUpResult.Status.ToString();
                        topupResult = topUpResult.Result.ToString();

                        Thread.Sleep(1000);
                    }

                    if (topupResult == "000")
                    {
                        SMSSend(mobile, destmobile, Convert.ToInt32(validTransactionData.Amount), getMerchantName);//mobile,destmobile, amount
                    }
                    else if ((topupId != "00") && (topupResult != "000"))
                    {
                        string merchantmobile = fundtransfer.da;

                    result = TopupPaymentReverse(sc, vid, mobile, destmobile, merchantmobile, amount, pin, tid);
                    }

                }

                //FOR NTC
                if (vid == "2" && statusCode == "200")
                {
                    MerchantTransaction merchantTransaction = new MerchantTransaction(vid, "NTC", destmobile, Convert.ToInt32(validTransactionData.Amount), validTransactionData.SourceMobile, validTransactionData.TraceId, "P", validTransactionData.CreatedDate, "", "", mobile, validTransactionData.createdTimeDate);

                    PassDataToMerchantTransactionController(merchantTransaction); //insert merchantdetail to db and status to pending
                                                                                  // string topUpResult =TopupMerchantVerfication(merchantTransaction);
                    Task<string> topUpResult = NTCTopupMerchantVerfication(merchantTransaction); // call soapRequest function
                    
                    //string responseCode = topUpResult.ToString();

                    string topupId = "";
                    string topupStatus = "";
                    string topupResult = "";

                    for (int i = 0; i < 6; i++)
                    {
                        Console.WriteLine("Sleep for 1 second!");

                        topupId = topUpResult.Id.ToString();
                        topupStatus = topUpResult.Status.ToString();
                        topupResult = topUpResult.Result.ToString();

                        Thread.Sleep(1000);
                    }

                    if (topupResult == "000")
                    {
                        SMSSend(mobile, destmobile, Convert.ToInt32(validTransactionData.Amount), getMerchantName);//mobile,destmobile, amount
                    }
                    else if ((topupId != "00") && (topupResult != "000"))
                    {
                        string merchantmobile = fundtransfer.da;

                        TopupPaymentReverse(sc, vid, mobile, destmobile, merchantmobile, amount, pin, tid);
                    }

                }
                //FOR NT LANDLINE
                if (vid == "7" && statusCode == "200")
                {
                    MerchantTransaction merchantTransaction = new MerchantTransaction(vid, "PSTN", destmobile, Convert.ToInt32(validTransactionData.Amount), validTransactionData.SourceMobile, validTransactionData.TraceId, "P", validTransactionData.CreatedDate, "", "", mobile, validTransactionData.createdTimeDate);

                    PassDataToMerchantTransactionController(merchantTransaction); //insert merchantdetail to db and status to pending
                                                                                  // string topUpResult =TopupMerchantVerfication(merchantTransaction);
                    Task<string> topUpResult = NTCTopupMerchantVerfication(merchantTransaction); // call soapRequest function

                    //string responseCode = topUpResult.ToString();

                    string topupId = "";
                    string topupStatus = "";
                    string topupResult = "";

                    for (int i = 0; i < 6; i++)
                    {
                        Console.WriteLine("Sleep for 1 second!");

                        topupId = topUpResult.Id.ToString();
                        topupStatus = topUpResult.Status.ToString();
                        topupResult = topUpResult.Result.ToString();

                        Thread.Sleep(1000);
                    }

                    if (topupResult == "000")
                    {
                        SMSSend(mobile, destmobile, Convert.ToInt32(validTransactionData.Amount), getMerchantName);//mobile,destmobile, amount
                    }
                    else if ((topupId != "00") && (topupResult != "000"))
                    {
                        string merchantmobile = fundtransfer.da;

                        TopupPaymentReverse(sc, vid, mobile, destmobile, merchantmobile, amount, pin, tid);
                    }

                }
                //FOR ADSL UNLIMITED
                if (vid == "1" && statusCode == "200" && selectADSL == "Unlimited")
                {
                    MerchantTransaction merchantTransaction = new MerchantTransaction(vid, "ADSL_UNLIMITED", destmobile, Convert.ToInt32(validTransactionData.Amount), validTransactionData.SourceMobile, validTransactionData.TraceId, "P", validTransactionData.CreatedDate, "", "", mobile, validTransactionData.createdTimeDate);

                    PassDataToMerchantTransactionController(merchantTransaction); //insert merchantdetail to db and status to pending
                                                                                  // string topUpResult =TopupMerchantVerfication(merchantTransaction);
                    Task<string> topUpResult = ADSLUnlimitedTopupMerchantVerfication(merchantTransaction); // call soapRequest function
                    
                    //string responseCode = topUpResult.ToString();

                    string topupId = "";
                    string topupStatus = "";
                    string topupResult = "";

                    for (int i = 0; i < 6; i++)
                    {
                        Console.WriteLine("Sleep for 1 second!");

                        topupId = topUpResult.Id.ToString();
                        topupStatus = topUpResult.Status.ToString();
                        topupResult = topUpResult.Result.ToString();

                        Thread.Sleep(1000);
                    }

                    if (topupResult == "000")
                    {
                        SMSSend(mobile, destmobile, Convert.ToInt32(validTransactionData.Amount), getMerchantName);//mobile,destmobile, amount
                    }
                    else if ((topupId != "00") && (topupResult != "000"))
                    {
                        string merchantmobile = fundtransfer.da;

                        TopupPaymentReverse(sc, vid, mobile, destmobile, merchantmobile, amount, pin, tid);
                    }

                }
                //FOR ADSL VOLUME BASED
                if (vid == "1" && statusCode == "200" && selectADSL == "VolumeBased")
                {
                    MerchantTransaction merchantTransaction = new MerchantTransaction(vid, "ADSL_VOLUMEBASED", destmobile, Convert.ToInt32(validTransactionData.Amount), validTransactionData.SourceMobile, validTransactionData.TraceId, "P", validTransactionData.CreatedDate, "", "", mobile, validTransactionData.createdTimeDate);

                    PassDataToMerchantTransactionController(merchantTransaction); //insert merchantdetail to db and status to pending
                                                                                  // string topUpResult =TopupMerchantVerfication(merchantTransaction);
                    Task<string> topUpResult = ADSLVolBasedTopupMerchantVerfication(merchantTransaction); // call soapRequest function

                    //string responseCode = topUpResult.ToString();

                    string topupId = "";
                    string topupStatus = "";
                    string topupResult = "";

                    for (int i = 0; i < 6; i++)
                    {
                        Console.WriteLine("Sleep for 1 second!");

                        topupId = topUpResult.Id.ToString();
                        topupStatus = topUpResult.Status.ToString();
                        topupResult = topUpResult.Result.ToString();

                        Thread.Sleep(1000);
                    }

                    if (topupResult == "000")
                    {
                        SMSSend(mobile, destmobile, Convert.ToInt32(validTransactionData.Amount), getMerchantName);//mobile,destmobile, amount
                    }
                    else if ((topupId != "00") && (topupResult != "000"))
                    {
                        string merchantmobile = fundtransfer.da;

                        TopupPaymentReverse(sc, vid, mobile, destmobile, merchantmobile, amount, pin, tid);
                    }

                }

            //}

            return result;

        }


        public string PassDataToMerchantTransactionController(MerchantTransaction userInfo)
        {
            string results = "false";
            try
            {
                int result = CustActivityUtils.InsertMerchantTransaction(userInfo);
                if (result > 0)
                {
                    results = "true";
                }
                else
                {
                    results = "false";
                }

            }
            catch (Exception ex)
            {
                results = "false";
            }


            return results;
        }
        public string UpdateMerchantTransactionController(MerchantTransaction userInfo)
        {
            string results = "false";
            try
            {
                int result = CustActivityUtils.UpdateStatusMerchantTransaction(userInfo);
                if (result > 0)
                {
                    results = "true";
                }
                else
                {
                    results = "false";
                }

            }
            catch (Exception ex)
            {
                results = "false";
            }


            return results;
        }
        static void BackgroundTaskWithObject(Object stateInfo)
        {
            FundTransfer data = (FundTransfer)stateInfo;
            Console.WriteLine($"Hi {data.tid} from ThreadPool.");
            Thread.Sleep(1000);
        }

        #region Test Verify Merchant

        protected async Task<string> VerifyMerchant(MerchantTransaction merchantTransaction)
        {
            MerchantReference.CommonInterfaceSoapClient commonInterfaceSoapClient = new MerchantReference.CommonInterfaceSoapClient();
            MerchantReference.SubmitRequestRequest submitRequest = new MerchantReference.SubmitRequestRequest();
            MerchantReference.HeaderInfo HeaderInfo = new MerchantReference.HeaderInfo();
            MerchantReference.RequestData input = new MerchantReference.RequestData();

            MerchantReference.SubmitRequestResponse submitRequestResponse = new MerchantReference.SubmitRequestResponse();

            //Test server purano data
            //HeaderInfo.H1 = "1";
            //HeaderInfo.H2 = "98FE1564-1E60-4C58-9CB1-5E36E0094407";
            //HeaderInfo.H3 = "D546D65E-D8B9-4Ef8-9842-3C6d522cFCC3";

            HeaderInfo.H1 = "5";
            HeaderInfo.H2 = "5C86A0ED-8542-43D3-8E08-64048F8EF104";
            HeaderInfo.H3 = "36484FA3-AD31-4C25-8617-56BC3205B3A5";

            submitRequest.serviceId = "001";
            submitRequest.serviceCode = "NCELL";

            input.Field1 = "9813999353";
            input.Field2 = "721335032887";
            input.Field3 = "20190801160216";
            input.Field4 = "50";
            input.Field5 = "8800000238";

            submitRequestResponse = await commonInterfaceSoapClient.SubmitRequestAsync(HeaderInfo, submitRequest.serviceId, submitRequest.serviceCode, input);
            string responseCode = submitRequestResponse.SubmitRequestResult.ResultCode;
            string resultDescription = submitRequestResponse.SubmitRequestResult.ResultDescription;
            if (responseCode != "")
            {
                if (responseCode == "200") { merchantTransaction.Status = "T"; } else { merchantTransaction.Status = "F"; }
                merchantTransaction.ResponseCode = responseCode;
                merchantTransaction.ResponseDescription = resultDescription;
                UpdateMerchantTransactionController(merchantTransaction);
            }
            return responseCode;
        }
        #endregion

        #region Verify Merchant

        public async Task<string> TopupMerchantVerfication(MerchantTransaction mNTransaction)
        {
            MerchantReference.CommonInterfaceSoapClient commonInterfaceSoapClient = new MerchantReference.CommonInterfaceSoapClient();
            MerchantReference.SubmitRequestRequest submitRequest = new MerchantReference.SubmitRequestRequest();
            MerchantReference.HeaderInfo HeaderInfo = new MerchantReference.HeaderInfo();
            MerchantReference.RequestData input = new MerchantReference.RequestData();

            MerchantReference.SubmitRequestResponse submitRequestResponse = new MerchantReference.SubmitRequestResponse();
            //Testing Header NCELL
            //HeaderInfo.H1 = "1";
            //HeaderInfo.H2 = "98FE1564-1E60-4C58-9CB1-5E36E0094407";
            //HeaderInfo.H3 = "D546D65E-D8B9-4Ef8-9842-3C6d522cFCC3";

            ////Live Header NCELL
            HeaderInfo.H1 = "5";
            HeaderInfo.H2 = "5C86A0ED-8542-43D3-8E08-64048F8EF104";
            HeaderInfo.H3 = "36484FA3-AD31-4C25-8617-56BC3205B3A5";

            //HeaderInfo.H1 = System.Web.Configuration.WebConfigurationManager.AppSettings["H1"];
            //HeaderInfo.H2 = System.Web.Configuration.WebConfigurationManager.AppSettings["H2"];
            //HeaderInfo.H3 = System.Web.Configuration.WebConfigurationManager.AppSettings["H3"];

            submitRequest.serviceId = "001"; //Service ID
            submitRequest.serviceCode = "NCELL"; // Service Name
            input.Field1 = mNTransaction.PAN; //?Mobile Number
            input.Field2 = mNTransaction.STAN; //?Retrival Reference

            //input.Field3 = mNTransaction.CreatedDate.ToString("dd/MM/yyyy HH:mm"); //Created Date
            input.Field3 = mNTransaction.CreatedTimeDate; //Created Date
            input.Field4 = mNTransaction.Amount;//Amount
            input.Field5 = mNTransaction.MobileNumber; // Mobile Number (Testing No: 8800000238)

            submitRequestResponse = await commonInterfaceSoapClient.SubmitRequestAsync(HeaderInfo, submitRequest.serviceId, submitRequest.serviceCode, input);
            string responseCode = submitRequestResponse.SubmitRequestResult.ResultCode;

            string resultDescription = submitRequestResponse.SubmitRequestResult.ResultDescription;

            if (responseCode != "")
            {
                if (responseCode == "000") { mNTransaction.Status = "T"; } else { mNTransaction.Status = "F"; }
                mNTransaction.ResponseCode = responseCode;
                mNTransaction.ResponseDescription = resultDescription;
                UpdateMerchantTransactionController(mNTransaction);  // Update and insert responsecode and description to db 
            }
            return responseCode;
        }
        #endregion

        #region Verify Merchant NTC
        public async Task<string> NTCTopupMerchantVerfication(MerchantTransaction mNTransaction)
        {
            MerchantReference.CommonInterfaceSoapClient commonInterfaceSoapClient = new MerchantReference.CommonInterfaceSoapClient();
            MerchantReference.SubmitRequestRequest submitRequest = new MerchantReference.SubmitRequestRequest();
            MerchantReference.HeaderInfo HeaderInfo = new MerchantReference.HeaderInfo();
            MerchantReference.RequestData input = new MerchantReference.RequestData();

            MerchantReference.SubmitRequestResponse submitRequestResponse = new MerchantReference.SubmitRequestResponse();

            string mobNumber = mNTransaction.MobileNumber;
            string serviceCode = "POSTPAID";

            //NT PREPAID Check
            if (mobNumber.Substring(0, 3) == "984" || mobNumber.Substring(0, 3) == "986")
            {
                serviceCode = "PREPAID";
            }

            //NT Landline Check
            if (mNTransaction.MerchantID == "7")
            {
                serviceCode = "PSTN";
                //mNTransaction.MobileNumber = mNTransaction.MobileNumber.Substring(1);
            }

            //test server ko header
            //HeaderInfo.H1 = "1";
            //HeaderInfo.H2 = "98FE1564-1E60-4C58-9CB1-5E36E0094407";
            //HeaderInfo.H3 = "D546D65E-D8B9-4Ef8-9842-3C6d522cFCC3";

            HeaderInfo.H1 = "5";
            HeaderInfo.H2 = "5C86A0ED-8542-43D3-8E08-64048F8EF104";
            HeaderInfo.H3 = "36484FA3-AD31-4C25-8617-56BC3205B3A5";

            //HeaderInfo.H1 = System.Web.Configuration.WebConfigurationManager.AppSettings["H1"];
            //HeaderInfo.H2 = System.Web.Configuration.WebConfigurationManager.AppSettings["H2"];
            //HeaderInfo.H3 = System.Web.Configuration.WebConfigurationManager.AppSettings["H3"];

            submitRequest.serviceId = "002"; //Service ID
            submitRequest.serviceCode = serviceCode; // Service Name
            input.Field1 = mNTransaction.PAN; //?Mobile Number
            input.Field2 = mNTransaction.STAN; //?Retrival Reference

            //input.Field3 = mNTransaction.CreatedDate.ToString("dd/MM/yyyy HH:mm"); //Created Date
            input.Field3 = mNTransaction.CreatedTimeDate; //Created Date
            input.Field4 = mNTransaction.Amount;//Amount
            input.Field5 = mNTransaction.MobileNumber; // Mobile Number (Testing No: 8800000238)

            submitRequestResponse = await commonInterfaceSoapClient.SubmitRequestAsync(HeaderInfo, submitRequest.serviceId, submitRequest.serviceCode, input);
            string responseCode = submitRequestResponse.SubmitRequestResult.ResultCode;
            string resultDescription = submitRequestResponse.SubmitRequestResult.ResultDescription;
            if (responseCode != "")
            {
                if (responseCode == "000") { mNTransaction.Status = "T"; } else { mNTransaction.Status = "F"; }
                mNTransaction.ResponseCode = responseCode;
                mNTransaction.ResponseDescription = resultDescription;
                UpdateMerchantTransactionController(mNTransaction);  // Update and insert responsecode and description to db 
            }
            return responseCode;
        }
        #endregion

        #region Verify Merchant ADSL Unlimited
        public async Task<string> ADSLUnlimitedTopupMerchantVerfication(MerchantTransaction mNTransaction)
        {
            MerchantReference.CommonInterfaceSoapClient commonInterfaceSoapClient = new MerchantReference.CommonInterfaceSoapClient();
            MerchantReference.SubmitRequestRequest submitRequest = new MerchantReference.SubmitRequestRequest();
            MerchantReference.HeaderInfo HeaderInfo = new MerchantReference.HeaderInfo();
            MerchantReference.RequestData input = new MerchantReference.RequestData();

            MerchantReference.SubmitRequestResponse submitRequestResponse = new MerchantReference.SubmitRequestResponse();

            string mobNumber = mNTransaction.MobileNumber;
            string serviceCode = "ADSLU";

            //test server ko header
            //HeaderInfo.H1 = "1";
            //HeaderInfo.H2 = "98FE1564-1E60-4C58-9CB1-5E36E0094407";
            //HeaderInfo.H3 = "D546D65E-D8B9-4Ef8-9842-3C6d522cFCC3";

            HeaderInfo.H1 = "5";
            HeaderInfo.H2 = "5C86A0ED-8542-43D3-8E08-64048F8EF104";
            HeaderInfo.H3 = "36484FA3-AD31-4C25-8617-56BC3205B3A5";

            //HeaderInfo.H1 = System.Web.Configuration.WebConfigurationManager.AppSettings["H1"];
            //HeaderInfo.H2 = System.Web.Configuration.WebConfigurationManager.AppSettings["H2"];
            //HeaderInfo.H3 = System.Web.Configuration.WebConfigurationManager.AppSettings["H3"];

            submitRequest.serviceId = "002"; //Service ID
            submitRequest.serviceCode = serviceCode; // Service Name
            input.Field1 = mNTransaction.PAN; //?Mobile Number
            input.Field2 = mNTransaction.STAN; //?Retrival Reference

            //input.Field3 = mNTransaction.CreatedDate.ToString("dd/MM/yyyy HH:mm"); //Created Date
            input.Field3 = mNTransaction.CreatedTimeDate; //Created Date
            input.Field4 = mNTransaction.Amount;//Amount
            input.Field5 = mNTransaction.MobileNumber; // Mobile Number (Testing No: 8800000238)

            submitRequestResponse = await commonInterfaceSoapClient.SubmitRequestAsync(HeaderInfo, submitRequest.serviceId, submitRequest.serviceCode, input);
            string responseCode = submitRequestResponse.SubmitRequestResult.ResultCode;
            string resultDescription = submitRequestResponse.SubmitRequestResult.ResultDescription;
            if (responseCode != "")
            {
                if (responseCode == "000") { mNTransaction.Status = "T"; } else { mNTransaction.Status = "F"; }
                mNTransaction.ResponseCode = responseCode;
                mNTransaction.ResponseDescription = resultDescription;
                UpdateMerchantTransactionController(mNTransaction);  // Update and insert responsecode and description to db 
            }
            return responseCode;
        }
        #endregion

        #region Verify Merchant ADSL Volume Based
        public async Task<string> ADSLVolBasedTopupMerchantVerfication(MerchantTransaction mNTransaction)
        {
            MerchantReference.CommonInterfaceSoapClient commonInterfaceSoapClient = new MerchantReference.CommonInterfaceSoapClient();
            MerchantReference.SubmitRequestRequest submitRequest = new MerchantReference.SubmitRequestRequest();
            MerchantReference.HeaderInfo HeaderInfo = new MerchantReference.HeaderInfo();
            MerchantReference.RequestData input = new MerchantReference.RequestData();

            MerchantReference.SubmitRequestResponse submitRequestResponse = new MerchantReference.SubmitRequestResponse();

            string mobNumber = mNTransaction.MobileNumber;
            string serviceCode = "ADSLV";

            //test server ko header
            //HeaderInfo.H1 = "1";
            //HeaderInfo.H2 = "98FE1564-1E60-4C58-9CB1-5E36E0094407";
            //HeaderInfo.H3 = "D546D65E-D8B9-4Ef8-9842-3C6d522cFCC3";

            HeaderInfo.H1 = "5";
            HeaderInfo.H2 = "5C86A0ED-8542-43D3-8E08-64048F8EF104";
            HeaderInfo.H3 = "36484FA3-AD31-4C25-8617-56BC3205B3A5";

            //HeaderInfo.H1 = System.Web.Configuration.WebConfigurationManager.AppSettings["H1"];
            //HeaderInfo.H2 = System.Web.Configuration.WebConfigurationManager.AppSettings["H2"];
            //HeaderInfo.H3 = System.Web.Configuration.WebConfigurationManager.AppSettings["H3"];

            submitRequest.serviceId = "002"; //Service ID
            submitRequest.serviceCode = serviceCode; // Service Name
            input.Field1 = mNTransaction.PAN; //?Mobile Number
            input.Field2 = mNTransaction.STAN; //?Retrival Reference

            //input.Field3 = mNTransaction.CreatedDate.ToString("dd/MM/yyyy HH:mm"); //Created Date
            input.Field3 = mNTransaction.CreatedTimeDate; //Created Date
            input.Field4 = mNTransaction.Amount;//Amount
            input.Field5 = mNTransaction.MobileNumber; // Mobile Number (Testing No: 8800000238)

            submitRequestResponse = await commonInterfaceSoapClient.SubmitRequestAsync(HeaderInfo, submitRequest.serviceId, submitRequest.serviceCode, input);
            string responseCode = submitRequestResponse.SubmitRequestResult.ResultCode;
            string resultDescription = submitRequestResponse.SubmitRequestResult.ResultDescription;
            if (responseCode != "")
            {
                if (responseCode == "000") { mNTransaction.Status = "T"; } else { mNTransaction.Status = "F"; }
                mNTransaction.ResponseCode = responseCode;
                mNTransaction.ResponseDescription = resultDescription;
                UpdateMerchantTransactionController(mNTransaction);  // Update and insert responsecode and description to db 
            }
            return responseCode;
        }
        #endregion
        public bool checkLandline(string destNumber)
        {
            if (destNumber.Length == 9 && (!destNumber.StartsWith("00")))
            {
                return true;
            }
            return false;
        }


        public string TopupPaymentReverse(string sc, string vid, string mobile, string destmobile, string merchantmobile, string amount, string pin, string retRef)
        {
            //string sc = qs["sc"];sc, vid, mobile, destmobile, merchantmobile, amount, pin, merchantpin
            //string vid = qs["vid"];
            //string mobile = qs["mobile"];  // user's mobile number
            //string destmobile = qs["destmobile"];  // destination's mobile number or topup no
            //string amount = qs["amount"];
            
            string tid = "";// qs["tid"];
            string note = "RetRef: " + retRef + " Rev.:" + destmobile + "-" + mobile;// qs["note"];
            string src = "http";// qs["src"];
            string sessionID = "";//qs["tokenID"];
            string selectADSL = "";//qs["selectADSL"]; //Unlimited //VolumeBased

            //SMS
            string SMSNTC = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalNTCSMSServerUrl"];
            string SMSNCELL = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalSMSServerUrl"];

            string result = "";
            string getMerchantName = "";

            ReplyMessage replyMessage = new ReplyMessage();

            CustActivityModel custsmsInfo = new CustActivityModel();
            //Utility Payment / TOPUP REVERSE
            if (sc == "31") //" Topup Wallet Check ";
            {
                sc = "10"; 
            }
            else if (sc == "34") //" Topup Bank Check ";
            {
                sc = "11";
            }

            FundTransfer fundtransferRev = new FundTransfer
            {
                tid = tid,
                sc = sc,
                mobile = merchantmobile, //mobile, //
                da = mobile,//da
                amount = amount,
                pin = pin,
                vid = vid,
                note = note,
                sourcechannel = src
            };

            ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, fundtransferRev);

            MNTransactionMaster validTransactionData = new MNTransactionMaster();
            
            string statusCode = "";
            string message = string.Empty;
            string failedmessage = string.Empty;

            //if (TokenGenerator.TokenChecker(sessionID, mobile, src) == false)
            //{
            //    // throw ex
            //    statusCode = "400";
            //    message = "Session expired. Please login again";
            //    failedmessage = message;
            //}
            //else
            //{
                if ((sc == null) || (mobile == null) || (vid == null) || (amount == null) || (pin == null) ||
                (src == null) || (double.Parse(amount) <= 0))
                {
                    // throw ex
                    statusCode = "400";
                    message = "Parameters Missing/Invalid";
                }
                else if ((vid == "1" || vid == "7") && !checkLandline(destmobile))
                {
                    statusCode = "400";
                    message = "Please enter valid Number";
                }
                else
                {
                //TransLimitCheck transLimitCheck = new TransLimitCheck();
                //string resultTranLimit = transLimitCheck.LimitCheck(mobile, destmobile, amount, sc, pin, src);

                //var jsonDataResult = JObject.Parse(resultTranLimit);
                //statusCode = jsonDataResult["StatusCode"].ToString();
                //string statusMsg = jsonDataResult["StatusMessage"].ToString();
                //message = jsonDataResult["StatusMessage"].ToString();
                //failedmessage = message;

                TranIDCheck tranIDCheck = new TranIDCheck();
                string resultTranID = tranIDCheck.GetTranIDCheck(retRef);

                    string TopUpRev = "T";
                    //start top reverse

                    if (TopUpRev == "T")
                    {
                        
                        //if ((statusCode == "200") && (statusMsg == "Success") && (statusCode != "400"))
                        //{
                            do
                            {
                                TraceIdGenerator traceid = new TraceIdGenerator();
                                tid = traceid.GenerateUniqueTraceID();
                                fundtransferRev.tid = tid;

                                bool traceIdCheck = false;
                                traceIdCheck = TraceIdCheck.IsValidTraceId(fundtransferRev.tid);
                                if (traceIdCheck == true)
                                {
                                    result = "Trace ID Repeated";
                                }
                                else
                                {
                                    result = "false";
                                }

                            } while (result == "Trace ID Repeated");

                            //// START MERCHANT DETAIL & REMARK ////
                            ///<summary>
                            /// GET MERCHANT DETAIL
                            /// </summary>
                            MNMerchantsController getMerchantDetails = new MNMerchantsController();

                            string getMerchantMobile = getMerchantDetails.PassVidToGetMerchantDetail(vid);
                            if ((getMerchantMobile != "") || (getMerchantMobile != null))
                            {
                                fundtransferRev.da = getMerchantMobile; //Set Destination Merchant Mobile number

                                getMerchantName = getMerchantDetails.PassVIdToGetMerchantName(vid);
                                if (getMerchantName.Equals("NT Topup"))
                                {
                                    getMerchantName = "NTC";
                                }

                                if (getMerchantName.Equals("NT ADSL"))
                                {
                                    getMerchantName = "ADSL";
                                }

                                if (getMerchantName.Equals("NT PSTN"))
                                {
                                    getMerchantName = "Landline";
                                }

                            }
                            else
                            {
                                statusCode = "500";
                                replyMessage.Response = "Destination Merchant Doesnot Exists";
                                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                                result = replyMessage.Response;
                                message = replyMessage.Response;
                                failedmessage = message;
                            }

                    //Reversal for service name to service no. for transaction ID:xxxxxxxxx & ref no.:xxxxxxxxxxx
                    string remarkMerchant = string.Empty;
                    if (sc == "11")
                    {
                        resultTranID = retRef.Substring(retRef.Length - 6, 6);
                        remarkMerchant = "Reversal for " + getMerchantName + " to " + mobile + " for Trace No: " + resultTranID;
                    }
                    if (sc == "10")
                    {
                        remarkMerchant = "Reversal for " + getMerchantName + " to " + mobile + " for Tran ID: " + resultTranID;
                    }
                    

                            //// END MERCHANT DETAIL & REMARK ////

                            ////start:Com focus one log////

                            MNFundTransfer mnft = new MNFundTransfer(fundtransferRev.tid, fundtransferRev.sc, fundtransferRev.mobile,
                                fundtransferRev.sa, fundtransferRev.amount, mobile, fundtransferRev.note, fundtransferRev.pin,
                                fundtransferRev.sourcechannel, "T", remarkMerchant);
                            var comfocuslog = new MNComAndFocusOneLog(mnft, DateTime.Now);
                            var mncomfocuslog = new MNComAndFocusOneLogsController();
                            result = mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                            ////end:Com focus one log//

                            if (result == "Success")
                            {
                                //NOTE:- may be need to validate before insert into reply typpe
                                //start:insert into reply type as HTTP//
                                var replyType = new MNReplyType(fundtransferRev.tid, "HTTP");
                                var mnreplyType = new MNReplyTypesController();
                                mnreplyType.InsertIntoReplyType(replyType);
                                //end:insert into reply type as HTTP//

                                
                                string notedDestmobile = "Topup for " + getMerchantName + " - " + destmobile;
                                string description1 = "";
                                if (vid == "2")//NTTOPUP
                                {
                                    description1 = "Thaili NT TopUp(" + destmobile + ")";
                                }


                        MNFundTransfer mnftWithMerchant = new MNFundTransfer(fundtransferRev.tid, fundtransferRev.sc,
                                    fundtransferRev.mobile, fundtransferRev.sa, fundtransferRev.amount, fundtransferRev.da, notedDestmobile,
                                    fundtransferRev.pin, fundtransferRev.sourcechannel, "T", "Rev. Topup"); //Revese & MerchantType
                        

                                //start:insert into transaction master//
                                if (mnft.valid() && mnftWithMerchant.valid())
                                {
                                    var transaction = new MNTransactionMaster(mnft);
                                    ////var transaction = new MNTransactionMaster(mnftWithMerchant, vid, description1, "T");
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
                                            || (result == "Invalid Product Request") || (result == "Please try again") || (result == ""))
                                        {
                                            statusCode = "400";
                                            message = result;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, "parameters missing/invalid");
                                            failedmessage = result;
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
                                            message = em.Error_111/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "114")
                                        {
                                            statusCode = result;
                                            message = em.Error_114/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "115")
                                        {
                                            statusCode = result;
                                            message = em.Error_115/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "116")
                                        {
                                            statusCode = result;
                                            message = em.Error_116/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "119")
                                        {
                                            statusCode = result;
                                            message = em.Error_119/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "121")
                                        {
                                            statusCode = result;
                                            message = em.Error_121/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "163")
                                        {
                                            statusCode = result;
                                            message = em.Error_163/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "180")
                                        {
                                            statusCode = result;
                                            message = em.Error_180/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "181")
                                        {
                                            statusCode = result;
                                            message = em.Error_181/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "182")
                                        {
                                            statusCode = result;
                                            message = em.Error_182/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "183")
                                        {
                                            statusCode = result;
                                            message = em.Error_183/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "184")
                                        {
                                            statusCode = result;
                                            message = em.Error_184/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "185")
                                        {
                                            statusCode = result;
                                            message = em.Error_185/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "186")
                                        {
                                            statusCode = result;
                                            message = em.Error_186/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "187")
                                        {
                                            statusCode = result;
                                            message = em.Error_187/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "188")
                                        {
                                            statusCode = result;
                                            message = em.Error_188/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "189")
                                        {
                                            statusCode = result;
                                            message = em.Error_189/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "190")
                                        {
                                            statusCode = result;
                                            message = em.Error_190/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "800")
                                        {
                                            statusCode = result;
                                            message = em.Error_800/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "902")
                                        {
                                            statusCode = result;
                                            message = em.Error_902/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "904")
                                        {
                                            statusCode = result;
                                            message = em.Error_904/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "906")
                                        {
                                            statusCode = result;
                                            message = em.Error_906/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "907")
                                        {
                                            statusCode = result;
                                            message = em.Error_907/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "909")
                                        {
                                            statusCode = result;
                                            message = em.Error_909/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "911")
                                        {
                                            statusCode = result;
                                            message = em.Error_911/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "913")
                                        {
                                            statusCode = result;
                                            message = em.Error_913/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "90")
                                        {
                                            statusCode = result;
                                            message = em.Error_90/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "91")
                                        {
                                            statusCode = result;
                                            message = em.Error_91/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "92")
                                        {
                                            statusCode = result;
                                            message = em.Error_92/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "94")
                                        {
                                            statusCode = result;
                                            message = em.Error_94/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "95")
                                        {
                                            statusCode = result;
                                            message = em.Error_95/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "98")
                                        {
                                            statusCode = result;
                                            message = em.Error_98/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        if (result == "99")
                                        {
                                            statusCode = result;
                                            message = em.Error_99/* + " " + result*/;
                                            failedmessage = message;
                                            mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        }
                                        else if (validTransactionData.ResponseCode == "OK")
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

                            //TopupReverse Msg
                            OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;

                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                try
                                {
                                    var jsonData = JObject.Parse(result);
                                    var amtTransfer = jsonData["AmounttransferredBalance"];

                                    //Sender
                                    string messagereply = "Dear " + CustCheckUtils.GetName(mobile) + "," + "\n";
                                    messagereply += getMerchantName + " successfully reverse NPR "
                                                    + validTransactionData.Amount + "." + "\n"; //destmobile
                                    messagereply += "Thank you. MNepal";

                                    //Receiver
                                    //string messagereplyReceiver = "Dear Customer," + "\n";
                                    string messagereplyReceiver = "Dear " + CustCheckUtils.GetName(destmobile) + "," + "\n";
                            messagereplyReceiver += getMerchantName + " was successfully reeversed with amount NPR "
                                                    + validTransactionData.Amount
                                                    + "." + "\n";
                                    messagereplyReceiver += "Thank you. MNepal";

                                    var client = new WebClient();

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

                                    ////For destmobile
                                    //string mobileReceiver = destmobile;
                                    //if ((mobileReceiver.Substring(0, 3) == "980") || (mobileReceiver.Substring(0, 3) == "981")) //FOR NCELL
                                    //{
                                    //    //FOR NCELL
                                    //    var content = client.DownloadString(
                                    //        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                    //        + "977" + mobileReceiver + "&Text=" + messagereplyReceiver + "");
                                    //}
                                    //else if ((mobileReceiver.Substring(0, 3) == "985") || (mobileReceiver.Substring(0, 3) == "984")
                                    //            || (mobileReceiver.Substring(0, 3) == "986"))
                                    //{
                                    //    //FOR NTC
                                    //    var content = client.DownloadString(
                                    //        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                    //        + "977" + mobileReceiver + "&Text=" + messagereplyReceiver + "");
                                    //}


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
                            else if ((response.StatusCode == HttpStatusCode.BadRequest) || (response.StatusCode == HttpStatusCode.InternalServerError) || (statusCode != "200"))
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

                    }
                    //end topup reverse



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
                            else
                            {
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
            //}


            /*For Status Code*/
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


        public string SMSSend(string mobile, string destmobile, int amount, string getMerchantName)
        {
            CustActivityModel custsmsInfo = new CustActivityModel();
            try
            {
                //Sender
                string messagereply = "Dear " + CustCheckUtils.GetName(mobile) + "," + "\n";
                messagereply += getMerchantName + " successful NPR "
                                + amount
                                + "." + "\n";
                messagereply += "Thank you. MNepal";

                //Receiver
                string messagereplyReceiver = "Dear Customer," + "\n";
                messagereplyReceiver += getMerchantName + " was successfully transferred with amount NPR "
                                + amount
                                + "." + "\n";
                messagereplyReceiver += "Thank you. MNepal";

                var client = new WebClient();

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
                    Amount = amount.ToString(),
                    SMSStatus = "Success",
                    SMSSenderReply = messagereply,
                    ErrorMessage = "",
                };

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return "true";

        }


    }
}
