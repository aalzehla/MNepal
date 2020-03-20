using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalProject.Models;
using MNepalProject.IRepository;
using MNepalProject.CustomExceptions;

namespace MNepalProject.Repository
{
    public class MNClientRepository : IMNClientRepository, IDisposable
    {
        private PetaPoco.Database database;
        private string DBName = "MNClient";
        private string primaryKey = "ID";
        private bool disposed = false;


        public MNClient DoesClientExist(MNClient rawclient)
        {
            var client = new MNClient();
            if (rawclient.ClientCode != "")
            {
                client = database.Single<MNClient>("Select * from MNClient (NOLOCK) WHERE ClientCode=@0", rawclient.ClientCode);
            }
            return client;
        }

        public MNClientRepository(PetaPoco.Database database)
        {
            // TODO: Complete member initialization
            this.database = database;
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

        public MNAccountInfo GetDefaultWalletNumber(MNClient clients)
        {

            var maincode = database.Query<long>("select top 1 MainCode from Master (NOLOCK) WHERE ClientCode=@0", clients.ClientCode);

            MNAccountInfo ma = new MNAccountInfo();
            ma.WalletNumber = maincode.ToString();
            ma.BIN = "0000";
            return ma;
        }

        /*new function*/
        public IEnumerable<MNClient> ClientList(string column, string parameter)
        {
            var sql = PetaPoco.Sql.Builder
                    .Append("Select * from MNClient (NOLOCK) ");
            if (!column.Contains(" ") && !String.IsNullOrEmpty(parameter))
            {
                string colStr = "where " + column + "=@0";
                colStr+= " and " + "Status" + "='Active'";
                sql.Append(colStr, parameter);
            }
            return database.Query<MNClient>(sql);
        }


        public string CreateNewClient(MNClient newclientDetails)
        {
            string reply = "";
            if (newclientDetails != null)
            {
                //dataContext.Execute("UPDATE MNClient SET PIN= @0 WHERE ClientCode = @1 and PIN=@2", newpin, clientdetails.client.ClientCode, pin);
                database.Execute("Insert into MNClient(ClientCode, Name,Address, PIN,Status)values(@0,@1,@2,@3,@4)",newclientDetails.ClientCode,newclientDetails.Name,newclientDetails.Address,newclientDetails.PIN,newclientDetails.Status);
                //database.Insert(newclientDetails);
                reply = "true";
            }
            else
            {
                reply = "false";

            }
            return reply;
            
        }
        public bool UpdateClient(MNClient mnclient)
        {
            database.Update<MNClient>("SET Status=@0 WHERE ClientCode=@1", mnclient.Status, mnclient.ClientCode);
            return true;
            
        }
    }
}