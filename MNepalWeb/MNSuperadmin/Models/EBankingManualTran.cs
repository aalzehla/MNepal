using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNSuperadmin.Models
{
    public class EBankingManualTran
    {

        //for customer support
        public string PRN { get; set; }

       
        public string ItemCode { get; set; }
        public decimal Amount { get; set; }

        public string Email { get; set; }

        public string Remarks { get; set; }
        public string ImageName { get; set; }
        public string Mode { get; set; }

        public string CSName { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public string Name { get; set; }
        public string Reqdate { get; set; }
        public string ReferenceNo { get; set; }
        public string MobileNo { get; set; }
        public string BID { get; set; }
        public string Status { get; set; }
        public string RejectRemarks { get; set; }
        public string btnCommand { get; set; }
    }
}