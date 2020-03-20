using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MNepalProject.Models;
namespace MNepalProject.Models
{
    public class MNBankAccountMap
    {
        [Key]
        public int ID { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(8)]
        public string ClientCode { get; set; }


        public string BankAccountNumber { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(4)]
        public string BIN { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(5)]
        public string BranchCode { get; set; }
        //[ForeignKey("BankId")]//
        // public Bank Bank { get; set; }

        public bool IsDefault { get; set; }
    }
}