namespace MNepalWCF.Models
{
    public class MNRemit
    {
        public int RemitID { get; set; }

        public string TraceID { get; set; }
        public string SenderMobileNo { get; set; }

        public string RecipientMobileNo { get; set; }

        public string BeneficialName { get; set; }

        public string RequestTokenCode { get; set; }

        public string Amount { get; set; }

        public string TokenID { get; set; }

        public string Purpose { get; set; }

        public string ClientCode { get; set; }

        public string PIN { get; set; }

        public string ServiceCode { get; set; }

        public string SourceChannel { get; set; }

        public string TokenCreatedDate { get; set; }

        public string TokenExpiryDate { get; set; }

        public string Mode { get; set; }
    }
}