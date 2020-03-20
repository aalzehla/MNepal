using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalProject.Models;
using MNepalProject.IRepository;

namespace MNepalProject.Repository
{
    public class MNClientExtRepository:IMNClienExtRepository,IDisposable
    {
        private PetaPoco.Database database;
        private MNComAndFocusOneLog mn;
        private string DBName = "MNClientExt";
        private string primaryKey = "ID";
        private bool disposed = false;

        public MNClientExtRepository(PetaPoco.Database database)
        {
            // TODO: Complete member initialization
            this.database = database;
        }

        public bool CheckIsUserValid(MNClientExt mnClient)
        {
            //var UserExists = database.Query<MNClientExt>("select * from MNClientExt where UserName=@0 and Password=@1", mnClient.UserName, mnClient.Password);
            string sql = "";
            if (mnClient.userType == "agent")
            {
                sql = "select a.ID,a.ClientCode,a.UserName from MNClientExt a (NOLOCK) inner join MNAgent b (NOLOCK) on a.ClientCode = b.ClientCode  where UserName=@0 and Password=@1 and userType=@2";
            }
            else {
                sql = "select * from MNClientExt (NOLOCK) WHERE UserName=@0 and Password=@1 and userType=@2";
            }
            var UserExists = database.Query<MNClientExt>(sql, mnClient.UserName, mnClient.Password,mnClient.userType);
            
            if (UserExists.Any())
            {
                return true;
            }
            else
            {
                return false;
            }
           
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

        public string InsertExtraDetailsToCreateNewClient(MNClientExt newclientExtraDetails)
        {
            string reply = "";
            if (newclientExtraDetails != null)
            {
                database.Insert("MNClientExt", "ID",newclientExtraDetails);
                reply = "true";
            }
            else
            {
                reply = "false";
            }
            return reply;
            
        }

        /*new function*/
        public IEnumerable<MNClientExt> ClientExtList(string column, string parameter)
        {
            var sql = PetaPoco.Sql.Builder
                     .Append("Select * from MNClientExt (NOLOCK) ");
            if (!column.Contains(" ") && !String.IsNullOrEmpty(parameter))
            {
                string colStr = "where " + column + "=@0";
                sql.Append(colStr, parameter);
            }
            return database.Query<MNClientExt>(sql);
        }

        /*end:new function*/

        
    }
}