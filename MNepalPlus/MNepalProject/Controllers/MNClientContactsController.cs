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
    public class MNClientContactsController : Controller
    {
        private IMNClientContactRepository mnClientContactRepository;


        public MNClientContactsController()
        {
            this.mnClientContactRepository = new MNClientContactRepository(new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection()));
        }


        public MNClientContact getClient(MNClientContact clientContact)
        {
            MNClientContact clientContacts = new MNClientContact();
            clientContacts = mnClientContactRepository.ClientContacts(clientContact);
            return clientContacts;
        }

        public MNClientContact getClientContact(MNClientContact clientContact)
        {
            MNClientContact clientContacts = new MNClientContact();
            clientContacts = mnClientContactRepository.ClientContacts(clientContact);
            return clientContacts;
        }
        // GET: MNClients
        public ActionResult Index()
        {
            return View();
        }
        /*new functions*/
        public IEnumerable<MNClientContact> ClientContactListByContactNumber(string ContactNumber)
        {
            IEnumerable<MNClientContact> ClientContacts = null;
            ClientContacts = mnClientContactRepository.ClientContactList("ContactNumber1", ContactNumber);
            return ClientContacts;
        }
        public MNClientContact ClientContactByContactNumber(string ContactNumber)
        {
            var clientContactList = this.ClientContactListByContactNumber(ContactNumber);
            if (clientContactList.single())
                return (MNClientContact)clientContactList.lastOne();
            return null;
        }

        public IEnumerable<MNClientContact> ClientContactListByCode(string ClientCode)
        {
            IEnumerable<MNClientContact> ClientContacts = null;
            ClientContacts = mnClientContactRepository.ClientContactList("ClientCode", ClientCode);
            return ClientContacts;
        }

        public MNClientContact ClientContactByCode(string ClientCode)
        {
            var clientContactList = this.ClientContactListByCode(ClientCode);
            if (clientContactList.single())
                return (MNClientContact)clientContactList.lastOne();
            return null;
        }

        public string GetContactDetailsToCreateNewClient(MNClientContact mnclientcontact)
        {
            string reply = "";
            try
            {
                mnClientContactRepository.CreateNewContactOfClient(mnclientcontact);
                reply = "true";

            }
            catch (Exception ex)
            {
                reply = "false";
            }
            return reply;
            
        }


    }
}