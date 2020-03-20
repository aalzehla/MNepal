using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNSuperadmin.Models
{
    public class MerchantTables
    {
        public MNMerchants MNMerchants { get; set; }

        public MNClient MNClient { get; set; }

        public MNClientContact MNClientContact { get; set; }

        public MNClientKYC MNClientKYC { get; set; }

        public MNMerchantKYCDoc MNMerchantKYCDoc { get; set; }

        public MNBankAccountMap MNBankAccountMap { get; set; }
    }
}