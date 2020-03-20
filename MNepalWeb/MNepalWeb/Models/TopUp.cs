using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalWeb.Models
{
    public class TopUp
    {
        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public string SourceMobileNo { get; set; }

        public string DestMobileNo { get; set; }

        public string Status { get; set; }

        public string RequestType { get; set; }

        public string TranID { get; set; }

        public string FTType { get; set; }

        
    }
}