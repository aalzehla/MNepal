using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ThailiMNepalApp.Models;
using ThailiMNepalApp.ViewModel;
using ThailiMNepalApp.UserModels;
using ThailiMNepalApp.Utilities;
using System.Globalization;
using System.Data;
using Newtonsoft.Json;
using System.Dynamic;
using System.Reflection;

using System.Text.RegularExpressions;

namespace ThailiMNepalApp.Controllers {
    public class AgentDashboardController : Controller {
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
        public ActionResult Index()

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
                ViewBag.COC = Session["COC"];
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }
 
        public ContentResult AgentDashboardTable(DataTableAjaxPostModel model, string ContactNumber1, string StartDate, string EndDate, string TranID, string change, string ToExcel)
        {
            if (!Request.IsAjaxRequest())
            {
                Response.Write("Invalid Execution");
                return Content("");
            }

            int filteredResultsCount;
            int totalResultsCount;
            string ParaChanged = "T";
            string convert;
            if (!string.IsNullOrEmpty(StartDate))
            {
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
                StartDate = DateTime.ParseExact(StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
            }
            
            if (!string.IsNullOrEmpty(EndDate))
            {
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
                EndDate = DateTime.ParseExact(EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
            }
            ParaChanged = change;
            var result = new List<MNAgentStatement>();
            if (Session["AdminActivity"] != null && ParaChanged == "F")
            {
                result = Session["AdminActivity"] as List<MNAgentStatement>;
            }
            else
            {
                ReportUserModel rep = new ReportUserModel();
                result = rep.GetAgentStatement(ContactNumber1, StartDate, EndDate, TranID);
                Session["AdminActivity"] = result;

            }
            if (ToExcel == "T")
            {

                DataTable excel = ToDataTable<MNAgentStatement>(result);
                string Handler = Guid.NewGuid().ToString();
                TempData[Handler] = excel;
                string FileName = DateTime.Now + "_AdminActivity.xls";
                convert = JsonConvert.SerializeObject(new
                {
                    FileGuid = Handler,
                    FileName = FileName
                });
                return Content(convert, "application/json");
            }
            var res = FilterAndSort<MNAgentStatement>(model, result, out totalResultsCount, out filteredResultsCount);
            var resultList = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();

                resultset.Date = item.Date;
                resultset.TranID = item.TranID;
               
                resultset.Description = item.Description;
                resultset.Debit = item.Debit;
                resultset.Credit = item.Credit;
                //resultset.Balance = item.Balance;

                //resultset.Remarks = string.Join(" ",item.Remarks,item.Description);
               // resultset.TimeStamp = item.TimeStamp;
                resultList.Add(resultset);
            }
            convert = JsonConvert.SerializeObject(new
            {
                draw = model.draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = resultList
            });
            return Content(convert, "application/json");

        }
        
    }
}