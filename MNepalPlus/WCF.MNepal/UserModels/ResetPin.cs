using MNepalProject.Connection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using WCF.MNepal.Models;

namespace WCF.MNepal.UserModels
{
    public class ResetPin
    {
        public DataTable GetUserName(PinChange objUserInfo)
        {
            DataTable dtableResult = null;
            SqlConnection conn = null;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNResetPin]", conn))
                    {
                        cmd.Parameters.AddWithValue("@ClientCode", objUserInfo.ClientCode);
                        cmd.Parameters.AddWithValue("@UserName", objUserInfo.mobile);
                        cmd.Parameters.AddWithValue("@Password", "");
                        cmd.Parameters.AddWithValue("@userType", "");
                        cmd.Parameters.AddWithValue("@ProfileName", "");
                        cmd.Parameters.AddWithValue("@PushSMS", "");
                        cmd.Parameters.AddWithValue("@COC", "");
                        cmd.Parameters.AddWithValue("@PIN", "");
                        cmd.Parameters.AddWithValue("@IMEINo", "");
                        cmd.Parameters.AddWithValue("@Mode", objUserInfo.Mode);
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 60;  // seconds
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset, "dtGetUserName");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables["dtGetUserName"];
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