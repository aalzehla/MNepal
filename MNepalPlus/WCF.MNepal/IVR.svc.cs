using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using WCF.MNepal.Models;
using MNepalProject.Services;

namespace WCF.MNepal
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class IVR
    {
        // To use HTTP GET, add [WebGet] attribute. (Default ResponseFormat is WebMessageFormat.Json)
        // To create an operation that returns XML,
        //     add [WebGet(ResponseFormat=WebMessageFormat.Xml)],
        //     and include the following line in the operation body:
        //         WebOperationContext.Current.OutgoingResponse.ContentType = "text/xml";
        [OperationContract]
        [WebInvoke(Method = "POST",

                  ResponseFormat = WebMessageFormat.Json
                  )]
        public string InsertData(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            string tid = qs["tid"];
            string sourcemobile = qs["sourcemobile"];
            string isprocessed = "F";
            DateTime dateTime = DateTime.Now;

            InsertDataForIVR insert = new InsertDataForIVR();
            bool getreply = insert.InsertData(tid, sourcemobile, isprocessed, dateTime);
            if (getreply == true)
            {
                return "WCF Response:- Data Inserted Successfully";
            }
            else
            {
                return "";
            }

        }

    }
}
