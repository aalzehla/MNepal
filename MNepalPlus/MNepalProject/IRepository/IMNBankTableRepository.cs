using MNepalProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MNepalProject.IRepository
{
    public interface IMNBankTableRepository
    {
        MNBankTable GetBankDetails(string bankCode);

        MNBankTable GetBankAcDetails(string clientCode);
    }
}
