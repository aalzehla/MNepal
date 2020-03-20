using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MNepalProject.Models;

namespace MNepalProject.Models
{
    public class MNFeature
    {
       // public Feature()
        //{
          //  this.ProductId = new HashSet<Course>();
        //} 


        [Key]
        public int ID { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public MNProductMaster ProductMaster { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(2)]
        public string FeatureCode { get; set; }
        //[ForeignKey("FeatureMasterId")]
        //public FeatureMaster FeatureMaster { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(4)]
        public string SourceBIN { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(4)]
        public string DestinationBIN { get; set; }

      

        public int LimitId { get; set; }

        [ForeignKey("LimitId")]
        public MNLimitMaster LimitMaster { get; set; }

        public string FeeId { get; set; }

        [ForeignKey("FeeId")]
        public MNFeeTable MNFeeTable { get; set; }
    }
}