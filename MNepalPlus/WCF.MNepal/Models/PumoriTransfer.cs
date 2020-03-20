using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCF.MNepal.Models
{
    public class PumoriTransfer
    {
        public string id { get; set; }
        public string tid { get; set; }

        public PumoriTransfer() { }
        public PumoriTransfer(string id, string tid)
        {
            this.id = id;
            this.tid = tid;
        }

    }
}