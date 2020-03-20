using CustApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace CustApp.UserModels
{
    public DataSet GetDematPaymentDetails(DMAT objUserInfo)
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


}