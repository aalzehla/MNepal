using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MNSuperadmin.Models;

namespace MNSuperadmin.Models
{
    public class MNClient
    {
        public string ClientCode { get; set; }

        public string Name { get; set; }


        //start milayako 02
        public string PStreet { get; set; }

        public string PProvince { get; set; }

        public string PDistrictID { get; set; }
        //
        public string PMunicipalityVDC { get; set; }
        //
        public string PVDC { get; set; }

        public string PHouseNo { get; set; }

        public string PWardNo { get; set; }

        public string CStreet { get; set; }

        public string CProvince { get; set; }

        public string CDistrictID { get; set; }
        //
        public string CMunicipalityVDC { get; set; }
        //
        public string CVDC { get; set; }

        public string CHouseNo { get; set; }

        public string CWardNo { get; set; }
        //end milayako 02
        //start delete garnu parne 02

        public string Address { get; set; }
        
        public string WardNumber { get; set; }

        public string Province { get; set; }

        public string District { get; set; }
        //end delete garnu parne 02


        //  public string PIN { get; set; }

        public string Status { get; set; }

        public string IsApproved { get; set; }

        public string IsRejected { get; set; }

        public string FName { get; set; }

        public string MName { get; set; }

        public string LName { get; set; }

        public string Gender { get; set; }

        public string ClientStatus { get; set; }

        public string BranchCode { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string ApprovedBy { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public string ProfileCode { get; set; }

        public string MobileNo { get; set; }

        public string IsModified { get; set; }

        public string ModifiedBy { get; set; }

        public string Remarks { get; set; }

        public string RejectedBy { get; set; }

        public string HasKYC { get; set; }

        public Guid ClientGuid { get; set; }

        public string ModifyingBranch { get; set; }

        public string IsCharged { get; set; }

        //Individual Transaction Limit//

        public string IndvTxn { get; set; }

        public string DateRange { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public string TransactionLimit { get; set; }
        public string LimitType { get; set; }
        public string TransactionCount { get; set; }
        public string TransactionLimitMonthly { get; set; }
        public string TransactionLimitDaily { get; set; }

        //For Service Commission

        public string Id { get; set; }
        public string FeeID { get; set; }
        public string TieredStart { get; set; }
        public string TieredEnd { get; set; }
        public string MinAmt { get; set; }
        public string MaxAmt { get; set; }
        public string Percentage { get; set; }
        public string FlatFee { get; set; }
        public string FeeType { get; set; }

    }
}