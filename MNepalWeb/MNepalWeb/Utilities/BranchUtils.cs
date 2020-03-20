using System.ComponentModel;
using System.Data;
using MNepalWeb.Models;
using MNepalWeb.UserModels;

namespace MNepalWeb.Utilities
{
    public class BranchUtils
    {
        public static int CreateBranchInfo(MNBranchTable branchInfo, string bankCode)
        {
            var objBranchModel = new BranchUserModel();
            var objBranchInfo = new MNBranchTable
            {
                BranchCode = branchInfo.BranchCode,
                BranchName = branchInfo.BranchName,
                BranchLocation = branchInfo.BranchLocation,
                BankCode = bankCode,
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

        public static DataSet GetDataSetPopulateBranchName(string bankCode)
        {
            var objBranchModel = new BranchUserModel();
            var objBranchInfo = new MNBranchTable()
            {
                BranchCode = "0",
                BranchName = "",
                BranchLocation = "",
                IsBlocked = "",
                BankCode = bankCode,
                Mode = "GABT" //Get ALL MNBranchTable 
            };
            return objBranchModel.GetDsBranchNameInfo(objBranchInfo);
        }

        public static DataSet GetDataSetPopulateBranchName(string branchCode,string bankCode)
        {
            var objBranchModel = new BranchUserModel();
            var objBranchInfo = new MNBranchTable()
            {
                BranchCode = branchCode,         
                BranchName = "",
                BranchLocation = "",
                IsBlocked = "",
                BankCode = bankCode,
                Mode = "GCBC" //Get Detail BankBranch FROM BankCode

                /* Mode = "DABN"*/ //Get Detail MNBranchTable FROM BranchName
            };
            return objBranchModel.GetDsBranchNameInfo(objBranchInfo);
        }


        #region "GET Checking BranchCode"
        public static DataTable GetCheckBranchCode(string branchcode,string BankCode)
        {
            var objBranchModel = new BranchUserModel();
            var objBranchInfo = new MNBranchTable()
            {
                BranchCode = branchcode,
                BranchName = "",
                BranchLocation = "",
                IsBlocked = "",
                BankCode = BankCode,
                Mode = "GCBC" //Get Checking Branch Code


            };
            return objBranchModel.GetBranchCodeInfo(objBranchInfo);

        }



        
        #endregion

    }
}