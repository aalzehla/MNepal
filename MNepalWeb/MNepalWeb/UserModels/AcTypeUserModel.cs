using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Microsoft.Practices.EnterpriseLibrary.Data;
using MNepalWeb.Connection;
using MNepalWeb.Models;

namespace MNepalWeb.UserModels
{
    public class AcTypeUserModel
    {
        public DataTable GetDsAcTypeInfo(AcTypeInfo objAcTypeInfo)
        {
            DataTable dtableResult = null;
            using (SqlConnection conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNAcTypeSelect]", conn))
                    {
                        cmd.Parameters.Add(new SqlParameter("@AcType", objAcTypeInfo.AcType));
                        cmd.Parameters.Add(new SqlParameter("@AcTypeDesc", objAcTypeInfo.AcTypeName));
                        cmd.Parameters.Add(new SqlParameter("@AcAllowEnquiry", objAcTypeInfo.AllowEnquiry));
                        cmd.Parameters.Add(new SqlParameter("@AcAllowTransaction", objAcTypeInfo.AllowTransaction));
                        cmd.Parameters.Add(new SqlParameter("@AcAllowAlert", objAcTypeInfo.AllowAlert));
                        cmd.Parameters.Add(new SqlParameter("@AcAllowActive", objAcTypeInfo.Active));
                        cmd.Parameters.Add(new SqlParameter("@mode", objAcTypeInfo.Mode));

                        cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.Output;
                        cmd.CommandType = CommandType.StoredProcedure;

                        using (SqlDataAdapter daAcType = new SqlDataAdapter(cmd))
                        {
                            using (DataSet dataset = new DataSet())
                            {
                                daAcType.Fill(dataset, "dtAcTypeInfo");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables["dtAcTypeInfo"];
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
            }
            return dtableResult;
        }

        public DataTable GetAccountInformation(AcTypeInfo objacInfo)
        {
            Database database;
            DataTable dtableResult = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNAcTypeSelect]"))
                {
                    database.AddInParameter(command, "@AcType", DbType.String, objacInfo.AcType);
                    database.AddInParameter(command, "@AcTypeDesc", DbType.String, objacInfo.AcTypeName);
                    database.AddInParameter(command, "@AcAllowEnquiry", DbType.String, objacInfo.AllowEnquiry);
                    database.AddInParameter(command, "@AcAllowTransaction", DbType.String, objacInfo.AllowTransaction);
                    database.AddInParameter(command, "@AcAllowAlert", DbType.String, objacInfo.AllowAlert);
                    database.AddInParameter(command, "@AcAllowActive", DbType.String, objacInfo.Active);
                    database.AddInParameter(command, "@mode", DbType.String, objacInfo.Mode);
                    database.AddOutParameter(command, "@ReturnValue", DbType.Int32, 3);

                    using (var dataset = new DataSet())
                    {
                        database.LoadDataSet(command, dataset, "dtAcTypeInfo");

                        if (dataset.Tables.Count > 0)
                        {
                            dtableResult = dataset.Tables["dtAcTypeInfo"];
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

        public int UpdateAccountType(AcTypeInfo objacInfo)
        {
            Database database;
            int ret;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNAcTypeSelect]"))
                {
                    database.AddInParameter(command, "@AcType", DbType.String, objacInfo.AcType);
                    database.AddInParameter(command, "@AcTypeDesc", DbType.String, objacInfo.AcTypeName);
                    database.AddInParameter(command, "@AcAllowEnquiry", DbType.String, objacInfo.AllowEnquiry);
                    database.AddInParameter(command, "@AcAllowTransaction", DbType.String, objacInfo.AllowTransaction);
                    database.AddInParameter(command, "@AcAllowAlert", DbType.String, objacInfo.AllowAlert);
                    database.AddInParameter(command, "@AcAllowActive", DbType.String, objacInfo.Active);
                    database.AddInParameter(command, "@mode", DbType.String, objacInfo.Mode);
                    database.AddOutParameter(command, "@ReturnValue", DbType.Int32, 3);

                    ret = database.ExecuteNonQuery(command);
                    if (objacInfo.Mode.Equals("UAD", StringComparison.InvariantCultureIgnoreCase))
                    {
                        ret = (int)database.GetParameterValue(command, "@ReturnValue");
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


        #region "Create Account Type Information"

        public int AcTypeInfo(AcTypeInfo objActypeInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNAcTypeSelect]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@AcType", objActypeInfo.AcType));
                        sqlCmd.Parameters.Add(new SqlParameter("@AcTypeDesc", objActypeInfo.AcTypeName));
                        sqlCmd.Parameters.Add(new SqlParameter("@AcAllowEnquiry", objActypeInfo.AllowEnquiry));
                        sqlCmd.Parameters.Add(new SqlParameter("@AcAllowTransaction", objActypeInfo.AllowTransaction));
                        sqlCmd.Parameters.Add(new SqlParameter("@AcAllowAlert", objActypeInfo.AllowAlert));
                        sqlCmd.Parameters.Add(new SqlParameter("@AcAllowActive", objActypeInfo.Active));
                        sqlCmd.Parameters.Add(new SqlParameter("@mode", objActypeInfo.Mode));

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

        #region "GET Checking AcType"
        public DataTable GetAcTypeInfo(AcTypeInfo objAcTypeInfo)
        {
            DataTable dtableResult = null;
            SqlConnection conn = null;

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNAcTypeSelect]", conn))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@AcType", objAcTypeInfo.AcType));
                        sqlCmd.Parameters.Add(new SqlParameter("@AcTypeDesc", objAcTypeInfo.AcTypeName));
                        sqlCmd.Parameters.Add(new SqlParameter("@AcAllowEnquiry", objAcTypeInfo.AllowEnquiry));
                        sqlCmd.Parameters.Add(new SqlParameter("@AcAllowTransaction", objAcTypeInfo.AllowTransaction));
                        sqlCmd.Parameters.Add(new SqlParameter("@AcAllowAlert", objAcTypeInfo.AllowAlert));
                        sqlCmd.Parameters.Add(new SqlParameter("@AcAllowActive", objAcTypeInfo.Active));
                        sqlCmd.Parameters.Add(new SqlParameter("@mode", objAcTypeInfo.Mode));
                        sqlCmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.Output;


                        using (SqlDataAdapter da = new SqlDataAdapter(sqlCmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 60;  // seconds
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset, "dtAcTypeInfo");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables["dtAcTypeInfo"];
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

    }
}