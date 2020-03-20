using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MNepalProject.Models;
using MNepalProject.IRepository;
using MNepalProject.Repository;
using System.Collections;
using MNepalProject.DAL;

namespace MNepalProject.Controllers
{
    public class MNClientsController : Controller
    {
        private IMNClientRepository mnClientRepository;


        public MNClientsController()
        {
            this.mnClientRepository = new MNClientRepository(new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection()));
        }


        public MNClient getClient(MNClient client)
        {
            MNClient clientDetails = new MNClient();
            clientDetails = mnClientRepository.DoesClientExist(client);
            return clientDetails;
        }

        // GET: MNClients
        public ActionResult Index()
        {
            return View();
        }



        public MNAccountInfo GetDefaultWallet(MNClient mnClient)
        {
            MNAccountInfo mnAccountInfo = new MNAccountInfo();
            mnAccountInfo = mnClientRepository.GetDefaultWalletNumber(mnClient);
            return mnAccountInfo;
        }

        public MNClient getTheClient(MNClient rawclient)
        {
            MNClient clientDetails = new MNClient();
            //clientDetails = mnClientRepository.Client(rawclient);
            return clientDetails;
        }
        /*new functions*/
        public IEnumerable<MNClient> ClientListByCode(string ClientCode)
        {
            IEnumerable<MNClient> Clients = null;
            Clients = mnClientRepository.ClientList("ClientCode", ClientCode);
            return Clients;
        }

        public MNClient ClientByCode(string ClientCode)
        {
            var clientlist = this.ClientListByCode(ClientCode);
            if (clientlist.single())
                return (MNClient)clientlist.lastOne();
            return null;
        }


        public string GetDetailsToCreateNewClient(MNClient mnclient)
        {
            string reply = "";
            try
            {
                mnClientRepository.CreateNewClient(mnclient);
                reply = "true";
            }
            catch(Exception ex)
            {
                reply = "false";
            }
            return reply;
        }
        public bool UpdateClientTable(MNClient mnclient)
        {
            try 
            {
                mnClientRepository.UpdateClient(mnclient);
                return true;
            }
            catch(Exception ex) 
            {
                return false;
            }
        }
    }
}