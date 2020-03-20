using MNepalProject.Connection;
using System;
using System.Data;
using System.Data.SqlClient;
using MNepalWCF.Models;

namespace MNepalWCF.UserModels
{
    public class PwdUserModel
    {
        public DataTable GetPwdInformation(PwdChange objUserInfo)
        {
            SqlConnection conn = null;
            DataTable dtableResult = null;

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNPwdCheck]", conn))
                    {
                        cmd.Parameters.AddWithValue("@SourceMobileNo", objUserInfo.mobile);
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset, "dtPwdInfo");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables["dtPwdInfo"];
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
    }
}