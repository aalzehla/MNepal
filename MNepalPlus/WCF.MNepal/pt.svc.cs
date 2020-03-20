using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using WCF.MNepal.Models;

namespace WCF.MNepal
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class pt
    {
        [OperationContract]
        [WebGet]
        public string request(string id,string tid)
        {
            PumoriTransfer pumoriTransfer = new PumoriTransfer(id,tid);
            // Add your operation implementation I am at master branch//
            return "sending message from SMS";
        }
    }
}
