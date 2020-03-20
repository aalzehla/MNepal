using Microsoft.Practices.EnterpriseLibrary.Data;
using MNSuperadmin.Connection;
using MNSuperadmin.Helper;
using MNSuperadmin.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;


namespace MNSuperadmin.UserModels
{
    public class PasswordUserModel
    {

        
        /// <summary>
        /// Retrieve the user information based on mode
        /// </summary>
        /// <param name="objPasswordInfo">Pass an instance of User information</param>
        /// <returns>Returns the password information based on user information</returns>
        public int UpdateUserPasswordInfo(UserInfo objPasswordInfo)
        {
            Database database;
            int ret;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNChangePassword]"))
                {
					database.AddInParameter(command, "@OPassword", DbType.String, HashAlgo.Hash(objPasswordInfo.OPassword));
                    database.AddInParameter(command, "@Password", DbType.String, HashAlgo.Hash(objPasswordInfo.Password));
                    database.AddInParameter(command, "@ClientCode", DbType.String, objPasswordInfo.ClientCode);
                    database.AddInParameter(command, "@mode", DbType.String, objPasswordInfo.Mode);
                    database.AddOutParameter(command, "@RegIDOut", DbType.Int32, objPasswordInfo.RegIDOut);
                    ret = database.ExecuteNonQuery(command);
                    if (objPasswordInfo.Mode.Equals("CPWD", StringComparison.InvariantCultureIgnoreCase))
                    {
                        ret = (int)database.GetParameterValue(command, "RegIDOut");
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

            return ret;
        }

        /// <summary>
        /// Retrieve the user information based on mode
        /// </summary>
        /// <param name="objPasswordInfo">Pass an instance of User information</param>
        /// <returns>Returns the password information based on user information</returns>
        public long ForgetUserPasswordInfo(UserInfo objPasswordInfo)
        {
            long ret;
            SqlConnection conn = null;
            SqlTransaction sTrans = null;

            try
            {
            
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    
                    conn.Open();
                    sTrans = conn.BeginTransaction();
                    using (SqlCommand cmd = new SqlCommand("[s_MNForgetPassword]", conn,sTrans))
                    {
                        cmd.Parameters.AddWithValue("@UserName", objPasswordInfo.UserName);
                        cmd.Parameters.AddWithValue("@Password", HashAlgo.Hash(objPasswordInfo.Password));
                        cmd.Parameters.AddWithValue("@mode", objPasswordInfo.Mode);
                        cmd.Parameters.Add("@RegIDOut", SqlDbType.BigInt);
                        cmd.Parameters["@RegIDOut"].Direction = ParameterDirection.Output;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.ExecuteNonQuery();
                        // read output value from @NewId
                        ret = Convert.ToInt64(cmd.Parameters["@RegIDOut"].Value);
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

        /// <summary>
        /// Retrieve the admin information based on mode
        /// </summary>
        /// <param name="objPasswordInfo">Pass an instance of Admin information</param>
        /// <returns>Returns the password information based on Admin information</returns>
        public int UpdateAdminPasswordInfo(UserInfo objPasswordInfo)
        {
            Database database;
            int ret;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNChangePassword]"))
                {
                    database.AddInParameter(command, "@OPassword", DbType.String, objPasswordInfo.UserName);
                    database.AddInParameter(command, "@Password", DbType.String, HashAlgo.Hash(objPasswordInfo.Password));
                    database.AddInParameter(command, "@ClientCode", DbType.String, objPasswordInfo.ClientCode);
                    database.AddInParameter(command, "@mode", DbType.String, objPasswordInfo.Mode);
                    database.AddOutParameter(command, "@RegIDOut", DbType.Int32, objPasswordInfo.RegIDOut);
                    ret = database.ExecuteNonQuery(command);
                    ret = (int)database.GetParameterValue(command, "RegIDOut");
                    
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

            return ret;
        }


        public int ResetPassword(UserInfo objPasswordInfo)
        {
            Database database;
            int ret;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNChangePassword]"))
                {
                    database.AddInParameter(command, "@OPassword", DbType.String, "");
                    database.AddInParameter(command, "@Password", DbType.String, HashAlgo.Hash(objPasswordInfo.Password));
                    database.AddInParameter(command, "@ClientCode", DbType.String, objPasswordInfo.ClientCode);
                    database.AddInParameter(command, "@mode", DbType.String, objPasswordInfo.Mode);
                    database.AddOutParameter(command, "@RegIDOut", DbType.Int32, objPasswordInfo.RegIDOut);
                    ret = database.ExecuteNonQuery(command);
                    if (objPasswordInfo.Mode.Equals("NPWD", StringComparison.InvariantCultureIgnoreCase))
                    {
                        ret = (int)database.GetParameterValue(command, "RegIDOut");
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

            return ret;
        }
    }
}