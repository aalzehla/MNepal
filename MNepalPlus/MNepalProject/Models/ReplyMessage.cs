using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Web;
using System.Web;

namespace MNepalProject.Models
{
    public class ReplyMessage
    {
        public string ResponseCode { get; set; }

        public string Response { get; set; }

        public void ResponseStatus(HttpStatusCode code, string description)
        {
            WebOperationContext ctx = WebOperationContext.Current;
            ctx.OutgoingResponse.StatusDescription = description;
            ctx.OutgoingResponse.StatusCode = code;
        }
    }
}