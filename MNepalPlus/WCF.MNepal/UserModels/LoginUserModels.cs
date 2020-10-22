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
    public class LoginUserModels
    {
        public DataTable GetUserInformation(UserInfo objUserInfo)
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
                                da.Fill(dataset, "dtUserInfo");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables["dtUserInfo"];
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


        #region CHECK PASSWORD BLOCK TIME 
        public int GetPasswordBlockTime(string username)
        {
            SqlConnection conn = null;
            int ret=1;
            DataTable dtableResult = null;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_BlockUserWrongPwd]", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserName", username);
                        cmd.Parameters.AddWithValue("@mode", "CBT"); //Check Password Tries
                       
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset, "dtUserInfo");
                                if (dataset.Tables.Count > 0)
                                {
                                    ret = 0;
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

            return ret;
        }
        #endregion

        #region Set Password Count
        public int SetPasswordCount(string userName,string mode) //mode 
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_BlockUserWrongPwd]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@UserName", userName));

                        sqlCmd.Parameters.AddWithValue("@mode", mode); //Set Password Count

                        //sqlCmd.Parameters.Add("@UIDOut", SqlDbType.Char, 500);
                        //sqlCmd.Parameters["@UIDOut"].Direction = ParameterDirection.Output;

                        ret = sqlCmd.ExecuteNonQuery();
                        //ret = Convert.ToInt32(sqlCmd.Parameters["@UIDOut"].Value);
                    }

                }
            }
            catch (Exception ex)
            {

                throw;
            }
            finally
            {
                if (sqlCon != null)
                {
                    sqlCon.Close();
                }
            }
            return ret;
        }
        #endregion


        //FOR PIN
        #region Set PIN Count
        public int SetPINCount(string userName, string mode) //mode 
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNBlockUserWrongPin]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@UserName", userName));

                        sqlCmd.Parameters.AddWithValue("@mode", mode); //Set Password Count

                        //sqlCmd.Parameters.Add("@UIDOut", SqlDbType.Char, 500);
                        //sqlCmd.Parameters["@UIDOut"].Direction = ParameterDirection.Output;

                        ret = sqlCmd.ExecuteNonQuery();
                        //ret = Convert.ToInt32(sqlCmd.Parameters["@UIDOut"].Value);
                    }

                }
            }
            catch (Exception ex)
            {

                throw;
            }
            finally
            {
                if (sqlCon != null)
                {
                    sqlCon.Close();
                }
            }
            return ret;
        }
        #endregion


        #region CHECK PIN BLOCK TIME 
        public int GetPINBlockTime(string username)
        {
            SqlConnection conn = null;
            int ret = 1;
            DataTable dtableResult = null;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNBlockUserWrongPin]", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserName", username);
                        cmd.Parameters.AddWithValue("@mode", "CBT"); //Check Password Tries

                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset, "dtUserInfo");
                                if (dataset.Tables.Count > 0)
                                {
                                    ret = 0;
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

            return ret;
        }
        #endregion

        #region Check OTP
        public DataTable CheckOTP(UserInfo objUserInfo)
        {
            SqlConnection conn = null;
            DataTable dtableResult = null;

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNCheckOTP]", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserName", objUserInfo.UserName);
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset, "dtUserInfo");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables["dtUserInfo"];
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
        #endregion


        #region GET MESSAGE 
        public string GetMessage(string MsgID)
        {
            SqlConnection conn = null;
            string ret = "";
            DataTable dtableResult = null;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNGetMessage]", conn))
                    {
                        cmd.Parameters.AddWithValue("@MsgID", MsgID);
                    
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset, "dtUserInfo");
                                if (dataset.Tables.Count > 0)
                                {
                                    DataTable TpData = dataset.Tables[0];
                                    if (TpData.Rows.Count > 0)
                                    {
                                        foreach (DataRow dr in TpData.Rows)
                                        {
                                            ret = dr["MsgCodeDesc"].ToString();
                                        }
                                    }
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

            return ret;
        }
        #endregion

    }
}