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
    public class QRCodeGeneratorUserModel
    {
        #region QRCode
        public int ResponseQRCodeInfo(QRCode qRCodeInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNQRCode]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@merchantName", qRCodeInfo.merchantName));
                        sqlCmd.Parameters.Add(new SqlParameter("@merchantId", qRCodeInfo.merchantId));
                        sqlCmd.Parameters.Add(new SqlParameter("@qRCodeUrl", qRCodeInfo.qrCodeImagePath));
                        sqlCmd.Parameters.Add(new SqlParameter("@mode", "uploadPhotoUrl"));
                        sqlCmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.Output;
                        sqlCmd.ExecuteNonQuery();
                        ret = Convert.ToInt32(sqlCmd.Parameters["@ReturnValue"].Value);
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