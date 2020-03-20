using MNepalProject.Controllers;
using MNepalProject.Models;
using System;
using System.IO;
using Newtonsoft.Json;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using WCF.MNepal.Models;
using System.Net;
using MNepalProject.Services;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Web;
using MNepalProject.DAL;
using System.Data;
using WCF.MNepal.Utilities;
using System.Collections.Generic;
using MNepalProject.Helper;
using WCF.MNepal.Helper;
using WCF.MNepal.ErrorMsg;
using System.Web.Script.Serialization;
using System.Text;
using System.Linq;

namespace WCF.MNepal
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
    public class query
    {
        //FOR NCELL
        string SMSNCELL = "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To=";
        //FOR NTC
        string SMSNTC = "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To=";

        [OperationContract]
        [WebInvoke(Method ="POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        public string authenticate(string uname, string pwd, string utype) ////public string authenticate(string tid, string UserName, string Password)Stream input) //
        {
            //StreamReader sr = new StreamReader(input);
            //string s = sr.ReadToEnd();
            //sr.Dispose();
            //NameValueCollection qs = HttpUtility.ParseQueryString(s);

            //string UserName = qs["UserName"];
            //string Password = qs["Password"];
            //string userType = qs["userType"];

            string UserName = uname;
            string Password = pwd;
            string userType = utype;

            ReplyMessage replyMessage = new ReplyMessage();

            string result = "";
            UserValidate uservalidate = new UserValidate(UserName, Password, userType.ToLower());
            Message<UserValidate> uservalidaterequest = new Message<UserValidate>(uservalidate);
            MNComAndFocusOneLog comfocuslog = new MNComAndFocusOneLog(0, uservalidaterequest.message, "0", DateTime.Now);
            MNComAndFocusOneLogsController comfocuslogs = new MNComAndFocusOneLogsController();

            //
            result = comfocuslogs.InsertIntoComFocusOne(comfocuslog);
            if (result == "Success")
            {
                if (String.IsNullOrEmpty(uservalidate.UserName) || String.IsNullOrEmpty(uservalidate.Password))
                {
                    replyMessage.Response = "parameters missing";

                    if (String.IsNullOrEmpty(uservalidate.UserName))
                    {
                        replyMessage.Response = "parameters missing";
                    }

                    if (String.IsNullOrEmpty(uservalidate.Password))
                    {
                        replyMessage.Response = "parameters missing";
                    }
                    if (String.IsNullOrEmpty(uservalidate.userType.ToLower()))
                    {
                        replyMessage.Response = "parameters missing";
                    }

                    replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                }
                else
                {
                    string ClientExtReply = "";
                    ClientExtReply = PassDataToMNClientExtController(uservalidate);
                    if (ClientExtReply == "true")
                    {
                        replyMessage.Response = "ok valid user";
                        replyMessage.ResponseStatus(HttpStatusCode.OK, replyMessage.Response);
                    }
                    else
                    {
                        replyMessage.Response = "Invalid User";
                        replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                    }
                }
            }
            else
            {
                replyMessage.Response = "request denied";
                replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, replyMessage.Response);
            }

            JObject jsonObject = new JObject();
            jsonObject["message"] = replyMessage.Response;
            return jsonObject["message"].ToString();
        }

        [OperationContract]
        [WebGet]
        public string balance(string tid, string sc, string mobile, string sa, string pin, string src)
        {
            string result = "";
            string message = string.Empty;
            string statusCode = string.Empty;
            string failedmessage = string.Empty;

            CustActivityModel custsmsInfo = new CustActivityModel();
            ReplyMessage replyMessage = new ReplyMessage();

            ErrorMessage em = new ErrorMessage();

            if ((tid == null) || (sc == null) || (mobile == null) || (sa == null) || (pin == null) || (src == null))
            {
                // throw ex
                statusCode = "400";
                replyMessage.Response = "Parameters Missing/Invalid";
                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, "Parameters Missing/Invalid");
                result = replyMessage.Response;
                message = replyMessage.Response;
            }
            else
            {
                //start: check customer status
                DataTable tbCustomerStatus = CustCheckUtils.GetCustBlockedUserInfo(mobile);
                if (tbCustomerStatus.Rows.Count == 1)
                {
                    if (((string)tbCustomerStatus.Rows[0]["Status"] == "Blocked") && ((string)tbCustomerStatus.Rows[0]["IsApproved"] == "Blocked"))
                    {
                        statusCode = "400";
                        replyMessage.Response = "Blocked your Account";
                        replyMessage.ResponseStatus(HttpStatusCode.BadRequest, "Blocked your Account");
                        result = replyMessage.Response;
                        message = replyMessage.Response;
                        var v = new
                        {
                            StatusCode = Convert.ToInt32(statusCode),
                            StatusMessage = message
                        };
                        result = JsonConvert.SerializeObject(v);
                        return result;
                    }
                    if (((string)tbCustomerStatus.Rows[0]["Status"] == "Blocked"))
                    {
                        statusCode = "400";
                        replyMessage.Response = "Your User has been blocked. Please contact your bank for further details";
                        replyMessage.ResponseStatus(HttpStatusCode.BadRequest, "Your User has been blocked. Please contact your bank for further details");
                        result = replyMessage.Response;
                        message = replyMessage.Response;
                        var v = new
                        {
                            StatusCode = Convert.ToInt32(statusCode),
                            StatusMessage = message
                        };
                        result = JsonConvert.SerializeObject(v);
                        return result;
                    }
                }
                //end: check customer status

                //start: checking trace id
                do
                {
                    TraceIdGenerator traceid = new TraceIdGenerator();
                    tid = traceid.GenerateUniqueTraceID();

                    bool traceIdCheck = false;
                    traceIdCheck = TraceIdCheck.IsValidTraceId(tid);
                    if (traceIdCheck == true)
                    {
                        result = "Trace ID Repeated";
                    }
                    else
                    {
                        result = "false";
                    }

                } while (result == "Trace ID Repeated");

               
                //End: TraceId
                //start:Com focus one log///
                BalanceQuery balanceQuery = new BalanceQuery(tid, sc, mobile, sa, pin, src);
                var comfocuslog = new MNComAndFocusOneLog(balanceQuery, DateTime.Now);
                var mncomfocuslog = new MNComAndFocusOneLogsController();
                mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
                //end:Com focus one log//

                //NOTE:- may be need to validate before insert into reply typpe
                //start:insert into reply type as HTTP//
                var replyType = new MNReplyType(balanceQuery.tid, "HTTP");
                var mnreplyType = new MNReplyTypesController();
                mnreplyType.InsertIntoReplyType(replyType);
                //end:insert into reply type as HTTP//


                //start:insert into transaction master//
                if (balanceQuery.valid())
                {
                    var transaction = new MNTransactionMaster(balanceQuery);
                    var mntransaction = new MNTransactionsController();
                    MNTransactionMaster validTransactionData = mntransaction.Validate(transaction, balanceQuery.pin);
                    result = validTransactionData.Response;

                    /*** ***/
                    if (validTransactionData.Response == "Error")
                    {
                        balanceQuery.Response = "error";
                        balanceQuery.ResponseStatus(HttpStatusCode.InternalServerError, "Internal server error - try again later, or contact support");
                        result = balanceQuery.Response;
                        statusCode = "500";
                        message = "Internal server error - try again later, or contact support";
                    }
                    else
                    {
                        if (result == "Invalid PIN")
                        {
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                            statusCode = "400";
                            message = result + " " + pin;
                        }
                        if ((result == "Limit Exceed")
                            || (result == "Invalid Source User") || (result == "Invalid Destination User")
                            || (result == "Invalid Product Request") || (result == "")
                            || (result == "Error in ResponeCode:Data Not Available")) //(result == "Trace ID Repeated") ||
                        {
                            
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                            statusCode = "400";
                            message = result;
                        }
                        if (result == "111")
                        {
                            statusCode = result;
                            message = em.Error_111 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "114")
                        {
                            statusCode = result;
                            message = em.Error_114 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "115")
                        {
                            statusCode = result;
                            message = em.Error_115 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "116")
                        {
                            statusCode = result;
                            message = em.Error_116 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "119")
                        {
                            statusCode = result;
                            message = em.Error_119 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "121")
                        {
                            statusCode = result;
                            message = em.Error_121 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "163")
                        {
                            statusCode = result;
                            message = em.Error_163 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "180")
                        {
                            statusCode = result;
                            message = em.Error_180 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "181")
                        {
                            statusCode = result;
                            message = em.Error_181 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "182")
                        {
                            statusCode = result;
                            message = em.Error_182 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "183")
                        {
                            statusCode = result;
                            message = em.Error_183 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "184")
                        {
                            statusCode = result;
                            message = em.Error_184 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "185")
                        {
                            statusCode = result;
                            message = em.Error_185 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "186")
                        {
                            statusCode = result;
                            message = em.Error_186 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "187")
                        {
                            statusCode = result;
                            message = em.Error_187 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "188")
                        {
                            statusCode = result;
                            message = em.Error_188 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "189")
                        {
                            statusCode = result;
                            message = em.Error_189 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "190")
                        {
                            statusCode = result;
                            message = em.Error_190 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "800")
                        {
                            statusCode = result;
                            message = em.Error_800 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "902")
                        {
                            statusCode = result;
                            message = em.Error_902 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "904")
                        {
                            statusCode = result;
                            message = em.Error_904 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "906")
                        {
                            statusCode = result;
                            message = em.Error_906 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "907")
                        {
                            statusCode = result;
                            message = em.Error_907 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "909")
                        {
                            statusCode = result;
                            message = em.Error_909 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "911")
                        {
                            statusCode = result;
                            message = em.Error_911 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "913")
                        {
                            statusCode = result;
                            message = em.Error_913 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "90")
                        {
                            statusCode = result;
                            message = em.Error_90 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "91")
                        {
                            statusCode = result;
                            message = em.Error_91 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "92")
                        {
                            statusCode = result;
                            message = em.Error_92 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "94")
                        {
                            statusCode = result;
                            message = em.Error_94 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "95")
                        {
                            statusCode = result;
                            message = em.Error_95 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "98")
                        {
                            statusCode = result;
                            message = em.Error_98 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        if (result == "99")
                        {
                            statusCode = result;
                            message = em.Error_99 + " " + result;
                            failedmessage = message;
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result);
                        }
                        else if (statusCode == "")
                        {
                            //string SyncTime = "0";
                            //DataTable dtSyncTime = LoginUtils.GetSyncInfo(mobile, tid);
                            //if (dtSyncTime.Rows.Count > 0)
                            //{
                            //    SyncTime = dtSyncTime.Rows[0]["SyncTime"].ToString();
                            //}

                            //string removeno = result.Substring(0, 1);
                            //result = result.Remove(Convert.ToInt32(removeno), 1);
                            
                            SaveSyncLogInfo(mobile, tid);
                            //Remove Last (.)
                            result = result.TrimEnd('.');

                            balanceQuery.ResponseStatus(HttpStatusCode.OK, result); //200 - OK
                            statusCode = "200";
                            message = result;// + SyncTime
                        }

                    }
                    /*** ***/

                }
                else
                {
                    statusCode = "400";
                    balanceQuery.Response = "error";
                    balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, "parameters missing/invalid");
                    result = balanceQuery.Response;
                }
                //end:insert into transaction master//
            }

            OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string transactiontype = "";
                if (sc == "20")
                {
                    transactiontype = "wallet";
                }
                else if (sc == "22")
                {
                    transactiontype = "bank";
                }
                //20-wallet
                //22-bank

                try
                {
                    string messagereply = "Dear Customer," + "\n";
                    messagereply += "Your balance on " + transactiontype + " account is NPR " + result + "\n";
                    messagereply += "-MNepal";

                    //var client = new WebClient();
                    ////var content = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=2&Password=test12test&From=9797&To=" + "977" + mobile + "&Text=" + messagereply + "");
                    ////var content = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To=" + "977" + mobile + "&Text=" + messagereply + "");

                    //if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                    //{
                    //    //FOR NCELL
                    //    var content = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                    //    + "977" + mobile + "&Text=" + messagereply + "");
                    //}
                    //else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                    //    || (mobile.Substring(0, 3) == "986")) //FOR NTC
                    //{
                    //    //FOR NTC
                    //    var content = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                    //        + "977" + mobile + "&Text=" + messagereply + "");
                    //}

                    custsmsInfo = new CustActivityModel()
                    {
                        UserName = mobile,
                        RequestMerchant = "Balance Query",
                        DestinationNo = "",
                        Amount = "",
                        SMSStatus = "Success",
                        SMSSenderReply = "Successfully Check the balance.",
                        ErrorMessage = "",
                    };
                }
                catch (Exception ex)
                {
                    // throw ex
                    statusCode = "400";
                    message = ex.Message;
                }
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                custsmsInfo = new CustActivityModel()
                {
                    UserName = mobile,
                    RequestMerchant = "Balance Query",
                    DestinationNo = "",
                    Amount = "",
                    SMSStatus = "Failed",
                    SMSSenderReply = message,
                    ErrorMessage = "",
                };

                //var v = new
                //{
                //    StatusCode = Convert.ToInt32(400),
                //    StatusMessage = result
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

            if (statusCode != "200")
            {
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = message
                };
                result = JsonConvert.SerializeObject(v);
            }
            else {
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = message
                };
                result = JsonConvert.SerializeObject(v);
            }

            return result;
        }

        #region MiniStatement

        [OperationContract]
        [WebGet]
        public string Statement(string tid, string sc, string mobile, string sa, string pin, string src)
        {
            string result = "";
            //start: checking trace id
            do
            {
                TraceIdGenerator traceid = new TraceIdGenerator();
                tid = traceid.GenerateUniqueTraceID();

                bool traceIdCheck = false;
                traceIdCheck = TraceIdCheck.IsValidTraceId(tid);
                if (traceIdCheck == true)
                {
                    result = "Trace ID Repeated";
                }
                else
                {
                    result = "false";
                }

            } while (result == "Trace ID Repeated");

            //End: TraceId
            //start:Com focus one log///
            BalanceQuery balanceQuery = new BalanceQuery(tid, sc, mobile, sa, pin, src);
            var comfocuslog = new MNComAndFocusOneLog(balanceQuery, DateTime.Now);
            var mncomfocuslog = new MNComAndFocusOneLogsController();
            mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
            //end:Com focus one log//

            //NOTE:- may be need to validate before insert into reply typpe
            //start:insert into reply type as HTTP//
            var replyType = new MNReplyType(balanceQuery.tid, "HTTP");
            var mnreplyType = new MNReplyTypesController();
            mnreplyType.InsertIntoReplyType(replyType);
            //end:insert into reply type as HTTP//


            //start:insert into transaction master//
            if (balanceQuery.valid())
            {
                var transaction = new MNTransactionMaster(balanceQuery);
                var mntransaction = new MNTransactionsController();
                MNTransactionMaster validTransactionData = mntransaction.Validate(transaction, balanceQuery.pin);
                result = validTransactionData.Response;
            }
            else
            {
                balanceQuery.Response = "error";
                balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, "parameters missing/invalid");
                result = balanceQuery.Response;
            }
            //end:insert into transaction master//
            return result;//Dear......
        }

        private string rearrange(string ministatement_row)
        {
            string processed_row = "";
            string processed_row1 = ministatement_row.Substring(0, 6);
            processed_row += processed_row1;
            string processed_row2 = ministatement_row.Substring(6, 12).Trim();

            string mytemp = processed_row2.TrimStart('0');
            var newString = mytemp.PadLeft(15, '.');
            processed_row += newString;
            string processed_row3 = ministatement_row.Substring(18, 1).Trim();
            var newString1 = processed_row3.PadLeft(5, '.');
            processed_row += newString1;
            return processed_row;
        }

        private string LookINTOResponseTableForMiniStatement(string tid)
        {
            string result = "";
            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;
            var newString = tid.PadLeft(6, '0');
            string sql = "select MiniStmntRec as MiniStmntRec from MNResponse where TraceNo = '" + newString + "'";
            try
            {
                using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(conStr))
                {
                    con.Open();
                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, con))
                    using (System.Data.SqlClient.SqlDataReader rdr = command.ExecuteReader())
                        while (rdr.Read())
                        {
                            result = rdr["MiniStmntRec"].ToString();
                        }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                result = ex.ToString();
            }
            return result;
        }

        private string GetMiniStatement(BalanceQuery balanceQuery)
        {
            string result = "";
            string bin = "0000";
            string SourceAccountNo = "";

            if (balanceQuery.sc == "21")
            {
                if (balanceQuery.sa != "")
                {
                    SourceAccountNo = balanceQuery.sa;
                }
                else
                {
                    //SourceAccountNo = GetSourceBankAccount(balanceQuery.mobile);  //bank account number 
                    SourceAccountNo = GetSourceWalletAccount(balanceQuery.mobile);  //wallet number 
                }

                //bin = getBIN(SourceAccountNo);
            }

            result = insertintoPumoriIN(balanceQuery.sc, bin, balanceQuery.mobile, balanceQuery.tid, SourceAccountNo);
            return result;
        }

        #endregion

        #region Account Info

        [OperationContract]
        [WebGet]
        public string account(string tid, string sc, string mobile, string sa, string pin, string src)
        {

            AccountInfo accinfo = new AccountInfo();
            accinfo.tid = tid;
            accinfo.sc = sc;
            accinfo.mobile = mobile;
            accinfo.sa = sa;
            accinfo.pin = pin;
            accinfo.src = src;

            string result = string.Empty;
            do
            {
                TraceIdGenerator traceid = new TraceIdGenerator();
                tid = traceid.GenerateUniqueTraceID();

                bool traceIdCheck = false;
                traceIdCheck = TraceIdCheck.IsValidTraceId(tid);
                if (traceIdCheck == true)
                {
                    result = "Trace ID Repeated";
                }
                else
                {
                    result = "false";
                }

            } while (result == "Trace ID Repeated");
            //start:Com focus one log///
            Message<AccountInfo> accInforequest = new Message<AccountInfo>(accinfo);
            MNComAndFocusOneLog comfocuslog = new MNComAndFocusOneLog(tid, 0, accInforequest.message, "0", DateTime.Now);
            MNComAndFocusOneLogsController comfocuslogs = new MNComAndFocusOneLogsController();

            ReplyMessage replyMessage = new ReplyMessage();

            string checkTid = IsTraceIDUnique(tid);
            if (checkTid == "true")
            {
                result = comfocuslogs.InsertIntoComFocusOne(comfocuslog);
                //end:Com focus one log//

                if (result == "Success")
                {
                    if (String.IsNullOrEmpty(tid) || String.IsNullOrEmpty(sc) || String.IsNullOrEmpty(mobile) || String.IsNullOrEmpty(pin))
                    {
                        replyMessage.Response = "Parameter Missing:Please Try Again!";
                        replyMessage.ResponseCode = HttpStatusCode.BadRequest.ToString();
                        replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                    }
                    else
                    {
                        if (sc == "63")  //Wallet Account Info
                        {
                            MNClientContact userdetails = new MNClientContact();
                            userdetails.ContactNumber1 = mobile;
                            ClientsDetails clientdetails = new ClientsDetails(userdetails);

                            if (clientdetails.clientExt != null)
                            {
                                //MNAccountInfo userAccInfo = new MNAccountInfo();
                                //MNAccountInfosController defAcccountInfo = new MNAccountInfosController();
                                //userAccInfo = defAcccountInfo.GetDefaultWallet(clientdetails.client);

                                MNClient agentinfo = new MNClient();

                                if (clientdetails.client.ClientCode != "" && clientdetails.clientExt.PIN == pin)
                                {
                                    if (clientdetails.accountInfo.AgentId != 0)
                                    {
                                        MNAgent agent = new MNAgent();
                                        agent.ID = clientdetails.accountInfo.AgentId; //Set ID to Get all Details of Agent

                                        AgentDetails getAgentdetails = new AgentDetails();
                                        agentinfo = getAgentdetails.AgentDetailsByAgentDetailsFromAgentId(agent);
                                    }
                                    else
                                    {
                                        agentinfo.Name = "Agent Not Available";
                                    }

                                    string BankName;
                                    string BankAcc;
                                    string maskBankAc;
                                    if (clientdetails.bankAccountMap != null)
                                    {
                                        MNBankTable getBank = new MNBankTable();
                                        MNBankTablesController bank = new MNBankTablesController();
                                        getBank = bank.GetBankName(clientdetails.bankAccountMap.BIN);
                                        BankName = getBank.BankName;
                                        //BankAcc = clientdetails.bankAccountMap.BankAccountNumber;
                                        getBank = bank.GetBankAcc(clientdetails.client.ClientCode);
                                        BankAcc = getBank.BankAccountNumber;

                                        var cardNumber = BankAcc;

                                        var firstDigits = cardNumber.Substring(0, 3);
                                        var lastDigits = cardNumber.Substring(cardNumber.Length - 3, 3);

                                        var requiredMask = new String('X', cardNumber.Length - firstDigits.Length - lastDigits.Length);

                                        maskBankAc = string.Concat(firstDigits, requiredMask, lastDigits);
                                        getBank.MaskedBankAcNumber = maskBankAc;
                                        clientdetails.bankAccountMap.BankAccountNumber = getBank.MaskedBankAcNumber;
                                    }
                                    else
                                    {
                                        BankName = "Bank Not Available";
                                        BankAcc = "Bank Account Not Available";
                                    }

                                    if (clientdetails.accountInfo.WalletNumber != "" || clientdetails.bankAccountMap.BankAccountNumber != "")
                                    {
                                        JObject o = new JObject();
                                        o["ClientCode"] = clientdetails.accountInfo.ClientCode;
                                        o["ClientName"] = clientdetails.client.Name;
                                        o["Address"] = clientdetails.client.Address;
                                        o["Status"] = clientdetails.client.Status;
                                        o["DefaultWalletNumber"] = clientdetails.accountInfo.WalletNumber;
                                        //DefaultBankAccount = BankAcc,
                                        o["DefaultBankAccount"] = clientdetails.bankAccountMap.BankAccountNumber;
                                        o["BankName"] = BankName;
                                        o["AgentName"] = agentinfo.Name;
                                        string json = JsonConvert.SerializeObject(o);
                                        replyMessage.Response = json;
                                        replyMessage.ResponseStatus(HttpStatusCode.OK, "Account Info Found");
                                    }
                                    else
                                    {
                                        replyMessage.Response = "Wallet Account Info Not Found";
                                        replyMessage.ResponseCode = HttpStatusCode.Unauthorized.ToString();
                                        replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, "Account Info Not Found");
                                    }
                                }
                                else
                                {
                                    replyMessage.Response = "Incorrect PIN";
                                    replyMessage.ResponseCode = HttpStatusCode.Unauthorized.ToString();
                                    replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, "Data Mismatch");
                                }
                            }
                            else
                            {
                                replyMessage.Response = "Client Not Found";
                                replyMessage.ResponseCode = "90";
                                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, "Client Not Found");
                            }
                        }
                        else
                        {
                            replyMessage.Response = "Invalid Service Code";
                            replyMessage.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            replyMessage.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Service Code");
                        }
                    }
                    //else if(sc == "64")
                    //{
                    //    MNClientContact userdetails = new MNClientContact();
                    //    userdetails.ContactNumber1 = mobile;
                    //    ClientsDetails clientdetails = new ClientsDetails(userdetails);

                    //    if (clientdetails.client.ClientCode != "")
                    //    {
                    //        MNBankAccountMap userBankInfo = new MNBankAccountMap();
                    //        MNBankAccountMapsController defBankAcccountInfo = new MNBankAccountMapsController();
                    //        userBankInfo = defBankAcccountInfo.GetDefaultBankAccount(clientdetails.client);

                    //        if (userBankInfo.BankAccountNumber != "")
                    //        {

                    //            replyMessage.ResponseStatus(HttpStatusCode.OK, replyMessage.Response);
                    //        }
                    //        else
                    //        {
                    //            replyMessage.Response = "Bank Account Info Not Found";
                    //            replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                    //        }
                    //    }
                    //}

                }
                else
                {
                    replyMessage.Response = "request denied";
                    replyMessage.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                    replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, replyMessage.Response);
                }
            }
            else
            {
                replyMessage.Response = "TraceID Repeated";
                replyMessage.ResponseCode = HttpStatusCode.BadRequest.ToString();
                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, "TraceID Repeated");
            }
            return replyMessage.Response;

        }

        #endregion

        #region Customer Kyc Detail

        [OperationContract]
        [WebGet]
        public string CustomerKycDetail(string tid, string sc, string mobile, string sa, string pin, string src)
        {
            ReplyMessage replyMessage = new ReplyMessage();
            try { 
            AccountInfo accinfo = new AccountInfo();
            accinfo.tid = tid;
            accinfo.sc = sc;
            accinfo.mobile = mobile;
            accinfo.sa = sa;
            accinfo.pin = pin;
            accinfo.src = src;

            string result = string.Empty;

            TraceIdGenerator traceid = new TraceIdGenerator();
            tid = traceid.GenerateUniqueTraceID();//GenerateUTraceID();

            ////start:Com focus one log///
            //Message<AccountInfo> accInforequest = new Message<AccountInfo>(accinfo);
            //MNComAndFocusOneLog comfocuslog = new MNComAndFocusOneLog(tid, 0, accInforequest.message, "0", DateTime.Now);
            //MNComAndFocusOneLogsController comfocuslogs = new MNComAndFocusOneLogsController();


            string checkTid = IsTraceIDUnique(tid);
            if (checkTid == "true")
            {
                // result = comfocuslogs.InsertIntoComFocusOne(comfocuslog);
                //end:Com focus one log//

                //if (result == "Success")
                //{
                if (String.IsNullOrEmpty(tid) || String.IsNullOrEmpty(sc) || String.IsNullOrEmpty(mobile) || String.IsNullOrEmpty(pin))
                {
                    replyMessage.Response = "Parameter Missing:Please Try Again!";
                    replyMessage.ResponseCode = HttpStatusCode.BadRequest.ToString();
                    replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                }
                else
                {
                    if (sc == "63")  //Wallet Account Info
                    {
                        MNClient mnClient = getMNCLientCodeByNumber(mobile);
                        if (mnClient.PIN == pin)
                        {
                            MNClientKyc clientKyc = getMNCLientKyc(mnClient.ClientCode);
                            ClientKYCDoc clientKYCDoc = getMNCLientKycDoc(mnClient.ClientCode);
                            string address = getAddress(mnClient.ClientCode);
                            string permanentAddress = getPermanentAddress(mnClient.ClientCode);
                            string BankAccountNumber = getBankAccountFromMobile(mobile);
                            JObject o = new JObject();
                            o["ClientCode"] = mnClient.ClientCode;
                            o["ClientName"] = mnClient.Name;
                            o["Address"] = address;
                            o["PermanentAddress"] = permanentAddress;
                            o["Status"] = mnClient.Status;
                            o["CustStatus"] = clientKyc.CustStatus;
                            o["BankAccountNumber"] = BankAccountNumber;

                            if (clientKyc.FathersName != null)
                            {
                                o["FathersName"] = clientKyc.FathersName;
                                o["GFathersName"] = clientKyc.GFathersName;
                                o["DateOfBirth"] = clientKyc.DateOfBirth;
                                o["Gender"] = clientKyc.Gender;
                                o["DocType"] = clientKYCDoc.DocType;
                                o["FrontImage"] = clientKYCDoc.FrontImage;
                                o["BackImage"] = clientKYCDoc.BackImage;
                                o["PassportImage"] = clientKYCDoc.PassportImage;
                            }
                            else
                            {
                                o["FathersName"] = "";
                                o["GFathersName"] = "";
                                o["DateOfBirth"] = "";
                                o["Gender"] = "";
                                o["DocType"] = "";
                                o["FrontImage"] = "";
                                o["BackImage"] = "";
                                o["PassportImage"] = "";
                            }

                            string json = JsonConvert.SerializeObject(o);
                            replyMessage.Response = json;
                            replyMessage.ResponseStatus(HttpStatusCode.OK, "Account Info Found");
                        }
                        else
                        {
                            replyMessage.Response = "Invalid PIN " + pin;
                            replyMessage.ResponseCode = HttpStatusCode.Unauthorized.ToString();
                            replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, "Account Info Not Found");
                        }
                    }
                    else
                    {
                        replyMessage.Response = "Invalid Service Code";
                        replyMessage.ResponseCode = HttpStatusCode.BadRequest.ToString();
                        replyMessage.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Service Code");
                    }
                }


                //}
                //else
                //{
                //    replyMessage.Response = "request denied";
                //    replyMessage.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                //    replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, replyMessage.Response);
                //}
            }
            else
            {
                replyMessage.Response = "TraceID Repeated";
                replyMessage.ResponseCode = HttpStatusCode.BadRequest.ToString();
                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, "TraceID Repeated");
            }
            }
            catch(Exception e) {
                
                replyMessage.Response = "Error";
                replyMessage.ResponseCode = HttpStatusCode.BadRequest.ToString();
                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, "TraceID Repeated");
            }
            return replyMessage.Response;

        }
        private string getBankAccountFromMobile(string contactNumber)
        {
            string BankAccountNumber = "";
            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;

            string sql = "SELECT * FROM dbo.v_MNClientDetail WHERE ContactNumber1 ='" + contactNumber + "' AND HasBankKYC='T'";
            try
            {
                using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(conStr))
                {
                    con.Open();
                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, con))
                    using (System.Data.SqlClient.SqlDataReader rdr = command.ExecuteReader())
                        while (rdr.Read())
                        {
                            BankAccountNumber = rdr["BankAccountNumber"].ToString();

                            //string CDistrict = getDistrictName(rdr["CDistrictID"].ToString());
                            //string CStreet = rdr["CStreet"].ToString();
                            //string CProvince = rdr["CProvince"].ToString();
                            //mnclient.Address = CStreet + "," + " " + CDistrict + "," + " " + "Province No. " + CProvince;
                        }
                    con.Close();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return BankAccountNumber;
        }
        #endregion
        /// <summary>
        /// ////////////
        /// </summary>
        /// <param name="tid"></param>
        /// <param name="sc"></param>
        /// <param name="mobile"></param>
        /// <param name="sa"></param>
        /// <param name="pin"></param>
        /// <param name="src"></param>
        /// <returns></returns>
        /// 
        #region Customer Kyc Detail

        [OperationContract]
        [WebGet]
        public string MerchantKycDetail(string tid, string sc, string mobile, string sa, string pin, string src)
        {
            ReplyMessage replyMessage = new ReplyMessage();
            try
            {
                AccountInfo accinfo = new AccountInfo();
                accinfo.tid = tid;
                accinfo.sc = sc;
                accinfo.mobile = mobile;
                accinfo.sa = sa;
                accinfo.pin = pin;
                accinfo.src = src;

                string result = string.Empty;

                TraceIdGenerator traceid = new TraceIdGenerator();
                tid = traceid.GenerateUniqueTraceID();//GenerateUTraceID();

                ////start:Com focus one log///
                //Message<AccountInfo> accInforequest = new Message<AccountInfo>(accinfo);
                //MNComAndFocusOneLog comfocuslog = new MNComAndFocusOneLog(tid, 0, accInforequest.message, "0", DateTime.Now);
                //MNComAndFocusOneLogsController comfocuslogs = new MNComAndFocusOneLogsController();


                string checkTid = IsTraceIDUnique(tid);
                if (checkTid == "true")
                {
                    // result = comfocuslogs.InsertIntoComFocusOne(comfocuslog);
                    //end:Com focus one log//

                    //if (result == "Success")
                    //{
                    if (String.IsNullOrEmpty(tid) || String.IsNullOrEmpty(sc) || String.IsNullOrEmpty(mobile) || String.IsNullOrEmpty(pin))
                    {
                        replyMessage.Response = "Parameter Missing:Please Try Again!";
                        replyMessage.ResponseCode = HttpStatusCode.BadRequest.ToString();
                        replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                    }
                    else
                    {
                        if (sc == "63")  //Wallet Account Info
                        {
                            MNClient mnClient = getMNCLientCodeByNumber(mobile);
                            if (mnClient.PIN == pin)
                            {
                                MNClientKyc clientKyc = getMNCLientKyc(mnClient.ClientCode);
                                ClientKYCDoc clientKYCDoc = getMNMerchantKycDoc(mnClient.ClientCode);
                                string address = getAddress(mnClient.ClientCode);
                                string permanentAddress = getPermanentAddress(mnClient.ClientCode);
                                string BankAccountNumber = getBankAccountFromMobile(mobile);
                                JObject o = new JObject();
                                o["ClientCode"] = mnClient.ClientCode;
                                o["ClientName"] = mnClient.Name;
                                o["Address"] = address;
                                o["PermanentAddress"] = permanentAddress;
                                o["Status"] = mnClient.Status;
                                o["CustStatus"] = clientKyc.CustStatus;
                                o["BankAccountNumber"] = BankAccountNumber;

                                if (clientKyc.FathersName != null)
                                {
                                    o["FathersName"] = clientKyc.FathersName;
                                    o["GFathersName"] = clientKyc.GFathersName;
                                    o["DateOfBirth"] = clientKyc.DateOfBirth;
                                    o["Gender"] = clientKyc.Gender;
                                    o["FrontImage"] = clientKYCDoc.FrontImage;
                                    o["BackImage"] = clientKYCDoc.BackImage;
                                    o["PassportImage"] = clientKYCDoc.PassportImage;
                                }
                                else
                                {
                                    o["FathersName"] = "";
                                    o["GFathersName"] = "";
                                    o["DateOfBirth"] = "";
                                    o["Gender"] = "";
                                    o["FrontImage"] = "";
                                    o["BackImage"] = "";
                                    o["PassportImage"] = "";
                                }

                                string json = JsonConvert.SerializeObject(o);
                                replyMessage.Response = json;
                                replyMessage.ResponseStatus(HttpStatusCode.OK, "Account Info Found");
                            }
                            else
                            {
                                replyMessage.Response = "Invalid PIN " + pin;
                                replyMessage.ResponseCode = HttpStatusCode.Unauthorized.ToString();
                                replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, "Account Info Not Found");
                            }
                        }
                        else
                        {
                            replyMessage.Response = "Invalid Service Code";
                            replyMessage.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            replyMessage.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Service Code");
                        }
                    }


                    //}
                    //else
                    //{
                    //    replyMessage.Response = "request denied";
                    //    replyMessage.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                    //    replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, replyMessage.Response);
                    //}
                }
                else
                {
                    replyMessage.Response = "TraceID Repeated";
                    replyMessage.ResponseCode = HttpStatusCode.BadRequest.ToString();
                    replyMessage.ResponseStatus(HttpStatusCode.BadRequest, "TraceID Repeated");
                }
            }
            catch (Exception e)
            {

                replyMessage.Response = "Error";
                replyMessage.ResponseCode = HttpStatusCode.BadRequest.ToString();
                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, "TraceID Repeated");
            }
            return replyMessage.Response;

        }
        #endregion



        [OperationContract]
        [WebGet]
        public string CustomerProfilePhoto(string tid, string sc, string mobile, string sa, string pin, string src)
        {

            string result = string.Empty;
            ReplyMessage replyMessage = new ReplyMessage();

            string checkTid = IsTraceIDUnique(tid);
            if (checkTid == "true")
            {

                                MNClient mnClient = getMNCLientCodeByNumber(mobile);
                                MNClientKyc clientKyc = getMNCLientKyc(mnClient.ClientCode);
                                ClientKYCDoc clientKYCDoc = getMNCLientKycDoc(mnClient.ClientCode);
                                JObject o = new JObject();
                if (clientKyc.CustStatus==null) {
                    clientKyc.CustStatus = "";
                }
                o["CustStatus"] = clientKyc.CustStatus;
                                    
                                if (clientKyc.FathersName != null)
                                {
                                   
                                    o["PassportImage"] = clientKYCDoc.PassportImage;
                                   
                                }
                                else
                                {
                                   
                                    o["PassportImage"] = "";
                                
                                }
                                string json = JsonConvert.SerializeObject(o);
                                replyMessage.Response = json;
                                replyMessage.ResponseStatus(HttpStatusCode.OK, "Account Info Found");
                            
            }
            else
            {
                replyMessage.Response = "TraceID Repeated";
                replyMessage.ResponseCode = HttpStatusCode.BadRequest.ToString();
                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, "TraceID Repeated");
            }
            return replyMessage.Response;
        }

        #region Change Default Account

        [OperationContract]
        [WebInvoke]
        public string changedefault(Stream input)
        {
            string tid = string.Empty;
            string sc = string.Empty;
            string mobile = string.Empty;
            string da = string.Empty;
            string pin = string.Empty;
            string src = string.Empty;
            string message = tid + sc + mobile + da + pin + src;

            ReplyMessage replyMessage = new ReplyMessage();
            replyMessage.Response = "Default Account will be Changed Soon. Thank You!";
            replyMessage.ResponseStatus(HttpStatusCode.OK, replyMessage.Response);

            JObject jsonObject = new JObject();
            jsonObject["message"] = replyMessage.Response;
            return jsonObject["message"].ToString();


            //string JSONresult = JsonConvert.SerializeObject(replyMessage);
            // return JSONresult;

        }

        #endregion

        #region ChangePIN

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json)]
        public string pin(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            string tid = qs["tid"];
            string sc = qs["sc"];
            string mobile = qs["mobile"];
            string pin = qs["pin"];
            string npin = qs["npin"];
            string src = qs["src"];

            ReplyMessage replyMessage = new ReplyMessage();

            string result = "";
            PinChange pinchange = new PinChange(tid, sc, mobile, pin, npin, src);
            Message<PinChange> pinchangerequest = new Message<PinChange>(pinchange);
            MNComAndFocusOneLog comfocuslog = new MNComAndFocusOneLog(tid, 0, pinchangerequest.message, "0", DateTime.Now);
            MNComAndFocusOneLogsController comfocuslogs = new MNComAndFocusOneLogsController();

            result = comfocuslogs.InsertIntoComFocusOne(comfocuslog);
            if (result == "Success")
            {
                if (String.IsNullOrEmpty(pinchange.tid) || String.IsNullOrEmpty(pinchange.sc) || String.IsNullOrEmpty(pinchange.mobile) || String.IsNullOrEmpty(pinchange.pin) || String.IsNullOrEmpty(pinchange.npin) || String.IsNullOrEmpty(pinchange.src))
                {
                    replyMessage.Response = "parameters missing";

                    replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                }
                else
                {
                    if (sc == "62")
                    {
                        List<PinChange> results = new List<PinChange>();
                        var resultList = new List<PinChange>();

                        DataTable dtableResult = PinUtils.GetPinInfo(mobile);
                        if (dtableResult != null)
                        {
                            foreach (DataRow dtableUser in dtableResult.Rows)
                            {
                                String oldPin = dtableUser["PIN"].ToString();
                                if (oldPin.Equals(pin))
                                {
                                    pin = dtableUser["PIN"].ToString();
                                }
                                else
                                {
                                    pin = "";
                                }
                            }
                        }

                        if (pin != "")
                        {

                            string PINChangedReply = "";
                            Pin changepin = new Pin();
                            PINChangedReply = changepin.ChangePIN(mobile, pin, npin);

                            if (PINChangedReply == "true")
                            {
                                replyMessage.Response = "PIN change successful";
                                replyMessage.ResponseStatus(HttpStatusCode.OK, replyMessage.Response);
                            }
                            else
                            {
                                replyMessage.Response = "Invalid PIN";
                                replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                            }
                        }
                        else
                        {
                            replyMessage.Response = "Please Enter Correct Old PIN.";
                            replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                        }
                    }

                    else
                    {
                        replyMessage.Response = "Invalid Service Code";
                        replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                    }
                }
            }
            else
            {
                replyMessage.Response = "request denied";
                replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, replyMessage.Response);
            }

            /*
            JObject jsonObject = new JObject();
            jsonObject["message"] = replyMessage.ResponseStatus;
            return jsonObject["message"].ToString();
            ReplyMessage replyMessage = new ReplyMessage();
            replyMessage.Response ="PIN will be Changed Soon. Thank You!";
            replyMessage.ResponseStatus(HttpStatusCode.OK, replyMessage.Response);

            JObject jsonObject = new JObject();
            jsonObject["message"] = replyMessage.ResponseStatus;
            return jsonObject["message"].ToString();
            */
            return replyMessage.Response;

        }

        #endregion

        #region getCustomerFullName
        [OperationContract]
        [WebGet]
        public string GetCustomerFullName(string tid, string sc, string mobile, string sa, string pin, string src)
        {

            string result = string.Empty;
            ReplyMessage replyMessage = new ReplyMessage();

            string checkTid = IsTraceIDUnique(tid);
            if (checkTid == "true")
            {

                MNClient mnClient = getMNCLientCodeByNumber(mobile);
                if ((mnClient.HasKYC != null))
                {
                    if (mnClient.HasKYC.Equals("F"))
                    {
                        MNClientKyc clientKyc = getMNCLientKycName(mnClient.ClientCode);

                        JObject o = new JObject();

                        o["FName"] = clientKyc.FName;
                        o["MName"] = clientKyc.MName;
                        o["LName"] = clientKyc.LName;


                        string json = JsonConvert.SerializeObject(o);
                        replyMessage.Response = json;
                        replyMessage.ResponseStatus(HttpStatusCode.OK, "Account Info Found");

                    }
                    else
                    {
                        replyMessage.Response = "Customer KYC is done";
                        replyMessage.ResponseCode = HttpStatusCode.BadRequest.ToString();
                        replyMessage.ResponseStatus(HttpStatusCode.BadRequest, "Customer KYC is done");
                    }
                }
                else
                {
                    replyMessage.Response = "Account not found";
                    replyMessage.ResponseCode = HttpStatusCode.BadRequest.ToString();
                    replyMessage.ResponseStatus(HttpStatusCode.BadRequest, "Customer KYC is done");
                }
            }
            else
            {
                replyMessage.Response = "TraceID Repeated";
                replyMessage.ResponseCode = HttpStatusCode.BadRequest.ToString();
                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, "TraceID Repeated");

            }

            return replyMessage.Response;
        }
        private MNClientKyc getMNCLientKycName(string clientcode)
        {
            MNClientKyc mnclientKyc = new MNClientKyc();
            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;

            string sql = "SELECT * FROM MNClientKYC where ClientCode = '" + clientcode + "'";
            try
            {
                using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(conStr))
                {
                    con.Open();
                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, con))
                    using (System.Data.SqlClient.SqlDataReader rdr = command.ExecuteReader())
                        while (rdr.Read())
                        {
                            mnclientKyc.FName = rdr["FName"].ToString();
                            mnclientKyc.MName = rdr["MName"].ToString();
                            mnclientKyc.LName = rdr["LName"].ToString();

                        }
                    con.Close();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return mnclientKyc;
        }
        #endregion

        #region ChangePwd

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json)]
        public string password(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            string tid = qs["tid"];
            string sc = qs["sc"];
            string mobile = qs["mobile"];
            string opwd = qs["opwd"];
            string npwd = qs["npwd"];
            string src = qs["src"];

            opwd = HashAlgo.Hash(opwd);
            npwd = HashAlgo.Hash(npwd);

            ReplyMessage replyMessage = new ReplyMessage();

            string result = "";
            PwdChange pwdchange = new PwdChange(tid, sc, mobile, opwd, npwd, src);
            Message<PwdChange> pwdchangerequest = new Message<PwdChange>(pwdchange);
            MNComAndFocusOneLog comfocuslog = new MNComAndFocusOneLog(tid, 0, pwdchangerequest.message, "0", DateTime.Now);
            MNComAndFocusOneLogsController comfocuslogs = new MNComAndFocusOneLogsController();

            result = comfocuslogs.InsertIntoComFocusOne(comfocuslog);
            if (result == "Success")
            {
                if (String.IsNullOrEmpty(pwdchange.tid) || String.IsNullOrEmpty(pwdchange.sc) || String.IsNullOrEmpty(pwdchange.mobile) || String.IsNullOrEmpty(pwdchange.opwd) || String.IsNullOrEmpty(pwdchange.npwd) || String.IsNullOrEmpty(pwdchange.src))
                {
                    replyMessage.Response = "parameters missing";
                    replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                }
                else
                {
                    if (sc == "64")
                    {
                        List<PwdChange> results = new List<PwdChange>();
                        var resultList = new List<PwdChange>();

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
                                else {
                                    opwd = "";
                                }
                            }
                        }

                        if (opwd != "")
                        {

                            string PwdChangedReply = "";

                            ChangePassword changepwd = new ChangePassword();
                            PwdChangedReply = changepwd.PasswordChange(mobile, opwd, npwd);

                            if (PwdChangedReply == "true")
                            {
                                replyMessage.Response = "Password change successful";
                                replyMessage.ResponseStatus(HttpStatusCode.OK, replyMessage.Response);
                            }
                            else
                            {
                                replyMessage.Response = "Invalid Password";
                                replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                            }
                        }
                        else {
                            replyMessage.Response = "Please Enter Correct Old Password.";
                            replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                        }
                    }

                    else
                    {
                        replyMessage.Response = "Invalid Service Code";
                        replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                    }
                }
            }
            else
            {
                replyMessage.Response = "request denied";
                replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, replyMessage.Response);
            }

            /*
            JObject jsonObject = new JObject();
            jsonObject["message"] = replyMessage.ResponseStatus;
            return jsonObject["message"].ToString();
            ReplyMessage replyMessage = new ReplyMessage();
            replyMessage.Response ="PIN will be Changed Soon. Thank You!";
            replyMessage.ResponseStatus(HttpStatusCode.OK, replyMessage.Response);

            JObject jsonObject = new JObject();
            jsonObject["message"] = replyMessage.ResponseStatus;
            return jsonObject["message"].ToString();
            */
            return replyMessage.Response;

        }
        #endregion


        #region Merchant List

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        public string merchant(string mobile, string category)
        {
            MNMerchantsController m = new MNMerchantsController();
            return @m.GETNewJSONMerchant(mobile, category);
        }

        #endregion


        /**************************************************/
        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json)]
        public string registration(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);
            string tid = qs["tid"];
            string sc = qs["sc"];
            string fname = qs["fname"];
            string mname = qs["mname"];
            string lname = qs["lname"];
            string amobile = qs["amobile"];
            string umobile = qs["umobile"];
            string amount = qs["amount"];
            string dob = qs["dob"];
            string street = qs["street"];
            string ward = qs["ward"];
            string district = qs["district"];
            string zone = qs["zone"];
            string photoid = qs["photoid"];
            string ivrlang = qs["ivrlang"];

            string result = "";

            ReplyMessage replyMessage = new ReplyMessage();
            do
            {
                TraceIdGenerator traceid = new TraceIdGenerator();
                tid = traceid.GenerateUniqueTraceID();

                bool traceIdCheck = false;
                traceIdCheck = TraceIdCheck.IsValidTraceId(tid);
                if (traceIdCheck == true)
                {
                    result = "Trace ID Repeated";
                }
                else
                {
                    result = "false";
                }

            } while (result == "Trace ID Repeated");

           
            AddNewUser createnewuser = new AddNewUser(tid, sc, fname, mname, lname, amobile, umobile, amount, dob, street, ward, district, zone, photoid, ivrlang);
            Message<AddNewUser> createnewuserRequest = new Message<AddNewUser>(createnewuser);
            MNComAndFocusOneLog comfocuslog = new MNComAndFocusOneLog(tid, 0, createnewuserRequest.message, "0", DateTime.Now);
            MNComAndFocusOneLogsController comfocuslogs = new MNComAndFocusOneLogsController();

            result = comfocuslogs.InsertIntoComFocusOne(comfocuslog);

            string UserName = "";
            string Password = "";
            string PIN = "";

            if (result == "Success")
            {
                if (String.IsNullOrEmpty(createnewuser.tid) || String.IsNullOrEmpty(createnewuser.fname) || String.IsNullOrEmpty(createnewuser.lname) || String.IsNullOrEmpty(createnewuser.amobile) || String.IsNullOrEmpty(createnewuser.umobile) || String.IsNullOrEmpty(createnewuser.amount) || String.IsNullOrEmpty(createnewuser.street))
                {
                    replyMessage.Response = "parameters missing";
                    replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                }
                else
                {
                    if (sc == "70")
                    {
                        MNClientContact userdetails = new MNClientContact();
                        userdetails.ContactNumber1 = createnewuser.umobile;
                        ClientsDetails clientdetails = new ClientsDetails(userdetails);  //For Creating new User

                        MNClientContact agentFulldetails = new MNClientContact();
                        agentFulldetails.ContactNumber1 = createnewuser.amobile;
                        ClientsDetails agentdetails = new ClientsDetails(agentFulldetails);

                        if (clientdetails.client == null && clientdetails.clientContact == null)
                        {
                            if (agentdetails.client != null && agentdetails.clientContact != null)
                            {
                                MNAgent agent = new MNAgent();
                                agent.ClientCode = agentdetails.client.ClientCode; //Set ClientCode to Get all Details of Agent

                                AgentDetails getAgentdetails = new AgentDetails();
                                MNAgent agentinfo = getAgentdetails.AgentDetailsByAgentDetailsFromMNClient(agent);

                                if (agentinfo != null)
                                {
                                    var dataContext = new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection());
                                    //Proceed To create new User
                                    var GetlastClientCode = dataContext.Single<MNClient>("select top 1 ClientCode from MNClient (NOLOCK) order by ClientCode desc");
                                    //int id = int.Parse(GetlastClientCode.ClientCode.Substring(5));

                                    int id = int.Parse(GetlastClientCode.ClientCode);
                                    int newid = id + 1;
                                    string newClientCode = newid.ToString().PadLeft(8, '0');
                                    /********************Create New User in Table Named MNClient*********/
                                    Pin generatePINfornewUser = new Pin();
                                    string UserPIN = generatePINfornewUser.GeneratePin();
                                    MNClient clientdetailsOfNewClient = new MNClient(newClientCode, createnewuser.fname + " " + createnewuser.mname + " " + createnewuser.lname, createnewuser.street + " ," + createnewuser.ward + "," + createnewuser.zone, UserPIN, "On Hold");

                                    MNClientsController sendClientdetailsToCreateNewClient = new MNClientsController();
                                    string MNClientresult = sendClientdetailsToCreateNewClient.GetDetailsToCreateNewClient(clientdetailsOfNewClient);

                                    /*******************End:*************************/
                                    if (MNClientresult == "true")
                                    {
                                        /********************Create New User in Table Named MNClientContact******/
                                        MNClientContact clientcontactOfNewClient = new MNClientContact(newClientCode, createnewuser.umobile.Trim(), null);

                                        MNClientContactsController sendContactDetailsToCreateNewClient = new MNClientContactsController();
                                        string MNClientContactresult = sendContactDetailsToCreateNewClient.GetContactDetailsToCreateNewClient(clientcontactOfNewClient);

                                        /*******************End:*************************/

                                        /********************Create New User in Table Named MNClientExt******/
                                        string GetWordToGeneratePassword = createnewuser.fname.Substring(2) + createnewuser.umobile.Substring(8);
                                        Pin getPwd = new Pin();
                                        string UserPassword = getPwd.ScrambleWord(GetWordToGeneratePassword);

                                        MNClientExt extradetailsOfNewClient = new MNClientExt(newClientCode, createnewuser.umobile.Trim(), UserPassword, "user");

                                        MNClientExtsController sendextradetailsOfNewClient = new MNClientExtsController();
                                        string MNClientExtresult = sendextradetailsOfNewClient.GetExtraClientDetailsToCreateNewClient(extradetailsOfNewClient);

                                        /*******************End:*************************/

                                        string TraceNo = createnewuser.tid.ToString().PadLeft(6, '0');
                                        string RetrievalRef = createnewuser.tid.ToString().PadLeft(12, '0');
                                        string RequestChannel = "HTTP";

                                        MNTransactionMaster mntrans = new MNTransactionMaster(TraceNo, createnewuser.sc.Trim(),
                                        createnewuser.amobile.Trim(), agentdetails.accountInfo.WalletNumber, agentdetails.accountInfo.BIN, agentdetails.accountInfo.BranchCode,
                                        RequestChannel, DateTime.Now, float.Parse(createnewuser.amount), newClientCode, 1, 1, null);

                                        MNTransactionsController senddataOfNewUser = new MNTransactionsController();
                                        int TransactionId = senddataOfNewUser.InsertNewUserInfoIntoTransDB(mntrans);

                                        MNTransactionLog transactionlog = new MNTransactionLog(TransactionId, DateTime.Now, 1, newClientCode);
                                        MNTransactionLogController translog = new MNTransactionLogController();
                                        translog.InsertDataIntoTransactionLog(transactionlog);

                                        MNRequest sendClientdetailsToMNRequest = new MNRequest(createnewuser.amobile.Trim(), "6011",
                                                createnewuser.sc.Trim(),
                                                agentdetails.accountInfo.BIN,
                                                agentdetails.accountInfo.BranchCode,
                                                agentdetails.accountInfo.WalletNumber,
                                                float.Parse(createnewuser.amount),
                                                null, TraceNo, DateTime.Now,
                                                RetrievalRef,
                                                newClientCode,
                                                "F");

                                        MNRequestsController mnrequestcontroller = new MNRequestsController();
                                        bool MNRequestReply = mnrequestcontroller.SendNewClientDetailsToMNRequest(sendClientdetailsToMNRequest);

                                        if (MNRequestReply == true)
                                        {
                                            var waitForReply = new MNResponse();
                                            for (int i = 0; i < 9999; i++)
                                            {
                                                waitForReply = LookINTOResponseTableForNewUserAccount(RetrievalRef);
                                                if (waitForReply.DestAccountNo != null)
                                                {
                                                    MNAccountInfo newUseraccinfo = new MNAccountInfo(newClientCode, waitForReply.DestAccountNo, waitForReply.DestBankCode, waitForReply.DestBranchCode, true, agentinfo.ID);

                                                    MNAccountInfosController createnewWalletForNewClient = new MNAccountInfosController();
                                                    createnewWalletForNewClient.InsertWalletForNewClient(newUseraccinfo);


                                                    MNClient updateClientStatus = new MNClient();
                                                    updateClientStatus.Status = "Active";
                                                    updateClientStatus.ClientCode = newClientCode;
                                                    MNClientsController sendUpdateDatas = new MNClientsController();
                                                    sendUpdateDatas.UpdateClientTable(updateClientStatus);

                                                    MNSubscribedProduct subsprod = new MNSubscribedProduct();
                                                    subsprod.ClientCode = newClientCode;
                                                    MNProductMaster pm = new MNProductMaster();
                                                    pm.ID = 1;
                                                    subsprod.ProductId = pm.ID;
                                                    subsprod.ProductMaster = pm;
                                                    subsprod.IsDefault = true;
                                                    subsprod.ProductStatus = "Active";

                                                    MNSubscribedProductsController sendDatas = new MNSubscribedProductsController();
                                                    sendDatas.InsertProductForNewUser(subsprod);

                                                    ClientsDetails details = new ClientsDetails(updateClientStatus);
                                                    MNClientExt mnclientext = getMNCLientExt(details.client.ClientCode);
                                                    //replyMessage.Response = "Your Client's PIN is " + details.client.PIN + "  And UserName is " + mnclientext.UserName + "  Password is " + mnclientext.Password;

                                                    UserName = mnclientext.UserName;
                                                    Password = mnclientext.Password;
                                                    PIN = details.client.PIN;

                                                    replyMessage.Response = "User Registered Successfully.";
                                                    replyMessage.ResponseStatus(HttpStatusCode.OK, "User Registered Successfully.");
                                                    break;

                                                }
                                            }

                                            if (waitForReply.DestAccountNo == null)
                                            {
                                                replyMessage.Response = "Will reply Through Other Channel";
                                                replyMessage.ResponseStatus(HttpStatusCode.OK, replyMessage.Response);

                                            }
                                        }
                                        else
                                        {
                                            replyMessage.Response = "TraceNo Repeated";
                                            replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, replyMessage.Response);
                                        }
                                    }
                                    else
                                    {
                                        replyMessage.Response = "Invalid Data";
                                        replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, replyMessage.Response);
                                    }
                                }
                                else
                                {
                                    replyMessage.Response = "Agent Doesnot Exists";
                                    replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                                }

                            }
                            else
                            {
                                replyMessage.Response = "Agent Data Unavailable";
                                replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                            }
                        }
                        else
                        {
                            replyMessage.Response = "User Already Exists";
                            replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                        }
                    }
                    else
                    {
                        replyMessage.Response = "Invalid Service Code";
                        replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                    }
                }
            }
            else
            {
                replyMessage.Response = "request denied";
                replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, replyMessage.Response);
            }

            OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    string messagereply = "Dear Customer," + "\n";
                    messagereply += "Your LoginName is:" + UserName + " Password is:" + Password + "\n";
                    messagereply += "and Transaction PIN is:" + PIN + "\n";
                    messagereply += "-MNepal";

                    string mobile = UserName;

                    var client = new WebClient();

                    //var content = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To=" + "977" + createnewuser.umobile.Trim() + "&Text=" + messagereply + "");

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
            return replyMessage.Response;

        }

        /***************************************************/
        private MNClientExt getMNCLientExt(string clientcode)
        {
            MNClientExt mnclientExt = new MNClientExt();
            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;

            string sql = "SELECT * FROM MNClientExt (NOLOCK) WHERE ClientCode = '" + clientcode + "'";
            try
            {
                using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(conStr))
                {
                    con.Open();
                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, con))
                    using (System.Data.SqlClient.SqlDataReader rdr = command.ExecuteReader())
                        while (rdr.Read())
                        {
                            mnclientExt.ClientCode = rdr["ClientCode"].ToString();
                            mnclientExt.UserName = rdr["UserName"].ToString();
                            mnclientExt.Password = rdr["Password"].ToString();

                        }
                    con.Close();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return mnclientExt;
        }

        //Customer Kyc
        private MNClientKyc getMNCLientKyc(string clientcode)
        {
            MNClientKyc mnclientKyc = new MNClientKyc();
            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;

            string sql = "SELECT * FROM MNClientKYC where ClientCode = '" + clientcode + "'";
            try
            {
                using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(conStr))
                {
                    con.Open();
                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, con))
                    using (System.Data.SqlClient.SqlDataReader rdr = command.ExecuteReader())
                        while (rdr.Read())
                        {
                            mnclientKyc.FathersName = rdr["FathersName"].ToString();
                            mnclientKyc.Gender = rdr["Gender"].ToString();
                            string dob = rdr["DateOfBirth"].ToString();
                            if (dob != "") {
                                DateTime enteredDate = DateTime.Parse(dob);
                                mnclientKyc.DateOfBirth = enteredDate.ToString("dd/MM/yyyy");
                            } else {
                                mnclientKyc.DateOfBirth = "";
                            }
                            mnclientKyc.GFathersName = rdr["GFathersName"].ToString();
                            mnclientKyc.CustStatus = rdr["CustStatus"].ToString();
                        }
                    con.Close();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return mnclientKyc;
        }

        private ClientKYCDoc getMNCLientKycDoc(string clientcode)
        {
            ClientKYCDoc mnclientKycDoc = new ClientKYCDoc();
            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;

            string sql = "SELECT * FROM MNClientKYCDoc where ClientCode = '" + clientcode + "'";
            try
            {
                using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(conStr))
                {
                    con.Open();
                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, con))
                    using (System.Data.SqlClient.SqlDataReader rdr = command.ExecuteReader())
                        while (rdr.Read())
                        {
                            mnclientKycDoc.DocType = rdr["DocType"].ToString();
                            mnclientKycDoc.FrontImage = rdr["FrontImage"].ToString();
                            mnclientKycDoc.BackImage = rdr["BackImage"].ToString();
                            mnclientKycDoc.PassportImage = rdr["PassportImage"].ToString();



                        }
                    con.Close();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return mnclientKycDoc;
        }
        /// <summary>
        /// //////////////
        /// </summary>
        /// <param name="contactNumber"></param>
        /// <returns></returns>

        private ClientKYCDoc getMNMerchantKycDoc(string clientcode)
        {
            ClientKYCDoc mnclientKycDoc = new ClientKYCDoc();
            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;

            string sql = "SELECT * FROM MNMerchantKYCDoc where ClientCode = '" + clientcode + "'";
            try
            {
                using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(conStr))
                {
                    con.Open();
                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, con))
                    using (System.Data.SqlClient.SqlDataReader rdr = command.ExecuteReader())
                        while (rdr.Read())
                        {

                            mnclientKycDoc.FrontImage = rdr["FrontImage"].ToString();
                            mnclientKycDoc.BackImage = rdr["BackImage"].ToString();
                            mnclientKycDoc.PassportImage = rdr["PassportImage"].ToString();



                        }
                    con.Close();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return mnclientKycDoc;
        }


        private MNClient getMNCLientCodeByNumber(string contactNumber)
        {
            MNClient mnclient = new MNClient();
            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;

            string sql = "SELECT * FROM dbo.v_MNClientDetail WHERE ContactNumber1 ='" + contactNumber + "'";
            try
            {
                using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(conStr))
                {
                    con.Open();
                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, con))
                    using (System.Data.SqlClient.SqlDataReader rdr = command.ExecuteReader())
                        while (rdr.Read())
                        {
                            mnclient.ClientCode = rdr["ClientCode"].ToString();
                            mnclient.Address = rdr["Address"].ToString();
                            mnclient.Name = rdr["Name"].ToString();
                            mnclient.PIN = rdr["PIN"].ToString();
                            mnclient.Status = rdr["Status"].ToString();
                           mnclient.HasKYC = rdr["HasKYC"].ToString();
                            //string CDistrict = getDistrictName(rdr["CDistrictID"].ToString());
                            //string CStreet = rdr["CStreet"].ToString();
                            //string CProvince = rdr["CProvince"].ToString();
                            //mnclient.Address = CStreet + "," + " " + CDistrict + "," + " " + "Province No. " + CProvince;
                        }
                    con.Close();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return mnclient;
        }
        private String getAddress(String clientCode) {
            
            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;
            string Address="";
            string sql = "SELECT * FROM MNClientKYC WHERE ClientCode ='" + clientCode + "'";
            try
            {
                using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(conStr))
                {
                    con.Open();
                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, con))
                    using (System.Data.SqlClient.SqlDataReader rdr = command.ExecuteReader())
                        while (rdr.Read())
                        {

                            string CDistrict = getDistrictName(rdr["PDistrict"].ToString());
                            string CStreet = rdr["PStreet"].ToString();
                            string CProvince = rdr["PProvince"].ToString();
                            Address = CStreet + "," + " " + CDistrict + "," + " " + "Province No. " + CProvince;
                        }
                    con.Close();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Address;
        }
        private String getPermanentAddress(String clientCode)
        {

            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;
            string Address = "";
            string sql = "SELECT * FROM MNClientKYC WHERE ClientCode ='" + clientCode + "'";
            try
            {
                using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(conStr))
                {
                    con.Open();
                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, con))
                    using (System.Data.SqlClient.SqlDataReader rdr = command.ExecuteReader())
                        while (rdr.Read())
                        {

                            string PDistrict = getDistrictName(rdr["PDistrict"].ToString());
                            string PStreet = rdr["PStreet"].ToString();
                            string PProvince = rdr["PProvince"].ToString();
                            Address = PStreet + "," + " " + PDistrict + "," + " " + "Province No. " + PProvince;
                        }
                    con.Close();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Address;
        }
        //Customer Kyc End
        public string getDistrictName(string id)
        {

            string districtName="";

            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;
        
            string sql = "select Name from MNDistrict where DistrictID='" + id + "'";
            try
            {
                using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(conStr))
                {
                    con.Open();
                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, con))
                    using (System.Data.SqlClient.SqlDataReader rdr = command.ExecuteReader())
                        while (rdr.Read())
                        {
                            districtName = rdr["Name"].ToString();
                        }
                    con.Close();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return districtName;


        }
        private MNResponse LookINTOResponseTableForNewUserAccount(string RetrievalRef)
        {
            string result = "";
            var mnresponse = new MNResponse();
            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;

            string sql = "SELECT * FROM MNResponse (NOLOCK) WHERE RetrievalRef = '" + RetrievalRef + "'";
            try
            {
                using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(conStr))
                {
                    con.Open();
                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, con))
                    using (System.Data.SqlClient.SqlDataReader rdr = command.ExecuteReader())
                        while (rdr.Read())
                        {
                            result = rdr["ResponseCode"].ToString();
                            string DestAccountNo = rdr["DestAccountNo"].ToString();
                            if (DestAccountNo != "" || result == "00")
                            {
                                string OriginID = rdr["OriginID"].ToString();
                                string OriginType = rdr["OriginType"].ToString();
                                string ServiceCode = rdr["ServiceCode"].ToString();
                                string DestBankCode = rdr["DestBankCode"].ToString();
                                string DestBranchCode = rdr["DestBranchCode"].ToString();
                                float Amount = float.Parse(rdr["Amount"].ToString());
                                string AcHolderName = rdr["AcHolderName"].ToString();
                                string ResponseDescription = rdr["ResponseDescription"].ToString();
                                /*
                               this.OriginID = OriginID;
                                this.OriginType = OriginType;
                                this.ServiceCode = ServiceCode;
                                this.DestBankCode = DestBankCode;
                                this.DestBranchCode = DestBranchCode;
                                this.DestAccountNo = DestAccountNo;
                                this.Amount=Amount;
                                this.AcHolderName=AcHolderName;
                                this.ResponseDescription = ResponseDescription;
                     
                               */
                                mnresponse = new MNResponse(OriginID, OriginType, ServiceCode, DestBankCode, DestBranchCode, DestAccountNo, Amount, AcHolderName, ResponseDescription);

                            }
                        }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                result = ex.ToString();
            }
            return mnresponse;
        }

        private string LookINTOResponseTable(string tid)
        {
            string result = "";
            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;
            var newString = tid.PadLeft(6, '0');
            string sql = "SELECT Balance FROM MNResponse (NOLOCK) WHERE TraceNo = '" + newString + "'";
            try
            {
                using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(conStr))
                {
                    con.Open();
                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, con))
                    using (System.Data.SqlClient.SqlDataReader rdr = command.ExecuteReader())
                        while (rdr.Read())
                        {
                            result = rdr["Balance"].ToString();
                        }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                result = ex.ToString();
            }
            return result;
        }


        private string GetBalance(BalanceQuery balanceQuery)
        {
            string result = "";
            string bin = "0000";
            string SourceAccountNo = "";

            if (balanceQuery.sc == "20") //wallet balance query 
            {
                if (balanceQuery.sa != "")
                {
                    SourceAccountNo = balanceQuery.sa;
                }
                else
                {
                    SourceAccountNo = GetSourceWalletAccount(balanceQuery.mobile);  ///wallet number 

                }
            }

            if (balanceQuery.sc == "22")
            {
                if (balanceQuery.sa != "")
                {
                    SourceAccountNo = balanceQuery.sa;
                }
                else
                {
                    SourceAccountNo = GetSourceBankAccount(balanceQuery.mobile);  //bank account number 
                }
                //bin = getBIN(SourceAccountNo);
            }

            result = insertintoPumoriIN(balanceQuery.sc, bin, balanceQuery.mobile, balanceQuery.tid, SourceAccountNo);
            return result;
        }

        private string GetSourceBankAccount(string mobile)
        {
            string result = "";
            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;
            string sql = "SELECT BankAccountNumber from MNBankAccountMap (NOLOCK) WHERE IsDefault=1 AND ClientCode in (SELECT ClientCode FROM MNClientContact WHERE ContactNumber1 = '" + mobile + "')";
            try
            {
                using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(conStr))
                {
                    con.Open();
                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, con))
                    using (System.Data.SqlClient.SqlDataReader rdr = command.ExecuteReader())
                        while (rdr.Read())
                        {
                            result = rdr["BankAccountNumber"].ToString();
                        }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                result = ex.ToString();
            }
            return result;
        }

        private string getBIN(string SourceAccountNo)
        {
            string result = "";
            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;
            string sql = "SELECT BIN FROM MNBankAccountMap (NOLOCK) WHERE BankAccountNumber= '" + SourceAccountNo + "'";
            try
            {
                using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(conStr))
                {
                    con.Open();
                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, con))
                    using (System.Data.SqlClient.SqlDataReader rdr = command.ExecuteReader())
                        while (rdr.Read())
                        {
                            result = rdr["BIN"].ToString();
                        }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                result = ex.ToString();
            }
            return result;
        }

        private string insertintoPumoriIN(string sc, string sourcebankcode, string mobile, string tid, string SourceAccountNo)
        {
            string result = "";
            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;
            string sql = "Insert into MNRequest(OriginID, OriginType, ServiceCode, SourceBankCode, SourceBranchCode, SourceAccountNo,";
            sql += " Amount, TraceNo, TranDate, RetrievalRef,";
            sql += "IsProcessed) values (";

            sql += "'" + "6061367890123456" + "',";
            sql += "'" + "6011" + "',";
            sql += "'" + sc + "',";
            sql += "'" + sourcebankcode + "',";

            sql += "'" + "001" + "',";

            sql += "'" + SourceAccountNo + "',";


            sql += "'" + 0 + "',";


            var newString = tid.PadLeft(6, '0');
            tid = newString;

            sql += "'" + tid + "',";

            sql += "'" + DateTime.Now + "',";

            var newStringretrival = tid.PadLeft(12, '0');
            sql += "'" + newStringretrival + "',";

            sql += "'" + "F" + "')";
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
            return result;
        }

        private string GetSourceWalletAccount(string mobile)
        {
            string result = "";
            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;
            string sql = "SELECT WalletNumber from MNAccountInfo (NOLOCK) WHERE IsDefault=1 and ClientCode in (SELECT ClientCode FROM MNClientContact WHERE ContactNumber1 = '" + mobile + "')";
            try
            {
                using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(conStr))
                {
                    con.Open();
                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, con))
                    using (System.Data.SqlClient.SqlDataReader rdr = command.ExecuteReader())
                        while (rdr.Read())
                        {
                            result = rdr["WalletNumber"].ToString();
                        }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                result = ex.ToString();
            }
            return result;
        }

        public string PassDataToMNClientExtController(UserValidate uv)
        {
            string reply = "";
            MNClientExt mnclientext = new MNClientExt();
            mnclientext.UserName = uv.UserName;
            mnclientext.Password = uv.Password;
            mnclientext.userType = uv.userType;


            MNClientExtsController mncontroller = new MNClientExtsController();
            reply = mncontroller.ValidateUser(mnclientext);

            return reply;
        }

        public string IsTraceIDUnique(string traceId)
        {
            string result = "";
            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;
            string sql = "SELECT count(*) as count FROM MNComAndFocusOneLog (NOLOCK) WHERE TraceId='" + traceId + "'";
            int getCount = 0;
            try
            {
                using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(conStr))
                {
                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, con))
                    {
                        command.Connection.Open();
                        using (System.Data.SqlClient.SqlDataReader rdr = command.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                getCount = int.Parse(rdr["count"].ToString());
                            }

                            if (getCount <= 0)
                            {
                                result = "true";
                            }
                            else
                            {
                                result = "false";
                            }
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                result = ex.ToString();
            }

            return result;

        }

        public static void SaveSyncLogInfo(string mobile, string tid)
        {
            bool newUserDevice = true;
            SyncDetail logInfo = new SyncDetail()
            {
                Mobile = mobile,
                Tid = tid
            };

            int results = LoginUtils.CreateMobileUsersInfo(logInfo);
            if (results > 0)
            {
                newUserDevice = false;
            }
            else
            {
                newUserDevice = true;
            }
        }


        #region NEA Location

        [OperationContract]
        [WebGet]
        public string nealoc()
        {
            string result = "";
            List<string> results = new List<string>() { };
            StringBuilder sb = new StringBuilder();
            JavaScriptSerializer jss = new JavaScriptSerializer();
            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;
            string sql = "SELECT NEABranchCode, NEABranchName FROM MNNEALocation (NOLOCK)";
            try
            {
                using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(conStr))
                {
                    con.Open();
                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, con))
                    using (System.Data.SqlClient.SqlDataReader rdr = command.ExecuteReader())
                    {
                        

                        ////Get All column 
                        //var columnNames = Enumerable.Range(0, rdr.FieldCount)
                        //                        .Select(rdr.GetName) //OR .Select("\""+  reader.GetName"\"") 
                        //                        .ToList();

                        ////Create headers
                        //sb.Append(string.Join(",", columnNames));

                        ////Append Line
                        //sb.AppendLine();
                                                
                        while (rdr.Read())
                        {
                            for (int i = 0; i < rdr.FieldCount; i++)
                            {
                                
                                string getName = rdr.GetName(i);
                                string value = rdr[i].ToString();
                                string values = getName + ":" + value;
                                if (values.Contains(","))
                                    values = "\"" + values + "\""; //getName + ":" + value + "\"";

                                sb.Append(values.Replace(Environment.NewLine, " ") + ",");
                            }
                            //for (int i = 0; i < rdr.FieldCount; i++)
                            //{
                            //    //string value = rdr[i].ToString();
                            //    //if (value.Contains(","))
                            //    //    value = "\"" + value + "\"";

                            //    //sb.Append(value.Replace(Environment.NewLine, " ") + ",");
                            //}
                            sb.Length--; // Remove the last comma
                            sb.AppendLine();

                            AccountInfo nea = new AccountInfo();
                            nea.mobile = rdr["NEABranchCode"].ToString();
                            nea.pin = rdr["NEABranchName"].ToString();
                            results.Add("NEABranchCode" + ":" + nea.mobile + "," + "NEABranchName" + ":" + nea.pin);
                            //results.Add(nea);
                            //string str = rdr["NEABranchCode"].ToString() + ","+ rdr["NEABranchName"].ToString();
                            //results.Insert(i, str);
                            //i++;
                        }
                    }
                        
                    con.Close();
                    result = JsonConvert.SerializeObject(sb, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                result = ex.ToString();
            }
            return result;
        }


        #endregion
    }
}
