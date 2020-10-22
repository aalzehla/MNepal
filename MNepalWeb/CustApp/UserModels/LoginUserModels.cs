using Microsoft.Practices.EnterpriseLibrary.Data;
using CustApp.Connection;
using CustApp.InterfaceServices;
using CustApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using CustApp.Helper;

namespace CustApp.UserModels
{
    public class LoginUserModels : IUserDbService
    {
        /// <summary>
        /// Retrieve the user information based on mode
        /// </summary>
        /// <param name="objUserInfo">Pass an instance of User information</param>
        /// <returns>Returns the  user information based on user information</returns>
        public DataTable GetUserInformation(UserInfo objUserInfo)
        {
            Database database;
            DataTable dtableResult = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNLogin]"))
                {
                    database.AddInParameter(command, "@UserName", DbType.String, objUserInfo.UserName);
                    database.AddInParameter(command, "@Password", DbType.String, objUserInfo.Password);
                    database.AddInParameter(command, "@ClientCode", DbType.String, objUserInfo.ClientCode);
                    database.AddInParameter(command, "@mode", DbType.String, objUserInfo.Mode);
                    using (var dataset = new DataSet())
                    {
                        database.LoadDataSet(command, dataset, "dtUserInfo");

                        if (dataset.Tables.Count > 0)
                        {
                            dtableResult = dataset.Tables["dtUserInfo"];
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
        #region"Block Message"
        public DataTable GetBlockRemarks(UserInfo objUserInfo)
        {
            Database database;
            DataTable dtableResult = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNLogin]"))
                {
                    database.AddInParameter(command, "@UserName", DbType.String, objUserInfo.UserName);
                    database.AddInParameter(command, "@Password", DbType.String, objUserInfo.Password);
                    database.AddInParameter(command, "@ClientCode", DbType.String, objUserInfo.ClientCode);
                    database.AddInParameter(command, "@mode", DbType.String, objUserInfo.Mode);
                    using (var dataset = new DataSet())
                    {
                        database.LoadDataSet(command, dataset, "dtUserInfo");

                        if (dataset.Tables.Count > 0)
                        {
                            dtableResult = dataset.Tables["dtUserInfo"];
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
        #endregion

        public DataSet GetUserInformationDSet(UserInfo objUserInfo)
        {
            Database database;
            DataSet dtset = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNLogin]"))
                {
                    database.AddInParameter(command, "@UserName", DbType.String, objUserInfo.UserName);
                    database.AddInParameter(command, "@Password", DbType.String, objUserInfo.Password);
                    database.AddInParameter(command, "@ClientCode", DbType.String, objUserInfo.ClientCode);
                    database.AddInParameter(command, "@mode", DbType.String, objUserInfo.Mode);
                    string[] tables = new string[] { "dtUserInfo", "dtTransaction" };
                    using (var dataset = new DataSet())
                    {
                        database.LoadDataSet(command, dataset, tables);

                        if (dataset.Tables.Count > 0)
                        {
                            dtset = dataset;
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

            return dtset;
        }



        public DataSet GetCusDetailProfileDSet(UserInfo objUserInfo)
        {
            Database database;
            DataSet dtset = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNLogin]"))
                {
                    database.AddInParameter(command, "@UserName", DbType.String, objUserInfo.UserName);
                    database.AddInParameter(command, "@Password", DbType.String, objUserInfo.Password);
                    database.AddInParameter(command, "@ClientCode", DbType.String, objUserInfo.ClientCode);
                    database.AddInParameter(command, "@mode", DbType.String, objUserInfo.Mode);
                    string[] tables = new string[] { "dtUserInfo", "dtKycDetail", "dtKycDoc" };
                    using (var dataset = new DataSet())
                    {
                        database.LoadDataSet(command, dataset, tables);

                        if (dataset.Tables.Count > 0)
                        {
                            dtset = dataset;
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

            return dtset;
        }

        public DataSet GetCustModifiedValue(UserInfo objUserInfo)
        {
            Database database;
            DataSet dtset = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNGetCustModifiedValue]"))
                {

                    database.AddInParameter(command, "@ClientCode", DbType.String, objUserInfo.ClientCode);
                    string[] tables = new string[] { "MNMakerChecker", "InMemMNTransactionAccount" };
                    using (var dataset = new DataSet())
                    {
                        database.LoadDataSet(command, dataset, tables);

                        if (dataset.Tables.Count > 0)
                        {
                            dtset = dataset;
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

            return dtset;
        }




        public DataSet GetAdminModifiedValue(UserInfo objUserInfo)
        {
            Database database;
            DataSet dtset = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNGetAdminModifiedValue]"))
                {

                    database.AddInParameter(command, "@ClientCode", DbType.String, objUserInfo.ClientCode);
                    string[] tables = new string[] { "MNMakerChecker" };
                    using (var dataset = new DataSet())
                    {
                        database.LoadDataSet(command, dataset, tables);
                        if (dataset.Tables.Count > 0)
                        {
                            dtset = dataset;
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

            return dtset;
        }



        /// <summary>
        /// Retrieve the user information based on mode
        /// </summary>
        /// <param name="objUserInfo">Pass an instance of User information</param>
        /// <returns>Returns the  user information based on user information</returns>
        public DataTable GetAllUserInformation(UserInfo objUserInfo)
        {
            Database database;
            DataTable dtableResult = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNUserInfo]"))
                {
                    database.AddInParameter(command, "@userType", DbType.String, "admin");
                    database.AddInParameter(command, "@mode", DbType.String, objUserInfo.Mode);
                    using (var dataset = new DataSet())
                    {
                        database.LoadDataSet(command, dataset, "dtUserInfo");

                        if (dataset.Tables.Count > 0)
                        {
                            dtableResult = dataset.Tables["dtUserInfo"];
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

        public int ResetFirstPassword(ViewModel.ResetVM model)
        {
            int ret;
            SqlConnection conn = null;
            SqlTransaction sTrans = null;

            try
            {

                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    sTrans = conn.BeginTransaction();
                    using (SqlCommand cmd = new SqlCommand("[s_MNFPasswordReset]", conn, sTrans))
                    {
                        cmd.Parameters.AddWithValue("@ClientCode", model.ClientCode);
                        cmd.Parameters.AddWithValue("@NewPin", HashAlgo.Hash(model.Pin));
                        cmd.Parameters.AddWithValue("@NewPassword", HashAlgo.Hash(model.Password));
                        cmd.Parameters.AddWithValue("@Mode", model.Mode);
                        cmd.Parameters.Add("@RetVal", SqlDbType.Int);
                        cmd.Parameters["@RetVal"].Direction = ParameterDirection.Output;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.ExecuteNonQuery();
                        // read output value from @NewId
                        ret = Convert.ToInt32(cmd.Parameters["@RetVal"].Value);
                        sTrans.Commit();
                        conn.Close();

                    }
                }

            }
            catch (Exception ex)
            {
                sTrans.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }

            return ret;



        }

        public Dictionary<string, string> Checkcredential(string ClientCode)
        {
            int ret;
            SqlConnection conn = null;
            SqlTransaction sTrans = null;
            SqlDataReader rdr = null;
            Dictionary<string, string> test = new Dictionary<string, string>();
            try
            {

                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    string Query = "Select PIN,Password FROM MNClientExt (NOLOCK) where ClientCode=@ClientCode";
                    using (SqlCommand cmd = new SqlCommand(Query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ClientCode", ClientCode);
                        cmd.CommandType = CommandType.Text;
                        rdr = cmd.ExecuteReader();
                        rdr.Read();
                        test.Add("PIN", rdr["PIN"].ToString());
                        test.Add("Password", rdr["Password"].ToString());
                        conn.Close();
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
                    conn.Close();
            }
            return test;
        }


        public void LogAction(MNAdminLog adminLog)
        {
            SqlConnection conn = null;
            try
            {
                using (conn = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {

                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "s_CheckAdminLOG";
                    cmd.Parameters.AddWithValue("@UserName", adminLog.UserId);
                    cmd.Parameters.AddWithValue("@UserAlias", adminLog.UserType);
                    cmd.Parameters.AddWithValue("@URL", adminLog.URL);
                    cmd.Parameters.AddWithValue("@Station", adminLog.IPAddress);
                    cmd.Parameters.AddWithValue("@Message", adminLog.Message);
                    cmd.Parameters.AddWithValue("@Branch", adminLog.Branch);
                    cmd.Parameters.AddWithValue("@Action", adminLog.Action);
                    cmd.Parameters.AddWithValue("@UniqueId", adminLog.UniqueId);
                    cmd.Parameters.AddWithValue("@PrivateIP", adminLog.PrivateIP);
                    cmd.Parameters.AddWithValue("@ClientDetails", adminLog.ClientDetails);
                    if (conn.State != ConnectionState.Open)
                        conn.Open();
                    cmd.Connection = conn;
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
        }


        #region Rejected List for Admin

        public List<UserInfo> GetAllAdminInformation(UserInfo objUserInfo, string isModified)
        {
            List<UserInfo> UserInfos = new List<UserInfo>();
            SqlConnection conn = null;
            SqlDataReader rdr;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        string Command = @"SELECT * FROM v_MNClientDetail (NOLOCK) 
                                            WHERE UserType = 'admin' AND IsApproved = 'UnApprove' AND IsRejected = 'T'
		                                  AND ISNULL(IsModified,'F')=@IsModified";


                        cmd.Parameters.AddWithValue("@IsModified", isModified);
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
                            Info.UserBranchName = rdr["UserBranchName"].ToString();
                            Info.UserBranchCode = rdr["UserBranchCode"].ToString();
                            Info.EmailAddress = rdr["EmailAddress"].ToString();

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



        #endregion

        #region Get Self Register Customer Detail
        public DataSet GetSelfRegInfoDSet(CustomerSRInfo objSrInfo)
        {
            Database database;
            DataSet dtset = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[MNRejectKYCDetail]"))
                {

                    database.AddInParameter(command, "@ClientCode", DbType.String, objSrInfo.ClientCode);

                    string[] tables = new string[] { "dtSrInfo" };
                    using (var dataset = new DataSet())
                    {
                        database.LoadDataSet(command, dataset, tables);

                        if (dataset.Tables.Count > 0)
                        {
                            dtset = dataset;
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

            return dtset;
        }
        #endregion

        #region Check KYC
        public DataTable CheckKYC(UserInfo objUserInfo)
        {
            DataTable dtableResult = null;
            SqlConnection conn = null;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNCheckKYC]", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserName", objUserInfo.UserName);
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 60;  // seconds
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset);
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables[0];
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
        #endregion

        #region Check Bank Link
        public DataTable CheckLinkBankAcc(UserInfo objUserInfo)
        {
            DataTable dtableResult = null;
            SqlConnection conn = null;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNCheckLinkBankAcc]", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserName", objUserInfo.UserName);
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 60;  // seconds
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset);
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables[0];
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
        #endregion

        #region GetCustomerName
        public DataTable GetCustomerName(UserInfo objUserInfo)
        {
            DataTable dtableResult = null;
            SqlConnection conn = null;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNGetCustomerName]", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserName", objUserInfo.UserName);
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 60;  // seconds
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset);
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables[0];
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
        #endregion

        //start ebabking clientcode

        #region to get clientcode from given mobile no
        public DataTable EBankingClientCode(CustomerSRInfo objUserInfo)
        {
            DataTable dtableResult = null;
            SqlConnection conn = null;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNEBankingClientCode]", conn))
                    {
                        cmd.Parameters.AddWithValue("@Mobile", objUserInfo.Mobile);
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 60;  // seconds
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset);
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables[0];
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
        #endregion

        #region Check IsFirstLogin
        public DataTable IsFirstLogin(UserInfo objUserInfo)
        {
            DataTable dtableResult = null;
            SqlConnection conn = null;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNCheckIsFirstLogin]", conn))
                    {
                        cmd.Parameters.AddWithValue("@ClientCode", objUserInfo.ClientCode);
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 60;  // seconds
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset);
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables[0];
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
        #endregion

        #region"block user for wrong pwd attemp" 
        public DataTable BlockUserWrongPwd(UserInfo objUserInfo)
        {
            Database database;
            DataTable dtableResult = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_BlockUserWrongPwd]"))
                {
                    database.AddInParameter(command, "@UserName", DbType.String, objUserInfo.UserName);

                    database.AddInParameter(command, "@mode", DbType.String, objUserInfo.Mode);
                    using (var dataset = new DataSet())
                    {
                        database.LoadDataSet(command, dataset, "dtableBlockUserWrongPwd");

                        if (dataset.Tables.Count > 0)
                        {
                            dtableResult = dataset.Tables["dtableBlockUserWrongPwd"];
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

        #endregion

        #region GET MESSAGE 
        public string GetMessage(string MsgID)
        {
            SqlConnection conn = null;
            string ret = "";
            DataTable dtableResult = null;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNGetMessage]", conn))
                    {
                        cmd.Parameters.AddWithValue("@MsgID", MsgID);

                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset, "dtUserInfo");
                                if (dataset.Tables.Count > 0)
                                {
                                    DataTable TpData = dataset.Tables[0];
                                    if (TpData.Rows.Count > 0)
                                    {
                                        foreach (DataRow dr in TpData.Rows)
                                        {
                                            ret = dr["MsgCodeDesc"].ToString();
                                        }
                                    }
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
                conn.Close();
            }

            return ret;
        }
        #endregion
    }


}