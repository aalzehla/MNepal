using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ThailiMerchantApp.Models
{
    public class TransactionInfo
    {
        public string AcNumber
        {
            get;
            set;
        }

        public string Alias
        {
            get;
            set;
        }

        public string AcOwner
        {
            get;
            set;
        }

        public string IsPrimary
        {
            get;
            set;
        }

        public string AcType
        {
            get;
            set;
        }

        public string TxnEnabled
        {
            get;
            set;
        }

        public string TBranchCode
        {
            get;
            set;
        }


    }
}