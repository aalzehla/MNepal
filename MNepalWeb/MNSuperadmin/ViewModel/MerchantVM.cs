using MNSuperadmin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNSuperadmin.ViewModel
{
    public class MerchantVM
    {
       
            public MerchantPay Parameter { get; set; }

            public List<MerchantInfo> MerchantInfo { get; set; }
    }



    public class MerchantInfo
    {
            public DateTime DatenTime { get; set; }

            public string TxnID { get; set; }

            public string InitMobileNo { get; set; }

            public string MerchantType { get; set; }

            public string MerchantName { get; set; }

            public decimal Amount { get; set; }

            public string ServiceType { get; set; }

            public string Status { get; set; }

            public string Message { get; set; }
        public string TranType { get; set; }
        public string Name { get; set; }
        public string ReferenceNo { get; set; }

        //For Summary///

        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public decimal TotalAmount { get; set; }

        public string SalesYear { get; set; }

        public string SalesMonth { get; set; }

        public string Sales { get; set; }

        public int NoOfTran { get; set; }

        public string TranDate { get; set; }

        //For reversal transaction
        public string PayMode { get; set; }
        public string VerifiedBy { get; set; }
        public string ApprovedBy { get; set; }
        public string tranDate { get; set; }
        public string EnteredAt { get; set; }
    }

    public class Merchants
    {
        public string Name { get; set; }
        public string ClientCode { get; set; }
    }

}