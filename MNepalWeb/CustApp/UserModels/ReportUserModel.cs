using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using CustApp.ViewModel;
using System.Data.SqlClient;
using CustApp.Models;
using CustApp.Connection;
using System.Globalization;
using CustApp.Utilities;

namespace CustApp.UserModels
{
    public class ReportUserModel
    {        


       
        /// <summary>
        /// end 111
        /// </summary>
        /// <param name="CategoryId"></param>
        /// <returns></returns>
        //IMPPP
        public List<MerchantList> GetMerchantListbyCategory(string CategoryId)
        {
            SqlDataReader rdr;
            SqlConnection conn = null;
            List<MerchantList> Merchants = new List<MerchantList>();
            try
            {
                string Query = @"SELECT M.Id AS CategoryId,M.Name AS Category ,Ms.mname  as [MerchantName] ,Ms.mid  AS MerchantId  FROM 
                                 dbo.MNMerchants Ms LEFT OUTER JOIN dbo.MNMerchant M ON M.Id = Ms.catid
								 WHERE M.Id=@CategoryId
								 ORDER BY MerchantName ASC";
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Connection = conn;
                        cmd.Parameters.AddWithValue("@CategoryId", CategoryId);
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            MerchantList merchant = new MerchantList();
                            merchant.MId = rdr["MerchantId"].ToString();
                            merchant.MName = rdr["MerchantName"].ToString();
                            Merchants.Add(merchant);

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
            return Merchants;
        }
        
        public List<MNAgentStatement> GetAgentStatement(string ContactNumber1, string StartDate, string EndDate, string TranID)
        {
            SqlConnection conn = null;
            List<MNAgentStatement> AdminActivities = new List<MNAgentStatement>();
            DataSet dataset = new DataSet();
            try
            {

                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "s_MNAgentMiniStmt";
                        cmd.Parameters.AddWithValue("@ContactNumber1", ContactNumber1);                        
                        cmd.Parameters.AddWithValue("@StartDate", StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", EndDate);
                        cmd.Parameters.AddWithValue("@TranID", TranID);


                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = conn;

                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {

                            da.SelectCommand.CommandTimeout = 0;  // seconds
                            da.Fill(dataset);
                        }
                        if (conn.State != ConnectionState.Closed)
                        {
                            conn.Close();
                        }
                        if (dataset.Tables.Count > 0)
                        {
                            AdminActivities = ExtraUtility.DatatableToListClass<MNAgentStatement>(dataset.Tables[0]);
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
            return AdminActivities;

        }
        
        public List<CustomerAccActivity> CustomerAccountActivityNew(CustReport Cus)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            DataTable AcActivity = new DataTable();
            List<CustomerAccActivity> ListRec = new List<CustomerAccActivity>();
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNCustomerMiniStmtTest", conn))
                    {
                        cmd.Parameters.AddWithValue("@ContactNumber1", Cus.UserName);
                        cmd.Parameters.AddWithValue("@StartDate", Cus.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", Cus.EndDate);
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

                AcActivity = dataset.Tables[0];
                //  DataTable Balance = dataset.Tables[1];

                if (AcActivity.Rows.Count > 0)
                {
                    foreach (DataRow dr in AcActivity.Rows)
                    {
                        CustomerAccActivity cuss = new CustomerAccActivity();
                        cuss.TranId = dr["TranID"].ToString();
                        //cuss.Date = DateTime.ParseExact(dr["Date"].ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture); //hh:mm:ss.fff tt
                         cuss.TimeStamp = dr["TimeStamp"].ToString();
                        cuss.Desc1 = dr["Description"].ToString();
                        cuss.Debit = decimal.Parse(dr["Debit"].ToString().Trim() == "" ? "0" : dr["Debit"].ToString().Trim());
                        cuss.Credit = decimal.Parse(dr["Credit"].ToString().Trim() == "" ? "0" : dr["Credit"].ToString().Trim());
                        cuss.Balance = dr["Balance"].ToString();
                        cuss.Status = dr["Status"].ToString();
                        ListRec.Add(cuss);
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
        
        public List<CustomerAccActivity> CustomerAccountActivityNewBnkAcc(CustReport Cus)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            DataTable AcActivity = new DataTable();
            List<CustomerAccActivity> ListRec = new List<CustomerAccActivity>();
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNCustomerBankStmtTest", conn))
                    {
                        cmd.Parameters.AddWithValue("@ContactNumber1", Cus.UserName);
                        cmd.Parameters.AddWithValue("@StartDate", Cus.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", Cus.EndDate);
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

                AcActivity = dataset.Tables[0];
                //  DataTable Balance = dataset.Tables[1];

                if (AcActivity.Rows.Count > 0)
                {
                    foreach (DataRow dr in AcActivity.Rows)
                    {
                        CustomerAccActivity cuss = new CustomerAccActivity();
                        cuss.TranId = dr["TranID"].ToString();
                        //cuss.Date = DateTime.ParseExact(dr["Date"].ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture); //hh:mm:ss.fff tt
                       // cuss.Date = dr["Date"].ToString();
                        cuss.TimeStamp = dr["TimeStamp"].ToString();
                        cuss.Desc1 = dr["Description"].ToString();
                        cuss.Debit = decimal.Parse(dr["Debit"].ToString().Trim() == "" ? "0" : dr["Debit"].ToString().Trim());
                        cuss.Credit = decimal.Parse(dr["Credit"].ToString().Trim() == "" ? "0" : dr["Credit"].ToString().Trim());
                        cuss.Balnc = dr["Balance"].ToString();
                        cuss.Status = dr["Status"].ToString();
                        ListRec.Add(cuss);
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

        public List<CashOutList> GetCashOutListApproved(CustReport Cus)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            DataTable AcActivity = new DataTable();
            List<CashOutList> ListRec = new List<CashOutList>();
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNCashOutList", conn))
                    {
                        cmd.Parameters.AddWithValue("@SenderMobileNo", Cus.UserName);
                        cmd.Parameters.AddWithValue("@Status", Cus.Status);
                        //cmd.Parameters.AddWithValue("@EndDate", Cus.EndDate);
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

                AcActivity = dataset.Tables[0];
                //  DataTable Balance = dataset.Tables[1];

                if (AcActivity.Rows.Count > 0)
                {
                    foreach (DataRow dr in AcActivity.Rows)
                    {
                        CashOutList cuss = new CashOutList();
                        cuss.TokenID = dr["TokenID"].ToString();
                        cuss.Amount = dr["Amount"].ToString();
                        cuss.CreatedDate = dr["TokenCreatedDate"].ToString().Split()[0];

                        //cuss.TokenCreatedDate = DateTime.Parse(cuss.CreatedDate);
                        //cuss.CreatedDate = cuss.TokenCreatedDate.ToString("yyyy/MM/dd");

                        cuss.ExpiryDate = dr["TokenExpiryDate"].ToString();

                        //cuss.ExpiryDate = dr["TokenExpiryDate"].ToString().Split()[0];

                        //string time = dr["TokenExpiryDate"].ToString().Split()[1] + " " + dr["TokenExpiryDate"].ToString().Split()[2];

                        //cuss.TokenExpiryDate = DateTime.Parse(cuss.ExpiryDate);
                        //cuss.ExpiryDate = cuss.TokenExpiryDate.ToString("yyyy/MM/dd") + " " + time;

                        ListRec.Add(cuss);
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

    }
}