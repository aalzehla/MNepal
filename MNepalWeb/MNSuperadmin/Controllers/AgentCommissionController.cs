using KellermanSoftware.CompareNetObjects;
using MNSuperadmin.Helper;
using MNSuperadmin.Models;
using MNSuperadmin.Settings;
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
    public class AgentCommissionController : Controller
    {


        #region Agent Commission 
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
            bool checkRole = roleChecker.checkIndexRole(System.Reflection.MethodBase.GetCurrentMethod().Name, clientCode, this.ControllerContext.RouteData.Values["controller"].ToString());
            //Check Role link end
            if (TempData["userType"] != null&& checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;
                CustReport para = new CustReport();
                return View(new RegCusDetailVM
                {
                    Parameter = para,
                    CustomerData = new List<CustomerData>()

                });
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        public ContentResult AgentCommisionTable(DataTableAjaxPostModel model, string Id, string change, string ToExcel)
        {
            if (!Request.IsAjaxRequest())
            {
                Response.Write("Invalid Execution");
                return Content("");
            }

            int filteredResultsCount;
            int totalResultsCount;
            string ParaChanged = "T";
            CustReport ac = new CustReport();
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

            ac.Id = Id;
            //ac.CustomerName = CustomerName;
            //ac.HasKYC = HasKYC;
            ac.UserName = (string)Session["UserName"];
            ParaChanged = change;
            var result = new List<CustomerData>();
            if (Session["CustomerData"] != null && ParaChanged == "F")
            {
                result = Session["CustomerData"] as List<CustomerData>;
            }
            else
            {
                ReportUserModel rep = new ReportUserModel();
                result = rep.AgentCommissionDetail(ac);
                Session["CustomerData"] = result;
            }

            if (ToExcel == "T")
            {
                DataTable excel = ToDataTable<CustomerData>(result);
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
            var res = FilterAndSort<CustomerData>(model, result, out totalResultsCount, out filteredResultsCount);
            var resultList = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();

                resultset.Id = item.Id;
                resultset.FeeId = item.FeeId;
                resultset.TieredStart = item.TieredStart;

                resultset.TieredEnd = item.TieredEnd;
                resultset.MinAmt = item.MinAmt;
                resultset.MaxAmt = item.MaxAmt;

                resultset.Percentage = item.Percentage;
                resultset.FlatFee = item.FlatFee;
                resultset.FeeType = item.FeeType;


                //resultset.CustomerName = item.CustomerName;
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

        public ContentResult AgentCommisionRejectedTable(DataTableAjaxPostModel model, string Id, string change, string ToExcel)
        {
            if (!Request.IsAjaxRequest())
            {
                Response.Write("Invalid Execution");
                return Content("");
            }

            int filteredResultsCount;
            int totalResultsCount;
            string ParaChanged = "T";
            CustReport ac = new CustReport();
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

            ac.Id = Id;
            //ac.CustomerName = CustomerName;
            //ac.HasKYC = HasKYC;
            ac.UserName = (string)Session["UserName"];
            ParaChanged = change;
            var result = new List<CustomerData>();
            if (Session["CustomerData"] != null && ParaChanged == "F")
            {
                result = Session["CustomerData"] as List<CustomerData>;
            }
            else
            {
                ReportUserModel rep = new ReportUserModel();
                result = rep.AgentCommissionRejectedDetail(ac);
                Session["CustomerData"] = result;
            }

            if (ToExcel == "T")
            {
                DataTable excel = ToDataTable<CustomerData>(result);
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
            var res = FilterAndSort<CustomerData>(model, result, out totalResultsCount, out filteredResultsCount);
            var resultList = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();

                resultset.Id = item.Id;
                resultset.FeeId = item.FeeId;
                resultset.TieredStart = item.TieredStart;

                resultset.TieredEnd = item.TieredEnd;
                resultset.MinAmt = item.MinAmt;
                resultset.MaxAmt = item.MaxAmt;

                resultset.Percentage = item.Percentage;
                resultset.FlatFee = item.FlatFee;
                resultset.FeeType = item.FeeType;


                //resultset.CustomerName = item.CustomerName;
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



        #region ViewServiceCommissionDetail
        // GET: AgentCommission/ServiceCommissionDetail
        [HttpGet]
        public ActionResult ViewServiceCommissionDetail(string id)
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

                CustomerSRInfo srInfo = new CustomerSRInfo();
                //DataSet DSet = ProfileUtils.GetSelfRegDetailDS(id);
                DataSet DSet = ProfileUtils.GetSelfRegDetailDSWalletCustStatusAgentCommission(id);

                DataTable dtableSrInfo = DSet.Tables["dtSrInfo"];

                if (dtableSrInfo != null && dtableSrInfo.Rows.Count > 0)
                {
                    srInfo.Id = dtableSrInfo.Rows[0]["Id"].ToString();

                    srInfo.FeeId = dtableSrInfo.Rows[0]["FeeId"].ToString();


                    srInfo.TieredStart = dtableSrInfo.Rows[0]["TieredStart"].ToString();
                    srInfo.TieredEnd = dtableSrInfo.Rows[0]["TieredEnd"].ToString();
                    srInfo.MinAmt = dtableSrInfo.Rows[0]["MinAmt"].ToString();

                    srInfo.MaxAmt = dtableSrInfo.Rows[0]["MaxAmt"].ToString();

                    srInfo.Percentage = dtableSrInfo.Rows[0]["Percentage"].ToString();
                    srInfo.FlatFee = dtableSrInfo.Rows[0]["FlatFee"].ToString();
                    srInfo.FeeType = dtableSrInfo.Rows[0]["FeeType"].ToString();

                }
                return View(srInfo);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        // POST: AgentCommission/ServiceCommissionDetail
        [HttpPost]
        public ActionResult ViewServiceCommissionDetail(string btnApprove, FormCollection collection)
        {
            UserInfo userInfoModify = new UserInfo();
            bool isUpdated = false;
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            string displayMessage = null;
            string messageClass = null;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                try
                {
                    if (btnApprove == "Modify")
                    {
                        string id = collection["txtId"].ToString();
                        string feeID = collection["FeeId"].ToString();
                        string tieredStart = collection["txtTieredStart"].ToString();
                        string tieredEnd = collection["txtTieredEnd"].ToString();
                        string minAmt = collection["txtMinAmt"].ToString();
                        string maxAmt = collection["txtMaxAmt"].ToString();
                        string percentage = collection["txtPercentage"].ToString();
                        string flatFee = collection["txtFlatFee"].ToString();
                        string feeType = collection["txtFeeType"].ToString();

                        /*New Data*/
                        userInfoModify.CommissionId = id;
                        userInfoModify.FeeID = feeID;
                        userInfoModify.TieredStart = tieredStart;
                        userInfoModify.TieredEnd = tieredEnd;
                        userInfoModify.MinAmt = minAmt;
                        userInfoModify.MaxAmt = maxAmt;
                        userInfoModify.Percentage = percentage;
                        userInfoModify.FlatFee = flatFee;
                        userInfoModify.FeeType = feeType;

                        /*map new data to MNUser*/
                        MNUser userdataNew = new MNUser();
                        MNUser cust = new MNUser();
                        MNFeeDetail mnclientnew = new MNFeeDetail();

                        mnclientnew.CommissionId = userInfoModify.CommissionId;
                        mnclientnew.FeeID = userInfoModify.FeeID;
                        mnclientnew.TieredStart = userInfoModify.TieredStart;
                        mnclientnew.TieredEnd = userInfoModify.TieredEnd;
                        mnclientnew.MinAmt = userInfoModify.MinAmt;
                        mnclientnew.MaxAmt = userInfoModify.MaxAmt;
                        mnclientnew.Percentage = userInfoModify.Percentage;
                        mnclientnew.FlatFee = userInfoModify.FlatFee;
                        mnclientnew.FeeType = userInfoModify.FeeType;

                        userdataNew.MNFeeDetails = mnclientnew;

                        /*old data*/
                        UserInfo userInfo = new UserInfo();
                        MNUser userdataOld = new MNUser();
                        //DataTable dtblRegistration = ProfileUtils.GetSelfRegDetailDSWalletCustStatusAgentCommission(Id);

                        DataSet DSet = ProfileUtils.GetSelfRegDetailDSWalletCustStatusAgentCommission(id);

                        DataTable dtableSrInfo = DSet.Tables["dtSrInfo"];
                        if (dtableSrInfo.Rows.Count == 1)
                        {
                            userInfo.CommissionId = dtableSrInfo.Rows[0]["Id"].ToString();
                            userInfo.FeeID = dtableSrInfo.Rows[0]["FeeId"].ToString();
                            userInfo.TieredStart = dtableSrInfo.Rows[0]["TieredStart"].ToString();
                            userInfo.TieredEnd = dtableSrInfo.Rows[0]["TieredEnd"].ToString();
                            userInfo.MinAmt = dtableSrInfo.Rows[0]["MinAmt"].ToString();
                            userInfo.MaxAmt = dtableSrInfo.Rows[0]["MaxAmt"].ToString();
                            userInfo.Percentage = dtableSrInfo.Rows[0]["Percentage"].ToString();
                            userInfo.FlatFee = dtableSrInfo.Rows[0]["FlatFee"].ToString();
                            userInfo.FeeType = dtableSrInfo.Rows[0]["FeeType"].ToString();

                            MNFeeDetail mnclientold = new MNFeeDetail();
                            mnclientold.CommissionId = userInfo.CommissionId;
                            mnclientold.FeeID = userInfo.FeeID;
                            mnclientold.TieredStart = userInfo.TieredStart;
                            mnclientold.TieredEnd = userInfo.TieredEnd;
                            mnclientold.MinAmt = userInfo.MinAmt;
                            mnclientold.MaxAmt = userInfo.MaxAmt;
                            mnclientold.Percentage = userInfo.Percentage;
                            mnclientold.FlatFee = userInfo.FlatFee;
                            mnclientold.FeeType = userInfo.FeeType;

                            userdataOld.MNFeeDetails = mnclientold;
                            /*map old data to MNUser*/
                        }

                        List<SelectListItem> item = new List<SelectListItem>();
                        DataSet dsBank = BranchUtils.GetDataSetPopulateBranchName();
                        ViewBag.bank = dsBank.Tables[0];
                        foreach (DataRow dr in ViewBag.bank.Rows)
                        {
                            item.Add(new SelectListItem
                            {
                                Text = @dr["BranchName"].ToString(),
                                Value = @dr["BranchCode"].ToString()
                            });
                        }

                        List<SelectListItem> itemProfile = new List<SelectListItem>();
                        DataSet dsProfile = UserProfileUtils.GetDataSetPopulateProfileName();

                        DataRow[] rows = dsProfile.Tables[0].Select(string.Format("ProfileStatus='Active'"));

                        foreach (DataRow dr in rows)
                        {
                            itemProfile.Add(new SelectListItem
                            {
                                Text = @dr["ProfileName"].ToString(),
                                Value = @dr["ProfileCode"].ToString()
                            });
                        }

                        ViewBag.AdminProfile = new SelectList(itemProfile, "Value", "Text", userInfoModify.ProfileName).OrderBy(x => x.Text);
                        ViewBag.BranchName = new SelectList(item, "Value", "Text", userInfoModify.BranchCode).OrderBy(x => x.Text);

                        /*Difference compare*/
                        CompareLogic compareLogic = new CompareLogic();
                        ComparisonConfig config = new ComparisonConfig();
                        config.MaxDifferences = int.MaxValue;
                        config.IgnoreCollectionOrder = false;
                        compareLogic.Config = config;
                        ComparisonResult result = compareLogic.Compare(userdataOld, userdataNew); //  firstparameter orginal,second parameter modified
                        List<MNMakerChecker> makerCheckers = new List<MNMakerChecker>();
                        if (!result.AreEqual)
                        {
                            isUpdated = true;
                            foreach (Difference diff in result.Differences)
                            {
                                MNMakerChecker makerchecker = new MNMakerChecker();
                                int index = diff.PropertyName.IndexOf('.');
                                makerchecker.ColumnName = diff.PropertyName.Substring(index + 1);
                                //makerchecker.Code = oldCust.MNClient.ClientCode;
                                makerchecker.TableName = diff.ParentPropertyName;
                                makerchecker.OldValue = diff.Object1Value;
                                makerchecker.NewValue = diff.Object2Value;
                                makerchecker.Module = "SUPERADMIN";
                                makerCheckers.Add(makerchecker);
                            }
                        }

                        if (isUpdated)
                        {
                           

                            string modifyingAdmin = (string)Session["UserName"];
                            string modifyingBranch = (string)Session["UserBranch"];
                            CusProfileUtils cUtil = new CusProfileUtils();
                            string ModifiedFieldXML = cUtil.GetMakerCheckerXMLStr(makerCheckers);
                            bool inserted = AgentUtils.InsertMakerAgentCommissionChecker(id, modifyingAdmin, modifyingBranch, ModifiedFieldXML);

                            displayMessage = inserted
                                                     ? "Changes will take effect after approval"
                                                     : "Error updating customer, Please try again";
                            messageClass = inserted ? CssSetting.SuccessMessageClass : CssSetting.FailedMessageClass;
                        }
                        else
                        {
                            displayMessage = "Nothing Changed";
                            messageClass = CssSetting.SuccessMessageClass;
                        }

                    }
                }
                catch (Exception ex)
                {
                    displayMessage = ex.Message;
                    messageClass = CssSetting.FailedMessageClass;
                }

                this.TempData["editregister_messsage"] = displayMessage;
                this.TempData["message_class"] = messageClass;
                //if (isUpdated)
                return RedirectToAction("Index", "AgentCommission");
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }

        #endregion

        #region Agent Commission Modification Approval list
        [HttpGet]
        public ActionResult ApproveAgentCommissionList(string Key, string Value)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            string UserName = Value;
            //Check Role link start
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkIndexRole(MethodBase.GetCurrentMethod().Name, clientCode, this.ControllerContext.RouteData.Values["controller"].ToString());
            //Check Role link end
            if (this.TempData["agentapprove_messsage"] != null&& checkRole)
            {
                this.ViewData["agentapprove_messsage"] = this.TempData["agentapprove_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }


            TempData["userType"] = userType;
            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;


                List<UserInfo> agentRegApprove = new List<UserInfo>();
                var agentCommissionInfo = AgentUtils.GetAgentCommissionApprove("agent", UserName);



                ViewBag.Value = Value;
                return View(agentCommissionInfo);

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        //Edit Data//
        // GET: AgentCommission/EditModified
        [HttpGet]
        public ActionResult EditModified(string CommissionId)
        {
            if (string.IsNullOrEmpty(CommissionId))
            {
                return RedirectToAction("Index", "AgentCommission");
            }
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

                //For Branch
                List<SelectListItem> item = new List<SelectListItem>();
                DataSet dsBank = BranchUtils.GetDataSetPopulateBranchName();
                ViewBag.bank = dsBank.Tables[0];
                foreach (DataRow dr in ViewBag.bank.Rows)
                {
                    if (dr["IsBlocked"].ToString() == "F")
                    {
                        item.Add(new SelectListItem
                        {
                            Text = dr["BranchName"].ToString(),
                            Value = dr["BranchCode"].ToString()
                        });
                    }
                }
                //ViewBag.BranchName = item;

                //For ProfileName
                List<SelectListItem> itemProfile = new List<SelectListItem>();
                DataSet dsProfile = UserProfileUtils.GetDataSetPopulateProfileName();
                ViewBag.profile = dsProfile.Tables[0];
                foreach (DataRow dr in ViewBag.profile.Rows)
                {
                    itemProfile.Add(new SelectListItem
                    {
                        Text = @dr["ProfileName"].ToString(),
                        Value = @dr["ProfileCode"].ToString()
                    });
                }


                CustomerSRInfo srInfo = new CustomerSRInfo();
                DataSet DSet = ProfileUtils.GetSelfRegDetailDSWalletCustStatusAgentCommission(CommissionId);

                DataTable dtableSrInfo = DSet.Tables["dtSrInfo"];
                if (dtableSrInfo.Rows.Count == 1)
                {
                    srInfo.Id = dtableSrInfo.Rows[0]["Id"].ToString();

                    srInfo.FeeId = dtableSrInfo.Rows[0]["FeeId"].ToString();


                    srInfo.TieredStart = dtableSrInfo.Rows[0]["TieredStart"].ToString();
                    srInfo.TieredEnd = dtableSrInfo.Rows[0]["TieredEnd"].ToString();
                    srInfo.MinAmt = dtableSrInfo.Rows[0]["MinAmt"].ToString();

                    srInfo.MaxAmt = dtableSrInfo.Rows[0]["MaxAmt"].ToString();

                    srInfo.Percentage = dtableSrInfo.Rows[0]["Percentage"].ToString();
                    srInfo.FlatFee = dtableSrInfo.Rows[0]["FlatFee"].ToString();
                    srInfo.FeeType = dtableSrInfo.Rows[0]["FeeType"].ToString();
                }
                else
                {
                    this.TempData["custapprove_messsage"] = "Commission Info not found";
                    this.TempData["message_class"] = CssSetting.FailedMessageClass;
                    return RedirectToAction("ModifiedApprove");
                }
                DataSet DSetMakerchecker = ProfileUtils.GetAdminModifiedValue(CommissionId);
                List<MNMakerChecker> ModifiedValues = ExtraUtility.DatatableToListClass<MNMakerChecker>(DSetMakerchecker.Tables["MNMakerChecker"]);
                srInfo.MakerChecker = ModifiedValues;
                ViewBag.AdminProfile = new SelectList(itemProfile, "Value", "Text", srInfo.ProfileName).OrderBy(x => x.Text);
                ViewBag.BranchName = new SelectList(item, "Value", "Text", srInfo.BranchCode).OrderBy(x => x.Text);//,selectedValue:userInfo.BranchCode

                return View(srInfo);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpPost]
        public ActionResult ApproveModifyCommission(UserInfo model, string btnApprove)
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

                string displayMessage = null;
                string messageClass = null;




                UserInfo userInfoApproval = new UserInfo();
                userInfoApproval.ClientCode = model.ClientCode;
                userInfoApproval.UserName = userName;

                if (btnApprove.ToUpper() == "REJECT")
                {
                    userInfoApproval.AdminUserName = Session["UserName"].ToString();
                    userInfoApproval.AdminBranch = Session["UserBranch"].ToString(); ;
                    userInfoApproval.Remarks = model.Remarks;
                    userInfoApproval.CommissionId = model.CommissionId;

                    string Rejected = "T";
                    string Approve = "UnApprove";

                    userInfoApproval.Remarks = model.Remarks;

                   
                    if (userInfoApproval.Remarks != null)
                    {
                        int ret = AgentUtils.RejectModifiedAgentCommission(userInfoApproval);
                        if (ret == 100)
                        {
                            displayMessage = "Commission Modification has been Rejected. Please Check Modification Rejected and perform accordingly";
                            messageClass = CssSetting.SuccessMessageClass;
                        }
                        else if (ret == -1 || ret != 100)
                        {
                            displayMessage = "Error while rejecting Commission " + model.Name;
                            messageClass = CssSetting.FailedMessageClass;
                        }
                    }
                    else
                    {
                        displayMessage = "Remarks is required.";
                        messageClass = CssSetting.FailedMessageClass;
                    }

                    this.TempData["agentapprove_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;
                    return RedirectToAction("ApproveAgentCommissionList");

                }
                else if (btnApprove.ToUpper() == "APPROVE")
                {
                    userInfoApproval.AdminUserName = Session["UserName"].ToString();
                    userInfoApproval.AdminBranch = Session["UserBranch"].ToString();
                    userInfoApproval.CommissionId = model.CommissionId;
                    string Rejected = "F";
                    string Approve = "Approve";

                    userInfoApproval.ApprovedBy = userName;
                    int ret = AgentUtils.ApproveModifiedAgentCommission(userInfoApproval);
                    if (ret == 100)
                    {
                        displayMessage = "Information modification for  Agent Commission " + model.Name + " has successfully been approved.";
                        messageClass = CssSetting.SuccessMessageClass;
                    }
                    else if (ret == -1 || ret != 100)
                    {
                        displayMessage = "Error while approving Admin Information";
                        messageClass = CssSetting.FailedMessageClass;
                    }

                    this.TempData["agentapprove_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;

                    return RedirectToAction("ApproveAgentCommissionList");
                }
                else
                {

                    this.TempData["agentapprove_messsage"] = "Unauthorized execution";
                    this.TempData["message_class"] = CssSetting.FailedMessageClass;

                    return RedirectToAction("ApproveAgentCommissionList");
                }
            }
            //else if (btnCommand.Substring(0, 13) == "btnSelApprove")
            //{
            //    RedirectToAction("ApproveList");
            //}
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }




        #endregion


        #region Agent Commission Rejected
        //SuperAdmin Modification Rejected
        // GET: SuperAdmin/RejectedList
        public ActionResult RejectedList()
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
                CustReport para = new CustReport();
                return View(new RegCusDetailVM
                {
                    Parameter = para,
                    CustomerData = new List<CustomerData>()

                });
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        #endregion
    }
    //userobj = AgentUtils.GetRejectedAgentCommissionList(IsModified);
}