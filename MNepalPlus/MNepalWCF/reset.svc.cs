using MNepalProject.Controllers;
using MNepalProject.Helper;
using MNepalProject.Models;
using MNepalProject.Services;
using System;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Web;
using WCF.MNepal.Models;
using WCF.MNepal.Utilities;

namespace WCF.MNepal
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class reset
    {
        /****PWD RESET *****/

        #region RESETPWD

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        public string ResetPwd(Stream input)
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

            if ((tid == "null") || (sc == "null") || (mobile == "null") || (opwd == "null") || (npwd == "null") ||
                (src == "null"))
            {
                statusCode = "400";
                message = "Parameters Missing/Invalid";
                failedmessage = message;
            }
            else
            {
                DataTable dtableResult = PwdUtils.GetPwdInfo(mobile);
                if (dtableResult != null)
                {
                    foreach (DataRow dtableUser in dtableResult.Rows)
                    {
                        String oldPwd = dtableUser["Password"].ToString();
                        if (oldPwd.Equals(opwd))
                        {
                            opwd = dtableUser["Password"].ToString();
                        }
                        else
                        {
                            opwd = "";
                        }
                    }
                }

                if (opwd != "")
                {
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
                            statusCode = "400";
                            replyMessage.ResponseCode = "400";
                            replyMessage.Response = "parameters missing";
                            replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                        }
                        else
                        {
                            if (sc == "64")
                            {
                                string PwdChangedReply = "";
                                ChangePassword changepwd = new ChangePassword();
                                PwdChangedReply = changepwd.PasswordReset(mobile, npwd);

                                if (PwdChangedReply == "true")
                                {
                                    statusCode = "200";
                                    replyMessage.ResponseCode = "200";
                                    replyMessage.Response = "Password change successful";
                                    replyMessage.ResponseStatus(HttpStatusCode.OK, replyMessage.Response);
                                }
                                else
                                {
                                    statusCode = "400";
                                    replyMessage.ResponseCode = "400";
                                    replyMessage.Response = "Invalid Password";
                                    replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                                    failedmessage = replyMessage.Response;
                                }

                            }
                            else
                            {
                                statusCode = "400";
                                replyMessage.ResponseCode = "400";
                                replyMessage.Response = "Invalid Service Code";
                                replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                                failedmessage = replyMessage.Response;
                            }
                        }
                    }
                    else
                    {
                        statusCode = "400";
                        replyMessage.ResponseCode = "400";
                        replyMessage.Response = "request denied";
                        replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, replyMessage.Response);
                        failedmessage = replyMessage.Response;
                    }
                }
                else
                {
                    statusCode = "400";
                    replyMessage.ResponseCode = "400";
                    replyMessage.Response = "Please Enter Correct Old Password.";
                    replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                    failedmessage = replyMessage.Response;
                }

                //return replyMessage.Response;
            }

            if ((replyMessage.ResponseCode != "200") || (statusCode != "200"))
            {
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = failedmessage
                };
                result = Newtonsoft.Json.JsonConvert.SerializeObject(v);
            }
            else if ((replyMessage.ResponseCode == "200") || (statusCode == "200"))
            {
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = replyMessage.Response
                };
                result = Newtonsoft.Json.JsonConvert.SerializeObject(v);
            }

            return result;
        }

        #endregion

        /*****END PWD RESET ******/

        /****PIN RESET *****/

        #region RESETPIN

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        public string ResetPin(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            string sc = qs["sc"]; //PinChange = 62
            string mobile = qs["mobile"];
            string opin = qs["opin"];
            string npin = qs["npin"];
            string src = qs["src"];

            ReplyMessage replyMessage = new ReplyMessage();

            string result = "";
            string statusCode = string.Empty;
            string message = string.Empty;
            string failedmessage = string.Empty;

            TraceIdGenerator tig = new TraceIdGenerator();
            string tid = tig.GenerateTraceID();

            if ((tid == "null") || (sc == "null") || (mobile == "null") || (npin == "null") ||
                (src == "null"))
            {
                statusCode = "400";
                message = "Parameters Missing/Invalid";
                failedmessage = message;
            }
            else
            {
                PinChange pinchange = new PinChange(tid, sc, mobile, opin, npin, src);
                Message<PinChange> pinchangerequest = new Message<PinChange>(pinchange);
                MNComAndFocusOneLog comfocuslog = new MNComAndFocusOneLog(tid, 0, pinchangerequest.message, "0", DateTime.Now);
                MNComAndFocusOneLogsController comfocuslogs = new MNComAndFocusOneLogsController();

                result = comfocuslogs.InsertIntoComFocusOne(comfocuslog);

                if (result == "Success")
                {
                    if (String.IsNullOrEmpty(pinchange.tid) || String.IsNullOrEmpty(pinchange.sc) || String.IsNullOrEmpty(pinchange.mobile)
                        || String.IsNullOrEmpty(pinchange.npin) || String.IsNullOrEmpty(pinchange.src)) //String.IsNullOrEmpty(pwdchange.opwd) ||
                    {
                        replyMessage.Response = "parameters missing";
                        replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                    }
                    else
                    {
                        if (sc == "62")
                        {
                            string PINChangedReply = "";
                            Pin changepin = new Pin();
                            PINChangedReply = changepin.PINReset(mobile, npin);

                            if (PINChangedReply == "true")
                            {
                                statusCode = "200";
                                replyMessage.ResponseCode = "200";
                                replyMessage.Response = "PIN change successful";
                                replyMessage.ResponseStatus(HttpStatusCode.OK, replyMessage.Response);
                            }
                            else
                            {
                                statusCode = "400";
                                replyMessage.ResponseCode = "400";
                                replyMessage.Response = "Invalid PIN";
                                replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                                failedmessage = replyMessage.Response;
                            }

                        }
                        else
                        {
                            statusCode = "400";
                            replyMessage.ResponseCode = "400";
                            replyMessage.Response = "Invalid Service Code";
                            replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                            failedmessage = replyMessage.Response;
                        }
                    }
                }
                else
                {
                    statusCode = "400";
                    replyMessage.ResponseCode = "400";
                    replyMessage.Response = "request denied";
                    replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, replyMessage.Response);
                    failedmessage = replyMessage.Response;
                }

                //return replyMessage.Response;
            }

            if ((replyMessage.ResponseCode != "200") || (statusCode != "200"))
            {
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = failedmessage
                };
                result = Newtonsoft.Json.JsonConvert.SerializeObject(v);
            }
            else if ((replyMessage.ResponseCode == "200") || (statusCode == "200"))
            {
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = replyMessage.Response
                };
                result = Newtonsoft.Json.JsonConvert.SerializeObject(v);
            }

            return result;
        }

        #endregion

        /*****END PIN RESET ******/

    }
}
