using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalWeb.Models
{
    public class MNProfileChargeClass
    {
        
            private String m_ProfileCode;
            private String m_Registration;
            private String m_ReNew;
            private String m_PinReset;
            private String m_ChargeAccount;



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
            public String Registration
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

            public String ReNew
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

            public String PinReset
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
        }
    }
