using System;
using System.Data;
using System.Data.SqlClient;
using MNSuperadmin.Connection;
using MNSuperadmin.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Collections.Generic;
using MNSuperadmin.Utilities;

namespace MNSuperadmin.UserModels
{
    public class UserProfileUserModel
    {
        /// <summary>
        /// Retrieve the user profile information based on mode
        /// </summary>
        /// <param name="objUserInfo">Pass an instance of User Profile information</param>
        /// <returns>Returns the  user information based on user profile information</returns>
        public DataTable GetAllUserProfileInfo(UserProfilesInfo objUserInfo)
        {
            DataTable dtableResult = null;
            SqlConnection conn = null;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNSuperProfileInfo]", conn))
                    {
                        cmd.Parameters.AddWithValue("@mode", objUserInfo.Mode);
                        cmd.CommandType = CommandType.StoredProcedure;

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 60;  // seconds
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset, "dtUserProfileInfo");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables["dtUserProfileInfo"];
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

        /// <summary>
        /// Retrieve the user profile name information based on mode
        /// </summary>
        /// <param name="objUserInfo"></param>
        /// <returns></returns>
        public DataSet GetDsProfileNameInfo(UserProfilesInfo objUserInfo)
        {
            DataSet ds = new DataSet();
            SqlConnection conn = null;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNSuperProfileInfo]", conn))
                    {
                        cmd.Parameters.Add("@mode", SqlDbType.NVarChar).Value = objUserInfo.Mode;
                        cmd.CommandType = CommandType.StoredProcedure;

                        using (SqlDataAdapter daBranch = new SqlDataAdapter(cmd))
                        {
                            daBranch.Fill(ds);
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

            return ds;
        }


        /// <summary>
        /// Retrieve the user information based on mode
        /// </summary>
        /// <param name="objUserInfo">Pass an instance of User information</param>
        /// <returns>Returns the  user information based on user information</returns>
        public DataTable GetAdminProfileUserInformation(UserProfilesInfo objUserInfo)
        {
            Database database;
            DataTable dtableResult = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNListSuperAdminProfile]"))
                {
                    database.AddInParameter(command, "@ProfileCode", DbType.String, objUserInfo.UPID);
                    database.AddInParameter(command, "@mode", DbType.String, objUserInfo.Mode);
                    using (var dataset = new DataSet())
                    {
                        database.LoadDataSet(command, dataset, "dtAdminUserInfo");

                        if (dataset.Tables.Count > 0)
                        {
                            dtableResult = dataset.Tables["dtAdminUserInfo"];
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

        public DataTable GetAdminProfileNameInformation(string ProfileName)
        {
            Database database;
            DataTable dtableResult = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
              
                using (var command = database.GetSqlStringCommand("Select * from MNProfileTable where Upper(ProfileName)=@ProfileName"))
                {
                    database.AddInParameter(command, "ProfileName", DbType.String, ProfileName.ToUpper());
                    using (var dataset = new DataSet())
                    {
                        database.LoadDataSet(command, dataset, "dtAdminUserInfo");

                        if (dataset.Tables.Count > 0)
                        {
                            dtableResult = dataset.Tables["dtAdminUserInfo"];
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
        /// <param name="objUserInfo">Pass an instance of User information</param>
        /// <returns>Returns the  user information based on user information</returns>
        public DataTable GetAdminProfileMenuUserInformation(UserProfilesInfo objUserInfo)
        {
            Database database;
            DataTable dtableResult = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNListSuperAdminProfile]"))
                {
                    database.AddInParameter(command, "@ProfileCode", DbType.String, objUserInfo.UPID);
                    database.AddInParameter(command, "@mode", DbType.String, objUserInfo.Mode);
                    using (var dataset = new DataSet())
                    {
                        database.LoadDataSet(command, dataset, "dtAdminUserInfo");

                        if (dataset.Tables.Count > 0)
                        {
                            dtableResult = dataset.Tables["dtAdminUserInfo"];
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

        public DataSet GetAdminUserMenuListInformation(UserProfilesInfo objUserInfo)
        {
            Database database;
            DataTable dtableResult = null;
            DataSet ds = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNVMSAdminUserProfile]"))
                {
                    database.AddInParameter(command, "@ClientCode", DbType.String, objUserInfo.ClientCode);
                    database.AddInParameter(command, "@mode", DbType.String, objUserInfo.Mode);
                    using (var dataset = new DataSet())
                    {
                        database.LoadDataSet(command, dataset, "dtAdminUserInfo");

                        if (dataset.Tables.Count > 0)
                        {
                            dtableResult = dataset.Tables["dtAdminUserInfo"];
                            ds = dataset;
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

            return ds;
        }


        #region "Create Admin Profile Information "

        /// <summary>
        /// Create Admin Profile Information
        /// </summary>
        /// <param name="objUserInfo"></param>
        /// <returns></returns>

        public int CreateAdminSetupInfo(string objMenuAllow, UserProfilesInfo objUserInfo)
        {
            int ret;

            using (SqlConnection sqlCon = new SqlConnection(DatabaseConnection.ConnectionStr()))
            {
                try
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNInsertAdminUserProfile]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        sqlCmd.Parameters.Add(new SqlParameter("@UProfileName", objUserInfo.ProfileName));
                        sqlCmd.Parameters.Add("@UProfileDesc", SqlDbType.VarChar, 20).Value = objUserInfo.ProfileDesc;
                        sqlCmd.Parameters.Add("@HierarchyList", SqlDbType.VarChar).Value = objMenuAllow.Replace(@"'", "");
                        sqlCmd.Parameters.Add("@mode", SqlDbType.VarChar).Value = objUserInfo.Mode;
                        sqlCmd.Parameters.Add("@RegIDOut", SqlDbType.Int );
                        sqlCmd.Parameters["@RegIDOut"].Direction = ParameterDirection.Output;
                        ret = sqlCmd.ExecuteNonQuery();
                        if (objUserInfo.Mode.Equals("IAUP", StringComparison.InvariantCultureIgnoreCase))
                        {
                            ret = Convert.ToInt32(sqlCmd.Parameters["@RegIDOut"].Value);
                        }

                    }
                    sqlCon.Close();
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }
            return ret;
        }

        #endregion



        public int CreateAdminProfile(string ProfileName, string ProfileDesc,string selectedMenuHiearchy)
        {
            int ret;

            using (SqlConnection sqlCon = new SqlConnection(DatabaseConnection.ConnectionStr()))
            {
                try
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNInsertSuperAdminUserProfile]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        sqlCmd.Parameters.Add(new SqlParameter("@UProfileName", ProfileName));
                        sqlCmd.Parameters.Add("@UProfileDesc", SqlDbType.VarChar, 20).Value = ProfileDesc;
                        sqlCmd.Parameters.Add("@HierarchyList", SqlDbType.VarChar).Value = selectedMenuHiearchy.Replace(@"'", "");
                        sqlCmd.Parameters.Add("@mode", SqlDbType.VarChar).Value = "IAUP";
                        sqlCmd.Parameters.Add("@RegIDOut", SqlDbType.Int);
                        sqlCmd.Parameters["@RegIDOut"].Direction = ParameterDirection.Output;
                        ret = sqlCmd.ExecuteNonQuery();
                        ret = Convert.ToInt32(sqlCmd.Parameters["@RegIDOut"].Value);
                      

                    }
                    sqlCon.Close();
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }
            return ret;
        }


        public int UpdateAdminProfile(string ProfileId,string ProfileName, string ProfileDesc, string UserProfileStatus,string selectedMenuHiearchy)
        {
            Database database;
            int ret;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNSuperUpdateAssignMenu]"))
                {
                    database.AddInParameter(command, "@HierarchyList", DbType.String, selectedMenuHiearchy.Replace(@"'", ""));
                    database.AddInParameter(command, "@UProfileName", DbType.String, ProfileName);
                    database.AddInParameter(command, "@UProfileDesc", DbType.String, ProfileDesc);
                    database.AddInParameter(command, "@Status", DbType.String, UserProfileStatus);
                    database.AddInParameter(command, "@UPID", DbType.String, ProfileId);

                    database.AddInParameter(command, "@mode", DbType.String, "UAUP");
                    database.AddOutParameter(command, "@RegIDOut", DbType.Int32,int.MaxValue);
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


        #region "Update Admin Profile Information "

        /// <summary>
        /// Update Admin Profile Information
        /// </summary>
        /// <param name="objUserInfo"></param>
        /// <returns></returns>
        public int UpdateAdminProfile(string objMenuAllow, UserProfilesInfo objUserInfo)
        {
            Database database;
            int ret;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNUpdateAssignMenu]"))
                {
                    database.AddInParameter(command, "@HierarchyList", DbType.String, objMenuAllow.Replace(@"'", ""));
                    database.AddInParameter(command, "@UProfileName", DbType.String, objUserInfo.ProfileName);
                    database.AddInParameter(command, "@UProfileDesc", DbType.String, objUserInfo.ProfileDesc);
                    database.AddInParameter(command, "@Status", DbType.String, objUserInfo.UserProfileStatus);
                    database.AddInParameter(command, "@UPID", DbType.String, objUserInfo.UPID);

                    database.AddInParameter(command, "@mode", DbType.String, objUserInfo.Mode);
                    database.AddOutParameter(command, "@RegIDOut", DbType.Int32, objUserInfo.RegIdOut);
                    ret = database.ExecuteNonQuery(command);
                    if (objUserInfo.Mode.Equals("UAUP", StringComparison.InvariantCultureIgnoreCase))
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

        #endregion
        public List<MNMenuTable> GetMenu()
        {
            DataSet ds = new DataSet();
            SqlConnection conn = null;
            List<MNMenuTable> menus = new List<MNMenuTable>();
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("Select * from MNSuperMenuTable (nolock) where CanBeAssigned=1 order by Hierarchy", conn))
                    {

                        cmd.CommandType = CommandType.Text;

                        using (SqlDataAdapter damenu = new SqlDataAdapter(cmd))
                        {
                            damenu.Fill(ds);
                        }
                    }
                    conn.Close();
                }
                if (ds.Tables.Count > 0)
                {
                    menus = ExtraUtility.DatatableToListClass<MNMenuTable>(ds.Tables[0]);
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

            return menus;
        }


        #region "SuperAdmin Check ROLE SETUP "

        public DataTable GetSuperAdminProfileNameInformation(string ProfileName)
        {
            Database database;
            DataTable dtableResult = null;

            try
            {
                database = DatabaseConnection.GetDatabase();

                using (var command = database.GetSqlStringCommand("SELECT * FROM MNSuperAdminProfileTable WHERE UPPER(SProfileName) = @ProfileName"))
                {
                    database.AddInParameter(command, "ProfileName", DbType.String, ProfileName.ToUpper());
                    using (var dataset = new DataSet())
                    {
                        database.LoadDataSet(command, dataset, "dtAdminUserInfo");

                        if (dataset.Tables.Count > 0)
                        {
                            dtableResult = dataset.Tables["dtAdminUserInfo"];
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
    }
}