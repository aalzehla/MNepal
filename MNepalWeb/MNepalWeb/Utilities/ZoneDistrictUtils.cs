using MNepalWeb.Connection;
using MNepalWeb.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;


namespace MNepalWeb.Utilities
{
    public class ZoneDistrictUtils
    {
        //List Zone
        public static IEnumerable<MNZone> GetMNZone()
        {
            string ConnectionString = DatabaseConnection.ConnectionStr().ToString();
            List<MNZone> MNZone = new List<MNZone>();
            try
            {

                string Sql = @"SELECT  MZ.ZoneID, MZ.Name
                               FROM  dbo.MNZone AS MZ ( NOLOCK ) ";

                DataTable dtbl = Utilities.SqlHelper.SqlHelper.ExecuteDataTable(ConnectionString, CommandType.Text, Sql);
                if (dtbl.Rows.Count > 0)
                {
                    for (int i = 0; i < dtbl.Rows.Count; i++)
                    {
                        MNZone.Add(
                            new MNZone
                            {
                                ZoneID = dtbl.Rows[i]["ZoneID"].ToString(),
                                ZoneName = dtbl.Rows[i]["Name"].ToString()
                            });
                    }
                }

            }

            catch (SqlException ex)
            {
                throw ex;
            }

            return MNZone;
        }

    }
}