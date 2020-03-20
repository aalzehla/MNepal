using MNepalProject.Connection;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using MNepalWCF.Helper;
using MNepalWCF.Models;

namespace MNepalWCF.UserModels
{
    public class SelfRegisterUserModel
    {
        #region "Register Self Member Information "

        public int RegisterUsersInfo(UserValidate objUserInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNSelfRegistration]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.AddWithValue("@UserName", objUserInfo.UserName);
                        sqlCmd.Parameters.AddWithValue("@Password", objUserInfo.Password);
                        sqlCmd.Parameters.AddWithValue("@PIN", objUserInfo.PIN);

                        sqlCmd.Parameters.AddWithValue("@UserType", objUserInfo.userType);
                        sqlCmd.Parameters.AddWithValue("@OTPCode", objUserInfo.OTPCode);
                        sqlCmd.Parameters.AddWithValue("@Source", objUserInfo.Source);                        

                        sqlCmd.Parameters.AddWithValue("@FName", objUserInfo.FName);
                        sqlCmd.Parameters.AddWithValue("@MName", objUserInfo.MName);
                        sqlCmd.Parameters.AddWithValue("@LName", objUserInfo.LName);
                        sqlCmd.Parameters.AddWithValue("@Gender", objUserInfo.Gender);

                        sqlCmd.Parameters.AddWithValue("@DateOfBirth", DateTime.ParseExact(objUserInfo.DateOfBirth, "dd/MM/yyyy", null));
                        sqlCmd.Parameters.AddWithValue("@CountryCode", objUserInfo.CountryCode);
                        sqlCmd.Parameters.AddWithValue("@Nationality", objUserInfo.Nationality);
                        sqlCmd.Parameters.AddWithValue("@FathersName", objUserInfo.FathersName);
                        sqlCmd.Parameters.AddWithValue("@MothersName", objUserInfo.MothersName);
                        sqlCmd.Parameters.AddWithValue("@SpouseName", objUserInfo.SpouseName);
                        sqlCmd.Parameters.AddWithValue("@MaritalStatus", objUserInfo.MaritalStatus);
                        sqlCmd.Parameters.AddWithValue("@GFathersName", objUserInfo.GrandFatherName);
                        sqlCmd.Parameters.AddWithValue("@FatherInLaw", objUserInfo.FatherInLaw);
                        sqlCmd.Parameters.AddWithValue("@Occupation", objUserInfo.Occupation);

                        sqlCmd.Parameters.AddWithValue("@PProvince", objUserInfo.PProvinceID);
                        sqlCmd.Parameters.AddWithValue("@PDistrictID", objUserInfo.PDistrictID);
                        sqlCmd.Parameters.AddWithValue("@PDistrict", objUserInfo.PDistrictID);

                        sqlCmd.Parameters.AddWithValue("@PMunicipalityVDC", objUserInfo.PMunicipalityVDC);
                        sqlCmd.Parameters.AddWithValue("@PHouseNo", objUserInfo.PHouseNo);
                        sqlCmd.Parameters.AddWithValue("@PWardNo", objUserInfo.PWardNo);
                        sqlCmd.Parameters.AddWithValue("@PStreet", objUserInfo.PStreet);

                        sqlCmd.Parameters.AddWithValue("@CProvince", objUserInfo.CProvinceID);
                        sqlCmd.Parameters.AddWithValue("@CDistrictID", objUserInfo.CDistrictID);
                        
                        sqlCmd.Parameters.AddWithValue("@CDistrict", objUserInfo.CDistrict);

                        sqlCmd.Parameters.AddWithValue("@CMunicipalityVDC", objUserInfo.CMunicipalityVDC);
                        sqlCmd.Parameters.AddWithValue("@CHouseNo", objUserInfo.CHouseNo);
                        sqlCmd.Parameters.AddWithValue("@CWardNo", objUserInfo.CWardNo);
                        sqlCmd.Parameters.AddWithValue("@CStreet", objUserInfo.CStreet);

