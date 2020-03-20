using System.ComponentModel;
using System.Data;
using MNSuperadmin.Models;
using MNSuperadmin.UserModels;

namespace MNSuperadmin.Utilities
{
    public class BranchUtils
    {
        public static int CreateBranchInfo(MNBranchTable branchInfo)
        {
            var objBranchModel = new BranchUserModel();
            var objBranchInfo = new MNBranchTable
            {
                BranchCode = branchInfo.BranchCode,
                BranchName = branchInfo.BranchName,
                BranchLocation = branchInfo.BranchLocation,
                IsBlocked = "F",
                Mode = "IBT"
            };
            return objBranchModel.BranchInfo(objBranchInfo);
        }

        public static int UpdateBranchInfo(MNBranchTable branchInfo)
        {
            var objBranchModel = new BranchUserModel();
            var objBranchInfo = new MNBranchTable
            {
                BranchCode = branchInfo.BranchCode,
                BranchName = branchInfo.BranchName,
                BranchLocation = branchInfo.BranchLocation,
                IsBlocked = branchInfo.IsBlocked,
                Mode = "UBT"
            };
            return objBranchModel.BranchInfo(objBranchInfo);
        }

        //public static int GetAllBranchInfo(MNBranchTable branchInfo)
        //{
        //    var objBranchModel = new BranchUserModel();
        //    var objBranchInfo = new MNBranchTable
        //    {
        //        BranchCode = branchInfo.BranchCode,
        //        BranchName = branchInfo.BranchName,
        //        BranchLocation = branchInfo.BranchLocation,
        //        IsBlocked = branchInfo.IsBlocked,
        //        Mode = "GABT"
        //    };
        //    return objBranchModel.GetAllBranchInfo(objBranchInfo);
        //}

        public static DataSet GetDataSetPopulateBranchName()
        {
            var objBranchModel = new BranchUserModel();
            var objBranchInfo = new MNBranchTable()
            {
                BranchCode = "0",
                BranchName = "",
                BranchLocation = "",
                IsBlocked = "",
                Mode = "GABT" //Get ALL MNBranchTable 
            };
            return objBranchModel.GetDsBranchNameInfo(objBranchInfo);
        }

        public static DataSet GetDataSetPopulateBranchName(string branchCode)
        {
            var objBranchModel = new BranchUserModel();
            var objBranchInfo = new MNBranchTable()
            {
                BranchCode = branchCode,         
                BranchName = "",
                BranchLocation = "",
                IsBlocked = "",
                Mode = "GCBC" //Get Detail MBranchTable FROM BranchCode

                /* Mode = "DABN"*/ //Get Detail MNBranchTable FROM BranchName
            };
            return objBranchModel.GetDsBranchNameInfo(objBranchInfo);
        }


        #region "GET Checking BranchCode"
        public static DataTable GetCheckBranchCode(string branchcode)
        {
            var objBranchModel = new BranchUserModel();
            var objBranchInfo = new MNBranchTable()
            {
                BranchCode = branchcode,
                BranchName = "",
                BranchLocation = "",
                IsBlocked = "",
                Mode = "GCBC" //Get Checking Branch Code


            };
            return objBranchModel.GetBranchCodeInfo(objBranchInfo);

        }



        
        #endregion

    }
}