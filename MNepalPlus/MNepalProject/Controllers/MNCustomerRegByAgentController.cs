using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MNepalProject.Models;

namespace MNepalProject.Controllers
{
    public class MNCustomerRegByAgentController : Controller
    {
        
        // GET: MNCustomerRegByAgent
        public ActionResult Index()
        {
            return View();
        }

        #region "Customer Registration By Agent"

        public string InsertIntoCustomerKycByAgent(MNClientKyc clientKyclog)
        {
            string result = "";
            if (ModelState.IsValid)
            {
                try
                {
        

                    result = "Success";
                }
                catch (Exception ex)
                {
                    result = ex.ToString();
                }
            }
            else
            {
                result = "ModelStateINValid";
            }
            return result;
        }

        #endregion
    }
}