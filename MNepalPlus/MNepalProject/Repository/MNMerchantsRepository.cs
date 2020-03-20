using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalProject.IRepository;
using MNepalProject.Models;
using MNepalProject.Controllers;

namespace MNepalProject.Repository
{
    public class MNMerchantsRepository: IMNMerchantsRepository, IDisposable
    {
        private PetaPoco.Database database;
        public MNError mnError;
        //private MNAccountInfo mnAccountInfo;
        //private string DBName = "MNAccountInfo";
        //private string primaryKey = "ID";
        //private bool disposed = false;

        public MNMerchantsRepository(PetaPoco.Database database)
        {
            // TODO: Complete member initialization
            this.database = database;
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public string GetMerchantInfo(string vid)
        {
            //var GetMerchantDefWallet = database.SingleOrDefault<MNAccountInfo>("select WalletNumber from MNAccountInfo where IsDefault=1 and ClientCode in (select ClientCode from MNMerchants where mid=@mid)", vid);
            //if(GetMerchantDefWallet.WalletNumber!= "" || GetMerchantDefWallet.WalletNumber != null)
            //{
            //    return GetMerchantDefWallet.WalletNumber;
            //}
            //else
            //{
            //    return "";
            //}
            var GetMerchantINfo = database.SingleOrDefault<MNClientContact>("select * from MNClientContact (NOLOCK) where ClientCode in (select ClientCode from MNMerchants (NOLOCK) where mid=@0)", vid);

            if (GetMerchantINfo != null)
            {
                if (GetMerchantINfo.ContactNumber1 != "" || GetMerchantINfo.ContactNumber1 != null)
                {
                    return GetMerchantINfo.ContactNumber1;
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
           
        }

       
    }
}