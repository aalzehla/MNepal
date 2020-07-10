using MNepalAPI.Connection;
using MNepalAPI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace MNepalAPI.UserModel
{
    public class GetQRCodeUrlModel
    {
        public DataTable GetUserQRCodeUrl(QRCodeReader objUserInfo)
        {
            DataTable dtableResult = null;
            SqlConnection conn = null;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNQRCode]", conn))
                    {
                        cmd.Parameters.AddWithValue("@merchantName", objUserInfo.merchantName);
                        cmd.Parameters.AddWithValue("@merchantId", objUserInfo.merchantId);
                        cmd.Parameters.AddWithValue("@qRCodeUrl", "");
                        cmd.Parameters.AddWithValue("@mode", "getPhoto");
                        cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.Output;
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 60;  // seconds
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset, "dtUserQRImage");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables["dtUserQRImage"];
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
    }
}