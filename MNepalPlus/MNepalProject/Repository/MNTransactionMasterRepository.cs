using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalProject.Models;
using MNepalProject.IRepository;

namespace MNepalProject.Repository
{
    public class MNTransactionMasterRepository : IMNTransactionMasterRepository, IDisposable
    {
        private PetaPoco.Database database;
        private MNTransactionMasterDB mn;
        private string DBName = "MNTransactionMaster";
        private string primaryKey = "ID";
        private bool disposed = false;




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

        public MNTransactionMasterRepository(PetaPoco.Database database)
        {
            // TODO: Complete member initialization
            this.database = database;
        }


        public bool IsTraceIDUnique(string traceId)
        {
            var tid = database.Query<MNTransactionMaster>("Select * from MNTransactionMaster (NOLOCK) where TraceId=@0", traceId);
            if (!tid.Any())
                return true;
            else
                return false;
        }

        public int InsertintoTransactionMaster(MNTransactionMaster mntransaction)
        {
            MNTransactionMasterDB mntdb = new MNTransactionMasterDB(mntransaction);
            database.Insert("MNTransactionMaster", "ID", mntdb);
            return mntdb.ID;

        }

        /*new function*/
        public IEnumerable<MNTransactionMaster> TransactionMasterList(string column, string parameter)
        {
            var sql = PetaPoco.Sql.Builder
                      .Append("Select * from MNTransactionMaster (NOLOCK) ");
            if (!column.Contains(" ") && !String.IsNullOrEmpty(parameter))
            {
                string colStr = "where " + column + "=@0";
                sql.Append(colStr, parameter);
            }
            return database.Query<MNTransactionMaster>(sql);
        }


        public void UpdateintoTransactionMaster(MNTransactionMaster  mn)
        {
            database.Update<MNTransactionMaster>("set StatusId=@0 where TraceId=@1", mn.StatusId, mn.TraceId);
        }
    }
}