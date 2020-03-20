using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalProject.Models.ViewModel
{
    public class MNTransactionViewModel
    {
        public string CreatedDate { get; set; }
        public string FeatureName { get; set; }
       // public string SourceMobile { get; set; }
        //public string SourceBINName { get; set; }
        public string DestinationMobile { get; set; }
       // public string DestinationBIN { get; set; }       
        public float Amount { get; set; }
        public string Description { get; set; }
        public string StatusName { get; set; }

    }
}