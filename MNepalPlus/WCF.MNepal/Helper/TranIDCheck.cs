using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WCF.MNepal.Utilities;

namespace WCF.MNepal.Helper
{
    public class TranIDCheck
    {
        public string GetTranIDCheck(string retref)
        {
            string result = string.Empty;

            DataTable dtableCheck = TranIDUtils.GetTraceIDInfo(retref);
            if (dtableCheck.Rows.Count == 0)
            {
                result = "0";
            }
            else if (dtableCheck.Rows.Count > 0)
            {
                result = dtableCheck.Rows[0]["TranId"].ToString();
            }

            return result;
        }
    }
}