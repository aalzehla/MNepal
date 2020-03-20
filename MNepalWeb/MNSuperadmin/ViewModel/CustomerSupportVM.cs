using MNSuperadmin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNSuperadmin.ViewModel
{
    public class CustomerSupportVM
    {
        public CustomerSupport Parameter { get; set; }

        public List<CustomerDataNew> CustomerDataNew { get; set; }

    }

    public class CustomerDataNew
    {
        //for customer support
        public string SupportId { get; set; }
        public string MobileNumber { get; set; }
        public string Category { get; set; }
        

        //public string Email { get; set; }
        //public string Remarks { get; set; }
    }
}