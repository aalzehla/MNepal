using System;
using System.Data;
using System.Data.SqlClient;
using MNepalProject.Connection;
using MNepalProject.Models;

namespace MNepalWCF.UserModels
{
    public class TransactionLimitUserModel
    {
        #region Transaction Limit Information

        public DataTable GetTranLimitInfo(MNClientExt objUserLimitInfo)
        {
            DataTable dtableResult = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNTranLimit]", conn))
                    {
                        cmd.Parameters.AddWithValue("@ContactNo", objUserLimitInfo.UserName);
                        cmd.Parameters.AddWithValue("@mode", "WTL");
                        cmd.Parameters.Add("@MsgStr", SqlDbType.VarChar, 255).Direction = ParameterDirection.Output;
                        cmd.CommandType = CommandType.StoredProcedure;

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 60;  // seconds
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset, "dtUserTranLimitInfo");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables["dtUserTranLimitInfo"];
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            
            return dtableResult;
        }

        #endregion


        #region Bank Transaction Limit Information

        public DataTable GetBankTranLimitInfo(MNClientExt objUserLimitInfo)
        {
            DataTable dtableResult = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNTranLimit]", conn))
                    {
                        cmd.Parameters.AddWithValue("@ContactNo", objUserLimitInfo.UserName);
                        cmd.Parameters.AddWithValue("@mode", "BTL");
                        cmd.Parameters.Add("@MsgStr", SqlDbType.VarChar, 255).Direction = ParameterDirection.Output;
                        cmd.CommandType = CommandType.StoredProcedure;

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 60;  // seconds
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset, "dtUserTranLimitInfo");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables["dtUserTranLimitInfo"];
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return dtableResult;
        }





        public DataTable GetTransactionLimit(MNClientExt objUserLimitInfo)
        {
            DataTable dtableResult = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNGetProfileLimit", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserName", objUserLimitInfo.UserName);
                        cmd.CommandType = CommandType.StoredProcedure;

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 60;  // seconds
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset);
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables[0];
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return dtableResult;
        }

        #endregion

        #region Wallet Transaction Limit Information

        public DataTable GetWalletTransactionLimit(MNClientExt objUserLimitInfo)
        {
            DataTable dtableResult = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNGetProfileWalletLimit", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserName", objUserLimitInfo.UserName);
                        cmd.CommandType = CommandType.StoredProcedure;

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 60;  // seconds
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset);
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables[0];
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return dtableResult;
        }

        #endregion


        #region Individual Transaction Limit
        
        public DataTable GetIndvTransactionLimit(MNClientExt objUserLimitInfo)
        {
            DataTable dtableResult = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.s_MNIndvTranLimit", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserName", objUserLimitInfo.UserName);
                        cmd.CommandType = CommandType.StoredProcedure;

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 60;  // seconds
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset);
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables[0];
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return dtableResult;
        }

        #endregion

        # region Individual Total Transaction Limit

        public DataTable GetBankTranIndvTotLimitInfo(MNClientExt objUserLimitInfo)
        {
            DataTable dtableResult = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNTranLimit]", conn))
                    {
                        cmd.Parameters.AddWithValue("@ContactNo", objUserLimitInfo.UserName);
                        cmd.Parameters.AddWithValue("@mode", "BITL");
                        cmd.Parameters.Add("@MsgStr", SqlDbType.VarChar, 255).Direction = ParameterDirection.Output;
                        cmd.CommandType = CommandType.StoredProcedure;

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 60;  // seconds
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset, "dtUserTranLimitInfo");
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables["dtUserTranLimitInfo"];
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return dtableResult;
        }

        #endregion


        #region Wallet Separate Transaction Limit Information 

        public DataTable GetSuperWalletTxnLimit(MNClientExt objUserLimitInfo)
        {
            DataTable dtableResult = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNGetSuperWalletLimit]", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserName", objUserLimitInfo.UserName);
                        cmd.Parameters.AddWithValue("@WalletProfileCode", objUserLimitInfo.WalletProfileCode);
                        cmd.CommandType = CommandType.StoredProcedure;

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 60;  // seconds
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset);
                                if (dataset.Tables.Count > 0)
                                {
                                    dtableResult = dataset.Tables[0];
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return dtableResult;
        }

        #endregion

    }
}