using MNSuperadmin.Helper;
using MNSuperadmin.Models;
using MNSuperadmin.UserModels;
using MNSuperadmin.Utilities;
using MNSuperadmin.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace MNSuperadmin.Controllers
{
    public class TransactionController : Controller
    {
        // GET: Transaction

        #region Transaction
        [HttpGet]
        public ActionResult Index()
    {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            //Check Role link start          
            string methodlink = System.Reflection.MethodBase.GetCurrentMethod().Name;
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkRole(methodlink, clientCode);
            //Check Role link end


            TempData["userType"] = userType;

            if (TempData["userType"] != null && checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;
                ViewBag.MType = "";

                MerchantPay para = new MerchantPay();
                para.StartDate = DateTime.Now.ToString("dd/MM/yyyy");
                para.EndDate = DateTime.Now.ToString("dd/MM/yyyy");
                ReportUserModel rep = new ReportUserModel();
                para.MerchantTypeList = rep.GetMerchantsType();

                ViewBag.MerchantCategory = MerchantUtils.GetMerchantsType();
                return View(new MerchantVM
                {
                    Parameter = para,
                    MerchantInfo = new List<MerchantInfo>()

                });



            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }

        [HttpPost]
        public ActionResult Index(MerchantVM model)
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
                ViewBag.Name = name;
                ViewBag.MType = "";

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

            string start = "", end = "";

            if (!string.IsNullOrEmpty(model.Parameter.StartDate))

            {
                if (!model.Parameter.StartDate.IsValidDate())
                {
                    ViewBag.Error = "Not valid start date";
                    return View(new MerchantVM
                    {
                        Parameter = new MerchantPay(),
                        MerchantInfo = new List<MerchantInfo>()

                    });
                }
                start = DateTime.ParseExact(model.Parameter.StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                  .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(model.Parameter.EndDate))
            {
                if (!model.Parameter.EndDate.IsValidDate())
                {
                    ViewBag.Error = "Not valid end date";
                    return View(new MerchantVM
                    {
                        Parameter = new MerchantPay { MerchantTypeList = new List<ViewModel.Merchants>() },
                        MerchantInfo = new List<MerchantInfo>()

                    });
                }
                end = DateTime.ParseExact(model.Parameter.EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                    .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

            }

            ViewBag.Error = "";
            ViewBag.message = "T";


            model.Parameter.StartDate = start;
            model.Parameter.EndDate = end;

            ReportUserModel rep = new ReportUserModel();
            List<MerchantInfo> report = rep.SummaryDetails(model.Parameter);
            MerchantVM vm = new MerchantVM();
            vm.MerchantInfo = report;
            vm.Parameter = model.Parameter;
            return View(vm);


        }

        static readonly string[] Months = new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

        public ContentResult SummaryTable(DataTableAjaxPostModel model, string StartDate, string EndDate, string Service, string MerchantName, string GroupBy, string change, string ToExcel, string ContactNumber)
        {
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
            MerchantPay ac = new MerchantPay();
            ac.StartDate = StartDate;
            ac.EndDate = EndDate;
            ac.Service = Service;
            ac.MerchantName = MerchantName;
            ac.GrpByDate = GroupBy;
            var result = new List<MerchantInfo>();
            if (Session["Summary"] != null && ParaChanged == "F")
            {
                result = Session["Summary"] as List<MerchantInfo>;
            }
            else
            {
                ReportUserModel rep = new ReportUserModel();
                result = rep.SummaryDetails(ac);
                Session["Summary"] = result;

            }
            if (ToExcel == "T")
            {

                List<MerchantInfo> modifiedMI = result.ToList();
                foreach (MerchantInfo item in result)
                {
                    int i = int.Parse(item.SalesMonth.Trim() == "" ? "0" : item.SalesMonth.Trim());
                    if (item.SalesMonth.Length <= 2 && item.SalesMonth.Length > 0)
                        item.SalesMonth = Months[i - 1];
                }

                DataTable excel = ToDataTable<MerchantInfo>(result);
                excel.Columns.Remove("DatenTime");
                excel.Columns.Remove("Amount");
                string Handler = Guid.NewGuid().ToString();
                TempData[Handler] = excel;
                string FileName = DateTime.Now + "_Summary.xls";
                convert = JsonConvert.SerializeObject(new
                {
                    FileGuid = Handler,
                    FileName = FileName
                });
                return Content(convert, "application/json");
            }
            var res = FilterAndSort<MerchantInfo>(model, result, out totalResultsCount, out filteredResultsCount);
            var resultList = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();
                resultset.MerchantName = item.MerchantName;
                resultset.ServiceType = item.ServiceType;
                resultset.TotalAmount = item.TotalAmount;
                resultset.NoOfTran = item.NoOfTran;
                resultset.SalesYear = item.SalesYear;
                resultset.SalesMonth = item.SalesMonth;
                resultList.Add(resultset);
            }
            convert = JsonConvert.SerializeObject(new
            {
                draw = model.draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = resultList,
                GroupedBy = ac.GrpByDate,
                StartDate = ac.StartDate,
                EndDate = ac.EndDate
            });
            return Content(convert, "application/json");

        }

        #endregion

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


    }
}