using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.Mvc;
using System.Web.SessionState;
using MNepalWeb.Connection;
using MNepalWeb.Models;
using MNepalWeb.Utilities;

namespace MNepalWeb.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class AdminDashboardController : Controller
    {
        // GET: AdminDashboard
        public ActionResult Index()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
             
            //bool cocType = (bool)Session["COC"];
             
            TempData["userType"] = userType;
            //TempData["cocType"] = cocType;

            if (TempData["userType"] != null)
            {
                bool cocType = (bool)Session["COC"];
                TempData["cocType"] = cocType;


                this.ViewData["userType"] = this.TempData["userType"];
                this.ViewData["cocType"] = this.TempData["cocType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;
                if ((userType == "admin")
                                   && (cocType == false))
                {
                    return RedirectToAction("Index", "BranchAdminDashboard");
                }
                var dashboarddataset = AdminDashBoard();
                ViewBag.TotalUsers = new DataTable();

                ViewBag.TotalUserswithProfile = new DataTable();
                ViewBag.ExpiredCustomer = new DataTable();
               
                //start milayako 03
                ViewBag.ActiveCustomers = new DataTable();
                ViewBag.InActiveCustomers = new DataTable();


                ViewBag.RegistrationApproval = new DataTable();
                ViewBag.ModificationApproval = new DataTable();

                ViewBag.PasswordTPinresetApproval = new DataTable();
                ViewBag.BlockUnblockApproval = new DataTable();


                ViewBag.ApproveListCustomers = new DataTable();
                ViewBag.RejectedListCustomers = new DataTable();

                //03

                if (dashboarddataset.Tables.Count>0)
                {
                    ViewBag.TotalUsers = dashboarddataset.Tables[0];

                    ViewBag.TotalUserswithProfile = dashboarddataset.Tables[1];
                    ViewBag.ExpiredCustomer = dashboarddataset.Tables[2];
                    
                    //03
                    ViewBag.ActiveCustomers = dashboarddataset.Tables[3];
                    ViewBag.InActiveCustomers = dashboarddataset.Tables[4];
                                        
                    ViewBag.RegistrationApproval = dashboarddataset.Tables[5];
                    ViewBag.ModificationApproval = dashboarddataset.Tables[6];

                    ViewBag.PasswordTPinresetApproval = dashboarddataset.Tables[7];
                    ViewBag.BlockUnblockApproval = dashboarddataset.Tables[8];

                    ViewBag.ApproveListCustomers = dashboarddataset.Tables[9];
                    ViewBag.RejectedListCustomers = dashboarddataset.Tables[10];
                    //03

                }

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        private DataSet AdminDashBoard()
        {
            DataSet dtset = null;
            SqlConnection conn = null;
            try
            {
                using (conn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[s_MNAdminDashboard]", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
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