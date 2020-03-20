using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MNepalProject.Models;
using MNepalProject.IRepository;
using MNepalProject.Repository;
using MNepalProject.DAL;

namespace MNepalProject.Controllers
{
    public class MNSubscribedProductsController : Controller
    {
        private IMNSuscribedProductRepository mnSuscribedProductRepository;
        public MNSubscribedProductsController()
        {
            this.mnSuscribedProductRepository = new MNSuscribedProductRepository(new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection()));
        }

        public IEnumerable<MNSubscribedProduct> GetProductList(MNSubscribedProduct mnSuscribedProduct)
        {
            return mnSuscribedProductRepository.GetSuscribedProductList(mnSuscribedProduct);
        }
        // GET: MNSubscribedProducts
        public ActionResult Index()
        {
            return View();
        }


        /*new functions*/
        public IEnumerable<MNSubscribedProduct> ClientSuscribedProductsByCode(string ClientCode)
        {
            IEnumerable<MNSubscribedProduct> SubscribedProducts = null;
            SubscribedProducts = mnSuscribedProductRepository.SuscribedProductList("ClientCode", ClientCode);
            return SubscribedProducts;
        }

        public IEnumerable<MNSubscribedProduct> ClientSuscribedActiveProductsByCode(string ClientCode)
        {
            List<MNSubscribedProduct> SubscribedActiveProducts = new List<MNSubscribedProduct>();

            IEnumerable<MNSubscribedProduct> SubscribedProductList = ClientSuscribedProductsByCode(ClientCode);
            foreach (MNSubscribedProduct SubscribedProduct in SubscribedProductList)
            {
                if (SubscribedProduct.ProductStatus == "Active")
                {
                    SubscribedActiveProducts.Add(SubscribedProduct);
                }

            }
            return SubscribedActiveProducts;
        }



        public MNSubscribedProduct ClientDefaultSuscribedActiveProductByCode(string ClientCode)
        {
            MNSubscribedProduct SubscribedDefaultActiveProduct = null;
            IEnumerable<MNSubscribedProduct> SubscribedProductList = ClientSuscribedActiveProductsByCode(ClientCode);
            foreach (var SubscribedActiveProduct in SubscribedProductList)
            {
                if (SubscribedActiveProduct.IsDefault)
                    SubscribedDefaultActiveProduct = SubscribedActiveProduct;
            }
            return SubscribedDefaultActiveProduct;
            /*
            var subscribedActiveProductList = this.ClientSuscribedActiveProductsByCode(ClientCode);
            if (subscribedActiveProductList.single())
                return (MNSubscribedProduct)subscribedActiveProductList.lastOne();
            return null;*/
        }

        public bool InsertProductForNewUser(MNSubscribedProduct mnproduct)
        {
            try
            {
                mnSuscribedProductRepository.InsertIntoDB(mnproduct);
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
            
        }
    }
}