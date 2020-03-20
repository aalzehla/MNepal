using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalProject.Models
{
    public class ResponseMsg
    {
        public int StatusCode
        {
            get;
            set;
        }

        public string StatusMessage
        {
            get;
            set;
        }
    }
}