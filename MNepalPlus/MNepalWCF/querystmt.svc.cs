using MNepalProject.Controllers;
using MNepalProject.Models;
using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

namespace WCF.MNepal
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class querystmt
    {
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
            return result;
        }
    }
}
