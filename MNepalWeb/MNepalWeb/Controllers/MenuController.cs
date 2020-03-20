using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MNepalWeb.Connection;
using MNepalWeb.Models;

namespace MNepalWeb.Controllers
{
    public class MenuController : Controller
    {
        // GET: Menu
        public ActionResult MenuLayout()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            var menu = new MenuInfo();

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                DataSet ds = GetParentMenu();
                ViewBag.Menu = ds.Tables[0];
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
            return PartialView("MenuLayout", menu);
        }

        public DataSet GetParentMenu()
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

        private DataTable GetSubMenuData(int parentMenuId)
        {
            string query = "SELECT MenuID, Hierarchy, Description FROM MNMenuTable WHERE ParentMenuID = @ParentMenuId";
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                DataTable dt = new DataTable();
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    using (SqlDataAdapter sda = new SqlDataAdapter())
                    {
                        cmd.Parameters.AddWithValue("@ParentMenuId", parentMenuId);
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = con;
                        sda.SelectCommand = cmd;
                        sda.Fill(dt);
                    }
                }
                return dt;
            }
        }

        //protected DataTable LoadMenuAdmin()
        //{
        //    DataTable dT = new DataTable();
        //    DataSet dsTemp = new DataSet();
        //    string sSql = "";
        //    sSql = " SELECT Hierarchy, Description FROM MNMenuTable (NOLOCK) where Hierarchy like 'A%' ORDER BY Hierarchy";

        //    using(SqlDataAdapter myRead = new SqlDataAdapter(sSql, DatabaseConnection.ConnectionString)
        //    {
        //        myRead.Fill(dsTemp);
        //        //Write the contents of the DataSet to a local XML file
        //        dsTemp.WriteXml(filePath);
        //        dT = dsTemp.Tables[0];
        //    }
        //    return dT;
        //}


        //private void PopulateMenu(DataTable dt, int parentMenuId, MenuItem parentMenuItem)
        //{
        //    string currentPage = Path.GetFileName(Request.Url.AbsolutePath);
        //    foreach (DataRow row in dt.Rows)
        //    {
        //        MenuItem menuItem = new MenuItem
        //        {
        //            Value = row["MenuId"].ToString(),
        //            Text = row["Title"].ToString(),
        //            NavigateUrl = row["Url"].ToString(),
        //            Selected = row["Url"].ToString().EndsWith(currentPage, StringComparison.CurrentCultureIgnoreCase)
        //        };
        //        if (parentMenuId == 0)
        //        {
        //            Menu1.Items.Add(menuItem);
        //            DataTable dtChild = this.GetData(int.Parse(menuItem.Value));
        //            PopulateMenu(dtChild, int.Parse(menuItem.Value), menuItem);
        //        }
        //        else
        //        {
        //            parentMenuItem.ChildItems.Add(menuItem);
        //        }
        //    }
        //}

    }
}