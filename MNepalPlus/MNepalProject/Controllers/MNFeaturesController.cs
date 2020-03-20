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
    public class MNFeaturesController : Controller
    {
        private IMNFeatureRepository mnFeatureRepository;
        public MNFeaturesController()
          {
              this.mnFeatureRepository = new MNFeatureRepository(new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection()));
          }

        public IEnumerable<MNFeature> GetFeatureList(MNFeature mnFeature) {
            return mnFeatureRepository.GetFeatureList(mnFeature);
        }
        // GET: MNFeatures
        public ActionResult Index()
        {
            return View();
        }

        public MNFeature FeatureDetailsByFeatureNProductNSourceBINNDestinationBIN(string FeatureCode, int ProductID, string SourceBIN, string DestinationBIN)
        {
            var mnFeatureList = mnFeatureRepository.FeatureByProductNBINDestinationBIN(FeatureCode, ProductID, SourceBIN, DestinationBIN);
            if (mnFeatureList.single())
                return (MNFeature)mnFeatureList.lastOne();
            return null;
        }

        public MNFeature FeatureDetailsByFeatureNProductNSourceBIN(string FeatureCode, int ProductID, string SourceBIN)
        {
            var mnFeatureList = mnFeatureRepository.FeatureByProductNBIN(FeatureCode, ProductID, SourceBIN);
            if (mnFeatureList.single())
                return (MNFeature)mnFeatureList.lastOne();
            return null;
        }
    }
}