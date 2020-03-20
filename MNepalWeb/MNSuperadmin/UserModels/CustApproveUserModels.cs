using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Microsoft.Practices.EnterpriseLibrary.Data;
using MNSuperadmin.Connection;
using MNSuperadmin .Models;

namespace MNSuperadmin.UserModels
{
    public class CustApproveUserModels
    {
        internal DataTable GetCustApproveDetailInfo(UserInfo objUserInfo)//CustApproveInfo
        {
            Database database;
            DataTable dtableResult = null;
            /*
              SELECT ClientCode, ContactNumber1 AS MobileNumber, Name, Address,*
			  FROM v_MNClientDetail 
			  WHERE UserType = 'user' AND IsApproved = 'UnApprove' AND ISNULL(IsModified,'F')='F' AND IsRejected='F'
            */
            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNCustUserInfoApprove]")) //
                {
                    database.AddInParameter(command, "@UserType", DbType.String, objUserInfo.UserType);
                    database.AddInParameter(command, "@mode", DbType.String, objUserInfo.Mode);
                    using (var dataset = new DataSet())
                    {
                        database.LoadDataSet(command, dataset, "dtCustomerInfoApprove");

                        if (dataset.Tables.Count > 0)
                        {
                            dtableResult = dataset.Tables["dtCustomerInfoApprove"];
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
        internal int CustInfoApproveOld1(UserInfo objUserInfo)//CustInfoApprove
        {
            Database database;
            int ret = 0;

            try
            {
                database = DatabaseConnection.GetDatabase();

                using (var command = database.GetStoredProcCommand("[s_MNCustomerInfoApprove]"))
                {
                    database.AddInParameter(command, "@ClientCode", DbType.String, objUserInfo.ClientCode);
                    database.AddInParameter(command, "@ApprovedBy", DbType.String, objUserInfo.IsApproved);
                    database.AddInParameter(command, "@IsApproved", DbType.String, objUserInfo.IsApproved);
                    database.AddInParameter(command, "@mode", DbType.String, objUserInfo.Mode);
                    database.AddInParameter(command, "@ReturnValue", DbType.Int32, ret);

                    ret = database.ExecuteNonQuery(command);
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
        internal int CustInfoApprove(UserInfo objUserInfo)//Single CustInfoApprove
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNCustomerInfoApprove]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", objUserInfo.ClientCode));
                        sqlCmd.Parameters.Add(new SqlParameter("@ApprovedBy", objUserInfo.ApprovedBy));
                        sqlCmd.Parameters.Add(new SqlParameter("@IsApproved", objUserInfo.IsApproved));
                        sqlCmd.Parameters.Add(new SqlParameter("@mode", objUserInfo.Mode));
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

        internal int CustInfoApproveWalletCustStatus(UserInfo objUserInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    //using (SqlCommand sqlCmd = new SqlCommand("[s_MNCustomerInfoApprove]", sqlCon))
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNCustUserInfoApprove]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", objUserInfo.ClientCode));
                        sqlCmd.Parameters.Add(new SqlParameter("@ApprovedBy", objUserInfo.ApprovedBy));
                        sqlCmd.Parameters.Add(new SqlParameter("@IsApproved", objUserInfo.IsApproved));
                        sqlCmd.Parameters.Add(new SqlParameter("@mode", objUserInfo.Mode));
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


        internal int CustInfoReject(UserInfo objUserInfo)//Single CustInfoApprove
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNRejectCustomerInfo]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", objUserInfo.ClientCode));
                        sqlCmd.Parameters.Add(new SqlParameter("@RejectedBy", objUserInfo.UserName));
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
                if (sqlCon != null)
                {
                    sqlCon.Close();
                }
            }
            return ret;
        }

      

        internal int CustInfoApproveSelected(List<UserInfo> objUserInfoList, string mode)// Multiple CustInfoApprove
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    // Generate list of ClientCodes 
                    List<string> cusIDList = new List<string>();
                    foreach (var cus in objUserInfoList)
                    {
                        cusIDList.Add(cus.ClientCode);
                    }

                    int cusCount = objUserInfoList.Count;
                    string strCusId = "'" + string.Join("','", cusIDList) + "'";

                    string approveBy = objUserInfoList[0].ApprovedBy;
                    string isApprove = objUserInfoList[0].IsApproved;

                    sqlCon.Open();

                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNCustomerInfoApproveSel]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@ClientIdList", strCusId));
                        sqlCmd.Parameters.Add(new SqlParameter("@ApprovedBy", approveBy));
                        sqlCmd.Parameters.Add(new SqlParameter("@IsApproved", isApprove));
                        sqlCmd.Parameters.Add(new SqlParameter("@mode", mode));
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
        internal DataTable GetCustModified(UserInfo objUserInfo)//CustApproveInfo
        {
            Database database;
            DataTable dtableResult = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNCustUserInfoApprove]"))
                {
                    database.AddInParameter(command, "@UserType", DbType.String, objUserInfo.UserType);
                    database.AddInParameter(command, "@mode", DbType.String, objUserInfo.Mode);
                    using (var dataset = new DataSet())
                    {
                        database.LoadDataSet(command, dataset, "dtCustomerInfoApprove");

                        if (dataset.Tables.Count > 0)
                        {
                            dtableResult = dataset.Tables["dtCustomerInfoApprove"];
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
        internal int ApproveCustModified(UserInfo objUserInfo)//Single CustInfoApprove
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNApproveCust]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", objUserInfo.ClientCode));
                        sqlCmd.Parameters.Add(new SqlParameter("@ApprovingUser", objUserInfo.AdminUserName));
                        sqlCmd.Parameters.Add(new SqlParameter("@ApprovingBranch", objUserInfo.AdminBranch));
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
        internal int CustRenew(string ClientCode, string UserName, string Amount, string AdminUserName, string RRNumber)//Single CustInfoApprove
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNClientRenew]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", ClientCode));
                        sqlCmd.Parameters.Add(new SqlParameter("@UserName", UserName));
                        sqlCmd.Parameters.Add(new SqlParameter("@ChargeAmount", Amount));
                        sqlCmd.Parameters.Add(new SqlParameter("@AdminUserName", AdminUserName));
                        sqlCmd.Parameters.Add(new SqlParameter("@ReferenceNumber", RRNumber));
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
        internal int ChangeCustStatus(string ClientCode, string Status)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("Update MNClient Set Status=@Status where ClientCode=@ClientCode", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.Text;

                        sqlCmd.Parameters.Add(new SqlParameter("@Status", Status));
                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", ClientCode));
                     
                        sqlCmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.Output;

                        sqlCmd.ExecuteNonQuery();
                        ret = sqlCmd.ExecuteNonQuery();
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
        internal int RejectCustModified(UserInfo objUserInfo)//Single CustInfoApprove
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNRejectCust]", sqlCon))
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
                if (sqlCon != null)
                {
                    sqlCon.Close();
                }
            }
            return ret;
        }
        internal List<UserInfo> GetAdminApproveDetailInfo(UserInfo objUserInfo, string UserName)
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
                  string Command = @"SELECT * FROM dbo.v_MNClientDetail WHERE UserType = 'superadmin' AND IsApproved = 'UnApprove' AND Status = 'InActive' AND ISNULL(IsRejected,'F')='F'";


