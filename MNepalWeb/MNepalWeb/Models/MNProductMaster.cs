using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MNepalWeb.Models
{
    public class MNProductMaster
    {
        [Key]
        public int ID { get; set; }
        
        public string ProductName { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(4)]
        public string BIN { get; set; }


        public virtual ICollection<MNFeature> Features { get; set; }  

       
    }
}