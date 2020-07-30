using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNSuperadmin.Models
{
    public class TopUpManualTran
    {
        
        public string Date { get; set; }
        public string MobNumber { get; set; }
        public string PayMedium { get; set; }

        public string MerchantName { get; set; }
        public string MerchantID { get; set; }

        public decimal Amount { get; set; }
        public string PayMode { get; set; }
        public string RetrievalRef { get; set; }
        public string UserName { get; set; }
        public string DestNumber { get; set; }
        public string RespDescription { get; set; }
        public string RespCode { get; set; }
        public string RespStatus { get; set; }
        public string BalanceCheck { get; set; }
        public string VerifiedBy { get; set; }
        public string VerifiedDate { get; set; }
        public string RejectedBy { get; set; }
        public string RejectedDate { get; set; }



        public string RejectRemarks { get; set; }
        public string btnCommand { get; set; }

        public string responseCode { get; set; }
        public string responseDescription { get; set; }
    }
}