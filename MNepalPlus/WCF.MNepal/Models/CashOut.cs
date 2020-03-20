using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCF.MNepal.Models
{
    public class CashOut
    {
        public int CashID { get; set; }

        public string TraceID { get; set; }
        public string AgentMobileNo { get; set; }

        public string CustomerMobileNo { get; set; }

        public string BeneficialName { get; set; }

        public string RequestTokenCode { get; set; }

        public string Amount { get; set; }

        public string TokenID { get; set; }

        public string Purpose { get; set; }

        public string ClientCode { get; set; }

        public string PIN { get; set; }

        public string ServiceCode { get; set; }

        public string SourceChannel { get; set; }

        public string TokenCreatedDate { get; set; }

        public string TokenExpiryDate { get; set; }

        public string Status { get; set; }

        public string Remarks { get; set; }

        public string Mode { get; set; }

        public string SenderMobileNo { get; set; }
    }
}