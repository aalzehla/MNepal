using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalProject.Models;
using MNepalProject.IRepository;

namespace MNepalProject.IRepository
{
    public interface IMNSuscribedProductRepository : IDisposable
    {
        IEnumerable<MNSubscribedProduct> GetSuscribedProductList(MNSubscribedProduct mnSuscribedProduct);
        IEnumerable<MNSubscribedProduct> SuscribedProductList(string column, string parameter);
        bool InsertIntoDB(MNSubscribedProduct mnsubsproduct);
    }
}
