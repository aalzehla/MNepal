using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace MNepalProject.Models
{
    [DataContract]
    public class MNClientKyc
    {
        public string ClientCode { get; set; }
        public string FName { get; set; }
        public string MName { get; set; }
        public string LName { get; set; }
        public string Gender { get; set; }
        public string DateOfBirth { get; set; }
        public string CountryCode { get; set; }
        public string Nationality { get; set; }
        public string FathersName { get; set; }
        public string SpouseName { get; set; }
        public string MaritalStatus { get; set; }
        public string GFathersName { get; set; }
        public string FatherInLaw { get; set; }
        public string Occupation { get; set; }
        public string PZone { get; set; }
        public string PDistrict { get; set; }
        public string PMunicipalityVdc { get; set; }
        public string PHouseNo { get; set; }
        public string PWardNo { get; set; }
        public string CZone { get; set; }
        public string CDistrict { get; set; }
        public string CMunicipalityVdc { get; set; }
        public string CHouseNo { get; set; }
        public string CWardNo { get; set; }
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
        public string PanNumber { get; set; }

        public string CustStatus { get; set; }
        public MNClientKyc() { }

        public MNClientKyc(string ClientCode, string LicenseIssueDate, string LicenseExpiryDate, string PanNumber)
        {
            this.ClientCode = ClientCode;

            this.LicenseIssueDate = LicenseIssueDate;
            this.LicenseExpiryDate = LicenseExpiryDate;
            this.PanNumber = PanNumber;
        }
    }
}