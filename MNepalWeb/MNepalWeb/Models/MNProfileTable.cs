using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MNepalWeb.Models
{
    public class MNProfileTable
    {
        public int? ProfileCode { get; set; }
        public string ProfileName { get; set; }
        public string ProfileDesc { get; set; }
        public string ProfileStatus { get; set; }
        public string ProfileGroup { get; set; }
        public string Mode { get; set; }
    }
}