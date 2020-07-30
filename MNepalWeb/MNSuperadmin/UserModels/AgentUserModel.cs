using Microsoft.Practices.EnterpriseLibrary.Data;
using MNSuperadmin.Connection;
using MNSuperadmin.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace MNSuperadmin.UserModels
{
    public class AgentUserModel
    {
        /*List of UnApproved Registered Agent */
        internal List<UserInfo> GetAgentRegApprove(UserInfo objUserInfo, string UserName)
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
                        string Command = @"SELECT * FROM dbo.v_MNClientDetail WHERE UserType = 'agent' AND IsApproved = 'UnApprove' AND Status = 'InActive' AND ISNULL(IsRejected,'F')='F'";


                        if (!string.IsNullOrEmpty(UserName))
                        {
                            Command = Command + " AND UPPER(UserName) = UPPER('" + UserName + "')";
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
                            Info.Address = rdr["Address"].ToString();
                            Info.EmailAddress = rdr["EmailAddress"].ToString();
                            Info.Status = rdr["Status"].ToString();
                            //Info.UserBranchName = rdr["UserBranchName"].ToString();
                            //Info.UserBranchCode = rdr["UserBranchCode"].ToString();                            
                            //Info.ProfileName = rdr["ProfileName"].ToString();
                            //Info.AProfileName = rdr["AProfileName"].ToString();
                            Info.CreatedBy = rdr["CreatedBy"].ToString();
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


        #region
        /*List of Modification Rejected Agent */
        internal List<UserInfo> GetAgentRegReject(UserInfo objUserInfo, string UserName)
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
                        string Command = @"SELECT * FROM dbo.v_MNClientDetail WHERE UserType = 'agent' AND IsApproved = 'UnApprove' AND Status = 'InActive' AND IsModified='F' AND IsRejected = 'T'";


                        if (!string.IsNullOrEmpty(UserName))
                        {
                            Command = Command + " AND UPPER(UserName) = UPPER('" + UserName + "')";
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
                            Info.Address = rdr["Address"].ToString();
                            Info.EmailAddress = rdr["EmailAddress"].ToString();
                            Info.Status = rdr["Status"].ToString();
                            //Info.UserBranchName = rdr["UserBranchName"].ToString();
                            //Info.UserBranchCode = rdr["UserBranchCode"].ToString();                            
                            //Info.ProfileName = rdr["ProfileName"].ToString();
                            //Info.AProfileName = rdr["AProfileName"].ToString();
                            Info.CreatedBy = rdr["CreatedBy"].ToString();
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

        #region
        /*List of Modification Rejected Agent */
        internal List<UserInfo> GetAgentModReject(UserInfo objUserInfo, string UserName)
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
                        string Command = @"SELECT * FROM dbo.v_MNClientDetail WHERE UserType = 'agent' AND IsApproved = 'Approve' AND Status = 'Active' AND IsModified='T' AND IsRejected = 'T'";


                        if (!string.IsNullOrEmpty(UserName))
                        {
                            Command = Command + " AND UPPER(UserName) = UPPER('" + UserName + "')";
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
                            Info.Address = rdr["Address"].ToString();
                            Info.EmailAddress = rdr["EmailAddress"].ToString();
                            Info.Status = rdr["Status"].ToString();
                            //Info.UserBranchName = rdr["UserBranchName"].ToString();
                            //Info.UserBranchCode = rdr["UserBranchCode"].ToString();                            
                            //Info.ProfileName = rdr["ProfileName"].ToString();
                            //Info.AProfileName = rdr["AProfileName"].ToString();
                            Info.CreatedBy = rdr["CreatedBy"].ToString();
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

        /*Agent Registration Approval*/
        public int AgentRegApprove(UserInfo objUserInfo, string Approve)
        {
            SqlConnection conn = null;
            int ret;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = @"UPDATE MNClient SET Status = 'Active', HasKYC = 'T', IsRejected = 'F', IsApproved = 'Approve', ApprovedBy = @ApprovedBy where ClientCode = @ClientCode";


                        cmd.Parameters.AddWithValue("@ClientCode", objUserInfo.ClientCode);
                        //cmd.Parameters.AddWithValue("@IsRejected", Rejected);
                        //cmd.Parameters.AddWithValue("@IsApproved", Approve);
                        cmd.Parameters.AddWithValue("@ApprovedBy", objUserInfo.ApprovedBy);
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

        public int MerchantRegApprove(UserInfo objUserInfo, string Approve)
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

        /*Agent Registration Reject*/
        public int AgentRegReject(UserInfo objUserInfo, string Reject)
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
                        cmd.Parameters.AddWithValue("@IsRejected", Reject);
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


        /*List of UnApproved Modification Agent */
        internal List<UserInfo> GetAgentModApprove(UserInfo objUserInfo, string UserName)
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
                        string Command = @"SELECT * FROM dbo.v_MNClientDetail WHERE UserType = 'agent' AND IsApproved = 'Approve' AND IsModified = 'T' AND Status = 'Active' AND ISNULL(IsRejected,'F')='F'";


                        if (!string.IsNullOrEmpty(UserName))
                        {
                            Command = Command + " AND UserName='" + UserName + "'";
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
                            Info.Address = rdr["Address"].ToString();
                            Info.EmailAddress = rdr["EmailAddress"].ToString();
                            Info.Status = rdr["Status"].ToString();
                            //Info.UserBranchName = rdr["UserBranchName"].ToString();
                            //Info.UserBranchCode = rdr["UserBranchCode"].ToString();                            
                            //Info.ProfileName = rdr["ProfileName"].ToString();
                            //Info.AProfileName = rdr["AProfileName"].ToString();
                            Info.CreatedBy = rdr["CreatedBy"].ToString();
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
        internal List<UserInfo> GetAgentCommissionApprove(UserInfo objUserInfo, string UserName)
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
                        string Command = @"SELECT * FROM dbo.MNFeeDetails where  IsApproved = 'UnApprove' AND IsModified = 'T' AND(  IsRejected='F' OR IsRejected IS Null)";


                        if (!string.IsNullOrEmpty(UserName))
                        {
                            Command = Command + " AND UserName='" + UserName + "'";
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
                            Info.CommissionId = rdr["Id"].ToString();
                            Info.FeeID = rdr["FeeId"].ToString();
                            Info.TieredStart = rdr["TieredStart"].ToString();
                            Info.TieredEnd = rdr["TieredEnd"].ToString();
                            Info.MinAmt = rdr["MinAmt"].ToString();
                            Info.MaxAmt = rdr["MaxAmt"].ToString();
                            Info.Percentage = rdr["Percentage"].ToString();
                            Info.FlatFee = rdr["FlatFee"].ToString();
                            
                          
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

        /*Agent Modification Approval*/
        internal int AgentModApprove(UserInfo objUserInfo)
        {
            SqlConnection sqlCon = null;
            //SqlConnection conn = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNApproveAgent]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.AddWithValue("@ClientCode", objUserInfo.ClientCode);
                        sqlCmd.Parameters.AddWithValue("@ApprovedBy", objUserInfo.ApprovedBy);
                        sqlCmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.Output;
                        sqlCmd.ExecuteNonQuery();
                        ret = Convert.ToInt32(sqlCmd.Parameters["@ReturnValue"].Value);
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

            return ret;
        }


        #region Agent Makerchecker--- NISCHAL
        public int InsertIntoMakerCheckerAgent(string ClientCode, string ModifyingAdmin, string ModifyingBranch, string ModifiedField)
        {

            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNAgentMakerChecker]", sqlCon))
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

        #endregion


        #region
        /*Agent Modification Approval*/
        public int AgentModReject(UserInfo objUserInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNRejectAgent]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", objUserInfo.ClientCode));
                        sqlCmd.Parameters.Add(new SqlParameter("@RejectingUser", objUserInfo.AdminUserName));
                        sqlCmd.Parameters.Add(new SqlParameter("@RejectingBranch", objUserInfo.AdminBranch));
                        sqlCmd.Parameters.Add(new SqlParameter("@Remarks", objUserInfo.Remarks));
                        sqlCmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.Output;
                        sqlCmd.ExecuteNonQuery();
                        ret = Convert.ToInt32(sqlCmd.Parameters["@ReturnValue"].Value);
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



        public int AgentRegisterReject(UserInfo objUserInfo, string Rejected)
        {
            SqlConnection conn = null;
            int ret;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = @"UPDATE MNClient SET Status = 'InActive', IsApproved = 'UnApprove', IsModified = 'F', IsRejected='T', RejectedBy= @RejectedBy  where ClientCode = @ClientCode";


                        cmd.Parameters.AddWithValue("@ClientCode", objUserInfo.ClientCode);
                        cmd.Parameters.AddWithValue("@RejectedBy", objUserInfo.RejectedBy);
                        //cmd.Parameters.AddWithValue("@ApprovedDate", objUserInfo.ApprovedDate);
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



        public DataSet GetAgentModifiedValue(UserInfo objUserInfo)
        {
            Database database;
            DataSet dtset = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNGetAgentModifiedValue]"))
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

        #endregion

        #region Agent Status Changed List
        public List<UserInfo> GetAgentStatusList(string MobileNo)
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
                        //  Command = @"Select * from v_MNClientDetail where Status in (@Status1,@Status2) AND IsApproved=@IsApproved AND UserType = 'agent'";
                        Command = @"Select * from v_MNClientDetail where BlockStatus in (@Status1,@Status2) AND UserType = 'agent'";
                       

                        if (!string.IsNullOrEmpty(MobileNo))
                        {
                            Command = Command + " AND UserName='" + MobileNo + "'";
                        }
                        cmd.CommandText = Command;
                        cmd.Parameters.AddWithValue("@Status1", "Blocked");
                        cmd.Parameters.AddWithValue("@Status2", "Active");
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
                            Info.EmailAddress = rdr["EmailAddress"].ToString();
                            Info.ModifyingAdmin = rdr["ModifiedBy"].ToString();
                            //Info.Status = rdr["BlockStatus"].ToString();
                            Info.Status = rdr["Status"].ToString();
                            Info.BlockStatus = rdr["BlockStatus"].ToString();
                            Info.BlockRemarks = rdr["BlockRemarks"].ToString();


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

        #region Approve Agent Status Changed List

        public int StatusApprove(string ClientCode)
        {
            SqlConnection conn = null;
            int ret = 0;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = @"UPDATE MNClient SET IsApproved=@IsApproved,ModifiedBy=''
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


        #endregion


        public int StatusReject(string ClientCode, string Status)
        {
            SqlConnection conn = null;
            int ret = 0;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = @"UPDATE MNClient SET IsApproved=@IsApproved, ModifiedBy='', ModifyingBranch='',BlockStatus='',Status=@Status WHERE ClientCode=@ClientCode ";
                        cmd.Parameters.AddWithValue("@ClientCode", ClientCode);
                        cmd.Parameters.AddWithValue("@Status", Status);
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

        #region AgentCommissionApprove
        public int RejectModifiedAgentCommission(UserInfo objCustomerUserInfo)
        {

            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNRejectAgentCommission]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", objCustomerUserInfo.CommissionId));
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

        public int ApproveModifiedAgentCommission(UserInfo objCustomerUserInfo)
        {

            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNApproveAgentCommission]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", objCustomerUserInfo.CommissionId));
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
        #endregion


        #region Rejected List for AgentCommission

        public List<UserInfo> GetRejectedAgentCommissionList(UserInfo objUserInfo, string isModified)
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
                        string Command = @"SELECT * FROM MNFeeDetails WHERE IsApproved = 'UnApprove' AND IsRejected = 'T'
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
                            Info.CommissionId = rdr["Id"].ToString();
                            Info.FeeID = rdr["FeeId"].ToString();
                            Info.TieredStart = rdr["TieredStart"].ToString();
                            Info.TieredEnd = rdr["TieredEnd"].ToString();
                            Info.MinAmt = rdr["MinAmt"].ToString();
                            Info.MaxAmt = rdr["MaxAmt"].ToString();
                            Info.Percentage = rdr["Percentage"].ToString();
                            Info.FlatFee = rdr["FlatFee"].ToString();
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

        #region MakerChaker for AgentCommission
        public int InsertIntoMakerCheckerAgentCommission(string ClientCode, string ModifyingAdmin, string ModifyingBranch, string ModifiedField)
        {

            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNAgentCommissionChecker]", sqlCon))
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
        #endregion

    }
}