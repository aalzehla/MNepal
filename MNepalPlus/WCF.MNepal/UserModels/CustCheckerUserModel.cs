using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using MNepalProject.Connection;
using MNepalProject.Models;
using WCF.MNepal.Models;

namespace WCF.MNepal.UserModels
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
        #region Customer User Information

        public DataTable SessionChecker(string userName,string sessionID,string mode)
        {
            DataTable dtableResult = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNCheckSessionID]", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserName", userName);                     
                        cmd.Parameters.AddWithValue("@SessionID", sessionID);                     
                        cmd.Parameters.AddWithValue("@mode", mode); //Check Session ID

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
        #region Customer Merchant User Information

        public DataTable GetMerchantUserCheckInfo(MNClientExt objCustUserInfo)
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
                        cmd.Parameters.AddWithValue("@mode", "GMC"); //Get Check User Name

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

        #region Customer User Status Information

        public string CheckAlreadyRequestedAccount(string UserName)
        {
            string result = "false";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNCheckBankLink]", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserName", UserName);
                        cmd.Parameters.AddWithValue("@mode", "CBR");
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
                                    DataTable dtableResult = dataset.Tables["dtCustUserInfo"];
                                    if (dtableResult.Rows.Count == 0)
                                    {
                                        result = "true";
                                    }

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

            return result;
        }

        #endregion

        #region Customer Block User Information

        public DataTable GetCustBlockedUserInfo(MNClientExt objCustUserInfo)
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
                        cmd.Parameters.AddWithValue("@mode", "GCBUN"); //Get Check Blocked User Name

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

        #region Bank Link Customer User Information

        public DataTable GetCustUserInfo(string MobileNumber)
        {
            DataTable dtableResult = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNLogin]", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserName", MobileNumber);
                        cmd.Parameters.AddWithValue("@Password", "");
                        cmd.Parameters.AddWithValue("@ClientCode", "");
                        cmd.Parameters.AddWithValue("@mode", "GCBUN"); //Get Check Blocked User Name

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

        #region Quick Register Customer KYC Information

        public DataTable GetQRCustKYCInfoUserModel(MNClientExt objCustUserInfo)
        {
            DataTable dtableResult = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNQuickCustRegKYCInfo]", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserName", objCustUserInfo.UserName);
                        cmd.Parameters.AddWithValue("@mode", "GQCKI"); //GET Quick Customer KYC Info

                        cmd.CommandType = CommandType.StoredProcedure;

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 60;  //seconds
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset, "dtCustKYCInfo");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables["dtCustKYCInfo"];
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

        #region Customer EBanking Detail Get MobileNumber From PRN

        public string GetMobileNumberFromPRN(string mode, string PRN)
        {
            string dtableResult = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNCheckEBankingRequest]", conn))
                    {
                        cmd.Parameters.AddWithValue("@PRN", PRN);
                        cmd.Parameters.AddWithValue("@mode", mode);

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
                                    DataTable dtable = dataset.Tables["dtCustUserInfo"];
                                    foreach (DataRow dr in dtable.Rows)
                                    {
                                        dtableResult = dtable.Rows[0]["ContactNumber1"].ToString();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                dtableResult = null;
            }

            return dtableResult;
        }

        #endregion
        
        #region Customer EBanking Detail

        public DataTable GetEBankingRequest(string mode, string PRN)
        { 
             DataTable dtable= null;

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNCheckEBankingRequest]", conn))
                    {
                        cmd.Parameters.AddWithValue("@PRN", PRN);
                        cmd.Parameters.AddWithValue("@mode", mode);

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
                                     dtable = dataset.Tables["dtCustUserInfo"];
                                  
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

            return dtable;
        }

        #endregion

        #region InsertEBankingResponse

        public int InsertEBankingResponse(SoapTransaction soapTransaction, string statusCode)
        {
            int ret;

            try
            {

                using (SqlConnection sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNEBankingResponse]", sqlCon))

                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@PaymentReferenceNumber", soapTransaction.PRN));
                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", soapTransaction.PRN)); // 
                        sqlCmd.Parameters.Add(new SqlParameter("@ItemCode", soapTransaction.ITC));
                        sqlCmd.Parameters.Add(new SqlParameter("@Amount", soapTransaction.Amount));
                        sqlCmd.Parameters.Add(new SqlParameter("@BankID", soapTransaction.BankID));
                        sqlCmd.Parameters.Add(new SqlParameter("@BID", soapTransaction.BID));
                        sqlCmd.Parameters.Add(new SqlParameter("@FirstName", soapTransaction.FirstName));
                        sqlCmd.Parameters.Add(new SqlParameter("@LastName", soapTransaction.LastName));
                        sqlCmd.Parameters.Add(new SqlParameter("@EmailID", soapTransaction.Email));
                        sqlCmd.Parameters.Add(new SqlParameter("@AMT1", soapTransaction.AMT1));
                        sqlCmd.Parameters.Add(new SqlParameter("@UID", soapTransaction.UID));
                        sqlCmd.Parameters.Add(new SqlParameter("@ResponseCode", statusCode));

                        //sqlCmd.Parameters.Add(new SqlParameter("@BIDSYS", soapTransaction.));
                        //sqlCmd.Parameters.Add(new SqlParameter("@Mobile", soapTransaction.));
                        //sqlCmd.Parameters.Add(new SqlParameter("@MID", soapTransaction.PRN));

                        sqlCmd.Parameters.AddWithValue("@mode", "IEBR");

                        sqlCmd.Parameters.Add("@RegIDOut", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@RegIDOut"].Direction = ParameterDirection.Output;

                        ret = sqlCmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return ret;
        }

        #endregion

        #region InsertRetrievalReference

        public int InsertRetrievalReference(SoapTransaction soapTransaction, string retRef)
        {
            int ret;

            try
            {

                using (SqlConnection sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNInsertRetRef]", sqlCon))

                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@PaymentReferenceNumber", soapTransaction.PRN));
                        sqlCmd.Parameters.Add(new SqlParameter("@RetrievalReference", retRef));

                        sqlCmd.Parameters.Add("@RegIDOut", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@RegIDOut"].Direction = ParameterDirection.Output;

                        ret = sqlCmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return ret;
        }

        #endregion

        #region Customer EBanking Response Detail

        public DataTable GetEBankingResponse(string mode, string PRN)
        {
            DataTable dtable = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNCheckEBankingResponse]", conn))
                    {
                        cmd.Parameters.AddWithValue("@PRN", PRN);
                        cmd.Parameters.AddWithValue("@mode", mode);

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
                                    dtable = dataset.Tables["dtCustUserInfo"];

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

            return dtable;
        }

        #endregion

        #region Customer GetCheckBankAccount Commented

        //public DataTable GetCheckBankAccount(string checkAccountNo)
        //{
        //    DataTable dtableResult = null;

        //    try
        //    {
        //        using (SqlConnection conn = new SqlConnection(DatabaseConnection.ConnectionString()))
        //        {
        //            conn.Open();
        //            using (SqlCommand cmd = new SqlCommand("[s_MNCheckBankAccount]", conn))
        //            {
        //                cmd.Parameters.AddWithValue("@BankAccountNo", checkAccountNo);

        //                cmd.Parameters.AddWithValue("@mode", "GCBA"); //Get Check User Name

        //                cmd.CommandType = CommandType.StoredProcedure;

        //                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
        //                {
        //                    // set the CommandTimeout
        //                    da.SelectCommand.CommandTimeout = 60;  //seconds
        //                    using (DataSet dataset = new DataSet())
        //                    {
        //                        da.Fill(dataset, "dtCustUserInfo");
        //                        if (dataset.Tables.Count > 0)
        //                        {
        //                            dtableResult = dataset.Tables["dtCustUserInfo"];
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.ToString());
        //    }

        //    return dtableResult;
        //}

        #endregion

        #region Customer Agent Information

        public DataTable GetAgentCheckInfo(MNClientExt objCustUserInfo)
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
                        cmd.Parameters.AddWithValue("@mode", "GAC"); //Get Check User Name

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

        #region Customer Merchant Information

        public DataTable GetMerchantCheckInfo(MNClientExt objCustUserInfo)
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
                        cmd.Parameters.AddWithValue("@mode", "GMC"); //Get Check User Name

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


        #region Customer Agent Information

        public DataTable GetUserCheckInfo(MNClientExt objCustUserInfo)
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
                        cmd.Parameters.AddWithValue("@mode", "GUC"); //Get Check User Name

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