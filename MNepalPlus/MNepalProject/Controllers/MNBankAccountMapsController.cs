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
    public class MNBankAccountMapsController : Controller
    {
        private IMNBankAccountMapRepository mnBankAccountMapRepository;

        public MNBankAccountMapsController()
        {
            this.mnBankAccountMapRepository = new MNBankAccountMapRepository(new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection()));
        }

        // GET: MNBankAccountMaps
        public ActionResult Index()
        {
            return View();
        }


        public MNBankAccountMap GetDefaultBankAccount(MNClient mnClient)
        {
            MNBankAccountMap mnBankAccountMap = new MNBankAccountMap();
            mnBankAccountMap = mnBankAccountMapRepository.GetDefaultBankAccount(mnClient);
            return mnBankAccountMap;
        }

        public bool BankAccountExist(MNBankAccountMap mnBankAccountMap)
        {
            MNBankAccountMap mnBankAcc = new MNBankAccountMap();
            mnBankAcc = mnBankAccountMapRepository.CheckBankAccountExists(mnBankAccountMap);
            if (mnBankAcc.BankAccountNumber != "")
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        /*new functions*/
        public IEnumerable<MNBankAccountMap> ClientBankAccountMapsByCode(string clientCode)
        {
            IEnumerable<MNBankAccountMap> clientBankAccMaps = null;
            clientBankAccMaps = mnBankAccountMapRepository.BankAccountList("ClientCode", clientCode);
            return clientBankAccMaps;
        }

        public MNBankAccountMap ClientDefaultBankAccount(string clientCode)
        {
            MNBankAccountMap clientDefaultBankAccount = null;
            IEnumerable<MNBankAccountMap> clientBankAccList = ClientBankAccountMapsByCode(clientCode);
            foreach (var clientBankAcc in clientBankAccList)
            {
                if (clientBankAcc.IsDefault)
                    clientDefaultBankAccount = clientBankAcc;
            }
            return clientDefaultBankAccount;
        }
    }
}