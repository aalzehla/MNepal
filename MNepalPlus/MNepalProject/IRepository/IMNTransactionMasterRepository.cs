using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MNepalProject.Models;

namespace MNepalProject.IRepository
{
    public interface IMNTransactionMasterRepository : IDisposable
    {
        int InsertintoTransactionMaster(MNTransactionMaster mntransaction);
        void Save();

        bool IsTraceIDUnique(string traceId);
        IEnumerable<MNTransactionMaster> TransactionMasterList(string column, string parameter);
        void UpdateintoTransactionMaster(MNTransactionMaster mnTransactionMaster);
    }
}
