using System;
using System.Configuration;

namespace MNepalProject.Connection
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