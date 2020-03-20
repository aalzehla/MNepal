using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNSuperadmin.Models
{
    public class MNBranchTable
    {
        public string BranchCode { get; set; }

        public string BranchName { get; set; }

        public string BranchLocation { get; set; }

        public string IsBlocked { get; set; }

        public string Mode { get; set; }
    }
}