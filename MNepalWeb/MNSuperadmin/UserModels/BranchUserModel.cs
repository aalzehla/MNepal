using System;
using System.Data;
using System.Data.SqlClient;
using MNSuperadmin.Connection;
using MNSuperadmin.Models;

namespace MNSuperadmin.UserModels
{
    public class BranchUserModel
    {
        #region "Create Branch Information"

        public int BranchInfo(MNBranchTable objBranchInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNBranch]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@BranchCode", objBranchInfo.BranchCode));
                        sqlCmd.Parameters.AddWithValue("@BranchName", objBranchInfo.BranchName);
                        sqlCmd.Parameters.AddWithValue("@BranchLocation", objBranchInfo.BranchLocation);
                        sqlCmd.Parameters.AddWithValue("@IsBlocked", objBranchInfo.IsBlocked);
                        sqlCmd.Parameters.AddWithValue("@mode", objBranchInfo.Mode);

                        sqlCmd.Parameters.Add("@MesgStr", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@MesgStr"].Direction = ParameterDirection.Output;

                        sqlCmd.Parameters.Add("@return_value", SqlDbType.Int);
                        sqlCmd.Parameters["@return_value"].Direction = ParameterDirection.ReturnValue;

                        sqlCmd.ExecuteNonQuery();
                        string i = sqlCmd.Parameters["@MesgStr"].Value.ToString();
                        ret = Convert.ToInt32(sqlCmd.Parameters["@return_value"].Value);
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


        #region "GET BRANCH NAME INFO"

        public DataSet GetDsBranchNameInfo(MNBranchTable objBranchInfo)
        {
            DataSet ds = new DataSet();
            SqlConnection conn = null;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNBranch]", conn))
                    {
                        cmd.Parameters.Add(new SqlParameter("@BranchCode", objBranchInfo.BranchCode));
                        cmd.Parameters.AddWithValue("@BranchName", objBranchInfo.BranchName);
                        cmd.Parameters.AddWithValue("@BranchLocation", objBranchInfo.BranchLocation);
                        cmd.Parameters.AddWithValue("@IsBlocked", objBranchInfo.IsBlocked);
                        cmd.Parameters.Add("@mode", SqlDbType.NVarChar).Value = objBranchInfo.Mode;
                        cmd.CommandType = CommandType.StoredProcedure;

                        using (SqlDataAdapter daBranch = new SqlDataAdapter(cmd))
                        {
                            daBranch.Fill(ds);
                        }
                    }
                    if (conn.State != ConnectionState.Closed)
                        conn.Close();
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

        #endregion


        #region Get All Branch Information"

        public int GetAllBranchInfo(MNBranchTable objBranchInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNBranch]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@BranchCode", objBranchInfo.BranchCode));
                        sqlCmd.Parameters.AddWithValue("@BranchName", objBranchInfo.BranchName);
                        sqlCmd.Parameters.AddWithValue("@BranchLocation", objBranchInfo.BranchLocation);
                        sqlCmd.Parameters.AddWithValue("@IsBlocked", objBranchInfo.IsBlocked);
                        sqlCmd.Parameters.AddWithValue("@mode", objBranchInfo.Mode);

                        sqlCmd.Parameters.Add("@MesgStr", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@MesgStr"].Direction = ParameterDirection.Output;

                        sqlCmd.Parameters.Add("@return_value", SqlDbType.Int);
                        sqlCmd.Parameters["@return_value"].Direction = ParameterDirection.ReturnValue;

                        sqlCmd.ExecuteNonQuery();
                        string i = sqlCmd.Parameters["@MesgStr"].Value.ToString();
                        ret = Convert.ToInt32(sqlCmd.Parameters["@return_value"].Value);
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


        #region "GET Checking BranchCode"
        public DataTable GetBranchCodeInfo(MNBranchTable objBranchInfo)
        {
            DataTable dtableResult = null;
            SqlConnection conn = null;
            int ret;

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNBranch]", conn))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@BranchCode", objBranchInfo.BranchCode));
                        sqlCmd.Parameters.AddWithValue("@BranchName", objBranchInfo.BranchName);
                        sqlCmd.Parameters.AddWithValue("@BranchLocation", objBranchInfo.BranchLocation);
                        sqlCmd.Parameters.AddWithValue("@IsBlocked", objBranchInfo.IsBlocked);
                        sqlCmd.Parameters.AddWithValue("@mode", objBranchInfo.Mode);

                        sqlCmd.Parameters.Add("@MesgStr", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@MesgStr"].Direction = ParameterDirection.Output;

                        sqlCmd.Parameters.Add("@return_value", SqlDbType.Int);
                        sqlCmd.Parameters["@return_value"].Direction = ParameterDirection.ReturnValue;

                        sqlCmd.ExecuteNonQuery();
                        string i = sqlCmd.Parameters["@MesgStr"].Value.ToString();
                        ret = Convert.ToInt32(sqlCmd.Parameters["@return_value"].Value);


                        using (SqlDataAdapter da = new SqlDataAdapter(sqlCmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 60;  // seconds
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset, "dtBranchCodeInfo");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables["dtBranchCodeInfo"];
                                }
                            }
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
                if (conn != null)
                {
                    conn.Close();
                }
            }
            return dtableResult;
        }




        


        #endregion

    }
}