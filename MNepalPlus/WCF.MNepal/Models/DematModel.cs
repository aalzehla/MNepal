using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCF.MNepal.Models
{
    public class DematModel
    {
        public string BoId { get; set; }
        public string DematName { get; set; }
        public string TotalAmount { get; set; }
        public string Fees { get; set; }
        public string ClientCode { get; set; }
        public string RetrievalRef { get; set; }
        public string UserName { get; set; }
        public string BankCode { get; set; }
        public string TimeStamp { get; set; }
        public string Mode { get; set; }
        public string Status { get; set; }
    }
    public class Dematobject
    {
        public string BoId { get; set; }
        public string DematName { get; set; }
        public float TotalAmount { get; set; }
        public Fee[] Fees { get; set; }
    }

    public class Fee
    {
        public string FiscalYear { get; set; }
        public string Description { get; set; }
        public float Amount { get; set; }
    }

    public class DematPayment
    {
        public string BoId { get; set; }
        public float Amount { get; set; }
        public string ReferenceId { get; set; }
        public string TimeStamp { get; set; }
        public string HashValue { get; set; }

    }


}