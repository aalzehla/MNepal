using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MNepalProject.Models;

namespace MNepalProject.IRepository
{
    public interface IMNClienExtRepository:IDisposable
    {
        bool CheckIsUserValid(MNClientExt mnClientExt);
        string InsertExtraDetailsToCreateNewClient(MNClientExt mnclientext);
        IEnumerable<MNClientExt> ClientExtList(string column, string parameter);
    }
}
