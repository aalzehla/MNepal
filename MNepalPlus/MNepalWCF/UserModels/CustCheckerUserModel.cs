using System;
using System.Data;
using System.Data.SqlClient;
using MNepalProject.Connection;
using MNepalProject.Models;
using MNepalWCF.Models;

namespace MNepalWCF.UserModels
{
    public class CustCheckerUserModel
    {
        #region Customer User Information

        public DataTable GetCustUserCheckInfo(MNClientExt objCustUserInfo)
        {
            DataTable dtableResult = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNLogin]", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserName", objCustUserInfo.UserName);
                        cmd.Parameters.AddWithValue("@Password", "");
                        cmd.Parameters.AddWithValue("@ClientCode", "");
                        cmd.Parameters.AddWithValue("@mode", "GCUN"); //Get Check User Name

                        cmd.CommandType = CommandType.StoredProcedure;

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 60;  //seconds
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset, "dtCustUserInfo");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables["dtCustUserInfo"];
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return dtableResult;
        }

        #endregion

        #region InsertSMSLog

        public bool InsertSMSLog(SMSLog log)
        {
            SqlConnection conn = null;
            SqlTransaction strans = null;
            bool result = false;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {


                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"INSERT INTO MNSMSPinLog(UserName,Message,SentOn,SentBy,Purpose)
                                           Values (@UserName,@Message,@SentOn,@SentBy,@Purpose)";
                    cmd.Parameters.AddWithValue("@UserName", log.UserName);
                    cmd.Parameters.AddWithValue("@Message", log.Message);
                    cmd.Parameters.AddWithValue("@SentOn", log.SentOn);
                    cmd.Parameters.AddWithValue("@SentBy", log.SentBy);
                    cmd.Parameters.AddWithValue("@Purpose", log.Purpose);

                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    cmd.Connection = conn;
                    strans = conn.BeginTransaction();
                    cmd.Transaction = strans;
                    int i = cmd.ExecuteNonQuery();
                    if (i == 1)
                    {
                        strans.Commit();
                        result = true;

                    }
                    else
                    {
                        result = false;
                    }

                    cmd.Dispose();


                }
            }
            catch (Exception ex)
            {
                string ss = ex.Message;
                strans.Rollback();
                result = false;
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();

            }
            return result;
        }

        #endregion

        #region Customer User Status Information

        public DataTable GetCustUserStatus(MNClientExt objCustUserInfo)
        {
            DataTable dtableResult = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNLogin]", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserName", objCustUserInfo.UserName);
                        cmd.Parameters.AddWithValue("@Password", "");
                        cmd.Parameters.AddWithValue("@ClientCode", "");
                        cmd.Parameters.AddWithValue("@mode", "GCSC");

                        cmd.CommandType = CommandType.StoredProcedure;

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 60;  //seconds
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset, "dtCustUserInfo");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables["dtCustUserInfo"];
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return dtableResult;
        }

        #endregion

    }
}