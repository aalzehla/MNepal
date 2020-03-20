namespace MNepalWCF.Models
{
    public class CustActivityModel
    {
        public string UserName { get; set; }
        public string RequestMerchant { get; set; }
        public string DestinationNo { get; set; }
        public string Amount { get; set; }
        public string SMSStatus { get; set; }
        public string SMSSenderReply { get; set; }
        public string ErrorMessage { get; set; }
        public string TranDate { get; set; }
        public string SMSTimeStamp { get; set; }
        public string Mode { get; set; }

        //IP
        public string RemoteIP { get; set; }
        public string ExternalIP { get; set; }
        public string LocalIP { get; set; }
    }
}