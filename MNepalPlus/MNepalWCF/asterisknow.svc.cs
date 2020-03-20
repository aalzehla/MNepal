using MNepalProject.Models;
using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Web;
namespace MNepalWCF
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class asterisknow
    {
        [OperationContract]
        [WebInvoke(Method = "POST",ResponseFormat = WebMessageFormat.Json)]
        public string connect(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            string PIN = qs["pin"];
            ReplyMessage replyMessage = new ReplyMessage();
            if (PIN != "")
            {

                replyMessage.Response = DateTime.Now + "  " + PIN + "  ok valid PIN";
                replyMessage.ResponseStatus(HttpStatusCode.OK, replyMessage.Response);
                WriteToTExtFile(replyMessage.Response);
            }
            else
            {
                replyMessage.Response = DateTime.Now + "  " + " PIN Empty ";
                replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                WriteToTExtFile(replyMessage.Response);
            }

            JObject jsonObject = new JObject();
            jsonObject["message"] = replyMessage.Response;
            return jsonObject["message"].ToString();
            // Add your operation implementation here
        }


        public void WriteToTExtFile(string content)
        {
            string lines = content;
            // Write the string to a file.
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\inetpub\wwwroot\MNepal.WCF\AsterixLog.txt", true))
            {
                file.WriteLine(lines);
                file.Close();
            }


        }
    }
}
