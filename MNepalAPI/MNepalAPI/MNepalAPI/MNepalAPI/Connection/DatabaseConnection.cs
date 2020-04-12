using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace MNepalAPI.Connection
{
    public class DatabaseConnection
    {

        /// <summary>
        /// Gets the ConnectionString to connection with database.
        /// </summary>
        public static string ConnectionString()
        {
            try
            {
                return ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unable to get the connection String. Please Contact Administrator !!" + ex);
            }
        }
    }
}