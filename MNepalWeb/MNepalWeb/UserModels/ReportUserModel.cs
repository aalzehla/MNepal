using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using MNepalWeb.ViewModel;
using System.Data.SqlClient;
using MNepalWeb.Models;
using MNepalWeb.Connection;
using System.Globalization;
using MNepalWeb.Utilities;

namespace MNepalWeb.UserModels
{
    public class ReportUserModel
    {

      //List<CustomerAccActivity>
        public List<CustomerAccActivity> CustomerAccountActivity(CustReport Cus)
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
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNGetCustActivity", conn))
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

                AcActivity = dataset.Tables[0];
             //  DataTable Balance = dataset.Tables[1];
              
                if (AcActivity.Rows.Count > 0)
                {
                    foreach (DataRow dr in AcActivity.Rows)
                    {
                        CustomerAccActivity cuss = new CustomerAccActivity();
                        cuss.TransactionDate = DateTime.ParseExact(dr["TransactionDate"].ToString(), "dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture); //hh:mm:ss.fff tt
                        cuss.UserName = dr["UserName"].ToString();
                        cuss.TransactionType = dr["TransactionType"].ToString();
                        cuss.DestinationNo = dr["DestinationNo"].ToString();
                        cuss.Amount = decimal.Parse(dr["Amount"].ToString().Trim() == "" ? "0" : dr["Amount"].ToString().Trim());
                        cuss.SMSStatus = dr["SMSStatus"].ToString();
                        cuss.SMSSenderReply = dr["SMSSenderReply"].ToString();
                        cuss.ErrorMessage = dr["ErrorMessage"].ToString();
                        cuss.SMSTimeStamp = dr["SMSTimeStamp"].ToString();
                        cuss.Name = dr["Name"].ToString();
                        //cuss.Credit = decimal.Parse(dr["Credit"].ToString().Trim() == "" ? "0" : dr["Credit"].ToString().Trim());
                        //cuss.Debit = decimal.Parse(dr["Debit"].ToString().Trim() == "" ? "0" : dr["Debit"].ToString().Trim());
                        
                       //cuss.ReferenceNo = dr["ReferenceNo"].ToString();
                        ListRec.Add(cuss);
                    }
                }
                //System.Web.HttpContext.Current.Session["CustomerAcActivity"] = AcActivity;
               

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

        public List<Merchants> GetMerchants()
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
                        cmd.CommandText = "SELECT mname,ClientCode FROM dbo.MNMerchants (NOLOCK) ";
                        cmd.Connection = conn;
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            Merchants merchant = new Merchants();
                            merchant.Name = rdr["mname"].ToString();
                            merchant.ClientCode = rdr["ClientCode"].ToString();
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
        public List<MerchantAcDetail> MerchantAccDetail(CustReport Cus)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;

            List<MerchantAcDetail> detail = new List<MerchantAcDetail>();
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNGetMerchantTran", conn))
                    {
                        cmd.Parameters.AddWithValue("@ClientCode", Cus.UserName);
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

                if (dataset.Tables.Count > 0)
                {
                    DataTable MerchantAcDetail = dataset.Tables[0];
                    if (MerchantAcDetail.Rows.Count > 0)
                    {
                        foreach (DataRow dr in MerchantAcDetail.Rows)
                        {
                            MerchantAcDetail MLog = new MerchantAcDetail();
                            MLog.TransactionType = dr["TransactionType"].ToString();
                            MLog.TransactionDate = dr["TransactionDate"].ToString();
                            MLog.Amount = string.Format("{0:n}", decimal.Parse(dr["Amount"].ToString().Trim() == "" ? "0" : dr["Amount"].ToString().Trim()));
                            MLog.Description = dr["Description"].ToString();
                            MLog.TranId = dr["TranId"].ToString();
                            MLog.ReferenceNo = dr["ReferenceNo"].ToString();
                            MLog.SourceAccountNo = dr["SourceAccountNo"].ToString();
                            MLog.UserName = dr["UserName"].ToString();
                            detail.Add(MLog);
                        }
                        HttpContext.Current.Session["MerchantDetail"] = MerchantAcDetail;
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

            return detail;
        }
        
        //CustomerDetails
        public List<CustomerData> CustomerDetails(CustReport Cus,string Approved, string Register)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            List<CustomerData> ListRec = new List<CustomerData>();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNReportCustomer", conn))
                    {
                        cmd.Parameters.AddWithValue("@ProfileCode", Cus.ProfileCode);
                        cmd.Parameters.AddWithValue("@StartDate", Cus.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", Cus.EndDate);
                        cmd.Parameters.AddWithValue("@MobileNo", Cus.MobileNo);
                        cmd.Parameters.AddWithValue("@UserName", Cus.UserName);
                        cmd.Parameters.AddWithValue("@Status", Cus.Status);
                        cmd.Parameters.AddWithValue("@Approved", Approved);
                        cmd.Parameters.AddWithValue("@RegisteredBy", Register);
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
                            CusData.ProfileName = dr["ProfileName"].ToString();
                            CusData.CreatedDate = dr["CreatedDate"].ToString();
                            CusData.ExpiryDate = dr["ExpiryDate"].ToString();
                            CusData.Status = dr["Status"].ToString();
                            CusData.Approved = dr["Approved"].ToString();
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
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNReportTopup", conn))
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


        //Recharge Details

        public List<TopUpInfo> RechargeDetails(TopUp Tp)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            List<TopUpInfo> ListRec = new List<TopUpInfo>();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNReportRecharge", conn))
                    {
                        cmd.Parameters.AddWithValue("@StartDate", Tp.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", Tp.EndDate);
                        cmd.Parameters.AddWithValue("@SourceMobileNo", Tp.SourceMobileNo);
                        cmd.Parameters.AddWithValue("@TranID", Tp.TranID);
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
                            TpInfo.TxnID = dr["TranID"].ToString();
                            TpInfo.InitMobileNo = dr["Initiator Mobile No"].ToString();
                            //TpInfo.DestMobileNo = dr["Destination Mobile No"].ToString();
                                TpInfo.Amount = decimal.Parse(dr["Amount"].ToString().Trim() == "" ? "0" : dr["Amount"].ToString().Trim()); 
                            TpInfo.ServiceType = dr["ServiceType"].ToString();
                            TpInfo.Status = dr["Status"].ToString();
                            TpInfo.Message = dr["Message"].ToString();
                            ListRec.Add(TpInfo);
                        }
                        }
                        HttpContext.Current.Session["Recharge"] = TpData;
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
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNRptFundTransfer", conn))
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
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNRptMerchantPymt", conn))
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

        //public Dictionary<string,string> GetMerchantbyCategory(string CategoryId,string catName)
        //{
        //    SqlDataReader rdr;
        //    SqlConnection conn = null;
        //    Dictionary<string, string> Merchant = new Dictionary<string, string>();
        //    try
        //    {
        //        string Query = @"SELECT M.Id AS CategoryId,M.Name AS Category ,Ms.mname  as [MerchantName] ,Ms.mid  AS MerchantId  FROM 
        //                         dbo.MNMerchants Ms LEFT OUTER JOIN dbo.MNMerchant M ON M.Id = Ms.catid
								// WHERE M.Id=@CategoryId
								// ORDER BY 1";
        //        using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
        //        {
        //            using (SqlCommand cmd = new SqlCommand())
        //            {
        //                cmd.CommandText = Query;
        //                cmd.Connection = conn;
        //                cmd.Parameters.AddWithValue("@CategoryId", CategoryId);
        //                if (conn.State != ConnectionState.Open)
        //                {
        //                    conn.Open();
        //                }
        //                rdr = cmd.ExecuteReader();
        //                while (rdr.Read())
        //                {
        //                  //  Merchant.Add();
        //                }
        //                if (conn.State != ConnectionState.Closed)
        //                {
        //                    conn.Close();
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //    finally
        //    {
        //        if (conn.State != ConnectionState.Closed)
        //        {
        //            conn.Close();
        //        }

        //    }
        //    return Merchant;
        //}

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

        //Summary Report

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
                            MI.NoOfTran = Convert.ToInt32(dr["NoOfTran"].ToString().Trim()==""?"0": dr["NoOfTran"].ToString().Trim());
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
                            MI.TranDate= dr["TranDate"].ToString();
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

        public List<BankLinkInfo> BankLinkDetails(BankLink bankLink)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            List<BankLinkInfo> ListRec = new List<BankLinkInfo>();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNBankLinkSummary", conn))
                    {

                        cmd.Parameters.AddWithValue("@StartDate", bankLink.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", bankLink.EndDate);
                        cmd.Parameters.AddWithValue("@MobileNo", bankLink.MobileNo);
                        cmd.Parameters.AddWithValue("@Status", bankLink.Status);
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
                            BankLinkInfo MI = new BankLinkInfo();

                            //MI.RequestedDate = DateTime.ParseExact(dr["RequestedDate"].ToString(), "dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture); //hh:mm:ss.fff tt
                            MI.RequestedDate = dr["RequestedDate"].ToString();
                            MI.MobileNo = dr["MobileNumber"].ToString();
                            MI.CustomerName = dr["Name"].ToString();
                            MI.BankAccNo = dr["AccountNumber"].ToString();
                            MI.Status = dr["Status"].ToString();
                            MI.VerifiedBy = dr["VerifiedBy"].ToString();
                            MI.VerifiedDate = dr["VerifiedDate"].ToString();
                            //DateTime.ParseExact(dr["VerifiedDate"].ToString(), "dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture); //hh:mm:ss.fff tt
                            MI.ApprovedBy = dr["ApprovedBy"].ToString();
                            MI.ApprovedDate = dr["ApprovedDate"].ToString();
                            //DateTime.ParseExact(dr["ApprovedDate"].ToString(), "dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture); //hh:mm:ss.fff tt

                            ListRec.Add(MI);
                        }

                        HttpContext.Current.Session["BankLinkInfo"] = TpData;
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

        //Get BranchCode
        public Dictionary<string, string> GetBranchCode()
        {
            SqlDataReader rdr;
            SqlConnection conn = null;
            Dictionary<string, string> dist = new Dictionary<string, string>();
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SELECT BranchCode,BranchName FROM MNBranchTable (NOLOCK) where BankCode='0004'";
                        cmd.Connection = conn;
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            
                            dist.Add(rdr["BranchCode"].ToString(), rdr["BranchName"].ToString());
                           
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
            return dist;
        }


        //Branchwise Report

        public List<BranchInfo> BranchRepDetail(BranchRep br, string bankCode)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            List<BranchInfo> ListRec = new List<BranchInfo>();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNStaffActivity", conn))
                    {

                        cmd.Parameters.AddWithValue("@BankCode", bankCode); 

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
                            BranchInfo BI = new BranchInfo();

                            BI.BranchName = dr["BranchName"].ToString();
                            BI.TotalCustomer = dr["TotalCustomer"].ToString();
                            BI.TotalApproveCustomer = dr["TotalApproveCustomer"].ToString();
                            BI.TotalUnApproveCustomer = dr["TotalUnApproveCustomer"].ToString();
                            BI.TotalRejectedCustomer = dr["TotalRejectedCustomer"].ToString();



                            ListRec.Add(BI);
                        }

                        HttpContext.Current.Session["BranchInfo"] = TpData;
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

        //MerchantStatement

        public List<SmtInfo> MerchantSmt(MerStatement ms)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            List<SmtInfo> ListRec = new List<SmtInfo>();

            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNMerchantStatement", conn))
                    {

                        cmd.Parameters.AddWithValue("@MainCodeDB", ms.MobileNo);
                        cmd.Parameters.AddWithValue("@StartDate", ms.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", ms.EndDate);
//                        cmd.Parameters.Add("@MsgStr", SqlDbType.VarChar).Direction = ParameterDirection.Output;


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
                            SmtInfo SI = new SmtInfo();

                            SI.TranDate = dr["TranDate"].ToString();
                            SI.TranID = dr["TranID"].ToString();
                            SI.InitiatorMobileNo = dr["Initiator Mobile No"].ToString();
                            SI.DrAmount = dr["DrAmt"].ToString();
                            SI.CrAmount = dr["CrAmt"].ToString();
                            SI.Description = dr["Description"].ToString();
                            SI.ValueDate = dr["ValueDate"].ToString();
                           




                            ListRec.Add(SI);
                        }

                        HttpContext.Current.Session["SmtInfo"] = TpData;
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

        public List<MNExpiryCustomers> GetExpiryCustomers(string UserName,string ProfileCode,string Status)
        {
            SqlConnection conn = null;
            List<MNExpiryCustomers> ExpiredCustomers = new List<MNExpiryCustomers>();
            DataSet dataset = new DataSet();
            try
            {
                
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "s_MNGetExpiryCustomers";
                        cmd.Parameters.AddWithValue("@UserName",UserName);
                        cmd.Parameters.AddWithValue("@ProfileCode",ProfileCode);
                        cmd.Parameters.AddWithValue("@Status", Status);
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
                        if(dataset.Tables.Count>0)
                        {
                            ExpiredCustomers= ExtraUtility.DatatableToListClass<MNExpiryCustomers>(dataset.Tables[0]);
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
            return ExpiredCustomers;
        }


        public List<Tuple<string, string>> GetTransactionStatusSummary(string Date)
        {
            SqlDataReader rdr;
            SqlConnection conn = null;
            List<Tuple<string, string>> TransactonStatus = new List<Tuple<string, string>>();
            try
            {
                string Query = @"SELECT COUNT(ResponseCode) AS TransactionCount,ResponseCode
                                 FROM dbo.MNResponse WHERE CAST(TranDate AS DATE)=@Date
                                 GROUP BY ResponseCode";
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Connection = conn;
                        cmd.Parameters.AddWithValue("@Date", Date);
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            string TransactionCount = rdr["TransactionCount"].ToString();
                            string ResponseCode = rdr["ResponseCode"].ToString();
                            TransactonStatus.Add(Tuple.Create(TransactionCount, ResponseCode));
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
            return TransactonStatus;
        }

        void LogQuery(SqlConnection conn,String Query)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = @"INSERT INTO MNExecLog (Text,ExecutedOn,ExecutedBy)
                                        Values(@Query,GetDate(),@ExecutedBy)";
                    cmd.Parameters.AddWithValue("@Query",Query);
                    cmd.Parameters.AddWithValue("@ExecutedBy", HttpContext.Current.Session["UserName"]??"IIS");
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    cmd.ExecuteNonQuery();
                    if (conn.State != ConnectionState.Closed)
                    {
                        conn.Close();
                    }
                }
            }
            catch
            {

            }
        }

        public DataTable ExecuteQuery(string Query,int TimeOut)
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
                        SqlDataReader rdr=cmd.ExecuteReader();
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

        public List<MNAdminActivity> GetAdminActivity(string UserName,string BranchCode,string StartDate,string EndDate)
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
                        cmd.CommandText = "s_GetMNAdminActivity";
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

        ///Cus Subscription Expiry 
        ////CustomerDetails
        //public List<ExpiryInfo> CusSubscription(CusSubscription Cus)
        //{
        //    DataSet dataset = new DataSet();
        //    SqlConnection conn = null;
        //    List<ExpiryInfo> ListRec = new List<ExpiryInfo>();

        //    try
        //    {
        //        using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
        //        {
        //            conn.Open();
        //            using (SqlCommand cmd = new SqlCommand("dbo.s_MNReportCustomer", conn))
        //            {

        //                cmd.Parameters.AddWithValue("@TimePeriod",Cus.TimePeriod);
        //                cmd.CommandType = CommandType.StoredProcedure;
        //                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
        //                {
        //                    // set the CommandTimeout
        //                    da.SelectCommand.CommandTimeout = 0;  // seconds
        //                    da.Fill(dataset);
        //                }
        //            }
        //        }
        //        if (conn.State != ConnectionState.Closed)
        //        {
        //            conn.Close();
        //        }
        //        if (dataset.Tables.Count > 0)
        //        {
        //            DataTable RegCus = dataset.Tables[0];


        //            if (RegCus.Rows.Count > 0)
        //            {
        //                foreach (DataRow dr in RegCus.Rows)
        //                {
        //                    ExpiryInfo CusData = new ExpiryInfo();
        //                    CusData.CustomerName = dr["CustomerName"].ToString();
        //                    CusData.ExpiredDate = dr["ExpiredDate"].ToString();
        //                    CusData.Address = dr["Address"].ToString();
        //                    CusData.Status = dr["Status"].ToString();
        //                    ListRec.Add(CusData);
        //                }

        //                HttpContext.Current.Session["ExpiryInfo"] = RegCus;

        //            }


        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //    finally
        //    {
        //        if (conn.State != ConnectionState.Closed)
        //        {
        //            conn.Close();
        //        }
        //        dataset.Dispose();
        //    }

        //    return ListRec;
        //}

        ////cus subscription expired
        //public List<ExpiryInfo> CusSubscriptionExpired()
        //{
        //    DataSet dataset = new DataSet();
        //    SqlConnection conn = null;
        //    List<ExpiryInfo> ListRec = new List<ExpiryInfo>();

        //    try
        //    {
        //        using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
        //        {
        //            conn.Open();
        //            using (SqlCommand cmd = new SqlCommand("dbo.s_MNStaffActivity", conn))
        //            {

        //                //cmd.Parameters.AddWithValue("@BranchCode", br.BranchCode);


        //                cmd.CommandType = CommandType.StoredProcedure;
        //                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
        //                {
        //                    // set the CommandTimeout
        //                    da.SelectCommand.CommandTimeout = 0;  // seconds
        //                    da.Fill(dataset);
        //                }
        //            }
        //        }
        //        if (conn.State != ConnectionState.Closed)
        //        {
        //            conn.Close();
        //        }


        //        if (dataset.Tables.Count > 0)
        //        {
        //            DataTable TpData = dataset.Tables[0];

        //            if (TpData.Rows.Count > 0)
        //            {
        //                foreach (DataRow dr in TpData.Rows)
        //                {
        //                    ExpiryInfo BI = new ExpiryInfo();

        //                    BI.CustomerName = dr["CustomerName"].ToString();
        //                    BI.ExpiredDate = dr["ExpiredDate"].ToString();
        //                    BI.Address = dr["Address"].ToString();
        //                    BI.Status = dr["Status"].ToString();

        //                    ListRec.Add(BI);
        //                }

        //                HttpContext.Current.Session["ExpiryInfo"] = TpData;
        //            }


        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //    finally
        //    {
        //        if (conn.State != ConnectionState.Closed)
        //        {
        //            conn.Close();
        //        }
        //        dataset.Dispose();
        //    }

        //    return ListRec;
        //}

    }
}