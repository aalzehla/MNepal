using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalWeb.Models
{
    public class MNCashIn
    {
        public string tid  { get; set; }
        public string usermobile { get; set; }
        public string destmobile { get; set; }
        public string amount { get; set; }
        public string pin { get; set; }

        public string sc { get; set; }
        public string sourcechannel { get; set; }
    }
}