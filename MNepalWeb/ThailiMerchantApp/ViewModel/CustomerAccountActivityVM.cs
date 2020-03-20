using System;
using System.Collections.Generic;
using ThailiMerchantApp.Models;


namespace ThailiMerchantApp.ViewModel
{
    public class CustomerAccountActivityVM
    {
        public CustReport Parameter { get; set; }
        public List<CustomerAccActivity> CustomerAccActivity { get; set; }

        public List<ResponseLog> ResponseLog { get; set; }

    }
    public class CustomerAccActivity
    {
        public string TransactionType { get; set; }

        public DateTime ReturnDate { get; set; }

       
        public DateTime TransactionDate { get; set; }
        public decimal Credit { get; set; }
        public decimal Debit { get; set; }
       
        public string TranId { get; set; }
        public string ReferenceNo { get; set; }

        public string UserName { get; set; }

        public string Remarks { get; set; }

        public string ErrorMessage { get; set; }

        public decimal Amount { get; set; }

        public string SMSStatus { get; set; }

        public string SMSSenderReply { get; set; }

        public string SMSTimeStamp { get; set; }

        public string DestinationNo { get; set; }

        ///start milayako 002

        public DateTime TranDate { get; set; }

       
        public string TimeStamp { get; set; }
        public string Desc1 { get; set; }

        
        public string Balance { get; set; } //for wallet balance

        public string Balnc { get; set; } //for bank balance

        public string Status { get; set; }  
        //end milayako 002

    }
    public class CustomerLog
    {
        public string ServiceType { get; set; }
        public string Source { get; set; }
        public string Sender { get; set; }
        public string SourceAccountNo { get; set; }
        public string Destination { get; set; }
        public string DestinationAccount { get; set; }
        public string Reciever { get; set; }
        public decimal Amount { get; set; }
        public DateTime TranDate { get; set; }
        public string TraceNo { get; set; }
        public string ResponseCode { get; set; }
        public string Description { get; set; }
    }
    public class CustomerLogVM {
        public CustReport Parameter { get; set; }
        public List<CustomerLog> CustomerLogs { get; set; }

    }
    public class Merchants
    {
        public string Name { get; set; }
        public string ClientCode { get; set; }
    }
    public class MerchantAcDetail{
        public string TransactionType { get; set; }
        public string TransactionDate { get; set; }
        public string Amount { get; set; }
        public string Description { get; set; }
        public string TranId { get; set; }
        public string ReferenceNo { get; set; }
        public string SourceAccountNo { get; set; }
        public string UserName { get; set; }
    }

    public class MerchantAcDetailVM
    {
        public List<MerchantAcDetail> MerchantAcDetails { get; set; }
        public string Balance { get; set; }
        public string Name { get; set; }
    }

    public class ResponseLog
    {
        public string OriginID { get; set; }

        public string OriginType { get; set; }

        public string ServiceCode { get; set; }

        public string SourceBankCode { get; set; }

        public string SourceBranchCode { get; set; }

        public string SourceAccountNo { get; set; }

        public string DestBankCode { get; set; }

        public string DestBranchCode { get; set; }

        public string DestAccountNo { get; set; }

        public decimal Amount { get; set; } //money

        public string FeeId { get; set; }

        public float FeeAmount { get; set; }

        public string TraceNo { get; set; }

        public DateTime TranDate { get; set; }

        public DateTime TranTime { get; set; }

        public string RetrievalRef { get; set; }

        public string ResponseCode { get; set; }

        public string ResponseDescription { get; set; }

        public string Balance { get; set; }

        public string AcHolderName { get; set; }

        public string MiniStmntRec { get; set; }

        public string TranId { get; set; }

        public char ReversalStatus { get; set; }

    }




}


