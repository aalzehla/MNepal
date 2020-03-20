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
    public class SAdminUserModel
    {

        public int SAdminRegisterUserInfo(UserInfo objUserInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNSAdminRegistration]", sqlCon))
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
                        sqlCmd.Parameters.AddWithValue("@Password", objUserInfo.Password);
                        sqlCmd.Parameters.AddWithValue("@UserType", objUserInfo.UserType);
                        //sqlCmd.Parameters.AddWithValue("@ProfileName", objUserInfo.UserGroup);
                        sqlCmd.Parameters.AddWithValue("@BranchCode", objUserInfo.BranchCode);
                        sqlCmd.Parameters.AddWithValue("@PushSMS", objUserInfo.PushSMS);
                        sqlCmd.Parameters.AddWithValue("@COC", objUserInfo.COC);
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
     public int SAdminUpdateUserInfo(UserInfo objUserInfo)
    {


            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNSAdminInfoUpdate]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@ClientCode", objUserInfo.ClientCode));
                        sqlCmd.Parameters.Add(new SqlParameter("@Name", objUserInfo.Name));
                        sqlCmd.Parameters.Add(new SqlParameter("@EmailAddress", objUserInfo.EmailAddress));
                        
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

    }
}