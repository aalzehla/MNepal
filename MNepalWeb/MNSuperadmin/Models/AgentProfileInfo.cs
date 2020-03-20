using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNSuperadmin.Models
{
    public class AgentProfileInfo
    {
        public string AgentName { get; set; }
        public string AgentMobileNo { get; set; }

        public string Location { get; set; }

        public string Balance { get; set; }

        public string WalletNumber { get; set; }

        public string IsApproved { get; set; }

        public string Status { get; set; }
 
        public string ClientCode { get; set; }
    }
}