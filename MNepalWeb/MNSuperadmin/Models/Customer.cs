using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNSuperadmin.Models
{
    public class MNUser
    {
        public MNClient MNClient { get; set; }
        public MNClientExt MNClientExt { get; set; }
        public MNClientContact MNClientContact { get; set; }
        public MNBankAccountMap MNBankAccountMap { get; set; }
        public List<TransactionInfo> MNTransactionAccounts { get; set; }

        public MNFeeDetail MNFeeDetails { get; set; }

    }
}