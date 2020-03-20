using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalWeb.Models
{
    public class MNClientKYCDoc
    {
        public string PassportImage { get; set; }
        public string FrontImage { get; set; }
        public string BackImage { get; set; }
        public string DocType { get; set; }
    }
}