using ThailiMNepalAgent.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Serialization;

namespace ThailiMNepalAgent.Utilities
{
    public static class ExtraUtility
    {
        public static bool IsInCollection(this string objecttofind, string[] collection)
        {
            int pos = Array.IndexOf(collection, objecttofind);
            if (pos > -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static string Encrypt(string input)
        {
            byte[] xorConstant = new byte[] { 0x79, 0x58, 0x25, 0x99 };

            var j = 0;
            byte[] data = Encoding.UTF8.GetBytes(input);
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Convert.ToByte(data[i] ^ xorConstant[j]);
                j++;
                if (j >= xorConstant.Length)
                    j = 0;
            }
            return Convert.ToBase64String(data);
        }

        public static string Decrypt(string input)
        {
            byte[] xorConstant = new byte[] { 0x79, 0x58, 0x25, 0x99 };
            byte[] data = Convert.FromBase64String(input);
            var j = 0;
            for (int i = 0; i < data.Length; i++)
            {

                data[i] = Convert.ToByte(data[i] ^ xorConstant[j]);
                j++;
                if (j >= xorConstant.Length)
                    j = 0;
            }
            return Encoding.UTF8.GetString(data);
        }


        public static string Capitalize(this string inputstring)
        {
            if (string.IsNullOrEmpty(inputstring))
            {
                return null;
            }
            string[] splitStr = inputstring.Split(' ');
            string[] Joinstr = new string[splitStr.Length];
            
            for (int i = 0; i < splitStr.Length; i++)
            {
                if (!string.IsNullOrEmpty(splitStr[i].Trim()))
                    Joinstr[i] = splitStr[i].First().ToString().ToUpperInvariant() + splitStr[i].Substring(1).ToLowerInvariant();
            }

            return string.Join(" ", Joinstr);
        }

        /// <summary>
        /// Checcks if the input date is in DD/mm/yyyy format
        /// </summary>
        /// <param name="InputDate"></param>
        /// <returns></returns>
        public static bool IsValidDate(this string InputDate)
        {
            DateTime d;
            bool isValid= DateTime.TryParseExact(InputDate,"dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out d);
            return isValid;
        }

        public  static string ToMoneyFormat()
        {
            return "";
        }

        public static string ToMMddYYYY(string DateInDDMMYYY)
        {
          return  DateTime.ParseExact(DateInDDMMYYY, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                  .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
        }
        public static bool HasSessionKey(string Key)
        {
            if (HttpContext.Current.Session[Key] != null)
                return true;
            else
                return false;

        }
        public static void ExportDataTableToExcel(DataTable source, string filename)
        {
            try
            {
                if (source.Rows.Count == 0)
                    throw new Exception("There are no details to export.");

                foreach (DataRow x in source.Rows)
                {
                    for (int y = 0; y < source.Columns.Count; y++)
                    {
                        System.Type rowType;
                        rowType = x[y].GetType();
                        if (rowType.FullName == "System.String")
                        {
                            x.BeginEdit();
                            x[y] = "&nbsp;" + x[y];
                            x.AcceptChanges();
                        }
                    }
                }

                HttpResponse response = HttpContext.Current.Response;

                response.Clear();
                response.Charset = "";

                response.ContentType = "application/vnd.ms-excel";
                response.AddHeader("Content-Disposition", "attachment;filename=\"" + filename + "\"");

                using (StringWriter sw = new StringWriter())
                {
                    using (HtmlTextWriter htw = new HtmlTextWriter(sw))
                    {
                        DataGrid dg = new DataGrid();
                        dg.HeaderStyle.Font.Bold = true;
                        dg.HeaderStyle.ForeColor = System.Drawing.Color.DarkMagenta;
                        dg.GridLines = GridLines.None;
                        dg.DataSource = source;
                        dg.DataBind();
                        dg.RenderControl(htw);
                        response.Write(sw.ToString());
                        response.Flush();
                        response.End();
                    }
                }
            }
            catch (ThreadAbortException Ex)
            {
                string ErrMsg = Ex.Message;
            }
            catch (Exception Ex)
            {
                string ErrMsg = Ex.Message;
            }
        }
        public static void DataTableToInMemExcel(DataTable source, string filename)
        {
            try
            {
                if (source.Rows.Count == 0)
                    throw new Exception("There are no details to export.");

                foreach (DataRow x in source.Rows)
                {
                    for (int y = 0; y < source.Columns.Count; y++)
                    {
                        System.Type rowType;
                        rowType = x[y].GetType();
                        if (rowType.FullName == "System.String")
                        {
                            x.BeginEdit();
                            x[y] = "&nbsp;" + x[y];
                            x.AcceptChanges();
                        }
                    }
                }
                byte[] bytes = null;
                using (var ms = new MemoryStream())
                {
                    using (StreamWriter sw = new StreamWriter(ms))
                    {
                        using (HtmlTextWriter htw = new HtmlTextWriter(sw))
                        {
                            DataGrid dg = new DataGrid();
                            dg.HeaderStyle.Font.Bold = true;
                            dg.HeaderStyle.ForeColor = System.Drawing.Color.DarkMagenta;
                            dg.GridLines = GridLines.None;
                            dg.DataSource = source;
                            dg.DataBind();
                            dg.RenderControl(htw);
                            ms.Position = 0;
                            bytes = ms.ToArray();
                            //or save to disk using FileStream (fs)
                            //ms.WriteTo();
                        }
                    }
                }
            
         
         
              
            }

            catch (ThreadAbortException Ex)
            {
                string ErrMsg = Ex.Message;
            }
            catch (Exception Ex)
            {
                string ErrMsg = Ex.Message;
            }
        }

        public static bool IsValidDatatable(DataTable Table, bool IgnoreRows = false)
        {
            if (Table == null) return false;
            if (Table.Columns.Count == 0) return false;
            if (!IgnoreRows && Table.Rows.Count == 0) return false;
            return true;
        }
        public static List<T> DatatableToListClass<T>(DataTable Table) where T : class, new()
        {
            if (!IsValidDatatable(Table))
                return new List<T>();

            Type classType = typeof(T);
            IList<PropertyInfo> propertyList = classType.GetProperties();

            // Parameter class has no public properties.
            if (propertyList.Count == 0)
                return new List<T>();

            List<string> columnNames = Table.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToList();

            List<T> result = new List<T>();
            try
            {
                foreach (DataRow row in Table.Rows)
                {
                    T classObject = new T();
                    foreach (PropertyInfo property in propertyList)
                    {
                        if (property != null && property.CanWrite)   // Make sure property isn't read only
                        {
                            if (columnNames.Contains(property.Name))  // If property is a column name
                            {
                                if (row[property.Name] != System.DBNull.Value)   // Don't copy over DBNull
                                {
                                    var type = Nullable.GetUnderlyingType(property.PropertyType);
                                    object propertyValue = System.Convert.ChangeType(
                                            row[property.Name],
                                          type == null ? property.PropertyType : type
                                        );
                                    property.SetValue(classObject, propertyValue, null);
                                }
                            }
                        }
                    }
                    result.Add(classObject);
                }
                return result;
            }
            catch
            {
                return new List<T>();
            }
        }


        public static T DatatableToSingleClass<T>(DataTable Table) where T : class, new()
        {
            if (!IsValidDatatable(Table))
                return new T();

            Type classType = typeof(T);
            IList<PropertyInfo> propertyList = classType.GetProperties();

            // Parameter class has no public properties.
            if (propertyList.Count == 0)
                return new T();

            List<string> columnNames = Table.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToList();

            T result = new T();
            try
            {
                foreach (DataRow row in Table.Rows)
                {
                    T classObject = new T();
                    foreach (PropertyInfo property in propertyList)
                    {
                        if (property != null && property.CanWrite)   // Make sure property isn't read only
                        {
                            if (columnNames.Contains(property.Name))  // If property is a column name
                            {
                                if (row[property.Name] != System.DBNull.Value)   // Don't copy over DBNull
                                {
                                    var type= Nullable.GetUnderlyingType(property.PropertyType);
                                    object propertyValue = System.Convert.ChangeType(
                                            row[property.Name],
                                           type==null?property.PropertyType:type
                                        );
                                    property.SetValue(classObject, propertyValue, null);
                                }
                            }
                        }
                    }
                    result = classObject;
                }
                return result;
            }
            catch (Exception ex)
            {
                return new T();
            }
        }
        // used for server side datatable.js
        public static List<T> FilterAndSort<T>(DataTableAjaxPostModel model, List<T> source, out int TotalCount, out int Filtered)
        {

            int skip = model.start;
            int take = model.length;

            string sortBy = "";
            bool sortDir = true;
            var filter = source.AsQueryable();
            Func<T, Object> orderByFunc = null;
            if (model.order != null)
            {
                sortBy = model.columns[model.order[0].column].data;
                sortDir = model.order[0].dir.ToLower() == "asc";
                orderByFunc = p => p.GetType().GetProperty(sortBy).GetValue(p, null);
            }
            if (orderByFunc != null)
            {
                if (sortDir)
                    filter = filter.OrderBy(orderByFunc).AsQueryable();
                else
                    filter = filter.OrderByDescending(orderByFunc).AsQueryable();
            }
            TotalCount = source.Count;
            Filtered = filter.Count();
            var res = filter.Skip(skip).Take(take).ToList();

            return res;
        }

        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable();

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            foreach (var column in dataTable.Columns.Cast<DataColumn>().ToArray())
            {
                if (dataTable.AsEnumerable().All(dr => dr.IsNull(column)))
                    dataTable.Columns.Remove(column);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }
        //public static string ListClassToXML<T>(this List<T> list)
        //{
        //    if (list.Count <= 0)
        //    {
        //        return null;
        //    }
        //    try
        //    {
        //        var stringwriter = new System.IO.StringWriter();
        //        var serializer = new XmlSerializer(typeof(T));
        //        serializer.Serialize(stringwriter, list);
        //        return stringwriter.ToString();
        //    }
        //    catch
        //    {
        //        throw;
        //    }

        //}

    }
}

