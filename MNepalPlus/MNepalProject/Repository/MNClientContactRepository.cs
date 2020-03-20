using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalProject.Models;
using MNepalProject.IRepository;

namespace MNepalProject.Repository
{
    public class MNClientContactRepository : IMNClientContactRepository, IDisposable
    {
        private PetaPoco.Database database;
        private string DBName = "MNClientContact";
        private string primaryKey = "ID";
        private bool disposed = false;


        /*new function*/
        public IEnumerable<MNClientContact> ClientContactList(string column, string parameter)
        {
            var sql = PetaPoco.Sql.Builder
                     .Append("SELECT * FROM MNClientContact (NOLOCK) ");
            if (!column.Contains(" ") && !String.IsNullOrEmpty(parameter))
            {
                string colStr = "WHERE " + column + "=@0";
                sql.Append(colStr, parameter);
            }
            return database.Query<MNClientContact>(sql);
        }

        /*end:new function*/

        public MNClientContact ClientContacts(MNClientContact clientContact)
        {
            var clientContacts = new MNClientContact();
            if (!String.IsNullOrEmpty(clientContact.ContactNumber1))
            {
                IEnumerable<MNClientContact> clientContactList = database.Query<MNClientContact>("SELECT * FROM MNClientContact (NOLOCK) WHERE ContactNumber1=@0", clientContact.ContactNumber1);
                if (clientContactList.Any())
                {
                    clientContacts = clientContactList.First();
                }
            }
            if (!String.IsNullOrEmpty(clientContact.ContactNumber2))
            {
                //clientContacts = database.Single<MNClientContact>("Select * from MNClientContact where ContactNumber2=@0", clientContact.ContactNumber2);
            }
            if (!String.IsNullOrEmpty(clientContact.ClientCode))
            {
                IEnumerable<MNClientContact> clientContactList = database.Query<MNClientContact>("SELECT * FROM MNClientContact (NOLOCK) WHERE ClientCode=@0", clientContact.ClientCode);
                if (clientContactList.Any())
                {
                    clientContacts = clientContactList.First();
                }
            }
            return clientContacts;
        }

        public MNClientContactRepository(PetaPoco.Database database)
        {
            this.database = database;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public string CreateNewContactOfClient(MNClientContact newclientContactDetails)
        {
            string reply = "";
            if (newclientContactDetails != null)
            {
                database.Insert("MNClientContact", "ID", newclientContactDetails);
                reply = "true";
            }
            else
            {
                reply = "false";

            }
            return reply;

        }
    }
}