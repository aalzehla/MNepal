﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ThailiMNepalAgent.UserModels;

namespace ThailiMNepalAgent.Utilities
{
    public class NEAUtils
    {
        #region
        public static Dictionary<string, string> GetNEAName()
        {
            var objNEAModel = new NEAUserModel();

            return objNEAModel.GetNEAName();
        }

        #endregion
    }
}