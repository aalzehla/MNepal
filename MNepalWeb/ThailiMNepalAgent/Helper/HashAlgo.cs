using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ThailiMNepalAgent.Helper
{
    public class HashAlgo
    {
        public static string Hash(string password)
        {
            var bytes = new System.Text.UTF8Encoding().GetBytes(password+ "mhichapuchha");
            byte[] hashBytes;
            using (var algorithm = new System.Security.Cryptography.SHA512Managed())
            {
                hashBytes = algorithm.ComputeHash(bytes);
            }
            return Convert.ToBase64String(hashBytes);
        }
    }
}