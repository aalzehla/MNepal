using MNepalProject.Controllers;
using MNepalProject.Models;
using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using WCF.MNepal.Models;
using System.Net;
using MNepalProject.Services;
using System.Collections.Specialized;
using System.Web;
using MNepalProject.DAL;
using System.Data;
using WCF.MNepal.Utilities;
using System.Collections.Generic;
using MNepalProject.Helper;
using System.Threading.Tasks;

namespace WCF.MNepal
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
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

            CustActivityModel custsmsInfo = new CustActivityModel();

            Task.Delay(1000).Wait(); // Wait 1sec
            TraceIdGenerator tig = new TraceIdGenerator();
            tid = tig.GenerateUniqueTraceID();

            //start:Com focus one log///
            BalanceQuery balanceQuery = new BalanceQuery(tid, sc, mobile, sa, pin, src);
            balanceQuery.tid = tid;

            if ((tid == "null") || (sc == "null") || (mobile == "null") || (sa == "null") || (pin == "null") || (src == "null"))
            {
                // throw ex
                statusCode = "400";
                balanceQuery.Response = "Parameters Missing/Invalid";
                balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, "Parameters Missing/Invalid");
                result = balanceQuery.Response;
                message = balanceQuery.Response;
            }
            else
            {
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
                        if ((result == "Trace ID Repeated") || (result == "Limit Exceed")
                            || (result == "Invalid Source User") || (result == "Invalid Destination User")
                            || (result == "Invalid Product Request") || (result == "")
                            || (result == "Error in ResponeCode:Data Not Available"))
                        {
                            
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                            statusCode = "400";
                            message = result;
                        }
                        else
                        {
                            //string SyncTime = "0";
                            //DataTable dtSyncTime = LoginUtils.GetSyncInfo(mobile, tid);
                            //if (dtSyncTime.Rows.Count > 0)
                            //{
                            //    SyncTime = dtSyncTime.Rows[0]["SyncTime"].ToString();
                            //}
                            SaveSyncLogInfo(mobile, tid);
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

            //start:Com focus one log///
            Message<AccountInfo> accInforequest = new Message<AccountInfo>(accinfo);
            MNComAndFocusOneLog comfocuslog = new MNComAndFocusOneLog(tid, 0, accInforequest.message, "0", DateTime.Now);
            MNComAndFocusOneLogsController comfocuslogs = new MNComAndFocusOneLogsController();

            ReplyMessage replyMessage = new ReplyMessage();

            string checkTid = IsTraceIDUnique(tid);
            if (checkTid == "true")
            {
                string result = "";
                result = comfocuslogs.InsertIntoComFocusOne(comfocuslog);
                //end:Com focus one log//

                if (result == "Success")
                {
                    if (String.IsNullOrEmpty(tid) || String.IsNullOrEmpty(sc) || String.IsNullOrEmpty(mobile) || String.IsNullOrEmpty(pin))
                    {
                        replyMessage.Response = "Parameter Missing:Please Try Again!";
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
                                        replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, "Account Info Not Found");
                                    }
                                }
                                else
                                {
                                    replyMessage.Response = "Incorrect PIN";
                                    replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, "Data Mismatch");
                                }
                            }
                            else
                            {
                                replyMessage.Response = "Client Not Found";
                                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, "Client Not Found");
                            }
                        }
                        else
                        {
                            replyMessage.Response = "Invalid Service Code";
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
                    replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, replyMessage.Response);
                }
            }
            else
            {
                replyMessage.Response = "TraceID Repeated";
                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, "TraceID Repeated");
            }
            return replyMessage.Response;

        }

        #endregion

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

            ReplyMessage replyMessage = new ReplyMessage();

            string result = "";
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
                                    var GetlastClientCode = dataContext.Single<MNClient>("select top 1 ClientCode from MNClient order by ClientCode desc");
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

            string sql = "SELECT * FROM MNClientExt where ClientCode = '" + clientcode + "'";
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

        private MNResponse LookINTOResponseTableForNewUserAccount(string RetrievalRef)
        {
            string result = "";
            var mnresponse = new MNResponse();
            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;

            string sql = "SELECT * FROM MNResponse where RetrievalRef = '" + RetrievalRef + "'";
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
            string sql = "select Balance from MNResponse where TraceNo = '" + newString + "'";
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
            string sql = "select BankAccountNumber from MNBankAccountMap where IsDefault=1 and ClientCode in (select ClientCode from MNClientContact where ContactNumber1 = '" + mobile + "')";
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
            string sql = "select BIN from MNBankAccountMap where BankAccountNumber= '" + SourceAccountNo + "'";
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
            string sql = "select WalletNumber from MNAccountInfo where IsDefault=1 and ClientCode in (select ClientCode from MNClientContact where ContactNumber1 = '" + mobile + "')";
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
            string sql = "Select count(*) as count from MNComAndFocusOneLog where TraceId='" + traceId + "'";
            int getCount = 0;
            try
            {
                using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(conStr))
                {
                    con.Open();
                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, con))
                    using (System.Data.SqlClient.SqlDataReader rdr = command.ExecuteReader())

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

    }
}
