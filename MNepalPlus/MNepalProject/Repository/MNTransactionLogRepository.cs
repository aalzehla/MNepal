using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalProject.Models;
using MNepalProject.IRepository;
namespace MNepalProject.Repository
{
    public class MNTransactionLogRepository:IMNTransactionLogRepository,IDisposable
    {
        private PetaPoco.Database database;
        private MNTransactionLog mn;
        private string DBName = "MNTransactionLog";
        private string primaryKey = "ID";
        private bool disposed = false;

        public void InsertintoTransactionLog(MNTransactionLog mntransactionLog)
        {
            MNTransactionLog mntdb = new MNTransactionLog(mntransactionLog);
            database.Insert("MNTransactionLog", "ID", mntdb);
            
        }

         public void Save()
        {
            //database.Insert(DBName, primaryKey,mn );
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    database.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public MNTransactionLogRepository(PetaPoco.Database database)
        {
            // TODO: Complete member initialization
            this.database = database;
        }
    }
}