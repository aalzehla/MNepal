using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalWeb.Models
{
    public class BalanceQuery //: ReplyMessage
    {
        public string tid;
        public string sc;
        public string mobile;
        public string sa;
        public string pin;
        public string sourcechannel;
        
        public BalanceQuery() { }
        public BalanceQuery(string tid, string sc, string mobile, string sa, string pin, string sourcechannel)
        {
            this.tid = tid;
            this.sc = sc;
            this.mobile = mobile;
            this.sa = sa;
            this.pin = pin;
            this.sourcechannel = sourcechannel;
        }

        public bool valid()
        {
            if (this.tid != "" && this.sc != "" && this.mobile != "" && this.pin != "")
                return true;
            else
                return false;
        }
    }
}