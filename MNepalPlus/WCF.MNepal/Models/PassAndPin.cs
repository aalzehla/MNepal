using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCF.MNepal.Models
{
    public class PassAndPin
    {
        public string password { get; set; }
        public string pin { get; set; }
        public int status { get; set; }

        public string result{ get; set; }

        public PassAndPin(string password, string pin, int status)
        {
            this.password = password;
            this.pin = pin;
            this.status = status;
        }

        public PassAndPin() { }
    }

   
}