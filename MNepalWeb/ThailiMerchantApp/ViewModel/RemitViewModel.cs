using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ThailiMerchantApp.ViewModel
{
    public class RemitViewModel
    {
        public string Message { get; set; }
        public string Token { get; set; }
        public string Sender { get; set; }
        public string Reciever { get; set; }
        public string Balance { get; set; }
        public string TokenAmount { get; set; }
        public string Method { get; set; }

    }
}