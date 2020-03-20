using MNepalProject.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using WCF.MNepal.Models;
using WCF.MNepal.Utilities;

namespace WCF.MNepal
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
    public class bankinfo
    {
        [OperationContract]
        [WebGet]
        public string Banklist()
        {
            string result = "";
            string message = string.Empty;
            string statusCode = string.Empty;
            string bankName = string.Empty;
            string bankCode = string.Empty;

            ReplyMessage replyMessage = new ReplyMessage();

            var resultList = new List<MNBankInfo>();
            //start: check bank list
            DataTable tbBankList = BankCheckUtils.GetBankInfo();
            if (tbBankList.Rows.Count > 0)
            {
                foreach (DataRow dtableBank in tbBankList.Rows)
                {
                    bankName = dtableBank["BankName"].ToString();
                    bankCode = dtableBank["BankCode"].ToString();
                    resultList.Add(new MNBankInfo { BankName = bankName, BankCode = bankCode});
                }

                string sJSONResponse = JsonConvert.SerializeObject(resultList);

                result = sJSONResponse;
                statusCode = "200";
                replyMessage.ResponseStatus(HttpStatusCode.OK, result); //200 - OK
                message = result;
            }
            if (tbBankList.Rows.Count == 0)
            {
                statusCode = "400";
                replyMessage.Response = "Bank Name is not listed";
                result = replyMessage.Response;
                message = result;
                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, result);
                result = replyMessage.Response;
            }
            //end: check bank list

            OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = message
                };
                result = JsonConvert.SerializeObject(v);
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
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
