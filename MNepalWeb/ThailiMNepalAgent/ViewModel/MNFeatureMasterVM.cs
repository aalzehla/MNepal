using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ThailiMNepalAgent.ViewModel
{
    public class MNFeatureMasterVM
    {
        public string IsSelected { get; set; }
        public string FeatureCode { get; set; }
        public string FeatureWord { get; set; }
        public string FeatureGroup { get; set; }
        public string FeatureName { get; set; }
        public string CanHaveMultiple { get; set; }
        public string TxnCount { get; set; }
        public string PerDayTxnAmt { get; set; }
        public string PerTxnAmt { get; set; }
        public string TxnAmtM { get; set; }
    }
}