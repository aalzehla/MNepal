using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MNepalProject.Models;

namespace MNepalProject.IRepository
{
    public interface IMNClientContactRepository
    {
        MNClientContact ClientContacts(MNClientContact clientContact);

        IEnumerable<MNClientContact> ClientContactList(string column, string parameter);
        string CreateNewContactOfClient(MNClientContact mnclientContact);
    }
}
