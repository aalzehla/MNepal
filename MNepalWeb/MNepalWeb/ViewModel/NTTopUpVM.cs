using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace MNepalWeb.ViewModel
{
    public class NTTopUpVM
    {
        

        [Required(ErrorMessage ="Phonenumber is required and cannot be left empty")]
        [Display(Name ="Phonenumber")]
        [RegularExpression(@"[9][8][0-9]{8}", ErrorMessage ="Phonenumber must start with 98 and should be 10 character long")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage ="Amount is required and cannot be left empty")]
        [Display(Name ="Amount")]
        public int Amount { get; set; }
    }
}