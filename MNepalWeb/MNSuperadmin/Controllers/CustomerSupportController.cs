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
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace MNSuperadmin.Controllers
{
    public class CustomerSupportController : Controller
    {
        // GET: CustomerSupport
        #region Customer Support 
        [HttpGet]
        public ActionResult Index()
        {

            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;
            //Check Role link start
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkIndexRole(MethodBase.GetCurrentMethod().Name, clientCode, this.ControllerContext.RouteData.Values["controller"].ToString());
            //Check Role link end
            if (TempData["userType"] != null&& checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;
                 
                CustomerSupport para = new CustomerSupport();
               
                 return View(new CustomerSupportVM
                 {
                    Parameter = para,
                     CustomerDataNew = new List<CustomerDataNew>()

                });
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        public ContentResult CustomerSupportTable(DataTableAjaxPostModel model, string MobileNumber, /*string CustomerName, */string Category, /*string HasKYC,*/ string change, string ToExcel)
        {
            if (!Request.IsAjaxRequest())
            {
                Response.Write("Invalid Execution");
                return Content("");
            }

            int filteredResultsCount;
            int totalResultsCount;
            string ParaChanged = "T"; 
            CustomerSupport ac = new CustomerSupport();
            string convert;
            if (Session["UserName"] == null)
            {

                convert = JsonConvert.SerializeObject(new
                {
                    draw = model.draw,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    error = "Session Expired, Please re-login."
                });

                return Content(convert, "application/json");

            }

             
            ac.MobileNumber = MobileNumber;
            
            ac.Category = Category;
           
            //ac.MobileNumber = (string)Session["UserName"];
            ParaChanged = change;
            var result = new List<CustomerDataNew>();
            if (Session["CustomerDataNew"] != null && ParaChanged == "F")
            {
                result = Session["CustomerDataNew"] as List<CustomerDataNew>;
            }
            else
            {
                
                CustomerSupportUserModel rep = new CustomerSupportUserModel();
                result = rep.CustomerSupportSearchUserModel(ac);
                
                Session["CustomerData"] = result;
            }

            if (ToExcel == "T")
            {
                DataTable excel = ToDataTable<CustomerDataNew>(result);
                //ExtraUtility.DataTableToInMemExcel(excel, "TEST.xls");
                string Handler = Guid.NewGuid().ToString();
                TempData[Handler] = excel;
                string FileName = DateTime.Now + "_CustomerData.xls";
                convert = JsonConvert.SerializeObject(new
                {
                    FileGuid = Handler,
                    FileName = FileName
                });
                return Content(convert, "application/json");
            }
            var res = FilterAndSort<CustomerDataNew>(model, result, out totalResultsCount, out filteredResultsCount);
            var resultList = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();

                resultset.SupportId = item.SupportId;

                resultset.MobileNumber = item.MobileNumber;
                resultset.Category = item.Category;

                //resultset.Email = item.Email;
                //resultset.Remarks = item.Remarks;



                //resultset.CreatedBy = item.CreatedBy;
                //resultset.HasKYC = item.HasKYC;
                //resultset.ClientCode = item.ClientCode;
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


        public ActionResult ViewCustomerSupport(string id)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            string mobile = (string)Session["Mobile"];

            if (this.TempData["custmodify_messsage"] != null)
            {
                this.ViewData["custmodify_messsage"] = this.TempData["custmodify_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;
                 
                CustomerSupport srInfo = new CustomerSupport(); 

                //DataSet DSet = ProfileUtils.GetCustomerSupport(id);
                DataSet DSet = CustomerSupportUtils.CustomerSupportDetails(id);

                DataTable dtableSrInfo = DSet.Tables["dtSrInfo"];

                if (dtableSrInfo != null && dtableSrInfo.Rows.Count > 0)
                {
                   
                    
                    srInfo.MobileNumber = dtableSrInfo.Rows[0]["MobileNumber"].ToString();

                    srInfo.Category = dtableSrInfo.Rows[0]["Category"].ToString();

                    srInfo.CSName = dtableSrInfo.Rows[0]["Name"].ToString();

                    srInfo.Email = dtableSrInfo.Rows[0]["Email"].ToString();

                    srInfo.Remarks = dtableSrInfo.Rows[0]["Remarks"].ToString();

                    srInfo.ImageName = dtableSrInfo.Rows[0]["ImageName"].ToString();
                    ViewBag.ImageName = srInfo.ImageName;

                }
                return View(srInfo);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }



        #endregion

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