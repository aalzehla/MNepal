using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalProject.Models;
using MNepalProject.DAL;

namespace MNepalProject.Services
{
    public class CheckProductSuscribedByClient:ReplyMessage
    {
        public bool isProductSuscribed { get; private set; }

        public CheckProductSuscribedByClient(string clientCode, string featureCode) {
            var dataContext = new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection());
            
            try
            {
                var SubscribedProduct = dataContext.Query<MNSubscribedProduct>("select * from MNSubscribedProduct where ClientCode=@0 and IsDefault=1 and ProductId in(select ProductId from MNFeature where FeatureCode=@1)", clientCode, featureCode);
                if (SubscribedProduct.Any())
                {
                    this.isProductSuscribed = true;
                }
                else
                {
                    this.isProductSuscribed = false;
                }
            }
            catch (Exception ex)
            {
                this.isProductSuscribed = false;
            }
            

           
        }
        public CheckProductSuscribedByClient(string clientCode, IEnumerable<MNSubscribedProduct> products, IEnumerable<MNFeature> features)
        { 
            
        }
    }
}