namespace MNepalWCF.ErrorMsg
{
    public class ErrorMessage
    {
        public string Error_99 = "Acquiring Bank time out";
        public string Error_AcqBank = "Acquiring Bank return an Error";

        public string Error_111 = "Invalid scheme type";
        public string Error_114 = "Invalid account number";

        public string Error_115 = "Requested function not supported";
        public string Error_116 = "Insufficient funds";

        public string Error_119 = "Transaction not permitted to card holder";
        public string Error_121 = "Withdrawal amount limit exceeded.";

        public string Error_163 = "Invalid Cheque Status";
        public string Error_180 = "Transfer Limit Exceeded";

        public string Error_181 = "Cheques are in different books";

        public string Error_182 = "Not all cheques could be stopped";
        public string Error_183 = "Cheque not issued to this account";

        public string Error_184 = "Requested Block operation failed since Account is closed/frozen";
        public string Error_185 = "Invalid Currency/Transaction Amount";

        public string Error_186 = "Block does not exist";
        public string Error_187 = "Cheque Stopped";

        public string Error_188 = "Invalid Rate Currency Combination";
        public string Error_189 = "Cheque Book Already Issued";

        public string Error_190 = "DD Already Paid";

        public string Error_800 = "Network message was accepted";

        public string Error_902 = "Invalid transaction";
        public string Error_904 = "Format Error";
        public string Error_906 = "Cut-over in progress";
        public string Error_907 = "Card issuer inoperative";

        public string Error_909 = "System malfunction";
        public string Error_911 = "Card issuer timed out";
        public string Error_913 = "Duplicate transmission";
    }
}