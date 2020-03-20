using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalWeb.Settings
{
    public static class CssSetting
    {
        public static string SuccessMessageClass
        {
            get
            {
                return "success_info";
            }
        }

        public static string FailedMessageClass
        {
            get
            {
                return "failed_info";
            }
        }
    }
}