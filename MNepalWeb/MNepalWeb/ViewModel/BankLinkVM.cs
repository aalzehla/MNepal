using MNepalWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalWeb.ViewModel
{
    public class BankLinkVM
    {
       
            public BankLink Parameter { get; set; }

            public List<BankLinkInfo> BankLinkInfo { get; set; }
    }



    public class BankLinkInfo
    {
            public string RequestedDate { get; set; }

            public string MobileNo { get; set; }

            public string CustomerName { get; set; }

            public string BankAccNo { get; set; }

            public string Status { get; set; }

            public string VerifiedBy { get; set; }

            public string VerifiedDate { get; set; }

            public string ApprovedBy { get; set; }

            public string ApprovedDate { get; set; }
        
    }
   
}