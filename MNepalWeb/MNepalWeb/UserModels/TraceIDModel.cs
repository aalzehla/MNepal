using Microsoft.Practices.EnterpriseLibrary.Data;
using MNepalWeb.Connection;
using MNepalWeb.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace MNepalWeb.UserModels
{
    public class TraceIdModel
    {
        /// <summary>
        /// Retrieve the Trace ID based on mode
        /// </summary>
        /// <param name="objMnTraceId">Pass an instance of Trace ID</param>
        /// <returns>Returns the  user information based on Trace ID</returns>
        public DataTable GetMnTraceId(MNTraceID objMnTraceId)
        {
            DataTable dtableResult = null;
            try
            {
                string query = "SELECT TOP 1 id from MNTraceIDHelper ORDER BY id DESC";
                using (SqlConnection sqlCon = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon))
                    {
                        sqlDa.Fill(dtableResult);
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return dtableResult;
        }
    }
}