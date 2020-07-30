using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ThailiMNepalAgent.Models
{
    public class ISP
    {
        public string ServiceProviderName { get; set; }
        public string CompanyCode { get; set; }
        public string ServiceCode { get; set; }
        public string TestNumbers { get; set; }
        public string Commission { get; set; }



        public string NWCounter { get; set; }
        public string NWBranchCode { get; set; }
        public string CustomerID { get; set; }
        public string TPin { get; set; }
        public string Remarks { get; set; }
        public string da { get; set; }
        public string note { get; set; }
        public string amount { get; set; }
        public string mobile { get; set; }
        public string Months { get; set; }
        public string TransactionMedium { get; set; }
        public string Mode { get; set; }
        public string CustomerName { get; set; }
        public string TotalAmountDue { get; set; }
        public string ClientCode { get; set; }
        public string UserName { get; set; }
        public string refStan { get; set; }
        public string billNumber { get; set; }
        public string responseCode { get; set; }
        public string retrievalReference { get; set; }






    }

    ////For MNPayPointWlinkPayments
    public class MNPayPointISPPayments
    {
        public string CustomerName { get; set; }
        public string NWBranchCode { get; set; }
        public string TotalAmountDue { get; set; }
        public string CustomerID { get; set; }
        public string description { get; set; }
        public string billDate { get; set; }
        public string billAmount { get; set; }
        public string totalAmount { get; set; }
        public string status { get; set; }
        public string amountfact { get; set; }
        public string amountmask { get; set; }
        public string amountmax { get; set; }
        public string amountmin { get; set; }
        public string amountstep { get; set; }
        public string codserv { get; set; }
        public string commission { get; set; }
        public string commisvalue { get; set; }
        public string destination { get; set; }
        public string requestId { get; set; }
        public string showCounter { get; set; }
        public string iCount { get; set; }
        public string legatNumber { get; set; }
        public string discountAmount { get; set; }
        public string counterRent { get; set; }
        public string fineAmount { get; set; }
        public string billDateFrom { get; set; }
        public string billDateTo { get; set; }
        public string payPointType { get; set; }
    }
}