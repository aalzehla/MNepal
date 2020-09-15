using MNepalAPI.BasicAuthentication;
using MNepalAPI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;

namespace MNepalAPI.Controllers
{
    [MyBasicAuthenticationFilter]
    public class SendBulkSMSController : ApiController
    {
        /// <summary>
        /// use ? for newline in textMessage and seperate mobileNumber with comma(,) for multiple
        /// </summary>

        [HttpPost]
        public async Task<List<Category>> BulkSMS(string mobileNumber, string textMessage)
        {
            string SMSNTC = ConfigurationManager.AppSettings["MNepalNTCSMSServerUrl"];
            string SMSNCELL = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalSMSServerUrl"];

            string messageToSend = "";
            //for newline replace begins 
            string input = textMessage;
            string pattern = "[?]";
            string replacement = Environment.NewLine;
            Regex rgx = new Regex(pattern);
            string result = rgx.Replace(input, replacement);
            //newline replace ends

            messageToSend += result;

            var client = new HttpClient();
            List<Category> categoryRepo = new List<Category>();
            String[] mobileNumberRepo = mobileNumber.Split(',');  // seperate mobile number with ,

            foreach (var mobNumber in mobileNumberRepo)
            {
                if ((mobNumber.Substring(0, 3) == "980") || (mobNumber.Substring(0, 3) == "981")) //FOR NCELL
                {
                    var content = client.GetAsync(
                        SMSNCELL + "977" + mobNumber + "&Text=" + messageToSend + "");
                }
                else if ((mobNumber.Substring(0, 3) == "985") || (mobNumber.Substring(0, 3) == "984")
                            || (mobNumber.Substring(0, 3) == "986"))
                {
                    var content = client.GetAsync(
                        SMSNTC + "977" + mobNumber + "&Text=" + messageToSend + "");
                }

            }
            return categoryRepo;
        }
    }
}
