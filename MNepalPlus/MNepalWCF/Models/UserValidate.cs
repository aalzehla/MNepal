using System.Web;

namespace MNepalWCF.Models
{
    public class UserValidate
    {
        //public string tid { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public string userType { get; set; }

        public string FName { get; set; }
        public string MName { get; set; }
        public string LName { get; set; }
        public string Gender { get; set; }

        public string DateOfBirth { get; set; }
        public string CountryCode { get; set; }
        public string Nationality { get; set; }
        public string FathersName { get; set; }
        public string MothersName { get; set; }
        public string MaritalStatus { get; set; }
        public string SpouseName { get; set; }
        public string GrandFatherName { get; set; }

        public string FatherInLaw { get; set; }
        public string Occupation { get; set; }
        public string EmailAddress { get; set; }


        public string PProvince { get; set; }
        public string PDistrict { get; set; }
        public string PMunicipalityVDC { get; set; }
        public string PHouseNo { get; set; }
        public string PWardNo { get; set; }
        public string PStreet { get; set; }

        public string CProvince { get; set; }
        public string CDistrict { get; set; }
        public string CMunicipalityVDC { get; set; }
        public string CHouseNo { get; set; }
        public string CWardNo { get; set; }
        public string CStreet { get; set; }

        public string CitizenshipNo { get; set; }
        public string CitizenPlaceOfIssue { get; set; }
        public string CitizenIssueDate { get; set; }


        public string PassportNo { get; set; }
        public string PassportPlaceOfIssue { get; set; }
        public string PassportIssueDate { get; set; }
        public string PassportExpiryDate { get; set; }


        public string LicenseNo { get; set; }
        public string LicensePlaceOfIssue { get; set; }
        public string LicenseIssueDate { get; set; }
        public string LicenseExpiryDate { get; set; }        
        
        public string PANNumber { get; set; }

        //
        public string Name { get; set; }
        public string Address { get; set; }
        public string PIN { get; set; }
        public string Status { get; set; }
        public string OTPCode { get; set; }
        public string ContactNumber1 { get; set; }
        public string IsApproved { get; set; }
        public string IsRejected { get; set; }
        public string WBankCode { get; set; }
        public string WBranchCode { get; set; }
        public string WIsDefault { get; set; }
        public string BankNo { get; set; }
        public string BranchCode { get; set; }
        public string IsDefault { get; set; }
        public string SelfReg { get; set; }
        public string Mode { get; set; }

        public string Source { get; set; }

        public string DocType { get; set; }

        public string PProvinceID { get; set; }
        public string CProvinceID { get; set; }
        public string PDistrictID { get; set; }
        public string CDistrictID { get; set; }

        public HttpPostedFileBase PassportPhoto { get; set; }

        public HttpPostedFileBase FrontPic { get; set; }

        public HttpPostedFileBase BackPic { get; set; }

        public string FrontImage { get; set; }

        public string BackImage { get; set; }

        public string PassportImage { get; set; }


        public UserValidate() { }
        public UserValidate(string UserName, string Password, string userType)
        {
            //this.tid = tid;
            this.UserName = UserName;
            this.Password = Password;
            this.userType = userType;
        }

        public UserValidate(string UserName, string Password, string Pin, string userType, string OTPCode, string src,
            string FName, string MName, string LName, string Gender, string DateOfBirth, string CountryCode, string Nationality,
            string FathersName, string MothersName, string GrandFatherName, string MaritalStatus, 
            string SpouseName,  string FatherInLaw, string Occupation, 
            string PProvince, string PDistrict, string PDistrictID, string PMunicipalityVDC, string PHouseNo, string PWardNo, string PStreet, string PProvinceID,
            string CProvince, string CDistrict, string CDistrictID, string CMunicipalityVDC, string CHouseNo, string CWardNo, string CStreet, string CProvinceID,
            string CitizenshipNo, string CitizenPlaceOfIssue, string CitizenIssueDate,
            string PassportNo, string PassportPlaceOfIssue, string PassportIssueDate, string PassportExpiryDate,
            string LicenseNo, string LicensePlaceOfIssue, string LicenseIssueDate, string LicenseExpiryDate,
            string PANNumber, string DocType, string IsRejected, string EmailAddress, string BranchCode, 
            string frontImg, string BackImg, string PassportImage)
        {
            this.UserName = UserName;
            this.Password = Password;
            this.PIN = Pin;
            this.userType = userType;
            this.OTPCode = OTPCode;
            this.Source = src;

            this.FName = FName;
            this.MName = MName;
            this.LName = LName;
            this.Gender = Gender;
            this.DateOfBirth = DateOfBirth;
            this.CountryCode = CountryCode;
            this.Nationality = Nationality;

            this.FathersName = FathersName;
            this.MothersName = MothersName;
            this.GrandFatherName = GrandFatherName;
            this.MaritalStatus = MaritalStatus;
            this.SpouseName = SpouseName;
            this.FatherInLaw = FatherInLaw;
            this.Occupation = Occupation;

            this.PProvince = PProvince;
            this.PDistrict = PDistrict;
            this.PDistrictID = PDistrictID;
            this.PMunicipalityVDC = PMunicipalityVDC;
            this.PHouseNo = PHouseNo;
            this.PWardNo = PWardNo;
            this.PStreet = PStreet;
            this.PProvinceID = PProvinceID;

            this.CProvince = CProvince;
            this.CDistrict = CDistrict;
            this.CDistrictID = CDistrictID;
            this.CMunicipalityVDC = CMunicipalityVDC;
            this.CHouseNo = CHouseNo;
            this.CWardNo = CWardNo;
            this.CStreet = CStreet;
            this.CProvinceID = CProvinceID;

            this.CitizenshipNo = CitizenshipNo;
            this.CitizenPlaceOfIssue = CitizenPlaceOfIssue;
            this.CitizenIssueDate = CitizenIssueDate;

            this.PassportNo = PassportNo;
            this.PassportPlaceOfIssue = PassportPlaceOfIssue;
            this.PassportIssueDate = PassportIssueDate;
            this.PassportExpiryDate = PassportExpiryDate;

            this.LicenseNo = LicenseNo;
            this.LicensePlaceOfIssue = LicensePlaceOfIssue;
            this.LicenseIssueDate = LicenseIssueDate;
            this.LicenseExpiryDate = LicenseExpiryDate;

            this.PANNumber = PANNumber;
            this.DocType = DocType;
            this.IsRejected = IsRejected;
            this.EmailAddress = EmailAddress;
            this.BranchCode = BranchCode;
            
            this.FrontImage = frontImg;
            this.BackImage = BackImg;
            this.PassportImage = PassportImage;
        }

    }
}