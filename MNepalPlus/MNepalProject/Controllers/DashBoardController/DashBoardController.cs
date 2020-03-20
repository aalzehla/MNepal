using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MNepalProject.Models.ViewModel;
using System.Threading.Tasks;
using MNepalProject.Models;
using MNepalProject.DAL;
using Newtonsoft.Json;

namespace MNepalProject.Controllers.DashBoardController
{
    [Authorize]
    public class DashBoardController : Controller
    {
        #region abc

        //List<MNTransactionViewModel> _tvm=new List<MNTransactionViewModel>();
        //var list = tmDAL.GetMyTransaction(User.Identity.Name);
        //foreach(var item in list)
        //{
        //    var _fc = item.FeatureCode;
        //    var featureName = tmDAL.GetFeatureName(_fc);

        //    var _status = item.StatusId;
        //    var _statusName = tmDAL.GetStatusName(_status);

        //    //var sBIN = item.SourceBIN;
        //    //var sBankName = tmDAL.GetBankName(sBIN);

        //    //var dBIN = item.DestinationBIN;
        //    //var dBankName = tmDAL.GetBankName(dBIN);

        //    MNTransactionViewModel tvm = new MNTransactionViewModel()
        //    {
        //        CreatedDate = item.CreatedDate.ToString("yyyy/MM/dd"),
        //        FeatureName=featureName,
        //        DestinationMobile=item.DestinationMobile,
        //        Amount=item.Amount,
        //        StatusName=_statusName,
        //        Description=item.Description
        //    };
        //    _tvm.Add(tvm);
        //}

        #endregion

        #region Inilization

        dalTransactionManagement tmDAL = new dalTransactionManagement();
        
        #endregion

        // GET: DashBoard
        public ActionResult Board()
        {
            return View();
        }

        public string GetMyActivities()
        {
            try
            {               
                var list = tmDAL.GetMyTransaction(User.Identity.Name);
                var jdata = JsonConvert.SerializeObject(list);
                return jdata;
            }
            catch(Exception ex)
            {
                throw;
            }
        }
    }
}