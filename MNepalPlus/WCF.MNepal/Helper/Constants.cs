using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace WCF.MNepal.Helper
{
    public class Constants
    {

        internal static string getTimeStamp()
        {
            return ((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();
        }
        internal static Int64 getLongTimeStamp()
        {
            return ((Int64)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
        }
        internal static string parseDate(string s)
        {
            var t = Convert.ToInt64(s) / 1000;

            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(t).ToLocalTime();
            var date = dt.ToString("dd MMMM yyyy hh mm ");

            return date;

        }

        private Random _random = new Random();
        internal string GenerateRandomNo()
        {
            return _random.Next(0, 999999).ToString("D6");
        }

        internal static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}