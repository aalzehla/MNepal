using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalWeb.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MNepalWeb.Models
{
    public class MNBankTable
    {
        [StringLength(4)]
        public string BankCode { get; set; }

        public string BankName { get; set; }
        public string ShortName { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public string BranchCode { get; set; }
        public string PoolAccount { get; set; }
        public string TerminalId { get; set; }
    }
}