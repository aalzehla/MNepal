using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ThailiMerchantApp.Models
{
    public class MNProfileClass
    {
        private String m_ProfileCode;
        private String m_ProfileDesc;
        private String m_ProfileStatus;
        private Nullable<Int32> m_RenewPeriod;
        private String m_AutoRenew;
        private String m_Charge;
        private String m_TxnLimit;

        private String m_HasCharge;
        private String m_IsDrAlert;
        private String m_IsCrAlert;
        private decimal m_MinDrAlertAmt;
        private decimal m_MinCrAlertAmt;

        public MNProfileClass() { }

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