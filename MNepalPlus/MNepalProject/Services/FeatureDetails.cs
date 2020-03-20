using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalProject.Models;
using MNepalProject.Controllers;

namespace MNepalProject.Services
{
    public class FeatureDetails
    {
       

        public MNFeature feature { get; private set; }

        public FeatureDetails(ClientsDetails ClientDetails, string FeatureCode)
        {
            /*
             select * from MNFeature where
             FeatureCode = '00' and
             SourceBIN = '0000' and
             DestinationBIN = '0000' and
             ProductId in
             (select ProductId from MNSubscribedProduct where
             ClientCode = '00000001' and
             IsDefault = 1 and
             ProductStatus = 'active')
             */
        }

        public FeatureDetails(int ProductID, string FeatureCode, string SourceBIN, string DestinationBIN)
        {
            //getting feature details by ProductID, FeatureCode, SourceBIN
            MNFeaturesController mnfeatureController = new MNFeaturesController();
            this.feature = mnfeatureController.FeatureDetailsByFeatureNProductNSourceBINNDestinationBIN(FeatureCode, ProductID, SourceBIN, DestinationBIN);
        }

        public FeatureDetails(int ProductID, string FeatureCode, string SourceBIN)
        {
            //getting feature details by ProductID, FeatureCode, SourceBIN
            MNFeaturesController mnfeatureController = new MNFeaturesController();
            this.feature = mnfeatureController.FeatureDetailsByFeatureNProductNSourceBIN(FeatureCode, ProductID, SourceBIN);
        }
    }
}