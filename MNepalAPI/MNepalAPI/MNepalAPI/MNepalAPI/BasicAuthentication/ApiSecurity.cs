using System.Configuration;

namespace MNepalAPI.BasicAuthentication
{
    public class ApiSecurity
    {
        public string AuthenticateAndLoad(string uname, string password)
        {
            string strUName = ConfigurationManager.AppSettings["UserName"];
            string strPassword = ConfigurationManager.AppSettings["Password"];

            if (uname == strUName && password == strPassword)
            {
                return "authenticate";
            }
            else
            {
                return null;
            }
        }
    }
}
