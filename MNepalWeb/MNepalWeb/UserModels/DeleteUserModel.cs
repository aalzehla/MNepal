using Microsoft.Practices.EnterpriseLibrary.Data;
using MNepalWeb.Connection;
using MNepalWeb.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace MNepalWeb.UserModels
{
    public class DeleteUserModel
    {
        /// <summary>
        /// Delete the user information based on mode
        /// </summary>
        /// <param name="objDeleteUserInfo">Pass an instance of User information</param>
        /// <returns>Returns the pin information based on user information</returns>
        public int DeleteUserStatusInfo(UserInfo objDeleteUserInfo)
        {
            Database database;
            int ret;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNChangeStatus]"))
                {
                    database.AddInParameter(command, "@Status", DbType.String, objDeleteUserInfo.Status);
                    database.AddInParameter(command, "@ClientCode", DbType.String, objDeleteUserInfo.ClientCode);
                    database.AddInParameter(command, "@mode", DbType.String, objDeleteUserInfo.Mode);
                    database.AddOutParameter(command, "@RegIDOut", DbType.Int32, objDeleteUserInfo.RegIDOut);
                    ret = database.ExecuteNonQuery(command);
                    if (objDeleteUserInfo.Mode.Equals("DUSST", StringComparison.InvariantCultureIgnoreCase))
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



        public int ChangeUserStatus(string ClientCode,string Status,string AdminBranchCode,string AdminUserName,string BlockRemarks)
        {
            SqlConnection sqlCon = null;
            int ret=0;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand())
                    {
                        //string command = "Update MNClient set BlockStatus=@Status,IsApproved='Blocked',ModifyingBranch=@ModifyingBranch,ModifiedBy=@ModifiedBy ,BlockRemarks=@BlockRemarks where ClientCode=@ClientCode";
                        string command = "Update MNClient set BlockStatus=@Status,ModifyingBranch=@ModifyingBranch,ModifiedBy=@ModifiedBy ,BlockRemarks=@BlockRemarks where ClientCode=@ClientCode";
                        sqlCmd.CommandText = command;
                        sqlCmd.Parameters.AddWithValue("@Status", Status);
                        sqlCmd.Parameters.AddWithValue("@ModifyingBranch", AdminBranchCode);
                        sqlCmd.Parameters.AddWithValue("@ModifiedBy", AdminUserName);
                        sqlCmd.Parameters.AddWithValue("@ClientCode", ClientCode);
                        sqlCmd.Parameters.AddWithValue("@BlockRemarks", BlockRemarks);
                        sqlCmd.Connection = sqlCon;
                        if (sqlCon.State != ConnectionState.Open)
                            sqlCon.Open();
                        ret = sqlCmd.ExecuteNonQuery();
                        if (sqlCon.State != ConnectionState.Closed)
                            sqlCon.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                if (sqlCon.State != ConnectionState.Closed)
                    sqlCon.Close();
                throw ex;
            }
            finally
            {
               
            }
            return ret;
        }
    }
}
