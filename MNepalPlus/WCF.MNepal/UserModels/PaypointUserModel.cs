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
    public class PaypointUserModel
    {

        #region request  payment
        public int RequestPaypointInfo(PaypointModel objreqPaypointInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNPaypoint]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@companyCode", objreqPaypointInfo.companyCode));
                        sqlCmd.Parameters.AddWithValue("@serviceCode", objreqPaypointInfo.serviceCode);
                        sqlCmd.Parameters.AddWithValue("@account", objreqPaypointInfo.account);
                        sqlCmd.Parameters.AddWithValue("@special1", objreqPaypointInfo.special1);
                        sqlCmd.Parameters.AddWithValue("@special2", objreqPaypointInfo.special2);

                        sqlCmd.Parameters.AddWithValue("@transactionDate", objreqPaypointInfo.transactionDate);
                        sqlCmd.Parameters.AddWithValue("@transactionId", objreqPaypointInfo.transactionId);

                        sqlCmd.Parameters.AddWithValue("@refStan", objreqPaypointInfo.refStan);
                        sqlCmd.Parameters.AddWithValue("@amount", objreqPaypointInfo.amount);
                        sqlCmd.Parameters.AddWithValue("@billNumber", objreqPaypointInfo.billNumber);

                        sqlCmd.Parameters.AddWithValue("@userId", objreqPaypointInfo.userId);
                        sqlCmd.Parameters.AddWithValue("@userPassword", objreqPaypointInfo.userPassword);
                        sqlCmd.Parameters.AddWithValue("@salePointType", objreqPaypointInfo.salePointType);
                        sqlCmd.Parameters.AddWithValue("@retrievalReference", objreqPaypointInfo.retrievalReference);
                        sqlCmd.Parameters.AddWithValue("@remarks", objreqPaypointInfo.remarks);
                        sqlCmd.Parameters.AddWithValue("@ClientCode", objreqPaypointInfo.ClientCode);
                        sqlCmd.Parameters.AddWithValue("@UserName", objreqPaypointInfo.UserName);
                        sqlCmd.Parameters.AddWithValue("@paypointType", objreqPaypointInfo.paypointType);








                        //sqlCmd.Parameters.AddWithValue("@ErrorMessage", objreqPaypointInfo.ErrorMessage);
                        sqlCmd.Parameters.AddWithValue("@Mode", objreqPaypointInfo.Mode);

                        sqlCmd.Parameters.Add("@RegIDOut", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@RegIDOut"].Direction = ParameterDirection.Output;

                        sqlCmd.Parameters.Add("@MsgStr", SqlDbType.VarChar, 500);
                        sqlCmd.Parameters["@MsgStr"].Direction = ParameterDirection.Output;

                        ret = sqlCmd.ExecuteNonQuery();
                        if (objreqPaypointInfo.Mode.Equals("SCA", StringComparison.InvariantCultureIgnoreCase))
                        {
                            ret = Convert.ToInt32(sqlCmd.Parameters["@RegIDOut"].Value);

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


        #region response paypoint payment (CP,EP,GP)
        public int ResponsePaypointInfo(PaypointModel objresPaypointInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNPaypointRes]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@companyCode", objresPaypointInfo.companyCode));
                        sqlCmd.Parameters.AddWithValue("@serviceCode", objresPaypointInfo.serviceCode);
                        sqlCmd.Parameters.AddWithValue("@account", objresPaypointInfo.account);
                        sqlCmd.Parameters.AddWithValue("@special1", objresPaypointInfo.special1);
                        sqlCmd.Parameters.AddWithValue("@special2", objresPaypointInfo.special2);

                        sqlCmd.Parameters.AddWithValue("@transactionDate", objresPaypointInfo.transactionDate);
                        sqlCmd.Parameters.AddWithValue("@transactionId", objresPaypointInfo.transactionId);

                        sqlCmd.Parameters.AddWithValue("@refStan", objresPaypointInfo.refStan);
                        sqlCmd.Parameters.AddWithValue("@amount", objresPaypointInfo.amount);
                        sqlCmd.Parameters.AddWithValue("@billNumber", objresPaypointInfo.billNumber);

                        sqlCmd.Parameters.AddWithValue("@userId", objresPaypointInfo.userId);
                        sqlCmd.Parameters.AddWithValue("@userPassword", objresPaypointInfo.userPassword);
                        sqlCmd.Parameters.AddWithValue("@salePointType", objresPaypointInfo.salePointType);
                        sqlCmd.Parameters.AddWithValue("@retrievalReference", objresPaypointInfo.retrievalReference);

                        sqlCmd.Parameters.AddWithValue("@responseCode", objresPaypointInfo.responseCode);
                        sqlCmd.Parameters.AddWithValue("@description", objresPaypointInfo.description);
                        sqlCmd.Parameters.AddWithValue("@customerName", objresPaypointInfo.customerName);
                        sqlCmd.Parameters.AddWithValue("@ClientCode", objresPaypointInfo.ClientCode);
                        sqlCmd.Parameters.AddWithValue("@UserName", objresPaypointInfo.UserName);
                        sqlCmd.Parameters.AddWithValue("@paypointType", objresPaypointInfo.paypointType);
                        sqlCmd.Parameters.AddWithValue("@resultMessage", objresPaypointInfo.resultMessage);





                        //sqlCmd.Parameters.AddWithValue("@ErrorMessage", objreqPaypointInfo.ErrorMessage);
                        sqlCmd.Parameters.AddWithValue("@Mode", objresPaypointInfo.Mode);

                        sqlCmd.Parameters.Add("@RegIDOut", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@RegIDOut"].Direction = ParameterDirection.Output;

                        sqlCmd.Parameters.Add("@MsgStr", SqlDbType.VarChar, 500);
                        sqlCmd.Parameters["@MsgStr"].Direction = ParameterDirection.Output;

                        ret = sqlCmd.ExecuteNonQuery();
                        if (objresPaypointInfo.Mode.Equals("SCA", StringComparison.InvariantCultureIgnoreCase))
                        {
                            ret = Convert.ToInt32(sqlCmd.Parameters["@RegIDOut"].Value);

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




        #region response paymentS
        public int ResponsePaypointPaymentInfo(PaypointModel objresPaypointPaymentInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNPaypointPaymentRes]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@description", objresPaypointPaymentInfo.descriptionP));
                        sqlCmd.Parameters.AddWithValue("@billDate", objresPaypointPaymentInfo.billDateP);
                        sqlCmd.Parameters.AddWithValue("@billAmount", objresPaypointPaymentInfo.billAmountP);
                        sqlCmd.Parameters.AddWithValue("@amount", objresPaypointPaymentInfo.amountP);
                        sqlCmd.Parameters.AddWithValue("@totalAmount", objresPaypointPaymentInfo.totalAmountP);

                        sqlCmd.Parameters.AddWithValue("@status", objresPaypointPaymentInfo.statusP);
                        sqlCmd.Parameters.AddWithValue("@amountfact", objresPaypointPaymentInfo.amountfactP);
                        sqlCmd.Parameters.AddWithValue("@amountmask", objresPaypointPaymentInfo.amountmaskP);
                        sqlCmd.Parameters.AddWithValue("@amountmax", objresPaypointPaymentInfo.amountmaxP);
                        sqlCmd.Parameters.AddWithValue("@amountmin", objresPaypointPaymentInfo.amountminP);

                        sqlCmd.Parameters.AddWithValue("@amountstep", objresPaypointPaymentInfo.amountstepP);
                        sqlCmd.Parameters.AddWithValue("@codserv", objresPaypointPaymentInfo.codservP);
                        sqlCmd.Parameters.AddWithValue("@commission", objresPaypointPaymentInfo.commissionP);
                        sqlCmd.Parameters.AddWithValue("@commisvalue", objresPaypointPaymentInfo.commisvalueP);
                        sqlCmd.Parameters.AddWithValue("@destination", objresPaypointPaymentInfo.destinationP);

                        sqlCmd.Parameters.AddWithValue("@fio", objresPaypointPaymentInfo.fioP);
                        sqlCmd.Parameters.AddWithValue("@i", objresPaypointPaymentInfo.iP);
                        sqlCmd.Parameters.AddWithValue("@id", objresPaypointPaymentInfo.idP);
                        sqlCmd.Parameters.AddWithValue("@j", objresPaypointPaymentInfo.jP);
                        sqlCmd.Parameters.AddWithValue("@requestId", objresPaypointPaymentInfo.requestIdP);
                        sqlCmd.Parameters.AddWithValue("@showCounter", objresPaypointPaymentInfo.show_counterP);
                        sqlCmd.Parameters.AddWithValue("@iCount", objresPaypointPaymentInfo.i_countP);

                        sqlCmd.Parameters.AddWithValue("@UserName", objresPaypointPaymentInfo.UserName);
                        sqlCmd.Parameters.AddWithValue("@ClientCode", objresPaypointPaymentInfo.ClientCode);



                        //sqlCmd.Parameters.AddWithValue("@ErrorMessage", objreqPaypointInfo.ErrorMessage);
                        sqlCmd.Parameters.AddWithValue("@Mode", objresPaypointPaymentInfo.Mode);

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



        #region response nepal waterpaymentS
        public int ResponsePaypointNepalWaterPaymentInfo(PaypointModel objresPaypointNepalWaterPaymentInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNPaypointNepalWaterPaymentRes]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@description", objresPaypointNepalWaterPaymentInfo.descriptionP));
                        sqlCmd.Parameters.AddWithValue("@billDate", objresPaypointNepalWaterPaymentInfo.billDateP);
                        sqlCmd.Parameters.AddWithValue("@billAmount", objresPaypointNepalWaterPaymentInfo.billAmountP);
                        sqlCmd.Parameters.AddWithValue("@amount", objresPaypointNepalWaterPaymentInfo.amountP);
                        sqlCmd.Parameters.AddWithValue("@totalAmount", objresPaypointNepalWaterPaymentInfo.totalAmountP);

                        sqlCmd.Parameters.AddWithValue("@status", objresPaypointNepalWaterPaymentInfo.statusP);
                        sqlCmd.Parameters.AddWithValue("@amountfact", objresPaypointNepalWaterPaymentInfo.amountfactP);
                        sqlCmd.Parameters.AddWithValue("@amountmask", objresPaypointNepalWaterPaymentInfo.amountmaskP);
                        sqlCmd.Parameters.AddWithValue("@amountmax", objresPaypointNepalWaterPaymentInfo.amountmaxP);
                        sqlCmd.Parameters.AddWithValue("@amountmin", objresPaypointNepalWaterPaymentInfo.amountminP);

                        sqlCmd.Parameters.AddWithValue("@amountstep", objresPaypointNepalWaterPaymentInfo.amountstepP);
                        sqlCmd.Parameters.AddWithValue("@codserv", objresPaypointNepalWaterPaymentInfo.codservP);
                        sqlCmd.Parameters.AddWithValue("@commission", objresPaypointNepalWaterPaymentInfo.commissionP);
                        sqlCmd.Parameters.AddWithValue("@commisvalue", objresPaypointNepalWaterPaymentInfo.commisvalueP);
                        sqlCmd.Parameters.AddWithValue("@destination", objresPaypointNepalWaterPaymentInfo.destinationP);

                        sqlCmd.Parameters.AddWithValue("@fio", objresPaypointNepalWaterPaymentInfo.fioP);
                        sqlCmd.Parameters.AddWithValue("@i", objresPaypointNepalWaterPaymentInfo.iP);
                        sqlCmd.Parameters.AddWithValue("@id", objresPaypointNepalWaterPaymentInfo.idP);
                        sqlCmd.Parameters.AddWithValue("@j", objresPaypointNepalWaterPaymentInfo.jP);
                        sqlCmd.Parameters.AddWithValue("@requestId", objresPaypointNepalWaterPaymentInfo.requestIdP);

                        sqlCmd.Parameters.AddWithValue("@showCounter", objresPaypointNepalWaterPaymentInfo.show_counterP);
                        sqlCmd.Parameters.AddWithValue("@iCount", objresPaypointNepalWaterPaymentInfo.i_countP);
                        sqlCmd.Parameters.AddWithValue("@legatNumber", objresPaypointNepalWaterPaymentInfo.legatNumberP);
                        sqlCmd.Parameters.AddWithValue("@discountAmount", objresPaypointNepalWaterPaymentInfo.discountAmountP);
                        sqlCmd.Parameters.AddWithValue("@counterRent", objresPaypointNepalWaterPaymentInfo.counterRentP);

                        sqlCmd.Parameters.AddWithValue("@fineAmount", objresPaypointNepalWaterPaymentInfo.fineAmountP);
                        sqlCmd.Parameters.AddWithValue("@billDateFrom", objresPaypointNepalWaterPaymentInfo.billDateFromP);
                        sqlCmd.Parameters.AddWithValue("@billDateTo", objresPaypointNepalWaterPaymentInfo.billDateToP);

                        sqlCmd.Parameters.AddWithValue("@UserName", objresPaypointNepalWaterPaymentInfo.UserName);
                        sqlCmd.Parameters.AddWithValue("@ClientCode", objresPaypointNepalWaterPaymentInfo.ClientCode);



                        //sqlCmd.Parameters.AddWithValue("@ErrorMessage", objreqPaypointInfo.ErrorMessage);
                        sqlCmd.Parameters.AddWithValue("@Mode", objresPaypointNepalWaterPaymentInfo.Mode);

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

        #region response Wlink paymentS
        public int ResponsePaypointWlinkPaymentInfo(PaypointModel objresPaypointWlinkPaymentInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNPaypointWlinkPaymentRes]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.AddWithValue("@description", objresPaypointWlinkPaymentInfo.description);
                        sqlCmd.Parameters.AddWithValue("@packageAmount", objresPaypointWlinkPaymentInfo.amountP);
                        sqlCmd.Parameters.AddWithValue("@packageId", objresPaypointWlinkPaymentInfo.PackageId);
                        sqlCmd.Parameters.AddWithValue("@billDate", objresPaypointWlinkPaymentInfo.billDateP);
                        sqlCmd.Parameters.AddWithValue("@billAmount", objresPaypointWlinkPaymentInfo.billAmountP);
                        sqlCmd.Parameters.AddWithValue("@billNumber", objresPaypointWlinkPaymentInfo.billNumberCP);
                        sqlCmd.Parameters.AddWithValue("@refStan", objresPaypointWlinkPaymentInfo.refStanCP);
                        sqlCmd.Parameters.AddWithValue("@customerName", objresPaypointWlinkPaymentInfo.customerNameCP);
                        sqlCmd.Parameters.AddWithValue("@companyCode", objresPaypointWlinkPaymentInfo.companyCodeCP);
                        sqlCmd.Parameters.AddWithValue("@UserName", objresPaypointWlinkPaymentInfo.userId);
                        sqlCmd.Parameters.AddWithValue("@ClientCode", objresPaypointWlinkPaymentInfo.customer_codeKI);
                        sqlCmd.Parameters.AddWithValue("@remainingDays", objresPaypointWlinkPaymentInfo.RemainingDays);

                        sqlCmd.Parameters.AddWithValue("@Mode", objresPaypointWlinkPaymentInfo.Mode);

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

        #region response Subisu paymentS
        public int ResponsePaypointSubisuPaymentInfo(PaypointModel objresPaypointSubisuPaymentInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNPaypointWlinkPaymentRes]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.AddWithValue("@description", "");
                        sqlCmd.Parameters.AddWithValue("@packageAmount","");
                        sqlCmd.Parameters.AddWithValue("@packageId", "");
                        sqlCmd.Parameters.AddWithValue("@billDate", objresPaypointSubisuPaymentInfo.billDateP);
                        sqlCmd.Parameters.AddWithValue("@billAmount", objresPaypointSubisuPaymentInfo.billAmountP);
                        sqlCmd.Parameters.AddWithValue("@billNumber", objresPaypointSubisuPaymentInfo.billNumberCP);
                        sqlCmd.Parameters.AddWithValue("@refStan", objresPaypointSubisuPaymentInfo.refStanCP);
                        sqlCmd.Parameters.AddWithValue("@customerName", objresPaypointSubisuPaymentInfo.customerNameCP);
                        sqlCmd.Parameters.AddWithValue("@companyCode", objresPaypointSubisuPaymentInfo.companyCodeCP);
                        sqlCmd.Parameters.AddWithValue("@UserName", objresPaypointSubisuPaymentInfo.userId);
                        sqlCmd.Parameters.AddWithValue("@ClientCode", objresPaypointSubisuPaymentInfo.customer_codeKI);
                        sqlCmd.Parameters.AddWithValue("@remainingDays", "");

                        sqlCmd.Parameters.AddWithValue("@Mode", objresPaypointSubisuPaymentInfo.Mode);

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

        #region response vianet
        public int ResponsePaypointVianetPaymentInfo(PaypointModel objresPaypointVianetPaymentInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNPaypointVianetPaymentRes]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.AddWithValue("@description", objresPaypointVianetPaymentInfo.description); 
                        sqlCmd.Parameters.AddWithValue("@packageAmount", objresPaypointVianetPaymentInfo.amountP); 
                        sqlCmd.Parameters.AddWithValue("@packageId", objresPaypointVianetPaymentInfo.PackageId);
                        sqlCmd.Parameters.AddWithValue("@billNumber", objresPaypointVianetPaymentInfo.billNumber);
                        sqlCmd.Parameters.AddWithValue("@refStan", objresPaypointVianetPaymentInfo.refStan);
                        sqlCmd.Parameters.AddWithValue("@billAmount", objresPaypointVianetPaymentInfo.amount);
                        sqlCmd.Parameters.AddWithValue("@billDate", objresPaypointVianetPaymentInfo.transactionDate);
                        sqlCmd.Parameters.AddWithValue("@customerName", objresPaypointVianetPaymentInfo.customerName);
                        sqlCmd.Parameters.AddWithValue("@companyCode", objresPaypointVianetPaymentInfo.companyCode);
                        sqlCmd.Parameters.AddWithValue("@UserName", objresPaypointVianetPaymentInfo.UserName);
                        sqlCmd.Parameters.AddWithValue("@ClientCode", objresPaypointVianetPaymentInfo.ClientCode);
                        sqlCmd.Parameters.AddWithValue("@Mode", objresPaypointVianetPaymentInfo.Mode);

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

        #region response dish home online
        public int ResponsePaypointDishHomeOnlinePaymentInfo(PaypointModel objresPaypointDishHomeOnlinePaymentInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNPaypointTvServicePaymentRes]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.AddWithValue("@billDate", objresPaypointDishHomeOnlinePaymentInfo.transactionDate);
                        sqlCmd.Parameters.AddWithValue("@billAmount", objresPaypointDishHomeOnlinePaymentInfo.amount);
                        sqlCmd.Parameters.AddWithValue("@billNumber", objresPaypointDishHomeOnlinePaymentInfo.billNumber);
                        sqlCmd.Parameters.AddWithValue("@refStan", objresPaypointDishHomeOnlinePaymentInfo.refStan);
                        sqlCmd.Parameters.AddWithValue("@customerName", objresPaypointDishHomeOnlinePaymentInfo.customerName);
                        sqlCmd.Parameters.AddWithValue("@companyCode", objresPaypointDishHomeOnlinePaymentInfo.companyCode);
                        sqlCmd.Parameters.AddWithValue("@UserName", objresPaypointDishHomeOnlinePaymentInfo.UserName);
                        sqlCmd.Parameters.AddWithValue("@ClientCode", objresPaypointDishHomeOnlinePaymentInfo.ClientCode);
                        sqlCmd.Parameters.AddWithValue("@description", objresPaypointDishHomeOnlinePaymentInfo.description);
                        sqlCmd.Parameters.AddWithValue("@packageAmount", objresPaypointDishHomeOnlinePaymentInfo.amountP);
                        sqlCmd.Parameters.AddWithValue("@bonus", objresPaypointDishHomeOnlinePaymentInfo.Bonus);
                        sqlCmd.Parameters.AddWithValue("@Mode", objresPaypointDishHomeOnlinePaymentInfo.Mode);



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

        #region response utility
        public int ResponsePaypointUtilityInfo(PaypointModel objresPaypointPaymentInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNTopUp]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.AddWithValue("@description", objresPaypointPaymentInfo.description);
                        sqlCmd.Parameters.AddWithValue("@packageAmount", objresPaypointPaymentInfo.amountP);
                        sqlCmd.Parameters.AddWithValue("@billDate", objresPaypointPaymentInfo.transactionDate);
                        sqlCmd.Parameters.AddWithValue("@billAmount", objresPaypointPaymentInfo.amount);
                        sqlCmd.Parameters.AddWithValue("@billNumber", objresPaypointPaymentInfo.billNumber);
                        sqlCmd.Parameters.AddWithValue("@refStan", objresPaypointPaymentInfo.refStan);
                        sqlCmd.Parameters.AddWithValue("@serviceNumber", objresPaypointPaymentInfo.customerName);
                        sqlCmd.Parameters.AddWithValue("@companyCode", objresPaypointPaymentInfo.companyCode);
                        sqlCmd.Parameters.AddWithValue("@userName", objresPaypointPaymentInfo.UserName);
                        sqlCmd.Parameters.AddWithValue("@clientCode", objresPaypointPaymentInfo.ClientCode);
                        sqlCmd.Parameters.AddWithValue("@serviceCode", objresPaypointPaymentInfo.serviceCode);
                        sqlCmd.Parameters.AddWithValue("@Mode", objresPaypointPaymentInfo.Mode);



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




        #region Response Checkpayment for khanepani invoice
        public int ResponsePaypointKhanepaniInvoiceInfo(PaypointModel objresPaypointKhanepaniInvoiceInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNPaypointKhanepaniInvoiceRes]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@status", objresPaypointKhanepaniInvoiceInfo.statusKI));
                        sqlCmd.Parameters.AddWithValue("@total_advance_amount", objresPaypointKhanepaniInvoiceInfo.total_advance_amountKI);
                        sqlCmd.Parameters.AddWithValue("@customer_code", objresPaypointKhanepaniInvoiceInfo.customer_codeKI);
                        sqlCmd.Parameters.AddWithValue("@address", objresPaypointKhanepaniInvoiceInfo.addressKI);
                        sqlCmd.Parameters.AddWithValue("@total_credit_sales_amount", objresPaypointKhanepaniInvoiceInfo.total_credit_sales_amountKI);

                        sqlCmd.Parameters.AddWithValue("@customer_name", objresPaypointKhanepaniInvoiceInfo.customer_nameKI);
                        sqlCmd.Parameters.AddWithValue("@current_month_dues", objresPaypointKhanepaniInvoiceInfo.current_month_duesKI);
                        sqlCmd.Parameters.AddWithValue("@mobile_number", objresPaypointKhanepaniInvoiceInfo.mobile_numberKI);
                        sqlCmd.Parameters.AddWithValue("@total_dues", objresPaypointKhanepaniInvoiceInfo.total_duesKI);
                        sqlCmd.Parameters.AddWithValue("@previous_dues", objresPaypointKhanepaniInvoiceInfo.previous_duesKI);

                        sqlCmd.Parameters.AddWithValue("@current_month_discount", objresPaypointKhanepaniInvoiceInfo.current_month_discountKI);
                        sqlCmd.Parameters.AddWithValue("@current_month_fine", objresPaypointKhanepaniInvoiceInfo.current_month_fineKI);
                        sqlCmd.Parameters.AddWithValue("@refStan", objresPaypointKhanepaniInvoiceInfo.refStan);
                        sqlCmd.Parameters.AddWithValue("@UserName", objresPaypointKhanepaniInvoiceInfo.UserName);
                        sqlCmd.Parameters.AddWithValue("@ClientCode", objresPaypointKhanepaniInvoiceInfo.ClientCode);
                        
                          

                         

                        //sqlCmd.Parameters.AddWithValue("@ErrorMessage", objreqPaypointInfo.ErrorMessage);
                        sqlCmd.Parameters.AddWithValue("@Mode", objresPaypointKhanepaniInvoiceInfo.Mode);

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


        #region "Get Payment Details"
        public DataSet GetNEAPaymentDetails(NEAFundTransfer objUserInfo)
        {
            SqlConnection sqlCon = null;
            DataSet dtset = null;

            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    string query = "SELECT TOP 1 * FROM MNPaypointResponse " +
                        "WHERE account='" + objUserInfo.SCNo + "' AND special1='" + objUserInfo.NEABranchCode + "' AND special2='" + objUserInfo.CustomerID +
                        "' AND ClientCode='" + objUserInfo.ClientCode + "' AND UserName='" + objUserInfo.UserName +
                        "' AND refStan='" + objUserInfo.refStan +
                        "' ORDER BY transactionId DESC";    //"' AND retrievalReference ='" + objUserInfo.retrievalReference + 
                    //account =@SCNo AND special1=@NEABranchName AND special2=@CustomerID AND ClientCode=@ClientCode AND UserName=@UserName

                    using (SqlCommand sqlCmd = new SqlCommand(query, sqlCon)) //"[s_MNPaypoints]"
                    {
                        sqlCmd.CommandType = CommandType.Text;

                        //sqlCmd.Parameters.AddWithValue("@SCNo", objUserInfo.SCNo);
                        //sqlCmd.Parameters.AddWithValue("@NEABranchName", objUserInfo.NEABranchCode);
                        //sqlCmd.Parameters.AddWithValue("@CustomerID",  objUserInfo.CustomerID);
                        //sqlCmd.Parameters.AddWithValue("@ClientCode", objUserInfo.ClientCode);
                        //sqlCmd.Parameters.AddWithValue("@UserName", objUserInfo.UserName);
                        //sqlCmd.Parameters.AddWithValue("@mode", objUserInfo.Mode);
                        //sqlCmd.Parameters.AddWithValue("@KhanepaniCounter", "");
                        //sqlCmd.Parameters.AddWithValue("@refStan", objUserInfo.refStan);

                        DataSet dataset = new DataSet();
                        using (SqlDataAdapter daAcType = new SqlDataAdapter(sqlCmd))
                        {
                            using (dataset = new DataSet())
                            {
                                daAcType.Fill(dataset, "dtResponse");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtset = dataset;
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
                if (sqlCon != null)
                {
                    sqlCon.Close();
                }
            }

            return dtset;
        }


        public DataSet GetNEAPaymentDetailsPay(NEAFundTransfer objUserInfo)
        {
            SqlConnection sqlCon = null;
            DataSet dtset = null;

            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();

                    string queryPayment = "SELECT TOP 1 * FROM MNPaypointPayments " +
                        "WHERE ClientCode = '" + objUserInfo.ClientCode + "' AND UserName = '" + objUserInfo.UserName + "' AND requestId = '" + objUserInfo.refStan + "'";
                    //ClientCode = @ClientCode AND UserName = @UserName AND requestId = @refStan 

                    using (SqlCommand sqlCmdQ = new SqlCommand(queryPayment, sqlCon)) //"[s_MNPaypoints]"
                    {
                        sqlCmdQ.CommandType = CommandType.Text;

                        DataSet dataset = new DataSet();
                        using (SqlDataAdapter daAcType = new SqlDataAdapter(sqlCmdQ))
                        {
                            using (dataset = new DataSet())
                            {
                                daAcType.Fill(dataset, "dtPayment");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtset = dataset;
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
                if (sqlCon != null)
                {
                    sqlCon.Close();
                }
            }

            return dtset;
        }

        #endregion


        #region Get NEA refStan From Response Table
        public static string getrefStan(NEAFundTransfer NEAObj)
        {
            string Query_refStan = "select refStan from MNPaypointResponse where account='" + NEAObj.SCNo + "' AND special1='" + NEAObj.NEABranchCode + "' AND special2='" + NEAObj.CustomerID + "' AND ClientCode='" + NEAObj.ClientCode + "' AND UserName='" + NEAObj.UserName + "'";
            DataTable dt = new DataTable();
            string refStan = string.Empty;

            using (SqlConnection conn = new SqlConnection(DatabaseConnection.ConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(Query_refStan, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        // set the CommandTimeout
                        da.SelectCommand.CommandTimeout = 60;  //seconds
                        using (dt = new DataTable())
                        {
                            da.Fill(dt);
                            foreach (DataRow row in dt.Rows)
                            {
                                refStan = row["refStan"].ToString();
                            }
                        }
                    }
                }
            }
            return refStan;
        }
        #endregion



        /// <summary>
        /// Retrieve the user information based on mode
        /// </summary>
        /// <param name="objUserInfo">Pass an instance of User information</param>
        /// <returns>Returns the  avail baln information based on user information</returns>
        public DataTable GetUserAvailBaln(UserInfo objUserInfo)
        {
            DataTable dtableResult = null;
            SqlConnection conn = null;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNUserWalletAvailBaln]", conn))
                    {
                        cmd.Parameters.AddWithValue("@ClientCode", objUserInfo.ClientCode);
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 60;  // seconds
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset, "dtUserAvailBaln");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables["dtUserAvailBaln"];
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


        #region Get KP refStan From Response Table
        public static string getKPrefStan(Khanepani KPObj)
        {
            string Query_refStan = "select Top 1 refStan from MNPaypointResponse where account='" + KPObj.CustomerID + "' AND serviceCode='" + KPObj.KhanepaniCounter + "' AND ClientCode='" + KPObj.ClientCode + "' AND UserName='" + KPObj.UserName + "'";
            DataTable dt = new DataTable();
            string refStan = string.Empty;

            using (SqlConnection conn = new SqlConnection(DatabaseConnection.ConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(Query_refStan, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        // set the CommandTimeout
                        da.SelectCommand.CommandTimeout = 60;  //seconds
                        using (dt = new DataTable())
                        {
                            da.Fill(dt);
                            foreach (DataRow row in dt.Rows)
                            {
                                refStan = row["refStan"].ToString();
                            }
                        }
                    }
                }
            }
            return refStan;
        }
        #endregion


        #region "Get KP Payment Details"
        public DataSet GetKPPaymentDetails(Khanepani objUserInfo)
        {
            SqlConnection sqlCon = null;
            DataSet dtset = null;

            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    string query = "SELECT TOP 1 * FROM MNPaypointResponse " +
                        "WHERE account='" + objUserInfo.CustomerID + "' AND serviceCode='" + objUserInfo.KhanepaniCounter +
                        "' AND ClientCode='" + objUserInfo.ClientCode + "' AND UserName='" + objUserInfo.UserName +
                        "' AND refStan='" + objUserInfo.refStan +
                        "' ORDER BY transactionId DESC"; //"' AND retrievalReference ='" + objUserInfo.retrievalReference + 

                    using (SqlCommand sqlCmd = new SqlCommand(query, sqlCon)) //"[s_MNPaypoints]"
                    {
                        sqlCmd.CommandType = CommandType.Text;

                        DataSet dataset = new DataSet();
                        using (SqlDataAdapter daAcType = new SqlDataAdapter(sqlCmd))
                        {
                            using (dataset = new DataSet())
                            {
                                daAcType.Fill(dataset, "dtResponse");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtset = dataset;
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
                if (sqlCon != null)
                {
                    sqlCon.Close();
                }
            }

            return dtset;
        }


        public DataSet GetKPPaymentDetailsPay(Khanepani objUserInfo)
        {
            SqlConnection sqlCon = null;
            DataSet dtset = null;

            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();

                    string queryPayment = "SELECT TOP 1 * FROM MNPaypointKhanepaniInvoice " +
                        "WHERE ClientCode = '" + objUserInfo.ClientCode + "' AND UserName = '" + objUserInfo.UserName + "' AND refStan = '" + objUserInfo.refStan + "'";
                    //ClientCode = @ClientCode AND UserName = @UserName AND requestId = @refStan 

                    using (SqlCommand sqlCmdQ = new SqlCommand(queryPayment, sqlCon)) //"[s_MNPaypoints]"
                    {
                        sqlCmdQ.CommandType = CommandType.Text;

                        DataSet dataset = new DataSet();
                        using (SqlDataAdapter daAcType = new SqlDataAdapter(sqlCmdQ))
                        {
                            using (dataset = new DataSet())
                            {
                                daAcType.Fill(dataset, "dtKhanepaniInvoice");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtset = dataset;
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
                if (sqlCon != null)
                {
                    sqlCon.Close();
                }
            }

            return dtset;
        }

        #endregion


        #region "Get NW Payment Details"
        public DataSet GetNWPaymentDetails(NepalWater objUserInfo)
        {
            SqlConnection sqlCon = null;
            DataSet dtset = null;

            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    string query = "SELECT TOP 1 * FROM MNPaypointResponse " +
                        "WHERE account='" + objUserInfo.CustomerID + "' AND serviceCode='" + objUserInfo.NWCounter +
                        "' AND ClientCode='" + objUserInfo.ClientCode + "' AND UserName='" + objUserInfo.UserName +
                        "' AND refStan='" + objUserInfo.refStan +
                        "' ORDER BY transactionId DESC";

                    using (SqlCommand sqlCmd = new SqlCommand(query, sqlCon)) //"[s_MNPaypoints]"
                    {
                        sqlCmd.CommandType = CommandType.Text;

                        DataSet dataset = new DataSet();
                        using (SqlDataAdapter daAcType = new SqlDataAdapter(sqlCmd))
                        {
                            using (dataset = new DataSet())
                            {
                                daAcType.Fill(dataset, "dtResponse");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtset = dataset;
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
                if (sqlCon != null)
                {
                    sqlCon.Close();
                }
            }

            return dtset;
        }


        public DataSet GetNWPaymentDetailsPay(NepalWater objUserInfo)
        {
            SqlConnection sqlCon = null;
            DataSet dtset = null;

            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();

                    string queryPayment = "SELECT TOP 1 * FROM MNPaypointNepalWaterPayments " +
                        "WHERE ClientCode = '" + objUserInfo.ClientCode + "' AND UserName = '" + objUserInfo.UserName + "' AND requestId = '" + objUserInfo.refStan + "'";
                    //ClientCode = @ClientCode AND UserName = @UserName AND requestId = @refStan 

                    using (SqlCommand sqlCmdQ = new SqlCommand(queryPayment, sqlCon)) //"[s_MNPaypoints]"
                    {
                        sqlCmdQ.CommandType = CommandType.Text;

                        DataSet dataset = new DataSet();
                        using (SqlDataAdapter daAcType = new SqlDataAdapter(sqlCmdQ))
                        {
                            using (dataset = new DataSet())
                            {
                                daAcType.Fill(dataset, "dtPayment");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtset = dataset;
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
                if (sqlCon != null)
                {
                    sqlCon.Close();
                }
            }

            return dtset;
        }

        #endregion

        #region response paypoint payment (CP,EP,GP)
        public int ResponsePaypointDHPinInfo(PaypointModel objresPaypointInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNPaypointDHPinRes]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@companyCode", objresPaypointInfo.companyCode));
                        sqlCmd.Parameters.AddWithValue("@serviceCode", objresPaypointInfo.serviceCode);
                        sqlCmd.Parameters.AddWithValue("@account", objresPaypointInfo.account);
                        sqlCmd.Parameters.AddWithValue("@special1", objresPaypointInfo.special1);
                        sqlCmd.Parameters.AddWithValue("@special2", objresPaypointInfo.special2);

                        sqlCmd.Parameters.AddWithValue("@transactionDate", objresPaypointInfo.transactionDate);
                        sqlCmd.Parameters.AddWithValue("@transactionId", objresPaypointInfo.transactionId);

                        sqlCmd.Parameters.AddWithValue("@refStan", objresPaypointInfo.refStan);
                        sqlCmd.Parameters.AddWithValue("@amount", objresPaypointInfo.amount);
                        sqlCmd.Parameters.AddWithValue("@billNumber", objresPaypointInfo.billNumber);

                        sqlCmd.Parameters.AddWithValue("@userId", objresPaypointInfo.userId);
                        sqlCmd.Parameters.AddWithValue("@userPassword", objresPaypointInfo.userPassword);
                        sqlCmd.Parameters.AddWithValue("@salePointType", objresPaypointInfo.salePointType);
                        sqlCmd.Parameters.AddWithValue("@retrievalReference", objresPaypointInfo.retrievalReference);

                        sqlCmd.Parameters.AddWithValue("@responseCode", objresPaypointInfo.responseCode);
                        sqlCmd.Parameters.AddWithValue("@description", objresPaypointInfo.description);
                        sqlCmd.Parameters.AddWithValue("@customerName", objresPaypointInfo.customerName);
                        sqlCmd.Parameters.AddWithValue("@ClientCode", objresPaypointInfo.ClientCode);
                        sqlCmd.Parameters.AddWithValue("@UserName", objresPaypointInfo.UserName);
                        sqlCmd.Parameters.AddWithValue("@paypointType", objresPaypointInfo.paypointType);
                        sqlCmd.Parameters.AddWithValue("@resultMessage", objresPaypointInfo.resultMessage);
                        sqlCmd.Parameters.AddWithValue("@voucherCode", objresPaypointInfo.voucherCode);
                        sqlCmd.Parameters.AddWithValue("@id", objresPaypointInfo.id);





                        //sqlCmd.Parameters.AddWithValue("@ErrorMessage", objreqPaypointInfo.ErrorMessage);
                        sqlCmd.Parameters.AddWithValue("@Mode", objresPaypointInfo.Mode);

                        sqlCmd.Parameters.Add("@RegIDOut", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@RegIDOut"].Direction = ParameterDirection.Output;

                        sqlCmd.Parameters.Add("@MsgStr", SqlDbType.VarChar, 500);
                        sqlCmd.Parameters["@MsgStr"].Direction = ParameterDirection.Output;

                        ret = sqlCmd.ExecuteNonQuery();
                        if (objresPaypointInfo.Mode.Equals("SCA", StringComparison.InvariantCultureIgnoreCase))
                        {
                            ret = Convert.ToInt32(sqlCmd.Parameters["@RegIDOut"].Value);

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