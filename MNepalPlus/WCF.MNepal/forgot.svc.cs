using MNepalProject.Controllers;
using MNepalProject.Helper;
using MNepalProject.Models;
using MNepalProject.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Web;
using WCF.MNepal.Helper;
using WCF.MNepal.Models;
using WCF.MNepal.Utilities;

namespace WCF.MNepal
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
    public class forgot
    {
        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        public string CheckUserName(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();

            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            string userName = qs["mobile"];
            string src = qs["src"];

            //SMS
            string SMSNTC = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalNTCSMSServerUrl"];
            string SMSNCELL = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalSMSServerUrl"];

            ReplyMessage replyMessage = new ReplyMessage();
            string result = "";
            string code = string.Empty;
            string statusCode = string.Empty;
            string message = string.Empty;

            if ((userName == null) || (src == null))
            {
                // throw ex
                statusCode = "400";
                message = "Parameters Missing/Invalid";
            }
            else
            {
                try
                {
                    if (UserNameCheck.IsValidUserNameForgotPassword(userName))
                    {
                        string validStatus = "";
                        DataTable dtableStatusResult = CustCheckUtils.GetCustStatusInfo(userName);
                        if (dtableStatusResult != null)
                        {
                            foreach (DataRow dtableUser in dtableStatusResult.Rows)
                            {
                                validStatus = dtableUser["Status"].ToString();
                            }
                        }

                        if (validStatus == "Expired")
                        {
                            statusCode = "400";
                            message = "Account is Blocked";
                            replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                        }
                        if (validStatus == "Blocked")
                        {
                            statusCode = "400";
                            message = "Account is Blocked";
                            replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                        }
                        else
                        {
                            if (LoginUtils.GetPasswordBlockTime(userName))
                            {
                                statusCode = "400";
                                result = "You have already attempt 3 times with wrong password,Please try again after 1 hour";
                                replyMessage.Response = "Account is Blocked";
                                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                                message = result;
                            }
                            else
                            {
                                TraceIdGenerator otp = new TraceIdGenerator();
                                code = otp.GetUniqueOTPKey();
                                string messagereply = "Dear " + CustCheckUtils.GetName(userName) + "," + "\n";
                                messagereply += " Your Verification Code is " + code
                                    + "." + "\n" + "Close this message and enter code to recover account.";
                                messagereply += "-MNepal";

                                var client = new WebClient();

                                string mobile = "";
                                mobile = userName;

                                if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                                {
                                    //FOR NCELL
                                    //var contents = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                                    //+ "977" + mobile + "&Text=" + messagereply + "");
                                    var contents = client.DownloadString(SMSNCELL
                                    + "977" + mobile + "&Text=" + messagereply + "");
                                }
                                else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                    || (mobile.Substring(0, 3) == "986"))
                                {
                                    //FOR NTC
                                    //var contents = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                                    //    + "977" + mobile + "&Text=" + messagereply + "");
                                    var contents = client.DownloadString(SMSNTC
                                        + "977" + mobile + "&Text=" + messagereply + "");
                                }

                                SMSLog log = new SMSLog();
                                log.SentBy = mobile;
                                log.Purpose = "Forgot Pin";
                                log.UserName = userName;
                                log.Message = messagereply;
                                CustCheckUtils.LogSMS(log);

                                statusCode = "200";
                                message = code;
                            }
                        }
                    }
                    else
                    {
                        //statusCode = "400";
                        //result = "The number is not registered !!";
                        //replyMessage.Response = "The number is not registered !!";
                        //replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                        //message = result;

                        statusCode = "400";
                        result = "A OTP code will be sent to your phone if the account exists in our system.";
                        replyMessage.Response = "A OTP code will be sent to your phone if the account exists in our system.";
                        replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                        message = result;
                    }

                }
                catch (Exception ex)
                {
                    statusCode = "400";
                    result = "Please Contact to the administrator !!" + ex;
                    replyMessage.Response = result;
                    replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                    message = result;
                }
            }

            if (statusCode != "200")
            {
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = message
                };
                result = JsonConvert.SerializeObject(v);
            }
            else if (statusCode == "200")
            {
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
        public string CheckAgentUserName(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();

            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            string userName = qs["mobile"];
            string src = qs["src"];

            //SMS
            string SMSNTC = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalNTCSMSServerUrl"];
            string SMSNCELL = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalSMSServerUrl"];

            ReplyMessage replyMessage = new ReplyMessage();
            string result = "";
            string code = string.Empty;
            string statusCode = string.Empty;
            string message = string.Empty;

            if ((userName == null) || (src == null))
            {
                // throw ex
                statusCode = "400";
                message = "Parameters Missing/Invalid";
            }
            else
            {
                try
                {
                    if (UserNameCheck.IsValidAgent(userName))
                    {
                        if (LoginUtils.GetPasswordBlockTime(userName))
                        {
                            statusCode = "400";
                            result = "You have already attempt 3 times with wrong password,Please try again after 1 hour";
                            replyMessage.Response = "Account is Blocked";
                            replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                            message = result;
                        }
                        else
                        {
                            TraceIdGenerator otp = new TraceIdGenerator();
                            code = otp.GetUniqueOTPKey();
                            string messagereply = "Dear " + CustCheckUtils.GetName(userName) + "," + "\n";
                            messagereply += " Your Verification Code is " + code
                                + "." + "\n" + "Close this message and enter code to recover account.";
                            messagereply += "-MNepal";

                            var client = new WebClient();

                            string mobile = "";
                            mobile = userName;

                            if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                            {
                                //FOR NCELL
                                //var contents = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                                //+ "977" + mobile + "&Text=" + messagereply + "");
                                var contents = client.DownloadString(SMSNCELL
                                    + "977" + mobile + "&Text=" + messagereply + "");
                            }
                            else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                || (mobile.Substring(0, 3) == "986"))
                            {
                                //FOR NTC
                                //var contents = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                                //    + "977" + mobile + "&Text=" + messagereply + "");
                                var contents = client.DownloadString(SMSNTC
                                    + "977" + mobile + "&Text=" + messagereply + "");
                            }

                            SMSLog log = new SMSLog();
                            log.SentBy = mobile;
                            log.Purpose = "Forgot Pin";
                            log.UserName = userName;
                            log.Message = messagereply;
                            CustCheckUtils.LogSMS(log);

                            statusCode = "200";
                            message = code;
                        }
                    }
                    else
                    {
                        //statusCode = "400";
                        //result = "The number is not registered !!";
                        //replyMessage.Response = "The number is not registered !!";
                        //replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                        //message = result;


                        statusCode = "400";
                        result = "A OTP code will be sent to your phone if the account exists in our system.";
                        replyMessage.Response = "A OTP code will be sent to your phone if the account exists in our system.";
                        replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                        message = result;
                         

                    }

                }
                catch (Exception ex)
                {
                    statusCode = "400";
                    result = "Please Contact to the administrator !!" + ex;
                    replyMessage.Response = result;
                    replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                    message = result;
                }
            }

            if (statusCode != "200")
            {
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = message
                };
                result = JsonConvert.SerializeObject(v);
            }
            else if (statusCode == "200")
            {
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = message
                };
                result = JsonConvert.SerializeObject(v);
            }

            return result;
        }




        // start for merchant forget pwd
        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        public string CheckMerchantUserName(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();

            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            string userName = qs["mobile"];
            string src = qs["src"];

            //SMS
            string SMSNTC = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalNTCSMSServerUrl"];
            string SMSNCELL = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalSMSServerUrl"];

            ReplyMessage replyMessage = new ReplyMessage();
            string result = "";
            string code = string.Empty;
            string statusCode = string.Empty;
            string message = string.Empty;

            if ((userName == null) || (src == null))
            {
                // throw ex
                statusCode = "400";
                message = "Parameters Missing/Invalid";
            }
            else
            {
                try
                {
                    if (UserNameCheck.IsValidMerchant(userName))
                    {
                        if (LoginUtils.GetPasswordBlockTime(userName))
                        {
                            statusCode = "400";
                            result = "You have already attempt 3 times with wrong password,Please try again after 1 hour";
                            replyMessage.Response = "Account is Blocked";
                            replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                            message = result;
                        }
                        else
                        {
                            TraceIdGenerator otp = new TraceIdGenerator();
                            code = otp.GetUniqueOTPKey();
                            string messagereply = "Dear " + CustCheckUtils.GetName(userName) + "," + "\n";
                            messagereply += " Your Verification Code is " + code
                                + "." + "\n" + "Close this message and enter code to recover account.";
                            messagereply += "-MNepal";

                            var client = new WebClient();

                            string mobile = "";
                            mobile = userName;

                            if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                            {
                                //FOR NCELL
                                //var contents = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                                //+ "977" + mobile + "&Text=" + messagereply + "");
                                var content = client.DownloadString(
                                                SMSNCELL + "977" + mobile + "&Text=" + messagereply + "");
                            }
                            else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                || (mobile.Substring(0, 3) == "986"))
                            {
                                //FOR NTC
                                //var contents = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                                //    + "977" + mobile + "&Text=" + messagereply + "");
                                var content = client.DownloadString(
                                                SMSNTC + "977" + mobile + "&Text=" + messagereply + "");
                            }

                            SMSLog log = new SMSLog();
                            log.SentBy = mobile;
                            log.Purpose = "Forgot Pin";
                            log.UserName = userName;
                            log.Message = messagereply;
                            CustCheckUtils.LogSMS(log);

                            statusCode = "200";
                            message = code;
                        }
                    }
                    else
                    {
                        //statusCode = "400";
                        //result = "The number is not registered !!";
                        //replyMessage.Response = "The number is not registered !!";
                        //replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                        //message = result;

                        statusCode = "400";
                        result = "A OTP code will be sent to your phone if the account exists in our system.";
                        replyMessage.Response = "A OTP code will be sent to your phone if the account exists in our system.";
                        replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                        message = result;
                    }

                }
                catch (Exception ex)
                {
                    statusCode = "400";
                    result = "Please Contact to the administrator !!" + ex;
                    replyMessage.Response = result;
                    replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                    message = result;
                }
            }

            if (statusCode != "200")
            {
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = message
                };
                result = JsonConvert.SerializeObject(v);
            }
            else if (statusCode == "200")
            {
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = message
                };
                result = JsonConvert.SerializeObject(v);
            }

            return result;
        }


        //end for merchant forget pwd



        /****PIN FORGOT *****/

        #region FORGOTPIN

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, 
            RequestFormat = WebMessageFormat.Json)]
        public string FPin(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            string sc = qs["sc"]; //PinChange = 62
            string mobile = qs["mobile"];
            string pin = qs["pin"];
            string npin = qs["npin"];
            string src = qs["src"];

            ReplyMessage replyMessage = new ReplyMessage();

            string result = "";
            string statusCode = string.Empty;
            string message = string.Empty;
            string failedmessage = string.Empty;

            TraceIdGenerator tig = new TraceIdGenerator();
            string tid = tig.GenerateTraceID();

            if ((tid == "null") || (sc == "null") || (mobile == "null") || (pin == "null") || (npin == "null") ||
                (src == "null"))
            {
                statusCode = "400";
                message = "Parameters Missing/Invalid";
                failedmessage = message;
            }
            else
            {
                PinChange pinchange = new PinChange(tid, sc, mobile, pin, npin, src);
                Message<PinChange> pinchangerequest = new Message<PinChange>(pinchange);
                MNComAndFocusOneLog comfocuslog = new MNComAndFocusOneLog(tid, 0, pinchangerequest.message, "0", DateTime.Now);
                MNComAndFocusOneLogsController comfocuslogs = new MNComAndFocusOneLogsController();

                result = comfocuslogs.InsertIntoComFocusOne(comfocuslog);

                if (result == "Success")
                {
                    if (String.IsNullOrEmpty(pinchange.tid) || String.IsNullOrEmpty(pinchange.sc) || String.IsNullOrEmpty(pinchange.mobile) || String.IsNullOrEmpty(pinchange.npin) || String.IsNullOrEmpty(pinchange.src))
                    //String.IsNullOrEmpty(pinchange.pin) ||
                    {
                        statusCode = "400";
                        replyMessage.Response = "parameters missing";
                        replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                        failedmessage = replyMessage.Response;
                    }
                    else
                    {
                        if (sc == "62")
                        {
                            string PINChangedReply = "";
                            Pin changepin = new Pin();
                            PINChangedReply = changepin.ForgotPIN(mobile, npin);

                            if (PINChangedReply == "true")
                            {
                                replyMessage.Response = "Your T-PIN has been successfully changed.";
                                replyMessage.ResponseStatus(HttpStatusCode.OK, replyMessage.Response);
                            }
                            else
                            {
                                statusCode = "400";
                                replyMessage.Response = "Invalid T-PIN";
                                replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                                failedmessage = replyMessage.Response;
                            }

                        }
                        else
                        {
                            statusCode = "400";
                            replyMessage.Response = "Invalid Service Code";
                            replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                            failedmessage = replyMessage.Response;
                        }
                    }
                }
                else
                {
                    statusCode = "400";
                    replyMessage.Response = "request denied";
                    replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, replyMessage.Response);
                    failedmessage = replyMessage.Response;
                }

                return replyMessage.Response;
            }

            if (statusCode != "200")
            {
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = message
                };
                result = JsonConvert.SerializeObject(v);
            }

            return result;
        }

        #endregion

        /*****END PIN FORGOT ******/


        /****PWD FORGOT *****/

        #region FORGOTPWD

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        public string FPwd(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            string sc = qs["sc"]; //PwdChange = 64
            string mobile = qs["mobile"];
            string opwd = qs["opwd"];
            string npwd = qs["npwd"];
            string src = qs["src"];

            ReplyMessage replyMessage = new ReplyMessage();

            string result = "";
            string statusCode = string.Empty;
            string message = string.Empty;
            string failedmessage = string.Empty;

            TraceIdGenerator tig = new TraceIdGenerator();
            string tid = tig.GenerateTraceID();

            if ((tid == "null") || (sc == "null") || (mobile == "null") || (npwd == "null") ||
                (src == "null"))
            {
                statusCode = "400";
                message = "Parameters Missing/Invalid";
                failedmessage = message;
            }
            else
            {
                opwd = HashAlgo.Hash(opwd);
                npwd = HashAlgo.Hash(npwd);

                PwdChange pwdchange = new PwdChange(tid, sc, mobile, opwd, npwd, src);
                Message<PwdChange> pwdchangerequest = new Message<PwdChange>(pwdchange);
                MNComAndFocusOneLog comfocuslog = new MNComAndFocusOneLog(tid, 0, pwdchangerequest.message, "0", DateTime.Now);
                MNComAndFocusOneLogsController comfocuslogs = new MNComAndFocusOneLogsController();

                result = comfocuslogs.InsertIntoComFocusOne(comfocuslog);

                if (result == "Success")
                {
                    if (String.IsNullOrEmpty(pwdchange.tid) || String.IsNullOrEmpty(pwdchange.sc) || String.IsNullOrEmpty(pwdchange.mobile) 
                        || String.IsNullOrEmpty(pwdchange.npwd) || String.IsNullOrEmpty(pwdchange.src)) //String.IsNullOrEmpty(pwdchange.opwd) ||
                    {
                        replyMessage.Response = "parameters missing";
                        replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                    }
                    else
                    {
                        if (sc == "64")
                        {
                            string PwdChangedReply = "";
                            ChangePassword changepwd = new ChangePassword();
                            PwdChangedReply = changepwd.PasswordForgot(mobile, npwd);

                            if (PwdChangedReply == "true")
                            {
                                replyMessage.Response = "Your password has been successfully changed.";
                                replyMessage.ResponseStatus(HttpStatusCode.OK, replyMessage.Response);
                            }
                            else
                            {
                                statusCode = "400";
                                replyMessage.Response = "Invalid Password";
                                replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                                failedmessage = replyMessage.Response;
                            }

                        }
                        else
                        {
                            statusCode = "400";
                            replyMessage.Response = "Invalid Service Code";
                            replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                            failedmessage = replyMessage.Response;
                        }
                    }
                }
                else
                {
                    statusCode = "400";
                    replyMessage.Response = "request denied";
                    replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, replyMessage.Response);
                    failedmessage = replyMessage.Response;
                }

                return replyMessage.Response;
            }

            if (statusCode != "200")
            {
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = message
                };
                result = JsonConvert.SerializeObject(v);
            }

            return result;
        }

        #endregion

        /*****END PWD FORGOT ******/

        
    }
}
