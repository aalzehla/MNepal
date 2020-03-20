using MNSuperadmin.Connection;
using MNSuperadmin.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MNSuperadmin.Controllers
{
    public class BranchSuperAdminDashboardController : Controller
    {
        // GET: BranchSuperAdminDashboard
        public ActionResult Index()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
           // string cocType = (string)Session["COC"];
            //bool cocType = (bool)Session["COC"];
            string UserBranchCode = (string)Session["UserBranch"];

            //TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                ///for in reset sending to login after refresh
                DataTable dtableUserCheckFirstLogin = ProfileUtils.PassChangedSuperadmin(clientCode);
                if (dtableUserCheckFirstLogin != null && dtableUserCheckFirstLogin.Rows.Count > 0)
                {
                    ViewBag.IsFirstLogin = dtableUserCheckFirstLogin.Rows[0]["IsFirstLogin"].ToString();
                }
            }
            if (TempData["userType"] != null && ViewBag.IsFirstLogin == "F")
                 
         //       if (TempData["userType"] != null)
            {
                string cocType = (string)Session["COC"];
                TempData["cocType"] = cocType;


                this.ViewData["userType"] = this.TempData["userType"];
                this.ViewData["cocType"] = this.TempData["cocType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                //if ((userType == "admin")
                //                   && (cocType == true))
                    if ((userType == "superadmin")
                    && (cocType == "T"))
                    {
                    return RedirectToAction("Index", "AdminDashboard");
                }

                var dashboarddataset = AdminDashBoard(UserBranchCode);
                ViewBag.TotalUsers = new DataTable();

                ViewBag.TotalUserswithProfile = new DataTable();
                ViewBag.ExpiredCustomer = new DataTable();

                ViewBag.ActiveCustomers = new DataTable();
                ViewBag.InActiveCustomers = new DataTable();

                ViewBag.BlockUnblockApproval = new DataTable();
                ViewBag.ApproveListCustomers = new DataTable();
                ViewBag.RejectedListCustomers = new DataTable();
                //
                ViewBag.RegistrationApprovalCount = new DataTable();
                ViewBag.ModificationApprovalCount = new DataTable();
                ViewBag.PasswordTPinresetApprovalCount = new DataTable();
                ViewBag.BlockUnblockApprovalCount = new DataTable();
                ViewBag.RegistrationRejectedCount = new DataTable();
                ViewBag.ModificationRejectedCount = new DataTable();
                //

                //START 05
                ViewBag.TotalAdmins = new DataTable();
                ViewBag.TotalAgents = new DataTable();
                ViewBag.TotalCustomers = new DataTable();
                ViewBag.TotalNewMembers = new DataTable();
                ViewBag.TotalSuperAdmins = new DataTable();


                ViewBag.SuperAdminRegistrationApproval = new DataTable();
                ViewBag.SuperAdminModificationApproval = new DataTable();
                ViewBag.AgentRegistrationApproval = new DataTable();
                ViewBag.AgentModificationApproval = new DataTable();
                ViewBag.CustomerWalletKYCVerification = new DataTable();
                ViewBag.AdminList = new DataTable();
                //END 05

                if (dashboarddataset.Tables.Count > 0)
                {
                    ViewBag.TotalUsers = dashboarddataset.Tables[0];
                    ViewBag.TotalUserswithProfile = dashboarddataset.Tables[1];
                    ViewBag.ExpiredCustomer = dashboarddataset.Tables[2];

                    ViewBag.ActiveCustomers = dashboarddataset.Tables[3];
                    ViewBag.InActiveCustomers = dashboarddataset.Tables[4];

                    ViewBag.BlockUnblockApproval = dashboarddataset.Tables[8];
                    ViewBag.ApproveListCustomers = dashboarddataset.Tables[9];
                    ViewBag.RejectedListCustomers = dashboarddataset.Tables[10];
                    //
                    ViewBag.RegistrationApprovalCount = dashboarddataset.Tables[11];
                    ViewBag.ModificationApprovalCount = dashboarddataset.Tables[12];
                    ViewBag.PasswordTPinresetApprovalCount = dashboarddataset.Tables[13];
                    ViewBag.BlockUnblockApprovalCount = dashboarddataset.Tables[14];
                    ViewBag.RegistrationRejectedCount = dashboarddataset.Tables[15];
                    ViewBag.ModificationRejectedCount = dashboarddataset.Tables[16];
                    //
                    //START 05
                    ViewBag.TotalAdmins = dashboarddataset.Tables[17];
                    ViewBag.TotalAgents = dashboarddataset.Tables[18];
                    ViewBag.TotalCustomers = dashboarddataset.Tables[19];
                    ViewBag.TotalNewMembers = dashboarddataset.Tables[20];
                    ViewBag.TotalSuperAdmins = dashboarddataset.Tables[21];

                    ViewBag.SuperAdminRegistrationApproval = dashboarddataset.Tables[22];
                    ViewBag.SuperAdminModificationApproval = dashboarddataset.Tables[23];
                    ViewBag.AgentRegistrationApproval = dashboarddataset.Tables[24];
                    ViewBag.AgentModificationApproval = dashboarddataset.Tables[25];
                    ViewBag.CustomerWalletKYCVerification = dashboarddataset.Tables[26];
                    ViewBag.AdminList = dashboarddataset.Tables[27];
                    //END 05

                }

                return View();
            }
            else
            {
                Session["LOGGED_USERNAME"] = null;
                return RedirectToAction("Index", "Login");
            }
        }


        private DataSet AdminDashBoard(string UserBranchCode)
        {
            DataSet dtset = null;
            SqlConnection conn = null;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNBranchSuperAdminDashboard]", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@UserBranchCode", SqlDbType.NVarChar).Value = UserBranchCode;

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            // set the CommandTimeout
                            da.SelectCommand.CommandTimeout = 60;  // seconds
                            using (DataSet dataset = new DataSet())
                            {
                                da.Fill(dataset);
                                dtset = dataset;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }

            return dtset;
        }

    }
}