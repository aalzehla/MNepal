using MNepalWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalWeb.ViewModel
{
    public class TopUpRepVM
    {
        public TopUp Parameter { get; set; }

        public List<TopUpInfo> TopUpInfo { get; set; }

    }

    public class TopUpInfo
    {
        public DateTime DatenTime { get; set; }

        public string TxnID { get; set; }

        public string InitMobileNo { get; set; }

        public decimal Amount { get; set; }

        public string Status { get; set; }

        public string Message { get; set; }

        public string ServiceType { get; set; }

        public string DestMobileNo { get; set; }
        public string ReferenceNo { get; set; }
    }
}