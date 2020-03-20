using MNepalProject.Connection;
using System;
using System.Data;
using System.Data.SqlClient;
using MNepalWCF.Models;

namespace MNepalWCF.UserModels
{
    public class CustActivityUserModel
    {
        public int CustRegisterSMSInfo(CustActivityModel objUserSMSInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNCustomerActivitiesInsert]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@UserName", objUserSMSInfo.UserName));
                        sqlCmd.Parameters.AddWithValue("@RequestMerchant", objUserSMSInfo.RequestMerchant);
                        sqlCmd.Parameters.AddWithValue("@DestinationNo", objUserSMSInfo.DestinationNo);
                        sqlCmd.Parameters.AddWithValue("@Amount", objUserSMSInfo.Amount);
                        sqlCmd.Parameters.AddWithValue("@Status", objUserSMSInfo.SMSStatus);
                        sqlCmd.Parameters.AddWithValue("@SMSSenderReply", objUserSMSInfo.SMSSenderReply);
                        sqlCmd.Parameters.AddWithValue("@ErrorMessage", objUserSMSInfo.ErrorMessage);
                        sqlCmd.Parameters.AddWithValue("@Mode", objUserSMSInfo.Mode);

                        sqlCmd.Parameters.Add("@RegIDOut", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@RegIDOut"].Direction = ParameterDirection.Output;

                        sqlCmd.Parameters.Add("@MsgStr", SqlDbType.VarChar, 500);
                        sqlCmd.Parameters["@MsgStr"].Direction = ParameterDirection.Output;

                        ret = sqlCmd.ExecuteNonQuery();
                        if (objUserSMSInfo.Mode.Equals("SCA", StringComparison.InvariantCultureIgnoreCase))
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
            finally
            {
                if (sqlCon != null)
                {
                    sqlCon.Close();
                }
            }
            return ret;
        }


        public int InsertIPInfo(CustActivityModel objUserSMSInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNInsertIP]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@remoteIP", objUserSMSInfo.RemoteIP));
                        sqlCmd.Parameters.AddWithValue("@externalIP", objUserSMSInfo.ExternalIP);
                        sqlCmd.Parameters.AddWithValue("@localIP", objUserSMSInfo.LocalIP);
                        sqlCmd.Parameters.AddWithValue("@mode", objUserSMSInfo.Mode);

                        sqlCmd.Parameters.Add("@RegIDOut", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@RegIDOut"].Direction = ParameterDirection.Output;

                        ret = sqlCmd.ExecuteNonQuery();
                        if (objUserSMSInfo.Mode.Equals("IP", StringComparison.InvariantCultureIgnoreCase))
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
            finally
            {
                if (sqlCon != null)
                {
                    sqlCon.Close();
                }
            }
            return ret;
        }

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

    }
}