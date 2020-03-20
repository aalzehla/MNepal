using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNSuperadmin.Models
{
    public class TransactionReport
    {
        public string Date { get; set; }

        public string AgentName { get; set; }

        public string AgentMobileNumber { get; set; }

        public string Services { get; set; }

        public string TransactionCount { get; set; }

        public string Status { get; set; }

        public string CommissionEarned { get; set; }

     
    }
}