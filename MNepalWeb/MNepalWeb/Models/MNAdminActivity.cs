using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalWeb.Models
{
    public class MNAdminActivity
    {

        public int SNo { get; set; }

        public string UserName { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }

        public string BranchCode { get; set; }
        public string Updates { get; set; }

        public string Description { get; set; }

        public string Code { get; set; }

        public string Remarks { get; set; }

        public string AccType { get; set; }

        public DateTime? TimeStamp { get; set; }


    }
}