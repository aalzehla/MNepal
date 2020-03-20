using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCF.MNepal.Models
{
    public class SoapTransaction
    {
        public string BID { get; set; }
        public string ITC { get; set; }
        public string PRN { get; set; }
        public string Amount { get; set; }
        public string BankID { get; set; }
       
        public string AMT1 { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CustRefType { get; set; }
        public string Email { get; set; }
        public string UID { get; set; }
        
    }
}