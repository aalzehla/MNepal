using MNepalProject.Models;
using System;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Web;
using MNepalWCF.Models;
using MNepalWCF.Utilities;

namespace MNepalWCF
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class authlogin
    {
        [OperationContract]
        [WebInvoke(Method = "POST",
                  ResponseFormat = WebMessageFormat.Json
                  )]
        public string Login(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);
            
            string userName = qs["mobile"];
            string password = qs["password"];
            string deviceId = qs["deviceId"];
            string mcode = qs["code"];
            string src = qs["src"];

            ReplyMessage replyMessage = new ReplyMessage();
            string result = "";
            string code = string.Empty;
            string statusCode = string.Empty;
            string message = string.Empty;
            string json= string.Empty;

            string Name = string.Empty;
            string UserType = string.Empty;
            string ClientCode = string.Empty;
            string TPin = string.Empty;
            string IsFirstLogin = string.Empty;
            string VerificationCode = string.Empty;

            if ((userName == null) || (password == null) || (deviceId == null))
            {
                statusCode = "400";
                message = "Parameters Missing/Invalid";
                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
            }
            else
            {
                try {

                    if ((userName != null) && (password != null) && (deviceId != null) && (mcode == null) || (mcode == ""))
                    {

                        Models.ResponseParams obj = new Models.ResponseParams();

                        DataTable dtableUser = LoginUtils.GetLoginInfo(userName, password);
                        if (dtableUser != null && dtableUser.Rows.Count > 0)
                        {
                            ClientCode = dtableUser.Rows[0]["ClientCode"].ToString();
                            Name = dtableUser.Rows[0]["Name"].ToString();
                            UserType = dtableUser.Rows[0]["UserType"].ToString();
                            TPin = dtableUser.Rows[0]["PIN"].ToString();
                            IsFirstLogin = dtableUser.Rows[0]["IsFirstLogin"].ToString();

                            string mobile = string.Empty;
                            mobile = userName;
                            Random random = new Random();
                            int pwd = random.Next(1000, 9999);
                            SaveLoginInfo(mobile, deviceId, pwd);

                            bool success = false;
                            if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                            {
                                //FOR NCELL
                                sendSMS(mobile, pwd);
                                success = true;//
                            }
                            else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                                || (mobile.Substring(0, 3) == "986"))
                            {
                                //FOR NTC
                                sendNTSMS(mobile, pwd);
                                success = true;//
                            }

                            if (success)
                            {
                                obj.Status = 200;
                                obj.Message = "Activation Code Sent.";
                                obj.VerificationCode = pwd;

                                var v = new { result = obj.Message, VerificationCode = obj.VerificationCode, UserName = Name, UserType = UserType, ClientCode = ClientCode, TPin = TPin, IsFirstLogin = IsFirstLogin };
                                json = JsonConvert.SerializeObject(v);
                                message = json;
                            }
                            else
                            {
                                obj.Status = 901;
                                obj.Message = "SMS server with failure message.";
                                obj.VerificationCode = 0;
                                Name = "";
                                UserType = "";
                                obj.VerificationCode = 0;
                                IsFirstLogin = "";
                                ClientCode = "";
                                TPin = "";

                                var v = new { result = obj.Message, VerificationCode = obj.VerificationCode, UserName = Name, UserType = UserType, ClientCode = ClientCode, TPin = TPin, IsFirstLogin = IsFirstLogin };
                                json = JsonConvert.SerializeObject(v);
                                message = json;
                            }

                        }
                        else
                        {
                            obj.Status = 901;
                            obj.Message = "SMS server with failure message.";
                            Name = "";
                            UserType = "";
                            obj.VerificationCode = 0;
                            IsFirstLogin = "";
                            ClientCode = "";
                            TPin = "";
                            var v = new { result = obj.Message, VerificationCode = obj.VerificationCode, UserName = Name, UserType = UserType, ClientCode = ClientCode, TPin = TPin, IsFirstLogin = IsFirstLogin };
                            json = JsonConvert.SerializeObject(v);
                            message = json;
                        }
                    }
                    else if ((userName != null) && (password != null) && (deviceId != null) || (mcode != ""))
                    {
                        Models.ResponseParams obj = new Models.ResponseParams();

                        DataTable dtableUser = LoginUtils.GetLoginInfo(userName, password);
                        if (dtableUser != null && dtableUser.Rows.Count > 0)
                        {

                            ClientCode = dtableUser.Rows[0]["ClientCode"].ToString();
                            Name = dtableUser.Rows[0]["Name"].ToString();
                            UserType = dtableUser.Rows[0]["UserType"].ToString();
                            TPin = dtableUser.Rows[0]["PIN"].ToString();
                            IsFirstLogin = dtableUser.Rows[0]["IsFirstLogin"].ToString();

                            string mobile = string.Empty;
                            mobile = userName;

                            bool success = true;
                            int pwd = int.Parse(code);
                            SaveLoginInfo(mobile, deviceId, pwd);

                            if (success)
                            {
                                statusCode = "200";
                                obj.Message = "Activation Code Sent.";
                                obj.VerificationCode = pwd;
                                var v = new { result = obj.Message, VerificationCode = pwd, UserName = Name, UserType = UserType, ClientCode = ClientCode, TPin = TPin, IsFirstLogin = IsFirstLogin };
                                json = JsonConvert.SerializeObject(v);
                                message = json;
                            }
                            else
                            {
                                statusCode = "901";
                                obj.Message = "SMS server with failure message.";
                                obj.VerificationCode = 0;
                                var v = new { result = obj.Message, VerificationCode = pwd, UserName = Name, UserType = UserType, ClientCode = ClientCode, TPin = TPin, IsFirstLogin = IsFirstLogin };
                                json = JsonConvert.SerializeObject(v);
                                message = json;
                            }
                        }
                        else
                        {
                            obj.Status = 901;
                            obj.Message = "SMS server with failure message.";
                            Name = "";
                            UserType = "";
                            obj.VerificationCode = 0;
                            statusCode = "901";
                            var v = new { result = obj.Message, VerificationCode = 0, UserName = "", UserType = "", ClientCode = ClientCode, TPin = "", IsFirstLogin = "" };
                            json = JsonConvert.SerializeObject(v);
                            message = json;
                        }

                        //var v = new { AmounttransferredBalance = Convert.ToDecimal(tamount).ToString("#,##0.00"), availableBalance = Balance, message = msgConstruct };
                        //string json = JsonConvert.SerializeObject(v);
                        //result.Add(new LoginAuth()
                        //{
                        //    Status = obj.Status,
                        //    Message = obj.Message,
                        //    VerificationCode = obj.VerificationCode,
                        //    UserName = Name,
                        //    UserType = UserType,
                        //    ClientCode = ClientCode,
                        //    TPin = TPin,
                        //    IsFirstLogin = IsFirstLogin
                        //});

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

            OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    string messagereply = "Dear Customer," + "\n";
                    messagereply += " Your OTP is " + code
                        + "." + "\n" + "Close this message and enter code to register your account.";
                    messagereply += "Thank-you. -MNepal";

                    var client = new WebClient();

                    string mobile = "";
                    mobile = userName;

                    if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                    {
                        //FOR NCELL
                        var contents = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                        + "977" + mobile + "&Text=" + messagereply + "");

                        statusCode = "200";
                        message = code;
                    }
                    else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                        || (mobile.Substring(0, 3) == "986"))
                    {
                        //FOR NTC
                        var contents = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                            + "977" + mobile + "&Text=" + messagereply + "");

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

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                statusCode = "400";
                result = "The number is registered !!";
                replyMessage.Response = "The number is registered !!";
                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                message = result;
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
            if (statusCode == "200")
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


        public static bool sendSMS(string mobile, int password)
        {
            //string url = "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878" + "&To=" + mobile + "&Text=Your Verification code is " + password + ". Close this message and enter code to activate account.\n-MNepal";
            string url = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalSMSServerUrl"] + "&To=" + mobile + "&Text=Your Verification code is " + password + ". Close this message and enter code to activate account.\n-MNepal";
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            //Get response from server

            HttpWebResponse httpWebResponse = (HttpWebResponse)request.GetResponse();

            int statuscode = (int)httpWebResponse.StatusCode;

            string responseString = new StreamReader(httpWebResponse.GetResponseStream()).ReadToEnd();
            httpWebResponse.Close();
            if (responseString == "success")
                return true;
            else
                return false;
        }

        public static bool sendNTSMS(string mobile, int password)
        {
            //string url = "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878" + "&To=" + mobile + "&Text=Your Verification code is " + password + ". Close this message and enter code to activate account.\n-MNepal";
            string url = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalNTCSMSServerUrl"] + "&To=" + mobile + "&Text=Your Verification code is " + password + ". Close this message and enter code to activate account.\n-MNepal";
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            //Get response from server

            HttpWebResponse httpWebResponse = (HttpWebResponse)request.GetResponse();

            int statuscode = (int)httpWebResponse.StatusCode;

            string responseString = new StreamReader(httpWebResponse.GetResponseStream()).ReadToEnd();
            httpWebResponse.Close();
            if (responseString == "success")
                return true;
            else
                return false;
        }

        public static void SaveLoginInfo(string mobile, string deviceId, int password)
        {
            bool newUserDevice = true;
            Login logInfo = new Login()
            {
                Mobile = mobile,
                DeviceID = deviceId,
                GeneratedPass = Convert.ToString(password)
            };

            int results = LoginUtils.CreateMobileUserInfo(logInfo);
            if (results > 0)
            {
                newUserDevice = false;
            }
            else
            {
                newUserDevice = true;
            }
        }

    }
}
