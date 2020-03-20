using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MNepalProject.Models;

namespace MNepalProject.IRepository
{
    public interface IMNFeatureMasterRepository:IDisposable
    {
        bool IsFeatureCodeExist(MNFeatureMaster mnFeatureMaster);
    }
}
