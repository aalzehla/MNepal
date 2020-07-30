using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;


//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;

namespace CustApp.Models
{
    public class MerchantCollege
    {
        public MerchantCollege(string TransactionMedium, string CollegeName, string Amount, string TPin)
        {
            this.TransactionMedium = TransactionMedium;
            this.CollegeName = CollegeName;
            this.Amount = Amount;
            this.TPin = TPin;
        }

        public MerchantCollege()
        {
        }

        [Required]
        [Display(Name = "College Name")]
        public string CollegeName { get; set; }

        [Required]
        [Display(Name = "Bill Number")]
        public string BillNumber { get; set; }

        [Required]
        [Display(Name = "Student Name")]
        public string StudentName { get; set; }

        [Required]
        [Display(Name = "Transaction Medium")]
        public string TransactionMedium { get; set; }

        public string Faculty { get; set; }

        [Required]
        [Display(Name = "Roll Number")]
        public string RollNumber { get; set; }

        public string Class { get; set; }
        public int Year { get; set; }

        public string Month { get; set; }

        public string Amount { get; set; }

        public string Remarks { get; set; }

        [Display(Name = "T-Pin")]
        public string TPin { get; set; }

        public string TokenUnique { get; set; }

    }

    public class MerchantRestaurant
    {
        public MerchantRestaurant()
        {

        }

        public MerchantRestaurant(string tid, string TransactionMedium, string RestaurantName, string mobile,  string Amount, string TPin, string note, string sourcechannel)
        {
            this.tid = tid;
            this.TransactionMedium = TransactionMedium;
            this.RestaurantName = RestaurantName;
            this.mobile = mobile; 
            this.Amount = Amount;
            
            this.TPin = TPin;
            this.note = note;
            this.sourcechannel = sourcechannel;
        }

        public string tid { get; set; }
        public string mobile { get; set; }
        public string note { get; set; }
        public string sourcechannel { get; set; }

        [Required]
        [Display(Name = "Restaurant Name")]
        public string RestaurantName { get; set; }

        [Required]
        [Display(Name = "Bill Number")]
        public int BillNumber { get; set; }


        [Required]
        [Display(Name = "Transaction Medium")]
        public string TransactionMedium { get; set; }


        public string Amount { get; set; }

        public string Remarks { get; set; }

        [Display(Name = "T-Pin")]
        public string TPin { get; set; }

        public string TokenUnique { get; set; }

    }

    public class MerchantSchool
    {
        [Required]
        [Display(Name = "School Name")]
        public string SchoolName { get; set; }

        [Required]
        [Display(Name = "Bill Number")]
        public string BillNumber { get; set; }

        [Required]
        [Display(Name = "Student Name")]
        public string StudentName { get; set; }

        [Required]
        [Display(Name = "Transaction Medium")]
        public string TransactionMedium { get; set; }

        public string Class { get; set; }

        [Required]
        [Display(Name = "Roll Number")]
        public string RollNumber { get; set; }

        public int Year { get; set; }

        public string Month { get; set; }

        public string Amount { get; set; }

        public string Remarks { get; set; }

        [Display(Name = "T-Pin")]
        public string TPin { get; set; }

        public string TokenUnique { get; set; }

    }

    public class MerchantInsurance
    {
        [Required]
        [Display(Name = "Transaction Medium")]
        public string TransactionMedium { get; set; }
        
        [Required]
        [Display(Name = "Insurance Company Name")]
        public string InsuranceName { get; set; }

        [Required]
        [Display(Name = "Policy Number")]
        public string PolicyNumber { get; set; }

        [Required]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }
        [Required]
        [Display(Name = "Customer Address")]
        public string CustomerAddress { get; set; }
        [Required]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }
        [Required]
        [Display(Name = "Agent Name")]
        public string AgentName { get; set; }
        [Required]
        [Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; }
        [Required]
        [Display(Name = "Policy Type")]
        public string Type { get; set; }

        public string Amount { get; set; }

        [Display(Name = "T-Pin")]
        public string TPin { get; set; }

        [Display(Name = "Policy Description")]
        public string Remarks { get; set; }

        public string TokenUnique { get; set; }

    }
}