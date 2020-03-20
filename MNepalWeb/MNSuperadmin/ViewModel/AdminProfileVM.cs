using MNSuperadmin.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MNSuperadmin.ViewModel {
    public class AdminProfileVM {

        public string ProfileCode { get; set; }
        [Display(Name="Role")]
        public string ProfileName { get; set; }
        [Display(Name = "Role Description")]
        public string ProfileDesc { get; set; }
        public string ProfileStatus { get; set;}
        public List<MNMenuTable> MNMenus { get; set; }

    }
}