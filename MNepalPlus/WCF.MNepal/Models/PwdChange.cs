using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCF.MNepal.Models
{
    public class PwdChange
    {
        public string tid { get; set; }
        public string sc { get; set; }
        public string mobile { get; set; }
        public string opwd { get; set; }
        public string npwd { get; set; }
        public string src { get; set; }
        
         public PwdChange() { }

         public PwdChange(string tid, string sc, string mobile, string opwd, string npwd, string src)
        {
            this.tid = tid;
            this.sc = sc;
            this.mobile = mobile;
            this.opwd = opwd;
            this.npwd = npwd;
            this.src = src;
        }
    }
}