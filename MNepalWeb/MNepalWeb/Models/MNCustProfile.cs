using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalWeb.Models
{
    public class MNCustProfile
    {
        public string ProfileCode { get; set; }
        public string ProfileDesc { get; set; }
        public char ProfileStatus { get; set; }
        public int RenewPeriod { get; set; }
        public char AutoRenew { get; set; }
        public char  HasCharge { get; set; }
        public char IsDrAlert { get; set; }
        public char IsCrAlert { get; set; }
        public float  MinDrAlertAmt { get; set; }
        public float MinCrAlertAmt { get; set; }


    }
}

