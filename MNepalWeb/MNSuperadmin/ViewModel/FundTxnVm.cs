using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNSuperadmin.Models;

namespace MNSuperadmin.ViewModel
{
    public class FundTxnVm
    {
        public TopUp Parameter { get; set; }

        public List<FundTransfer> FundTransfer { get; set; }

        public List<EBankingTran> EBankingTran { get; set; }

    }

    public class FundTransfer
    {
        public DateTime DatenTime { get; set; }

        public string TxnID { get; set; }

        public string SourceMobileNo { get; set; }

        public decimal Amount { get; set; }

        public string Status { get; set; }

        public string Message { get; set; }

        public string FTType { get; set; }

        public string DestMobileNo { get; set; }
        public string PaymentReferenceNumber { get; set; }
        public string ReferenceNo { get; set; }
    }

    public class EBankingTran
    {
        public string EBDate { get; set; }
        public string EID { get; set; }
        public string ClientCode { get; set; }
        public string PaymentReferenceNumber { get; set; }
        public string UserName { get; set; }
        public string ItemCode { get; set; }
        public decimal Amount { get; set; }
        public string Amt { get; set; }
        public string BID { get; set; }
        public string Status { get; set; }
        public string Name { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public string RejectedRemarks { get; set; }
        public string ReferenceNo { get; set; }
        public string VerifiedBy { get; set; }
        public string VerifiedDate { get; set; }
        public string RejectedBy { get; set; }
        public string RejectedDate { get; set; }
        public DateTime Verifieddate { get; set; }
    }
}