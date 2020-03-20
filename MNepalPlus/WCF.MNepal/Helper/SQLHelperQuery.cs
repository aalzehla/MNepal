using MNepalProject.Connection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace WCF.MNepal.Helper
{
    public class SQLHelperQuery
    {
        public DataTable AccessQueryMethod(string Query)
        {
            DataTable dt = new DataTable();
            try
            {
                DataSet ds = new DataSet();
                using (SqlConnection c = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    c.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(Query, DatabaseConnection.ConnectionString()))
                    {
                        da.Fill(dt);
                    }
                    c.Close();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return dt;

        }
    }
}