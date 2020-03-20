using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MNepalProject.Models;
using MNepalProject.IRepository;

namespace MNepalProject.Repository
{
    public class MNBankAccountMapRepository : IMNBankAccountMapRepository, IDisposable
    {
        private PetaPoco.Database database;
        private MNBankAccountMap mnBankAccountMap;
        private string DBName = "MNBankAccountMap";
        private string primaryKey = "ID";
        private bool disposed = false;

        public MNBankAccountMapRepository(PetaPoco.Database database)
        {
            // TODO: Complete member initialization
            this.database = database;
        }

        public MNBankAccountMap GetDefaultBankAccount(MNClient mnClient)
        {
            MNBankAccountMap mnBankAccount = database.Single<MNBankAccountMap>("SELECT * FROM MNBankAccountMap (NOLOCK) WHERE ClientCode=@0 and IsDefault='True'", mnClient.ClientCode);
            return mnBankAccount;
        }


        public MNBankAccountMap CheckBankAccountExists(MNBankAccountMap mnBankAcc)
        {
            MNBankAccountMap mnBankAccMap = database.Single<MNBankAccountMap>("SELECT * FROM MNBankAccountMap (NOLOCK) WHERE BankAccountNumber=@0 and IsDefault='True'", mnBankAcc.BankAccountNumber);
            return mnBankAccMap;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }


        public IEnumerable<MNBankAccountMap> BankAccountList(string column, string parameter)
        {
            var sql = PetaPoco.Sql.Builder
                      .Append("SELECT * FROM MNBankAccountMap (NOLOCK) ");
            if (!column.Contains(" ") && !String.IsNullOrEmpty(parameter))
            {
                string colStr = "WHERE " + column + "=@0";
                sql.Append(colStr, parameter);
            }
            return database.Query<MNBankAccountMap>(sql);
        }
    }
}
