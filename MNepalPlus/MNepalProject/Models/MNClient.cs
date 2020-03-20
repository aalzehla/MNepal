using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MNepalProject.Models;

namespace MNepalProject.Models
{
    public class MNClient
    {
        [Key]
        [Column(TypeName = "VARCHAR")]
        [StringLength(8)]
        public string ClientCode { get; set; } //should be addressed by changing table structure.
        public string Name { get; set; }
        public string Address { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(4)]
        public string PIN { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(10)]
        public string Status { get; set; }

        public string HasKYC { get; set; }
        public MNClient() { }
        public MNClient(string ClientCode, string Name, string Address, string PIN, string Status)
        {
            this.ClientCode = ClientCode;
            this.Name = Name;
            this.Address = Address;
            this.PIN = PIN;
            this.Status = Status;
            
        }
    }
}