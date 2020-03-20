using MNepalProject.Connection;
using System;
using System.Data;
using System.Data.SqlClient;

namespace MNepalWCF.UserModels
{
    public class CreateMobileDetailUserModels
    {
        #region "Create Mobile Information "

        /// <summary>
        /// Create Mobile Information
        /// </summary>
        /// <param name="objUserInfo"></param>
        /// <returns></returns>
        public int CreateMobileUserInfo(Models.Login objUserInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNMobileLoginInsert]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@Mobile", objUserInfo.Mobile));
                        sqlCmd.Parameters.AddWithValue("@DeviceID", objUserInfo.DeviceID);
                        sqlCmd.Parameters.AddWithValue("@GeneratedPass", objUserInfo.GeneratedPass);
                        sqlCmd.Parameters.AddWithValue("@mode", objUserInfo.Mode);

                        sqlCmd.Parameters.Add("@UIDOut", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@UIDOut"].Direction = ParameterDirection.Output;

                        ret = sqlCmd.ExecuteNonQuery();
                        if (objUserInfo.Mode.Equals("IMD", StringComparison.InvariantCultureIgnoreCase))
                        {
                            ret = Convert.ToInt32(sqlCmd.Parameters["@UIDOut"].Value);

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