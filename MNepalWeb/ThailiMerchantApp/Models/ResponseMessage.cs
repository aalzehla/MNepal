using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ThailiMerchantApp.Models
{
    public class Response
    {
        public string StatusCode { get; set; }
        public ResponseMessage StatusMessage { get; set; }
    }
    public class ResponseMessage
    {
        public string ResponseCode
        {
            get;
            set;
        }
        public string RequestedToken { get; set; }
        public string d
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }

        public string AmountTransferredBalance
        {
            get;
            set;
        }

        public string AvailableBalance
        {
            get;
            set;
        }
        

    }
}