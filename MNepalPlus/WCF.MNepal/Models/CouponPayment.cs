using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCF.MNepal.Models
{
    public class CouponPayment
    {
        public string sc { get; set; }
        public string vid { get; set; }
        public string mobile { get; set; }
        public string sa { get; set; }
        public string amount { get; set; }
        public string qty { get; set; }
        public string tid { get; set; }
        public string pin { get; set; }
        public string note { get; set; }
        public string sourcechannel { get; set; }
        public string da { get; set; }

        public string TokenUnique { get; set; }

    }
}