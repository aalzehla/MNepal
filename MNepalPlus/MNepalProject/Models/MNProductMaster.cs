using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MNepalProject.Models;
namespace MNepalProject.Models
{
    public class MNProductMaster
    {
        [Key]
        public int ID { get; set; }
        
        public string ProductName { get; set; }

        //[ForeignKey("BankId")]
        //public Bank Bank { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(4)]
        public string BIN { get; set; }


        public virtual ICollection<MNFeature> Features { get; set; }  

       
    }
}