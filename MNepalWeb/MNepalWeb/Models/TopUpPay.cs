using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MNepalWeb.Models
{
    public class TopUpPay
    {
       
        public string MobileNumber { get; set; }//Receiver mobile number

        [Display(Name = "Transaction Medium")]
        public string TransactionMedium { get; set; }

        public decimal Amount { get; set; }
        [Display(Name = "T-PIN")]
        public string PIN { get; set; }
        [Display(Name = "Unlimited")]
        public string Unlimited { get; set; }
        [Display(Name = "Volume Based")]
        public string VolumeBased { get; set; }
    }
}