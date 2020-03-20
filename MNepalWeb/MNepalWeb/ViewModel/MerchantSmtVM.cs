using MNepalWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace MNepalWeb.ViewModel
{
    public class MerchantSmtVM
    {
        public MerStatement Parameter { get; set; }

        public List<SmtInfo> SmtInfo { get; set; }
    }

    public class SmtInfo
    {
        public string TranDate { get; set; }

        public string TranID{ get; set; }

        public string InitiatorMobileNo { get; set; }

        public string DrAmount { get; set; }

        public string CrAmount { get; set; }

        public string Description { get; set; }

        public string ValueDate { get; set; }
    }
}