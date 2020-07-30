using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;
using Microsoft.Practices.EnterpriseLibrary.Data;
using MNSuperadmin.Connection;
using MNSuperadmin.Helper;
using MNSuperadmin.Models;
using MNSuperadmin.ViewModel;

namespace MNSuperadmin.UserModels
{
    public class ManualTransactionUserModel
    {
        #region "Verify EBanking Transaction"

        public int verifyManualEBankingTrans(EBankingManualTran eBankingManualTran, string userName)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNEBankingManualTran]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        
                        sqlCmd.Parameters.AddWithValue("@PRN", eBankingManualTran.PRN);
                        sqlCmd.Parameters.AddWithValue("@Amount", eBankingManualTran.Amount);
                        sqlCmd.Parameters.AddWithValue("@BID", eBankingManualTran.BID);
                        sqlCmd.Parameters.AddWithValue("@Status", eBankingManualTran.Status);
                        sqlCmd.Parameters.AddWithValue("@userName", userName);
                        sqlCmd.Parameters.AddWithValue("@mode", "EVMT");

                        sqlCmd.Parameters.Add("@MesgStr", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@MesgStr"].Direction = ParameterDirection.Output;

                        sqlCmd.Parameters.Add("@return_value", SqlDbType.Int);
                        sqlCmd.Parameters["@return_value"].Direction = ParameterDirection.ReturnValue;

                        sqlCmd.ExecuteNonQuery();
                        string i = sqlCmd.Parameters["@MesgStr"].Value.ToString();
                        ret = Convert.ToInt32(sqlCmd.Parameters["@return_value"].Value);

                        //sqlCmd.Parameters["@return_value"].Direction = ParameterDirection.ReturnValue;

                        //sqlCmd.ExecuteNonQuery();
                        //string i = sqlCmd.Parameters["@MesgStr"].Value.ToString();
                        //ret = Convert.ToInt32(sqlCmd.Parameters["@return_value"].Value);
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

        #region View Verified List for approval/rejection
        public List<EBankingTran> EBankingVerifiedTranDetails(MerchantPay Mpay)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            List<EBankingTran> ListRec = new List<EBankingTran>();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNEBankingApproveTranDetails", conn))
                    {

                        cmd.Parameters.AddWithValue("@StartDate", Mpay.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", Mpay.EndDate);
                        cmd.Parameters.AddWithValue("@SourceMobileNo", Mpay.SourceMobileNo);
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 0;  // seconds
                            da.Fill(dataset);
                        }
                    }
                }
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }


                if (dataset.Tables.Count > 0)
                {
                    DataTable TpData = dataset.Tables[0];
                    if (TpData.Rows.Count > 0)
                    {
                        foreach (DataRow dr in TpData.Rows)
                        {
                            EBankingTran ET = new EBankingTran();
                            //ET.EBDate = DateTime.ParseExact(dr["EBDate"].ToString(), "yyyy/MM/dd hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                            ET.EBDate = dr["EBDate"].ToString();
                            ET.UserName = dr["UserName"].ToString();
                            ET.PaymentReferenceNumber = dr["PaymentReferenceNumber"].ToString();
                            ET.VerifiedBy = dr["VerifiedBy"].ToString();
                            ET.Amount = decimal.Parse(dr["Amount"].ToString().Trim() == "" ? "0" : dr["Amount"].ToString());
                            ET.VerifiedDate = dr["VerifiedDate"].ToString();
                            ET.ReferenceNo = dr["ReferenceNo"].ToString();
                            ListRec.Add(ET);

                        }

                        HttpContext.Current.Session["MerchantInfo"] = TpData;
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
                dataset.Dispose();
            }

            return ListRec;
        }
        #endregion

        #region View Rejected List
        public List<EBankingTran> EBankingRejectedTranDetails(MerchantPay Mpay)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            List<EBankingTran> ListRec = new List<EBankingTran>();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNEBankingRejectedTranDetails", conn))
                    {

                        cmd.Parameters.AddWithValue("@StartDate", Mpay.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", Mpay.EndDate);
                        cmd.Parameters.AddWithValue("@SourceMobileNo", Mpay.SourceMobileNo);
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 0;  // seconds
                            da.Fill(dataset);
                        }
                    }
                }
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }


                if (dataset.Tables.Count > 0)
                {
                    DataTable TpData = dataset.Tables[0];
                    if (TpData.Rows.Count > 0)
                    {
                        foreach (DataRow dr in TpData.Rows)
                        {
                            EBankingTran ET = new EBankingTran();
                            //ET.EBDate = DateTime.ParseExact(dr["EBDate"].ToString(), "yyyy/MM/dd hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                            ET.EBDate = dr["EBDate"].ToString();
                            ET.UserName = dr["UserName"].ToString();
                            ET.PaymentReferenceNumber = dr["PaymentReferenceNumber"].ToString();
                            ET.RejectedBy = dr["RejectedBy"].ToString();
                            ET.Amount = decimal.Parse(dr["Amount"].ToString().Trim() == "" ? "0" : dr["Amount"].ToString());
                            ET.RejectedDate = dr["RejectedDate"].ToString();
                            ET.ReferenceNo = dr["ReferenceNo"].ToString();
                            ListRec.Add(ET);

                        }

                        HttpContext.Current.Session["MerchantInfo"] = TpData;
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
                dataset.Dispose();
            }

            return ListRec;
        }
        #endregion

        #region "Reject EBanking Transaction"

        public int RejectManualEBankingTrans(EBankingManualTran eBankingManualTran, string userName)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNEBankingManualTran]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.AddWithValue("@PRN", eBankingManualTran.PRN);
                        sqlCmd.Parameters.AddWithValue("@Amount", eBankingManualTran.Amount);
                        sqlCmd.Parameters.AddWithValue("@userName", userName);
                        sqlCmd.Parameters.AddWithValue("@RejectRemarks", eBankingManualTran.Remarks);
                        sqlCmd.Parameters.AddWithValue("@mode", "ERMT"); //EBanking Reject Manual Tran

                        sqlCmd.Parameters.Add("@MesgStr", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@MesgStr"].Direction = ParameterDirection.Output;

                        sqlCmd.Parameters.Add("@return_value", SqlDbType.Int);
                        sqlCmd.Parameters["@return_value"].Direction = ParameterDirection.ReturnValue;

                        sqlCmd.ExecuteNonQuery();
                        string i = sqlCmd.Parameters["@MesgStr"].Value.ToString();
                        ret = Convert.ToInt32(sqlCmd.Parameters["@return_value"].Value);

                        //sqlCmd.Parameters["@return_value"].Direction = ParameterDirection.ReturnValue;

                        //sqlCmd.ExecuteNonQuery();
                        //string i = sqlCmd.Parameters["@MesgStr"].Value.ToString();
                        //ret = Convert.ToInt32(sqlCmd.Parameters["@return_value"].Value);
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

        #region "Approve EBanking Transaction"

        public int ApproveManualEBankingTrans(EBankingManualTran eBankingManualTran, string userName)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNEBankingManualTran]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.AddWithValue("@PRN", eBankingManualTran.PRN);
                        sqlCmd.Parameters.AddWithValue("@Amount", eBankingManualTran.Amount);
                        sqlCmd.Parameters.AddWithValue("@userName", userName);
                        sqlCmd.Parameters.AddWithValue("@mode", "EAMT"); //EBanking Approve Manual Tran

                        sqlCmd.Parameters.Add("@MesgStr", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@MesgStr"].Direction = ParameterDirection.Output;

                        sqlCmd.Parameters.Add("@return_value", SqlDbType.Int);
                        sqlCmd.Parameters["@return_value"].Direction = ParameterDirection.ReturnValue;

                        sqlCmd.ExecuteNonQuery();
                        string i = sqlCmd.Parameters["@MesgStr"].Value.ToString();
                        ret = Convert.ToInt32(sqlCmd.Parameters["@return_value"].Value);

                        //sqlCmd.Parameters["@return_value"].Direction = ParameterDirection.ReturnValue;

                        //sqlCmd.ExecuteNonQuery();
                        //string i = sqlCmd.Parameters["@MesgStr"].Value.ToString();
                        //ret = Convert.ToInt32(sqlCmd.Parameters["@return_value"].Value);
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

        #region "Re Verify EBanking Transaction"

        public int ReVerifyManualEBankingTrans(EBankingManualTran eBankingManualTran, string userName)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNEBankingManualTran]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.AddWithValue("@PRN", eBankingManualTran.PRN);
                        sqlCmd.Parameters.AddWithValue("@Amount", eBankingManualTran.Amount);
                        sqlCmd.Parameters.AddWithValue("@userName", userName);
                        sqlCmd.Parameters.AddWithValue("@mode", "ERVMT");

                        sqlCmd.Parameters.Add("@MesgStr", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@MesgStr"].Direction = ParameterDirection.Output;

                        sqlCmd.Parameters.Add("@return_value", SqlDbType.Int);
                        sqlCmd.Parameters["@return_value"].Direction = ParameterDirection.ReturnValue;

                        sqlCmd.ExecuteNonQuery();
                        string i = sqlCmd.Parameters["@MesgStr"].Value.ToString();
                        ret = Convert.ToInt32(sqlCmd.Parameters["@return_value"].Value);

                        //sqlCmd.Parameters["@return_value"].Direction = ParameterDirection.ReturnValue;

                        //sqlCmd.ExecuteNonQuery();
                        //string i = sqlCmd.Parameters["@MesgStr"].Value.ToString();
                        //ret = Convert.ToInt32(sqlCmd.Parameters["@return_value"].Value);
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

        #region Topup Manual Transaction
        #region Top Up Manual Verify Transaction Details
        public List<TopUpManualTran> TopUpManualVerifyList(MerchantPay Mpay)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            List<TopUpManualTran> ListRec = new List<TopUpManualTran>();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNTopupVerifyList", conn))
                    {

                        cmd.Parameters.AddWithValue("@StartDate", Mpay.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", Mpay.EndDate);
                        cmd.Parameters.AddWithValue("@SourceMobileNo", Mpay.SourceMobileNo);
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 0;  // seconds
                            da.Fill(dataset);
                        }
                    }
                }
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }


                if (dataset.Tables.Count > 0)
                {
                    DataTable TpData = dataset.Tables[0];
                    if (TpData.Rows.Count > 0)
                    {
                        foreach (DataRow dr in TpData.Rows)
                        {
                            TopUpManualTran TT = new TopUpManualTran();
                            //TT.Date = DateTime.ParseExact(dr["EnteredAt"].ToString(), "yyyy/MM/dd hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                            TT.Date = dr["EnteredAt"].ToString();
                            TT.MobNumber = dr["OriginID"].ToString();
                            TT.PayMedium = dr["PayMedium"].ToString();
                            TT.MerchantName = dr["MerchantName"].ToString();
                            TT.Amount = decimal.Parse(dr["Amount"].ToString().Trim() == "" ? "0" : dr["Amount"].ToString());
                            TT.PayMode = dr["PayMode"].ToString();
                            TT.RetrievalRef = dr["RetrievalRef"].ToString();
                            ListRec.Add(TT);

                        }

                        HttpContext.Current.Session["TopupVerifyInfo"] = TpData;
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
                dataset.Dispose();
            }

            return ListRec;
        }

        public TopUpManualTran TopUpManualVerifyList(string retReferencce)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            List<TopUpManualTran> ListRec = new List<TopUpManualTran>();
            TopUpManualTran Details = new TopUpManualTran();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNTopupVerifyList", conn))
                    {

                        cmd.Parameters.AddWithValue("@RetReference", retReferencce);
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 0;  // seconds
                            da.Fill(dataset);
                        }
                    }
                }
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }


                if (dataset.Tables.Count > 0)
                {
                    DataTable TpData = dataset.Tables[0];
                    if (TpData.Rows.Count > 0)
                    {
                        foreach (DataRow dr in TpData.Rows)
                        {

                            Details.Date = dr["EnteredAt"].ToString();
                            Details.MobNumber = dr["OriginID"].ToString();
                            Details.PayMedium = dr["PayMedium"].ToString();
                            Details.MerchantName = dr["MerchantName"].ToString();
                            Details.Amount = decimal.Parse(dr["Amount"].ToString().Trim() == "" ? "0" : dr["Amount"].ToString());
                            Details.PayMode = dr["PayMode"].ToString();
                            Details.RetrievalRef = dr["RetrievalRef"].ToString();
                            Details.UserName = dr["Name"].ToString();
                            Details.DestNumber = dr["MobileNumber"].ToString();
                            Details.RespDescription = dr["ResponseDescription"].ToString();
                            Details.RespCode = dr["ResponseCode"].ToString();
                            Details.RespStatus = dr["RespStatus"].ToString();
                            Details.MerchantID = dr["MerchantID"].ToString();

                        }

                        HttpContext.Current.Session["TopupVerifyInfo"] = TpData;
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
                dataset.Dispose();
            }

            return Details;
        }
        #endregion

        #region "Verify Top Up Transaction"

        public int verifyManualToUpTrans(TopUpManualTran TopUpManualTran, string userName)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNTopUpManualTran]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.AddWithValue("@retrievalReference", TopUpManualTran.RetrievalRef);
                        sqlCmd.Parameters.AddWithValue("@userNumber", TopUpManualTran.MobNumber);
                        sqlCmd.Parameters.AddWithValue("@ResponseDescription", TopUpManualTran.RespDescription);
                        sqlCmd.Parameters.AddWithValue("@userName", userName);
                        sqlCmd.Parameters.AddWithValue("@mode", "TVMT");

                        sqlCmd.Parameters.Add("@MesgStr", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@MesgStr"].Direction = ParameterDirection.Output;

                        sqlCmd.Parameters.Add("@return_value", SqlDbType.Int);
                        sqlCmd.Parameters["@return_value"].Direction = ParameterDirection.ReturnValue;

                        sqlCmd.ExecuteNonQuery();
                        string i = sqlCmd.Parameters["@MesgStr"].Value.ToString();
                        ret = Convert.ToInt32(sqlCmd.Parameters["@return_value"].Value);

                        //sqlCmd.Parameters["@return_value"].Direction = ParameterDirection.ReturnValue;

                        //sqlCmd.ExecuteNonQuery();
                        //string i = sqlCmd.Parameters["@MesgStr"].Value.ToString();
                        //ret = Convert.ToInt32(sqlCmd.Parameters["@return_value"].Value);
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

        #region Top Up Manual Approval Transaction Details
        public List<TopUpManualTran> TopUpVerifiedTranDetails(MerchantPay Mpay)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            List<TopUpManualTran> ListRec = new List<TopUpManualTran>();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNTopUpApproveTranDetails", conn))
                    {

                        cmd.Parameters.AddWithValue("@StartDate", Mpay.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", Mpay.EndDate);
                        cmd.Parameters.AddWithValue("@SourceMobileNo", Mpay.SourceMobileNo);
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 0;  // seconds
                            da.Fill(dataset);
                        }
                    }
                }
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }


                if (dataset.Tables.Count > 0)
                {
                    DataTable TpData = dataset.Tables[0];
                    if (TpData.Rows.Count > 0)
                    {
                        foreach (DataRow dr in TpData.Rows)
                        {
                            TopUpManualTran TT = new TopUpManualTran();
                            //TT.Date = DateTime.ParseExact(dr["EnteredAt"].ToString(), "yyyy/MM/dd hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                            TT.Date = dr["EnteredAt"].ToString();
                            TT.MobNumber = dr["OriginID"].ToString();
                            //TT.PayMedium = dr["PayMedium"].ToString();
                            TT.MerchantName = dr["MerchantName"].ToString();
                            TT.Amount = decimal.Parse(dr["Amount"].ToString().Trim() == "" ? "0" : dr["Amount"].ToString());
                            //TT.PayMode = dr["PayMode"].ToString();
                            TT.RetrievalRef = dr["RetrievalRef"].ToString();
                            TT.VerifiedBy = dr["VerifiedBy"].ToString();
                            TT.VerifiedDate = dr["VerifiedDate"].ToString();
                            ListRec.Add(TT);

                        }

                        HttpContext.Current.Session["ApprovalInfo"] = TpData;
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
                dataset.Dispose();
            }

            return ListRec;
        }

        public TopUpManualTran TopUpVerifiedTranDetails(string retReference)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            List<TopUpManualTran> ListRec = new List<TopUpManualTran>();
            TopUpManualTran Details = new TopUpManualTran();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNTopUpApproveTranDetails", conn))
                    {

                        cmd.Parameters.AddWithValue("@RetReference", retReference);
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 0;  // seconds
                            da.Fill(dataset);
                        }
                    }
                }
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }


                if (dataset.Tables.Count > 0)
                {
                    DataTable TpData = dataset.Tables[0];
                    if (TpData.Rows.Count > 0)
                    {
                        foreach (DataRow dr in TpData.Rows)
                        {
                            //TT.Date = DateTime.ParseExact(dr["EnteredAt"].ToString(), "yyyy/MM/dd hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                            Details.Date = dr["EnteredAt"].ToString();
                            Details.MobNumber = dr["OriginID"].ToString();
                            Details.PayMedium = dr["PayMedium"].ToString();
                            Details.MerchantName = dr["MerchantName"].ToString();
                            Details.Amount = decimal.Parse(dr["Amount"].ToString().Trim() == "" ? "0" : dr["Amount"].ToString());
                            Details.PayMode = dr["PayMode"].ToString();
                            Details.RetrievalRef = dr["RetrievalRef"].ToString();
                            Details.UserName = dr["Name"].ToString();
                            Details.DestNumber = dr["MobileNumber"].ToString();
                            Details.RespDescription = dr["ResponseDescription"].ToString();
                            Details.RespCode = dr["ResponseCode"].ToString();
                            Details.RespStatus = dr["RespStatus"].ToString();
                            Details.MerchantID = dr["MerchantID"].ToString();
                            Details.VerifiedDate = dr["VerifiedBy"].ToString();
                            Details.VerifiedDate = dr["VerifiedDate"].ToString();

                        }

                        HttpContext.Current.Session["ApprovalInfo"] = TpData;
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
                dataset.Dispose();
            }

            return Details;
        }

        #region "Top Up Manual Transaction"

        public int ManualTopUpTrans(TopUpManualTran TopUpManualTran, string userName,string merchantNo, string serviceCode)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNTopupManualTransaction]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.AddWithValue("@SourceMobileNo", merchantNo);
                        sqlCmd.Parameters.AddWithValue("@sourceRetreivalRef", TopUpManualTran.RetrievalRef);
                        sqlCmd.Parameters.AddWithValue("@CustomerMobileNo", TopUpManualTran.DestNumber);
                        sqlCmd.Parameters.AddWithValue("@DestMobileNo", TopUpManualTran.MobNumber);
                        sqlCmd.Parameters.AddWithValue("@MerchantName", TopUpManualTran.MerchantName);
                        sqlCmd.Parameters.AddWithValue("@serviceCode", serviceCode);
                        sqlCmd.Parameters.AddWithValue("@Amount", TopUpManualTran.Amount);
                        sqlCmd.Parameters.AddWithValue("@userName", userName);
                        //sqlCmd.Parameters.AddWithValue("@mode", "TVMT");
                        
                        sqlCmd.Parameters.Add("@MesgStr", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@MesgStr"].Direction = ParameterDirection.Output;

                        sqlCmd.Parameters.Add("@return_value", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@return_value"].Direction = ParameterDirection.Output;

                        //sqlCmd.Parameters.Add("@return_value", SqlDbType.Int);
                        //sqlCmd.Parameters["@return_value"].Direction = ParameterDirection.ReturnValue;

                        sqlCmd.ExecuteNonQuery();
                        string i = sqlCmd.Parameters["@MesgStr"].Value.ToString();
                        string retref = sqlCmd.Parameters["@return_value"].Value.ToString();
                        ret = Convert.ToInt32(sqlCmd.Parameters["@RetrievalRef"].Value);

                        //sqlCmd.Parameters["@return_value"].Direction = ParameterDirection.ReturnValue;

                        //sqlCmd.ExecuteNonQuery();
                        //string i = sqlCmd.Parameters["@MesgStr"].Value.ToString();
                        //ret = Convert.ToInt32(sqlCmd.Parameters["@return_value"].Value);
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

        public string ManualTopUpTrans1(TopUpManualTran TopUpManualTran, string userName, string merchantNo, string serviceCode)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            string ret = "";

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNTopupManualTransaction1", conn))
                    {

                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@SourceMobileNo", merchantNo);
                        cmd.Parameters.AddWithValue("@sourceRetreivalRef", TopUpManualTran.RetrievalRef);
                        cmd.Parameters.AddWithValue("@CustomerMobileNo", TopUpManualTran.DestNumber);
                        cmd.Parameters.AddWithValue("@DestMobileNo", TopUpManualTran.MobNumber);
                        cmd.Parameters.AddWithValue("@MerchantName", TopUpManualTran.MerchantName);
                        cmd.Parameters.AddWithValue("@serviceCode", serviceCode);
                        cmd.Parameters.AddWithValue("@Amount", TopUpManualTran.Amount);
                        cmd.Parameters.AddWithValue("@userName", userName);

                        cmd.Parameters.Add("@return_value", SqlDbType.Int);
                        cmd.Parameters["@return_value"].Direction = ParameterDirection.ReturnValue;

                        cmd.Parameters.Add("@MesgStr", SqlDbType.Char, 500);
                        cmd.Parameters["@MesgStr"].Direction = ParameterDirection.Output;

                        cmd.CommandType = CommandType.StoredProcedure;
                        
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 0;  // seconds
                            da.Fill(dataset);
                        }
                    }
                }
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }


                if (dataset.Tables.Count > 0)
                {
                    DataTable TpData = dataset.Tables[0];
                    if (TpData.Rows.Count > 0)
                    {
                        foreach (DataRow dr in TpData.Rows)
                        {
                            //TT.Date = DateTime.ParseExact(dr["EnteredAt"].ToString(), "yyyy/MM/dd hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                            ret = dr["Column1"].ToString();
                            //TT.responseDescription = dr["ResponseDescription"].ToString();

                        }

                        HttpContext.Current.Session["ApprovalInfo"] = TpData;
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
                dataset.Dispose();
            }

            return ret;
        }

        #endregion

        #region "Approve Top Up Transaction"

        public int ApproveManualToUpTrans(TopUpManualTran TopUpManualTran, string userName)
        {
            SqlConnection sqlCon = null;
            List<TopUpManualTran> ListRec = new List<TopUpManualTran>();
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNTopUpManualTran]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.AddWithValue("@retrievalReference", TopUpManualTran.RetrievalRef);
                        sqlCmd.Parameters.AddWithValue("@userNumber", TopUpManualTran.MobNumber);
                        sqlCmd.Parameters.AddWithValue("@userName", userName);
                        sqlCmd.Parameters.AddWithValue("@mode", "TAMT");

                        sqlCmd.Parameters.Add("@MesgStr", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@MesgStr"].Direction = ParameterDirection.Output;

                        sqlCmd.Parameters.Add("@return_value", SqlDbType.Int);
                        sqlCmd.Parameters["@return_value"].Direction = ParameterDirection.ReturnValue;

                        sqlCmd.ExecuteNonQuery();
                        string i = sqlCmd.Parameters["@MesgStr"].Value.ToString();
                        ret = Convert.ToInt32(sqlCmd.Parameters["@return_value"].Value);

                        //sqlCmd.Parameters["@return_value"].Direction = ParameterDirection.ReturnValue;

                        //sqlCmd.ExecuteNonQuery();
                        //string i = sqlCmd.Parameters["@MesgStr"].Value.ToString();
                        //ret = Convert.ToInt32(sqlCmd.Parameters["@return_value"].Value);
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

        #region "Reject Top up Transaction"

        public int RejectManualTopUpTrans(TopUpManualTran topUpManualTran, string userName)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNTopUpManualTran]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.AddWithValue("@retrievalReference", topUpManualTran.RetrievalRef);
                        sqlCmd.Parameters.AddWithValue("@userNumber", topUpManualTran.MobNumber);
                        sqlCmd.Parameters.AddWithValue("@userName", userName);
                        sqlCmd.Parameters.AddWithValue("@RejectRemarks", topUpManualTran.RejectRemarks);
                        sqlCmd.Parameters.AddWithValue("@mode", "TRMT");

                        sqlCmd.Parameters.Add("@MesgStr", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@MesgStr"].Direction = ParameterDirection.Output;

                        sqlCmd.Parameters.Add("@return_value", SqlDbType.Int);
                        sqlCmd.Parameters["@return_value"].Direction = ParameterDirection.ReturnValue;

                        sqlCmd.ExecuteNonQuery();
                        string i = sqlCmd.Parameters["@MesgStr"].Value.ToString();
                        ret = Convert.ToInt32(sqlCmd.Parameters["@return_value"].Value);

                        //sqlCmd.Parameters["@return_value"].Direction = ParameterDirection.ReturnValue;

                        //sqlCmd.ExecuteNonQuery();
                        //string i = sqlCmd.Parameters["@MesgStr"].Value.ToString();
                        //ret = Convert.ToInt32(sqlCmd.Parameters["@return_value"].Value);
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

        #endregion

        #region Reject Topup Manual Transaction
        public List<TopUpManualTran> TopUpRejectedTranDetails(MerchantPay Mpay)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            List<TopUpManualTran> ListRec = new List<TopUpManualTran>();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNTopUpRejectTranDetails", conn))
                    {

                        cmd.Parameters.AddWithValue("@StartDate", Mpay.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", Mpay.EndDate);
                        cmd.Parameters.AddWithValue("@SourceMobileNo", Mpay.SourceMobileNo);
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 0;  // seconds
                            da.Fill(dataset);
                        }
                    }
                }
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }


                if (dataset.Tables.Count > 0)
                {
                    DataTable TpData = dataset.Tables[0];
                    if (TpData.Rows.Count > 0)
                    {
                        foreach (DataRow dr in TpData.Rows)
                        {
                            TopUpManualTran TT = new TopUpManualTran();
                            //TT.Date = DateTime.ParseExact(dr["EnteredAt"].ToString(), "yyyy/MM/dd hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                            TT.Date = dr["EnteredAt"].ToString();
                            TT.MobNumber = dr["OriginID"].ToString();
                            //TT.PayMedium = dr["PayMedium"].ToString();
                            TT.MerchantName = dr["MerchantName"].ToString();
                            TT.Amount = decimal.Parse(dr["Amount"].ToString().Trim() == "" ? "0" : dr["Amount"].ToString());
                            //TT.PayMode = dr["PayMode"].ToString();
                            TT.RetrievalRef = dr["RetrievalRef"].ToString();
                            TT.RejectedBy = dr["RejectedBy"].ToString();
                            TT.RejectedDate = dr["RejectedDate"].ToString();
                            TT.RejectRemarks = dr["RejectRemarks"].ToString();
                            ListRec.Add(TT);

                        }

                        HttpContext.Current.Session["RejectedInfo"] = TpData;
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
                dataset.Dispose();
            }

            return ListRec;
        }

        public TopUpManualTran TopUpRejectedTranDetails(string retReference)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            List<TopUpManualTran> ListRec = new List<TopUpManualTran>();
            TopUpManualTran Details = new TopUpManualTran();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNTopUpRejectTranDetails", conn))
                    {

                        cmd.Parameters.AddWithValue("@RetReference", retReference);
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 0;  // seconds
                            da.Fill(dataset);
                        }
                    }
                }
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }


                if (dataset.Tables.Count > 0)
                {
                    DataTable TpData = dataset.Tables[0];
                    if (TpData.Rows.Count > 0)
                    {
                        foreach (DataRow dr in TpData.Rows)
                        {
                            //TT.Date = DateTime.ParseExact(dr["EnteredAt"].ToString(), "yyyy/MM/dd hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                            Details.Date = dr["EnteredAt"].ToString();
                            Details.MobNumber = dr["OriginID"].ToString();
                            Details.PayMedium = dr["PayMedium"].ToString();
                            Details.MerchantName = dr["MerchantName"].ToString();
                            Details.Amount = decimal.Parse(dr["Amount"].ToString().Trim() == "" ? "0" : dr["Amount"].ToString());
                            Details.PayMode = dr["PayMode"].ToString();
                            Details.RetrievalRef = dr["RetrievalRef"].ToString();
                            Details.UserName = dr["Name"].ToString();
                            Details.DestNumber = dr["MobileNumber"].ToString();
                            Details.RespDescription = dr["ResponseDescription"].ToString();
                            Details.RespCode = dr["ResponseCode"].ToString();
                            Details.RespStatus = dr["RespStatus"].ToString();
                            Details.MerchantID = dr["MerchantID"].ToString();
                            Details.VerifiedDate = dr["RejectedBy"].ToString();
                            Details.VerifiedDate = dr["RejectedDate"].ToString();
                            Details.RejectRemarks = dr["RejectRemarks"].ToString();

                        }

                        HttpContext.Current.Session["RejectedInfo"] = TpData;
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
                dataset.Dispose();
            }

            return Details;
        }
        #endregion

        public static DataTable GetMerchantDetail(string merchantID)
        {
            Database database;
            DataTable dtableResult = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNGetMerchantDetails]"))
                {
                    database.AddInParameter(command, "@merchantID", DbType.String, merchantID);
                    using (var dataset = new DataSet())
                    {
                        database.LoadDataSet(command, dataset, "dtCustomerInfo");

                        if (dataset.Tables.Count > 0)
                        {
                            dtableResult = dataset.Tables["dtCustomerInfo"];
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
                database = null;
                Database.ClearParameterCache();
            }

            return dtableResult;
        }

        public string GetUserName(string mobileNumber)
        {
            SqlConnection conn = null;
            SqlDataReader rdr;
            string name = "";
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        string Command = @"select MK.FName AS 'Name' from v_MNClientDetail VMC INNER JOIN MNClientKYC MK ON VMC.ClientCode = MK.ClientCode where VMC.UserName = '" + mobileNumber + "'";
                        
                        cmd.CommandText = Command;
                        cmd.Connection = conn;
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            name = rdr["Name"].ToString();
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


            }

            return name;
        }
        public string GetResponseCode1(string retrievalReference)
        {
            SqlConnection conn = null;
            SqlDataReader rdr;
            string ret = "";

            TopUpManualTran TT = new TopUpManualTran();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        string Command = @"select ResponseCode,ResponseDescription FROM MNResponse where RetrievalRef = '" + retrievalReference + "'";

                        cmd.CommandText = Command;
                        cmd.Connection = conn;
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            ret = rdr["ResponseCode"].ToString();
                            TT.responseDescription = rdr["ResponseDescription"].ToString();

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


            }

            return ret;
        }

        public string GetResponseCode(string retrievalReference)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            string ret = "";

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNGetRespCode", conn))
                    {

                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@RetReference", retrievalReference);
                        
                        cmd.CommandType = CommandType.StoredProcedure;

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 0;  // seconds
                            da.Fill(dataset);
                        }
                    }
                }
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }


                if (dataset.Tables.Count > 0)
                {
                    DataTable TpData = dataset.Tables[0];
                    if (TpData.Rows.Count > 0)
                    {
                        foreach (DataRow dr in TpData.Rows)
                        {
                            //TT.Date = DateTime.ParseExact(dr["EnteredAt"].ToString(), "yyyy/MM/dd hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                            ret = dr["ResponseCode"].ToString();
                            //TT.responseDescription = dr["ResponseDescription"].ToString();

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
                dataset.Dispose();
            }

            return ret;
        }


        public string getResponseDescription(string result)
        {
            string message = "";
            ErrorMessage em = new ErrorMessage();

            if (result == "111")
            {
                message = em.Error_111;
            }
            if (result == "114")
            {
                message = em.Error_114;
            }
            if (result == "115")
            {
                message = em.Error_115;
            }
            if (result == "116")
            {
                message = em.Error_116;
            }
            if (result == "119")
            {
                message = em.Error_119;
            }
            if (result == "121")
            {
                message = em.Error_121;
            }
            if (result == "163")
            {
                message = em.Error_163/* + " " + result*/;
            }
            if (result == "180")
            {
                message = em.Error_180/* + " " + result*/;
            }
            if (result == "181")
            {
                message = em.Error_181/* + " " + result*/;
            }
            if (result == "182")
            {
                message = em.Error_182/* + " " + result*/;
            }
            if (result == "183")
            {
                message = em.Error_183/* + " " + result*/;
            }
            if (result == "184")
            {
                message = em.Error_184/* + " " + result*/;
            }
            if (result == "185")
            {
                message = em.Error_185/* + " " + result*/;
            }
            if (result == "186")
            {
                message = em.Error_186/* + " " + result*/;
            }
            if (result == "187")
            {
                message = em.Error_187/* + " " + result*/;
            }
            if (result == "188")
            {
                message = em.Error_188/* + " " + result*/;
            }
            if (result == "189")
            {
                message = em.Error_189/* + " " + result*/;
            }
            if (result == "190")
            {
                message = em.Error_190/* + " " + result*/;
            }
            if (result == "800")
            {
                message = em.Error_800/* + " " + result*/;
            }
            if (result == "902")
            {
                message = em.Error_902/* + " " + result*/;
            }
            if (result == "904")
            {
                message = em.Error_904/* + " " + result*/;
            }
            if (result == "906")
            {
                message = em.Error_906/* + " " + result*/;
            }
            if (result == "907")
            {
                message = em.Error_907/* + " " + result*/;
            }
            if (result == "909")
            {
                message = em.Error_909/* + " " + result*/;
            }
            if (result == "911")
            {
                message = em.Error_911/* + " " + result*/;
            }
            if (result == "913")
            {
                message = em.Error_913/* + " " + result*/;
            }
            if (result == "90")
            {
                message = em.Error_90/* + " " + result*/;
            }
            if (result == "91")
            {
                message = em.Error_91/* + " " + result*/;
            }
            if (result == "92")
            {
                message = em.Error_92/* + " " + result*/;
            }
            if (result == "94")
            {
                message = em.Error_94/* + " " + result*/;
            }
            if (result == "95")
            {
                message = em.Error_95/* + " " + result*/;
            }
            if (result == "98")
            {
                message = em.Error_98/* + " " + result*/;
            }
            if (result == "99")
            {
                message = em.Error_99/* + " " + result*/;
            }


            return message;
        }


        #region "Re verify Top up Transaction"

        public int ReVerifyManualTopUpTrans(TopUpManualTran topUpManualTran, string userName)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNTopUpManualTran]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.AddWithValue("@retrievalReference", topUpManualTran.RetrievalRef);
                        sqlCmd.Parameters.AddWithValue("@userNumber", topUpManualTran.MobNumber);
                        sqlCmd.Parameters.AddWithValue("@userName", userName);
                        sqlCmd.Parameters.AddWithValue("@mode", "TRVMT");

                        sqlCmd.Parameters.Add("@MesgStr", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@MesgStr"].Direction = ParameterDirection.Output;

                        sqlCmd.Parameters.Add("@return_value", SqlDbType.Int);
                        sqlCmd.Parameters["@return_value"].Direction = ParameterDirection.ReturnValue;

                        sqlCmd.ExecuteNonQuery();
                        string i = sqlCmd.Parameters["@MesgStr"].Value.ToString();
                        ret = Convert.ToInt32(sqlCmd.Parameters["@return_value"].Value);

                        //sqlCmd.Parameters["@return_value"].Direction = ParameterDirection.ReturnValue;

                        //sqlCmd.ExecuteNonQuery();
                        //string i = sqlCmd.Parameters["@MesgStr"].Value.ToString();
                        //ret = Convert.ToInt32(sqlCmd.Parameters["@return_value"].Value);
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

        #endregion
    }
}