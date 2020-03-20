using MNepalProject.Connection;
using MNepalProject.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace WCF.MNepal.UserModels
{
    public class TraceIdCheckerUserModel
    {
        #region TraceID Information

        public DataTable GetTraceIDCheckInfo(MNTraceID objTraceIDInfo)
        {
            DataTable dtableResult = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNTraceIDCheck]", conn))
                    {
                        cmd.Parameters.AddWithValue("@TraceId", objTraceIDInfo.TraceID);
                        cmd.Parameters.AddWithValue("@mode", "GTID"); //Get TraceID

                        cmd.CommandType = CommandType.StoredProcedure;

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 60;  //seconds
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset, "dtTraceIDInfo");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables["dtTraceIDInfo"];
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