using MNepalProject.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalProject.Models;
using System.Data.SqlClient;
using System.Configuration;

namespace MNepalProject.Services
{
    public class InsertDataForIVR
    {

        public bool InsertData(string tid, string sourcemobile, string isprocessed, DateTime date)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString;
            SqlConnection sqlConnection = new SqlConnection(connectionString);


            //Insert into MNIVRLog
            string statement = "INSERT INTO MNIVRLOG(traceid, sourcemobile,datetime,isprocessed) VALUES(@tid, @sourcemobile,@date,@isprocessed)";
            SqlCommand command = new SqlCommand();
            command.CommandType = System.Data.CommandType.Text;
            command.CommandText = statement;
            command.Connection = sqlConnection;
            command.CommandTimeout = 60;
            command.Parameters.AddWithValue("@tid", tid);
            command.Parameters.AddWithValue("@sourcemobile", sourcemobile);
            command.Parameters.AddWithValue("@date", date);
            command.Parameters.AddWithValue("@isprocessed", isprocessed);
            sqlConnection.Open();
            command.ExecuteNonQuery();
            sqlConnection.Close();



            //string stmt = "INSERT INTO MNIVRIN(traceid, sourcemobile,isprocessed,datetime) VALUES(@tid, @sourcemobile, @isprocessed, @date) WITH(NOLOCK)";
            string stmt = "INSERT INTO MNIVRIN(traceid, sourcemobile,isprocessed,datetime) VALUES(@tid, @sourcemobile, @isprocessed, @date)";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = stmt;
            cmd.Connection = sqlConnection;
            cmd.CommandTimeout = 60;
            cmd.Parameters.AddWithValue("@tid", tid);
            cmd.Parameters.AddWithValue("@sourcemobile", sourcemobile);
            cmd.Parameters.AddWithValue("@isprocessed", isprocessed);
            cmd.Parameters.AddWithValue("@date", date);
            sqlConnection.Open();
            cmd.ExecuteNonQuery();
            sqlConnection.Close();

            try
            {
                return true;
            }

            catch (Exception ex)
            {
                return false;
            }



           
            



        }


    }
          

}