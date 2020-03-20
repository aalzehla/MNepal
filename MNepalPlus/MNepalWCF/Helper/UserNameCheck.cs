using System.Data;
using MNepalWCF.Utilities;

namespace MNepalWCF.Helper
{
    public class UserNameCheck
    {
        public static bool IsValidUserName(string username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                DataTable dtCheckUserName = CustCheckUtils.GetCustUserInfo(username);
                if (dtCheckUserName.Rows.Count == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
    }
}