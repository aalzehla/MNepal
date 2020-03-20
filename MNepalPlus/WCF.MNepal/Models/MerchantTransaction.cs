using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCF.MNepal.Models
{
    public class MerchantTransaction
    {

        public int TranID { get; set; }
        public string MerchantID { get; set; }
        public string MerchantName { get; set; }
        public string MobileNumber { get; set; }
        public int Amount { get; set; }
        public string PAN { get; set; }       
        public string STAN { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate{ get; set; }

        public string ResponseCode { get; set; }

        public string ResponseDescription { get; set; }
        
        public string UserName { get; set; }
        public string CreatedTimeDate { get; set; }

        public MerchantTransaction(string merchantID, string merchantName, string mobileNumber, int amount, string pAN, string sTAN, string status, string createdTimeDate, string responseCode, string responseDescription, string userName)
        {
            MerchantID = merchantID;
            MerchantName = merchantName;
            MobileNumber = mobileNumber;
            Amount = amount;
            PAN = pAN;
            STAN = sTAN;
            Status = status;
            CreatedTimeDate = createdTimeDate;
            ResponseCode = responseCode;
            ResponseDescription = responseDescription;
            UserName = userName;
        }
        public MerchantTransaction(string merchantID, string merchantName, string mobileNumber, int amount, string pAN, string sTAN, string status, DateTime createdDate, string responseCode, string responseDescription, string userName,string createdTimeDate)
        {
            MerchantID = merchantID;
            MerchantName = merchantName;
            MobileNumber = mobileNumber;
            Amount = amount;
            PAN = pAN;
            STAN = sTAN;
            Status = status;
            CreatedDate = createdDate;
            ResponseCode = responseCode;
            ResponseDescription = responseDescription;
            UserName = userName;
            CreatedTimeDate = createdTimeDate;
        }

        public MerchantTransaction()
        {
        }

    }
}