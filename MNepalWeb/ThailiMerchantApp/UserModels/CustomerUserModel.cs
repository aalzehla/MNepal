using Microsoft.Practices.EnterpriseLibrary.Data;
using ThailiMerchantApp.Connection;
using ThailiMerchantApp.Models;
using ThailiMerchantApp.Utilities;
using ThailiMerchantApp.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ThailiMerchantApp.UserModels
{
    public class CustomerUserModel
    {
        /// <summary>
        /// Retrieve the customer detail information based on mode
        /// </summary>
        /// <param name="objUserInfo">Pass an instance of User information</param>
        /// <returns>Returns the customer detail information based on customer information</returns>
        public DataTable GetCustomerDetailInformation(UserInfo objUserInfo)
        {
            Database database;
            DataTable dtableResult = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNUserInfoSearch]"))
                {
                    database.AddInParameter(command, "@MobileNo", DbType.String, objUserInfo.ContactNumber1); 
                    database.AddInParameter(command, "@Name", DbType.String, objUserInfo.Name);
                    database.AddInParameter(command, "@WalletNumber", DbType.String, objUserInfo.WalletNumber);
                    database.AddInParameter(command, "@ClientCode", DbType.String, objUserInfo.ClientCode);
                    database.AddInParameter(command, "@mode", DbType.String, objUserInfo.Mode);
                    using (var dataset = new DataSet())
                    {
                        database.LoadDataSet(command, dataset, "dtCustomerInfo");

                        if (dataset.Tables.Count > 0)
                        {
                            dtableResult = dataset.Tables["dtCustomerInfo"];
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
                database = null;
                Database.ClearParameterCache();
            }

            return dtableResult;
        }

        /// <summary>
        /// Retrieve the user information based on mode
        /// </summary>
        /// <param name="objCustomerUserInfo">Pass an instance of User information</param>
        /// <returns>Returns the pin information based on user information</returns>
        public int UpdateCustomerUserInfo(UserInfo objCustomerUserInfo)
        {

            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNCustomerInfoUpdate]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", objCustomerUserInfo.ClientCode));
                        sqlCmd.Parameters.Add(new SqlParameter("@Name", objCustomerUserInfo.Name));

                        //sqlCmd.Parameters.Add(new SqlParameter("@FName"," " ));
                        //sqlCmd.Parameters.Add(new SqlParameter("@MName", " "));
                        //sqlCmd.Parameters.Add(new SqlParameter("@LName", " "));
                        //sqlCmd.Parameters.Add(new SqlParameter("@Gender", objCustomerUserInfo.Gender));

                        sqlCmd.Parameters.Add(new SqlParameter("@ProfileCode", objCustomerUserInfo.ProfileCode));
                        sqlCmd.Parameters.Add(new SqlParameter("@EmailAddress", objCustomerUserInfo.EmailAddress));
                        sqlCmd.Parameters.Add(new SqlParameter("@AcBranchCode", objCustomerUserInfo.BranchCode));
                        sqlCmd.Parameters.Add(new SqlParameter("@BankNumber", objCustomerUserInfo.BankNo));
                        sqlCmd.Parameters.Add(new SqlParameter("@BankAccountNumber", objCustomerUserInfo.BankAccountNumber));
                        //sqlCmd.Parameters.Add(new SqlParameter("@COC", objCustomerUserInfo.COC));
                        sqlCmd.Parameters.Add(new SqlParameter("@mode", objCustomerUserInfo.Mode));
                        sqlCmd.Parameters.Add(new SqlParameter("@Address", objCustomerUserInfo.Address));
                        sqlCmd.Parameters.Add(new SqlParameter("@NewMobileNo", objCustomerUserInfo.NewMobileNo));
                        sqlCmd.Parameters.Add(new SqlParameter("@ContactNumber1", objCustomerUserInfo.ContactNumber1));
                        sqlCmd.Parameters.Add(new SqlParameter("@ContactNumber2", objCustomerUserInfo.ContactNumber2));
                        sqlCmd.Parameters.Add(new SqlParameter("@Pin"," "));
                        sqlCmd.Parameters.Add(new SqlParameter("@ClientStatus", objCustomerUserInfo.ClientStatus));
                        sqlCmd.Parameters.Add(new SqlParameter("@Status", objCustomerUserInfo.Status));
                        sqlCmd.Parameters.Add(new SqlParameter("@TxnAccounts", objCustomerUserInfo.TxnAccounts));
                        sqlCmd.Parameters.Add(new SqlParameter("@ModifyingBranch", objCustomerUserInfo.AdminBranch));
                        sqlCmd.Parameters.Add(new SqlParameter("@ModifiedBy", objCustomerUserInfo.AdminUserName));
                        sqlCmd.Parameters.Add(new SqlParameter("@IsApproved", objCustomerUserInfo.IsApproved));
                        //Added//
                        sqlCmd.Parameters.Add(new SqlParameter("@IndvTxn", objCustomerUserInfo.Transaction));
                        sqlCmd.Parameters.Add(new SqlParameter("@DateRange", objCustomerUserInfo.DateRange));
                        sqlCmd.Parameters.Add(new SqlParameter("@StartDate", objCustomerUserInfo.StartDate));
                        sqlCmd.Parameters.Add(new SqlParameter("@EndDate", objCustomerUserInfo.EndDate));
                        sqlCmd.Parameters.Add(new SqlParameter("@TransactionLimit", objCustomerUserInfo.TransactionLimit));
                        sqlCmd.Parameters.Add(new SqlParameter("@LimitType", objCustomerUserInfo.LimitType));
                        sqlCmd.Parameters.Add(new SqlParameter("@TransactionLimitMonthly", objCustomerUserInfo.TransactionLimitMonthly));
                        sqlCmd.Parameters.Add(new SqlParameter("@TransactionCount", objCustomerUserInfo.TransactionCount));
                        sqlCmd.Parameters.Add(new SqlParameter("@TransactionLimitDaily", objCustomerUserInfo.TransactionLimitDaily));
                        ////////
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





            //Database database;
            //int ret;

            //try
            //{
            //    database = DatabaseConnection.GetDatabase();
            //    using (var command = database.GetStoredProcCommand("[s_MNAdminInfoUpdate]"))
            //    {
            //        database.AddInParameter(command, "@ClientCode", DbType.String, objCustomerUserInfo.ClientCode);
            //        //database.AddInParameter(command, "@Address", DbType.String, objCustomerUserInfo.Address);
            //        //database.AddInParameter(command, "@Status", DbType.String, objCustomerUserInfo.Status);
            //        database.AddInParameter(command, "@ProfileName", DbType.String, objCustomerUserInfo.ProfileName);
            //        database.AddInParameter(command, "@Name", DbType.String, objCustomerUserInfo.Name);
            //        database.AddInParameter(command, "@EmailAddress", DbType.String, objCustomerUserInfo.EmailAddress);
            //        database.AddInParameter(command, "@BranchCode", DbType.String, objCustomerUserInfo.BranchCode);
            //        database.AddInParameter(command, "@COC", DbType.String, objCustomerUserInfo.COC);
            //        database.AddInParameter(command, "@ReturnValue", DbType.Int32, objCustomerUserInfo.ReturnValue);


            //        //database.AddInParameter(command, "@ContactNumber1", DbType.String, objCustomerUserInfo.ContactNumber1);
            //        //database.AddInParameter(command, "@ContactNumber2", DbType.String, objCustomerUserInfo.ContactNumber2);
            //        //database.AddInParameter(command, "@WalletNumber", DbType.String, objCustomerUserInfo.WalletNumber);

            //        //database.AddInParameter(command, "@BINBankCode", DbType.String, objCustomerUserInfo.BankNo);
            //        //database.AddInParameter(command, "@BankBranchCode", DbType.String, objCustomerUserInfo.BranchCode);
            //        //database.AddInParameter(command, "@BankAccountNumber", DbType.String, objCustomerUserInfo.BankAccountNumber);
            //        //database.AddInParameter(command, "@PIN", DbType.String, objCustomerUserInfo.PIN);

            //        //database.AddInParameter(command, "@IsApproved", DbType.String, objCustomerUserInfo.IsApproved);
            //        //database.AddInParameter(command, "@IsRejected", DbType.String, objCustomerUserInfo.IsRejected);

            //        database.AddInParameter(command, "@mode", DbType.String, objCustomerUserInfo.Mode);
            //        //database.AddOutParameter(command, "@RegIDOut", DbType.Int32, objCustomerUserInfo.RegIDOut);
            //        ret = database.ExecuteNonQuery(command);
            //        if (objCustomerUserInfo.Mode.Equals("UAINFO", StringComparison.InvariantCultureIgnoreCase))
            //        {
            //            ret = (int)database.GetParameterValue(command, "RegIDOut");
            //        }
            //    }

            //}
            //catch (Exception ex)
            //{
            //    throw new Exception(ex.Message);
            //}
            //finally
            //{
            //    database = null;
            //    Database.ClearParameterCache();
            //}

            //    return ret;
        }


        public int UpdateAgentInfo(UserInfo objCustomerUserInfo)
        {

            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNAgentUpdate]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", objCustomerUserInfo.ClientCode));
                        sqlCmd.Parameters.Add(new SqlParameter("@Name", objCustomerUserInfo.Name));

                        sqlCmd.Parameters.Add(new SqlParameter("@Address", objCustomerUserInfo.Address));

                        sqlCmd.Parameters.Add(new SqlParameter("@EmailAddress", objCustomerUserInfo.EmailAddress));
                        sqlCmd.Parameters.Add(new SqlParameter("@ContactNumber1", objCustomerUserInfo.ContactNumber1));
                        sqlCmd.Parameters.Add(new SqlParameter("@ContactNumber2", objCustomerUserInfo.ContactNumber2));
                        sqlCmd.Parameters.Add(new SqlParameter("@BankAccountNumber", objCustomerUserInfo.BankAccountNumber));
                        sqlCmd.Parameters.Add(new SqlParameter("@BankBinNo", objCustomerUserInfo.BankNo));
                        sqlCmd.Parameters.Add(new SqlParameter("@BranchCode", objCustomerUserInfo.BranchCode));

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





            //Database database;
            //int ret;

            //try
            //{
            //    database = DatabaseConnection.GetDatabase();
            //    using (var command = database.GetStoredProcCommand("[s_MNAdminInfoUpdate]"))
            //    {
            //        database.AddInParameter(command, "@ClientCode", DbType.String, objCustomerUserInfo.ClientCode);
            //        //database.AddInParameter(command, "@Address", DbType.String, objCustomerUserInfo.Address);
            //        //database.AddInParameter(command, "@Status", DbType.String, objCustomerUserInfo.Status);
            //        database.AddInParameter(command, "@ProfileName", DbType.String, objCustomerUserInfo.ProfileName);
            //        database.AddInParameter(command, "@Name", DbType.String, objCustomerUserInfo.Name);
            //        database.AddInParameter(command, "@EmailAddress", DbType.String, objCustomerUserInfo.EmailAddress);
            //        database.AddInParameter(command, "@BranchCode", DbType.String, objCustomerUserInfo.BranchCode);
            //        database.AddInParameter(command, "@COC", DbType.String, objCustomerUserInfo.COC);
            //        database.AddInParameter(command, "@ReturnValue", DbType.Int32, objCustomerUserInfo.ReturnValue);


            //        //database.AddInParameter(command, "@ContactNumber1", DbType.String, objCustomerUserInfo.ContactNumber1);
            //        //database.AddInParameter(command, "@ContactNumber2", DbType.String, objCustomerUserInfo.ContactNumber2);
            //        //database.AddInParameter(command, "@WalletNumber", DbType.String, objCustomerUserInfo.WalletNumber);

            //        //database.AddInParameter(command, "@BINBankCode", DbType.String, objCustomerUserInfo.BankNo);
            //        //database.AddInParameter(command, "@BankBranchCode", DbType.String, objCustomerUserInfo.BranchCode);
            //        //database.AddInParameter(command, "@BankAccountNumber", DbType.String, objCustomerUserInfo.BankAccountNumber);
            //        //database.AddInParameter(command, "@PIN", DbType.String, objCustomerUserInfo.PIN);

            //        //database.AddInParameter(command, "@IsApproved", DbType.String, objCustomerUserInfo.IsApproved);
            //        //database.AddInParameter(command, "@IsRejected", DbType.String, objCustomerUserInfo.IsRejected);

            //        database.AddInParameter(command, "@mode", DbType.String, objCustomerUserInfo.Mode);
            //        //database.AddOutParameter(command, "@RegIDOut", DbType.Int32, objCustomerUserInfo.RegIDOut);
            //        ret = database.ExecuteNonQuery(command);
            //        if (objCustomerUserInfo.Mode.Equals("UAINFO", StringComparison.InvariantCultureIgnoreCase))
            //        {
            //            ret = (int)database.GetParameterValue(command, "RegIDOut");
            //        }
            //    }

            //}
            //catch (Exception ex)
            //{
            //    throw new Exception(ex.Message);
            //}
            //finally
            //{
            //    database = null;
            //    Database.ClearParameterCache();
            //}

            //    return ret;
        }
        public int UpdateAdminInfo(UserInfo objCustomerUserInfo)
        {

            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNAdminInfoUpdate]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", objCustomerUserInfo.ClientCode));
                        sqlCmd.Parameters.Add(new SqlParameter("@Name", objCustomerUserInfo.Name));
                        sqlCmd.Parameters.Add(new SqlParameter("@ProfileCode", objCustomerUserInfo.ProfileCode));
                        sqlCmd.Parameters.Add(new SqlParameter("@EmailAddress", objCustomerUserInfo.EmailAddress));
                        sqlCmd.Parameters.Add(new SqlParameter("@BranchCode", objCustomerUserInfo.BranchCode));
                        sqlCmd.Parameters.Add(new SqlParameter("@COC", objCustomerUserInfo.COC));
                        sqlCmd.Parameters.Add(new SqlParameter("@Status", objCustomerUserInfo.Status));
                        sqlCmd.Parameters.Add(new SqlParameter("@mode", objCustomerUserInfo.Mode));
                        sqlCmd.Parameters.Add(new SqlParameter("@ModifyingBranch", objCustomerUserInfo.AdminBranch));
                        sqlCmd.Parameters.Add(new SqlParameter("@ModifiedBy", objCustomerUserInfo.AdminUserName));
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





            //Database database;
            //int ret;

            //try
            //{
            //    database = DatabaseConnection.GetDatabase();
            //    using (var command = database.GetStoredProcCommand("[s_MNAdminInfoUpdate]"))
            //    {
            //        database.AddInParameter(command, "@ClientCode", DbType.String, objCustomerUserInfo.ClientCode);
            //        //database.AddInParameter(command, "@Address", DbType.String, objCustomerUserInfo.Address);
            //        //database.AddInParameter(command, "@Status", DbType.String, objCustomerUserInfo.Status);
            //        database.AddInParameter(command, "@ProfileName", DbType.String, objCustomerUserInfo.ProfileName);
            //        database.AddInParameter(command, "@Name", DbType.String, objCustomerUserInfo.Name);
            //        database.AddInParameter(command, "@EmailAddress", DbType.String, objCustomerUserInfo.EmailAddress);
            //        database.AddInParameter(command, "@BranchCode", DbType.String, objCustomerUserInfo.BranchCode);
            //        database.AddInParameter(command, "@COC", DbType.String, objCustomerUserInfo.COC);
            //        database.AddInParameter(command, "@ReturnValue", DbType.Int32, objCustomerUserInfo.ReturnValue);


            //        //database.AddInParameter(command, "@ContactNumber1", DbType.String, objCustomerUserInfo.ContactNumber1);
            //        //database.AddInParameter(command, "@ContactNumber2", DbType.String, objCustomerUserInfo.ContactNumber2);
            //        //database.AddInParameter(command, "@WalletNumber", DbType.String, objCustomerUserInfo.WalletNumber);

            //        //database.AddInParameter(command, "@BINBankCode", DbType.String, objCustomerUserInfo.BankNo);
            //        //database.AddInParameter(command, "@BankBranchCode", DbType.String, objCustomerUserInfo.BranchCode);
            //        //database.AddInParameter(command, "@BankAccountNumber", DbType.String, objCustomerUserInfo.BankAccountNumber);
            //        //database.AddInParameter(command, "@PIN", DbType.String, objCustomerUserInfo.PIN);

            //        //database.AddInParameter(command, "@IsApproved", DbType.String, objCustomerUserInfo.IsApproved);
            //        //database.AddInParameter(command, "@IsRejected", DbType.String, objCustomerUserInfo.IsRejected);

            //        database.AddInParameter(command, "@mode", DbType.String, objCustomerUserInfo.Mode);
            //        //database.AddOutParameter(command, "@RegIDOut", DbType.Int32, objCustomerUserInfo.RegIDOut);
            //        ret = database.ExecuteNonQuery(command);
            //        if (objCustomerUserInfo.Mode.Equals("UAINFO", StringComparison.InvariantCultureIgnoreCase))
            //        {
            //            ret = (int)database.GetParameterValue(command, "RegIDOut");
            //        }
            //    }

            //}
            //catch (Exception ex)
            //{
            //    throw new Exception(ex.Message);
            //}
            //finally
            //{
            //    database = null;
            //    Database.ClearParameterCache();
            //}

            //    return ret;
        }
        public int UpdateMerchantInfo(UserInfo objCustomerUserInfo)
        {

            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNMerchantInfoUpdate]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", objCustomerUserInfo.ClientCode));
                        sqlCmd.Parameters.Add(new SqlParameter("@Name", objCustomerUserInfo.Name));
                        sqlCmd.Parameters.Add(new SqlParameter("@Status", objCustomerUserInfo.Status));
                        sqlCmd.Parameters.Add(new SqlParameter("@IsApproved", objCustomerUserInfo.IsApproved));
                        sqlCmd.Parameters.Add(new SqlParameter("@Address", objCustomerUserInfo.Address));
                        sqlCmd.Parameters.Add(new SqlParameter("@EmailAddress", objCustomerUserInfo.EmailAddress));
                        sqlCmd.Parameters.Add(new SqlParameter("@ContactNumber1", objCustomerUserInfo.ContactNumber1));
                        sqlCmd.Parameters.Add(new SqlParameter("@ContactNumber2", objCustomerUserInfo.ContactNumber2));
                        sqlCmd.Parameters.Add(new SqlParameter("@BankAccountNumber", objCustomerUserInfo.BankAccountNumber));
                        sqlCmd.Parameters.Add(new SqlParameter("@BankCode", objCustomerUserInfo.BankNo));
                        sqlCmd.Parameters.Add(new SqlParameter("@AcBranchCode", objCustomerUserInfo.BranchCode));

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





            //Database database;
            //int ret;

            //try
            //{
            //    database = DatabaseConnection.GetDatabase();
            //    using (var command = database.GetStoredProcCommand("[s_MNAdminInfoUpdate]"))
            //    {
            //        database.AddInParameter(command, "@ClientCode", DbType.String, objCustomerUserInfo.ClientCode);
            //        //database.AddInParameter(command, "@Address", DbType.String, objCustomerUserInfo.Address);
            //        //database.AddInParameter(command, "@Status", DbType.String, objCustomerUserInfo.Status);
            //        database.AddInParameter(command, "@ProfileName", DbType.String, objCustomerUserInfo.ProfileName);
            //        database.AddInParameter(command, "@Name", DbType.String, objCustomerUserInfo.Name);
            //        database.AddInParameter(command, "@EmailAddress", DbType.String, objCustomerUserInfo.EmailAddress);
            //        database.AddInParameter(command, "@BranchCode", DbType.String, objCustomerUserInfo.BranchCode);
            //        database.AddInParameter(command, "@COC", DbType.String, objCustomerUserInfo.COC);
            //        database.AddInParameter(command, "@ReturnValue", DbType.Int32, objCustomerUserInfo.ReturnValue);


            //        //database.AddInParameter(command, "@ContactNumber1", DbType.String, objCustomerUserInfo.ContactNumber1);
            //        //database.AddInParameter(command, "@ContactNumber2", DbType.String, objCustomerUserInfo.ContactNumber2);
            //        //database.AddInParameter(command, "@WalletNumber", DbType.String, objCustomerUserInfo.WalletNumber);

            //        //database.AddInParameter(command, "@BINBankCode", DbType.String, objCustomerUserInfo.BankNo);
            //        //database.AddInParameter(command, "@BankBranchCode", DbType.String, objCustomerUserInfo.BranchCode);
            //        //database.AddInParameter(command, "@BankAccountNumber", DbType.String, objCustomerUserInfo.BankAccountNumber);
            //        //database.AddInParameter(command, "@PIN", DbType.String, objCustomerUserInfo.PIN);

            //        //database.AddInParameter(command, "@IsApproved", DbType.String, objCustomerUserInfo.IsApproved);
            //        //database.AddInParameter(command, "@IsRejected", DbType.String, objCustomerUserInfo.IsRejected);

            //        database.AddInParameter(command, "@mode", DbType.String, objCustomerUserInfo.Mode);
            //        //database.AddOutParameter(command, "@RegIDOut", DbType.Int32, objCustomerUserInfo.RegIDOut);
            //        ret = database.ExecuteNonQuery(command);
            //        if (objCustomerUserInfo.Mode.Equals("UAINFO", StringComparison.InvariantCultureIgnoreCase))
            //        {
            //            ret = (int)database.GetParameterValue(command, "RegIDOut");
            //        }
            //    }

            //}
            //catch (Exception ex)
            //{
            //    throw new Exception(ex.Message);
            //}
            //finally
            //{
            //    database = null;
            //    Database.ClearParameterCache();
            //}

            //    return ret;
        }
        public DataTable GetAgentIdModel(UserInfo objUserInfo)
        {
            Database database;
            DataTable dtableResult = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                
                using (var command = database.GetStoredProcCommand("[s_MNUserInfoSearch]"))
                {
                    database.AddInParameter(command, "@MobileNo", DbType.String, objUserInfo.ContactNumber1);
                    database.AddInParameter(command, "@Name", DbType.String, objUserInfo.Name);
                    database.AddInParameter(command, "@WalletNumber", DbType.String, objUserInfo.WalletNumber);
                    database.AddInParameter(command, "@ClientCode", DbType.String, objUserInfo.ClientCode);
                    database.AddInParameter(command, "@mode", DbType.String, objUserInfo.Mode);
                    using (var dataset = new DataSet())
                    {
                        database.LoadDataSet(command, dataset, "dtAgentInfo");

                        if (dataset.Tables.Count > 0)
                        {
                            dtableResult = dataset.Tables["dtAgentInfo"];
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
                database = null;
                Database.ClearParameterCache();
                
            }

            return dtableResult;
        }
       
        public bool InsertSMSLog(SMSLog log)
        {
            SqlConnection conn = null;
            SqlTransaction strans = null;
            bool result = false;
            try
            {
                using (conn = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {


                        SqlCommand cmd = new SqlCommand();
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = @"INSERT INTO MNSMSPinLog(UserName,Message,SentOn,SentBy,Purpose)
                                           Values (@UserName,@Message,@SentOn,@SentBy,@Purpose)";
                        cmd.Parameters.AddWithValue("@UserName",log.UserName);
                        cmd.Parameters.AddWithValue("@Message", log.Message);
                        cmd.Parameters.AddWithValue("@SentOn", log.SentOn);
                        cmd.Parameters.AddWithValue("@SentBy", log.SentBy);
                        cmd.Parameters.AddWithValue("@Purpose", log.Purpose);

                        if (conn.State != ConnectionState.Open)
                            conn.Open();

                        cmd.Connection = conn;
                        strans = conn.BeginTransaction();
                        cmd.Transaction = strans;
                        int i = cmd.ExecuteNonQuery();
                        if (i==1)
                        {
                            strans.Commit();
                            result = true;
                            
                        }
                        else
                        {
                            result = false;
                        }

                      cmd.Dispose();
                   

                }
            }
            catch(Exception ex)
            {
                strans.Rollback();
                result= false;
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
              
            }
            return result;
        }



        public List<UserInfo> GetRejectedUser(string UserName)
        {
            List<UserInfo> UserInfos = new List<UserInfo>();
            SqlConnection conn = null;
            string Command = string.Empty;
            SqlDataReader rdr;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        Command = @"Select * from v_MNClientDetail where Status ='Expired' ";

                        if (!string.IsNullOrEmpty(UserName))
                        {
                            Command = Command+" AND UserName=@UserName";
                            cmd.Parameters.AddWithValue("UserName", UserName);
                        }
                        cmd.CommandText = Command;
                        cmd.Connection = conn;
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            UserInfo Info = new UserInfo();
                            Info.UserName = rdr["UserName"].ToString();
                            Info.ClientCode = rdr["ClientCode"].ToString();
                            Info.Name = rdr["Name"].ToString();
                            Info.CreatedDate = rdr["CreatedDate"].ToString();
                            Info.ProfileName = rdr["ProfileCode"].ToString();
                            Info.BankAccountNumber = rdr["BankAccountNumber"].ToString();
                            Info.Status = rdr["Status"].ToString();

                            UserInfos.Add(Info);
                        }
                        if (conn.State != ConnectionState.Closed)
                        {
                            conn.Close();
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


            }
            return UserInfos;
        }



        /// <summary>
        /// Inserts into MNMakerChecker,InMemMNTransactionAccount for checking 
        /// </summary>
        /// <param name="ClientCode">Customer Client Code</param>
        /// <param name="ModifyingAdmin">Modifying Admin</param>
        /// <param name="ModifyingBranch"> Modifying Admin Branch Code</param>
        /// <param name="ModifiedField">XML String of ModifiedFields</param>
        /// <param name="AccToDelete">XML String of transaction accounts to delete</param>
        /// <param name="AccToAdd">XML String of transaction accounts to add</param>
        /// <returns>-1 if insert fails,99 if no error occurs but data is not inserted,100 Successful</returns>
        public int InsertIntoMakerChecker(string ClientCode,string ModifyingAdmin,string ModifyingBranch, string ModifiedField,string AccToDelete,string AccToAdd)
        {

            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNClientMakerChecker]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", ClientCode));
                        sqlCmd.Parameters.Add(new SqlParameter("@ModifyingUser", ModifyingAdmin));
                        sqlCmd.Parameters.Add(new SqlParameter("@ModifyingBranch", ModifyingBranch));
                        sqlCmd.Parameters.Add(new SqlParameter("@ModifiedFields", ModifiedField));
                        sqlCmd.Parameters.Add(new SqlParameter("@TxnAccountsToDelete", AccToDelete));
                        sqlCmd.Parameters.Add(new SqlParameter("@TxnAccountsToAdd", AccToAdd));
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



        public int InsertIntoMakerCheckerAdmin(string ClientCode, string ModifyingAdmin, string ModifyingBranch, string ModifiedField)
        {

            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNAdminMakerChecker]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", ClientCode));
                        sqlCmd.Parameters.Add(new SqlParameter("@ModifyingUser", ModifyingAdmin));
                        sqlCmd.Parameters.Add(new SqlParameter("@ModifyingBranch", ModifyingBranch));
                        sqlCmd.Parameters.Add(new SqlParameter("@ModifiedFields", ModifiedField));
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











        #region ADMIN APPROVE REJECTED LIST

        public int AdminApprove(UserInfo objUserInfo, string Rejected, string Approve)
        {
            SqlConnection conn = null;
            int ret;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = @"UPDATE MNClient SET IsRejected=@IsRejected,IsApproved=@IsApproved,ModifiedBy=@ModifiedBy,ModifyingBranch=@ModifyingBranch
                                               Where ClientCode=@ClientCode ";

                        
                        cmd.Parameters.AddWithValue("@ClientCode", objUserInfo.ClientCode);
                        cmd.Parameters.AddWithValue("@IsRejected", Rejected);
                        cmd.Parameters.AddWithValue("@IsApproved", Approve);
                        cmd.Parameters.AddWithValue("@ModifiedBy", objUserInfo.AdminUserName);
                        cmd.Parameters.AddWithValue("@ModifyingBranch", objUserInfo.AdminBranch);
                        cmd.Connection = conn;
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        ret = cmd.ExecuteNonQuery();
                        if (conn.State != ConnectionState.Closed)
                        {
                            conn.Close();
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


            }

            return ret;
        }






        public int AdminReject(UserInfo objUserInfo, string Rejected, string Approve)
        {
            SqlConnection conn = null;
            int ret;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = @"UPDATE MNClient SET IsRejected=@IsRejected,IsApproved=@IsApproved,Remarks=@Remarks,RejectedBy=@RejectedBy
                                               Where ClientCode=@ClientCode ";


                        cmd.Parameters.AddWithValue("@ClientCode", objUserInfo.ClientCode);
                        cmd.Parameters.AddWithValue("@IsRejected", Rejected);
                        cmd.Parameters.AddWithValue("@IsApproved", Approve);
                        cmd.Parameters.AddWithValue("@Remarks", objUserInfo.Remarks);
                        cmd.Parameters.AddWithValue("@RejectedBy", objUserInfo.AdminUserName);
                        cmd.Connection = conn;
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        ret = cmd.ExecuteNonQuery();
                        if (conn.State != ConnectionState.Closed)
                        {
                            conn.Close();
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


            }

            return ret;
        }

        #endregion
        public List<UserInfo> GetUserStatusList(string BranchCode, bool COC, string MobileNo)
        {
            List<UserInfo> UserInfos = new List<UserInfo>();
            SqlConnection conn = null;
            string Command = string.Empty;
            SqlDataReader rdr;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        Command = @"Select * from v_MNClientDetail where Status in (@Status1,@Status2) AND IsApproved=@IsApproved";
                        if (!COC)
                        {
                            Command = Command + " AND ModifyingBranch='" + BranchCode + "'";
                        }
                        if (!string.IsNullOrEmpty(MobileNo))
                        {
                            Command = Command + " AND UserName='" + MobileNo + "'";
                        }
                        cmd.CommandText = Command;
                        cmd.Parameters.AddWithValue("@Status1", "Blocked"); //PIN Reset
                        cmd.Parameters.AddWithValue("@Status2", "Active"); //Pass Reset
                        cmd.Parameters.AddWithValue("@IsApproved", "Blocked");
                        cmd.Connection = conn;
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            UserInfo Info = new UserInfo();
                            Info.UserName = rdr["UserName"].ToString();
                            Info.ClientCode = rdr["ClientCode"].ToString();
                            Info.Name = rdr["Name"].ToString();
                            Info.ModifyingBranch = rdr["ModifyingBranch"].ToString();
                            Info.ModifyingAdmin = rdr["ModifiedBy"].ToString();
                            Info.BankAccountNumber = rdr["BankAccountNumber"].ToString();
                            Info.Status = rdr["Status"].ToString();

                            UserInfos.Add(Info);
                        }
                        if (conn.State != ConnectionState.Closed)
                        {
                            conn.Close();
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


            }
            return UserInfos;
        }


        public int StatusApprove(string ClientCode)
        {
            SqlConnection conn = null;
            int ret=0;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = @"UPDATE MNClient SET IsApproved=@IsApproved,ModifiedBy='',ModifyingBranch=''
                                               Where ClientCode=@ClientCode ";
                        cmd.Parameters.AddWithValue("@ClientCode", ClientCode);
                        cmd.Parameters.AddWithValue("@IsApproved", "Approve");
                        cmd.Connection = conn;
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        ret = cmd.ExecuteNonQuery();
                        if (conn.State != ConnectionState.Closed)
                        {
                            conn.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }
                ret = 0;
            }
            finally
            {

            }
            return ret;
        }

        #region Admin Registration Approve/Rejected
        // AdminRegApprove
        public int AdminRegApprove(UserInfo objUserInfo, string Approve)
        {
            SqlConnection conn = null;
            int ret;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = @"UPDATE MNClient SET Status = 'Active', IsApproved = 'Approve' where ClientCode = @ClientCode";


                        cmd.Parameters.AddWithValue("@ClientCode", objUserInfo.ClientCode);
                        //cmd.Parameters.AddWithValue("@IsRejected", Rejected);
                        //cmd.Parameters.AddWithValue("@IsApproved", Approve);
                        //cmd.Parameters.AddWithValue("@ModifiedBy", objUserInfo.AdminUserName);
                        //cmd.Parameters.AddWithValue("@ModifyingBranch", objUserInfo.AdminBranch);
                        cmd.Connection = conn;
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        ret = cmd.ExecuteNonQuery();
                        if (conn.State != ConnectionState.Closed)
                        {
                            conn.Close();
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


            }

            return ret;
        }


        public int AdminRegReject(UserInfo objUserInfo, string Rejected)
        {
            SqlConnection conn = null;
            int ret;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = @"UPDATE MNClient SET IsRejected=@IsRejected,Remarks=@Remarks,RejectedBy=@RejectedBy
                                               Where ClientCode=@ClientCode ";


                        cmd.Parameters.AddWithValue("@ClientCode", objUserInfo.ClientCode);
                        cmd.Parameters.AddWithValue("@IsRejected", Rejected);
                        //cmd.Parameters.AddWithValue("@IsApproved", Approve);
                        cmd.Parameters.AddWithValue("@Remarks", objUserInfo.Remarks);
                        cmd.Parameters.AddWithValue("@RejectedBy", objUserInfo.AdminUserName);
                        cmd.Connection = conn;
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        ret = cmd.ExecuteNonQuery();
                        if (conn.State != ConnectionState.Closed)
                        {
                            conn.Close();
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


            }

            return ret;
        }
        #endregion


        #region Approve and edit info of Admin from Rejected List
        public int AprvRjAdmin(UserInfo objCustomerUserInfo)
        {

            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNAdminInfoUpdate]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", objCustomerUserInfo.ClientCode));
                        sqlCmd.Parameters.Add(new SqlParameter("@Name", objCustomerUserInfo.Name));
                        sqlCmd.Parameters.Add(new SqlParameter("@ProfileCode", objCustomerUserInfo.ProfileCode));
                        sqlCmd.Parameters.Add(new SqlParameter("@EmailAddress", objCustomerUserInfo.EmailAddress));
                        sqlCmd.Parameters.Add(new SqlParameter("@BranchCode", objCustomerUserInfo.BranchCode));
                        sqlCmd.Parameters.Add(new SqlParameter("@COC", objCustomerUserInfo.COC));
                        sqlCmd.Parameters.Add(new SqlParameter("@Status", objCustomerUserInfo.Status));
                        sqlCmd.Parameters.Add(new SqlParameter("@mode", objCustomerUserInfo.Mode));
                        sqlCmd.Parameters.Add(new SqlParameter("@ModifyingBranch", objCustomerUserInfo.AdminBranch));
                        sqlCmd.Parameters.Add(new SqlParameter("@ModifiedBy", objCustomerUserInfo.AdminUserName));
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

        public int ApproveModifiedAdmin(UserInfo objCustomerUserInfo)
        {

            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNApproveAdmin]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", objCustomerUserInfo.ClientCode));
                        sqlCmd.Parameters.Add(new SqlParameter("@ApprovingBranch", objCustomerUserInfo.AdminBranch));
                        sqlCmd.Parameters.Add(new SqlParameter("@ApprovingUser", objCustomerUserInfo.AdminUserName));
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




        public int RejectModifiedAdmin(UserInfo objCustomerUserInfo)
        {

            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNRejectAdmin]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", objCustomerUserInfo.ClientCode));
                        sqlCmd.Parameters.Add(new SqlParameter("@RejectingBranch", objCustomerUserInfo.AdminBranch));
                        sqlCmd.Parameters.Add(new SqlParameter("@RejectingUser", objCustomerUserInfo.AdminUserName));
                        sqlCmd.Parameters.Add(new SqlParameter("@Remarks", objCustomerUserInfo.Remarks));
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



        //public Customer GetCustById(string Id)
        //{
        //    Database database;
        //    DataSet dtset = new DataSet();
        //    MNClient MNClient = new MNClient();
        //    MNClientContact MNClientContact = new MNClientContact();
        //    MNClientExt MNClientExt = new MNClientExt();
        //    MNBankAccountMap MNBankAccountMap = new MNBankAccountMap();
        //    List<TransactionInfo> MNTransactionAccounts = new List<TransactionInfo>();
        //    Customer Cust = new Customer
        //    {
        //        MNClient = MNClient,
        //        MNClientContact = MNClientContact,
        //        MNClientExt = MNClientExt,
        //        MNBankAccountMap = MNBankAccountMap,
        //        MNTransactionAccounts = MNTransactionAccounts
        //    };
        //    try
        //    {
        //        database = DatabaseConnection.GetDatabase();
        //        using (var command = database.GetStoredProcCommand("[s_MNGetClientByCode]"))
        //        {
        //            database.AddInParameter(command, "@ClientCode", DbType.String, Id);
        //            string[] table = new string[] { "MNClient", "MNClientExt", "MNClientContact", "MNBankAccountMap", "MNTransactionAccount" };
        //            using (var dataset = new DataSet())
        //            {
        //                database.LoadDataSet(command, dataset, table);
        //                dtset = dataset;
        //            }

        //        }
        //        Cust.MNClient = ExtraUtility.DatatableToSingleClass<MNClient>(dtset.Tables["MNClient"]);
        //        Cust.MNClientExt = ExtraUtility.DatatableToSingleClass<MNClientExt>(dtset.Tables["MNClientExt"]);
        //        Cust.MNClientContact = ExtraUtility.DatatableToSingleClass<MNClientContact>(dtset.Tables["MNClientContact"]);
        //        Cust.MNBankAccountMap = ExtraUtility.DatatableToSingleClass<MNBankAccountMap>(dtset.Tables["MNBankAccountMap"]);
        //        Cust.MNTransactionAccounts = ExtraUtility.DatatableToListClass<TransactionInfo>(dtset.Tables["MNTransactionAccount"]);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //    finally
        //    {
        //        database = null;
        //        //  Database.ClearParameterCache();
        //    }

        //    return Cust;
        //}

        //test//
        //Customer Rejected List search//
        //By Mobile No//

        public DataTable GetCusRejUnInformation(UserInfo objUserInfo)
        {
            Database database;
            DataTable dtableResult = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNUserInfoSearch]"))
                {
                    database.AddInParameter(command, "@MobileNo", DbType.String, objUserInfo.ContactNumber1);
                    database.AddInParameter(command, "@Name", DbType.String, objUserInfo.Name);
                    database.AddInParameter(command, "@WalletNumber", DbType.String, objUserInfo.WalletNumber);
                    database.AddInParameter(command, "@ClientCode", DbType.String, objUserInfo.ClientCode);
                    database.AddInParameter(command, "@userType", DbType.String, "admin");
                    database.AddInParameter(command, "@mode", DbType.String, objUserInfo.Mode);
                    using (var dataset = new DataSet())
                    {
                        database.LoadDataSet(command, dataset, "dtCustomerInfo");

                        if (dataset.Tables.Count > 0)
                        {
                            dtableResult = dataset.Tables["dtCustomerInfo"];
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
                database = null;
                Database.ClearParameterCache();
            }

            return dtableResult;
        }

    }
}