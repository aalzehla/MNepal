using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCF.MNepal.Models
{
    public class ClientKYC
    {
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
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
        public string GFathersName { get; set; }
        public string FatherInLaw { get; set; }
        public string Occupation { get; set; }

        public string PProvince { get; set; }

        public string PProvinceName { get; set; }
        public string PDistrict { get; set; }

        public string PDistrictName { get; set; }

        public string PMunicipalityVDC { get; set; }
        public string PHouseNo { get; set; }
        public string PWardNo { get; set; }
        public string PStreet { get; set; }

        public string CProvince { get; set; }

        public string CProvinceName { get; set; }
        public string CDistrict { get; set; }

        public string CDistrictName { get; set; }
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

        public string DateADBS { get; set; }
        public string BSDateOfBirth { get; set; }
        public string CustStatus { get; set; }
        public string PANNumber { get; set; }

        public string DocType { get; set; }
        
        public string FrontImage { get; set; }

        public string BackImage { get; set; }

        public string PassportImage { get; set; }
    }
}