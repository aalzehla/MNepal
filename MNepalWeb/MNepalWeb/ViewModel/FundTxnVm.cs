using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalWeb.Models;

namespace MNepalWeb.ViewModel
{
    public class FundTxnVm
    {
        public TopUp Parameter { get; set; }

        public List<FundTransfer> FundTransfer { get; set; }

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
}