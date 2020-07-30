﻿using ThailiMNepalAgent.Connection;
using ThailiMNepalAgent.Models;
using System;
using System.Data;
using System.Data.SqlClient;

namespace ThailiMNepalAgent.UserModels
{
    public class ReqTokenUserModels
    {
        /// <summary>
        /// Retrieve the user information based on mode
        /// </summary>
        /// <param name="objUserInfo">Pass an instance of User information</param>
        /// <returns>Returns the  avail baln information based on user information</returns>
        public DataTable GetRegToken(TopUpPay objUserInfo)
        {
            DataTable dtableResult = null;
            SqlConnection conn = null;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNReqVerify]", conn))
                    {
                        cmd.Parameters.AddWithValue("@ReqVerifyToken", objUserInfo.TokenUnique);
                        cmd.Parameters.AddWithValue("@mode", "GR");
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 60;  // seconds
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset, "dtReqToken");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables["dtReqToken"];
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


        #region ReqTokenInsert
        public int InsertReqToken(string reqToken)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNReqVerify]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.AddWithValue("@ReqVerifyToken", reqToken);

                        sqlCmd.Parameters.AddWithValue("@mode", "IR");

                        //sqlCmd.Parameters.Add("@RegIDOut", SqlDbType.Char, 500);
                        //sqlCmd.Parameters["@RegIDOut"].Direction = ParameterDirection.Output;

                        ret = sqlCmd.ExecuteNonQuery();
                        //ret = Convert.ToInt32(sqlCmd.Parameters["@RegIDOut"].Value);

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