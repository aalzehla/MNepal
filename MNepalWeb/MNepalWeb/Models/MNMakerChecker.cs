using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalWeb.Models
{
    public class MNMakerChecker
    {
        public string Module { get; set; }

        public string EditedBy { get; set; }

        public DateTime? EditedOn { get; set; }

        public string BranchCode { get; set; }

        public string Code { get; set; }

        public string TableName { get; set; }

        public string ColumnName { get; set; }

        public string OldValue { get; set; }

        public string NewValue { get; set; }

        public string ApprovedBy { get; set; }

        public DateTime? ApprovedOn { get; set; }

        public string IsRejected { get; set; }
    }
}