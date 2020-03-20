namespace MNepalWCF
{
    public class FundTransfer
    {
       // public FundTransfer fundtransfer;
        /*
        public FundTransfer(FundTransfer fundtransfer)
        {
            this.fundtransfer = fundtransfer;
        }*/

        public string tid { get; set; }
        public string mobile { get; set; }
        public string sc { get; set; }

        public string sa { get; set; }

        public string da { get; set; }

        public string amount { get; set; }

        public string pin { get; set; }
        public string note { get; set; }
        public string sourcechannel { get; set; }
        
        /*
        public bool valid()
        {
            if (fundtransfer.tid != "" && fundtransfer.sc != "" && fundtransfer.mobile != "" && fundtransfer.amount != "")
                return true;
            else
                return false;
        }*/

    }
}