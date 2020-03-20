using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCF.MNepal.Models
{
    public class Login
    {
        public int UserID { get; set; }
        public string Mobile { get; set; }
        public string DeviceID { get; set; }

        public bool? IsActiveDevice { get; set; }
        public DateTime? LoginDate { get; set; }
        public string GeneratedPass { get; set; }

        public string Mode { get; set; }
        public string Token { get; set; }

        public string IPAdress { get; set; }
        public string Status { get; set; }

        public string MACAddress { get; set; }
        public string PublicIPAddress { get; set; }
    }
}