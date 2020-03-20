using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using MNepalProject.Models;
using System.ComponentModel.DataAnnotations.Schema;
namespace MNepalProject.Models
{
    public class MNFeatureMaster
    {
        [Key]
        [Column(TypeName = "VARCHAR")]
        [StringLength(2)]
        public string FeatureCode { get; set; }

        public string FeatureWord { get; set; }
        public string FeatureGroup { get; set; }
        public string FeatureName { get; set; }
        public string CanHaveMultiple { get; set; }
        

    }
}