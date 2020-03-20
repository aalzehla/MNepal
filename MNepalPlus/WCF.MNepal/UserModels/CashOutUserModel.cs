using MNepalProject.Connection;
using MNepalProject.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using WCF.MNepal.Models;

namespace WCF.MNepal.UserModels
{
    public class CashOutUserModel
    {
        #region "Remit Information "

        /// <summary>
        /// Register the Remit Information
        /// </summary>
        /// <param name="objRemitInfo"></param>
        /// <returns></returns>
        public int InsertCashOutInfo(CashOut objCashOutInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNRemit]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", objCashOutInfo.ClientCode));
                        sqlCmd.Parameters.AddWithValue("@TraceID", objCashOutInfo.TraceID);
                        sqlCmd.Parameters.AddWithValue("@SenderMobileNo", objCashOutInfo.CustomerMobileNo);
                        //sqlCmd.Parameters.AddWithValue("@RecipientMobileNo", objCashOutInfo.AgentMobileNo);
                        sqlCmd.Parameters.AddWithValue("@RecipientMobileNo", "");
                        sqlCmd.Parameters.AddWithValue("@BeneficialName", objCashOutInfo.BeneficialName);
                        sqlCmd.Parameters.AddWithValue("@RequestTokenCode", objCashOutInfo.RequestTokenCode);
                        sqlCmd.Parameters.AddWithValue("@Amount", objCashOutInfo.Amount);
                        sqlCmd.Parameters.AddWithValue("@TokenID", objCashOutInfo.TokenID);
                        sqlCmd.Parameters.AddWithValue("@Purpose", objCashOutInfo.Purpose);
                        sqlCmd.Parameters.AddWithValue("@TokenCreatedDate", objCashOutInfo.TokenCreatedDate);
                        sqlCmd.Parameters.AddWithValue("@TokenExpiryDate", objCashOutInfo.TokenExpiryDate);
                        
                        sqlCmd.Parameters.AddWithValue("@mode", objCashOutInfo.Mode);

                        sqlCmd.Parameters.Add("@RegIDOut", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@RegIDOut"].Direction = ParameterDirection.Output;

                        ret = sqlCmd.ExecuteNonQuery();
                        if (objCashOutInfo.Mode.Equals("CO", StringComparison.InvariantCultureIgnoreCase))
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

        #endregion


        /// <summary>
        /// Retrieve the user information based on mode
        /// </summary>
        /// <param name="objUserInfo">Pass an instance of User information</param>
        /// <returns>Returns the  avail baln information based on user information</returns>
        public DataTable GetCashOutInfo(CashOut objCashInfo)
        {
            DataTable dtableResult = null;
            SqlConnection conn = null;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNCashOutDetail]", conn))
                    {
                        cmd.Parameters.AddWithValue("@RecipientMobileNo", objCashInfo.AgentMobileNo);
                        cmd.Parameters.AddWithValue("@SenderMobileNo", objCashInfo.CustomerMobileNo);
                        cmd.Parameters.AddWithValue("@RequestTokenCode", objCashInfo.RequestTokenCode);
                        cmd.Parameters.AddWithValue("@Amount", objCashInfo.Amount);
                        cmd.Parameters.AddWithValue("@mode", objCashInfo.Mode);
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 60;  // seconds
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset, "dtRemitInfo");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables["dtRemitInfo"];
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

        public int UpdateCashOut(CashOut objCashOutInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNCashOut]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        
                        sqlCmd.Parameters.AddWithValue("@SenderMobileNo", objCashOutInfo.CustomerMobileNo);
                        sqlCmd.Parameters.AddWithValue("@RecipientMobileNo", objCashOutInfo.AgentMobileNo);
                        sqlCmd.Parameters.AddWithValue("@status", objCashOutInfo.Status);
                        sqlCmd.Parameters.AddWithValue("@remarks", objCashOutInfo.Remarks);
                        sqlCmd.Parameters.AddWithValue("@RequestTokenCode", objCashOutInfo.RequestTokenCode);
                        sqlCmd.Parameters.AddWithValue("@mode", objCashOutInfo.Mode);
                        
                        ret = sqlCmd.ExecuteNonQuery();
                        
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



        #region CashOutList

        //public DataTable GetCashOutList(MNClientExt objCustUserInfo)
             public DataTable GetCashOutList(CashOutList objCustUserInfo)
        {
            DataTable dtableResult = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNCashOutList]", conn))
                    {
                        cmd.Parameters.AddWithValue("@SenderMobileNo", objCustUserInfo.SenderMobileNo);
                        cmd.Parameters.AddWithValue("@TokenID", objCustUserInfo.TokenID);
                        cmd.Parameters.AddWithValue("@RecipientMobileNo", objCustUserInfo.RecipientMobileNo); 
                        cmd.Parameters.AddWithValue("@Amount", objCustUserInfo.Amount);
                        cmd.Parameters.AddWithValue("@Status", objCustUserInfo.Status);


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


        #region"check RequestTokenCode
        
        public DataTable CheckRequestToken(CashOut objCustUserInfo)
        {
            DataTable dtableResult = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNCashOutCheckRequestTokenCode]", conn))
                    {
                        cmd.Parameters.AddWithValue("@RequestTokenCode", objCustUserInfo.RequestTokenCode);
                        cmd.Parameters.AddWithValue("@SenderMobileNo", objCustUserInfo.SenderMobileNo);
                        cmd.Parameters.AddWithValue("@mode", "CR"); // check requestokencode

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