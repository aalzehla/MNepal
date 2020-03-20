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
    public class LogUserModel
    {
        public List<ResponseLog> CustomerResLog(CustReport Cus)
        {
            DataSet dataset = new DataSet();
            SqlConnection conn = null;
            DataTable AcActivity = new DataTable();
            List<ResponseLog> ListRec = new List<ResponseLog>();
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNSummaryResponseDetailRpt", conn))
                    {
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
                        ResponseLog cuss = new ResponseLog();
                        cuss.OriginID = dr["OriginID"].ToString();
                        cuss.OriginType = dr["OriginType"].ToString();
                        cuss.ServiceCode = dr["ServiceCode"].ToString();
                        cuss.SourceBankCode = dr["SourceBankCode"].ToString();
                        cuss.SourceBranchCode = dr["SourceBranchCode"].ToString();
                        cuss.SourceAccountNo = dr["SourceAccountNo"].ToString();
                        cuss.DestBankCode = dr["DestBankCode"].ToString();
                        //cuss.DestBranchCode = dr["DestBranchCode"].ToString();
                        //cuss.DestAccountNo = dr["DestAccountNo"].ToString();
                        cuss.SourceBranchCode = dr["SourceBranchCode"].ToString();
                        cuss.Amount = decimal.Parse(dr["Amount"].ToString().Trim() == "" ? "0" : dr["Amount"].ToString().Trim());
                        cuss.FeeId = dr["FeeId"].ToString();
                        cuss.FeeAmount = float.Parse(dr["Amount"].ToString().Trim() == "" ? "0" : dr["Amount"].ToString().Trim());
                        cuss.TraceNo = dr["TraceNo"].ToString();
                        //Convert.ToDateTime(dr["TranDate"].ToString()).ToString("dd/MM/yyyy")
                        cuss.TranDate = Convert.ToDateTime(dr["TranDate"].ToString());//.ToString("dd/MM/yyyy")//hh:mm:ss.fff tt
                        //cuss.TranTime = DateTime.ParseExact(dr["TranTime"].ToString(), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture); //hh:mm:ss.fff tt
                        cuss.RetrievalRef = dr["RetrievalRef"].ToString();
                        cuss.ResponseCode = dr["ResponseCode"].ToString();
                        cuss.ResponseDescription = dr["ResponseDescription"].ToString();
                        cuss.Balance = dr["Balance"].ToString();
                        cuss.AcHolderName = dr["AcHolderName"].ToString();
                        //cuss.MiniStmntRec = dr["MiniStmntRec"].ToString();
                        //cuss.ReversalStatus = Convert.ToChar(dr["ReversalStatus"].ToString());
                        cuss.TranId = dr["TranId"].ToString();

                       
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
    }
}