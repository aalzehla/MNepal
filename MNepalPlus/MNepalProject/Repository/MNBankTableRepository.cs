using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalProject.Models;
using MNepalProject.IRepository;

namespace MNepalProject.Repository
{
    public class MNBankTableRepository : IMNBankTableRepository
    {
        public MNError mnError;
        private PetaPoco.Database database;
        private MNBankTable mnBankInfo;
        private string DBName = "MNBankTable";
        private string primaryKey = "BankCode";
        private bool disposed = false;


        public MNBankTableRepository(PetaPoco.Database database)
        {
            this.database = database;
        }

        public MNBankTable GetBankDetails(string bankCode)
        {
            MNBankTable mnBankInfo = new MNBankTable();
            mnBankInfo = database.Single<MNBankTable>("SELECT * FROM MNBankTable (NOLOCK) WHERE BankCode=@0", bankCode);
            return mnBankInfo;
        }

        public MNBankTable GetBankAcDetails(string clientCode)
        {
            MNBankTable mnBankInfo = new MNBankTable();
            mnBankInfo = database.Single<MNBankTable>("SELECT * FROM MNBankAccountMap (NOLOCK) WHERE ClientCode=@0", clientCode);
            return mnBankInfo;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}