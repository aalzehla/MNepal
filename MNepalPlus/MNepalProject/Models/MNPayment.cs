using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalProject.Models
{
    public class MNPayment
    {
        public string sc { get; set; }
        public string vid { get; set; }
        public string mobile { get; set; }
        public string sa { get; set; }//source account
        public string prod { get; set; }
        public string amount { get; set; }
        public string tid { get; set; }
        public string pin { get; set; }
        public string note { get; set; }
        public string src { get; set; }
    }
}