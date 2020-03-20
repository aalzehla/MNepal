using System;
using System.Data;
using System.Data.SqlClient;
using MNepalProject.Connection;
using MNepalProject.Models;

namespace MNepalWCF.UserModels
{
    public class AgentCheckerUserModel
    {
        #region Transaction Limit Information

        public DataTable GetAgentCheckInfo(MNClientExt objAgentInfo)
        {
            DataTable dtableResult = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNLogin]", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserName", objAgentInfo.UserName);
                        cmd.Parameters.AddWithValue("@Password", "");
                        cmd.Parameters.AddWithValue("@ClientCode", "");
                        cmd.Parameters.AddWithValue("@mode", "GAC");//AgentChecker
                        
                        cmd.CommandType = CommandType.StoredProcedure;

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 60;  //seconds
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset, "dtAgentInfo");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables["dtAgentInfo"];
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