using MNSuperadmin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNSuperadmin.ViewModel
{
    public class RegCusDetailVM
    {
        
        public CustReport Parameter { get; set; }

        public List<CustomerData> CustomerData { get; set; }

        

        //public DateTime StartDate { get; set; }

        //public DateTime EndDate { get; set; }
    }



    public class CustomerData
    {
        public string MobileNo { get; set; }

        public string CustomerName { get; set; }

        public string ProfileName { get; set; }

        public string CreatedDate { get; set; }

        public string  ExpiryDate { get; set; }

        public string Status { get; set; }
        public string Approved { get; set; }


        public string MobileNumber { get; set; }

        public string HasKYC { get; set; }
        public string CreatedBy { get; set; }
        public string ClientCode { get; set; }

        //for agent commission

        public string Id { get; set; }
        public string FeeId { get; set; }
        public string TieredStart { get; set; }

        public string TieredEnd { get; set; }
        public string MinAmt { get; set; }
        public string MaxAmt { get; set; }
        public string Percentage { get; set; }
        public string FlatFee { get; set; }

      public string FeeType { get; set; }

    }


}