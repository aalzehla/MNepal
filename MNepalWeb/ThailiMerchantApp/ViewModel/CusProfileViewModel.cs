using ThailiMerchantApp.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ThailiMerchantApp.Models
{
    public class CusProfileViewModel
    {
        public CusProfileViewModel()
        {
            MNFeatures = new List<MNFeatureMasterVM>();
        }
        public String m_ProfileCode;
        public String m_ProfileDesc;
        public String m_ProfileStatus;
        public Nullable<Int32> m_RenewPeriod;
        public String m_AutoRenew;
        public String m_Charge;
        public String m_TxnLimit;
        
        public string m_Registration;
        public string m_ReNew;
        public string m_PinReset;
        public String m_ChargeAccount;

        public String m_HasCharge;
        public String m_IsDrAlert;
        public String m_IsCrAlert;
        public decimal m_MinDrAlertAmt;
        public decimal m_MinCrAlertAmt;

        public IEnumerable<MNFeatureMasterVM> MNFeatures { get; set; }


        public String ProfileCode
        {
            get
            {
                return m_ProfileCode;
            }
            set
            {
                m_ProfileCode = value;
            }
        }
        public String ProfileDesc
        {
            get
            {
                return m_ProfileDesc;
            }
            set
            {
                m_ProfileDesc = value;
            }
        }
        public String ProfileStatus
        {
            get
            {
                return m_ProfileStatus;
            }
            set
            {
                m_ProfileStatus = value;
            }
        }

        public Nullable<Int32> RenewPeriod
        {
            get
            {
                return m_RenewPeriod;
            }
            set
            {
                m_RenewPeriod = value;
            }
        }

        public String AutoRenew
        {
            get
            {
                return m_AutoRenew;
            }
            set
            {
                m_AutoRenew = value;
            }
        }

        public String Charge
        {
            get
            {
                return m_Charge;
            }
            set
            {
                m_Charge = value;
            }
        }

        public String TxnLimit
        {
            get
            {
                return m_TxnLimit;
            }
            set
            {
                m_TxnLimit = value;
            }
        }

             
        public string Registration
        {
            get
            {
                return m_Registration;
            }
            set
            {
                m_Registration = value;
            }
        }

        public string ReNew
        {
            get
            {
                return m_ReNew;
            }
            set
            {
                m_ReNew = value;
            }
        }

        public string PinReset
        {
            get
            {
                return m_PinReset;
            }
            set
            {
                m_PinReset = value;
            }
        }

        public String ChargeAccount
        {
            get
            {
                return m_ChargeAccount;
            }
            set
            {
                m_ChargeAccount = value;
            }
        }

        public String HasCharge
        {
            get
            {
                return m_HasCharge;
            }
            set
            {
                m_HasCharge = value;
            }
        }

        public String IsDrAlert
        {
            get
            {
                return m_IsDrAlert;
            }
            set
            {
                m_IsDrAlert = value;
            }
        }
        public String IsCrAlert
        {
            get
            {
                return m_IsCrAlert;
            }
            set
            {
                m_IsCrAlert = value;
            }
        }
        public Decimal MinDrAlertAmt
        {
            get
            {
                return m_MinDrAlertAmt;
            }
            set
            {
                m_MinDrAlertAmt = value;
            }
        }
        public Decimal MinCrAlertAmt
        {
            get
            {
                return m_MinCrAlertAmt;
            }
            set
            {
                m_MinCrAlertAmt = value;
            }
        }








    }





}
