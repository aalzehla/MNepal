using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ThailiMNepalAgent.Models
{
    public class KYC
    {

        public KYC()
        {
            Occupations = new List<Occupation>();
            ExtraInfos = new List<ExtraInfo>();
        }
        public string ClientCode { get; set; }

        [Required]
        [Display(Name="Full Name")]
        public string Name { get; set; }
        [Required]
        [Display(Name = "Date of Birth")]
        public string DOB { get; set; }
      
        [Display(Name = "District")]
        public string PerDistrict { get; set; }
        [Display(Name = "Municipality/VDC")]
        public string PerMunicipality { get; set; }
        [Display(Name="Ward No")]
        public string PerWardNo { get; set; }
        [Display(Name = "Street")]
        public string PerStreet { get; set; }
        [Display(Name = "House Number")]
        public string PerHouseNo { get; set; }
        [Display(Name = "State")]
        public string PerStateNo { get; set; }
        [Display(Name = "District")]
        public string CurDistrict { get; set; }
        [Display(Name = "Municipality/VDC")]
        public string CurMunicipality { get; set; }
        [Display(Name = "Ward No")]
        public string CurWardNo { get; set; }
        [Display(Name = "Street")]

        public string CurStreet { get; set; }
        [Display(Name = "House Number")]
        public string CurHouseNo { get; set; }
        [Display(Name = "State")]
        public string CurStateNo { get; set; }
        [Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; }
        [Display(Name = "LandLine Number")]
        public string LandlineNo { get; set; }  //Optional\
        [Display(Name = "Identification Type")]
        public string IdentificationType { get; set; }
        [Display(Name = "Citizenship Number")]
        public string CitizenShipNo { get; set; }
        [Display(Name = "Issued District")]
        public string CitizenShipIssuedBy { get; set; }
        [Display(Name = "Issued Date")]
        public string CitizenShipIssuedDate { get; set; }
        [Display(Name = "Passport Number")]
        public string PassportNo { get; set; }
        [Display(Name = "Issued District")]
        public string PassportIssuedBy { get; set; }
        [Display(Name = "Issued Date")]
        public string PassportIssuedDate { get; set; }
        [Display(Name = "PAN Number")]
        public string PanNo { get; set; }
        [Display(Name = "Father's Name")]
        public string FatherName { get; set; }
        [Display(Name = "Grandfather's Name")]
        public string GFatherName { get; set; }
        [Display(Name = "Mother's Name")]
        public string MotherName { get; set; }
        [Display(Name = "Marital Status")]
        public string MaritalStatus { get; set; }
        [Display(Name = "Spouse")]
        public string SpouseName { get; set; }
        [Display(Name = "Son")]
        public string Son { get; set; } //if multiple seperated by semicolon
        [Display(Name = "Daughter")]
        public string Daughter { get; set; } //if multiple seperated by semicolon
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; }
        [Display(Name = "Relation to Person")]
        public string RelationwithContactPerson { get; set; }
        [Display(Name = "Contact Number")]
        public string ContactPersonNumber { get; set; }
        public List<Occupation> Occupations{ get; set; }
        public List<ExtraInfo> ExtraInfos{ get; set; }
        public string Photo { get; set; }
        public string IdenitificationImage { get; set; }

    }
    
    public class Occupation
    {

        [Display(Name = "Organizaton Name")]
        public string OrganizationName { get; set; }
        [Display(Name = "Address")]
        public string Address { get; set; }
        [Display(Name = "Designation")]
        public string Designation { get; set; }
        [Display(Name = "Estimated Annual Income")]
        public int EstimatedIncome { get; set; }
        
    }
    public class ExtraInfo
    {
        public string Key { get; set; }
        
        public string Value { get; set; }
        
    }

}