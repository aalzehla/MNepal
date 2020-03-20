using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalProject.Models;
using MNepalProject.IRepository;

namespace MNepalProject.Repository
{
    public class MNComAndFocusOneLogRepository : IMNComAndFocusOneLogRepository, IDisposable
    {
        private PetaPoco.Database database;
        private MNComAndFocusOneLog mn;
        private string DBName = "MNComAndFocusOneLog";
        private string primaryKey = "ID";
        private bool disposed = false;

        public MNComAndFocusOneLogRepository(PetaPoco.Database database)
        {
            this.database = database;
        }


        public void InsertComAndFocusOneLog(MNComAndFocusOneLog mnComAndFocusOneLog)
        {
            this.mn = mnComAndFocusOneLog;
        }

        public void Save()
        {
            database.Insert(DBName, primaryKey, this.mn);
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
    }
}