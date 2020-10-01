using MNepalAPI.BasicAuthentication;
using MNepalAPI.Models;
using MNepalAPI.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using static MNepalAPI.Models.Notifications;

namespace MNepalAPI.Controllers
{
    [MyBasicAuthenticationFilter]
    public class SendBulkSMSController : ApiController
    {
        [HttpPost]
        public async Task<HttpResponseMessage> BulkSMS(RootObject rootObject)
        {
            string SMSNTC = ConfigurationManager.AppSettings["MNepalNTCSMSServerUrl"];
            string SMSNCELL = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalSMSServerUrl"];

            string messageToSend = rootObject.notificationsList.FirstOrDefault().message;
            var client = new HttpClient();
            foreach (var item in rootObject.notificationsList)
            {
                if ((item.customerNumber.Substring(0, 3) == "980") || (item.customerNumber.Substring(0, 3) == "981")) //FOR NCELL
                {
                    var content = client.GetAsync(
                        SMSNCELL + "977" + item.customerNumber + "&Text=" + messageToSend + "");
                }
                else if ((item.customerNumber.Substring(0, 3) == "985") || (item.customerNumber.Substring(0, 3) == "984")
                            || (item.customerNumber.Substring(0, 3) == "986"))
                {
                    var content = client.GetAsync(
                        SMSNTC + "977" + item.customerNumber + "&Text=" + messageToSend + "");
                }
                var dateTime = DateTime.Now;
                //database
                int resultsPayments = BulkSMSUtilities.BulkSMS(item.customerNumber, messageToSend, dateTime);

            }

            return Request.CreateResponse(HttpStatusCode.OK);


        }
            
        
    }
}
