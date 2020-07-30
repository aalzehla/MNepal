using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalWeb.Models
{
    public class MNUser
    {
        public MNClient MNClient { get; set; }
        public MNClientKYC MNClientKYC { get; set; }
        public MNClientKYCDoc MNClientKYCDoc { get; set; }
        public MNClientExt MNClientExt { get; set; }
        public MNClientContact MNClientContact { get; set; }
        public MNBankAccountMap MNBankAccountMap { get; set; }
        public List<TransactionInfo> MNTransactionAccounts { get; set; }
    }
}