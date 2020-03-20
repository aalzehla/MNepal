using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using MNepalProject.Controllers;
using MNepalProject.Models;
using MNepalProject.Helper;

namespace MNepalWCF
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class BalanceQueryService
    {
        [OperationContract]
        [WebGet]
        public string Balance(string tid, string sc, string mobile, string sa, string pin, string src)
        {
            string result = "";
            string message = string.Empty;
            string statusCode = string.Empty;
            string failedmessage = string.Empty;

            TraceIdGenerator tig = new TraceIdGenerator();
            tid = tig.GenerateUniqueTraceID();

            //start:Com focus one log///
            BalanceQuery balanceQuery = new BalanceQuery(tid, sc, mobile, sa, pin, src);

            if ((tid == "null") || (sc == "null") || (mobile == "null") || (sa == "null") || (pin == "null") || (src == "null"))
            {
                // throw ex
                statusCode = "400";
                balanceQuery.Response = "Parameters Missing/Invalid";
                balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, "Parameters Missing/Invalid");
                result = balanceQuery.Response;
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
                        if ((result == "Trace ID Repeated") || (result == "Limit Exceed") || (result == "Invalid PIN")
                            || (result == "Invalid Source User") || (result == "Invalid Destination User")
                            || (result == "Invalid Product Request") || (result == "") )
                        {
                            balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                            statusCode = "400";
                            message = result;
                        }
                        if (result.Substring(0, 5) == "Error")
                        {
                            statusCode = "400";
                            message = "Connection Failure from Gateway. Please Contact your Bank." + result;
                            balanceQuery.ResponseStatus(HttpStatusCode.InternalServerError, message);
                            failedmessage = result;
                        }
                        else
                        {
                            balanceQuery.ResponseStatus(HttpStatusCode.OK, result); //200 - OK
                            statusCode = "200";
                            message = result;
                        }

                    }
                    /*** ***/

                }
                else
                {
                    balanceQuery.Response = "error";
                    balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, "parameters missing/invalid");
                    result = balanceQuery.Response;
                }
                //end:insert into transaction master//
            }
            
            return result;
        }

    }
}
