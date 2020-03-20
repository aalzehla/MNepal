using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalProject.Models;
using MNepalProject.IRepository;

namespace MNepalProject.Repository
{
    public class MNFeatureRepository : IMNFeatureRepository, IDisposable
    {
        private PetaPoco.Database database;
        private MNFeature mnFeature;
        private string DBName = "MNFeature";
        private string primaryKey = "ID";
        private bool disposed = false;

        public IEnumerable<MNFeature> GetFeatureList(MNFeature mnFeature)
        {
            IEnumerable<MNFeature> mnFeatures = database.Query<MNFeature>("select * from MNFeature (NOLOCK) where FeatureCode=@0", mnFeature.FeatureCode);
            return mnFeatures;
        }

        public MNFeatureRepository(PetaPoco.Database database)
        {
            this.database = database;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    database.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            //GC.SuppressFinalize(this);
        }


        public IEnumerable<MNFeature> FeatureList(string column, string parameter)
        {
            /* var sql = PetaPoco.Sql.Builder
                  .Append("Select * from MNFeature ");
             if (!column.Contains(" ") && !String.IsNullOrEmpty(parameter))
             {
                 string colStr = "where " + column + "=@0";
                 sql.Append(colStr, parameter);
             }
             return database.Query<MNFeature>(sql);
             */


            var sql = PetaPoco.Sql.Builder
                .Append(@"
                        Select * from MNFeature (NOLOCK)
                        LEFT JOIN MNFeatureMaster (NOLOCK) ON MNFeature.FeatureCode = MNFeatureMaster.FeatureCode
                        ");
            if (!column.Contains(" ") && !String.IsNullOrEmpty(parameter))
            {
                string colStr = "where MNFeature.MNFeature." + column + "=@0";
                sql.Append(colStr, parameter);
            }

            var mnFeatureList = database.Fetch<MNFeature>(sql);
            return mnFeatureList;
        }


        public IEnumerable<MNFeature> FeatureByFeatureCodeNProduct(string FeatureCode, int ProductId)
        {
            /*
                        var result = database.Query<MNFeature, MNFeatureMaster>(
                 "select p.*, pr.* from MNFeature " +
                 "p inner join MNFeatureMaster pr on pr.FeatureCode=p.FeatureCode");

            */

            var sql = PetaPoco.Sql.Builder
                .Append(@"
                        Select * from MNFeature (NOLOCK)
                        LEFT JOIN MNFeatureMaster (NOLOCK) ON MNFeature.FeatureCode = MNFeatureMaster.FeatureCode
                        ");
            if (!String.IsNullOrEmpty(FeatureCode) && !String.IsNullOrEmpty(ProductId.ToString()))
            {
                string colStr = "where " + "MNFeature.FeatureCode" + "=@0 and MNFeature.ProductId = @1";
                sql.Append(colStr, FeatureCode, ProductId);
            }
            var mnFeatureList = database.Fetch<MNFeature>(sql);
            return null;
        }


        public IEnumerable<MNFeature> FeatureListByProductId(int productId)
        {
            var sql = PetaPoco.Sql.Builder
               .Append(@"
                        Select * from MNFeature (NOLOCK)
                        LEFT JOIN MNFeatureMaster (NOLOCK) ON MNFeature.FeatureCode = MNFeatureMaster.FeatureCode
                        ");
            if (!String.IsNullOrEmpty(productId.ToString()))
            {
                string colStr = "where " + "MNFeature.ProductId = @0";
                sql.Append(colStr, productId);
            }
            var mnFeatureList = database.Fetch<MNFeature>(sql);
            return mnFeatureList;
        }


        public IEnumerable<MNFeature> FeatureByProductNBIN(string FeatureCode, int ProductID, string SourceBIN)
        {
            var sql = PetaPoco.Sql.Builder
              .Append(@"
                        Select * from MNFeature (NOLOCK)
                        ");
            if (!String.IsNullOrEmpty(FeatureCode.ToString()) && !String.IsNullOrEmpty(ProductID.ToString()) && !String.IsNullOrEmpty(SourceBIN.ToString()))
            {
                string colStrFeatureCode = "where " + "FeatureCode = @0";
                //sql.Append(colStrFeatureCode, FeatureCode);
                string colStrProductID = " and " + "ProductId = @1";
                //sql.Append(colStrProductID, ProductID);
                string colStrSourceBIN = " and " + "SourceBIN = @2";
                sql.Append(colStrFeatureCode + colStrProductID + colStrSourceBIN, FeatureCode, ProductID, SourceBIN);
            }
            var mnFeatureList = database.Fetch<MNFeature>(sql);
            return mnFeatureList;
        }


        public IEnumerable<MNFeature> FeatureByProductNBINDestinationBIN(string FeatureCode, int ProductID, string SourceBIN, string DestinationBIN)
        {
            var sql = PetaPoco.Sql.Builder
             .Append(@"
                        Select * from MNFeature (NOLOCK)
                        ");
            if (!String.IsNullOrEmpty(FeatureCode.ToString()) && !String.IsNullOrEmpty(ProductID.ToString()) && !String.IsNullOrEmpty(SourceBIN.ToString()) && !String.IsNullOrEmpty(DestinationBIN.ToString()))
            {
                string colStrFeatureCode = "where " + "FeatureCode = @0";
                string colStrProductID = " and " + "ProductId = @1";
                string colStrSourceBIN = " and " + "SourceBIN = @2";
                string colStrDestinationBIN = " and " + "DestinationBIN = @3";
                sql.Append(colStrFeatureCode + colStrProductID + colStrSourceBIN + colStrDestinationBIN, FeatureCode, ProductID, SourceBIN, DestinationBIN);
            }
            var mnFeatureList = database.Fetch<MNFeature>(sql);
            return mnFeatureList;
        }
    }
}