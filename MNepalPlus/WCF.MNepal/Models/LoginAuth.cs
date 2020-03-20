using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCF.MNepal.Models
{
    public class LoginAuth
    {
        public string UserName
        {
            get;
            set;
        }

        public string UserType
        {
            get;
            set;
        }

        public int VerificationCode
        {
            get;
            set;
        }

        public string IsFirstLogin
        {
            get;
            set;
        }
        public string ClientCode
        {
            get;
            set;
        }
        public string TPin
        {
            get;
            set;
        }

        public string Password
        {
            get;
            set;
        }

        public int Status
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }

        public string Mode
        {
            get;
            set;
        }
        public string HasKYC
        {
            get;
            set;
        }

        public string BankAccountNo
        {
            get;
            set;
        }


        public string HasBankKYC
        {
            get;
            set;
        }

        public string IsRejected
        {
            get;
            set;
        }

        public string Remarks
        {
            get;
            set;
        }
        public string PinChanged
        {
            get;
            set;
        }

        public string PassChanged
        {
            get;
            set;
        }
        public string Token
        {
            get;
            set;
        }

        public string catid
        {
            get;
            set;
        }
        public string Name1
        {
            get;
            set;
        }
        public string mname
        {
            get;
            set;
        }
        public string mid
        {
            get;
            set;
        }
    }
}