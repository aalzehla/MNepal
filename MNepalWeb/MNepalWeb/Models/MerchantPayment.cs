using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;


//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;

namespace MNepalWeb.Models
{
    public class MerchantCollege
    {
        [Required]
        [Display(Name = "College Name")]
        public string CollegeName { get; set; }

        [Required]
        [Display(Name = "Bill Number")]
        public int BillNumber { get; set; }

        [Required]
        [Display(Name = "Student Name")]
        public string StudentName { get; set; }

        [Required]
        [Display(Name = "Transaction Medium")]
        public string TransactionMedium { get; set; }

        public string Faculty { get; set; }

        [Required]
        [Display(Name = "Roll Number")]
        public int RollNumber { get; set; }

        public int Year { get; set; }

        public string Month { get; set; }

        public int Amount { get; set; }

        public string Purpose { get; set; }

        [Display(Name = "T-Pin")]
        public string TPin { get; set; }


    }

    public class MerchantRestaurant
    {
        [Required]
        [Display(Name = "Restaurant Name")]
        public string RestaurantName { get; set; }

        [Required]
        [Display(Name = "Bill Number")]
        public int BillNumber { get; set; }


        [Required]
        [Display(Name = "Transaction Medium")]
        public string TransactionMedium { get; set; }


        public int Amount { get; set; }

        public string Purpose { get; set; }

        [Display(Name = "T-Pin")]
        public string TPin { get; set; }


    }

    public class MerchantSchool
    {
        [Required]
        [Display(Name = "School Name")]
        public string SchoolName { get; set; }

        [Required]
        [Display(Name = "Bill Number")]
        public int BillNumber { get; set; }

        [Required]
        [Display(Name = "Student Name")]
        public string StudentName { get; set; }

        [Required]
        [Display(Name = "Transaction Medium")]
        public string TransactionMedium { get; set; }

        public string Class { get; set; }

        [Required]
        [Display(Name = "Roll Number")]
        public int RollNumber { get; set; }

        public int Year { get; set; }

        public string Month { get; set; }

        public int Amount { get; set; }

        public string Purpose { get; set; }

        [Display(Name = "T-Pin")]
        public string TPin { get; set; }

    }
}