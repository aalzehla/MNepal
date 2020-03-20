using System;
using System.Data;

using MNSuperadmin.Connection;

using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data.SqlClient;
using MNSuperadmin.Models;

namespace MNSuperadmin.UserModels
{
    public class WalletLimitUserModel
    {
        //get wallet info
        public static DataTable GetAllWalletInfo()
        {
            Database database;
            DataTable dtableResult = null;

            try
            {
                database = DatabaseConnection.GetDatabase();

                using (var command = database.GetSqlStringCommand("SELECT cast(WPerTxnAmt as decimal(10,2)) AS WPerTxnAmt, cast(WPerDayAmt as decimal(10,2)) AS WPerDayAmt, cast(WTxnAmtM as decimal(10,2)) AS WTxnAmtM, * FROM dbo.MNWTxnLimit"))
                {

                    using (var dataset = new DataSet())
                    {
                        database.LoadDataSet(command, dataset, "dtUserProfileInfo");

                        if (dataset.Tables.Count > 0)
                        {
                            dtableResult = dataset.Tables["dtUserProfileInfo"];
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

        public DataTable GetWalletInfo(string WalletProfileCode)
        {
            Database database;
            DataTable dtableResult = null;

            try
            {
                database = DatabaseConnection.GetDatabase();

                using (var command = database.GetSqlStringCommand("SELECT * FROM dbo.MNWTxnLimit WHERE WalletProfileCode = @WalletProfileCode"))
                {
                    database.AddInParameter(command, "WalletProfileCode", DbType.String, WalletProfileCode);
                    using (var dataset = new DataSet())
                    {
                        database.LoadDataSet(command, dataset, "dtUserProfileInfo");
                        
                            if (dataset.Tables.Count > 0)
                        {
                            dtableResult = dataset.Tables["dtUserProfileInfo"];
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

        public DataSet GetWalletInfoDSet(LimitWallet objWalletLimitInfo)
        {
            Database database;
            DataSet dtset = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNWalletInfo]"))
                {
                    database.AddInParameter(command, "@WalletProfileCode", DbType.String, objWalletLimitInfo.WalletProfileCode);
                    database.AddInParameter(command, "@Mode", DbType.String, objWalletLimitInfo.Mode);
                    string[] tables = new string[] { "dtWalletInfo" };
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

        public int UpdateWalletLimit(UserProfilesInfo objWalletLimitInfo)
        {
            SqlConnection sqlCon = null;
            int ret;
            try
            {
                using (sqlCon = new SqlConnection(Connection.DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNWalletInfoUpdate]", sqlCon))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;

                        sqlCmd.Parameters.Add(new SqlParameter("@WalletProfileCode", objWalletLimitInfo.WalletProfileCode));
                        sqlCmd.Parameters.Add(new SqlParameter("@WTxnCount", objWalletLimitInfo.WTxnCount));
                        sqlCmd.Parameters.Add(new SqlParameter("@WPerTxnAmt", objWalletLimitInfo.WPerTxnAmt));
                        sqlCmd.Parameters.Add(new SqlParameter("@WPerDayAmt", objWalletLimitInfo.WPerDayAmt));
                        sqlCmd.Parameters.Add(new SqlParameter("@WTxnAmtM", objWalletLimitInfo.WTxnAmtM));
                                                        
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

    }
}