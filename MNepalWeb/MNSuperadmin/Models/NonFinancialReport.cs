using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNSuperadmin.Models
{
    public class NonFinancialReport
    {
        public string Date { get; set; }
        public string CustomerName { get; set; }
        public string MobileNumber { get; set; }
        public string TransID { get; set; }
        public string TransactionType { get; set; }
        public string Status { get; set; }

    }
}