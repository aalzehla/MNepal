using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalProject.DAL
{
    public class GlobalConnection
    {
        public PetaPoco.Database getConnection()
        {
            return (new PetaPoco.Database("DbConnectionString"));
        }
    }
}