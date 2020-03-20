using MNepalProject.Connection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using WCF.MNepal.Models;

namespace WCF.MNepal.UserModels
{
    public class UpdateFTUserModel
    {
        #region "Create Mobile Information Pwd AND PIN CHange "

        /// <summary>
        /// UpdateFT Information
        /// </summary>
        /// <param name="objUserInfo"></param>
        /// <returns></returns>
        public int UpdateFTInfo(LoginAuth objUserInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNFPasswordReset]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", objUserInfo.ClientCode));
                        sqlCmd.Parameters.AddWithValue("@NewPin", objUserInfo.TPin);
                        sqlCmd.Parameters.AddWithValue("@NewPassword", objUserInfo.Password);
                        sqlCmd.Parameters.AddWithValue("@Mode", "FL");

                        sqlCmd.Parameters.Add("@RetVal", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@RetVal"].Direction = ParameterDirection.Output;

                        ret = sqlCmd.ExecuteNonQuery();
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