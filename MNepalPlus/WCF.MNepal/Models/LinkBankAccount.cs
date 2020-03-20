
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCF.MNepal.Models
{
    public class LinkBankAccount
    {
        public string ClientCode { get; set; }
        public string Name { get; set; }
        public string AccountNumber { get; set; }

        public string MobileNumber { get; set; }

        public DateTime DateOfBirth { get; set; }

        public LinkBankAccount(string name, string accountNumber, string mobileNumber, DateTime dateOfBirth)
        {
           
            Name = name;
            AccountNumber = accountNumber;
            MobileNumber = mobileNumber;
            DateOfBirth = dateOfBirth;
        }

        public LinkBankAccount() { }
    }
}