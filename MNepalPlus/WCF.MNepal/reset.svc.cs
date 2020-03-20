using MNepalProject.Controllers;
using MNepalProject.Helper;
using MNepalProject.Models;
using MNepalProject.Services;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using WCF.MNepal.Helper;
using WCF.MNepal.Models;
using WCF.MNepal.UserModels;
using WCF.MNepal.Utilities;

namespace WCF.MNepal
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
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
                        if (oldPwd.Equals(HashAlgo.Hash(opwd)))
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
                                    replyMessage.Response = "Your Password has been successfully changed.";
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

        ///****PIN RESET *****/

        //#region RESETPIN

        //[OperationContract]
        //[WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        //public string ResetPin(Stream input)
        //{
        //    StreamReader sr = new StreamReader(input);
        //    string s = sr.ReadToEnd();
        //    sr.Dispose();
        //    NameValueCollection qs = HttpUtility.ParseQueryString(s);

        //    string sc = qs["sc"]; //PinChange = 62
        //    string mobile = qs["mobile"];
        //    string opin = qs["opin"];
        //    string npin = qs["npin"];
        //    string src = qs["src"];

        //    ReplyMessage replyMessage = new ReplyMessage();

        //    string result = "";
        //    string statusCode = string.Empty;
        //    string message = string.Empty;
        //    string failedmessage = string.Empty;

        //    TraceIdGenerator tig = new TraceIdGenerator();
        //    string tid = tig.GenerateTraceID();

        //    if ((tid == "null") || (sc == "null") || (mobile == "null") || (npin == "null") ||
        //        (src == "null"))
        //    {
        //        statusCode = "400";
        //        message = "Parameters Missing/Invalid";
        //        failedmessage = message;
        //    }
        //    else
        //    {
        //        PinChange pinchange = new PinChange(tid, sc, mobile, opin, npin, src);
        //        Message<PinChange> pinchangerequest = new Message<PinChange>(pinchange);
        //        MNComAndFocusOneLog comfocuslog = new MNComAndFocusOneLog(tid, 0, pinchangerequest.message, "0", DateTime.Now);
        //        MNComAndFocusOneLogsController comfocuslogs = new MNComAndFocusOneLogsController();

        //        result = comfocuslogs.InsertIntoComFocusOne(comfocuslog);

        //        if (result == "Success")
        //        {
        //            if (String.IsNullOrEmpty(pinchange.tid) || String.IsNullOrEmpty(pinchange.sc) || String.IsNullOrEmpty(pinchange.mobile)
        //                || String.IsNullOrEmpty(pinchange.npin) || String.IsNullOrEmpty(pinchange.src)) //String.IsNullOrEmpty(pwdchange.opwd) ||
        //            {
        //                replyMessage.Response = "parameters missing";
        //                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
        //            }
        //            else
        //            {
        //                if (sc == "62")
        //                {
        //                    string PINChangedReply = "";
        //                    Pin changepin = new Pin();
        //                    PINChangedReply = changepin.PINReset(mobile, npin);

        //                    if (PINChangedReply == "true")
        //                    {
        //                        statusCode = "200";
        //                        replyMessage.ResponseCode = "200";
        //                        replyMessage.Response = "PIN change successful";
        //                        replyMessage.ResponseStatus(HttpStatusCode.OK, replyMessage.Response);
        //                    }
        //                    else
        //                    {
        //                        statusCode = "400";
        //                        replyMessage.ResponseCode = "400";
        //                        replyMessage.Response = "Invalid PIN";
        //                        replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
        //                        failedmessage = replyMessage.Response;
        //                    }

        //                }
        //                else
        //                {
        //                    statusCode = "400";
        //                    replyMessage.ResponseCode = "400";
        //                    replyMessage.Response = "Invalid Service Code";
        //                    replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
        //                    failedmessage = replyMessage.Response;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            statusCode = "400";
        //            replyMessage.ResponseCode = "400";
        //            replyMessage.Response = "request denied";
        //            replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, replyMessage.Response);
        //            failedmessage = replyMessage.Response;
        //        }

        //        //return replyMessage.Response;
        //    }

        //    if ((replyMessage.ResponseCode != "200") || (statusCode != "200"))
        //    {
        //        var v = new
        //        {
        //            StatusCode = Convert.ToInt32(statusCode),
        //            StatusMessage = failedmessage
        //        };
        //        result = Newtonsoft.Json.JsonConvert.SerializeObject(v);
        //    }
        //    else if ((replyMessage.ResponseCode == "200") || (statusCode == "200"))
        //    {
        //        var v = new
        //        {
        //            StatusCode = Convert.ToInt32(statusCode),
        //            StatusMessage = replyMessage.Response
        //        };
        //        result = Newtonsoft.Json.JsonConvert.SerializeObject(v);
        //    }

        //    return result;
        //}

        //#endregion

        ///*****END PIN RESET ******/

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
                DataTable dtableResult = PinUtils.GetPinInfo(mobile);
                if (dtableResult != null)
                {
                    foreach (DataRow dtableUser in dtableResult.Rows)
                    {
                        String oldPwd = dtableUser["PIN"].ToString();
                        if (oldPwd.Equals(opin))
                        {
                            opin = dtableUser["PIN"].ToString();
                        }
                        else
                        {
                            opin = "";
                        }
                    }
                }
                if (opin != "")
                {
                    PinChange pinchange = new PinChange(tid, sc, mobile, opin, npin, src);
                    Message<PinChange> pinchangerequest = new Message<PinChange>(pinchange);
                    MNComAndFocusOneLog comfocuslog = new MNComAndFocusOneLog(tid, 0, pinchangerequest.message, "0", DateTime.Now);
                    MNComAndFocusOneLogsController comfocuslogs = new MNComAndFocusOneLogsController();

                    result = comfocuslogs.InsertIntoComFocusOne(comfocuslog);

                    if (result == "Success")
                    {
                        if (String.IsNullOrEmpty(pinchange.tid) || String.IsNullOrEmpty(pinchange.sc) || String.IsNullOrEmpty(pinchange.mobile)
                            || String.IsNullOrEmpty(pinchange.npin) || String.IsNullOrEmpty(pinchange.src))
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
                                    replyMessage.Response = "Your T-PIN has been successfully changed. ";
                                    replyMessage.ResponseStatus(HttpStatusCode.OK, replyMessage.Response);
                                }
                                else
                                {
                                    statusCode = "400";
                                    replyMessage.ResponseCode = "400";
                                    replyMessage.Response = "Invalid T-PIN";
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
                    replyMessage.Response = "Please Enter Correct Old T-PIN.";
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

        /*****END PIN RESET ******/


        /****PWD N PIN RESET *****/

        #region RESETPWDNPIN

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        public string PwdResetPin(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            string mobile = qs["mobile"];
            string clientcode = qs["clientCode"];
            string opwd = qs["opwd"];
            string npwd = qs["npwd"];
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

            if ((tid == "null") || (mobile == "null") || (opwd == "null") || (npwd == "null") ||
                (opin == "null") || (npin == "null") || (src == "null"))
            {
                statusCode = "400";
                message = "Parameters Missing/Invalid";
                failedmessage = message;
            }
            else
            {
                opwd = HashAlgo.Hash(opwd);
                npwd = HashAlgo.Hash(npwd);

                DataTable dtableResultPwd = PwdUtils.GetPwdInfo(mobile);
                if (dtableResultPwd != null)
                {
                    foreach (DataRow dtableUser in dtableResultPwd.Rows)
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
                    DataTable dtableResult = PinUtils.GetPinInfo(mobile);
                    if (dtableResult != null)
                    {
                        foreach (DataRow dtableUser in dtableResult.Rows)
                        {
                            String oldPwd = dtableUser["PIN"].ToString();
                            if (oldPwd.Equals(opin))
                            {
                                opin = dtableUser["PIN"].ToString();
                            }
                            else
                            {
                                opin = "";
                            }
                        }
                    }

                    if (opin != "")
                    {
                        var objFTModel = new UpdateFTUserModel();
                        var objFTInfo = new LoginAuth
                        {
                            ClientCode = clientcode,
                            TPin = npin,
                            Password = npwd
                        };
                        int id = objFTModel.UpdateFTInfo(objFTInfo);

                        if (id > 0)
                        {
                            statusCode = "200";
                            replyMessage.ResponseCode = "200";
                            replyMessage.Response = "Your Password/T-PIN has been successfully changed.";
                            replyMessage.ResponseStatus(HttpStatusCode.OK, replyMessage.Response);
                        }
                        else
                        {
                            statusCode = "400";
                            replyMessage.ResponseCode = "400";
                            replyMessage.Response = "Invalid Password/T-PIN";
                            replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                            failedmessage = replyMessage.Response;
                        }

                        /*
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

                        PinChange pinchange = new PinChange(tid, sc, mobile, opin, npin, src);
                        Message<PinChange> pinchangerequest = new Message<PinChange>(pinchange);
                        MNComAndFocusOneLog comfocuslogpin = new MNComAndFocusOneLog(tid, 0, pinchangerequest.message, "0", DateTime.Now);
                        MNComAndFocusOneLogsController comfocuslogspin = new MNComAndFocusOneLogsController();

                        result = comfocuslogspin.InsertIntoComFocusOne(comfocuslogpin);

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
                        */
                    }
                    else
                    {
                        statusCode = "400";
                        replyMessage.ResponseCode = "400";
                        replyMessage.Response = "Please Enter Correct Old T-PIN.";
                        replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
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

        /*****END PWD N PIN RESET ******/



        #region Reset Thaili PIN

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        public string ResetThailiPin(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            string mobile = qs["UserName"];
            string clientCode = qs["clientCode"];
            string name = qs["name"];

            ReplyMessage replyMessage = new ReplyMessage();
            string result = "";
            string statusCode = "200";
            string message = string.Empty;
            string failedmessage = string.Empty;

            TraceIdGenerator tig = new TraceIdGenerator();
            string tid = tig.GenerateTraceID();
            if ((mobile == null) || (clientCode == null))
            {
                statusCode = "400";
                message = "Parameters Missing/Invalid";
            }

            DataTable verifyUser = ResetPinUtils.GetExistUserName(clientCode, mobile); //checks if same username is typed
            if (verifyUser != null && verifyUser.Rows.Count > 0)
            {
                var getUser = verifyUser.Rows[0]["UserName"].ToString();
                if (getUser == mobile)
                {
                    int PIN = PinUtils.ResetPinInfo(mobile, clientCode);  //get new pin

                    string messagereply = "";
                    //if (PIN != null)
                    //{
                    messagereply = "Dear " + name.Split(' ').First() + "," + "\n";

                    messagereply += " You have successfully reset your T-PIN" + " on date " +
                                        DateTime.Now + ". And your new T-PIN is: " + PIN
                                    + "." + "\n";
                    messagereply += "Thank you. MNepal";

                    var client = new WebClient();

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

                    statusCode = "200";
                    var v = new
                    {
                        StatusCode = Convert.ToInt32(statusCode),
                        NewPin = PIN,
                        StatusMessage = "Success"
                    };

                    result = Newtonsoft.Json.JsonConvert.SerializeObject(v);
                }

                else
                {
                    statusCode = "400";
                    message = "Invalid Mobile Number";

                    var v = new
                    {
                        StatusCode = Convert.ToInt32(statusCode),
                        StatusMessage = message
                    };

                    result = Newtonsoft.Json.JsonConvert.SerializeObject(v);
                }
            }
            else
            {
                statusCode = "400";
                message = "Please enter your registered mobile number";

                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = message
                };

                result = Newtonsoft.Json.JsonConvert.SerializeObject(v);
            }

            return result;
            #endregion


        }
    }
}
