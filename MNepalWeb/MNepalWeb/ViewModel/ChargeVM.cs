using MNepalWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalWeb.ViewModel
{
    public class ChargeVM
    {
        public MNProfileChargeClass ProfileCharge { get; set; }
        public MNProfileClass CustProfile { get; set; }
        public UserInfo UserInfo { get; set; }
        public List<MNCustChargeLog> ChargeLogs{ get; set; }
        public bool ChargeReg { get; set; }
        public bool ChargePinReset { get; set; }
        public bool ChargeRenew { get; set; }
        public bool IsCharged { get; set; }
        public string ExpiredOn { get; set; }



    }
}