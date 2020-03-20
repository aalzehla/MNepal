using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WCF.MNepal.Utilities;

namespace WCF.MNepal.Helper
{
    public class TraceIdCheck
    {
        public static bool IsValidTraceId(string traceID)
        {
            if (!string.IsNullOrEmpty(traceID))
            {
                DataTable dtTraceID = TraceIdCheckUtils.GetTraceIDInfo(traceID);
                if (dtTraceID.Rows.Count == 1)
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