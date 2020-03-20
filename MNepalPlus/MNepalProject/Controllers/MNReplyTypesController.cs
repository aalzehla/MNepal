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
    public class MNReplyTypesController:Controller
    {
        private IMNReplyTypeRepository mnReplyTypeRepository;

         public MNReplyTypesController()
      {
          this.mnReplyTypeRepository = new MNReplyTypeRepository(new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection()));
      }


         public void InsertIntoReplyType(MNReplyType mnReplyType)
        {
            string result = "";
            if (ModelState.IsValid)
            {
                try
                {
                    mnReplyTypeRepository.InsertReplyType(mnReplyType);
                    mnReplyTypeRepository.Save();
                }
                catch (Exception ex)
                {
                    result = ex.ToString();
                }
            }
            else
            {
                result = "ModelStateINValid";//
            }
        }
    }
}