                        if (!string.IsNullOrEmpty(UserName))
                        {
                            //Command = Command + " AND UserName='" +UserName+ "'";
                            Command = Command + " AND UPPER(UserName) like UPPER('" + UserName+ "%')";
                             
                             
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
                            Info.UserBranchName= rdr["UserBranchName"].ToString();
                            Info.UserBranchCode = rdr["UserBranchCode"].ToString();
                            Info.EmailAddress = rdr["EmailAddress"].ToString();
                            Info.ProfileName= rdr["ProfileName"].ToString();
                            Info.AProfileName = rdr["AProfileName"].ToString();
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

        #region new KYC Verificat6ion
        internal DataTable GetCustApproveKYCDetailInfo(UserInfo objUserInfo)//CustApproveInfo
        {
            Database database;
            DataTable dtableResult = null;
            /*
              SELECT ClientCode, ContactNumber1 AS MobileNumber, Name, Address,*
			  FROM v_MNClientDetail 
			  WHERE UserType = 'user' AND IsApproved = 'UnApprove' AND ISNULL(IsModified,'F')='F' AND IsRejected='F'
            */
            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNCustUserInfoKYCApprove]")) //
                {
                    database.AddInParameter(command, "@UserType", DbType.String, objUserInfo.UserType);
                    database.AddInParameter(command, "@mode", DbType.String, objUserInfo.Mode);
                    using (var dataset = new DataSet())
                    {
                        database.LoadDataSet(command, dataset, "dtCustomerInfoApprove");

                        if (dataset.Tables.Count > 0)
                        {
                            dtableResult = dataset.Tables["dtCustomerInfoApprove"];
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
        #region KYC approve or reject
        internal int CustInfoKYCApprove(UserInfo objUserInfo)//Single CustInfoApprove
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNCustomerInfoApprove]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", objUserInfo.ClientCode));
                        sqlCmd.Parameters.Add(new SqlParameter("@ApprovedBy", objUserInfo.ApprovedBy));
                        sqlCmd.Parameters.Add(new SqlParameter("@IsApproved", objUserInfo.IsApproved));
                        sqlCmd.Parameters.Add(new SqlParameter("@mode", objUserInfo.Mode));
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
        internal int CustInfoKYCReject(UserInfo objUserInfo)//Single CustInfoApprove
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNRejectCustomerInfo]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", objUserInfo.ClientCode));
                        sqlCmd.Parameters.Add(new SqlParameter("@RejectedBy", objUserInfo.UserName));
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