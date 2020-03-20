using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using ThailiMNepalAgent.Connection;
using System.Data.SqlClient;

namespace ThailiMNepalAgent.UserModels
{
    public class NEAUserModel
    {
        public Dictionary<string, string> GetNEAName()
        {
            SqlDataReader rdr;
            SqlConnection conn = null;
            Dictionary<string, string> ListNEA = new Dictionary<string, string>();
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SELECT * FROM MNNEALocation (NOLOCK) order by NEABranchName ASC ";
                        cmd.Connection = conn;
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            string NEAName = rdr["NEABranchName"].ToString();
                            string NEABranch = rdr["NEABranchCode"].ToString();
                            //string NEATypeId = rdr["NeaID"].ToString();
                            ListNEA.Add(NEAName, NEABranch);
                        }
                        if (conn.State != ConnectionState.Closed)
                        {
                            conn.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }

            }
            return ListNEA;
        }
    }
}