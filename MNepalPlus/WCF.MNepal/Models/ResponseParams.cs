using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCF.MNepal.Models
{
    public class ResponseParams
    {
        public int Status { get; set; }
        public dynamic Message { get; set; }

        public int VerificationCode { get; set; }
    }
}