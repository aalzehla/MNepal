using System.Web.Mvc;
using MNepalProject.Models;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Net;
using System;
namespace MNepalProject.Controllers
{
    [Authorize]
    public class MNCashInOutController : Controller
    {

        #region CashIn

        //Get
        public ActionResult CashIn()
        {
            return View();
        }

        public async Task<string> GetTraceID()
        {
            //call Rest Api to get new TraceId
           // Task<string> getTraceId = method to call traceId; 

            //await retrieve the string operator when task complete 
            //string TraceId = await getTraceId;
            //return (TraceId);

            return "123456879";
        }

        #endregion

        #region CashOut

        //Get
        public ActionResult CashOut()
        {
            return View();
        }

        #endregion
    }
}