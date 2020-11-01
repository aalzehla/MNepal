using ThailiMNepalAgent.Connection;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ThailiMNepalAgent.Models
{
    public class DMATUserModel
    {
        #region DMAT
        public Dictionary<string, string> GetDMATName()
        {
            SqlDataReader rdr;
            SqlConnection conn = null;
            Dictionary<string, string> ListDMAT = new Dictionary<string, string>();
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "select * from MNDemat (NOLOCK) order by DName ASC";
                        cmd.Connection = conn;
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            string DMATCode = rdr["DCode"].ToString();
                            string DMATName = rdr["DName"].ToString();
                            //string NEATypeId = rdr["NeaID"].ToString();
                            ListDMAT.Add(DMATCode, DMATName);
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
            return ListDMAT;
        }
        #endregion

        public DataSet GetDematPaymentDetails(DMAT objUserInfo)
        {
            Database database;
            DataSet dtset = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNDematRequest]"))
                {
                    database.AddInParameter(command, "@BoId", DbType.String, null);
                    database.AddInParameter(command, "@DematName", DbType.String, null);
                    database.AddInParameter(command, "@ClientCode", DbType.String, objUserInfo.ClientCode);
                    database.AddInParameter(command, "@UserName", DbType.String, objUserInfo.UserName);
                    database.AddInParameter(command, "@mode", DbType.String, objUserInfo.Mode);
                    database.AddInParameter(command, "@Fees", DbType.String, null);
                    database.AddInParameter(command, "@TotalAmount", DbType.String, null);
                    database.AddInParameter(command, "@RetrievalRef", DbType.String, objUserInfo.refStan);
                    database.AddInParameter(command, "@BankCode", DbType.String, null);
                    database.AddInParameter(command, "@TimeStamp", DbType.String, null);
                    database.AddInParameter(command, "@MsgStr", DbType.String, null);


                    string[] tables = new string[] { "dtResponse", "dtPayment" };
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

    }
}