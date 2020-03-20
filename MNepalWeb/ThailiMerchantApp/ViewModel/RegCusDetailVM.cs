using ThailiMerchantApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ThailiMerchantApp.ViewModel
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


        public string mobileno { get; set; }


        public string TranDate { get; set; }

        public string Debit { get; set; }

        public string Credit { get; set; }

        public string Desc1 { get; set; }

        public string Balance { get; set; }

         
    }


}