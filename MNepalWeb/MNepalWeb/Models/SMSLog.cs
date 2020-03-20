using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalWeb.Models
{
    public class SMSLog
    {
        public string UserName { get; set; }
        public string Message { get; set; }
        public string SentOn { get; set; }
        public string SentBy { get; set; }
        public string Purpose { get; set; }
    }
}