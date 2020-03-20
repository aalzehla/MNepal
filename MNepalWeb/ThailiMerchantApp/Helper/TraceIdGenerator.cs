using ThailiMerchantApp.Connection;
using ThailiMerchantApp.Models;
using ThailiMerchantApp.UserModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ThailiMerchantApp.Helper
{
    public class TraceIdGenerator
    {
        public static int GetID()
        {
            SqlConnection conn = null;
            try
            {   
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    int id = 0;
                    DataTable dtableResult = new DataTable();
                    using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 id from MNTraceIDHelper ORDER BY id DESC", conn))
                    {
                        using (SqlDataAdapter da = new SqlDataAdapter())
                        {
                            da.SelectCommand = cmd;
                            da.Fill(dtableResult);
                                    
                            if (dtableResult.Rows.Count == 1)
                            {
                                id = Convert.ToInt32(dtableResult.Rows[0]["id"].ToString());
                            }
                                    
                        }
                        
                    }
                    return id;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }
        }


        public void InsertTraceID(string tID)
        {
            SqlConnection conn = null;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    string sqlInsert = "INSERT INTO MNTraceIDHelper VALUES(@tID)";
                    using (SqlCommand cmd = new SqlCommand(sqlInsert, conn))
                    {
                        cmd.Parameters.Add("@tID", SqlDbType.VarChar).Value = tID;
        
                        cmd.CommandType = CommandType.Text;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }
        }


        public string GenerateTraceID()
        {
            var id = (GetID() + 1).ToString();//this.

            var _traceID = id.PadLeft(11, '0') + 'W';

            this.InsertTraceID(_traceID);

            return _traceID;
        }

        //Generate Token
        public static string GetUniqueKey()
        {
            int maxSize = 6;
            //int minSize = 5;
            char[] chars = new char[62];
            string a;
            a = "1234567890";//ABCDEFGHIJKLMNOPQRSTUVWXYZ
            chars = a.ToCharArray();
            int size = maxSize;
            byte[] data = new byte[1];

            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                size = maxSize;
                data = new byte[maxSize];
                crypto.GetNonZeroBytes(data);
            }

            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }

        //Generate Unique ID FOR WALLET NUMBER
        public static string GetUniqueWalletNo()
        {
            int maxSize = 8;
            //int minSize = 5;
            char[] chars = new char[62];
            string a;
            a = "1234567890";//ABCDEFGHIJKLMNOPQRSTUVWXYZ
            chars = a.ToCharArray();
            int size = maxSize;
            byte[] data = new byte[1];

            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                size = maxSize;
                data = new byte[maxSize];
                crypto.GetNonZeroBytes(data);
            }

            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }

    }
}