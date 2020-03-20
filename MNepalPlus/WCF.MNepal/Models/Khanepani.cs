using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCF.MNepal.Models
{
    public class Khanepani
    {
        public string KhanepaniCounter { get; set; }
        public string KpBranchCode { get; set; }
        public string CustomerID { get; set; }
        public string TPin { get; set; }
        public string Remarks { get; set; }
        public string da { get; set; }
        public string note { get; set; }
        public string amount { get; set; }
        public string mobile { get; set; }
        public string Months { get; set; }
        public string TransactionMedium { get; set; }
        public string Mode { get; set; }
        public string CustomerName { get; set; }
        public string TotalAmountDue { get; set; }
        public string ClientCode { get; set; }
        public string UserName { get; set; }
        public string refStan { get; set; }
        public string billNumber { get; set; }
        public string responseCode { get; set; }
        public string retrievalReference { get; set; }

        ////For MNKhanepaniInvoice
        public string status { get; set; }
        public string total_advance_amount { get; set; }
        public string customer_code { get; set; }
        public string address { get; set; }
        public string total_credit_sales_amount { get; set; }
        public string customer_name { get; set; }
        public string current_month_dues { get; set; }
        public string mobile_number { get; set; }
        public string total_dues { get; set; }
        public string previous_dues { get; set; }
        public string current_month_discount { get; set; }
        public string current_month_fine { get; set; }
    }
}