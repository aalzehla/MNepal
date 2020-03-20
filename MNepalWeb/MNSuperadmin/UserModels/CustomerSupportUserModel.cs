using Microsoft.Practices.EnterpriseLibrary.Data;
using MNSuperadmin.Connection;
using MNSuperadmin.InterfaceServices;
using MNSuperadmin.Models;
using MNSuperadmin.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace MNSuperadmin.UserModels
{
    public class CustomerSupportUserModel 
    {

        //FOR customer support
        //CustomerDetails
        public List<CustomerDataNew> CustomerSupportSearchUserModel(CustomerSupport Cus/*, string Approved*/)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            List<CustomerDataNew> ListRec = new List<CustomerDataNew>();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNCustSupport", conn))
                    {

                        cmd.Parameters.AddWithValue("@MobileNumber", Cus.MobileNumber); 
                        cmd.Parameters.AddWithValue("@Category", Cus.Category);  
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 0;  // seconds
                            da.Fill(dataset);
                        }
                    }
                }
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }
                if (dataset.Tables.Count > 0)
                {
                    DataTable RegCus = dataset.Tables[0];


                    if (RegCus.Rows.Count > 0)
                    {
                        foreach (DataRow dr in RegCus.Rows)
                        {
                            CustomerDataNew CusData = new CustomerDataNew();

                            CusData.SupportId = dr["SupportId"].ToString();
                            CusData.MobileNumber = dr["MobileNumber"].ToString(); 
                            CusData.Category = dr["Category"].ToString();
                            //CusData.Email = dr["Email"].ToString();
                            //CusData.Remarks = dr["Remarks"].ToString();
                           
                            //CusData.CreatedBy = dr["CreatedBy"].ToString();
                            //CusData.HasKYC = dr["HasKYC"].ToString();
                            //CusData.ClientCode = dr["ClientCode"].ToString();

                            ListRec.Add(CusData);
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
                dataset.Dispose();
            }

            return ListRec;
        }

        //for customer support

        #region  Customer Support Details
        public DataSet CustomerSupportDetails(CustomerSupport objSrInfo)
        {
            Database database;
            DataSet dtset = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNGetCustomerSupport]"))

                { 
                    database.AddInParameter(command, "@SupportId", DbType.String, objSrInfo.SupportId);
                    database.AddInParameter(command, "@mode", DbType.String, objSrInfo.Mode);

                    string[] tables = new string[] { "dtSrInfo" };
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