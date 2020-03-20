using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MNepalProject.Models;

namespace MNepalProject.IRepository
{
    public interface IMNClientRepository
    {
        MNClient DoesClientExist(MNClient client);
        MNAccountInfo GetDefaultWalletNumber(MNClient clients);

        /*new function*/
        IEnumerable<MNClient> ClientList(string column, string parameter);
        string CreateNewClient(MNClient mnclient);
        bool UpdateClient(MNClient mnclient);
    }
}
