using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace WCF.MNepal.Models
{
    public class PinChange
    {
            public string tid { get; set; }
            public string sc { get; set; }
            public string mobile { get; set; }
            public string pin { get; set; }
            public string npin { get; set; }
            public string src { get; set; }

        //
        public string ClientCode { get; set; }
        public string Mode { get; set; }

        //

        public PinChange() { }
        public PinChange(string tid, string sc, string mobile,string pin,string npin,string src)
        {
            //this.tid = tid;
            this.tid = tid;
            this.sc = sc;
            this.mobile = mobile;
            this.pin = pin;
            this.npin = npin;
            this.src = src;
        }

        public class ResetPin
        {
            public string UserName { get; set; }
            public string PIN { get; set; }
            public string ClientCode { get; set; }
        }
    }
}