using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustApp.Models
{
    public class CashOutList
    {
        public string SenderMobileNo { get; set; }
        public string RecipientMobileNo { get; set; }
        public string Amount { get; set; }
        public string TokenID { get; set; }
        public DateTime TokenCreatedDate { get; set; }
        public string CreatedDate { get; set; }
        public DateTime TokenExpiryDate { get; set; }
        public string ExpiryDate { get; set; }
    }
}