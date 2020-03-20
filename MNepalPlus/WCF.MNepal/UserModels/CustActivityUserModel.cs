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


        #region "Insert Customer Support Form "

        public int InsertCustSupportForm(CustomerSupport objUserInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNCustomerSupport]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.AddWithValue("@MobileNumber", objUserInfo.MobileNumber);
                        sqlCmd.Parameters.AddWithValue("@Name", objUserInfo.Name);
                        sqlCmd.Parameters.AddWithValue("@Remarks", objUserInfo.Remarks);
                        sqlCmd.Parameters.AddWithValue("@Email", objUserInfo.Email);
                        sqlCmd.Parameters.AddWithValue("@Category", objUserInfo.Category);
                        sqlCmd.Parameters.AddWithValue("@ImageName", objUserInfo.UploadedImageName);
                        sqlCmd.Parameters.AddWithValue("@mode", "IICST");

                        sqlCmd.Parameters.Add("@RegIDOut", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@RegIDOut"].Direction = ParameterDirection.Output;

                        ret = sqlCmd.ExecuteNonQuery();

                        ret = Convert.ToInt32(sqlCmd.Parameters["@RegIDOut"].Value);

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

        #endregion

        #region "InsertMerchantTransaction"

        public int InsertMerchantTransaction(MerchantTransaction objUserInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNInsertIntoMerchantTransaction]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.AddWithValue("@MobileNumber", objUserInfo.MobileNumber);
                        sqlCmd.Parameters.AddWithValue("@MerchantID", objUserInfo.MerchantID);
                        sqlCmd.Parameters.AddWithValue("@MerchantName", objUserInfo.MerchantName);
                        sqlCmd.Parameters.AddWithValue("@PAN", objUserInfo.PAN);
                        sqlCmd.Parameters.AddWithValue("@STAN", objUserInfo.STAN);
                        sqlCmd.Parameters.AddWithValue("@Amount", objUserInfo.Amount);
                        sqlCmd.Parameters.AddWithValue("@CreatedDate", objUserInfo.CreatedDate);
                        sqlCmd.Parameters.AddWithValue("@Status", objUserInfo.Status);
                        sqlCmd.Parameters.AddWithValue("@ResponseCode", objUserInfo.ResponseCode);
                        sqlCmd.Parameters.AddWithValue("@ResponseDescription", objUserInfo.ResponseDescription);
                        sqlCmd.Parameters.AddWithValue("@UserName", objUserInfo.UserName);
                        sqlCmd.Parameters.AddWithValue("@mode", "IIMTS");

                        sqlCmd.Parameters.Add("@RegIDOut", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@RegIDOut"].Direction = ParameterDirection.Output;

                        ret = sqlCmd.ExecuteNonQuery();

                        ret = Convert.ToInt32(sqlCmd.Parameters["@RegIDOut"].Value);

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

        #endregion

        #region "UpdateStatusMerchantTransaction "

        public int UpdateStatusMerchantTransaction(MerchantTransaction objUserInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNInsertIntoMerchantTransaction]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.AddWithValue("@MobileNumber", objUserInfo.MobileNumber);
                        sqlCmd.Parameters.AddWithValue("@MerchantID", objUserInfo.MerchantID);
                        sqlCmd.Parameters.AddWithValue("@MerchantName", objUserInfo.MerchantName);
                        sqlCmd.Parameters.AddWithValue("@PAN", objUserInfo.PAN);
                        sqlCmd.Parameters.AddWithValue("@STAN", objUserInfo.STAN);
                        sqlCmd.Parameters.AddWithValue("@Amount", objUserInfo.Amount);
                        sqlCmd.Parameters.AddWithValue("@CreatedDate", objUserInfo.CreatedDate);
                        sqlCmd.Parameters.AddWithValue("@Status", objUserInfo.Status);
                        sqlCmd.Parameters.AddWithValue("@ResponseCode", objUserInfo.ResponseCode);
                        sqlCmd.Parameters.AddWithValue("@ResponseDescription", objUserInfo.ResponseDescription);
                        sqlCmd.Parameters.AddWithValue("@UserName", objUserInfo.UserName);
                        sqlCmd.Parameters.AddWithValue("@mode", "USIMTS");

                        sqlCmd.Parameters.Add("@RegIDOut", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@RegIDOut"].Direction = ParameterDirection.Output;

                        ret = sqlCmd.ExecuteNonQuery();

                        ret = Convert.ToInt32(sqlCmd.Parameters["@RegIDOut"].Value);

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

        #endregion

        #region Insert Merchant Transaction Detail
        public int InsertMerchantTransactionDetail(FundTransfer fundTransfer)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNInsertMerchantTransactionDetail]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.AddWithValue("@studName", fundTransfer.studName);
                        sqlCmd.Parameters.AddWithValue("@merchantName", fundTransfer.da);
                        sqlCmd.Parameters.AddWithValue("@class", fundTransfer.clss);
                        sqlCmd.Parameters.AddWithValue("@year", fundTransfer.year);
                        sqlCmd.Parameters.AddWithValue("@month", fundTransfer.month);
                        sqlCmd.Parameters.AddWithValue("@rollNo", fundTransfer.rollNo);
                        sqlCmd.Parameters.AddWithValue("@billNo", fundTransfer.billNo);
                        sqlCmd.Parameters.AddWithValue("@amount", fundTransfer.amount);
                        sqlCmd.Parameters.AddWithValue("@remarks", fundTransfer.remarks);
                        sqlCmd.Parameters.AddWithValue("@agentName", fundTransfer.agentName);
                        sqlCmd.Parameters.AddWithValue("@userName", fundTransfer.mobile);
                        sqlCmd.Parameters.AddWithValue("@mode", fundTransfer.merchantType);

                        sqlCmd.Parameters.Add("@RegIDOut", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@RegIDOut"].Direction = ParameterDirection.Output;

                        ret = sqlCmd.ExecuteNonQuery();

                        ret = Convert.ToInt32(sqlCmd.Parameters["@RegIDOut"].Value);

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
        #endregion

    }
}