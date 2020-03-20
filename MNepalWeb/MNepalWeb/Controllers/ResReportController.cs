using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MNepalWeb.Models;
using MNepalWeb.ViewModel;
using MNepalWeb.Utilities;
using MNepalWeb.UserModels;
using Newtonsoft.Json;
using System.Globalization;
using System.Dynamic;
using System.Data;
using System.Reflection;

namespace MNepalReport.Controllers
{
    public class ResReportController : Controller
    {
        [HttpGet]
        public ActionResult ResponseReport()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                //ViewBag.Name = name;
                ViewBag.Error = "";
                CustReport para = new CustReport();
                para.StartDate = DateTime.Now.ToString("dd/MM/yyyy");
                para.EndDate = DateTime.Now.ToString("dd/MM/yyyy");
                return View(new CustomerAccountActivityVM
                {
                    Parameter = para,
                    ResponseLog = new List<ResponseLog>(),
                });
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
       
          

        [HttpPost]

        public ContentResult ResponseReportTable(DataTableAjaxPostModel model, string StartDate, string EndDate, string change, string ToExcel)
        {
            if (!Request.IsAjaxRequest())
            {
                Response.Write("Invalid Execution");
                return Content("");
            }

            int filteredResultsCount;
            int totalResultsCount;
            String ParaChanged = "T";
            CustReport ac = new CustReport();
            string convert;
            if (!StartDate.IsValidDate())
            {

                convert = JsonConvert.SerializeObject(new
                {

                    draw = model.draw,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    error = "Not valid Start Date"
                });

                return Content(convert, "application/json");
            }
            if (!EndDate.IsValidDate())
            {

                convert = JsonConvert.SerializeObject(new
                {

                    draw = model.draw,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    error = "Not valid End Date"
                });

                return Content(convert, "application/json");
            }
            
            
            ac.StartDate = DateTime.ParseExact(StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                    .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture); 
            ac.EndDate = DateTime.ParseExact(EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                    .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
            ParaChanged = change;

            var result = new List<ResponseLog>();
            if (Session["CustomerAccActivity"] != null && ParaChanged == "F")
            {
                result = Session["CustomerAccActivity"] as List<ResponseLog>;
            }
            else
            {
                LogUserModel rep = new LogUserModel();
                result = rep.CustomerResLog(ac);
                Session["CustomerAccActivity"] = result;
            }
            if (ToExcel == "T")
            {
                DataTable excel = ToDataTable<ResponseLog>(result);
                //ExtraUtility.DataTableToInMemExcel(excel, "TEST.xls");
                string Handler = Guid.NewGuid().ToString();
                TempData[Handler] = excel;
                string FileName = DateTime.Now + "CustomerResponseLog.xls";
                convert = JsonConvert.SerializeObject(new
                {
                    FileGuid = Handler,
                    FileName = FileName
                });
                return Content(convert, "application/json");
            }
            var res = FilterAndSort<ResponseLog>(model, result, out totalResultsCount, out filteredResultsCount);
            var obj = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();
                resultset.OriginID = item.OriginID;
                resultset.OriginType = item.OriginType;
                resultset.ServiceCode = item.ServiceCode;
                resultset.SourceBankCode = item.SourceBankCode;
                resultset.SourceBranchCode = item.SourceBranchCode;
                resultset.SourceAccountNo = item.SourceAccountNo;
                resultset.DestBankCode = item.DestBankCode;
                //resultset.DestBranchCode = item.DestBranchCode;
                //resultset.DestAccountNo = item.DestAccountNo;
                resultset.Amount = item.Amount;
                resultset.FeeId = item.FeeId;
                resultset.FeeAmount = item.FeeAmount;
                resultset.TraceNo = item.TraceNo;
                resultset.TranDate = item.TranDate.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);//"dd /MM/yyyy", CultureInfo.InvariantCulture);
                //resultset.TranTime = item.TranTime.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);//"dd /MM/yyyy", CultureInfo.InvariantCulture);
                resultset.RetrievalRef = item.RetrievalRef;
                resultset.ResponseCode = item.ResponseCode;
                resultset.ResponseDescription = item.ResponseDescription;
                resultset.Balance = item.Balance;
                resultset.AcHolderName = item.AcHolderName;
                //resultset.MiniStmntRec = item.MiniStmntRec;
                //resultset.ReversalStatus = item.ReversalStatus;
                resultset.TranId = item.TranId;
                obj.Add(resultset);

            }
            convert = JsonConvert.SerializeObject(new
            {

                draw = model.draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = obj
            });

            return Content(convert, "application/json");
        }

        public DataTable ToDataTable<T>(List<T> items)
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

        [HttpGet]
        public void Download(string fileGuid, string fileName)
        {
            if (TempData[fileGuid] != null)
            {
                DataTable result = TempData[fileGuid] as DataTable;
                ExtraUtility.ExportDataTableToExcel(result, fileName);
            }
            else
            {
                Response.Write("Bad Request");
            }
        }

        public List<T> FilterAndSort<T>(DataTableAjaxPostModel model, List<T> source, out int TotalCount, out int Filtered)
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
    }
}

       
        