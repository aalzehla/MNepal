using MNepalWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalWeb.ViewModel
{
    public class BranchVM
    {
        public BranchRep Parameter { get; set; }

        public List<BranchInfo> BranchInfo { get; set; }
    }


    public class BranchInfo
    {
        //According to BranchCode
        //public string UserBranchCode { get; set; }

        //public string NoOfTran { get; set; }

        //public string TotalAmount { get; set; }

        //public string SourceAccountNo { get; set; }

        //public string CreatedBy { get; set; }

        //New Branch Report

        public string BranchName { get; set; }

        public string TotalCustomer { get; set; }

        public string TotalApproveCustomer { get; set; }

        public string TotalUnApproveCustomer { get; set; }

        public string TotalRejectedCustomer { get; set; }
    }
}