using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNSuperadmin.Models
{
    public class CustomerLog
    {
        public string ServiceType { get; set; }
        public string Source { get; set; }
        public string Sender { get; set; }
        public string SourceAccountNo { get; set; }
        public string Destination { get; set; }
        public string DestinationAccount { get; set; }
        public string Reciever { get; set; }
        public decimal Amount { get; set; }
        public DateTime TranDate { get; set; }
        public string TraceNo { get; set; }
        public string ResponseCode { get; set; }
        public string Description { get; set; }
    }
}