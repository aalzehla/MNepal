using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MNepalProject.Models
{
    
    public class MNUsers
    {

        public string sc { get; set; }
        public string firstname{get;set;}
        public string middlename { get; set; }
        public string lastname{get;set;}
        public string familyname { get; set; }
       // public string amobile{get;set;}//person logged in
        public string umobile { get; set; }//person whose account is created
        public string amount { get; set; }
        public string dob { get; set; }
        public string street{get;set;}
        public string ward{get;set;}
        public string district{get;set;}
        public string zone{get;set;}
        public string photoid{get;set;}//photoid
        public string ivrLang { get; set; }
   
     
    }
}