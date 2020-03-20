using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustApp.ViewModel
{
    public class MerchantPaymentVM
    {
        public string Message { get; set; }
        public string Tid { get; set; }
        public string Sender { get; set; }
        public string MName { get; set; }
        public string Balance { get; set; }
        public string Amount { get; set; }
        public string Method { get; set; }
    }
}