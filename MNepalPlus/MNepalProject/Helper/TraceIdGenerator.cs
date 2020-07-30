using MNepalProject.DAL;
using System;
using System.Data;
using System.Data.SqlClient;
using MNepalProject.Connection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WCF.MNepal.Helper;

namespace MNepalProject.Helper
{
    public class TraceIdGenerator
    {
        GlobalConnection gc = new GlobalConnection();

        //public int GetID()
        //{
        //    try
        //    {
        //        using(var db=gc.getConnection())
        //        {
        //            var id=0;
        //            var _result = db.Query<MNTraceID>("SELECT TOP 1 id from MNTraceIDHelper ORDER BY id DESC").ToList();
        //            if (_result.Count() == 1)
        //            {
        //                foreach (var item in _result)
        //                {
        //                    id = item.id;
        //                }
        //            }
        //            return id;
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        //public void InsertTraceID(string tID)
        //{
        //    try
        //    {
        //        using(var db=gc.getConnection())
        //        {
        //            var _tID = new MNTraceID()
        //            {
        //                TraceID=tID
        //            };
        //            db.Insert(_tID);
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        throw;
        //    }
        //}

        async Task PutTaskDelay()
        {
            await Task.Delay(1000);// Wait for 1sec
        }

        public string GenerateTraceID()
        {
            //var id=(this.GetID()+1).ToString();

            //var _traceID = id.PadLeft(12, '0');// + '1';

            //this.InsertTraceID(_traceID);

            //return _traceID;

            PutTaskDelay();
            
            long numOfTicks = DateTime.Now.Ticks;

            string num = numOfTicks.ToString();
            string lasttwelveDigits = num.Substring((num.Length - 12), 12);

            string output = lasttwelveDigits;// outputcolon;

            this.InsertTraceUID(output);
            var _traceID = output;

            return _traceID;
        }


        /*public int GetID()
        {
            SqlConnection conn = null;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionString()))
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
        }*/


        public void InsertTraceID(string tID)
        {
            SqlConnection conn = null;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionString()))
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

        //START FOR UNIQUE TRACE ID FROM DB
        public int GetTraceID()
        {
            SqlConnection conn = null;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionString()))
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


        /***
         * Generate N Trace ID 
         */
        public void InsertTraceUID(string tID)
        {
            SqlConnection conn = null;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionString()))
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

        public string GenerateUniqueTraceID()
        {
            long numOfTicks = DateTime.Now.Ticks;
                        
            string num = numOfTicks.ToString();
            string lasttwelveDigits = num.Substring((num.Length - 12), 12);

            string output = lasttwelveDigits;

            InsertTraceUID(output);
            var _traceID = output;
            return _traceID;
        }
        //END FOR UNIQUE TRACE ID FROM DB

        public string GenerateUTraceIDByDate()
        {
            DateTime dateNow = DateTime.Now;
            string outputdate = String.Format("{0:dd/MM/yy HH:mm:ss}", dateNow); //{0:dd/MM/yy HH:mm:ss}

            string outputNospace = Regex.Replace(outputdate, " ", "");
            string outputslash = Regex.Replace(outputNospace, "/", "");
            string outputcolon = Regex.Replace(outputslash, ":", "");

            string output = outputcolon;

            var _traceID = output;

            return _traceID;
        }


        //Generate OTP KEY
        public string GetUniqueOTPKey()
        {
            int maxSize = 6;
            //int minSize = 5;
            char[] chars = new char[62];
            string a;
            a = "1234567890";
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


        /*
        //Generate Token
        public string GetUniqueKey()
        {
            int maxSize = 6;
            //int minSize = 5;
            char[] chars = new char[62];
            string a;
            a = "1234567890";
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
        */

        //Generate Token
        public static string GetReqTokenCode()
        {
            int maxSize = 6;
            //int minSize = 5;
            char[] chars = new char[62];
            string a;
            a = "1234567890";
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

        /*
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
        */

        public string GetUniqueChar()
        {
            int maxSize = 6;
            //int minSize = 5;
            char[] chars = new char[62];
            string a;
            a = "1234567890abcdefghijklmnopqrstuvwxyz";
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