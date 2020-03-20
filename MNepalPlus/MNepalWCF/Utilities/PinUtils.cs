using System.Data;
using MNepalWCF.Models;
using MNepalWCF.UserModels;

namespace MNepalWCF.Utilities
{
    public class PinUtils
    {
        public static DataTable GetPinInfo(string mobileNo)
        {
            var objUserModel = new PinUserModel();
            var objUserInfo = new PinChange()
            {
                mobile = mobileNo
            };
            return objUserModel.GetPinInformation(objUserInfo);
        }
    }
}