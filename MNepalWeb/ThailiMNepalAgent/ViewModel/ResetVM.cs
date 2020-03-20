using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ThailiMNepalAgent.ViewModel
{
    public class ResetVM
    {
        public string  ClientCode { get; set; }
        public string  PhoneNumber { get; set; }       
        
        public string OldPassword { get; set; }
       
        public string OldPin { get; set; }
        
        public string Password { get; set; }
       
        public string Pin { get; set; }
        
        public string Mode { get; set; }
        public string Message { get; set; }
    }
}