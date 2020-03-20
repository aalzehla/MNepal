using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNSuperadmin.Models
{
    public class CustReport
    {
        public string UserName { get; set; }
        
        public string StartDate { get; set; }
        public string EndDate { get; set; }

        public string ProfileCode { get; set; }

        public string MobileNo { get; set; }
        public string MobileNumber { get; set; }
        public string CustomerName { get; set; }

        public string Status { get; set; }
        public string  HasKYC { get; set; }
        public string CreatedBy { get; set; }

        public string ServiceProvider { get; set; }


        //for agent commission

        
        public string FeeId { get; set; }
        public string Id { get; set; }
        public string TieredStart { get; set; }

        public string TieredEnd { get; set; }
        public string MinAmt { get; set; }
        public string MaxAmt { get; set; }
        public string Percentage { get; set; }
        public string FlatFee { get; set; }

        public string FeeType { get; set; }

    }

    
}