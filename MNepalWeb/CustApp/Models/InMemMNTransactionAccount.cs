using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustApp.Models {
    public class InMemMNTransactionAccount {
        public string ClientCode { get; set; }

        public string AcNumber { get; set; }

        public string Alias { get; set; }

        public string AcOwner { get; set; }

        public string IsPrimary { get; set; }

        public string AcType { get; set; }

        public string TxnEnabled { get; set; }

        public string TBranchCode { get; set; }

        public string ModifyingBranch { get; set; }

        public string ModifyingAdmin { get; set; }

        public string Action { get; set; }

        public DateTime? ModifiedOn { get; set; }

    }

}