using Microsoft.Practices.EnterpriseLibrary.Data;
using MNSuperadmin.Connection;
using MNSuperadmin.Controllers;
using MNSuperadmin.Models;
using MNSuperadmin.Utilities;
using MNSuperadmin.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;

namespace MNSuperadmin.UserModels
{
    public class ReportUserModel
    {
        public static DataTable GetAgentDetail(string MobileNumber, string StartDate, string EndDate)
        {
            Database database;
            DataTable dtableResult = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNRptRegisterAgentNew]"))
                {
                   
                    database.AddInParameter(command, "@ContactNumber1", DbType.String, MobileNumber);
                    database.AddInParameter(command, "@StartDate", DbType.String, StartDate);
                    database.AddInParameter(command, "@EndDate", DbType.String, EndDate);
                    using (var dataset = new DataSet())
                    {
                        database.LoadDataSet(command, dataset, "dtUserInfo");

                        if (dataset.Tables.Count > 0)
                        {
                            dtableResult = dataset.Tables["dtUserInfo"];
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

        public List<NonFinancialReport> GetNonFinancialReport(string ContactNumber1)
        {
            SqlConnection conn = null;
            List<NonFinancialReport> NonFinancialReport = new List<NonFinancialReport>();
            DataSet dataset = new DataSet();
            try
            {

                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "s_MNReportNonFinancial";
                        cmd.Parameters.AddWithValue("@ContactNumber1", ContactNumber1);
                       
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
                            NonFinancialReport = ExtraUtility.DatatableToListClass<NonFinancialReport>(dataset.Tables[0]);
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
            return NonFinancialReport;

        }

        public List<FinancialReportActivity> GetFinancialReportActivity(FinancialReport financialRepObj)
        {
            SqlConnection conn = null;
            DataTable FinAcActivity = new DataTable();
            List<FinancialReportActivity> FinancialReport = new List<FinancialReportActivity>();
            DataSet dataset = new DataSet();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "s_MNReportFinancial";
                        cmd.Parameters.AddWithValue("@ContactNumber1", financialRepObj.MobileNumber);
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
                    }
                }
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }

                FinAcActivity = dataset.Tables[0];
                if (FinAcActivity.Rows.Count > 0)
                {
                    foreach (DataRow dr in FinAcActivity.Rows)
                    {
                        FinancialReportActivity cuss = new FinancialReportActivity();
                        cuss.Date = dr["Date"].ToString();
                        cuss.CustomerName = dr["CustomerName"].ToString();
                        cuss.MobileNumber = dr["MobileNumber"].ToString();
                        cuss.TargetNumber = dr["TargetNumber"].ToString();
                        cuss.TranID = dr["TranID"].ToString();
                        cuss.Amount = String.Format("{0:0.00}", dr["Amount"]);
                        cuss.TransactionType = dr["TransactionType"].ToString();
                        cuss.Status = dr["Status"].ToString();
                        cuss.Message = dr["Message"].ToString();

                        FinancialReport.Add(cuss);
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
            return FinancialReport;
        }
        
        public List<TransactionReport> GetTransactionReport(string ContactNumber1, MerchantPay Mpay)
        {
            SqlConnection conn = null;
            List<TransactionReport> TransactionReport = new List<TransactionReport>();
            DataSet dataset = new DataSet();
            try
            {

                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "s_MNReportTransaction";
                        cmd.Parameters.AddWithValue("@ContactNumber1", ContactNumber1);
                        cmd.Parameters.AddWithValue("@StartDate", Mpay.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", Mpay.EndDate);
                        cmd.Parameters.AddWithValue("@ServiceType", Mpay.Service);
                        cmd.Parameters.AddWithValue("@GroupBy", Mpay.GrpByDate);

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
                            TransactionReport = ExtraUtility.DatatableToListClass<TransactionReport>(dataset.Tables[0]);
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
            return TransactionReport;

        }
        
        //CustomerDetails
        public List<CustomerData> CustomerDetails(CustReport Cus/*, string Approved*/)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            List<CustomerData> ListRec = new List<CustomerData>();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    //using (SqlCommand cmd = new SqlCommand("dbo.s_MNReportWalletCustStatus", conn))
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNRptWalletCustStatusSARegistered", conn))

                    {
                        //cmd.Parameters.AddWithValue("@ProfileCode", Cus.ProfileCode);
                        //cmd.Parameters.AddWithValue("@StartDate", Cus.StartDate);
                        //cmd.Parameters.AddWithValue("@EndDate", Cus.EndDate);

                        cmd.Parameters.AddWithValue("@UserName", Cus.MobileNo);
                        cmd.Parameters.AddWithValue("@CustName", Cus.CustomerName);
                        cmd.Parameters.AddWithValue("@HasKYC", Cus.HasKYC);
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
                            CustomerData CusData = new CustomerData();
                            CusData.MobileNo = dr["MobileNumber"].ToString();
                            CusData.CustomerName = dr["CustomerName"].ToString();
                            CusData.CreatedBy = dr["CreatedBy"].ToString(); 
                            CusData.HasKYC = dr["HasKYC"].ToString();
                            CusData.ClientCode = dr["ClientCode"].ToString();  
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
        
        //start 
        public List<CustomerData> AgentCommissionDetail(CustReport Cus/*, string Approved*/)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            List<CustomerData> ListRec = new List<CustomerData>();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNGetAgentCommissionDetail", conn))
                    {
                     
                        cmd.Parameters.AddWithValue("@FeeId", Cus.Id);
                        //cmd.Parameters.AddWithValue("@CustName", Cus.CustomerName);
                        //cmd.Parameters.AddWithValue("@HasKYC", Cus.HasKYC);
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
                            CustomerData CusData = new CustomerData();
                            CusData.Id = dr["Id"].ToString();
                            CusData.FeeId = dr["FeeId"].ToString();
                            CusData.TieredStart = dr["TieredStart"].ToString();

                            CusData.TieredEnd = dr["TieredEnd"].ToString();
                            CusData.MinAmt = dr["MinAmt"].ToString();
                            CusData.MaxAmt = dr["MaxAmt"].ToString();

                            CusData.Percentage = dr["Percentage"].ToString();
                            CusData.FlatFee = dr["FlatFee"].ToString(); 
                            CusData.FeeType = dr["FeeType"].ToString();

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
        public List<CustomerData> AgentCommissionRejectedDetail(CustReport Cus/*, string Approved*/)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            List<CustomerData> ListRec = new List<CustomerData>();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNAgentCommissionRejectedDetail", conn))
                    {
                     
                        cmd.Parameters.AddWithValue("@FeeId", Cus.FeeId);
                        //cmd.Parameters.AddWithValue("@CustName", Cus.CustomerName);
                        //cmd.Parameters.AddWithValue("@HasKYC", Cus.HasKYC);
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
                            CustomerData CusData = new CustomerData();
                            CusData.Id = dr["Id"].ToString();
                            CusData.FeeId = dr["FeeId"].ToString();
                            CusData.TieredStart = dr["TieredStart"].ToString();

                            CusData.TieredEnd = dr["TieredEnd"].ToString();
                            CusData.MinAmt = dr["MinAmt"].ToString();
                            CusData.MaxAmt = dr["MaxAmt"].ToString();

                            CusData.Percentage = dr["Percentage"].ToString();
                            CusData.FlatFee = dr["FlatFee"].ToString(); 
                            CusData.FeeType = dr["FeeType"].ToString();

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
        
        #region Get SuperAdmin Activities

      
        public List<MNAdminActivity> GetSuperAdminActivity(string UserName, string BranchCode, string StartDate, string EndDate)
        {
            SqlConnection conn = null;
            List<MNAdminActivity> AdminActivities = new List<MNAdminActivity>();
            DataSet dataset = new DataSet();
            try
            {

                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "s_GetMNSuperAdminActivity";
                        cmd.Parameters.AddWithValue("@UserName", UserName);
                        cmd.Parameters.AddWithValue("@BranchCode", BranchCode);
                        cmd.Parameters.AddWithValue("@StartDate", StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", EndDate);
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
                            AdminActivities = ExtraUtility.DatatableToListClass<MNAdminActivity>(dataset.Tables[0]);
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
        #endregion
        //end
        #region Get Customer Login Activities


        public List<CustomerLoginLog> GetCustomerLoginActivities(string UserName, string StartDate, string EndDate)
        {
            SqlConnection conn = null;
            List<CustomerLoginLog> AdminActivities = new List<CustomerLoginLog>();
            DataSet dataset = new DataSet();
            try
            {

                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "[s_MNGetCustRegLog]";
                        cmd.Parameters.AddWithValue("@UserName", UserName);
                        cmd.Parameters.AddWithValue("@StartDate", StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", EndDate);
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
                            AdminActivities = ExtraUtility.DatatableToListClass<CustomerLoginLog>(dataset.Tables[0]);
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
        #endregion

        public List<CustomerLog> CustomerAccountLog(CustReport Cus)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            List<CustomerLog> ListRec = new List<CustomerLog>();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNGetCustLog", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserName", Cus.UserName);
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

                DataTable CusLog = dataset.Tables[0];

                if (CusLog.Rows.Count > 0)
                {
                    foreach (DataRow dr in CusLog.Rows)
                    {
                        CustomerLog CLog = new CustomerLog();
                        CLog.ServiceType = dr["ServiceType"].ToString();
                        CLog.Source = dr["Source"].ToString();
                        //CLog.Sender = dr["Sender"].ToString();
                        //CLog.SourceAccountNo = dr["SourceAccountNo"].ToString();
                        CLog.Destination = dr["Destination"].ToString();
                        CLog.DestinationAccount = dr["DestinationAccount"].ToString();
                        CLog.Reciever = dr["Reciever"].ToString();
                        CLog.Amount = decimal.Parse(dr["Amount"].ToString().Trim() == "" ? "0" : dr["Amount"].ToString().Trim());
                        CLog.TranDate = DateTime.ParseExact(dr["TranDate"].ToString(), "dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                        // CLog.TraceNo = dr["TraceNo"].ToString();
                        CLog.ResponseCode = dr["ResponseCode"].ToString();
                        CLog.Description = dr["Description"].ToString();
                        ListRec.Add(CLog);
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

        public List<Merchants> GetMerchantsType()
        {
            SqlDataReader rdr;
            SqlConnection conn = null;
            List<Merchants> ListMerchants = new List<Merchants>();
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SELECT Name FROM MNMerchant (NOLOCK) ";
                        cmd.Connection = conn;
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            Merchants merchant = new Merchants();
                            merchant.Name = rdr["Name"].ToString();
                            ListMerchants.Add(merchant);
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
        //for service type
        public string GetMerchantsFromType(string Value, bool AddWhere)
        {
            SqlDataReader rdr;
            SqlConnection conn = null;
            string Option = string.Empty;
            try
            {
                string Query = @"SELECT M.Name ,Ms.mname  as [MerchantName] FROM 
                                 dbo.MNMerchants Ms LEFT OUTER JOIN dbo.MNMerchant M ON M.Id = Ms.catid";

                if (AddWhere)
                {
                    Query = Query + string.Format(" Where M.Name='{0}'", Value);
                }
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Connection = conn;
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            Option = Option + string.Format("<option value='{0}'>{1}</option>", rdr["MerchantName"].ToString(), rdr["MerchantName"].ToString());
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
            return Option;
        }

        public List<MerchantInfo> SummaryDetails(MerchantPay Mpay)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            List<MerchantInfo> ListRec = new List<MerchantInfo>();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNSummaryRpt", conn))
                    {

                        cmd.Parameters.AddWithValue("@StartDate", Mpay.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", Mpay.EndDate);
                        cmd.Parameters.AddWithValue("@ServiceType", Mpay.Service);
                        cmd.Parameters.AddWithValue("@MerchantName", Mpay.MerchantName);
                        cmd.Parameters.AddWithValue("@GroupBy", Mpay.GrpByDate);
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
                    DataTable TpData = dataset.Tables[0];
                    if (TpData.Rows.Count > 0)
                    {
                        foreach (DataRow dr in TpData.Rows)
                        {
                            MerchantInfo MI = new MerchantInfo();
                            //MI.StartDate = dr["From"].ToString();
                            //MI.EndDate = dr["To"].ToString();
                            //MI.MerchantType = dr["Merchant Type"].ToString();
                            MI.MerchantName = dr["MerchantName"].ToString();
                            MI.ServiceType = dr["ServiceType"].ToString();
                            MI.TotalAmount = decimal.Parse(dr["TotalAmount"].ToString().Trim() == "" ? "0" : dr["TotalAmount"].ToString().Trim());
                            MI.NoOfTran = Convert.ToInt32(dr["NoOfTran"].ToString().Trim() == "" ? "0" : dr["NoOfTran"].ToString().Trim());
                            //MI.Sales = dr["Sales"].ToString();
                            MI.SalesYear = dr["SalesYear"].ToString();
                            MI.SalesMonth = dr["SalesMonth"].ToString();

                            ListRec.Add(MI);
                        }

                        HttpContext.Current.Session["MerchantInfo"] = TpData;
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

        //Summary details list
        public List<MerchantInfo> SummaryDetailList(MerchantPay Mpay)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            List<MerchantInfo> ListRec = new List<MerchantInfo>();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNSummaryDetailRpt", conn))
                    {
                        cmd.Parameters.AddWithValue("@StartDate", Mpay.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", Mpay.EndDate);
                        cmd.Parameters.AddWithValue("@ServiceType", Mpay.Service);
                        cmd.Parameters.AddWithValue("@MerchantName", Mpay.MerchantName);
                        cmd.Parameters.AddWithValue("@SalesYear", Mpay.SalesYear);
                        cmd.Parameters.AddWithValue("@SalesMonth", Mpay.SalesMonth);
                        cmd.Parameters.AddWithValue("@GroupedBy", Mpay.GrpByDate);


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
                    DataTable TpData = dataset.Tables[0];
                    if (TpData.Rows.Count > 0)
                    {
                        foreach (DataRow dr in TpData.Rows)
                        {
                            MerchantInfo MI = new MerchantInfo();
                            MI.TranDate = dr["TranDate"].ToString();
                            MI.TxnID = dr["TranID"].ToString();
                            MI.InitMobileNo = dr["Initiator Mobile No"].ToString();
                            MI.ServiceType = dr["ServiceType"].ToString();
                            MI.MerchantName = dr["MerchantName"].ToString();
                            MI.TotalAmount = decimal.Parse(dr["Amount"].ToString().Trim() == "" ? "0" : dr["Amount"].ToString().Trim());
                            ListRec.Add(MI);
                        }

                        HttpContext.Current.Session["MerchantInfo"] = TpData;
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

        //QueryExecutor
        public DataTable ExecuteQuery(string Query, int TimeOut)
        {
            SqlConnection conn = null;
            DataTable dtbl = new DataTable();
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "s_GetOutput";
                        cmd.Parameters.AddWithValue("@Query", Query);
                        cmd.Parameters.AddWithValue("@UserName", HttpContext.Current.Session["UserName"] ?? "IIS");
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = conn;
                        cmd.CommandTimeout = TimeOut;

                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        SqlDataReader rdr = cmd.ExecuteReader();
                        dtbl.Load(rdr);

                        if (conn.State != ConnectionState.Closed)
                        {
                            conn.Close();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                dtbl.Columns.Add("Message");
                dtbl.Rows.Add(ex.Message);
                //throw new Exception(ex.Message);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }

            }
            return dtbl;
        }

        //Merchant Details
        public List<MerchantInfo> MerchantDetails(MerchantPay Mpay)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            List<MerchantInfo> ListRec = new List<MerchantInfo>();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNRptMerchantPymtSuperAdmin", conn))
                    {

                        cmd.Parameters.AddWithValue("@StartDate", Mpay.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", Mpay.EndDate);
                        cmd.Parameters.AddWithValue("@SourceMobileNo", Mpay.SourceMobileNo);
                        cmd.Parameters.AddWithValue("@MerchantType", Mpay.MerchantType);
                        cmd.Parameters.AddWithValue("@MerchantName", Mpay.MerchantName);
                        cmd.Parameters.AddWithValue("@Status", Mpay.Status);
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
                    DataTable TpData = dataset.Tables[0];
                    if (TpData.Rows.Count > 0)
                    {
                        foreach (DataRow dr in TpData.Rows)
                        {
                            if (dr["TranDate"].ToString() != "~")
                            {
                                MerchantInfo MI = new MerchantInfo();
                                MI.DatenTime = DateTime.ParseExact(dr["TranDate"].ToString(), "dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                                MI.TxnID = dr["TranId"].ToString();
                                MI.InitMobileNo = dr["Initiator Mobile No"].ToString();
                                MI.MerchantType = dr["Merchant Type"].ToString();
                                MI.MerchantName = dr["Merchant Name"].ToString();
                                MI.Amount = decimal.Parse(dr["Amount"].ToString().Trim() == "" ? "0" : dr["Amount"].ToString());
                                MI.Status = dr["Status"].ToString();
                                MI.Message = dr["Message"].ToString();
                                MI.TranType = dr["TransactionType"].ToString();
                                MI.Name = dr["Name"].ToString();
                                MI.ReferenceNo = dr["ReferenceNo"].ToString();
                                ListRec.Add(MI);
                            }
                        }

                        HttpContext.Current.Session["MerchantInfo"] = TpData;
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

        public string GetMerchantsFromTypeMerchant(string Value, bool AddWhere)
        {
            SqlDataReader rdr;
            SqlConnection conn = null;
            string Option = string.Empty;
            try
            {
                string Query = @"SELECT MerchantName FROM MNPayPointMerchants";

                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Connection = conn;
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            Option = Option + string.Format("<option value='{0}'>{1}</option>", rdr["MerchantName"].ToString(), rdr["MerchantName"].ToString());
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
            return Option;
        }

        //FundTransfer Details

        public List<FundTransfer> FundTxnDetails(TopUp Tp)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            List<FundTransfer> ListRec = new List<FundTransfer>();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNRptFundTransferSuperAdmin", conn))
                    {
                        cmd.Parameters.AddWithValue("@TranId", Tp.TranID);
                        cmd.Parameters.AddWithValue("@StartDate", Tp.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", Tp.EndDate);
                        cmd.Parameters.AddWithValue("@SourceMobileNo", Tp.SourceMobileNo);
                        cmd.Parameters.AddWithValue("@DestMobileNo", Tp.DestMobileNo);
                        cmd.Parameters.AddWithValue("@FTType", Tp.FTType);
                        cmd.Parameters.AddWithValue("@Status", Tp.Status);
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            //set the CommandTimeout
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
                    DataTable TpData = dataset.Tables[0];

                    if (dataset.Tables.Count > 0)
                    {
                        if (TpData.Rows.Count > 0)
                        {
                            foreach (DataRow dr in TpData.Rows)
                            {

                                if (dr["TranDate"].ToString() != "~")
                                {
                                    FundTransfer FT = new FundTransfer();
                                    FT.DatenTime = DateTime.ParseExact(dr["TranDate"].ToString(), "dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                                    FT.TxnID = dr["TranID"].ToString();
                                    FT.FTType = dr["FTType"].ToString();
                                    FT.SourceMobileNo = dr["Source Mobile No"].ToString();
                                    FT.DestMobileNo = dr["Destination Mobile No"].ToString();
                                    FT.Amount = decimal.Parse(dr["Amount"].ToString().Trim() == "" ? "0" : dr["Amount"].ToString().Trim());
                                    FT.Status = dr["Status"].ToString();
                                    FT.Message = dr["Message"].ToString();
                                    FT.ReferenceNo = dr["ReferenceNo"].ToString();
                                    ListRec.Add(FT);
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
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }
                dataset.Dispose();
            }

            return ListRec;
        }

        public List<FundTransfer> FundTxnEBankingDetails(TopUp Tp)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            List<FundTransfer> ListRec = new List<FundTransfer>();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNRptFundTransferEBanking", conn))
                    {
                        cmd.Parameters.AddWithValue("@TranId", Tp.TranID);
                        cmd.Parameters.AddWithValue("@StartDate", Tp.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", Tp.EndDate);
                        cmd.Parameters.AddWithValue("@SourceMobileNo", Tp.SourceMobileNo);
                        cmd.Parameters.AddWithValue("@DestMobileNo", Tp.DestMobileNo);
                        cmd.Parameters.AddWithValue("@FTType", Tp.FTType);
                        cmd.Parameters.AddWithValue("@Status", Tp.Status);
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            //set the CommandTimeout
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
                    DataTable TpData = dataset.Tables[0];

                    if (dataset.Tables.Count > 0)
                    {
                        if (TpData.Rows.Count > 0)
                        {
                            foreach (DataRow dr in TpData.Rows)
                            {

                                if (dr["TranDate"].ToString() != "~")
                                {
                                    FundTransfer FT = new FundTransfer();
                                    FT.DatenTime = DateTime.ParseExact(dr["TranDate"].ToString(), "dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                                    FT.TxnID = dr["TranID"].ToString();
                                    //FT.FTType = dr["FTType"].ToString();
                                    FT.FTType = "Bank to Wallet(e-Banking)";
                                    //FT.SourceMobileNo = dr["Source Mobile No"].ToString();
                                    FT.SourceMobileNo = "NIBL";
                                    FT.DestMobileNo = dr["Destination Mobile No"].ToString();
                                    FT.Amount = decimal.Parse(dr["Amount"].ToString().Trim() == "" ? "0" : dr["Amount"].ToString().Trim());
                                    FT.Status = dr["Status"].ToString();
                                    FT.Message = dr["Message"].ToString();
                                    FT.PaymentReferenceNumber = dr["PaymentReferenceNumber"].ToString();
                                    ListRec.Add(FT);
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
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }
                dataset.Dispose();
            }

            return ListRec;
        }

        public List<string> GetMerchantbyCategory(string CategoryId)
        {
            SqlDataReader rdr;
            SqlConnection conn = null;
            List<string> Merchants = new List<string>();
            try
            {
                string Query = @"SELECT M.Id AS CategoryId,M.Name AS Category ,Ms.mname  as [MerchantName] ,Ms.mid  AS MerchantId  FROM 
                                 dbo.MNMerchants Ms LEFT OUTER JOIN dbo.MNMerchant M ON M.Id = Ms.catid
								 WHERE M.Id=@CategoryId
								 ORDER BY 1";
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
                            Merchants.Add(rdr["MerchantName"].ToString());
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

        //TopUp Record
        public List<TopUpInfo> TopUpDetails(TopUp Tp)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            List<TopUpInfo> ListRec = new List<TopUpInfo>();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNReportTopupSuperAdmin", conn))
                    {
                        cmd.Parameters.AddWithValue("@StartDate", Tp.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", Tp.EndDate);
                        cmd.Parameters.AddWithValue("@SourceMobileNo", Tp.SourceMobileNo);
                        cmd.Parameters.AddWithValue("@TranID", Tp.TranID);

                        cmd.Parameters.AddWithValue("@DestMobileNo", Tp.DestMobileNo);
                        cmd.Parameters.AddWithValue("@RequestType", Tp.RequestType);
                        cmd.Parameters.AddWithValue("@Status", Tp.Status);
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

                    DataTable TpData = dataset.Tables[0];
                    if (TpData.Rows.Count > 0)
                    {
                        foreach (DataRow dr in TpData.Rows)
                        {
                            if (dr["TranDate"].ToString() != "~")
                            {
                                TopUpInfo TpInfo = new TopUpInfo();
                                TpInfo.DatenTime = DateTime.ParseExact(dr["TranDate"].ToString(), "dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                                TpInfo.TxnID = dr["TranId"].ToString();
                                TpInfo.InitMobileNo = dr["Initiator Mobile No"].ToString();
                                TpInfo.DestMobileNo = dr["Destination Mobile No"].ToString();
                                TpInfo.Amount = decimal.Parse(dr["Amount"].ToString().Trim() == "" ? "0" : dr["Amount"].ToString().Trim());
                                TpInfo.ServiceType = dr["ServiceType"].ToString();
                                TpInfo.Status = dr["Status"].ToString();
                                TpInfo.Message = dr["Message"].ToString();
                                TpInfo.ReferenceNo = dr["ReferenceNo"].ToString();
                                ListRec.Add(TpInfo);
                            }
                        }
                        HttpContext.Current.Session["TopUp"] = TpData;
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

        //EBanking Transaction Details
        public List<EBankingTran> EBankingTranDetails(MerchantPay Mpay)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            List<EBankingTran> ListRec = new List<EBankingTran>();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNEBankingTranDetails", conn))
                    {

                        cmd.Parameters.AddWithValue("@StartDate", Mpay.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", Mpay.EndDate);
                        cmd.Parameters.AddWithValue("@SourceMobileNo", Mpay.SourceMobileNo);
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
                    DataTable TpData = dataset.Tables[0];
                    if (TpData.Rows.Count > 0)
                    {
                        foreach (DataRow dr in TpData.Rows)
                        {
                                EBankingTran ET = new EBankingTran();
                                //ET.EBDate = DateTime.ParseExact(dr["EBDate"].ToString(), "yyyy/MM/dd hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                                ET.EBDate = dr["EBDate"].ToString();
                            ET.UserName = dr["UserName"].ToString();
                                ET.PaymentReferenceNumber = dr["PaymentReferenceNumber"].ToString();
                                ET.ItemCode = dr["ItemCode"].ToString();
                                ET.Amount = decimal.Parse(dr["Amount"].ToString().Trim() == "" ? "0" : dr["Amount"].ToString());
                                ET.BID = dr["BID"].ToString();
                                ET.ReferenceNo = dr["ReferenceNo"].ToString();
                                ListRec.Add(ET);
                            
                        }

                        HttpContext.Current.Session["MerchantInfo"] = TpData;
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

        public EBankingTran EBankingTranDetails(string prn )
        {
            SqlDataReader dr;
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            List<EBankingTran> ListRec = new List<EBankingTran>();
            EBankingTran Details = new EBankingTran();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "select ERQ.*, V.UserName, V.Name, ME.BID, ME.Status, ME.RejectRemarks, KYC.FName,KYC.LName from MNEBankingRequest ERQ LEFT OUTER JOIN v_MNClientDetail V ON ERQ.ClientCode = V.ClientCode LEFT OUTER JOIN ManualTranEBankingTable ME ON ERQ.PaymentReferenceNumber = ME.PaymentReferenceNumber LEFT OUTER JOIN MNClientKYC KYC ON V.ClientCode = KYC.ClientCode WHERE ERQ.PaymentReferenceNumber = '" + prn + "';";
                        cmd.Connection = conn;
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        dr = cmd.ExecuteReader();
                        while (dr.Read())
                        {
                            //EBankingTran ET = new EBankingTran();
                            Details.EBDate = dr["EBDate"].ToString();
                            Details.PaymentReferenceNumber = dr["PaymentReferenceNumber"].ToString();
                            Details.ItemCode = dr["ItemCode"].ToString();
                            Details.Amount = decimal.Parse(dr["Amount"].ToString().Trim() == "" ? "0" : dr["Amount"].ToString());
                            Details.ReferenceNo = dr["retrievalReference"].ToString();
                            Details.UserName = dr["UserName"].ToString();
                            Details.Name = dr["Name"].ToString();
                            Details.BID = dr["BID"].ToString();
                            Details.Status = dr["Status"].ToString();
                            Details.FName = dr["FName"].ToString();
                            Details.LName = dr["LName"].ToString();
                            Details.RejectedRemarks = dr["RejectRemarks"].ToString();
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

            return Details;
        }

        #region EBanking Cash Load
        public List<EBankingTran> EBankingCashLoad(MerchantPay Mpay)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            List<EBankingTran> ListRec = new List<EBankingTran>();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNEBankingCashLoad", conn))
                    {

                        cmd.Parameters.AddWithValue("@StartDate", Mpay.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", Mpay.EndDate);
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
                    DataTable TpData = dataset.Tables[0];
                    if (TpData.Rows.Count > 0)
                    {
                        foreach (DataRow dr in TpData.Rows)
                        {
                            EBankingTran ET = new EBankingTran();
                            //ET.EBDate = DateTime.ParseExact(dr["EBDate"].ToString(), "yyyy/MM/dd hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                            ET.EID = dr["EBankingID"].ToString();
                            ET.ClientCode = dr["ClientCode"].ToString();
                            ET.PaymentReferenceNumber = dr["PaymentReferenceNumber"].ToString();
                            ET.ItemCode = dr["ItemCode"].ToString();
                            ET.Amount = decimal.Parse(dr["Amount"].ToString().Trim() == "" ? "0" : dr["Amount"].ToString());
                            ET.EBDate = dr["EBDate"].ToString();
                            ET.ReferenceNo = dr["retrievalReference"].ToString();
                            ET.Status = dr["Status"].ToString();
                            
                            ListRec.Add(ET);

                        }

                        HttpContext.Current.Session["MerchantInfo"] = TpData;
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
        #endregion




















        //Nischal
        //public static IEnumerable<MNCustProfile> GetMNCustProfile()
        //{


        //    string ConnectionString = DatabaseConnection.ConnectionStr().ToString();

        //    List<MNCustProfile> MNCustProfile = new List<MNCustProfile>();
        //    try
        //    {

        //        string Sql = @"SELECT  MP.CustProfileCode, MP.ProfileDesc, MP.ProfileStatus, MP.RenewPeriod, MP.AutoRenew,
        //                       MPC.Registration, MPC.ReNew, MPC.PinReset, MPC.ChargeAccount, MP.HasCharge, MP.IsDrAlert, MP.IsCrAlert, MP.MinDrAlertAmt,
        //                       MP.MinCrAlertAmt
        //                       FROM    dbo.MNCustProfile AS MP ( NOLOCK )
        //                       INNER JOIN dbo.MNProfileCharge AS MPC ( NOLOCK ) ON MPC.ProfileCode = MP.CustProfileCode";

        //        DataTable dtbl = Utilities.SqlHelper.SqlHelper.ExecuteDataTable(ConnectionString, CommandType.Text, Sql);
        //        if (dtbl.Rows.Count > 0)
        //        {
        //            for (int i = 0; i < dtbl.Rows.Count; i++)
        //            {
        //                MNCustProfile.Add(
        //                    new MNCustProfile
        //                    {
        //                        ProfileCode = dtbl.Rows[i]["CustProfileCode"].ToString(),
        //                        ProfileDesc = dtbl.Rows[i]["ProfileDesc"].ToString(),
        //                        ProfileStatus = Convert.ToChar(dtbl.Rows[i]["ProfileStatus"].ToString())
        //                    });
        //            }
        //        }

        //    }

        //    catch (SqlException ex)
        //    {
        //        throw ex;
        //    }

        //    return MNCustProfile;
        //}

        //CustomerDetails
        public List<CustomerData> CustomerDetail(CustReport Cus)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            List<CustomerData> ListRec = new List<CustomerData>();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    //using (SqlCommand cmd = new SqlCommand("dbo.s_MNReportCustomer", conn))
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNReportCustomerSuperAdmin", conn))
                    {
                        //cmd.Parameters.AddWithValue("@ProfileCode", Cus.ProfileCode);
                        cmd.Parameters.AddWithValue("@StartDate", Cus.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", Cus.EndDate);
                        cmd.Parameters.AddWithValue("@MobileNo", Cus.MobileNo);
                        cmd.Parameters.AddWithValue("@UserName", Cus.UserName);
                        cmd.Parameters.AddWithValue("@KYCStatus", Cus.Status);
                        //cmd.Parameters.AddWithValue("@Approved", Approved);
                        //cmd.Parameters.AddWithValue("@RegisteredBy", Register);
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
                            CustomerData CusData = new CustomerData();
                            CusData.MobileNo = dr["MobileNumber"].ToString();
                            CusData.CustomerName = dr["CustomerName"].ToString();
                            //CusData.AccountNo = dr["AccountNo"].ToString();
                            //CusData.ProfileName = dr["ProfileName"].ToString();
                            CusData.CreatedDate = dr["CreatedDate"].ToString();
                            //CusData.ExpiryDate = dr["ExpiryDate"].ToString();
                            CusData.Status = dr["Status"].ToString();
                            CusData.Approved = dr["KYCstatus"].ToString();
                            CusData.CreatedBy = dr["CreatedBy"].ToString();
                            CusData.ApprovedBy = dr["ApprovedBy"].ToString();
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



        //Auto Reversal Details
        public List<MerchantInfo> ReversalDetails(MerchantPay Mpay)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            List<MerchantInfo> ListRec = new List<MerchantInfo>();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNRptAutoReversalTransaction", conn))
                    {

                        cmd.Parameters.AddWithValue("@StartDate", Mpay.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", Mpay.EndDate);
                        cmd.Parameters.AddWithValue("@SourceMobileNo", Mpay.SourceMobileNo);
                        cmd.Parameters.AddWithValue("@MerchantType", Mpay.MerchantType);                        
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
                    DataTable TpData = dataset.Tables[0];
                    if (TpData.Rows.Count > 0)
                    {
                        foreach (DataRow dr in TpData.Rows)
                        {
                            if (dr["EnteredAt"].ToString() != "~")
                            {
                                MerchantInfo MI = new MerchantInfo();
                                //MI.DatenTime = DateTime.ParseExact(dr["TranDate"].ToString(), "dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                                MI.TxnID = dr["TranId"].ToString();
                                MI.EnteredAt = dr["EnteredAt"].ToString();
                                MI.InitMobileNo = dr["Initiator Mobile No"].ToString();
                                //MI.MerchantType = dr["Merchant Type"].ToString();
                                string MerTyp= "Topup";
                                MI.MerchantType = MerTyp;
                                //MI.MerchantName = dr["Merchant Name"].ToString();
                                MI.Amount = decimal.Parse(dr["Amount"].ToString().Trim() == "" ? "0" : dr["Amount"].ToString());
                                MI.Status = dr["status"].ToString();
                                MI.Message = dr["Message"].ToString();
                                MI.MerchantName = dr["MerchantName"].ToString();
                                MI.TranType = dr["TransactionType"].ToString();
                                MI.Name = dr["Name"].ToString();
                                MI.ReferenceNo = dr["ReferenceNo"].ToString();
                                MI.PayMode = dr["PayMode"].ToString();
                                ListRec.Add(MI);
                            }
                        }

                        HttpContext.Current.Session["MerchantInfo"] = TpData;
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







        // Manual Reversal Details
        public List<MerchantInfo> ManualReversalDetails(MerchantPay Mpay)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            List<MerchantInfo> ListRec = new List<MerchantInfo>();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNRptManualReversalTransaction", conn))
                    {

                        cmd.Parameters.AddWithValue("@StartDate", Mpay.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", Mpay.EndDate);
                        cmd.Parameters.AddWithValue("@SourceMobileNo", Mpay.SourceMobileNo);
                        cmd.Parameters.AddWithValue("@MerchantType", Mpay.MerchantType);
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
                    DataTable TpData = dataset.Tables[0];
                    if (TpData.Rows.Count > 0)
                    {
                        foreach (DataRow dr in TpData.Rows)
                        {
                            if (dr["EnteredAt"].ToString() != "~")
                            {
                                MerchantInfo MI = new MerchantInfo();
                                MI.EnteredAt = dr["EnteredAt"].ToString();
                                
                                MI.TxnID = dr["TranId"].ToString();
                                MI.InitMobileNo = dr["Initiator Mobile No"].ToString();
                                //MI.MerchantType = dr["Merchant Type"].ToString();
                                string MerTyp = "Topup";
                                MI.MerchantType = MerTyp;
                                MI.MerchantName = dr["MerchantName"].ToString();
                                MI.Amount = decimal.Parse(dr["Amount"].ToString().Trim() == "" ? "0" : dr["Amount"].ToString());
                                MI.Status = dr["status"].ToString();
                                MI.Message = dr["Message"].ToString();
                                //MI.MerchantName = dr["Name"].ToString();
                                MI.TranType = dr["TransactionType"].ToString();
                                MI.Name = dr["Name"].ToString();
                                MI.ReferenceNo = dr["ReferenceNo"].ToString();
                                MI.PayMode = dr["PayMode"].ToString();
                                MI.VerifiedBy = dr["VerifiedBy"].ToString();
                                MI.ApprovedBy = dr["ApprovedBy"].ToString();
                                ListRec.Add(MI);
                            }
                        }

                        HttpContext.Current.Session["Reversal"] = TpData;
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
