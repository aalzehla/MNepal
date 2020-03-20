using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNSuperadmin.Models
{
    public class CustomerSupport
    {

        //for customer support
        public string SupportId { get; set; }

       
        public string MobileNumber { get; set; }
        public string Category { get; set; }

        public string Email { get; set; }

        public string Remarks { get; set; }
        public string ImageName { get; set; }
        public string Mode { get; set; }

        public string CSName { get; set; }
    }
}