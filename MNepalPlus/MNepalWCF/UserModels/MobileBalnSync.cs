using MNepalProject.Connection;
using System;
using System.Data;
using System.Data.SqlClient;
using MNepalWCF.Models;

namespace MNepalWCF.UserModels
{
    public class MobileBalnSync
    {
        #region "Create Mobile Information "

        /// <summary>
        /// Create Mobile Information
        /// </summary>
        /// <param name="objUserInfo"></param>
        /// <returns></returns>
        public int CreateMobileSyncInfo(SyncDetail objUserInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNBalnSync]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@Mobile", objUserInfo.Mobile));
                        sqlCmd.Parameters.AddWithValue("@TID", objUserInfo.Tid);
                        sqlCmd.Parameters.AddWithValue("@mode", objUserInfo.Mode);

                        sqlCmd.Parameters.Add("@LSOut", SqlDbType.VarChar, 500);
                        sqlCmd.Parameters["@LSOut"].Direction = ParameterDirection.Output;

                        ret = sqlCmd.ExecuteNonQuery();
                        if (objUserInfo.Mode.Equals("ISBD", StringComparison.InvariantCultureIgnoreCase))
                        {
                            ret = 1;//Convert.ToInt32(sqlCmd.Parameters["@LSOut"].Value.ToString());

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

        #region "Get Last Sync Information "

        public DataTable GetLastSyncInfo(SyncDetail objUserInfo)
        {
            SqlConnection conn = null;
            DataTable dtableResult = null;

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNBalnSync]", conn))
                    {
                        cmd.Parameters.AddWithValue("@Mobile", objUserInfo.Mobile);
                        cmd.Parameters.AddWithValue("@TID", objUserInfo.Tid);
                        cmd.Parameters.AddWithValue("@mode", objUserInfo.Mode);
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset, "dtSyncInfo");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables["dtSyncInfo"];
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
                conn.Close();
            }

            return dtableResult;
        }

        #endregion

    }
}