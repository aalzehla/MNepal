using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalProject.Models;
using MNepalProject.IRepository;

namespace MNepalProject.Repository
{
    public class MNReplyTypeRepository : IMNReplyTypeRepository, IDisposable
    {
        private PetaPoco.Database database;
        private MNReplyType mnReplyType;
        private string DBName = "MNReplyType";
        private string primaryKey = "ID";
        private bool disposed = false;

        public MNReplyTypeRepository(PetaPoco.Database database)
        {
            this.database = database;
        }


        public void InsertReplyType(MNReplyType mnReplyType)
        {
            this.mnReplyType = mnReplyType;

        }

        public void Save()
        {
            database.Insert(DBName, primaryKey, this.mnReplyType);
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