namespace MNepalWCF.Models
{
    public class UserInfo
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string UserType { get; set; }
        public string ClientCode { get; set; }
        public string OTPCode { get; set; }
        public string Source { get; set; }
        public string Mode { get; set; }

        public UserInfo() { }
        public UserInfo(string UserName, string Password, string userType, string ClientCode)
        {
            //this.tid = tid;
            this.UserName = UserName;
            this.Password = Password;
            this.UserType = userType;
            this.ClientCode = ClientCode;
        }

    }
}