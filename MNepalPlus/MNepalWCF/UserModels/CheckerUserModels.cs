using MNepalProject.Connection;
using System;
using System.Data;
using System.Data.SqlClient;
using MNepalWCF.Models;

namespace MNepalWCF.UserModels
{
    public class CheckerUserModels
    {
        public DataTable GetRegisterInformation(UserInfo objUserInfo)
        {
            SqlConnection conn = null;
            DataTable dtableResult = null;

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNLogin]", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserName", objUserInfo.UserName);
                        cmd.Parameters.AddWithValue("@Password", objUserInfo.Password);
                        cmd.Parameters.AddWithValue("@ClientCode", objUserInfo.ClientCode);
                        cmd.Parameters.AddWithValue("@mode", objUserInfo.Mode);
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset, "dtRegisterInfo");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables["dtRegisterInfo"];
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
                conn.Close();
            }

            return dtableResult;
        }


        public int RegisterUsersInfo(UserInfo objUserInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNSelfRegistration]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@UserName", objUserInfo.UserName));
                        sqlCmd.Parameters.Add(new SqlParameter("@UserType", objUserInfo.UserType));
                        sqlCmd.Parameters.Add(new SqlParameter("@OTPCode", objUserInfo.OTPCode));
                        sqlCmd.Parameters.Add(new SqlParameter("@Source", objUserInfo.Source));
                        sqlCmd.Parameters.Add(new SqlParameter("@mode", objUserInfo.Mode));

                        ret = sqlCmd.ExecuteNonQuery();
                        if (objUserInfo.Mode.Equals("ISR", StringComparison.InvariantCultureIgnoreCase))
                        {
                            ret = Convert.ToInt32(sqlCmd.Parameters["@RegIDOut"].Value);

                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return ret;
        }


        public DataTable GetRegisterOTPInformation(UserInfo objUserInfo)
        {
            SqlConnection conn = null;
            DataTable dtableResult = null;

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNSelfRegistration]", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserName", objUserInfo.UserName);
                        cmd.Parameters.AddWithValue("@Password", objUserInfo.Password);
                        cmd.Parameters.AddWithValue("@ClientCode", objUserInfo.ClientCode);
                        cmd.Parameters.AddWithValue("@mode", objUserInfo.Mode);
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset, "dtRegisterSyncInfo");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables["dtRegisterSyncInfo"];
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
                conn.Close();
            }

            return dtableResult;
        }

    }
}