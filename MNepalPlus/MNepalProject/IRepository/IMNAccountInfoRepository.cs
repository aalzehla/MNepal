using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MNepalProject.Models;

namespace MNepalProject.IRepository
{
    public interface IMNAccountInfoRepository
    {
        MNAccountInfo GetDefaultWalletNumber(MNClient mnClient);

        MNAccountInfo CheckWalletExists(MNAccountInfo mnAccountInfo);

        IEnumerable<MNAccountInfo> GetWalletList(MNAccountInfo rawAccountInfo);
        IEnumerable<MNAccountInfo> WalletList(string column, string parameter);
        bool CreateAccountInfoOFNewClient(MNAccountInfo mnaccinfo);
    }
}
