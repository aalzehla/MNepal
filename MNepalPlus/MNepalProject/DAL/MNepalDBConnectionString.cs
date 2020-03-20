using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalProject.DAL
{
    public static class MNepalDBConnectionStringProvider
    {
        public static string GetConnection()
        {
            return "DbConnectionString";
        }
    }
}