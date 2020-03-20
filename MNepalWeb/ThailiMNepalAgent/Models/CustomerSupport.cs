using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ThailiMNepalAgent.Models
{
    public class CustomerSupport
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string Remarks { get; set; }
        public string Date { get; set; }
        public string Category { get; set; }
        public HttpPostedFileBase Image { get; set; }
    
     

        public CustomerSupport()
        {
        }

        public CustomerSupport(string name, string email, string mobileNumber, string remarks, string category)
        {
            Name = name;
            Email = email;
            MobileNumber = mobileNumber;
            Remarks = remarks;
            Category = category;
        }
    }
}