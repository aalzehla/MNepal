using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalProject.Models;
using MNepalProject.IRepository;

namespace MNepalProject.Repository
{
    public class MNSuscribedProductRepository : IMNSuscribedProductRepository, IDisposable
    {
        private PetaPoco.Database database;
        private MNSubscribedProduct mnSubscribedProduct;
        private string DBName = "MNSubscribedProduct";
        private string primaryKey = "ID";
        private bool disposed = false;

        public IEnumerable<MNSubscribedProduct> GetSuscribedProductList(MNSubscribedProduct mnSuscribedProduct)
        {
            IEnumerable<MNSubscribedProduct> mnSubscribedProducts = database.Query<MNSubscribedProduct>("select * from MNSubscribedProduct (NOLOCK) WHERE ClientCode=@0", mnSuscribedProduct.ClientCode);
            return mnSubscribedProducts;
        }

        public MNSuscribedProductRepository(PetaPoco.Database database)
        {
            this.database = database;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }


        public IEnumerable<MNSubscribedProduct> SuscribedProductList(string column, string parameter)
        {
            var sql = PetaPoco.Sql.Builder
                  .Append(@"
                        Select * from MNSubscribedProduct (NOLOCK)
                        LEFT JOIN MNProductMaster (NOLOCK) ON MNSubscribedProduct.ProductId = MNProductMaster.ID
                        ");
            if (!column.Contains(" ") && !String.IsNullOrEmpty(parameter))
            {
                string colStr = "where MNSubscribedProduct." + column + "=@0";
                sql.Append(colStr, parameter);
            }

            var mnSubscribedProductList = database.Fetch<MNSubscribedProduct, MNProductMaster>(sql);
            return mnSubscribedProductList;
        }
        public bool InsertIntoDB(MNSubscribedProduct mn)
        {
            
            try
            {
                database.Execute("Insert into MNSubscribedProduct (ClientCode,ProductId,IsDefault,ProductStatus) values ('" + mn.ClientCode + "','" + mn.ProductId + "','" + mn.IsDefault + "','" + mn.ProductStatus + "') ");
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
    }
}