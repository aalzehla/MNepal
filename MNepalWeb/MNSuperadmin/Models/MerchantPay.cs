using MNSuperadmin.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNSuperadmin.Models
{
    public class MerchantPay
    {
        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public string SourceMobileNo { get; set; }

      

        public string Status { get; set; }

        public string MerchantType { get; set; }
        public List<Merchants> MerchantTypeList { get; set; }

        public string MerchantName { get; set; }

        //for Summary Report//
        public string Service { get; set; }

        public string GrpByDate { get; set; }

        //for summary detail report

        public string  Sales { get; set; }

        public string SalesYear { get; set; }

        public string SalesMonth { get; set; }




    }
}