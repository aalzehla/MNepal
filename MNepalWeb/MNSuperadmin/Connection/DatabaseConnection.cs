using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace MNSuperadmin.Connection
{
    public class DatabaseConnection
    {
        /// <summary>
        /// Gets the ConnectionString to connection with database.
        /// </summary>
        public static string ConnectionString
        {
            get
            {
               // return ConfigurationManager.ConnectionStrings["MNepalDBConnectionString"].ConnectionString;

                ConnectionStringSettings settings =
                    ConfigurationManager.ConnectionStrings["MNepalDBConnectionString"];
                string connectString = settings.ConnectionString;
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectString);
                if(HttpContext.Current.Session["UserName"]!=null)
                {
                    builder.WorkstationID = HttpContext.Current.Session["UserName"].ToString()+"|"+HttpContext.Current.Session["UserBranch"];
                }

                return builder.ToString();

            }
        }

        /// <summary>
        /// Create the database based on connectionString
        /// </summary>
        /// <returns></returns>
        public static Database GetDatabase()
        {
            return new SqlDatabase(ConnectionString);
        }

        public static string ConnectionStr()
        {
            try
            {
                return ConnectionString;
                // return ConfigurationManager.ConnectionStrings["MNepalDBConnectionString"].ConnectionString;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unable to get the connection String. Please Contact Administrator !!" + ex);
            }
        }

    }
}