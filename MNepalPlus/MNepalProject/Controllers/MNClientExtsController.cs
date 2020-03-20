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
    public class MNClientExtsController : Controller
    {
        private IMNClienExtRepository mnClientExtRepository;

        
          public MNClientExtsController()
          {
              
              this.mnClientExtRepository = new MNClientExtRepository(new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection()));
              
          }

            public string ValidateUser(MNClientExt clientext)
            {
                string reply = "";

                if (mnClientExtRepository.CheckIsUserValid(clientext))
                {
                    return reply="true";
                }
                else { return reply="false"; }
               
            }

            public string GetExtraClientDetailsToCreateNewClient(MNClientExt mnclientext)
            {
                string reply = "";
                try
                {
                    mnClientExtRepository.InsertExtraDetailsToCreateNewClient(mnclientext);
                    reply = "true";
                }
                catch(Exception ex)
                {
                    reply = "false";
                }
                return reply;
            
            }

            /*new functions*/
            public IEnumerable<MNClientExt> ClientExtListByContactNumber(string ContactNumber)
            {
                IEnumerable<MNClientExt> ClientExts = null;
                ClientExts = mnClientExtRepository.ClientExtList("UserName", ContactNumber);
                return ClientExts;
            }
            public MNClientExt ClientExtByContactNumber(string ContactNumber)
            {
                var clientExtList = this.ClientExtListByContactNumber(ContactNumber);
                if (clientExtList.single())
                    return (MNClientExt)clientExtList.lastOne();
                return null;
            }


            public IEnumerable<MNClientExt> ClientExtListByCode(string ClientCode)
            {
                IEnumerable<MNClientExt> ClientContacts = null;
                ClientContacts = mnClientExtRepository.ClientExtList("ClientCode", ClientCode);
                return ClientContacts;
            }

            public MNClientExt ClientExtsByCode(string ClientCode)
            {
                var clientExtList = this.ClientExtListByCode(ClientCode);
                if (clientExtList.single())
                    return (MNClientExt)clientExtList.lastOne();
                return null;
            }
        

      
    }
}

