using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNSuperadmin.ViewModel
{
    public class CustomerLoginLog
    {
        public string SNo { get; set; }
        public string Status { get; set; }
        public string UserName { get; set; }
        public string Date { get; set; }
        public string Description { get; set; }

        public string PrivateIP { get; set; }

    }
}