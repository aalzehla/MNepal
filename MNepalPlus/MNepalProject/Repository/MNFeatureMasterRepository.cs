using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalProject.Models;
using MNepalProject.IRepository;


namespace MNepalProject.Repository
{
    public class MNFeatureMasterRepository : IMNFeatureMasterRepository, IDisposable
    {
        private PetaPoco.Database database;
        private MNFeatureMaster mnFeatureMaster;
        private string DBName = "MNFeatureMaster";
        private string primaryKey = "FeatureCode";
        private bool disposed = false;

        public bool IsFeatureCodeExist(MNFeatureMaster featureCode)
        {
            var serviceCode = database.Query<MNFeatureMaster>("select * from MNFeatureMaster (NOLOCK) where FeatureCode=@0", featureCode.FeatureCode);
            if (serviceCode.Any())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public MNFeatureMasterRepository(PetaPoco.Database database)
        {
            // TODO: Complete member initialization
            this.database = database;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}