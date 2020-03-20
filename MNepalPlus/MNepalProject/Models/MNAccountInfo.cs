using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using MNepalProject.Models;
using System.ComponentModel.DataAnnotations;

namespace MNepalProject.Models
{
    public class MNAccountInfo
    {
        public int ID { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(8)]
        public string ClientCode { get; set; }
        

        public string WalletNumber { get; set; }

        public string BankAcNumber { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(4)]
        public string BIN { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(5)]
        public string BranchCode { get; set; }

        // [ForeignKey("BIN")]
        //public Bank Bank { get; set; }

        public bool IsDefault { get; set; }
        public int AgentId { get; set; } 

        public MNAccountInfo() { }
        public MNAccountInfo(string ClientCode,string WalletNumber,string BIN,string BranchCode,bool IsDefault,int AgentId)
        {
            this.ClientCode = ClientCode;
            this.WalletNumber = WalletNumber;
            this.BIN = BIN;
            this.BranchCode = BranchCode;
            this.IsDefault = IsDefault;
            this.AgentId = AgentId;
        }

        public MNAccountInfo(string ClientCode, string WalletNumber, string BIN, string BranchCode, bool IsDefault, int AgentId, string BankAcNumber)
        {
            this.ClientCode = ClientCode;
            this.WalletNumber = WalletNumber;
            this.BIN = BIN;
            this.BranchCode = BranchCode;
            this.IsDefault = IsDefault;
            this.AgentId = AgentId;
            this.BankAcNumber = BankAcNumber;
        }



    }
}