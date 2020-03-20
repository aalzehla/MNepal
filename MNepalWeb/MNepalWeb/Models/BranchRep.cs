using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalWeb.Models
{
    public class BranchRep
    {
        public string BranchCode { get; set; }

        public string CreatedBy { get; set; }

        public Dictionary<string, string> BranchList { get; set; }
    }
}