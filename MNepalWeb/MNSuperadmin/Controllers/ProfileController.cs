using MNSuperadmin.Models;
using MNSuperadmin.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;

namespace MNSuperadmin.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class ProfileController : Controller
    {
        // GET: Profile
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

                UserInfo userInfo = new UserInfo();
                DataTable dtableUser = ProfileUtils.GetUserProfileInfo(clientCode);
                if (dtableUser != null && dtableUser.Rows.Count > 0)
                {

                    userInfo.Name = dtableUser.Rows[0]["Name"].ToString();
                    userInfo.UserName = dtableUser.Rows[0]["UserName"].ToString();
                    //userInfo.UserBranchName = dtableUser.Rows[0]["UserBranchName"].ToString();
                    userInfo.EmailAddress = dtableUser.Rows[0]["EmailAddress"].ToString();
                    userInfo.AProfileName = dtableUser.Rows[0]["AProfileName"].ToString();
                    userInfo.COC = dtableUser.Rows[0]["COC"].ToString();
                    userInfo.Status = dtableUser.Rows[0]["Status"].ToString();
                    userInfo.BankNo = dtableUser.Rows[0]["BankNo"].ToString();

                    ViewBag.UserName = userInfo.UserName;
                    //ViewBag.UserBranchName = userInfo.UserBranchName;
                    ViewBag.EmailAddress = userInfo.EmailAddress;
                    ViewBag.AProfileName = userInfo.AProfileName;
                    ViewBag.COC = userInfo.COC;
                    ViewBag.Status = userInfo.Status;
                    ViewBag.BankNo = userInfo.BankNo;
                }

                    return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
    }
}