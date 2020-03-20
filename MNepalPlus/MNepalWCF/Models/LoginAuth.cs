namespace MNepalWCF.Models
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
    }
}