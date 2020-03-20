using MNepalProject.Helper;
using MNepalProject.Models;
using System;
using System.Collections.Specialized;
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
    public class requestCode
    {
        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        public string ActivationCode(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();

            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            string userName = qs["UserName"];
            string deviceId = qs["deviceID"];
            string src = qs["src"];

            ReplyMessage replyMessage = new ReplyMessage();
            string result = "";
            string code = string.Empty;
            string statusCode = string.Empty;
            string message = string.Empty;

            if ((userName == null) || (deviceId == null) || (src == null))
            {
                // throw ex
                statusCode = "400";
                message = "Parameters Missing/Invalid";
            }
            else
            {
                try
                {
                    if (UserNameCheck.IsValidUserName(userName))
                    {
                        TraceIdGenerator otp = new TraceIdGenerator();
                        code = otp.GetUniqueOTPKey();
                        string messagereply = "Dear Customer," + "\n";
                        messagereply += " Your Verification Code is " + code
                            + "." + "\n" + "Close this message and enter code to activate account.";
                        messagereply += "-MNepal";

                        var client = new WebClient();

                        string mobile = "";
                        mobile = userName;

                        if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                        {
                            //FOR NCELL
                            var contents = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                            + "977" + mobile + "&Text=" + messagereply + "");
                        }
                        else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                            || (mobile.Substring(0, 3) == "986"))
                        {
                            //FOR NTC
                            var contents = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                                + "977" + mobile + "&Text=" + messagereply + "");
                        }

                        SMSLog log = new SMSLog();
                        log.SentBy = mobile;
                        log.Purpose = "Verification Code";
                        log.UserName = userName;
                        log.Message = messagereply;
                        CustCheckUtils.LogSMS(log);

                        statusCode = "200";
                        message = code;
                    }
                    else
                    {
                        statusCode = "400";
                        result = "\n The number is not registered !!";
                        replyMessage.Response = "\n The number is not registered !!";
                        replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                        message = result;
                    }

                }
                catch (Exception ex)
                {
                    statusCode = "400";
                    result = "\n Please Contact to the administrator !!" + ex;
                    replyMessage.Response = result;
                    replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                    message = result;
                }
            }

            
            if (statusCode == "200")
            {
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = message
                };
                result = JsonConvert.SerializeObject(v);
            }
            else
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

    }
}
