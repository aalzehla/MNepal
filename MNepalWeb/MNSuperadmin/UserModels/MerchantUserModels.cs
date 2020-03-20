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
    public class MerchantUserModels
    {
        /// <summary>
        /// Retrieve the merchant detail information based on mode
        /// </summary>
        /// <param name="objMerchantInfo">Pass an instance of User information</param>
        /// <returns>Returns the merchant detail information based on merchant information</returns>
        #region
        public DataTable GetMerchantDetailInformation(MNMerchants objMerchantInfo)
        {
            Database database;
            DataTable dtableResult = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNMerchantDetails]"))
                {
                    database.AddInParameter(command, "@mode", DbType.String, objMerchantInfo.Mode);
                    using (var dataset = new DataSet())
                    {
                        database.LoadDataSet(command, dataset, "dtMerchantInfo");

                        if (dataset.Tables.Count > 0)
                        {
                            dtableResult = dataset.Tables["dtMerchantInfo"];
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

        #region
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
                        cmd.CommandText = @"UPDATE MNClient SET Status = 'Active', IsApproved = 'Approve', HasKYC = 'T' where ClientCode = @ClientCode";


                        cmd.Parameters.AddWithValue("@ClientCode", objUserInfo.ClientCode);                        
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

        #region
        internal int RejectMerchantModified(UserInfo objUserInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNRejectMerchant]", sqlCon))
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
        #endregion

        #region
        public DataSet GetMerchantModifiedValue(UserInfo objUserInfo)
        {
            Database database;
            DataSet dtset = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNGetMerchantModifiedValue]"))
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

        #region
        public int UpdateRejectedMerchantInfo(UserInfo objCustomerUserInfo, String MerchantCategory)
        {

            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNMerchantInfoUpdateRejectedSuperAdmin]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", objCustomerUserInfo.ClientCode));
                        sqlCmd.Parameters.Add(new SqlParameter("@Name", objCustomerUserInfo.BusinessName));
                        sqlCmd.Parameters.Add(new SqlParameter("@Status", objCustomerUserInfo.Status));
                        sqlCmd.Parameters.Add(new SqlParameter("@IsApproved", objCustomerUserInfo.IsApproved));
                        sqlCmd.Parameters.Add(new SqlParameter("@Address", objCustomerUserInfo.Address));
                        sqlCmd.Parameters.Add(new SqlParameter("@FirstName", objCustomerUserInfo.FName));
                        sqlCmd.Parameters.Add(new SqlParameter("@MiddleName", objCustomerUserInfo.MName));
                        sqlCmd.Parameters.Add(new SqlParameter("@LastName", objCustomerUserInfo.LName));
                        sqlCmd.Parameters.Add(new SqlParameter("@PStreet", objCustomerUserInfo.PStreet));
                        sqlCmd.Parameters.Add(new SqlParameter("@PVDC", objCustomerUserInfo.PVDC));
                        sqlCmd.Parameters.Add(new SqlParameter("@PHouseNo", objCustomerUserInfo.PHouseNo));
                        sqlCmd.Parameters.Add(new SqlParameter("@PWardNo", objCustomerUserInfo.PWardNo));
                        sqlCmd.Parameters.Add(new SqlParameter("@PDistrictID", objCustomerUserInfo.PDistrictID));
                        sqlCmd.Parameters.Add(new SqlParameter("@PProvinceID", objCustomerUserInfo.PProvince));
                        sqlCmd.Parameters.Add(new SqlParameter("@BusinessName", objCustomerUserInfo.BusinessName));
                        sqlCmd.Parameters.Add(new SqlParameter("@CatId", MerchantCategory));
                        sqlCmd.Parameters.Add(new SqlParameter("@RegistrationNumber", objCustomerUserInfo.RegistrationNumber));
                        sqlCmd.Parameters.Add(new SqlParameter("@VATNumber", objCustomerUserInfo.VATNumber));
                        sqlCmd.Parameters.Add(new SqlParameter("@LandlineNumber", objCustomerUserInfo.LandlineNumber));

                        sqlCmd.Parameters.Add(new SqlParameter("@EmailAddress", objCustomerUserInfo.EmailAddress));
                        sqlCmd.Parameters.Add(new SqlParameter("@PANNumber", objCustomerUserInfo.PanNo));
                        sqlCmd.Parameters.Add(new SqlParameter("@WebsiteName", objCustomerUserInfo.WebsiteName));
                        sqlCmd.Parameters.Add(new SqlParameter("@ContactNumber1", objCustomerUserInfo.ContactNumber1));
                        sqlCmd.Parameters.Add(new SqlParameter("@ContactNumber2", objCustomerUserInfo.ContactNumber2));
                        sqlCmd.Parameters.Add(new SqlParameter("@BankAccountNumber", objCustomerUserInfo.BankAccountNumber));
                        sqlCmd.Parameters.Add(new SqlParameter("@BankCode", objCustomerUserInfo.BankNo));
                        sqlCmd.Parameters.Add(new SqlParameter("@AcBranchCode", objCustomerUserInfo.BranchCode));

                        sqlCmd.Parameters.Add(new SqlParameter("@CitizenshipNo", objCustomerUserInfo.Citizenship));
                        sqlCmd.Parameters.Add(new SqlParameter("@CitizenPlaceOfIssue", objCustomerUserInfo.CitizenshipPlaceOfIssue));
                        sqlCmd.Parameters.Add(new SqlParameter("@CitizenIssueDate", objCustomerUserInfo.CitizenshipIssueDate));
                        sqlCmd.Parameters.Add(new SqlParameter("@BSCitizenIssueDate", objCustomerUserInfo.BSCitizenshipIssueDate));

                        sqlCmd.Parameters.Add(new SqlParameter("@PassportImage", objCustomerUserInfo.PassportImageName));
                        sqlCmd.Parameters.Add(new SqlParameter("@FrontImage", objCustomerUserInfo.FrontImageName));
                        sqlCmd.Parameters.Add(new SqlParameter("@BackImage", objCustomerUserInfo.BackImageName));
                        sqlCmd.Parameters.Add(new SqlParameter("@RegCertiImage", objCustomerUserInfo.RegCertificatePhotoName));
                        sqlCmd.Parameters.Add(new SqlParameter("@TaxClearFrontImage", objCustomerUserInfo.TaxClearFrontName));
                        sqlCmd.Parameters.Add(new SqlParameter("@TaxClearBackImage", objCustomerUserInfo.TaxClearBackName));


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
        internal int MerchantModApprove(UserInfo objUserInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
           {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNApproveMerchant]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", objUserInfo.ClientCode));
                        sqlCmd.Parameters.Add(new SqlParameter("@ApprovingUser", objUserInfo.AdminUserName));                        
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

        #region Merchant Reject/Approve
        public int MerchantRegReject(UserInfo objUserInfo, string Rejected)
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
                        cmd.Parameters.AddWithValue("@Remarks", objUserInfo.Remarks);//objUserInfo.Remarks
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

        #region Merchant Reject/Approve
        public int MerchantModReject(UserInfo objUserInfo, string Rejected)
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
                        cmd.Parameters.AddWithValue("@Remarks", objUserInfo.Remarks);//objUserInfo.Remarks
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

        #region Rejected List for Merchant

        public List<UserInfo> GetAllMerchantModifiedInformation(UserInfo objUserInfo, string isModified, string UserName)
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
                        string Command = @"SELECT * FROM dbo.v_MNClientDetail WHERE UserType = 'merchant' AND IsApproved = 'Approve' AND IsModified ='T' AND Status = 'Active' AND IsRejected='F'";

                        if (!string.IsNullOrEmpty(UserName))
                        {
                            //Command = Command + " AND UserName='" +UserName+ "'";
                            Command = Command + " AND UPPER(Name) like UPPER('" + UserName + "%')";
                        }

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

        #region Rejected List for Merchant

        public List<UserInfo> GetAllMerchantModifyRejectedInformation(UserInfo objUserInfo, string isModified,string UserName)
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
                        string Command = @"SELECT * FROM v_MNClientDetail WHERE UserType = 'merchant' AND IsApproved = 'Approve' AND IsRejected= 'T'  AND IsModified= 'T' ";
                        if (!string.IsNullOrEmpty(UserName))
                        {
                            //Command = Command + " AND UserName='" +UserName+ "'";
                            Command = Command + " AND UPPER(Name) like UPPER('" + UserName + "%')";
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
                            Info.UserBranchName = rdr["UserBranchName"].ToString();
                            Info.UserBranchCode = rdr["UserBranchCode"].ToString();
                            Info.EmailAddress = rdr["EmailAddress"].ToString();
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

        #region Makerchecker
        public int InsertIntoMakerCheckerMerchant(string ClientCode, string ModifyingAdmin, string ModifyingBranch, string ModifiedField)
        {

            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNMerchantMakerChecker]", sqlCon))
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


        #region Rejected List for Merchant

        public List<UserInfo> GetAllMerchantRejectedInformation(UserInfo objUserInfo, string isModified, string UserName)
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
                        string Command = @"SELECT * FROM v_MNClientDetail WHERE UserType = 'merchant' AND IsRejected = 'T' AND ISNULL(IsModified,'F')=@IsModified";

                        if (!string.IsNullOrEmpty(UserName))
                        {
                            //Command = Command + " AND UserName='" +UserName+ "'";
                            Command = Command + " AND UPPER(Name) like UPPER('" + UserName + "%')";
                        }
                        //AND IsApproved = 'UnApprove'
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

        #region Rejected List for Merchant

        public List<UserInfo> GetAllMerchantApprovedInformation(UserInfo objUserInfo, string isModified, string UserName)
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
                        string Command = @"SELECT * FROM v_MNClientDetail WHERE UserType = 'merchant' AND IsApproved = 'Approve' AND IsRejected= 'F' AND IsModified= 'F' ";


                        if (!string.IsNullOrEmpty(UserName))
                        {
                            //Command = Command + " AND UPPER(Name) like UPPER('" +UserName+ "%')";
                            Command = Command + " AND (UPPER(Name) like UPPER('" + UserName + "%') OR UPPER(UserName) like UPPER('" + UserName + "%'))";        //Search mobile and Name               
                        }
                        //AND IsApproved = 'UnApprove'
                        //cmd.Parameters.AddWithValue("@IsModified", isModified);
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


        #region Rejected List for Merchant

        public List<UserInfo> GetAllRegisteredMerchantInformation(UserInfo objUserInfo, string isModified, string UserName)
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
                        //string Command = @"SELECT * FROM v_MNClientDetail WHERE UserType = 'merchant' AND IsApproved = 'Approve' AND IsRejected= 'F'  
                        //            AND ISNULL(IsModified,'F')=@IsModified";

                        string Command = @"SELECT * FROM dbo.v_MNClientDetail WHERE UserType = 'merchant' AND IsApproved = 'UnApprove' AND Status = 'InActive' AND ISNULL(IsRejected,'F')='F'";

                        if (!string.IsNullOrEmpty(UserName))
                        {
                            //Command = Command + " AND UserName='" +UserName+ "'";
                            Command = Command + " AND UPPER(Name) like UPPER('" + UserName + "%')";


                        }
                        //AND IsApproved = 'UnApprove'
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


        public DataTable GetCustomerDetailInformation(UserInfo objUserInfo)
        {
            Database database;
            DataTable dtableResult = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNMerchantDetails]"))
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

        public int RegisterMerchantInfo(UserInfo objUserInfo, string MerchantCategory)
        {
            SqlConnection sqlCon = null;
            SqlTransaction sTrans = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {

                    sqlCon.Open();
                    sTrans = sqlCon.BeginTransaction();
                    
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNMerchantRegSuperAdmin]", sqlCon))
                    {
                        sqlCmd.Transaction = sTrans;
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", objUserInfo.ClientCode));
                        sqlCmd.Parameters.AddWithValue("@Name", objUserInfo.BusinessName);

                        sqlCmd.Parameters.AddWithValue("@FirstName", objUserInfo.FName);
                        sqlCmd.Parameters.AddWithValue("@MiddleName", objUserInfo.MName);
                        sqlCmd.Parameters.AddWithValue("@LastName", objUserInfo.LName);
                        sqlCmd.Parameters.AddWithValue("@PStreet", objUserInfo.PStreet);
                        sqlCmd.Parameters.AddWithValue("@PVDC", objUserInfo.PVDC);
                        sqlCmd.Parameters.AddWithValue("@PHouseNo", objUserInfo.PHouseNo);
                        sqlCmd.Parameters.AddWithValue("@PWardNo", objUserInfo.PWardNo);
                        sqlCmd.Parameters.AddWithValue("@PDistrictID", objUserInfo.PDistrictID);
                        sqlCmd.Parameters.AddWithValue("@PProvinceID", objUserInfo.PProvince);
                        sqlCmd.Parameters.AddWithValue("@BusinessName", objUserInfo.BusinessName);
                        sqlCmd.Parameters.AddWithValue("@RegistrationNumber", objUserInfo.RegistrationNumber);
                        sqlCmd.Parameters.AddWithValue("@VATNumber", objUserInfo.VATNumber);
                        sqlCmd.Parameters.AddWithValue("@PANNumber", objUserInfo.PanNo);
                        sqlCmd.Parameters.AddWithValue("@WebsiteName", objUserInfo.WebsiteName);
                        //sqlCmd.Parameters.AddWithValue("@MerchantType", objUserInfo.MerchantType);
                        sqlCmd.Parameters.AddWithValue("@LandlineNumber", objUserInfo.LandlineNumber);
                        sqlCmd.Parameters.AddWithValue("@Address", objUserInfo.Address);
                        sqlCmd.Parameters.AddWithValue("@CatId", MerchantCategory);
                        sqlCmd.Parameters.AddWithValue("@PIN", objUserInfo.PIN);
                        sqlCmd.Parameters.AddWithValue("@Status", objUserInfo.Status);

                        sqlCmd.Parameters.AddWithValue("@ContactNumber1", objUserInfo.ContactNumber1);
                        sqlCmd.Parameters.AddWithValue("@ContactNumber2", objUserInfo.ContactNumber2);
                        sqlCmd.Parameters.AddWithValue("@EmailAddress", objUserInfo.EmailAddress);
                        sqlCmd.Parameters.AddWithValue("@UserName", objUserInfo.UserName);
                        sqlCmd.Parameters.AddWithValue("@Password", objUserInfo.Password);

                        sqlCmd.Parameters.AddWithValue("@IsApproved", objUserInfo.IsApproved);
                        sqlCmd.Parameters.AddWithValue("@IsRejected", objUserInfo.IsRejected);
                        sqlCmd.Parameters.AddWithValue("@UserType", objUserInfo.UserType);

                        sqlCmd.Parameters.AddWithValue("@WalletNumber", objUserInfo.WalletNumber);


                        sqlCmd.Parameters.AddWithValue("@WBankCodeBin", objUserInfo.WBankCode);
                        sqlCmd.Parameters.AddWithValue("@WBranchCode", objUserInfo.WBranchCode);
                        sqlCmd.Parameters.AddWithValue("@WIsDefault", objUserInfo.WIsDefault);
                        sqlCmd.Parameters.AddWithValue("@AgentId", objUserInfo.AgentId);

                        sqlCmd.Parameters.AddWithValue("@BankAccountNumber", objUserInfo.BankAccountNumber);
                        sqlCmd.Parameters.AddWithValue("@BankNo", objUserInfo.BankNo);
                        sqlCmd.Parameters.AddWithValue("@BranchCode", objUserInfo.BranchCode);
                        sqlCmd.Parameters.AddWithValue("@BIsDefault", objUserInfo.IsDefault);

                        sqlCmd.Parameters.Add(new SqlParameter("@CitizenshipNo", objUserInfo.Citizenship));
                        sqlCmd.Parameters.Add(new SqlParameter("@CitizenIssueDate", objUserInfo.CitizenshipIssueDate));
                        sqlCmd.Parameters.Add(new SqlParameter("@BSCitizenIssueDate", objUserInfo.BSCitizenshipIssueDate));
                        sqlCmd.Parameters.Add(new SqlParameter("@CitizenPlaceOfIssue", objUserInfo.CitizenshipPlaceOfIssue));

                        sqlCmd.Parameters.AddWithValue("@FrontImage", objUserInfo.FrontImage);
                        sqlCmd.Parameters.AddWithValue("@BackImage", objUserInfo.BackImage);
                        sqlCmd.Parameters.AddWithValue("@PassportImage", objUserInfo.PassportImage);
                        sqlCmd.Parameters.AddWithValue("@RegCertiImage", objUserInfo.RegCertificateImage);
                        sqlCmd.Parameters.AddWithValue("@TaxClearFrontImage", objUserInfo.TaxClearFrontImage);
                        sqlCmd.Parameters.AddWithValue("@TaxClearBackImage", objUserInfo.TaxClearBackImage);

                        sqlCmd.Parameters.AddWithValue("@retrievalRef", objUserInfo.retrievalReference);
                        sqlCmd.Parameters.AddWithValue("@Remark", "Register By Superadmin THAILI MERCHANT ACTYPE: ");

                        sqlCmd.Parameters.Add("@RegIDOut", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@RegIDOut"].Direction = ParameterDirection.Output;

                        ret = sqlCmd.ExecuteNonQuery();
                        ret = Convert.ToInt32(sqlCmd.Parameters["@RegIDOut"].Value);
                        sTrans.Commit();
                    }

                }
            }
            catch (Exception ex)
            {
                sTrans.Rollback();
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

        #region Update Merchant Status
        public int UpdateMerchantStatus(string ClientCode, string Status, string SAdminBranchCode, string SAdminUserName, string BlockRemarks)
        {

            SqlConnection sqlCon = null;
            int ret = 0;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand())
                    {
                        string command = "Update MNClient set BlockStatus=@Status,ModifyingBranch=@ModifyingBranch,ModifiedBy=@ModifiedBy ,BlockRemarks=@BlockRemarks where ClientCode=@ClientCode";
                        sqlCmd.CommandText = command;
                        sqlCmd.Parameters.AddWithValue("@Status", Status);
                        sqlCmd.Parameters.AddWithValue("@ModifyingBranch", SAdminBranchCode);
                        sqlCmd.Parameters.AddWithValue("@ModifiedBy", SAdminUserName);
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
        #endregion

        #region Merchant Status Changed List
        public List<UserInfo> GetMerchantStatusList(string MobileNo)
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
                        Command = @"Select * from v_MNClientDetail where BlockStatus in (@Status1,@Status2) AND UserType = 'merchant'";


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

        #region Merchant Pin/Password List
        public List<UserInfo> GetPinApproveList(string BranchCode, bool COC, string MobileNo)
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
                        Command = @"Select * from v_MNClientDetail where Status in (@Status1,@Status2,@Status3) AND IsApproved=@IsApproved  AND UserType='merchant' ";
                        if (!COC)
                        {
                            Command = Command + " AND ModifyingBranch='" + BranchCode + "'";
                        }
                        if (!string.IsNullOrEmpty(MobileNo))
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
        #endregion



        public Dictionary<string,string> GetMerchantsType()
        {
            SqlDataReader rdr;
            SqlConnection conn = null;
            Dictionary<string, string> ListMerchants = new Dictionary<string, string>();
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SELECT * FROM MNMerchant (NOLOCK) ";
                        cmd.Connection = conn;
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            string MerchantType = rdr["Name"].ToString();
                            string MerchantTypeId = rdr["Id"].ToString();
                            ListMerchants.Add(MerchantType,MerchantTypeId);
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
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }

            }
            return ListMerchants;
        }

        public Dictionary<string, string> GetServiceType()
        {
            SqlDataReader rdr;
            SqlConnection conn = null;
            Dictionary<string, string> ListMerchants = new Dictionary<string, string>();
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SELECT * FROM MNServiceCodes";
                        cmd.Connection = conn;
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            string MerchantType = rdr["ServiceCodeDesc"].ToString();
                            string MerchantTypeId = rdr["ServiceCode"].ToString();
                            ListMerchants.Add(MerchantType, MerchantTypeId);
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
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }

            }
            return ListMerchants;
        }

        #region RegisterResponse
        public int InsertResponseQuickSelfReg(string userName, string retRef, string statusCode, string statusMsg)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_RegResponseInsert]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.AddWithValue("@UserName", userName);
                        sqlCmd.Parameters.AddWithValue("@retrievalRef", retRef);
                        sqlCmd.Parameters.AddWithValue("@StatusCode", statusCode);
                        sqlCmd.Parameters.AddWithValue("@StatusMsg", statusMsg);

                        sqlCmd.Parameters.AddWithValue("@mode", "RQISR");

                        sqlCmd.Parameters.Add("@RegIDOut", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@RegIDOut"].Direction = ParameterDirection.Output;

                        ret = sqlCmd.ExecuteNonQuery();
                        ret = Convert.ToInt32(sqlCmd.Parameters["@RegIDOut"].Value);

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