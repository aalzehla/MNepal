using ThailiMNepalAgent.Connection;
using ThailiMNepalAgent.Models;
using System;
using System.Data;
using System.Data.SqlClient;

namespace ThailiMNepalAgent.UserModels
{
    public class BankUserModel
    {
        /// <summary>
        /// Retrieve the user information based on mode
        /// </summary>
        /// <param name="objUserInfo">Pass an instance of User information</param>
        /// <returns>Returns the  avail baln information based on user information</returns>
        public DataSet GetDsBankCodeInfo(UserInfo objUserInfo)
        {
            DataSet dsBank = new DataSet();
            SqlTransaction transaction = null;

            using (SqlConnection connection = new SqlConnection(DatabaseConnection.ConnectionStr()))
            {
                try
                {
                    connection.Open();
                    
                    using (SqlCommand cmdBank = new SqlCommand("s_MNBankTable", connection, transaction))
                    {

                        cmdBank.CommandType = CommandType.StoredProcedure;
                        cmdBank.Parameters.Add("@mode", SqlDbType.NVarChar).Value = objUserInfo.Mode;
                        using (SqlDataAdapter daBank = new SqlDataAdapter(cmdBank))
                        {
                            daBank.Fill(dsBank);
                        }
                    }
                    if (connection.State != ConnectionState.Closed)
                        connection.Close();
                    
                    return dsBank;
                }
                catch (ArithmeticException ex)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    throw new ArithmeticException("illegal expression " + ex);
                }
                finally
                {
                    if (connection != null)
                    {
                        connection.Dispose();
                        connection.Close();
                    }
                }
            }
        }


        /// <summary>
        /// Retrieve the user information based on mode
        /// </summary>
        /// <param name="objUserInfo">Pass an instance of User information</param>
        /// <returns>Returns the  avail baln information based on user information</returns>
        public DataSet GetDsBranchCodeInfo(UserInfo objUserInfo)
        {
            DataSet ds = new DataSet();
            SqlConnection conn = null;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNGetBranch]", conn))
                    {
                        cmd.Parameters.Add("@BankCode", SqlDbType.NVarChar).Value = objUserInfo.WBankCode;
                        cmd.Parameters.Add("@mode", SqlDbType.NVarChar).Value = objUserInfo.Mode;
                        cmd.CommandType = CommandType.StoredProcedure;

                        
                        using (SqlDataAdapter daBank = new SqlDataAdapter(cmd))
                        {
                            daBank.Fill(ds);
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

            return ds;
        }

    }
}