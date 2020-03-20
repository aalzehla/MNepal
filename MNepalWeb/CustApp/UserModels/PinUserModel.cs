using Microsoft.Practices.EnterpriseLibrary.Data;
using CustApp.Connection;
using CustApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace CustApp.UserModels
{
    public class PinUserModel
    {
        /// <summary>
        /// Retrieve the user information based on mode
        /// </summary>
        /// <param name="objPasswordInfo">Pass an instance of User information</param>
        /// <returns>Returns the pin information based on user information</returns>
        public int UpdateUserPinInfo(UserInfo objPasswordInfo)
        {
            Database database;
            int ret;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNChangePassword]"))
                {
					database.AddInParameter(command, "@OPassword", DbType.String, objPasswordInfo.OPIN);
                    database.AddInParameter(command, "@Password", DbType.String, objPasswordInfo.PIN);
                    database.AddInParameter(command, "@ClientCode", DbType.String, objPasswordInfo.ClientCode);
                    database.AddInParameter(command, "@mode", DbType.String, objPasswordInfo.Mode);
                    database.AddOutParameter(command, "@RegIDOut", DbType.Int32, objPasswordInfo.RegIDOut);
                    ret = database.ExecuteNonQuery(command);
                    if (objPasswordInfo.Mode.Equals("CPIN", StringComparison.InvariantCultureIgnoreCase))
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





        public int UpdateUserPin(UserInfo model,string status)
        {
            int ret;
            SqlConnection conn = null;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = @"UPDATE MNClient SET Status=@Status,IsApproved=@IsApproved,ModifiedBy=@ModifiedBy,ModifyingBranch=@ModifyingBranch
                                               Where ClientCode=@ClientCode";
                        cmd.Parameters.AddWithValue("@ClientCode", model.ClientCode);
                        cmd.Parameters.AddWithValue("@Status", status); //PIN Reset
                        cmd.Parameters.AddWithValue("@IsApproved", "UnApprove");
                        cmd.Parameters.AddWithValue("@ModifiedBy", model.AdminUserName);
                        cmd.Parameters.AddWithValue("@ModifyingBranch", model.AdminBranch);
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
        

        public List<UserInfo> GetPinApproveList(string BranchCode,bool COC,string MobileNo)
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
                        Command = @"Select * from v_MNClientDetail where Status in (@Status1,@Status2,@Status3) AND IsApproved=@IsApproved";
                        if (!COC)
                        {
                            Command = Command + " AND ModifyingBranch='"+BranchCode+"'";
                        }
                        if(!string.IsNullOrEmpty(MobileNo))
                        {
                            Command = Command + " AND UserName='" + MobileNo + "'";
                        }
                        cmd.CommandText = Command;
                        cmd.Parameters.AddWithValue("@Status1", "PINR"); //PIN Reset
                        cmd.Parameters.AddWithValue("@Status2", "PASR"); //Pass Reset
                        cmd.Parameters.AddWithValue("@Status3", "PPR"); //BOTH Reset
                        cmd.Parameters.AddWithValue("@IsApproved", "UnApprove");
                        cmd.Connection = conn;
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        rdr = cmd.ExecuteReader();
                        while(rdr.Read())
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


        public int ApproveUserPin(UserInfo model,string Mode)
        {
            Database database;
            int ret;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNPinResetApp]"))
                {
                    database.AddInParameter(command, "@ClientCode", DbType.String, model.ClientCode);
                    database.AddInParameter(command, "@PIN", DbType.String,model.PIN);
                    database.AddInParameter(command, "@Password", DbType.String, model.Password);
                    database.AddInParameter(command, "@Mode", DbType.String, Mode);
                    database.AddOutParameter(command, "@RegIDOut", DbType.Int32, model.RegIDOut);
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

        public int RevertPasswordReset(string  ClientCode)
        {
            int ret;
            SqlConnection conn = null;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = @"UPDATE MNClient SET Status='Active',IsApproved='Approve',ModifiedBy='',ModifyingBranch=''
                                               Where ClientCode=@ClientCode";
                        cmd.Parameters.AddWithValue("@ClientCode", ClientCode);
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





        #region Admin Password Reset Approval

        //Admin Password Reset by Maker

        public int UpdatePasswordReset(UserInfo model, string status)
        {
            int ret;
            SqlConnection conn = null;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = @"UPDATE MNClient SET Status='APR',IsApproved=@IsApproved,ModifiedBy=@ModifiedBy,ModifyingBranch=@ModifyingBranch
                                               Where ClientCode=@ClientCode";
                        cmd.Parameters.AddWithValue("@ClientCode", model.ClientCode);
                        cmd.Parameters.AddWithValue("@Status", status); //PIN Reset
                        cmd.Parameters.AddWithValue("@IsApproved", "UnApprove");
                        cmd.Parameters.AddWithValue("@ModifiedBy", model.AdminUserName);
                        cmd.Parameters.AddWithValue("@ModifyingBranch", model.AdminBranch);
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










        public List<UserInfo> GetPasswordAAL(string UserName)
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
                        Command = @"Select * from v_MNClientDetail where Status = 'APR' AND IsApproved=@IsApproved";
                       
                        if (!string.IsNullOrEmpty(UserName))
                        {
                            Command = Command + " AND UserName='" + UserName + "'";
                        }
                        cmd.CommandText = Command;
                        cmd.Parameters.AddWithValue("@Status", "APR");
                        cmd.Parameters.AddWithValue("@IsApproved", "UnApprove");
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
                            Info.EmailAddress = rdr["EmailAddress"].ToString();
                            Info.UserBranchCode = rdr["UserBranchCode"].ToString();
                            Info.UserBranchName = rdr["UserBranchName"].ToString();
                            Info.ProfileName = rdr["ProfileName"].ToString();
                            Info.AProfileName = rdr["AProfileName"].ToString();

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

    }
}

