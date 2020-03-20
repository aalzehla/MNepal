using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNSuperadmin.Models {
    public class MNAdminLog
    {

        public string UserId { get; set; }
        public string UserType { get; set; }
        public string URL { get; set; }
        public string Action { get; set; }
        public string UniqueId { get; set; }
        public string IPAddress { get; set; }
        public string PrivateIPAddress { get; set; }
        public string Message { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Branch { get; set; }
        public string PrivateIP { get; set; }
        
        public string ClientDetails { get; set; }
        

    } 
    }