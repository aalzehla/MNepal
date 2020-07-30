using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ThailiMerchantApp.Models
{
    public class TopUpPay
    {
       
        public TopUpPay(string tid, string TransactionMedium, string mobile, string mobileNumber, decimal Amount,  string Pin, string note, string sourcechannel)
        {
            this.tid = tid;
            this.TransactionMedium = TransactionMedium;
            ///this.mobile = mobile; 
            this.Amount = Amount;
            this.MobileNumber = mobileNumber;
            this.Pin = Pin;
             this.note = note;
            this.sourcechannel = sourcechannel;
        } 

        public TopUpPay()
        {

        } 

        public string tid { get; set; }
        public string mobile { get; set; }
        public string note { get; set; }
        public string sourcechannel { get; set; }
        public string MobileNumber { get; set; }//Receiver mobile number

        [Display(Name = "Transaction Medium")]
        public string TransactionMedium { get; set; }

        public decimal Amount { get; set; }

        [Display(Name = "T-PIN")]
        public string Pin { get; set; }
        [Display(Name = "Unlimited")]
        public string Unlimited { get; set; }
        [Display(Name = "Volume Based")]
        public string VolumeBased { get; set; }

        public string SelectRecharge { get; set; }

        public string TokenUnique { get; set; }

    }
}