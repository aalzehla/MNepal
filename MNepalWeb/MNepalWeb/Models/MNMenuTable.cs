using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalWeb.Models {

    public partial class MNMenuTable {
        public int MenuID { get; set; }

        public string MenuModuleName { get; set; }
        public string ParentMenu { get; set; }

        public string Hierarchy { get; set; }

        public string Description { get; set; }

        public string ParentMenuID { get; set; }

        public string LinkUrl { get; set; }

        public string GroupAllowed { get; set; }

        public bool? CanBeAssigned { get; set; }

    }

}