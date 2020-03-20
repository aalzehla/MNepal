using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MNepalWeb.Models;
using System.Data.SqlClient;
using System.Text;
using System.Web.Configuration;
using MNepalWeb.Connection;
using MNepalWeb.Utilities;

namespace MNepalWeb.Controllers
{
    public class NavigationController : Controller
    {
        // GET: Navigation
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SideMenuPartial()
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

                DataSet ds = GetParentMenu(clientCode);

                DataTable dt = new DataTable();
                string str = string.Empty;
                //string ConnectionString = DatabaseConnection.ConnectionStr(); //@"SERVER=PAHADIPC\SQLEXP12;Database=PPIVMNepal;User Id=sa;Password=software";
                //SqlConnection conn = new SqlConnection(ConnectionString);
                //string strcmd = "SELECT * FROM dbo.MNMenuTable AS MMT  ORDER BY MMT.Hierarchy";

                //using (SqlDataAdapter da = new SqlDataAdapter(strcmd, conn))
                //{
                //    da.Fill(dt);
                //}

                string webName = WebConfigurationManager.AppSettings["WebAddress"];

                dt = ds.Tables[0];

                StringBuilder sb = new StringBuilder();
                string oldHierarchy = string.Empty;
                //sb.Append("<ul>");
                foreach (DataRow dr in dt.Rows)
                {
                    string hierarchy = dr["Hierarchy"].ToString();
                    string controller = dr["MenuModuleName"].ToString();
                    string url = dr["LinkUrl"].ToString();
                    string description = dr["Description"].ToString();

                    if (oldHierarchy.Trim() != string.Empty)
                    {
                        if (hierarchy.Substring(0, 1) != oldHierarchy)
                        {
                            sb.Append("</ul></li>");
                        }
                    }

                    if (hierarchy.Length == 1)
                    {
                        oldHierarchy = hierarchy;
                        sb.Append("<li class= " + "\"" + "treeview" + "\"" + "><a href=" + "\"" + "#" + "\"" + "><span>" + description + " </span><span class= " + "\"" + "pull-right-container" + "\"" + "> <i class=" + "\"" + "fa fa-angle-left pull-right" + "\"" + "></i></span></a>");
                        sb.Append("<ul class= " + "\"" + "treeview-menu" + "\"" + ">");
                    }

                    else if (hierarchy.Length == 2)
                    {
                        sb.Append("<li>" +
                            "<a href=" + "\"" + "/" + webName + controller + "/" + url + "\"" + "><span>" + description + "</span></a></li>");//"/"
                        //sb.Append("<li><a href=" + "\"" + url + "\"" + " + id=" + "\"" + "btnclick" + "\"" + "><span>" + description + "</span></a></li>");
                    }
                }


                sb.Append("</ul>");
                sb.Append("</li>");
                //sb.Append("</ul>");

                string test = sb.ToString();

                ViewBag.Menu = test; //ds.Tables[0];

                List<MenuItem> list = new List<MenuItem>();

                //list.Add(new MenuItem { Link = "/Test/Index", LinkName = "Home" });
                //list.Add(new MenuItem { Link = "/Test/Login", LinkName = "Login" });
                
                return PartialView("SideMenuPartial", list);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }


        public DataSet GetParentMenu(string clientCode)
        {
            List<UserProfilesInfo> userProfilesList = new List<UserProfilesInfo>();
            DataSet dtUserProfile = UserProfileUtils.GetAdminUserProfileInfo1(clientCode);
            return dtUserProfile;
        }

        public DataSet Get_Subcategory(int parentMenuId)
        {
            using (SqlConnection con = new SqlConnection(DatabaseConnection.ConnectionStr()))
            {
                con.Open();
                SqlCommand com = new SqlCommand("SELECT MenuID, Description, ParentMenuID, LinkUrl FROM MNMenuTable where ParentMenuID=@parentMenuId", con);

                com.Parameters.AddWithValue("@parentMenuId", parentMenuId);

                SqlDataAdapter da = new SqlDataAdapter(com);

                DataSet ds = new DataSet();

                da.Fill(ds);
                con.Close();
                return ds;
            }

        }

        public void Get_Submenu(int parentMenuId)
        {

            DataSet ds = Get_Subcategory(parentMenuId);

            List<MenuItem> submenulist = new List<MenuItem>();

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                submenulist.Add(new MenuItem
                {
                    Id = Convert.ToInt32(dr["MenuID"]),
                    Name = dr["Description"].ToString(),
                    Url = dr["LinkUrl"].ToString()

                });
            }
            Session["submenu"] = submenulist;
        }

        public DataSet GetParentMenu_1(string clientCode)
        {
            using (SqlConnection con = new SqlConnection(DatabaseConnection.ConnectionStr()))
            {
                con.Open();
                SqlCommand com = new SqlCommand("SELECT MenuID, Description, ParentMenuID FROM MNMenuTable WHERE ParentMenuID = 0",
                    con);
                SqlDataAdapter da = new SqlDataAdapter(com);
                DataSet ds = new DataSet();
                da.Fill(ds);
                con.Close();

                return ds;
            }
        }

    }
}