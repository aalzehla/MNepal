using MNSuperadmin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNSuperadmin.ViewModel
{
    public class FinancialReportVM
    {
        public FinancialReport Parameter { get; set; }
        public List<FinancialReportActivity> FinancialReportActivity { get; set; }

    }

    public class FinancialReportActivity
    {
            public string Date { get; set; }
            public string CustomerName { get; set; }
            public string MobileNumber { get; set; }
            public string TargetNumber { get; set; }
            public string TranID { get; set; }
            public string TransactionType { get; set; }
            public string Status { get; set; }
            public string Message { get; set; }
            public string Amount { get; set; }
    }


}