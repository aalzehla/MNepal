using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using MNepalProject.Models;
using System.ComponentModel.DataAnnotations;
namespace MNepalProject.Models
{
    public class MNSubscribedProduct
    {
        public int ID { get; set; }


        [Column(TypeName = "VARCHAR")]
        [StringLength(8)]
        public string ClientCode { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public MNProductMaster ProductMaster { get; set; }

        
        public bool IsDefault { get; set; }

        public string ProductStatus { get; set; }
    }
}