using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalProject.Models;
using MNepalProject.IRepository;


namespace MNepalProject.Repository
{
    public class MNAccountInfoRepository : IMNAccountInfoRepository, IDisposable
    {
        public MNError mnError;
        private PetaPoco.Database database;
        private MNAccountInfo mnAccountInfo;
        private string DBName = "MNAccountInfo";
        private string primaryKey = "ID";
        private bool disposed = false;


        public MNAccountInfoRepository(PetaPoco.Database database)
        {
            this.database = database;
        }



        public MNAccountInfo GetDefaultWalletNumber(MNClient mnClient)
        {
            MNAccountInfo mnaccountInfo = database.Single<MNAccountInfo>("SELECT * FROM MNAccountInfo (NOLOCK) WHERE ClientCode=@0 and IsDefault='True'", mnClient.ClientCode);
            //mnaccountInfo.BIN = "0000";
            return mnaccountInfo;
        }


        public MNAccountInfo CheckWalletExists(MNAccountInfo mnAccountInfo)
        {
            MNAccountInfo mnaccountInfo = database.Single<MNAccountInfo>("SELECT * FROM MNAccountInfo (NOLOCK) WHERE WalletNumber=@0", mnAccountInfo.WalletNumber);
            return mnAccountInfo;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<MNAccountInfo> WalletList(string column, string parameter)
        {
            var sql = PetaPoco.Sql.Builder
                      .Append("SELECT * FROM MNAccountInfo (NOLOCK) ");
            if (!column.Contains(" ") && !String.IsNullOrEmpty(parameter))
            {
                string colStr = "where " + column + "=@0";
                sql.Append(colStr, parameter);
            }
            return database.Query<MNAccountInfo>(sql);
        }

        public IEnumerable<MNAccountInfo> GetWalletList(MNAccountInfo rawAccountInfo)
        {
            IEnumerable<MNAccountInfo> mnaccountInfoList = null;
            try
            {
                mnaccountInfoList = database.Query<MNAccountInfo>("SELECT * FROM MNAccountInfo (NOLOCK) WHERE ClientCode=@0", rawAccountInfo.ClientCode);
                return mnaccountInfoList;
            }
            catch (Exception ex)
            {
                this.mnError = new MNError(true, "MNAccountInfoRepository>>GetWalletList" + ex.ToString());
            }
            return mnaccountInfoList;
        }

        public bool CreateAccountInfoOFNewClient(MNAccountInfo mnacc)
        {
            if (mnacc != null)
            {
                database.Insert("MNAccountInfo", "ID",mnacc);
                
                return true;
            }
            else
            {
                return false;

            }
            
        }
    }
}