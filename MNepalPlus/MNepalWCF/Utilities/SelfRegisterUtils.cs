using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using MNepalWCF.Models;
using MNepalWCF.UserModels;

namespace MNepalWCF.Utilities
{
    public class SelfRegisterUtils
    {
        public static int RegisterUsersInfo(UserValidate userInfo)
        {
            var objMobileModel = new SelfRegisterUserModel();
            var objMobileInfo = new UserValidate
            {
                UserName = userInfo.UserName,
                Password = userInfo.Password,//GeneratePassword(),
                PIN = userInfo.PIN,
                userType = userInfo.userType,
                OTPCode = userInfo.OTPCode,
                Source = userInfo.Source,

                FName = userInfo.FName,
                MName = userInfo.MName,
                LName = userInfo.LName,
                Gender = userInfo.Gender,
                DateOfBirth = DateTime.ParseExact(userInfo.DateOfBirth, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                CountryCode = userInfo.CountryCode,
                Nationality = userInfo.Nationality,
                FathersName = userInfo.FathersName,
                MothersName = userInfo.MothersName,
                GrandFatherName = userInfo.GrandFatherName,
                MaritalStatus = userInfo.MaritalStatus,
                SpouseName = userInfo.SpouseName,
                FatherInLaw = userInfo.FatherInLaw,
                Occupation = userInfo.Occupation,

                PProvince = userInfo.PProvince,
                PDistrict = userInfo.PDistrict,
                PProvinceID = userInfo.PProvinceID,
                PDistrictID = userInfo.PDistrictID,
                PMunicipalityVDC = userInfo.PMunicipalityVDC,
                PHouseNo = userInfo.PHouseNo,
                PWardNo = userInfo.PWardNo,
                PStreet = userInfo.PStreet,

                CProvince = userInfo.CProvince,
                CDistrict = userInfo.CDistrict,
                CProvinceID = userInfo.CProvinceID,
                CDistrictID = userInfo.CDistrictID,
                CMunicipalityVDC = userInfo.CMunicipalityVDC,
                CHouseNo = userInfo.CHouseNo,
                CWardNo = userInfo.CWardNo,
                CStreet = userInfo.CStreet,

                CitizenshipNo = userInfo.CitizenshipNo,
                CitizenPlaceOfIssue = userInfo.CitizenPlaceOfIssue,
                CitizenIssueDate = DateTime.ParseExact(userInfo.CitizenIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                PassportNo = userInfo.PassportNo,
                PassportPlaceOfIssue = userInfo.PassportPlaceOfIssue,
                PassportIssueDate = userInfo.PassportIssueDate,
                PassportExpiryDate = userInfo.PassportExpiryDate,
                LicenseNo = userInfo.LicenseNo,
                LicensePlaceOfIssue = userInfo.LicensePlaceOfIssue,
                LicenseIssueDate = userInfo.LicenseIssueDate,
                LicenseExpiryDate = userInfo.LicenseExpiryDate,
                PANNumber = userInfo.PANNumber,
                IsRejected = userInfo.IsRejected,
                Status = "Active",
                EmailAddress = userInfo.EmailAddress,
                BranchCode = userInfo.BranchCode,
                DocType = userInfo.DocType,
                FrontImage = userInfo.FrontImage,
                BackImage = userInfo.BackImage,
                PassportImage = userInfo.PassportImage,
                Mode = "ISR" //INSERT SELF REGISTRATION PROFILE
            };
            return objMobileModel.RegisterUsersInfo(objMobileInfo);
        }


        public static string GeneratePin()
        {
            int _min = 1000;
            int _max = 9999;
            Random _rdm = new Random();
            return _rdm.Next(_min, _max).ToString();
        }

        static Random rnd = new Random();
        public static string GeneratePassword()
        {
            int maxlength = 8;
            int minlength = 8;
            int length = rnd.Next(minlength, maxlength);
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }


        //Generate Token
        public static string GetUniqueOTPKey()
        {
            int maxSize = 6;
            //int minSize = 5;
            char[] chars = new char[62];
            string a;
            a = "1234567890";
            chars = a.ToCharArray();
            int size = maxSize;
            byte[] data = new byte[1];

            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                size = maxSize;
                data = new byte[maxSize];
                crypto.GetNonZeroBytes(data);
            }

            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }


        public static string GetProvinceID(string ProvinceName) {
            var objProvinceModel = new SelfRegisterUserModel();
            var objProvinceInfo = new UserValidate
            {
                PProvince = ProvinceName
            };
            return objProvinceModel.ProvinceInfo(objProvinceInfo);
        }
    }
}