using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MNepalProject.DAL;
using MNepalProject.Models;
using MNepalProject.IRepository;
using MNepalProject.Repository;


namespace MNepalProject.Controllers
{
    public class MNAccountInfosController : Controller
    {
        private IMNAccountInfoRepository mnAccountInfoRepository;

        public MNAccountInfosController()
        {
            this.mnAccountInfoRepository = new MNAccountInfoRepository(new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection()));
        }

        public MNAccountInfo GetDefaultWallet(MNClient mnClient)
        {
            MNAccountInfo mnAccountInfo = new MNAccountInfo();
            mnAccountInfo = mnAccountInfoRepository.GetDefaultWalletNumber(mnClient);
            return mnAccountInfo;
        }
        // GET: MNAccountInfos
        public ActionResult Index()
        {
            var dataContext = new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection());
            var mnaccountInfos = dataContext.Query<MNAccountInfo>("Select * from MNAccountInfo");
            return View(mnaccountInfos);
        }

        public bool sourceWalletExist(MNAccountInfo mnAccountInfo)
        {
            MNAccountInfo mnAccountInfos = new MNAccountInfo();
            mnAccountInfos = mnAccountInfoRepository.CheckWalletExists(mnAccountInfo);
            if (mnAccountInfo.WalletNumber != "")
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public IEnumerable<MNAccountInfo> accountInfoList(MNAccountInfo rawAccountInfo)
        {
            IEnumerable<MNAccountInfo> mnAccountInfoList = null;
            try
            {
                mnAccountInfoList = mnAccountInfoRepository.GetWalletList(rawAccountInfo);
            }
            catch (Exception ex)
            {

            }

            return mnAccountInfoList;
        }

        /*new functions*/
        public IEnumerable<MNAccountInfo> ClientAccountInfosByCode(string ClientCode)
        {
            return mnAccountInfoRepository.WalletList("ClientCode", ClientCode);
        }

        public MNAccountInfo ClientDefaultAccountInfoByCode(string ClientCode)
        {
            MNAccountInfo DefaultWallet = null;
            IEnumerable<MNAccountInfo> WalletList = ClientAccountInfosByCode(ClientCode);
            foreach (var DefaultWalletAcc in WalletList)
            {
                if (DefaultWalletAcc.IsDefault)
                    DefaultWallet = DefaultWalletAcc;
            }
            return DefaultWallet;
        }

        public bool InsertWalletForNewClient(MNAccountInfo mnacc)
        {
            try
            {
                mnAccountInfoRepository.CreateAccountInfoOFNewClient(mnacc);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}