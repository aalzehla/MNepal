using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalWeb.Models
{
    public class MNCustChargeLog
    {
        public string ClientCode { get; set; }
        public string UserName { get; set; }
        public string ChargeAmount { get; set; }
        public string ChargeDescription { get; set; }
        public string DeductedDate {get;set;}
        public string ProcessedBy { get; set; }
        public string RRNumber { get; set; }
    }
}