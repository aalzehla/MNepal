using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using WCF.MNepal.Utilities;

namespace WCF.MNepal.Helper
{
    public class TokenGenerator
    {
        public static string Generate()
        {
            int size = 35;
            var charSet = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var chars = charSet.ToCharArray();
            var data = new byte[1];
            var crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            data = new byte[size];
            crypto.GetNonZeroBytes(data);
            var result = new StringBuilder(size);
            foreach (var b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }

        public static bool TokenChecker(string sessionID,string mobile,string src) {
            if (sessionID != null&& sessionID!="")
            {
                if (src == "gprs")
                {
                    // Check session id Android
                    DataTable checkDataTable = CustCheckUtils.SessionChecker(mobile, sessionID, "CSIA"); //mode check session id Android
                    if (checkDataTable.Rows.Count == 1 && checkDataTable.Rows[0]["Result"].ToString() == "1")
                    {
                        return true;
                    }
                }
                else
                {
                    //  Check session id web
                    DataTable checkDataTable = CustCheckUtils.SessionChecker(mobile, sessionID, "CSIW");//mode check session id web
                    if (checkDataTable.Rows.Count == 1 && checkDataTable.Rows[0]["Result"].ToString() == "1")
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}