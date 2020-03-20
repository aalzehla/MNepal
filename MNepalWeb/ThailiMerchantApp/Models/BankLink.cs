using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ThailiMerchantApp.Models
{
    public class BankLink
    {
        public BankLink(string accNo)
        {
            this.accNo = accNo;
        }

        public BankLink()
        {

        }

        public string accNo { get; set; }
        public string bankName { get; set; }
        public string DOB { get; set; }
        public string DOB1 { get; set; }
        public string BSDateOfBirth { get; set; }
    }
}