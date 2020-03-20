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
    public class DematUserModel
    {
        #region Response Demat Payment Info Get
        public int ResponseDematPaymentInfo(DematModel objresDematPaymentInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNDematRequest]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;


                        sqlCmd.Parameters.AddWithValue("@BoId", objresDematPaymentInfo.BoId);
                        sqlCmd.Parameters.AddWithValue("@DematName", objresDematPaymentInfo.DematName);
                        sqlCmd.Parameters.AddWithValue("@TotalAmount", objresDematPaymentInfo.TotalAmount);
                        sqlCmd.Parameters.AddWithValue("@Fees", objresDematPaymentInfo.Fees);
                        sqlCmd.Parameters.AddWithValue("@ClientCode", objresDematPaymentInfo.ClientCode);
                        sqlCmd.Parameters.AddWithValue("@RetrievalRef", objresDematPaymentInfo.RetrievalRef);
                        sqlCmd.Parameters.AddWithValue("@UserName", objresDematPaymentInfo.UserName);
                        sqlCmd.Parameters.AddWithValue("@BankCode", objresDematPaymentInfo.BankCode);
                        sqlCmd.Parameters.AddWithValue("@TimeStamp", objresDematPaymentInfo.TimeStamp);
                        sqlCmd.Parameters.AddWithValue("@mode", objresDematPaymentInfo.Mode);

                        sqlCmd.Parameters.Add("@RegIDOut", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@RegIDOut"].Direction = ParameterDirection.Output;

                        sqlCmd.Parameters.Add("@MsgStr", SqlDbType.VarChar, 500);
                        sqlCmd.Parameters["@MsgStr"].Direction = ParameterDirection.Output;

                        ret = sqlCmd.ExecuteNonQuery();
                        //if (objresPaypointPaymentInfo.Mode.Equals("SCA", StringComparison.InvariantCultureIgnoreCase))
                        //{
                        //    ret = Convert.ToInt32(sqlCmd.Parameters["@RegIDOut"].Value);

                        //}
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

        #region Response Demat Paymnet Info Post
        public int ResponseDematPaymentInfoPost(DematModel objresDematExecutePaymentInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNDematResponse]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;


                        sqlCmd.Parameters.AddWithValue("@BoId", objresDematExecutePaymentInfo.BoId);
                        sqlCmd.Parameters.AddWithValue("@TotalAmount", objresDematExecutePaymentInfo.TotalAmount);
                        sqlCmd.Parameters.AddWithValue("@TimeStamp", objresDematExecutePaymentInfo.TimeStamp);
                        sqlCmd.Parameters.AddWithValue("@RetrievalRef", objresDematExecutePaymentInfo.RetrievalRef);
                        sqlCmd.Parameters.AddWithValue("@UserName", objresDematExecutePaymentInfo.UserName);
                        sqlCmd.Parameters.AddWithValue("@Status", objresDematExecutePaymentInfo.Status);
                        sqlCmd.Parameters.AddWithValue("@BankCode", objresDematExecutePaymentInfo.BankCode);
                        sqlCmd.Parameters.AddWithValue("@ClientCode", objresDematExecutePaymentInfo.ClientCode);


                        sqlCmd.Parameters.AddWithValue("@Mode", objresDematExecutePaymentInfo.Mode);

                        sqlCmd.Parameters.Add("@RegIDOut", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@RegIDOut"].Direction = ParameterDirection.Output;

                        sqlCmd.Parameters.Add("@MsgStr", SqlDbType.VarChar, 500);
                        sqlCmd.Parameters["@MsgStr"].Direction = ParameterDirection.Output;

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