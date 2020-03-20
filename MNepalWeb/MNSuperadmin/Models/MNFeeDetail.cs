using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNSuperadmin.Models
{
    public class MNFeeDetail
    {
        public string CommissionId { get; set; }
        public string FeeID { get; set; }
        public string TieredStart { get; set; }
        public string TieredEnd { get; set; }
        public string MinAmt { get; set; }
        public string MaxAmt { get; set; }
        public string Percentage { get; set; }
        public string FlatFee { get; set; }
        public string FeeType { get; set; }
    }
}