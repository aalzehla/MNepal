using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCF.MNepal.Models
{
    public class CustomerSupport
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string Remarks { get; set; }
        public string Date { get; set; }
        public string Category { get; set; }
        public string UploadedImageName { get; set; }

    
     

        public CustomerSupport()
        {
        }

        public CustomerSupport(string name, string email, string mobileNumber, string remarks, string category,string updatedImageName)
        {
            Name = name;
            Email = email;
            MobileNumber = mobileNumber;
            Remarks = remarks;
            Category = category;
            UploadedImageName = updatedImageName;
        }
    }
}