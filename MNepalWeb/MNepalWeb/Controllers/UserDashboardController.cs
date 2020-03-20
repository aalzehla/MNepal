using MNepalWeb.Models;
using MNepalWeb.Utilities;
using System.Data;
using System.Web.Mvc;
using System.Web.SessionState;

namespace MNepalWeb.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class UserDashboardController : Controller
    {
        // GET: UserDashboardContent
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
                ViewBag.Name = name;

                MNBalance availBaln = new MNBalance();
                DataTable dtableUser = AvailBalnUtils.GetAvailBaln(clientCode);
                if (dtableUser != null && dtableUser.Rows.Count > 0)
                {
                    availBaln.amount = dtableUser.Rows[0]["AvailBaln"].ToString();

                    ViewBag.AvailBalnAmount = availBaln.amount;
                }
                return View();
            }
            else
            {
                return RedirectToAction("Index","Login");
            }
        }
    }
}