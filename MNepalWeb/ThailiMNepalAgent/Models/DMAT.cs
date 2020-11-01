using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ThailiMNepalAgent.Models
{
    public class DMAT
    {
        public string NWCounter { get; set; }
        public string NWBranchCode { get; set; }
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
        public string DematCode { get; set; }
        public string DematName { get; set; }
        public string TimeStamp { get; set; }
        public string TokenUnique { get; set; }
    }

    public class DematGet
    {
        public string BoId { get; set; }
    }


    public class DematObject
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
        //public string BankName { get; set; }
        //public string TimeStamp { get; set; }
        //public string HashValue { get; set; }

    }

    public class ListFees
    {
        public string FiscalYear { get; set; }
        public string Description { get; set; }
        public string Amount { get; set; }
    }

    public class DematCheckPayment
    {
        public string NWCounter { get; set; }
        public string NWBranchCode { get; set; }
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
        public string DematCode { get; set; }
        public string DematName { get; set; }

    }
}