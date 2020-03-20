using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNSuperadmin.Models
{
    public class LimitWallet
    {
        public string WalletProfileCode { get; set; }

        public string WTxnCount { get; set; }

        public string WPerTxnAmt { get; set; }

        public string WPerDayAmt { get; set; }

        public string WTxnAmtM { get; set; }

        public string Remarks { get; set; }

        public string UserType { get; set; }

        public string Mode { get; set; }

    }
}