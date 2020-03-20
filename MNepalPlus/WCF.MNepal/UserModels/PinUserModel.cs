using MNepalProject.Connection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using WCF.MNepal.Models;
using WCF.MNepal.Utilities;

namespace WCF.MNepal.UserModels
{
    public class PinUserModel
    {
        public DataTable GetPinInformation(PinChange objUserInfo)
        {
            SqlConnection conn = null;
            DataTable dtableResult = null;

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNPinCheck]", conn))
                    {
                        cmd.Parameters.AddWithValue("@SourceMobileNo", objUserInfo.mobile);
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset, "dtPinInfo");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables["dtPinInfo"];
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
                conn.Close();
            }

            return dtableResult;
        }

        //reset pin
        public int ResetPinInformation(PinChange objUserInfo)
        {
            SqlConnection conn = null;
            DataTable dtableResult = null;
            string generatePin = PinUtils.GeneratePin();
            int ret;

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNResetPin]", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@UserName", objUserInfo.mobile);
                        cmd.Parameters.AddWithValue("@ClientCode", objUserInfo.ClientCode);
                        cmd.Parameters.AddWithValue("@PIN", generatePin);
                        cmd.Parameters.AddWithValue("@Password", "");
                        cmd.Parameters.AddWithValue("@userType", "");
                        cmd.Parameters.AddWithValue("@ProfileName", "");
                        cmd.Parameters.AddWithValue("@PushSMS", "");
                        cmd.Parameters.AddWithValue("@COC", "");
                        cmd.Parameters.AddWithValue("@IMEINo", "");
                        cmd.Parameters.AddWithValue("@Mode", "");
                        cmd.ExecuteNonQuery();

                        ret =Convert.ToInt32(generatePin);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                conn.Close();
            }

            return ret;
        }
    }
}