using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalWeb.Models {
    public class MNExpiryCustomers {


        public string Name { get; set; }
        public string Address { get; set; }
        public string ContactNumber { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string  ProfileCode { get; set; }
        public int RenewPeriod { get; set; }
        public string AutoRenew { get; set; }
        public string ExpiryPeriodInDays { get; set; }
        public bool Expired { get; set; }

        
    }
}