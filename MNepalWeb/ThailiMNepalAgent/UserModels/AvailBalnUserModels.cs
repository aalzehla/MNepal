using Microsoft.Practices.EnterpriseLibrary.Data;
using ThailiMNepalAgent.Connection;
using ThailiMNepalAgent.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ThailiMNepalAgent.UserModels
{
    public class AvailBalnUserModels
    {
        /// <summary>
        /// Retrieve the user information based on mode
        /// </summary>
        /// <param name="objUserInfo">Pass an instance of User information</param>
        /// <returns>Returns the  avail baln information based on user information</returns>
        public DataTable GetUserAvailBaln(UserInfo objUserInfo)
        {
            DataTable dtableResult = null;
            SqlConnection conn = null;
            try
            {   
                using(conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNUserWalletAvailBaln]",conn))
                    {
                        cmd.Parameters.AddWithValue("@ClientCode", objUserInfo.ClientCode);
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 60;  // seconds
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset, "dtUserAvailBaln");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables["dtUserAvailBaln"];
                                }
                            }
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
                if (conn != null)
                {
                    conn.Close();
                }
            }

            return dtableResult;
        }
    }
}