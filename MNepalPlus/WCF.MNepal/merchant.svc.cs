﻿using System;
using System.IO;
using Newtonsoft.Json;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Web;
using MNepalProject.Controllers;
using MNepalProject.Models;
using WCF.MNepal.Utilities;
using MNepalProject.Helper;
using WCF.MNepal.Helper;
using WCF.MNepal.Models;
using WCF.MNepal.ErrorMsg;
using System.Threading;

namespace WCF.MNepal
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class merchant
    {
        [OperationContract]
        [WebInvoke(Method = "POST",
                  ResponseFormat = WebMessageFormat.Json
                  )]

        public string payment(Stream input)
        {

            //string tid, string sc, string vid, string mobile,string sa, string prod, string amount,string pin, string note, string src
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            string sc = qs["sc"]; //TransactionMedium
            string vid = qs["vid"]; //MerchantID
            string mobile = qs["mobile"];  //user's mobile number
            string sa = qs["sa"];
            string prod = qs["prod"];      // ProductID like Student RollNo, NEA Bill
            string amount = qs["amount"];
            string tid = qs["tid"];

            string billNo = qs["billNo"];
            string studName = qs["studName"];
            string merchantType = qs["merchantType"];

            string clss = qs["class"];
            string year = qs["year"];
            string month = qs["month"];
            string rollNo = qs["rollNo"];
            string remarks = qs["remarks"];
            string agentName = qs["agentName"];

            string pin = qs["pin"];
            string note = qs["note"];
            string src = qs["src"];
            string tokenID = qs["tokenID"];

            string result = "";
            string GetMerchantName = "";

            ReplyMessage replyMessage = new ReplyMessage();

            CustActivityModel custsmsInfo = new CustActivityModel();

            FundTransfer fundtransfer = new FundTransfer
            {
                tid = tid,
                sc = sc,
                mobile = mobile,
                amount = amount,
                pin = pin,
                sourcechannel = src,
                //add for merchant
                billNo = billNo,
                studName = studName,
                merchantType = merchantType,
                clss = clss,
                year = year,
                month = month,
                rollNo = rollNo,
                remarks = remarks,
                agentName = agentName
            };

            ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, fundtransfer);

            //Merchant Payment
            if (sc == "30" || sc == "33")
            {
                fundtransfer.da = vid;
                fundtransfer.note = note;
            }

            //Utility Payment / TOPUP
            if (sc == "31" || sc == "34")
            {
                fundtransfer.da = vid;
                fundtransfer.note = note + ":" + prod;
            }

            //string remoteIP = GetUserIP();
            //string externalIP = getIPFromAgent();
            //string localIP = getInternalLANIpAddress();

            //custsmsInfo = new CustActivityModel()
            //{
            //    LocalIP = localIP,
            //    ExternalIP = externalIP,
            //    RemoteIP = remoteIP,
            //};

            //int resultsIP = CustActivityUtils.InsertIPInfo(custsmsInfo);
            //if (resultsIP > 0)
            //{
            //    message = result;
            //}
            //else
            //{
            //    message = result;
            //}


            string totalCount = string.Empty;
            string totalBAmount = string.Empty;
            string totalBCount = string.Empty;
            string balance = string.Empty;
            string statusCode = "";
            string message = string.Empty;
            string customerNo = string.Empty;
            string failedmessage = string.Empty;

            MNTransactionMaster validTransactionData = new MNTransactionMaster();

            if (TokenGenerator.TokenChecker(tokenID, mobile, src) == false)
            {
                // throw ex
                statusCode = "400";
                message = "Session expired. Please login again";
                failedmessage = message;
            }
            else
            {
                if ((tid == null) || (sc == null) || (mobile == null) || (vid == null) || (amount == null) || (pin == null) ||
                (src == null) || (double.Parse(amount) <= 0))
                {
                    // throw ex
                    statusCode = "400";
                    message = "Parameters Missing/Invalid";
                }
                else
                {
                    TransLimitCheck transLimitCheck = new TransLimitCheck();
                    string resultTranLimit = transLimitCheck.LimitCheck(mobile, "", amount, sc, pin, src);

                    var jsonDataResult = JObject.Parse(resultTranLimit);
                    statusCode = jsonDataResult["StatusCode"].ToString();
                    string statusMsg = jsonDataResult["StatusMessage"].ToString();
                    string bankBaln = jsonDataResult["BankBaln"].ToString();
                    message = jsonDataResult["StatusMessage"].ToString();
                    failedmessage = message;

                    if (sc == "30" || sc == "31")
                    {
                        GetMerchantName = "Merchant Wallet Check";
                    }

                    //Utility Payment / TOPUP
                    if (sc == "33" || sc == "34")
                    {
                        GetMerchantName = "Merchant Bank Check";
                    }


                    if ((statusCode == "200") && (statusMsg == "Success"))
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
                                result = "RetrievalRef/Trace ID Repeated";
                            }
                            else
                            {
                                result = "false";
                            }

                        } while (result == "RetrievalRef/Trace ID Repeated");

                        //End: TraceId

                        ////start:Com focus one log///
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
                                replyMessage.Response = "Destination Merchant Doesnot Exists";
                                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                                result = replyMessage.Response;
                                failedmessage = message;
                            }

                            //string ftNotedescription = GetMerchantName + ":" + fundtransfer.note;
                            string ftNotedescription = "Bill payment : " + GetMerchantName;
                            MNFundTransfer mnftWithMerchant;
                            mnftWithMerchant = new MNFundTransfer(fundtransfer.tid, fundtransfer.sc,
                                            fundtransfer.mobile, fundtransfer.sa, fundtransfer.amount, fundtransfer.da, ftNotedescription,
                                            fundtransfer.pin, fundtransfer.sourcechannel, fundtransfer.billNo, fundtransfer.studName, fundtransfer.merchantType);

                            //mnftWithMerchant = new MNFundTransfer(fundtransfer.tid, fundtransfer.sc,
                            //                   fundtransfer.mobile, fundtransfer.sa, fundtransfer.amount, fundtransfer.da, ftNotedescription,
                            //                   fundtransfer.pin, fundtransfer.sourcechannel, fundtransfer.merchantType);  //fundtransfer.note


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
                                        || (result == "Invalid Product Request") || (result == "Please try again") || (result == ""))
                                    {
                                        statusCode = "400";
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        message = result;
                                        failedmessage = result;

                                        var v = new
                                        {

                                            StatusCode = Convert.ToInt32(statusCode),
                                            StatusMessage = result
                                        };
                                        result = JsonConvert.SerializeObject(v);
                                    }
                                    if (result == "Invalid PIN")
                                    {
                                        statusCode = "400";
                                        mnft.ResponseStatus(HttpStatusCode.BadRequest, result);
                                        message = result + " " + fundtransfer.pin;
                                        failedmessage = result + " " + fundtransfer.pin;
                                        var v = new
                                        {

                                            StatusCode = Convert.ToInt32(statusCode),
                                            StatusMessage = result
                                        };
                                        result = JsonConvert.SerializeObject(v);
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
                                        int result1 = CustActivityUtils.InsertMerchantTransactionDetails(fundtransfer);

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
                            statusCode = "400";
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

                                string messagereply = "Dear " + CustCheckUtils.GetName(mobile) + "," + "\n";
                                //messagereply += GetMerchantName + " was successfully transferred with amount NPR" + validTransactionData.Amount //amtTransfer
                                //                + "." + "\n"; //validTransactionData.CreatedDate

                                messagereply += " You have successfully paid NPR " + validTransactionData.Amount
                                                + " to " + GetMerchantName
                                                + "." + "\n"; //validTransactionData.CreatedDate

                                messagereply += "Thank you. MNepal";

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

                                //For Merchant
                                /*string mobileMerchant = fundtransfer.da;
                                if ((mobileMerchant.Substring(0, 3) == "980") || (mobileMerchant.Substring(0, 3) == "981")) //FOR NCELL
                                {
                                    //FOR NCELL
                                    var content = client.DownloadString(
                                        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                        + "977" + mobileMerchant + "&Text=" + messagereply + "");
                                }
                                else if ((mobileMerchant.Substring(0, 3) == "985") || (mobileMerchant.Substring(0, 3) == "984")
                                            || (mobileMerchant.Substring(0, 3) == "986"))
                                {
                                    //FOR NTC
                                    var content = client.DownloadString(
                                        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                        + "977" + mobileMerchant + "&Text=" + messagereply + "");
                                }*/

                                custsmsInfo = new CustActivityModel()
                                {
                                    UserName = mobile,
                                    RequestMerchant = GetMerchantName,
                                    DestinationNo = "",
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
                        else if ((response.StatusCode == HttpStatusCode.BadRequest)
                            || (response.StatusCode == HttpStatusCode.InternalServerError) || (statusCode != "200"))
                        {
                            custsmsInfo = new CustActivityModel()
                            {
                                UserName = mobile,
                                RequestMerchant = GetMerchantName,
                                DestinationNo = "",
                                Amount = validTransactionData.Amount.ToString(),
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
                        if (statusCode == "")
                        {
                            statusCode = "400";
                        }
                        else
                        {
                            statusCode = statusCode.ToString();
                        }
                        message = replyMessage.Response;
                        failedmessage = message;

                        custsmsInfo = new CustActivityModel()
                        {
                            UserName = mobile,
                            RequestMerchant = GetMerchantName,
                            DestinationNo = "",
                            Amount = validTransactionData.Amount.ToString(),
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


        //// Remote IP Address (useful for getting user's IP from public site; run locally it just returns localhost)
        public string GetUserIP()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] address = ipAddress.Split(',');

                if (address.Length != 0)
                {

                    return address[0];
                }

            }

            return context.Request.ServerVariables["REMOTE_ADDR"];

        }

        //// External IP Address
        public string getIPFromAgent()
        {
            string ip = null;
            WebClient client = new WebClient();
            // Add a user agent header in case the requested URI contains a query.
            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR1.0.3705;)");
            string baseurl = "http://checkip.dyndns.org/";
            Stream data = client.OpenRead(baseurl);
            StreamReader reader = new StreamReader(data);
            string s = reader.ReadToEnd();
            data.Close();
            reader.Close();
            s = s.Replace("<html><head><title></title></head><body>", "").Replace("</body></html>", "").ToString();
            int i = s.IndexOf(':');
            // Remainder of string starting at ':' then one more space that's why i added +1
            string d = s.Substring(i + 1);

            // LblIPaddress.Text = s;
            //  return   ip = s;
            return ip = d.Trim();

        }

        //LOCALIP
        public string getInternalLANIpAddress()
        {
            // Get the host name first
            string myHostName = Dns.GetHostName();

            IPHostEntry iphostEntries = Dns.GetHostEntry(myHostName);
            // Get the IP Address from the IP Host Entry Address
            IPAddress[] arrIP = iphostEntries.AddressList;
            return arrIP[arrIP.Length - 1].ToString();

        }

        static void BackgroundTaskWithObject(Object stateInfo)
        {
            FundTransfer data = (FundTransfer)stateInfo;
            Console.WriteLine($"Hi {data.tid} from ThreadPool.");
            Thread.Sleep(1000);
        }

    }
}

