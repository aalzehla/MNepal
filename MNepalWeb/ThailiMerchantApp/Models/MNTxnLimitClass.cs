using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ThailiMerchantApp.Models
{
    public class MNTxnLimitClass
    {
        private String m_ProfileCode;
        private String m_FeatureCode;
        private string m_TxnCount;
        private string m_PerTxnAmt;
        private string m_PerDayAmt;
        private string m_TxnAmtM;


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
        public String FeatureCode
        {
            get
            {
                return m_FeatureCode;
            }
            set
            {
                m_FeatureCode = value;
            }
        }
        public string TxnCount
        {
            get
            {
                return m_TxnCount;
            }
            set
            {
                m_TxnCount = value;
            }
        }

        public string PerTxnAmt
        {
            get
            {
                return m_PerTxnAmt;
            }
            set
            {
                m_PerTxnAmt = value;
            }
        }

        public string PerDayAmt
        {
            get
            {
                return m_PerDayAmt;
            }
            set
            {
                m_PerDayAmt = value;
            }
        }

        public string TxnAmtM
        {
            get
            {
                return m_TxnAmtM;
            }
            set
            {
                m_TxnAmtM = value;
            }
        }

    }
}