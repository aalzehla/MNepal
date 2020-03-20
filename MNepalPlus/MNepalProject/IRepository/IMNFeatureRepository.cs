using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MNepalProject.Models;

namespace MNepalProject.IRepository
{
    public interface IMNFeatureRepository : IDisposable
    {
        IEnumerable<MNFeature> GetFeatureList(MNFeature mnFeature);
        IEnumerable<MNFeature> FeatureList(string column, string parameter);

        IEnumerable<MNFeature> FeatureByFeatureCodeNProduct(string FeatureCode, int ProductId);


        IEnumerable<MNFeature> FeatureListByProductId(int productId);

        IEnumerable<MNFeature> FeatureByProductNBIN(string FeatureCode, int ProductID, string SourceBIN);

        IEnumerable<MNFeature> FeatureByProductNBINDestinationBIN(string FeatureCode, int ProductID, string SourceBIN, string DestinationBIN);
    }
}
