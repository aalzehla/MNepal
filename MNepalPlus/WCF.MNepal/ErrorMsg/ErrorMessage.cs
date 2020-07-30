using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCF.MNepal.ErrorMsg
{
    public class ErrorMessage
    {
        public string Error_90 = "ClientCode not found in MNClient Table";
        public string Error_91 = "Could not create account";
        public string Error_92 = "s_MNProcessRequest returned error";
        public string Error_94 = "comm object response parsing error";
        public string Error_95 = "s_MNProcessRequest returned error";
        public string Error_98 = "on E: Exception";
        public string Error_99 = "Timeout reading from server";

        public string Error_AcqBank = "Acquiring Bank return an Error";

        public string Error_111 = "Invalid scheme type";
        public string Error_114 = "Invalid Account Number";

        public string Error_115 = "Requested function not supported";
        public string Error_116 = "Insufficient Balance";

        //public string Error_119 = "Transaction not permitted to card holder";
        public string Error_119 = "Sorry please try again later";
        public string Error_121 = "Your withdrawal amount limit has been exceeded";

        public string Error_163 = "Invalid Cheque Status";
        public string Error_180 = "Your transfer limit has been exceeded";

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

        public string Error_902 = "Invalid Transaction";
        public string Error_904 = "Connection problem. Please try again";
        public string Error_906 = "Connection problem. Please try again"; //"Cut-over in progress";
        public string Error_907 = "Connection problem. Please try again";

        public string Error_909 = "Cannot connect to bank .Please try again later";
        public string Error_911 = "Cannot connect to bank .Please try again later";
        public string Error_913 = "Duplicate transaction";

        public string Error_508 = "Monthly Transaction amount limit reached";
    }
}