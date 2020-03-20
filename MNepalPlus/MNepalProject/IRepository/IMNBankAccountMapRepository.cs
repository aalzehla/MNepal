using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MNepalProject.Models;

namespace MNepalProject.IRepository
{
    public interface IMNBankAccountMapRepository
    {
        MNBankAccountMap GetDefaultBankAccount(Models.MNClient mnClient);
        MNBankAccountMap CheckBankAccountExists(Models.MNBankAccountMap mnBankAccountMap);

        IEnumerable<MNBankAccountMap> BankAccountList(string p, string ClientCode);
    }
}
