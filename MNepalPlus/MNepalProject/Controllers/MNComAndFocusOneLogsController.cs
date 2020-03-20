using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MNepalProject.DAL;
using MNepalProject.Models;
using MNepalProject.IRepository;
using MNepalProject.Repository;

namespace MNepalProject.Controllers
{
    public class MNComAndFocusOneLogsController : Controller
    {
        private IMNComAndFocusOneLogRepository mnRepository;

        public MNComAndFocusOneLogsController()
      {
          this.mnRepository = new MNComAndFocusOneLogRepository(new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection()));
      }


        public string InsertIntoComFocusOne(MNComAndFocusOneLog comfocuslog)
        {
            string result = "";
            if (ModelState.IsValid)
            {
                try
                {
                    mnRepository.InsertComAndFocusOneLog(comfocuslog);
                    mnRepository.Save();
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
    }
}
