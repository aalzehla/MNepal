using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MNepalProject.Models;
using MNepalProject.IRepository;
using MNepalProject.Repository;
using MNepalProject.DAL;

namespace MNepalProject.Controllers
{
    public class MNFeatureMastersController : Controller
    {
        private IMNFeatureMasterRepository mnFeatureMasterRepository;

        
        public MNFeatureMastersController()
      {
          this.mnFeatureMasterRepository = new MNFeatureMasterRepository(new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection()));
      }

        public bool DoesFeatureCodeExist(MNFeatureMaster mnfeature)
        {
            if(mnFeatureMasterRepository.IsFeatureCodeExist(mnfeature))
            {
                return true;
            }
            else { return false; }

            
        }
        // GET: MNFeatureMasters
        public ActionResult Index()
        {
            return View();
        }
    }
}