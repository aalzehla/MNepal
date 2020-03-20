using MNepalProject.Controllers;
using MNepalProject.Models;
using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Net;
using System.Web;
using System.Collections.Specialized;
using MNepalWCF.Utilities;
using MNepalProject.Helper;
using MNepalWCF.ErrorMsg;
using MNepalWCF.Models;

namespace MNepalWCF
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class coupon
    {
        [OperationContract]
        [WebInvoke(Method = "POST",
                  ResponseFormat = WebMessageFormat.Json
                  )]


        public string telco(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();

            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            string sc = qs["sc"];
            string vid = qs["vid"];
            string sa = qs["sa"];
            string mobile = qs["mobile"];
            string amount = qs["amount"];
            string qty = qs["qty"];
            string tid = qs["tid"];
            string pin = qs["pin"];
            string note = qs["note"];
            string src = qs["src"];

            string result = "";
            string GetMerchantName = "";
            string statusCode = string.Empty;
            string message = string.Empty;
            qty = "1";
            var couponNumber = "";
            string failedmessage = string.Empty;

            ReplyMessage replyMessage = new ReplyMessage();

            CustActivityModel custsmsInfo = new CustActivityModel();

            TraceIdGenerator tig = new TraceIdGenerator();
            tid = tig.GenerateUniqueTraceID(); //GenerateTraceID();
            

            CouponPayment c = new CouponPayment();
            c.sc = sc;
            c.vid = vid;
            c.sa = sa;
            c.mobile = mobile;
            c.amount = amount;
            c.qty = qty;
            c.tid = tid;
            c.pin = pin;
            c.note = "Quantity " + c.qty +"," + note ;
            c.sourcechannel = src;

            string da = vid;//"vid:" + vid; + "," + "qty:" + qty;
            c.da = da;


            //Recharge Payment
            if (sc == "32" || sc == "35")
            {
                c.da = vid;
                c.note = note + ":" + mobile;//"recharge " + mobile + " " + amount
            }

            ////start:Com focus one log///
            MNFundTransfer mnft = new MNFundTransfer(c.tid, c.sc, c.mobile, c.sa, c.amount, c.da, c.note, c.pin, c.sourcechannel);

            if ((tid == "null") || (sc == "null") || (mobile == "null") || (vid == "null") || (amount == "null") || (pin == "null") ||
                (src == "null"))
            {
                // throw ex
                statusCode = "400";
                message = "Parameters Missing/Invalid";
                mnft.ResponseStatus(HttpStatusCode.BadRequest, message);
                failedmessage = message;
            }
            else
            {

                TransLimitCheck transLimitCheck = new TransLimitCheck();
                string resultTranLimit = transLimitCheck.LimitCheck(mobile, "", amount, sc, pin, src);

                var jsonDataResult = JObject.Parse(resultTranLimit);
                statusCode = jsonDataResult["StatusCode"].ToString();
                string statusMsg = jsonDataResult["StatusMessage"].ToString();
                message = jsonDataResult["StatusMessage"].ToString();
                failedmessage = message;

                if (sc == "32")
                {
                    GetMerchantName = "Recharge Wallet Check";
                }
                if (sc == "35")
                {
                    GetMerchantName = "Recharge Bank Check";
                }

                if ((statusCode == "200") && (message == "Success"))
                {

                    ////start:Com focus one log///
                    //MNFundTransfer mnft = new MNFundTransfer(c.tid, c.sc, c.mobile, c.sa, c.amount, c.da, c.note, c.pin, c.sourcechannel);
                    var comfocuslog = new MNComAndFocusOneLog(mnft, DateTime.Now);
                    var mncomfocuslog = new MNComAndFocusOneLogsController();
                    result = mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                    ////end:Com focus one log//


                    if (result == "Success")
                    {
                        //NOTE:- may be need to validate before insert into reply typpe
                        //start:insert into reply type as HTTP//
                        var replyType = new MNReplyType(c.tid, "HTTP");
                        var mnreplyType = new MNReplyTypesController();
                        mnreplyType.InsertIntoReplyType(replyType);
                        //end:insert into reply type as HTTP//

                        MNMerchantsController getMerchantDetails = new MNMerchantsController();

                        string GetMerchantMobile = getMerchantDetails.PassVidToGetMerchantDetail(vid);

                        if (GetMerchantMobile == "" || GetMerchantMobile == null)
                        {
                            statusCode = "500";
                            mnft.Response = "Destination Merchant Doesnot Exists";
                            replyMessage.Response = "Destination Merchant Doesnot Exists";
                            mnft.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                            result = mnft.Response;
                            message = mnft.Response;
                            failedmessage = message;
                        }
                        else
                        {
                            GetMerchantName = getMerchantDetails.PassVIdToGetMerchantName(vid);
                            c.da = GetMerchantMobile; //+ "," + GetMerchantName + "," + qty; //Set Destination Merchant Mobile number 

                            string cnote = GetMerchantName + ":" + c.note;
                            MNFundTransfer mnftForCoupon = new MNFundTransfer(c.tid, c.sc, c.mobile, c.sa, c.amount, c.da, cnote, c.pin, c.sourcechannel);//c.note

                            MNTransactionMaster validTransactionData = new MNTransactionMaster();

                            //start:insert into transaction master//
                            if (mnft.valid() && mnftForCoupon.valid())
                            {
                                var transaction = new MNTransactionMaster(mnftForCoupon);
                                var mntransaction = new MNTransactionsController();
                                validTransactionData = mntransaction.Validate(transaction, mnftForCoupon.pin);
                                result = validTransactionData.Response;

                                /*** ***/
                                ErrorMessage em = new ErrorMessage();
                                if (validTransactionData.Response == "Error")
                                {
                                    mnft.Response = "error";
                                    mnft.ResponseStatus(HttpStatusCode.BadRequest, "Internal server error - try again later, or contact support");
                                    result = mnft.Response;
                                    statusCode = "500";
                                    message = "Internal server error - try again later, or contact support";
                                    failedmessage = message;
                                }
                                else
                                {
                                    if ((result == "Trace ID Repeated") || (result == "Limit Exceed")
                                        || (result == "Invalid Source User") || (result == "Invalid Destination User") || (result == "Destination Merchant Doesnot Exists")
                                        || (result == "Invalid Product Request") || (result == "") )
                                    {
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                        statusCode = "400";
                                        failedmessage = result;
                                        message = result;
                                    }
                                    if (result == "Invalid PIN")
                                    {
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                                        statusCode = "400";
                                        failedmessage = result + pin;
                                        message = result + pin;
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
                                    else if (statusCode == "200")
                                    {
                                        mnft.ResponseStatus(HttpStatusCode.OK, result);
                                        statusCode = "200";
                                        var v = new
                                        {

                                            StatusCode = Convert.ToInt32(statusCode),
                                            StatusMessage = result
                                        };
                                        result = JsonConvert.SerializeObject(v);
                                    }

                                }
                                /*** ***/

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
                    }
                    else
                    {
                        mnft.Response = "Data Insertion Failed in Check DB Connection: TraceID limit might have exceeded";
                        mnft.ResponseStatus(HttpStatusCode.InternalServerError, replyMessage.Response);
                        result = mnft.Response;
                        failedmessage = result;
                    }

                }
                else
                {
                    mnft.Response = failedmessage;
                    mnft.ResponseStatus(HttpStatusCode.BadRequest, failedmessage);
                    result = mnft.Response;
                    statusCode = "400";
                    message = mnft.Response;
                    failedmessage = message;
                }

            }

            OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    var jsonData = JObject.Parse(result);
                    string statusMessage = jsonData["StatusMessage"].ToString();
                    var jsonDataCoupon = JObject.Parse(statusMessage);
                    //var 
                        couponNumber = jsonDataCoupon["couponnumber"].ToString();

                    string messagereply = "Dear Customer," + "\n";
                    //messagereply += GetMerchantName + " purchased successfully.";
                    //messagereply += " Recharge number is: " + couponNumber + "\n";
                    messagereply += "Please, recharge your " + GetMerchantName + " account with provided PIN ";
                    messagereply += couponNumber + "\n";
                    messagereply += "Thank you. MNepal";

                    custsmsInfo = new CustActivityModel()
                    {
                        UserName = mobile,
                        RequestMerchant = GetMerchantName,
                        DestinationNo = "PIN: " + couponNumber,
                        Amount = amount,
                        SMSStatus = "Success",
                        SMSSenderReply = messagereply,
                        ErrorMessage = "",
                    };

                    var client = new WebClient();
                    
                    //var content = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To=" + "977" + mobile + "&Text=" + messagereply + "");

                    if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                    {
                        //FOR NCELL
                        var content = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                        + "977" + mobile + "&Text=" + messagereply + "");
                    }
                    else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                        || (mobile.Substring(0, 3) == "986"))
                    {
                        //FOR NTC
                        var content = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                            + "977" + mobile + "&Text=" + messagereply + "");
                    }

                    


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
                    RequestMerchant = GetMerchantName,
                    DestinationNo = "",
                    Amount = amount,
                    SMSStatus = "Failed",
                    SMSSenderReply = "",
                    ErrorMessage = failedmessage,
                };

                //var v = new
                //{
                //    StatusCode = Convert.ToInt32(400),
                //    StatusMessage = message
                //};
                //result = JsonConvert.SerializeObject(v);
            }

            //Register For SMS
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
                    StatusMessage = message
                };
                result = JsonConvert.SerializeObject(v);
            }

            return result;

           
        }
       
    }
}