                        sqlCmd.Parameters.AddWithValue("@CitizenshipNo", objUserInfo.CitizenshipNo);
                        sqlCmd.Parameters.AddWithValue("@CitizenPlaceOfIssue", objUserInfo.CitizenPlaceOfIssue);
                        sqlCmd.Parameters.AddWithValue("@CitizenIssueDate", DateTime.ParseExact(objUserInfo.CitizenIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)); //null

                        sqlCmd.Parameters.AddWithValue("@PassportNo", objUserInfo.PassportNo);
                        sqlCmd.Parameters.AddWithValue("@PassportPlaceOfIssue", objUserInfo.PassportPlaceOfIssue);
                        sqlCmd.Parameters.AddWithValue("@PassportIssueDate", DateTime.ParseExact(objUserInfo.PassportIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture));
                        sqlCmd.Parameters.AddWithValue("@PassportExpiryDate", DateTime.ParseExact(objUserInfo.PassportExpiryDate, "dd/MM/yyyy", CultureInfo.InvariantCulture));

                        sqlCmd.Parameters.AddWithValue("@LicenseNo", objUserInfo.LicenseNo);
                        sqlCmd.Parameters.AddWithValue("@LicensePlaceOfIssue", objUserInfo.LicensePlaceOfIssue);
                        sqlCmd.Parameters.AddWithValue("@LicenseIssueDate", DateTime.ParseExact(objUserInfo.LicenseIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture));
                        sqlCmd.Parameters.AddWithValue("@LicenseExpiryDate", DateTime.ParseExact(objUserInfo.LicenseExpiryDate, "dd/MM/yyyy", CultureInfo.InvariantCulture));

                        sqlCmd.Parameters.AddWithValue("@PANNumber", objUserInfo.PANNumber);
                        sqlCmd.Parameters.AddWithValue("@DocType", objUserInfo.DocType);
                        sqlCmd.Parameters.AddWithValue("@IsRejected", objUserInfo.IsRejected);
                        sqlCmd.Parameters.AddWithValue("@Status", objUserInfo.Status);
                        sqlCmd.Parameters.AddWithValue("@EmailAddress", objUserInfo.EmailAddress);
                        sqlCmd.Parameters.AddWithValue("@BranchCode", objUserInfo.BranchCode);
                        
                        sqlCmd.Parameters.AddWithValue("@FrontImage", objUserInfo.FrontImage);
                        sqlCmd.Parameters.AddWithValue("@BackImage", objUserInfo.BackImage);
                        sqlCmd.Parameters.AddWithValue("@PassportImage", objUserInfo.PassportImage);
                        
                        sqlCmd.Parameters.AddWithValue("@mode", objUserInfo.Mode);

                        sqlCmd.Parameters.Add("@RegIDOut", SqlDbType.Char, 500);
                        sqlCmd.Parameters["@RegIDOut"].Direction = ParameterDirection.Output;

                        ret = sqlCmd.ExecuteNonQuery();
                        if (objUserInfo.Mode.Equals("ISR", StringComparison.InvariantCultureIgnoreCase))
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

        #region "ProvinceID"

        public string ProvinceInfo(UserValidate objUserInfo)
        {
            SqlConnection sqlCon = null;
            string ret = string.Empty;
            DataTable dt = new DataTable();
            SQLHelperQuery objDAL = new SQLHelperQuery();
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    sqlCon.Open();
                    string query = "SELECT ProvinceID FROM MNProvince WHERE Name = '" + objUserInfo.PProvince + "'";

                    using (SqlCommand sqlCmd = new SqlCommand(sqlCon.ToString()))
                    {
                        sqlCmd.CommandType = CommandType.Text;
                        dt = objDAL.AccessQueryMethod(query);
                        foreach (DataRow row in dt.Rows)
                        {
                            foreach (DataColumn column in dt.Columns)
                            {
                                ret = (row[column].ToString());
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