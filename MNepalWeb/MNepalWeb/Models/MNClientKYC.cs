using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalWeb.Models
{
    public class MNClientKYC
    {
        public string Gender { get; set; }
        //NISCHAL
        public string DOB { get; set; }
        public string DateOfBirth { get; set; }
        public string BSDateOfBirth { get; set; }
        public string Nationality { get; set; }
        public string Country { get; set; }
        public string FathersName { get; set; }
        public string MothersName { get; set; }
        public string GFathersName { get; set; }
        public string Occupation { get; set; }
        public string MaritalStatus { get; set; }
        public string SpouseName { get; set; }
        public string FatherInLawName { get; set; }
        public string FatherInLaw { get; set; }
        public string Document { get; set; }
        public string CitizenshipNo { get; set; } 
        public string CitizenPlaceOfIssue { get; set; }
        //public string CitizenshipIssueDate { get; set; }
        public string CitizenIssueDate { get; set; }

        public string BSCitizenIssueDate { get; set; }
        public string LicenseNo { get; set; }
        public string LicensePlaceOfIssue { get; set; }
        public string LicenseIssueDate { get; set; }
        public string BSLicenseIssueDate { get; set; }
        public string LicenseExpiryDate { get; set; }
        public string BSLicenseExpiryDate { get; set; }
        public string LicenseExpireDate { get; set; }
        public string PassportNo { get; set; }
        public string PassportPlaceOfIssue { get; set; }
        public string PassportIssueDate { get; set; }
        public string PassportExpireDate { get; set; }
        public string PassportExpiryDate { get; set; }

       
    }
}