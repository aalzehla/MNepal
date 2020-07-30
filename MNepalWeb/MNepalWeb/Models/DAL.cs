using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;


//

using System.Collections;

//
namespace MNepalWeb.Models
{
    public class DAL
    {
        DataSet ds;
        SqlDataAdapter da;

        public static SqlConnection MNepalDBConnectionString()
        {
            //Reading the connection string from web.config    
            string Name = Connection.DatabaseConnection.ConnectionString; 
            //Passing the string in sqlconnection.    
            SqlConnection con = new SqlConnection(Name);
            //Check wheather the connection is close or not if open close it else open it    
            if (con.State == ConnectionState.Open)
            {
                con.Close();

            }
            else
            {

                con.Open();
            }
            return con;

        }
        //Creating a method which accept any type of query from controller to execute and give result.    
        //result kept in datatable and send back to the controller.    
        public DataTable MyMethod(string Query)
        {
            DataTable dt = new DataTable();
            try
            {
                ds = new DataSet();
                da = new SqlDataAdapter(Query, DAL.MNepalDBConnectionString());
                da.Fill(dt);
            }
            catch (Exception e) {
                throw e;
            }
            return dt;

        }
    }
}