using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using ThailiMNepalAgent.Connection;
using System.Data.SqlClient;
using ThailiMNepalAgent.Models;

namespace ThailiMNepalAgent.UserModels
{
    public class PaypointUserModel
    {
        #region NEA
        public Dictionary<string, string> GetNEAName()
        {
            SqlDataReader rdr;
            SqlConnection conn = null;
            Dictionary<string, string> ListNEA = new Dictionary<string, string>();
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SELECT * FROM MNNEALocation (NOLOCK) order by NEABranchName ASC ";
                        cmd.Connection = conn;
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            string NEAName = rdr["NEABranchName"].ToString();
                            string NEABranch = rdr["NEABranchCode"].ToString();
                            ListNEA.Add(NEAName, NEABranch);
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
            return ListNEA;
        }
        #endregion
        
        #region Khanepani
        public Dictionary<string, string> GetKhanepaniName()
        {
            SqlDataReader rdr;
            SqlConnection conn = null;
            Dictionary<string, string> ListKhanepani = new Dictionary<string, string>();
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SELECT * FROM MNKhanepaniLocation (NOLOCK) order by KpBranchName ASC ";
                        cmd.Connection = conn;
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            string NEAName = rdr["KpBranchName"].ToString();
                            string NEABranch = rdr["KpBranchCode"].ToString();
                            //string NEATypeId = rdr["NeaID"].ToString();
                            ListKhanepani.Add(NEAName, NEABranch);
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
            return ListKhanepani;
        }
        #endregion

        #region Nepal Water
        public Dictionary<string, string> GetNepalWaterName()
        {
            SqlDataReader rdr;
            SqlConnection conn = null;
            Dictionary<string, string> ListNepalWater = new Dictionary<string, string>();
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SELECT * FROM MNNepalWaterLocation (NOLOCK) order by NwBranchName ASC ";
                        cmd.Connection = conn;
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            string NwBranchName = rdr["NwBranchName"].ToString();
                            string NwBranchCode = rdr["NwBranchCode"].ToString();
                            //string NEATypeId = rdr["NeaID"].ToString();
                            ListNepalWater.Add(NwBranchName, NwBranchCode);
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
            return ListNepalWater;
        }
        #endregion

        public DataSet GetNEAPaymentDetails(NEAFundTransfer objUserInfo)
        {
            Database database;
            DataSet dtset = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNPaypoints]"))
                {
                    database.AddInParameter(command, "@SCNo", DbType.String, objUserInfo.SCNo);
                    database.AddInParameter(command, "@NEABranchName", DbType.String, objUserInfo.NEABranchCode);
                    database.AddInParameter(command, "@CustomerID", DbType.String, objUserInfo.CustomerID);
                    database.AddInParameter(command, "@ClientCode", DbType.String, objUserInfo.ClientCode);
                    database.AddInParameter(command, "@UserName", DbType.String, objUserInfo.UserName);
                    database.AddInParameter(command, "@mode", DbType.String, objUserInfo.Mode);
                    database.AddInParameter(command, "@KhanepaniCounter", DbType.String, null);
                    database.AddInParameter(command, "@refStan", DbType.String, objUserInfo.refStan);
                    string[] tables = new string[] { "dtResponse", "dtPayment"};
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

        public DataSet GetKPPaymentDetails(Khanepani objUserInfo)
        {
            Database database;
            DataSet dtset = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNPaypoints]"))
                {
                    database.AddInParameter(command, "@KhanepaniCounter", DbType.String, objUserInfo.KhanepaniCounter);
                    database.AddInParameter(command, "@CustomerID", DbType.String, objUserInfo.CustomerID);
                    database.AddInParameter(command, "@ClientCode", DbType.String, objUserInfo.ClientCode);
                    database.AddInParameter(command, "@UserName", DbType.String, objUserInfo.UserName);
                    database.AddInParameter(command, "@mode", DbType.String, objUserInfo.Mode);
                    database.AddInParameter(command, "@SCNo", DbType.String, null);
                    database.AddInParameter(command, "@NEABranchName", DbType.String, null);
                    database.AddInParameter(command, "@refStan", DbType.String, objUserInfo.refStan);
                    string[] tables = new string[] { "dtResponse", "dtKhanepaniInvoice" };

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

        public DataSet GetNWPaymentDetails(NepalWater objUserInfo)
        {
            Database database;
            DataSet dtset = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNPaypoints]"))
                {
                    database.AddInParameter(command, "@KhanepaniCounter", DbType.String, objUserInfo.NWCounter);
                    database.AddInParameter(command, "@CustomerID", DbType.String, objUserInfo.CustomerID);
                    database.AddInParameter(command, "@ClientCode", DbType.String, objUserInfo.ClientCode);
                    database.AddInParameter(command, "@UserName", DbType.String, objUserInfo.UserName);
                    database.AddInParameter(command, "@mode", DbType.String, objUserInfo.Mode);
                    database.AddInParameter(command, "@SCNo", DbType.String, null);
                    database.AddInParameter(command, "@NEABranchName", DbType.String, null);
                    database.AddInParameter(command, "@refStan", DbType.String, objUserInfo.refStan);
                    string[] tables = new string[] { "dtResponse", "dtNWPayment" };

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

        #region Wlink

        public DataSet GetWlinkPaymentDetails(ISP objUserInfo)
        {
            Database database;
            DataSet dtset = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNPaypoints]"))
                {
                    database.AddInParameter(command, "@KhanepaniCounter", DbType.String, null);
                    database.AddInParameter(command, "@CustomerID", DbType.String, objUserInfo.CustomerID);
                    database.AddInParameter(command, "@ClientCode", DbType.String, objUserInfo.ClientCode);
                    database.AddInParameter(command, "@UserName", DbType.String, objUserInfo.UserName);
                    database.AddInParameter(command, "@mode", DbType.String, objUserInfo.Mode);
                    database.AddInParameter(command, "@SCNo", DbType.String, null);
                    database.AddInParameter(command, "@NEABranchName", DbType.String, null);
                    database.AddInParameter(command, "@refStan", DbType.String, objUserInfo.refStan);
                    string[] tables = new string[] { "dtResponse", "dtNWPayment" };

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

        #region Subisu

        public DataSet GetSubisuPaymentDetails(ISP objUserInfo)
        {
            Database database;
            DataSet dtset = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNPaypoints]"))
                {
                    database.AddInParameter(command, "@KhanepaniCounter", DbType.String, null);
                    database.AddInParameter(command, "@CustomerID", DbType.String, objUserInfo.CustomerID);
                    database.AddInParameter(command, "@ClientCode", DbType.String, objUserInfo.ClientCode);
                    database.AddInParameter(command, "@UserName", DbType.String, objUserInfo.UserName);
                    database.AddInParameter(command, "@mode", DbType.String, objUserInfo.Mode);
                    database.AddInParameter(command, "@SCNo", DbType.String, null);
                    database.AddInParameter(command, "@NEABranchName", DbType.String, null);
                    database.AddInParameter(command, "@refStan", DbType.String, objUserInfo.refStan);
                    string[] tables = new string[] { "dtResponse", "dtSubisuPayment" };

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

    }
}