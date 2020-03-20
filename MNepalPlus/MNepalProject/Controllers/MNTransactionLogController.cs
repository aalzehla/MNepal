using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MNepalProject.Models;
using MNepalProject.IRepository;
using MNepalProject.Repository;
using MNepalProject.Services;
using MNepalProject.DAL;

namespace MNepalProject.Controllers
{
   

    public class MNTransactionLogController
    {
         private IMNTransactionLogRepository mnTransactionLogrepository;

         public MNTransactionLogController()        
         {
            this.mnTransactionLogrepository = new MNTransactionLogRepository(new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection()));
         }

        public void InsertDataIntoTransactionLog(MNTransactionLog mntransactionLog)
        {
            mnTransactionLogrepository.InsertintoTransactionLog(mntransactionLog);
        }
    



    }
}