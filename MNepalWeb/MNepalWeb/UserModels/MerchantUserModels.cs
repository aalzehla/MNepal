using Microsoft.Practices.EnterpriseLibrary.Data;
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
    public class MerchantUserModels
    {
        /// <summary>
        /// Retrieve the merchant detail information based on mode
        /// </summary>
        /// <param name="objMerchantInfo">Pass an instance of User information</param>
        /// <returns>Returns the merchant detail information based on merchant information</returns>
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
    }
}