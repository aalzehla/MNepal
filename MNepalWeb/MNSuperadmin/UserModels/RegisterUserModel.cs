using MNSuperadmin.Connection;
using MNSuperadmin.Helper;
using MNSuperadmin.Models;
using System;
using System.Data;
using System.Data.SqlClient;

namespace MNSuperadmin.UserModels
{
    public class RegisterUserModel
    {
        #region "Update Member Information "

        /// <summary>
        /// Update the Member Information
        /// </summary>
        /// <param name="objUserInfo"></param>
        /// <returns></returns>
        public int UpdateMemberInfo(UserInfo objUserInfo)
        {
            int ret;
            try
            {
                using (SqlConnection sqlCon = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNUserUpdateInfo]", sqlCon))
                    {
                        sqlCmd.Parameters.AddWithValue("@ClientCode", objUserInfo.ClientCode);
                        sqlCmd.Parameters.AddWithValue("@Password", objUserInfo.Password);

                        sqlCmd.Parameters.Add("@RegIDOut", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@RegIDOut"].Direction = ParameterDirection.Output;

                        ret = sqlCmd.ExecuteNonQuery();
                        if (objUserInfo.Mode.Equals("UPWD", StringComparison.InvariantCultureIgnoreCase))
                        {
                            ret = (int)sqlCmd.Parameters["@RegIDOut"].Value;
                        }
                    }
                    sqlCon.Close();
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return ret;
        }

        #endregion
        

        #region "Checking Mobile Number"


        public DataTable GetMobileInfo(UserInfo objUserMobileNoInfo)
        {
            DataTable dtableResult = null;
            SqlConnection conn = null;

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNUserMobileNo]", conn))
                    {
                        cmd.Parameters.AddWithValue("@mobileNo", objUserMobileNoInfo.ContactNumber1);
                        cmd.Parameters.AddWithValue("@mode", objUserMobileNoInfo.Mode);
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 60;  // seconds
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset, "dtUserMobileNoInfo");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables["dtUserMobileNoInfo"];
                                }
                            }
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
                if (conn != null)
                {
                    conn.Close();
                }
            }
            return dtableResult;
        }


        #endregion


        #region "Checking Client Code"

        /// <summary>
        /// Retrieve the user information based on mode
        /// </summary>
        /// <param name="objUserInfo">Pass an instance of User information</param>
        /// <returns>Returns the  avail baln information based on user information</returns>
        public DataTable GetClientCodeInfo(UserInfo objUserInfo)
        {
            DataTable dtableResult = null;
            SqlConnection conn = null;

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNBankTable]", conn))
                    {
                        cmd.Parameters.AddWithValue("@mode", objUserInfo.Mode);
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 60;  // seconds
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset, "dtUserClientCode");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables["dtUserClientCode"];
                                }
                            }
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
                if (conn != null)
                {
                    conn.Close();
                }
            }
            return dtableResult;
        }

        #endregion


        #region "Register User Information "

        /// <summary>
        /// Register the User Information
        /// </summary>
        /// <param name="objUserInfo"></param>
        /// <returns></returns>
        public int AdminRegisterUserInfo(UserInfo objUserInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNSuperAdminRegistration]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", objUserInfo.ClientCode));
                        sqlCmd.Parameters.AddWithValue("@Name", objUserInfo.Name);
                        sqlCmd.Parameters.AddWithValue("@Address", objUserInfo.Address);
                        
                        sqlCmd.Parameters.AddWithValue("@Status", objUserInfo.Status);
                        sqlCmd.Parameters.AddWithValue("@IsApproved", objUserInfo.IsApproved);
                        sqlCmd.Parameters.AddWithValue("@IsRejected", objUserInfo.IsRejected);

                        sqlCmd.Parameters.AddWithValue("@ContactNumber1", objUserInfo.ContactNumber1);
                        sqlCmd.Parameters.AddWithValue("@ContactNumber2", objUserInfo.ContactNumber2);
                        sqlCmd.Parameters.AddWithValue("@EmailAddress", objUserInfo.EmailAddress);

                        sqlCmd.Parameters.AddWithValue("@UserName", objUserInfo.UserName);
                        sqlCmd.Parameters.AddWithValue("@Password", HashAlgo.Hash(objUserInfo.Password));
                        sqlCmd.Parameters.AddWithValue("@UserType", objUserInfo.UserType);
                        sqlCmd.Parameters.AddWithValue("@ProfileName", objUserInfo.UserGroup);
                        sqlCmd.Parameters.AddWithValue("@BranchCode", objUserInfo.BranchCode);
                        sqlCmd.Parameters.AddWithValue("@PushSMS", objUserInfo.PushSMS);
                        sqlCmd.Parameters.AddWithValue("@COC", objUserInfo.COC);
                        sqlCmd.Parameters.AddWithValue("@CreatedBy", objUserInfo.CreatedBy);
                        sqlCmd.Parameters.AddWithValue("@mode", objUserInfo.Mode);

                        sqlCmd.Parameters.Add("@RegIDOut", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@RegIDOut"].Direction = ParameterDirection.Output;

                        ret = sqlCmd.ExecuteNonQuery();
                        if (objUserInfo.Mode.Equals("IAP", StringComparison.InvariantCultureIgnoreCase))
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




   

        /// <summary>
        /// Register the User Information
        /// </summary>
        /// <param name="objUserInfo"></param>
        /// <returns></returns>
        public int AgentRegisterUserInfo(UserInfo objUserInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNAgentRegKyc]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                     
                        sqlCmd.Parameters.AddWithValue("@Name", objUserInfo.Name);
                        sqlCmd.Parameters.AddWithValue("@Address", objUserInfo.Address);
                        sqlCmd.Parameters.AddWithValue("@PIN", objUserInfo.PIN);
                        sqlCmd.Parameters.AddWithValue("@Status", objUserInfo.Status);
                        sqlCmd.Parameters.AddWithValue("@FName", objUserInfo.FName);
                        sqlCmd.Parameters.AddWithValue("@MName", objUserInfo.MName);
                        sqlCmd.Parameters.AddWithValue("@LName", objUserInfo.LName);
                        sqlCmd.Parameters.AddWithValue("@Gender", objUserInfo.Gender);
                        sqlCmd.Parameters.AddWithValue("@DateOfBirth", objUserInfo.DOB);
                        sqlCmd.Parameters.AddWithValue("@BSDateOfBirth", objUserInfo.BSDateOfBirth);
                        sqlCmd.Parameters.AddWithValue("@EmailAddress", objUserInfo.EmailAddress);

                        sqlCmd.Parameters.AddWithValue("@Nationality", objUserInfo.Nationality);
                        sqlCmd.Parameters.AddWithValue("@Country", objUserInfo.Country);
                        sqlCmd.Parameters.AddWithValue("@FathersName", objUserInfo.FatherName);
                        sqlCmd.Parameters.AddWithValue("@MothersName", objUserInfo.MotherName);
                        sqlCmd.Parameters.AddWithValue("@MaritalStatus", objUserInfo.MaritalStatus);
                        sqlCmd.Parameters.AddWithValue("@SpouseName", objUserInfo.SpouseName);
                        //start father
                        sqlCmd.Parameters.AddWithValue("@FatherInLaw", objUserInfo.FatherInLaw);
                        //end father
                        sqlCmd.Parameters.AddWithValue("@GFathersName", objUserInfo.GrandFatherName);
                        sqlCmd.Parameters.AddWithValue("@Occupation", objUserInfo.Occupation);

                        sqlCmd.Parameters.AddWithValue("@PProvince", objUserInfo.PProvince);
                        sqlCmd.Parameters.AddWithValue("@PDistrictID", objUserInfo.PDistrict);
                        //sqlCmd.Parameters.AddWithValue("@PZone", objUserInfo.PZone);
                        sqlCmd.Parameters.AddWithValue("@PDistrict", objUserInfo.PDistrict);
                        sqlCmd.Parameters.AddWithValue("@PMunicipalityVDC", objUserInfo.PVDC);
                        sqlCmd.Parameters.AddWithValue("@PHouseNo", objUserInfo.PHouseNo);
                        sqlCmd.Parameters.AddWithValue("@PWardNo", objUserInfo.PWardNo);
                        sqlCmd.Parameters.AddWithValue("@PStreet", objUserInfo.PStreet);

                        sqlCmd.Parameters.AddWithValue("@CProvince", objUserInfo.CProvince);
                        sqlCmd.Parameters.AddWithValue("@CDistrictID", objUserInfo.CDistrict);
                        //sqlCmd.Parameters.AddWithValue("@CZone", objUserInfo.CZone);
                        sqlCmd.Parameters.AddWithValue("@CDistrict", objUserInfo.CDistrict);
                        sqlCmd.Parameters.AddWithValue("@CMunicipalityVDC", objUserInfo.CVDC);
                        sqlCmd.Parameters.AddWithValue("@CHouseNo", objUserInfo.CHouseNo);
                        sqlCmd.Parameters.AddWithValue("@CWardNo", objUserInfo.CWardNo);
                        sqlCmd.Parameters.AddWithValue("@CStreet", objUserInfo.CStreet);

                        sqlCmd.Parameters.AddWithValue("@CitizenshipNo", objUserInfo.Citizenship);
                        sqlCmd.Parameters.AddWithValue("@CitizenIssueDate", objUserInfo.CitizenshipIssueDate);
                        sqlCmd.Parameters.AddWithValue("@BSCitizenIssueDate", objUserInfo.BSCitizenshipIssueDate);
                        sqlCmd.Parameters.AddWithValue("@CitizenPlaceOfIssue", objUserInfo.CitizenshipPlaceOfIssue);
                        sqlCmd.Parameters.AddWithValue("@LicenseNo", objUserInfo.License);
                        sqlCmd.Parameters.AddWithValue("@LicenseIssueDate", objUserInfo.LicenseIssueDate);
                        sqlCmd.Parameters.AddWithValue("@BSLicenseIssueDate", objUserInfo.BSLicenseIssueDate);
                        sqlCmd.Parameters.AddWithValue("@LicensePlaceOfIssue", objUserInfo.LicensePlaceOfIssue);
                        sqlCmd.Parameters.AddWithValue("@LicenseExpiryDate", objUserInfo.LicenseExpireDate);
                        sqlCmd.Parameters.AddWithValue("@BSLicenseExpiryDate", objUserInfo.BSLicenseExpireDate);
                        sqlCmd.Parameters.AddWithValue("@PassportNo", objUserInfo.Passport);
                        sqlCmd.Parameters.AddWithValue("@PassportIssueDate", objUserInfo.PassportIssueDate);
                        sqlCmd.Parameters.AddWithValue("@PassportPlaceOfIssue", objUserInfo.PassportPlaceOfIssue);
                        sqlCmd.Parameters.AddWithValue("@PassportExpiryDate", objUserInfo.PassportExpireDate);
                        sqlCmd.Parameters.AddWithValue("@PANNumber", objUserInfo.PanNo);
                        
                        sqlCmd.Parameters.AddWithValue("@DocType", objUserInfo.Document);                      
                        sqlCmd.Parameters.AddWithValue("@UserName", objUserInfo.UserName);
                        sqlCmd.Parameters.AddWithValue("@Password", objUserInfo.Password);
                        sqlCmd.Parameters.AddWithValue("@IsApproved", objUserInfo.IsApproved);
                        sqlCmd.Parameters.AddWithValue("@IsRejected", objUserInfo.IsRejected);
                        sqlCmd.Parameters.AddWithValue("@UserType", objUserInfo.UserType);
                        sqlCmd.Parameters.AddWithValue("@CreatedBy", objUserInfo.CreatedBy);
                        //sqlCmd.Parameters.AddWithValue("@BankAccountNumber", objUserInfo.BankAccountNumber);
                        //sqlCmd.Parameters.AddWithValue("@BankNo", objUserInfo.BankNo);
                        //sqlCmd.Parameters.AddWithValue("@BranchCode", objUserInfo.BranchCode);
                        sqlCmd.Parameters.AddWithValue("@FrontImage", objUserInfo.FrontImage);
                        sqlCmd.Parameters.AddWithValue("@BackImage", objUserInfo.BackImage);
                        sqlCmd.Parameters.AddWithValue("@PassportImage", objUserInfo.PassportImage);

                        sqlCmd.Parameters.AddWithValue("@retrievalRef", objUserInfo.retrievalReference);
                        sqlCmd.Parameters.AddWithValue("@Remark", "Register By Superadmin THAILI AGENT ACTYPE: ");



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


        public int RegisterUsersInfo(UserInfo objUserInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNRegistration]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", objUserInfo.ClientCode));
                        sqlCmd.Parameters.AddWithValue("@Name", objUserInfo.Name);
                        sqlCmd.Parameters.AddWithValue("@Address", objUserInfo.Address);
                        //start milayako3
                        sqlCmd.Parameters.AddWithValue("@WardNumber", objUserInfo.WardNumber);
                        sqlCmd.Parameters.AddWithValue("@Province", objUserInfo.Province);
                        sqlCmd.Parameters.AddWithValue("@District", objUserInfo.District);
                        //end milayako3
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
                        sqlCmd.Parameters.AddWithValue("@ProfileCode", objUserInfo.ProfileCode);

                        sqlCmd.Parameters.AddWithValue("@WBankCodeBin", objUserInfo.WBankCode);
                        sqlCmd.Parameters.AddWithValue("@WBranchCode", objUserInfo.WBranchCode);
                        sqlCmd.Parameters.AddWithValue("@WIsDefault", objUserInfo.WIsDefault);
                        sqlCmd.Parameters.AddWithValue("@AgentId", objUserInfo.AgentId);

                        sqlCmd.Parameters.AddWithValue("@BankAccountNumber", objUserInfo.BankAccountNumber);
                        sqlCmd.Parameters.AddWithValue("@BankNo", objUserInfo.BankNo);
                        sqlCmd.Parameters.AddWithValue("@BranchCode", objUserInfo.BranchCode);
                        sqlCmd.Parameters.AddWithValue("@BIsDefault", objUserInfo.IsDefault);
                        sqlCmd.Parameters.AddWithValue("@TxnAccounts", objUserInfo.TxnAccounts);

                        sqlCmd.Parameters.AddWithValue("@UserBranchCode", objUserInfo.AdminBranch);
                        sqlCmd.Parameters.AddWithValue("@CreatedBy", objUserInfo.AdminUserName);

                        ////
                        sqlCmd.Parameters.AddWithValue("@IndvTxn", objUserInfo.Transaction);
                        sqlCmd.Parameters.AddWithValue("@DateRange", objUserInfo.DateRange);
                        sqlCmd.Parameters.AddWithValue("@StartDate", objUserInfo.StartDate);
                        sqlCmd.Parameters.AddWithValue("@EndDate", objUserInfo.EndDate);
                        sqlCmd.Parameters.AddWithValue("@LimitType", objUserInfo.LimitType);
                        sqlCmd.Parameters.AddWithValue("@TransactionLimit", objUserInfo.TransactionLimit);
                        sqlCmd.Parameters.AddWithValue("@TransactionCount", objUserInfo.TransactionCount);
                        sqlCmd.Parameters.AddWithValue("@TransactionLimitMonthly", objUserInfo.TransactionLimitMonthly);
                        sqlCmd.Parameters.AddWithValue("@TransactionLimitDaily", objUserInfo.TransactionLimitDaily);

                        ////

                        sqlCmd.Parameters.AddWithValue("@mode", objUserInfo.Mode);

                        sqlCmd.Parameters.Add("@RegIDOut", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@RegIDOut"].Direction = ParameterDirection.Output;

                        ret = sqlCmd.ExecuteNonQuery();
                        if (objUserInfo.Mode.Equals("IUP", StringComparison.InvariantCultureIgnoreCase))
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

        #region "Create Wallet A/C Information"

        public int CreateWalletAcInfo(UserInfo objUserInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNCreateWalletAc]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", objUserInfo.ClientCode));
                        sqlCmd.Parameters.AddWithValue("@MainCode", objUserInfo.WalletNumber);
                        sqlCmd.Parameters.AddWithValue("@Name", objUserInfo.Name);
                        sqlCmd.Parameters.AddWithValue("@Address", objUserInfo.Address);

                        sqlCmd.Parameters.Add("@MesgStr", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@MesgStr"].Direction = ParameterDirection.Output;

                        sqlCmd.Parameters.Add("@return_value", SqlDbType.Int);
                        sqlCmd.Parameters["@return_value"].Direction = ParameterDirection.ReturnValue;

                        sqlCmd.ExecuteNonQuery();
                        string i = sqlCmd.Parameters["@MesgStr"].Value.ToString();
                        ret = Convert.ToInt32(sqlCmd.Parameters["@return_value"].Value);
                    }
                    sqlCon.Close();

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

        #region Customer Self Registration
        public int CustomerSelfRegInfo(CustomerSRInfo objsrInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNSelfRegistration]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;


                        //sqlCmd.Parameters.AddWithValue("@Name", objsrInfo.Name);
                        //sqlCmd.Parameters.AddWithValue("@Address", objsrInfo.Address);
                        sqlCmd.Parameters.AddWithValue("@PIN", objsrInfo.PIN);
                        sqlCmd.Parameters.AddWithValue("@Status", objsrInfo.Status);
                        sqlCmd.Parameters.AddWithValue("@FName", objsrInfo.FName);
                        sqlCmd.Parameters.AddWithValue("@MName", objsrInfo.MName);
                        sqlCmd.Parameters.AddWithValue("@LName", objsrInfo.LName);
                        sqlCmd.Parameters.AddWithValue("@Gender", objsrInfo.Gender);
                        sqlCmd.Parameters.AddWithValue("@DateOfBirth", objsrInfo.DOB);
                        sqlCmd.Parameters.AddWithValue("@CountryCode", objsrInfo.Country);
                        sqlCmd.Parameters.AddWithValue("@Nationality", objsrInfo.Nationality);
                        sqlCmd.Parameters.AddWithValue("@FathersName", objsrInfo.FatherName);
                        sqlCmd.Parameters.AddWithValue("@MothersName", objsrInfo.MotherName);
                        sqlCmd.Parameters.AddWithValue("@SpouseName", objsrInfo.SpouseName);
                        sqlCmd.Parameters.AddWithValue("@MaritalStatus", objsrInfo.MaritalStatus);
                        sqlCmd.Parameters.AddWithValue("@GFathersName", objsrInfo.GrandFatherName);
                        sqlCmd.Parameters.AddWithValue("@FatherInLaw", objsrInfo.FatherInLaw);
                        sqlCmd.Parameters.AddWithValue("@Occupation", objsrInfo.Occupation);
                        //
                        sqlCmd.Parameters.AddWithValue("@PProvince", objsrInfo.PProvince);
                        sqlCmd.Parameters.AddWithValue("@PDistrictID", objsrInfo.PDistrict);
                        sqlCmd.Parameters.AddWithValue("@PZone", "");
                        sqlCmd.Parameters.AddWithValue("@PDistrict", "");
                        sqlCmd.Parameters.AddWithValue("@PMunicipalityVDC", objsrInfo.PVDC);
                        sqlCmd.Parameters.AddWithValue("@PHouseNo", objsrInfo.PHouseNo);
                        sqlCmd.Parameters.AddWithValue("@PWardNo", objsrInfo.PWardNo);
                        sqlCmd.Parameters.AddWithValue("@PStreet", objsrInfo.PStreet);
                        //sqlCmd.Parameters.AddWithValue("@PAddress", objsrInfo.PAddress);

                        sqlCmd.Parameters.AddWithValue("@CProvince", objsrInfo.CProvince);
                        sqlCmd.Parameters.AddWithValue("@CDistrictID", objsrInfo.CDistrict);
                        sqlCmd.Parameters.AddWithValue("@CZone", "");
                        sqlCmd.Parameters.AddWithValue("@CDistrict", "");
                        sqlCmd.Parameters.AddWithValue("@CMunicipalityVDC", objsrInfo.CVDC);
                        sqlCmd.Parameters.AddWithValue("@CHouseNo", objsrInfo.CHouseNo);
                        sqlCmd.Parameters.AddWithValue("@CWardNo", objsrInfo.CWardNo);
                        sqlCmd.Parameters.AddWithValue("@CStreet", objsrInfo.CStreet);
                        //sqlCmd.Parameters.AddWithValue("@CAddress", objsrInfo.CAddress);
                        sqlCmd.Parameters.AddWithValue("@CitizenshipNo", objsrInfo.Citizenship);
                        sqlCmd.Parameters.AddWithValue("@CitizenIssueDate", objsrInfo.CitizenshipIssueDate);
                        sqlCmd.Parameters.AddWithValue("@CitizenPlaceOfIssue", objsrInfo.CitizenshipPlaceOfIssue);
                        sqlCmd.Parameters.AddWithValue("@LicenseNo", objsrInfo.License);
                        sqlCmd.Parameters.AddWithValue("@LicenseIssueDate", objsrInfo.LicenseIssueDate);
                        sqlCmd.Parameters.AddWithValue("@LicensePlaceOfIssue", objsrInfo.LicensePlaceOfIssue);
                        sqlCmd.Parameters.AddWithValue("@LicenseExpiryDate", objsrInfo.LicenseExpiryDate);
                        sqlCmd.Parameters.AddWithValue("@PassportNo", objsrInfo.Passport);
                        sqlCmd.Parameters.AddWithValue("@PassportIssueDate", objsrInfo.PassportIssueDate);
                        sqlCmd.Parameters.AddWithValue("@PassportPlaceOfIssue", objsrInfo.PassportPlaceOfIssue);
                        sqlCmd.Parameters.AddWithValue("@PassportExpiryDate", objsrInfo.PassportExpiryDate);
                        sqlCmd.Parameters.AddWithValue("@PANNumber", objsrInfo.PanNo);
                        sqlCmd.Parameters.AddWithValue("@OTPCode", objsrInfo.OTPCode);
                        sqlCmd.Parameters.AddWithValue("@Source", objsrInfo.Source);
                        
                        sqlCmd.Parameters.AddWithValue("@DocType", objsrInfo.Document);
                        //sqlCmd.Parameters.AddWithValue("@ContactNumber1", objUserInfo.ContactNumber1);
                        //sqlCmd.Parameters.AddWithValue("@ContactNumber2", objUserInfo.ContactNumber2);
                        sqlCmd.Parameters.AddWithValue("@EmailAddress", objsrInfo.EmailAddress);
                        sqlCmd.Parameters.AddWithValue("@UserName", objsrInfo.UserName);
                        sqlCmd.Parameters.AddWithValue("@Password", objsrInfo.Password);
                        //sqlCmd.Parameters.AddWithValue("@IsApproved", objsrInfo.IsApproved);
                        sqlCmd.Parameters.AddWithValue("@IsRejected", objsrInfo.IsRejected);
                        sqlCmd.Parameters.AddWithValue("@UserType", objsrInfo.UserType);
                        //sqlCmd.Parameters.AddWithValue("@BankAccountNumber", objsrInfo.BankAccountNumber);
                        //sqlCmd.Parameters.AddWithValue("@BankNo", objsrInfo.BankNo);
                        sqlCmd.Parameters.AddWithValue("@BranchCode", objsrInfo.BranchCode);
                        sqlCmd.Parameters.AddWithValue("@FrontImage", objsrInfo.FrontImage);
                        sqlCmd.Parameters.AddWithValue("@BackImage", objsrInfo.BackImage);
                        sqlCmd.Parameters.AddWithValue("@PassportImage", objsrInfo.PassportImage);
                        sqlCmd.Parameters.AddWithValue("@mode", objsrInfo.Mode);

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