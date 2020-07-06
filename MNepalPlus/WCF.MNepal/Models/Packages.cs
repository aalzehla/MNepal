using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCF.MNepal.Models
{
    public class Packages
    {
        public string Description { get; set; }
        public string Amount { get; set; }
        public string PackageId { get; set; }
        public string Bonus { get; set; }
        public string commissionAmount { get; set; }
    }

    public class SmartCards
    {
        public string smartCards { get; set; }
    }

    public class User
    {
        public string user { get; set; }
    }
}