using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCF.MNepal.Models
{
    public class ClientKYCDoc
    {
        public string DocType { get; set; }

        public string FrontImage { get; set; }

        public string BackImage { get; set; }

        public string PassportImage { get; set; }

        public ClientKYCDoc() { }

        public ClientKYCDoc(string DocType, string FrontImage, string BackImage, string PassportImage)
        {
            this.DocType = DocType;
            this.FrontImage = FrontImage;
            this.BackImage = BackImage;
            this.PassportImage = PassportImage;
        }
    }
}