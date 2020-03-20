using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MNSuperadmin.Models;
namespace MNSuperadmin.Models
{
    public class MNBankAccountMap
    {
        public int ID { get; set; }

        public string ClientCode { get; set; }

        public string BankAccountNumber { get; set; }

        public string BIN { get; set; }

        public bool IsDefault { get; set; }

        public string BranchCode { get; set; }
    }
}