using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace MNepalWeb.Models
{
    public class ExecuteQuery
    {
        public string Query { get; set; }
        public string TimeOut { get; set; }
        public DataTable Data { get; set; }
        public string Message { get; set; }
    }
}