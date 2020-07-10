using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MNepalAPI.Models
{
    public class QRCode
    {
        [Display(Name = "Merchant Name")]
        public string merchantName { get; set; }
        [Display(Name = "Merchant ID")]
        public string merchantId { get; set; }
        [Display(Name = "QRCode Image")]
        public string qrCodeImagePath { get; set; }
    }

    public class QRCodeReader
    {
        public string merchantName { get; set; }
        public string merchantId { get; set; }
    }
}