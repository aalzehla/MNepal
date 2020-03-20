using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MNSuperadmin.Models;

namespace MNSuperadmin.Models
{
    public class MNClientExt
    {
        public int ID { get; set; }

        public string ClientCode { get; set; }

        public string UserName { get; set; }

      //  public string Password { get; set; }

        public string userType { get; set; }

        public string ProfileName { get; set; }

        public string PushSMS { get; set; }

        public string COC { get; set; }

        public Guid ClientAuthGuid { get; set; }

       // public string PIN { get; set; }

    }
}