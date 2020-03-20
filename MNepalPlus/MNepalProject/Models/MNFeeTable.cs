using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using MNepalProject.Models;
using System.ComponentModel.DataAnnotations.Schema;
namespace MNepalProject.Models
{
    public class MNFeeTable
    {
        [Key]
        [Column(TypeName = "VARCHAR(4)")]
        public string FeeId { get; set; }
        public string FeeType { get; set; }
        public string Details { get; set; }
        public string FlatFee { get; set; }
        
    }
}