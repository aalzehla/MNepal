using MNepalProject.Connection;
using System;
using System.Data;
using System.Data.SqlClient;
using MNepalWCF.Models;

namespace MNepalWCF.UserModels
{
    public class RemitUserModel
    {
        #region "Insert Remit Information "

        /// <summary>
        /// Register the Remit Information
        /// </summary>
        /// <param name="objRemitInfo"></param>
        /// <returns></returns>
        public int InsertRemitInfo(MNRemit objRemitInfo)
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

                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", objRemitInfo.ClientCode));
                        sqlCmd.Parameters.AddWithValue("@TraceID", objRemitInfo.TraceID);
                        sqlCmd.Parameters.AddWithValue("@SenderMobileNo", objRemitInfo.SenderMobileNo);
                        sqlCmd.Parameters.AddWithValue("@RecipientMobileNo", objRemitInfo.RecipientMobileNo);
                        sqlCmd.Parameters.AddWithValue("@BeneficialName", objRemitInfo.BeneficialName);

                        sqlCmd.Parameters.AddWithValue("@RequestTokenCode", objRemitInfo.RequestTokenCode);
                        sqlCmd.Parameters.AddWithValue("@Amount", objRemitInfo.Amount);
                        sqlCmd.Parameters.AddWithValue("@TokenID", objRemitInfo.TokenID);
                        sqlCmd.Parameters.AddWithValue("@Purpose", objRemitInfo.Purpose);
                        sqlCmd.Parameters.AddWithValue("@TokenCreatedDate", objRemitInfo.TokenCreatedDate);
                        sqlCmd.Parameters.AddWithValue("@TokenExpiryDate", objRemitInfo.TokenExpiryDate);

                        sqlCmd.Parameters.AddWithValue("@mode", objRemitInfo.Mode);

                        sqlCmd.Parameters.Add("@RegIDOut", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@RegIDOut"].Direction = ParameterDirection.Output;

                        ret = sqlCmd.ExecuteNonQuery();
                        if (objRemitInfo.Mode.Equals("IR", StringComparison.InvariantCultureIgnoreCase))
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

        #region "Get Remit Information "

        /// <summary>
        /// Retrieve the user information based on mode
        /// </summary>
        /// <param name="objUserInfo">Pass an instance of User information</param>
        /// <returns>Returns the  avail baln information based on user information</returns>
        public DataTable GetRemitInfo(MNRemit objRemitInfo)
        {
            DataTable dtableResult = null;
            SqlConnection conn = null;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNRemitGetDetail]", conn))
                    {
                        cmd.Parameters.AddWithValue("@RecipientMobileNo", objRemitInfo.RecipientMobileNo);
                        cmd.Parameters.AddWithValue("@mode", objRemitInfo.Mode);
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

        #endregion

        #region "Remit Token Information "

        /// <summary>
        /// Register the Remit Token Information
        /// </summary>
        /// <param name="objRemitTokenInfo"></param>
        /// <returns></returns>
        public int InsertRemitTokenInfo(MNRemit objRemitTokenInfo)
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

                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", objRemitTokenInfo.ClientCode));
                        sqlCmd.Parameters.AddWithValue("@TraceID", objRemitTokenInfo.TraceID);
                        sqlCmd.Parameters.AddWithValue("@RequestTokenCode", objRemitTokenInfo.RequestTokenCode);
                        sqlCmd.Parameters.AddWithValue("@TokenID", objRemitTokenInfo.TokenID);

                        sqlCmd.Parameters.AddWithValue("@SenderMobileNo", objRemitTokenInfo.SenderMobileNo);
                        sqlCmd.Parameters.AddWithValue("@RecipientMobileNo", objRemitTokenInfo.RecipientMobileNo);
                        sqlCmd.Parameters.AddWithValue("@BeneficialName", objRemitTokenInfo.BeneficialName);

                        sqlCmd.Parameters.AddWithValue("@Amount", objRemitTokenInfo.Amount);
                        sqlCmd.Parameters.AddWithValue("@Purpose", objRemitTokenInfo.Purpose);
                        sqlCmd.Parameters.AddWithValue("@TokenCreatedDate", objRemitTokenInfo.TokenCreatedDate);
                        sqlCmd.Parameters.AddWithValue("@TokenExpiryDate", objRemitTokenInfo.TokenExpiryDate);

                        sqlCmd.Parameters.AddWithValue("@mode", objRemitTokenInfo.Mode);

                        sqlCmd.Parameters.Add("@RegIDOut", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@RegIDOut"].Direction = ParameterDirection.Output;

                        ret = sqlCmd.ExecuteNonQuery();
                        if (objRemitTokenInfo.Mode.Equals("UTIR", StringComparison.InvariantCultureIgnoreCase))
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

    }
}