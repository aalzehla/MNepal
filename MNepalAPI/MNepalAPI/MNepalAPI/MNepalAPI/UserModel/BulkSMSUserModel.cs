using MNepalAPI.Connection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using static MNepalAPI.Models.Notifications;

namespace MNepalAPI.UserModel
{
    public class BulkSMSUserModel
    {
        #region notifications
        public int ResponseBulkSMSInfo(BulkSMSModel objresBulkSMSInfo)  
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNBulkSMS]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.AddWithValue("@contactNumber", objresBulkSMSInfo.customerNumber);
                        sqlCmd.Parameters.AddWithValue("@message", objresBulkSMSInfo.message);
                        sqlCmd.Parameters.AddWithValue("@bulkSMSDate", objresBulkSMSInfo.smsDateTime);                       
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