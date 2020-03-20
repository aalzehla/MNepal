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
    public class MNRequestsController : Controller
    {
        private IMNRequestRepository mnRequestRepository;
        // GET: MNRequests
        public ActionResult Index()
        {
            return View();
        }

        public MNRequestsController()
        {
            this.mnRequestRepository = new MNRequestRepository(new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection()));
        }

        public bool SendNewClientDetailsToMNRequest(MNRequest mnrequest)
        {
            try
            {
                mnRequestRepository.InsertIntoMNRequestToCreateNewUser(mnrequest);
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
            
        }
    }
}