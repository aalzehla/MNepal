namespace MNepalWCF.Models
{
    public class ResponseParams
    {
        public int Status { get; set; }
        public dynamic Message { get; set; }

        public int VerificationCode { get; set; }
    }
}